using log4net;
using log4net.Core;

namespace GroundTruth.Engine
{
    public interface IConsoleLogger
    {
        void Write(ILog log, log4net.Core.Level level);
        void Write(ILog log, log4net.Core.Level level, string format, params object[] args);
        void WriteLine(ILog log, log4net.Core.Level level);
        void WriteLine(ILog log, log4net.Core.Level level, string format, params object[] args);
    }
}
