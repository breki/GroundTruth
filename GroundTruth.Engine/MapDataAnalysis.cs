using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GroundTruth.Engine
{
    public class MapDataAnalysis
    {
        public SortedList<int, int> HardwareLevelsUsed
        {
            get { return hardwareLevelsUsed; }
        }

        public IDictionary<int, int> HardwareToLogicalLevelDictionary
        {
            get { return hardwareToLogicalLevelDictionary; }
        }

        public IDictionary<int, int> LogicalToHardwareLevelDictionary
        {
            get { return logicalToHardwareLevelDictionary; }
        }

        [SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Postprocess")]
        public void Postprocess ()
        {
            FixHardwareLevelsUsed();
            CalculateLogicalLevels();
        }

        public void MarkUsedHardwareLevel (int level)
        {
            if (false == hardwareLevelsUsed.ContainsKey (level))
                hardwareLevelsUsed.Add (level, 0);

            hardwareLevelsUsed[level] = hardwareLevelsUsed[level] + 1;
        }

        private void CalculateLogicalLevels()
        {
            int logicalLevel = 0;
            for (int i = HardwareLevelsUsed.Count - 1; i >= 0; i--)
            {
                int hardwareLevel = HardwareLevelsUsed.Keys[i];

                HardwareToLogicalLevelDictionary.Add (hardwareLevel, logicalLevel);
                LogicalToHardwareLevelDictionary.Add (logicalLevel, hardwareLevel);

                logicalLevel++;
            }            
        }

        private void FixHardwareLevelsUsed ()
        {
            if (false == HardwareLevelsUsed.Keys.Contains (24))
                MarkUsedHardwareLevel (24);
            if (false == HardwareLevelsUsed.Keys.Contains (12))
                MarkUsedHardwareLevel (12);
        }

        private SortedList<int, int> hardwareLevelsUsed = new SortedList<int, int> ();
        private Dictionary<int, int> hardwareToLogicalLevelDictionary = new Dictionary<int, int> ();
        private Dictionary<int, int> logicalToHardwareLevelDictionary = new Dictionary<int, int> ();
    }
}