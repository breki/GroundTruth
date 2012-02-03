using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace GroundTruth.Engine.Tasks
{
    public class GenerateMapSourceRegFilesTask : ITask
    {
        public bool SkipOnNonWindows
        {
            get { return true; }
        }

        public string TaskDescription
        {
            get { return "Generate MapSource registry files"; }
        }

        public void Execute(ITaskRunner taskRunner)
        {
            GenerateRegistryFile(taskRunner, true);
            GenerateRegistryFile(taskRunner, false);
        }

        [SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Regedit")]
        public static string MakeRegeditFriendlyPath (string path)
        {
            return path.Replace(@"\", @"\\");
        }

        private static void GenerateRegistryFile(ITaskRunner taskRunner, bool addFile)
        {
            string regFileName = Path.Combine(
                taskRunner.MapMakerSettings.OutputPath,
                String.Format(
                    CultureInfo.InvariantCulture,
                    addFile ? "{0}_add.reg" : "{0}_delete.reg",
                    taskRunner.MapMakerSettings.ProductCode));

            using (StreamWriter writer = new StreamWriter(regFileName))
            {
                writer.WriteLine ("REGEDIT4");
                writer.WriteLine ();
                writer.WriteLine (
                    @"[{0}HKEY_LOCAL_MACHINE\SOFTWARE\Garmin\MapSource\Families\{1}]", 
                    addFile ? "" : "-",
                    taskRunner.MapMakerSettings.FamilyName);

                if (addFile)
                {
                    writer.WriteLine (
                        @"""ID""=hex:{0:x},{1:x}",
                        taskRunner.MapMakerSettings.FamilyCode & 0xff,
                        (taskRunner.MapMakerSettings.FamilyCode >> 8) & 0xff);
                    writer.WriteLine (@"""TYP""=""{0}""",
                                      MakeRegeditFriendlyPath (Path.GetFullPath (
                                                                   taskRunner.GetProductFile (GenerateTypeFileTask.TypeFile).
                                                                       ProductFileName)));
                    writer.WriteLine ();

                    writer.WriteLine (
                        @"[HKEY_LOCAL_MACHINE\SOFTWARE\Garmin\MapSource\Families\{0}\{1}]",
                        taskRunner.MapMakerSettings.FamilyName,
                        taskRunner.MapMakerSettings.ProductCode);
                    writer.WriteLine (@"""LOC""=""{0}""",
                                     MakeRegeditFriendlyPath (Path.GetFullPath (taskRunner.MapMakerSettings.OutputPath)));
                    writer.WriteLine (@"""BMAP""=""{0}""",
                                     MakeRegeditFriendlyPath (Path.GetFullPath (
                                                                  taskRunner.GetProductFile (GeneratePreviewAndTdbFilesTask.PreviewImgFile).
                                                                      ProductFileName)));
                    writer.WriteLine (@"""TDB""=""{0}""",
                                     MakeRegeditFriendlyPath (Path.GetFullPath (
                                                                  taskRunner.GetProductFile (GeneratePreviewAndTdbFilesTask.TdbFile).ProductFileName)));
                }
            }

            taskRunner.RegisterProductFile(
                new ProductFile(
                    addFile ? MapSourceAddRegFile : MapSourceDeleteRegFile, 
                    regFileName, 
                    false));
        }

        public const string MapSourceAddRegFile = "MapSourceAddRegFile";
        public const string MapSourceDeleteRegFile = "MapSourceDeleteRegFile";
    }
}
