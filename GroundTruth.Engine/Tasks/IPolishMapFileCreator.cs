using System.IO;

namespace GroundTruth.Engine.Tasks
{
    public interface IPolishMapFileCreator
    {
        string CurrentPolishMapFileName { get; }
        string CurrentMapId { get; }
        string CurrentMapName { get; }

        Stream CreatePolishMapFile ();
    }
}