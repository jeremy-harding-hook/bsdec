using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using static System.Environment;

namespace BsdecCore
{
    public static class Logging
    {
        public readonly static Logger Log = LogManager.GetCurrentClassLogger();
        public static void FlushLogs()
        {
            LogManager.Shutdown();
        }

        static Logging()
        {
            LoggingConfiguration config = new();

            // Targets where to log to: just the file (no console available)
            string logfilePath;
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            int uniqueFileNumber = 0;
            do
            {
                string fileAppendix = uniqueFileNumber == 0 ? "" : $"_{uniqueFileNumber:D3}";
                logfilePath = Path.Combine(
                GetFolderPath(SpecialFolder.ApplicationData),
#if RELEASELINUX
                "bsdec",
                "logs",
#else
                "Bsdec",
                "Logs",
#endif
                $"bsdec-gui-log-{currentDateTime}{fileAppendix}.log"
                );
                uniqueFileNumber ++;
            } while (Path.Exists(logfilePath));

            FileTarget logfileTarget = new("logfile")
            {
                FileName = logfilePath,
                Layout = "${longdate}\t[${ThreadId}]  \t${level:uppercase=true}\t${message:withexception=true:exceptionSeparator=\n\t}"
            };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfileTarget);

            // Apply config           
            LogManager.Configuration = config;
        }
    }
}