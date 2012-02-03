using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Brejc.OsmLibrary;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.LabelBuilding
{
    public class LabelExpression
    {
        public IList<ILabelExpressionElement> Elements
        {
            get { return elements; }
        }

        public int ElementsCount
        {
            get { return elements.Count; }
        }

        public bool IsConstant
        {
            get
            {
                foreach (ILabelExpressionElement element in elements)
                {
                    if (false == element.IsConstant)
                        return false;
                }

                return true;
            }
        }

        public void AddElement(ILabelExpressionElement element)
        {
            elements.Add(element);
        }

        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        public string BuildLabel (MapMakerSettings mapMakerSettings, OsmObjectBase osmObject, OsmRelation parentRelation)
        {
            StringBuilder label = new StringBuilder();

            foreach (ILabelExpressionElement element in elements)
                element.BuildLabel (mapMakerSettings, label, osmObject, parentRelation, null);

            string labelAsString = label.ToString();
            labelAsString = mapMakerSettings.CharactersConversionDictionary.Convert(labelAsString);

            return labelAsString;
        }

        private List<ILabelExpressionElement> elements = new List<ILabelExpressionElement>();
    }
}