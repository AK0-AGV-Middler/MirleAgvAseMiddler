using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller.Tools
{
    public class LogType
    {                   
        public string Name { get; set; }
        public string LogFileName { get; set; }
        public string DirName { get; set; }
        public bool DelOverdueFile { get; set; } = true;
        public int FileKeepDay { get; set; } = 30;
        public int LogMaxSize { get; set; } = 2;
        public bool LogEnable { get; set; } = true;
        public string LineSeparateToken { get; set; } = "$.$";
        public string FileExtension { get; set; } = ".txt";
        public int DequeueInterval { get; set; } = 1000;
    }
}
