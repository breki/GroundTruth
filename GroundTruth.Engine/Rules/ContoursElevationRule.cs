using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Brejc.Geometry;
using GroundTruth.Engine.PolishMaps;

namespace GroundTruth.Engine.Rules
{
    public class ContoursElevationRule
    {
        public ContoursElevationRule(string ruleName, int elevationMultiple)
        {
            this.ruleName = ruleName;
            this.elevationMultiple = elevationMultiple;
        }

        public void AddTemplate (ContourRenderingTemplate template)
        {
            templates.Add(template);
        }

        public bool IsContourMatch (double contourElevation)
        {
            return contourElevation%elevationMultiple == 0;
        }

        public void MarkHardwareLevelsUsed (MapDataAnalysis analysis)
        {
            foreach (ContourRenderingTemplate template in templates)
                template.MarkHardwareLevelsUsed (analysis);
        }

        public void RegisterTypes(TypesRegistry typesRegistry)
        {
            foreach (ContourRenderingTemplate template in templates)
                template.RegisterType(ruleName, typesRegistry, false);
        }

        [SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void RenderContour (
            IPointD2List contourPoints,
            double contourElevation,
            MapDataAnalysis analysis,
            CGpsMapperMapWriter mapWriter)
        {
            foreach (ContourRenderingTemplate template in templates)
                template.RenderContour(contourPoints, contourElevation, analysis, mapWriter);
        }

        private int elevationMultiple;
        private readonly string ruleName;
        private List<ContourRenderingTemplate> templates = new List<ContourRenderingTemplate>();
    }
}