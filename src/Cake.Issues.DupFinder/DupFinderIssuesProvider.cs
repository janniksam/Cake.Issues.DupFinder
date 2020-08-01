namespace Cake.Issues.DupFinder
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
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

            var logDocument = XDocument.Parse(this.IssueProviderSettings.LogFileContent.ToStringUsingEncoding());
            foreach (var duplicate in logDocument.Descendants("Duplicate"))
            {
                if (!TryGetCost(duplicate, out var cost))
                {
                    this.Log.Warning(
                        "Cost of the current duplicate could not be determined. Skipped.");
                    continue;
                }

                // Gets all fragments from a duplicate
                var fragments = this.GetDuplicateFragments(duplicate);
                if (fragments.Count < 2)
                {
                    this.Log.Warning(
                        "There are less than two fragments for the current duplicate. " +
                        "A duplicate needs at least two fragment to be an actual duplicate. Skipped.");
                    continue;
                }

                foreach (var fragment in fragments)
                {
                    var identifier = GetIdentifier(cost, fragments, fragment);
                    var message = GetSimpleMessage(cost, fragments, fragment);
                    var htmlMessage = this.GetHtmlMessage(cost, fragments, fragment);
                    var mdMessage = this.GetMarkdownMessage(cost, fragments, fragment);

                    var issue =
                        IssueBuilder
                            .NewIssue(identifier, message, this)
                            .WithMessageInHtmlFormat(htmlMessage)
                            .WithMessageInMarkdownFormat(mdMessage)
                            .InFile(fragment.FilePath, fragment.LineStart, fragment.LineEnd, null, null)
                            .OfRule("dupFinder")
                            .WithPriority(IssuePriority.Warning)
                            .Create();

                    result.Add(issue);
                }
            }

            return result;
        }

        private static string GetIdentifier(int cost, IReadOnlyCollection<DuplicateFragment> fragments, DuplicateFragment fragment)
        {
            var otherFragments =
                fragments.Where(f => !f.Equals(fragment)).OrderBy(f => f.FilePath).Select(p => p.FilePath);
            return $"{fragment.FilePath}-{cost}-{string.Join("-", otherFragments)}";
        }

        private string GetHtmlMessage(int cost, IReadOnlyCollection<DuplicateFragment> fragments, DuplicateFragment fragment)
        {
            var builder = new StringBuilder();
            builder.Append($"Possible duplicate detected (cost {cost}).");
            builder.Append("<br/>The following fragments were found that might be duplicates:");
            foreach (var possibleDuplicateFragment in fragments.Where(f => !f.Equals(fragment)))
            {
                builder.Append("<br/>");

                var link = this.ResolveFileLinkForFragment(possibleDuplicateFragment);
                builder.Append(
                    link == null
                        ? $"<code>{possibleDuplicateFragment.FilePath}</code> (Line {possibleDuplicateFragment.LineStart} to {possibleDuplicateFragment.LineEnd})"
                        : $"<a href='{link}'>{possibleDuplicateFragment.FilePath}</a> (Line {possibleDuplicateFragment.LineStart} to {possibleDuplicateFragment.LineEnd})");
            }

            return builder.ToString();
        }

        private string GetMarkdownMessage(int cost, IReadOnlyCollection<DuplicateFragment> fragments, DuplicateFragment fragment)
        {
            var builder = new StringBuilder();
            builder.Append($"Possible duplicate detected (cost {cost}).\r\n");
            builder.Append("The following fragments were found that might be duplicates:");
            foreach (var possibleDuplicateFragment in fragments.Where(f => !f.Equals(fragment)))
            {
                builder.Append("\r\n");

                var link = this.ResolveFileLinkForFragment(possibleDuplicateFragment);
                builder.Append(
                    link == null
                        ? $"`{possibleDuplicateFragment.FilePath}` (Line {possibleDuplicateFragment.LineStart} to {possibleDuplicateFragment.LineEnd})"
                        : $"[{possibleDuplicateFragment.FilePath}]({link}) (Line {possibleDuplicateFragment.LineStart} to {possibleDuplicateFragment.LineEnd})");
            }

            return builder.ToString();
        }

        private Uri ResolveFileLinkForFragment(DuplicateFragment fragment)
        {
            if (this.Settings?.FileLinkSettings == null)
            {
                return null;
            }

            var issue =
                IssueBuilder
                    .NewIssue(fragment.FilePath, fragment.FilePath, this)
                    .InFile(fragment.FilePath, fragment.LineStart)
                    .Create();

            var resolvedPattern = this.Settings.FileLinkSettings.GetFileLink(issue);
            return resolvedPattern;
        }

        private static string GetSimpleMessage(int cost, IReadOnlyCollection<DuplicateFragment> fragments, DuplicateFragment fragment)
        {
            var builder = new StringBuilder();
            builder.Append($"Possible duplicate detected (cost {cost}).\r\n");
            builder.Append("The following fragments were found that might be duplicates:");
            foreach (var possibleDuplicateFragment in fragments.Where(f => !f.Equals(fragment)))
            {
                builder.Append("\r\n");
                builder.Append(
                    $"\"{possibleDuplicateFragment.FilePath}\" (Line {possibleDuplicateFragment.LineStart} to {possibleDuplicateFragment.LineEnd})");
            }

            return builder.ToString();
        }

        private static bool TryGetCost(XElement duplicate, out int cost)
        {
            cost = 0;

            var costAttr = duplicate.Attribute("Cost");
            if (costAttr == null)
            {
                return false;
            }

            var costValue = costAttr.Value;
            if (string.IsNullOrWhiteSpace(costValue))
            {
                return false;
            }

            cost = int.Parse(costValue, CultureInfo.InvariantCulture);
            return true;
        }

        private static bool TryGetFile(XContainer fragment, out string fileName)
        {
            fileName = string.Empty;

            var fileAttr = fragment.Element("FileName");
            if (fileAttr == null)
            {
                return false;
            }

            fileName = fileAttr.Value;
            return !string.IsNullOrWhiteSpace(fileName);
        }

        private static bool TryGetLine(XContainer fragment, out int lineStart, out int lineEnd)
        {
            lineStart = -1;
            lineEnd = -1;

            var lineRangeElement = fragment.Element("LineRange");
            var lineStartAttr = lineRangeElement?.Attribute("Start");
            var lineEndAttr = lineRangeElement?.Attribute("End");

            var lineStartValue = lineStartAttr?.Value;
            var lineEndValue = lineEndAttr?.Value;
            if (string.IsNullOrWhiteSpace(lineStartValue) ||
                string.IsNullOrWhiteSpace(lineEndValue))
            {
                return false;
            }

            lineStart = int.Parse(lineStartValue, CultureInfo.InvariantCulture);
            lineEnd = int.Parse(lineEndValue, CultureInfo.InvariantCulture);
            return true;
        }

        private IReadOnlyCollection<DuplicateFragment> GetDuplicateFragments(XContainer duplicate)
        {
            var items = new List<DuplicateFragment>();
            foreach (var issue in duplicate.Descendants("Fragment"))
            {
                if (!TryGetFile(issue, out var file))
                {
                    this.Log.Warning("FilePath of the current Fragment could not be determined. Skipped.");
                    continue;
                }

                if (!TryGetLine(issue, out var lineStart, out var lineEnd))
                {
                    this.Log.Warning("The location of the current Fragment could not be determined. Skipped.");
                    continue;
                }

                items.Add(new DuplicateFragment(file, lineStart, lineEnd));
            }

            return items;
        }

        private class DuplicateFragment
        {
            internal DuplicateFragment(string filePath, int lineStart, int lineEnd)
            {
                this.FilePath = filePath;
                this.LineStart = lineStart;
                this.LineEnd = lineEnd;
            }

            internal string FilePath { get; }

            internal int LineStart { get; }

            internal int LineEnd { get; }
        }
    }
}
