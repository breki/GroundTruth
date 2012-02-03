using System.Globalization;
using Brejc.OsmLibrary;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.LabelBuilding
{
    public class SpecialCodeLabelBuildingFunction : ILabelBuildingFunction
    {
        public SpecialCodeLabelBuildingFunction(int specialCode)
        {
            this.specialCode = specialCode;
        }

        public string Calculate (MapMakerSettings mapMakerSettings, OsmObjectBase osmObjectBase, Tag osmTag, OsmRelation parentRelation)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "~[0x{0:x}]",
                specialCode);
        }

        private readonly int specialCode;
    }
}