using System;
using System.Collections.Generic;
using System.Globalization;
using GroundTruth.Engine.PolishMaps;

namespace GroundTruth.Engine.Rules
{
    public class ContoursElevationRuleMap : IComparer<int>
    {
        public ContoursElevationRuleMap()
        {
            rules = new SortedList<int, ContoursElevationRule>(this);
        }

        public void AddTemplate (int elevationMultiple, ContourRenderingTemplate template)
        {
            if (false == rules.ContainsKey(elevationMultiple))
                rules.Add(
                    elevationMultiple, 
                    new ContoursElevationRule(
                        String.Format (CultureInfo.InvariantCulture, "ContourRule{0}", rules.Count + 1),
                        elevationMultiple));

            ContoursElevationRule rule = rules[elevationMultiple];

            rule.AddTemplate(template);
        }

        public int Compare(int x, int y)
        {
            return -x.CompareTo(y);
        }

        public ContoursElevationRule FindMatchingRule (int elevation)
        {
            foreach (ContoursElevationRule rule in rules.Values)
            {
                if (rule.IsContourMatch(elevation))
                    return rule;
            }

            return null;
        }

        public void MarkHardwareLevelsUsed (MapDataAnalysis analysis)
        {
            foreach (ContoursElevationRule rule in rules.Values)
                rule.MarkHardwareLevelsUsed(analysis);
        }

        public void RegisterTypes (TypesRegistry typesRegistry)
        {
            foreach (ContoursElevationRule rule in rules.Values)
                rule.RegisterTypes(typesRegistry);
        }

        private SortedList<int, ContoursElevationRule> rules;
    }
}