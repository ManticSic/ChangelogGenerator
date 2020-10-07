using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Octokit;


namespace ChangelogGenerator.Models
{
    internal class MilestoneObject
    {
        public MilestoneObject(Milestone milestone, IReadOnlyList<PullRequest> pullRequests)
        {
            Milestone    = milestone;
            PullRequests = pullRequests;
        }

        [CanBeNull]
        public Milestone Milestone { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<PullRequest> PullRequests { get; }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<MilestoneObject> From([NotNull] [ItemNotNull] IReadOnlyList<PullRequest> pullRequests)
        {
            IList<MilestoneObject> transformedCollection = GroupPullrequestsByMilestone(pullRequests)
                                                          .Where(entry => entry.Value.Any())
                                                          .Select(From)
                                                          .ToList();

            return OrderMilestones(transformedCollection);
        }

        private static IDictionary<(string title, Milestone milestone), IList<PullRequest>> GroupPullrequestsByMilestone(
            [NotNull] [ItemNotNull] IReadOnlyList<PullRequest> pullRequests)
        {
            IDictionary<(string title, Milestone milestone), IList<PullRequest>> collection =
                new Dictionary<(string title, Milestone milestone), IList<PullRequest>>
                {
                    {
                        (null, null), new List<PullRequest>()
                    }
                };

            foreach(PullRequest pullRequest in pullRequests)
            {
                (string Title, Milestone milestone) key;

                if(pullRequest.Milestone == null)
                {
                    key = (null, null);
                }
                else
                {
                    Milestone                           milestone = pullRequest.Milestone;
                    (string Title, Milestone milestone) tempKey   = (milestone.Title, milestone);

                    if(collection.All(entry => entry.Key.title != pullRequest.Milestone.Title))
                    {
                        collection.Add(tempKey, new List<PullRequest>());
                    }

                    key = collection.Keys.First(entry => entry.title == pullRequest.Milestone.Title);
                }

                collection[key]
                   .Add(pullRequest);
            }

            return collection;
        }

        private static IEnumerable<MilestoneObject> OrderMilestones(IList<MilestoneObject> transformedCollection)
        {
            MilestoneObject unknownVersionEntry = transformedCollection.FirstOrDefault(entry => entry.Milestone == null);

            IEnumerable<MilestoneObject> entriesWithVersion = transformedCollection.Where(entry => entry.Milestone != null);

            IOrderedEnumerable<MilestoneObject> entriesWithOpenVersion = entriesWithVersion
                                                                        .Where(entry => entry.Milestone?.ClosedAt == null)
                                                                        .OrderByDescending(entry => entry.Milestone.Title);

            IOrderedEnumerable<MilestoneObject> entriesWithClosedVersion = entriesWithVersion
                                                                          .Where(entry => entry.Milestone?.ClosedAt != null)
                                                                          .OrderByDescending(entry => entry.Milestone?.ClosedAt);

            List<MilestoneObject> result = new List<MilestoneObject>();

            if(unknownVersionEntry != null)
            {
                result.Add(unknownVersionEntry);
            }

            if(entriesWithOpenVersion.Any())
            {
                result.AddRange(entriesWithOpenVersion);
            }

            if(entriesWithClosedVersion.Any())
            {
                result.AddRange(entriesWithClosedVersion);
            }

            return result;
        }

        [NotNull]
        private static MilestoneObject From(KeyValuePair<(string title, Milestone milestone), IList<PullRequest>> entry)
        {
            return new MilestoneObject(entry.Key.milestone, entry.Value.ToList()
                                                                 .AsReadOnly());
        }
    }
}
