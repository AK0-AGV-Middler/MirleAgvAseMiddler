using Mirle.Agv.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Controller;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using Mirle.Agv.Model.TransferSteps;

namespace Mirle.Agv.Controller
{
    public class MoveControlHandler
    {
        public CreateMoveControlList CreateMoveCommandList { get; set; }
        public EnumMoveState MoveState { get; private set; } = EnumMoveState.Idle;
        public MoveControlConfig moveControlConfig;
        private MapInfo theMapInfo = new MapInfo();
        private Logger logger = LoggerAgent.Instance.GetLooger("MoveControlCSV");
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private string device = "MoveControl";
        private Dictionary<EnumAddressAction, TRTimeToAngleRange> trTimeToAngleRange = new Dictionary<EnumAddressAction, TRTimeToAngleRange>();
        public event EventHandler<EnumMoveComplete> OnMoveFinished;
        public ElmoDriver elmoDriver;
        public List<Sr2000Driver> DriverSr2000List = new List<Sr2000Driver>();
        public OntimeReviseConfig ontimeReviseConfig = null;
        private AgvMoveRevise agvRevise;
        public MoveCommandData command { get; private set; } = new MoveCommandData();
        public Location location = new Location();
        private AlarmHandler alarmHandler;
        private ComputeFunction computeFunction = new ComputeFunction();
        private PlcAgent plcAgent = null;
        public MoveControlParameter ControlData { get; private set; } = new MoveControlParameter();
        public SimulateState FakeState { get; set; } = new SimulateState();
        private Thread threadSCVLog;
        public bool DebugCSVMode { get; set; } = false;
        public List<string[]> deubgCsvLogList = new List<string[]>();
        private bool isAGVMCommand = false;
        public string MoveCommandID { get; set; } = "";
        public bool SimulationMode { get; set; } = false;
        private bool simulationIsMoving = false;
        private MoveControlSafetyData safetyData = new MoveControlSafetyData();
        private Dictionary<int, double> TRAngleToTime = new Dictionary<int, double>();
        public string AGVStopResult { get; set; }
        private const int debugFlowLogMaxLength = 10000;
        public string DebugFlowLog { get; set; }
        public double LoopTime { get; set; } = 0;
        private System.Diagnostics.Stopwatch loopTimeTimer = new System.Diagnostics.Stopwatch();
        public bool SR2000Connected { get; private set; } = true;
        public string MoveControlVersion { get; set; }

        private void SetDebugFlowLog(string functionName, string message)
        {
            DebugFlowLog = DateTime.Now.ToString("HH:mm:ss.fff") + "\t" + functionName + "\t" + message + "\r\n" + DebugFlowLog;
            if (DebugFlowLog.Length > debugFlowLogMaxLength)
                DebugFlowLog = DebugFlowLog.Substring(0, debugFlowLogMaxLength);
        }

        private void SendAlarmCode(int alarmCode)
        {
            try
            {
                WriteLog("MoveControl", "3", device, "", "SetAlarm, alarmCode : " + alarmCode.ToString());
                alarmHandler.SetAlarm(alarmCode);
            }
            catch (Exception ex)
            {
                WriteLog("Error", "3", device, "", "SetAlarm失敗, Excption : " + ex.ToString());
            }
        }

        #region TR奇怪角度計算
        private double TREocderToAngle(double nowEncoder)
        {
            double velocity = moveControlConfig.TurnParameter[EnumAddressAction.TR350].Velocity;
            double time = nowEncoder / velocity;
            TRTimeToAngleRange trTimeToAngle = trTimeToAngleRange[EnumAddressAction.TR350];

            double jerk = moveControlConfig.TurnParameter[EnumAddressAction.TR350].AxisParameter.Jerk;
            double acc = moveControlConfig.TurnParameter[EnumAddressAction.TR350].AxisParameter.Acceleration;
            double dec = moveControlConfig.TurnParameter[EnumAddressAction.TR350].AxisParameter.Deceleration;

            double angle = 0;
            double deltaTime = 0;

            if (time < trTimeToAngle.TimeRange[0])
            {
                deltaTime = time;
                angle = jerk * deltaTime * deltaTime * deltaTime / 6;
                angle += trTimeToAngle.AngleRange[0];
            }
            else if (time < trTimeToAngle.TimeRange[1])
            {
                deltaTime = time - trTimeToAngle.TimeRange[0];
                double endVelocity = trTimeToAngle.TurnVelocity[1] + acc * deltaTime;
                angle = (trTimeToAngle.TurnVelocity[1] + endVelocity) * deltaTime / 2;
                angle += trTimeToAngle.AngleRange[1];
            }
            else if (time < trTimeToAngle.TimeRange[2])
            {
                deltaTime = time - trTimeToAngle.TimeRange[1];
                angle = trTimeToAngle.TurnVelocity[3] * deltaTime;
                double deltaAngle = jerk * trTimeToAngle.TimeRange[0] * trTimeToAngle.TimeRange[0] * trTimeToAngle.TimeRange[0] / 6 -
                                    jerk * deltaTime * deltaTime * deltaTime / 6;
                angle = angle - deltaAngle;
                angle += trTimeToAngle.AngleRange[2];
            }
            else if (time < trTimeToAngle.TimeRange[3])
            {
                deltaTime = time - trTimeToAngle.TimeRange[2];
                angle = trTimeToAngle.TurnVelocity[3] * deltaTime;
                angle += trTimeToAngle.AngleRange[3];
            }
            else if (time < trTimeToAngle.TimeRange[4])
            {
                deltaTime = time - trTimeToAngle.TimeRange[3];
                angle = trTimeToAngle.TurnVelocity[4] * deltaTime;
                double deltaAngle = jerk * deltaTime * deltaTime * deltaTime / 6;
                angle = angle - deltaAngle;
                angle += trTimeToAngle.AngleRange[4];
            }
            else if (time < trTimeToAngle.TimeRange[5])
            {
                deltaTime = time - trTimeToAngle.TimeRange[4];
                double endVelocity = trTimeToAngle.TurnVelocity[5] - dec * deltaTime;
                angle = (trTimeToAngle.TurnVelocity[5] + endVelocity) * deltaTime / 2;
                angle += trTimeToAngle.AngleRange[5];
            }
            else if (time < trTimeToAngle.TimeRange[6])
            {
                deltaTime = time - trTimeToAngle.TimeRange[5];
                angle = jerk * trTimeToAngle.TimeRange[0] * trTimeToAngle.TimeRange[0] * trTimeToAngle.TimeRange[0] / 6 -
                        jerk * deltaTime * deltaTime * deltaTime / 6;
                angle += trTimeToAngle.AngleRange[6];
            }
            else
            {
                angle = 90;
            }

            return angle;
        }

        private double GetTRFlowAngleChange(double nowEncoder)
        {
            double velocity = moveControlConfig.TurnParameter[ControlData.NowAction].Velocity;
            double time = nowEncoder / velocity;
            TRTimeToAngleRange trTimeToAngle = trTimeToAngleRange[ControlData.NowAction];

            double jerk = moveControlConfig.TurnParameter[ControlData.NowAction].AxisParameter.Jerk;
            double acc = moveControlConfig.TurnParameter[ControlData.NowAction].AxisParameter.Acceleration;
            double dec = moveControlConfig.TurnParameter[ControlData.NowAction].AxisParameter.Deceleration;

            double angle = 0;
            double deltaTime = 0;

            if (time < trTimeToAngle.TimeRange[0])
            {
                deltaTime = time;
                angle = jerk * deltaTime * deltaTime * deltaTime / 6;
                angle += trTimeToAngle.AngleRange[0];
            }
            else if (time < trTimeToAngle.TimeRange[1])
            {
                deltaTime = time - trTimeToAngle.TimeRange[0];
                double endVelocity = trTimeToAngle.TurnVelocity[1] + acc * deltaTime;
                angle = (trTimeToAngle.TurnVelocity[1] + endVelocity) * deltaTime / 2;
                angle += trTimeToAngle.AngleRange[1];
            }
            else if (time < trTimeToAngle.TimeRange[2])
            {
                deltaTime = time - trTimeToAngle.TimeRange[1];
                angle = trTimeToAngle.TurnVelocity[3] * deltaTime;
                double deltaAngle = jerk * trTimeToAngle.TimeRange[0] * trTimeToAngle.TimeRange[0] * trTimeToAngle.TimeRange[0] / 6 -
                                    jerk * deltaTime * deltaTime * deltaTime / 6;
                angle = angle - deltaAngle;
                angle += trTimeToAngle.AngleRange[2];
            }
            else if (time < trTimeToAngle.TimeRange[3])
            {
                deltaTime = time - trTimeToAngle.TimeRange[2];
                angle = trTimeToAngle.TurnVelocity[3] * deltaTime;
                angle += trTimeToAngle.AngleRange[3];
            }
            else if (time < trTimeToAngle.TimeRange[4])
            {
                deltaTime = time - trTimeToAngle.TimeRange[3];
                angle = trTimeToAngle.TurnVelocity[4] * deltaTime;
                double deltaAngle = jerk * deltaTime * deltaTime * deltaTime / 6;
                angle = angle - deltaAngle;
                angle += trTimeToAngle.AngleRange[4];
            }
            else if (time < trTimeToAngle.TimeRange[5])
            {
                deltaTime = time - trTimeToAngle.TimeRange[4];
                double endVelocity = trTimeToAngle.TurnVelocity[5] - dec * deltaTime;
                angle = (trTimeToAngle.TurnVelocity[5] + endVelocity) * deltaTime / 2;
                angle += trTimeToAngle.AngleRange[5];
            }
            else if (time < trTimeToAngle.TimeRange[6])
            {
                deltaTime = time - trTimeToAngle.TimeRange[5];
                angle = jerk * trTimeToAngle.TimeRange[0] * trTimeToAngle.TimeRange[0] * trTimeToAngle.TimeRange[0] / 6 -
                        jerk * deltaTime * deltaTime * deltaTime / 6;
                angle += trTimeToAngle.AngleRange[6];
            }
            else
            {
                angle = 90;
            }

            return angle;
        }

        private void SetTRTimeToAngleRange()
        {
            TRTimeToAngleRange temp = new TRTimeToAngleRange();

            double vel = moveControlConfig.TurnParameter[EnumAddressAction.TR350].AxisParameter.Velocity;
            double acc = moveControlConfig.TurnParameter[EnumAddressAction.TR350].AxisParameter.Acceleration;
            double dec = moveControlConfig.TurnParameter[EnumAddressAction.TR350].AxisParameter.Deceleration;
            double jerk = moveControlConfig.TurnParameter[EnumAddressAction.TR350].AxisParameter.Jerk;

            double targetAngle = 90;
            double nowAngle = 0;
            double nowTime = 0;

            double timeAccChange = acc / jerk;
            double velocityAccChange = jerk * timeAccChange * timeAccChange / 2;
            double timeSameAcc = (vel - 2 * velocityAccChange) / acc;

            double distanceAccUp = jerk * timeAccChange * timeAccChange * timeAccChange / 6;
            double distanceAccDown = timeAccChange * vel - distanceAccUp;
            double distanceSameAcc = vel * timeSameAcc / 2;

            nowTime += timeAccChange;
            temp.TimeRange.Add(nowTime);
            temp.AngleRange.Add(nowAngle);
            temp.TurnVelocity.Add(0);
            nowAngle += distanceAccUp;

            nowTime += timeSameAcc;
            temp.TimeRange.Add(nowTime);
            temp.AngleRange.Add(nowAngle);
            temp.TurnVelocity.Add(velocityAccChange);
            nowAngle += distanceSameAcc;

            nowTime += timeAccChange;
            temp.TimeRange.Add(nowTime);
            temp.AngleRange.Add(nowAngle);
            temp.TurnVelocity.Add(vel - velocityAccChange);
            nowAngle += distanceAccDown;

            double timeSameVelocity = (targetAngle - 2 * nowAngle) / vel;

            nowTime += timeSameVelocity;
            temp.TimeRange.Add(nowTime);
            temp.AngleRange.Add(nowAngle);
            temp.TurnVelocity.Add(vel);
            nowAngle = targetAngle - nowAngle;

            nowTime += timeAccChange;
            temp.TimeRange.Add(nowTime);
            temp.AngleRange.Add(nowAngle);
            temp.TurnVelocity.Add(vel);
            nowAngle += distanceAccDown;

            nowTime += timeSameAcc;
            temp.TimeRange.Add(nowTime);
            temp.AngleRange.Add(nowAngle);
            temp.TurnVelocity.Add(vel - velocityAccChange);
            nowAngle += distanceSameAcc;

            nowTime += timeAccChange;
            temp.TimeRange.Add(nowTime);
            temp.AngleRange.Add(nowAngle);
            temp.TurnVelocity.Add(velocityAccChange);

            trTimeToAngleRange.Add(EnumAddressAction.TR350, temp);
            trTimeToAngleRange.Add(EnumAddressAction.TR50, temp);

            double angleDouble = 0;
            int angleInt = 0;
            double encoder = 0;

            for (; angleInt <= 90; encoder += 1)
            {
                angleDouble = TREocderToAngle(encoder);
                if (Math.Abs((double)angleInt - angleDouble) < 0.5)
                {
                    TRAngleToTime.Add(angleInt, encoder / 300);
                    angleInt++;
                }
            }
        }
        #endregion 

        private string GetUpLevelDirectory(string path, int upLevel)
        {
            var directory = File.GetAttributes(path).HasFlag(FileAttributes.Directory)
                ? path : Path.GetDirectoryName(path);

            upLevel = upLevel < 0 ? 0 : upLevel;

            for (var i = 0; i < upLevel; i++)
                directory = Path.GetDirectoryName(directory);

            return directory;
        }

        private void GetVersion()
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            string fullPath = GetUpLevelDirectory(path, 2) + "\\MoveControl";

            MoveControlVersion = computeFunction.GetFileLastTime(fullPath);
        }

        public MoveControlHandler(MapInfo theMapInfo, AlarmHandler alarmHandler, PlcAgent plcAgent)
        {
            GetVersion();
            this.alarmHandler = alarmHandler;
            this.theMapInfo = theMapInfo;
            this.plcAgent = plcAgent;
            ReadMoveControlConfigXML(@"D:\MoveControl\MoveControlConfig.xml");
            SetTRTimeToAngleRange();
            InitailSr2000(moveControlConfig.Sr2000ConfigPath);
            elmoDriver = new ElmoDriver(moveControlConfig.ElmoConfigPath, this.alarmHandler);

            ReadOntimeReviseConfigXML(moveControlConfig.OnTimeReviseConfigPath);
            CreateMoveCommandList = new CreateMoveControlList(DriverSr2000List, moveControlConfig, this.alarmHandler);

            agvRevise = new AgvMoveRevise(ontimeReviseConfig, elmoDriver, DriverSr2000List);

            loopTimeTimer.Reset();
            loopTimeTimer.Start();

            ControlData.MoveControlThread = new Thread(MoveControlThread);
            ControlData.MoveControlThread.Start();

            threadSCVLog = new Thread(WriteLogCSV);
            threadSCVLog.Start();

            //MapPosition a = new MapPosition(10000, 1500);
            //MapPosition b = new MapPosition(10000, 5000);
            //WallData wall = new WallData("test", a, b, 1000);
            //List<MapSectionBeamDisable> test = new List<MapSectionBeamDisable>();

            //try
            //{
            //    computeFunction.ComputeWallByPass(theMapInfo.allMapSections, wall, ref test);
            //}
            //catch
            //{
            //}
        }

        public void CloseMoveControlHandler()
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            ControlData.CloseMoveControl = true;

            StopAndClear();
            timer.Reset();
            timer.Start();
            Thread.Sleep(moveControlConfig.SleepTime);
            while (MoveState != EnumMoveState.Idle)
            {
                if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue * 5)
                {
                    ControlData.MoveControlThread.Abort();
                    EMSControl("StopAndClear time out !");
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            if (!elmoDriver.MoveCompelete(EnumAxis.GT) || !elmoDriver.MoveCompelete(EnumAxis.GX))
            {
                try
                {
                    plcAgent.SetForcELMOServoOffOn();
                    WriteLog("MoveControl", "7", device, "", "經過StopAndClear後group仍在動作,通知PLC斷電!");
                }
                catch
                {
                    WriteLog("MoveControl", "7", device, "", "經過StopAndClear後group仍在動作,通知PLC斷電,但跳Excption!");
                }
            }
            else
                elmoDriver.DisableAllAxis();

            foreach (Sr2000Driver sr2000 in DriverSr2000List)
            {
                sr2000.Disconnect();
            }
        }

        private void WriteLog(string category, string logLevel, string device, string carrierId, string message,
                             [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string classMethodName = GetType().Name + ":" + memberName;
            LogFormat logFormat = new LogFormat(category, logLevel, classMethodName, device, carrierId, message);

            loggerAgent.LogMsg(logFormat.Category, logFormat);
            SetDebugFlowLog(memberName, message);
        }

        #region Read MoveControlConfig XML
        private AxisData ReadAxisData(XmlElement element)
        {
            AxisData returnAxisData = new AxisData();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Acceleration":
                        returnAxisData.Acceleration = double.Parse(item.InnerText);
                        break;
                    case "Deceleration":
                        returnAxisData.Deceleration = double.Parse(item.InnerText);
                        break;
                    case "Jerk":
                        returnAxisData.Jerk = double.Parse(item.InnerText);
                        break;
                    case "Velocity":
                        returnAxisData.Velocity = double.Parse(item.InnerText);
                        break;
                    case "Distance":
                        returnAxisData.Distance = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return returnAxisData;
        }

        private void ReadSafetyDistanceXML(XmlElement element)
        {
            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Vchange":
                    case "Stop":
                    case "SlowStop":
                    case "End":
                    case "Move":
                    case "ReviseClose":
                    case "ReviseOpen":
                    case "TR":
                    case "R2000":
                        //moveControlConfig.SafteyDistance.Add(
                        //    (EnumCommandType)Enum.Parse(typeof(EnumCommandType), item.Name),
                        //    double.Parse(item.InnerText));
                        moveControlConfig.SafteyDistance[(EnumCommandType)Enum.Parse(typeof(EnumCommandType), item.Name)] = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }
        }

        private SafetyData ReadSafetyDataXML(XmlElement element)
        {
            SafetyData temp = new SafetyData();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Enable":
                        temp.Enable = bool.Parse(item.InnerText);
                        break;
                    case "Range":
                        temp.Range = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return temp;
        }

        private void ReadSafetyXML(XmlElement element)
        {
            SafetyData temp;

            foreach (XmlNode item in element.ChildNodes)
            {
                temp = new SafetyData();

                switch (item.Name)
                {
                    case "TurnOut":
                    case "LineBarcodeInterval":
                    case "OntimeReviseSectionDeviationLine":
                    case "OntimeReviseSectionDeviationHorizontal":
                    case "OntimeReviseTheta":
                    case "UpdateDeltaPositionRange":
                    case "OneTimeRevise":
                    case "VChangeSafetyDistance":
                        temp = ReadSafetyDataXML((XmlElement)item);
                        //moveControlConfig.Safety.Add(
                        //    (EnumMoveControlSafetyType)Enum.Parse(typeof(EnumMoveControlSafetyType), item.Name),
                        //    temp);
                        moveControlConfig.Safety[(EnumMoveControlSafetyType)Enum.Parse(typeof(EnumMoveControlSafetyType), item.Name)] = temp;
                        break;
                    default:
                        break;
                }
            }
        }

