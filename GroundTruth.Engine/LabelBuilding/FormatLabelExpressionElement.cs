using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Brejc.OsmLibrary;
using GroundTruth.Engine.Tasks;
using Wintellect.PowerCollections;

namespace GroundTruth.Engine.LabelBuilding
{
    public class FormatLabelExpressionElement : ILabelExpressionElement
    {
        public FormatLabelExpressionElement(string format)
        {
            this.format = format;
        }

        public string Format
        {
            get { return format; }
        }

        public bool IsConstant
        {
            get { return true; }
        }

        [SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void BuildLabel (
            MapMakerSettings mapMakerSettings, 
            StringBuilder label, 
            OsmObjectBase osmObject, 
            OsmRelation parentRelation, 
            Tag osmTag)
        {
            try
            {
                string expandedString = regexFunction.Replace(
                    format, 
                    delegate(Match match)
                        {
                            string functionName = match.Groups["function"].Value;

                            if (false == registeredFunctions.ContainsKey(functionName))
                                throw new ArgumentException(
                                    String.Format(
                                        CultureInfo.InvariantCulture,
                                        "Unknown label building function '{0}'",
                                        functionName));

                            ILabelBuildingFunction function = registeredFunctions[functionName];
                            string result = function.Calculate(mapMakerSettings, osmObject, osmTag, parentRelation);

                            return result;
                        });

                label.Append(expandedString);
            }
            catch (Exception)
            {
                // an exception occurred, we do nothing
            }
        }

        [SuppressMessage ("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static FormatLabelExpressionElement ()
        {
            registeredFunctions = new Dictionary<string, ILabelBuildingFunction>();    
            registeredFunctions.Add("value", new ValueLabelBuildingFunction());
            registeredFunctions.Add("elevation", new ElevationLabelBuildingFunction());
            registeredFunctions.Add("uppercase", new UppercaseLabelBuildingFunction());
            registeredFunctions.Add("1c", new SpecialCodeLabelBuildingFunction(0x1c));
            registeredFunctions.Add("1e", new SpecialCodeLabelBuildingFunction(0x1e));
            registeredFunctions.Add("1f", new SpecialCodeLabelBuildingFunction(0x1f));
            registeredFunctions.Add("01", new SpecialCodeLabelBuildingFunction (0x1));
            registeredFunctions.Add("02", new SpecialCodeLabelBuildingFunction (0x2));
            registeredFunctions.Add("03", new SpecialCodeLabelBuildingFunction (0x3));
            registeredFunctions.Add("04", new SpecialCodeLabelBuildingFunction (0x4));
            registeredFunctions.Add("05", new SpecialCodeLabelBuildingFunction (0x5));
            registeredFunctions.Add("06", new SpecialCodeLabelBuildingFunction (0x6));
        }

        private string format;
        private static readonly Regex regexFunction = new Regex (@"\$(?<function>\w+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static Dictionary<string, ILabelBuildingFunction> registeredFunctions;
    }
}