using System;
using Brejc.Common.Props;

namespace GroundTruth.Engine
{
    public interface ISerializersRegistry
    {
        bool IsValueDeserializable(object propertyType, string optionStringValue);
        object DeserializeValue(Type nativeType, string optionStringValue);
        void RegisterSerializer<T>(IPropertyValueSerializer serializer);
    }

    public class SerializersRegistry : ISerializersRegistry
    {
        public bool IsValueDeserializable(object propertyType, string optionStringValue)
        {
            throw new NotImplementedException();
        }

        public object DeserializeValue(Type nativeType, string optionStringValue)
        {
            throw new NotImplementedException();
        }

        public void RegisterSerializer<T>(IPropertyValueSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}