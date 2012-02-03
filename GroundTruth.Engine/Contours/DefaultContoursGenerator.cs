using System;
using System.Collections.Generic;
using Brejc.Rasters;

namespace GroundTruth.Engine.Contours
{
    // todo ibf
    public class DefaultContoursGenerator : IContoursGenerator
    {
        public DefaultContoursGenerator(IRasterSource demSource)
        {
            this.demSource = demSource;
        }

        //public IbfUtilities IbfUtilities
        //{
        //    get { return ibfUtilities; }
        //    set { ibfUtilities = value; }
        //}

        public void Run(ContoursGenerationParameters parameters)
        {
            ValidateParameters(parameters);

            throw new NotImplementedException();
            //IbfFile ibfFile = new IbfFile (parameters.OutputFile);

            //IList<string> areaDefinitionFiles = ibfUtilities.GenerateAreaDefinitions (
            //    ibfFile,
            //    demSource,
            //    parameters.IbfParameters);
            //ibfUtilities.Create (ibfFile, areaDefinitionFiles);
        }

        private static void ValidateParameters(ContoursGenerationParameters parameters)
        {
            //if (parameters.IbfParameters.GenerationBounds == null)
            //    throw new ArgumentException ("Map boundaries have not been specified");
        }

        private IRasterSource demSource;
        //private IbfUtilities ibfUtilities = new IbfUtilities ();
    }
}