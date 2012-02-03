using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Brejc.OsmLibrary;

namespace GroundTruth.Engine.Contours.Ibf2Osm
{
    [SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ele")]
    public class EleTagger : IContourOsmWayTagger
    {
        public void Tag(OsmWay way, short elevation)
        {
            way.SetTag("ele", elevation.ToString(CultureInfo.InvariantCulture));
        }
    }
}