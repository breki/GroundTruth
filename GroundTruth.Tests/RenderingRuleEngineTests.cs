using System.IO;
using System.Text;
using Brejc.Geometry;
using Brejc.OsmLibrary;
using GroundTruth.Engine.MapDataSources;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Tasks;
using MbUnit.Framework;
using Rhino.Mocks;

namespace GroundTruth.Tests
{
    [TestFixture]
    public class RenderingRuleEngineTests : GroundTruthFixtureBase
    {
        /// <summary>
        /// Tests rendering rules which use relation selectors.
        /// </summary>
        [Test]
        public void RenderCycleRouteRelation()
        {
            osmObjectMother
                .AddRelation()
                .Tag("type", "route")
                .AddWay(5)
                .AddToRelation("dictator");

            RenderingRulesMother rulesMother =
                new RenderingRulesMother ()
                .AddLineRule ()
                .AddRelationSelectors ("type", "route");

            OsmDataSource osmFile = new OsmDataSource (testDataProvider, SerializersRegistry);

            string mapContent = GenerateMap(rulesMother, osmFile);

            Assert.AreEqual (1, GetCountOf (mapContent, "[POLYLINE]"));
        }

        /// <summary>
        /// Tests how the rendering rule engine processes mixed-mode rules: rules which target both relations and ordinary OSM elements.
        /// </summary>
        [Test]
        public void MixedModeSelector()
        {
            osmObjectMother
                .AddRelation()
                .Tag("type", "route")
                .AddWay(5)
                .Tag("highway", "footway")
                .AddToRelation("dictator")
                .AddWay(5)
                .Tag("highway", "footway");

            RenderingRulesMother rulesMother =
                new RenderingRulesMother ()
                .AddLineRule()
                .AddSelectors("relation:{{tag|type|route}} or {{tag|highway|footway}}");

            OsmDataSource osmFile = new OsmDataSource (testDataProvider, SerializersRegistry);

            string mapContent = GenerateMap (rulesMother, osmFile);
            Assert.AreEqual (2, GetCountOf (mapContent, "[POLYLINE]"));
        }

        /// <summary>
        /// Tests rendering of multipolygons with holes.
        /// </summary>
        [Test]
        public void Multipolygon()
        {
            RenderingRulesMother rulesMother =
                new RenderingRulesMother ()
                .AddAreaRule()
                .AddSelectors ("{{tag|natural|water}}");

            OsmDataSource osmFile = new OsmDataSource (
                new OsmFileData (@"..\..\..\..\Data\Samples\KosmosProjects\Bled\Bled.osm", FileSystem), SerializersRegistry);

            string mapContent = GenerateMap (rulesMother, osmFile);

            Assert.AreEqual (1, GetCountOf (mapContent, "[POLYGON]"));
            Assert.AreEqual (2, GetCountOf (mapContent, "Data"));
        }

        /// <summary>
        /// Makes sure the same OSM element can be reused in multiple relations' rules.
        /// </summary>
        [Test]
        public void UsingObjectInMoreThanOneRelation()
        {
            osmObjectMother
                .AddRelation()
                .Tag("type", "route")
                .AddWay(5)
                .AddToRelation("dictator");

            long wayId = osmObjectMother.CurrentObject.ObjectId;

            // add the way to another relation
            osmObjectMother
                .AddRelation()
                .Tag("type", "somethingelse")
                .SelectWay (wayId)
                .AddToRelation("otherrole");

            RenderingRulesMother rulesMother =
                new RenderingRulesMother()
                    .AddLineRule()
                    .AddRelationSelectors("type", "route")
                    .AddLineRule()
                    .AddRelationSelectors("type", "somethingelse");

            OsmDataSource osmFile = new OsmDataSource (testDataProvider, SerializersRegistry);

            string mapContent = GenerateMap (rulesMother, osmFile);

            Assert.AreEqual (2, GetCountOf (mapContent, "[POLYLINE]"));
        }

        [Test]
        public void EquatorialLatitudes ()
        {
            string value = CGpsMapperMapWriter.RenderCoordinateValue(0.00001);
            Assert.AreEqual("0.00001", value);
        }

        [Test]
        public void IfBackgroundColorIsSetThenRenderBackgroundPolygon()
        {
            osmObjectMother
                .AddNode()
                .AddNode()
                .AddNode();

            RenderingRulesMother rulesMother =
                new RenderingRulesMother()
                    .SetOptions(o => o.LandBackgroundColor = new GisColor(0x218CFF))
                    .SetOptions(o => o.ForceBackgroundColor = true);

            OsmDataSource osmFile = new OsmDataSource (testDataProvider, SerializersRegistry);

            string mapContent = GenerateMap (rulesMother, osmFile);

            Assert.AreEqual (1, GetCountOf (mapContent, "[POLYGON]"));     
        }

        private string GenerateMap(RenderingRulesMother rulesMother, OsmDataSource osmFile)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                polishMapFileCreator.Expect (c => c.CreatePolishMapFile ()).Return (stream);

                settings.RenderingRules = rulesMother.Rules;

                osmFile.AnalyzeData (settings);
                osmFile.GeneratePolishMapFiles (runner, polishMapFileCreator);

                string mapContent = GetMapContentFromStream(stream);
                return mapContent;
            }
        }

        private static int GetCountOf (string content, string substring)
        {
            int i = 0;
            int count = 0;
            while (true)
            {
                i = content.IndexOf (substring, i);
                if (i == -1)
                    break;
                count++;
                i++;
            }

            return count;
        }

        private static string GetMapContentFromStream(MemoryStream stream)
        {
            byte[] data = stream.ToArray();
            string mapContent = Encoding.UTF8.GetString(data);
            Assert.IsFalse(string.IsNullOrEmpty(mapContent));
            return mapContent;
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            osmObjectMother = new OsmObjectMother ();
            
            this.testDataProvider = MockRepository.GenerateMock<IOsmDatabaseProvider> ();
            testDataProvider.Expect(p => p.IsReloadable).Return(true).Repeat.Any();
            testDataProvider.Expect(p => p.Provide()).Return((InMemoryOsmDatabase) osmObjectMother.OsmDatabase)
                .Repeat.Any();

            polishMapFileCreator = MockRepository.GenerateStub<IPolishMapFileCreator> ();

            this.settings = new MapMakerSettings ();

            this.runner = MockRepository.GenerateStub<ITaskRunner> ();
        }

        private OsmObjectMother osmObjectMother;
        private IPolishMapFileCreator polishMapFileCreator;
        private ITaskRunner runner;
        private MapMakerSettings settings;
        private IOsmDatabaseProvider testDataProvider;
    }
}
