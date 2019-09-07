using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
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

namespace Mirle.Agv.Controller
{
    public class MoveControlHandler
    {
        private CreateMoveControlList createMoveControlList;
        public EnumMoveState MoveState { get; private set; } = EnumMoveState.Idle;
        public MoveControlConfig moveControlConfig;
        private MapInfo theMapInfo = new MapInfo();
        private Logger logger = LoggerAgent.Instance.GetLooger("MoveControlCSV");
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private string device = "MoveControl";
        private Dictionary<EnumAddressAction, TRTimeToAngleRange> trTimeToAngleRange = new Dictionary<EnumAddressAction, TRTimeToAngleRange>();

        public ElmoDriver elmoDriver;
        public List<Sr2000Driver> DriverSr2000List = new List<Sr2000Driver>();
        public OntimeReviseConfig ontimeReviseConfig = null;
        private AgvMoveRevise agvRevise;

        public event EventHandler<EnumMoveComplete> OnMoveFinished;

        public MoveCommandData command { get; private set; } = new MoveCommandData();

        public Location location = new Location();
        private AlarmHandler alarmHandler;

        private ComputeFunction computeFunction = new ComputeFunction();

        public MoveControlParameter ControlData { get; private set; } = new MoveControlParameter();
        private Thread threadSCVLog;
        public bool DebugCSVMode { get; set; } = false;
        public List<string[]> deubgCsvLogList = new List<string[]>();
        private bool isAGVMCommand = false;
        public string MoveCommandID { get; set; } = "";

        public bool SimulationMode { get; set; } = false;
        private bool simulationIsMoving = false;
        private MoveControlSafetyData safetyData = new MoveControlSafetyData();
        private Dictionary<int, double> TRAngleToTime = new Dictionary<int, double>();

        public bool DebugFlowMode { get; set; } = true;
        public string AGVStopResult { get; set; }
        private const int debugFlowLogMaxLength = 10000;

        public string DebugFlowLog { get; set; }
        public int WaitReseveIndex { get; set; }
        public double LoopTime { get; set; } = 0;
        private System.Diagnostics.Stopwatch loopTimeTimer = new System.Diagnostics.Stopwatch();

        public string autoTime { get; set; } = "啟動時間 : ";
        public string errorTime { get; set; } = "異常停止時間 : ";

        private void SetDebugFlowLog(string functionName, string message)
        {
            if (DebugFlowMode)
            {
                DebugFlowLog = DateTime.Now.ToString("HH:mm:ss.fff") + "\t" + functionName + "\t" + message + "\r\n" + DebugFlowLog;
                if (DebugFlowLog.Length > debugFlowLogMaxLength)
                    DebugFlowLog = DebugFlowLog.Substring(0, debugFlowLogMaxLength);
            }
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

        public MoveControlHandler(MapInfo theMapInfo, AlarmHandler alarmHandler)
        {
            this.alarmHandler = alarmHandler;
            this.theMapInfo = theMapInfo;
            ReadMoveControlConfigXML("MoveControlConfig.xml");
            SetTRTimeToAngleRange();
            InitailSr2000(moveControlConfig.Sr2000ConfigPath);
            elmoDriver = new ElmoDriver(moveControlConfig.ElmoConfigPath, this.alarmHandler);

            ReadOntimeReviseConfigXML(moveControlConfig.OnTimeReviseConfigPath);
            createMoveControlList = new CreateMoveControlList(DriverSr2000List, moveControlConfig, this.alarmHandler);

            agvRevise = new AgvMoveRevise(ontimeReviseConfig, elmoDriver, DriverSr2000List, moveControlConfig.Safety, this.alarmHandler);

            loopTimeTimer.Reset();
            loopTimeTimer.Start();

            ControlData.MoveControlThread = new Thread(MoveControlThread);
            ControlData.MoveControlThread.Start();

            threadSCVLog = new Thread(WriteLogCSV);
            threadSCVLog.Start();
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
                        moveControlConfig.SafteyDistance.Add(
                            (EnumCommandType)Enum.Parse(typeof(EnumCommandType), item.Name),
                            double.Parse(item.InnerText));
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
                        temp = ReadSafetyDataXML((XmlElement)item);
                        moveControlConfig.Safety.Add(
                            (EnumMoveControlSafetyType)Enum.Parse(typeof(EnumMoveControlSafetyType), item.Name),
                            temp);
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
                        temp.Enable = (item.InnerText == "Enable");
                        moveControlConfig.SensorByPass.Add(
                            (EnumSensorSafetyType)Enum.Parse(typeof(EnumSensorSafetyType), item.Name),
                            temp);
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
                    case "LowVelocity":
                        moveControlConfig.LowVelocity = double.Parse(item.InnerText);
                        break;
                    case "MoveCommandDistanceMagnification":
                        moveControlConfig.MoveCommandDistanceMagnification = double.Parse(item.InnerText);
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
                Vehicle.Instance.CurVehiclePosition.RealPosition = location.Real.Position;
            }
        }

        private void UpdateDelta()
        {
            double realEncoder = MapPositionToEncoder(command.SectionLineList[command.IndexOflisSectionLine], location.Barcode.Position);
            // 此Barcode是多久之前的資料,基本上為正值(s).
            double deltaTime = ((double)location.ScanTime + (DateTime.Now - location.BarcodeGetDataTime).TotalMilliseconds) / 1000;
            // 真實Barcode回推的RealEncoder需要再加上這個時間*速度(Elmo速度本身就帶正負號).
            realEncoder = realEncoder + location.XFLVelocity * deltaTime;
            // RealEncoder = elmoEncoder + offset + delta.
            location.Delta = realEncoder - (location.ElmoEncoder + location.Offset);

            if (location.Encoder != null && moveControlConfig.Safety[EnumMoveControlSafetyType.UpdateDeltaPositionRange].Enable)
            {
                double deltaTimeDistance = Math.Abs(location.XFLVelocity) *
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

                    Vehicle.Instance.CurVehiclePosition.BarcodePosition = location.Barcode.Position;
                    return true;
                }
            }
            else
            {
                if (location.Barcode == null)
                {
                    MapPosition tempPosition = new MapPosition(0, 0);
                    //location.Barcode = new AGVPosition(tempPosition, 0, 0, 20, DateTime.Now, 0, 0, EnumBarcodeMaterial.Iron);
                    location.Barcode = new AGVPosition(tempPosition, 90, 0, 20, DateTime.Now, 0, 0, EnumBarcodeMaterial.Iron);
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

                if (elmoData != null)
                {
                    location.XFLVelocity = elmoData.Feedback_Velocity;
                    location.XRRVelocity = elmoDriver.ElmoGetVelocity(EnumAxis.XRR);
                    location.ElmoGetDataTime = elmoData.GetDataTime;
                    // 此筆Elmo資料是多久之前的,基本上時間會是正值(s).
                    double deltaTime = ((DateTime.Now - location.ElmoGetDataTime).TotalMilliseconds + moveControlConfig.SleepTime / 2) / 1000;
                    // 真實Encoder需要再加上這個時間*速度(Elmo速度本身就帶正負號).
                    location.ElmoEncoder = elmoData.Feedback_Position + location.XFLVelocity * deltaTime;
                }
            }
            else
            {
                if (!simulationIsMoving)
                {
                    location.XFLVelocity = 0;
                    location.XRRVelocity = 0;
                }
                else
                {
                    location.XFLVelocity = ControlData.DirFlag ? ControlData.VelocityCommand : -ControlData.VelocityCommand;
                    location.XRRVelocity = ControlData.DirFlag ? ControlData.VelocityCommand : -ControlData.VelocityCommand;

                    location.ElmoEncoder = location.ElmoEncoder + 0.005 * location.XFLVelocity;
                }

                location.ElmoGetDataTime = DateTime.Now;
            }
        }

