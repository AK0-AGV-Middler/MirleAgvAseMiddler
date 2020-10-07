using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Controller;

namespace Mirle.Agv.AseMiddler.Model.TransferSteps
{

    public class EmptyTransferStep : TransferStep
    {
        public EmptyTransferStep() : this("")
        {
        }
        public EmptyTransferStep(string cmdId) : base(cmdId) => type = EnumTransferStepType.Empty;
    }
}
