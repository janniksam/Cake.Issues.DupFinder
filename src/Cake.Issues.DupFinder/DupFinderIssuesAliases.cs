namespace Cake.Issues.DupFinder
{
    using Cake.Core;
    using Cake.Core.Annotations;
    using Cake.Core.IO;

    /// <summary>
    /// Contains functionality for reading issues from JetBrains dupFinder log files.
    /// </summary>
    [CakeAliasCategory(IssuesAliasConstants.MainCakeAliasCategory)]
    public static class DupFinderIssuesAliases
    {
        /// <summary>
        /// Gets the name of the dupFinder issue provider.
        /// This name can be used to identify issues based on the <see cref="IIssue.ProviderType"/> property.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Name of the dupFinder issue provider.</returns>
        [CakePropertyAlias]
        [CakeAliasCategory(IssuesAliasConstants.IssueProviderCakeAliasCategory)]
        public static string DupFinderIssuesProviderTypeName(
            this ICakeContext context)
        {
            context.NotNull(nameof(context));

            return typeof(DupFinderIssuesProvider).FullName;
        }

        /// <summary>
        /// Gets an instance of a provider for issues reported by JetBrains dupFinder using a log file from disk.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logFilePath">Path to the dupFinder log file.</param>
        /// <returns>Instance of a provider for issues reported by JetBrains dupFinder.</returns>
        /// <example>
        /// <para>Read issues reported by JetBrains dupFinder:</para>
        /// <code>
        /// <![CDATA[
        ///     var issues =
        ///         ReadIssues(
        ///             DupFinderIssuesFromFilePath(@"c:\build\DupFinder.xml"),
        ///             @"c:\repo");
        /// ]]>
        /// </code>
        /// </example>
        [CakeMethodAlias]
        [CakeAliasCategory(IssuesAliasConstants.IssueProviderCakeAliasCategory)]
        public static IIssueProvider DupFinderIssuesFromFilePath(
            this ICakeContext context,
            FilePath logFilePath)
        {
            context.NotNull(nameof(context));
            logFilePath.NotNull(nameof(logFilePath));

            return context.DupFinderIssues(new DupFinderIssuesSettings(logFilePath));
        }

        /// <summary>
        /// Gets an instance of a provider for issues reported by JetBrains dupFinder using log file content.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logFileContent">Content of the dupFinder log file.</param>
        /// <returns>Instance of a provider for issues reported by JetBrains dupFinder.</returns>
        /// <example>
        /// <para>Read issues reported by JetBrains dupFinder:</para>
        /// <code>
        /// <![CDATA[
        ///     var issues =
        ///         ReadIssues(
        ///             DupFinderIssuesFromContent(logFileContent)),
        ///             @"c:\repo");
        /// ]]>
        /// </code>
        /// </example>
        [CakeMethodAlias]
        [CakeAliasCategory(IssuesAliasConstants.IssueProviderCakeAliasCategory)]
        public static IIssueProvider DupFinderIssuesFromContent(
            this ICakeContext context,
            string logFileContent)
        {
            context.NotNull(nameof(context));
            logFileContent.NotNullOrWhiteSpace(nameof(logFileContent));

            return context.DupFinderIssues(new DupFinderIssuesSettings(logFileContent.ToByteArray()));
        }

        /// <summary>
        /// Gets an instance of a provider for issues reported by JetBrains dupFinder using specified settings.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="settings">Settings for reading the dupFinder log.</param>
        /// <returns>Instance of a provider for issues reported by JetBrains dupFinder.</returns>
        /// <example>
        /// <para>Read issues reported by JetBrains dupFinder:</para>
        /// <code>
        /// <![CDATA[
        ///     var settings =
        ///         new DupFinderIssuesSettings(
        ///             @"c:\build\DupFinder.xml));
        ///
        ///     var issues =
        ///         ReadIssues(
        ///             DupFinderIssues(settings),
        ///             @"c:\repo");
        /// ]]>
        /// </code>
        /// </example>
        [CakeMethodAlias]
        [CakeAliasCategory(IssuesAliasConstants.IssueProviderCakeAliasCategory)]
        public static IIssueProvider DupFinderIssues(
            this ICakeContext context,
            DupFinderIssuesSettings settings)
        {
            context.NotNull(nameof(context));
            settings.NotNull(nameof(settings));

            return new DupFinderIssuesProvider(context.Log, settings);
        }
    }
}
