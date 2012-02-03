using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Brejc.Common.Props;
using Brejc.OsmLibrary;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Rules;
using GroundTruth.Engine.Wiki;
using log4net;

namespace GroundTruth.Engine
{
    public class WikiRulesParser : WikiRulesParserBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WikiRulesParser"/> class.
        /// </summary>
        /// <param name="inputStream">The input stream from which the rendering rules will be parsed..</param>
        public WikiRulesParser(
            Stream inputStream, 
            TypesRegistry typesRegistry,
            CharactersConversionDictionary charactersConversionDictionary,
            ISerializersRegistry serializersRegistry)
        {
            this.typesRegistry = typesRegistry;
            this.charactersConversionDictionary = charactersConversionDictionary;
            this.serializersRegistry = serializersRegistry;

            WikiParser.SetWikiContentSource(inputStream);
        }

        public CharactersConversionDictionary CharactersConversionDictionary
        {
            get { return charactersConversionDictionary; }
        }

        /// <summary>
        /// Gets the rendering rules.
        /// </summary>
        /// <value>The rendering rules.</value>
        public RenderingRuleSet Rules
        {
            get { return rules; }
        }

        public TypesRegistry TypesRegistry
        {
            get { return typesRegistry; }
        }

        /// <summary>
        /// Parses the rendering rules and adds the parsed rules to the <see cref="Rules"/> property.
        /// </summary>
        public void Parse ()
        {
            string currentMainSection = null;
            string currentSubsection = null;
            bool ignoreRestOfMainSection = false;
            bool ignoreRestOfSubsection = false;
            bool moveNext = true;
            rules = new RenderingRuleSet();

            while (true)
            {
                WikiContentType wikiContentType;

                if (moveNext)
                    wikiContentType = WikiParser.Next ();
                else
                {
                    wikiContentType = WikiParser.Context.CurrentType;
                    moveNext = true;
                }

                if (wikiContentType == WikiContentType.Eof)
                    break;

                if (wikiContentType == WikiContentType.Section || wikiContentType == WikiContentType.Text)
                {
                    WikiSection section = WikiParser.Context.LowestSection;
                    if (section == null)
                        continue;

                    if (wikiContentType == WikiContentType.Section)
                    {
                        if (section.SectionLevel == 1)
                        {
                            if (log.IsDebugEnabled)
                                log.DebugFormat("Found new main section: '{0}'", section.SectionName);

                            currentMainSection = section.SectionName;
                            currentSubsection = null;
                            ignoreRestOfMainSection = false;
                            ignoreRestOfSubsection = false;
                        }
                        else if (section.SectionLevel == 2)
                        {
                            currentSubsection = section.SectionName;
                            ignoreRestOfSubsection = false;
                        }
                    }
                    else if (wikiContentType == WikiContentType.Text 
                        && (ignoreRestOfMainSection || ignoreRestOfSubsection))
                        continue;

                    switch (currentMainSection.ToLowerInvariant())
                    {
                        case "rendering rules":
                            {
                                if (currentSubsection == null)
                                    continue;

                                switch (section.SectionName.ToLowerInvariant())
                                {
                                    case "options":
                                        SkipToTableAndParseIt(ParseOption);
                                        AssertRulesVersionCompatibility();
                                        ignoreRestOfSubsection = true;
                                        break;

                                    case "points":
                                        SkipToTableAndParseIt (ParsePointRule);
                                        ignoreRestOfSubsection = true;
                                        break;

                                    case "lines":
                                        SkipToTableAndParseIt (ParseLineRule);
                                        ignoreRestOfSubsection = true;
                                        break;

                                    case "areas":
                                        SkipToTableAndParseIt (ParseAreaRule);
                                        ignoreRestOfSubsection = true;
                                        break;

                                    default:
                                        ThrowParseError("Invalid section: '{0}'", section.SectionName);
                                        return;
                                }

                                break;
                            }

                        case "standard garmin types":
                            {
                                if (currentSubsection == null)
                                    continue;

                                switch (section.SectionName.ToLowerInvariant())
                                {
                                    case "points":
                                        SkipToTableAndParseIt ((tableRowCells)
                                            =>
                                            ParseStandardGarminTypesDictionary (
                                                tableRowCells,
                                                typesRegistry.GarminPointTypesDictionary));
                                        ignoreRestOfSubsection = true;
                                        break;

                                    case "lines":
                                        SkipToTableAndParseIt ((tableRowCells)
                                                   =>
                                                   ParseStandardGarminTypesDictionary(
                                                    tableRowCells,
                                                    typesRegistry.GarminLineTypesDictionary));
                                        ignoreRestOfSubsection = true;
                                        break;

                                    case "areas":
                                        
                                        SkipToTableAndParseIt ((tableRowCells)
                                                   =>
                                                   ParseStandardGarminTypesDictionary(
                                                    tableRowCells,
                                                    typesRegistry.GarminAreaTypesDictionary));
                                        ignoreRestOfSubsection = true;
                                        break;

                                    default:
                                        ThrowParseError("Invalid Garmin standard types section: '{0}'",
                                            section.SectionName);
                                        return;
                                }

                                break;
                            }

                        case "characters conversion table":
                            {
                                SkipToTableAndParseIt (ParseCharactersConversionTable);
                                ignoreRestOfMainSection = true;
                                break;
                            }

                        case "patterns":
                            {
                                if (currentSubsection == null)
                                    continue;

                                ParsePointPattern(section.SectionName);
                                moveNext = false;
                                break;
                            }
                    }
                }
            }
        }

