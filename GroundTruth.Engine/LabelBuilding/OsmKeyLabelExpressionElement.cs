using System;
using System.Text;
using Brejc.OsmLibrary;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.LabelBuilding
{
    public class OsmKeyLabelExpressionElement : ILabelExpressionElement
    {
        public OsmKeyLabelExpressionElement(string keyName)
        {
            this.keyName = keyName;
        }

        public OsmKeyLabelExpressionElement(string keyName, string conditionalFormat)
        {
            this.keyName = keyName;
            conditionalElement = new FormatLabelExpressionElement(conditionalFormat);
        }

        public ILabelExpressionElement ConditionalElement
        {
            get { return conditionalElement; }
        }

        public bool IsConstant
        {
            get { return false; }
        }

        public string KeyName
        {
            get { return keyName; }
        }

        public void BuildLabel (
            MapMakerSettings mapMakerSettings, 
            StringBuilder label, 
            OsmObjectBase osmObject, 
            OsmRelation parentRelation, 
            Tag osmTag)
        {
            if (keyName.StartsWith ("relation:", StringComparison.OrdinalIgnoreCase))
            {
                string relationKeyName = keyName.Substring (9);

                if (parentRelation != null && parentRelation.HasTag (relationKeyName))
                {
                    // if there are no conditions attached, simply use the tag's value
                    if (null == conditionalElement)
                        label.Append (parentRelation.GetTagValue (relationKeyName));
                    else
                    {
                        // otherwise we must do some extra work
                        conditionalElement.BuildLabel (mapMakerSettings, label, osmObject, parentRelation, parentRelation.GetTag (relationKeyName));
                    }
                }
            }
            else if (osmObject.HasTag(keyName))
            {
                // if there are no conditions attached, simply use the tag's value
                if (null == conditionalElement)
                    label.Append(osmObject.GetTagValue(keyName));
                else
                {
                    // otherwise we must do some extra work
                    conditionalElement.BuildLabel(mapMakerSettings, label, osmObject, parentRelation, osmObject.GetTag(keyName));
                }
            }
        }

        private string keyName;
        private ILabelExpressionElement conditionalElement;
    }
}