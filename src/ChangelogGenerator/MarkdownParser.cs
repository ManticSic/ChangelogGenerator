using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Markdig;
using Markdig.Syntax;

using UnityContainerAttributeRegistration.Attribute;


namespace ChangelogGenerator
{
    [RegisterType(typeof(IMarkdownParser))]
    internal class MarkdownParser : IMarkdownParser
    {
        private const string ChangelogHeaderMark                 = "##";
        private const string ChangelogHeading                    = "changelog";
        private const char   ChangelogTypeSeparator              = ':';
        private const int    ExpectedRawChangelogEntrySplitCount = 2;

        public IDictionary<string, IList<string>> GetChangelogEntries([NotNull] string markdown)
        {
            return GetParsedChangelogEntries(markdown)
                  .GroupBy(tuple => tuple.type, tuple => tuple)
                  .ToDictionary(grp => grp.Key, grp => grp.Select(tuple => tuple.entry)
                                                          .ToList())
                  .ToDictionary(dict => dict.Key, dict => dict.Value as IList<string>);
        }

        public string GetChangelogBy(VersionEntry versionEntry)
        {
            if(!versionEntry.Entries.Any())
            {
                return String.Empty;
            }

            string result = String.Empty;
            result += GetVersionTitle(versionEntry);

            foreach((string versionEntryHeadingText, IList<string> entries) in versionEntry.Entries)
            {
                if(!entries.Any())
                {
                    continue;
                }

                result += "\n\n";
                result += GetVersionEntryHeading(versionEntryHeadingText);
                result += "\n\n";
                result += GetEntryList(entries);
            }

            return result;
        }

        private string GetEntryList(IList<string> entries)
        {
            return String.Join('\n', entries.Select(entry => $"* {entry}"));
        }

        private string GetVersionEntryHeading(string heading)
        {
            return $"### {heading}";
        }

        private string GetVersionTitle(VersionEntry versionEntry)
        {
            string result = "## ";

            if(versionEntry.Title != null)
            {
                result += $"{versionEntry.Title} ";

                if(versionEntry.ReleaseDate != null)
                {
                    result += $"({versionEntry.ReleaseDate:yyyy-MM-dd})";
                }
                else
                {
                    result += "(OPEN)";
                }
            }
            else
            {
                result += "UNKNOWN VERSION";
            }

            return result;
        }

        private IEnumerable<(string type, string entry)> GetParsedChangelogEntries([NotNull] string markdown)
        {
            IList<string> rawChangelogEntries = GetRawChangelogEntries(markdown);

            foreach(string rawChangelogEntry in rawChangelogEntries)
            {
                string[] parts = rawChangelogEntry.Split(ChangelogTypeSeparator, ExpectedRawChangelogEntrySplitCount);

                if(parts.Length != ExpectedRawChangelogEntrySplitCount)
                {
                    throw new InvalidOperationException("Malformed raw changelog entry:");
                }

                string type = parts[0]
                   .Trim();
                string entry = parts[1]
                   .Trim();

                yield return (type, entry);
            }
        }

        private IList<string> GetRawChangelogEntries([NotNull] string markdown)
        {
            List<string> entries = new List<string>();

            MarkdownDocument markdownDocument   = Markdown.Parse(markdown);
            bool             isChildOfChangelog = false;

            foreach(Block block in markdownDocument)
            {
                if(isChildOfChangelog)
                {
                    if(block is HeadingBlock headingBlock && IsHeadingBlockOnChangelogHeadingLevel(headingBlock))
                    {
                        break;
                    }

                    if(block is ListBlock listBlock)
                    {
                        entries.AddRange(GetListItems(listBlock, markdown));
                    }
                }

                if(IsChangelogHeading(block, markdown))
                {
                    isChildOfChangelog = true;
                }
            }

            return entries;
        }

        private bool IsChangelogHeading([NotNull] Block block, [NotNull] string markdown)
        {
            if(!(block is HeadingBlock headingBlock && IsHeadingBlockOnChangelogHeadingLevel(headingBlock)))
            {
                return false;
            }

            string headingText = markdown.Substring(headingBlock.Span.Start, headingBlock.Span.Length)
                                         .Substring(ChangelogHeaderMark.Length)
                                         .Trim();

            return headingText.ToLower() == ChangelogHeading;
        }

        private bool IsHeadingBlockOnChangelogHeadingLevel([NotNull] HeadingBlock headingBlock)
        {
            return headingBlock.Level == ChangelogHeaderMark.Length;
        }

        private IList<string> GetListItems([NotNull] ListBlock listBlock, [NotNull] string markdown)
        {
            IList<string> result = new List<string>();

            foreach(Block block in listBlock)
            {
                string entry = markdown.Substring(block.Span.Start, block.Span.Length)
                                       .Substring(1)
                                       .Trim();

                result.Add(entry);
            }

            return result;
        }
    }
}