        private void AssertRulesVersionCompatibility()
        {
            string rulesVersionString = rules.Options.RulesVersion;

            if (false == String.IsNullOrEmpty (rulesVersionString))
            {
                try
                {
                    Version rulesVersion = new Version (rulesVersionString);

                    if ((rulesVersion.Major == 1 && rulesVersion.Minor > 6)
                        || (rulesVersion.Major > 1))
                        throw new ArgumentException("Incompatibile rendering rules version.");
                }
                catch (FormatException)
                {
                    throw new ArgumentException ("Invalid rendering rules version.");
                }
            }
        }

        private string ParseAreaPattern (string patternText)
        {
            MatchCollection matches = RegexPattern.Matches (patternText);
            if (matches.Count < 1)
                ThrowParseError ("Invalid area pattern: '{0}'", patternText);

            Match match = matches[0];
            Group group = match.Groups["line"];

            int initialPatternWidth = -1;
            int patternHeight = 0;

            StringBuilder patternCollected = new StringBuilder ();
            foreach (Capture capture in group.Captures)
            {
                string patternLine = capture.Value;

                if (patternLine != null)
                {
                    // all pattern lines have to be of the same length
                    if (initialPatternWidth != -1)
                    {
                        if (patternLine.Length != initialPatternWidth)
                            ThrowParseError("All pattern lines in a single rule have to be of the same width.");
                    }
                    else
                    {
                        initialPatternWidth = patternLine.Length;
                        CheckForPowerOf2 (initialPatternWidth, "Area pattern width can only be of 1, 2, 4, 8, 16 or 32 characters.");
                    }

                    if (patternCollected.Length > 0)
                        patternCollected.Append ('|');
                    patternCollected.Append (patternLine);
                    patternHeight++;
                }
            }

            CheckForPowerOf2 (patternHeight, "Area pattern height can only be of 1, 2, 4, 8, 16 or 32 characters.");

            return patternCollected.ToString ();
        }

