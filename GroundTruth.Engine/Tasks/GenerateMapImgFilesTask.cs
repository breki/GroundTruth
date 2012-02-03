using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace GroundTruth.Engine.Tasks
{
    [SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Img")]
    public class GenerateMapImgFilesTask : ITask
    {
        public bool SkipOnNonWindows
        {
            get { return false; }
        }

        public string TaskDescription
        {
            get { return string.Format (CultureInfo.InvariantCulture, "Generate map IMG files"); }
        }

        public void Execute (ITaskRunner taskRunner)
        {
            // generate IMG files
            foreach (ProductFile file in taskRunner.ListProductFiles (GenerateMapPolishFilesTask.PolishMapFileType))
            {
                taskRunner.RunCGpsMapper("\"{0}\"", file.ProductFileName);

                string imgFileName = Path.ChangeExtension(file.ProductFileName, ".img");
                taskRunner.RegisterProductFile(new ProductFile(MapImgFileName, imgFileName, false));
            }
        }

        [SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Img")]
        public const string MapImgFileName = "MapImgFileName";
    }
}