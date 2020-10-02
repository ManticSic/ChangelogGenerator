using System;
using System.Collections.Generic;

using Octokit;


namespace ChangelogGenerator.Test.Builder
{
    public class PullRequestBuilder : IBuilder<PullRequest>
    {
        public long Id { get; set; }

        public string NodeId { get; set; }

        public string Url { get; set; }

        public string HtmlUrl { get; set; }

        public string DiffUrl { get; set; }

        public string PatchUrl { get; set; }

        public string IssueUrl { get; set; }

        public string StatusesUrl { get; set; }

        public int Number { get; set; }

        public ItemState State { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? ClosedAt { get; set; }

        public DateTimeOffset? MergedAt { get; set; }

        public GitReference Head { get; set; }

        public GitReference Base { get; set; }

        public User User { get; set; }

        public User Assignee { get; set; }

        public IReadOnlyList<User> Assignees { get; set; }

        public bool Draft { get; set; }

        public bool? Mergeable { get; set; }

        public MergeableState? MergeableState { get; set; }

        public User MergedBy { get; set; }

        public string MergeCommitSha { get; set; }

        public int Comments { get; set; }

        public int Commits { get; set; }

        public int Additions { get; set; }

        public int Deletions { get; set; }

        public int ChangedFiles { get; set; }

        public Milestone Milestone { get; set; }

        public bool Locked { get; set; }

        public bool? MaintainerCanModify { get; set; }

        public IReadOnlyList<User> RequestedReviewers { get; set; }

        public IReadOnlyList<Team> RequestedTeams { get; set; }

        public IReadOnlyList<Label> Labels { get; set; }

        public PullRequest Build()
        {
            return new PullRequest(Id, NodeId, Url, HtmlUrl, DiffUrl, PatchUrl, IssueUrl, StatusesUrl, Number, State, Title, Body, CreatedAt, UpdatedAt, ClosedAt, MergedAt, Head, Base, User, Assignee, Assignees, Draft, Mergeable, MergeableState, MergedBy, MergeCommitSha, Comments, Commits, Additions, Deletions, ChangedFiles, Milestone, Locked, MaintainerCanModify, RequestedReviewers, RequestedTeams, Labels);
        }
    }
}
