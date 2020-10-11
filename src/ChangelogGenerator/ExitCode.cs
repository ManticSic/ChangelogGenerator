namespace ChangelogGenerator
{
    internal enum ExitCode
    {
        Success                 = 0,
        FailedToLoadData        = 100,
        FailedToCreateChangelog = 200,
        FailedToWriteFile       = 300,
    }
}
