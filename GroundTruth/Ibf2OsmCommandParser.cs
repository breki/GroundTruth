using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GroundTruth.Engine.Contours.Ibf2Osm;
using NDesk.Options;

namespace GroundTruth
{
    public class Ibf2OsmCommandParser
    {
        public Ibf2OsmCommandParser()
        {
            options = new OptionSet () {
            { "i|ibf=", "specifies the {IBF file} to use to generate contours from (the default is output.ibf)",
              (string ibfFileName) => parameters.IbfFileName = ibfFileName},
            { "od|outputdir=", "output {directory path} where all OSM files will be stored (the default is the Output directory)",
              (string outputDir) => parameters.OutputDir = outputDir},
            { "of|outputfileformat=", "{file format} of output files (the default is 'contours{0}.osm.bz2' where '{0}' will be replaced with the ID of the file)",
              (string outputFileFormat) => parameters.OutputFileFormat = outputFileFormat},
            { "sid|startid=", "starting {ID} for all OSM ways and nodes (the default is 1000000000)",
              (int id) => parameters.StartId = id},
            { "tagce", "adds 'contour=elevation' tag to all contour OSM ways",
              v => { if (v != null) parameters.OsmWayTaggers.Add(new ContourElevationTagTagger()); }},
            { "cat=", "adds the 'contour_ext' tag with elevation category based on {category elevations list}: <major>,<medium>",
              (string catParams) =>
                  {
                      string[] par = catParams.Split(',');
                      if (par.Length != 2)
                          throw new ArgumentException(
                              string.Format(
                                CultureInfo.InvariantCulture,
                                "Invalid category elevations list: '{0}'",
                                catParams));

                      int majorele = int.Parse(par[0], CultureInfo.InvariantCulture);
                      int mediumele = int.Parse(par[1], CultureInfo.InvariantCulture);

                      parameters.OsmWayTaggers.Add(new CategoryTagger(majorele, mediumele));
                  }
                },
            { "nonwin", "does nothing, it's just here for scripting purposes",
              v => { ; }
                },
            };
        }

        public Ibf2OsmGenerationParameters Parse (IEnumerable<string> args)
        {
            parameters = new Ibf2OsmGenerationParameters ();
            options.Parse (args);
            return parameters;
        }

        public void WriteOptionDescriptions (TextWriter textWriter)
        {
            options.WriteOptionDescriptions (textWriter);
        }

        private OptionSet options;
        private Ibf2OsmGenerationParameters parameters;
    }
}