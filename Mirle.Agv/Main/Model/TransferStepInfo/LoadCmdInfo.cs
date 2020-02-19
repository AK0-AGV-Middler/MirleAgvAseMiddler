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
        public LoadCmdInfo(MainFlowHandler mainFlowHandler, AgvcTransCmd agvcTransCmd) : base(mainFlowHandler, agvcTransCmd)
        {
            this.type = EnumTransferStepType.Load;
            this.PortAddress = agvcTransCmd.LoadAddressId;
            this.SlotNumber = agvcTransCmd.SlotNumber;
            MapAddress mapAddress = theMapInfo.allMapAddresses[PortAddress];
            this.IsEqPio = mapAddress.PioDirection != EnumPioDirection.None;
            this.PioDirection = mapAddress.PioDirection;            
        }
    }
}
