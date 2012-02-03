using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GroundTruth.Engine
{
    public class PatternDefinition
    {
        public PatternDefinition()
        {
        }

        public PatternDefinition(string patternName)
        {
            this.patternName = patternName;
        }

        public int ColorsCount
        {
            get { return Math.Max(colorList.Count, colorDictionary.Count); }
        }

        public int Height
        {
            get
            {
                return patternLines.Count;
            }
        }

        public string PatternName
        {
            get { return patternName; }
        }

        public int Width
        {
            get
            {
                if (patternLines.Count > 0)
                    return patternLines[0].Length;
                return -1;
            }
        }

        public IList<string> PatternLines
        {
            get { return patternLines; }
        }

        public void AddColor (string color)
        {
            if (colorDictionary.Count > 0)
                throw new InvalidOperationException ("You cannot use both color list and color dictionary in the same instance of PatternDefinition.");

            colorList.Add (color);
        }

        public void AddColor (string colorId, string color)
        {
            if (colorList.Count > 0)
                throw new InvalidOperationException("You cannot use both color list and color dictionary in the same instance of PatternDefinition.");

            colorDictionary.Add(colorId, color);
        }

        public string GetColorByIndex (int index)
        {
            return colorList[index];
        }

        public string GetColorByKey (string colorKey)
        {
            return colorDictionary[colorKey];
        }

        [SuppressMessage ("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<KeyValuePair<string, string>> EnumerateColorDictionary ()
        {
            return colorDictionary;
        }

        public IEnumerable<string> EnumerateColorList()
        {
            return colorList;
        }

        private Dictionary<string, string> colorDictionary = new Dictionary<string, string>();
        private List<string> colorList = new List<string>();
        private List<string> patternLines = new List<string> ();
        private string patternName;
    }
}