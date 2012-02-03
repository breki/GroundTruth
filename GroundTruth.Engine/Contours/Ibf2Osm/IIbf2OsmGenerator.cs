using Brejc.Common;

namespace GroundTruth.Engine.Contours.Ibf2Osm
{
    // todo ibf
    public interface IIbf2OsmGenerator
    {
        AsyncOperationCallback ProgressCallback { get; set; }

        void Run (Ibf2OsmGenerationParameters parameters);        
    }
}