using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Brejc.Geometry;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Rules;

namespace GroundTruth.Engine.Tasks
{
    [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MapMaker")]
    public class MapMakerSettings
    {
        public IDictionary<string, string> AdditionalMapParameters
        {
            get { return additionalMapParameters; }
        }

        public string CGpsMapperPath
        {
            get { return cGpsMapperPath; }
            set { cGpsMapperPath = value; }
        }

        public CharactersConversionDictionary CharactersConversionDictionary
        {
            get { return charactersConversionDictionary; }
        }

        public string CharactersConversionTableSource
        {
            get { return charactersConversionTableSource; }
            set { charactersConversionTableSource = value; }
        }

        public ContoursElevationRuleMap ContoursRenderingRules
        {
            get { return contoursRenderingRules; }
            set { contoursRenderingRules = value; }
        }

        public string ContoursRenderingRulesSource
        {
            get { return contoursRenderingRulesSource; }
            set { contoursRenderingRulesSource = value; }
        }

        public char ElevationUnits
        {
            get { return elevationUnits; }
            set { elevationUnits = value; }
        }

        public int ExternalCommandTimeoutInMinutes
        {
            get { return externalCommandTimeoutInMinutes; }
            set { externalCommandTimeoutInMinutes = value; }
        }

        public int FamilyCode
        {
            get { return familyCode; }
            set { familyCode = value; }
        }

        public string FamilyName
        {
            get { return familyName; }
            set { familyName = value; }
        }

        public MapContentStatistics MapContentStatistics
        {
            get { return mapContentStatistics; }
        }

        public string MapNamePrefix
        {
            get { return mapNamePrefix; }
            set { mapNamePrefix = value; }
        }

        public string MapTransparencyMode
        {
            get { return mapTransparencyMode; }
            set { mapTransparencyMode = value; }
        }

        public int MapVersion
        {
            get { return mapVersion; }
            set { mapVersion = value; }
        }

        [SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cgpsmapper")]
        public bool NoCgpsmapper
        {
            get { return noCgpsmapper; }
            set { noCgpsmapper = value; }
        }

        public bool NonWindowsMode
        {
            get { return nonWindowsMode; }
            set { nonWindowsMode = value; }
        }

        public IList<IMapDataSource> MapDataSources
        {
            get { return mapDataSources; }
        }

        public string OutputPath
        {
            get { return outputPath; }
            set { outputPath = value; }
        }

        public int ProductCode
        {
            get { return productCode; }
            set { productCode = value; }
        }

        public string ProductName
        {
            get { return productName; }
            set { productName = value; }
        }

        public RenderingRuleSet RenderingRules
        {
            get { return renderingRules; }
            set { renderingRules = value; }
        }

        public string RenderingRulesSource
        {
            get { return renderingRulesSource; }
            set { renderingRulesSource = value; }
        }

        public string SendMapExePath
        {
            get { return sendMapExePath; }
            set { sendMapExePath = value; }
        }

        public float? SimplifyLevel
        {
            get { return simplifyLevel; }
            set { simplifyLevel = value; }
        }

        public bool SkipCoastlineProcessing
        {
            get { return skipCoastlineProcessing; }
            set { skipCoastlineProcessing = value; }
        }

        public Point2<double> SplitFrame
        {
            get { return splitFrame; }
            set { splitFrame = value; }
        }

        public string StandardGarminTypesSource
        {
            get { return standardGarminTypesSource; }
            set { standardGarminTypesSource = value; }
        }

        public string StartingMapId
        {
            get { return startingMapId; }
            set { startingMapId = value; }
        }

        public string TempDir
        {
            get { return Path.Combine (outputPath, "Temp"); }
        }

        [SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tre")]
        public int? TreSize
        {
            get { return treSize; }
            set { treSize = value; }
        }

        public TypesRegistry TypesRegistry
        {
            get { return typesRegistry; }
            set { typesRegistry = value; }
        }

        public bool UploadToGps
        {
            get { return uploadToGps; }
            set { uploadToGps = value; }
        }

        public void CheckParameters ()
        {
            //if (string.IsNullOrEmpty(startingMapId))
            //    throw new ArgumentException("Map ID is missing.");

            //if (string.IsNullOrEmpty (mapNamePrefix))
            //    throw new ArgumentException ("Map name is missing.");

            if (mapDataSources.Count == 0)
                throw new ArgumentException ("OSM files are missing.");

            if (string.IsNullOrEmpty (renderingRulesSource))
                throw new ArgumentException ("Rendering rules are missing.");
        }

        public void SetMapDataSources (IList<IMapDataSource> mapDataSources)
        {
            this.mapDataSources = mapDataSources;
        }

        public void UseDefaultParameterValueIfMissing (string optionName, string optionValue)
        {
            if (false == additionalMapParameters.ContainsKey (optionName))
                additionalMapParameters.Add (optionName, optionValue);
        }

        private Dictionary<string, string> additionalMapParameters = new Dictionary<string, string> ();
        private string cGpsMapperPath = ".";
        private CharactersConversionDictionary charactersConversionDictionary = new CharactersConversionDictionary();
        private string charactersConversionTableSource = @"Rules/CharacterConversionTable.txt";
        private string contoursRenderingRulesSource = @"Rules/ContoursRulesMetric.txt";
        private ContoursElevationRuleMap contoursRenderingRules;
        private char elevationUnits = 'm';
        private int externalCommandTimeoutInMinutes = 60;
        private int familyCode = 1;
        private string familyName = "GroundTruth Maps";
        private MapContentStatistics mapContentStatistics = new MapContentStatistics();
        private IList<IMapDataSource> mapDataSources = new List<IMapDataSource>();
        private string mapNamePrefix = "GroundTruth Map";
        private string mapTransparencyMode = "N";
        private int mapVersion = 100;
        private bool noCgpsmapper;
        private bool nonWindowsMode;
        private string outputPath = "Maps";
        private int productCode = 1;
        private string productName = "GroundTruth Maps";
        private string renderingRulesSource = @"Rules/DefaultRules.txt";
        private RenderingRuleSet renderingRules;
        private string sendMapExePath = "sendmap20.exe";
        private float? simplifyLevel;
        private bool skipCoastlineProcessing;
        private Point2<double> splitFrame;
        private string standardGarminTypesSource = @"Rules/StandardGarminTypes.txt";
        private string startingMapId = "12345678";
        private int? treSize;
        private TypesRegistry typesRegistry = new TypesRegistry();
        private bool uploadToGps;
    }
}