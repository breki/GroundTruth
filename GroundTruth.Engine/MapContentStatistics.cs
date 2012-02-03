using System.Collections.Generic;

namespace GroundTruth.Engine
{
    public class MapContentStatistics
    {
        public IDictionary<string, int> GeneratedMapFeaturesCount
        {
            get { return generatedMapFeaturesCount; }
        }

        public void IncrementFeaturesCount (string renderingRuleId)
        {
            if (false == generatedMapFeaturesCount.ContainsKey(renderingRuleId))
               generatedMapFeaturesCount.Add(renderingRuleId, 0);

            generatedMapFeaturesCount[renderingRuleId] = generatedMapFeaturesCount[renderingRuleId] + 1;
        }

        private readonly Dictionary<string,int> generatedMapFeaturesCount = new Dictionary<string, int>();
    }
}