using System.Collections.Generic;
using GroundTruth.Engine.Wiki;

namespace GroundTruth.Engine.Wiki
{
    public class WikiParserContext
    {
        public string CurrentLine
        {
            get { return currentLine; }
        }

        public WikiContentType CurrentType
        {
            get { return currentType; }
        }

        public int LineCounter
        {
            get { return lineCounter; }
        }

        public WikiSection LowestSection
        {
            get
            {
                if (sections.Count > 0)
                    return sections[sections.Count-1];
                return null;
            }
        }
        
        public void RegisterNewLine (string newLine)
        {
            currentLine = newLine;
            lineCounter++;
        }

        public void RegisterSection (WikiSection section)
        {
            sections.RemoveAll(ws => ws.SectionLevel >= section.SectionLevel);
            sections.Add(section);
        }

        public WikiContentType RegisterContentType (WikiContentType contentType)
        {
            this.currentType = contentType;
            return contentType;
        }
        
        private string currentLine;
        private WikiContentType currentType;
        private int lineCounter;
        private List<WikiSection> sections = new List<WikiSection> ();
    }
}