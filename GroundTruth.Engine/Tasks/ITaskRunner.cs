using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GroundTruth.Engine.Tasks
{
    public interface ITaskRunner
    {
        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MapMaker")]
        MapMakerSettings MapMakerSettings { get; }

        void AddTask(ITask task);
        ProductFile GetProductFile (string productFileType);
        IList<ProductFile> ListAllFinalProductFiles();
        IList<ProductFile> ListProductFiles (string productFileType);
        void RegisterProductFile (ProductFile productFile);
        void Run();
        void RunCGpsMapper(string commandLineFormat, params object[] args);
        void RunCPreview(string commandLineFormat, params object[] args);
        void RunSendMap(string workingDirectory, string commandLineFormat, params object[] args);
    }
}