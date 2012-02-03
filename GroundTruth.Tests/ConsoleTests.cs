using System.Collections;
using Brejc.DemLibrary.Srtm;
using Brejc.Rasters;
using Castle.Windsor;
using MbUnit.Framework;

namespace GroundTruth.Tests
{
    [TestFixture]
    public class ConsoleTests
    {
        [Test]
        public void ConfigureWindsor()
        {
            IRasterSource source = windsorContainer.Resolve<IRasterSource> ();
            Srtm3IndexFetcher indexFetcher = windsorContainer.Resolve<Srtm3IndexFetcher> ();

            Srtm3Source srtm3Source = (Srtm3Source) source;

            // todo new
            //Assert.AreEqual(
            //    "http://dds.cr.usgs.gov/srtm/version2_1/",
            //    ((DefaultSrtmServerClient) srtm3Source.SrtmServerClient).SrtmServerUrl);

            string[] args = new string[0];
            ConsoleApp consoleApp = CreateConsoleApp(args);
        }

        [Test, Explicit]
        public void MakingMap()
        {
            ConsoleApp consoleApp = CreateConsoleApp(
                new string[]
                    {
                        @"makemap",
                        @"-osm=sicily.osm",
                        //@"-testmap",
                        @"-r=PEDI_RenderingRulest-1.txt",
                        //@"-r=..\..\..\GroundTruth\Rules\DefaultRules.txt",
                        //@"-r=..\..\..\GroundTruth\Rules\HikingMapRules.txt",
                        @"-ct=..\..\..\GroundTruth\Rules\CharacterConversionTable.txt",
                        @"-tt=..\..\..\GroundTruth\Rules\StandardGarminTypes.txt",
                        @"-outputpath=pedi",
                        @"-mi=78320001",
                        @"-mn=Main Map",
                        @"-fc=457",
                        @"-fn=GroundTruth OSM Maps",
                        @"-pc=732",
                        @"-pn=Slovenia OSM Hiking Map",
                        //@"-ele=f",
                        //@"-param=LBLcoding:9",
                        //@"-param=CodePage:1252",
                        @"-param=PreProcess:P",
                        @"-simplifylevel=10",
                        @"-of=..\..\SampleOptionsFile.txt",
                    });
            Assert.AreEqual(0, consoleApp.Process());
        }

        [Test]
        public void MultipleMaps ()
        {
            ConsoleApp consoleApp = CreateConsoleApp (
                new string[]
                    {
                        @"makemap",
                        @"-osm=..\..\..\..\Data\Samples\KosmosProjects\Maribor\Maribor.osm",
                        @"-mn=Map1",
                        @"-osm=..\..\..\..\Data\Samples\KosmosProjects\Isle of Wight\osmxapi_20080706_213852.osm",
                        @"-mn=Map2",
                        @"-r=..\..\..\GroundTruth\Rules\HikingMapRules.txt",
                        @"-ct=..\..\..\GroundTruth\Rules\CharacterConversionTable.txt",
                        @"-tt=..\..\..\GroundTruth\Rules\StandardGarminTypes.txt",
                        @"-outputpath=multiplemaps",
                        @"-mi=5565656",
                        @"-fc=888",
                        @"-fn=Test Multi Map",
                        @"-pc=999",
                        @"-pn=Test Multi Map",
                        //@"-nocgp=true"
                        @"-cgp=..\..\..\..\lib\cgpsmapper",
                        "-extimeout=10"
                    });
            Assert.AreEqual (0, consoleApp.Process ());
        }

        [Test, Pending]
        public void SplittedMaps ()
        {
            ConsoleApp consoleApp = CreateConsoleApp (
                new string[]
                    {
                        @"makemap",
                        @"-osm=..\..\..\..\Data\Samples\KosmosProjects\Maribor\Maribor.osm",
                        @"-mn=Map1",
                        @"-osm=..\..\..\..\Data\Samples\KosmosProjects\Isle of Wight\osmxapi_20080706_213852.osm",
                        @"-mn=Map2",
                        @"-r=..\..\..\GroundTruth\Rules\HikingMapRules.txt",
                        @"-ct=..\..\..\GroundTruth\Rules\CharacterConversionTable.txt",
                        @"-tt=..\..\..\GroundTruth\Rules\StandardGarminTypes.txt",
                        @"-outputpath=multiplemaps",
                        @"-mi=5565656",
                        @"-fc=888",
                        @"-fn=Test Split Map",
                        @"-pc=999",
                        @"-pn=Test Split Map",
                        //@"-nocgp=true"
                        @"-cgp=..\..\..\..\lib\cgpsmapper",
                        @"-splitframe=0.001,0001",
                        @"-osmosispath=..\..\..\..\lib\osmosis\bin\osmosis.bat"
                    });
            Assert.AreEqual (0, consoleApp.Process ());
        }

        [Test, Explicit]
        public void DownloadOsmData()
        {
            ConsoleApp consoleApp = CreateConsoleApp (
                new string[]
                    {
                        @"getdata",
                        @"-bu=http://www.openstreetmap.org/?lat=46.493&lon=15.5138&zoom=13&layers=0B00FTF",
                    });
            Assert.AreEqual(0, consoleApp.Process());
        }

