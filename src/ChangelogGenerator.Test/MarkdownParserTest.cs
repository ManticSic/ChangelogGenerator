using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using ChangelogGenerator.Models;
using ChangelogGenerator.Test.Builder;

using NUnit.Framework;

using Octokit;

using static NUnit.Framework.Assert;


namespace ChangelogGenerator.Test
{
    public class MarkdownParserTest
    {
        [Test]
        [TestCase("description_at-the-end.md", new[] {"an added entry"}, new[] {"a changed entry"}, new[] {"a deprecated entry"},
                  new[] {"a fixed entry"}, new[] {"a removed entry"}, new[] {"a security entry"})]
        [TestCase("description_at-the-middle.md", new[] {"an added entry"}, new[] {"a changed entry"},
                  new[] {"a deprecated entry"}, new[] {"a fixed entry"}, new[] {"a removed entry"}, new[] {"a security entry"})]
        public void TestGetChangelogEntries(string   fileName,
                                            string[] expectedAddedEntries,
                                            string[] expectedChangedEntries,
                                            string[] expectedDeprecatedEntries,
                                            string[] expectedFixedEntries,
                                            string[] expectedRemovedEntries,
                                            string[] expectedSecurityEntries)
        {
            IMarkdownParser markdownParser = new MarkdownParser();
            string          markdownText   = File.ReadAllText($"./Assets/{fileName}");

            IDictionary<string, IList<string>> result = markdownParser.GetChangelogEntries(markdownText);

            AreEqual(6, result.Keys.Count);

            ExpectEquivalentEntries(result, "added", expectedAddedEntries);
            ExpectEquivalentEntries(result, "changed", expectedChangedEntries);
            ExpectEquivalentEntries(result, "deprecated", expectedDeprecatedEntries);
            ExpectEquivalentEntries(result, "fixed", expectedFixedEntries);
            ExpectEquivalentEntries(result, "removed", expectedRemovedEntries);
            ExpectEquivalentEntries(result, "security", expectedSecurityEntries);
        }

        [Test]
        [TestCase("description_empty.md")]
        [TestCase("description_no-changelog.md")]
        public void TestGetChangelogEntries_Empty(string fileName)
        {
            IMarkdownParser markdownParser = new MarkdownParser();
            string          markdownText   = File.ReadAllText($"./Assets/{fileName}");

            IDictionary<string, IList<string>> result = markdownParser.GetChangelogEntries(markdownText);

            AreEqual(0, result.Keys.Count);
        }

        [Test]
        [TestCase("description_invalid-changelog.md")]
        public void TestGetChangelogEntries_Invalid(string fileName)
        {
            IMarkdownParser markdownParser = new MarkdownParser();
            string          markdownText   = File.ReadAllText($"./Assets/{fileName}");

            Throws<InvalidOperationException>(() => markdownParser.GetChangelogEntries(markdownText));
        }

        [Test]
        public void TestGetChangelogBy()
        {
            IMarkdownParser markdownParser = new MarkdownParser();

            Milestone milestone = new MilestoneBuilder
                                  {
                                      Title = "v1.2.3",
                                      ClosedAt = DateTimeOffset.ParseExact("01.11.1993", "dd.MM.yyyy",
                                                                           CultureInfo.InvariantCulture),
                                  }.Build();
            MilestoneObject milestoneObject = new MilestoneObject(milestone, new PullRequest[]
                                                                             {
                                                                             });
            Dictionary<string, IList<string>> entries = new Dictionary<string, IList<string>>
                                                        {
                                                            {
                                                                "added", new[]
                                                                         {
                                                                             "first added entry",
                                                                             "second added entry",
                                                                         }
                                                            },
                                                            {
                                                                "changed", new[]
                                                                           {
                                                                               "first changed entry",
                                                                               "second changed entry",
                                                                           }
                                                            },
                                                        };
            VersionEntry versionEntry = new VersionEntry(milestoneObject, entries);

            string expected =
                "## v1.2.3 (1993-11-01)\n\n### added\n\n* first added entry\n* second added entry\n\n### changed\n\n* first changed entry\n* second changed entry";

            string result = markdownParser.GetChangelogBy(versionEntry);

            AreEqual(expected, result);
        }

