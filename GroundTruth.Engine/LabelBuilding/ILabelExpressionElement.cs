using System.Diagnostics.CodeAnalysis;
using System.Text;
using Brejc.OsmLibrary;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.LabelBuilding
{
    public interface ILabelExpressionElement
    {
        bool IsConstant { get; }

        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        void BuildLabel (
            MapMakerSettings mapMakerSettings,
            StringBuilder label, 
            OsmObjectBase osmObject, 
            OsmRelation parentRelation,
            Tag osmTag);
    }
}