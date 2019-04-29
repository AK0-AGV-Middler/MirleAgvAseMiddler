using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.TransferCmds
{
    public class EmptyTransCmd : TransCmd
    {
        public EmptyTransCmd() : base()
        {
            type = EnumTransCmdType.Empty;
        }
    }
}
