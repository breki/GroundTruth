using System.Diagnostics.CodeAnalysis;
using Brejc.OsmLibrary;
using GroundTruth.Engine.MapDataSources;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.PolishMaps
{
    /// <summary>
    /// Defines an interface for graphically representation of OSM elements.
    /// </summary>
    public interface IOsmObjectRenderingTemplate : IRenderingTemplate
    {
        /// <param name="osmObject">The OSM element to render.</param>
        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        void RenderOsmObject (
            MapMakerSettings mapMakerSettings,
            MapDataAnalysis analysis,
            InMemoryOsmDatabase osmDatabase,
            OsmObjectBase osmObject,
            OsmRelation parentRelation,
            CGpsMapperMapWriter mapWriter);
    }
}