        private void SafetyTurnOutAndLineBarcodeInterval(bool newBarcodeData)
        {
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

        private void UpdatePosition()
        {
            LoopTime = loopTimeTimer.ElapsedMilliseconds;
            loopTimeTimer.Reset();
            loopTimeTimer.Start();

            UpdateElmo();
            bool newBarcodeData = UpdateSR2000();

            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
            {
                if (MoveState != EnumMoveState.TR && MoveState != EnumMoveState.R2000 && newBarcodeData)
                    UpdateDelta();

                UpdateReal();
                SafetyTurnOutAndLineBarcodeInterval(newBarcodeData);
            }
            else
            {
                if (Vehicle.Instance.AutoState != EnumAutoState.Auto && newBarcodeData)
                {
                    location.Real = location.Barcode;
                    location.Real.AGVAngle = computeFunction.GetAGVAngle(location.Real.AGVAngle);
                    Vehicle.Instance.CurVehiclePosition.RealPosition = location.Real.Position;
                    Vehicle.Instance.CurVehiclePosition.VehicleAngle = location.Real.AGVAngle;
                }
            }
        }
        #endregion

        #region CommandControl
        private void GetAccTimeAndDistance(double vel, double acc, double jerk, ref double time, ref double distance)
        {
            time = acc / jerk;
            vel = Math.Abs(vel);
            double deltaVelocity = time * acc / 2 * 2;
            double lastDeltaVelocity;
            double lastDeltaTime;

            if (deltaVelocity == vel)
            {
                time = time * 2;
            }
            else if (deltaVelocity > vel)
            {
                deltaVelocity = vel / 2;
                time = Math.Sqrt(deltaVelocity * 2 / jerk);
                time = time * 2;
            }
            else
            {
                lastDeltaVelocity = vel - deltaVelocity;
                lastDeltaTime = lastDeltaVelocity / acc;
                time = 2 * time + lastDeltaTime;
            }

            distance = vel * time / 2;
        }

        private double GetTRStopTurnDec()
        {
            return 0;
            double vel = (location.XFLVelocity + location.XRRVelocity) / 2;
            double time = 0;
            double distance = 0;
            double nowEncoder = 0;

            GetAccTimeAndDistance(vel, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk, ref time, ref distance);
            nowEncoder = location.ElmoEncoder + (ControlData.DirFlag ? distance : -distance);
            double deltaAngle = GetTRFlowAngleChange(nowEncoder);
            double nowAngle = Math.Abs(ControlData.WheelAngle - elmoDriver.ElmoGetGTPosition());
            double nowTurnVelocity = Math.Abs(elmoDriver.ElmoGetGTVelocity());
            double decTime = 2 * Math.Abs(deltaAngle - nowAngle) / nowTurnVelocity;
            return Math.Abs(deltaAngle - nowAngle) / decTime;
        }

        private double GetTRStartTurnAcc()
        {
            return 0;
            double vel = moveControlConfig.TurnParameter[ControlData.NowAction].Velocity;
            double time = 0;
            double distance = 0;
            double nowEncoder = 0;

            GetAccTimeAndDistance(vel, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Jerk, ref time, ref distance);
            nowEncoder = location.ElmoEncoder + (ControlData.DirFlag ? distance : -distance);
            double deltaAngle = GetTRFlowAngleChange(nowEncoder);
            double nowAngle = Math.Abs(ControlData.WheelAngle - elmoDriver.ElmoGetGTPosition());

            double nowTurnVelocity = Math.Abs(elmoDriver.ElmoGetGTVelocity());
            double decTime = 2 * Math.Abs(deltaAngle - nowAngle) / nowTurnVelocity;
            return Math.Abs(deltaAngle - nowAngle) / decTime;
            //return Math.Abs(deltaAngle - nowAngle) * 2 / time / time;
        }

        private void GetTRStartTurnAccDecAngleWithSensorStop(ref double acc, ref double dec, ref double angle)
        {
            acc = 165;
            dec = 165;
            angle = 5;
        }

        private bool IsInTRPath(EnumAddressAction type, double encoder, double startAngle, double targetAngle)
        {
            double nowAngle = elmoDriver.ElmoGetGTPosition();
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

            return Math.Abs(idealAngle - nowAngle) <= 10;
        }

        private void TRControl_SimulationMode(int wheelAngle, EnumAddressAction type)
        {
            double velocity = moveControlConfig.TurnParameter[type].Velocity;
            double r = moveControlConfig.TurnParameter[type].R;

            WriteLog("MoveControl", "7", device, "", "start, velocity : " + velocity.ToString("0") + ", r : " + r.ToString("0") +
                ", 舵輪將旋轉至 " + wheelAngle.ToString("0") + "度!");
            MoveState = EnumMoveState.TR;
            ControlData.NowAction = type;

            double xFLVelocity = Math.Abs(location.XFLVelocity);
            double xRRVelocity = Math.Abs(location.XRRVelocity);
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
                if (ControlData.FlowStopRequeset)
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
            if (SimulationMode)
            {
                TRControl_SimulationMode(wheelAngle, type);
                return;
            }

            double velocity = moveControlConfig.TurnParameter[type].Velocity;
            double r = moveControlConfig.TurnParameter[type].R;
            double safetyVelocityRange = moveControlConfig.TurnParameter[type].SafetyVelocityRange;

            WriteLog("MoveControl", "7", device, "", "start, velocity : " + velocity.ToString("0") + ", r : " + r.ToString("0") +
                ", 舵輪將旋轉至 " + wheelAngle.ToString("0") + "度!");
            MoveState = EnumMoveState.TR;
            ControlData.NowAction = type;

            double xFLVelocity = Math.Abs(location.XFLVelocity);
            double xRRVelocity = Math.Abs(location.XRRVelocity);
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
                WriteLog("MoveControl", "4", device, "", " TR中 GT Moving~");

            if (xFLVelocity > velocity + safetyVelocityRange && xRRVelocity > velocity + safetyVelocityRange)
            { // 超速GG, 不該發生.
                EMSControl("超速.., XFL vel : " + xFLVelocity.ToString("0") + ", XRR vel : " + xRRVelocity.ToString("0"));
                return;
            }

            if (ControlData.SensorStop)
            {
                double acc = 0;
                double dec = 0;
                double angle = 0;
                GetTRStartTurnAccDecAngleWithSensorStop(ref acc, ref dec, ref angle);

                elmoDriver.ElmoMove(EnumAxis.GT, angle, moveControlConfig.TurnParameter[type].AxisParameter.Velocity, EnumMoveType.Absolute,
                                    acc, dec, moveControlConfig.TurnParameter[type].AxisParameter.Jerk);
            }
            else
            {
                if (Math.Abs(xFLVelocity - velocity) <= safetyVelocityRange &&
                    Math.Abs(xRRVelocity - velocity) <= safetyVelocityRange)
                { // Normal
                    elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.TurnParameter[type].AxisParameter.Velocity, EnumMoveType.Absolute,
                                        moveControlConfig.TurnParameter[type].AxisParameter.Acceleration,
                                        moveControlConfig.TurnParameter[type].AxisParameter.Deceleration,
                                        moveControlConfig.TurnParameter[type].AxisParameter.Jerk);
                }
                else
                { // 太慢 處理??
                    EMSControl("速度過慢.., XFL vel : " + xFLVelocity.ToString("0") +
                                                             ", XRR vel : " + xRRVelocity.ToString("0"));
                }
            }

            while (!elmoDriver.WheelAngleCompare(wheelAngle, moveControlConfig.StartWheelAngleRange))
            {
                UpdatePosition();
                SensorSafety();
                if (ControlData.FlowStopRequeset)
                    return;

                if (!IsInTRPath(type, Math.Abs(ControlData.TurnStartEncoder - location.ElmoEncoder), ControlData.WheelAngle, wheelAngle))
                {
                    EMSControl("不再TR預計路徑上,異常停止!");
                    return;
                }

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
                WriteLog("MoveControl", "4", device, "", "R2000取得奇怪的wheelAngle : " + wheelAngle.ToString("0.0"));
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

                if (ControlData.FlowStopRequeset)
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

                if (ControlData.FlowStopRequeset)
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

            Vehicle.Instance.CurVehiclePosition.VehicleAngle = location.Real.AGVAngle;

            DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));

