using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class Command
    {
        public MapPosition Position { get; set; }
        public double TriggerEncoder { get; set; }
        public double SafetyDistance { get; set; }
        public EnumCommandType CmdType { get; set; }
        public EnumAddressAction TRType { get; set; }
        public double Velocity { get; set; }
        public double Distance { get; set; }
        public MapPosition EndPosition { get; set; }
        public double EndEncoder { get; set; }
        public bool DirFlag { get; set; }
        public int WheelAngle { get; set; }
        public int ReserveNumber { get; set; }
        public bool NextRserveCancel { get; set; }
        public bool IsFirstMove { get; set; }
        public bool IsTurnVChange { get; set; }

        public Command()
        {
            Position = null;
            EndPosition = null;
            CmdType = EnumCommandType.Stop;
        }
    }
}

