namespace GroundTruth.Engine.PolishMaps
{
    public interface IRenderingTemplate
    {
        MapElementStyle Style { get; }
        GarminMapTypeRegistration TypeRegistration { get; }
        void RegisterType (
            string ruleName, 
            TypesRegistry typesRegistry,
            bool insertAsFirst);
    }
}