using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Controller;

namespace Mirle.Agv.AseMiddler.Model.TransferSteps
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
