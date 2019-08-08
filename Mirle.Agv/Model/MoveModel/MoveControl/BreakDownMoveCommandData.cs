using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class BreakDownMoveCommandData
    {
        public Dictionary<EnumAddressAction, double> TurnInSafetyDistance { get; set; } = new Dictionary<EnumAddressAction, double>();
        public Dictionary<EnumAddressAction, double> TurnOutSafetyDistance { get; set; } = new Dictionary<EnumAddressAction, double>();
        public int WheelAngle { get; set; } = 0;
        public bool DirFlag { get; set; } = true;
        public double BackDistance { get; set; }
        public double TempDistance { get; set; } = 0;
        public bool IsTurnOut { get; set; } = false;
        public bool IsFirstTurnIn { get; set; } = true;
        public bool IsEnd { get; set; } = false;
        public double StartMoveEncoder { get; set; } = 0;
        public double TurnInOutDistance { get; set; } = 0;
        public double StartByPassDistance { get; set; } = 0;
        public double EndByPassDistance { get; set; } = 0;
        public MapPosition StartNode { get; set; }
        public MapPosition EndNode { get; set; }
        public MapPosition TempNode { get; set; }
        public MapPosition NextNode { get; set; }
        public int Index { get; set; } = 0;
        public EnumAddressAction TurnType { get; set; } = EnumAddressAction.TR350;
    }
}
