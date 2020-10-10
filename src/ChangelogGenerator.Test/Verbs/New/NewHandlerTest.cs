using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

using ChangelogGenerator.Test.Builder;
using ChangelogGenerator.Verbs;
using ChangelogGenerator.Verbs.New;

using Moq;

using NUnit.Framework;

using Octokit;


namespace ChangelogGenerator.Test.Verbs.New
{
    public class NewHandlerTest
    {
        [Test]
        public void TestRun()
        {
            NewOptions options = new NewOptions
                                 {
                                     Owner      = "ManticSic",
                                     Repository = "ChangelogGenerator"
                                 };

            Mock<IGitHubClient>           client                 = new Mock<IGitHubClient>();
            Mock<ILog>                    log                    = new Mock<ILog>();
            Mock<IMarkdownParser>         markdownParser         = new Mock<IMarkdownParser>();
            Mock<IFileSystem>             fileSystem             = new Mock<IFileSystem>();
            Mock<IEnvironmentAbstraction> environmentAbstraction = new Mock<IEnvironmentAbstraction>();

            Milestone milestone = new MilestoneBuilder
                                  {
                                      Title = "v1.0.0",
                                      ClosedAt = DateTimeOffset.ParseExact("15.09.1993", "dd.MM.yyyy",
                                                                           CultureInfo.InvariantCulture)
                                  }.Build();

            PullRequest pullRequest = new PullRequestBuilder
                                      {
                                          Milestone = milestone,
                                          Body      = File.ReadAllText("./Assets/description_at-the-middle.md")
                                      }.Build();

            IDictionary<string, IList<string>> changelogEntries = new Dictionary<string, IList<string>>
                                                                  {
                                                                      {
                                                                          "added", new List<string>
                                                                                   {
                                                                                       "an added entry"
                                                                                   }
                                                                      },
                                                                      {
                                                                          "changed", new List<string>
                                                                                     {
                                                                                         "a changed entry"
                                                                                     }
                                                                      },
                                                                      {
                                                                          "deprecated", new List<string>
                                                                              {
                                                                                  "a deprecated entry"
                                                                              }
                                                                      },
                                                                      {
                                                                          "fixed", new List<string>
                                                                                   {
                                                                                       "a fixed entry"
                                                                                   }
                                                                      },
                                                                      {
                                                                          "removed", new List<string>
                                                                                     {
                                                                                         "a removed entry"
                                                                                     }
                                                                      },
                                                                      {
                                                                          "security", new List<string>
                                                                              {
                                                                                  "a security entry"
                                                                              }
                                                                      }
                                                                  };

            string changelogText =
                "## v1.0.0 (1993-09-15)\n\n### added\n\n* an added entry\n\n### changed\n\n* a changed entry\n\n### deprecated\n\n* a deprecated entry\n\n### fixed\n\n* a fixed entry\n\n### removed\n\n* a removed entry\n\n### security\n\n* a security entry\n\n### added\n\n* an added entry";

            client.Setup(mock => mock.Repository.Get(It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(Task.FromResult(new Repository(1337)));
            client.Setup(mock => mock.PullRequest.GetAllForRepository(It.IsAny<long>(), It.IsAny<PullRequestRequest>()))
                  .Returns(Task.FromResult(new List<PullRequest>
                                           {
                                               pullRequest
                                           }.AsReadOnly() as IReadOnlyList<PullRequest>));

            markdownParser.Setup(mock => mock.GetChangelogEntries(It.IsAny<string>()))
                          .Returns(changelogEntries);
            markdownParser.Setup(mock => mock.GetChangelogBy(It.IsAny<VersionEntry>()))
                          .Returns(changelogText);

            Mock<IFile> file = new Mock<IFile>();

            fileSystem.Setup(mock => mock.File)
                      .Returns(file.Object);
            file.Setup(mock => mock.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => { }));

            IVerbHandler verbHandler = new NewHandler(options,
                                                      client.Object,
                                                      log.Object,
                                                      markdownParser.Object,
                                                      fileSystem.Object,
                                                      environmentAbstraction.Object);

            verbHandler.Run();

            client.Verify(mock => mock.Repository.Get(options.Owner, options.Repository), Times.Once);
            client.Verify(mock => mock.PullRequest.GetAllForRepository(1337, It.IsAny<PullRequestRequest>()), Times.Once);

            fileSystem.Verify(mock => mock.File.WriteAllTextAsync("CHANGELOG.md", changelogText, It.IsAny<CancellationToken>()),
                              Times.Once);

            environmentAbstraction.Verify(mock => mock.Exit(0), Times.Once);
        }

        // todo test error cases
        // trouble handling Environment.exit... application does not exit
    }
}
