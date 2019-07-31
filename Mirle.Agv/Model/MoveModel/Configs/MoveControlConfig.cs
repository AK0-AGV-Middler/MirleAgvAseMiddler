using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Configs
{
    public class MoveControlConfig
    {
        public string Sr2000ConfigPath { get; set; }
        public string OnTimeReviseConfigPath { get; set; }
        public string ElmoConfigPath { get; set; }
        public int SleepTime { get; set; }
        public double TR350Delta { get; set; }
        public double TR50Delta { get; set; }
        public double R2000Delta { get; set; }
        public double LowVelocity { get; set; }
        public double EQVelocity { get; set; }
        public double EQVelocityDistance { get; set; }
        public Dictionary<EnumAddressAction, double> TurnFlowDistance { get; set; } = new Dictionary<EnumAddressAction, double>();
        public AxisData Move { get; set; }
        public AxisData Turn { get; set; }
        public AxisData TR { get; set; }
        public Dictionary<EnumCommandType, double> SafteyDistance { get; set; } = new Dictionary<EnumCommandType, double>();
        public double ElmoDataInterval_half { get; set; }
        public double MoveCommandDistanceMagnification { get; set; }
        public double StartWheelAngleRange { get; set; }
        public double SlowStopDistance { get; set; }
        public double TurnSpeedSafetyRange { get; set; }
        public double AGVMaxVelocity { get; set; }

        public MoveControlConfig()
        {
            Sr2000ConfigPath = "SR2000Config.xml";
            OnTimeReviseConfigPath = "OntimeReviseConfig.xml";
            ElmoConfigPath = "MotionParameter.xml";
            SleepTime = 5;
            TR350Delta = -150;
            TR50Delta = -21;
            R2000Delta = -900;
            LowVelocity = 300;
            EQVelocity = 80;
            EQVelocityDistance = 100;
            Move = new AxisData(500, 500, 2000);
            Turn = new AxisData(165, 165, 990, 50);
            TR = new AxisData(165, 165, 990, 75);
            TurnFlowDistance.Add(EnumAddressAction.TR50, 50);
            TurnFlowDistance.Add(EnumAddressAction.TR350, 200);
            TurnFlowDistance.Add(EnumAddressAction.R2000, 100);

            MoveCommandDistanceMagnification = 1.1;
            StartWheelAngleRange = 0.1;
            AGVMaxVelocity = 400;

            SlowStopDistance = EQVelocity * EQVelocity / 2 / Move.Deceleration;

            ElmoDataInterval_half = 20;
            TurnSpeedSafetyRange = 5 / 100;

            SafteyDistance.Add(EnumCommandType.Vchange, 200);
            SafteyDistance.Add(EnumCommandType.Stop, 50);
            SafteyDistance.Add(EnumCommandType.SlowStop, 20);
            SafteyDistance.Add(EnumCommandType.End, 10);
            SafteyDistance.Add(EnumCommandType.Move, 50);
            SafteyDistance.Add(EnumCommandType.ReviseClose, 100);
            SafteyDistance.Add(EnumCommandType.ReviseOpen, 100);
            SafteyDistance.Add(EnumCommandType.TR, 40);
        }
    }
}
