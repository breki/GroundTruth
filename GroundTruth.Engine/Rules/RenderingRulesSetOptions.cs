using Brejc.Common.Props;
using Brejc.Geometry;

namespace GroundTruth.Engine.Rules
{
    public class RenderingRulesSetOptions : IRenderingRulesSetOptions
    {
        public RenderingRulesSetOptions ()
        {
            IPropertiesSchema schema = new PropertiesSchema(null);
            propertiesBag = new PropertiesBag(schema, true);

            using (PropertiesSchemaBuilder builder = new PropertiesSchemaBuilder (schema))
            {
                builder
                    .Add<bool>(SettingIdForceBackgroundColor).Default(false)
                    .Add<GisColor>(SettingIdLandBackgroundColor).Default(new GisColor(unchecked(0xFFEFBF)))
                    .Add<string>(SettingIdMinKosmosVersion)
                    .Add<GisColor> (SettingIdSeaColor).Default (new GisColor (unchecked (0x5EAEFF)));
            }
        }

        public const string SettingIdForceBackgroundColor = "ForceBackgroundColor";
        public const string SettingIdLandBackgroundColor = "LandBackgroundColor";
        public const string SettingIdMinKosmosVersion = "RulesVersion";
        public const string SettingIdSeaColor = "SeaColor";

        public bool ForceBackgroundColor
        {
            get { return propertiesBag.Get2<bool> (SettingIdForceBackgroundColor).Value; }
            set { propertiesBag.SetValue(SettingIdForceBackgroundColor, value); }
        }

        public GisColor LandBackgroundColor
        {
            get { return propertiesBag.Get2<GisColor> (SettingIdLandBackgroundColor).Value; }
            set { propertiesBag.SetValue (SettingIdLandBackgroundColor, value); }
        }

        public string RulesVersion
        {
            get { return propertiesBag.Get<string> (SettingIdMinKosmosVersion); }
            set { propertiesBag.SetValue(SettingIdMinKosmosVersion, value); }
        }

        public IProperties Properties
        {
            get { return propertiesBag; }
        }

        public GisColor SeaColor
        {
            get { return propertiesBag.Get2<GisColor> (SettingIdSeaColor).Value; }
            set { propertiesBag.SetValue (SettingIdSeaColor, value); }
        }

        private PropertiesBag propertiesBag;
    }
}