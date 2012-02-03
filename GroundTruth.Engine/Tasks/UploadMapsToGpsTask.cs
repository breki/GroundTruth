using System.Globalization;
using System.IO;
using System.Reflection;

namespace GroundTruth.Engine.Tasks
{
    public class UploadMapsToGpsTask : ITask
    {
        public bool SkipOnNonWindows
        {
            get { return false; }
        }

        public string TaskDescription
        {
            get { return string.Format(CultureInfo.InvariantCulture, "Upload map files to the GPS unit"); }
        }

        public void Execute(ITaskRunner taskRunner)
        {
            // first send a dummy map, so that we avoid the "memory effect"
            string dummyMapPath = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "DummyMap.img"));

            taskRunner.RunSendMap (".", "\"{0}\"", dummyMapPath);

            string listFileName = taskRunner.GetProductFile(GenerateMapListFileTask.SendMapListFileType).ProductFileName;

            // now send the real map
            taskRunner.RunSendMap(
                Path.GetDirectoryName(listFileName),
                "-f \"{0}\"",
                Path.GetFileName(listFileName));
        }
    }
}