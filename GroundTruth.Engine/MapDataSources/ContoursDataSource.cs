using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Brejc.Geometry;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Rules;
using GroundTruth.Engine.Tasks;
using GroundTruth.Engine.Wiki;
using log4net;
using log4net.Core;

namespace GroundTruth.Engine.MapDataSources
{
    // todo ibf
    public class ContoursDataSource : MapDataSourceBase
    {
        public ContoursDataSource(string ibfFileName)
        {
            this.ibfFileName = ibfFileName;
        }

        public override void ReleaseData()
        {
            //ibfFile = null;
        }

        public override void Split(
            MapMakerSettings mapMakerSettings, 
            IList<IMapDataSource> mapDataSourcesAfterSplitting,
            IProgramRunner programRunner)
        {
            mapDataSourcesAfterSplitting.Add(this);
        }

        protected override void AnalyzeDataInternal()
        {
            LoadRulesIfNecessary();

            Analysis = new MapDataAnalysis();
            Settings.ContoursRenderingRules.MarkHardwareLevelsUsed(Analysis);
            Settings.ContoursRenderingRules.RegisterTypes(Settings.TypesRegistry);

            tileBoxRegistration = Settings.TypesRegistry.RegisterNewLineType("ContoursTileBox");
            tileBoxRegistration.MaxLevel = 24;
            tileBoxRegistration.MinLevel = 24;
            tileBoxRegistration.Pattern = new PatternDefinition();
            tileBoxRegistration.Pattern.AddColor ("#7F3300");
            tileBoxRegistration.Pattern.PatternLines.Add("0000");
        }

        public override int DefaultSimplifyLevel
        {
            get { return 1; }
        }

        public override int DefaultTreeSize
        {
            get { return 2000; }
        }

        public override string DetermineMapTransparencyMode ()
        {
            // we should always use semi-transparent mode for contour maps
            return "S";
        }

        protected override void FetchData()
        {
            //if (ibfFile == null)
            //{
            //    IbfUtilities utilities = new IbfUtilities ();
            //    ibfFile = utilities.Open (ibfFileName);
            //}
        }

        protected override void GenerateFiles(
            IPolishMapFileCreator polishMapFileCreator, 
            ITaskRunner taskRunner)
        {
            //for (
            //    ibfAreaDefinitionIndex = 0;
            //    ibfAreaDefinitionIndex < ibfFile.AreaDirectory.Entries.Count;
            //    ibfAreaDefinitionIndex++)
            //{
            //    IbfAreaDirectoryEntry areaDirectoryEntry = ibfFile.AreaDirectory.Entries[ibfAreaDefinitionIndex];

            //    string mapFileInfo = string.Format(
            //        CultureInfo.InvariantCulture,
            //        "Contours file (bounds = {0})",
            //        areaDirectoryEntry.AreaBounds);

            //    this.GenerateIndividualFile(polishMapFileCreator, taskRunner, mapFileInfo);
            //}
        }

        protected override void GeneratePolishMapFileInternal(CGpsMapperMapWriter mapWriter)
        {
            //IbfUtilities utilities = new IbfUtilities ();

            //IbfAreaDirectoryEntry areaDirectoryEntry = ibfFile.AreaDirectory.Entries[ibfAreaDefinitionIndex];
            //IbfAreaDefinition areaDefinition = utilities.LoadAreaDefinition (ibfFile, ibfAreaDefinitionIndex);

            //foreach (IbfElevationDefinition elevationDefinition in areaDefinition.ElevationDefinitions.Values)
            //{
            //    foreach (IbfIsohypseDefinition isohypseDefinition in elevationDefinition.IsohypseDefinitions)
            //    {
            //        Point2<double>[] isohypsePoints = isohypseDefinition.CalculateIsohypsePoints(
            //            areaDirectoryEntry,
            //            areaDefinition);

            //        // skip contours which are not really useful (all points are the same)
            //        bool allPointsTheSame = CheckIsohypseHasAllSamePoints (isohypseDefinition, isohypsePoints);

            //        if (allPointsTheSame)
            //            continue;

            //        RenderContourIfMatchedByRule(elevationDefinition, isohypsePoints, mapWriter);
            //    }
            //}

            //RenderTileBox (areaDirectoryEntry, mapWriter);
        }

        //private static bool CheckIsohypseHasAllSamePoints(
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

        private void LoadRulesIfNecessary()
        {
            if (Settings.ContoursRenderingRules == null)
            {
                consoleLogger.WriteLine (log, Level.Info,
                    "Fetching contours rendering rules from '{0}'...",
                    Settings.ContoursRenderingRulesSource);

                using (Stream stream = WikiPageStreamProvider.Open (Settings.ContoursRenderingRulesSource))
                {
                    using (ContoursRulesParser parser = new ContoursRulesParser())
                    {
                        Settings.ContoursRenderingRules = parser.Parse(stream);
                    }
                }
            }
        }

        //private void RenderContourIfMatchedByRule(
        //    IbfElevationDefinition elevationDefinition, 
        //    Point2<double>[] isohypsePoints, 
        //    CGpsMapperMapWriter mapWriter)
        //{
        //    ContoursElevationRule rule = Settings.ContoursRenderingRules.FindMatchingRule(elevationDefinition.Elevation);

        //    if (rule != null)
        //    {
        //        rule.RenderContour(isohypsePoints, elevationDefinition.Elevation, Analysis, mapWriter);
        //        MarkMapAsNotEmpty();
        //    }
        //}

        //private void RenderTileBox (IbfAreaDirectoryEntry areaDirectoryEntry, CGpsMapperMapWriter mapWriter)
        //{
        //    if (false == this.IsEmptyMap)
        //    {
        //        ContoursElevationRule rule = Settings.ContoursRenderingRules.FindMatchingRule (0);

        //        List<Point2<double>> boxPoints = new List<Point2<double>> ();
        //        Bounds2 areaBounds = areaDirectoryEntry.AreaBounds;

        //        boxPoints.Add(new Point2<double>(areaBounds.MinX, areaBounds.MinY));
        //        boxPoints.Add(new Point2<double>(areaBounds.MinX, areaBounds.MaxY));
        //        boxPoints.Add(new Point2<double>(areaBounds.MaxX, areaBounds.MaxY));
        //        boxPoints.Add(new Point2<double>(areaBounds.MaxX, areaBounds.MinY));
        //        boxPoints.Add(new Point2<double>(areaBounds.MinX, areaBounds.MinY));

        //        mapWriter.AddSection("POLYLINE")
        //            .AddTypeReference(tileBoxRegistration)
        //            .AddCoordinates(
        //                "Data",
        //                Analysis.HardwareToLogicalLevelDictionary[tileBoxRegistration.MaxLevel],
        //                boxPoints)
        //            .Add ("EndLevel", Analysis.HardwareToLogicalLevelDictionary[tileBoxRegistration.MinLevel]);
        //    }
        //}

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        //private IbfFile ibfFile;
        private readonly string ibfFileName;
        //private int ibfAreaDefinitionIndex;
        private static readonly ILog log = LogManager.GetLogger (typeof (ContoursDataSource));
        private LineTypeRegistration tileBoxRegistration;
    }
}