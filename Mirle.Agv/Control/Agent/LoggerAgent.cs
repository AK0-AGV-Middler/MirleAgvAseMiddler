using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mirle.Agv.Control.Tools;


namespace Mirle.Agv.Control
{
    public class LoggerAgent
    {
        private Dictionary<string, Logger> dicLoggers;
        public static string LogConfigPath { get; set; } = "Log.ini";
        private ConfigHandler configHandler;
        private static readonly Lazy<LoggerAgent> lazyInstance = new Lazy<LoggerAgent>(() => new LoggerAgent());
        public static LoggerAgent Instance { get { return lazyInstance.Value; } }

        private LoggerAgent()
        {
            dicLoggers = new Dictionary<string, Logger>();

            LogConfigInitial();
        }

        private void LogConfigInitial()
        {
            string fullConfigPath = Path.Combine(Environment.CurrentDirectory, LogConfigPath);
            configHandler = new ConfigHandler(fullConfigPath);

            // 確認 File 是否存在
            if (!File.Exists(fullConfigPath))
            {
                throw new Exception(string.Concat("File ", fullConfigPath, " is not existed."));
            }

            // 讀取 section = category的資料
            LogBasicConfigs logBasicConfigs = new LogBasicConfigs();
            var sectionName = "Basic";
            logBasicConfigs.Number = Convert.ToInt32(configHandler.GetString(sectionName, "Number", "0"));
            logBasicConfigs.SectionBaseName = configHandler.GetString(sectionName, "SectionBaseName", "LogType");

            if (logBasicConfigs.Number <= 0)
            {
                throw new Exception(string.Concat("Please add Category in iniFile."));
            }

            // 讀取各個 Category 的資料
            for (int i = 1; i < logBasicConfigs.Number + 1; i++)
            {
                string strSectionName = string.Concat(logBasicConfigs.SectionBaseName, i.ToString());

                LogType aLogType = new LogType();
                aLogType.Name = configHandler.GetString(strSectionName, "Name", "Empty");
                aLogType.LogFileName = configHandler.GetString(strSectionName, "LogFileName", "Empty");
                aLogType.DirName = configHandler.GetString(strSectionName, "DirName", "Empty");
                aLogType.DelOverdueFile = configHandler.GetBool(strSectionName, "DelOverdueFile", "True");
                aLogType.FileKeepDay = Convert.ToInt32(configHandler.GetString(strSectionName, "FileKeepDay", "30"));
                aLogType.LogMaxSize = Convert.ToInt32(configHandler.GetString(strSectionName, "LogMaxSize", "2"));
                aLogType.LogEnable = configHandler.GetBool(strSectionName, "LogEnable","True");
                aLogType.LineSeparateToken = configHandler.GetString(strSectionName, "LineSeparateToken", "$.$");
                aLogType.FileExtension = configHandler.GetString(strSectionName, "FileExtension", ".txt");
                aLogType.DequeueInterval = int.Parse(configHandler.GetString("Basic", "DequeueInterval", "1000"));

                Logger logger = new Logger(aLogType);

                dicLoggers.Add(aLogType.Name, logger);
            }
        }       

        public void LogMsg(string type,LogFormat logFormat)
        {
            if (dicLoggers.ContainsKey(type))
            {
                Logger logger = dicLoggers[type];
                logger.SaveLogFile(type, logFormat.LogLevel, logFormat.ClassFunctionName, logFormat.Device, logFormat.CarrierId, logFormat.Message);
            }
        }
    }
}
