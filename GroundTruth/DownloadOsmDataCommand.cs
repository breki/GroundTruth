using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Brejc.Common;
using Brejc.Geometry;
using Brejc.OsmLibrary.Helpers;
using Brejc.OsmLibrary.Osmxapi;
using GroundTruth.Engine;
using log4net;
using log4net.Core;
using NDesk.Options;

namespace GroundTruth
{
    public class DownloadOsmDataCommand : IConsoleCommand
    {
        public DownloadOsmDataCommand()
        {
            options = new OptionSet() {
            { "o|outputfile=", "output {OSM file} (the default is output.osm)",
              (string outputFile) => this.outputFile = outputFile},
            { "b|bounds=", "specifies map boundaries as {bounds}: <minlat>,<minlong>,<maxlat>,<maxlong>",
              (string boundsParams) =>
                  {
                      string[] par = boundsParams.Split(',');
                      if (par.Length != 4)
                          throw new ArgumentException(
                              string.Format(
                                CultureInfo.InvariantCulture,
                                "Invalid bounds parameters: '{0}'",
                                boundsParams));

                      double minlat = double.Parse(par[0], CultureInfo.InvariantCulture);
                      double minlng = double.Parse(par[1], CultureInfo.InvariantCulture);
                      double maxlat = double.Parse(par[2], CultureInfo.InvariantCulture);
                      double maxlng = double.Parse(par[3], CultureInfo.InvariantCulture);

                      osmBounds = new OsmBoundsHelper (new Bounds2(minlng, minlat, maxlng, maxlat));
                  }
                },
            { "bu|boundsurl=", "specifies map boundaries by using OSM map {url} (please put the url inside double quotes)",
              (string url) =>
                  {
                      if (log.IsDebugEnabled)
                          log.DebugFormat("boundsurl={0}", url);

                      osmBounds = OsmBoundsHelper.FromSlippyMapUrl(new Uri(url));
                  }
                },
            { "bb|boundsbox=", "specifies map boundaries as a {box} <lat>,<long>,<box size in km's>",
              (string boxParams) =>
                  {
                      string[] par = boxParams.Split(',');
                      if (par.Length != 3)
                          throw new ArgumentException(
                              string.Format(
                                CultureInfo.InvariantCulture,
                                "Invalid box parameters: '{0}'",
                                boxParams));

                      double lat = double.Parse(par[0], CultureInfo.InvariantCulture);
                      double lng = double.Parse(par[1], CultureInfo.InvariantCulture);
                      double size = double.Parse(par[2], CultureInfo.InvariantCulture);

                      osmBounds = OsmBoundsHelper.FromBox(lat, lng, size);
                  }
                },
            { "xurl|osmxapiurl=", string.Format(
                CultureInfo.InvariantCulture,
                "specifies the {{url}} of the OSMXAPI service (the default is {0})",
                OsmxapiClient.DefaultOsmxapiUrl),
              (string url) =>
                  {
                      if (log.IsDebugEnabled)
                          log.DebugFormat("boundsurl={0}", url);

                      osmxapiUrl = new Uri(url);
                  }
                },
            { "nonwin", "does nothing, it's just here for scripting purposes",
              v => { ; }
                },
            };
        }

        public string CommandDescription
        {
            get { return "Downloads map data from the OSM server"; }
        }

        public string CommandName
        {
            get { return "getdata"; }
        }

        public int Execute(IEnumerable<string> args)
        {
            List<string> unhandledArguments = options.Parse(args);

            if (unhandledArguments.Count > 0)
                throw new ArgumentException("There are some unsupported options.");

            if (osmBounds == null)
                throw new ArgumentException("Map bounds were not specified");

            OsmxapiClient osmxapiClient = new OsmxapiClient();
            if (osmxapiUrl != null)
                osmxapiClient.OsmxapiUrl = osmxapiUrl;

            osmxapiClient.DownloadProgressCallback =
                (totalBytesDownloaded) =>
                consoleLogger.Write(log, Level.Info, "\rRead {0} so far...          ",
                     FormattingUtils.FormatByteSizeToString(
                         totalBytesDownloaded,
                         System.Globalization.CultureInfo.
                             InvariantCulture));

            osmxapiClient.DownloadBounds = osmBounds.Bounds;
            using (Stream fileStream = File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                osmxapiClient.DownloadData(fileStream);

            consoleLogger.WriteLine(log, Level.Info, 
                "\rFinished downloading data ({0})       ",
                FormattingUtils.FormatByteSizeToString(osmxapiClient.TotalBytesDownloaded, System.Globalization.CultureInfo.InvariantCulture));
            consoleLogger.Write (log, Level.Info, "Saved to file {0}", outputFile);

            return 0;
        }

        public void ShowHelp()
        {
            options.WriteOptionDescriptions(System.Console.Out);
        }

        private IConsoleLogger consoleLogger = new DefaultConsoleLogger ();
        private static readonly ILog log = LogManager.GetLogger (typeof (ConsoleApp));
        private OptionSet options;
        private OsmBoundsHelper osmBounds;
        private string outputFile = "output.osm";
        private Uri osmxapiUrl;
    }
}