using CommandLine;


namespace ChangelogGenerator.Verbs
{
    internal class Options
    {
        [Option("repository",
                Required = true,
                Default  = null,
                HelpText = "Set the repository name.")]
        public string Repository { get; set; }

        [Option("owner",
                Required = true,
                Default  = null,
                HelpText = "Set the owner of the repository.")]
        public string Owner { get; set; }

        [Option("token",
                Required = false,
                Default  = null,
                HelpText = "GitHub authentication token.")]
        public string Token { get; set; } = null;

        [Option('o', "output",
                Required = false,
                Default  = "CHANGELOG.md",
                HelpText = "Set output file name.")]
        public string Output { get; set; } = "CHANGELOG.md";

        [Option('v', "verbose",
                Required = false,
                Default  = false,
                HelpText = "Be verbose.")]
        public bool Verbose { get; set; } = false;
    }
}
