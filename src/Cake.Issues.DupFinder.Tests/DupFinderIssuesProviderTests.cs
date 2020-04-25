namespace Cake.Issues.DupFinder.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Cake.Issues.Testing;
    using Cake.Testing;
    using Shouldly;
    using Xunit;

    public sealed class DupFinderIssuesProviderTests
    {
        public sealed class TheCtor
        {
            [Fact]
            public void Should_Throw_If_Log_Is_Null()
            {
                // Given / When
                var result = Record.Exception(() =>
                    new DupFinderIssuesProvider(
                        null,
                        new DupFinderIssuesSettings("Foo".ToByteArray())));

                // Then
                result.IsArgumentNullException("log");
            }

            [Fact]
            public void Should_Throw_If_IssueProviderSettings_Are_Null()
            {
                // Given / When
                var result = Record.Exception(() => new DupFinderIssuesProvider(new FakeLog(), null));

                // Then
                result.IsArgumentNullException("issueProviderSettings");
            }
        }

        public sealed class TheReadIssuesMethod
        {
            private const string ExpectedIssueMessage =
                "Possible duplicate detected (cost 100).\r\nThe following fragments were found that might be duplicates:\r\n`Src\\Bar.cs` (Line 17 to 233)";

            [Fact]
            public void Should_Read_Issue_Correct()
            {
                // Given
                var fixture = new DupFinderIssuesProviderFixture("DupFinder.xml");

                // When
                var issues = fixture.ReadIssues().ToList();

                // Then
                issues.Count.ShouldBe(4);

                var issueToVerify = issues.Single(p => p.FilePath() == "Src/Foo.cs" && p.Line == 16);
                IssueChecker.Check(
                    issueToVerify,
                    IssueBuilder.NewIssue(
                            ExpectedIssueMessage,
                            "Cake.Issues.DupFinder.DupFinderIssuesProvider",
                            "DupFinder")
                        .InFile(@"Src\Foo.cs", 16)
                        .OfRule("dupFinder")
                        .WithPriority(IssuePriority.Warning)
                        .Create());
            }

            [Fact(Timeout = 50)]
            public void ShouldReadBigFileWith1000DuplicatesInUnder50ms()
            {
                // Given
                var fixture = new DupFinderIssuesProviderFixture("DupFinder1000Duplicates.xml");

                IList<IIssue> issues = new List<IIssue>();

                // When
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10; i++)
                {
                    issues = fixture.ReadIssues().ToList();
                }

                sw.Stop();

                // Then
                issues.Count.ShouldBe(2885);
                sw.ElapsedMilliseconds.ShouldBeLessThan(500);
            }
        }
    }
}
