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
    public class PlcForkCommand
    {
        public ushort CommandNo { get;  set; } // 0 ~ 65535
        public EnumForkCommand ForkCommandType { get; set; }
        public EnumStageDirection Direction { get; set; }
        public string StageNo { get; set; } = "5";
        public bool IsEqPio { get; set; }
        public ushort ForkSpeed { get; set; }
        public string Reason { get; set; } = "";

        public EnumForkCommandState ForkCommandState { get; set; } = EnumForkCommandState.Queue;

        public PlcForkCommand(ushort aCommandNo, EnumForkCommand aEnumForkCommand, string aStageNo, EnumStageDirection aDirection, bool isEqPio, ushort aForkSpeed)
        {
            CommandNo = aCommandNo;
            ForkCommandType = aEnumForkCommand;
            StageNo = string.IsNullOrEmpty(aStageNo) ? "5" : aStageNo;            
            Direction = aDirection;
            IsEqPio = isEqPio;
            ForkSpeed = aForkSpeed;
        }
    }
}
