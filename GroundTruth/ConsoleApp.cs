using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GroundTruth.Engine;
using GroundTruth.Engine.Contours;
using GroundTruth.Engine.Contours.Ibf2Osm;
using log4net;
using log4net.Core;
using NDesk.Options;

namespace GroundTruth
{
    public class ConsoleApp
    {
        public ConsoleApp (string[] args, ICollection<IConsoleCommand> commands)
        {
            this.args = args;

            if (commands.Count != 4)
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Something is wrong with the initialization code - {0} console commands are missing.",
                        4 - commands.Count));

            foreach (IConsoleCommand command in commands)
                AddCommand(command);
        }

        [SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public int Process ()
        {
            ShowBanner();

            try
            {
                if (args.Length == 0)
                {
                    ShowHelp();
                    return 0;
                }

                string commandName = args[0];

                if (commands.ContainsKey(commandName))
                {
                    IConsoleCommand command = commands[commandName];

                    List<string> remainingArgs = new List<string>(args);
                    remainingArgs.RemoveAt(0);

                    return command.Execute(remainingArgs);
                }
                else
                {
                    throw new ArgumentException(
                        String.Format(
                            CultureInfo.InvariantCulture,
                            "Unknown command: '{0}'",
                            commandName));
                }
            }
            catch (OptionException ex)
            {
                consoleLogger.WriteLine (log, Level.Error, "ERROR: {0}", ex);
                ShowHelp();
            }
            catch (ArgumentException ex)
            {
                consoleLogger.WriteLine (log, Level.Error, "ERROR: {0}", ex);
                ShowHelp ();
            }
            catch (Exception ex)
            {
                consoleLogger.WriteLine (log, Level.Fatal, "ERROR: {0}", ex);                
            }

            return 1;
        }

        private void AddCommand (IConsoleCommand command)
        {
            commands.Add(command.CommandName, command);
        }

        private void ShowBanner()
        {
            consoleLogger.WriteLine(log, Level.Info);
            System.Diagnostics.FileVersionInfo version = System.Diagnostics.FileVersionInfo.GetVersionInfo
                (System.Reflection.Assembly.GetExecutingAssembly ().Location);
            consoleLogger.WriteLine (log, Level.Info, "GroundTruth v{0} by Igor Brejc", version.FileVersion);
            consoleLogger.WriteLine (log, Level.Info, "Generates Garmin maps from OpenStreetMap data");
            consoleLogger.WriteLine (log, Level.Info, "Licensed under GPL v3 license");
            consoleLogger.WriteLine (log, Level.Info, "Visit http://wiki.openstreetmap.org/wiki/GroundTruth for more info");
            consoleLogger.WriteLine(log, Level.Info);
        }

        private void ShowHelp()
        {
            consoleLogger.WriteLine (log, Level.Info, "USAGE: GroundTruth <command> <options>");
            consoleLogger.WriteLine (log, Level.Info, "-----------------------");
            consoleLogger.WriteLine (log, Level.Info, "LIST OF COMMANDS:");
            consoleLogger.WriteLine(log, Level.Info);

            foreach (IConsoleCommand command in commands.Values)
            {
                consoleLogger.WriteLine (log, Level.Info, "Command '{0}': {1}", command.CommandName, command.CommandDescription);
                consoleLogger.WriteLine(log, Level.Info);
                consoleLogger.WriteLine (log, Level.Info, "Options for '{0}':", command.CommandName);
                consoleLogger.WriteLine(log, Level.Info);
                command.ShowHelp();
                consoleLogger.WriteLine (log, Level.Info, "-----------------------");
            }
        }

        private readonly string[] args;
        private SortedDictionary<string, IConsoleCommand> commands = new SortedDictionary<string, IConsoleCommand>();
        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        private static readonly ILog log = LogManager.GetLogger (typeof (ConsoleApp));
    }
}
