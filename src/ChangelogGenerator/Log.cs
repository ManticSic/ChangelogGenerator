using System;

using ChangelogGenerator.Verbs;


namespace ChangelogGenerator
{
    internal class Log : ILog
    {
        private readonly bool verbose;

        public Log(Options options)
        {
            verbose = options.Verbose;
        }

        public void Info(object value)
        {
            Console.WriteLine(value);
        }

        public void Info(string value)
        {
            Console.WriteLine(value);
        }

        public void VerboseInfo(object value)
        {
            if(verbose)
            {
                Console.WriteLine(value);
            }
        }

        public void VerboseInfo(string value)
        {
            if(verbose)
            {
                Console.WriteLine(value);
            }
        }

        public void Error(object value)
        {
            Console.Error.WriteLine(value);
        }

        public void Error(string value)
        {
            Console.Error.WriteLine(value);
        }

        public void VerboseError(object value)
        {
            if(verbose)
            {
                Console.Error.WriteLine(value);
            }
        }

        public void VerboseError(string value)
        {
            if(verbose)
            {
                Console.Error.WriteLine(value);
            }
        }
    }
}
