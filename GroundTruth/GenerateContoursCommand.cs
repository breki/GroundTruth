using System;
using System.Collections.Generic;
using GroundTruth.Engine.Contours;

namespace GroundTruth
{
    public class GenerateContoursCommand : IConsoleCommand
    {
        public GenerateContoursCommand(IContoursGenerator contoursGenerator)
        {
            this.contoursGenerator = contoursGenerator;
        }

        public string CommandDescription
        {
            get { return "Generates elevation contours for a given area"; }
        }

        public string CommandName
        {
            get { return "contours"; }
        }

        public int Execute(IEnumerable<string> args)
        {
            ContoursGenerationParameters parameters = parser.Parse(args);
            contoursGenerator.Run(parameters);

            return 0;
        }

        public void ShowHelp()
        {
            parser.WriteOptionDescriptions(System.Console.Out);
        }

        private readonly IContoursGenerator contoursGenerator;
        private IbfCommandParser parser = new IbfCommandParser ();
    }
}