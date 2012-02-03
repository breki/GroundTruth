using System;
using Brejc.Common.FileSystem;
using Brejc.OsmLibrary;

namespace GroundTruth.Tests
{
    public class OsmObjectMother
    {
        public OsmObjectBase CurrentObject
        {
            get { return currentObject; }
        }

        public OsmRelation CurrentRelation
        {
            get { return currentRelation; }
        }

        public IOsmDataFastAccess OsmDatabase
        {
            get { return osmDatabase; }
        }

        public OsmObjectMother AddNode()
        {
            double latitude = ConstructLatitude (idCounter);

            OsmNode node = new OsmNode(idCounter++, latitude, idCounter * 0.001);
            osmDatabase.AddNode(node);
            currentObject = node;
            return this;
        }

        public OsmObjectMother AddWay (int nodesCount)
        {
            OsmWay way = new OsmWay(idCounter++);
            osmDatabase.AddWay(way);

            for (int i = 0; i < nodesCount; i++)
            {
                AddNode();
                way.AddNode(currentObject.ObjectId);
            }

            currentObject = way;

            return this;
        }

        public OsmObjectMother AddRelation()
        {
            OsmRelation relation = new OsmRelation(idCounter++);
            osmDatabase.AddRelation(relation);

            currentObject = relation;
            currentRelation = relation;
            return this;
        }

        public OsmObjectMother AddToRelation(string role)
        {
            OsmReferenceType referenceType;

            if (currentObject is OsmNode)
                referenceType = OsmReferenceType.Node;
            else if (currentObject is OsmWay)
                referenceType = OsmReferenceType.Way;
            else if (currentObject is OsmRelation)
                referenceType = OsmReferenceType.Relation;
            else
                throw new NotSupportedException();

            currentRelation.AddMember(referenceType, currentObject.ObjectId, role);

            return this;
        }

        public OsmObjectMother LoadFromFile(string fileName)
        {
            osmDatabase = new InMemoryOsmDatabase();
            OsmXmlReader osmXmlReader = new OsmXmlReader ();
            osmXmlReader.Read (fileName, new WindowsFileSystem(), osmDatabase);

            osmDatabase.RemoveWaysWithMissingNodes ();

            return this;
        }

        public OsmObjectMother SelectWay (long wayId)
        {
            currentObject = osmDatabase.GetWay(wayId);
            return this;
        }

        public OsmObjectMother SetLatitudeMode (LatitudeMode mode)
        {
            latitudeMode = mode;
            return this;
        }

        public OsmObjectMother Tag (string key, string value)
        {
            currentObject.SetTag(key, value);
            return this;
        }

        private double ConstructLatitude(long variable)
        {
            switch (latitudeMode)
            {
                case LatitudeMode.NorthernHemisphere:
                    return variable*0.001;
                case LatitudeMode.SouthernHemisphere:
                    return variable * -0.001;
                case LatitudeMode.AroundEquator:
                    return variable * 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private InMemoryOsmDatabase osmDatabase = new InMemoryOsmDatabase ();
        private OsmObjectBase currentObject;
        private OsmRelation currentRelation;
        private long idCounter = 1;
        private LatitudeMode latitudeMode = LatitudeMode.NorthernHemisphere;
    }
}