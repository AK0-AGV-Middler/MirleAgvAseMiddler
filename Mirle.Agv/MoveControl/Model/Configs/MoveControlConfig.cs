using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Configs
{
    [Serializable]
    public class MoveControlConfig
    {
        public string Sr2000ConfigPath { get; set; }
        public string OnTimeReviseConfigPath { get; set; }
        public string ElmoConfigPath { get; set; }
        public int SleepTime { get; set; }
        public double LowVelocity { get; set; }
        public double EQVelocity { get; set; }
        public double EQVelocityDistance { get; set; }
        public AxisData Move { get; set; }
        public AxisData Turn { get; set; }
        public Dictionary<EnumCommandType, double> SafteyDistance { get; set; } = new Dictionary<EnumCommandType, double>();
        public double MoveCommandDistanceMagnification { get; set; }
        public double StartWheelAngleRange { get; set; }
        public int TurnTimeoutValue { get; set; }
        public int SlowStopTimeoutValue { get; set; }
        public int CSVLogInterval { get; set; }
        public int SecondCorrectionX { get; set; }
        public double MoveStartWaitTime { get; set; }
        public double ReserveSafetyDistance { get; set; }
        public Dictionary<EnumMoveControlSafetyType, SafetyData> Safety { get; set; } = new Dictionary<EnumMoveControlSafetyType, SafetyData>();
        public Dictionary<EnumSensorSafetyType, SafetyData> SensorByPass { get; set; } = new Dictionary<EnumSensorSafetyType, SafetyData>();

        public Dictionary<EnumAddressAction, AGVTurnParameter> TurnParameter { get; set; } = new Dictionary<EnumAddressAction, AGVTurnParameter>();
    }
}
