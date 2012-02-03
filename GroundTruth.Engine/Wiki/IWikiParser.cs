using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GroundTruth.Engine.Wiki;

namespace GroundTruth.Tests
{
    public interface IWikiParser : IDisposable
    {
        WikiParserContext Context { get; }

        [SuppressMessage ("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Next")]
        WikiContentType Next ();
        [SuppressMessage ("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        void ParseTable (Action<IList<string>> processTableRowDelegate);
        void SetWikiContentSource(Stream stream);
    }
}