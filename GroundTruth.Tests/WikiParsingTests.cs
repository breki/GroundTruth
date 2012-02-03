using System.IO;
using GroundTruth.Engine.Wiki;
using MbUnit.Framework;

namespace GroundTruth.Tests
{
    [TestFixture]
    public class WikiParsingTests
    {
        [Test]
        public void Test()
        {
            using (IWikiParser parser = new MediaWikiParser ())
            {
                using (Stream stream = File.OpenRead("../../../GroundTruth/Rules/HikingMapRules.txt"))
                {
                    parser.SetWikiContentSource(stream);

                    WikiContentType wikiContentType;

                    for (int i = 0; i < 3; i++)
                    {
                        wikiContentType = parser.Next();
                        Assert.AreEqual(WikiContentType.Text, wikiContentType);
                    }

                    wikiContentType = parser.Next();
                    Assert.AreEqual(WikiContentType.Section, wikiContentType);
                    Assert.AreEqual("Rendering Rules", parser.Context.LowestSection.SectionName);
                    Assert.AreEqual(1, parser.Context.LowestSection.SectionLevel);

                    wikiContentType = parser.Next ();
                    Assert.AreEqual (WikiContentType.Section, wikiContentType);
                    Assert.AreEqual ("Options", parser.Context.LowestSection.SectionName);
                    Assert.AreEqual (2, parser.Context.LowestSection.SectionLevel);

                    wikiContentType = parser.Next ();
                    Assert.AreEqual (WikiContentType.Table, wikiContentType);
                    parser.ParseTable(null);

                    wikiContentType = parser.Next ();
                    Assert.AreEqual (WikiContentType.Section, wikiContentType);
                    Assert.AreEqual ("Areas", parser.Context.LowestSection.SectionName);
                    Assert.AreEqual (2, parser.Context.LowestSection.SectionLevel);

                    wikiContentType = parser.Next ();
                    Assert.AreEqual (WikiContentType.Table, wikiContentType);
                    parser.ParseTable (null);

                    wikiContentType = parser.Next ();
                    Assert.AreEqual (WikiContentType.Section, wikiContentType);
                    Assert.AreEqual ("Lines", parser.Context.LowestSection.SectionName);
                    Assert.AreEqual (2, parser.Context.LowestSection.SectionLevel);

                    wikiContentType = parser.Next ();
                    Assert.AreEqual (WikiContentType.Table, wikiContentType);
                    parser.ParseTable (null);

                    wikiContentType = parser.Next ();
                    Assert.AreEqual (WikiContentType.Section, wikiContentType);
                    Assert.AreEqual ("Points", parser.Context.LowestSection.SectionName);
                    Assert.AreEqual (2, parser.Context.LowestSection.SectionLevel);

                    wikiContentType = parser.Next ();
                    Assert.AreEqual (WikiContentType.Table, wikiContentType);
                    parser.ParseTable (null);

                    wikiContentType = parser.Next ();
                    Assert.AreEqual (WikiContentType.Section, wikiContentType);
                    Assert.AreEqual ("Patterns", parser.Context.LowestSection.SectionName);
                    Assert.AreEqual (1, parser.Context.LowestSection.SectionLevel);
                }
            }
        }
    }
}