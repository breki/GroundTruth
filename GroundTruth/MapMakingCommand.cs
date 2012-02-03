using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Brejc.Common.FileSystem;
using Brejc.Common.Props;
using GroundTruth.Engine;
using GroundTruth.Engine.MapDataSources;
using GroundTruth.Engine.Tasks;
using log4net;
using log4net.Core;
using NDesk.Options;

namespace GroundTruth
{
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MapMaking")]
    public class MapMakingCommand : IConsoleCommand
    {
        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        public MapMakingCommand (
            ITaskRunner mapMaker, 
            ISerializersRegistry serializersRegistry,
            IFileSystem fileSystem)
        {
            this.mapMaker = mapMaker;
            this.serializersRegistry = serializersRegistry;
            this.fileSystem = fileSystem;

            MapMakerSettings mapMakerSettings = mapMaker.MapMakerSettings;

            options = new OptionSet() {
            { "osm|osmfile=", "a path to the {OSM file} to generate the map from. Can be used multiple times to specify multiple OSM files."
                + @" If this option is not specified, output.osm file will be used.",
              (string osmFile) => mapMakerSettings.MapDataSources.Add(new OsmDataSource(new OsmFileData(osmFile, fileSystem), serializersRegistry))
                },
            { "ibf|ibffile=", "a path to the {IBF contours file} to generate the map from. Can be used multiple times to specify multiple IBF files.",
              (string ibfFileName) => mapMakerSettings.MapDataSources.Add(new ContoursDataSource(ibfFileName))
                },
            { "op|outputpath=", "the {directory path} where all output files (IMG, TYP...) will be stored (default is the Maps directory)",
              (string v) => mapMakerSettings.OutputPath = v },
            { "r|rules=", "the {rendering rules} to use (either a local file path or an OSM wiki rules URL)."
                +@" If this option is not specified, the default rules (Rules/DefaultRules.txt) will be used.",
              (string v) => mapMakerSettings.RenderingRulesSource = v },
            { "cr|contourrules=", "the contours {rendering rules} to use (either a local file path or an OSM wiki rules URL)."
                +@" If this option is not specified, the default rules (Rules/ContoursRules.txt) will be used.",
              (string v) => mapMakerSettings.ContoursRenderingRulesSource = v },
            { "ct|chartable=", "the {source} for character conversion table to use (either a local file path or an OSM wiki rules URL)." 
            + @" If this option is not specified, the local conversion table file (Rules/CharacterConversionTable.txt) will be used.",
              (string v) =>
                  {
                      mapMakerSettings.CharactersConversionTableSource = v;
                  } },
            { "tt|typetable=", "the {source} for standard Garmin types table to use (either a local file path or an OSM wiki rules URL)." 
            + @" If this option is not specified, the local file (Rules/StandardGarminTypes.txt) will be used.",
              (string v) =>
                  {
                      mapMakerSettings.StandardGarminTypesSource = v;
                  } },
            { "mi|mapid=", "starting map {ID} for generated maps. It has to be a 8-digit number.",
              (string v) => mapMakerSettings.StartingMapId = v },
            { "mn|mapname=", "sets the Garmin map {name} prefix used for all IMG files (each file gets an additional suffix ID). ",
              (string v) => mapMakerSettings.MapNamePrefix = v },
            { "fc|familycode=", "Garmin map family {code} (integer, default value is 1)",
              (int v) => mapMakerSettings.FamilyCode = v },
            { "fn|familyname=", "Garmin map family {name}",
              (string v) => mapMakerSettings.FamilyName = v },
            { "pc|productcode=", "Garmin map product {code} (integer)",
              (int v) => mapMakerSettings.ProductCode = v },
            { "pn|productname=", "Garmin map product {name}",
              (string v) => mapMakerSettings.ProductName = v },
            { "nocgp", "specifies that only polish (.MP) files will be generated. cgpsmapper will not be used in this case.",
              v => { if (v != null) mapMakerSettings.NoCgpsmapper = true; }
                },
            { "nosea", "specifies that no coastline processing will be done (i.e. the sea areas will not be rendered as polygons). Use this option if you have problems with sea 'flooding'.",
              v => { if (v != null) mapMakerSettings.SkipCoastlineProcessing = true; }
                },
            { "cgp=", "cGpsMapper toolset {path} (just the directory; the default path is the current directory)",
              (string v) => mapMakerSettings.CGpsMapperPath = v },
            { "u|upload", "upload maps to the GPS unit using SendMap",
              v => { if (v != null) mapMakerSettings.UploadToGps = true; }
                },
            { "sendmapexe=", "SendMap.exe {path} (the default path is the current directory)",
              (string v) => mapMakerSettings.SendMapExePath = v },
            { "param=", "specifies additional map {0:parameter} and {1:value} (see cGpsMapper manual for more info)",
              (string k, string v) => mapMakerSettings.AdditionalMapParameters.Add(k, v)},
#if DEBUG
              { "testmap", "generates a test map",
              v =>
                  {
                      if (v != null)
                      {
                          mapMakerSettings.MapDataSources.Add(new OsmDataSource(new TestOsmDataProvider(), serializersRegistry));
                          mapMakerSettings.RenderingRules = TestOsmDataProvider.CreateTestRenderingRules();
                  }
                }
            },
#endif
            { "ele|elevation=", "specifies elevation {unit} to use in generated maps (m for meters, f for feet; default is meters)",
              (string elevationUnits) =>
            {
                if (false == elevationUnits.StartsWith("m", StringComparison.OrdinalIgnoreCase)
                    && false == elevationUnits.StartsWith("f", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Invalid elevation units.");

                mapMakerSettings.ElevationUnits = elevationUnits.ToLowerInvariant()[0];
            }},
            { "transparent=", "specifies a {map transparency mode} (Y=full, S=semi, N=not transp.). Default mode is N. Contour maps are always semi-transparent, regardless of this setting.",
              (string transparencyMode) => mapMakerSettings.MapTransparencyMode = transparencyMode
            },
            { "tresize=", "specifies cgpsmapper's TreSize {integer value}. Default value depends on the type of the map.",
              (int treSize) => mapMakerSettings.TreSize = treSize
            },
            { "simplifylevel=", "specifies cgpsmapper's SimplifyLevel {value}. Default value depends on the type of the map.",
              (float simplifyLevel) => mapMakerSettings.SimplifyLevel = simplifyLevel
            },
            //{ "splitframe=", "specifies the {split frame (lat,lng)} used to split OSM files into smaller chunks. By default no spliting is done.",
            //  (string splitFrame) =>
            //      {
            //          string[] splitFrameValues = splitFrame.Split(',');

            //          if (splitFrameValues.Length != 2)
            //              throw new ArgumentException(
            //                  string.Format(
            //                    CultureInfo.InvariantCulture,
            //                    "Invalid split frame parameters: '{0}'",
            //                    splitFrame));

            //          double splitFrameY = double.Parse(splitFrameValues[0], CultureInfo.InvariantCulture);
            //          double splitFrameX = double.Parse(splitFrameValues[1], CultureInfo.InvariantCulture);

            //          mapMakerSettings.SplitFrame = new Point2<double>(splitFrameY, splitFrameX);
            //      }
            //},
            { "nonwin", "turns on the non-Windows mode (suitable if running on Linux or Mac)",
              v => { if (v != null) mapMakerSettings.NonWindowsMode = true; }
                },
            { "extimeout=", "specifies the {timeout} in minutes for external programs (like cgpsmapper) to execute. The default is 60 minutes, after which the application aborts",
              (int timeout) => mapMakerSettings.ExternalCommandTimeoutInMinutes = timeout
                },
            { "of|optionsfile=", "specifies a {file} to read command line options from",
              (string optionsFile) => optionsFileName = optionsFile
            },
        };
        }

        public string CommandDescription
        {
            get { return "Creates maps for Garmin GPS units"; }
        }

        public string CommandName { get { return "makemap"; } }

        [SuppressMessage ("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public int Execute (IEnumerable<string> args)
        {
            try
            {
                List<string> unhandledArguments = options.Parse(args);

                if (unhandledArguments.Count > 0)
                {
                    consoleLogger.WriteLine (log, Level.Error, "There are some unsupported options:");
                    foreach (string unhandledArgument in unhandledArguments)
                        consoleLogger.WriteLine(log, Level.Error, unhandledArgument);

                    consoleLogger.WriteLine (log, Level.Error, String.Empty);

                    throw new ArgumentException ();
                }

                if (optionsFileName != null)
                {
                    // read options from a file
                    // each line represents a single option

                    string[] optionsFromFile = File.ReadAllLines(optionsFileName);
                    unhandledArguments = options.Parse(optionsFromFile);
                }

                if (mapMaker.MapMakerSettings.MapDataSources.Count == 0)
                    mapMaker.MapMakerSettings.MapDataSources.Add(
                        new OsmDataSource(new OsmFileData("output.osm", fileSystem), serializersRegistry));

                PrepareTasks();

                mapMaker.Run();

                return 0;
            }
            catch (OptionException ex)
            {
                consoleLogger.WriteLine (log, Level.Error, "ERROR: {0}", ex);
                ShowHelp ();
            }
            catch (ArgumentException ex)
            {
                consoleLogger.WriteLine (log, Level.Error, "ERROR: {0}", ex);
                ShowHelp ();
            }

            return 1;            
        }

        public void ShowHelp()
        {
            options.WriteOptionDescriptions(System.Console.Out);
        }

        private void PrepareTasks()
        {
            MapMakerSettings mapMakerSettings = mapMaker.MapMakerSettings;

            mapMaker.AddTask (new GenerateMapPolishFilesTask (mapMakerSettings, serializersRegistry));
            mapMaker.AddTask (new GenerateMapTypesPolishFileTask ());

            if (false == mapMakerSettings.NoCgpsmapper)
            {
                mapMaker.AddTask(new GenerateTypeFileTask());
                mapMaker.AddTask(new GenerateMapImgFilesTask());
                mapMaker.AddTask(new GeneratePreviewPolishFileTask());
                mapMaker.AddTask(new GeneratePreviewAndTdbFilesTask());
                mapMaker.AddTask(new GenerateMapListFileTask());

                if (mapMakerSettings.UploadToGps)
                    mapMaker.AddTask(new UploadMapsToGpsTask());
            }

            mapMaker.AddTask(new CopyProductFilesToOutputDirTask());

            if (false == mapMakerSettings.NoCgpsmapper)
                mapMaker.AddTask (new GenerateMapSourceRegFilesTask ());
        }

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        private static readonly ILog log = LogManager.GetLogger (typeof (ConsoleApp));
        private ITaskRunner mapMaker;
        private readonly ISerializersRegistry serializersRegistry;
        private readonly IFileSystem fileSystem;
        private OptionSet options;
        private string optionsFileName;
    }
}
