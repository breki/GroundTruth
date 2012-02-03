using System;
using Brejc.OsmLibrary;

namespace GroundTruth.Engine.Contours.Ibf2Osm
{
    public interface IContourOsmWayTagger
    {
        void Tag(OsmWay way, Int16 elevation);
    }
}