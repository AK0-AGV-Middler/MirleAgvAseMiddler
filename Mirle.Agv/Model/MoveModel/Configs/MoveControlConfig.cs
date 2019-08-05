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
        public Dictionary<CommandType, double> SafteyDistance { get; set; } = new Dictionary<CommandType, double>();
        public double ElmoDataInterval_half { get; set; }
        public double MoveCommandDistanceMagnification { get; set; }
        public double StartWheelAngleRange { get; set; }
        public double SlowStopDistance { get; set; }
        public double TurnSpeedSafetyRange { get; set; }
        public double AGVMaxVelocity { get; set; }
        public int TurnTimeoutValue { get; set; }
        public int SlowStopTimeoutValue { get; set; }
        public int CSVLogInterval { get; set; }

        public Dictionary<EnumAddressAction, AGVTurnParameter> TR { get; set; } = new Dictionary<EnumAddressAction, AGVTurnParameter>();

        public MoveControlConfig()
        {
            AGVTurnParameter temp = new AGVTurnParameter();
            temp.Velocity = 300;
            temp.R = 350;
            temp.VChangeDistance = 200;
            temp.CloseReviseDistance = 200;
            temp.AxisParameter = new AxisData(165, 165, 990, 75);
            TR.Add(EnumAddressAction.TR350, temp);

            temp = new AGVTurnParameter();
            temp.Velocity = 50;
            temp.R = 50;
            temp.VChangeDistance = 100;
            temp.CloseReviseDistance = 150;
            temp.AxisParameter = new AxisData(165, 165, 990, 75);
            TR.Add(EnumAddressAction.TR50, temp);

            TurnTimeoutValue = 5000;
            SlowStopTimeoutValue = 2000;

            Sr2000ConfigPath = "SR2000Config.xml";
            OnTimeReviseConfigPath = "OntimeReviseConfig.xml";
            ElmoConfigPath = "MotionParameter.xml";
            SleepTime = 5;
            CSVLogInterval = 50;
            LowVelocity = 300;
            EQVelocity = 80;
            EQVelocityDistance = 100;
            Move = new AxisData(500, 500, 2000);
            Turn = new AxisData(165, 165, 990, 50);

            MoveCommandDistanceMagnification = 1.1;
            StartWheelAngleRange = 0.1;
            AGVMaxVelocity = 400;

            SlowStopDistance = EQVelocity * EQVelocity / 2 / Move.Deceleration;

            ElmoDataInterval_half = 20;
            TurnSpeedSafetyRange = 5 / 100;

            SafteyDistance.Add(CommandType.Vchange, 200);
            SafteyDistance.Add(CommandType.Stop, 50);
            SafteyDistance.Add(CommandType.SlowStop, 20);
            SafteyDistance.Add(CommandType.End, 10);
            SafteyDistance.Add(CommandType.Move, 50);
            SafteyDistance.Add(CommandType.ReviseClose, 100);
            SafteyDistance.Add(CommandType.ReviseOpen, 100);
            SafteyDistance.Add(CommandType.TR, 40);
        }
    }
}
