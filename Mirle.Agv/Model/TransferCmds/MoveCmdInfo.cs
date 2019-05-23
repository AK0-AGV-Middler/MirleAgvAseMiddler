using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Control;

namespace Mirle.Agv.Model.TransferCmds
{
    public class MoveCmdInfo : TransCmd
    {       
        public string MoveEndAddress { get; set; } // LoadAddress or UnloadAddress
        public double TotalMoveLength { get; set; }

        public MapSection Section { get; set; }
        public bool IsPrecisePositioning { get; set; }  //是否二次定位 // = IsMoveEndSection 本次連續移動最後一個Section

        public MapAddress MapAddress { get; set; }


        public MoveCmdInfo() : base()
        {
            type = EnumTransCmdType.Move;
        }       
    }
}
