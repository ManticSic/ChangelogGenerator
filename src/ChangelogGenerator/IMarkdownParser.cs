using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


namespace ChangelogGenerator
{
    internal interface IMarkdownParser
    {
        public IDictionary<string, IList<string>> GetChangelogEntries([NotNull] string markdown);

        public string GetChangelogBy(VersionEntry versionEntry);
    }
}
