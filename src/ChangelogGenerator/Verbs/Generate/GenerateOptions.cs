using CommandLine;


namespace ChangelogGenerator.Verbs.Generate
{
    [Verb("generate", HelpText = "Generate a changelog for a specific milestone.")]
    internal class GenerateOptions : Options
    {
        [Option("milestone",
                Required = true,
                HelpText = "Title of the milestone.")]
        public string MilestoneTitle { get; set; }
    }
}
