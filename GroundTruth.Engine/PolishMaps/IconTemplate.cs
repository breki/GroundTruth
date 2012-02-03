using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Brejc.Geometry;
using Brejc.OsmLibrary;
using GroundTruth.Engine.LabelBuilding;
using GroundTruth.Engine.MapDataSources;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.PolishMaps
{
    public class IconTemplate : OsmObjectRenderingTemplateBase
    {
        public override void RegisterType (
            string ruleName, 
            TypesRegistry typesRegistry,
            bool insertAsFirst)
        {
            // if the type was already registered, skip it
            if (typesRegistry.PointTypeRegistrations.ContainsKey (ruleName))
                return;

            PointTypeRegistration typeRegistration;

            string typeName = null;

            if (Style.HasParameter ("typename"))
                typeName = Style.GetParameter ("typename");
            else
                typeName = Style.GetParameter ("rulename");

            // is it a standard Garmin type?
            if (typesRegistry.GarminPointTypesDictionary.HasType (typeName))
            {
                int standardType = typesRegistry.GarminPointTypesDictionary.GetType (typeName);
                typeRegistration = typesRegistry.RegisterStandardPointType (standardType, ruleName);
            }
            else
            {
                typeRegistration = typesRegistry.RegisterNewPointType(ruleName);
                typeRegistration.TypeName = typeName;
            }

            if (Style.HasParameter("iconurl"))
            {
                string iconUrl = Style.GetParameter("iconurl");
                WebClient webClient = new WebClient();
                byte[] imageData = webClient.DownloadData(iconUrl);

                MemoryStream memoryStream = new MemoryStream (imageData);
                Bitmap iconImage = (Bitmap) Bitmap.FromStream (memoryStream);

                Dictionary<int, int> colorsIndexed = new Dictionary<int, int>();
                
                for (int y = 0; y < iconImage.Height; y++)
                {
                    StringBuilder patternLine = new StringBuilder();

                    for (int x = 0; x < iconImage.Width; x++)
                    {
                        Color color = iconImage.GetPixel(x, y);
                        int colorRgb = color.ToArgb();
                        int colorIndex = 0;

                        // if we found a new color
                        if (false == colorsIndexed.ContainsKey (colorRgb))
                        {
                            typeRegistration.Pattern.AddColor((colorRgb & 0xffffff).ToString("x", CultureInfo.InvariantCulture));
                            colorsIndexed.Add(colorRgb, colorsIndexed.Count);
                        }

                        colorIndex = colorsIndexed[colorRgb];
                        patternLine.Append(colorIndex);
                    }

                    typeRegistration.Pattern.PatternLines.Add(patternLine.ToString());
                }
            }
            else if (Style.HasParameter("patternurl"))
            {
                Match match = regexWikiLink.Match(Style.GetParameter("patternurl"));

                typeRegistration.Pattern = typesRegistry.GetPattern(match.Groups["patternname"].Value);
            }

            if (Style.HasParameter ("label"))
            {
                LabelExpressionParser labelExpressionParser = new LabelExpressionParser ();
                typeRegistration.Label = labelExpressionParser.Parse (Style.GetParameter ("label"), 0);
            }

            typeRegistration.MinLevel = Math.Max (12, Style.MinZoomFactor);
            typeRegistration.MaxLevel = Math.Min (24, Style.MaxZoomFactor);

            this.TypeRegistration = typeRegistration;
        }

        [SuppressMessage ("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public override void RenderOsmObject (
            MapMakerSettings mapMakerSettings,
            MapDataAnalysis analysis,
            InMemoryOsmDatabase osmDatabase,
            OsmObjectBase osmObject, 
            OsmRelation parentRelation,
            CGpsMapperMapWriter mapWriter)
        {
            mapWriter.AddSection ("POI")
                .AddTypeReference (TypeRegistration);

            // find the location to put the icon on
            OsmNode iconNode = null;

            if (osmObject is OsmNode)
                iconNode = (OsmNode) osmObject;
            else if (osmObject is OsmWay)
            {
                PointD2 location = Brejc.OsmLibrary.Helpers.OsmGeometryUtils.FindAreaCenterPoint (
                    (OsmWay)osmObject, 
                    osmDatabase);
                iconNode = new OsmNode(1, location.X, location.Y);
            }
            else
                throw new InvalidOperationException("Internal error.");

            mapWriter
                .AddCoordinates ("Data", analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MaxLevel], iconNode)
                .Add ("EndLevel", analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MinLevel])
                ;

            if (this.TypeRegistration.Label != null && false == this.TypeRegistration.Label.IsConstant)
                mapWriter.Add ("Label", this.TypeRegistration.Label.BuildLabel (mapMakerSettings, osmObject, parentRelation));
        }

        private static readonly Regex regexWikiLink = new Regex (@"#\s*(?<patternname>.+)", RegexOptions.ExplicitCapture);
    }
}