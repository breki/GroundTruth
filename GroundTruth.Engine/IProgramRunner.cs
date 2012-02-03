namespace GroundTruth.Engine
{
    public interface IProgramRunner
    {
        void RunExternalProgram(
            string programExePath,
            string workingDirectory,
            string commandLineFormat,
            params object[] args);        
    }
}