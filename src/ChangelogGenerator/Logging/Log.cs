using System.IO;

using ChangelogGenerator.Verbs;

using Unity;

using UnityContainerAttributeRegistration.Attribute;


namespace ChangelogGenerator.Logging
{
    [RegisterType(typeof(ILog))]
    internal class Log : ILog
    {
        private readonly TextWriter stdout;
        private readonly TextWriter stderr;
        private readonly bool       verbose;

        public Log(Options options, [Dependency("stdout")] TextWriter stdout, [Dependency("stderr")] TextWriter stderr)
        {
            this.stdout = stdout;
            this.stderr = stderr;
            verbose     = options.Verbose;
        }

        public void Info(object value)
        {
            stdout.WriteLine(value);
        }

        public void Info(string value)
        {
            stdout.WriteLine(value);
        }

        public void VerboseInfo(object value)
        {
            if(verbose)
            {
                stdout.WriteLine(value);
            }
        }

        public void VerboseInfo(string value)
        {
            if(verbose)
            {
                stdout.WriteLine(value);
            }
        }

        public void Error(object value)
        {
            stderr.WriteLine(value);
        }

        public void Error(string value)
        {
            stderr.WriteLine(value);
        }

        public void VerboseError(object value)
        {
            if(verbose)
            {
                stderr.WriteLine(value);
            }
        }

        public void VerboseError(string value)
        {
            if(verbose)
            {
                stderr.WriteLine(value);
            }
        }
    }
}
