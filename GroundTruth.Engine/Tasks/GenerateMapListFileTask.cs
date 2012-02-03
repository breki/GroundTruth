using System;
using System.Globalization;
using System.IO;

namespace GroundTruth.Engine.Tasks
{
    /// <summary>
    /// Generates the map list file to be used by the SendMap tool for uploading maps to Garmin units.
    /// </summary>
    public class GenerateMapListFileTask : ITask
    {
        public bool SkipOnNonWindows
        {
            get { return false; }
        }

        public string TaskDescription
        {
            get { return string.Format (CultureInfo.InvariantCulture, "Generate map list file for SendMap"); }
        }

        public void Execute (ITaskRunner taskRunner)
        {
            string sendMapMapListFileName = Path.GetFullPath (
                Path.Combine (
                    taskRunner.MapMakerSettings.TempDir,
                    String.Format (
                        CultureInfo.InvariantCulture,
                        "filelist_{0}.txt",
                        taskRunner.MapMakerSettings.ProductCode)));

            using (FileStream stream = File.Open (sendMapMapListFileName, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter (stream))
                {
                    writer.WriteLine (":region");
                    // first add the TYP file
                    writer.WriteLine (Path.GetFileName(
                        taskRunner.GetProductFile(GenerateTypeFileTask.TypeFile).ProductFileName));

                    foreach (ProductFile file in taskRunner.ListProductFiles(GenerateMapImgFilesTask.MapImgFileName))
                        writer.WriteLine (Path.GetFileName(file.ProductFileName));
                }
            }

            taskRunner.RegisterProductFile (new ProductFile (SendMapListFileType, sendMapMapListFileName, false));
        }

        public const string SendMapListFileType = "SendMapListFile";
    }
}