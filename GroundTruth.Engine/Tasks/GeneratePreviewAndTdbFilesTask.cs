using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using log4net;
using log4net.Core;

namespace GroundTruth.Engine.Tasks
{
    public class GeneratePreviewAndTdbFilesTask : ITask
    {
        public bool SkipOnNonWindows
        {
            get { return true; }
        }

        public string TaskDescription
        {
            get { return string.Format (CultureInfo.InvariantCulture, "Generate preview map and TDB files"); }
        }

        public void Execute (ITaskRunner taskRunner)
        {
            ProductFile file = taskRunner.GetProductFile(GeneratePreviewPolishFileTask.PolishPreviewFile);

            if (file == null)
            {
                consoleLogger.WriteLine (log, Level.Warn, "WARNING: there is no polish preview file to process, preview file will not be generated");
                return;
            }

            taskRunner.RunCPreview(
                "\"{0}\"",
                file.ProductFileName);

            string previewIntermediateFile = Path.GetFullPath(
                Path.Combine(
                  taskRunner.MapMakerSettings.TempDir,
                  String.Format(
                      CultureInfo.InvariantCulture,
                      "{0}.mp",
                      taskRunner.MapMakerSettings.ProductCode)));

            taskRunner.RunCGpsMapper(
                "\"{0}\"", 
                previewIntermediateFile);

            taskRunner.RegisterProductFile(new ProductFile(PreviewImgFile, Path.ChangeExtension(previewIntermediateFile, ".img"), false));
            taskRunner.RegisterProductFile (new ProductFile (TdbFile, Path.ChangeExtension (previewIntermediateFile, ".tdb"), false));
        }

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        private static readonly ILog log = LogManager.GetLogger (typeof (GeneratePreviewAndTdbFilesTask));
        [SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Img")]
        public const string PreviewImgFile = "PreviewImgFile";
        public const string TdbFile = "TdbFile";
    }
}