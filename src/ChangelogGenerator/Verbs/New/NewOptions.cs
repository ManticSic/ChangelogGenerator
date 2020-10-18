using CommandLine;


namespace ChangelogGenerator.Verbs.New
{
    [Verb("new", HelpText = "Generate a new or override a existing changelog file.")]
    internal class NewOptions : Options
    {
        [Option("exclude-unknown",
                Required = false,
                Default  = false,
                HelpText = "Exclude pull requests without milestones")]
        public bool ExcludeUnknown { get; set; } = false;
    }
}