        [Test]
        public void GenerateContourMaps()
        {
            ConsoleApp consoleApp = CreateConsoleApp (
                new string[]
                    {
                        @"makemap",
                        @"-ibf=..\..\..\..\Data\Samples\IBF\Mallorca.ibf",
                        @"-mn=Map1",
                        @"-tt=..\..\..\GroundTruth\Rules\StandardGarminTypes.txt",
                        @"-outputpath=contourmaps",
                        @"-mi=5565656",
                        @"-fc=888",
                        @"-fn=Test Multi Map",
                        @"-pc=999",
                        @"-pn=Test Multi Map",
                        //@"-nocgp=true",
                        @"-cgp=..\..\..\..\lib\cgpsmapper",
                    });
            Assert.AreEqual (0, consoleApp.Process ());            
        }

        [Test, Explicit("Needs access to NASA FTP site")]
        public void GenerateFeetContourMaps ()
        {
            ConsoleApp consoleApp;

            consoleApp = CreateConsoleApp (
                new string[]
                    {
                        @"contours",
                        @"-b=46.25,15.25,46.75,15.75",
                        @"-feet",
                        @"-int=50"
                    });
            Assert.AreEqual (0, consoleApp.Process ());

            consoleApp = CreateConsoleApp (
                new string[]
                    {
                        @"makemap",
                        @"-osm=..\..\..\..\Data\Samples\KosmosProjects\Maribor\Maribor.osm",
                        @"-ibf=output.ibf",
                        @"-mn=Map1",
                        @"-cr=..\..\..\GroundTruth\Rules\ContoursRulesFeet.txt",
                        //@"-cr=..\..\..\GroundTruth\Rules\ContoursRulesMetric.txt",
                        @"-ele=f",
                        @"-r=..\..\..\GroundTruth\Rules\HikingMapRules.txt",
                        @"-ct=..\..\..\GroundTruth\Rules\CharacterConversionTable.txt",
                        @"-tt=..\..\..\GroundTruth\Rules\StandardGarminTypes.txt",
                        @"-outputpath=contourmaps",
                        @"-mi=5565656",
                        @"-fc=888",
                        @"-fn=Test Multi Map",
                        @"-pc=999",
                        @"-pn=Test Multi Map",
                        //@"-nocgp=true",
                        @"-cgp=..\..\..\..\lib\cgpsmapper",
                    });
            Assert.AreEqual (0, consoleApp.Process ());
        }

        [Test]
        public void Ibf2Osm()
        {
            ConsoleApp consoleApp = CreateConsoleApp (
                new string[]
                    {
                        @"ibf2osm",
                        @"-ibf=../../../../Data/Samples/IBF/Maribor.ibf",
                        @"-outputfileformat=blabla{0}.osm",
                        @"-sid=1000",
                        @"-sid=1000",
                        @"-tagce",
                        @"-cat=1000,250"
                    });
            Assert.AreEqual (0, consoleApp.Process ());
        }

        [Test, Explicit]
        public void TryToGenerateContoursOutsideOfSrtmBoundaries()
        {
            ConsoleApp consoleApp;

            consoleApp = CreateConsoleApp (
                new string[]
                    {
                        @"contours",
                        @"-bu=http://www.openstreetmap.org/?lat=70.5&lon=27&zoom=8&layers=B000FTF",
                        @"-int=50"
                    });
            Assert.AreEqual (0, consoleApp.Process ());

            consoleApp = CreateConsoleApp (
                new string[]
                    {
                        @"makemap",
                        @"-ibf=output.ibf",
                        @"-mn=Map1",
                        @"-cr=..\..\..\GroundTruth\Rules\ContoursRulesMetric.txt",
                        @"-r=..\..\..\GroundTruth\Rules\HikingMapRules.txt",
                        @"-ct=..\..\..\GroundTruth\Rules\CharacterConversionTable.txt",
                        @"-tt=..\..\..\GroundTruth\Rules\StandardGarminTypes.txt",
                        @"-outputpath=contourmaps",
                        @"-mi=5565656",
                        @"-fc=888",
                        @"-fn=Test Multi Map",
                        @"-pc=999",
                        @"-pn=Test Multi Map",
                        @"-cgp=..\..\..\..\lib\cgpsmapper",
                    });
            Assert.AreEqual (0, consoleApp.Process ());
        }

        /// <summary>Test case setup code.</summary>
        [SetUp]
        public void Setup()
        {
            windsorContainer = Program.ConfigureWindsor();
            commands = windsorContainer.ResolveAll<IConsoleCommand> ();
        }

        [FixtureSetUp]
        public void TestFixtureSetup ()
        {
            log4net.Config.XmlConfigurator.Configure ();
        }

        private ConsoleApp CreateConsoleApp (string[] args)
        {
            return windsorContainer.Resolve<ConsoleApp> (new Hashtable () { { "args", args }, { "commands", commands } });           
        }

        private IConsoleCommand[] commands;
        private IWindsorContainer windsorContainer;
    }
}