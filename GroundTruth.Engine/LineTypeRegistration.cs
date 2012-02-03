using System.Collections.Generic;

namespace GroundTruth.Engine
{
    public class LineTypeRegistration : GarminMapTypeRegistration
    {
        public LineTypeRegistration (int typeId, string ruleName)
            : base (typeId, ruleName)
        {
        }

        public int BorderWidth
        {
            get { return borderWidth; }
            set { borderWidth = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        private int borderWidth;
        private int width = 1;
    }
}