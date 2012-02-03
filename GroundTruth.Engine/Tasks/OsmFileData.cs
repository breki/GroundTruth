using Brejc.Common.FileSystem;
using Brejc.OsmLibrary;
using log4net;
using log4net.Core;

namespace GroundTruth.Engine.Tasks
{
    public class OsmFileData : IOsmDatabaseProvider
    {
        public OsmFileData(string osmFileName, IFileSystem fileSystem)
        {
            this.osmFileName = osmFileName;
            this.fileSystem = fileSystem;
        }

        public bool IsReloadable
        {
            get { return true; }
        }

        public string OsmFileName
        {
            get { return osmFileName; }
            set { osmFileName = value; }
        }

        public InMemoryOsmDatabase Provide ()
        {
            consoleLogger.WriteLine(log, Level.Info, "Loading OSM file '{0}'...", osmFileName);

            InMemoryOsmDatabase osmDatabase = new InMemoryOsmDatabase ();
            OsmXmlReader osmXmlReader = new OsmXmlReader ();
            osmXmlReader.Read (osmFileName, fileSystem, osmDatabase);

            osmDatabase.RemoveWaysWithMissingNodes ();

            return osmDatabase;
        }

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger();
        private static readonly ILog log = LogManager.GetLogger(typeof (OsmFileData));
        private string osmFileName;
        private readonly IFileSystem fileSystem;
    }
}