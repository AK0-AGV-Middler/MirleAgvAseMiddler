using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mirle.Agv.Control.Tools;

namespace Mirle.Agv.Control.Tools.Logger
{
    public class Logger
    {
        public static readonly long MB = 1024 * 1024;
        public static readonly int LOG_COUNTER_LIMITER = 0;
        public static readonly int LOG_FORMAT_LENGTH = 19;

        public static readonly String LOG_DEBUG = "Debug";

        // Default value

        private String _LINE_SEPARATE_TOKEN = "$.$";
        private String _FILE_EXTENSION = ".txt";

        private String mbrStrPath = "";
        private String mbrStrFileName = "";
        private String mbrStrDirName = "";

        private int mbrIntFileMaxSize;
        private bool mbrBolDeleteOverdueFile = false;
        private int mbrIntFileKeepDay;
        private bool mbrLogEnable = false;

        private int mbrIntLogLevel;

        private Thread tCheckOverdueFile;

        private FileStream mbrObjFileStream;
        private StreamWriter mbrObjFileWriteStream;
        private Encoding mbrObjStreamEncoding = Encoding.UTF8;  // 設定編碼格式字元編碼/解碼 類別

        private StringBuilder mbrObjStringBuilder = new StringBuilder();

        private int mbrIntLogCounter = 0;

        private String mbrStrSaveFileName = "";
        private String mbrStrSaveFilePath = "";

        private long mbrLngFileSize;
        private long mbrLngLogMaxSize;
        private String mbrStrDateTime;
        private String mbrStrCopyName;

        private String aDateFormat;
        private String aTimeFormat;
        private DateTime dtTimeOfOverdueFileCheck;

        private static Object aWriteObject = new Object();

        private Queue aSyncLogDataQueue;
        private Thread tDataSave;

        private static FileStream aDebugFileStream;
        private static StreamWriter aDebugFileWriteStream;
        private static Object aDebugLockObject = new Object();

        public String LINE_SEPARATE_TOKEN { get { return _LINE_SEPARATE_TOKEN; } }
        public String FILE_EXTENSION { get { return _FILE_EXTENSION; } }

        public bool LogEnable
        {
            get { return this.mbrLogEnable; }
            set { this.mbrLogEnable = value; }
        }
        public int LogLevel
        {
            get { return this.mbrIntLogLevel; }
            set { this.mbrIntLogLevel = value; }
        }
        public String LogFileName
        {
            get { return this.mbrStrFileName; }
        }
        public int FileKeepDay
        {
            get { return this.mbrIntFileKeepDay; }
            set { this.mbrIntFileKeepDay = value; }
        }
        public bool DeleteOverdueFile
        {
            get { return this.mbrBolDeleteOverdueFile; }
            set { this.mbrBolDeleteOverdueFile = value; }
        }
        public long LogMaxSize
        {
            get { return this.mbrLngLogMaxSize; }
            set => this.mbrLngLogMaxSize = value * MB;
        }

        public void SetConfiguration(String dateFormat, String timeFormat)
        {
            this.aDateFormat = dateFormat;
            this.aTimeFormat = timeFormat;
        }

