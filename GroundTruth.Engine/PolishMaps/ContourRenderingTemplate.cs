using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Brejc.Geometry;

namespace GroundTruth.Engine.PolishMaps
{
    public class ContourRenderingTemplate : TemplateBase
    {
        public void MarkHardwareLevelsUsed (MapDataAnalysis analysis)
        {
            if (Style.MinZoomFactor != int.MinValue)
                analysis.MarkUsedHardwareLevel (Style.MinZoomFactor);
            if (Style.MaxZoomFactor != int.MaxValue)
                analysis.MarkUsedHardwareLevel (Style.MaxZoomFactor);
        }

        public override void RegisterType(
            string ruleName, 
            TypesRegistry typesRegistry,
            bool insertAsFirst)
        {
            // if the type was already registered, skip it
            if (typesRegistry.LineTypeRegistrations.ContainsKey (ruleName))
                return;

            LineTypeRegistration typeRegistration;

            string typeName = null;

            if (Style.HasParameter ("typename"))
                typeName = Style.GetParameter ("typename");
            else
                typeName = Style.GetParameter ("rulename");

            // is it a standard Garmin type?
            if (typesRegistry.GarminLineTypesDictionary.HasType (typeName))
            {
                int standardType = typesRegistry.GarminLineTypesDictionary.GetType (typeName);
                typeRegistration = typesRegistry.RegisterStandardLineType (standardType, ruleName);
            }
            else
            {
                typeRegistration = typesRegistry.RegisterNewLineType (ruleName);
                typeRegistration.TypeName = typeName;
            }

            IList<string> colors = Style.GetParameter<IList<string>> ("colors");
            foreach (string color in colors)
                typeRegistration.Pattern.AddColor (color);

            typeRegistration.Width = Style.GetParameter<int> ("width", 2);
            typeRegistration.BorderWidth = Style.GetParameter<int> ("borderwidth", 0);

            if (Style.HasParameter ("pattern"))
            {
                string pattern = Style.GetParameter ("pattern");
                string[] patternLines = pattern.Split ('|');

                foreach (string patternLine in patternLines)
                    typeRegistration.Pattern.PatternLines.Add (patternLine);
            }

            if (Style.HasParameter ("showelevation"))
                showElevation = Style.GetParameter<bool>("showelevation");

            typeRegistration.MinLevel = Math.Max (12, Style.MinZoomFactor);
            typeRegistration.MaxLevel = Math.Min (24, Style.MaxZoomFactor);

            this.TypeRegistration = typeRegistration;
        }

        [SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void RenderContour (
            IPointD2List contourPoints, 
            double contourElevation,
            MapDataAnalysis analysis,
            CGpsMapperMapWriter mapWriter)
        {
            mapWriter.AddSection ("POLYLINE")
                .AddTypeReference (TypeRegistration)
                .AddCoordinates (
                "Data",
                analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MaxLevel],
                contourPoints)
                .Add ("EndLevel", analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MinLevel]);

            if (showElevation)
                mapWriter.Add ("Label", (int)contourElevation);
        }

        private bool showElevation;
    }
}