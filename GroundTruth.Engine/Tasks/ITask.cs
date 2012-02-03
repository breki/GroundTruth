namespace GroundTruth.Engine.Tasks
{
    public interface ITask
    {
        bool SkipOnNonWindows { get; }
        string TaskDescription { get; }

        void Execute(ITaskRunner taskRunner);
    }
}