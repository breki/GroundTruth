using Brejc.Geometry;
using GroundTruth.Engine;
using GroundTruth.Engine.MapDataSources;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Tasks;
using MbUnit.Framework;

namespace GroundTruth.Tests
{
    [TestFixture]
    public class MapMakerTests : GroundTruthFixtureBase
    {
        [Test]
        public void TestMapMaker()
        {
            MapMakerSettings mapMakerSettings = new MapMakerSettings();
            mapMakerSettings.MapDataSources.Add (
                new OsmDataSource (new OsmFileData (@"..\..\..\..\Data\Samples\KosmosProjects\Maribor\Maribor.osm", FileSystem), SerializersRegistry));
            mapMakerSettings.StartingMapId = "12345678";
            mapMakerSettings.MapNamePrefix = "TestMap";
            //mapMakerSettings.RenderingRulesSource = @"http://wiki.openstreetmap.org/index.php?title=GroundTruth&action=raw";
            mapMakerSettings.RenderingRulesSource = @"..\..\..\GroundTruth\Rules\HikingMapRules.txt";
            mapMakerSettings.StandardGarminTypesSource = @"..\..\..\GroundTruth\Rules\StandardGarminTypes.txt";
            mapMakerSettings.CharactersConversionTableSource = @"..\..\..\GroundTruth\Rules\CharacterConversionTable.txt";
            mapMakerSettings.CGpsMapperPath = @"..\..\..\..\lib\cgpsmapper";
            mapMakerSettings.SplitFrame = new Point2<double>(0.1, 0.1);

            MapMaker mapMaker = new MapMaker ();
            mapMaker.MapMakerSettings = mapMakerSettings;
            mapMaker.AddTask (new GenerateMapPolishFilesTask (mapMakerSettings, SerializersRegistry));
            mapMaker.AddTask (new GenerateMapTypesPolishFileTask ());
            mapMaker.AddTask (new GenerateTypeFileTask ());
            mapMaker.AddTask (new GenerateMapImgFilesTask ());
            mapMaker.AddTask (new GeneratePreviewPolishFileTask ());
            mapMaker.AddTask (new GeneratePreviewAndTdbFilesTask ());
            mapMaker.AddTask (new GenerateMapListFileTask ());
            //mapMaker.AddTask (new UploadMapsToGpsTask ());
            mapMaker.AddTask (new CopyProductFilesToOutputDirTask ());
            mapMaker.AddTask (new GenerateMapSourceRegFilesTask ());
            mapMaker.Run ();
        }

        [Test]
        public void MakeMapWithSeaPolygons ()
        {
            MapMakerSettings mapMakerSettings = new MapMakerSettings ();
            mapMakerSettings.MapDataSources.Add (
                new OsmDataSource (new OsmFileData (@"..\..\..\..\Data\Samples\KosmosProjects\Coastlines\Coastlines2.osm", FileSystem), SerializersRegistry));
            mapMakerSettings.StartingMapId = "12345678";
            mapMakerSettings.MapNamePrefix = "TestMap";
            //mapMakerSettings.RenderingRulesSource = @"http://wiki.openstreetmap.org/index.php?title=GroundTruth&action=raw";
            mapMakerSettings.RenderingRulesSource = @"..\..\..\GroundTruth\Rules\HikingMapRules.txt";
            mapMakerSettings.StandardGarminTypesSource = @"..\..\..\GroundTruth\Rules\StandardGarminTypes.txt";
            mapMakerSettings.CharactersConversionTableSource = @"..\..\..\GroundTruth\Rules\CharacterConversionTable.txt";
            mapMakerSettings.CGpsMapperPath = @"..\..\..\..\lib\cgpsmapper";
            mapMakerSettings.SplitFrame = new Point2<double> (0.1, 0.1);

            MapMaker mapMaker = new MapMaker ();
            mapMaker.MapMakerSettings = mapMakerSettings;
            mapMaker.AddTask (new GenerateMapPolishFilesTask (mapMakerSettings, SerializersRegistry));
            mapMaker.AddTask (new GenerateMapTypesPolishFileTask ());
            mapMaker.AddTask (new GenerateTypeFileTask ());
            mapMaker.AddTask (new GenerateMapImgFilesTask ());
            mapMaker.AddTask (new GeneratePreviewPolishFileTask ());
            mapMaker.AddTask (new GeneratePreviewAndTdbFilesTask ());
            mapMaker.AddTask (new GenerateMapListFileTask ());
            //mapMaker.AddTask (new UploadMapsToGpsTask ());
            mapMaker.AddTask (new CopyProductFilesToOutputDirTask ());
            mapMaker.AddTask (new GenerateMapSourceRegFilesTask ());
            mapMaker.Run ();
        }

        [Test, Explicit]
        public void MakeMapFromNet()
        {
            MapMakerSettings mapMakerSettings = new MapMakerSettings ();
            mapMakerSettings.MapDataSources.Add (
                new OsmDataSource (new OsmFileData (@"..\..\..\..\Data\Samples\KosmosProjects\Maribor\Maribor.osm", FileSystem), SerializersRegistry));
            mapMakerSettings.StartingMapId = "12345678";
            mapMakerSettings.MapNamePrefix = "TestMap";
            mapMakerSettings.RenderingRulesSource = @"http://wiki.openstreetmap.org/index.php?title=GroundTruth_Driving_Map&action=edit";
            //mapMakerSettings.RenderingRulesSource = @"GroundTruthTests\SampleRules.txt";
            mapMakerSettings.StandardGarminTypesSource = @"..\..\..\GroundTruth\Rules\StandardGarminTypes.txt";
            mapMakerSettings.CharactersConversionTableSource = @"..\..\..\GroundTruth\Rules\CharacterConversionTable.txt";
            mapMakerSettings.CGpsMapperPath = @"..\..\..\..\lib\cgpsmapper";

            MapMaker mapMaker = new MapMaker ();
            mapMaker.MapMakerSettings = mapMakerSettings;
            mapMaker.AddTask (new GenerateMapPolishFilesTask (mapMakerSettings, SerializersRegistry));
            mapMaker.AddTask (new GenerateMapTypesPolishFileTask ());
            mapMaker.AddTask (new GenerateTypeFileTask ());
            mapMaker.AddTask (new GenerateMapImgFilesTask ());
            mapMaker.AddTask (new GeneratePreviewPolishFileTask ());
            mapMaker.AddTask (new GeneratePreviewAndTdbFilesTask ());
            mapMaker.AddTask (new GenerateMapListFileTask ());
            //mapMaker.AddTask (new UploadMapsToGpsTask ());
            mapMaker.AddTask (new CopyProductFilesToOutputDirTask ());
            mapMaker.AddTask (new GenerateMapSourceRegFilesTask ());
            mapMaker.Run ();
        }

        [Test]
        public void CheckNewTypesIdGeneration()
        {
            TypesRegistry reg = new TypesRegistry();

            for (int i = 0; i < 100; i++)
            {
                AreaTypeRegistration type = reg.RegisterNewAreaType(i.ToString(), false);

                Assert.AreNotEqual<int> (0x010f20, type.TypeId);
                Assert.AreNotEqual<int> (0x010f21, type.TypeId);
                Assert.AreNotEqual<int> (0x010f30, type.TypeId);
            }
        }

        [FixtureSetUp]
        public void TestFixtureSetup()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}