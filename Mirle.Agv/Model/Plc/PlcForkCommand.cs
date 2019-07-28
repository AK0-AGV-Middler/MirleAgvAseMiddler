using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;
using Mirle.Agv;

namespace Mirle.Agv.Model
{    
    [Serializable]
    public class PLCForkCommand
    {
        public ushort CommandNo { get;  set; } // 0 ~ 65535
        public EnumForkCommand ForkCommandType { get; set; }
        public EnumStageDirection Direction { get; set; }
        public string StageNo { get; set; } = "1";
        public bool Eqif { get; set; }
        public ushort ForkSpeed { get; set; }
        public string Reason { get; set; } = "";

        public EnumForkCommandState ForkCommandState { get; set; } = EnumForkCommandState.Queue;

        public PLCForkCommand(ushort aCommandNo, EnumForkCommand aEnumForkCommand, string aStageNo, EnumStageDirection aDirection, bool aEqif, ushort aForkSpeed)
        {
            CommandNo = aCommandNo;
            ForkCommandType = aEnumForkCommand;
            StageNo = string.IsNullOrEmpty(aStageNo) ? "1" : aStageNo;            
            Direction = aDirection;
            Eqif = aEqif;
            ForkSpeed = aForkSpeed;
        }
    }
}
