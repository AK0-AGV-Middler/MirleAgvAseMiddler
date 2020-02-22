using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Controller;
 
using Mirle.AgvAseMiddler.Model.Configs;
using Mirle.Tools;

namespace Mirle.AgvAseMiddler.Model.TransferSteps
{
    [Serializable]
    public class MoveCmdInfo : TransferStep
    {
        public MapAddress EndAddress { get; set; } = new MapAddress();     
      
        public MoveCmdInfo(MapAddress endAddress,string cmdId) :base(cmdId)
        {
            type = EnumTransferStepType.Move;
            this.EndAddress = endAddress;
        }    
    }

    [Serializable]
    public class MoveToChargerCmdInfo : MoveCmdInfo
    {
        public MoveToChargerCmdInfo(MapAddress endAddress, string cmdId) : base(endAddress,cmdId)
        {
            type = EnumTransferStepType.MoveToCharger;
        }


    }
}
