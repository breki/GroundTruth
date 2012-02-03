using System.Collections.Generic;
using Brejc.OsmLibrary;

namespace GroundTruth.Engine.MapDataSources
{
    public class MultipolygonRelationsProcessor
    {
        public MultipolygonRelationsProcessor(InMemoryOsmDatabase osmDatabase)
        {
            throw new System.NotImplementedException();
        }

        public void RememberMultipolygonRelation(OsmRelation relation)
        {
            throw new System.NotImplementedException();
        }

        public bool IsWayUsedAsInnerPolygon(long objectId)
        {
            throw new System.NotImplementedException();
        }

        public bool IsWayUsedAsOuterPolygon(long objectId)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<OsmAreaWithHoles> ListAreasWithHolesForSpecificWay(long objectId)
        {
            throw new System.NotImplementedException();
        }
    }
}