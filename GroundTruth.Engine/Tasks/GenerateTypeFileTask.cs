using System.Globalization;
using System.IO;

namespace GroundTruth.Engine.Tasks
{
    public class GenerateTypeFileTask : ITask
    {
        public bool SkipOnNonWindows
        {
            get { return false; }
        }

        public string TaskDescription
        {
            get { return string.Format (CultureInfo.InvariantCulture, "Generate TYP file"); }
        }

        public void Execute (ITaskRunner taskRunner)
        {
            ProductFile polishTypeFile = taskRunner.GetProductFile (GenerateMapTypesPolishFileTask.PolishTypeFile);

            string typeFileName = (Path.Combine (
                        Path.GetDirectoryName (polishTypeFile.ProductFileName),
                        Path.GetFileNameWithoutExtension (polishTypeFile.ProductFileName)));

            taskRunner.RunCGpsMapper(
                "typ \"{0}\" \"{1}\"",
                Path.GetFileName(polishTypeFile.ProductFileName),
                Path.GetFileName(typeFileName));

            taskRunner.RegisterProductFile(new ProductFile(TypeFile, typeFileName, false));
        }

        public const string TypeFile = "TypeFile";
    }
}