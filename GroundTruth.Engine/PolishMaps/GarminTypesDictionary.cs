using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GroundTruth.Engine.PolishMaps
{
    [SuppressMessage ("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class GarminTypesDictionary
    {
        public GarminTypesDictionary()
        {
        }

        public int TypesCount { get { return types.Count; } }

        public void AddType(string typeName, int typeId)
        {
            types.Add(typeName, typeId);
        }

        public int GetType (string typeName)
        {
            if (false == types.ContainsKey(typeName))
                throw new KeyNotFoundException(string.Format(CultureInfo.InvariantCulture, "Garmin type '{0}' not defined", typeName));

            return types[typeName];
        }

        public bool HasType(string typeName)
        {
            return types.ContainsKey(typeName);
        }

        private Dictionary<string,int> types = new Dictionary<string, int>();
    }
}