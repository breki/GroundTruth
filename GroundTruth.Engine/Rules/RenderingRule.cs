using System.Collections.Generic;
using System.Text;
using Brejc.OsmLibrary;
using GroundTruth.Engine.PolishMaps;

namespace GroundTruth.Engine.Rules
{
    public class RenderingRule
    {
        public RenderingRule (string ruleName,
                              RenderingRuleTargets targets,
                              IOsmElementSelector selector,
                              IOsmObjectRenderingTemplate template)
        {
            this.ruleName = ruleName;
            this.targets = targets;
            this.selector = selector;
            this.template = template;
        }

        public string RuleName
        {
            get { return ruleName; }
            set { ruleName = value; }
        }

        public IOsmElementSelector Selector
        {
            get { return selector; }
            set { selector = value; }
        }

        public RenderingRuleTargets Targets
        {
            get { return targets; }
            set { targets = value; }
        }

        public bool TargetsRelation
        {
            get { return selector.TargetsRelation; }
        }

        public IOsmObjectRenderingTemplate Template
        {
            get { return template; }
            set { template = value; }
        }

        public virtual bool IsMatch (OsmRelation parentRelation, OsmObjectBase osmElement)
        {
            if (selector == null)
                return true;

            return selector.IsMatch (parentRelation, osmElement);
        }

        public void MarkHardwareLevelsUsed (MapDataAnalysis analysis)
        {
            if (Template.Style.MinZoomFactor != int.MinValue)
                analysis.MarkUsedHardwareLevel (Template.Style.MinZoomFactor);
            if (Template.Style.MaxZoomFactor != int.MaxValue)
                analysis.MarkUsedHardwareLevel (Template.Style.MaxZoomFactor);
        }

        public override string ToString()
        {
            StringBuilder desc = new StringBuilder();
            desc.AppendFormat("Name='{0}',", ruleName);
            desc.AppendFormat("Targets='{0}',", targets);
            return desc.ToString();
        }

        private IOsmElementSelector selector;
        private string ruleName;
        private RenderingRuleTargets targets;
        private IOsmObjectRenderingTemplate template;
    }
}