        private void ParseAreaRule(IList<string> tableRowCells)
        {
            string ruleName = GetTableRowCell(tableRowCells, 0, false, "Rule name missing");
            string selectorText = GetTableRowCell(tableRowCells, 1, false, "Rule selector missing");
            string minLevelText = GetTableRowCell (tableRowCells, 2, true, null);
            string maxLevelText = GetTableRowCell (tableRowCells, 3, true, null);
            string typeName = GetTableRowCell (tableRowCells, 4, true, null);
            string label = GetTableRowCell (tableRowCells, 5, true, null);
            string colorsText = GetTableRowCell (tableRowCells, 6, false, "Area colors value missing");
            string patternText = GetTableRowCell (tableRowCells, 7, true, null);

            IOsmElementSelector osmElementSelector = ParseRuleSelector(tableRowCells[1]);
            IList<string> colors = ParseColors(colorsText);

            PolygonTemplate template = new PolygonTemplate ();
            template.Style.SetParameter ("rulename", ruleName);
            if (false == string.IsNullOrEmpty (minLevelText))
                template.Style.MinZoomFactor = ParseLevel(minLevelText);
            if (false == string.IsNullOrEmpty (maxLevelText))
                template.Style.MaxZoomFactor = ParseLevel (maxLevelText);
            template.Style.SetParameter("colors", colors);
            if (false == string.IsNullOrEmpty(typeName))
                template.Style.SetParameter ("typename", typeName);
            if (false == String.IsNullOrEmpty(label))
                template.Style.SetParameter ("label", label);
            if (false == String.IsNullOrEmpty (patternText))
                template.Style.SetParameter ("pattern", ParseAreaPattern (patternText));

            RenderingRule rule = new RenderingRule(ruleName, RenderingRuleTargets.Areas, osmElementSelector, template);
            rules.AddRule(rule);
        }

        private void ParseCharactersConversionTable (IList<string> tableRowCells)
        {
            string originalChar = GetTableRowCell (tableRowCells, 0, false, "Original character missing");
            if (originalChar.Length > 1)
                ThrowParseError("Only a single character is supported for originals in the characters conversion table.");

            string convertedChars = GetTableRowCell (tableRowCells, 1, true, null);

            charactersConversionDictionary.AddConversion(originalChar[0], convertedChars);
        }

        private void ParseColorTable (IList<string> tableRowCells)
        {
            string colorId = GetTableRowCell (tableRowCells, 0, false, "Color ID missing");
            string colorText = GetTableRowCell (tableRowCells, 1, false, "Color missing");
            IList<string> colors = ParseColors(colorText);
            if (colors.Count == 0)
                ThrowParseError("Color missing");
            if (colors.Count > 1)
                ThrowParseError("Only one color per row can be specified");

            currentParsedPattern.AddColor(colorId, colors[0]);
        }

        private void ParseLineRule (IList<string> tableRowCells)
        {
            string ruleName = GetTableRowCell (tableRowCells, 0, false, "Rule name missing");
            string selectorText = GetTableRowCell (tableRowCells, 1, false, "Rule selector missing");
            string minLevelText = GetTableRowCell (tableRowCells, 2, true, null);
            string maxLevelText = GetTableRowCell (tableRowCells, 3, true, null);
            string typeName = GetTableRowCell (tableRowCells, 4, true, null);
            string label = GetTableRowCell (tableRowCells, 5, true, null);
            string colorsText = GetTableRowCell (tableRowCells, 6, false, "Line colors value missing");
            string widthText = GetTableRowCell (tableRowCells, 7, true, null);
            string borderWidthText = GetTableRowCell (tableRowCells, 8, true, null);
            string patternText = GetTableRowCell (tableRowCells, 9, true, null);

            IOsmElementSelector osmElementSelector = ParseRuleSelector (tableRowCells[1]);

            bool lineWidthsSpecified = false;

            PolylineTemplate template = new PolylineTemplate ();
            template.Style.SetParameter ("rulename", ruleName);
            if (false == string.IsNullOrEmpty (minLevelText))
                template.Style.MinZoomFactor = ParseLevel (minLevelText);
            if (false == string.IsNullOrEmpty (maxLevelText))
                template.Style.MaxZoomFactor = ParseLevel (maxLevelText);
            template.Style.SetParameter ("colors", ParseColors (colorsText));
            if (false == string.IsNullOrEmpty (typeName))
                template.Style.SetParameter ("typename", typeName);
            if (false == String.IsNullOrEmpty (label))
                template.Style.SetParameter ("label", label);
            if (false == String.IsNullOrEmpty (widthText))
            {
                lineWidthsSpecified = true;
                template.Style.SetParameter("width", ParseLineWidth(widthText));
            }
            if (false == String.IsNullOrEmpty (borderWidthText))
            {
                lineWidthsSpecified = true;
                template.Style.SetParameter("borderwidth", ParseLineWidth(borderWidthText));
            }
            if (false == String.IsNullOrEmpty (patternText))
            {
                if (lineWidthsSpecified)
                    ThrowParseError("Both line widths and the line pattern are specified. These are mutually exclusive - choose one or the other");

                template.Style.SetParameter("pattern", ParseLinePattern(patternText));
            }

            RenderingRule rule = new RenderingRule (ruleName, RenderingRuleTargets.Ways, osmElementSelector, template);
            rules.AddRule (rule);
        }

