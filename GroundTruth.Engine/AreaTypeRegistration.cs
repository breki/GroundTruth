using System.Collections.Generic;

namespace GroundTruth.Engine
{
    public class AreaTypeRegistration : GarminMapTypeRegistration
    {
        public AreaTypeRegistration (int typeId, string ruleName)
            : base (typeId, ruleName)
        {
        }
    }
}