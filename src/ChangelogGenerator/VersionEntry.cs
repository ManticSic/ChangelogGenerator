using System;
using System.Collections.Generic;

using ChangelogGenerator.Models;

using JetBrains.Annotations;


namespace ChangelogGenerator
{
    internal class VersionEntry
    {
        public VersionEntry(MilestoneObject milestone, IDictionary<string, IList<string>> entries)
        {
            Title       = milestone.Milestone?.Title;
            ReleaseDate = milestone.Milestone?.ClosedAt;
            Entries     = entries;
        }

        [CanBeNull]
        public string Title { get; }

        public DateTimeOffset? ReleaseDate { get; }

        public IDictionary<string, IList<string>> Entries { get; }
    }
}
