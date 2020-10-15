using ChangelogGenerator.Verbs;

using NUnit.Framework;


namespace ChangelogGenerator.Test.Verbs
{
    public class UnknownVerbHandlerTest
    {
        [Test]
        public void TestRun()
        {
            IVerbHandler verbHandler = new UnknownVerbHandler();

            verbHandler.Run();

            Assert.Pass();
        }
    }
}
