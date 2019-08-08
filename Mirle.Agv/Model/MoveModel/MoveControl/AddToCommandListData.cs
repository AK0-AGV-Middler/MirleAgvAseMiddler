using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class AddToCommandListData
    {
        public double CommandDistance { get; set; }
        public double MoveStartEncoder { get; set; } = 0;
        public bool DirFlag { get; set; }
        public int StartWheelAngle { get; set; }
        public int NowWheelAngle { get; set; }
        public int InsertIndex { get; set; } = 0;
        public int MoveCmdStartReserveNumber { get; set; } = 0;
        public int IndexOfReserveList { get; set; } = 0;
        public MapPosition LastNode { get; set; }
        public EnumAddressAction LastAction { get; set; }
        public double LastVelocity { get; set; }
        public double NowVelocityCommand { get; set; }
        public double Distance { get; set; }
        public double STDistance { get; set; }
        public double NowVelocity { get; set; }
        public double TurnOutDistance { get; set; }
    }
}
