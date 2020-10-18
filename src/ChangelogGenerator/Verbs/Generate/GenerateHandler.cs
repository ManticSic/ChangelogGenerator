using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;

using ChangelogGenerator.Logging;

using Octokit;

using UnityContainerAttributeRegistration.Attribute;


namespace ChangelogGenerator.Verbs.Generate
{
    [RegisterType]
    internal class GenerateHandler : VerbHandler<GenerateOptions>
    {
        public GenerateHandler(GenerateOptions         options,
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
            Log.VerboseInfo($"Generate changelog for {Options.MilestoneTitle}.");

            IReadOnlyList<PullRequest> pullRequests = await LoadPullRequestsAsync(PullRequestFilter);

            if(pullRequests == null || pullRequests.Count < 1)
            {
                Log.Error("Failed to load any pull request matching the filter.");
                Log.VerboseError("Failed to load any pull request, but HTTP request was successful.");
                Exit(ExitCode.FailedToLoadData);

                return;
            }

            Log.VerboseInfo($"Successfully fetched {pullRequests.Count} pull requests.");
            string changelog = CreateChangelogMarkdown(pullRequests);

            Log.VerboseInfo($"Changelog:\n{changelog}");

            await WriteChangelogAsync(changelog);

            Exit(ExitCode.Success);
        }

        private bool PullRequestFilter(PullRequest pullRequest)
        {
            bool pullRequestHasRequestedMilestone = pullRequest.Milestone?.Title == Options.MilestoneTitle;

            return pullRequestHasRequestedMilestone;
        }
    }
}
