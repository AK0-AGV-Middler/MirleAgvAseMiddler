using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class Command
    {
        public MapPosition Position { get; set; }
        public double TriggerEncoder { get; set; }
        public double SafetyDistance { get; set; }
        public EnumCommandType CmdType { get; set; }
        public EnumAddressAction TurnType { get; set; }
        public double Velocity { get; set; }
        public double Distance { get; set; }
        public MapPosition EndPosition { get; set; }
        public double EndEncoder { get; set; }
        public bool DirFlag { get; set; }
        public int WheelAngle { get; set; }
        public int ReserveNumber { get; set; } = -1;
        public bool NextRserveCancel { get; set; } = false;
        public int NextReserveNumber { get; set; } = -1;
        public EnumMoveStartType MoveType { get; set; }
        public EnumVChangeType VChangeType { get; set; }
        public double NowVelocity { get; set; }

        public Command()
        {
            Position = null;
            EndPosition = null;
            NextRserveCancel = false;
            CmdType = EnumCommandType.Stop;
        }
    }
}

