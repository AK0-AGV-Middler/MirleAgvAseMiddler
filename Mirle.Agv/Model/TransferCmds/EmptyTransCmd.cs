using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.TransferCmds
{
    public class EmptyTransCmd : TransCmd
    {
        public EmptyTransCmd() : this(new MapInfo()) { }     
        public EmptyTransCmd(MapInfo theMapInfo) : base(theMapInfo)
        {
            type = EnumTransCmdType.Empty;
        }
    }
}
