namespace GroundTruth.Engine
{
    public class ParsingContext
    {
        public int Line
        {
            get { return line; }
            set { line = value; }
        }

        public string RuleName
        {
            get { return ruleName; }
            set { ruleName = value; }
        }

        public string TemplateParameterName
        {
            get { return templateParameterName; }
            set { templateParameterName = value; }
        }

        public string TemplateParameterValue
        {
            get { return templateParameterValue; }
            set { templateParameterValue = value; }
        }

        public string TemplateType
        {
            get { return templateType; }
            set { templateType = value; }
        }

        public void NewLine (int line)
        {
            this.line = line;
            ruleName = null;
            templateParameterName = null;
            templateParameterValue = null;
            templateType = null;
        }

        private int line;
        private string ruleName;
        private string templateParameterName;
        private string templateParameterValue;
        private string templateType;
    }
}