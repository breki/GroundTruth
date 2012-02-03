using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Brejc.Common;
using Brejc.Common.FileSystem;
using Brejc.Geometry;
using Brejc.OsmLibrary;

namespace GroundTruth.Engine.Contours.Ibf2Osm
{
    // todo ibf
    public class DefaultIbf2OsmGenerator : IIbf2OsmGenerator
    {
        public DefaultIbf2OsmGenerator(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public AsyncOperationCallback ProgressCallback
        {
            get { return progressCallback; }
            set { progressCallback = value; }
        }

        public void Run (Ibf2OsmGenerationParameters parameters)
        {
            this.parameters = parameters;
            wayId = parameters.StartId;
            nodeId = parameters.StartId;

            //LoadIbfFile();

            //for (int ibfAreaDefinitionIndex = 0; 
            //     ibfAreaDefinitionIndex < ibfFile.AreaDirectory.Entries.Count; 
            //     ibfAreaDefinitionIndex++)
            //{
            //    GenerateOsmFileForArea(ibfAreaDefinitionIndex);
            //}
        }

        //private void LoadIbfFile()
        //{
        //    IbfUtilities utilities = new IbfUtilities ();
        //    ibfFile = utilities.Open (parameters.IbfFileName);
        //}

        //private void GenerateOsmFileForArea(int ibfAreaDefinitionIndex)
        //{
        //    InMemoryOsmDatabase osmDatabase = new InMemoryOsmDatabase();

        //    IbfUtilities utilities = new IbfUtilities ();

        //    IbfAreaDirectoryEntry areaDirectoryEntry = ibfFile.AreaDirectory.Entries[ibfAreaDefinitionIndex];
        //    IbfAreaDefinition areaDefinition = utilities.LoadAreaDefinition (ibfFile, ibfAreaDefinitionIndex);

        //    foreach (IbfElevationDefinition elevationDefinition in areaDefinition.ElevationDefinitions.Values)
        //    {
        //        foreach (IbfIsohypseDefinition isohypseDefinition in elevationDefinition.IsohypseDefinitions)
        //        {
        //            Point2<double>[] isohypsePoints = isohypseDefinition.CalculateIsohypsePoints (
        //                areaDirectoryEntry,
        //                areaDefinition);

        //            // skip contours which are not really useful (all points are the same)
        //            bool allPointsTheSame = CheckIsohypseHasAllSamePoints (isohypseDefinition, isohypsePoints);

        //            if (allPointsTheSame)
        //                continue;

        //            AddContourToOsmDatabase (elevationDefinition, isohypsePoints, osmDatabase);
        //        }
        //    }

        //    SaveOsmFile(osmDatabase);
        //}

        //private static bool CheckIsohypseHasAllSamePoints (
        //    IbfIsohypseDefinition isohypseDefinition,
        //    Point2<double>[] isohypsePoints)
        //{
        //    bool allPointsTheSame = true;

        //    for (int i = 1; i < isohypseDefinition.MovementsCount; i++)
        //    {
        //        if (isohypsePoints[0] != isohypsePoints[i])
        //        {
        //            allPointsTheSame = false;
        //            break;
        //        }
        //    }

        //    return allPointsTheSame;
        //}

        //[SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "definition")]
        //private void AddContourToOsmDatabase (
        //    IbfElevationDefinition definition, 
        //    Point2<double>[] isohypsePoints,
        //    InMemoryOsmDatabase osmDatabase)
        //{
        //    OsmWay isohypseWay = new OsmWay (wayId++);
        //    isohypseWay.ExtendedData = new OsmObjectExtendedData();
        //    isohypseWay.ExtendedData.Visible = false;
        //    isohypseWay.ExtendedData.Version = 1;
        //    osmDatabase.AddWay (isohypseWay);

        //    TagWay(isohypseWay, definition.Elevation);

        //    int firstNodeId = nodeId;

        //    for (int i = 0; i < isohypsePoints.Length; i++)
        //    {
        //        Point2<double> point = isohypsePoints[i];

        //        if (i == isohypsePoints.Length - 1)
        //        {
        //            if (point == isohypsePoints[0])
        //            {
        //                isohypseWay.AddNode(firstNodeId);
        //                break;
        //            }
        //        }

        //        OsmNode node = new OsmNode (nodeId++, point.X, point.Y);
        //        node.ExtendedData = new OsmObjectExtendedData();
        //        node.ExtendedData.Visible = false;
        //        node.ExtendedData.Version = 1;
        //        osmDatabase.AddNode (node);

        //        isohypseWay.AddNode (node.ObjectId);
        //    }
        //}

        private void TagWay(OsmWay way, short elevation)
        {
            foreach (IContourOsmWayTagger tagger in parameters.OsmWayTaggers)
                tagger.Tag(way, elevation);
        }

        private void SaveOsmFile (IOsmDataFastAccess osmDatabase)
        {
            if (false == Directory.Exists (parameters.OutputDir))
                Directory.CreateDirectory(parameters.OutputDir);

            string osmFileName = Path.Combine(
                parameters.OutputDir,
                string.Format(
                    CultureInfo.InvariantCulture,
                    parameters.OutputFileFormat,
                    ++osmFilesCounter));

            OsmXmlWriter writer = new OsmXmlWriter();
            writer.Settings.GeneratorName = "GroundTruth";
            writer.Write(osmFileName, fileSystem, osmDatabase);
        }

        //private IbfFile ibfFile;
        private int nodeId;
        private int osmFilesCounter;
        private AsyncOperationCallback progressCallback;
        private Ibf2OsmGenerationParameters parameters;
        private int wayId;
        private readonly IFileSystem fileSystem;
    }
}