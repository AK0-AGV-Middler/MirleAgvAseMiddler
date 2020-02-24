using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Controller;

namespace Mirle.AgvAseMiddler.Model.TransferSteps
{
    [Serializable]
    public class UnloadCmdInfo : RobotCommand
    {
        public UnloadCmdInfo(AgvcTransCmd agvcTransCmd) : base(agvcTransCmd)
        {
            this.type = EnumTransferStepType.Unload;
            this.PortAddressId = agvcTransCmd.UnloadAddressId;                   
        }
    }
}