        private void AddDebugLog(String sFunctionName, String sMessage)
        {
            lock (aDebugLockObject)
            {
                String sDebugLogPath = System.Environment.CurrentDirectory + @"\Log\Debug" + FILE_EXTENSION;
                if (!File.Exists(sDebugLogPath))
                {
                    // 建立檔案
                    aDebugFileStream = new FileStream(sDebugLogPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                }
                else
                {
                    aDebugFileStream = new FileStream(sDebugLogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                }
                aDebugFileWriteStream = new StreamWriter(aDebugFileStream, mbrObjStreamEncoding);

                sMessage = sMessage + " by " + this.mbrStrDirName + @"\" + this.mbrStrFileName;
                String log = String.Concat(DateTime.Now.ToString("yyyy-MM-dd@HH-mm-ss.fff@"), LOG_DEBUG, "@", sFunctionName,
                    "@", Thread.CurrentThread.Name + "_" + Thread.CurrentThread.GetHashCode().ToString(), "@@@", sMessage, Environment.NewLine);
                aDebugFileWriteStream.Write(log); // 寫入檔案
                aDebugFileStream.Close();
            }
        }

        private void WriteLog(String sMessage)
        {
            lock (aWriteObject)
            {
                int iStep = 0;
                try
                {
                    sMessage = sMessage + Environment.NewLine;
                    mbrObjFileWriteStream.Write(sMessage);   //  寫入檔案
                    mbrObjFileWriteStream.Flush();
                    iStep = iStep + 1;

                    mbrLngFileSize = new FileInfo(mbrStrSaveFilePath).Length;
                    if (mbrLngFileSize > mbrLngLogMaxSize)
                    {
                        // 超過限制的大小，換檔再刪除
                        SpinWait.SpinUntil(() => false, 1000); // 避免產生同時間的檔案
                        mbrStrDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                        mbrStrCopyName = String.Concat(mbrStrPath, mbrStrFileName, "_", mbrStrDateTime, FILE_EXTENSION);
                        File.Copy(mbrStrSaveFilePath, mbrStrCopyName);
                        iStep = iStep + 1;

                        // 清除檔案內容
                        mbrObjFileWriteStream.Close();
                        mbrObjFileStream = new FileStream(mbrStrSaveFilePath, FileMode.Truncate, FileAccess.Write, FileShare.Read);
                        mbrObjFileWriteStream = new StreamWriter(mbrObjFileStream, mbrObjStreamEncoding);

                        iStep = iStep + 1;
                    }

                    if (mbrBolDeleteOverdueFile)
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

        public void SaveLogFile(String sMessage)
        {
            try
            {
                if (mbrLogEnable)
                {
                    sMessage.Replace(Environment.NewLine, LINE_SEPARATE_TOKEN);
                    this.aSyncLogDataQueue.Enqueue(sMessage);
                }
            }
            catch (Exception ex)
            {
                AddDebugLog("SaveLogFile", sMessage + ex.StackTrace);
            }
        }

        public void SaveLogFile(String sCategory, String sLogLevel, String sClassFunctionName, String Device, String CarrierId, String sMessage)
        {
            try
            {
                String str = String.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "@", sCategory, "@", sLogLevel, "@", sClassFunctionName, "@", Device, "@", CarrierId, "@", sMessage);
                SaveLogFile(str);
            }
            catch (Exception ex)
            {
                AddDebugLog("SaveLogFile", sMessage + ex.StackTrace);
            }
        }

        private void ThreadDataSave()
        {
            while (true)
            {
                try
                {
                    if (this.aSyncLogDataQueue.Count > 0)
                    {
                        String sLog = aSyncLogDataQueue.Dequeue().ToString();
                        if (null != sLog)
                        {
                            WriteLog(sLog);
                        }
                        else
                        {
                            AddDebugLog("ThreadDataSave", "sLog is Nothing.");
                        }

                    }

                    SpinWait.SpinUntil(() => false, 10);
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
                DirectoryInfo dirInfo = new DirectoryInfo(this.mbrStrPath);
                String sDirName = dirInfo.Name;
                FileInfo[] allFiles = dirInfo.GetFiles();

                foreach (FileInfo fiTemp in allFiles)
                {
                    String sFileName = fiTemp.Name;
                    int iStartPos = sFileName.IndexOf("_", 0);
                    int iEndPos = sFileName.IndexOf(FILE_EXTENSION, 0);
                    if (iStartPos != 0 && iEndPos != 0)
                    {
                        String sFileDatetime = sFileName.Substring(iStartPos + 1, (iEndPos - iStartPos) - 1);
                        if (sFileDatetime.Length == LOG_FORMAT_LENGTH)
                        {
                            DateTime fileDate = DateTime.ParseExact(sFileDatetime, "yyyy-MM-dd_HH-mm-ss", null);

                            if (dateDiff("D", fileDate, DateTime.Now) > System.Convert.ToInt64(mbrIntFileKeepDay))
                            {
                                string sFilePath = string.Concat(this.mbrStrPath, sFileName);
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

        /** 
         * Replace VB Datediff function
         * 
        */
        private double dateDiff(string dateInterval, System.DateTime startDate, System.DateTime endDate)
        {
            double diff = 0;
            System.TimeSpan TS = new System.TimeSpan(endDate.Ticks - startDate.Ticks);

            switch (dateInterval.ToLower())
            {
                case "year":
                    diff = Convert.ToDouble(TS.TotalDays / 365);
                    break;
                case "m":
                    diff = Convert.ToDouble((TS.TotalDays / 365) * 12);
                    break;
                case "d":
                    diff = Convert.ToDouble(TS.TotalDays);
                    break;
                case "hour":
                    diff = Convert.ToDouble(TS.TotalHours);
                    break;
                case "minute":
                    diff = Convert.ToDouble(TS.TotalMinutes);
                    break;
                case "second":
                    diff = Convert.ToDouble(TS.TotalSeconds);
                    break;
            }

            return diff;
        }

        public Logger(string sIniFilePath)
        {
            if (String.IsNullOrWhiteSpace(sIniFilePath))
            {
                throw new Exception("The path should not be null, empty, or white space.");
            }

        }

        public Logger(CategoryTypeBean bean)
        {
            LogMain(bean.LogFileName, bean.DirName, bean.DelOverdueFile, bean.FileKeepDay, bean.LogMaxSize, bean.LogEnable, bean.LineSeparateToken, bean.FileExtension);
        }

        public Logger(string sLogName, string sDirName)
        {
            LogMain(sLogName, sDirName, LoggerConstants.DEFAULT_BOOL_DEL_OVER_DUE_FILE, LoggerConstants.DEFAULT_INT_FILE_KEEP_DAY, LoggerConstants.DEFAULT_INT_LOG_MAXSIZE, LoggerConstants.DEFAULT_BOOL_LOG_ENABLE, LoggerConstants.DEFAULT_STR_LINE_SEPARATE_TOKEN, LoggerConstants.DEFAULT_STR_FILE_EXTENSION);
        }

        public Logger(string sLogName, string sDirName, bool bDelOverdueFile, int iFileKeepDay, int iLogMaxSize, bool bLogEnable)
        {
            LogMain(sLogName, sDirName, bDelOverdueFile, iFileKeepDay, iLogMaxSize, bLogEnable, LoggerConstants.DEFAULT_STR_LINE_SEPARATE_TOKEN, LoggerConstants.DEFAULT_STR_FILE_EXTENSION);
        }

        public Logger(string sLogName, string sDirName, bool bDelOverdueFile, int iFileKeepDay, int iLogMaxSize, bool bLogEnable, string sLineSeparateToken)
        {
            LogMain(sLogName, sDirName, bDelOverdueFile, iFileKeepDay, iLogMaxSize, bLogEnable, sLineSeparateToken, LoggerConstants.DEFAULT_STR_FILE_EXTENSION);
        }

        public Logger(string sLogName, string sDirName, bool bDelOverdueFile, int iFileKeepDay, int iLogMaxSize, bool bLogEnable, string sLineSeparateToken, string sFileExtension)
        {
            LogMain(sLogName, sDirName, bDelOverdueFile, iFileKeepDay, iLogMaxSize, bLogEnable, sLineSeparateToken, sFileExtension);
        }

        private void LogMain(string sLogName, string sDirName, bool bDelOverdueFile, int iFileKeepDay, int iLogMaxSize, bool bLogEnable, string sLineSeparateToken, string sFileExtension)
        {
            this._LINE_SEPARATE_TOKEN = null == sLineSeparateToken ? this._LINE_SEPARATE_TOKEN : sLineSeparateToken;
            this._FILE_EXTENSION = null == sFileExtension ? this._FILE_EXTENSION : sFileExtension;

            this.aSyncLogDataQueue = Queue.Synchronized(new Queue());

            tDataSave = new Thread(ThreadDataSave);
            tDataSave.IsBackground = true;
            tDataSave.Name = "ThreadDataSave";
            tDataSave.Start();

            this.dtTimeOfOverdueFileCheck = DateTime.Now;
            aDateFormat = "yyyy-MM-dd";
            aTimeFormat = "HH-mm-ss.fff";

            // check input parameter
            this.mbrIntFileKeepDay = iFileKeepDay;
            this.mbrIntFileMaxSize = iLogMaxSize;
            this.mbrLogEnable = bLogEnable;
            this.mbrBolDeleteOverdueFile = bDelOverdueFile;

            mbrLngLogMaxSize = mbrIntFileMaxSize * MB;

            // 應該檢查不合法字元
            if (CheckNameOrPath(sLogName) && CheckNameOrPath(sDirName))
            {
                mbrStrFileName = sLogName;
                mbrStrDirName = sDirName;
                var path = @"D:\CsProject\Mirle.Agv\Mirle.Agv\bin\Debug\";
                if (Directory.Exists(path))
                {
                    mbrStrPath = string.Concat(path, @"\Log\", sDirName, @"\");
                }
                else
                {
                    mbrStrPath = string.Concat(Directory.GetCurrentDirectory(), @"\Log\", sDirName, @"\");
                }

                mbrStrSaveFileName = string.Concat(sLogName, FILE_EXTENSION);              // 存檔名稱
                mbrStrSaveFilePath = string.Concat(mbrStrPath, mbrStrSaveFileName);        // 要被開啟處理的檔案

                if (File.Exists(mbrStrSaveFilePath) == false)
                {
                    // 檔案不存在
                    if (Directory.Exists(mbrStrPath) == false)
                        // 資料夾不存在
                        Directory.CreateDirectory(mbrStrPath);
                }

                try // V0.0.1 added
                {
                    if (File.Exists(mbrStrSaveFilePath) == false)
                        mbrObjFileStream = new FileStream(mbrStrSaveFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);   // 建立檔案
                    else
                        mbrObjFileStream = new FileStream(mbrStrSaveFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    mbrObjFileWriteStream = new StreamWriter(mbrObjFileStream, mbrObjStreamEncoding);
                }
                // V0.0.1 added start
                catch (Exception ex)
                {
                    if (ex is IOException && IsFileLocked(ex))
                    {
                    }
                }
            }
        }

        private static bool IsFileLocked(Exception exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }

        #region CheckNameOrPath() 判斷路徑或檔名是是否有不合法的字元
        public static bool CheckNameOrPath(string StrNameOrPath)
        {
            char[] arrChar = new char[] { ',', '>', '<', '-', '!', '~' };

            // 判斷是否傳入值為空
            if (StrNameOrPath == null)
                // MessageBox.Show("Unit 的名稱不得為空")
                return false;

            // 
            foreach (char badChar in System.IO.Path.GetInvalidPathChars())
            {
                if (StrNameOrPath.IndexOf(badChar) > -1)
                    // MessageBox.Show("名稱中有不合法的字元")
                    return false;
            }

            foreach (char badChar in arrChar)
            {
                if (StrNameOrPath.IndexOf(badChar) > -1)
                    // MessageBox.Show("名稱中有不合法的字元")
                    return false;
            }
            return true;
        }
        #endregion

        public static List<CategoryTypeBean> ReadLogIniFile(String sIniFilePath)
        {
            string strSectionName = "";
            List<CategoryTypeBean> listCategoryTypeBean = new List<CategoryTypeBean>();
            ConfigHandler configHandler;
            CategoryBean categoryBean;
            CategoryTypeBean categoryTypeBean;

            // 確認 File 是否存在
            if (!File.Exists(sIniFilePath))
            {
                throw new Exception(String.Concat("File ", sIniFilePath, " is not existed."));
            }


            // 讀取 section = category的資料
            //configHandler = new ConfigHandler(ref sIniFilePath, ref LoggerConstants.INIFILE_CATEGORY);
            configHandler = new ConfigHandler(sIniFilePath);
            categoryBean = new CategoryBean();
            categoryBean.Number = Convert.ToInt32(configHandler.GetString(LoggerConstants.INIFILE_CATEGORY, LoggerConstants.INIFILE_CATEGORY_NUMBER, categoryBean.Number.ToString()));
            categoryBean.SectionBaseName = configHandler.GetString(LoggerConstants.INIFILE_CATEGORY, LoggerConstants.INIFILE_CATEGORY_SCETION_BASE_NAME, categoryBean.SectionBaseName);

            if (categoryBean.Number == 0)
            {
                throw new Exception(String.Concat("Please add Category in iniFile."));
            }

            // 讀取各個 Category 的資料
            for (int i = 1; i < categoryBean.Number + 1; i++)
            {
                strSectionName = String.Concat(categoryBean.SectionBaseName, i.ToString());
                //configHandler = new ConfigHandler(ref sIniFilePath, ref strSectionName);
                configHandler = new ConfigHandler(sIniFilePath);


                categoryTypeBean = new CategoryTypeBean();
                categoryTypeBean.Name = configHandler.GetString(strSectionName, LoggerConstants.INIFILE_CATEGORYTYPE_NAME, "");
                categoryTypeBean.LogFileName = configHandler.GetString(strSectionName, LoggerConstants.INIFILE_CATEGORYTYPE_LOG_FILE_NAME, "");
                categoryTypeBean.DirName = configHandler.GetString(strSectionName, LoggerConstants.INIFILE_CATEGORYTYPE_DIR_NAME, "");
                categoryTypeBean.DelOverdueFile = Convert.ToBoolean(configHandler.GetString(strSectionName, LoggerConstants.INIFILE_CATEGORYTYPE_DEL_OVER_DUE_FILE, LoggerConstants.DEFAULT_BOOL_DEL_OVER_DUE_FILE.ToString()));
                categoryTypeBean.FileKeepDay = Convert.ToInt32(configHandler.GetString(strSectionName, LoggerConstants.INIFILE_CATEGORYTYPE_FILE_KEEP_DAY, LoggerConstants.DEFAULT_INT_FILE_KEEP_DAY.ToString())); ;
                categoryTypeBean.LogMaxSize = Convert.ToInt32(configHandler.GetString(strSectionName, LoggerConstants.INIFILE_CATEGORYTYPE_LOG_MAXSIZE, LoggerConstants.DEFAULT_INT_LOG_MAXSIZE.ToString()));
                categoryTypeBean.LogEnable = Convert.ToBoolean(configHandler.GetString(strSectionName, LoggerConstants.INIFILE_CATEGORYTYPE_LOG_ENABLE, LoggerConstants.DEFAULT_BOOL_LOG_ENABLE.ToString()));
                categoryTypeBean.LineSeparateToken = configHandler.GetString(strSectionName, LoggerConstants.INIFILE_CATEGORYTYPE_LINE_SEPARATE_TOKEN, LoggerConstants.DEFAULT_STR_LINE_SEPARATE_TOKEN);
                categoryTypeBean.FileExtension = configHandler.GetString(strSectionName, LoggerConstants.INIFILE_CATEGORYTYPE_FILE_EXTENSION, LoggerConstants.DEFAULT_STR_FILE_EXTENSION);

                listCategoryTypeBean.Add(categoryTypeBean);

            }



            return listCategoryTypeBean;

        }

    }
}
