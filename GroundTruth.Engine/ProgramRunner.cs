using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using log4net;
using log4net.Core;

namespace GroundTruth.Engine
{
    public class ProgramRunner : IProgramRunner
    {
        public ProgramRunner(int externalCommandTimeoutInMinutes)
        {
            this.externalCommandTimeoutInMinutes = externalCommandTimeoutInMinutes;
        }

        public void RunExternalProgram(
            string programExePath, 
            string workingDirectory, 
            string commandLineFormat, 
            params object[] args)
        {
            if (false == File.Exists (programExePath))
                throw new ArgumentException (
                    String.Format (
                        CultureInfo.InvariantCulture,
                        "Program '{0}' could not be found.", programExePath));

            string commandLineArgs = String.Format (CultureInfo.InvariantCulture, commandLineFormat, args);
            ProcessStartInfo processStartInfo = new ProcessStartInfo (programExePath, commandLineArgs);

            consoleLogger.WriteLine (
                log, 
                Level.Debug, 
                "Running program {0} ('{1}') for the maximum duration of {2} minutes", 
                programExePath, 
                commandLineArgs,
                externalCommandTimeoutInMinutes);

            processStartInfo.CreateNoWindow = true;
            string fullPath = Path.GetFullPath (workingDirectory);
            consoleLogger.WriteLine (log, Level.Debug, "Setting working directory to '{0}'", fullPath);
            processStartInfo.WorkingDirectory = fullPath;

            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;

            using (Process process = new Process ())
            {
                process.StartInfo = processStartInfo;
                process.ErrorDataReceived += new DataReceivedEventHandler (process_ErrorDataReceived);
                process.OutputDataReceived += new DataReceivedEventHandler (process_OutputDataReceived);
                process.Start ();

                process.BeginOutputReadLine ();
                process.BeginErrorReadLine ();

                if (false == process.WaitForExit ((int)TimeSpan.FromMinutes (externalCommandTimeoutInMinutes).TotalMilliseconds))
                {
                    string message = string.Format(
                        CultureInfo.InvariantCulture,
                        "The program '{0}' did not finish in time ({1} minutes), aborting.",
                        programExePath,
                        externalCommandTimeoutInMinutes);

                    throw new ArgumentException(message);
                }

                if (process.ExitCode != 0)
                    throw new ArgumentException ("Map making failed.");

                process.Close();
            }
        }

        private void process_ErrorDataReceived (object sender, DataReceivedEventArgs e)
        {
            consoleLogger.WriteLine (log, Level.Debug, "[exec] {0}", e.Data);
        }

        private void process_OutputDataReceived (object sender, DataReceivedEventArgs e)
        {
            consoleLogger.WriteLine (log, Level.Debug, "[exec] {0}", e.Data);
        }

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        private static readonly ILog log = LogManager.GetLogger (typeof (ProgramRunner));
        private int externalCommandTimeoutInMinutes = 60;
    }
}