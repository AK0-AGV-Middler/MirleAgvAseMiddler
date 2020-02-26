using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Controller;
using Mirle.Agv.AseMiddler.Model.Configs;

namespace Mirle.Agv.AseMiddler.Model.TransferSteps
{
    [Serializable]
    public abstract class TransferStep
    {
        protected EnumTransferStepType type;
        public string CmdId { get; set; } = "";

        public TransferStep(string cmdId)
        {
            this.CmdId = cmdId;
        }

        public EnumTransferStepType GetTransferStepType() { return type; }
    }
}
