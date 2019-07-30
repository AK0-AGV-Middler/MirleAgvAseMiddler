using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferCmds
{
    public class LoadCmdInfo : TransferStep
    {
        public string LoadAddress { get; set; } = "Empty";
        public int StageNum { get; set; }
        public EnumStageDirection StageDirection { get; set; } = EnumStageDirection.None;
        public bool IsEqPio { get; set; }
        public ushort ForkSpeed { get; set; } = 100;

        public LoadCmdInfo():this(new MapInfo()) { }
        public LoadCmdInfo(MapInfo theMapInfo) : base(theMapInfo)
        {
            type = EnumTransCmdType.Load;
        }
    }
}
