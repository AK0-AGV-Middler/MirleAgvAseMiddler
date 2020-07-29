using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Controller;

namespace Mirle.Agv.AseMiddler.Model.TransferSteps
{

    public class UnloadCmdInfo : RobotCommand
    {
        public UnloadCmdInfo(AgvcTransCmd agvcTransCmd) : base(agvcTransCmd)
        {
            this.type = EnumTransferStepType.Unload;
            this.PortAddressId = agvcTransCmd.UnloadAddressId;
        }
    }
}