        private void ReadSensorByBpassXML(XmlElement element)
        {
            SafetyData temp;

            foreach (XmlNode item in element.ChildNodes)
            {
                temp = new SafetyData();

                switch (item.Name)
                {
                    case "Charging":
                    case "ForkHome":
                    case "BeamSensor":
                    case "BeamSensorTR":
                    case "TRFlowStart":
                    case "BeamSensorR2000":
                    case "R2000FlowStat":
                    case "Bumper":
                    case "CheckAxisState":
                    case "TRPathMonitoring":
                    case "EndPositionOffset":
                    case "SecondCorrectionBySide":
                        temp.Enable = (item.InnerText == "Enable");
                        //moveControlConfig.SensorByPass.Add(
                        //    (EnumSensorSafetyType)Enum.Parse(typeof(EnumSensorSafetyType), item.Name),
                        //    temp);
                        moveControlConfig.SensorByPass[(EnumSensorSafetyType)Enum.Parse(typeof(EnumSensorSafetyType), item.Name)] = temp;
                        break;
                    default:
                        break;
                }
            }
        }

        private void ReadR2000ParameterXML(XmlElement element, ref AGVTurnParameter r2000Parameter)
        {
            AxisData temp;
            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "InnerWheelTurn":
                    case "OuterWheelTurn":
                    case "InnerWheelMove":
                    case "OuterWheelMove":
                        temp = ReadAxisData((XmlElement)item);
                        r2000Parameter.R2000Parameter.Add(
                            (EnumR2000Parameter)Enum.Parse(typeof(EnumR2000Parameter), item.Name),
                            temp);
                        break;
                    default:
                        break;
                }
            }
        }

        private AGVTurnParameter ReadTurnTML(XmlElement element)
        {
            AGVTurnParameter temp = new AGVTurnParameter();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "R":
                        temp.R = double.Parse(item.InnerText);
                        break;
                    case "Velocity":
                        temp.Velocity = double.Parse(item.InnerText);
                        break;
                    case "VChangeSafetyDistance":
                        temp.VChangeSafetyDistance = double.Parse(item.InnerText);
                        break;
                    case "CloseReviseDistance":
                        temp.CloseReviseDistance = double.Parse(item.InnerText);
                        break;
                    case "Distance":
                        temp.Distance = double.Parse(item.InnerText);
                        break;
                    case "SafetyVelocityRange":
                        temp.SafetyVelocityRange = double.Parse(item.InnerText);
                        break;
                    case "AxisParameter":
                        temp.AxisParameter = ReadAxisData((XmlElement)item);
                        break;
                    case "R2000Parameter":
                        ReadR2000ParameterXML((XmlElement)item, ref temp);
                        break;
                    default:
                        break;
                }
            }

            return temp;
        }

        private void ReadTurnTypeXML(XmlElement element)
        {
            AGVTurnParameter temp;

            foreach (XmlNode item in element.ChildNodes)
            {
                temp = new AGVTurnParameter();

                switch (item.Name)
                {
                    case "TR50":
                    case "TR350":
                        temp = ReadTurnTML((XmlElement)item);
                        temp.TRPathMonitoringSafetyRange = temp.R * Math.PI / 2 / 5;
                        moveControlConfig.TurnParameter.Add(
                            (EnumAddressAction)Enum.Parse(typeof(EnumAddressAction), item.Name),
                            temp);
                        break;

                    case "R2000":
                        temp = ReadTurnTML((XmlElement)item);
                        moveControlConfig.TurnParameter.Add(
                            (EnumAddressAction)Enum.Parse(typeof(EnumAddressAction), item.Name),
                            temp);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ReadMoveControlConfigXML(string path)
        {
            if (path == null || path == "")
            {
                WriteLog("MoveControl", "3", device, "", "MoveControlConfig 路徑錯誤為null或空值,請檢查程式內部的string.");
                return;
            }

            XmlDocument doc = new XmlDocument();

            string xmlPath = Path.Combine(Environment.CurrentDirectory, path);

            if (!File.Exists(xmlPath))
            {
                WriteLog("MoveControl", "3", device, "", "找不到MotionParameter.xml.");
                return;
            }

            moveControlConfig = new MoveControlConfig();

            doc.Load(xmlPath);
            var rootNode = doc.DocumentElement;

            foreach (XmlNode item in rootNode.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Move":
                        moveControlConfig.Move = ReadAxisData((XmlElement)item);
                        break;
                    case "Turn":
                        moveControlConfig.Turn = ReadAxisData((XmlElement)item);
                        break;
                    case "EQ":
                        moveControlConfig.EQ = ReadAxisData((XmlElement)item);
                        break;
                    case "SecondCorrectionX":
                        moveControlConfig.SecondCorrectionX = Int16.Parse(item.InnerText);
                        break;
                    case "TurnTimeoutValue":
                        moveControlConfig.TurnTimeoutValue = Int16.Parse(item.InnerText);
                        break;
                    case "SlowStopTimeoutValue":
                        moveControlConfig.SlowStopTimeoutValue = Int16.Parse(item.InnerText);
                        break;
                    case "MoveStartWaitTime":
                        moveControlConfig.MoveStartWaitTime = double.Parse(item.InnerText);
                        break;
                    case "Sr2000ConfigPath":
                        moveControlConfig.Sr2000ConfigPath = item.InnerText;
                        break;
                    case "ElmoConfigPath":
                        moveControlConfig.ElmoConfigPath = item.InnerText;
                        break;
                    case "OnTimeReviseConfigPath":
                        moveControlConfig.OnTimeReviseConfigPath = item.InnerText;
                        break;
                    case "SleepTime":
                        moveControlConfig.SleepTime = Int16.Parse(item.InnerText);
                        break;
                    case "CSVLogInterval":
                        moveControlConfig.CSVLogInterval = Int16.Parse(item.InnerText);
                        break;
                    case "PauseDelateTime":
                        moveControlConfig.PauseDelateTime = Int16.Parse(item.InnerText);
                        break;
                    case "LowVelocity":
                        moveControlConfig.LowVelocity = double.Parse(item.InnerText);
                        break;
                    case "MoveCommandDistanceMagnification":
                        moveControlConfig.MoveCommandDistanceMagnification = double.Parse(item.InnerText);
                        break;
                    case "MoveCommandDistanceConstant":
                        moveControlConfig.MoveCommandDistanceConstant = double.Parse(item.InnerText);
                        break;
                    case "StartWheelAngleRange":
                        moveControlConfig.StartWheelAngleRange = double.Parse(item.InnerText);
                        break;
                    case "ReserveSafetyDistance":
                        moveControlConfig.ReserveSafetyDistance = double.Parse(item.InnerText);
                        break;
                    case "NormalStopDistance":
                        moveControlConfig.NormalStopDistance = double.Parse(item.InnerText);
                        break;
                    case "VelocitySafetyRange":
                        moveControlConfig.VelocitySafetyRange = double.Parse(item.InnerText);
                        break;
                    case "VChangeSafetyDistanceMagnification":
                        moveControlConfig.VChangeSafetyDistanceMagnification = double.Parse(item.InnerText);
                        break;
                    case "SafteyDistance":
                        ReadSafetyDistanceXML((XmlElement)item);
                        break;
                    case "Safety":
                        ReadSafetyXML((XmlElement)item);
                        break;
                    case "SensorByPass":
                        ReadSensorByBpassXML((XmlElement)item);
                        break;
                    case "TurnType":
                        ReadTurnTypeXML((XmlElement)item);
                        break;
                    default:
                        break;
                }
            }

            if (moveControlConfig.Move.Velocity > 1000)
            {
                moveControlConfig.Move.Velocity = 1000;
                WriteLog("MoveControl", "7", device, "", "推測速度超過1000 elmo drive會出問題, 強制降回1000!");
            }
        }
        #endregion

        #region SR2000 Initail
        private void InitailSr2000(string path)
        {
            if (path == null || path == "")
            {
                WriteLog("MoveControl", "3", device, "", "Sr2000ConfigPath 路徑錯誤為null或空值,請檢查MoveControlConfig內的Sr2000ConfigPath.");
                return;
            }

            XmlDocument doc = new XmlDocument();
            Sr2000Config sr2000Config;
            Sr2000Driver driverSr2000;
            string xmlPath = path;

            if (!File.Exists(xmlPath))
            {
                WriteLog("MoveControl", "3", device, "", "找不到Sr2000ConfigPath.xml.");
                return;
            }

            doc.Load(xmlPath);
            var rootNode = doc.DocumentElement;     // <Motion>         
            int i = 0;

            foreach (XmlNode item in rootNode.ChildNodes)
            {
                sr2000Config = ConvertXmlElementToSr2000Config((XmlElement)item);
                driverSr2000 = new Sr2000Driver(sr2000Config, theMapInfo, i, alarmHandler);
                if (!driverSr2000.GetConnect())
                    SR2000Connected = false;

                DriverSr2000List.Add(driverSr2000);
                i++;
            }
        }

        private void ReadTrapezoidalCrrection(XmlElement element, Sr2000Config sr2000Config)
        {
            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Up":
                        sr2000Config.Up = double.Parse(item.InnerText);
                        break;
                    case "Down":
                        sr2000Config.Down = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }
        }

        private Sr2000Config ConvertXmlElementToSr2000Config(XmlElement element)
        {
            Sr2000Config sr2000Config = new Sr2000Config();

            sr2000Config.ViewCenter = new MapPosition();
            sr2000Config.ViewOffset = new MapPosition();
            sr2000Config.Change = new MapPosition();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "ID":
                        sr2000Config.ID = item.InnerText;
                        break;
                    case "IP":
                        sr2000Config.IP = item.InnerText;
                        break;
                    case "ReaderToCenterDegree":
                        sr2000Config.ReaderToCenterDegree = double.Parse(item.InnerText);
                        break;
                    case "ReaderToCenterDistance":
                        sr2000Config.ReaderToCenterDistance = double.Parse(item.InnerText);
                        break;
                    case "ReaderSetupAngle":
                        sr2000Config.ReaderSetupAngle = double.Parse(item.InnerText);
                        break;
                    case "OffsetX":
                        sr2000Config.ViewOffset.X = float.Parse(item.InnerText);
                        break;
                    case "OffsetY":
                        sr2000Config.ViewOffset.Y = float.Parse(item.InnerText);
                        break;
                    case "OffsetTheta":
                        sr2000Config.OffsetTheta = double.Parse(item.InnerText);
                        break;
                    case "DatumX":
                        sr2000Config.ViewCenter.X = float.Parse(item.InnerText);
                        break;
                    case "DatumY":
                        sr2000Config.ViewCenter.Y = float.Parse(item.InnerText);
                        break;
                    case "ChangeX":
                        sr2000Config.Change.X = float.Parse(item.InnerText);
                        break;
                    case "ChangeY":
                        sr2000Config.Change.Y = float.Parse(item.InnerText);
                        break;
                    case "TimeOutValue":
                        sr2000Config.TimeOutValue = Int16.Parse(item.InnerText);
                        break;
                    case "SleepTime":
                        sr2000Config.SleepTime = Int16.Parse(item.InnerText);
                        break;
                    case "LogMode":
                        sr2000Config.LogMode = bool.Parse(item.InnerText);
                        break;
                    case "TrapezoidalCrrection":
                        ReadTrapezoidalCrrection((XmlElement)item, sr2000Config);
                        break;
                    case "DistanceSafetyRange":
                        sr2000Config.DistanceSafetyRange = double.Parse(item.InnerText) / 100;
                        break;
                    default:
                        break;
                }
            }

            sr2000Config.Target = new MapPosition(sr2000Config.ViewCenter.X + sr2000Config.ViewOffset.X, sr2000Config.ViewCenter.Y + sr2000Config.ViewOffset.Y);

            return sr2000Config;
        }
        #endregion

        #region ReadReviseConfig
        private void ReadOntimeReviseConfigXML(string path)
        {
            if (path == null || path == "")
            {
                WriteLog("MoveControl", "3", device, "", "OnTimeReviseConfigPath 路徑錯誤為null或空值,請檢查MoveControlConfig內的OnTimeReviseConfigPath.");
                return;
            }

            XmlDocument doc = new XmlDocument();

            string xmlPath = Path.Combine(Environment.CurrentDirectory, path);

            if (!File.Exists(xmlPath))
            {
                WriteLog("MoveControl", "3", device, "", "找不到OnTimeReviseConfigPath.xml.");
                return;
            }

            doc.Load(xmlPath);
            ontimeReviseConfig = ConvertXmlElementToLineReviseConfig((XmlElement)doc.DocumentElement);
        }

        private ThetaSectionDeviation ConvertXmlElementToThetaSectionDeviation(XmlElement element)
        {
            ThetaSectionDeviation thetaSectionDeviation = new ThetaSectionDeviation();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Theta":
                        thetaSectionDeviation.Theta = double.Parse(item.InnerText);
                        break;
                    case "SectionDeviation":
                        thetaSectionDeviation.SectionDeviation = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return thetaSectionDeviation;
        }

        private List<SpeedAndMaxTheta> ConvertXmlElementToSpeedToMaxTheta(XmlElement element)
        {
            List<SpeedAndMaxTheta> listSpeedAndMaxTheta = new List<SpeedAndMaxTheta>();
            SpeedAndMaxTheta speedAndMaxTheta;

            foreach (XmlNode item in element.ChildNodes)
            {
                speedAndMaxTheta = new SpeedAndMaxTheta();
                foreach (XmlNode nodeItem in item)
                {
                    switch (nodeItem.Name)
                    {
                        case "Speed":
                            speedAndMaxTheta.Speed = double.Parse(nodeItem.InnerText);
                            break;
                        case "MaxTheta":
                            speedAndMaxTheta.MaxTheta = double.Parse(nodeItem.InnerText);
                            break;
                        default:
                            break;
                    }
                }

                listSpeedAndMaxTheta.Add(speedAndMaxTheta);
            }

            return listSpeedAndMaxTheta;
        }

        private OntimeReviseConfig ConvertXmlElementToLineReviseConfig(XmlElement element)
        {
            OntimeReviseConfig ontimeReviseConfig = new OntimeReviseConfig();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "MaxVelocity":
                        ontimeReviseConfig.MaxVelocity = double.Parse(item.InnerText);
                        break;
                    case "MinVelocity":
                        ontimeReviseConfig.MinVelocity = double.Parse(item.InnerText);
                        break;
                    case "ThetaSpeed":
                        ontimeReviseConfig.ThetaSpeed = double.Parse(item.InnerText);
                        break;
                    case "LinePriority":
                        ontimeReviseConfig.LinePriority = ConvertXmlElementToThetaSectionDeviation((XmlElement)item);
                        break;
                    case "HorizontalPriority":
                        ontimeReviseConfig.HorizontalPriority = ConvertXmlElementToThetaSectionDeviation((XmlElement)item);
                        break;
                    case "ModifyPriority":
                        ontimeReviseConfig.ModifyPriority = ConvertXmlElementToThetaSectionDeviation((XmlElement)item);
                        break;
                    case "Return0ThetaPriority":
                        ontimeReviseConfig.Return0ThetaPriority = ConvertXmlElementToThetaSectionDeviation((XmlElement)item);
                        break;
                    case "SpeedToMaxTheta":
                        ontimeReviseConfig.SpeedToMaxTheta = ConvertXmlElementToSpeedToMaxTheta((XmlElement)item);
                        break;

                    default:
                        break;
                }
            }

            return ontimeReviseConfig;
        }
        #endregion

        #region Position { barcode, elmo encoder, delta, real }更新.
        private MapPosition GetMapPosition(SectionLine sectionLine, double encode)
        {
            double x = sectionLine.Start.X + (sectionLine.End.X - sectionLine.Start.X) * (encode - sectionLine.EncoderStart) / (sectionLine.EncoderEnd - sectionLine.EncoderStart);
            double y = sectionLine.Start.Y + (sectionLine.End.Y - sectionLine.Start.Y) * (encode - sectionLine.EncoderStart) / (sectionLine.EncoderEnd - sectionLine.EncoderStart);
            MapPosition returnPosition = new MapPosition(x, y);

            return returnPosition;
        }

        private double MapPositionToEncoder(SectionLine sectionLine, MapPosition position)
        {
            if (sectionLine.Start.X == sectionLine.End.X)
            {
                return sectionLine.EncoderStart + (sectionLine.EncoderEnd - sectionLine.EncoderStart) *
                      (position.Y - sectionLine.Start.Y) / (sectionLine.End.Y - sectionLine.Start.Y);
            }
            else if (sectionLine.Start.Y == sectionLine.End.Y)
            {
                return sectionLine.EncoderStart + (sectionLine.EncoderEnd - sectionLine.EncoderStart) *
                      (position.X - sectionLine.Start.X) / (sectionLine.End.X - sectionLine.Start.X);
            }
            else
            {
                WriteLog("MoveControl", "4", device, "", "不該有再R2000中將Barcode Position轉回Encoder的情況.");
                return 0;
            }
        }

        private void UpdateReal()
        {
            if (safetyData.NowMoveState == EnumMoveState.Error)
                return;

            if (command.SectionLineList.Count != 0)
            {
                double realElmoEncode = location.ElmoEncoder + location.Delta + location.Offset;
                location.RealEncoder = realElmoEncode;
                double encoderPosition = realElmoEncode;

                if (command.SectionLineList[command.IndexOflisSectionLine].DirFlag)
                {
                    if (realElmoEncode > command.SectionLineList[command.IndexOflisSectionLine].TransferPositionEnd)
                        realElmoEncode = command.SectionLineList[command.IndexOflisSectionLine].TransferPositionEnd;
                    else if (realElmoEncode < command.SectionLineList[command.IndexOflisSectionLine].TransferPositionStart)
                        realElmoEncode = command.SectionLineList[command.IndexOflisSectionLine].TransferPositionStart;

                    if (command.IndexOflisSectionLine != 0 && command.IndexOflisSectionLine != command.SectionLineList.Count - 1)
                    {
                        if (encoderPosition > command.SectionLineList[command.IndexOflisSectionLine].EncoderEnd)
                            encoderPosition = command.SectionLineList[command.IndexOflisSectionLine].EncoderEnd;
                        else if (encoderPosition < command.SectionLineList[command.IndexOflisSectionLine].EncoderStart)
                            encoderPosition = command.SectionLineList[command.IndexOflisSectionLine].EncoderStart;
                    }
                }
                else
                {
                    if (realElmoEncode < command.SectionLineList[command.IndexOflisSectionLine].TransferPositionEnd)
                        realElmoEncode = command.SectionLineList[command.IndexOflisSectionLine].TransferPositionEnd;
                    else if (realElmoEncode > command.SectionLineList[command.IndexOflisSectionLine].TransferPositionStart)
                        realElmoEncode = command.SectionLineList[command.IndexOflisSectionLine].TransferPositionStart;

                    if (command.IndexOflisSectionLine != 0 && command.IndexOflisSectionLine != command.SectionLineList.Count - 1)
                    {
                        if (encoderPosition < command.SectionLineList[command.IndexOflisSectionLine].EncoderEnd)
                            encoderPosition = command.SectionLineList[command.IndexOflisSectionLine].EncoderEnd;
                        else if (encoderPosition > command.SectionLineList[command.IndexOflisSectionLine].EncoderStart)
                            encoderPosition = command.SectionLineList[command.IndexOflisSectionLine].EncoderStart;
                    }
                }

                location.Real.Position = GetMapPosition(command.SectionLineList[command.IndexOflisSectionLine], realElmoEncode);
                location.Encoder.Position = GetMapPosition(command.SectionLineList[command.IndexOflisSectionLine], encoderPosition);
                Vehicle.Instance.VehicleLocation.RealPosition = location.Real.Position;
            }
        }

        private void UpdateDelta(bool secondCorrection)
        {
            if (safetyData.NowMoveState == EnumMoveState.Error)
                return;

            double realEncoder;

            if (secondCorrection && moveControlConfig.SensorByPass[EnumSensorSafetyType.SecondCorrectionBySide].Enable)
                realEncoder = MapPositionToEncoder(command.SectionLineList[command.IndexOflisSectionLine], location.Barcode.BarcodeCenter);
            else
                realEncoder = MapPositionToEncoder(command.SectionLineList[command.IndexOflisSectionLine], location.Barcode.Position);

            // 此Barcode是多久之前的資料,基本上為正值(s).
            double deltaTime = ((double)location.ScanTime + (DateTime.Now - location.BarcodeGetDataTime).TotalMilliseconds) / 1000;
            // 真實Barcode回推的RealEncoder需要再加上這個時間*速度(Elmo速度本身就帶正負號).
            realEncoder = realEncoder + location.Velocity * deltaTime;
            // RealEncoder = elmoEncoder + offset + delta.
            location.Delta = realEncoder - (location.ElmoEncoder + location.Offset);

            if (location.Encoder != null && moveControlConfig.Safety[EnumMoveControlSafetyType.UpdateDeltaPositionRange].Enable)
            {
                double deltaTimeDistance = Math.Abs(location.Velocity) *
               ((DateTime.Now - location.Barcode.GetDataTime).TotalMilliseconds + moveControlConfig.SleepTime + location.Barcode.ScanTime) / 1000;
                double distance = Math.Sqrt(Math.Pow(location.Encoder.Position.X - location.Barcode.Position.X, 2) +
                                            Math.Pow(location.Encoder.Position.Y - location.Barcode.Position.Y, 2));
                if (distance > moveControlConfig.Safety[EnumMoveControlSafetyType.UpdateDeltaPositionRange].Range + deltaTimeDistance)
                {
                    EMSControl("UpdateDelta中Barcode讀取位置和Encode(座標)差距" +
                        distance.ToString("0.0") + "mm,已超過安全設置的" +
                        moveControlConfig.Safety[EnumMoveControlSafetyType.UpdateDeltaPositionRange].Range.ToString("0") +
                        "mm,因此啟動EMS! Encoder ( " + location.Encoder.Position.X.ToString("0") + ", " + location.Encoder.Position.Y.ToString("0") +
                        " ), Barcoder ( " + location.Barcode.Position.X.ToString("0") + ", " + location.Barcode.Position.Y.ToString("0") +
                        " ), realEncoder : " + location.RealEncoder.ToString("0"));
                }
            }
        }

        private void UpdateThetaSectionDeviation()
        {
            AGVPosition agvPosition = null;

            int index = -1;
            for (int i = 0; i < DriverSr2000List.Count; i++)
            {
                agvPosition = DriverSr2000List[i].GetAGVPosition();

                if (agvPosition != null)
                {
                    if (computeFunction.IsSameAngle(agvPosition.BarcodeAngleInMap, agvPosition.AGVAngle, ControlData.WheelAngle))
                    {
                        index = i;
                        break;
                    }
                    else
                        agvPosition = null;
                }
            }

            if (agvPosition != null)
            {
                if (location.ThetaAndSectionDeviation == null ||
                !(location.ThetaAndSectionDeviation.Count == agvPosition.Count && location.ThetaAndSectionDeviation.Index == index))
                {
                    double sectionDeviation = 0;
                    double theta =
                        agvPosition.AGVAngle + (command.SectionLineList[command.IndexOflisSectionLine].DirFlag ? 0 : 180) +
                        ControlData.WheelAngle - command.SectionLineList[command.IndexOflisSectionLine].SectionAngle;

                    while (theta > 180 || theta <= -180)
                    {
                        if (theta > 180)
                            theta -= 360;
                        else if (theta <= -180)
                            theta += 360;
                    }

                    switch (command.SectionLineList[command.IndexOflisSectionLine].SectionAngle)
                    {
                        case 0:
                            sectionDeviation = agvPosition.Position.Y - command.SectionLineList[command.IndexOflisSectionLine].Start.Y;
                            ControlData.SectionDeviationOffset = command.EndOffsetY;
                            break;
                        case 180:
                            sectionDeviation = -(agvPosition.Position.Y - command.SectionLineList[command.IndexOflisSectionLine].Start.Y);
                            ControlData.SectionDeviationOffset = -command.EndOffsetY;
                            break;
                        case 90:
                            sectionDeviation = agvPosition.Position.X - command.SectionLineList[command.IndexOflisSectionLine].Start.X;
                            ControlData.SectionDeviationOffset = command.EndOffsetX;
                            break;
                        case -90:
                            sectionDeviation = -(agvPosition.Position.X - command.SectionLineList[command.IndexOflisSectionLine].Start.X);
                            ControlData.SectionDeviationOffset = -command.EndOffsetX;
                            break;
                        default:
                            break;
                    }

                    location.ThetaAndSectionDeviation = new ThetaSectionDeviation(theta, sectionDeviation, agvPosition.Count, index);
                }
            }
            else
            {
                location.ThetaAndSectionDeviation = null;
            }
        }

        private void ThetaSectionDeviationSafetyCheck()
        {
            if (location.ThetaAndSectionDeviation == null)
                return;

            if (moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseTheta].Enable)
            {
                if (Math.Abs(location.ThetaAndSectionDeviation.Theta) >
                    moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range)
                {
                    EMSControl("角度偏差" + location.ThetaAndSectionDeviation.Theta.ToString("0.0") +
                        "度,已超過安全設置的" +
                        moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range.ToString("0.0") +
                        "度,因此啟動EMS!");

                    return;
                }
            }

            if (moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Enable)
            {
                if (ControlData.WheelAngle != 0 && moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal].Enable)
                {
                    if (Math.Abs(location.ThetaAndSectionDeviation.SectionDeviation) > moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal].Range)
                    {
                        EMSControl("橫移偏差" + location.ThetaAndSectionDeviation.SectionDeviation.ToString("0") +
                            "mm,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal].Range.ToString("0") +
                            "mm,因此啟動EMS!");
                    }
                }
                else
                {
                    if (Math.Abs(location.ThetaAndSectionDeviation.SectionDeviation) > moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range)
                    {
                        EMSControl("軌道偏差" + location.ThetaAndSectionDeviation.SectionDeviation.ToString("0") +
                            "mm,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range.ToString("0") +
                            "mm,因此啟動EMS!");
                    }
                }
            }
        }

        private bool UpdateSR2000()
        {
            if (!SimulationMode)
            {
                AGVPosition agvPosition = null;
                int index = 0;

                for (; index < DriverSr2000List.Count; index++)
                {
                    agvPosition = DriverSr2000List[index].GetAGVPosition();
                    if (agvPosition != null)
                    {
                        if (agvPosition.Type == EnumBarcodeMaterial.Papper)
                            agvPosition = null;
                        else if (MoveState == EnumMoveState.Idle || MoveState == EnumMoveState.Error)
                            break;
                        else if (computeFunction.IsSameAngle(agvPosition.BarcodeAngleInMap, agvPosition.AGVAngle, ControlData.WheelAngle))
                            break;
                        else
                            agvPosition = null;
                    }
                }

                if (agvPosition != null && !(location.LastBarcodeCount == agvPosition.Count && location.IndexOfSr2000List == index))
                {   // 有資料且和上次的不是同一筆.
                    location.LastBarcodeCount = agvPosition.Count;
                    location.IndexOfSr2000List = index;
                    location.Barcode = agvPosition;
                    location.ScanTime = agvPosition.ScanTime;
                    location.BarcodeGetDataTime = agvPosition.GetDataTime;

                    Vehicle.Instance.VehicleLocation.BarcodePosition = location.Barcode.Position;
                    return true;
                }
            }
            else
            {
                if (location.Barcode == null)
                {
                    MapPosition tempPosition = new MapPosition(0, 0);
                    location.Barcode = new AGVPosition(tempPosition, tempPosition, 0, 0, 20, DateTime.Now, 0, 0, EnumBarcodeMaterial.Iron);
                    //location.Barcode = new AGVPosition(tempPosition, 90, 0, 20, DateTime.Now, 0, 0, EnumBarcodeMaterial.Iron);
                    return true;
                }
            }

            return false;
        }

        private void UpdateElmo()
        {
            if (!SimulationMode)
            {
                ElmoAxisFeedbackData elmoData = elmoDriver.ElmoGetFeedbackData(EnumAxis.XFL);

                location.GTPosition = elmoDriver.ElmoGetPosition(EnumAxis.GT);
                location.GTMoveCompelete = elmoDriver.MoveCompelete(EnumAxis.GT);
                location.GXMoveCompelete = elmoDriver.MoveCompelete(EnumAxis.GX);

                if (elmoData != null)
                {
                    location.Velocity = elmoDriver.ElmoGetVelocity(EnumAxis.GX);
                    location.ElmoGetDataTime = elmoData.GetDataTime;
                    // 此筆Elmo資料是多久之前的,基本上時間會是正值(s).
                    double deltaTime = ((DateTime.Now - location.ElmoGetDataTime).TotalMilliseconds + moveControlConfig.SleepTime / 2) / 1000;
                    // 真實Encoder需要再加上這個時間*速度(Elmo速度本身就帶正負號).
                    location.ElmoEncoder = elmoData.Feedback_Position + location.Velocity * deltaTime;
                }
            }
            else
            {
                if (!simulationIsMoving)
                {
                    location.Velocity = 0;
                }
                else
                {
                    location.Velocity = ControlData.DirFlag ? ControlData.RealVelocity : -ControlData.RealVelocity;
                    location.ElmoEncoder = location.ElmoEncoder + (double)moveControlConfig.SleepTime / 1000 * location.Velocity;
                }

                location.ElmoGetDataTime = DateTime.Now;
            }
        }

        private void SafetyTurnOutAndLineBarcodeInterval(bool newBarcodeData)
        {
            if (safetyData.NowMoveState == EnumMoveState.Error)
                return;

            safetyData.NowMoveState = MoveState;
            if (safetyData.NowMoveState != safetyData.LastMoveState)
            {
                safetyData.LastReadBarcodeReset = true;

                if (safetyData.NowMoveState == EnumMoveState.Moving &&
                   (safetyData.LastMoveState == EnumMoveState.R2000 || safetyData.LastMoveState == EnumMoveState.TR))
                {
                    safetyData.IsTurnOut = true;
                    safetyData.TurnOutElmoEncoder = location.ElmoEncoder;
                }
            }

            if (safetyData.IsTurnOut && !SimulationMode && moveControlConfig.Safety[EnumMoveControlSafetyType.TurnOut].Enable)
            {
                if (newBarcodeData)
                {
                    WriteLog("MoveControl", "7", device, "", "出彎" + Math.Abs(location.ElmoEncoder - safetyData.TurnOutElmoEncoder).ToString("0") +
                        "mm讀取到Barcode,再安全設置的" +
                        moveControlConfig.Safety[EnumMoveControlSafetyType.TurnOut].Range.ToString("0") +
                        "mm內!");
                    safetyData.IsTurnOut = false;
                }
                else
                {
                    if (Math.Abs(location.ElmoEncoder - safetyData.TurnOutElmoEncoder) >
                        moveControlConfig.Safety[EnumMoveControlSafetyType.TurnOut].Range)
                    {
                        SendAlarmCode(130000);
                        EMSControl("出彎" + Math.Abs(location.ElmoEncoder - safetyData.TurnOutElmoEncoder).ToString("0") +
                            "mm未讀取到Barcode,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.TurnOut].Range.ToString("0") +
                            "mm,因此啟動EMS!");
                    }
                }
            }

            if (safetyData.NowMoveState == EnumMoveState.Moving && !SimulationMode &&
                moveControlConfig.Safety[EnumMoveControlSafetyType.LineBarcodeInterval].Enable)
            {
                if (safetyData.LastReadBarcodeReset)
                {
                    if (newBarcodeData)
                    {
                        safetyData.LastReadBarcodeReset = false;
                        safetyData.LastReadBarcodeElmoEncoder = location.ElmoEncoder;
                    }
                }
                else if (newBarcodeData)
                {
                    safetyData.LastReadBarcodeElmoEncoder = location.ElmoEncoder;
                }
                else
                {
                    if (Math.Abs(location.ElmoEncoder - safetyData.LastReadBarcodeElmoEncoder) >
                        moveControlConfig.Safety[EnumMoveControlSafetyType.LineBarcodeInterval].Range)
                    {
                        SendAlarmCode(130001);
                        EMSControl("直線超過" + Math.Abs(location.ElmoEncoder - safetyData.LastReadBarcodeElmoEncoder).ToString("0") +
                            "mm未讀取到Barcode,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.LineBarcodeInterval].Range.ToString("0") +
                            "mm,因此啟動EMS!");
                    }
                }
            }

            safetyData.LastMoveState = safetyData.NowMoveState;
        }

        private void UpdatePosition(bool secondCorrection = false)
        {
            LoopTime = loopTimeTimer.ElapsedMilliseconds;
            loopTimeTimer.Reset();
            loopTimeTimer.Start();

            UpdateElmo();
            bool newBarcodeData = UpdateSR2000();

            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
            {
                if (MoveState != EnumMoveState.TR && MoveState != EnumMoveState.R2000)
                {
                    UpdateThetaSectionDeviation();
                    ThetaSectionDeviationSafetyCheck();

                    if (location.ThetaAndSectionDeviation != null && ControlData.EQVChange)
                    {
                        location.ThetaAndSectionDeviation.SectionDeviation += ControlData.SectionDeviationOffset;
                        location.ThetaAndSectionDeviation.Theta += command.EndOffsetTheta;
                    }

                    if (newBarcodeData)
                        UpdateDelta(secondCorrection);
                }

                UpdateReal();
                SafetyTurnOutAndLineBarcodeInterval(newBarcodeData);
            }
            else
            {
                if (Vehicle.Instance.AutoState != EnumAutoState.Auto && newBarcodeData)
                {
                    location.Real = location.Barcode;
                    location.Real.AGVAngle = computeFunction.GetAGVAngle(location.Real.AGVAngle);
                    Vehicle.Instance.VehicleLocation.RealPosition = location.Real.Position;
                    Vehicle.Instance.VehicleLocation.VehicleAngle = location.Real.AGVAngle;
                }
            }
        }
        #endregion

        #region CommandControl
        private bool IsInTRPath(EnumAddressAction type, double encoder, double startAngle, double targetAngle)
        {
            double nowAngle = location.GTPosition;
            nowAngle -= startAngle;
            targetAngle -= startAngle;

            if (targetAngle < 0)
            {
                targetAngle = -targetAngle;
                nowAngle = -nowAngle;
            }

            if (nowAngle < 0)
                nowAngle = 0;
            else if (nowAngle > 90)
                nowAngle = 90;

            double idealAngle = GetTRFlowAngleChange(Math.Abs(location.ElmoEncoder - ControlData.TurnStartEncoder));

            return Math.Abs(idealAngle - nowAngle) <= 15;
        }

        private bool TurnGorupAxisNearlyAngle()
        {
            double range = 15;
            double angle_TFL = elmoDriver.ElmoGetPosition(EnumAxis.TFL);
            double angle_TFR = elmoDriver.ElmoGetPosition(EnumAxis.TFR);
            double angle_TRL = elmoDriver.ElmoGetPosition(EnumAxis.TRL);
            double angle_TRR = elmoDriver.ElmoGetPosition(EnumAxis.TRR);

            bool result = (Math.Abs(angle_TFL - angle_TFR) <= range) && (Math.Abs(angle_TFL - angle_TRL) <= range) &&
                          (Math.Abs(angle_TFL - angle_TRR) <= range) && (Math.Abs(angle_TFR - angle_TRL) <= range) &&
                          (Math.Abs(angle_TFR - angle_TRR) <= range) && (Math.Abs(angle_TRL - angle_TRR) <= range);

            return result;
        }

        private void TRControl_SimulationMode(int wheelAngle, EnumAddressAction type)
        {
            double velocity = moveControlConfig.TurnParameter[type].Velocity;
            double r = moveControlConfig.TurnParameter[type].R;

            WriteLog("MoveControl", "7", device, "", "start, velocity : " + velocity.ToString("0") + ", r : " + r.ToString("0") +
                ", 舵輪將旋轉至 " + wheelAngle.ToString("0") + "度!");
            MoveState = EnumMoveState.TR;
            ControlData.NowAction = type;

            double distance = r * 2;
            double simulationDisntace = r * Math.PI / 2;

            ControlData.TurnStartEncoder = location.ElmoEncoder;

            switch (wheelAngle)
            {
                case 0:
                    BeamSensorSingleOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                    break;
                case 90:
                    BeamSensorSingleOn((ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                    break;
                case -90:
                    BeamSensorSingleOn((ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                    break;
                default:
                    EMSControl("switch (TRWheelAngle) default..EMS.");
                    break;
            }

            elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.TurnParameter[type].AxisParameter.Velocity, EnumMoveType.Absolute,
                                moveControlConfig.TurnParameter[type].AxisParameter.Acceleration,
                                moveControlConfig.TurnParameter[type].AxisParameter.Deceleration,
                                moveControlConfig.TurnParameter[type].AxisParameter.Jerk);

            while (Math.Abs(location.ElmoEncoder - ControlData.TurnStartEncoder) < simulationDisntace)
            {
                UpdatePosition();
                SensorSafety();
                if (ControlData.FlowStopRequeset || MoveState == EnumMoveState.Error)
                    return;
                Thread.Sleep(moveControlConfig.SleepTime);
            }

            command.IndexOflisSectionLine++;

            switch (wheelAngle)
            {
                case 0:
                    BeamSensorOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                    DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                    break;
                case 90:
                    BeamSensorOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                    DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                    break;
                case -90:
                    BeamSensorOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                    DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                    break;
                default:
                    EMSControl("switch (wheelAngle) default..EMS..");
                    break;
            }

            ControlData.WheelAngle = wheelAngle;
            location.Delta = location.Delta + (ControlData.DirFlag ? (distance - (location.ElmoEncoder - ControlData.TurnStartEncoder)) :
                                               -(distance - (ControlData.TurnStartEncoder - location.ElmoEncoder)));
            UpdatePosition();

            MoveState = EnumMoveState.Moving;
            safetyData.TurningByPass = false;
            ControlData.CanPause = true;
            WriteLog("MoveControl", "7", device, "", "end.");
        }

        private void TRControl(int wheelAngle, EnumAddressAction type)
        {
            ControlData.CanPause = false;
            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensorTR].Enable)
                safetyData.TurningByPass = true;

            if (SimulationMode)
            {
                TRControl_SimulationMode(wheelAngle, type);
                return;
            }

            double velocity = moveControlConfig.TurnParameter[type].Velocity;
            double r = moveControlConfig.TurnParameter[type].R;
            double safetyVelocityRange = moveControlConfig.TurnParameter[type].SafetyVelocityRange;

            double changeToNextSectionLineWheelAngle = (wheelAngle + ControlData.WheelAngle) / 2;

            WriteLog("MoveControl", "7", device, "", "start, velocity : " + velocity.ToString("0") + ", r : " + r.ToString("0") +
                ", 舵輪將旋轉至 " + wheelAngle.ToString("0") + "度, 旋轉至 " + changeToNextSectionLineWheelAngle.ToString("0") + "!");

            MoveState = EnumMoveState.TR;
            ControlData.NowAction = type;

            double agvVelocity = Math.Abs(location.Velocity);
            double distance = r * 2;

            ControlData.TurnStartEncoder = location.ElmoEncoder;

            switch (wheelAngle)
            {
                case 0:
                    BeamSensorSingleOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                    break;
                case 90:
                    BeamSensorSingleOn((ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                    break;
                case -90:
                    BeamSensorSingleOn((ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                    break;
                default:
                    EMSControl("switch (TRWheelAngle) default..EMS.");
                    break;
            }

            if (!elmoDriver.MoveCompelete(EnumAxis.GT))
            {
                SendAlarmCode(152002);
                WriteLog("MoveControl", "4", device, "", " TR中 GT Moving~");
            }

            if (agvVelocity > velocity + safetyVelocityRange)
            { // 超速GG, 不該發生.
                SendAlarmCode(152000);
                EMSControl("超速.., vel : " + agvVelocity.ToString("0"));
                return;
            }
            else if (Math.Abs(agvVelocity - velocity) <= safetyVelocityRange)
            { // Normal
                elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.TurnParameter[type].AxisParameter.Velocity, EnumMoveType.Absolute,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Acceleration,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Deceleration,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Jerk);
            }
            else
            { // 太慢 處理??
                SendAlarmCode(152003);
                EMSControl("速度過慢.., vel : " + agvVelocity.ToString("0"));
            }

            while (!elmoDriver.WheelAngleCompare(wheelAngle, moveControlConfig.StartWheelAngleRange))
            {
                UpdatePosition();
                SensorSafety();
                if (ControlData.FlowStopRequeset || MoveState == EnumMoveState.Error)
                    return;

                if (moveControlConfig.SensorByPass[EnumSensorSafetyType.TRPathMonitoring].Enable)
                {
                    if (!TurnGorupAxisNearlyAngle())
                    {
                        SendAlarmCode(152003);
                        EMSControl("四輪角度差異過大,EMS!");
                        return;
                    }
                    else if (!IsInTRPath(type, Math.Abs(ControlData.TurnStartEncoder - location.ElmoEncoder), ControlData.WheelAngle, wheelAngle))
                    {
                        SendAlarmCode(152001);
                        EMSControl("不再TR預計路徑上,異常停止!");
                        return;
                    }
                }

                if (changeToNextSectionLineWheelAngle != -1)
                {
                    if ((ControlData.WheelAngle > wheelAngle && location.GTPosition < changeToNextSectionLineWheelAngle) ||
                        (ControlData.WheelAngle < wheelAngle && location.GTPosition > changeToNextSectionLineWheelAngle))
                    {
                        changeToNextSectionLineWheelAngle = -1;
                        command.IndexOflisSectionLine++;

                        switch (wheelAngle)
                        {
                            case 0:
                                BeamSensorOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                                DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                                break;
                            case 90:
                                BeamSensorOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                                DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                                break;
                            case -90:
                                BeamSensorOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                                DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                                break;
                            default:
                                EMSControl("switch (wheelAngle) default..EMS..");
                                break;
                        }

                        WriteLog("MoveControl", "4", device, "", "Section Line + 1 !");
                    }
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            ControlData.WheelAngle = wheelAngle;
            location.Delta = location.Delta + (ControlData.DirFlag ? (distance - (location.ElmoEncoder - ControlData.TurnStartEncoder)) :
                                               -(distance - (ControlData.TurnStartEncoder - location.ElmoEncoder)));
            UpdatePosition();

            MoveState = EnumMoveState.Moving;
            safetyData.TurningByPass = false;
            ControlData.CanPause = true;
            WriteLog("MoveControl", "7", device, "", "end.");
        }

        private bool OkToTurnZero(double outerWheelEncoder, double trunZeroEncoder)
        {
            if (ControlData.DirFlag)
                return outerWheelEncoder >= trunZeroEncoder;
            else
                return outerWheelEncoder <= trunZeroEncoder;
        }

        public void R2000Control_SimulationMode(int wheelAngle)
        {
            WriteLog("MoveControl", "7", device, "", "start, 舵輪往" + (wheelAngle == 1 ? "左旋轉!" : "右旋轉!"));

            int moveDirlag = ControlData.DirFlag ? 1 : -1;
            double outerWheelEncoder;
            double trunZeroEncoder;
            double startOuterWheelEncoder;
            double startEncoder = location.ElmoEncoder;

            double distance = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R * Math.Sqrt(2);

            MoveState = EnumMoveState.R2000;
            ControlData.NowAction = EnumAddressAction.R2000;
            command.IndexOflisSectionLine++;

            if (wheelAngle == 1 || wheelAngle == -1)
                outerWheelEncoder = location.ElmoEncoder;
            else
            {
                EMSControl("R2000取得奇怪的wheelAngle");
                return;
            }

            startOuterWheelEncoder = outerWheelEncoder;
            trunZeroEncoder = outerWheelEncoder + (ControlData.DirFlag ? moveControlConfig.TurnParameter[EnumAddressAction.R2000].Distance :
                                                                        -moveControlConfig.TurnParameter[EnumAddressAction.R2000].Distance);

            WriteLog("MoveControl", "7", device, "", "開始旋轉, startOuterWheelEncoder : " + startOuterWheelEncoder.ToString("0.0") +
                                                ", 預計回正OuterWheelEncode : " + trunZeroEncoder.ToString("0.0"));


            while (!OkToTurnZero(outerWheelEncoder, trunZeroEncoder))
            {
                UpdatePosition();
                SensorSafety();
                outerWheelEncoder = location.ElmoEncoder;

                if (ControlData.FlowStopRequeset || MoveState == EnumMoveState.Error)
                    return;

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            WriteLog("MoveControl", "7", device, "", "開始回正, outerWheelEncoder : " + outerWheelEncoder.ToString("0.0"));

            Thread.Sleep(moveControlConfig.SleepTime * 2);

            double turnZeroEncoder = location.ElmoEncoder;
            double turnZeroDistance = 100;

            while (Math.Abs(turnZeroEncoder - location.ElmoEncoder) < turnZeroDistance)
            {
                UpdatePosition();
                SensorSafety();

                if (ControlData.FlowStopRequeset || MoveState == EnumMoveState.Error)
                    return;

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            location.Delta = location.Delta + (ControlData.DirFlag ? (distance - (location.ElmoEncoder - startEncoder)) :
                                               -(distance - (startEncoder - location.ElmoEncoder)));

            command.IndexOflisSectionLine++;
            UpdatePosition();
            if ((wheelAngle == -1 && ControlData.DirFlag) ||
                (wheelAngle == 1 && !ControlData.DirFlag))
                location.Real.AGVAngle -= 90;
            else
                location.Real.AGVAngle += 90;

            if (location.Real.AGVAngle > 180)
                location.Real.AGVAngle -= 360;
            else if (location.Real.AGVAngle <= -180)
                location.Real.AGVAngle += 360;

            Vehicle.Instance.VehicleLocation.VehicleAngle = location.Real.AGVAngle;

            DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));

            MoveState = EnumMoveState.Moving;
            safetyData.TurningByPass = false;
            ControlData.CanPause = true;
            WriteLog("MoveControl", "7", device, "", " end.");
        }

        public void R2000Control(int wheelAngle)
        {
            ControlData.CanPause = false;
            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensorR2000].Enable)
                safetyData.TurningByPass = true;

            if (SimulationMode)
            {
                R2000Control_SimulationMode(wheelAngle);
                return;
            }

            WriteLog("MoveControl", "7", device, "", "start, 舵輪往" + (wheelAngle == 1 ? "左旋轉!" : "右旋轉!"));

            AxisData leftMove;
            AxisData leftTurn;
            AxisData rightMove;
            AxisData rightTurn;
            int moveDirlag = ControlData.DirFlag ? 1 : -1;
            double outerWheelEncoder;
            double trunZeroEncoder;
            double startOuterWheelEncoder;
            double startEncoder = location.ElmoEncoder;

            double distance = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R * Math.Sqrt(2);

            MoveState = EnumMoveState.R2000;
            ControlData.NowAction = EnumAddressAction.R2000;
            command.IndexOflisSectionLine++;

            double velocity = moveControlConfig.TurnParameter[EnumAddressAction.R2000].Velocity;
            double safetyVelocityRange = moveControlConfig.TurnParameter[EnumAddressAction.R2000].SafetyVelocityRange;
            double agvVelocity = Math.Abs(location.Velocity);

            if (Math.Abs(agvVelocity - velocity) <= safetyVelocityRange)
            { // Normal
            }
            else if (agvVelocity > velocity)
            { // 超速GG, 不該發生.
                SendAlarmCode(153000);
                EMSControl("超速.., vel : " + agvVelocity.ToString("0"));
                return;
            }
            else
            { // 太慢 處理??
                SendAlarmCode(153001);
                EMSControl("速度過慢.., vel : " + agvVelocity.ToString("0"));
                return;
            }

            if (wheelAngle == 1)
            {
                outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.XFL, true);
                leftTurn = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.InnerWheelTurn];
                rightTurn = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.OuterWheelTurn];

                leftMove = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.InnerWheelMove];
                rightMove = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.OuterWheelMove];
            }
            else if (wheelAngle == -1)
            {
                outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.XFR, true);
                rightTurn = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.InnerWheelTurn];
                leftTurn = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.OuterWheelTurn];

                rightMove = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.InnerWheelMove];
                leftMove = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.OuterWheelMove];
            }
            else
            {
                EMSControl("R2000取得奇怪的wheelAngle");
                return;
            }

            startOuterWheelEncoder = outerWheelEncoder;
            trunZeroEncoder = outerWheelEncoder + (ControlData.DirFlag ? moveControlConfig.TurnParameter[EnumAddressAction.R2000].Distance :
                                                                        -moveControlConfig.TurnParameter[EnumAddressAction.R2000].Distance);

            WriteLog("MoveControl", "7", device, "", "開始旋轉, startOuterWheelEncoder : " + startOuterWheelEncoder.ToString("0.0") +
                                                ", 預計回正OuterWheelEncode : " + trunZeroEncoder.ToString("0.0"));

            elmoDriver.ElmoMove(EnumAxis.VXFL, leftMove.Distance * moveDirlag, leftMove.Velocity, EnumMoveType.Absolute,
                               leftMove.Acceleration, leftMove.Deceleration, leftMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTFL, leftTurn.Distance * wheelAngle, leftTurn.Velocity, EnumMoveType.Absolute,
                               leftTurn.Acceleration, leftTurn.Deceleration, leftTurn.Jerk);

            elmoDriver.ElmoMove(EnumAxis.VXFR, rightMove.Distance * moveDirlag, rightMove.Velocity, EnumMoveType.Absolute,
                               rightMove.Acceleration, rightMove.Deceleration, rightMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTFR, rightTurn.Distance * wheelAngle, rightTurn.Velocity, EnumMoveType.Absolute,
                               rightTurn.Acceleration, rightTurn.Deceleration, rightTurn.Jerk);

            elmoDriver.ElmoMove(EnumAxis.VXRL, leftMove.Distance * moveDirlag, leftMove.Velocity, EnumMoveType.Absolute,
                               leftMove.Acceleration, leftMove.Deceleration, leftMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTRL, -leftTurn.Distance * wheelAngle, leftTurn.Velocity, EnumMoveType.Absolute,
                               leftTurn.Acceleration, leftTurn.Deceleration, leftTurn.Jerk);

            elmoDriver.ElmoMove(EnumAxis.VXRR, rightMove.Distance * moveDirlag, rightMove.Velocity, EnumMoveType.Absolute,
                               rightMove.Acceleration, rightMove.Deceleration, rightMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTRR, -rightTurn.Distance * wheelAngle, rightTurn.Velocity, EnumMoveType.Absolute,
                               rightTurn.Acceleration, rightTurn.Deceleration, rightTurn.Jerk);

            while (!OkToTurnZero(outerWheelEncoder, trunZeroEncoder))
            {
                if (wheelAngle == 1)
                    outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.XFL, true);
                else if (wheelAngle == -1)
                    outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.XFR, true);

                UpdatePosition();
                SensorSafety();

                if (ControlData.FlowStopRequeset || MoveState == EnumMoveState.Error)
                    return;

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            WriteLog("MoveControl", "7", device, "", "開始回正, outerWheelEncoder : " + outerWheelEncoder.ToString("0.0"));
            elmoDriver.ElmoStop(EnumAxis.VXFL, leftMove.Deceleration, leftMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTFL, 0, leftTurn.Velocity, EnumMoveType.Absolute, leftTurn.Acceleration, leftTurn.Deceleration, leftTurn.Jerk);

            elmoDriver.ElmoStop(EnumAxis.VXFR, rightMove.Deceleration, rightMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTFR, 0, rightTurn.Velocity, EnumMoveType.Absolute, rightTurn.Acceleration, rightTurn.Deceleration, rightTurn.Jerk);

            elmoDriver.ElmoStop(EnumAxis.VXRL, leftMove.Deceleration, leftMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTRL, 0, leftTurn.Velocity, EnumMoveType.Absolute, leftTurn.Acceleration, leftTurn.Deceleration, leftTurn.Jerk);

            elmoDriver.ElmoStop(EnumAxis.VXRR, rightMove.Deceleration, rightMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTRR, 0, rightTurn.Velocity, EnumMoveType.Absolute, rightTurn.Acceleration, rightTurn.Deceleration, rightTurn.Jerk);

            Thread.Sleep(moveControlConfig.SleepTime * 2);
            while (!elmoDriver.MoveCompeleteVirtual(EnumAxisType.Move))
            {
                UpdatePosition();
                SensorSafety();

                if (ControlData.FlowStopRequeset || MoveState == EnumMoveState.Error)
                    return;

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            location.Delta = location.Delta + (ControlData.DirFlag ? (distance - (location.ElmoEncoder - startEncoder)) :
                                               -(distance - (startEncoder - location.ElmoEncoder)));

            command.IndexOflisSectionLine++;
            UpdatePosition();
            if ((wheelAngle == -1 && ControlData.DirFlag) ||
                (wheelAngle == 1 && !ControlData.DirFlag))
                location.Real.AGVAngle -= 90;
            else
                location.Real.AGVAngle += 90;

            if (location.Real.AGVAngle > 180)
                location.Real.AGVAngle -= 360;
            else if (location.Real.AGVAngle <= -180)
                location.Real.AGVAngle += 360;

            Vehicle.Instance.VehicleLocation.VehicleAngle = location.Real.AGVAngle;

            DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));

            MoveState = EnumMoveState.Moving;
            safetyData.TurningByPass = false;
            ControlData.CanPause = true;
            WriteLog("MoveControl", "7", device, "", " end.");
        }

        private void SetVChangeSafetyParameter(double oldVelocity, double velocity)
        {
            if (velocity < oldVelocity)
            {
                if (Math.Abs(location.Velocity) <= moveControlConfig.VelocitySafetyRange && velocity != 0)
                {
                    double distance = computeFunction.GetAccDecDistance(
                        0, velocity + moveControlConfig.Safety[EnumMoveControlSafetyType.VChangeSafetyDistance].Range,
                        moveControlConfig.Move.Acceleration, moveControlConfig.Move.Jerk);

                    ControlData.VChangeSafetyTargetEncoder = location.ElmoEncoder + (ControlData.DirFlag ? distance : -distance);
                    ControlData.VChangeSafetyVelocity = velocity;
                    ControlData.VChangeSafetyType = EnumVChangeSpeedLowerSafety.MoveStartSpeedLower;
                }
                else
                {
                    bool vChangeComplete = Math.Abs(Math.Abs(location.Velocity) - ControlData.RealVelocity) <= moveControlConfig.VelocitySafetyRange;

                    double vel = 0;
                    double distance = 0;

                    if (!vChangeComplete && Math.Abs(location.Velocity) < ControlData.RealVelocity)
                    {
                        vel = Math.Abs(location.Velocity);
                        distance = computeFunction.GetDecDistanceOneJerk(oldVelocity, velocity,
                                   moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk, ref vel) *
                                   moveControlConfig.VChangeSafetyDistanceMagnification;

                        distance = distance * 2;
                        vel = Math.Abs(location.Velocity);
                        ControlData.VChangeSafetyTargetEncoder = location.ElmoEncoder + (ControlData.DirFlag ? distance : -distance);
                        ControlData.VChangeSafetyVelocity = vel;
                    }
                    else
                    {
                        distance = computeFunction.GetDecDistanceOneJerk(oldVelocity, velocity,
                                        moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk, ref vel) *
                                        moveControlConfig.VChangeSafetyDistanceMagnification;

                        ControlData.VChangeSafetyTargetEncoder = location.ElmoEncoder + (ControlData.DirFlag ? distance : -distance);
                        ControlData.VChangeSafetyVelocity = vel;
                    }

                    ControlData.VChangeSafetyType = EnumVChangeSpeedLowerSafety.SpeedLower;

                    WriteLog("MoveControl", "7", device, "", "oldVelocity : " + oldVelocity.ToString("0") + ", velocity: " + velocity.ToString("0"));
                    WriteLog("MoveControl", "7", device, "", ControlData.VChangeSafetyType.ToString() + ", distance : " + distance.ToString("0") + ", Velocity: " + vel.ToString("0"));
                }
            }
            else
                ControlData.VChangeSafetyType = EnumVChangeSpeedLowerSafety.None;
        }

        private void VchangeControl(double velocity, EnumVChangeType vChangeType, int TRWheelAngle, double nowVelocity)
        {
            WriteLog("MoveControl", "7", device, "", " start, Velocity : " + velocity.ToString("0"));

            if (vChangeType == EnumVChangeType.EQ || vChangeType == EnumVChangeType.SlowStop)
            {
                if (nowVelocity != 0 && nowVelocity > ControlData.RealVelocity)
                {
                    double oldDistance = computeFunction.GetAccDecDistance(nowVelocity, velocity, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                    double newDistance = computeFunction.GetAccDecDistance(ControlData.RealVelocity, velocity, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                    double triggerEncoder = command.CommandList[command.IndexOfCmdList].TriggerEncoder + (ControlData.DirFlag ? oldDistance - newDistance : -(oldDistance - newDistance));
                    Command temp = CreateMoveCommandList.NewVChangeCommand(command.CommandList[command.IndexOfCmdList].Position, triggerEncoder, velocity, ControlData.DirFlag, vChangeType);
                    temp.NowVelocity = ControlData.RealVelocity;
                    command.CommandList.Insert(command.IndexOfCmdList + 1, temp);
                    ControlData.KeepsLowSpeedStateByEQVChange = ControlData.SensorState;
                    ControlData.VelocityCommand = ControlData.RealVelocity;
                    return;
                }

                ControlData.KeepsLowSpeedStateByEQVChange = EnumVehicleSafetyAction.Stop;
            }

            if (vChangeType != EnumVChangeType.SensorSlow)
                ControlData.VelocityCommand = velocity;

            if (velocity != ControlData.RealVelocity && ControlData.SensorState != EnumVehicleSafetyAction.Stop)
            {
                if (ControlData.SensorState == EnumVehicleSafetyAction.Normal)
                {   // 一般情況.
                    ControlData.RealVelocity = velocity;
                    agvRevise.SettingReviseData(velocity, ControlData.DirFlag);
                    SetVChangeSafetyParameter(Math.Abs(location.Velocity), velocity);

                    elmoDriver.ElmoGroupVelocityChange(EnumAxis.GX, velocity / moveControlConfig.Move.Velocity);
                }
                else
                {   // 降速觸發中.
                    if (velocity <= moveControlConfig.LowVelocity)
                    {   // 需要降到lowSpeed或更慢的情況.
                        ControlData.RealVelocity = velocity;
                        agvRevise.SettingReviseData(velocity, ControlData.DirFlag);
                        SetVChangeSafetyParameter(Math.Abs(location.Velocity), velocity);

                        elmoDriver.ElmoGroupVelocityChange(EnumAxis.GX, velocity / moveControlConfig.Move.Velocity);
                    }
                    else if (ControlData.RealVelocity < moveControlConfig.LowVelocity)
                    {   // 需要升速且目前速度低於lowSpeed.
                        ControlData.RealVelocity = moveControlConfig.LowVelocity;
                        agvRevise.SettingReviseData(moveControlConfig.LowVelocity, ControlData.DirFlag);
                        SetVChangeSafetyParameter(Math.Abs(location.Velocity), moveControlConfig.LowVelocity);

                        elmoDriver.ElmoGroupVelocityChange(EnumAxis.GX, moveControlConfig.LowVelocity / moveControlConfig.Move.Velocity);
                    }
                }
            }

            if (vChangeType == EnumVChangeType.TRTurn)
            {
                switch (TRWheelAngle)
                {
                    case 0:
                        bool turnLeft = (ControlData.WheelAngle == 90 && ControlData.DirFlag) ||
                                        (ControlData.WheelAngle == -90 && !ControlData.DirFlag);
                        DirLightTurn(turnLeft ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                        break;
                    case 90:
                        DirLightTurn(ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right);
                        break;
                    case -90:
                        DirLightTurn(ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                        break;
                    default:
                        EMSControl("switch (TRWheelAngle) default..EMS.");
                        break;
                }
            }
            else if (vChangeType == EnumVChangeType.R2000Turn)
            {
                DirLightTurn(TRWheelAngle == -1 ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
            }
            else if (vChangeType == EnumVChangeType.EQ)
            {
                if (moveControlConfig.SensorByPass[EnumSensorSafetyType.EndPositionOffset].Enable)
                    ControlData.EQVChange = true;
            }

            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void MoveCommandControl(double velocity, double distance, bool dirFlag, int wheelAngle, EnumMoveStartType moveType)
        {
            WriteLog("MoveControl", "7", device, "", "start, 方向 : " + (dirFlag ? "前進" : "後退") + ", distance : " + distance.ToString("0") +
                                                     ", velocity : " + velocity.ToString("0") + ", wheelAngle : " + wheelAngle.ToString("0") +
                                                     ", moveType : " + moveType.ToString() +
                                                     (moveType == EnumMoveStartType.FirstMove ? ", 為第一次移動,需等待兩秒!" : ""));

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            if (moveType == EnumMoveStartType.FirstMove || moveType == EnumMoveStartType.ChangeDirFlagMove)
            {
                if (ControlData.WheelAngle != wheelAngle)
                    WriteLog("MoveControl", "7", device, "", "舵輪目前角度 : " + ControlData.WheelAngle.ToString() + ", 應旋轉到 : " + wheelAngle.ToString());

                elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.Turn.Velocity, EnumMoveType.Absolute,
                        moveControlConfig.Turn.Acceleration, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);

                timer.Reset();
                timer.Start();
                Thread.Sleep(moveControlConfig.SleepTime);
                UpdatePosition();
                Thread.Sleep(moveControlConfig.SleepTime);
                UpdatePosition();

                while (!location.GTMoveCompelete && !SimulationMode && !elmoDriver.WheelAngleCompare(wheelAngle, 5))
                {
                    UpdatePosition();
                    SensorSafety();

                    if (timer.ElapsedMilliseconds > moveControlConfig.TurnTimeoutValue)
                    {
                        SendAlarmCode(152003);
                        EMSControl("舵輪旋轉Timeout!");
                        return;
                    }

                    Thread.Sleep(moveControlConfig.SleepTime);
                }

                ControlData.WheelAngle = wheelAngle;

                switch (ControlData.WheelAngle)
                {
                    case 0: // 朝前面.
                        BeamSensorSingleOn((dirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                        DirLightSingleOn((dirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                        break;
                    case 90: // 朝左.
                        BeamSensorSingleOn((dirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                        DirLightSingleOn((dirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                        break;
                    case -90: // 朝右.
                        BeamSensorSingleOn((dirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                        DirLightSingleOn((dirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                        break;
                    default:
                        WriteLog("MoveControl", "4", device, "", "switch (wheelAngle) default..EMS..");
                        return;
                }

                ControlData.DirFlag = dirFlag;
            }

            if (moveType == EnumMoveStartType.FirstMove)
            {
                ControlData.TrigetEndEncoder = location.RealEncoder + (dirFlag ? distance : -distance);
                timer.Reset();
                timer.Start();
                while (timer.ElapsedMilliseconds < moveControlConfig.MoveStartWaitTime)
                {
                    UpdatePosition();
                    SensorSafety();
                    Thread.Sleep(moveControlConfig.SleepTime);
                }

                timer.Reset();
                timer.Start();
                while (elmoDriver.ElmoGetDisable(EnumAxis.GX) && !SimulationMode)
                {
                    UpdatePosition();
                    SensorSafety();

                    if (timer.ElapsedMilliseconds > moveControlConfig.TurnTimeoutValue)
                    {
                        WriteLog("MoveControl", "4", device, "", "Enable Timeout!");
                        return;
                    }

                    Thread.Sleep(moveControlConfig.SleepTime);
                }
            }
            else if (moveType == EnumMoveStartType.ChangeDirFlagMove)
            {
                ControlData.TrigetEndEncoder = location.RealEncoder + (dirFlag ? distance : -distance);
                command.IndexOflisSectionLine++;
            }

            if (moveType != EnumMoveStartType.SensorStopMove)
                ControlData.VelocityCommand = velocity;

            ControlData.RealVelocity = velocity;

            ControlData.SensorState = GetSensorState();

            if (ControlData.SensorState != EnumVehicleSafetyAction.Stop)
            {
                if (dirFlag)
                    elmoDriver.ElmoMove(EnumAxis.GX, distance, velocity, EnumMoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                else
                    elmoDriver.ElmoMove(EnumAxis.GX, -distance, velocity, EnumMoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

                SetVChangeSafetyParameter(0, velocity);

                simulationIsMoving = true;
                ControlData.VChangeSafetyType = EnumVChangeSpeedLowerSafety.None;

                if (ControlData.SensorState == EnumVehicleSafetyAction.LowSpeed)
                {
                    double vel = GetVChangeVelocity(moveControlConfig.LowVelocity);
                    Command temp = CreateMoveCommandList.NewVChangeCommand(null, 0, vel, ControlData.DirFlag, EnumVChangeType.SensorSlow);
                    command.CommandList.Insert(command.IndexOfCmdList + 1, temp);
                }
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "由於啟動時 Sensor State 為Stop,因此不啟動!");
            }


            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void SlowStopControl(MapPosition endPosition, int nextReserveIndex)
        {
            WriteLog("MoveControl", "7", device, "", "start, SensorState : " + ControlData.SensorState.ToString());

            if (ControlData.SensorState != EnumVehicleSafetyAction.Stop)
                elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Reset();
            timer.Start();
            while (!location.GXMoveCompelete)
            {
                UpdatePosition();
                //SensorSafety();

                if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue)
                {
                    EMSControl("SlowStop Timeout!");
                    return;
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            BeamSensorCloseAll();
            DirLightCloseAll();

            if (nextReserveIndex == -1)
            {
                for (int i = command.IndexOfCmdList + 1; i < command.CommandList.Count; i++)
                {
                    if (command.CommandList[i].CmdType == EnumCommandType.Move)
                    {
                        ControlData.DirFlag = command.CommandList[i].DirFlag;
                        break;
                    }
                }
            }

            ControlData.VChangeSafetyType = EnumVChangeSpeedLowerSafety.None;
            simulationIsMoving = false;
            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void SecondCorrectionControl(double endEncoder)
        {
            WriteLog("MoveControl", "7", device, "", "start");
            ControlData.SecondCorrection = true;
            UpdatePosition(ControlData.WheelAngle == 0);

            WriteLog("MoveControl", "7", device, "", "nowEncoder : " + location.RealEncoder.ToString("0") + ", endEncoder : " + endEncoder.ToString("0"));

            double endEncoderDelta = location.RealEncoder - endEncoder;

            if (Math.Abs(endEncoder - location.RealEncoder) > moveControlConfig.SecondCorrectionX)
            {
                elmoDriver.ElmoMove(EnumAxis.GX, endEncoder - location.RealEncoder, moveControlConfig.EQ.Velocity, EnumMoveType.Relative,
                                    moveControlConfig.EQ.Acceleration, moveControlConfig.EQ.Deceleration, moveControlConfig.EQ.Jerk);

                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Reset();
                timer.Start();

                Thread.Sleep(moveControlConfig.SleepTime);
                UpdatePosition();
                Thread.Sleep(moveControlConfig.SleepTime);
                UpdatePosition();
                while (!location.GXMoveCompelete)
                {
                    UpdatePosition();
                    SensorSafety();

                    if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue)
                    {
                        EMSControl("SecondCorrectionControl Timeout!");
                        return;
                    }

                    Thread.Sleep(moveControlConfig.SleepTime);
                }

                Thread.Sleep(500);
            }

            elmoDriver.DisableMoveAxis();

            bool newBarcode = UpdateSR2000();

            try
            {
                if (newBarcode)
                {
                    double deltaX = location.Barcode.Position.X - command.End.X;
                    double deltaY = location.Barcode.Position.Y - command.End.Y;
                    double theta = location.Barcode.AGVAngle - location.Real.AGVAngle;
                    plcAgent.SetVehiclePositionValue(deltaX.ToString("0.0"), deltaY.ToString("0.0"), theta.ToString("0.00"), location.Real.AGVAngle.ToString("0"), endEncoderDelta.ToString("0.0"));
                    WriteLog("MoveControl", "7", device, "", "send plc : x = " + deltaX.ToString("0.0") + ", y = " +
                                                              deltaY.ToString("0.0") + ", theta = " + theta.ToString("0.00") + ", AGV車頭方向 = " +
                                                              location.Real.AGVAngle.ToString("0") + ", 二修距離 = " + endEncoderDelta.ToString("0.0"));
                }
                else
                    WriteLog("MoveControl", "7", device, "", "send plc : 讀取不到Barcode!");
            }
            catch (Exception ex)
            {
                WriteLog("MoveControl", "7", device, "", "send plc : Excption : " + ex.ToString() + " !");
            }

            MoveState = EnumMoveState.Idle;
            MoveFinished(EnumMoveComplete.Success);
            ControlData.SecondCorrection = false;
            WriteLog("MoveControl", "7", device, "", "end");
            WriteLog("MoveControl", "7", device, "", "Move Compelete !");
        }

        private void SensorStopControl()
        {
            WriteLog("MoveControl", "7", device, "", "start, MoveState : " + MoveState.ToString());

            switch (MoveState)
            {
                case EnumMoveState.Moving:
                    if (!location.GXMoveCompelete)
                        elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                    break;
                case EnumMoveState.TR:

                    WriteLog("MoveControl", "7", device, "", "EnumMoveState.TR ");

                    elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                    elmoDriver.ElmoStop(EnumAxis.GT, moveControlConfig.TurnParameter[ControlData.NowAction].AxisParameter.Deceleration, moveControlConfig.TurnParameter[ControlData.NowAction].AxisParameter.Jerk);

                    if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.TRFlowStart].Enable)
                    {
                        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                        timer.Reset();
                        timer.Start();

                        while (!elmoDriver.MoveCompelete(EnumAxis.GX))
                        {
                            UpdatePosition();
                            SensorSafety();

                            if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue)
                            {
                                EMSControl("TR Flow Stop TimeOut!");
                            }

                            Thread.Sleep(moveControlConfig.SleepTime);
                        }

                        elmoDriver.DisableMoveAxis();
                        AGVStopResult = "TR Flow Start Disable!";
                        MoveState = EnumMoveState.Error;
                        MoveFinished(EnumMoveComplete.Fail);
                        BeamSensorCloseAll();
                        DirLightCloseAll();
                    }

                    break;
                case EnumMoveState.R2000:
                    elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                    elmoDriver.ElmoStop(EnumAxis.VTFL);
                    elmoDriver.ElmoStop(EnumAxis.VTFR);
                    elmoDriver.ElmoStop(EnumAxis.VTRL);
                    elmoDriver.ElmoStop(EnumAxis.VTRR);

                    if (!elmoDriver.MoveCompeleteVirtual(EnumAxisType.Turn))
                    {
                        elmoDriver.ElmoStop(EnumAxis.VXFL);
                        elmoDriver.ElmoStop(EnumAxis.VXFR);
                        elmoDriver.ElmoStop(EnumAxis.VXRL);
                        elmoDriver.ElmoStop(EnumAxis.VXRR);
                    }

                    if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.R2000FlowStat].Enable)
                    {
                        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                        timer.Reset();
                        timer.Start();

                        while (!location.GXMoveCompelete)
                        {
                            UpdatePosition();
                            SensorSafety();

                            if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue)
                            {
                                EMSControl("R2000 Flow Stop TimeOut!");
                            }

                            Thread.Sleep(moveControlConfig.SleepTime);
                        }

                        elmoDriver.DisableMoveAxis();
                        AGVStopResult = "R2000 Flow Start Disable!";
                        MoveState = EnumMoveState.Error;
                        MoveFinished(EnumMoveComplete.Fail);
                        BeamSensorCloseAll();
                        DirLightCloseAll();
                    }

                    break;
                default:
                    break;
            }

            SetVChangeSafetyParameter(Math.Abs(location.Velocity), 0);
            simulationIsMoving = false;
            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void EMSControl(string emsResult)
        {
            WriteLog("MoveControl", "7", device, "", "start");
            AGVStopResult = emsResult;
            WriteLog("MoveControl", "7", device, "", "EMS Stop : " + emsResult);
            WriteLog("Error", "7", device, "", "EMS Stop : " + emsResult);
            MoveState = EnumMoveState.Error;

            double stopDistance = computeFunction.GetAccDecDistance(Math.Abs(location.Velocity), 0,
                                    moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk) *
                                    moveControlConfig.VChangeSafetyDistanceMagnification;
            double startStopEncoder = location.ElmoEncoder;

            elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            elmoDriver.ElmoStop(EnumAxis.GT, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);

            while (!location.GXMoveCompelete && Math.Abs(startStopEncoder - location.ElmoEncoder) < stopDistance)
            {
                UpdatePosition();
                Thread.Sleep(moveControlConfig.SleepTime);
            }

            if (!location.GXMoveCompelete)
            {
                try
                {
                    plcAgent.SetForcELMOServoOffOn();
                    WriteLog("MoveControl", "7", device, "", "該停止的距離仍在移動,通知PLC斷Elmo驅動器的電源!");
                }
                catch
                {
                    WriteLog("MoveControl", "7", device, "", "該停止的距離仍在移動,通知PLC斷Elmo驅動器的電源,但跳Excption!");
                }
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "EMS正常停止,Disable走行軸!");
                elmoDriver.DisableMoveAxis();
            }

            MoveFinished(EnumMoveComplete.Fail);
            ControlData.OntimeReviseFlag = false;
            ControlData.SecondCorrection = false;
            simulationIsMoving = false;
            BeamSensorCloseAll();
            DirLightCloseAll();
            WriteLog("MoveControl", "7", device, "", "end");
        }
        #endregion

        #region 檢查觸發.

        private bool TriggerCommand(Command cmd)
        {
            if (ControlData.SensorState == EnumVehicleSafetyAction.Stop && (cmd.CmdType == EnumCommandType.Move || cmd.CmdType == EnumCommandType.End))
                return false;

            if (cmd.ReserveNumber < command.ReserveList.Count)
            {
                if (cmd.Position == null)
                {
                    WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 觸發,為立即觸發");
                    return true;
                }
                else
                {
                    if ((cmd.DirFlag && location.RealEncoder > cmd.TriggerEncoder + cmd.SafetyDistance) ||
                       (!cmd.DirFlag && location.RealEncoder < cmd.TriggerEncoder - cmd.SafetyDistance))
                    {
                        EMSControl("Command : " + cmd.CmdType.ToString() + ", 超過Triiger觸發區間,EMS.. dirFlag : " +
                                     (ControlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
                                     location.RealEncoder.ToString("0.0") + ", triggerEncoder : " + cmd.TriggerEncoder.ToString("0.0"));
                        return false;
                    }
                    else if ((cmd.DirFlag && location.RealEncoder > cmd.TriggerEncoder) ||
                            (!cmd.DirFlag && location.RealEncoder < cmd.TriggerEncoder))
                    {
                        WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 觸發, dirFlag : " +
                                  (ControlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
                                 location.RealEncoder.ToString("0.0") + ", triggerEncoder : " + cmd.TriggerEncoder.ToString("0.0"));
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        private void ExecuteCommandList()
        {
            if (command.IndexOfCmdList < command.CommandList.Count && TriggerCommand(command.CommandList[command.IndexOfCmdList]))
            {
                WriteLog("MoveControl", "7", device, "", "Barcode Position ( " + location.Barcode.Position.X.ToString("0") + ", " + location.Barcode.Position.Y.ToString("0") +
                                                         " ), Real Position ( " + location.Real.Position.X.ToString("0") + ", " + location.Real.Position.Y.ToString("0") +
                                                         " ), Encoder Position ( " + location.Encoder.Position.X.ToString("0") + ", " + location.Encoder.Position.Y.ToString("0") + " )");

                switch (command.CommandList[command.IndexOfCmdList].CmdType)
                {
                    case EnumCommandType.TR:
                        TRControl(command.CommandList[command.IndexOfCmdList].WheelAngle, command.CommandList[command.IndexOfCmdList].TurnType);
                        break;
                    case EnumCommandType.R2000:
                        R2000Control(command.CommandList[command.IndexOfCmdList].WheelAngle);
                        break;
                    case EnumCommandType.Vchange:
                        VchangeControl(command.CommandList[command.IndexOfCmdList].Velocity, command.CommandList[command.IndexOfCmdList].VChangeType,
                                       command.CommandList[command.IndexOfCmdList].WheelAngle, command.CommandList[command.IndexOfCmdList].NowVelocity);
                        break;
                    case EnumCommandType.ReviseOpen:
                        if (!ControlData.OntimeReviseFlag)
                        {
                            agvRevise.SettingReviseData(ControlData.VelocityCommand, ControlData.DirFlag);
                            ControlData.OntimeReviseFlag = true;
                        }

                        break;
                    case EnumCommandType.ReviseClose:
                        ControlData.OntimeReviseFlag = false;
                        if (command.CommandList[command.IndexOfCmdList].TurnType == EnumAddressAction.R2000)
                            elmoDriver.ElmoMove(EnumAxis.GT, 0, 20, EnumMoveType.Absolute);
                        else
                            elmoDriver.ElmoStop(EnumAxis.GT);
                        break;
                    case EnumCommandType.Move:
                        MoveCommandControl(command.CommandList[command.IndexOfCmdList].Velocity, command.CommandList[command.IndexOfCmdList].Distance,
                                           command.CommandList[command.IndexOfCmdList].DirFlag, command.CommandList[command.IndexOfCmdList].WheelAngle,
                                           command.CommandList[command.IndexOfCmdList].MoveType);
                        break;
                    case EnumCommandType.SlowStop:
                        SlowStopControl(command.CommandList[command.IndexOfCmdList].EndPosition, command.CommandList[command.IndexOfCmdList].NextReserveNumber);
                        break;
                    case EnumCommandType.Stop:
                        if (command.ReserveList[command.CommandList[command.IndexOfCmdList].NextReserveNumber].GetReserve)
                            WriteLog("MoveControl", "7", device, "", "取得下段Reserve點, 因此取消此命令~!");
                        else
                        {
                            ControlData.WaitReserveIndex = command.CommandList[command.IndexOfCmdList].NextReserveNumber;
                            WriteLog("MoveControl", "7", device, "", "因未取得Reserve index = " + ControlData.WaitReserveIndex.ToString() + ", 因此停車 !");
                        }
                        break;
                    case EnumCommandType.End:
                        SecondCorrectionControl(command.CommandList[command.IndexOfCmdList].EndEncoder);
                        break;
                    default:
                        break;
                }

                command.IndexOfCmdList++;
            }
        }

        private void MoveControlThread()
        {
            double[] reviseWheelAngle = new double[4];
            System.Diagnostics.Stopwatch waitDelateTime = new System.Diagnostics.Stopwatch();

            try
            {
                while (true)
                {
                    UpdatePosition();

                    if (!ControlData.FlowStop && MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
                    {
                        ExecuteCommandList();
                        SensorSafety();
                    }

                    if (MoveState == EnumMoveState.Moving && ControlData.OntimeReviseFlag)
                    {
                        if (agvRevise.OntimeReviseByAGVPositionAndSection(ref reviseWheelAngle,
                            location.ThetaAndSectionDeviation, ControlData.WheelAngle, location.Velocity))
                        {
                            if (!location.GXMoveCompelete && location.GTMoveCompelete)
                            {
                                elmoDriver.ElmoMove(EnumAxis.GT, reviseWheelAngle[0], reviseWheelAngle[1], reviseWheelAngle[2], reviseWheelAngle[3],
                                                  ontimeReviseConfig.ThetaSpeed, EnumMoveType.Absolute, moveControlConfig.Turn.Acceleration,
                                                  moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);
                            }
                        }
                    }

                    if (location.GXMoveCompelete && ControlData.SensorState == EnumVehicleSafetyAction.Stop)
                    {
                        if (ControlData.FlowStopRequeset)
                        {
                            WriteLog("MoveControl", "7", device, "", "AGV已經停止!");
                            ControlData.FlowStop = true;
                            ControlData.FlowStopRequeset = false;
                        }

                        if (ControlData.PauseRequest)
                        {
                            ControlData.PauseAlready = true;
                            ControlData.PauseRequest = false;
                            MoveFinished(EnumMoveComplete.Pause);
                        }
                    }

                    if (ControlData.FlowStop && ControlData.FlowClear)
                    {
                        WriteLog("MoveControl", "7", device, "", "AGV已經停止 State 切換成 Idle!");
                        BeamSensorCloseAll();
                        DirLightCloseAll();
                        elmoDriver.DisableMoveAxis();
                        MoveState = EnumMoveState.Idle;
                        MoveFinished(EnumMoveComplete.Fail);
                        ControlData.FlowStop = false;
                        ControlData.FlowClear = false;
                        location.Real = null;
                    }

                    if (ControlData.PauseAlready && ControlData.CancelRequest)
                    {
                        waitDelateTime.Reset();
                        waitDelateTime.Start();
                        elmoDriver.DisableMoveAxis();

                        while (waitDelateTime.ElapsedMilliseconds < moveControlConfig.PauseDelateTime)
                        {
                            UpdatePosition();
                            SensorSafety();
                            Thread.Sleep(moveControlConfig.SleepTime);
                        }

                        ControlData.PauseAlready = false;
                        ControlData.CancelRequest = false;
                        WriteLog("MoveControl", "7", device, "", "AGV已Pause完成,Cancel!");
                        MoveState = EnumMoveState.Idle;
                        MoveFinished(EnumMoveComplete.Cancel);
                        WriteLog("MoveControl", "7", device, "", "AGV已Pause完成,Cancel已完成!");
                    }

                    if (MoveState == EnumMoveState.Idle && ControlData.CloseMoveControl)
                        break;

                    Thread.Sleep(moveControlConfig.SleepTime);
                }
            }
            catch
            {
                SendAlarmCode(150000);
                EMSControl("MoveControl主Thread Expction!");
            }
        }

        #region 障礙物檢知
        public bool IsCharging()
        {
            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.Charging].Enable)
                return false;

            if (SimulationMode)
                return FakeState.IsCharging;
            else
                return Vehicle.Instance.ThePlcVehicle.Batterys.Charging;
        }

        public bool ForkNotHome()
        {
            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.ForkHome].Enable)
                return false;

            if (SimulationMode)
                return FakeState.ForkNotHome;
            else
                return !Vehicle.Instance.ThePlcVehicle.Robot.ForkHome || Vehicle.Instance.ThePlcVehicle.Robot.ForkBusy;
        }

        private EnumVehicleSafetyAction GetBumperState()
        {
            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.Bumper].Enable)
                return EnumVehicleSafetyAction.Normal;

            if (SimulationMode)
                return FakeState.BumpSensorState;
            else
                return Vehicle.Instance.ThePlcVehicle.BumperAlarmStatus ? EnumVehicleSafetyAction.Stop : EnumVehicleSafetyAction.Normal;
        }

        private EnumVehicleSafetyAction GetBeamSensorState()
        {
            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensor].Enable)
                return EnumVehicleSafetyAction.Normal;

            if (SimulationMode)
                return FakeState.BeamSensorState;
            else
                return Vehicle.Instance.ThePlcVehicle.VehicleSafetyAction;
        }

        private double GetVChangeVelocity(double velocity)
        {
            double nextVChangeDistance = -1;
            int index = -1;

            for (int i = command.IndexOfCmdList; i < command.CommandList.Count; i++)
            {
                if (command.CommandList[i].CmdType == EnumCommandType.Vchange)
                {
                    if (command.CommandList[i].Position != null)
                        index = i;

                    break;
                }
                else if (command.CommandList[i].CmdType == EnumCommandType.SlowStop && !command.CommandList[i].NextRserveCancel)
                    break;
            }

            if (nextVChangeDistance == -1)
                return velocity;
            else if (ControlData.SensorState == EnumVehicleSafetyAction.LowSpeed)
            {
                if (command.CommandList[index].Velocity > velocity && velocity == moveControlConfig.LowVelocity)
                    return velocity;
            }

            nextVChangeDistance = Math.Abs(location.RealEncoder - command.CommandList[index].TriggerEncoder);

            double returnVelocity = CreateMoveCommandList.GetFirstVChangeCommandVelocity(moveControlConfig.Move.Velocity, 0,
                velocity, nextVChangeDistance);

            if (returnVelocity > velocity)
            {
                WriteLog("MoveControl", "7", device, "", "---GetVChangeVelocity 出問題, returnVelocity > velocity.............................!");
                return velocity;
            }
            else if (returnVelocity < 0)
            {
                WriteLog("MoveControl", "7", device, "", "---GetVChangeVelocity 出問題, returnVelocity < 0.............................!");
                return velocity;
            }

            if (returnVelocity == 0)
                return command.CommandList[index].Velocity;
            else
                return returnVelocity;
        }

        private bool CheckIndexOfCommandCanTrigger(int index)
        {
            bool dirFlag = true;

            if (command.CommandList[index].CmdType == EnumCommandType.Move)
                dirFlag = command.CommandList[index].DirFlag;
            else
                dirFlag = ControlData.DirFlag;

            if (dirFlag)
                return location.RealEncoder > command.CommandList[index].TriggerEncoder;
            else
                return location.RealEncoder < command.CommandList[index].TriggerEncoder;
        }

        private bool NextCommandIsXXX(EnumCommandType type, ref int index)
        {
            for (int i = command.IndexOfCmdList; i < command.CommandList.Count; i++)
            {
                if (command.CommandList[i].CmdType == type)
                {
                    index = i;
                    return true;
                }
                else if (command.CommandList[i].Position != null)
                    return false;
            }

            return false;
        }

        private void SensorStartMove(EnumVehicleSafetyAction nowAction)
        {
            ///狀況1. 下筆命令是移動且無法觸發..當作二修.
            /// 
            ///狀況2. 下筆命令是移動且可以觸發..不做事情. 
            /// 
            ///狀況3. 其他狀況..直接下動令+VChange.
            ///
            ///如果是 SlowVelocity 要加入降速.
            ///
            int index = 0;

            if (NextCommandIsXXX(EnumCommandType.Move, ref index))
            {
                if (!CheckIndexOfCommandCanTrigger(index))
                { // 狀況1.
                    double targetEncoder = command.CommandList[command.IndexOfCmdList].TriggerEncoder +
                        (command.CommandList[command.IndexOfCmdList].DirFlag ? command.CommandList[command.IndexOfCmdList].SafetyDistance / 2 :
                                                              -command.CommandList[command.IndexOfCmdList].SafetyDistance / 2);

                    WriteLog("MoveControl", "7", device, "", "下筆移動命令無法觸發,因此進行二修 : distance : " + (targetEncoder - location.RealEncoder).ToString("0") +
                                                                   ", velocity : " + moveControlConfig.EQ.Velocity.ToString("0") + "!");

                    elmoDriver.ElmoMove(EnumAxis.GX, targetEncoder - location.RealEncoder, moveControlConfig.EQ.Velocity, EnumMoveType.Relative,
                         moveControlConfig.EQ.Acceleration, moveControlConfig.EQ.Deceleration, moveControlConfig.EQ.Jerk);

                    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                    timer.Reset();
                    timer.Start();

                    Thread.Sleep(moveControlConfig.SleepTime);
                    UpdatePosition();
                    Thread.Sleep(moveControlConfig.SleepTime);
                    UpdatePosition();
                    while (!location.GXMoveCompelete)
                    {
                        UpdatePosition();
                        Thread.Sleep(moveControlConfig.SleepTime);

                        if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue * 3)
                        {
                            EMSControl("SensorStartMove Fake SecondCorrection Timeout!");
                            return;
                        }
                    }
                }
                else
                {
                    WriteLog("MoveControl", "7", device, "", "下筆移動命令可以觸發,因此不插入移動命令!");
                }
            }
            else if (!NextCommandIsXXX(EnumCommandType.End, ref index))
            {
                // 狀況3.
                double distance = ControlData.TrigetEndEncoder - location.RealEncoder;

                WriteLog("MoveControl", "7", device, "", "一般情況,插入移動命令 : distance : " +
                    (ControlData.TrigetEndEncoder - location.RealEncoder).ToString("0") + "!");

                Command temp = CreateMoveCommandList.NewMoveCommand(null, 0, Math.Abs(distance),
                    moveControlConfig.Move.Velocity, ControlData.DirFlag, 0, EnumMoveStartType.SensorStopMove);
                command.CommandList.Insert(command.IndexOfCmdList, temp);

                if (nowAction == EnumVehicleSafetyAction.Normal || ControlData.VelocityCommand < moveControlConfig.LowVelocity)
                {
                    double vel = GetVChangeVelocity(ControlData.VelocityCommand);
                    temp = CreateMoveCommandList.NewVChangeCommand(null, 0, vel, ControlData.DirFlag);
                }
                else
                {
                    double vel = GetVChangeVelocity(moveControlConfig.LowVelocity);
                    temp = CreateMoveCommandList.NewVChangeCommand(null, 0, vel, ControlData.DirFlag, EnumVChangeType.SensorSlow);
                }

                command.CommandList.Insert(command.IndexOfCmdList + 1, temp);
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "二修可觸發,不做事情!");
            }
        }

        private void SensorStartMoveTRAction()
        {
            EMSControl("TR flow start 還未實作!");
        }

        private void SensorActionToNormal()
        {
            switch (ControlData.SensorState)
            {
                case EnumVehicleSafetyAction.LowSpeed:
                    // 加入升速.
                    if (ControlData.VelocityCommand > moveControlConfig.LowVelocity/* && !elmoDriver.MoveCompelete(EnumAxis.GX)*/)
                    {
                        double vel = GetVChangeVelocity(ControlData.VelocityCommand);
                        WriteLog("MoveControl", "7", device, "", "Sensor切換至Normal,速度提升至" + vel.ToString("0") + "!");
                        Command temp = CreateMoveCommandList.NewVChangeCommand(null, 0, vel, ControlData.DirFlag);
                        command.CommandList.Insert(command.IndexOfCmdList, temp);
                    }

                    break;
                case EnumVehicleSafetyAction.Stop:
                    // 加入啟動.
                    if (MoveState == EnumMoveState.TR)
                        SensorStartMoveTRAction();
                    else if (MoveState == EnumMoveState.R2000)
                        EMSControl("暫時By pass TR2000中啟動!");
                    else
                        SensorStartMove(EnumVehicleSafetyAction.Normal);

                    break;
                case EnumVehicleSafetyAction.Normal:
                default:
                    break;
            }
        }

        private void SensorActionToSlow()
        {
            switch (ControlData.SensorState)
            {
                case EnumVehicleSafetyAction.Normal:
                    // 加入降速.
                    if (ControlData.VelocityCommand > moveControlConfig.LowVelocity)
                    {
                        WriteLog("MoveControl", "7", device, "", "Sensor切換至LowSpeed,降速至300!");
                        Command temp = CreateMoveCommandList.NewVChangeCommand(null, 0, moveControlConfig.LowVelocity, ControlData.DirFlag, EnumVChangeType.SensorSlow);
                        command.CommandList.Insert(command.IndexOfCmdList, temp);
                    }
                    else
                        WriteLog("MoveControl", "7", device, "", "Sensor切換至LowSpeed,但目前速度小於等於300,不做降速!");

                    break;
                case EnumVehicleSafetyAction.Stop:
                    // 加入啟動且降速.
                    if (MoveState == EnumMoveState.TR)
                        SensorStartMoveTRAction();
                    else if (MoveState == EnumMoveState.R2000)
                        EMSControl("暫時By pass TR2000中啟動!");
                    else
                        SensorStartMove(EnumVehicleSafetyAction.LowSpeed);
                    break;
                case EnumVehicleSafetyAction.LowSpeed:
                default:
                    break;
            }
        }

        private void SensorActionToStop()
        {
            switch (ControlData.SensorState)
            {
                case EnumVehicleSafetyAction.Normal:
                case EnumVehicleSafetyAction.LowSpeed:
                    SensorStopControl();
                    break;
                case EnumVehicleSafetyAction.Stop:
                default:
                    break;
            }
        }

        private void SensorAction(EnumVehicleSafetyAction beamsensorState)
        {
            if (beamsensorState == ControlData.SensorState)
                return;

            switch (beamsensorState)
            {
                case EnumVehicleSafetyAction.Normal:
                    SensorActionToNormal();
                    break;
                case EnumVehicleSafetyAction.LowSpeed:
                    SensorActionToSlow();
                    break;
                case EnumVehicleSafetyAction.Stop:
                    SensorActionToStop();
                    break;
                default:
                    break;
            }
        }

        private bool CheckAxisNoError()
        {
            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.CheckAxisState].Enable)
                return true;

            if (SimulationMode)
                return FakeState.AxisNormal;
            else
                return elmoDriver.CheckAxisNoError();
        }

        private bool CheckAxisEnableAndLinked()
        {
            if (SimulationMode)
                return true;

            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.CheckAxisState].Enable)
                return true;
            else
                return elmoDriver.CheckAxisEnableAndLinked();
        }

        private void VChangeSafety()
        {
            if (!moveControlConfig.Safety[EnumMoveControlSafetyType.VChangeSafetyDistance].Enable)
                return;

            switch (ControlData.VChangeSafetyType)
            {
                case EnumVChangeSpeedLowerSafety.SpeedLower:
                    if (ControlData.DirFlag && location.ElmoEncoder > ControlData.VChangeSafetyTargetEncoder ||
                       !ControlData.DirFlag && location.ElmoEncoder < ControlData.VChangeSafetyTargetEncoder)
                    {
                        if (Math.Abs(location.Velocity) > moveControlConfig.VelocitySafetyRange + ControlData.VChangeSafetyVelocity)
                        {
                            WriteLog("MoveControl", "7", device, "", "降速指令未降速, 目前速度 : " + Math.Abs(location.Velocity).ToString("0") +
                                                                           ", 安全速度 : " + ControlData.VChangeSafetyVelocity.ToString("0") + " !");
                            SendAlarmCode(151000);
                            EMSControl("降速指令未降速!");
                        }
                        else
                        {
                            WriteLog("MoveControl", "7", device, "", "降速變化正常,取消降速監控!");
                            ControlData.VChangeSafetyType = EnumVChangeSpeedLowerSafety.None;
                        }
                    }

                    break;

                case EnumVChangeSpeedLowerSafety.MoveStartSpeedLower:
                    if (ControlData.DirFlag && location.ElmoEncoder > ControlData.VChangeSafetyTargetEncoder ||
                       !ControlData.DirFlag && location.ElmoEncoder < ControlData.VChangeSafetyTargetEncoder)
                    {
                        if (Math.Abs(location.Velocity) > ControlData.VChangeSafetyVelocity + moveControlConfig.VelocitySafetyRange)
                        {
                            WriteLog("MoveControl", "7", device, "", "啟動VChange未降速, 目前速度 : " + Math.Abs(location.Velocity).ToString("0") +
                                                                           ", 安全速度 : " + ControlData.VChangeSafetyVelocity.ToString("0") + " !");
                            SendAlarmCode(151000);
                            EMSControl("降速指令未降速!");
                        }
                        else
                        {
                            WriteLog("MoveControl", "7", device, "", "啟動VChange正常,取消降速監控!");
                            ControlData.VChangeSafetyType = EnumVChangeSpeedLowerSafety.None;
                        }
                    }

                    break;

                case EnumVChangeSpeedLowerSafety.None:
                default:
                    break;
            }
        }

        private EnumVehicleSafetyAction GetSensorState()
        {
            EnumVehicleSafetyAction sensorState = EnumVehicleSafetyAction.Normal;
            EnumVehicleSafetyAction beamSensorState = GetBeamSensorState();
            EnumVehicleSafetyAction bumpSensorState = GetBumperState();

            if (beamSensorState != ControlData.BeamSensorState)
                WriteLog("MoveControl", "7", device, "", "BeamSensorState 從 " + ControlData.BeamSensorState.ToString() +
                         " 變更為 " + beamSensorState.ToString() + " (原始資料,只受模擬模式或Disable BeamSensor功能影響)!");

            if (bumpSensorState != ControlData.BumpSensorState)
                WriteLog("MoveControl", "7", device, "", "BumpSensorState 從 " + ControlData.BumpSensorState.ToString() +
                         " 變更為 " + bumpSensorState.ToString() + " (原始資料,只受模擬模式或Disable BumpSensor功能影響)!");

            ControlData.BeamSensorState = beamSensorState;
            ControlData.BumpSensorState = bumpSensorState;

            if (beamSensorState != EnumVehicleSafetyAction.Normal && (safetyData.TurningByPass || ControlData.SecondCorrection || !CanStopInNextTurn()))
                beamSensorState = EnumVehicleSafetyAction.Normal;

            if (ControlData.WaitReserveIndex != -1)
            {
                if (command.ReserveList[ControlData.WaitReserveIndex].GetReserve)
                {
                    WriteLog("MoveControl", "7", device, "", "取得Reserve index = " + ControlData.WaitReserveIndex.ToString() + ", WaitReserveIndex 變回 -1 !");
                    ControlData.WaitReserveIndex = -1;
                }
            }

            if (ControlData.FlowStopRequeset || ControlData.PauseRequest || ControlData.PauseAlready || ControlData.WaitReserveIndex != -1 ||
                beamSensorState == EnumVehicleSafetyAction.Stop || bumpSensorState == EnumVehicleSafetyAction.Stop)
                sensorState = EnumVehicleSafetyAction.Stop;
            else if (beamSensorState == EnumVehicleSafetyAction.LowSpeed || bumpSensorState == EnumVehicleSafetyAction.LowSpeed)
                sensorState = EnumVehicleSafetyAction.LowSpeed;
            else
                sensorState = EnumVehicleSafetyAction.Normal;

            if (ControlData.KeepsLowSpeedStateByEQVChange != EnumVehicleSafetyAction.Stop && sensorState != EnumVehicleSafetyAction.Stop)
                sensorState = ControlData.KeepsLowSpeedStateByEQVChange;

            return sensorState;
        }

        private EnumVehicleSafetyAction ProcessSensorState(EnumVehicleSafetyAction sensorState)
        {
            if (SimulationMode)
                return sensorState;

            double nowVel = Math.Abs(elmoDriver.ElmoGetVelocity(EnumAxis.GX));
            bool vChangeComplete = Math.Abs(nowVel - ControlData.RealVelocity) <= moveControlConfig.VelocitySafetyRange;

            if (ControlData.SensorState == EnumVehicleSafetyAction.LowSpeed &&
                sensorState == EnumVehicleSafetyAction.Normal)
            {
                if (!vChangeComplete || location.GXMoveCompelete)
                    sensorState = EnumVehicleSafetyAction.LowSpeed;
            }
            else if (ControlData.SensorState == EnumVehicleSafetyAction.Normal &&
                     sensorState == EnumVehicleSafetyAction.LowSpeed)
            {
                if (location.GXMoveCompelete)
                {
                    sensorState = EnumVehicleSafetyAction.Normal;
                }
                else if (!vChangeComplete && nowVel < ControlData.RealVelocity)
                {
                    sensorState = EnumVehicleSafetyAction.Stop;
                    WriteLog("MoveControl", "7", device, "", "升速中觸發LowSpeed訊號,由於無法直接降速,因此變更訊號為Stop!");
                }
            }
            else if (ControlData.SensorState == EnumVehicleSafetyAction.Stop &&
                     sensorState != EnumVehicleSafetyAction.Stop)
            {   // 停止->移動,如果還在動,延遲訊號.
                if (!location.GXMoveCompelete)
                    sensorState = EnumVehicleSafetyAction.Stop;
            }
            else if (ControlData.SensorState != EnumVehicleSafetyAction.Stop &&
                     sensorState == EnumVehicleSafetyAction.Stop)
            {   // 移動->停止,如果還沒動,延遲訊號.
                if (location.GXMoveCompelete)
                    sensorState = ControlData.SensorState;
            }

            return sensorState;
        }

        private EnumVehicleSafetyAction UpdateSensorState()
        {
            EnumVehicleSafetyAction sensorState = GetSensorState();
            return ProcessSensorState(sensorState);
        }

        private void SensorSafety()
        {
            if (MoveState == EnumMoveState.Idle || MoveState == EnumMoveState.Error)
                return;

            if (ForkNotHome())
            {
                SendAlarmCode(140000);
                EMSControl("走行中Fork不在Home點!");
            }
            else if (!CheckAxisNoError())
            {
                SendAlarmCode(142000);
                EMSControl("走行中Axis Error!");
            }
            else
            {
                EnumVehicleSafetyAction sensorState = UpdateSensorState();
                if (ControlData.SensorState != sensorState)
                    WriteLog("MoveControl", "7", device, "", "SensorState 從 " + ControlData.SensorState.ToString() +
                             " 變更為 " + sensorState.ToString() + "!");

                SensorAction(sensorState);
                ControlData.SensorState = sensorState;
                VChangeSafety();
            }
        }
        #endregion

        #region BeamSensor切換
        private void BeamSensorSingleOn(EnumBeamSensorLocate locate)
        {
            switch (locate)
            {
                case EnumBeamSensorLocate.Front:
                    Vehicle.Instance.ThePlcVehicle.MoveFront = true;
                    break;

                case EnumBeamSensorLocate.Back:
                    Vehicle.Instance.ThePlcVehicle.MoveBack = true;
                    break;

                case EnumBeamSensorLocate.Left:
                    Vehicle.Instance.ThePlcVehicle.MoveLeft = true;
                    break;

                case EnumBeamSensorLocate.Right:
                    Vehicle.Instance.ThePlcVehicle.MoveRight = true;
                    break;

                default:
                    break;
            }

            WriteLog("MoveControl", "7", device, "", "Beam sensor 切換 : 修改 " + locate.ToString() + " 變更為On !");
        }

        private void BeamSensorOnlyOn(EnumBeamSensorLocate locate)
        {
            bool front = false;
            bool back = false;
            bool left = false;
            bool right = false;

            switch (locate)
            {
                case EnumBeamSensorLocate.Front:
                    front = true;
                    break;
                case EnumBeamSensorLocate.Back:
                    back = true;
                    break;
                case EnumBeamSensorLocate.Left:
                    left = true;
                    break;
                case EnumBeamSensorLocate.Right:
                    right = true;
                    break;
                default:
                    break;
            }

            Vehicle.Instance.ThePlcVehicle.MoveFront = front;
            Vehicle.Instance.ThePlcVehicle.MoveBack = back;
            Vehicle.Instance.ThePlcVehicle.MoveLeft = left;
            Vehicle.Instance.ThePlcVehicle.MoveRight = right;

            WriteLog("MoveControl", "7", device, "", "Beam sensor 切換 : 只剩 " + locate.ToString() + " On !");
        }

        private void BeamSensorCloseAll()
        {
            Vehicle.Instance.ThePlcVehicle.MoveFront = false;
            Vehicle.Instance.ThePlcVehicle.MoveBack = false;
            Vehicle.Instance.ThePlcVehicle.MoveLeft = false;
            Vehicle.Instance.ThePlcVehicle.MoveRight = false;

            WriteLog("MoveControl", "7", device, "", "Beam sensor 切換 : 全部關掉!");
        }
        #endregion

        #region 方向燈切換
        private void DirLightSingleOn(EnumBeamSensorLocate locate)
        {
            switch (locate)
            {
                case EnumBeamSensorLocate.Front:
                    Vehicle.Instance.ThePlcVehicle.Forward = true;
                    break;
                case EnumBeamSensorLocate.Back:
                    Vehicle.Instance.ThePlcVehicle.Backward = true;
                    break;
                case EnumBeamSensorLocate.Left:
                    Vehicle.Instance.ThePlcVehicle.TraverseLeft = true;
                    break;
                case EnumBeamSensorLocate.Right:
                    Vehicle.Instance.ThePlcVehicle.TraverseRight = true;
                    break;
                default:
                    break;
            }

            WriteLog("MoveControl", "7", device, "", "方向燈切換 : 修改 " + locate.ToString() + " 變更為On !");
        }

        private void DirLightTurn(EnumBeamSensorLocate turnDir)
        {
            Vehicle.Instance.ThePlcVehicle.Forward = false;
            Vehicle.Instance.ThePlcVehicle.Backward = false;
            Vehicle.Instance.ThePlcVehicle.TraverseLeft = false;
            Vehicle.Instance.ThePlcVehicle.TraverseRight = false;

            switch (turnDir)
            {
                case EnumBeamSensorLocate.Front:
                case EnumBeamSensorLocate.Back:
                    WriteLog("MoveControl", "7", device, "", "DirLightTurn有問題,轉彎不應該有Front或Back的方向!");
                    break;
                case EnumBeamSensorLocate.Left:
                    if (ControlData.DirFlag)
                        Vehicle.Instance.ThePlcVehicle.SteeringFL = true;
                    else
                        Vehicle.Instance.ThePlcVehicle.SteeringBL = true;
                    break;
                case EnumBeamSensorLocate.Right:
                    if (ControlData.DirFlag)
                        Vehicle.Instance.ThePlcVehicle.SteeringFR = true;
                    else
                        Vehicle.Instance.ThePlcVehicle.SteeringBR = true;
                    break;
                default:
                    break;
            }

            WriteLog("MoveControl", "7", device, "", "AGV Turn " + turnDir.ToString() + "!");
        }

        private void DirLightOnlyOn(EnumBeamSensorLocate locate)
        {
            bool front = false;
            bool back = false;
            bool left = false;
            bool right = false;

            switch (locate)
            {
                case EnumBeamSensorLocate.Front:
                    front = true;
                    break;
                case EnumBeamSensorLocate.Back:
                    back = true;
                    break;
                case EnumBeamSensorLocate.Left:
                    left = true;
                    break;
                case EnumBeamSensorLocate.Right:
                    right = true;
                    break;
                default:
                    break;
            }

            Vehicle.Instance.ThePlcVehicle.Forward = front;
            Vehicle.Instance.ThePlcVehicle.Backward = back;
            Vehicle.Instance.ThePlcVehicle.TraverseLeft = left;
            Vehicle.Instance.ThePlcVehicle.TraverseRight = right;
            Vehicle.Instance.ThePlcVehicle.SteeringFL = false;
            Vehicle.Instance.ThePlcVehicle.SteeringFR = false;
            Vehicle.Instance.ThePlcVehicle.SteeringBL = false;
            Vehicle.Instance.ThePlcVehicle.SteeringBR = false;

            WriteLog("MoveControl", "7", device, "", "方向燈切換 : 只剩 " + locate.ToString() + " On !");
        }

        private void DirLightCloseAll()
        {
            Vehicle.Instance.ThePlcVehicle.Forward = false;
            Vehicle.Instance.ThePlcVehicle.Backward = false;
            Vehicle.Instance.ThePlcVehicle.TraverseLeft = false;
            Vehicle.Instance.ThePlcVehicle.TraverseRight = false;
            Vehicle.Instance.ThePlcVehicle.SteeringFL = false;
            Vehicle.Instance.ThePlcVehicle.SteeringFR = false;
            Vehicle.Instance.ThePlcVehicle.SteeringBL = false;
            Vehicle.Instance.ThePlcVehicle.SteeringBR = false;

            WriteLog("MoveControl", "7", device, "", "方向燈切換 : 全部關掉!");
        }
        #endregion

        #region 外部連結 : 產生List、DebugForm相關、狀態、移動完成.
        /// <summary>
        ///  when move finished, call this function to notice other class instance that move is finished with status
        /// </summary>
        public void MoveFinished(EnumMoveComplete status)
        {
            WriteLog("MoveControl", "7", device, "", "status : " + status.ToString());

            if (status == EnumMoveComplete.Fail)
                WriteLog("Error", "7", device, "", "status : " + status.ToString());

            if (status == EnumMoveComplete.Pause)
            {
                if (isAGVMCommand)
                    OnMoveFinished?.Invoke(this, status);
            }
            else if (ControlData.CommandMoving)
            {
                BeamSensorCloseAll();
                DirLightCloseAll();
                ControlData.CommandMoving = false;
                ControlData.OntimeReviseFlag = false;

                if (isAGVMCommand)
                    OnMoveFinished?.Invoke(this, status);
            }
            else
            {
                BeamSensorCloseAll();
                DirLightCloseAll();
                ControlData.OntimeReviseFlag = false;
                WriteLog("MoveControl", "7", device, "", "error : no Command send MoveFinished, status : " + status.ToString());
            }
        }

        private void ResetEncoder(MapPosition start, MapPosition end, bool dirFlag)
        {
            WriteLog("MoveControl", "7", device, "", "start");

            double elmoEncoder = elmoDriver.ElmoGetPosition(EnumAxis.XFL);// 更新elmo encoder(走行距離).

            if (SimulationMode)
            {
                WriteLog("MoveControl", "7", device, "", "SimulationMode 因此把Real設定為start位置!");
                location.Real.Position = start;
                elmoEncoder = location.ElmoEncoder;
            }

            if (location.Real == null)
            {
                EMSControl("location.Real == null EMS");
                return;
            }

            if (start.X == end.X)
            {
                if (dirFlag)
                    location.Offset = -elmoEncoder + location.Real.Position.Y - start.Y;
                else
                    location.Offset = -elmoEncoder + location.Real.Position.Y - start.Y;
            }
            else if (start.Y == end.Y)
            {
                if (dirFlag)
                    location.Offset = -elmoEncoder + location.Real.Position.X - start.X;
                else
                    location.Offset = -elmoEncoder + location.Real.Position.X - start.X;
            }
            else
            {
                EMSControl("不該有R2000啟動動作.");
            }

            location.Encoder = new AGVPosition();
            location.Encoder.Position = new MapPosition(location.Real.Position.X, location.Real.Position.Y);
            location.Encoder.AGVAngle = location.Real.AGVAngle;
            location.Delta = 0;
            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void ResetFlag()
        {
            ControlData.SensorState = EnumVehicleSafetyAction.Normal;
            ControlData.BeamSensorState = EnumVehicleSafetyAction.Normal;
            ControlData.BumpSensorState = EnumVehicleSafetyAction.Normal;
            ControlData.OntimeReviseFlag = false;
            ControlData.KeepsLowSpeedStateByEQVChange = EnumVehicleSafetyAction.Stop;
            ControlData.PauseRequest = false;
            ControlData.PauseAlready = false;
            ControlData.CancelRequest = false;
            ControlData.FlowStopRequeset = false;
            ControlData.FlowStop = false;
            ControlData.FlowClear = false;
            ControlData.SecondCorrection = false;
            ControlData.RealVelocity = 0;
            ControlData.EQVChange = false;

            ControlData.VChangeSafetyType = EnumVChangeSpeedLowerSafety.None;
            safetyData = new MoveControlSafetyData();
            AGVStopResult = "";

            if (command.CommandList.Count != 0 && command.CommandList[0].CmdType == EnumCommandType.Move)
            {
                ControlData.WaitReserveIndex = command.CommandList[0].ReserveNumber;
                ControlData.DirFlag = command.CommandList[0].DirFlag;
                ControlData.WheelAngle = command.CommandList[0].WheelAngle;
            }
            else
                ControlData.WaitReserveIndex = -1;

            safetyData.TurningByPass = false;
            ControlData.CommandMoving = true;
            ControlData.CanPause = true;

            ResetEncoder(command.SectionLineList[0].Start, command.SectionLineList[0].End, command.SectionLineList[0].DirFlag);
            Task.Factory.StartNew(() =>
            {
                elmoDriver.EnableMoveAxis();
            });
        }

        public bool TransferMove(MoveCmdInfo moveCmd, ref string errorMessage)
        {
            WriteLog("MoveControl", "7", device, "", "start");

            if (ControlData.CloseMoveControl)
            {
                WriteLog("MoveControl", "7", device, "", "程式關閉中,拒絕Debug Form命令.");
                errorMessage = "程式關閉中,拒絕Debug Form命令.";
                return false;
            }
            else if ((MoveState != EnumMoveState.Error && MoveState != EnumMoveState.Idle))
            {
                WriteLog("MoveControl", "7", device, "", "移動中,因此無視~!");
                errorMessage = "移動中,因此無視~!";
                return false;
            }
            else if (IsCharging())
            {
                WriteLog("MoveControl", "7", device, "", "Charging中,因此無視~!");
                errorMessage = "Charging中";
                return false;
            }
            else if (ForkNotHome())
            {
                WriteLog("MoveControl", "7", device, "", "Fork不在Home點,因此無視~!");
                errorMessage = "Fork不在Home點";
                return false;
            }

            if (location.Real != null)
            {
                if (SimulationMode)
                {
                    WriteLog("MoveControl", "7", device, "", "模擬模式中,因此跳過起點和目前位置距離安全判斷!");
                }
                else
                {
                    double distance = Math.Sqrt(Math.Pow(location.Real.Position.X - moveCmd.AddressPositions[0].X, 2) +
                                                Math.Pow(location.Real.Position.Y - moveCmd.AddressPositions[0].Y, 2));

                    WriteLog("MoveControl", "7", device, "", "起點和目前位置安全判斷, 起點 : ( " + moveCmd.AddressPositions[0].X.ToString("0") +
                                                             ", " + moveCmd.AddressPositions[0].Y.ToString("0") + " ), 目前位置 : ( " +
                                                             location.Real.Position.X.ToString("0") + ", " + location.Real.Position.Y.ToString("0") +
                                                             " ), 距離為 : " + distance.ToString("0"));

                    if (distance > 50)
                    {
                        WriteLog("MoveControl", "7", device, "", "起點和目前位置差距過大!");
                        errorMessage = "起點和目前位置差距過大!";
                        return false;
                    }
                }
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "AGV迷航中(不知道目前在哪),因此無法接受命令!");
                errorMessage = "AGV迷航中(不知道目前在哪),因此無法接受命令!";
                return false;
            }

            MoveCommandData tempCommand = CreateMoveCommandList.CreateMoveControlListSectionListReserveList(
                                          moveCmd, location.Real, ControlData.WheelAngle, ref errorMessage);

            if (tempCommand == null)
            {
                WriteLog("MoveControl", "7", device, "", "命令分解失敗~!, errorMessage : " + errorMessage);
                return false;
            }

            command = tempCommand;

            ResetFlag();

            MoveCommandID = moveCmd.CmdId;
            isAGVMCommand = true;
            MoveState = EnumMoveState.Moving;
            WriteLog("MoveControl", "7", device, "", "sucess! 開始執行動作~!");
            return true;
        }

        public void StopAndClear()
        {
            WriteLog("MoveControl", "7", device, "", "StopAndClear!");

            if (MoveState != EnumMoveState.Error && MoveState != EnumMoveState.Idle)
            {
                if (!ControlData.FlowStop)
                    ControlData.FlowStopRequeset = true;

                ControlData.FlowClear = true;
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "StopAndClear時AGV已停止 State 直接切換成 Idle!");
                MoveState = EnumMoveState.Idle;
            }
        }

        public bool IsLocationRealNotNull()
        {
            if (!elmoDriver.CheckAxisNoError())
                return false;

            return location.Real != null;
        }

        private bool CanStopInNextTurn()
        {
            int index = -1;

            for (int i = command.IndexOfCmdList; i < command.CommandList.Count; i++)
            {
                if (command.CommandList[i].CmdType == EnumCommandType.SlowStop)
                    break;
                else if (command.CommandList[i].CmdType == EnumCommandType.TR)
                {
                    if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensorTR].Enable)
                        index = i;

                    break;
                }
                else if (command.CommandList[i].CmdType == EnumCommandType.R2000)
                {
                    if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensorR2000].Enable)
                        index = i;

                    break;
                }
            }

            if (index == -1)
                return true;

            double vel = 0;

            if (SimulationMode)
                vel = ControlData.RealVelocity;
            else
                vel = elmoDriver.ElmoGetVelocity(EnumAxis.GX);

            bool vChangeComplete = Math.Abs(Math.Abs(location.Velocity) - ControlData.RealVelocity) <= moveControlConfig.VelocitySafetyRange;

            double distance = 0;

            if (!vChangeComplete && Math.Abs(location.Velocity) < ControlData.RealVelocity)
            {
                double tempVel = Math.Abs(location.Velocity);
                distance = computeFunction.GetDecDistanceOneJerk(location.Velocity, 0,
                           moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk, ref tempVel) *
                           moveControlConfig.VChangeSafetyDistanceMagnification;

                distance = distance * 2;
            }

            double decDistance = computeFunction.GetAccDecDistance(vel, 0, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            double accDistance = computeFunction.GetAccDecDistance(0, command.CommandList[index].Velocity, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Jerk);
            double safetyDistance = moveControlConfig.TurnParameter[command.CommandList[index].TurnType].VChangeSafetyDistance;
            double allDistance = decDistance + accDistance + safetyDistance + distance;

            double stopEncoder = location.RealEncoder + (ControlData.DirFlag ? allDistance : -allDistance);

            if (ControlData.DirFlag)
                return stopEncoder <= command.CommandList[index].TriggerEncoder;
            else
                return stopEncoder >= command.CommandList[index].TriggerEncoder;
        }

        private bool CanPauseNow()
        {
            WriteLog("MoveControl", "7", device, "", "開始判斷目前位置是否可以Pause!");

            if (ControlData.SensorState == EnumVehicleSafetyAction.Stop && ControlData.WaitReserveIndex != -1)
            {
                WriteLog("MoveControl", "7", device, "", "由於目前已經處於Reserve stop,因此直接略過判斷(可以Pause)!");
                return true;
            }

            if (!ControlData.CanPause || !CanStopInNextTurn())
            {
                WriteLog("MoveControl", "7", device, "", "因為在R2000和TR中,所以無法Pause!");
                return false;
            }

            double vel = 0;

            if (SimulationMode)
                vel = ControlData.RealVelocity;
            else
                vel = elmoDriver.ElmoGetVelocity(EnumAxis.GX);

            double decDistance = computeFunction.GetAccDecDistance(vel, 0, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

            WriteLog("MoveControl", "7", device, "", "目前車速為 : " + vel.ToString("0") + ", 預計需要 " + decDistance.ToString("0") + " mm才能停止");
            double stopEncoder = location.RealEncoder + (ControlData.DirFlag ? decDistance : -decDistance);

            WriteLog("MoveControl", "7", device, "", "目前RealEncoder : " + location.RealEncoder.ToString("0") +
                                                      ", 預計停止RealEncoder : " + stopEncoder.ToString("0") + "!");

            if (ControlData.DirFlag)
            {
                return stopEncoder <= command.SectionLineList[command.IndexOflisSectionLine].TransferPositionEnd &&
                       stopEncoder >= command.SectionLineList[command.IndexOflisSectionLine].TransferPositionStart;
            }
            else
            {
                return stopEncoder >= command.SectionLineList[command.IndexOflisSectionLine].TransferPositionEnd &&
                       stopEncoder <= command.SectionLineList[command.IndexOflisSectionLine].TransferPositionStart;
            }
        }

        public bool VehclePause()
        {
            WriteLog("MoveControl", "7", device, "", "Pause Request!");
            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error &&
                !ControlData.PauseRequest && !ControlData.PauseAlready && CanPauseNow())
            {
                ControlData.PauseRequest = true;
                WriteLog("MoveControl", "7", device, "", "Pause Request接受!");
                return true;
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "Pause Request拒絕!");
                return false;
            }
        }

        public void VehcleContinue()
        {
            WriteLog("MoveControl", "7", device, "", "Pause Request!");

            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error && ControlData.PauseAlready)
            {
                WriteLog("MoveControl", "7", device, "", "Pause Request接受!");
                ControlData.PauseAlready = false;
            }
            else
                WriteLog("MoveControl", "7", device, "", "Pause Request拒絕!");
        }

        public void VehcleCancel()
        {
            WriteLog("MoveControl", "7", device, "", "Cancel Request!");
            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error && !ControlData.CancelRequest)
            {
                WriteLog("MoveControl", "7", device, "", "Cancel Request接受!");
                ControlData.CancelRequest = true;
            }
            else
                WriteLog("MoveControl", "7", device, "", "Cancel Request拒絕!");
        }

        public MoveCommandData CreateMoveControlListSectionListReserveList(MoveCmdInfo moveCmd, ref string errorMessage)
        {
            if (ControlData.CloseMoveControl)
            {
                errorMessage = "程式關閉中,拒絕Debug Form命令.";
                return null;
            }
            else if (Vehicle.Instance.AutoState == EnumAutoState.Auto)
            {
                errorMessage = "Auto Mode,拒絕Debug Form命令.";
                return null;
            }
            else if (IsCharging())
            {
                errorMessage = "Charging中";
                return null;
            }
            else if (ForkNotHome())
            {
                errorMessage = "Fork不在Home點";
                return null;
            }
            else if (!SimulationMode && location.Real != null)
            {
                double distance = Math.Sqrt(Math.Pow(location.Real.Position.X - moveCmd.AddressPositions[0].X, 2) +
                                            Math.Pow(location.Real.Position.Y - moveCmd.AddressPositions[0].Y, 2));
                if (distance > 50)
                {
                    errorMessage = "起點和目前位置差距過大.";
                    return null;
                }
            }

            return CreateMoveCommandList.CreateMoveControlListSectionListReserveList(
                        moveCmd, location.Real, ControlData.WheelAngle, ref errorMessage);
        }

        public void GetMoveCommandListInfo(List<Command> cmdList, ref List<string> logMessage)
        {
            logMessage = new List<string>();

            if (cmdList == null)
                CreateMoveCommandList.GetMoveCommandListInfo(command.CommandList, ref logMessage);
            else
                CreateMoveCommandList.GetMoveCommandListInfo(cmdList, ref logMessage);
        }

        public void GetReserveListInfo(List<ReserveData> reserveList, ref List<string> logMessage)
        {
            logMessage = new List<string>();

            if (reserveList == null)
                CreateMoveCommandList.GetReserveListInfo(command.ReserveList, ref logMessage);
            else
                CreateMoveCommandList.GetReserveListInfo(reserveList, ref logMessage);
        }

        public bool TransferMoveDebugMode(MoveCommandData command)
        {
            if (Vehicle.Instance.AutoState == EnumAutoState.Auto)
                return false;

            WriteLog("MoveControl", "7", device, "", "start");
            this.command = command;

            ResetFlag();

            MoveCommandID = "DebugForm" + DateTime.Now.ToString("HH:mm:ss");
            isAGVMCommand = false;
            MoveState = EnumMoveState.Moving;
            WriteLog("MoveControl", "7", device, "", "sucess! 開始執行動作~!");
            return true;
        }

        public void StopFlagOn()
        {
            WriteLog("MoveControl", "7", device, "", "外部控制啟動StopControl!");
            ControlData.FlowStopRequeset = true;
        }

        public bool AddReservedMapPosition(MapPosition mapPosition)
        {
            if (MoveState == EnumMoveState.Idle)
            {
                WriteLog("MoveControl", "7", device, "", "Idle情況不該收到Reserve.. 座標" +
                         "( " + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString("0") + " ) !");
                return false;
            }

            if (command.IndexOfReserveList >= command.ReserveList.Count)
            {
                WriteLog("MoveControl", "7", device, "", "Reserve已經全部取得,但收到Reserve.. 座標" +
                         "( " + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString("0") + " ) !");
                return false;
            }

            if (command.ReserveList[command.IndexOfReserveList].Position.X == mapPosition.X &&
                command.ReserveList[command.IndexOfReserveList].Position.Y == mapPosition.Y)
            {
                command.ReserveList[command.IndexOfReserveList].GetReserve = true;
                command.IndexOfReserveList++;
                WriteLog("MoveControl", "7", device, "", "取得Reserve node : index = " + command.IndexOfReserveList.ToString() +
                         "( " + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString("0") + " ) !");

                return true;
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "Reserve node 跳號或無此Reserve,座標 : ( "
                    + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString() + " ), 跳過 index = " + command.IndexOfReserveList.ToString() +
                         "( " + command.ReserveList[command.IndexOfReserveList].Position.X.ToString("0") + ", " +
                                command.ReserveList[command.IndexOfReserveList].Position.Y.ToString("0") + " ) !");

                return false;
            }
        }

        public void AddReservedIndexForDebugModeTest(int index)
        {
            if (MoveState == EnumMoveState.Idle)
                return;

            if (index >= 0 && index < command.ReserveList.Count)
            {
                for (int i = 0; i <= index; i++)
                {
                    if (!command.ReserveList[i].GetReserve)
                        AddReservedMapPosition(command.ReserveList[i].Position);
                }
            }
        }

        public void AddAllReserve()
        {
            if (MoveState == EnumMoveState.Idle)
                return;

            while (command.IndexOfReserveList < command.ReserveList.Count)
                AddReservedMapPosition(command.ReserveList[command.IndexOfReserveList].Position);
        }

        public int GetReserveIndex()
        {
            return command.IndexOfReserveList;
        }
        #endregion

        #region CSV log
        private void AddCSV(ref string csvLog, string logString)
        {
            csvLog = csvLog + "," + logString;
        }

        private void WriteLogCSV()
        {
            string csvLog;

            string[] oneRowData;
            string[] splitTime;
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            ElmoAxisFeedbackData feedBackData;
            EnumAxis[] order = new EnumAxis[18] { EnumAxis.XFL, EnumAxis.XFR, EnumAxis.XRL, EnumAxis.XRR,
                                                  EnumAxis.TFL, EnumAxis.TFR, EnumAxis.TRL, EnumAxis.TRR,
                                                  EnumAxis.VXFL, EnumAxis.VXFR, EnumAxis.VXRL, EnumAxis.VXRR,
                                                  EnumAxis.VTFL, EnumAxis.VTFR, EnumAxis.VTRL, EnumAxis.VTRR,
                                                  EnumAxis.GX, EnumAxis.GT };
            AGVPosition logAGVPosition;
            ThetaSectionDeviation logThetaDeviation;
            Sr2000ReadData logReadData;
            DateTime now;

            bool csvLogResult;

            while (true)
            {
                timer.Reset();
                timer.Start();
                //  Debug 
                //BarcodeX	BarocdeY	ElmoEncoder	elmoV	TurnP	TurnV

                csvLogResult = DebugCSVMode && (MoveState != EnumMoveState.Idle);
                //  Time
                now = DateTime.Now;
                csvLog = now.ToString("yyyy/MM/dd HH:mm:ss.fff");

                //  State
                AddCSV(ref csvLog, MoveState.ToString());

                //  RealEncoder
                AddCSV(ref csvLog, location.RealEncoder.ToString("0.0"));

                //  NextCommand	TriggerEncoder
                if (MoveState != EnumMoveState.Idle && command.IndexOfCmdList < command.CommandList.Count)
                {
                    AddCSV(ref csvLog, command.CommandList[command.IndexOfCmdList].CmdType.ToString());

                    if (command.CommandList[command.IndexOfCmdList].Position != null)
                        AddCSV(ref csvLog, command.CommandList[command.IndexOfCmdList].TriggerEncoder.ToString("0"));
                    else
                        AddCSV(ref csvLog, "Now");
                }
                else
                {
                    AddCSV(ref csvLog, "Empty");
                    AddCSV(ref csvLog, "Empty");
                }

                //  Delta
                AddCSV(ref csvLog, location.Delta.ToString("0.0"));

                //  Offset
                AddCSV(ref csvLog, location.Offset.ToString("0.0"));

                //  RealPosition
                logAGVPosition = location.Real;
                if (logAGVPosition != null)
                {
                    AddCSV(ref csvLog, logAGVPosition.Position.X.ToString("0.0"));
                    AddCSV(ref csvLog, logAGVPosition.Position.Y.ToString("0.0"));
                }
                else
                {
                    AddCSV(ref csvLog, "N/A");
                    AddCSV(ref csvLog, "N/A");
                }

                //  BarcodePosition
                //  X Y
                logAGVPosition = location.Barcode;
                if (logAGVPosition != null)
                {
                    AddCSV(ref csvLog, logAGVPosition.Position.X.ToString("0.0"));
                    AddCSV(ref csvLog, logAGVPosition.Position.Y.ToString("0.0"));
                }
                else
                {
                    AddCSV(ref csvLog, "N/A");
                    AddCSV(ref csvLog, "N/A");
                }

                //  EncoderPosition
                //  X Y
                logAGVPosition = location.Encoder;
                if (logAGVPosition != null && logAGVPosition.Position != null)
                {
                    AddCSV(ref csvLog, logAGVPosition.Position.X.ToString("0.0"));
                    AddCSV(ref csvLog, logAGVPosition.Position.Y.ToString("0.0"));
                }
                else
                {
                    AddCSV(ref csvLog, "N/A");
                    AddCSV(ref csvLog, "N/A");
                }

                logThetaDeviation = location.ThetaAndSectionDeviation;
                if (logThetaDeviation != null)
                {
                    AddCSV(ref csvLog, logThetaDeviation.Theta.ToString("0.0"));
                    AddCSV(ref csvLog, logThetaDeviation.SectionDeviation.ToString("0.0"));
                }
                else
                {
                    AddCSV(ref csvLog, "N/A");
                    AddCSV(ref csvLog, "N/A");
                }

                //  SR2000
                //  count	scanTime	X	Y	theta   BarcodeAngle    delta	theta   
                for (int i = 0; i < 2; i++)
                {
                    if (DriverSr2000List.Count > i)
                    {
                        logReadData = DriverSr2000List[i].GetReadData();
                        if (logReadData != null)
                        {
                            logAGVPosition = logReadData.AGV;
                            logThetaDeviation = logReadData.ReviseData;
                            if (logAGVPosition != null)
                            {
                                AddCSV(ref csvLog, logAGVPosition.Count.ToString("0"));
                                AddCSV(ref csvLog, logAGVPosition.GetDataTime.ToString("HH:mm:ss.ff"));
                                AddCSV(ref csvLog, logAGVPosition.ScanTime.ToString("0"));
                                AddCSV(ref csvLog, logAGVPosition.Position.X.ToString("0"));
                                AddCSV(ref csvLog, logAGVPosition.Position.Y.ToString("0"));
                                AddCSV(ref csvLog, logAGVPosition.AGVAngle.ToString("0.0"));
                                AddCSV(ref csvLog, logAGVPosition.BarcodeAngleInMap.ToString("0"));
                            }
                            else
                            {
                                AddCSV(ref csvLog, "N/A");
                                AddCSV(ref csvLog, "N/A");
                                AddCSV(ref csvLog, "N/A");
                                AddCSV(ref csvLog, "N/A");
                                AddCSV(ref csvLog, "N/A");
                                AddCSV(ref csvLog, "N/A");
                                AddCSV(ref csvLog, "N/A");
                            }

                            if (logThetaDeviation != null)
                            {
                                AddCSV(ref csvLog, logThetaDeviation.Theta.ToString("0.0"));
                                AddCSV(ref csvLog, logThetaDeviation.SectionDeviation.ToString("0.0"));
                            }
                            else
                            {
                                AddCSV(ref csvLog, "N/A");
                                AddCSV(ref csvLog, "N/A");
                            }

                            if (logReadData.Barcode1 != null && logReadData.Barcode2 != null)
                            {
                                AddCSV(ref csvLog, logReadData.Barcode1.ID.ToString("0"));
                                AddCSV(ref csvLog, logReadData.Barcode2.ID.ToString("0"));
                            }
                            else
                            {
                                AddCSV(ref csvLog, "N/A");
                                AddCSV(ref csvLog, "N/A");
                            }
                        }
                    }
                    else
                    {
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                    }
                }

                //  Elmo
                //  count   position	velocity	toc	disable	moveComplete	error
                for (int i = 0; i < 8; i++)
                {
                    feedBackData = elmoDriver.ElmoGetFeedbackData(order[i]);
                    if (feedBackData != null)
                    {
                        AddCSV(ref csvLog, feedBackData.Count.ToString("0"));
                        AddCSV(ref csvLog, feedBackData.Feedback_Position.ToString("0.0"));
                        AddCSV(ref csvLog, feedBackData.Feedback_Velocity.ToString("0.0"));
                        AddCSV(ref csvLog, feedBackData.Feedback_Position_Error.ToString("0.0"));
                        AddCSV(ref csvLog, feedBackData.Feedback_Torque.ToString("0.0"));
                        AddCSV(ref csvLog, feedBackData.Disable ? "Disable" : "Enable");
                        AddCSV(ref csvLog, feedBackData.StandStill ? "Stop" : "Move");
                        AddCSV(ref csvLog, feedBackData.ErrorStop ? "Error" : "Normal");
                    }
                    else
                    {
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                    }
                }

                for (int i = 8; i < 16; i++)
                {
                    feedBackData = elmoDriver.ElmoGetFeedbackData(order[i]);
                    if (feedBackData != null)
                    {
                        AddCSV(ref csvLog, feedBackData.Count.ToString("0"));
                        AddCSV(ref csvLog, feedBackData.Feedback_Position.ToString("0.0"));
                        AddCSV(ref csvLog, feedBackData.Disable ? "Disable" : "Enable");
                        AddCSV(ref csvLog, feedBackData.StandStill ? "Stop" : "Move");
                        AddCSV(ref csvLog, feedBackData.ErrorStop ? "Error" : "Normal");
                    }
                    else
                    {
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                        AddCSV(ref csvLog, "N/A");
                    }
                }

                for (int i = 16; i < 18; i++)
                {
                    AddCSV(ref csvLog, elmoDriver.MoveCompelete(order[i]) ? "Stop" : "Move");
                }

                logger.SavePureLog(csvLog);

                if (csvLogResult)
                {
                    oneRowData = Regex.Split(csvLog, ",", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
                    splitTime = Regex.Split(oneRowData[0], " ", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
                    if (splitTime != null && splitTime.Length > 1)
                        oneRowData[0] = splitTime[1];

                    lock (deubgCsvLogList)
                    {
                        deubgCsvLogList.Add(oneRowData);
                        if (deubgCsvLogList.Count > 3000)
                            deubgCsvLogList.RemoveAt(0);
                    }
                }

                while (timer.ElapsedMilliseconds < moveControlConfig.CSVLogInterval)
                    Thread.Sleep(1);
            }
        }
        #endregion
    }
}