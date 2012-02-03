using System.Collections.Generic;
using System.Text;
using Brejc.Geometry;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Tasks;
using log4net;
using log4net.Core;

namespace GroundTruth.Engine.MapDataSources
{
    public abstract class MapDataSourceBase : IMapDataSource
    {
        public abstract int DefaultSimplifyLevel { get; }
        public abstract int DefaultTreeSize { get; }

        public void AnalyzeData (MapMakerSettings settings)
        {
            if (log.IsDebugEnabled)
                log.Debug("AnalyzeData");

            this.settings = settings;

            FetchData();
            AnalyzeDataInternal();
            analysis.Postprocess();

            consoleLogger.WriteLine (log, Level.Info, "Hardware levels used: {0}", analysis.HardwareLevelsUsed.Count);
        }

        public virtual string DetermineMapTransparencyMode()
        {
            return settings.MapTransparencyMode;
        }

        public void GeneratePolishMapFiles (
            ITaskRunner taskRunner,
            IPolishMapFileCreator polishMapFileCreator)
        {
            if (log.IsDebugEnabled)
                log.Debug ("GeneratePolishMapFiles");

            FetchData();
            GenerateFiles(polishMapFileCreator, taskRunner);
            ReleaseData ();
        }

        public abstract void ReleaseData ();
        public abstract void Split(
            MapMakerSettings mapMakerSettings, 
            IList<IMapDataSource> mapDataSourcesAfterSplitting,
            IProgramRunner programRunner);

        public MapDataAnalysis Analysis
        {
            get { return analysis; }
            protected set { analysis = value; }
        }

        protected MapMakerSettings Settings
        {
            get { return settings; }
        }

        protected bool IsEmptyMap
        {
            get { return isEmptyMap; }
        }

        protected abstract void AnalyzeDataInternal ();
        protected abstract void FetchData ();

        protected abstract void GenerateFiles(
            IPolishMapFileCreator polishMapFileCreator, 
            ITaskRunner taskRunner);

        protected void GenerateIndividualFile(
            IPolishMapFileCreator polishMapFileCreator, 
            ITaskRunner taskRunner,
            string mapFileInfo)
        {
            using (CGpsMapperMapWriter mapWriter = new CGpsMapperMapWriter (
                polishMapFileCreator.CreatePolishMapFile (), 
                mapFileInfo))
            {
                consoleLogger.WriteLine (log, Level.Info, "Generating polish map file...");

                WriteBasicMapParameters(polishMapFileCreator, mapWriter);

                // write down used levels
                mapWriter.Add ("Levels", analysis.LogicalToHardwareLevelDictionary.Count);

                int logicalLevel = 0;
                for (int i = analysis.HardwareLevelsUsed.Count - 1; i >= 0; i--)
                    mapWriter.AddFormat ("Level{0}={1}", logicalLevel++, analysis.HardwareLevelsUsed.Keys[i]);

                for (int i = 0; i < analysis.HardwareLevelsUsed.Count; i++)
                    mapWriter.AddFormat ("Zoom{0}={1}", i, i);

                isEmptyMap = true;
                GeneratePolishMapFileInternal (mapWriter);

                mapWriter.FinishMap ();
            }

            if (false == isEmptyMap)
                taskRunner.RegisterProductFile (
                    new ProductFile (
                        GenerateMapPolishFilesTask.PolishMapFileType,
                        polishMapFileCreator.CurrentPolishMapFileName,
                        true));
        }

        protected abstract void GeneratePolishMapFileInternal (
            CGpsMapperMapWriter mapWriter);

        protected void MarkMapAsNotEmpty()
        {
            isEmptyMap = false;
        }

        private void WriteBasicMapParameters(IPolishMapFileCreator polishMapFileCreator, CGpsMapperMapWriter mapWriter)
        {
            mapWriter
                .AddSection("IMG ID")
                .Add("ID", polishMapFileCreator.CurrentMapId)
                .Add("Name", polishMapFileCreator.CurrentMapName)
                .Add("Elevation", settings.ElevationUnits.ToString())
                .Add("Transparent", DetermineMapTransparencyMode());

            float simplifyLevel = DefaultSimplifyLevel;
            if (Settings.SimplifyLevel.HasValue)
                simplifyLevel = Settings.SimplifyLevel.Value;

            mapWriter
                .Add("SimplifyLevel", simplifyLevel);

            int treSize = DefaultTreeSize;
            if (Settings.TreSize.HasValue)
                treSize = Settings.TreSize.Value;

            mapWriter
                .Add ("TreSize", treSize);

            // write all additional parameters to the map
            foreach (
                KeyValuePair<string, string> mapParameter in
                    settings.AdditionalMapParameters)
                mapWriter.Add (mapParameter.Key, mapParameter.Value);
        }

        private MapDataAnalysis analysis;
        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        private bool isEmptyMap;
        static readonly private ILog log = LogManager.GetLogger (typeof (MapDataSourceBase));
        private MapMakerSettings settings;
    }
}