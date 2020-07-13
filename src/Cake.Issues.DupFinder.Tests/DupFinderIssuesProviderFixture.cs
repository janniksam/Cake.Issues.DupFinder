namespace Cake.Issues.DupFinder.Tests
{
    using Cake.Issues.Testing;

    internal class DupFinderIssuesProviderFixture : BaseConfigurableIssueProviderFixture<DupFinderIssuesProvider, DupFinderIssuesSettings>
    {
        public DupFinderIssuesProviderFixture(string fileResourceName)
            : base(fileResourceName)
        {
        }

        protected override string FileResourceNamespace => "Cake.Issues.DupFinder.Tests.Testfiles.";
    }
}
