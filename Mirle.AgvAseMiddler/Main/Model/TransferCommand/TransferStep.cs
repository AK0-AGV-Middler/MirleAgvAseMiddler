using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Controller;
using Mirle.AgvAseMiddler.Model.Configs;

namespace Mirle.AgvAseMiddler.Model.TransferSteps
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
