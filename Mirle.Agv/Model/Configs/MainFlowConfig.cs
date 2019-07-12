using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Configs
{
    public class MainFlowConfig
    {
        public string LogConfigPath { get; set; }
        public int TransCmdsCheckInterval { get; set; }
        public int DoTransCmdsInterval { get; set; }
        public int ReserveLength { get; set; }
        public int AskReserveInterval { get; set; }
    }
}
