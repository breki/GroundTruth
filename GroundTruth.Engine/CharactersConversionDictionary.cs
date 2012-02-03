using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace GroundTruth.Engine
{
    [SuppressMessage ("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class CharactersConversionDictionary
    {
        public void AddConversion (char original, string converted)
        {
            if (dictionary.ContainsKey (original))
            {
                // if the duplicate is exactly the same, ignore it

                if (String.Compare(converted, dictionary[original], StringComparison.Ordinal) != 0)
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.InvariantCulture,
                            "Character '{0}' has been entered more than once in the characters conversion table",
                            original));
            }
            else
                dictionary.Add(original, converted);
        }

        [SuppressMessage ("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public string Convert (string originalString)
        {
            StringBuilder convertedString = new StringBuilder();

            for (int i = 0; i < originalString.Length; i++)
            {
                char c = originalString[i];
                if (dictionary.ContainsKey (c))
                    convertedString.Append (dictionary[c]);
                else
                    convertedString.Append(c);
            }

            return convertedString.ToString();
        }

        private Dictionary<char, string> dictionary = new Dictionary<char, string>();
    }
}
