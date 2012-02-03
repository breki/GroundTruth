namespace GroundTruth.Engine.PolishMaps
{
    public abstract class TemplateBase : IRenderingTemplate
    {
        public MapElementStyle Style
        {
            get { return style; }
        }

        public GarminMapTypeRegistration TypeRegistration
        {
            get { return typeRegistration; }
            protected set { typeRegistration = value;}
        }

        public abstract void RegisterType (
            string ruleName, 
            TypesRegistry typesRegistry,
            bool insertAsFirst);

        private MapElementStyle style = new MapElementStyle();
        private GarminMapTypeRegistration typeRegistration;
    }
}