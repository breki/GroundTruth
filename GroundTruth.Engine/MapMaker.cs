using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using GroundTruth.Engine.Tasks;
using log4net;
using log4net.Core;
using Wintellect.PowerCollections;

namespace GroundTruth.Engine
{
    [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MapMaker")]
    public class MapMaker : ITaskRunner
    {
        public MapMaker()
        {
        }

        public MapMakerSettings MapMakerSettings
        {
            get { return mapMakerSettings; }
            set { mapMakerSettings = value; }
        }

        public void AddTask(ITask task)
        {
            tasks.Add(task);
        }

        public ProductFile GetProductFile(string productFileType)
        {
            if (false == productFiles.ContainsKey(productFileType))
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "ERROR: there is no product file of type '{0}'", 
                    productFileType);

                throw new ArgumentException (message);                
            }

            ICollection<ProductFile> files = productFiles[productFileType];
            if (files.Count > 1)
                throw new InvalidOperationException("More than one product file.");

            foreach (ProductFile file in files)
                return file;

            throw new KeyNotFoundException ();
        }

        public IList<ProductFile> ListAllFinalProductFiles()
        {
            List<ProductFile> finalFiles = new List<ProductFile>();

            foreach (KeyValuePair<string, ICollection<ProductFile>> pair in productFiles)
            {
                foreach (ProductFile productFile in pair.Value)
                {
                    if (false == productFile.IsTemporary)
                        finalFiles.Add(productFile);
                }
            }

            return finalFiles;
        }

        public IList<ProductFile> ListProductFiles(string productFileType)
        {
            if (false == productFiles.ContainsKey (productFileType))
            {
                string message = string.Format (
                    CultureInfo.InvariantCulture,
                    "ERROR: there are no product files of type '{0}'",
                    productFileType);

                throw new ArgumentException(message);
            }

            // return a copy of the list, since we may want to modify the original colection while this list
            // is still used
            return new List<ProductFile>(productFiles[productFileType]).AsReadOnly();
        }

        public void PrepareTempDirectory()
        {
            if (Directory.Exists(mapMakerSettings.TempDir))
                Directory.Delete (mapMakerSettings.TempDir, true);

            Directory.CreateDirectory (mapMakerSettings.TempDir);
        }

        public void RegisterProductFile(ProductFile productFile)
        {
            if (false == File.Exists(productFile.ProductFileName))
                throw new InvalidOperationException(String.Format(
                    CultureInfo.InvariantCulture,
                    "The product file '{0}' does not exist.", 
                    productFile.ProductFileName));

            productFiles.Add(productFile.ProductFileType, productFile);
        }

        public void RunCGpsMapper(string commandLineFormat, params object[] args)
        {
            string cgpsmapperExeName = mapMakerSettings.NonWindowsMode ? "cgpsmapper-static" : "cgpsmapper.exe";

            CreateProgramRunnerIfNecessary ();

            programRunner.RunExternalProgram(
                Path.Combine (mapMakerSettings.CGpsMapperPath, cgpsmapperExeName), 
                mapMakerSettings.TempDir, 
                commandLineFormat, 
                args);
        }

        public void RunCPreview(string commandLineFormat, params object[] args)
        {
            CreateProgramRunnerIfNecessary ();

            programRunner.RunExternalProgram (
                Path.Combine (
                    mapMakerSettings.CGpsMapperPath, 
                    "cpreview.exe"),
                mapMakerSettings.TempDir,
                commandLineFormat,
                args);
        }

        public void RunSendMap(string workingDirectory, string commandLineFormat, params object[] args)
        {
            CreateProgramRunnerIfNecessary();
            programRunner.RunExternalProgram (mapMakerSettings.SendMapExePath, workingDirectory, commandLineFormat, args);
        }

        public void Run()
        {
            mapMakerSettings.CheckParameters();

            PrepareTempDirectory ();

            foreach (ITask task in tasks)
            {
                if (false == mapMakerSettings.NonWindowsMode || false == task.SkipOnNonWindows)
                {
                    consoleLogger.WriteLine(log, Level.Info, "TASK: {0}", task.TaskDescription);
                    task.Execute(this);
                }
            }
        }

        private void CreateProgramRunnerIfNecessary()
        {
            if (programRunner == null)
                programRunner = new ProgramRunner(mapMakerSettings.ExternalCommandTimeoutInMinutes);
        }

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        private static readonly ILog log = LogManager.GetLogger (typeof (MapMaker));
        private MapMakerSettings mapMakerSettings = new MapMakerSettings();
        private readonly MultiDictionary<string, ProductFile> productFiles = new MultiDictionary<string, ProductFile> (true);
        private IProgramRunner programRunner;
        private readonly IList<ITask> tasks = new List<ITask>();
    }
}
