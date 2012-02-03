using Brejc.Common;

namespace GroundTruth.Engine.Contours
{
    public interface IContoursGenerator
    {
        void Run(ContoursGenerationParameters parameters);
    }
}