            MoveState = EnumMoveState.Moving;
            safetyData.TurningByPass = false;
            ControlData.CanPause = true;
            WriteLog("MoveControl", "7", device, "", " end.");
        }

        public void R2000Control(int wheelAngle)
        {
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
            double xFLVelocity = Math.Abs(location.XFLVelocity);
            double xRRVelocity = Math.Abs(location.XRRVelocity);

            if (Math.Abs(xFLVelocity - velocity) <= safetyVelocityRange &&
                Math.Abs(xRRVelocity - velocity) <= safetyVelocityRange)
            { // Normal
            }
            else if (xFLVelocity > velocity && xRRVelocity > velocity)
            { // 超速GG, 不該發生.
                EMSControl("超速.., XFL vel : " + xFLVelocity.ToString("0") +
                                                         ", XRR vel : " + xRRVelocity.ToString("0"));
                return;
            }
            else
            { // 太慢 處理??
                EMSControl("速度過慢.., XFL vel : " + xFLVelocity.ToString("0") +
                                                         ", XRR vel : " + xRRVelocity.ToString("0"));
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
                WriteLog("MoveControl", "4", device, "", "R2000取得奇怪的wheelAngle : " + wheelAngle.ToString("0.0"));
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

                if (ControlData.FlowStopRequeset)
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

                if (ControlData.FlowStopRequeset)
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

            Vehicle.Instance.CurVehiclePosition.VehicleAngle = location.Real.AGVAngle;

            DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));

