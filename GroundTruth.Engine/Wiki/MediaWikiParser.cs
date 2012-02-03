using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using GroundTruth.Engine.Wiki;

namespace GroundTruth.Tests
{
    public class MediaWikiParser : IWikiParser
    {
        public WikiParserContext Context
        {
            get { return context; }
        }

        public WikiContentType Next()
        {
            while (true)
            {
                string line = ReadLine();

                // there is no more wiki text to parse
                if (line == null)
                    return context.RegisterContentType(WikiContentType.Eof);

                if (false == regexWhitespaceLine.IsMatch(line))
                    break;
            }

            Match match;
            match = regexSectionHeadingAny.Match (context.CurrentLine);
            if (match.Success)
                return ParseSection (context.CurrentLine, match);

            match = regexTableStart.Match (context.CurrentLine);
            if (match.Success)
                return context.RegisterContentType(WikiContentType.Table);

            return context.RegisterContentType(WikiContentType.Text);
        }

        public void ParseTable (Action<IList<string>> processTableRowDelegate)
        {
            if (context.CurrentType != WikiContentType.Table)
                throw new InvalidOperationException();

            firstTableRowRead = false;
            endOfTableEncountered = false;
            while (false == endOfTableEncountered)
            {
                IList<string> cells = ReadTableDataRow ();
                if (cells == null)
                    break;
                if (processTableRowDelegate != null)
                    processTableRowDelegate (cells);
            }
        }

        public void SetWikiContentSource(Stream stream)
        {
            reader = new StreamReader(stream);
            context = new WikiParserContext();
        }

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
                    if (reader != null)
                        reader.Dispose();
                }

                disposed = true;
            }
        }

        private WikiContentType ParseSection(string line, Match match)
        {
            string sectionName = match.Groups["section"].Value.Trim();

            int i = line.IndexOf('=');
            int j = i;
            while (line[j + 1] == '=')
                j++;

            int sectionLevel = j - i;

            context.RegisterSection(new WikiSection(sectionName, sectionLevel));
            return context.RegisterContentType(WikiContentType.Section);
        }

        private string ReadLine ()
        {
            context.RegisterNewLine(reader.ReadLine());
            return context.CurrentLine;
        }

        private IList<string> ReadTableDataRow ()
        {
            List<string> cells = null;

            bool skipCurrentRow = false;

            while (true)
            {
                string line = ReadLine ();

                if (line == null)
                    return cells;

                line = line.Trim ();

                if (line.StartsWith ("|}", StringComparison.OrdinalIgnoreCase))
                {
                    endOfTableEncountered = true;
                    break;
                }

                if (line.StartsWith ("!", StringComparison.OrdinalIgnoreCase))
                {
                    skipCurrentRow = true;
                    continue;
                }

                // if we are on a new row
                if (line.StartsWith ("|-", StringComparison.OrdinalIgnoreCase))
                {
                    if (false == skipCurrentRow && firstTableRowRead)
                        break;

                    skipCurrentRow = false;
                    firstTableRowRead = true;
                    continue;
                }

                if (false == skipCurrentRow)
                {
                    if (cells == null)
                        cells = new List<string> ();

                    // if we are on a new cell
                    if (line.StartsWith ("|", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] separators = { "||" };
                        string[] columns = line.Substring (1).Split (separators, StringSplitOptions.None);

                        foreach (string column in columns)
                            cells.Add (column.Trim ());
                    }
                    else
                    {
                        // append the contents to the current last cell
                        cells[cells.Count - 1] = String.Format (
                            CultureInfo.InvariantCulture,
                            "{0} {1}",
                            cells[cells.Count - 1],
                            line);
                    }
                }

                // other cases: ignore

                continue;
            }

            return cells;
        }

        private WikiParserContext context;
        private bool disposed;
        private bool endOfTableEncountered;
        private bool firstTableRowRead;
        private StreamReader reader;

        private static readonly Regex regexSectionHeadingAny = new Regex (@"\s*^=+\s*(?<section>[^=]+)\s*=+\s*$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        private static readonly Regex regexTableStart = new Regex (@"\s*\{\|", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        private static readonly Regex regexWhitespaceLine = new Regex (@"^\s*$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
    }
}