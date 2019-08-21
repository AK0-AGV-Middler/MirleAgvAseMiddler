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
        protected MapInfo theMapInfo = new MapInfo();
        protected EnumTransferCommandType type;
        public string CmdId { get; set; } = "Empty";
        public string CstId { get; set; } = "Empty";

        //public TransCmd() : this(new MapInfo()) { }
        public TransferStep(MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
        }

        public EnumTransferCommandType GetEnumTransferCommandType()
        {
            return type;
        }       
    }
}
