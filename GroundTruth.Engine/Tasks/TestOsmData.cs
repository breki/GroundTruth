using System.Globalization;
using Brejc.OsmLibrary;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Rules;

namespace GroundTruth.Engine.Tasks
{
    /// <summary>
    /// Represents a source for a test OSM data which is used to display all of the available
    /// Garmin POI icons.
    /// </summary>
    public class TestOsmDataProvider : IOsmDatabaseProvider
    {
        public bool IsReloadable
        {
            get { return false; }
        }

        public static RenderingRuleSet CreateTestRenderingRules()
        {
            RenderingRuleSet rules = new RenderingRuleSet();

            for (int i = 0; i < pointTypesCount; i++)
            {
                IOsmElementSelector selector = new KeyValueSelector(
                    "garmin_icon", 
                    i.ToString(CultureInfo.InvariantCulture));
                IconTemplate template = new IconTemplate();

                template.Style.SetParameter("standardtype", i);
                template.Style.SetParameter("label", i.ToString("x", CultureInfo.InvariantCulture));
                
                RenderingRule rule = new RenderingRule(
                    string.Format(CultureInfo.InvariantCulture, "GarminIcon{0}", i),
                    RenderingRuleTargets.Nodes,
                    selector,
                    template);

                rules.AddRule(rule);
            }

            return rules;
        }

        public InMemoryOsmDatabase Provide ()
        {
            InMemoryOsmDatabase osmDatabase = new InMemoryOsmDatabase();

            for (int i = 0; i < pointTypesCount; i++)
            {
                OsmNode node = new OsmNode (i + 1, 15 + (i % width) * 0.1, 46 + (i / width) * 0.1);
                node.SetTag("garmin_icon", i.ToString(CultureInfo.InvariantCulture));
                osmDatabase.AddNode (node);
            }

            return osmDatabase;
        }

        private const int pointTypesCount = 256;
        private const int width = 16;
    }
}