        private void ParseOption (IList<string> tableRowCells)
        {
            if (tableRowCells.Count < 2 || tableRowCells.Count > 3)
            {
                log.Warn("Incorrect number of columns for rule option, it will be ignored");
                return;
            }

            string optionName = tableRowCells[0].Trim ();
            string optionStringValue = tableRowCells[1].Trim ();

            // if the setting is not recognized
            if (false == rules.Options.Properties.HasProperty (optionName))
            {
                log.WarnFormat(
                    "Option '{0}' is not recognized, it will be ignored.",
                    optionName);
            }
            else
            {
                IPropertyDescriptor property = rules.Options.Properties.Schema.GetPropertyDescriptor(optionName);

                if (false == serializersRegistry.IsValueDeserializable (property.NativeType, optionStringValue))
                {
                    log.WarnFormat(
                        "Option '{0}' has an invalid value '{1}', it will be ignored.",
                        optionName,
                        optionStringValue);
                }
                else
                {
                    object optionValue = serializersRegistry.DeserializeValue (
                        property.NativeType,
                        optionStringValue);

                    rules.Options.Properties.SetValue(optionName, optionValue);
                }
            }
        }

        private void ParsePointPattern (string patternName)
        {
            currentParsedPattern = new PatternDefinition (patternName);

            SkipToTableAndParseIt (ParseColorTable);

            // collect all pattern lines
            StringBuilder patternText = new StringBuilder();
            while (true)
            {
                WikiContentType wikiContentType = WikiParser.Next();

                if (wikiContentType != WikiContentType.Text)
                    break;

                patternText.Append (WikiParser.Context.CurrentLine);
            } 

            MatchCollection matches = regexPointPattern.Matches (patternText.ToString());
            if (matches.Count < 1)
                ThrowParseError ("Invalid point pattern: '{0}'", patternText.ToString());

            Match match = matches[0];
            Group group = match.Groups["line"];

            foreach (Capture capture in group.Captures)
            {
                string patternLine = capture.Value;

                if (patternLine != null)
                {
                    // all pattern lines have to be of the same length
                    if (currentParsedPattern.Width != -1)
                    {
                        if (patternLine.Length != currentParsedPattern.Width)
                            ThrowParseError ("All pattern lines have to be of the same width.");
                    }

                    currentParsedPattern.PatternLines.Add (patternLine);

                    if (currentParsedPattern.Width > 24)
                        ThrowParseError ("Point pattern can have a maximum width of 24 characters");

                }
            }

            if (currentParsedPattern.Height > 24)
                ThrowParseError("Point pattern can have a maximum height of 24 characters");

            this.typesRegistry.Patterns.Add (currentParsedPattern.PatternName, currentParsedPattern);
        }

