using Brejc.Common.FileSystem;
using Brejc.Common.Props;
using Brejc.Geometry;
using GroundTruth.Engine;
using MbUnit.Framework;

namespace GroundTruth.Tests
{
    public abstract class GroundTruthFixtureBase
    {
        [SetUp]
        public virtual void Setup ()
        {
            serializersRegistry = new SerializersRegistry ();
            serializersRegistry.RegisterSerializer<GisColor> (new GisColorPropertyValueSerializer ());
            fileSystem = new WindowsFileSystem();
        }

        protected ISerializersRegistry SerializersRegistry
        {
            get { return serializersRegistry; }
        }

        protected IFileSystem FileSystem
        {
            get { return fileSystem; }
        }

        private ISerializersRegistry serializersRegistry;
        private IFileSystem fileSystem;
    }
}