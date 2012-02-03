using System;
using System.Collections.Generic;
using GroundTruth.Engine.Contours.Ibf2Osm;

namespace GroundTruth
{
    public class Ibf2OsmCommand : IConsoleCommand
    {
        public Ibf2OsmCommand(IIbf2OsmGenerator generator)
        {
            this.generator = generator;
        }

        public string CommandDescription
        {
            get { return "Generates OSM files from the IBF contours data"; }
        }

        public string CommandName
        {
            get { return "ibf2osm"; }
        }

        public int Execute(IEnumerable<string> args)
        {
            Ibf2OsmGenerationParameters parameters = parser.Parse (args);

            generator.ProgressCallback =
                (message, progress, level) =>
                {
                    if (message != null)
                        Console.Out.WriteLine (message);
                    return true;
                };

            generator.Run (parameters);

            return 0;
        }

        public void ShowHelp()
        {
            parser.WriteOptionDescriptions (System.Console.Out);
        }

        private readonly IIbf2OsmGenerator generator;
        private Ibf2OsmCommandParser parser = new Ibf2OsmCommandParser ();
    }
}