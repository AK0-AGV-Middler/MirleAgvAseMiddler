using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Controller;

namespace Mirle.AgvAseMiddler.Model.TransferSteps
{
    [Serializable]
    public class LoadCmdInfo : RobotCommand
    {          
        public LoadCmdInfo(AgvcTransCmd agvcTransCmd) : base(agvcTransCmd)
        {
            this.type = EnumTransferStepType.Load;
            this.PortAddressId = agvcTransCmd.LoadAddressId;                
        }
    }
}
