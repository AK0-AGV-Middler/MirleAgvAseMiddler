using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferCmds
{
   public class LoadCmdInfo : TransCmd
    {
        public string LoadAddress { get; set; } = "Empty";
        public int StageNum { get; set; }

        public LoadCmdInfo() : base()
        {
            type = EnumTransCmdType.Load;
        }
    }
}
