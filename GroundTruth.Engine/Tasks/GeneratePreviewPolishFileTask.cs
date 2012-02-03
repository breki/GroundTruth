using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using log4net;
using log4net.Core;

namespace GroundTruth.Engine.Tasks
{
    public class GeneratePreviewPolishFileTask : ITask
    {
        public bool SkipOnNonWindows
        {
            get { return true; }
        }

        public string TaskDescription
        {
            get { return string.Format (CultureInfo.InvariantCulture, "Generate preview polish file"); }
        }

        public void Execute (ITaskRunner taskRunner)
        {
            string polishPreviewFileName = Path.GetFullPath (Path.Combine (
                                                                 taskRunner.MapMakerSettings.TempDir,
                                                                 String.Format (CultureInfo.InvariantCulture, "{0}_pv.txt", taskRunner.MapMakerSettings.ProductCode)));

            using (Stream mapOutputStream = File.Open (polishPreviewFileName, FileMode.Create))
            {
                using (CGpsMapperGeneralFileWriter writer = new CGpsMapperGeneralFileWriter (
                    mapOutputStream, 
                    false,
                    "preview file"))
                {
                    writer
                        .AddSection("MAP")
                        .Add("FileName", taskRunner.MapMakerSettings.ProductCode)
                        .Add("MapsourceName", taskRunner.MapMakerSettings.ProductName)
                        .Add("MapSetName", taskRunner.MapMakerSettings.ProductName)
                        .Add("CDSetName", taskRunner.MapMakerSettings.ProductName)
                        .Add("MapVersion", taskRunner.MapMakerSettings.MapVersion)
                        .Add("ProductCode", taskRunner.MapMakerSettings.ProductCode)
                        .Add("CODEPAGE", taskRunner.MapMakerSettings.AdditionalMapParameters["CodePage"])
                        .Add("FID", taskRunner.MapMakerSettings.FamilyCode)
                        .Add("ID", taskRunner.MapMakerSettings.StartingMapId)
                        .Add("Copy1", "This map was from OpenStreetMap data using GroundTruth tool (by Igor Brejc)")
                        .Add("Copy2", "Visit http://igorbrejc.net/groundtruthhome")
                        .Add("Copy3", String.Format(
                                          CultureInfo.InvariantCulture,
                                          "Creation time: {0:D} {0:t}", 
                                          DateTime.Now))
                        .Add ("Preview", "Y");

                    // TODO: what is the lowest level used in detailed maps?
                    writer
                        .Add ("Levels", 2)
                        .Add ("Level0", 11)
                        .Add ("Level1", 10);

                    writer
                        .Add ("Zoom0", 5)
                        .Add ("Zoom1", 6);

                    // add the list of detailed files
                    writer
                        .AddSection ("Files");

                    IList<ProductFile> mapImgFiles = taskRunner.ListProductFiles (GenerateMapImgFilesTask.MapImgFileName);

                    if (mapImgFiles.Count == 0)
                    {
                        consoleLogger.WriteLine (log, Level.Warn, "WARNING: there are no IMG files to process, polish preview file will not be generated");
                        return; 
                    }

                    foreach (ProductFile file in mapImgFiles)
                        writer.Add("img", Path.GetFileName(file.ProductFileName));

                    writer
                        .FinishMap ();
                }
            }

            taskRunner.RegisterProductFile(new ProductFile(PolishPreviewFile, polishPreviewFileName, true));
        }

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        private static readonly ILog log = LogManager.GetLogger (typeof (GenerateMapPolishFilesTask));
        public const string PolishPreviewFile = "PolishPreviewFile";
    }
}