        private void ParsePointRule (IList<string> tableRowCells)
        {
            string ruleName = GetTableRowCell (tableRowCells, 0, false, "Rule name missing");
            string selectorText = GetTableRowCell (tableRowCells, 1, false, "Rule selector missing");
            string minLevelText = GetTableRowCell (tableRowCells, 2, true, null);
            string maxLevelText = GetTableRowCell (tableRowCells, 3, true, null);
            string typeName = GetTableRowCell (tableRowCells, 4, true, null);
            string label = GetTableRowCell (tableRowCells, 5, true, null);
            string iconText = GetTableRowCell (tableRowCells, 6, true, null);

            IOsmElementSelector osmElementSelector = ParseRuleSelector (tableRowCells[1]);

            IconTemplate template = new IconTemplate ();
            template.Style.SetParameter("rulename", ruleName);
            if (false == string.IsNullOrEmpty (minLevelText))
                template.Style.MinZoomFactor = ParseLevel (minLevelText);
            if (false == string.IsNullOrEmpty (maxLevelText))
                template.Style.MaxZoomFactor = ParseLevel (maxLevelText);
            if (false == string.IsNullOrEmpty(typeName))
                template.Style.SetParameter("typename", typeName);
            if (false == String.IsNullOrEmpty (label))
                template.Style.SetParameter ("label", label);

            if (false == String.IsNullOrEmpty (iconText))
            {
                // first check if it is a Wiki link
                Match match = regexWikiLink.Match(iconText);
                if (match.Success)
                {
                    string wikiLink = match.Groups["link"].Value.Trim();

                    // now check that the link is a local one (we are currently not supporting remote links)
                    if (false == wikiLink.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                        ThrowParseError(
                            "Currently only local links (to the same page) are supported for point patterns: '{0}'",
                            wikiLink);

                    template.Style.SetParameter("patternurl", wikiLink);
                }
                else
                    template.Style.SetParameter("iconurl", iconText);
            }

            // NOTE: the rule will target both OSM nodes and areas
            // this way we can put an icon on the conter of an area (example: place=locality)
            RenderingRule rule = new RenderingRule (ruleName, RenderingRuleTargets.Nodes | RenderingRuleTargets.Areas, osmElementSelector, template);
            rules.AddRule (rule);
        }

        private void ParseStandardGarminTypesDictionary (
            IList<string> tableRowCells, 
            GarminTypesDictionary garminTypesDictionary)
        {
            string typeName = GetTableRowCell (tableRowCells, 0, false, "Type name missing");
            string typeIdHex = GetTableRowCell (tableRowCells, 1, false, "Type id missing");

            int typeId = 0;
            try
            {
                typeId = int.Parse(typeIdHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                ThrowParseError(
                    "Standard Garmin type '{0}' has an invalid type ID '{1}'",
                    typeName,
                    typeIdHex);
            }

            garminTypesDictionary.AddType (typeName, typeId);
        }

        [SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private IOsmElementSelector ParseRuleSelector (string selectorText)
        {
            try
            {
                OsmElementSelectorParser parser = new OsmElementSelectorParser();
                IOsmElementSelector selector = parser.Parse(selectorText);

                return selector;
            }
            catch (Exception ex)
            {
                ThrowParseError("Could not parse the selector: '{0}'", ex.Message);
                return null; // dummy
            }
        }

        private CharactersConversionDictionary charactersConversionDictionary;
        private PatternDefinition currentParsedPattern;
        private static readonly ILog log = LogManager.GetLogger (typeof (WikiRulesParser));
        private static readonly Regex regexPointPattern = new Regex (@"((?<line>\w+)\s*\<br\/\>\s*)+", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        private static readonly Regex regexWikiLink = new Regex(@"\[\[(?<link>.+)\]\]", RegexOptions.ExplicitCapture);
        private RenderingRuleSet rules;
        private readonly ISerializersRegistry serializersRegistry;
        private TypesRegistry typesRegistry;
    }
}
