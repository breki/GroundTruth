using System.Collections.Generic;

namespace GroundTruth.Engine.Contours.Ibf2Osm
{
    public class Ibf2OsmGenerationParameters
    {
        public Ibf2OsmGenerationParameters()
        {
            osmWayTaggers.Add(new EleTagger());
        }

        public string IbfFileName
        {
            get { return ibfFileName; }
            set { ibfFileName = value; }
        }

        public IList<IContourOsmWayTagger> OsmWayTaggers
        {
            get { return osmWayTaggers; }
        }

        public string OutputDir
        {
            get { return outputDir; }
            set { outputDir = value; }
        }

        public string OutputFileFormat
        {
            get { return outputFileFormat; }
            set { outputFileFormat = value; }
        }

        public int StartId
        {
            get { return startId; }
            set { startId = value; }
        }

        private string ibfFileName = "output.ibf";
        private string outputDir = "Output";
        private string outputFileFormat = "Contours{0}.osm.bz2";
        private int startId = 1000000000;
        private List<IContourOsmWayTagger> osmWayTaggers = new List<IContourOsmWayTagger>();
    }
}