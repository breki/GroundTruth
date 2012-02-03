using System.Diagnostics.CodeAnalysis;
using Brejc.OsmLibrary;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.LabelBuilding
{
    public interface ILabelBuildingFunction
    {
        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        string Calculate (
            MapMakerSettings mapMakerSettings, 
            OsmObjectBase osmObjectBase, 
            Tag osmTag, 
            OsmRelation parentRelation);
    }
}