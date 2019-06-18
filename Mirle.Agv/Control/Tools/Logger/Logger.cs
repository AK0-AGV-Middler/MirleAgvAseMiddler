using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Mirle.Agv.Control.Tools
{
    public class Logger
    {
        public static readonly long MB = 1024 * 1024;
        public static readonly int LOG_FORMAT_LENGTH = 19;

        public static readonly String LOG_DEBUG = "Debug";

        // Default value
        private LogType logType;

        private string strDirectoryFullPath = "Empty";

        private FileStream fileStream;
        private StreamWriter fileWriteStream;
        private Encoding encodingType = Encoding.UTF8;  // 設定編碼格式字元編碼/解碼 類別

        private string strFileFullPath = "";
        private long lngLogMaxSize;

        private DateTime dtTimeOfOverdueFileCheck = DateTime.Now;

        private static object theWriteLocker = new object();

        private Queue queInputLogData;
        private Queue queOutputLogData;
        private Thread thdDataSave;

        private static FileStream debugFileStream;
        private static StreamWriter debugFileWriteStream;
        private static object theDebugLocker = new object();

        public Logger(LogType aLogType)
        {
            logType = aLogType;

            queInputLogData = Queue.Synchronized(new Queue());
            queOutputLogData = Queue.Synchronized(new Queue());

            thdDataSave = new Thread(ThreadBufferDataSave);
            thdDataSave.IsBackground = true;
            thdDataSave.Name = "ThreadDataSave";
            thdDataSave.Start();

            lngLogMaxSize = logType.LogMaxSize * MB;

            // 應該檢查不合法字元
            PathCheck();
        }

        private void PathCheck()
        {
            CheckPathValid(logType.LogFileName);
            CheckPathValid(logType.DirName);
            strDirectoryFullPath = Path.Combine(Environment.CurrentDirectory, "Log", logType.DirName);
            var saveFullName = logType.LogFileName + logType.FileExtension; // 存檔名稱
            strFileFullPath = Path.Combine(strDirectoryFullPath, saveFullName);        // 要被開啟處理的檔案

            if (!Directory.Exists(strDirectoryFullPath))
            {
                Directory.CreateDirectory(strDirectoryFullPath);
            }

            if (File.Exists(strFileFullPath))
            {
                fileStream = new FileStream(strFileFullPath, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            else
            {
                fileStream = new FileStream(strFileFullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);   // 建立檔案
            }

            fileWriteStream = new StreamWriter(fileStream, encodingType);
        }

        private void AddDebugLog(string sFunctionName, string sMessage)
        {
            lock (theDebugLocker)
            {
                string sDebugLogPath = Path.Combine(Environment.CurrentDirectory, "Log", "LogError" + logType.FileExtension);

                if (!File.Exists(sDebugLogPath))
                {
                    // 建立檔案
                    debugFileStream = new FileStream(sDebugLogPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                }
                else
                {
                    debugFileStream = new FileStream(sDebugLogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                }
                debugFileWriteStream = new StreamWriter(debugFileStream, encodingType);

                sMessage = sMessage + " by " + logType.DirName + @"\" + logType.LogFileName;
                string log = string.Concat(DateTime.Now.ToString("yyyy-MM-dd@HH-mm-ss.fff@"), LOG_DEBUG, "@", sFunctionName,
                    "@", Thread.CurrentThread.Name + "_" + Thread.CurrentThread.GetHashCode().ToString(), "@@@", sMessage, Environment.NewLine);
                debugFileWriteStream.Write(log); // 寫入檔案
                debugFileWriteStream.Flush();
                debugFileStream.Close();
            }
        }

        private void WriteLog(string sMessage)
        {
            lock (theWriteLocker)
            {
                int iStep = 0;
                try
                {
                    fileWriteStream.Write(sMessage);   //  寫入檔案
                    fileWriteStream.Flush();
                    iStep = iStep + 1;

                    var fileSize = new FileInfo(strFileFullPath).Length;
                    if (fileSize > lngLogMaxSize)
                    {
                        // 超過限制的大小，換檔再刪除
                        SpinWait.SpinUntil(() => false, 1000); // 避免產生同時間的檔案
                        var dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                        var copyName = string.Concat(strDirectoryFullPath, logType.LogFileName, "_", dateTime, logType.FileExtension);
                        File.Copy(strFileFullPath, copyName);
                        iStep = iStep + 1;

                        // 清除檔案內容
                        fileWriteStream.Close();
                        fileStream = new FileStream(strFileFullPath, FileMode.Truncate, FileAccess.Write, FileShare.Read);
                        fileWriteStream = new StreamWriter(fileStream, encodingType);

                        iStep = iStep + 1;
                    }

                    if (logType.DelOverdueFile)
                    {
                        if (DateTime.Compare(DateTime.Now, dtTimeOfOverdueFileCheck.AddMinutes(10)) > 0)
                        {
                            CheckOverdueFile();
                            dtTimeOfOverdueFileCheck = DateTime.Now;
                        }
                    }
                    iStep = iStep + 1;

                }
                catch (Exception ex)
                {
                    AddDebugLog("SaveLogFile", sMessage + ex.StackTrace + ", iStep =" + iStep);
                }
            }
        }

        private void SaveLogFile(string sMessage)
        {
            try
            {
                if (logType.LogEnable)
                {
                    sMessage.Replace(Environment.NewLine, logType.LineSeparateToken);
                    queInputLogData.Enqueue(sMessage);
                }
            }
            catch (Exception ex)
            {
                AddDebugLog("SaveLogFile", sMessage + ex.StackTrace);
            }
        }

        public void SaveLogFile(string sCategory, string sLogLevel, string sClassFunctionName, string Device, string CarrierId, string sMessage)
        {
            try
            {
                string str = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "@", sCategory, "@", sLogLevel, "@", sClassFunctionName, "@", Device, "@", CarrierId, "@", sMessage);
                SaveLogFile(str);
            }
            catch (Exception ex)
            {
                AddDebugLog("SaveLogFile", sMessage + ex.StackTrace);
            }
        }

        private void ThreadBufferDataSave()
        {
            while (true)
            {
                try
                {
                    string totalMsg = "";
                    queOutputLogData = queInputLogData;
                    queInputLogData = Queue.Synchronized(new Queue());

                    while (queOutputLogData.Count > 0)
                    {
                        var msg = queOutputLogData.Dequeue().ToString();
                        totalMsg += msg + Environment.NewLine;
                    }

                    if (!string.IsNullOrWhiteSpace(totalMsg))
                    {
                        WriteLog(totalMsg);
                    }

                    SpinWait.SpinUntil(() => false, logType.DequeueInterval);

                }
                catch (Exception ex)
                {
                    AddDebugLog("ThreadDataSave", ex.StackTrace);
                }
            }

        }

        private void ThreadAsyncDataSave()
        {
            while (true)
            {
                try
                {
                    string sLog = queInputLogData.Dequeue().ToString();
                    if (null != sLog)
                    {
                        WriteLog(sLog);
                    }
                    else
                    {
                        SpinWait.SpinUntil(() => false, 10);
                    }
                }
                catch (Exception ex)
                {
                    AddDebugLog("ThreadDataSave", ex.StackTrace);
                }
            }
        }

        private void CheckOverdueFile()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(strDirectoryFullPath);
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
                                string sFilePath = Path.Combine(strDirectoryFullPath, fileName);
                                if (File.Exists(sFilePath))
                                    File.Delete(sFilePath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddDebugLog("CheckOverdueFile", ex.StackTrace);
            }
        }

        //Replace VB Datediff function
        private int DayDiff(DateTime startDate, DateTime endDate)
        {
            TimeSpan TS = new TimeSpan(endDate.Ticks - startDate.Ticks);
            return Convert.ToInt32(TS.TotalDays);
        }


        #region CheckPathValid() 判斷路徑或檔名是是否有不合法的字元

        public void CheckPathValid(string path)
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

        #endregion
    }
}
