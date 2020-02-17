using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model.Configs
{
    [Serializable]
    public class MoveControlConfig
    {
        public string Sr2000ConfigPath { get; set; }
        public string OnTimeReviseConfigPath { get; set; }
        public string ElmoConfigPath { get; set; }
        public int SleepTime { get; set; }
        public double LowVelocity { get; set; }
        public AxisData EQ { get; set; }
        public AxisData Move { get; set; }
        public AxisData Turn { get; set; }
        public Dictionary<EnumCommandType, double> SafteyDistance { get; set; } = new Dictionary<EnumCommandType, double>();
        public double MoveCommandDistanceMagnification { get; set; }
        public double MoveCommandDistanceConstant { get; set; }
        public double StartWheelAngleRange { get; set; }
        public int TurnTimeoutValue { get; set; }
        public int SlowStopTimeoutValue { get; set; }
        public int CSVLogInterval { get; set; }
        public int SecondCorrectionX { get; set; }
        public int PauseDelateTime { get; set; }
        public double MoveStartWaitTime { get; set; }
        public double ReserveSafetyDistance { get; set; }
        public double NormalStopDistance { get; set; }
        public double VelocitySafetyRange { get; set; }
        public double VChangeSafetyDistanceMagnification { get; set; }
        public double OverrideTimeoutValue { get; set; } = 2000;
        public double RetryMoveDistance { get; set; } = 100;
        public Dictionary<EnumMoveControlSafetyType, SafetyData> Safety { get; set; } = new Dictionary<EnumMoveControlSafetyType, SafetyData>();
        public Dictionary<EnumSensorSafetyType, SafetyData> SensorByPass { get; set; } = new Dictionary<EnumSensorSafetyType, SafetyData>();

        public Dictionary<EnumAddressAction, AGVTurnParameter> TurnParameter { get; set; } = new Dictionary<EnumAddressAction, AGVTurnParameter>();

        public MoveControlConfig()
        {
            SafetyData tempSensorData;

            foreach (EnumCommandType item in (EnumCommandType[])Enum.GetValues(typeof(EnumCommandType)))
            {
                SafteyDistance.Add(item, 100);
            }

            foreach (EnumMoveControlSafetyType item in (EnumMoveControlSafetyType[])Enum.GetValues(typeof(EnumMoveControlSafetyType)))
            {
                tempSensorData = new SafetyData();
                Safety.Add(item, tempSensorData);
            }

            foreach (EnumSensorSafetyType item in (EnumSensorSafetyType[])Enum.GetValues(typeof(EnumSensorSafetyType)))
            {
                tempSensorData = new SafetyData();
                SensorByPass.Add(item, tempSensorData);
            }
        }
    }
}
