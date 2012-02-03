using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Brejc.Geometry;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.Rules
{
    public class RenderingRuleSet
    {
        public const string LandPolygonsRuleName = "LandPolygons";
        public const string SeaPolygonsRuleName = "SeaPolygons";

        public PolygonTemplate LandPolygonsTemplate
        {
            get { return landPolygonsTemplate; }
            set { landPolygonsTemplate = value; }
        }

        public IRenderingRulesSetOptions Options
        {
            get { return options; }
            set { options = value; }
        }

        public object OriginalData
        {
            get { return originalData; }
            set { originalData = value; }
        }

        public int RulesCount { get { return rules.Count; } }

        public PolygonTemplate SeaPolygonsTemplate
        {
            get { return seaPolygonsTemplate; }
            set { seaPolygonsTemplate = value; }
        }

        public RenderingRule this[int index]
        {
            get { return rules[index]; }
            set
            {
                rules[index] = value;
            }
        }

        public void AddRule (RenderingRule renderingRule)
        {
            MakeSureRuleNameIsUnique(renderingRule);
            rules.Add (renderingRule);
        }

        public IEnumerable<RenderingRule> EnumerateRules ()
        {
            return rules;
        }

        /// <summary>
        /// Determines whether the specified rule exists in the set.
        /// </summary>
        /// <param name="ruleName">Name of the rule.</param>
        /// <returns>
        /// 	<c>true</c> if the specified rule exists in the set; otherwise, <c>false</c>.
        /// </returns>
        public bool HasRule (string ruleName)
        {
            return null != rules.Find (delegate (RenderingRule rule)
                                           {
                                               return rule.RuleName == ruleName;
                                           });
        }

        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        public void RegisterSeaAndLandPolygonsTemplatesIfNecessary (MapMakerSettings mapMakerSettings)
        {
            if (landPolygonsTemplate == null)
                RegisterLandPolygonsTemplate (mapMakerSettings);
            if (seaPolygonsTemplate == null)
                RegisterSeaPolygonsTemplate (mapMakerSettings);
        }

        private void MakeSureRuleNameIsUnique(RenderingRule renderingRule)
        {
            if (null != rules.Find(r => 0 == String.Compare(r.RuleName, renderingRule.RuleName, StringComparison.Ordinal)))
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Rule name '{0}' is used more than once in rules.",
                    renderingRule.RuleName);
                throw new ArgumentException(message);
            }
        }

        private void RegisterSeaPolygonsTemplate (MapMakerSettings mapMakerSettings)
        {
            GisColor color = Options.SeaColor;
            seaPolygonsTemplate = RegisterPriorityPolygonTemplate (
                RenderingRuleSet.SeaPolygonsRuleName, 
                color, 
                mapMakerSettings);
        }

        private void RegisterLandPolygonsTemplate(MapMakerSettings mapMakerSettings)
        {
            GisColor color = Options.LandBackgroundColor;
            landPolygonsTemplate = RegisterPriorityPolygonTemplate (
                RenderingRuleSet.LandPolygonsRuleName,
                color,
                mapMakerSettings);
        }

        private static PolygonTemplate RegisterPriorityPolygonTemplate(
            string ruleName,
            GisColor color, 
            MapMakerSettings mapMakerSettings)
        {
            string colorAsString = string.Format (
                CultureInfo.InvariantCulture,
                "{000000:X}",
                color.Rgb);

            List<string> colors = new List<string> ();
            colors.Add (colorAsString);

            PolygonTemplate template = new PolygonTemplate ();

            template.Style.SetParameter (
                "rulename",
                ruleName);
            template.Style.SetParameter ("colors", colors);
            //template.Style.SetParameter ("typename", typeName);

            template.RegisterType (
                ruleName,
                mapMakerSettings.TypesRegistry,
                true);

            return template;
        }

        private PolygonTemplate landPolygonsTemplate;
        private IRenderingRulesSetOptions options = new RenderingRulesSetOptions ();
        private object originalData;
        private List<RenderingRule> rules = new List<RenderingRule> ();
        private PolygonTemplate seaPolygonsTemplate;
    }
}