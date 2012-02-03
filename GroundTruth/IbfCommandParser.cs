using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Brejc.Geometry;
using Brejc.OsmLibrary.Helpers;
using GroundTruth.Engine.Contours;
using NDesk.Options;

namespace GroundTruth
{
    public class IbfCommandParser
    {
        public IbfCommandParser()
        {
            options = new OptionSet () {
            { "o|outputfile=", "output {IBF file} (the default is output.ibf)",
              (string outputFile) => parameters.OutputFile = outputFile},
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

                      parameters.GenerationBounds = 
                          new OsmBoundsHelper (new Bounds2(minlng, minlat, maxlng, maxlat)).Bounds;
                  }
                },
            { "bu|boundsurl=", "specifies map boundaries by using OSM map {url} (please put the url inside double quotes)",
              (string url) =>
                  {
                      parameters.GenerationBounds = OsmBoundsHelper.FromSlippyMapUrl(new Uri(url)).Bounds;
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

                      parameters.GenerationBounds = OsmBoundsHelper.FromBox(lat, lng, size).Bounds;
                  }
                },
            { "feet", "uses feet as elevation units (the default is meters)",
              v => { parameters.ElevationUnit = ContourUnits.Feet; }
                },
            { "int|interval=", "specifies the {interval} (a real value, in elevation units) between contours",
              v => { parameters.IsohypseIntervalInUnits = double.Parse(v, CultureInfo.InvariantCulture) ; }
                },
            { "gridlat=", "specifies the height of the generated contour tiles (in {degrees of latitude} - default is 0.25 degrees)",
              v => { parameters.LatitudeGrid = double.Parse(v, CultureInfo.InvariantCulture) ; }
                },
            { "gridlon=", "specifies the width of the generated contour tiles (in {degrees of longitude} - default is 0.25 degrees)",
              v => { parameters.LongitudeGrid = double.Parse(v, CultureInfo.InvariantCulture) ; }
                },
            { "nocut", "does not cut the contour tiles to the map bounds, it keeps them whole (the tiles are cut by default)",
              v => { parameters.CutToBounds = false; }
                },
            { "nonwin", "does nothing, it's just here for scripting purposes",
              v => { ; }
                },
            };
        }

        public ContoursGenerationParameters Parse(IEnumerable<string> args)
        {
            parameters = new ContoursGenerationParameters();
            parameters.CutToBounds = true;
            options.Parse(args);
            return parameters;
        }

        public void WriteOptionDescriptions(TextWriter textWriter)
        {
            options.WriteOptionDescriptions(textWriter);
        }

        private ContoursGenerationParameters parameters;
        private OptionSet options;
    }
}