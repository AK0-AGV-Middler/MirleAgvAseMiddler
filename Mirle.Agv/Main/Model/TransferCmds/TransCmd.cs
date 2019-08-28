using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferCmds
{
    [Serializable]
    public abstract class TransferStep
    {
        protected Vehicle theVehicle = Vehicle.Instance;
        protected MapInfo theMapInfo;
        protected MainFlowHandler mainFlowHandler;
        protected MiddleAgent middleAgent;
        protected EnumTransferStepType type;
        public string CmdId { get; set; } = "Empty";
        public string CstId { get; set; } = "Empty";

        //public TransCmd() : this(new MapInfo()) { }
        public TransferStep(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            theMapInfo = mainFlowHandler.TheMapInfo;
            middleAgent = mainFlowHandler.GetMiddleAgent();
        }

        public EnumTransferStepType GetTransferStepType() { return type; }       

    }
}
