using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Control.Tools.Logger;
using Mirle.Agv.Model;
using System.IO;

namespace Mirle.Agv
{
    public class LoggerAgent
    {
        public static string RootDir { get; set; }

        private Dictionary<string, Logger> dicLoggers;

        public static string LogConfigPath { get; set; }

        private static readonly Lazy<LoggerAgent> lazyInstance = new Lazy<LoggerAgent>(() => new LoggerAgent());

        private LoggerAgent()
        {
            dicLoggers = new Dictionary<string, Logger>();
            string fullConfigPath = Path.Combine(RootDir, LogConfigPath);
            List<CategoryTypeBean> listCategory = Logger.ReadLogIniFile(fullConfigPath);
            foreach (CategoryTypeBean bean in listCategory)
            {
                Logger logger = new Logger(bean);
                string logFileName = logger.LogFileName;
                dicLoggers.Add(logFileName, logger);
            }
        }

        public static LoggerAgent Instance { get { return lazyInstance.Value; } }

        public void LogDebug(LogFormat logFormat)
        {
            if (dicLoggers.ContainsKey("Debug"))
            {
                Logger logger = dicLoggers["Debug"];
                logger.SaveLogFile("Debug", logFormat.LogLevel, logFormat.ClassFunctionName, logFormat.Device, logFormat.CarrierId, logFormat.Message);
            }
        }

        public void LogInfo(LogFormat logFormat)
        {
            if (dicLoggers.ContainsKey("Info"))
            {
                Logger logger = dicLoggers["Info"];
                logger.SaveLogFile("Info", logFormat.LogLevel, logFormat.ClassFunctionName, logFormat.Device, logFormat.CarrierId, logFormat.Message);
            }
        }

        public void LogError(LogFormat logFormat)
        {
            if (dicLoggers.ContainsKey("Error"))
            {
                Logger logger = dicLoggers["Error"];
                logger.SaveLogFile("Error", logFormat.LogLevel, logFormat.ClassFunctionName, logFormat.Device, logFormat.CarrierId, logFormat.Message);
            }
        }

        public void LogComm(LogFormat logFormat)
        {
            if (dicLoggers.ContainsKey("Comm"))
            {
                Logger logger = dicLoggers["Comm"];
                logger.SaveLogFile("Comm", logFormat.LogLevel, logFormat.ClassFunctionName, logFormat.Device, logFormat.CarrierId, logFormat.Message);
            }
        }
    }
}
