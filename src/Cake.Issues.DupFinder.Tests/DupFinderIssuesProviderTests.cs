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
            private const string ExpectedIssueIdentifier =
                "Src\\Foo.cs-100-Src\\Bar.cs-Src\\FooBar.cs";
 
            private const string ExpectedIssueMessage =
                "Possible duplicate detected (cost 100).\r\nThe following fragments were found that might be duplicates:\r\n" +
                "\"Src\\Bar.cs\" (Line 17 to 233)\r\n" +
                "\"Src\\FooBar.cs\" (Line 18 to 234)";

            private const string ExpectedIssueMarkdownMessage =
                "Possible duplicate detected (cost 100).\r\n" +
                "The following fragments were found that might be duplicates:\r\n" +
                "`Src\\Bar.cs` (Line 17 to 233)\r\n" +
                "`Src\\FooBar.cs` (Line 18 to 234)";

            private const string ExpectedIssueHtmlMessage =
                "Possible duplicate detected (cost 100).<br/>" +
                "The following fragments were found that might be duplicates:<br/>" +
                "<code>Src\\Bar.cs</code> (Line 17 to 233)<br/>" +
                "<code>Src\\FooBar.cs</code> (Line 18 to 234)";

            [Fact]
            public void Should_Read_Issue_Correct()
            {
                // Given
                var fixture = new DupFinderIssuesProviderFixture("DupFinder.xml");

                // When
                var issues = fixture.ReadIssues().ToList();

                // Then
                issues.Count.ShouldBe(5);
                issues.Count(i => 
                    i.FilePath() == "Src/Foo.cs" && 
                    i.Line == 16 && i.EndLine == 232).ShouldBe(1);

                var issueToVerify = issues.Single(i =>
                    i.FilePath() == "Src/Foo.cs" &&
                    i.Line == 16 && i.EndLine == 232);

                IssueChecker.Check(
                    issueToVerify,
                    IssueBuilder.NewIssue(
                            ExpectedIssueIdentifier,
                            ExpectedIssueMessage,
                            "Cake.Issues.DupFinder.DupFinderIssuesProvider",
                            "DupFinder")
                        .WithMessageInMarkdownFormat(ExpectedIssueMarkdownMessage)
                        .WithMessageInHtmlFormat(ExpectedIssueHtmlMessage)
                        .InFile(@"Src\Foo.cs", 16, 232, null, null)
                        .OfRule("dupFinder")
                        .WithPriority(IssuePriority.Warning)
                        .Create());
            }

            [Fact(Timeout = 50)]
            public void ShouldReadBigFileWith1000DuplicatesInUnder100ms()
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
                sw.ElapsedMilliseconds.ShouldBeLessThan(1000);
            }
        }
    }
}
