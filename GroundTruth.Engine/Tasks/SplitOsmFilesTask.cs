using System.Collections.Generic;
using System.Globalization;

namespace GroundTruth.Engine.Tasks
{
    public class SplitOsmFilesTask : ITask
    {
        public IProgramRunner ProgramRunner
        {
            get { return programRunner; }
            set { programRunner = value; }
        }

        public bool SkipOnNonWindows
        {
            get { return false; }
        }

        public string TaskDescription
        {
            get { return string.Format (CultureInfo.InvariantCulture, "Split OSM file"); }
        }

        public void Execute(ITaskRunner taskRunner)
        {
            if (taskRunner.MapMakerSettings.SplitFrame.X > 0)
            {
                List<IMapDataSource> mapDataSourcesAfterSplitting = new List<IMapDataSource>();
                foreach (IMapDataSource mapDataSource in taskRunner.MapMakerSettings.MapDataSources)
                    mapDataSource.Split(
                        taskRunner.MapMakerSettings, 
                        mapDataSourcesAfterSplitting,
                        programRunner);

                taskRunner.MapMakerSettings.SetMapDataSources(mapDataSourcesAfterSplitting);
            }
        }

        private IProgramRunner programRunner;
    }
}