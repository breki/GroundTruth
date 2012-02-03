using Brejc.DemLibrary;
using Brejc.DemLibrary.Srtm;
using Brejc.Geometry;
using Castle.Windsor;
using GroundTruth.Engine.Contours;
using MbUnit.Framework;
using Rhino.Mocks;

namespace GroundTruth.Tests
{
    [TestFixture]
    public class IbfCommandTests
    {
        [Test]
        public void ParseCommandArgs()
        {
            IbfCommandParser parser = new IbfCommandParser();
            ContoursGenerationParameters parameters = parser.Parse (
                new string[]
                    {
                        "-o:iso.ibf", 
                        "-b=10,20.3,15,22.33",
                        "-feet",
                        "-interval:150",
                        "-gridlat=0.5",
                        "-gridlon=0.5",
                        "-nocut",
                    });

            Assert.AreEqual("iso.ibf", parameters.OutputFile);
            Assert.AreEqual (new Bounds2 (20.3, 10, 22.33, 15), parameters.GenerationBounds);
            Assert.AreEqual (ContourUnits.Feet, parameters.ElevationUnit);
            Assert.AreEqual (150, parameters.IsohypseIntervalInUnits);
            Assert.AreEqual (0.5, parameters.LatitudeGrid);
            Assert.AreEqual (0.5, parameters.LongitudeGrid);
            Assert.IsFalse(parameters.CutToBounds);
        }

        [Test]
        public void ParseCommandArgsDefault ()
        {
            IbfCommandParser parser = new IbfCommandParser ();
            ContoursGenerationParameters parameters = parser.Parse(new string[] {});

            Assert.AreEqual ("output.ibf", parameters.OutputFile);
            Assert.AreEqual (null, parameters.GenerationBounds);
            Assert.AreEqual (ContourUnits.Meters, parameters.ElevationUnit);
            Assert.AreEqual (20, parameters.IsohypseIntervalInUnits);
            Assert.AreEqual (0.25, parameters.LatitudeGrid);
            Assert.AreEqual (0.25, parameters.LongitudeGrid);
            Assert.IsTrue(parameters.CutToBounds);
        }

        [Test, ExpectedArgumentException("Map boundaries have not been specified")]
        public void GenerateIbfMissingBounds()
        {
            ContoursGenerationParameters parameters = new ContoursGenerationParameters();
            IContoursGenerator generator = windsorContainer.Resolve<IContoursGenerator> ();
            generator.Run (parameters);
        }

        [Test]
        public void GenerateSampleIbf ()
        {
            ContoursGenerationParameters parameters = new ContoursGenerationParameters ();
            parameters.GenerationBounds = new Bounds2 (15.5, 46.5, 15.6, 46.6);

            // todo new
            //DemSystemConfiguration configuration = (DemSystemConfiguration) windsorContainer.Resolve<IDemSystemConfiguration>();
            //configuration.DemCacheDirectoryRoot = @"../../../../Data/Samples/Dem";

            IContoursGenerator generator = windsorContainer.Resolve<IContoursGenerator>();
            generator.Run (parameters);
        }

        [Test, Explicit]
        public void GenerateIbfForMariborAndAround ()
        {
            ContoursGenerationParameters parameters = new ContoursGenerationParameters ();
            parameters.GenerationBounds = new Bounds2 (15.25, 46.25, 15.75, 46.75);
            parameters.OutputFile = "Maribor.ibf";
            parameters.IsohypseIntervalInUnits = 10;
            parameters.LatitudeGrid = 0.5;
            parameters.LongitudeGrid = 0.5;

            IContoursGenerator generator = windsorContainer.Resolve<IContoursGenerator> ();
            generator.Run (parameters);
        }

        [SetUp]
        public void Setup ()
        {
            windsorContainer = Program.ConfigureWindsor ();
        }

        private IWindsorContainer windsorContainer;
    }
}