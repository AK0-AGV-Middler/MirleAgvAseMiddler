using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferSteps
{
    [Serializable]
    public class LoadCmdInfo : TransferStep
    {
        public string LoadAddress { get; set; } = "";
        public int StageNum { get; set; }
        public EnumStageDirection StageDirection { get; set; } = EnumStageDirection.None;
        public bool IsEqPio { get; set; }
        public ushort ForkSpeed { get; set; } = 100;

        public LoadCmdInfo():this(new MainFlowHandler()) { }
        public LoadCmdInfo(MainFlowHandler mainFlowHandler) : base(mainFlowHandler)
        {
            type = EnumTransferStepType.Load;
        }
    }
}
