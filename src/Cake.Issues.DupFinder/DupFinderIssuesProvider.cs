namespace Cake.Issues.DupFinder
{
    using System.Collections.Generic;
    using Cake.Core.Diagnostics;

    /// <summary>
    /// Provider for issues reported by JetBrains dupFinder.
    /// </summary>
    internal class DupFinderIssuesProvider : BaseConfigurableIssueProvider<DupFinderIssuesSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DupFinderIssuesProvider"/> class.
        /// </summary>
        /// <param name="log">The Cake log context.</param>
        /// <param name="issueProviderSettings">Settings for the issue provider.</param>
        public DupFinderIssuesProvider(ICakeLog log, DupFinderIssuesSettings issueProviderSettings)
            : base(log, issueProviderSettings)
        {
        }

        /// <inheritdoc />
        public override string ProviderName => "DupFinder";

        /// <inheritdoc />
        protected override IEnumerable<IIssue> InternalReadIssues()
        {
            var result = new List<IIssue>();

            //// TODO Implement parsing of log file

            return result;
        }
    }
}
