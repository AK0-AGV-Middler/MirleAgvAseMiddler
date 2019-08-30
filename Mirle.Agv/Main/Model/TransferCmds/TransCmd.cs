using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;
using Mirle.Agv.Model.Configs;

namespace Mirle.Agv.Model.TransferCmds
{
    [Serializable]
    public abstract class TransferStep
    {
        protected Vehicle theVehicle = Vehicle.Instance;
        protected MapInfo theMapInfo;
        protected MainFlowHandler mainFlowHandler;
        protected MiddleAgent middleAgent;
        protected MapConfig mapConfig;
        protected EnumTransferStepType type;
        public string CmdId { get; set; } = "";
        public string CstId { get; set; } = "";

        //public TransCmd() : this(new MapInfo()) { }
        public TransferStep(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            theMapInfo = mainFlowHandler.TheMapInfo;
            middleAgent = mainFlowHandler.GetMiddleAgent();
            mapConfig = mainFlowHandler.GetMapConfig();
        }

        public EnumTransferStepType GetTransferStepType() { return type; }       

    }
}
