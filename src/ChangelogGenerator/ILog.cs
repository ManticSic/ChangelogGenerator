namespace ChangelogGenerator
{
    internal interface ILog
    {
        public void Info(object value);

        public void Info(string value);

        public void VerboseInfo(object value);

        public void VerboseInfo(string value);

        public void Error(object value);

        public void Error(string value);

        public void VerboseError(object value);

        public void VerboseError(string value);
    }
}
