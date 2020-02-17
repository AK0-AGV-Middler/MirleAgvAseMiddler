using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Controller;

namespace Mirle.AgvAseMiddler.Model.TransferSteps
{
    [Serializable]
    public class UnloadCmdInfo : TransferStep
    {
        public string UnloadAddress { get; set; } = "";
        public int StageNum { get; set; }
        public EnumStageDirection StageDirection { get; set; } = EnumStageDirection.None;
        public bool IsEqPio { get; set; }
        public ushort ForkSpeed { get; set; } = 100;

        public UnloadCmdInfo():this(new MainFlowHandler()) { }
        public UnloadCmdInfo(MainFlowHandler mainFlowHandler) : base(mainFlowHandler)
        {
            type = EnumTransferStepType.Unload;
        }
    }
}
