using Brejc.OsmLibrary;

namespace GroundTruth.Engine.Tasks
{
    public interface IOsmDatabaseProvider
    {
        /// <summary>
        /// Gets a value indicating whether this provider can reload the same OSM data without incurring much
        /// performance problems.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this provider is reloadable; otherwise, <c>false</c>.
        /// </value>
        bool IsReloadable { get; }

        InMemoryOsmDatabase Provide ();
    }
}