namespace GroundTruth.Engine.Wiki
{
    public class WikiSection
    {
        public WikiSection(string sectionName, int sectionLevel)
        {
            this.sectionName = sectionName;
            this.sectionLevel = sectionLevel;
        }

        public string SectionName
        {
            get { return sectionName; }
        }

        public int SectionLevel
        {
            get { return sectionLevel; }
        }

        private string sectionName;
        private int sectionLevel;
    }
}