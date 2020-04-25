namespace Cake.Issues.DupFinder
{
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
                    continue;
                }

                // Gets all fragments from a duplicate
                var fragments = GetDuplicateFragements(duplicate);
                if (fragments.Count < 2)
                {
                    continue;
                }

                foreach (var fragment in fragments)
                {
                    var builder = new StringBuilder();
                    builder.AppendLine($"Possible duplicate detected (cost {cost}).");
                    builder.AppendLine("The following fragments were found that might be duplicates:");
                    foreach (var possibleDuplicateFragment in fragments.Where(innerDuplicate => !innerDuplicate.Equals(fragment)))
                    {
                        builder.Append($"`{possibleDuplicateFragment.File}` (Line {possibleDuplicateFragment.LineStart} to {possibleDuplicateFragment.LineEnd})");
                    }

                    result.Add(
                        IssueBuilder
                            .NewIssue(builder.ToString(), this)
                            .InFile(fragment.File, fragment.LineStart)
                            .OfRule("dupFinder")
                            .WithPriority(IssuePriority.Warning)
                            .Create());
                }
            }

            return result;
        }

        private static IReadOnlyCollection<DuplicateFragment> GetDuplicateFragements(XContainer duplicate)
        {
            var items = new List<DuplicateFragment>();
            foreach (var issue in duplicate.Descendants("Fragment"))
            {
                if (!TryGetFile(issue, out var file))
                {
                    continue;
                }

                if (!TryGetLine(issue, out var lineStart, out var lineEnd))
                {
                    continue;
                }

                items.Add(new DuplicateFragment(file, lineStart, lineEnd));
            }

            return items;
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

        private class DuplicateFragment
        {
            internal DuplicateFragment(string file, int lineStart, int lineEnd)
            {
                this.File = file; 
                this.LineStart = lineStart;
                this.LineEnd = lineEnd;
            }

            internal string File { get; }

            internal int LineStart { get; }

            internal int LineEnd { get; }
        }
    }
}
