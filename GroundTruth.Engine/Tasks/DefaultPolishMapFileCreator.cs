using System;
using System.Globalization;
using System.IO;

namespace GroundTruth.Engine.Tasks
{
    public class DefaultPolishMapFileCreator : IPolishMapFileCreator
    {
        public DefaultPolishMapFileCreator (MapMakerSettings settings)
        {
            this.settings = settings;

            try
            {
                currentMapId = int.Parse (settings.StartingMapId, CultureInfo.InvariantCulture) - 1;
            }
            catch (Exception)
            {
                throw new ArgumentException ("Starting map ID is in the wrong format");
            }
        }

        public string CurrentPolishMapFileName
        {
            get { return currentPolishMapFileName; }
        }

        public string CurrentMapId
        {
            get
            {
                return currentMapId.ToString("00000000", CultureInfo.InvariantCulture);
            }
        }

        public string CurrentMapName
        {
            get { return String.Format(CultureInfo.InvariantCulture, "{0} {1}", settings.MapNamePrefix, mapCounter); }
        }

        public Stream CreatePolishMapFile ()
        {
            currentMapId++;
            mapCounter++;
            currentPolishMapFileName = Path.GetFullPath (
                Path.Combine (
                    settings.TempDir,
                    String.Format (CultureInfo.InvariantCulture, "{0}.mp", CurrentMapId)));

            return File.Open (currentPolishMapFileName, FileMode.Create);
        }

        private int currentMapId;
        private string currentPolishMapFileName;
        private int mapCounter;
        private MapMakerSettings settings;
    }
}