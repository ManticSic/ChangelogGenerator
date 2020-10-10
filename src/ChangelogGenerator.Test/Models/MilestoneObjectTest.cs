using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using ChangelogGenerator.Models;
using ChangelogGenerator.Test.Builder;

using NUnit.Framework;

using Octokit;

using static NUnit.Framework.Assert;


namespace ChangelogGenerator.Test.Models
{
    public class MilestoneObjectTest
    {
        [Test]
        public void TestFrom()
        {
            PullRequest pr1 = new PullRequestBuilder
                              {
                                  Milestone = new MilestoneBuilder
                                              {
                                                  Title = "Milestone1",
                                                  ClosedAt = DateTimeOffset.ParseExact(
                                                      "01.11.1993", "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                              }.Build()
                              }.Build();

            PullRequest pr2 = new PullRequestBuilder
                              {
                                  Milestone = new MilestoneBuilder
                                              {
                                                  Title = "Milestone1",
                                                  ClosedAt = DateTimeOffset.ParseExact(
                                                      "01.11.1993", "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                              }.Build()
                              }.Build();

            PullRequest pr3 = new PullRequestBuilder
                              {
                                  Milestone = new MilestoneBuilder
                                              {
                                                  Title = "Milestone2",
                                                  ClosedAt = DateTimeOffset.ParseExact(
                                                      "15.09.1993", "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                              }.Build()
                              }.Build();

            PullRequest pr4 = new PullRequestBuilder
                              {
                                  Milestone = new MilestoneBuilder
                                              {
                                                  Title = "Milestone2",
                                                  ClosedAt = DateTimeOffset.ParseExact(
                                                      "15.09.1993", "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                              }.Build()
                              }.Build();

            PullRequest pr5 = new PullRequestBuilder
                              {
                                  Milestone = new MilestoneBuilder
                                              {
                                                  Title = "Milestone2",
                                                  ClosedAt = DateTimeOffset.ParseExact(
                                                      "15.09.1993", "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                              }.Build()
                              }.Build();

            PullRequest pr6 = new PullRequestBuilder
                              {
                                  Milestone = new MilestoneBuilder
                                              {
                                                  Title = "Milestone3"
                                              }.Build()
                              }.Build();

            PullRequest pr7 = new PullRequestBuilder().Build();

            IReadOnlyList<PullRequest> pullRequests = new[]
                                                      {
                                                          pr1,
                                                          pr2,
                                                          pr3,
                                                          pr4,
                                                          pr5,
                                                          pr6,
                                                          pr7
                                                      };

            IList<MilestoneObject> result = MilestoneObject.From(pullRequests)
                                                           .ToList();

            AreEqual(4, result.Count());

            AreEqual(null, result[0]
                          .Milestone?.Title);
            AreEqual(1, result[0]
                       .PullRequests.Count);

            AreEqual("Milestone3", result[1]
                                  .Milestone?.Title);
            AreEqual(1, result[1]
                       .PullRequests.Count);

            AreEqual("Milestone1", result[2]
                                  .Milestone?.Title);
            AreEqual(2, result[2]
                       .PullRequests.Count);

            AreEqual("Milestone2", result[3]
                                  .Milestone?.Title);
            AreEqual(3, result[3]
                       .PullRequests.Count);
        }
    }
}
