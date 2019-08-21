using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferCmds
{
    [Serializable]
    public class UnloadCmdInfo : TransferStep
    {
        public string UnloadAddress { get; set; } = "Empty";
        public int StageNum { get; set; }
        public EnumStageDirection StageDirection { get; set; } = EnumStageDirection.None;
        public bool IsEqPio { get; set; }
        public ushort ForkSpeed { get; set; } = 100;

        public UnloadCmdInfo():this(new MapInfo()) { }
        public UnloadCmdInfo(MapInfo theMapInfo) : base(theMapInfo)
        {
            type = EnumTransferCommandType.Unload;
        }
    }
}
