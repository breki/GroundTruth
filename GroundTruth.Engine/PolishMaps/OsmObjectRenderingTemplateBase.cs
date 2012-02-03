using Brejc.OsmLibrary;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.PolishMaps
{
    public abstract class OsmObjectRenderingTemplateBase : TemplateBase, IOsmObjectRenderingTemplate
    {
        public abstract void RenderOsmObject (
            MapMakerSettings mapMakerSettings,
            MapDataAnalysis analysis,
            InMemoryOsmDatabase osmDatabase,
            OsmObjectBase osmObject,
            OsmRelation parentRelation,
            CGpsMapperMapWriter mapWriter);

        protected static OsmNode[] GetNodesForWay (IOsmDataFastAccess osmDatabase, OsmWay way)
        {
            OsmNode[] nodes = new OsmNode[way.NodesCount];
            for (int i = 0; i < way.NodesCount; i++)
                nodes[i] = osmDatabase.GetNode (way.Nodes[i]);

            return nodes;
        }
    }
}