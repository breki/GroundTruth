using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Brejc.Geometry;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine
{
    public interface IMapDataSource
    {
        int DefaultSimplifyLevel { get; }
        int DefaultTreeSize { get; }

        void AnalyzeData (MapMakerSettings settings);
        string DetermineMapTransparencyMode();

        void GeneratePolishMapFiles(
            ITaskRunner taskRunner, 
            IPolishMapFileCreator polishMapFileCreator);

        void ReleaseData();

        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        void Split (
            MapMakerSettings mapMakerSettings, 
            IList<IMapDataSource> mapDataSourcesAfterSplitting,
            IProgramRunner programRunner
            );
    }
}