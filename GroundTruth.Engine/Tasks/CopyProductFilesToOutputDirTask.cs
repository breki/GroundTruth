using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GroundTruth.Engine.Tasks
{
    public class CopyProductFilesToOutputDirTask : ITask
    {
        public bool SkipOnNonWindows
        {
            get { return false; }
        }

        public string TaskDescription
        {
            get { return "Copy product files to the output directory"; }
        }

        public void Execute(ITaskRunner taskRunner)
        {
            foreach (ProductFile productFile in taskRunner.ListAllFinalProductFiles())
            {
                string destinationPath = Path.Combine(
                    taskRunner.MapMakerSettings.OutputPath,
                    Path.GetFileName(productFile.ProductFileName));
                File.Copy(productFile.ProductFileName, destinationPath, true);
                productFile.ProductFileName = destinationPath;
            }
        }
    }
}
