namespace Cake.Issues.DupFinder
{
    using Cake.Core.IO;

    /// <summary>
    /// Settings for <see cref="DupFinderIssuesAliases"/>.
    /// </summary>
    public class DupFinderIssuesSettings : IssueProviderSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DupFinderIssuesSettings"/> class.
        /// </summary>
        /// <param name="logFilePath">Path to the Inspect Code log file.</param>
        public DupFinderIssuesSettings(FilePath logFilePath)
            : base(logFilePath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DupFinderIssuesSettings"/> class.
        /// </summary>
        /// <param name="logFileContent">Content of the Inspect Code log file.</param>
        public DupFinderIssuesSettings(byte[] logFileContent)
            : base(logFileContent)
        {
        }
    }
}
