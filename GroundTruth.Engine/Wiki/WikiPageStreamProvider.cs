using System.IO;
using System.Text;
using Brejc.OsmLibrary.OsmWiki;

namespace GroundTruth.Engine.Wiki
{
    public sealed class WikiPageStreamProvider
    {
        public static Stream Open (string wikiPageSource)
        {
            IOsmWikiPageProvider pageProvider =
                OsmWikiPageProviderFactory.GetProvider(wikiPageSource, null);

            string wikiPageContents = pageProvider.ProvideWikiPage (false);

            return new MemoryStream(Encoding.UTF8.GetBytes(wikiPageContents));
        }

        private WikiPageStreamProvider()
        {
        }
    }
}