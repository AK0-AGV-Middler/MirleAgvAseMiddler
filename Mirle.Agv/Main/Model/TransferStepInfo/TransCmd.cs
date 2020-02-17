using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Controller;
using Mirle.AgvAseMiddler.Model.Configs;

namespace Mirle.AgvAseMiddler.Model.TransferSteps
{
    [Serializable]
    public abstract class TransferStep
    {
        protected Vehicle theVehicle = Vehicle.Instance;
        protected MapInfo theMapInfo;
        protected MainFlowHandler mainFlowHandler;
        protected AgvcConnector agvcConnector;
        protected MapConfig mapConfig;
        protected EnumTransferStepType type;
        public string CmdId { get; set; } = "";
        public string CstId { get; set; } = "";

        //public TransCmd() : this(new MapInfo()) { }
        public TransferStep(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            theMapInfo = mainFlowHandler.TheMapInfo;
            agvcConnector = mainFlowHandler.GetAgvcConnector();
            mapConfig = mainFlowHandler.GetMapConfig();
        }

        public EnumTransferStepType GetTransferStepType() { return type; }       

    }
}
