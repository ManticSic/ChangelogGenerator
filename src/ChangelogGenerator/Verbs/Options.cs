using CommandLine;


namespace ChangelogGenerator.Verbs
{
    internal class Options
    {
        [Option("product",
                Required = true,
                HelpText =
                    "The name (and optionally version) of the product using this library, the name of your GitHub organization, or your GitHub username (in that order of preference).")]
        public string ProductInformation { get; set; }

        [Option("repository",
                Required = false,
                Default  = null,
                HelpText = "Set the repository name.")]
        public string Repository { get; set; }

        [Option("owner",
                Required = false,
                Default  = null,
                HelpText = "Set the owner of the repository.")]
        public string Owner { get; set; }

        [Option("token",
                Required = false,
                Default  = null,
                HelpText = "GitHub authenfication token.")]
        public string Token { get; set; }

        [Option('o', "output",
                Required = false,
                Default  = "CHANGELOG.md",
                HelpText = "Set output file name. Default: CHANGELOG.md.")]
        public string Output { get; set; }

        [Option('v', "verbose",
                Required = false,
                Default  = false,
                HelpText = "Be verbose.")]
        public bool Verbose { get; set; }
    }
}
