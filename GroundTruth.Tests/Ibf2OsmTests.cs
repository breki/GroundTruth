using Brejc.Common.FileSystem;
using GroundTruth.Engine.Contours.Ibf2Osm;
using MbUnit.Framework;

namespace GroundTruth.Tests
{
    [TestFixture]
    public class Ibf2OsmTests
    {
        [Test]
        public void Ibf2Osm()
        {
            IIbf2OsmGenerator generator = new DefaultIbf2OsmGenerator(new WindowsFileSystem());
            Ibf2OsmGenerationParameters parameters = new Ibf2OsmGenerationParameters();
            parameters.IbfFileName = "../../../../Data/Samples/IBF/Maribor.ibf";

            generator.Run(parameters);
        }
    }
}