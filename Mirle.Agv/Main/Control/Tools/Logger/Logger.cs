using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace Mirle.Agv.Controller.Tools
{
    [Serializable]
    public class Logger
    {
        public static readonly long MB = 1024 * 1024;
        public static readonly int LOG_FORMAT_LENGTH = 19;

        // Default value
        private LogType logType;

        private string directoryFullPath = "Empty";
        //private string firstLineString = "";

        //private FileStream fileStream;
        //private StreamWriter fileWriteStream;
        //private Encoding encodingType = Encoding.UTF8;  // 設定編碼格式字元編碼/解碼 類別

        private string logFileFullPath = "";
        private long lngLogMaxSize = 0;

        private DateTime dtTimeOfOverdueFileCheck = DateTime.Now;

        //private static object theWriteLocker = new object();

        private ConcurrentQueue<string> queInputLogData;
        private ConcurrentQueue<string> queOutputLogData;
        private Thread thdDataSave;

        /// <summary>
        /// New a logger by log type
        /// </summary>
        /// <param name="aLogType"></param>
        public Logger(LogType aLogType)
        {
            try
            {
                logType = aLogType;

                queInputLogData = new ConcurrentQueue<string>();
                queOutputLogData = new ConcurrentQueue<string>();

                thdDataSave = new Thread(ThreadBufferDataSave);
                thdDataSave.IsBackground = true;
                thdDataSave.Name = "ThreadDataSave";
                thdDataSave.Start();

                lngLogMaxSize = logType.LogMaxSize * MB;
                // 應該檢查不合法字元
                MakeSurePathExist();
            }
            catch (Exception ex)
            {
                ExceptionLog("Logger", ex.StackTrace);
            }
        }

        private void MakeSurePathExist()
        {
            MakeSurePathValid(logType.LogFileName);
            MakeSurePathValid(logType.DirName);
            directoryFullPath = Path.Combine(Environment.CurrentDirectory, "Log", logType.DirName);
            var saveFullName = string.Concat(logType.LogFileName, logType.FileExtension); // 存檔名稱
            logFileFullPath = Path.Combine(directoryFullPath, saveFullName);        // 要被開啟處理的檔案

            if (!Directory.Exists(directoryFullPath))
            {
                Directory.CreateDirectory(directoryFullPath);
            }

            if (!File.Exists(logFileFullPath))
            {
                File.Create(logFileFullPath);
                if (!string.IsNullOrWhiteSpace(logType.FirstLineString))
                {
                    WriteFirstLine();
                }
            }
        }

        private void WriteFirstLine()
        {
            using (FileStream stream = new FileStream(logFileFullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8))
                {
                    sw.WriteLine(logType.FirstLineString);
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// Save this class exceptions
        /// </summary>
        /// <param name="aFunctionName"></param>
        /// <param name="aMessage"></param>
        public void ExceptionLog(string aFunctionName, string aMessage)
        {
            try
            {
                using (FileStream stream = new FileStream("LoggerException.txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Concat(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), "\t", aFunctionName, "\t", aMessage));
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        #region Enqueue

        /// <summary>
        /// Classical log method
        /// </summary>
        /// <param name="logFormat"></param>
        public void Log(LogFormat logFormat)
        {
            try
            {
                LogToQueue(logFormat.GetString());
            }
            catch (Exception ex)
            {
                ExceptionLog("SaveLogFile", string.Concat(logFormat.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Simplely log some string message
        /// </summary>
        /// <param name="aMessage"></param>
        public void LogString(string aMessage)
        {
            try
            {
                LogToQueue(aMessage);
            }
            catch (Exception ex)
            {
                ExceptionLog("SavePureLog", string.Concat(aMessage, ex.StackTrace));
            }
        }

        /// <summary>
        /// Simplely log some string message with some class and method name
        /// </summary>
        /// <param name="classMethodName"></param>
        /// <param name="aMessage"></param>
        public void LogString(string classMethodName, string aMessage)
        {
            try
            {
                LogToQueue(string.Concat(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), ",\t", classMethodName, ",\t", aMessage));
            }
            catch (Exception ex)
            {
                ExceptionLog("SavePureLog", string.Concat(aMessage, ex.StackTrace));
            }
        }

        private void LogToQueue(string aMessage)
        {
            try
            {
                if (logType.LogEnable)
                {
                    aMessage.Replace(Environment.NewLine, logType.LineSeparateToken);
                    queInputLogData.Enqueue(aMessage);
                }
            }
            catch (Exception ex)
            {
                ExceptionLog("SaveLogFile", aMessage + ex.StackTrace);
            }
        }

        #endregion

        #region Dequeue

        private void ThreadBufferDataSave()
        {
            while (true)
            {
                try
                {
                    //string totalMsg = "";
                    queOutputLogData = queInputLogData;
                    //queInputLogData = Queue.Synchronized(new Queue());
                    queInputLogData = new ConcurrentQueue<string>();

                    while (queOutputLogData.Count > 0)
                    {
                        //var msg = queOutputLogData.Dequeue().ToString();
                        if (queOutputLogData.TryDequeue(out string msg))
                        {
                            WriteLog(msg);
                            CheckFileSize();
                        }
                        //totalMsg = string.Concat(totalMsg, msg, Environment.NewLine);
                    }

                    CheckOverdueDate();
                    //if (!string.IsNullOrWhiteSpace(totalMsg))
                    //{
                    //    WriteLog(totalMsg);
                    //}

                    SpinWait.SpinUntil(() => false, logType.DequeueInterval);
                }
                catch (Exception ex)
                {
                    ExceptionLog("ThreadDataSave", ex.StackTrace);
                    SpinWait.SpinUntil(() => false, logType.DequeueInterval);
                }
            }
        }

        private void WriteLog(string aMessage)
        {
            #region 1.0
            //lock (theWriteLocker)
            //{
            //    int iStep = 0;
            //    try
            //    {
            //        fileWriteStream.Write(aMessage);   //  寫入檔案
            //        fileWriteStream.Flush();
            //        iStep = iStep + 1;

            //        var fileSize = new FileInfo(logFileFullPath).Length;
            //        if (fileSize > lngLogMaxSize)
            //        {
            //            // 超過限制的大小，換檔再刪除
            //            SpinWait.SpinUntil(() => false, 1000); // 避免產生同時間的檔案
            //            var dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            //            var copyName = string.Concat(logType.LogFileName, "_", dateTime, logType.FileExtension);
            //            copyName = Path.Combine(directoryFullPath, copyName);
            //            File.Copy(logFileFullPath, copyName);
            //            iStep = iStep + 1;

            //            // 清除檔案內容
            //            fileWriteStream.Close();
            //            fileStream = new FileStream(logFileFullPath, FileMode.Truncate, FileAccess.Write, FileShare.Read);
            //            fileWriteStream = new StreamWriter(fileStream, encodingType);
            //            fileWriteStream.Write(firstLineString + Environment.NewLine);
            //            fileWriteStream.Flush();

            //            iStep = iStep + 1;
            //        }

            //        if (logType.DelOverdueFile)
            //        {
            //            if (DateTime.Compare(DateTime.Now, dtTimeOfOverdueFileCheck.AddMinutes(10)) > 0)
            //            {
            //                CheckOverdueFile();
            //                dtTimeOfOverdueFileCheck = DateTime.Now;
            //            }
            //        }
            //        iStep = iStep + 1;

            //    }
            //    catch (Exception ex)
            //    {
            //        ExceptionLog("SaveLogFile", aMessage + ex.StackTrace + ", iStep =" + iStep);
            //    }
            //}
            #endregion

            #region 2.0
            using (FileStream stream = new FileStream(logFileFullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8))
                {
                    sw.WriteLine(aMessage);
                    sw.Flush();
                    sw.Close();
                }
            }
            #endregion
        }

        private void CheckFileSize()
        {
            var fileSize = new FileInfo(logFileFullPath).Length;
            if (fileSize > lngLogMaxSize)
            {
                // 超過限制的大小，換檔再刪除
                SpinWait.SpinUntil(() => false, 1000); // 避免產生同時間的檔案
                var dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var copyName = string.Concat(logType.LogFileName, "_", dateTime, logType.FileExtension);
                copyName = Path.Combine(directoryFullPath, copyName);
                File.Copy(logFileFullPath, copyName);

                // 清除檔案內容                
                FileStream stream = new FileStream(logFileFullPath, FileMode.Create);
                stream.Close();
                //fileWriteStream.Close();
                //fileStream = new FileStream(logFileFullPath, FileMode.Truncate, FileAccess.Write, FileShare.Read);
                //fileWriteStream = new StreamWriter(fileStream, encodingType);
                //fileWriteStream.Write(firstLineString + Environment.NewLine);
                //fileWriteStream.Flush();
            }
        }

        private void CheckOverdueFile()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directoryFullPath);
                FileInfo[] allFiles = dirInfo.GetFiles();

                foreach (FileInfo fileInfo in allFiles)
                {
                    string fileName = fileInfo.Name;
                    int startPos = fileName.IndexOf("_", 0);
                    int endPos = fileName.IndexOf(logType.FileExtension, 0);
                    if (startPos != 0 && endPos != 0)
                    {
                        string fileDateTime = fileName.Substring(startPos + 1, (endPos - startPos) - 1);
                        if (fileDateTime.Length == LOG_FORMAT_LENGTH)
                        {
                            DateTime fileDate = DateTime.ParseExact(fileDateTime, "yyyy-MM-dd_HH-mm-ss", null);

                            if (DayDiff(fileDate, DateTime.Now) > logType.FileKeepDay)
                            {
                                string sFilePath = Path.Combine(directoryFullPath, fileName);
                                if (File.Exists(sFilePath))
                                    File.Delete(sFilePath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLog("CheckOverdueFile", ex.StackTrace);
            }
        }

        private void CheckOverdueDate()
        {
            if (logType.DelOverdueFile)
            {
                if (DateTime.Compare(DateTime.Now, dtTimeOfOverdueFileCheck.AddMinutes(10)) > 0)
                {
                    CheckOverdueFile();
                    dtTimeOfOverdueFileCheck = DateTime.Now;
                }
            }
        }

        //Replace VB Datediff function
        private int DayDiff(DateTime startDate, DateTime endDate)
        {
            TimeSpan TS = new TimeSpan(endDate.Ticks - startDate.Ticks);
            return Convert.ToInt32(TS.TotalDays);
        }

        #endregion

        /// <summary>
        /// Get direction name of this logger
        /// </summary>
        /// <returns></returns>
        public string GetLogTypeName()
        {
            return logType.Name;
        }

        /// <summary>
        /// 判斷路徑或檔名是是否有不合法的字元
        /// </summary>
        /// <param name="path"></param>
        public void MakeSurePathValid(string path)
        {

            char[] errorChar = new char[] { ',', '>', '<', '-', '!', '~' };

            // 判斷是否傳入值為空
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "Empty";
            }

            foreach (char badChar in Path.GetInvalidPathChars())
            {
                if (path.IndexOf(badChar) > -1)
                    path = "HasInvalidCharInPath";
            }

            foreach (char badChar in errorChar)
            {
                if (path.IndexOf(badChar) > -1)
                    // MessageBox.Show("名稱中有不合法的字元")
                    path = "HasErrorCharInPath";
            }
        }
    }
}
