using System;
using System.Collections.Generic;
using System.Globalization;
using Brejc.OsmLibrary;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Rules;

namespace GroundTruth.Tests
{
    public class RenderingRulesMother
    {
        public RenderingRule CurrentRule
        {
            get { return currentRule; }
        }

        public RenderingRuleSet Rules
        {
            get { return rules; }
        }

        public RenderingRulesMother AddAreaRule ()
        {
            string ruleName = counter.ToString (CultureInfo.InvariantCulture);

            PolygonTemplate template = new PolygonTemplate ();
            template.Style.SetParameter ("rulename", ruleName);

            List<string> colors = new List<string> ();
            colors.Add ("ffffff");
            template.Style.SetParameter ("colors", colors);

            currentRule = new RenderingRule (
                ruleName,
                RenderingRuleTargets.Areas,
                new OsmElementSelectorContainer (OsmElementSelectorContainerOperation.And),
                template);
            rules.AddRule (currentRule);

            counter++;
            return this;
        }

        public RenderingRulesMother AddLineRule ()
        {
            string ruleName = counter.ToString (CultureInfo.InvariantCulture);

            PolylineTemplate template = new PolylineTemplate ();
            template.Style.SetParameter ("rulename", ruleName);

            List<string> colors = new List<string> ();
            colors.Add ("ffffff");
            template.Style.SetParameter ("colors", colors);

            currentRule = new RenderingRule (
                ruleName,
                RenderingRuleTargets.Ways,
                new OsmElementSelectorContainer (OsmElementSelectorContainerOperation.And),
                template);
            rules.AddRule (currentRule);

            counter++;
            return this;
        }

        public RenderingRulesMother AddRelationSelectors (params string[] keyValuePairs)
        {
            for (int i = 0; i < keyValuePairs.Length; i += 2)
            {
                ((OsmElementSelectorContainer)currentRule.Selector).ChildSelectors.Add (
                    new KeyValueSelector (
                        keyValuePairs[i], keyValuePairs[i + 1], true));
            }

            return this;
        }

        public RenderingRulesMother AddSelectors (params string[] keyValuePairs)
        {
            for (int i = 0; i < keyValuePairs.Length; i += 2)
            {
                ((OsmElementSelectorContainer)currentRule.Selector).ChildSelectors.Add (
                    new KeyValueSelector (
                        keyValuePairs[i], keyValuePairs[i + 1]));
            }

            return this;
        }

        public RenderingRulesMother AddSelectors (string expression)
        {
            OsmElementSelectorParser parser = new OsmElementSelectorParser ();
            IOsmElementSelector selector = parser.Parse(expression);
            currentRule.Selector = selector;
            return this;
        }

        public RenderingRulesMother SetOptions (Action<IRenderingRulesSetOptions> optionsAction)
        {
            optionsAction(rules.Options);
            return this;
        }

        private int counter;
        private RenderingRule currentRule;
        private RenderingRuleSet rules = new RenderingRuleSet ();
    }
}