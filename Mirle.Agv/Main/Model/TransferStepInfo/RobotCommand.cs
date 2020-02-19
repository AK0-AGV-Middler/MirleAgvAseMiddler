using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Controller;

namespace Mirle.AgvAseMiddler.Model.TransferSteps
{
    public class RobotCommand : TransferStep
    {
        public string PortAddress { get; set; } = "";
        public EnumPioDirection PioDirection { get; set; } = EnumPioDirection.None;
        public bool IsEqPio { get; set; }
        public ushort ForkSpeed { get; set; } = 100;
        public string SlotNumber { get; set; } = "A";
        
        public RobotCommand(MainFlowHandler mainFlowHandler, AgvcTransCmd agvcTransCmd) : base(mainFlowHandler)
        {
            this.CstId = agvcTransCmd.CassetteId;
            this.CmdId = agvcTransCmd.CommandId;
        }

    }
}
