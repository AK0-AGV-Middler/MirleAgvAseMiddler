using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Control.Tools.Logger
{
    class LoggerSample
    {

        public void Sample1()
        {
            Logger logReaderLogger = new Logger("LogReader", "LogReader");
            logReaderLogger.SaveLogFile("sCategory", "sLogLevel", "sClassFunctionName", "Device", "CarrierId", "sMessage");
        }

        public void Sample2()
        {
            Logger logReaderLogger = new Logger("LogReader", "LogReader", true, 60, 2, true);
            logReaderLogger.SaveLogFile("{Error message}");
        }

        public void Sample3()
        {
            Logger logReaderLogger = new Logger("LogReader", "LogReader", true, 60, 2, true, "q0.0p");
            logReaderLogger.SaveLogFile("{Error message}");
        }

        public void Sample4()
        {
            Logger logReaderLogger = new Logger("LogReader", "LogReader", true, 60, 2, true, "q0.0p", ".log");
            logReaderLogger.SaveLogFile("{Error message}");
        }
        
        public void Sample5()
        {
            CategoryTypeBean bean = new CategoryTypeBean();
            Logger logReaderLogger = new Logger(bean);
            logReaderLogger.SaveLogFile("{Error message}");
        }

        /// <summary>
        /// 讀取 Log.ini file 再根據裡面資訊建立 Logger
        /// </summary>
        public void Sample6()
        {
            List<CategoryTypeBean> listCategory = Logger.ReadLogIniFile("{Log.ini file path}");
            foreach (CategoryTypeBean bean in listCategory)
            {
                Logger logReaderLogger = new Logger(bean);
                logReaderLogger.SaveLogFile("{Error message}");
            }
        }

        /// <summary>
        /// 如何取得必要參數
        /// </summary>
        public void Sample7()
        {
            CategoryTypeBean bean = new CategoryTypeBean();
            Logger logReaderLogger = new Logger(bean);
            logReaderLogger.SaveLogFile("{Error message}");
            string lineSeparatorToken = logReaderLogger.LINE_SEPARATE_TOKEN;
            string fileExtension = logReaderLogger.FILE_EXTENSION;
        }


    }
}
