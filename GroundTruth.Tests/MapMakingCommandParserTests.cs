using GroundTruth.Engine.Tasks;
using MbUnit.Framework;
using Rhino.Mocks;

namespace GroundTruth.Tests
{
    [TestFixture]
    public class MapMakingCommandParserTests : GroundTruthFixtureBase
    {
        [Test]
        public void MultipleMaps()
        {
            MapMakerSettings settings = new MapMakerSettings();

            ITaskRunner taskRunner = MockRepository.GenerateStub<ITaskRunner>();
            taskRunner.Expect(r => r.MapMakerSettings).Return(settings).Repeat.Any();

            MapMakingCommand cmd = new MapMakingCommand(taskRunner, SerializersRegistry, FileSystem);
            cmd.Execute(new[] {"-osm:test1.osm", "-osm:test2.osm"});

            Assert.AreEqual(2, settings.MapDataSources.Count);
        }
    }
}