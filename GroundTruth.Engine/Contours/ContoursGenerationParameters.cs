using System;
using Brejc.Geometry;

namespace GroundTruth.Engine.Contours
{
    public enum ContourUnits
    {
        Meters,
        Feet
    }

    // todo ibf
    public class ContoursGenerationParameters
    {
        //public GenerateIbfAreaDefinitionsParameters IbfParameters
        //{
        //    get { return ibfParameters; }
        //    set { ibfParameters = value; }
        //}

        public string OutputFile
        {
            get { return outputFile; }
            set { outputFile = value; }
        }

        public Bounds2 GenerationBounds { get; set; }

        public ContourUnits ElevationUnit
        {
            get 
            {
                return elevationUnit;
            }

            set 
            {
                elevationUnit = value;
            }
        }

        public double IsohypseIntervalInUnits
        {
            get {
                return isohypseIntervalInUnits;
            }
            set {
                isohypseIntervalInUnits = value;
            }
        }

        public double LatitudeGrid
        {
            get {
                return latitudeGrid;
            }
            set {
                latitudeGrid = value;
            }
        }

        public double LongitudeGrid
        {
            get {
                return longitudeGrid;
            }
            set {
                longitudeGrid = value;
            }
        }

        public bool CutToBounds
        {
            get {
                return cutToBounds;
            }
            set {
                cutToBounds = value;
            }
        }

        //private GenerateIbfAreaDefinitionsParameters ibfParameters = new GenerateIbfAreaDefinitionsParameters();
        private string outputFile = "output.ibf";
        private ContourUnits elevationUnit = ContourUnits.Meters;
        private double isohypseIntervalInUnits = 20;
        private double latitudeGrid = 0.25;
        private double longitudeGrid = 0.25;
        private bool cutToBounds = true;
    }
}