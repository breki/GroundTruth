using System;
using System.Globalization;
using System.IO;
using Brejc.Common.Props;
using GroundTruth.Engine.Rules;
using GroundTruth.Engine.Wiki;
using log4net;
using log4net.Core;

namespace GroundTruth.Engine.Tasks
{
    public class GenerateMapPolishFilesTask : ITask
    {
        public GenerateMapPolishFilesTask(
            MapMakerSettings settings,
            ISerializersRegistry serializersRegistry)
        {
            this.serializersRegistry = serializersRegistry;
            polishMapFileCreator = new DefaultPolishMapFileCreator(settings);
        }

        public IPolishMapFileCreator PolishMapFileCreator
        {
            get { return polishMapFileCreator; }
            set { polishMapFileCreator = value; }
        }

        public bool SkipOnNonWindows
        {
            get { return false; }
        }

        public string TaskDescription
        {
            get { return string.Format (CultureInfo.InvariantCulture, "Generate map polish files"); }
        }

        public void Execute (ITaskRunner taskRunner)
        {
            FetchRulesFromWikiPage(taskRunner, taskRunner.MapMakerSettings.StandardGarminTypesSource, "standard Garmin types dictionary");
            FetchRulesFromWikiPage(taskRunner, taskRunner.MapMakerSettings.CharactersConversionTableSource, "character conversion table");

            // define default values for certain map parameters (if they are not overriden by the user)
            taskRunner.MapMakerSettings.UseDefaultParameterValueIfMissing ("CodePage", "1250");
            taskRunner.MapMakerSettings.UseDefaultParameterValueIfMissing ("LBLcoding", "9");
            taskRunner.MapMakerSettings.UseDefaultParameterValueIfMissing ("PreProcess", "F");
            taskRunner.MapMakerSettings.UseDefaultParameterValueIfMissing ("RgnLimit", "1024");

            consoleLogger.WriteLine (log, Level.Info, "Scanning map data...");

            bool moreThanOneSource = taskRunner.MapMakerSettings.MapDataSources.Count > 0;
            foreach (IMapDataSource mapDataSource in taskRunner.MapMakerSettings.MapDataSources)
            {
                mapDataSource.AnalyzeData(taskRunner.MapMakerSettings);
                if (moreThanOneSource)
                {
                    mapDataSource.ReleaseData();
                    // force garbage collection
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    System.GC.Collect();
                }
            }

            foreach (IMapDataSource mapDataSource in taskRunner.MapMakerSettings.MapDataSources)
                mapDataSource.GeneratePolishMapFiles(
                    taskRunner, 
                    polishMapFileCreator);

            ReportMapContentStatistics(taskRunner);
        }

        public const string PolishMapFileType = "PolishMapFile";

        private void FetchRulesFromWikiPage (
            ITaskRunner taskRunner, 
            string wikiPageSource,
            string whatIsBeingFetched)
        {
            consoleLogger.WriteLine (log, Level.Info, 
                "Fetching {1} from '{0}'...",
                wikiPageSource,
                whatIsBeingFetched);

            using (Stream stream = WikiPageStreamProvider.Open (wikiPageSource))
            {
                using (WikiRulesParser rulesParser = new WikiRulesParser(
                    stream,
                    taskRunner.MapMakerSettings.TypesRegistry,
                    taskRunner.MapMakerSettings.CharactersConversionDictionary,
                    serializersRegistry))
                {
                    rulesParser.Parse();
                }
            }
        }

        private void ReportMapContentStatistics(ITaskRunner taskRunner)
        {
            consoleLogger.WriteLine (log, Level.Info, "---------------------");
            consoleLogger.WriteLine (log, Level.Info, "Polyline types used: {0}",
                           taskRunner.MapMakerSettings.TypesRegistry.LineTypeRegistrations.Count);
            consoleLogger.WriteLine (log, Level.Info, "Polygon types used: {0}",
                           taskRunner.MapMakerSettings.TypesRegistry.AreaTypeRegistrations.Count);
            consoleLogger.WriteLine (log, Level.Info, "Point types used: {0}",
                           taskRunner.MapMakerSettings.TypesRegistry.PointTypeRegistrations.Count);
            consoleLogger.WriteLine (log, Level.Info);

            consoleLogger.WriteLine (log, Level.Info, "Map content statistics (by rendering rules):");
            consoleLogger.WriteLine (log, Level.Info);

            if (taskRunner.MapMakerSettings.RenderingRules != null)
            {
                foreach (RenderingRule renderingRule in taskRunner.MapMakerSettings.RenderingRules.EnumerateRules())
                {
                    int featuresGenerated = 0;

                    if (
                        taskRunner.MapMakerSettings.MapContentStatistics.GeneratedMapFeaturesCount.ContainsKey(
                            renderingRule.RuleName))
                        featuresGenerated =
                            taskRunner.MapMakerSettings.MapContentStatistics.GeneratedMapFeaturesCount[
                                renderingRule.RuleName];

                    consoleLogger.WriteLine(log, Level.Info, "Rule '{0}': {1} features generated",
                                            renderingRule.RuleName, featuresGenerated);
                }
            }

            consoleLogger.WriteLine (log, Level.Info, "---------------------");
        }

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger();
        private static readonly ILog log = LogManager.GetLogger (typeof (GenerateMapPolishFilesTask));
        private IPolishMapFileCreator polishMapFileCreator;
        private readonly ISerializersRegistry serializersRegistry;
    }
}