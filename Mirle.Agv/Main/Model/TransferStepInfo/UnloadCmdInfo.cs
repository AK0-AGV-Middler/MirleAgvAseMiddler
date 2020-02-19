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
        public UnloadCmdInfo(MainFlowHandler mainFlowHandler, AgvcTransCmd agvcTransCmd) : base(mainFlowHandler, agvcTransCmd)
        {
            this.type = EnumTransferStepType.Unload;
            this.PortAddress = agvcTransCmd.UnloadAddressId;
            this.SlotNumber = agvcTransCmd.SlotNumber;
            MapAddress mapAddress = theMapInfo.allMapAddresses[PortAddress];
            this.IsEqPio = mapAddress.PioDirection != EnumPioDirection.None;
            this.PioDirection = mapAddress.PioDirection;
        }
    }
}
