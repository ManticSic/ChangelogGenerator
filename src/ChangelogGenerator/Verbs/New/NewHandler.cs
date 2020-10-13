using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;

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

            Log.VerboseInfo($"Successfully fetched {pullRequests?.Count} pull requests.");
            string changelog = CreateChangelogMarkdown(pullRequests);

            Log.VerboseInfo("Changelog:");
            Log.VerboseInfo(changelog);

            await WriteChangelogAsync(changelog);

            Exit(ExitCode.Success);
        }
    }
}
