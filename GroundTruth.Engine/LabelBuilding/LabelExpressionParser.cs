using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GroundTruth.Engine.LabelBuilding
{
    public class LabelExpressionParser
    {
        public LabelExpression Parse (string expression, int lineCounter)
        {
            this.lineCounter = lineCounter;

            LabelExpression labelExpression = new LabelExpression();

            string[] splits = expression.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string split in splits)
            {
                string elementDef = split.Trim();

                // skip empty splits
                if (elementDef.Length == 0)
                    continue;

                Match match = elementRegex.Match(elementDef);
                if (false == match.Success)
                    ThrowParseError("Syntax error in label '{0}'", split);

                string keyValue = match.Groups["key"].Value;
                string stringValue = match.Groups["string"].Value;

                ILabelExpressionElement labelExpressionElement = null;
                if (false == String.IsNullOrEmpty(keyValue))
                {
                    if (false == String.IsNullOrEmpty(stringValue))
                        labelExpressionElement = new OsmKeyLabelExpressionElement (keyValue, stringValue);
                    else
                        labelExpressionElement = new OsmKeyLabelExpressionElement (keyValue);
                }
                else
                    labelExpressionElement = new FormatLabelExpressionElement(stringValue);

                labelExpression.AddElement(labelExpressionElement);
            }

            return labelExpression;
        }

        [SuppressMessage ("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void ThrowParseError (string errorMessageFormat, params object[] args)
        {
            string errorMessage = String.Format (CultureInfo.InvariantCulture, errorMessageFormat, args);

            throw new ArgumentException (
                String.Format (
                    CultureInfo.InvariantCulture,
                    "{0} (line {1})",
                    errorMessage,
                    lineCounter));
        }

        private int lineCounter;
        private static Regex elementRegex = new Regex(@"^(?<key>[a-zA-Z_0-9!#$%&'()*+,-./:;<=>?@\[\]\\]+)?\s*(\""(?<string>[^\""]*)\"")?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static string[] separators = new string[] {"++"};
    }
}
