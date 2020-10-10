using System;


namespace ChangelogGenerator
{
    internal class EnvironmentAbstraction : IEnvironmentAbstraction
    {
        public void Exit(int exitCode)
        {
            Environment.Exit(exitCode);
        }
    }
}
