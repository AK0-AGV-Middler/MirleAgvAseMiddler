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
        public double TurnSpeedSafetyRange { get; set; }
        public int TurnTimeoutValue { get; set; }
        public int SlowStopTimeoutValue { get; set; }
        public int CSVLogInterval { get; set; }
        public int SecondCorrectionX { get; set; }
        public double MoveStartWaitTime { get; set; }
        public Dictionary<EnumMoveControlSafetyType, SafetyData> Safety { get; set; } = new Dictionary<EnumMoveControlSafetyType, SafetyData>();

        public Dictionary<EnumAddressAction, AGVTurnParameter> TurnParameter { get; set; } = new Dictionary<EnumAddressAction, AGVTurnParameter>();

        public MoveControlConfig()
        {
            /*
            AGVTurnParameter temp = new AGVTurnParameter();
            AxisData tempAxisData;
            temp.Velocity = 300;
            temp.R = 350;
            temp.VChangeSafetyDistance = 200;
            temp.CloseReviseDistance = 100;
            temp.AxisParameter = new AxisData(165, 165, 990, 75);
            TurnParameter.Add(EnumAddressAction.TR350, temp);

            temp = new AGVTurnParameter();
            temp.Velocity = 50;
            temp.R = 50;
            temp.VChangeSafetyDistance = 50;
            temp.CloseReviseDistance = 50;
            temp.AxisParameter = new AxisData(165, 165, 990, 75);
            TurnParameter.Add(EnumAddressAction.TR50, temp);

            SecondCorrectionX = 5;

            TurnTimeoutValue = 5000;
            SlowStopTimeoutValue = 2000;
            MoveStartWaitTime = 2000;

            Sr2000ConfigPath = @"D:\SR2000Parameter\SR2000Config.xml";
            OnTimeReviseConfigPath = "OntimeReviseConfig.xml";
            ElmoConfigPath = "MotionParameter.xml";
            SleepTime = 5;
            CSVLogInterval = 50;
            LowVelocity = 300;
            EQVelocity = 80;
            EQVelocityDistance = 100;
            Move = new AxisData(500, 500, 2000, 600);
            Turn = new AxisData(165, 165, 990, 50);

            temp = new AGVTurnParameter();

            tempAxisData = new AxisData(20.9, 20.9, 368.6, 18.3, 42.7);
            temp.R2000Parameter.Add(EnumR2000Parameter.InnerWheelTurn, tempAxisData);
            //double 轉向內輪加速度 = 20.9;
            //double 轉向內輪JERK = 368.6;
            //double 轉向內輪速度 = 18.3;
            //double 轉向內輪角度 = 42.7;

            tempAxisData = new AxisData(11.7, 11.7, 206.8, 10.3, 23.96);
            temp.R2000Parameter.Add(EnumR2000Parameter.OuterWheelTurn, tempAxisData);
            //double 轉向外輪加速度 = 11.7;
            //double 轉向外輪JERK = 206.8;
            //double 轉向外輪速度 = 10.3;
            //double 轉向外輪角度 = 23.96;

            tempAxisData = new AxisData(7.2, 7.2, 28.8, 138 - 122, -3255.08 * 16 / 122);
            temp.R2000Parameter.Add(EnumR2000Parameter.InnerWheelMove, tempAxisData);
            //double 走行內輪加速度 = 7.2;
            //double 走行內輪JERK = 28.8;
            //double 走行內輪速度 = 138 - 122;
            //double 走行內輪距離 = 3255.08 * 16 / 122;
            //double 走行內輪距離without減速段 = 2807.28;

            tempAxisData = new AxisData(27.8, 27.8, 165, 203.87 - 138, 5136.55 * 66 / 204);
            temp.R2000Parameter.Add(EnumR2000Parameter.OuterWheelMove, tempAxisData);
            //double 走行外輪加速度 = 27.8;
            //double 走行外輪JERK = 165;
            //double 走行外輪速度 = 203.87 - 138;
            //double 走行外輪距離 = 5136.55 * 66 / 204;
            //double 走行外輪距離without減速段 = 4507.45;

            temp.Velocity = 138;
            temp.R = 2285;
            temp.VChangeSafetyDistance = 50;
            temp.CloseReviseDistance = 50;

            TurnParameter.Add(EnumAddressAction.R2000, temp);

            MoveCommandDistanceMagnification = 1.1;
            StartWheelAngleRange = 0.1;


            TurnSpeedSafetyRange = 0.05;

            SafteyDistance.Add(EnumCommandType.Vchange, 200);
            SafteyDistance.Add(EnumCommandType.Stop, 50);
            SafteyDistance.Add(EnumCommandType.SlowStop, 20);
            SafteyDistance.Add(EnumCommandType.End, 10);
            SafteyDistance.Add(EnumCommandType.Move, 100);
            SafteyDistance.Add(EnumCommandType.ReviseClose, 100);
            SafteyDistance.Add(EnumCommandType.ReviseOpen, 100);
            SafteyDistance.Add(EnumCommandType.TR, 40);

            SafetyData tempSafetyData = new SafetyData();
            tempSafetyData.Range = 30;
            Safety.Add(EnumMoveControlSafetyType.TurnOut, tempSafetyData);

            tempSafetyData.Range = 3000;
            Safety.Add(EnumMoveControlSafetyType.LineBarcodeInterval, tempSafetyData);

            tempSafetyData.Range = 30;
            Safety.Add(EnumMoveControlSafetyType.OntimeReviseSectionDeviation, tempSafetyData);

            tempSafetyData.Range = 1.5;
            Safety.Add(EnumMoveControlSafetyType.OntimeReviseTheta, tempSafetyData);

            */
        }
    }
}
