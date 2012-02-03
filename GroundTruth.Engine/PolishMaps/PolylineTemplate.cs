using System;
using System.Collections.Generic;
using Brejc.OsmLibrary;
using GroundTruth.Engine.LabelBuilding;
using GroundTruth.Engine.MapDataSources;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.PolishMaps
{
    public class PolylineTemplate : OsmObjectRenderingTemplateBase
    {
        public override void RegisterType (
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
                typeRegistration.Pattern.AddColor(color);

            typeRegistration.Width = Style.GetParameter<int> ("width", 2);
            typeRegistration.BorderWidth = Style.GetParameter<int> ("borderwidth", 0);

            if (Style.HasParameter("pattern"))
            {
                string pattern = Style.GetParameter("pattern");
                string[] patternLines = pattern.Split('|');

                foreach (string patternLine in patternLines)
                    typeRegistration.Pattern.PatternLines.Add (patternLine);
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

        public override void RenderOsmObject(
            MapMakerSettings mapMakerSettings,
            MapDataAnalysis analysis,
            InMemoryOsmDatabase osmDatabase,
            OsmObjectBase osmObject,
            OsmRelation parentRelation,
            CGpsMapperMapWriter mapWriter)
        {
            mapWriter.AddSection ("POLYLINE")
                .AddTypeReference (TypeRegistration)
                .AddCoordinates (
                    "Data", 
                    analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MaxLevel], 
                    GetNodesForWay(osmDatabase, (OsmWay)osmObject))
                .Add ("EndLevel", analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MinLevel])
                ;

            if (this.TypeRegistration.Label != null && false == this.TypeRegistration.Label.IsConstant)
                mapWriter.Add ("Label", this.TypeRegistration.Label.BuildLabel (mapMakerSettings, osmObject, parentRelation).ToUpperInvariant());
        }
    }
}