using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Octokit;


namespace ChangelogGenerator.Verbs
{
    internal abstract class VerbHandler<T> : IVerbHandler
    {
        protected readonly T                       Options;
        protected readonly IGitHubClient           GitHubClient;
        protected readonly ILog                    Log;
        private readonly   IEnvironmentAbstraction environment;

        protected VerbHandler(T options, IGitHubClient gitHubClient, ILog log, IEnvironmentAbstraction environment)
        {
            Options      = options;
            GitHubClient = gitHubClient;
            Log          = log;

            this.environment = environment;
        }

        protected abstract Task RunAsync();

        protected void ExitWithException([NotNull] string message, [NotNull] Exception exception, ExitCode exitCode)
        {
            Log.Error(message);
            Log.VerboseError(exception);
            Exit(exitCode);
        }

        protected void Exit(ExitCode exitCode)
        {
            if(exitCode != ExitCode.Success)
            {
                Log.Info("The application has unexpectedly terminated. Use --verbose for more information.");
            }

            environment.Exit((int) exitCode);
        }

        void IVerbHandler.Run()
        {
            Task task = RunAsync();
            task.Wait();
        }
    }
}