        [Test]
        public void TestGetChangelogBy_VersionWithoutEntries()
        {
            IMarkdownParser markdownParser = new MarkdownParser();

            Milestone milestone = new MilestoneBuilder
                                  {
                                      Title = "v1.2.3",
                                      ClosedAt = DateTimeOffset.ParseExact("01.11.1993", "dd.MM.yyyy",
                                                                           CultureInfo.InvariantCulture),
                                  }.Build();
            MilestoneObject milestoneObject = new MilestoneObject(milestone, new PullRequest[]
                                                                             {
                                                                             });
            Dictionary<string, IList<string>> entries      = new Dictionary<string, IList<string>>();
            VersionEntry                      versionEntry = new VersionEntry(milestoneObject, entries);

            string result = markdownParser.GetChangelogBy(versionEntry);

            AreEqual(String.Empty, result);
        }

        [Test]
        public void TestGetChangelogBy_VersionEntryWithEmptyEntry()
        {
            IMarkdownParser markdownParser = new MarkdownParser();

            Milestone milestone = new MilestoneBuilder
                                  {
                                      Title = "v1.2.3",
                                      ClosedAt = DateTimeOffset.ParseExact("01.11.1993", "dd.MM.yyyy",
                                                                           CultureInfo.InvariantCulture),
                                  }.Build();
            MilestoneObject milestoneObject = new MilestoneObject(milestone, new PullRequest[]
                                                                             {
                                                                             });
            Dictionary<string, IList<string>> entries = new Dictionary<string, IList<string>>
                                                        {
                                                            {
                                                                "added", new string[0]
                                                            },
                                                            {
                                                                "changed", new[]
                                                                           {
                                                                               "first changed entry",
                                                                               "second changed entry",
                                                                           }
                                                            },
                                                        };
            VersionEntry versionEntry = new VersionEntry(milestoneObject, entries);

            string result = markdownParser.GetChangelogBy(versionEntry);

            string expected = "## v1.2.3 (1993-11-01)\n\n### changed\n\n* first changed entry\n* second changed entry";

            AreEqual(expected, result);
        }

        [Test]
        public void TestGetChangelogBy_WithoutClosedMilestone()
        {
            IMarkdownParser markdownParser = new MarkdownParser();

            Milestone milestone = new MilestoneBuilder
                                  {
                                      Title = "v1.2.3",
                                  }.Build();
            MilestoneObject milestoneObject = new MilestoneObject(milestone, new PullRequest[]
                                                                             {
                                                                             });
            Dictionary<string, IList<string>> entries = new Dictionary<string, IList<string>>
                                                        {
                                                            {
                                                                "added", new[]
                                                                         {
                                                                             "first added entry",
                                                                             "second added entry",
                                                                         }
                                                            },
                                                        };
            VersionEntry versionEntry = new VersionEntry(milestoneObject, entries);

            string result = markdownParser.GetChangelogBy(versionEntry);

            string expected = "## v1.2.3 (OPEN)\n\n### added\n\n* first added entry\n* second added entry";

            AreEqual(expected, result);
        }

        [Test]
        public void TestGetChangelogBy_WithoutMilestone()
        {
            IMarkdownParser markdownParser = new MarkdownParser();

            MilestoneObject milestoneObject = new MilestoneObject(null, new PullRequest[]
                                                                        {
                                                                        });
            Dictionary<string, IList<string>> entries = new Dictionary<string, IList<string>>
                                                        {
                                                            {
                                                                "added", new[]
                                                                         {
                                                                             "first added entry",
                                                                             "second added entry",
                                                                         }
                                                            },
                                                        };
            VersionEntry versionEntry = new VersionEntry(milestoneObject, entries);

            string result = markdownParser.GetChangelogBy(versionEntry);

            string expected = "## UNKNOWN VERSION\n\n### added\n\n* first added entry\n* second added entry";

            AreEqual(expected, result);
        }

        private void ExpectEquivalentEntries(IDictionary<string, IList<string>> result, string key, string[] expectedEntries)
        {
            IsTrue(result.ContainsKey(key));
            AreEqual(expectedEntries.Length, result[key]
                        .Count);
            That(result[key], Is.EquivalentTo(expectedEntries));
        }
    }
}
