using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using ChangelogGenerator.Logging;
using ChangelogGenerator.Test.Builder;
using ChangelogGenerator.Verbs;

using JetBrains.Annotations;

using Moq;

using NUnit.Framework;

using Octokit;

using static NUnit.Framework.Assert;


namespace ChangelogGenerator.Test.Verbs
{
    internal class VerbHandlerTest
    {
        [Test]
        [TestCase(ExitCode.Success)]
        [TestCase(ExitCode.FailedToLoadData)]
        [TestCase(ExitCode.FailedToCreateChangelog)]
        [TestCase(ExitCode.FailedToWriteFile)]
        public void TestExit(ExitCode exitCode)
        {
            Options                       options            = new Options();
            Mock<ILog>                    logMock            = new Mock<ILog>();
            Mock<IEnvironmentAbstraction> environmentMock    = new Mock<IEnvironmentAbstraction>();
            Mock<IFileSystem>             fileSystemMock     = new Mock<IFileSystem>();
            Mock<IGitHubClient>           gitHubClientMock   = new Mock<IGitHubClient>();
            Mock<IMarkdownParser>         markdownParserMock = new Mock<IMarkdownParser>();

            TestVerbHandler verbHandler = new TestVerbHandler(options, logMock.Object, environmentMock.Object,
                                                              fileSystemMock.Object, gitHubClientMock.Object,
                                                              markdownParserMock.Object);

            verbHandler.Exit(exitCode);

            if(exitCode != ExitCode.Success)
            {
                logMock.Verify(mock => mock.Info(It.IsAny<string>()), Times.Once);
            }

            environmentMock.Verify(mock => mock.Exit((int) exitCode), Times.Once);

            logMock.VerifyNoOtherCalls();
            environmentMock.VerifyNoOtherCalls();
            fileSystemMock.VerifyNoOtherCalls();
            gitHubClientMock.VerifyNoOtherCalls();
            markdownParserMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TestLoadPullRequestsAsync()
        {
            Options                       options            = new Options();
            Mock<ILog>                    logMock            = new Mock<ILog>();
            Mock<IEnvironmentAbstraction> environmentMock    = new Mock<IEnvironmentAbstraction>();
            Mock<IFileSystem>             fileSystemMock     = new Mock<IFileSystem>();
            Mock<IGitHubClient>           gitHubClientMock   = new Mock<IGitHubClient>();
            Mock<IMarkdownParser>         markdownParserMock = new Mock<IMarkdownParser>();

            Milestone milestone = new MilestoneBuilder
                                  {
                                      Title = "v1.0.0",
                                      ClosedAt = DateTimeOffset.ParseExact("15.09.1993", "dd.MM.yyyy",
                                                                           CultureInfo.InvariantCulture),
                                  }.Build();
            PullRequest pullRequest = new PullRequestBuilder
                                      {
                                          Milestone = milestone,
                                          Body      = File.ReadAllText("./Assets/description_at-the-middle.md"),
                                      }.Build();

            gitHubClientMock.Setup(mock => mock.Repository.Get(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.FromResult(new Repository(1337)));
            gitHubClientMock.Setup(mock => mock.PullRequest.GetAllForRepository(It.IsAny<long>(), It.IsAny<PullRequestRequest>()))
                            .Returns(Task.FromResult(
                                         new List<PullRequest> {pullRequest}.AsReadOnly() as IReadOnlyList<PullRequest>));

            TestVerbHandler verbHandler = new TestVerbHandler(options, logMock.Object, environmentMock.Object,
                                                              fileSystemMock.Object, gitHubClientMock.Object,
                                                              markdownParserMock.Object);

            IReadOnlyList<PullRequest> result = await verbHandler.LoadPullRequestsAsync();

            NotNull(result);
            AreEqual(1, result.Count);
            AreEqual(pullRequest, result[0]);
        }

        [Test]
        public async Task TestLoadPullRequestsAsync_WithPredicate()
        {
            Options                       options            = new Options();
            Mock<ILog>                    logMock            = new Mock<ILog>();
            Mock<IEnvironmentAbstraction> environmentMock    = new Mock<IEnvironmentAbstraction>();
            Mock<IFileSystem>             fileSystemMock     = new Mock<IFileSystem>();
            Mock<IGitHubClient>           gitHubClientMock   = new Mock<IGitHubClient>();
            Mock<IMarkdownParser>         markdownParserMock = new Mock<IMarkdownParser>();

            Milestone milestone = new MilestoneBuilder
                                  {
                                      Title = "v1.0.0",
                                      ClosedAt = DateTimeOffset.ParseExact("15.09.1993", "dd.MM.yyyy",
                                                                           CultureInfo.InvariantCulture),
                                  }.Build();
            PullRequest pullRequest1 = new PullRequestBuilder
                                       {
                                           Milestone = milestone,
                                           Body      = File.ReadAllText("./Assets/description_at-the-middle.md"),
                                       }.Build();
            PullRequest pullRequest2 = new PullRequestBuilder
                                       {
                                           Milestone = milestone,
                                       }.Build();
            PullRequest pullRequest3 = new PullRequestBuilder
                                       {
                                           Body = File.ReadAllText("./Assets/description_at-the-middle.md"),
                                       }.Build();
            PullRequest pullRequest4 = new PullRequestBuilder
                                       {
                                           Milestone = milestone,
                                           Body      = File.ReadAllText("./Assets/description_at-the-middle.md"),
                                       }.Build();

            gitHubClientMock.Setup(mock => mock.Repository.Get(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.FromResult(new Repository(1337)));
            gitHubClientMock.Setup(mock => mock.PullRequest.GetAllForRepository(It.IsAny<long>(), It.IsAny<PullRequestRequest>()))
                            .Returns(Task.FromResult(
                                         new List<PullRequest>
                                             {pullRequest1, pullRequest2, pullRequest3, pullRequest4}
                                            .AsReadOnly() as IReadOnlyList<PullRequest>));

            int predicateCounter = 0;
            Func<PullRequest, bool> predicate = pullRequest =>
                                                {
                                                    predicateCounter++;

                                                    return pullRequest.Milestone != null
                                                        && !String.IsNullOrWhiteSpace(pullRequest.Body);
                                                };

            TestVerbHandler verbHandler = new TestVerbHandler(options, logMock.Object, environmentMock.Object,
                                                              fileSystemMock.Object, gitHubClientMock.Object,
                                                              markdownParserMock.Object);

            IReadOnlyList<PullRequest> result = await verbHandler.LoadPullRequestsAsync(predicate);

            NotNull(result);
            AreEqual(4, predicateCounter);
            AreEqual(2, result.Count);
            AreEqual(pullRequest1, result[0]);
            AreEqual(pullRequest4, result[1]);
        }

        [Test]
        public async Task TestLoadPullRequestsAsync_FailedToLoadRepository()
        {
            Options                       options            = new Options();
            Mock<ILog>                    logMock            = new Mock<ILog>();
            Mock<IEnvironmentAbstraction> environmentMock    = new Mock<IEnvironmentAbstraction>();
            Mock<IFileSystem>             fileSystemMock     = new Mock<IFileSystem>();
            Mock<IGitHubClient>           gitHubClientMock   = new Mock<IGitHubClient>();
            Mock<IMarkdownParser>         markdownParserMock = new Mock<IMarkdownParser>();

            gitHubClientMock.Setup(mock => mock.Repository.Get(It.IsAny<string>(), It.IsAny<string>()))
                            .Throws(new ApiException("", HttpStatusCode.Unauthorized));

            TestVerbHandler verbHandler = new TestVerbHandler(options, logMock.Object, environmentMock.Object,
                                                              fileSystemMock.Object, gitHubClientMock.Object,
                                                              markdownParserMock.Object);

            IReadOnlyList<PullRequest> result = await verbHandler.LoadPullRequestsAsync();

            IsEmpty(result);
            logMock.Verify(mock => mock.Error(It.IsAny<string>()), Times.Once);
            logMock.Verify(mock => mock.VerboseError(It.IsAny<Exception>()), Times.Once);
            logMock.Verify(mock => mock.Info(It.IsAny<string>()), Times.Once);
            environmentMock.Verify(mock => mock.Exit((int) ExitCode.FailedToLoadData), Times.Once);
            gitHubClientMock.Verify(mock => mock.Repository.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            logMock.VerifyNoOtherCalls();
            environmentMock.VerifyNoOtherCalls();
            gitHubClientMock.VerifyNoOtherCalls();
            markdownParserMock.VerifyNoOtherCalls();
            fileSystemMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TestLoadPullRequestsAsync_FailedToLoadPullrequests()
        {
            Options                       options            = new Options();
            Mock<ILog>                    logMock            = new Mock<ILog>();
            Mock<IEnvironmentAbstraction> environmentMock    = new Mock<IEnvironmentAbstraction>();
            Mock<IFileSystem>             fileSystemMock     = new Mock<IFileSystem>();
            Mock<IGitHubClient>           gitHubClientMock   = new Mock<IGitHubClient>();
            Mock<IMarkdownParser>         markdownParserMock = new Mock<IMarkdownParser>();

            gitHubClientMock.Setup(mock => mock.Repository.Get(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.FromResult(new Repository(1337)));
            gitHubClientMock.Setup(mock => mock.PullRequest.GetAllForRepository(It.IsAny<long>(), It.IsAny<PullRequestRequest>()))
                            .Throws(new ApiException("", HttpStatusCode.Unauthorized));

            TestVerbHandler verbHandler = new TestVerbHandler(options, logMock.Object, environmentMock.Object,
                                                              fileSystemMock.Object, gitHubClientMock.Object,
                                                              markdownParserMock.Object);

            IReadOnlyList<PullRequest> result = await verbHandler.LoadPullRequestsAsync();

            IsEmpty(result);
            logMock.Verify(mock => mock.Error(It.IsAny<string>()), Times.Once);
            logMock.Verify(mock => mock.VerboseError(It.IsAny<Exception>()), Times.Once);
            logMock.Verify(mock => mock.Info(It.IsAny<string>()), Times.Once);
            environmentMock.Verify(mock => mock.Exit((int) ExitCode.FailedToLoadData), Times.Once);
            gitHubClientMock.Verify(mock => mock.Repository.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            gitHubClientMock.Verify(
                mock => mock.PullRequest.GetAllForRepository(It.IsAny<long>(), It.IsAny<PullRequestRequest>()), Times.Once);
            logMock.VerifyNoOtherCalls();
            environmentMock.VerifyNoOtherCalls();
            gitHubClientMock.VerifyNoOtherCalls();
            markdownParserMock.VerifyNoOtherCalls();
            fileSystemMock.VerifyNoOtherCalls();
        }

        // todo whats the best way to write a test for this method?!
        // [Test]
        // public void TestCreateChangelogMarkdown()
        // {
        //     Options                       options            = new Options();
        //     Mock<ILog>                    logMock            = new Mock<ILog>();
        //     Mock<IEnvironmentAbstraction> environmentMock    = new Mock<IEnvironmentAbstraction>();
        //     Mock<IFileSystem>             fileSystemMock     = new Mock<IFileSystem>();
        //     Mock<IGitHubClient>           gitHubClientMock   = new Mock<IGitHubClient>();
        //     Mock<IMarkdownParser>         markdownParserMock = new Mock<IMarkdownParser>();
        //
        //     markdownParserMock.Setup(mock => mock.GetChangelogBy(It.IsAny<VersionEntry>()))
        //                       .Returns();
        //
        //     IReadOnlyList<PullRequest> pullRequests = new List<PullRequest>{};
        //
        //     TestVerbHandler verbHandler = new TestVerbHandler(options, logMock.Object, environmentMock.Object, fileSystemMock.Object, gitHubClientMock.Object, markdownParserMock.Object);
        //
        //     string result = verbHandler.CreateChangelogMarkdown(pullRequests);
        // }

        [Test]
        public async Task TestWriteChangelogAsync()
        {
            Options                       options            = new Options();
            Mock<ILog>                    logMock            = new Mock<ILog>();
            Mock<IEnvironmentAbstraction> environmentMock    = new Mock<IEnvironmentAbstraction>();
            Mock<IFileSystem>             fileSystemMock     = new Mock<IFileSystem>();
            Mock<IGitHubClient>           gitHubClientMock   = new Mock<IGitHubClient>();
            Mock<IMarkdownParser>         markdownParserMock = new Mock<IMarkdownParser>();

            fileSystemMock
               .Setup(mock => mock.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(Task.Run(() => { }));

            TestVerbHandler verbHandler = new TestVerbHandler(options, logMock.Object, environmentMock.Object,
                                                              fileSystemMock.Object, gitHubClientMock.Object,
                                                              markdownParserMock.Object);

            await verbHandler.WriteChangelogAsync(String.Empty);

            fileSystemMock.Verify(
                mock => mock.File.WriteAllTextAsync("CHANGELOG.md", String.Empty, It.IsAny<CancellationToken>()));
            logMock.VerifyNoOtherCalls();
            environmentMock.VerifyNoOtherCalls();
            gitHubClientMock.VerifyNoOtherCalls();
            markdownParserMock.VerifyNoOtherCalls();
            fileSystemMock.VerifyNoOtherCalls();
        }

        [Test]
        [TestCaseSource(nameof(WriteChangelogAsync_FailedToWrite_TestCases))]
        public async Task TestWriteChangelogAsync_FailedToWrite(Exception exception)
        {
            Options                       options            = new Options();
            Mock<ILog>                    logMock            = new Mock<ILog>();
            Mock<IEnvironmentAbstraction> environmentMock    = new Mock<IEnvironmentAbstraction>();
            Mock<IFileSystem>             fileSystemMock     = new Mock<IFileSystem>();
            Mock<IGitHubClient>           gitHubClientMock   = new Mock<IGitHubClient>();
            Mock<IMarkdownParser>         markdownParserMock = new Mock<IMarkdownParser>();

            fileSystemMock
               .Setup(mock => mock.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Throws(exception);

            TestVerbHandler verbHandler = new TestVerbHandler(options, logMock.Object, environmentMock.Object,
                                                              fileSystemMock.Object, gitHubClientMock.Object,
                                                              markdownParserMock.Object);

            await verbHandler.WriteChangelogAsync(String.Empty);

            fileSystemMock.Verify(
                mock => mock.File.WriteAllTextAsync("CHANGELOG.md", String.Empty, It.IsAny<CancellationToken>()));
            logMock.Verify(mock => mock.Info(It.IsAny<string>()), Times.Once);
            logMock.Verify(mock => mock.Error(It.IsAny<string>()), Times.Once);
            logMock.Verify(mock => mock.VerboseError(It.IsAny<Exception>()), Times.Once);
            environmentMock.Verify(mock => mock.Exit((int) ExitCode.FailedToWriteFile), Times.Once);
            logMock.VerifyNoOtherCalls();
            environmentMock.VerifyNoOtherCalls();
            gitHubClientMock.VerifyNoOtherCalls();
            markdownParserMock.VerifyNoOtherCalls();
            fileSystemMock.VerifyNoOtherCalls();
        }

        private static IEnumerable<Exception> WriteChangelogAsync_FailedToWrite_TestCases()
        {
            yield return new ArgumentException();
            yield return new PathTooLongException();
            yield return new DirectoryNotFoundException();
            yield return new NotSupportedException();
            yield return new SecurityException();
            yield return new IOException();
            yield return new UnauthorizedAccessException();
        }

        private class TestVerbHandler : VerbHandler<Options>
        {
            public TestVerbHandler(Options options,
                                   ILog log,
                                   IEnvironmentAbstraction environment,
                                   IFileSystem fileSystem,
                                   IGitHubClient gitHubClient,
                                   IMarkdownParser markdownParser) : base(options, log, environment, fileSystem, gitHubClient,
                                                                          markdownParser)
            {
            }

            public void Exit(ExitCode exitCode)
            {
                base.Exit(exitCode);
            }

            public Task<IReadOnlyList<PullRequest>> LoadPullRequestsAsync(Func<PullRequest, bool> predicate)
            {
                return base.LoadPullRequestsAsync(predicate);
            }

            public Task<IReadOnlyList<PullRequest>> LoadPullRequestsAsync()
            {
                return base.LoadPullRequestsAsync();
            }

            public string CreateChangelogMarkdown([NotNull] [ItemNotNull] IReadOnlyList<PullRequest> pullRequests)
            {
                return base.CreateChangelogMarkdown(pullRequests);
            }

            public Task WriteChangelogAsync([NotNull] string changelog)
            {
                return base.WriteChangelogAsync(changelog);
            }

            protected override Task RunAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}
