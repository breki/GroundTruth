using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GroundTruth.Engine.Wiki;
using GroundTruth.Tests;

namespace GroundTruth.Engine
{
    public abstract class WikiRulesParserBase : IDisposable
    {
        public IWikiParser WikiParser
        {
            get { return wikiParser; }
            set { wikiParser = value; }
        }

        protected static Regex RegexColors
        {
            get { return regexColors; }
        }

        protected static Regex RegexPattern
        {
            get { return regexPattern; }
        }

        protected void CheckForPowerOf2 (int value, string errorMessage)
        {
            switch (value)
            {
                case 1:
                case 2:
                case 4:
                case 8:
                case 16:
                case 32:
                    break;
                default:
                    ThrowParseError (errorMessage);
                    break;
            }
        }

        protected string GetTableRowCell (IList<string> tableRowCells, int cellIndex, bool canBeEmpty, string missingCellErrorMessage)
        {
            if (tableRowCells.Count <= cellIndex)
            {
                if (false == canBeEmpty)
                    ThrowParseError (missingCellErrorMessage);
                else
                    return null;
            }

            string cellText = tableRowCells[cellIndex].Trim ();

            if (false == canBeEmpty && string.IsNullOrEmpty (cellText))
                ThrowParseError (missingCellErrorMessage);

            return cellText;
        }

        protected IList<string> ParseColors (string colorsText)
        {
            Match match = regexColors.Match (colorsText);
            if (false == match.Success)
                ThrowParseError ("Could not parse colors: '{0}'", colorsText);

            List<string> colors = new List<string> ();

            while (match.Success)
            {
                string color = match.Groups["color"].Value;
                colors.Add (color);

                match = match.NextMatch ();
            }

            return colors;
        }

        protected int ParseLevel (string levelText)
        {
            int level = int.Parse (levelText, CultureInfo.InvariantCulture);
            if (level < 12 || level > 24)
                ThrowParseError ("Level out of range: {0}.", level);

            return level;
        }

        protected string ParseLinePattern (string patternText)
        {
            MatchCollection matches = RegexPattern.Matches (patternText);
            if (matches.Count < 1)
                ThrowParseError ("Invalid line pattern: '{0}'", patternText);

            int patternWidth = -1;
            int patternHeight = 0;

            Match match = matches[0];
            Group group = match.Groups["line"];

            StringBuilder pattern = new StringBuilder ();
            foreach (Capture capture in group.Captures)
            {
                string patternLine = capture.Value;
                if (patternLine != null)
                {
                    // all pattern lines have to be of the same length
                    if (patternWidth != -1)
                    {
                        if (patternLine.Length != patternWidth)
                            ThrowParseError ("All pattern lines in a single rule have to be of the same width.");
                    }
                    else
                    {
                        patternWidth = patternLine.Length;
                        CheckForPowerOf2 (patternWidth, "Line pattern width can only be of 1, 2, 4, 8, 16 or 32 characters.");
                    }

                    if (pattern.Length > 0)
                        pattern.Append ('|');
                    pattern.Append (patternLine);
                    patternHeight++;
                }
            }

            if (patternHeight > 32)
                ThrowParseError ("Line patterns can have a maximum height of 32 characters");

            return pattern.ToString ();
        }

        protected int ParseLineWidth (string widthText)
        {
            int width = int.Parse (widthText, CultureInfo.InvariantCulture);
            if (width < 0 || width > 10)
                ThrowParseError ("Line width is out of range: {0}", width);
            return width;
        }

        [SuppressMessage ("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected void SkipToTableAndParseIt (Action<IList<string>> processTableRowDelegate)
        {
            while (true)
            {
                WikiContentType contentType = wikiParser.Next ();
                if (contentType == WikiContentType.Table)
                {
                    wikiParser.ParseTable (processTableRowDelegate);
                    return;
                }

                if (contentType == WikiContentType.Text)
                    continue;

                ThrowParseError ("Wiki table expected");
            }
        }

        protected void ThrowParseError (string errorMessageFormat, params object[] args)
        {
            string errorMessage = String.Format (CultureInfo.InvariantCulture, errorMessageFormat, args);

            throw new ArgumentException (
                String.Format (
                    CultureInfo.InvariantCulture,
                    "{0} (line {1})",
                    errorMessage,
                    wikiParser.Context.LineCounter));
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">If <code>false</code>, cleans up native resources. 
        /// If <code>true</code> cleans up both managed and native resources</param>
        protected virtual void Dispose (bool disposing)
        {
            if (false == disposed)
            {
                if (disposing)
                {
                    wikiParser.Dispose ();
                }

                disposed = true;
            }
        }

        private bool disposed;

        #endregion

        private IWikiParser wikiParser = new MediaWikiParser ();
        private static readonly Regex regexColors = new Regex (@"#(?<color>[0-9a-f]{6})", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        private static readonly Regex regexPattern = new Regex (@"^((?<line>[01]+)\s*)+$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
    }
}