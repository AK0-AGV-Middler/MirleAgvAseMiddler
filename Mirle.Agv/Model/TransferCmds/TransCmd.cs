using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferCmds
{
    public abstract class TransCmd
    {
        protected Vehicle theVehicle = Vehicle.Instance;
        protected MapInfo theMapInfo = new MapInfo();
        protected EnumTransCmdType type;
        public string CmdId { get; set; } = "Empty";
        public string CstId { get; set; } = "Empty";

        //public TransCmd() : this(new MapInfo()) { }
        public TransCmd(MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
        }

        public EnumTransCmdType GetCommandType()
        {
            return type;
        }

        public TransCmd Clone()
        {
            return ExtensionMethods.DeepClone(this);
        }
    }
}
