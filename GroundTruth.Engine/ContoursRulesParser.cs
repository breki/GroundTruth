using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Rules;
using GroundTruth.Engine.Wiki;

namespace GroundTruth.Engine
{
    public class ContoursRulesParser : WikiRulesParserBase
    {
        public ContoursElevationRuleMap Parse (Stream inputStream)
        {
            WikiParser.SetWikiContentSource(inputStream);

            rules = new ContoursElevationRuleMap ();
            templateCounter = 0;
            string currentMainSection = null;

            while (true)
            {
                WikiContentType wikiContentType;

                wikiContentType = WikiParser.Next ();

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
                            currentMainSection = section.SectionName;
                    }
                    else if (wikiContentType == WikiContentType.Text)
                        continue;

                    if (currentMainSection == null)
                        continue;

                    switch (currentMainSection.ToLowerInvariant ())
                    {
                        case "contours rendering rules":
                            {
                                SkipToTableAndParseIt (ParseRule);
                                break;
                            }
                    }
                }
            }

            return rules;
        }

        private void ParseRule (IList<string> tableRowCells)
        {
            int i = 0;
            string selectorText = GetTableRowCell (tableRowCells, i++, false, "Rule selector missing");
            string minLevelText = GetTableRowCell (tableRowCells, i++, true, null);
            string maxLevelText = GetTableRowCell (tableRowCells, i++, true, null);
            string typeName = GetTableRowCell (tableRowCells, i++, true, null);
            string showElevation = GetTableRowCell (tableRowCells, i++, true, null);
            string colorsText = GetTableRowCell (tableRowCells, i++, false, "Line colors value missing");
            string widthText = GetTableRowCell (tableRowCells, i++, true, null);
            string borderWidthText = GetTableRowCell (tableRowCells, i++, true, null);
            string patternText = GetTableRowCell (tableRowCells, i++, true, null);

            int elevationMultiple = 0;

            try
            {
                elevationMultiple = Int32.Parse(tableRowCells[0], CultureInfo.InvariantCulture);
                if (elevationMultiple <= 0)
                    throw new FormatException();
            }
            catch (FormatException)
            {
                ThrowParseError(
                    "Invalid elevation multiple: '{0}'. It must be a positive integer number.", 
                    tableRowCells[0]);
            }

            bool lineWidthsSpecified = false;

            ContourRenderingTemplate template = new ContourRenderingTemplate ();
            template.Style.SetParameter ("rulename", String.Format(CultureInfo.InvariantCulture, "Template{0}", ++templateCounter));
            if (false == string.IsNullOrEmpty (minLevelText))
                template.Style.MinZoomFactor = ParseLevel (minLevelText);
            if (false == string.IsNullOrEmpty (maxLevelText))
                template.Style.MaxZoomFactor = ParseLevel (maxLevelText);
            template.Style.SetParameter ("colors", ParseColors (colorsText));
            if (false == string.IsNullOrEmpty (typeName))
                template.Style.SetParameter ("typename", typeName);
            if (false == String.IsNullOrEmpty (showElevation))
            {
                bool showElevationValue = showElevation.Equals("yes", StringComparison.OrdinalIgnoreCase);
                template.Style.SetParameter("showelevation", showElevationValue);
            }
            if (false == String.IsNullOrEmpty (widthText))
            {
                lineWidthsSpecified = true;
                template.Style.SetParameter ("width", ParseLineWidth (widthText));
            }
            if (false == String.IsNullOrEmpty (borderWidthText))
            {
                lineWidthsSpecified = true;
                template.Style.SetParameter ("borderwidth", ParseLineWidth (borderWidthText));
            }
            if (false == String.IsNullOrEmpty (patternText))
            {
                if (lineWidthsSpecified)
                    ThrowParseError ("Both line widths and the line pattern are specified. These are mutually exclusive - choose one or the other");

                template.Style.SetParameter ("pattern", ParseLinePattern (patternText));
            }

            rules.AddTemplate(elevationMultiple, template);
        }

        private ContoursElevationRuleMap rules;
        private int templateCounter;
    }
}