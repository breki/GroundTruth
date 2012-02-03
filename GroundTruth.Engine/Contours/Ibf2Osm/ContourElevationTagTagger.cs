using Brejc.OsmLibrary;

namespace GroundTruth.Engine.Contours.Ibf2Osm
{
    public class ContourElevationTagTagger : IContourOsmWayTagger
    {
        public void Tag(OsmWay way, short elevation)
        {
            way.SetTag("contour", "elevation");
        }
    }
}