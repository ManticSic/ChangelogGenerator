using System;

using Octokit;


namespace ChangelogGenerator.Test.Builder
{
    public class MilestoneBuilder : IBuilder<Milestone>
    {
        public string Url { get; set; } = String.Empty;

        public string HtmlUrl { get; set; } = String.Empty;

        public long Id { get; set; } = 0;

        public int Number { get; set; } = 0;

        public string NodeId { get; set; } = String.Empty;

        public ItemState State { get; set; } = ItemState.Closed;

        public string Title { get; set; } = String.Empty;

        public string Description { get; set; } = String.Empty;

        public User Creator { get; set; } = new User();

        public int OpenIssues { get; set; } = 0;

        public int ClosedIssues { get; set; } = 0;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.MinValue;

        public DateTimeOffset? DueOn { get; set; } = null;

        public DateTimeOffset? ClosedAt { get; set; } = null;

        public DateTimeOffset? UpdatedAt { get; set; } = null;

        public Milestone Build()
        {
            return new Milestone(Url, HtmlUrl, Id, Number, NodeId, State, Title, Description, Creator, OpenIssues, ClosedIssues,
                                 CreatedAt, DueOn, ClosedAt, UpdatedAt);
        }
    }
}
