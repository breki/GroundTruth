using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Brejc.Geometry;
using Brejc.OsmLibrary;
using GroundTruth.Engine.LabelBuilding;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.PolishMaps
{
    public class PolygonTemplate : OsmObjectRenderingTemplateBase
    {
        public override void RegisterType (
            string ruleName, 
            TypesRegistry typesRegistry,
            bool insertAsFirst)
        {
            // if the type was already registered, skip it
            if (typesRegistry.AreaTypeRegistrations.ContainsKey (ruleName))
                return;

            AreaTypeRegistration typeRegistration = null;

            string typeName = null;

            if (Style.HasParameter("typename"))
                typeName = Style.GetParameter ("typename");
            else
                typeName = Style.GetParameter("rulename");

            // is it a standard Garmin type?
            if (typesRegistry.GarminAreaTypesDictionary.HasType (typeName))
            {
                int standardType = typesRegistry.GarminAreaTypesDictionary.GetType (typeName);
                typeRegistration = typesRegistry.RegisterStandardAreaType(standardType, ruleName);
            }
            else
            {
                typeRegistration = typesRegistry.RegisterNewAreaType (ruleName, insertAsFirst);
                typeRegistration.TypeName = typeName;
            }

            IList<string> colors = Style.GetParameter<IList<string>>("colors");
            foreach (string color in colors)
                typeRegistration.Pattern.AddColor(color);

            if (Style.HasParameter ("label"))
            {
                LabelExpressionParser labelExpressionParser = new LabelExpressionParser();
                typeRegistration.Label = labelExpressionParser.Parse(Style.GetParameter("label"), 0);
            }

            if (Style.HasParameter ("pattern"))
            {
                string pattern = Style.GetParameter ("pattern");
                string[] patternLines = pattern.Split ('|');

                foreach (string patternLine in patternLines)
                    typeRegistration.Pattern.PatternLines.Add (patternLine);
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
            mapWriter.AddSection ("POLYGON")
                .AddTypeReference (TypeRegistration)
                .AddCoordinates (
                    "Data", 
                    analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MaxLevel],
                    GetNodesForWay(osmDatabase, (OsmWay)osmObject));

            // rendering of holes
            if (osmObject is OsmAreaWithHoles)
            {
                OsmAreaWithHoles areaWithHoles = (OsmAreaWithHoles) osmObject;
                foreach (int holeWayId in areaWithHoles.EnumerateHolesWaysIds())
                {
                    OsmWay holeWay = osmDatabase.GetWay(holeWayId);
                    if (holeWay.NodesCount > 3)
                        mapWriter.AddCoordinates (
                            "Data",
                            analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MaxLevel],
                            GetNodesForWay (osmDatabase, holeWay));
                }
            }

            mapWriter
                .Add ("EndLevel", analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MinLevel])
                ;

            if (this.TypeRegistration.Label != null && false == this.TypeRegistration.Label.IsConstant)
                mapWriter.Add ("Label", this.TypeRegistration.Label.BuildLabel (mapMakerSettings, osmObject, parentRelation));
        }

        [SuppressMessage ("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "mapMakerSettings")]
        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        [SuppressMessage ("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void RenderPolygon (
            MapMakerSettings mapMakerSettings,
            MapDataAnalysis analysis,
            IPointD2List polygonPoints,
            CGpsMapperMapWriter mapWriter)
        {
            mapWriter.AddSection ("POLYGON")
                .AddTypeReference (TypeRegistration)
                .AddCoordinates (
                    "Data",
                    analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MaxLevel],
                    polygonPoints);

            mapWriter
                .Add ("EndLevel", analysis.HardwareToLogicalLevelDictionary[TypeRegistration.MinLevel])
                ;
        }
    }
}