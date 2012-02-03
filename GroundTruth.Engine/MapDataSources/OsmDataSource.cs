using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Brejc.Common.Props;
using Brejc.Geometry;
using Brejc.OsmLibrary;
using Brejc.OsmLibrary.CoastlinesProcessing;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Rules;
using GroundTruth.Engine.Tasks;
using GroundTruth.Engine.Wiki;
using log4net;
using log4net.Core;

namespace GroundTruth.Engine.MapDataSources
{
    public class OsmDataSource : MapDataSourceBase
    {
        public OsmDataSource(
            IOsmDatabaseProvider osmDatabaseProvider,
            ISerializersRegistry serializersRegistry)
        {
            this.osmDatabaseProvider = osmDatabaseProvider;
            this.serializersRegistry = serializersRegistry;
        }

        public InMemoryOsmDatabase OsmDatabase
        {
            get { return osmDatabase; }
        }

        public override int DefaultSimplifyLevel
        {
            get { return 10; }
        }

        public override int DefaultTreeSize
        {
            get { return 1000; }
        }

        public override void ReleaseData()
        {
            if (osmDatabaseProvider.IsReloadable)
            {
                osmDatabase.Clear ();
                osmDatabase = null;
            }
        }

        public override void Split(
            MapMakerSettings mapMakerSettings,
            IList<IMapDataSource> mapDataSourcesAfterSplitting,
            IProgramRunner programRunner)
        {
            throw new NotImplementedException();

            //if (this.osmDatabaseProvider is OsmFileData)
            //{
            //    FetchData();
            //    Bounds2 mapBounds = osmDatabase.CalculateBounds();
            //    ReleaseData();

            //    IOsmosisClient client = OsmosisFacade.SplitOsmFile(
            //        Path.GetFullPath(((OsmFileData) this.osmDatabaseProvider).OsmFileName),
            //        Path.GetFullPath (mapMakerSettings.TempDir),
            //        mapBounds,
            //        mapMakerSettings.SplitFrame.X,
            //        mapMakerSettings.SplitFrame.Y);

            //    client.Verbose = true;
            //    client.ConstructCommandLineArgs();
                
            //    StringBuilder args = new StringBuilder();
            //    foreach (string arg in client.CommandLineArguments)
            //    {
            //        string arg2 = arg;
            //        if (arg2 == "--read-xml")
            //            arg2 = "--read-xml enableDateParsing=false";
            //        args.AppendFormat ("{0} ", arg2);
            //    }

            //    programRunner.RunExternalProgram(
            //        Path.GetFullPath(mapMakerSettings.OsmosisPath),
            //        Path.GetFullPath(mapMakerSettings.TempDir),
            //        args.ToString());

            //    foreach (string outputFileName in client.OutputFileNames)
            //        mapDataSourcesAfterSplitting.Add(new OsmDataSource(new OsmFileData(outputFileName)));
            //}
            //else
            //    mapDataSourcesAfterSplitting.Add(this);   
        }

        protected override void AnalyzeDataInternal()
        {
            LoadRulesIfNecessary ();

            this.Analysis = new MapDataAnalysis();
            RunRenderingRulesEngine(true, null);
        }

        protected override void FetchData()
        {
            if (osmDatabaseProvider.IsReloadable || osmDatabase == null)
                osmDatabase = osmDatabaseProvider.Provide ();
        }

        protected override void GenerateFiles(
            IPolishMapFileCreator polishMapFileCreator, 
            ITaskRunner taskRunner)
        {
            string mapFileInfo = null;

            GenerateIndividualFile(polishMapFileCreator, taskRunner, mapFileInfo);
        }

        protected override void GeneratePolishMapFileInternal(CGpsMapperMapWriter mapWriter)
        {
            RunRenderingRulesEngine(false,  mapWriter);
            MarkMapAsNotEmpty();
        }

