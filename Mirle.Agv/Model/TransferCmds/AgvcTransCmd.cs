using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Control;

namespace Mirle.Agv.Model.TransferCmds
{
    public class AgvcTransCmd
    {
        public EnumAgvcTransCmdType CmdType { get; set; }
        public string[] ToLoadSections { get; set; }
        public string[] ToUnloadSections { get; set; }
        public string LoadAddress { get; set; }
        public string UnloadAddtess { get; set; }
        public string CarrierId { get; set; }
        public string CmdId { get; set; }
    }

}
