using System.Collections.Generic;
using System.Globalization;

namespace GroundTruth.Engine.PolishMaps
{
    public class TypesRegistry
    {
        public GarminTypesDictionary GarminAreaTypesDictionary
        {
            get { return garminAreaTypesDictionary; }
        }

        public GarminTypesDictionary GarminLineTypesDictionary
        {
            get { return garminLineTypesDictionary; }
        }

        public GarminTypesDictionary GarminPointTypesDictionary
        {
            get { return garminPointTypesDictionary; }
        }

        public IDictionary<string, PatternDefinition> Patterns
        {
            get { return patterns; }
        }

        public IDictionary<string, AreaTypeRegistration> AreaTypeRegistrations
        {
            get { return areaTypeRegistrations; }
        }

        public IList<AreaTypeRegistration> AreaTypeRegistrationsByPriority
        {
            get { return areaTypeRegistrationsByPriority; }
        }

        public IDictionary<string, LineTypeRegistration> LineTypeRegistrations
        {
            get { return lineTypeRegistrations; }
        }

        public IDictionary<string, PointTypeRegistration> PointTypeRegistrations
        {
            get { return pointTypeRegistrations; }
        }

        public PatternDefinition GetPattern(string patternName)
        {
            if (false == patterns.ContainsKey(patternName))
                throw new KeyNotFoundException(string.Format(CultureInfo.InvariantCulture, "Pattern '{0}' is not defined", patternName));

            return patterns[patternName];
        }

        public AreaTypeRegistration RegisterNewAreaType (string ruleName, bool insertAsFirst)
        {
            int id = customAreaNextTypeId++;

            if (customPointNextTypeId == 0x4b) 
                customPointNextTypeId++;

            if (customPointNextTypeId == 0x4c)
                customPointNextTypeId++;

            if ((customAreaNextTypeId & 0xff) == 20)
                customAreaNextTypeId += 0xe0;

            AreaTypeRegistration reg = new AreaTypeRegistration (id, ruleName);
            areaTypeRegistrations.Add (ruleName, reg);

            if (insertAsFirst)
                areaTypeRegistrationsByPriority.Insert(0, reg);
            else
                areaTypeRegistrationsByPriority.Add(reg);

            return reg;
        }

        public LineTypeRegistration RegisterNewLineType (string ruleName)
        {
            int id = customLineNextTypeId++;

            if ((customLineNextTypeId & 0xff) == 20)
                customLineNextTypeId += 0xe0;

            LineTypeRegistration reg = new LineTypeRegistration (id, ruleName);
            lineTypeRegistrations.Add (ruleName, reg);
            return reg;
        }

        public PointTypeRegistration RegisterNewPointType (string ruleName)
        {
            int id = customPointNextTypeId++;

            if ((customPointNextTypeId & 0xff) == 20)
                customPointNextTypeId += 0xe0;

            PointTypeRegistration reg = new PointTypeRegistration (id, ruleName);
            pointTypeRegistrations.Add (ruleName, reg);
            return reg;
        }

        public AreaTypeRegistration RegisterStandardAreaType (int typeId, string ruleName)
        {
            AreaTypeRegistration reg = new AreaTypeRegistration (typeId, ruleName);
            areaTypeRegistrations.Add (ruleName, reg);
            areaTypeRegistrationsByPriority.Add(reg);
            return reg;
        }

        public LineTypeRegistration RegisterStandardLineType (int typeId, string ruleName)
        {
            LineTypeRegistration reg = new LineTypeRegistration (typeId, ruleName);
            lineTypeRegistrations.Add (ruleName, reg);
            return reg;
        }

        public PointTypeRegistration RegisterStandardPointType (int typeId, string ruleName)
        {
            PointTypeRegistration reg = new PointTypeRegistration (typeId, ruleName);
            pointTypeRegistrations.Add(ruleName, reg);
            return reg;
        }

        private int customAreaNextTypeId = 0x010f00;
        private int customLineNextTypeId = 0x010e00;
        private int customPointNextTypeId = 0x011500;
        private GarminTypesDictionary garminAreaTypesDictionary = new GarminTypesDictionary();
        private GarminTypesDictionary garminLineTypesDictionary = new GarminTypesDictionary();
        private GarminTypesDictionary garminPointTypesDictionary = new GarminTypesDictionary();
        private Dictionary<string, PatternDefinition> patterns = new Dictionary<string, PatternDefinition>();
        private Dictionary<string, PointTypeRegistration> pointTypeRegistrations = new Dictionary<string, PointTypeRegistration> ();
        private Dictionary<string, AreaTypeRegistration> areaTypeRegistrations = new Dictionary<string, AreaTypeRegistration> ();
        private List<AreaTypeRegistration> areaTypeRegistrationsByPriority = new List<AreaTypeRegistration> ();
        private Dictionary<string, LineTypeRegistration> lineTypeRegistrations = new Dictionary<string, LineTypeRegistration> ();
    }
}