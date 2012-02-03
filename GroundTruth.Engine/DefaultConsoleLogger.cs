using System;
using System.Globalization;
using log4net;
using log4net.Core;

namespace GroundTruth.Engine
{
    public class DefaultConsoleLogger : IConsoleLogger
    {
        public void Write (ILog log, Level level)
        {
            Write(log, level, String.Empty);    
        }

        public void Write (ILog log, Level level, string format, params object[] args)
        {
            if (level >= Level.Error)
                Console.Error.Write(format, args);
            else if (level >= Level.Debug)
                Console.Out.Write(format, args);

            log.Logger.Log(typeof(DefaultConsoleLogger), level, string.Format(CultureInfo.InvariantCulture, format, args), null);
        }

        public void WriteLine (ILog log, Level level)
        {
            WriteLine (log, level, String.Empty);
        }

        public void WriteLine (ILog log, Level level, string format, params object[] args)
        {
            if (level >= Level.Error)
                Console.Error.WriteLine (format, args);
            else if (level >= Level.Debug)
                Console.Out.WriteLine (format, args);

            log.Logger.Log (typeof (DefaultConsoleLogger), level, string.Format (CultureInfo.InvariantCulture, format, args), null);
        }
    }
}