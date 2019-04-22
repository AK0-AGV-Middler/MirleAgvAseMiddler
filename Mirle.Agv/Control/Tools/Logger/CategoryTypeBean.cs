using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Control.Tools.Logger
{
    public class CategoryTypeBean
    {
        private bool bDelOverdueFile = LoggerConstants.DEFAULT_BOOL_DEL_OVER_DUE_FILE;
        private int iFileKeepDay = LoggerConstants.DEFAULT_INT_FILE_KEEP_DAY;
        private int iLogMaxSize = LoggerConstants.DEFAULT_INT_LOG_MAXSIZE;
        private bool bLogEnable = LoggerConstants.DEFAULT_BOOL_LOG_ENABLE;
        private string sLineSeparateToken = LoggerConstants.DEFAULT_STR_LINE_SEPARATE_TOKEN;
        private string sFileExtension = LoggerConstants.DEFAULT_STR_FILE_EXTENSION;

        public String Name { get; set; }
        public String LogFileName { get; set; }
        public String DirName { get; set; }
        public bool DelOverdueFile { get { return bDelOverdueFile; } set { bDelOverdueFile = value; } }
        public int FileKeepDay { get { return iFileKeepDay; } set { iFileKeepDay = value; } }
        public int LogMaxSize { get { return iLogMaxSize; } set { iLogMaxSize = value; } }
        public bool LogEnable { get { return bLogEnable; } set { bLogEnable = value; } }
        public String LineSeparateToken { get { return sLineSeparateToken; } set { sLineSeparateToken = value; } }
        public String FileExtension { get { return sFileExtension; } set { sFileExtension = value; } }

    }
}
