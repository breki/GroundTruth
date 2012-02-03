using Brejc.Common.Props;
using Brejc.Geometry;

namespace GroundTruth.Engine.Rules
{
    public interface IRenderingRulesSetOptions
    {
        bool ForceBackgroundColor { get; set; }
        GisColor LandBackgroundColor { get; set; }
        IProperties Properties { get; }
        string RulesVersion { get; set; }
        GisColor SeaColor { get; set; }
    }
}