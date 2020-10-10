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

using UnityContainerAttributeRegistration.Attribute;


namespace ChangelogGenerator.Verbs.New
{
    [RegisterType]
    internal class NewHandler : VerbHandler<NewOptions>
    {
        private readonly IMarkdownParser markdownParser;
        private readonly IFileSystem     fileSystem;

        public NewHandler(NewOptions              options,
                          IGitHubClient           gitHubClient,
                          ILog                    log,
                          IMarkdownParser         markdownParser,
                          IFileSystem             fileSystem,
                          IEnvironmentAbstraction environment) : base(options, gitHubClient, log, environment)
        {
            this.markdownParser = markdownParser;
            this.fileSystem     = fileSystem;
        }

        protected override async Task RunAsync()
        {
            Log.VerboseInfo("Create full changelog");

            IReadOnlyList<PullRequest> pullRequests = await LoadPullRequestsAsync();

            Log.VerboseInfo($"Successfully fetched {pullRequests?.Count} pull requests.");
            string changelog = CreateChangelogMarkdown(pullRequests);

            Log.VerboseInfo("Changelog:");
            Log.VerboseInfo(changelog);

            await WriteChangelogAsync(changelog);

            Exit(ExitCode.Success);
        }

        private async Task<IReadOnlyList<PullRequest>> LoadPullRequestsAsync()
        {
            Repository                 repository   = null;
            IReadOnlyList<PullRequest> pullRequests = null;

            PullRequestRequest pullRequestRequest = new PullRequestRequest
                                                    {
                                                        State = ItemStateFilter.All
                                                    };

            try
            {
                repository   = await GitHubClient.Repository.Get(Options.Owner, Options.Repository);
                pullRequests = await GitHubClient.PullRequest.GetAllForRepository(repository.Id, pullRequestRequest);
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

        private string CreateChangelogMarkdown([NotNull] [ItemNotNull] IReadOnlyList<PullRequest> pullRequests)
        {
            IEnumerable<MilestoneObject> milestones = MilestoneObject.From(pullRequests);
            IList<VersionEntry> versionEntries = milestones.Select(CreateVersionEntry)
                                                           .ToList();

            string changelog = String.Join("\n\n\n", versionEntries.Select(markdownParser.GetChangelogBy)
                                                                   .ToList());

            return changelog;
        }

        private async Task WriteChangelogAsync([NotNull] string changelog)
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
    }
}
