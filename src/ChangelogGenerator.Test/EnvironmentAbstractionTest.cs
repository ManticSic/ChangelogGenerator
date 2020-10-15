using System;

using NUnit.Framework;


namespace ChangelogGenerator.Test
{
    public class EnvironmentAbstractionTest
    {
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void TestExit(int exitCode)
        {
            int?        usedCode   = null;
            Action<int> exitAction = code => { usedCode = code; };

            IEnvironmentAbstraction environment = new EnvironmentAbstraction(exitAction);

            environment.Exit(exitCode);

            Assert.NotNull(usedCode);
            Assert.AreEqual(exitCode, usedCode);
        }
    }
}