            MoveState = EnumMoveState.Moving;
            safetyData.TurningByPass = false;
            ControlData.CanPause = true;
            WriteLog("MoveControl", "7", device, "", " end.");
        }

        public bool CanVehPause()
        {
            return false;
        }

        private void VchangeControl(double velocity, EnumVChangeType vChangeType, int TRWheelAngle = 0)
        {
            WriteLog("MoveControl", "7", device, "", " start, Velocity : " + velocity.ToString("0"));

            if (vChangeType != EnumVChangeType.SensorSlow)
                ControlData.VelocityCommand = velocity;

            if (velocity != ControlData.RealVelocity)
            {
                if (!ControlData.SensorSlow || velocity <= moveControlConfig.LowVelocity)
                {
                    ControlData.RealVelocity = velocity;
                    agvRevise.SettingReviseData(velocity, ControlData.DirFlag);

                    if (!ControlData.SensorStop)
                    {
                        velocity /= moveControlConfig.Move.Velocity;
                        elmoDriver.ElmoGroupVelocityChange(EnumAxis.GX, velocity);
                    }
                }
            }

            if (vChangeType == EnumVChangeType.TRTurn)
            {
                ControlData.CanPause = false;

                if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensorTR].Enable)
                    safetyData.TurningByPass = true;

                switch (TRWheelAngle)
                {
                    case 0:
                        DirLightTurn(ControlData.WheelAngle == 90 ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                        break;
                    case 90:
                        DirLightTurn(EnumBeamSensorLocate.Left);
                        break;
                    case -90:
                        DirLightTurn(EnumBeamSensorLocate.Right);
                        break;
                    default:
                        EMSControl("switch (TRWheelAngle) default..EMS.");
                        break;
                }
            }
            else if (vChangeType == EnumVChangeType.R2000Turn)
            {
                ControlData.CanPause = false;

                if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensorR2000].Enable)
                    safetyData.TurningByPass = true;

                DirLightTurn(ControlData.WheelAngle == -1 ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
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
                elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.Turn.Velocity, EnumMoveType.Absolute,
                        moveControlConfig.Turn.Acceleration, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);

                timer.Reset();
                timer.Start();
                Thread.Sleep(moveControlConfig.SleepTime * 2);
                while (!elmoDriver.MoveCompelete(EnumAxis.GT) && !SimulationMode)
                {
                    UpdatePosition();
                    SensorSafety();

                    if (timer.ElapsedMilliseconds > moveControlConfig.TurnTimeoutValue)
                    {
                        EMSControl("舵輪旋轉Timeout!");
                        return;
                    }

                    Thread.Sleep(moveControlConfig.SleepTime);
                }

                ControlData.WheelAngle = wheelAngle;
            }

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

            if (moveType == EnumMoveStartType.FirstMove)
            {
                ControlData.TrigetEndEncoder = dirFlag ? distance : -distance;
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
                ControlData.TrigetEndEncoder = ControlData.TrigetEndEncoder + (dirFlag ? distance : -distance);
                command.IndexOflisSectionLine++;
            }

            if (moveType == EnumMoveStartType.FirstMove || moveType == EnumMoveStartType.ChangeDirFlagMove)
            {
                ControlData.DirFlag = dirFlag;
            }

            if (moveType != EnumMoveStartType.SensorStopMove)
                ControlData.VelocityCommand = velocity;

            ControlData.RealVelocity = velocity;

            if (dirFlag)
                elmoDriver.ElmoMove(EnumAxis.GX, distance, velocity, EnumMoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            else
                elmoDriver.ElmoMove(EnumAxis.GX, -distance, velocity, EnumMoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

            simulationIsMoving = true;
            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void SlowStopControl(MapPosition endPosition, int nextReserveIndex)
        {
            WriteLog("MoveControl", "7", device, "", "start");

            if (!ControlData.SensorStop)
                elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Reset();
            timer.Start();
            while (!elmoDriver.MoveCompelete(EnumAxis.GX))
            {
                UpdatePosition();
                SensorSafety();

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
            else
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (command.IndexOfCmdList + i < command.CommandList.Count)
                    {
                        if (command.CommandList[command.IndexOfCmdList + i].CmdType == EnumCommandType.Move)
                            break;

                        if (command.CommandList[command.IndexOfCmdList + i].Position != null)
                        {
                            if ((ControlData.DirFlag && location.RealEncoder > command.CommandList[command.IndexOfCmdList + i].TriggerEncoder) ||
                               (!ControlData.DirFlag && location.RealEncoder < command.CommandList[command.IndexOfCmdList + i].TriggerEncoder))
                            {
                                WriteLog("MoveControl", "7", device, "", "Reserve Stop, 由於action : " + command.CommandList[command.IndexOfCmdList + i].CmdType.ToString() +
                                                                     "的觸發點已超過目前位置,更改為立即觸發!");
                                command.CommandList[command.IndexOfCmdList + i].Position = null;
                            }
                        }
                    }
                }

                double distance = ControlData.TrigetEndEncoder - location.RealEncoder;

                Command temp = createMoveControlList.NewMoveCommand(location.Real.Position, location.RealEncoder, Math.Abs(distance),
                    moveControlConfig.Move.Velocity, ControlData.DirFlag, 0, EnumMoveStartType.ReserveStopMove, nextReserveIndex);
                command.CommandList.Insert(command.IndexOfCmdList + 1, temp);

                if (ControlData.VelocityCommand != moveControlConfig.Move.Velocity)
                {
                    double vel = GetVChangeVelocity(ControlData.VelocityCommand);
                    temp = createMoveControlList.NewVChangeCommand(null, 0, vel, ControlData.DirFlag);
                    command.CommandList.Insert(command.IndexOfCmdList + 2, temp);
                }
            }

            simulationIsMoving = false;
            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void SecondCorrectionControl(double endEncoder)
        {
            WriteLog("MoveControl", "7", device, "", "start");
            UpdatePosition();

            WriteLog("MoveControl", "7", device, "", "nowEncoder : " + location.RealEncoder.ToString("0") + ", endEncoder : " + endEncoder.ToString("0"));

            if (Math.Abs(endEncoder - location.RealEncoder) > moveControlConfig.SecondCorrectionX)
            {
                elmoDriver.ElmoMove(EnumAxis.GX, endEncoder - location.RealEncoder, moveControlConfig.EQ.Velocity, EnumMoveType.Relative,
                                    moveControlConfig.EQ.Acceleration, moveControlConfig.EQ.Deceleration, moveControlConfig.EQ.Jerk);

                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Reset();
                timer.Start();

                Thread.Sleep(moveControlConfig.SleepTime * 2);
                while (!elmoDriver.MoveCompelete(EnumAxis.GX))
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
            MoveState = EnumMoveState.Idle;
            MoveFinished(EnumMoveComplete.Success);
            WriteLog("MoveControl", "7", device, "", "end");
            WriteLog("MoveControl", "7", device, "", "Move Compelete !");
        }

        private void StopControl()
        {
            WriteLog("MoveControl", "7", device, "", "start");
            elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            elmoDriver.ElmoStop(EnumAxis.GT, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);

            if (MoveState == EnumMoveState.R2000)
            {
                WriteLog("MoveControl", "7", device, "", "為R2000中的StopControl需停止虛擬軸.");
                elmoDriver.ElmoStop(EnumAxis.VTFL);
                elmoDriver.ElmoStop(EnumAxis.VTFR);
                elmoDriver.ElmoStop(EnumAxis.VTRL);
                elmoDriver.ElmoStop(EnumAxis.VTRR);
                elmoDriver.ElmoStop(EnumAxis.VXFL);
                elmoDriver.ElmoStop(EnumAxis.VXFR);
                elmoDriver.ElmoStop(EnumAxis.VXRL);
                elmoDriver.ElmoStop(EnumAxis.VXRR);
            }

            ControlData.MoveControlStop = false;
            simulationIsMoving = false;
            BeamSensorCloseAll();
            DirLightCloseAll();

            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void SensorStopControl()
        {
            WriteLog("SensorStopControl", "7", device, "", "start, MoveState : " + MoveState.ToString());

            switch (MoveState)
            {
                case EnumMoveState.Moving:
                    elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                    break;
                case EnumMoveState.TR:

                    WriteLog("SensorStopControl", "7", device, "", "EnumMoveState.TR ");

                    double dec = GetTRStopTurnDec();
                    elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                    elmoDriver.ElmoStop(EnumAxis.GT, dec, moveControlConfig.TurnParameter[ControlData.NowAction].AxisParameter.Jerk);
                    WriteLog("SensorStopControl", "7", device, "", dec.ToString("0.0"));

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

                        AGVStopResult = "TR Flow Start Disable!";
                        MoveFinished(EnumMoveComplete.Fail);
                        BeamSensorCloseAll();
                        DirLightCloseAll();
                        MoveState = EnumMoveState.Error;
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

                        while (!elmoDriver.MoveCompelete(EnumAxis.GX))
                        {
                            UpdatePosition();
                            SensorSafety();

                            if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue)
                            {
                                EMSControl("R2000 Flow Stop TimeOut!");
                            }

                            Thread.Sleep(moveControlConfig.SleepTime);
                        }

                        AGVStopResult = "R2000 Flow Start Disable!";
                        MoveFinished(EnumMoveComplete.Fail);
                        BeamSensorCloseAll();
                        DirLightCloseAll();
                        MoveState = EnumMoveState.Error;
                    }

                    break;
                default:
                    break;
            }

            WriteLog("SensorStopControl", "7", device, "", "end");
        }

        private void EMSControl(string emsResult)
        {
            WriteLog("MoveControl", "7", device, "", "start");
            AGVStopResult = emsResult;
            WriteLog("MoveControl", "7", device, "", "EMS Stop : " + emsResult);

            errorTime = "異常停止時間 : " + DateTime.Now.ToString("HH:mm:ss");

            elmoDriver.ElmoStop(EnumAxis.GX);
            elmoDriver.ElmoStop(EnumAxis.GT);
            MoveState = EnumMoveState.Error;
            MoveFinished(EnumMoveComplete.Fail);
            ControlData.OntimeReviseFlag = false;
            simulationIsMoving = false;
            BeamSensorCloseAll();
            DirLightCloseAll();
            WriteLog("MoveControl", "7", device, "", "end");
        }
        #endregion

        #region 檢查觸發.
        private bool CheckGetNextReserve(Command cmd)
        {
            if (cmd.NextRserveCancel)
            {
                if (cmd.NextReserveNumber < command.ReserveList.Count)
                {
                    if (command.ReserveList[cmd.NextReserveNumber].GetReserve)
                    {
                        command.IndexOfCmdList++;
                        WriteLog("MoveControl", "7", device, "", "取得下段Reserve點, 因此取消此命令~!");
                        return false;
                    }
                    else
                        return true;
                }
                else
                {
                    EMSControl("??? 取得下段Reserve要取消此命令,但是Reserve List並無下個Reserve點? ..EMS..");
                    return false;
                }
            }
            else
                return true;
        }

        private bool TriggerCommand(Command cmd)
        {
            if (ControlData.SensorStop && (cmd.CmdType == EnumCommandType.Move || cmd.CmdType == EnumCommandType.End))
                return false;

            if (cmd.ReserveNumber < command.ReserveList.Count && (cmd.ReserveNumber == -1 || command.ReserveList[cmd.ReserveNumber].GetReserve))
            {
                WaitReseveIndex = -1;

                if (cmd.Position == null)
                {
                    if (CheckGetNextReserve(cmd))
                    {
                        WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 觸發,為立即觸發");
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    if (cmd.DirFlag)
                    {
                        if (location.RealEncoder > cmd.TriggerEncoder + cmd.SafetyDistance)
                        {
                            EMSControl("Command : " + cmd.CmdType.ToString() + ", 超過Triiger觸發區間,EMS.. dirFlag : " +
                                         (ControlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
                                         location.RealEncoder.ToString("0.0") + ", triggerEncoder : " + cmd.TriggerEncoder.ToString("0.0"));
                            return false;
                        }
                        else if (location.RealEncoder > cmd.TriggerEncoder)
                        {
                            if (CheckGetNextReserve(cmd))
                            {
                                WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 觸發, dirFlag : " +
                                          (ControlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
                                         location.RealEncoder.ToString("0.0") + ", triggerEncoder : " + cmd.TriggerEncoder.ToString("0.0"));
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                    else
                    {
                        if (location.RealEncoder < cmd.TriggerEncoder - cmd.SafetyDistance)
                        {
                            EMSControl("Command : " + cmd.CmdType.ToString() + ", 超過Triiger觸發區間,EMS.. dirFlag : " +
                                         (ControlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
                                         location.RealEncoder.ToString("0.0") + ", triggerEncoder : " + cmd.TriggerEncoder.ToString("0.0"));
                            return false;
                        }
                        else if (location.RealEncoder < cmd.TriggerEncoder)
                        {
                            if (CheckGetNextReserve(cmd))
                            {
                                WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 觸發, dirFlag : " +
                                          (ControlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
                                         location.RealEncoder.ToString("0.0") + ", triggerEncoder : " + cmd.TriggerEncoder.ToString("0.0"));
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                }
            }
            else
                WaitReseveIndex = cmd.ReserveNumber;

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
                                       command.CommandList[command.IndexOfCmdList].WheelAngle);
                        break;
                    case EnumCommandType.ReviseOpen:
                        if (ControlData.OntimeReviseFlag == false)
                        {
                            agvRevise.SettingReviseData(ControlData.VelocityCommand, ControlData.DirFlag);
                            agvRevise.ResetOneTimeReivseParameter();
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
                    case EnumCommandType.Stop:
                        SlowStopControl(command.CommandList[command.IndexOfCmdList].EndPosition, command.CommandList[command.IndexOfCmdList].NextReserveNumber);
                        break;
                    case EnumCommandType.End:
                        SecondCorrectionControl(command.CommandList[command.IndexOfCmdList].EndEncoder);
                        break;
                    default:
                        break;
                }

                if (!ControlData.FlowStopRequeset && MoveState != EnumMoveState.Error)
                {
                    command.IndexOfCmdList++;
                    ExecuteCommandList();
                }
            }
        }

        private void MoveControlThread()
        {
            double[] reviseWheelAngle = new double[4];
            string ontimeReviseEMSMessage;

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

                    if (ControlData.OntimeReviseFlag && !ControlData.MoveControlStop &&
                             !ControlData.SensorStop && MoveState == EnumMoveState.Moving)
                    {
                        ontimeReviseEMSMessage = "";

                        if (agvRevise.OntimeReviseByAGVPositionAndSection(ref reviseWheelAngle, ControlData.WheelAngle,
                            location.XFLVelocity, location.ElmoEncoder,
                            command.SectionLineList[command.IndexOflisSectionLine], ref ontimeReviseEMSMessage))
                        {
                            if (ontimeReviseEMSMessage != "")
                            {
                                EMSControl(ontimeReviseEMSMessage);
                                AGVStopResult = ontimeReviseEMSMessage;
                            }
                            else
                            {
                                if (ControlData.WheelAngle != 0)
                                {
                                    reviseWheelAngle[0] = ControlData.WheelAngle + (reviseWheelAngle[0] - ControlData.WheelAngle) * 1.5;
                                    reviseWheelAngle[1] = ControlData.WheelAngle + (reviseWheelAngle[1] - ControlData.WheelAngle) * 1.5;
                                    reviseWheelAngle[2] = ControlData.WheelAngle + (reviseWheelAngle[2] - ControlData.WheelAngle) * 1.5;
                                    reviseWheelAngle[3] = ControlData.WheelAngle + (reviseWheelAngle[3] - ControlData.WheelAngle) * 1.5;
                                }

                                elmoDriver.ElmoMove(EnumAxis.GT, reviseWheelAngle[0], reviseWheelAngle[1], reviseWheelAngle[2], reviseWheelAngle[3],
                                                  ontimeReviseConfig.ThetaSpeed, EnumMoveType.Absolute, moveControlConfig.Turn.Acceleration,
                                                  moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);
                            }
                        }
                    }

                    if (ControlData.FlowStopRequeset && elmoDriver.MoveCompelete(EnumAxis.GX))
                    {
                        WriteLog("MoveControl", "7", device, "", "AGV已經停止 State 切換成 Idle!");
                        ControlData.FlowStop = true;
                        ControlData.FlowStopRequeset = false;
                    }

                    if (ControlData.FlowStop && ControlData.FlowClear)
                    {
                        WriteLog("MoveControl", "7", device, "", "AGV已經停止 State 切換成 Idle!");
                        MoveState = EnumMoveState.Idle;
                        ControlData.FlowClear = false;
                        ControlData.FlowStop = false;
                    }

                    Thread.Sleep(moveControlConfig.SleepTime);
                }
            }
            catch
            {
                EMSControl("MoveControl主Thread Expction!");
            }
        }

        #region 障礙物檢知
        public bool IsCharging()
        {
            if (SimulationMode)
                return false;

            if (moveControlConfig.SensorByPass[EnumSensorSafetyType.Charging].Enable)
                return Vehicle.Instance.ThePlcVehicle.Batterys.Charging;
            else
                return false;
        }

        public bool ForkNotHome()
        {
            if (SimulationMode)
                return false;

            if (moveControlConfig.SensorByPass[EnumSensorSafetyType.ForkHome].Enable)
                return !Vehicle.Instance.ThePlcVehicle.Robot.ForkHome || Vehicle.Instance.ThePlcVehicle.Robot.ForkBusy;
            else
                return false;
        }

        private bool GetBumperState()
        {
            if (SimulationMode)
                return false;

            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.Bumper].Enable)
                return false;

            return Vehicle.Instance.ThePlcVehicle.BumperAlarmStatus;
        }

        public EnumVehicleSafetyAction test = EnumVehicleSafetyAction.Normal;

        private EnumVehicleSafetyAction GetBeamSensorState()
        {
            if (SimulationMode)
                return EnumVehicleSafetyAction.Normal;

            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensor].Enable)
                return EnumVehicleSafetyAction.Normal;

            if (safetyData.TurningByPass)
                return EnumVehicleSafetyAction.Normal;

            //return test;
            return Vehicle.Instance.ThePlcVehicle.VehicleSafetyAction;
        }

        private bool CheckNextCommandTrigger()
        {
            if (ControlData.DirFlag)
            {
                return location.RealEncoder > command.CommandList[command.IndexOfCmdList].TriggerEncoder;
            }
            else
            {
                return location.RealEncoder < command.CommandList[command.IndexOfCmdList].TriggerEncoder;
            }
        }

        private double GetVChangeVelocity(double velocity)
        {
            double nextVChangeDistance = -1;

            for (int i = command.IndexOfCmdList; i < command.CommandList.Count; i++)
            {
                if (command.CommandList[i].CmdType == EnumCommandType.Vchange)
                {
                    if (command.CommandList[i].Position != null)
                        nextVChangeDistance = Math.Abs(location.RealEncoder - command.CommandList[i].TriggerEncoder);

                    break;
                }
                else if (command.CommandList[i].CmdType == EnumCommandType.SlowStop && !command.CommandList[i].NextRserveCancel)
                    break;
            }

            if (nextVChangeDistance == -1)
                return velocity;

            double returnVelocity = createMoveControlList.GetFirstVChangeCommandVelocity(moveControlConfig.Move.Velocity, 0,
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

            return returnVelocity;
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

            if (command.CommandList[command.IndexOfCmdList].CmdType == EnumCommandType.Move)
            {
                if (!CheckNextCommandTrigger())
                { // 狀況1.
                    double targetEncoder = command.CommandList[command.IndexOfCmdList].TriggerEncoder +
                        (command.CommandList[command.IndexOfCmdList].DirFlag ? command.CommandList[command.IndexOfCmdList].SafetyDistance / 2 :
                                                              -command.CommandList[command.IndexOfCmdList].SafetyDistance / 2);

                    elmoDriver.ElmoMove(EnumAxis.GX, targetEncoder - location.RealEncoder, moveControlConfig.EQ.Velocity, EnumMoveType.Relative,
                         moveControlConfig.EQ.Acceleration, moveControlConfig.EQ.Deceleration, moveControlConfig.EQ.Jerk);

                    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                    timer.Reset();
                    timer.Start();

                    Thread.Sleep(moveControlConfig.SleepTime * 2);
                    while (!elmoDriver.MoveCompelete(EnumAxis.GX))
                    {
                        UpdatePosition();
                        Thread.Sleep(moveControlConfig.SleepTime);

                        if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue * 3)
                        {
                            EMSControl("SecondCorrectionControl Timeout!");
                            return;
                        }
                    }
                }

                if (nowAction == EnumVehicleSafetyAction.LowSpeed && command.IndexOfCmdList + 1 < command.CommandList.Count)
                {
                    if (command.CommandList[command.IndexOfCmdList + 1].CmdType != EnumCommandType.Vchange ||
                        command.CommandList[command.IndexOfCmdList + 1].Velocity > moveControlConfig.LowVelocity)
                    {
                        double vel = GetVChangeVelocity(moveControlConfig.LowVelocity);
                        Command temp = createMoveControlList.NewVChangeCommand(null, 0, vel, ControlData.DirFlag, EnumVChangeType.SensorSlow);
                        command.CommandList.Insert(command.IndexOfCmdList + 1, temp);
                    }
                }
            }
            else
            {
                // 狀況3.
                double distance = ControlData.TrigetEndEncoder - location.RealEncoder;

                Command temp = createMoveControlList.NewMoveCommand(null, 0, Math.Abs(distance),
                    moveControlConfig.Move.Velocity, ControlData.DirFlag, 0, EnumMoveStartType.SensorStopMove);
                command.CommandList.Insert(command.IndexOfCmdList, temp);

                if (nowAction == EnumVehicleSafetyAction.Normal || ControlData.VelocityCommand < moveControlConfig.LowVelocity)
                {
                    double vel = GetVChangeVelocity(ControlData.VelocityCommand);
                    temp = createMoveControlList.NewVChangeCommand(null, 0, vel, ControlData.DirFlag);
                }
                else
                {
                    double vel = GetVChangeVelocity(moveControlConfig.LowVelocity);
                    temp = createMoveControlList.NewVChangeCommand(null, 0, vel, ControlData.DirFlag, EnumVChangeType.SensorSlow);
                }

                command.CommandList.Insert(command.IndexOfCmdList + 1, temp);
            }
        }

        private void SensorStartMoveTRAction()
        {
            double acc = GetTRStartTurnAcc();
            double distance = ControlData.TrigetEndEncoder - location.RealEncoder;
            double vel = moveControlConfig.TurnParameter[ControlData.NowAction].Velocity;

            elmoDriver.ElmoMove(EnumAxis.GX, distance, moveControlConfig.Move.Velocity, EnumMoveType.Relative,
                     moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            VchangeControl(vel, EnumVChangeType.Normal);

            elmoDriver.ElmoMove(EnumAxis.GT, command.CommandList[command.IndexOfCmdList].WheelAngle,
                                moveControlConfig.TurnParameter[ControlData.NowAction].AxisParameter.Velocity,
                                EnumMoveType.Absolute, acc,
                                moveControlConfig.TurnParameter[ControlData.NowAction].AxisParameter.Deceleration,
                                moveControlConfig.TurnParameter[ControlData.NowAction].AxisParameter.Jerk);
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
                        WriteLog("SensorStopControl", "7", device, "", "Sensor切換至Normal,速度提升至" + vel.ToString("0") + "!");
                        Command temp = createMoveControlList.NewVChangeCommand(null, 0, vel, ControlData.DirFlag);
                        command.CommandList.Insert(command.IndexOfCmdList, temp);
                    }

                    break;
                case EnumVehicleSafetyAction.Stop:
                    // 加入啟動.
                    if (MoveState == EnumMoveState.TR)
                    {
                        SensorStartMoveTRAction();
                    }
                    else if (MoveState == EnumMoveState.R2000)
                    {
                        EMSControl("暫時By pass TR2000中啟動!");
                    }
                    else
                    {
                        SensorStartMove(EnumVehicleSafetyAction.Normal);
                    }

                    break;
                case EnumVehicleSafetyAction.Normal:
                default:
                    break;
            }

            ControlData.SensorSlow = false;
            ControlData.SensorStop = false;
        }

        private void SensorActionToSlow()
        {
            switch (ControlData.SensorState)
            {
                case EnumVehicleSafetyAction.Normal:
                    // 加入降速.
                    if (ControlData.VelocityCommand > moveControlConfig.LowVelocity)
                    {
                        WriteLog("SensorStopControl", "7", device, "", "Sensor切換至LowSpeed,降速至300!");
                        Command temp = createMoveControlList.NewVChangeCommand(null, 0, moveControlConfig.LowVelocity, ControlData.DirFlag, EnumVChangeType.SensorSlow);
                        command.CommandList.Insert(command.IndexOfCmdList, temp);
                    }
                    else
                        WriteLog("SensorStopControl", "7", device, "", "Sensor切換至LowSpeed,但目前速度小於等於300,不做降速!");

                    break;
                case EnumVehicleSafetyAction.Stop:
                    // 加入啟動且降速.
                    if (MoveState == EnumMoveState.TR)
                    {
                        SensorStartMoveTRAction();
                    }
                    else if (MoveState == EnumMoveState.R2000)
                    {
                        EMSControl("暫時By pass TR2000中啟動!");
                    }
                    else
                    {
                        SensorStartMove(EnumVehicleSafetyAction.LowSpeed);
                    }
                    break;
                case EnumVehicleSafetyAction.LowSpeed:
                default:
                    break;
            }

            ControlData.SensorSlow = true;
            ControlData.SensorStop = false;
        }

        private void SensorActionToStop()
        {
            switch (ControlData.SensorState)
            {
                case EnumVehicleSafetyAction.Normal:
                case EnumVehicleSafetyAction.LowSpeed:
                    // 加入停止.
                    SensorStopControl();

                    break;
                case EnumVehicleSafetyAction.Stop:
                default:
                    break;
            }

            ControlData.SensorSlow = false;
            ControlData.SensorStop = true;
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
            if (SimulationMode)
                return true;

            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.CheckAxisState].Enable)
                return true;
            else
                return elmoDriver.CheckAxisNoError();
        }

        private void SensorSafety()
        {
            if (MoveState == EnumMoveState.Idle || MoveState == EnumMoveState.Error)
                return;

            //if (IsCharging())
            //{
            //    SendAlarmCode(141000);
            //    EMSControl("走行中出現Charging訊號!");
            //}
            //else
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

            if (ControlData.FlowStopRequeset)
            {
                ControlData.MoveControlStop = true;
            }

            if (ControlData.MoveControlStop)
            {
                StopControl();
            }

            EnumVehicleSafetyAction beamsensorState = GetBeamSensorState();

            bool bumperState = GetBumperState();

            if (bumperState)
                beamsensorState = EnumVehicleSafetyAction.Stop;

            SensorAction(beamsensorState);

            ControlData.SensorState = beamsensorState;
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
            switch (turnDir)
            {
                case EnumBeamSensorLocate.Front:
                case EnumBeamSensorLocate.Back:
                    WriteLog("MoveControl", "7", device, "", "DirLightTurn有問題,轉彎不應該有Front或Back的方向!");
                    break;

                case EnumBeamSensorLocate.Left:
                    Vehicle.Instance.ThePlcVehicle.SpinTurnLeft = true;
                    break;

                case EnumBeamSensorLocate.Right:
                    Vehicle.Instance.ThePlcVehicle.SpinTurnRight = true;
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
            Vehicle.Instance.ThePlcVehicle.SpinTurnLeft = false;
            Vehicle.Instance.ThePlcVehicle.SpinTurnRight = false;

            WriteLog("MoveControl", "7", device, "", "方向燈切換 : 只剩 " + locate.ToString() + " On !");
        }

        private void DirLightCloseAll()
        {
            Vehicle.Instance.ThePlcVehicle.Forward = false;
            Vehicle.Instance.ThePlcVehicle.Backward = false;
            Vehicle.Instance.ThePlcVehicle.TraverseLeft = false;
            Vehicle.Instance.ThePlcVehicle.TraverseRight = false;
            Vehicle.Instance.ThePlcVehicle.SpinTurnLeft = false;
            Vehicle.Instance.ThePlcVehicle.SpinTurnRight = false;

            WriteLog("MoveControl", "7", device, "", "方向燈切換 : 全部關掉!");
        }
        #endregion

        #region 外部連結 : 產生List、DebugForm相關、狀態、移動完成.
        /// <summary>
        ///  when move finished, call this function to notice other class instance that move is finished with status
        /// </summary>
        public void MoveFinished(EnumMoveComplete status)
        {
            if (ControlData.CommandMoving)
            {
                ControlData.CommandMoving = false;

                WriteLog("MoveControl", "7", device, "", "status : " + status.ToString());
                if (isAGVMCommand)
                    OnMoveFinished?.Invoke(this, status);
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "error : no Command send MoveFinished, status : " + status.ToString());
            }
        }

        private void ResetEncoder(MapPosition start, MapPosition end, bool dirFlag)
        {
            WriteLog("MoveControl", "7", device, "", "start");
            MapPosition nowPosition;

            double elmoEncoder = elmoDriver.ElmoGetPosition(EnumAxis.XFL);// 更新elmo encoder(走行距離).

            if (SimulationMode)
            {
                location.Real.Position = start;
                elmoEncoder = location.ElmoEncoder;
            }

            if (location.Real != null)
                nowPosition = location.Real.Position;
            else
            {
                EMSControl("location.Real == null EMS");
                return;
            }

            if (start.X == end.X)
            {
                if (dirFlag)
                    location.Offset = -elmoEncoder + nowPosition.Y - start.Y;
                else
                    location.Offset = -elmoEncoder + nowPosition.Y - start.Y;
            }
            else if (start.Y == end.Y)
            {
                if (dirFlag)
                    location.Offset = -elmoEncoder + nowPosition.X - start.X;
                else
                    location.Offset = -elmoEncoder + nowPosition.X - start.X;
            }
            else
            {
                WriteLog("MoveControl", "4", device, "", "不該有R2000啟動動作.");
                ControlData.MoveControlStop = true;
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
            ControlData.SensorStop = false;
            ControlData.SensorSlow = false;
            ControlData.OntimeReviseFlag = false;
            ControlData.FlowStopRequeset = false;
            ControlData.FlowStop = false;
            ControlData.FlowClear = false;
            safetyData = new MoveControlSafetyData();
            AGVStopResult = "";
            WaitReseveIndex = -1;
            safetyData.TurningByPass = false;
            ControlData.CommandMoving = true;
            ControlData.CanPause = true;

            ResetEncoder(command.SectionLineList[0].Start, command.SectionLineList[0].End, command.SectionLineList[0].DirFlag);
            Task.Factory.StartNew(() =>
            {
                elmoDriver.EnableMoveAxis();
            });
        }

        public bool TransferMove(MoveCmdInfo moveCmd)
        {
            WriteLog("MoveControl", "7", device, "", "start");

            string errorMessage = "";

            if ((MoveState != EnumMoveState.Error && MoveState != EnumMoveState.Idle))
            {
                WriteLog("MoveControl", "7", device, "", "移動中,因此無視~!");
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
            else if (!SimulationMode && location.Real != null)
            {
                double distance = Math.Sqrt(Math.Pow(location.Real.Position.X - moveCmd.AddressPositions[0].X, 2) +
                                            Math.Pow(location.Real.Position.Y - moveCmd.AddressPositions[0].Y, 2));
                if (distance > 50)
                {
                    errorMessage = "起點和目前位置差距過大.";
                    return false;
                }
            }

            MoveCommandData tempCommand = createMoveControlList.CreateMoveControlListSectionListReserveList(
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
            ControlData.FlowStopRequeset = true;
            ControlData.FlowClear = true;
        }

        public bool IsLocationRealNotNull()
        {
            errorTime = "異常停止時間 : ";
            autoTime = "啟動時間 : " + DateTime.Now.ToString("HH:mm:ss");
            return location.Real != null;
        }

        public void VehclePause()
        {
            WriteLog("MoveControl", "7", device, "", "Pause Request!");
            if (!ControlData.PauseRequest && !ControlData.PauseAlready)
                ControlData.PauseRequest = true;
        }

        public void VehcleContinue()
        {
            WriteLog("MoveControl", "7", device, "", "Pause Request!");
            if (!ControlData.ContinueRequest && ControlData.PauseAlready)
                ControlData.ContinueRequest = true;
        }

        public void VehcleCancel()
        {
            WriteLog("MoveControl", "7", device, "", "Cancel Request!");
            if (!ControlData.CancelRequest)
                ControlData.CancelRequest = true;
        }

        public MoveCommandData CreateMoveControlListSectionListReserveList(MoveCmdInfo moveCmd, ref string errorMessage)
        {
            if (Vehicle.Instance.AutoState == EnumAutoState.Auto)
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

            return createMoveControlList.CreateMoveControlListSectionListReserveList(
                        moveCmd, location.Real, ControlData.WheelAngle, ref errorMessage);
        }

        public void GetMoveCommandListInfo(List<Command> cmdList, ref List<string> logMessage)
        {
            logMessage = new List<string>();

            if (cmdList == null)
                createMoveControlList.GetMoveCommandListInfo(command.CommandList, ref logMessage);
            else
                createMoveControlList.GetMoveCommandListInfo(cmdList, ref logMessage);
        }

        public void GetReserveListInfo(List<ReserveData> reserveList, ref List<string> logMessage)
        {
            logMessage = new List<string>();

            if (reserveList == null)
                createMoveControlList.GetReserveListInfo(command.ReserveList, ref logMessage);
            else
                createMoveControlList.GetReserveListInfo(reserveList, ref logMessage);
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

        public void StatusChange()
        {
            WriteLog("MoveControl", "7", device, "", "外部控制強制清除命令!");
            ControlData.WheelAngle = 0;
            MoveState = EnumMoveState.Idle;
        }

        public bool AddReservedMapPosition(MapPosition mapPosition)
        {
            if (MoveState == EnumMoveState.Idle)
            {
                WriteLog("MoveControl", "7", device, "", "Idle情況不該收到Reserve.. 座標" +
                         "( " + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString() + " ) !");
                return false;
            }

            if (command.IndexOfReserveList > command.ReserveList.Count)
            {
                WriteLog("MoveControl", "7", device, "", "Reserve已經全部取得,但收到Reserve.. 座標" +
                         "( " + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString() + " ) !");
                return false;
            }

            if (command.ReserveList[command.IndexOfReserveList].Position.X == mapPosition.X &&
                command.ReserveList[command.IndexOfReserveList].Position.Y == mapPosition.Y)
            {
                command.ReserveList[command.IndexOfReserveList].GetReserve = true;
                command.IndexOfReserveList++;
                WriteLog("MoveControl", "7", device, "", "取得Reserve node : index = " + command.IndexOfReserveList.ToString() +
                         "( " + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString() + " ) !");
                return true;
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "Reserve node 跳號或無此Reserve,座標 : ( "
                    + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString() + " ), 跳過 index = " + command.IndexOfReserveList.ToString() +
                         "( " + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString() + " ) !");
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
                    {
                        AddReservedMapPosition(command.ReserveList[i].Position);
                    }
                }
            }
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
                                AddCSV(ref csvLog, logAGVPosition.AGVAngle.ToString("0"));
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
