namespace GroundTruth.Engine.Tasks
{
    public class ProductFile
    {
        public ProductFile(string productFileType, string productFileName, bool isTemporary)
        {
            this.productFileName = productFileName;
            this.productFileType = productFileType;
            this.isTemporary = isTemporary;
        }

        public string ProductFileName
        {
            get { return productFileName; }
            set { productFileName = value; }
        }

        public string ProductFileType
        {
            get { return productFileType; }
        }

        public bool IsTemporary
        {
            get { return isTemporary; }
        }

        private string productFileName;
        private string productFileType;
        private bool isTemporary;
    }
}