        [SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void AddLandAndSeaPolygons (
            CoastlinesProcessed coastlinesProcessed,
            bool analysisMode,
            CGpsMapperMapWriter mapWriter)
        {
            if (coastlinesProcessed.CoastlinesToRender.Count > 0)
            {
                if (analysisMode)
                    Settings.RenderingRules.RegisterSeaAndLandPolygonsTemplatesIfNecessary (Settings);
                else
                {
                    // fill download bounding boxes with sea color
                    foreach (Bounds2 seaBoxes in coastlinesProcessed.BoundsToUse)
                    {
                        Polyline2 boundsPolyline = new Polyline2();
                        boundsPolyline.AddVertex(new PointD2(seaBoxes.MinX, seaBoxes.MinY));
                        boundsPolyline.AddVertex (new PointD2 (seaBoxes.MaxX, seaBoxes.MinY));
                        boundsPolyline.AddVertex (new PointD2 (seaBoxes.MaxX, seaBoxes.MaxY));
                        boundsPolyline.AddVertex (new PointD2 (seaBoxes.MinX, seaBoxes.MaxY));
                        boundsPolyline.AddVertex (new PointD2 (seaBoxes.MinX, seaBoxes.MinY));

                        PointD2List pointList = new PointD2List(boundsPolyline.VerticesCount);
                        foreach (PointD2 vertex in boundsPolyline.Vertices)
                            pointList.AddPoint(vertex);

                        Settings.RenderingRules.SeaPolygonsTemplate.RenderPolygon (
                            Settings,
                            Analysis,
                            new PointD2Array(pointList),
                            mapWriter);
                    }

                    // now add polygons for all of the islands
                    foreach (CoastlineAsPoints coastline in coastlinesProcessed.CoastlinesToRender)
                    {
                        Settings.RenderingRules.LandPolygonsTemplate.RenderPolygon (
                            Settings,
                            Analysis,
                            coastline.CoastlinePoints,
                            mapWriter);
                    }
                }
            }
            else if (Settings.RenderingRules.Options.ForceBackgroundColor)
            {
                if (log.IsDebugEnabled)
                    log.Debug("There are no coastlines to process, but we need to render the map background polygon");

                if (analysisMode)
                    Settings.RenderingRules.RegisterSeaAndLandPolygonsTemplatesIfNecessary (Settings);
                else
                    RenderMapBoundsBackgroundPolygon(mapWriter);
            }
        }

        private void LoadRulesIfNecessary ()
        {
            if (Settings.RenderingRules == null)
            {
                consoleLogger.WriteLine (log, Level.Info,
                    "Fetching rendering rules from '{0}'...",
                    Settings.RenderingRulesSource);

                using (Stream stream = WikiPageStreamProvider.Open (Settings.RenderingRulesSource))
                {
                    using (WikiRulesParser parser = new WikiRulesParser (
                        stream, 
                        Settings.TypesRegistry,
                        Settings.CharactersConversionDictionary,
                        serializersRegistry))
                    {
                        parser.Parse();
                        Settings.RenderingRules = parser.Rules;
                    }
                }
            }
        }

        private void RenderMapBoundsBackgroundPolygon(CGpsMapperMapWriter mapWriter)
        {
            Polyline2 boundsPolyline = new Polyline2 ();
            Bounds2 mapBounds = osmDatabase.CalculateBounds();

            boundsPolyline.AddVertex (new PointD2 (mapBounds.MinX, mapBounds.MinY));
            boundsPolyline.AddVertex (new PointD2 (mapBounds.MaxX, mapBounds.MinY));
            boundsPolyline.AddVertex (new PointD2 (mapBounds.MaxX, mapBounds.MaxY));
            boundsPolyline.AddVertex (new PointD2 (mapBounds.MinX, mapBounds.MaxY));
            boundsPolyline.AddVertex (new PointD2 (mapBounds.MinX, mapBounds.MinY));

            PointD2List pointList = new PointD2List (boundsPolyline.VerticesCount);
            foreach (PointD2 vertex in boundsPolyline.Vertices)
                pointList.AddPoint (vertex);

            Settings.RenderingRules.LandPolygonsTemplate.RenderPolygon (
                Settings,
                Analysis,
                pointList,
                mapWriter);
        }

        [SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void RunRenderingRulesEngine (bool inAnalysisMode, CGpsMapperMapWriter mapWriter)
        {
            try
            {
                CoastlineProcessor coastlineProcessor = new CoastlineProcessor (this.OsmDatabase);
                if (false == Settings.SkipCoastlineProcessing)
                    coastlineProcessor.Process();

                AddLandAndSeaPolygons(
                    coastlineProcessor.CoastlinesProcessed,
                    inAnalysisMode,
                    mapWriter);
            }
            catch (Exception ex)
            {
                log.Warn("Something was wrong with coastline processing", ex);
                // do nothing
            }

            RenderingRuleEngine renderingRuleEngine = new RenderingRuleEngine (Analysis);

            renderingRuleEngine.GenerateMapContent (
                this,
                mapWriter,
                Settings,
                inAnalysisMode);
        }

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        private static readonly ILog log = LogManager.GetLogger (typeof (OsmDataSource));
        private InMemoryOsmDatabase osmDatabase;
        private IOsmDatabaseProvider osmDatabaseProvider;
        private readonly ISerializersRegistry serializersRegistry;
    }
}