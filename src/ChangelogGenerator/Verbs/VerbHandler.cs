using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using ChangelogGenerator.Models;

using JetBrains.Annotations;

using Octokit;


namespace ChangelogGenerator.Verbs
{
    internal abstract class VerbHandler<T> : IVerbHandler where T : Options
    {
        protected readonly T                       Options;
        protected readonly ILog                    Log;
        private readonly   IGitHubClient           gitHubClient;
        private readonly   IMarkdownParser         markdownParser;
        private readonly   IFileSystem             fileSystem;
        private readonly   IEnvironmentAbstraction environment;

        protected VerbHandler(T                       options,
                              ILog                    log,
                              IEnvironmentAbstraction environment,
                              IFileSystem             fileSystem,
                              IGitHubClient           gitHubClient,
                              IMarkdownParser         markdownParser)
        {
            Options           = options;
            this.gitHubClient = gitHubClient;
            Log               = log;

            this.environment    = environment;
            this.markdownParser = markdownParser;
            this.fileSystem     = fileSystem;
        }

        protected abstract Task RunAsync();

        protected void ExitWithException([NotNull] string message, [NotNull] Exception exception, ExitCode exitCode)
        {
            Log.Error(message);
            Log.VerboseError(exception);
            Exit(exitCode);
        }

        protected void Exit(ExitCode exitCode)
        {
            if(exitCode != ExitCode.Success)
            {
                Log.Info("The application has unexpectedly terminated. Use --verbose for more information.");
            }

            environment.Exit((int) exitCode);
        }

        protected async Task<IReadOnlyList<PullRequest>> LoadPullRequestsAsync()
        {
            Repository                 repository   = null;
            IReadOnlyList<PullRequest> pullRequests = null;

            PullRequestRequest pullRequestRequest = new PullRequestRequest
                                                    {
                                                        State = ItemStateFilter.All,
                                                    };

            try
            {
                repository   = await gitHubClient.Repository.Get(Options.Owner, Options.Repository);
                pullRequests = await gitHubClient.PullRequest.GetAllForRepository(repository.Id, pullRequestRequest);
            }
            catch(ApiException exception) when(repository == null)
            {
                ExitWithException($"Failed to load repository data for {Options.Owner}, {Options.Repository}.",
                                  exception, ExitCode.FailedToLoadData);
            }
            catch(ApiException exception) when(pullRequests == null)
            {
                ExitWithException($"Failed to load pullrequest for repository {repository?.Owner}/{repository?.Name}.",
                                  exception, ExitCode.FailedToLoadData);
            }

            return pullRequests;
        }

        protected string CreateChangelogMarkdown([NotNull] [ItemNotNull] IReadOnlyList<PullRequest> pullRequests)
        {
            IEnumerable<MilestoneObject> milestones = MilestoneObject.From(pullRequests);
            IList<VersionEntry> versionEntries = milestones.Select(CreateVersionEntry)
                                                           .ToList();

            string changelog = String.Join("\n\n\n", versionEntries.Select(markdownParser.GetChangelogBy)
                                                                   .ToList());

            return changelog;
        }

        protected async Task WriteChangelogAsync([NotNull] string changelog)
        {
            try
            {
                await fileSystem.File.WriteAllTextAsync(Options.Output, changelog);
            }
            catch(ArgumentNullException exception)
            {
                // Output path is null.
                ExitWithException("Output path is null", exception, ExitCode.FailedToWriteFile);
            }
            catch(ArgumentException exception)
            {
                // Output path is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.
                ExitWithException(
                    "Output path is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.",
                    exception, ExitCode.FailedToWriteFile);
            }
            catch(PathTooLongException exception)
            {
                // Output path exceeds the system-defined maximum length.
                ExitWithException("Output path exceeds the system-defined maximum length.", exception,
                                  ExitCode.FailedToWriteFile);
            }
            catch(DirectoryNotFoundException exception)
            {
                // The specified output path is invalid (for example, it is on an unmapped drive).
                ExitWithException("The specified output path is invalid (for example, it is on an unmapped drive).", exception,
                                  ExitCode.FailedToWriteFile);
            }
            catch(NotSupportedException exception)
            {
                // Output path is in an invalid format.
                ExitWithException("Output path is in an invalid format.", exception, ExitCode.FailedToWriteFile);
            }
            catch(SecurityException exception)
            {
                // The caller does not have the required permission.
                ExitWithException("You have not the required permission to write the file.", exception,
                                  ExitCode.FailedToWriteFile);
            }
            catch(IOException exception)
            {
                // An I/O error occurred while opening the file.
                ExitWithException("Failed to write file.", exception, ExitCode.FailedToWriteFile);
            }
            catch(UnauthorizedAccessException exception)
            {
                // No permission to write the file
                ExitWithException("You have no permission to write the file.", exception, ExitCode.FailedToWriteFile);
            }
        }

        private VersionEntry CreateVersionEntry([NotNull] MilestoneObject milestone)
        {
            return new VersionEntry(milestone, GetChangelogEntriesForPullRequests(milestone.PullRequests));
        }

        private IDictionary<string, IList<string>> GetChangelogEntriesForPullRequests(
            [NotNull] [ItemNotNull] IReadOnlyList<PullRequest> pullRequests)
        {
            IDictionary<string, List<string>> changelogEntries = new Dictionary<string, List<string>>();

            foreach(PullRequest pullRequest in pullRequests)
            {
                IDictionary<string, IList<string>> changelogEntriesForPullRequest =
                    markdownParser.GetChangelogEntries(pullRequest.Body);

                Merge(changelogEntriesForPullRequest, changelogEntries);
            }

            return changelogEntries.ToDictionary(dict => dict.Key, dict => dict.Value as IList<string>);
        }

        private void Merge(IDictionary<string, IList<string>> items, IDictionary<string, List<string>> into)
        {
            foreach((string key, IList<string> value) in items)
            {
                if(!into.ContainsKey(key))
                {
                    into.Add(key, new List<string>());
                }

                into[key]
                   .AddRange(value);
            }
        }

        void IVerbHandler.Run()
        {
            Task task = RunAsync();
            task.Wait();
        }
    }
}
