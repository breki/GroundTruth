using Brejc.OsmLibrary;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.LabelBuilding
{
    public class UppercaseLabelBuildingFunction : ILabelBuildingFunction
    {
        public string Calculate(MapMakerSettings mapMakerSettings, OsmObjectBase osmObjectBase, Tag osmTag, OsmRelation parentRelation)
        {
            return osmTag.Value.ToUpperInvariant();
        }
    }
}