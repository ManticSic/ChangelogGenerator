using System.IO.Abstractions;
using System.Threading.Tasks;

using ChangelogGenerator.Logging;

using Octokit;

using UnityContainerAttributeRegistration.Attribute;


namespace ChangelogGenerator.Verbs.New
{
    [RegisterType]
    internal class NewHandler : VerbHandler<NewOptions>
    {
        public NewHandler(NewOptions              options,
                          ILog                    log,
                          IEnvironmentAbstraction environment,
                          IFileSystem             fileSystem,
                          IGitHubClient           gitHubClient,
                          IMarkdownParser         markdownParser) : base(options, log, environment, fileSystem, gitHubClient,
                                                                         markdownParser)
        {
        }

        protected override async Task RunAsync()
        {
            Log.VerboseInfo("Create full changelog");

            IReadOnlyList<PullRequest> pullRequests = await LoadPullRequestsAsync();

            if(pullRequests == null || pullRequests.Count < 1)
            {
                Log.Error("Failed to load pull request.");
                Log.VerboseError("Failed to load pull request, but HTTP request was successful.");
                Exit(ExitCode.FailedToLoadData);

                return;
            }

            Log.VerboseInfo($"Successfully fetched {pullRequests?.Count} pull requests.");
            string changelog = CreateChangelogMarkdown(pullRequests);

            Log.VerboseInfo($"Changelog:\n{changelog}");

            await WriteChangelogAsync(changelog);

            Exit(ExitCode.Success);
        }
    }
}
