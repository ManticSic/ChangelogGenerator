using System.IO;

using ChangelogGenerator.Logging;
using ChangelogGenerator.Verbs;

using Moq;

using NUnit.Framework;


namespace ChangelogGenerator.Test.Logging
{
    public class LogTest
    {
        [Test]
        public void TestInfoObject()
        {
            Options         options = new Options{Verbose = false};
            Mock<TextWriter> stdout = new Mock<TextWriter>();
            Mock<TextWriter> stderr = new Mock<TextWriter>();

            ILog log = new Log(options, stdout.Object, stderr.Object);

            object obj = new { };
            log.Info(obj);
            log.VerboseInfo(obj);

            stdout.Verify(mock => mock.WriteLine(obj), Times.Once);
            stdout.VerifyNoOtherCalls();
        }

        [Test]
        public void TestInfoString()
        {
            Options          options = new Options{Verbose = false};
            Mock<TextWriter> stdout  = new Mock<TextWriter>();
            Mock<TextWriter> stderr  = new Mock<TextWriter>();

            ILog log = new Log(options, stdout.Object, stderr.Object);

            string str = "Hello World";
            log.Info(str);
            log.VerboseInfo(str);

            stdout.Verify(mock => mock.WriteLine(str), Times.Once);
            stdout.VerifyNoOtherCalls();
        }
        [Test]
        public void TestErrorObject()
        {
            Options          options = new Options{Verbose = false};
            Mock<TextWriter> stdout  = new Mock<TextWriter>();
            Mock<TextWriter> stderr  = new Mock<TextWriter>();

            ILog log = new Log(options, stdout.Object, stderr.Object);

            object obj = new { };
            log.Error(obj);
            log.VerboseError(obj);

            stderr.Verify(mock => mock.WriteLine(obj), Times.Once);
            stderr.VerifyNoOtherCalls();
        }

        [Test]
        public void TestErrorString()
        {
            Options          options = new Options{Verbose = false};
            Mock<TextWriter> stdout  = new Mock<TextWriter>();
            Mock<TextWriter> stderr  = new Mock<TextWriter>();

            ILog log = new Log(options, stdout.Object, stderr.Object);

            string str = "Hello World";
            log.Error(str);
            log.VerboseError(str);

            stderr.Verify(mock => mock.WriteLine(str), Times.Once);
            stderr.VerifyNoOtherCalls();
        }
        [Test]
        public void TestInfoObject_Verbose()
        {
            Options          options = new Options{Verbose = true};
            Mock<TextWriter> stdout  = new Mock<TextWriter>();
            Mock<TextWriter> stderr  = new Mock<TextWriter>();

            ILog log = new Log(options, stdout.Object, stderr.Object);

            object obj = new { };
            log.Info(obj);
            log.VerboseInfo(obj);

            stdout.Verify(mock => mock.WriteLine(obj), Times.Exactly(2));
            stdout.VerifyNoOtherCalls();
        }

        [Test]
        public void TestInfoString_Verbose()
        {
            Options          options = new Options{Verbose = true};
            Mock<TextWriter> stdout  = new Mock<TextWriter>();
            Mock<TextWriter> stderr  = new Mock<TextWriter>();

            ILog log = new Log(options, stdout.Object, stderr.Object);

            string str = "Hello World";
            log.Info(str);
            log.VerboseInfo(str);

            stdout.Verify(mock => mock.WriteLine(str), Times.Exactly(2));
            stdout.VerifyNoOtherCalls();
        }
        [Test]
        public void TestErrorObject_Verbose()
        {
            Options          options = new Options{Verbose = true};
            Mock<TextWriter> stdout  = new Mock<TextWriter>();
            Mock<TextWriter> stderr  = new Mock<TextWriter>();

            ILog log = new Log(options, stdout.Object, stderr.Object);

            object obj = new { };
            log.Error(obj);
            log.VerboseError(obj);

            stderr.Verify(mock => mock.WriteLine(obj), Times.Exactly(2));
            stderr.VerifyNoOtherCalls();
        }

        [Test]
        public void TestErrorString_Verbose()
        {
            Options          options = new Options{Verbose = true};
            Mock<TextWriter> stdout  = new Mock<TextWriter>();
            Mock<TextWriter> stderr  = new Mock<TextWriter>();

            ILog log = new Log(options, stdout.Object, stderr.Object);

            string str = "Hello World";
            log.Error(str);
            log.VerboseError(str);

            stderr.Verify(mock => mock.WriteLine(str), Times.Exactly(2));
            stderr.VerifyNoOtherCalls();
        }
    }
}
