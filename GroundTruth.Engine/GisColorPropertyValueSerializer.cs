using System;
using System.Drawing;
using Brejc.Common.Props;
using Brejc.Geometry;

namespace GroundTruth.Engine
{
    public class GisColorPropertyValueSerializer : IPropertyValueSerializer
    {
        public bool CanDeserializeType (Type propertyType)
        {
            return propertyType == typeof (GisColor);
        }

        public object DeserializeValue (string serializedValue)
        {
            return new GisColor (ColorTranslator.FromHtml (serializedValue).ToArgb ());
        }

        public bool IsCDataNecessary (object value)
        {
            return false;
        }

        public string SerializeValue (object value)
        {
            GisColor colorValue = (GisColor)value;

            return ColorTranslator.ToHtml (Color.FromArgb (colorValue.Argb));
        }
    }
}