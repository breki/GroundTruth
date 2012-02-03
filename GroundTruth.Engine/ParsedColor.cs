using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GroundTruth.Engine
{
    public class ParsedColor
    {
        public int Argb
        {
            get { return argb; }
            set { argb = value; }
        }

        public ParsedColorResultMode ResultMode
        {
            get { return resultMode; }
        }

        [SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static ParsedColor Parse (string colorName)
        {
            ParsedColor parsedColor = new ParsedColor ();

            try
            {
                string normalizedColorName = colorName.Trim ().ToLowerInvariant ();

                // first try to parse it as it is
                try
                {
                    parsedColor.ParseColorValue (normalizedColorName);
                }
                catch (Exception)
                {
                    if (hexRegex.IsMatch (normalizedColorName))
                    {
                        parsedColor.resultMode = ParsedColorResultMode.MissingHash;
                        parsedColor.ParseColorValue ("#" + normalizedColorName);
                    }
                }
            }
            catch (Exception)
            {
                parsedColor.resultMode = ParsedColorResultMode.InvalidColor;
                parsedColor.argb = Color.Red.ToArgb ();
            }

            return parsedColor;
        }

        private void ParseColorValue (string argbValue)
        {
            argb = ColorTranslator.FromHtml (argbValue).ToArgb ();
        }

        private int argb;
        private ParsedColorResultMode resultMode = ParsedColorResultMode.ColorOk;

        private static Regex hexRegex = new Regex (@"[0-9a-f]+", RegexOptions.IgnoreCase);
    }
}