using System;

namespace GroundTruth.Engine.Rules
{
    [Flags]
    public enum RenderingRuleTargets
    {
        None = 0,
        Nodes = 1 << 0,
        Ways = 1 << 1,
        Areas = 1 << 2,
        Relations = 1 << 3,
    }
}