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
        private SLAMControl slamControl = new SLAMControl();
        public EnumMoveState MoveState { get; private set; } = EnumMoveState.Idle;
        public MoveControlConfig moveControlConfig;
        private MapInfo theMapInfo = new MapInfo();
        private Logger logger = LoggerAgent.Instance.GetLooger("MoveControlCSV");
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private string device = "MoveControl";
        private Dictionary<EnumAddressAction, TRTimeToAngleRange> trTimeToAngleRange = new Dictionary<EnumAddressAction, TRTimeToAngleRange>();
        public event EventHandler<EnumMoveComplete> OnMoveFinished;
        public event EventHandler<EnumMoveComplete> OnRetryMoveFinished;
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
        public string AGVState { get; set; } = EnumMoveState.Idle.ToString();

        public bool LeftRead { get; set; } = false;
        public bool RightRead { get; set; } = false;

        private List<Sr2000Config> sr2000ConfigList = new List<Sr2000Config>();

        private void SetDebugFlowLog(string functionName, string message)
        {
            DebugFlowLog = String.Concat(DateTime.Now.ToString("HH:mm:ss.fff"), "\t", functionName, "\t", message, "\r\n", DebugFlowLog);

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
            double velocity = moveControlConfig.TurnParameter[ControlData.TurnType].Velocity;
            double time = nowEncoder / velocity;
            TRTimeToAngleRange trTimeToAngle = trTimeToAngleRange[ControlData.TurnType];

            double jerk = moveControlConfig.TurnParameter[ControlData.TurnType].AxisParameter.Jerk;
            double acc = moveControlConfig.TurnParameter[ControlData.TurnType].AxisParameter.Acceleration;
            double dec = moveControlConfig.TurnParameter[ControlData.TurnType].AxisParameter.Deceleration;

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
                if (nowEncoder > trTimeToAngle.TimeRange[6] * moveControlConfig.TurnParameter[ControlData.TurnType].Velocity + moveControlConfig.SafteyDistance[EnumCommandType.Move] / 2)
                    angle = -90;
                else
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
            ReadMoveControlConfigXML(@"D:\AgvConfigs\MoveControlConfig.xml");
            slamControl.InitailSLAM("");
            WriteSafetyAndSensorByPassLog();
            SetTRTimeToAngleRange();
            InitailSr2000(moveControlConfig.Sr2000ConfigPath);
            elmoDriver = new ElmoDriver(moveControlConfig.ElmoConfigPath, this.alarmHandler);

            ReadOntimeReviseConfigXML(moveControlConfig.OnTimeReviseConfigPath);
            CreateMoveCommandList = new CreateMoveControlList(DriverSr2000List, moveControlConfig, sr2000ConfigList, this.alarmHandler, theMapInfo.allMapBarcodeLines);

            agvRevise = new AgvMoveRevise(ontimeReviseConfig, elmoDriver, DriverSr2000List);


            for ( int i = 0; i < 5 && !(elmoDriver.Connected && DriverSr2000List[0].sr2000Info.Connect && DriverSr2000List[1].sr2000Info.Connect) ; i++)
            {
                Thread.Sleep(500);

                if (!elmoDriver.Connected)
                    elmoDriver = new ElmoDriver(moveControlConfig.ElmoConfigPath, this.alarmHandler);

                if (!DriverSr2000List[0].sr2000Info.Connect)
                    DriverSr2000List[0].RetryConnect();

                if (!DriverSr2000List[1].sr2000Info.Connect)
                    DriverSr2000List[1].RetryConnect();
            }

            loopTimeTimer.Reset();
            loopTimeTimer.Start();

            ControlData.MoveControlThread = new Thread(MoveControlThread);
            ControlData.MoveControlThread.Start();

            threadSCVLog = new Thread(WriteLogCSV);
            threadSCVLog.Start();
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
                    SendAlarmCode(155002);
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            if (!elmoDriver.AllAxisStop())
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
            string classMethodName = String.Concat(GetType().Name, ":", memberName);
            LogFormat logFormat = new LogFormat(category, logLevel, classMethodName, device, carrierId, message);

            loggerAgent.LogMsg(logFormat.Category, logFormat);

            if (category == "MoveControl")
                SetDebugFlowLog(memberName, message);
        }

        private void WriteSafetyAndSensorByPassLog()
        {
            string logMessage = "Safety : \r\n";

            foreach (EnumMoveControlSafetyType item in (EnumMoveControlSafetyType[])Enum.GetValues(typeof(EnumMoveControlSafetyType)))
            {
                logMessage = logMessage + item.ToString() + " : " + (moveControlConfig.Safety[item].Enable ? "Enable" : "Disable") +
                             ", Range : " + moveControlConfig.Safety[item].Range.ToString("0.0") + "\r\n";
            }

            logMessage = logMessage + "\r\nSensorSafety : \r\n";

            foreach (EnumSensorSafetyType item in (EnumSensorSafetyType[])Enum.GetValues(typeof(EnumSensorSafetyType)))
            {
                logMessage = logMessage + item.ToString() + " : " + (moveControlConfig.SensorByPass[item].Enable ? "Enable\r\n" : "Disable\r\n");
            }

            WriteLog("MoveControl", "7", device, "", logMessage);
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
                    case "TRPathMonitoring":
                    case "IdleNotWriteLog":
                    case "BarcodePositionSafety":
                    case "StopWithoutReason":
                    case "BeamSensorR2000":
                        temp = ReadSafetyDataXML((XmlElement)item);
                        try
                        {
                            moveControlConfig.Safety[(EnumMoveControlSafetyType)Enum.Parse(typeof(EnumMoveControlSafetyType), item.Name)] = temp;
                        }
                        catch { }

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
                    case "R2000FlowStat":
                    case "Bumper":
                    case "CheckAxisState":
                    case "EndPositionOffset":
                    case "SecondCorrectionBySide":
                        temp.Enable = (item.InnerText == "Enable");
                        try
                        {
                            moveControlConfig.SensorByPass[(EnumSensorSafetyType)Enum.Parse(typeof(EnumSensorSafetyType), item.Name)] = temp;
                        }
                        catch { }

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
                    case "OverrideTimeoutValue":
                        moveControlConfig.OverrideTimeoutValue = double.Parse(item.InnerText);
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

            //if (moveControlConfig.Move.Velocity > 1000)
            //{
            //    moveControlConfig.Move.Velocity = 1000;
            //    WriteLog("MoveControl", "7", device, "", "推測速度超過1000 elmo drive會出問題, 強制降回1000!");
            //}
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
                sr2000ConfigList.Add(sr2000Config);
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
                Vehicle.Instance.VehicleLocation.AgvcPosition = location.Encoder.Position;
            }
        }

        private void UpdateDelta(bool secondCorrection)
        {
            if (safetyData.NowMoveState == EnumMoveState.Error)
                return;

            double realEncoder;

            if (secondCorrection && moveControlConfig.SensorByPass[EnumSensorSafetyType.SecondCorrectionBySide].Enable)
                realEncoder = MapPositionToEncoder(command.SectionLineList[command.IndexOflisSectionLine], location.agvPosition.BarcodeCenter);
            else
                realEncoder = MapPositionToEncoder(command.SectionLineList[command.IndexOflisSectionLine], location.agvPosition.Position);

            // 此Barcode是多久之前的資料,基本上為正值(s).
            double deltaTime = ((double)location.ScanTime + (DateTime.Now - location.BarcodeGetDataTime).TotalMilliseconds) / 1000;
            // 真實Barcode回推的RealEncoder需要再加上這個時間*速度(Elmo速度本身就帶正負號).
            realEncoder = realEncoder + location.Velocity * deltaTime;
            // RealEncoder = elmoEncoder + offset + delta.
            location.Delta = realEncoder - (location.ElmoEncoder + location.Offset);
        }

        private void UpdateThetaSectionDeviationAndSafetyCheck()
        {
            if (location.agvPosition == null)
            {
                location.ThetaAndSectionDeviation = null;
                return;
            }

            double sectionDeviation = 0;
            double theta =
                location.agvPosition.AGVAngle + (command.SectionLineList[command.IndexOflisSectionLine].DirFlag ? 0 : 180) +
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
                    sectionDeviation = location.agvPosition.Position.Y - command.SectionLineList[command.IndexOflisSectionLine].Start.Y;
                    ControlData.SectionDeviationOffset = command.EndOffsetY;
                    break;
                case 180:
                    sectionDeviation = -(location.agvPosition.Position.Y - command.SectionLineList[command.IndexOflisSectionLine].Start.Y);
                    ControlData.SectionDeviationOffset = -command.EndOffsetY;
                    break;
                case 90:
                    sectionDeviation = location.agvPosition.Position.X - command.SectionLineList[command.IndexOflisSectionLine].Start.X;
                    ControlData.SectionDeviationOffset = command.EndOffsetX;
                    break;
                case -90:
                    sectionDeviation = -(location.agvPosition.Position.X - command.SectionLineList[command.IndexOflisSectionLine].Start.X);
                    ControlData.SectionDeviationOffset = -command.EndOffsetX;
                    break;
                default:
                    break;
            }

            location.ThetaAndSectionDeviation = new ThetaSectionDeviation(theta, sectionDeviation, location.agvPosition.Count);

            if (moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseTheta].Enable)
            {
                if (Math.Abs(location.ThetaAndSectionDeviation.Theta) >
                    moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range)
                {
                    WriteLog("MoveControl", "7", device, "", String.Concat("nowAGV車資 : ", location.agvPosition.AGVAngle, ", section index : ", command.IndexOflisSectionLine,
                                                                           ", dirFlag : ", command.SectionLineList[command.IndexOflisSectionLine].DirFlag.ToString(),
                                                                           ", SectionAngle : ", command.SectionLineList[command.IndexOflisSectionLine].SectionAngle));
                    EMSControl(String.Concat("nowAGV車資 : ", location.agvPosition.AGVAngle, ", 角度偏差", location.ThetaAndSectionDeviation.Theta.ToString("0.0"),
                        "度,已超過安全設置的", moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range.ToString("0.0"),
                        "度,因此啟動EMS!"));
                    SendAlarmCode(154000);
                    return;
                }
            }

            if (moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Enable)
            {
                if (ControlData.WheelAngle != 0 && moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal].Enable)
                {
                    if (Math.Abs(location.ThetaAndSectionDeviation.SectionDeviation) > moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal].Range)
                    {
                        EMSControl(String.Concat("nowBarcodePosition : ( ", location.agvPosition.Position.X.ToString(), ", ", location.agvPosition.Position.Y.ToString(),
                                                 " ), ", "橫移偏差", location.ThetaAndSectionDeviation.SectionDeviation.ToString("0"), "mm,已超過安全設置的",
                                                 moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal].Range.ToString("0"),
                                                 "mm,因此啟動EMS!"));
                        SendAlarmCode(154001);
                    }
                }
                else
                {
                    if (Math.Abs(location.ThetaAndSectionDeviation.SectionDeviation) > moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range)
                    {
                        EMSControl(String.Concat("nowBarcodePosition : ( ", location.agvPosition.Position.X.ToString(), ", ", location.agvPosition.Position.Y.ToString(),
                                                 " ), ", "橫移偏差", location.ThetaAndSectionDeviation.SectionDeviation.ToString("0"), "mm,已超過安全設置的",
                                                 moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range.ToString("0"),
                                                 "mm,因此啟動EMS!"));
                        SendAlarmCode(154001);
                    }
                }
            }
        }

        private bool UpdateSR2000(bool moving)
        {
            if (!SimulationMode)
            {
                AGVPosition temp = null;
                AGVPosition agvPosition = null;
                int index = 0;

                for (int i = 0; i < DriverSr2000List.Count; i++)
                {
                    temp = DriverSr2000List[i].GetAGVPosition();

                    if (i == 0)
                        location.BarcodeLeft = temp;
                    else if (i == 1)
                        location.BarcodeRight = temp;

                    if (temp != null && (computeFunction.IsSameAngle(temp.BarcodeAngleInMap, temp.AGVAngle, ControlData.WheelAngle) || !moving))
                    {
                        if (agvPosition == null || (agvPosition.Type != EnumBarcodeMaterial.Iron && temp.Type == EnumBarcodeMaterial.Iron))
                        {
                            agvPosition = temp;
                            index = i;
                        }
                    }
                }

                if (agvPosition != null && !(location.LastBarcodeCount == agvPosition.Count && location.IndexOfSr2000List == index))
                {   // 有資料且和上次的不是同一筆.
                    location.LastBarcodeCount = agvPosition.Count;
                    location.IndexOfSr2000List = index;
                    location.agvPosition = agvPosition;
                    location.ScanTime = agvPosition.ScanTime;
                    location.BarcodeGetDataTime = agvPosition.GetDataTime;

                    if (location.agvPosition.Type == EnumBarcodeMaterial.Iron)
                    {
                        if (location.PositingBarcodeID != location.LastPositingBarcodeID)
                            location.LastPositingBarcodeID = location.PositingBarcodeID;

                        location.PositingBarcodeID = location.agvPosition.BarcodeLineID;
                    }

                    Vehicle.Instance.VehicleLocation.BarcodePosition = location.agvPosition.Position;
                    return true;
                }
            }
            else
            {
                if (location.agvPosition == null)
                {
                    MapPosition tempPosition = new MapPosition(0, 0);
                    location.agvPosition = new AGVPosition(tempPosition, tempPosition, 0, 0, 20, DateTime.Now, 0, 0, EnumBarcodeMaterial.Iron, "Simulate");
                    //location.Barcode = new AGVPosition(tempPosition, 90, 0, 20, DateTime.Now, 0, 0, EnumBarcodeMaterial.Iron);
                    return true;
                }
            }

            return false;
        }

        private void UpdateWheelAngle()
        {
            if (elmoDriver.Connected)
            {
                if (elmoDriver.WheelAngleCompare(90, 10))
                    ControlData.WheelAngle = 90;
                else if (elmoDriver.WheelAngleCompare(-90, 10))
                    ControlData.WheelAngle = -90;
                else
                    ControlData.WheelAngle = 0;
            }
        }

        private void UpdateElmo()
        {
            if (!SimulationMode)
            {
                ElmoAxisFeedbackData elmoData = elmoDriver.ElmoGetFeedbackData(EnumAxis.XFL);

                location.GTPosition = elmoDriver.ElmoGetPosition(EnumAxis.GT);
                location.GTMoveCompelete = elmoDriver.TurnAxisStop();
                location.GXMoveCompelete = elmoDriver.MoveAxisStop();

                if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error &&
                    location.GXMoveCompelete && ControlData.SensorState != EnumVehicleSafetyAction.Stop)
                {
                    if (!ControlData.StopWithoutReason)
                    {
                        ControlData.StopWithoutReason = true;
                        ControlData.StopWithoutReasonTimer.Restart();
                    }
                }
                else
                    ControlData.StopWithoutReason = false;

                if (elmoData != null)
                {
                    location.Velocity = elmoDriver.ElmoGetVelocity(EnumAxis.GX);
                    Vehicle.Instance.VehicleLocation.Speed = Math.Abs(location.Velocity);
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
                    Vehicle.Instance.VehicleLocation.Speed = 0;
                }
                else
                {
                    location.Velocity = ControlData.DirFlag ? ControlData.RealVelocity : -ControlData.RealVelocity;
                    location.ElmoEncoder = location.ElmoEncoder + (double)moveControlConfig.SleepTime / 1000 * location.Velocity;
                    Vehicle.Instance.VehicleLocation.Speed = ControlData.RealVelocity;
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
                        EMSControl("出彎" + Math.Abs(location.ElmoEncoder - safetyData.TurnOutElmoEncoder).ToString("0") +
                            "mm未讀取到Barcode,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.TurnOut].Range.ToString("0") +
                            "mm,因此啟動EMS!");
                        SendAlarmCode(130000);
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
                        EMSControl("直線超過" + Math.Abs(location.ElmoEncoder - safetyData.LastReadBarcodeElmoEncoder).ToString("0") +
                            "mm未讀取到Barcode,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.LineBarcodeInterval].Range.ToString("0") +
                            "mm,因此啟動EMS!");
                        SendAlarmCode(130001);
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
            bool newBarcode;

            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
            {
                newBarcode = UpdateSR2000(true);

                if (MoveState != EnumMoveState.TR && MoveState != EnumMoveState.R2000)
                {
                    if (newBarcode)
                        UpdateThetaSectionDeviationAndSafetyCheck();

                    if (location.ThetaAndSectionDeviation != null && ControlData.EQVChange)
                    {
                        location.ThetaAndSectionDeviation.SectionDeviation += ControlData.SectionDeviationOffset;
                        location.ThetaAndSectionDeviation.Theta += command.EndOffsetTheta;
                    }

                    if (newBarcode && location.agvPosition.Type == EnumBarcodeMaterial.Iron)
                        UpdateDelta(secondCorrection);
                }

                UpdateReal();
                SafetyTurnOutAndLineBarcodeInterval(newBarcode);
            }
            else
            {
                newBarcode = UpdateSR2000(false);

                if (Vehicle.Instance.AutoState != EnumAutoState.Auto && newBarcode && location.agvPosition.Type == EnumBarcodeMaterial.Iron)
                {
                    location.Real = location.agvPosition;
                    location.Real.AGVAngle = computeFunction.GetAGVAngle(location.Real.AGVAngle);
                    Vehicle.Instance.VehicleLocation.RealPosition = location.Real.Position;
                    Vehicle.Instance.VehicleLocation.VehicleAngle = location.Real.AGVAngle;
                    Vehicle.Instance.VehicleLocation.AgvcPosition = location.Real.Position;
                }
            }
        }
        #endregion

        #region
        private bool UpdateAGVPosition()
        {
            if (UpdateSR2000(MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error))
                return true;

            AGVPosition temp = null;

            if (slamControl.GetAGVPosition(ref temp))
            {
                location.agvPosition = temp;
                return true;
            }
            else
                return false;
        }

        private void UpdateLocation(bool secondCorrection = false)
        {
            LoopTime = loopTimeTimer.ElapsedMilliseconds;
            loopTimeTimer.Restart();

            UpdateElmo();

            bool newAGVPosition = UpdateAGVPosition();

            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
            {
                if (MoveState != EnumMoveState.TR && MoveState != EnumMoveState.R2000)
                {
                    if (newAGVPosition)
                        UpdateThetaSectionDeviationAndSafetyCheck();

                    if (location.ThetaAndSectionDeviation != null && ControlData.EQVChange)
                    {
                        location.ThetaAndSectionDeviation.SectionDeviation += ControlData.SectionDeviationOffset;
                        location.ThetaAndSectionDeviation.Theta += command.EndOffsetTheta;
                    }

                    if (newAGVPosition && location.agvPosition.Type == EnumBarcodeMaterial.Iron)
                        UpdateDelta(secondCorrection);
                }

                UpdateReal();
                SafetyTurnOutAndLineBarcodeInterval(newAGVPosition);
            }
            else
            {
                if (Vehicle.Instance.AutoState != EnumAutoState.Auto && newAGVPosition)
                {
                    location.Real = location.agvPosition;
                    location.Real.AGVAngle = computeFunction.GetAGVAngle(location.Real.AGVAngle);
                    Vehicle.Instance.VehicleLocation.RealPosition = location.Real.Position;
                    Vehicle.Instance.VehicleLocation.VehicleAngle = location.Real.AGVAngle;
                }
            }
        }
        #endregion

        #region CommandControl
        private bool IsInTRPath(EnumAddressAction type, double encoder, double startAngle, double targetAngle, double range)
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

            bool result = Math.Abs(idealAngle - nowAngle) <= range;

            if (!result)
                WriteLog("MoveControl", "7", device, "", String.Concat("四輪平均角度 : ", nowAngle.ToString("0"), ", 預計平均角度 : ", idealAngle.ToString("0")));

            return result;
        }

        private bool TurnGorupAxisNearlyAngle(double range)
        {
            double angle_TFL = elmoDriver.ElmoGetPosition(EnumAxis.TFL);
            double angle_TFR = elmoDriver.ElmoGetPosition(EnumAxis.TFR);
            double angle_TRL = elmoDriver.ElmoGetPosition(EnumAxis.TRL);
            double angle_TRR = elmoDriver.ElmoGetPosition(EnumAxis.TRR);

            bool result = (Math.Abs(angle_TFL - angle_TFR) <= range) && (Math.Abs(angle_TFL - angle_TRL) <= range) &&
                          (Math.Abs(angle_TFL - angle_TRR) <= range) && (Math.Abs(angle_TFR - angle_TRL) <= range) &&
                          (Math.Abs(angle_TFR - angle_TRR) <= range) && (Math.Abs(angle_TRL - angle_TRR) <= range);

            if (!result)
                WriteLog("MoveControl", "7", device, "", String.Concat("四輪角度 TFL : ", angle_TFL.ToString("0"), ", TFR : ", angle_TFR.ToString("0"),
                                                                                ",TRL : ", angle_TRL.ToString("0"), ",TRR : ", angle_TRR.ToString("0")));

            return result;
        }

        private void TRControl_SimulationMode(int wheelAngle, EnumAddressAction type)
        {
            double velocity = moveControlConfig.TurnParameter[type].Velocity;
            double r = moveControlConfig.TurnParameter[type].R;

            WriteLog("MoveControl", "7", device, "", "start, velocity : " + velocity.ToString("0") + ", r : " + r.ToString("0") +
                ", 舵輪將旋轉至 " + wheelAngle.ToString("0") + "度!");
            MoveState = EnumMoveState.TR;
            ControlData.TurnType = type;

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
                    SendAlarmCode(159998);
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
                    BeamSensorOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back);
                    BeamSensorOnlyAwake(ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back);
                    DirLightOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back);
                    break;
                case 90:
                    BeamSensorOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right);
                    BeamSensorOnlyAwake(ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right);
                    DirLightOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right);
                    break;
                case -90:
                    BeamSensorOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                    BeamSensorOnlyAwake(ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                    DirLightOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                    break;
                default:
                    EMSControl("switch (wheelAngle) default..EMS..");
                    SendAlarmCode(159998);
                    break;
            }

            ControlData.WheelAngle = wheelAngle;
            location.Delta = location.Delta + (ControlData.DirFlag ? (distance - (location.ElmoEncoder - ControlData.TurnStartEncoder)) :
                                               -(distance - (ControlData.TurnStartEncoder - location.ElmoEncoder)));
            UpdatePosition();
            Vehicle.Instance.VehicleLocation.MoveDirectionAngle = computeFunction.GetCurrectAngle(-(location.Real.AGVAngle + ControlData.WheelAngle + (ControlData.DirFlag ? 0 : 180)));

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
            ControlData.TurnType = type;

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
                    SendAlarmCode(159998);
                    break;
            }

            if (!elmoDriver.MoveCompelete(EnumAxis.GT))
            {
                SendAlarmCode(152002);
                WriteLog("MoveControl", "4", device, "", " TR中 GT Moving~");
            }

            if (agvVelocity > velocity + safetyVelocityRange)
            { // 超速GG, 不該發生.
                EMSControl("超速.., vel : " + agvVelocity.ToString("0"));
                SendAlarmCode(152000);
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
                EMSControl("速度過慢.., vel : " + agvVelocity.ToString("0"));
                SendAlarmCode(152003);
            }

            while (!elmoDriver.WheelGTCompare(wheelAngle, moveControlConfig.StartWheelAngleRange))
            {
                UpdatePosition();
                SensorSafety();
                if (ControlData.FlowStopRequeset || MoveState == EnumMoveState.Error)
                    return;

                if (moveControlConfig.Safety[EnumMoveControlSafetyType.TRPathMonitoring].Enable)
                {
                    if (!TurnGorupAxisNearlyAngle(moveControlConfig.Safety[EnumMoveControlSafetyType.TRPathMonitoring].Range))
                    {
                        EMSControl("四輪角度差異過大,EMS!");
                        SendAlarmCode(152003);
                        return;
                    }
                    else if (!IsInTRPath(type, Math.Abs(ControlData.TurnStartEncoder - location.ElmoEncoder),
                        ControlData.WheelAngle, wheelAngle, moveControlConfig.Safety[EnumMoveControlSafetyType.TRPathMonitoring].Range))
                    {
                        EMSControl("不再TR預計路徑上,異常停止!");
                        SendAlarmCode(152001);
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
                                BeamSensorOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back);
                                BeamSensorOnlyAwake(ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back);
                                DirLightOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back);
                                break;
                            case 90:
                                BeamSensorOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right);
                                BeamSensorOnlyAwake(ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right);
                                DirLightOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right);
                                break;
                            case -90:
                                BeamSensorOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                                BeamSensorOnlyAwake(ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                                DirLightOnlyOn(ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                                break;
                            default:
                                EMSControl("switch (wheelAngle) default..EMS..");
                                SendAlarmCode(159998);
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
            Vehicle.Instance.VehicleLocation.MoveDirectionAngle = computeFunction.GetCurrectAngle(-(location.Real.AGVAngle + ControlData.WheelAngle + (ControlData.DirFlag ? 0 : 180)));


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
            ControlData.TurnType = EnumAddressAction.R2000;
            command.IndexOflisSectionLine++;

            if (wheelAngle == 1 || wheelAngle == -1)
                outerWheelEncoder = location.ElmoEncoder;
            else
            {
                EMSControl("R2000取得奇怪的wheelAngle");
                SendAlarmCode(159998);
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
            Vehicle.Instance.VehicleLocation.MoveDirectionAngle = computeFunction.GetCurrectAngle(-(location.Real.AGVAngle + ControlData.WheelAngle + (ControlData.DirFlag ? 0 : 180)));

            DirLightOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));

            MoveState = EnumMoveState.Moving;
            safetyData.TurningByPass = false;
            ControlData.CanPause = true;
            WriteLog("MoveControl", "7", device, "", " end.");
        }

        public void R2000Control(int wheelAngle)
        {
            ControlData.CanPause = false;
            if (!moveControlConfig.Safety[EnumMoveControlSafetyType.BeamSensorR2000].Enable)
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
            ControlData.TurnType = EnumAddressAction.R2000;
            command.IndexOflisSectionLine++;

            double velocity = moveControlConfig.TurnParameter[EnumAddressAction.R2000].Velocity;
            double safetyVelocityRange = moveControlConfig.TurnParameter[EnumAddressAction.R2000].SafetyVelocityRange;
            double agvVelocity = Math.Abs(location.Velocity);

            if (Math.Abs(agvVelocity - velocity) <= safetyVelocityRange)
            { // Normal
            }
            else if (agvVelocity > velocity)
            { // 超速GG, 不該發生.
                EMSControl("超速.., vel : " + agvVelocity.ToString("0"));
                SendAlarmCode(153000);
                return;
            }
            else
            { // 太慢 處理??
                EMSControl("速度過慢.., vel : " + agvVelocity.ToString("0"));
                SendAlarmCode(153001);
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
                SendAlarmCode(159998);
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

            Thread.Sleep(500);
            while (!elmoDriver.MoveCompeleteVirtual(EnumAxisType.Turn))
            {
                UpdatePosition();
                SensorSafety();

                if (ControlData.FlowStopRequeset || MoveState == EnumMoveState.Error)
                    return;

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            if (!elmoDriver.WheelAngleCompare(leftTurn.Distance * wheelAngle, rightTurn.Distance * wheelAngle,
                                             -leftTurn.Distance * wheelAngle, -rightTurn.Distance * wheelAngle, 5))
            {
                EMSControl("虛擬軸轉向沒Link");
                SendAlarmCode(142000);
            }

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
            Vehicle.Instance.VehicleLocation.MoveDirectionAngle = computeFunction.GetCurrectAngle(-(location.Real.AGVAngle + ControlData.WheelAngle + (ControlData.DirFlag ? 0 : 180)));
            
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
                        BeamSensorSingleAwake(turnLeft ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                        break;
                    case 90:
                        DirLightTurn(ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right);
                        BeamSensorSingleAwake(ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back);
                        break;
                    case -90:
                        DirLightTurn(ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left);
                        BeamSensorSingleAwake(ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back);
                        break;
                    default:
                        EMSControl("switch (TRWheelAngle) default..EMS.");
                        SendAlarmCode(159998);
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

                if (command.EndAddressLoadUnload)
                {
                    try
                    {
                        WriteLog("MoveControl", "7", device, "", "終點站要取放貨, 通知Plc Fork Servo On!");
                        plcAgent.SendVehicleDecreaseSpeedFlag();
                    }
                    catch
                    {
                        WriteLog("MoveControl", "7", device, "", "通知Plc Fork Servo On, Excption!");
                    }
                }
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
                switch (ControlData.WheelAngle)
                {
                    case 0: // 朝前面.
                        BeamSensorSingleOn((dirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                        BeamSensorSingleAwake((dirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                        DirLightSingleOn((dirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                        break;
                    case 90: // 朝左.
                        BeamSensorSingleOn((dirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                        BeamSensorSingleAwake((dirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                        DirLightSingleOn((dirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                        break;
                    case -90: // 朝右.
                        BeamSensorSingleOn((dirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                        BeamSensorSingleAwake((dirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                        DirLightSingleOn((dirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                        break;
                    default:
                        WriteLog("MoveControl", "4", device, "", "switch (wheelAngle) default..EMS..");
                        return;
                }

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

                if (!SimulationMode)
                {
                    while (!location.GTMoveCompelete || !elmoDriver.WheelAngleCompare(wheelAngle, 5))
                    {
                        UpdatePosition();
                        SensorSafety();

                        if (timer.ElapsedMilliseconds > moveControlConfig.TurnTimeoutValue)
                        {
                            EMSControl("移動前旋轉轉向軸旋轉不到位!");
                            SendAlarmCode(150001);
                            return;
                        }

                        Thread.Sleep(moveControlConfig.SleepTime);
                    }
                }


                ControlData.WheelAngle = wheelAngle;
                ControlData.TrigetEndEncoder = location.RealEncoder + (dirFlag ? distance : -distance);
                ControlData.DirFlag = dirFlag;

                if (moveType == EnumMoveStartType.FirstMove)
                {
                    if (ControlData.MoveStartNoWaitTime)
                    {
                        WriteLog("MoveControl", "7", device, "", "為Override的First Move移動,因此取消等待2秒!");
                        ControlData.MoveStartNoWaitTime = false;
                    }
                    else
                    {
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
                }
                else if (moveType == EnumMoveStartType.ChangeDirFlagMove)
                {
                    command.IndexOflisSectionLine++;
                    command.BarcodeLineListIndex++;
                    command.IndexOfLeftBarcodeLineList = 0;
                    command.IndexOfRightBarcodeLineList = 0;
                }
            }

            if (moveType != EnumMoveStartType.SensorStopMove)
                ControlData.VelocityCommand = velocity;

            ControlData.RealVelocity = velocity;

            ControlData.SensorState = GetSensorState();

            if (ControlData.SensorState != EnumVehicleSafetyAction.Stop)
            {

                Vehicle.Instance.VehicleLocation.MoveDirectionAngle = computeFunction.GetCurrectAngle(-(location.Real.AGVAngle + ControlData.WheelAngle + (ControlData.DirFlag ? 0 : 180)));
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
                    SendAlarmCode(155000);
                    return;
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            BeamSensorCloseAll();
            BeamSensorSleepAll();
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
                        SendAlarmCode(156000);
                        return;
                    }

                    Thread.Sleep(moveControlConfig.SleepTime);
                }

                Thread.Sleep(500);
            }

            elmoDriver.DisableMoveAxis();

            bool newBarcode = UpdateSR2000(true);

            try
            {
                if (newBarcode && location.agvPosition.Type == EnumBarcodeMaterial.Iron)
                {
                    double deltaX = location.agvPosition.Position.X - command.End.X;
                    double deltaY = location.agvPosition.Position.Y - command.End.Y;
                    double theta = location.agvPosition.AGVAngle - location.Real.AGVAngle;
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
            MoveFinished(EnumMoveComplete.Success, command.IsRetryMove);
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
                    if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.TRFlowStart].Enable)
                    {
                        EMSControl("TR Flow Stop 且未開啟TR啟動功能!");
                        return;
                    }
                    else
                    {
                        elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                        elmoDriver.ElmoStop(EnumAxis.GT, moveControlConfig.TurnParameter[ControlData.TurnType].AxisParameter.Deceleration, moveControlConfig.TurnParameter[ControlData.TurnType].AxisParameter.Jerk);
                    }

                    break;
                case EnumMoveState.R2000:
                    if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.R2000FlowStat].Enable)
                    {
                        EMSControl("R2000 Flow Stop 且未開啟R2000啟動功能!");
                        SendAlarmCode(153002);
                        return;
                    }
                    else
                    {
                        elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                        elmoDriver.ElmoStop(EnumAxis.VTFL);
                        elmoDriver.ElmoStop(EnumAxis.VTFR);
                        elmoDriver.ElmoStop(EnumAxis.VTRL);
                        elmoDriver.ElmoStop(EnumAxis.VTRR);

                        elmoDriver.ElmoStop(EnumAxis.VXFL);
                        elmoDriver.ElmoStop(EnumAxis.VXFR);
                        elmoDriver.ElmoStop(EnumAxis.VXRL);
                        elmoDriver.ElmoStop(EnumAxis.VXRR);
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
            EnumMoveState nowAction = MoveState;
            MoveState = EnumMoveState.Error;

            double stopDistance = computeFunction.GetAccDecDistance(Math.Abs(location.Velocity), 0,
                                    moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk) *
                                    moveControlConfig.VChangeSafetyDistanceMagnification;
            double startStopEncoder = location.ElmoEncoder;

            elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            elmoDriver.ElmoStop(EnumAxis.GT, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);

            if (nowAction == EnumMoveState.R2000)
            {
                elmoDriver.ElmoStop(EnumAxis.VTFL);
                elmoDriver.ElmoStop(EnumAxis.VTFR);
                elmoDriver.ElmoStop(EnumAxis.VTRL);
                elmoDriver.ElmoStop(EnumAxis.VTRR);

                elmoDriver.ElmoStop(EnumAxis.VXFL);
                elmoDriver.ElmoStop(EnumAxis.VXFR);
                elmoDriver.ElmoStop(EnumAxis.VXRL);
                elmoDriver.ElmoStop(EnumAxis.VXRR);
            }

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

            MoveFinished(EnumMoveComplete.Fail, command.IsRetryMove);
            ControlData.OntimeReviseFlag = false;
            ControlData.SecondCorrection = false;
            simulationIsMoving = false;
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
                        WriteLog("MoveControl", "7", device, "", String.Concat("LastPositingBarcodeID : ", location.LastPositingBarcodeID, ", PositingBarcodeID : ", location.PositingBarcodeID));
                        EMSControl(String.Concat("Command : ", cmd.CmdType.ToString(), ", 超過Triiger觸發區間,EMS.. dirFlag : ", (ControlData.DirFlag ? "往前" : "往後"),
                                     ", Encoder : ", location.RealEncoder.ToString("0.0"), ", triggerEncoder : ", cmd.TriggerEncoder.ToString("0.0")));
                        SendAlarmCode(150002);
                        return false;
                    }
                    else if ((cmd.DirFlag && location.RealEncoder > cmd.TriggerEncoder) ||
                            (!cmd.DirFlag && location.RealEncoder < cmd.TriggerEncoder))
                    {
                        WriteLog("MoveControl", "7", device, "", String.Concat("Command : ", cmd.CmdType.ToString(), ", 觸發, dirFlag : ",
                                  (ControlData.DirFlag ? "往前" : "往後"), ", Encoder : ", location.RealEncoder.ToString("0.0"),
                                  ", triggerEncoder : ", cmd.TriggerEncoder.ToString("0.0"), ", 定位Barcode ID : ", location.PositingBarcodeID));
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
                WriteLog("MoveControl", "7", device, "", String.Concat("Barcode Position ( ", location.agvPosition.Position.X.ToString("0"), ", " + location.agvPosition.Position.Y.ToString("0"),
                                                                       " ), Real Position ( ", location.Real.Position.X.ToString("0"), ", ", location.Real.Position.Y.ToString("0"),
                                                                       " ), Encoder Position ( ", location.Encoder.Position.X.ToString("0"), ", " + location.Encoder.Position.Y.ToString("0") + " )"));

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
                    if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
                    {
                        UpdatePosition();
                        ExecuteCommandList();
                        SensorSafety();
                        FinalBarcodePositionSafety();
                    }
                    else
                        UpdatePosition();

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
                        if (ControlData.PauseRequest)
                        {
                            ControlData.PauseAlready = true;
                            ControlData.PauseRequest = false;

                            MoveFinished(EnumMoveComplete.Pause, command.IsRetryMove);
                        }
                    }

                    if (ControlData.PauseAlready && ControlData.CancelRequest)
                    {
                        WriteLog("MoveControl", "7", device, "", "AGV已經停止 Cancel Start!");
                        waitDelateTime.Reset();
                        waitDelateTime.Start();
                        if (!ControlData.CancelNotSendEvent)
                            elmoDriver.DisableMoveAxis();

                        while (waitDelateTime.ElapsedMilliseconds < moveControlConfig.PauseDelateTime)
                        {
                            UpdatePosition();
                            SensorSafety();
                            Thread.Sleep(moveControlConfig.SleepTime);
                        }

                        ControlData.PauseAlready = false;
                        ControlData.CancelRequest = false;
                        MoveState = EnumMoveState.Idle;

                        MoveFinished(EnumMoveComplete.Cancel, command.IsRetryMove);
                        WriteLog("MoveControl", "7", device, "", "AGV已經停止 Cancel已完成!");
                    }

                    if (MoveState == EnumMoveState.Idle && ControlData.CloseMoveControl)
                        break;

                    Thread.Sleep(moveControlConfig.SleepTime);
                }
            }
            catch
            {
                EMSControl("MoveControl主Thread Expction!");
                SendAlarmCode(150000);
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

            if (index == -1)
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
                            SendAlarmCode(156001);
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
            SendAlarmCode(159999);
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
                    {
                        EMSControl("暫時By pass TR2000中啟動!");
                        SendAlarmCode(159999);
                    }
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
                    {
                        EMSControl("暫時By pass TR2000中啟動!");
                        SendAlarmCode(159999);
                    }
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

        private bool CheckAxisNoError(ref int alarmcode)
        {
            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.CheckAxisState].Enable)
                return true;

            if (SimulationMode)
            {
                if (!FakeState.AxisNormal)
                    alarmcode = 142000;

                return FakeState.AxisNormal;
            }
            else
                return elmoDriver.CheckAxisNoError(ref alarmcode);
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
                            EMSControl("降速指令未降速!");
                            SendAlarmCode(151000);
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
                            EMSControl("降速指令未降速!");
                            SendAlarmCode(151000);
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

            if (ControlData.BeamSensorState == EnumVehicleSafetyAction.Normal)
            {
                ControlData.R2000Stop = false;
            }

            if (beamSensorState == EnumVehicleSafetyAction.Stop && !ControlData.SecondCorrection)
            {
                EnumCommandType type = EnumCommandType.End;

                bool canStop = CanStopInNextTurn(ref type);

                if (!canStop)
                {
                    if (type == EnumCommandType.TR)
                    {
                        if (moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensorTR].Enable)
                            canStop = true;
                    }
                    else if (type == EnumCommandType.R2000)
                    {
                        if (moveControlConfig.Safety[EnumMoveControlSafetyType.BeamSensorR2000].Enable)
                        {
                            if (ControlData.R2000Stop)
                            {
                                if (ControlData.R2000StopTimer.ElapsedMilliseconds > moveControlConfig.Safety[EnumMoveControlSafetyType.BeamSensorR2000].Range)
                                    canStop = true;
                            }
                            else
                            {
                                ControlData.R2000Stop = true;
                                ControlData.R2000StopTimer.Restart();
                            }
                        }
                    }
                }

                if (safetyData.TurningByPass || !canStop)
                    beamSensorState = EnumVehicleSafetyAction.Normal;
            }

            if (ControlData.WaitReserveIndex != -1)
            {
                if (command.ReserveList[ControlData.WaitReserveIndex].GetReserve)
                {
                    WriteLog("MoveControl", "7", device, "", "取得Reserve index = " + ControlData.WaitReserveIndex.ToString() + ", WaitReserveIndex 變回 -1 !");
                    ControlData.WaitReserveIndex = -1;
                }
            }

            if (ControlData.PauseRequest || ControlData.PauseAlready || ControlData.WaitReserveIndex != -1 ||
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

        private void UpdateAGVState()
        {
            if (MoveState == EnumMoveState.Idle || MoveState == EnumMoveState.Error)
                AGVState = MoveState.ToString();
            else
            {
                if (ControlData.SensorState == EnumVehicleSafetyAction.Stop)
                {
                    if (ControlData.PauseRequest || ControlData.PauseAlready)
                        AGVState = "AGVC Command : Pause";
                    else if (ControlData.WaitReserveIndex != -1)
                        AGVState = "Wait Reserve";
                    else if (ControlData.BeamSensorState == EnumVehicleSafetyAction.Stop)
                        AGVState = "BeamSensor";
                    else if (ControlData.FlowStopRequeset)
                        AGVState = "StopAndClear";
                    else
                        AGVState = "Stop ?? ";
                }
                else
                    AGVState = EnumMoveState.Moving.ToString();
            }
        }

        private bool IsAGVStopWithReasonTimeout()
        {
            if (SimulationMode)
                return false;

            if (moveControlConfig.Safety[EnumMoveControlSafetyType.StopWithoutReason].Enable && ControlData.StopWithoutReason)
                return ControlData.StopWithoutReasonTimer.ElapsedMilliseconds > (moveControlConfig.Safety[EnumMoveControlSafetyType.StopWithoutReason].Range);
            else
                return false;
        }

        private void SensorSafety()
        {
            if (MoveState == EnumMoveState.Idle || MoveState == EnumMoveState.Error)
                return;

            int alarmcode = 0;

            if (ForkNotHome())
            {
                EMSControl("走行中Fork不在Home點!");
                SendAlarmCode(140000);
            }
            else if (!CheckAxisNoError(ref alarmcode))
            {
                if (elmoDriver.CheckAxisAllError())
                    WriteLog("MoveControl", "7", device, "", "Elmo All Axis Error!");

                EMSControl("走行中Axis Error!");
                SendAlarmCode(alarmcode);
            }
            else if (ControlData.FlowStopRequeset)
            {
                EMSControl("Stop Request!");
                ControlData.FlowStopRequeset = false;

                if (ControlData.FlowClear)
                {
                    WriteLog("MoveControl", "7", device, "", "AGV已經停止 State 切換成 Idle!");
                    MoveState = EnumMoveState.Idle;
                    ControlData.FlowClear = false;
                    AGVStopResult = "";

                    if (!SimulationMode)
                        location.Real = null;
                }
            }
            else if (IsAGVStopWithReasonTimeout())
            {
                EMSControl("AGV默停!");
                SendAlarmCode(150003);
            }
            else
            {
                EnumVehicleSafetyAction sensorState = UpdateSensorState();
                if (ControlData.SensorState != sensorState)
                    WriteLog("MoveControl", "7", device, "", "SensorState 從 " + ControlData.SensorState.ToString() +
                             " 變更為 " + sensorState.ToString() + "!");

                SensorAction(sensorState);
                ControlData.SensorState = sensorState;
                UpdateAGVState();

                VChangeSafety();
            }
        }
        #endregion

        #region BeamSensor Sleep/Awake切換
        private void BeamSensorSingleAwake(EnumBeamSensorLocate locate)
        {
            switch (locate)
            {
                case EnumBeamSensorLocate.Front:
                    //Vehicle.Instance.ThePlcVehicle.MoveFront = true;
                    break;

                case EnumBeamSensorLocate.Back:
                    //Vehicle.Instance.ThePlcVehicle.MoveBack = true;
                    break;

                case EnumBeamSensorLocate.Left:
                    //Vehicle.Instance.ThePlcVehicle.MoveLeft = true;
                    break;

                case EnumBeamSensorLocate.Right:
                    //Vehicle.Instance.ThePlcVehicle.MoveRight = true;
                    break;

                default:
                    break;
            }

            WriteLog("MoveControl", "7", device, "", "Beam sensor Sleep/Awake切換 : 修改 " + locate.ToString() + " 變更為Awake !");
        }

        private void BeamSensorOnlyAwake(EnumBeamSensorLocate locate)
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

            //Vehicle.Instance.ThePlcVehicle.MoveFront = front;
            //Vehicle.Instance.ThePlcVehicle.MoveBack = back;
            //Vehicle.Instance.ThePlcVehicle.MoveLeft = left;
            //Vehicle.Instance.ThePlcVehicle.MoveRight = right;

            WriteLog("MoveControl", "7", device, "", "Beam sensor Sleep/Awake切換 : 只剩 " + locate.ToString() + " Awake !");
        }

        private void BeamSensorSleepAll()
        {
            //Vehicle.Instance.ThePlcVehicle.MoveFront = false;
            //Vehicle.Instance.ThePlcVehicle.MoveBack = false;
            //Vehicle.Instance.ThePlcVehicle.MoveLeft = false;
            //Vehicle.Instance.ThePlcVehicle.MoveRight = false;

            WriteLog("MoveControl", "7", device, "", "Beam sensor 切換 : all sleep!");
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

        #region 終極Barcode位置安全保護
        private bool ShouldInThisBarcode(BarcodeSafetyData barcodeSafetyData, double barcodeEncoder)
        {
            return (ControlData.DirFlag && barcodeEncoder > barcodeSafetyData.StartEncoder) ||
                  (!ControlData.DirFlag && barcodeEncoder < barcodeSafetyData.StartEncoder);
        }

        private bool OutOfTriggerRange(BarcodeSafetyData barcodeSafetyData, double barcodeEncoder)
        {
            return (ControlData.DirFlag && barcodeEncoder > barcodeSafetyData.StartEncoder + moveControlConfig.Safety[EnumMoveControlSafetyType.BarcodePositionSafety].Range) ||
                  (!ControlData.DirFlag && barcodeEncoder < barcodeSafetyData.StartEncoder - moveControlConfig.Safety[EnumMoveControlSafetyType.BarcodePositionSafety].Range);
        }

        private bool ShouldOutThisBarcode(BarcodeSafetyData barcodeSafetyData, double barcodeEncoder)
        {
            return (ControlData.DirFlag && barcodeEncoder > barcodeSafetyData.EndEncoder) ||
                  (!ControlData.DirFlag && barcodeEncoder < barcodeSafetyData.EndEncoder);
        }

        private void FinalBarcodePositionSafety()
        {
            if (!moveControlConfig.Safety[EnumMoveControlSafetyType.BarcodePositionSafety].Enable || MoveState == EnumMoveState.Idle || MoveState == EnumMoveState.Error)
                return;

            double barcodeEncoder = location.RealEncoder - sr2000ConfigList[0].TimeOutValue / 1000 * location.Velocity / 1.5;

            // Left : 
            if (command.LeftBarcodeLineList != null && command.BarcodeLineListIndex < command.LeftBarcodeLineList.Count &&
                command.IndexOfLeftBarcodeLineList < command.LeftBarcodeLineList[command.BarcodeLineListIndex].Count)
            {
                if (ShouldOutThisBarcode(command.LeftBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfLeftBarcodeLineList], barcodeEncoder))
                {
                    if (!SimulationMode && command.LeftBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfLeftBarcodeLineList].MustRead)
                    {
                        EMSControl(String.Concat("SR2000L沒讀取到Barcode ID : ", command.LeftBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfLeftBarcodeLineList].BarcodeLineID));
                        SendAlarmCode(130009);
                        return;
                    }
                    else
                    {
                        LeftRead = false;
                        command.IndexOfLeftBarcodeLineList++;
                    }
                }
                else if (ShouldInThisBarcode(command.LeftBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfLeftBarcodeLineList], barcodeEncoder))
                {
                    LeftRead = true;

                    if (location.BarcodeLeft != null && location.BarcodeLeft.BarcodeLineID == command.LeftBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfLeftBarcodeLineList].BarcodeLineID)
                    {
                        LeftRead = false;
                        command.IndexOfLeftBarcodeLineList++;
                    }
                }
            }

            // Right :

            if (command.RightBarcodeLineList != null && command.BarcodeLineListIndex < command.RightBarcodeLineList.Count &&
                command.IndexOfRightBarcodeLineList < command.RightBarcodeLineList[command.BarcodeLineListIndex].Count)
            {

                if (ShouldOutThisBarcode(command.RightBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfRightBarcodeLineList], barcodeEncoder))
                {
                    if (!SimulationMode && command.RightBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfRightBarcodeLineList].MustRead)
                    {
                        EMSControl(String.Concat("SR2000R沒讀取到Barcode ID : ", command.RightBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfRightBarcodeLineList].BarcodeLineID));
                        SendAlarmCode(130009);
                        return;
                    }
                    else
                    {
                        RightRead = false;
                        command.IndexOfRightBarcodeLineList++;
                    }
                }
                else if (ShouldInThisBarcode(command.RightBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfRightBarcodeLineList], barcodeEncoder))
                {
                    RightRead = true;

                    if (location.BarcodeRight != null && location.BarcodeRight.BarcodeLineID == command.RightBarcodeLineList[command.BarcodeLineListIndex][command.IndexOfRightBarcodeLineList].BarcodeLineID)
                    {
                        RightRead = false;
                        command.IndexOfRightBarcodeLineList++;
                    }
                }
            }
        }
        #endregion

        #region 外部連結 : 產生List、DebugForm相關、狀態、移動完成.
        public void MoveFinished(EnumMoveComplete status, bool isRetryMove = false)
        {
            bool sendEvent = true;
            WriteLog("MoveControl", "7", device, "", "status : " + status.ToString());

            if (status == EnumMoveComplete.Fail)
                WriteLog("Error", "7", device, "", "status : " + status.ToString());

            if (status != EnumMoveComplete.Pause)
            {
                simulationIsMoving = false;
                BeamSensorCloseAll();
                BeamSensorSleepAll();
                DirLightCloseAll();
                ControlData.OntimeReviseFlag = false;
                ControlData.MoveControlCommandComplete = true;
                ControlData.MoveControlCommandCompleteTimer.Restart();
            }

            if (ControlData.CommandMoving)
            {
                if (status == EnumMoveComplete.Pause)
                {
                    if (ControlData.PauseNotSendEvent)
                    {
                        WriteLog("MoveControl", "7", device, "", "AGV已經停止 PauseAlready, 但不發送Event!");
                        ControlData.PauseNotSendEvent = false;
                        sendEvent = false;
                    }
                }
                else
                {
                    ControlData.CommandMoving = false;

                    if (status == EnumMoveComplete.Cancel && ControlData.CancelNotSendEvent)
                    {
                        WriteLog("MoveControl", "7", device, "", "AGV已經停止 Cancel已完成, 但不發送Event!");
                        ControlData.CancelNotSendEvent = false;
                        sendEvent = false;
                    }
                }
            }
            else
                WriteLog("MoveControl", "7", device, "", "error : no Command send MoveFinished, status : " + status.ToString());

            if (isAGVMCommand && sendEvent)
            {
                WriteLog("MoveControl", "7", device, "", "send event to middler : " + status.ToString());
                if (!isRetryMove)
                    OnMoveFinished?.Invoke(this, status);
                else
                    OnRetryMoveFinished?.Invoke(this, status);
            }

            UpdateAGVState();
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
                SendAlarmCode(159998);
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
                SendAlarmCode(159999);
            }

            location.Encoder = new AGVPosition();
            location.Encoder.Position = new MapPosition(location.Real.Position.X, location.Real.Position.Y);
            location.Encoder.AGVAngle = location.Real.AGVAngle;
            location.Delta = 0;
            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void ResetFlag()
        {
            ControlData.StopWithoutReason = false;
            ControlData.ResetMoveControlCommandMoving = true;
            ControlData.SensorState = EnumVehicleSafetyAction.Normal;
            ControlData.BeamSensorState = EnumVehicleSafetyAction.Normal;
            ControlData.BumpSensorState = EnumVehicleSafetyAction.Normal;
            ControlData.OntimeReviseFlag = false;
            ControlData.KeepsLowSpeedStateByEQVChange = EnumVehicleSafetyAction.Stop;
            ControlData.PauseRequest = false;
            ControlData.PauseAlready = false;
            ControlData.PauseNotSendEvent = false;
            ControlData.CancelRequest = false;
            ControlData.CancelNotSendEvent = false;
            ControlData.FlowStopRequeset = false;
            ControlData.FlowClear = false;
            ControlData.SecondCorrection = false;
            ControlData.RealVelocity = 0;
            ControlData.EQVChange = false;
            ControlData.R2000Stop = false;

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
                WriteLog("MoveControl", "7", device, "", "程式關閉中,拒絕AGVM Move命令.");
                errorMessage = "程式關閉中,拒絕AGVM Move命令.";
                AGVStopResult = "程式關閉中,拒絕AGVM Move命令.";
                SendAlarmCode(110000);
                return false;
            }
            else if ((MoveState != EnumMoveState.Error && MoveState != EnumMoveState.Idle))
            {
                WriteLog("MoveControl", "7", device, "", "移動中,因此無視~!");
                errorMessage = "移動中,因此無視~!";
                AGVStopResult = "移動中,因此無視AGVM Move命令~!";
                SendAlarmCode(110001);
                return false;
            }
            else if (IsCharging())
            {
                WriteLog("MoveControl", "7", device, "", "Charging中,因此無視~!");
                errorMessage = "Charging中";
                AGVStopResult = "Charging中,因此無視AGVM Move命令~!";
                SendAlarmCode(110002);
                return false;
            }
            else if (ForkNotHome())
            {
                WriteLog("MoveControl", "7", device, "", "Fork不在Home點,因此無視~!");
                errorMessage = "Fork不在Home點";
                AGVStopResult = "Fork不在Home點,因此無視AGVM Move命令~!";
                SendAlarmCode(110003);
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
                        AGVStopResult = "起點和目前位置差距過大,因此無視AGVM Move命令~!";
                        SendAlarmCode(110004);
                        return false;
                    }
                }
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "AGV迷航中(不知道目前在哪),因此無法接受命令!");
                errorMessage = "AGV迷航中(不知道目前在哪),因此無法接受命令!";
                AGVStopResult = "AGV迷航中(不知道目前在哪),因此無法接受AGVM Move命令!";
                SendAlarmCode(110005);
                return false;
            }

            UpdateWheelAngle();
            MoveCommandData tempCommand = CreateMoveCommandList.CreateMoveControlListSectionListReserveList(
                                          moveCmd, location.Real, ControlData.WheelAngle, ref errorMessage);

            if (tempCommand == null)
            {
                WriteLog("MoveControl", "7", device, "", "命令分解失敗~!, errorMessage : " + errorMessage);
                AGVStopResult = errorMessage;
                SendAlarmCode(110006);
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

        public bool TransferMove_Override(MoveCmdInfo moveCmd, ref string errorMessage)
        {
            WriteLog("MoveControl", "7", device, "", "Override start");

            if (MoveState == EnumMoveState.Idle)
            {
                WriteLog("MoveControl", "7", device, "", "Idle中收到override....轉傳成一般移動命令試試..");
                return TransferMove(moveCmd, ref errorMessage);
            }

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Reset();
            timer.Start();

            if (!VehclePause(true))
            {
                WriteLog("MoveControl", "7", device, "", "AGV不處於不能Pause狀態,因此拒絕AGVM Override命令.");
                errorMessage = "AGV不處於不能Pause狀態,因此拒絕AGVM Override命令.";
                AGVStopResult = "AGV不處於不能Pause狀態,因此拒絕AGVM Override命令.";
                SendAlarmCode(111000);
                WriteLog("MoveControl", "7", device, "", String.Concat("Override 失敗,", errorMessage));
                return false;
            }

            while (!ControlData.PauseAlready)
            {
                if (timer.ElapsedMilliseconds > moveControlConfig.OverrideTimeoutValue)
                {
                    errorMessage = "wait PauseAlready timeout!";
                    SendAlarmCode(111001);
                    WriteLog("MoveControl", "7", device, "", String.Concat("Override 失敗,", errorMessage));
                    return false;
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            if (MoveState == EnumMoveState.Idle || MoveState == EnumMoveState.Error)
            {
                errorMessage = "PauseAlready, 但是車子狀態變為" + MoveState.ToString() + "!";
                WriteLog("MoveControl", "7", device, "", String.Concat("Override 失敗,", errorMessage));
                return false;
            }

            VehcleCancel(true);

            while (MoveState != EnumMoveState.Idle)
            {
                if (timer.ElapsedMilliseconds > moveControlConfig.OverrideTimeoutValue)
                {
                    errorMessage = "wait Cancel timeout!";
                    SendAlarmCode(111001);
                    WriteLog("MoveControl", "7", device, "", String.Concat("Override 失敗,", errorMessage));
                    return false;
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            ControlData.MoveStartNoWaitTime = true;

            if (!TransferMove(moveCmd, ref errorMessage))
            {
                ControlData.MoveStartNoWaitTime = false;
                WriteLog("MoveControl", "7", device, "", String.Concat("Override 失敗,", errorMessage));
                return false;
            }

            WriteLog("MoveControl", "7", device, "", "Override 成功!");
            return true;
        }

        public void TransferMove_RetryMove()
        {
            WriteLog("MoveControl", "7", device, "", "start!");

            if (command.End != null && command.RetryMovePosition != null)
            {
                MoveCmdInfo temp = new MoveCmdInfo();
                temp.StartAddress = new MapAddress();
                temp.StartAddress.AddressOffset = new MapAddressOffset();
                temp.StartAddress.AddressOffset.OffsetX = 0;
                temp.StartAddress.AddressOffset.OffsetY = 0;
                temp.StartAddress.AddressOffset.OffsetTheta = 0;
                temp.EndAddress.AddressOffset = new MapAddressOffset();
                temp.EndAddress.AddressOffset.OffsetX = 0;
                temp.EndAddress.AddressOffset.OffsetY = 0;
                temp.EndAddress.AddressOffset.OffsetTheta = 0;
                temp.MovingSections = new List<MapSection>();
                MapSection tempMovingSection = new MapSection();
                tempMovingSection.Speed = moveControlConfig.EQ.Velocity;
                tempMovingSection.Type = EnumSectionType.None;
                temp.MovingSections.Add(tempMovingSection);
                temp.MovingSections.Add(tempMovingSection);
                temp.SectionSpeedLimits.Add(moveControlConfig.EQ.Velocity);
                temp.SectionSpeedLimits.Add(moveControlConfig.EQ.Velocity);
                temp.AddressActions = new List<EnumAddressAction>();
                temp.MovingAddress = new List<MapAddress>();
                MapAddress tempMapAddress = new MapAddress();
                tempMapAddress.Position = command.End;
                temp.MovingAddress.Add(tempMapAddress);
                tempMapAddress = new MapAddress();
                tempMapAddress.Position = command.RetryMovePosition;
                temp.MovingAddress.Add(tempMapAddress);
                tempMapAddress = new MapAddress();
                tempMapAddress.Position = command.End;
                temp.MovingAddress.Add(tempMapAddress);
                temp.AddressPositions.Add(command.End);
                temp.AddressPositions.Add(command.RetryMovePosition);
                temp.AddressPositions.Add(command.End);

                bool result = true;
                if (ControlData.CloseMoveControl)
                    result = false;
                else if ((MoveState != EnumMoveState.Error && MoveState != EnumMoveState.Idle))
                    result = false;
                else if (IsCharging())
                    result = false;
                else if (ForkNotHome())
                    result = false;

                if (location.Real != null)
                {
                    double distance = Math.Sqrt(Math.Pow(location.Real.Position.X - command.End.X, 2) +
                                                Math.Pow(location.Real.Position.Y - command.End.Y, 2));

                    if (distance > 50)
                        result = false;
                }
                else
                    result = false;

                string errorMessage = "";

                if (result)
                {
                    UpdateWheelAngle();
                    MoveCommandData tempCommand = CreateMoveCommandList.CreateMoveControlListSectionListReserveList(
                                                  temp, location.Real, ControlData.WheelAngle, ref errorMessage);

                    if (tempCommand == null)
                    {
                        WriteLog("MoveControl", "7", device, "", "命令分解失敗~!,直接報完成!");

                        Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(1000);
                            MoveFinished(EnumMoveComplete.Success, true);
                        });
                    }
                    else
                    {
                        tempCommand.IsRetryMove = true;
                        command = tempCommand;
                        ResetFlag();
                        MoveCommandID = temp.CmdId;
                        isAGVMCommand = true;
                        MoveState = EnumMoveState.Moving;
                        AddAllReserve();
                    }
                }
                else
                {
                    WriteLog("MoveControl", "7", device, "", "狀態檢查未通過,直接報完成!");

                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1000);
                        MoveFinished(EnumMoveComplete.Success, true);
                    });
                }
            }
            else
            {
                WriteLog("MoveControl", "7", device, "", "無RetryMove可以嘗試,直接報完成!");

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    MoveFinished(EnumMoveComplete.Success, true);
                });
            }
        }

        public void StopAndClear([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            WriteLog("MoveControl", "7", device, "", memberName + " : StopAndClear!");

            if (MoveState != EnumMoveState.Error && MoveState != EnumMoveState.Idle)
            {
                if (MoveState == EnumMoveState.Moving)
                {
                    string sensorStateLog = String.Concat("SensorState : ", ControlData.SensorState.ToString(), ", Bump : ", ControlData.BumpSensorState.ToString(),
                                                          ", Beam : ", ControlData.BeamSensorState.ToString(), ", Wait Reserve : ", ControlData.WaitReserveIndex.ToString(),
                                                          ", Pause : ", ((ControlData.PauseRequest || ControlData.PauseAlready) ? EnumVehicleSafetyAction.Stop : EnumVehicleSafetyAction.Normal),
                                                          ", FlowStop : ", ((ControlData.FlowStopRequeset) ? EnumVehicleSafetyAction.Stop : EnumVehicleSafetyAction.Normal));
                    WriteLog("MoveControl", "7", device, "", sensorStateLog);
                }

                ControlData.FlowStopRequeset = true;
                ControlData.FlowClear = true;
            }
            else
            {
                ControlData.FlowStopRequeset = false;
                ControlData.FlowClear = false;
                AGVStopResult = "";

                if (!SimulationMode && MoveState == EnumMoveState.Error)
                    location.Real = null;

                WriteLog("MoveControl", "7", device, "", "StopAndClear時AGV已停止 State 直接切換成 Idle!");
                MoveState = EnumMoveState.Idle;
            }
        }

        public bool MoveControlCanAuto(ref string errorMessage)
        {
            int errorcode = 0;

            if (MoveState != EnumMoveState.Idle)
            {
                if (MoveState == EnumMoveState.Error)
                    errorMessage = "AGV Error狀態!";
                else
                    errorMessage = "流程移動中!";

                SendAlarmCode(120000);
                return false;
            }
            else if (location.Real == null)
            {
                errorMessage = "迷航中,沒讀取到鐵Barcode!";
                SendAlarmCode(120001);
                return false;
            }
            else if (Vehicle.Instance.VehicleLocation.LastAddress.Id == "" || Vehicle.Instance.VehicleLocation.LastSection.Id == "")
            {
                errorMessage = "迷航中,認不出目前所在Address、Section!";
                SendAlarmCode(120002);
                return false;
            }
            else if (!elmoDriver.CheckAxisNoError(ref errorcode))
            {
                errorMessage = "有軸異常!";
                SendAlarmCode(120003);
                return false;
            }
            else if (!elmoDriver.ElmoAxisTypeAllServoOn(EnumAxisType.Turn))
            {
                errorMessage = "請Enable所有軸!";
                SendAlarmCode(120004);
                return false;
            }
            else if (!elmoDriver.MoveAxisStop())
            {
                errorMessage = "AGV移動中!";
                SendAlarmCode(120005);
                return false;
            }

            return true;
        }

        private bool CanStopInNextTurn(ref EnumCommandType type)
        {
            int index = -1;

            for (int i = command.IndexOfCmdList; i < command.CommandList.Count; i++)
            {
                if (command.CommandList[i].CmdType == EnumCommandType.SlowStop)
                    break;
                else if (command.CommandList[i].CmdType == EnumCommandType.TR)
                {
                    index = i;
                    type = EnumCommandType.TR;

                    break;
                }
                else if (command.CommandList[i].CmdType == EnumCommandType.R2000)
                {
                    index = i;
                    type = EnumCommandType.R2000;

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

            EnumCommandType type = EnumCommandType.End;

            bool canStop = CanStopInNextTurn(ref type);

            if (!canStop)
            {
                if (type == EnumCommandType.TR)
                {
                    if (moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensorTR].Enable)
                        canStop = true;
                }
                else if (type == EnumCommandType.R2000)
                {
                    if (moveControlConfig.Safety[EnumMoveControlSafetyType.BeamSensorR2000].Enable)
                        canStop = true;
                }
            }

            if (!ControlData.CanPause || !canStop)
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

        public bool VehclePause(bool hideFunction = false)
        {
            WriteLog("MoveControl", "7", device, "", "Pause Request!");

            if (ControlData.PauseRequest)
            {
                WriteLog("MoveControl", "7", device, "", "AGV PauseRequest,因此不再發送PauseRequest!");
                return true;
            }
            else if (ControlData.PauseAlready)
            {
                WriteLog("MoveControl", "7", device, "", "AGV PauseAlready,因此不再發送PauseRequest!");
                return true;
            }
            else if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error &&
                !ControlData.PauseRequest && !ControlData.PauseAlready && CanPauseNow())
            {
                WriteLog("MoveControl", "7", device, "", "Pause Request接受!");
                ControlData.PauseRequest = true;

                if (hideFunction)
                    ControlData.PauseNotSendEvent = true;

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

        public void VehcleCancel(bool hideFunction = false)
        {
            WriteLog("MoveControl", "7", device, "", "Cancel Request!");
            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error && !ControlData.CancelRequest)
            {
                WriteLog("MoveControl", "7", device, "", "Cancel Request接受!");
                ControlData.CancelRequest = true;

                if (hideFunction)
                    ControlData.CancelNotSendEvent = true;
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

            UpdateWheelAngle();
            return CreateMoveCommandList.CreateMoveControlListSectionListReserveList(
                        moveCmd, location.Real, ControlData.WheelAngle, ref errorMessage, false);
        }

        public bool GetPositionActions(ref MoveCmdInfo moveCommand, ref string errorMessage)
        {
            UpdateWheelAngle();
            return CreateMoveCommandList.GetMoveCommandAddressAction(ref moveCommand, location.Real, ControlData.WheelAngle, ref errorMessage, false);
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
            if (Vehicle.Instance.AutoState == EnumAutoState.Auto || MoveState != EnumMoveState.Idle)
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

            while (true)
            {
                timer.Reset();
                timer.Start();

                if (ControlData.ResetMoveControlCommandMoving)
                {
                    ControlData.ResetMoveControlCommandMoving = false;
                    ControlData.MoveControlCommandMoving = true;
                    ControlData.MoveControlCommandComplete = false;
                }

                if (moveControlConfig.Safety[EnumMoveControlSafetyType.IdleNotWriteLog].Enable &&
                                      ControlData.MoveControlCommandComplete)
                {
                    if (ControlData.MoveControlCommandCompleteTimer.ElapsedMilliseconds >
                        moveControlConfig.Safety[EnumMoveControlSafetyType.IdleNotWriteLog].Range)
                    {
                        ControlData.MoveControlCommandComplete = false;
                        ControlData.MoveControlCommandMoving = false;
                    }
                }

                if (!moveControlConfig.Safety[EnumMoveControlSafetyType.IdleNotWriteLog].Enable ||
                    ControlData.MoveControlCommandMoving)
                {
                    //  Debug 
                    //BarcodeX	BarocdeY	ElmoEncoder	elmoV	TurnP	TurnV

                    //  Time
                    now = DateTime.Now;
                    csvLog = now.ToString("yyyy/MM/dd HH:mm:ss.fff");

                    //  State
                    if (MoveState != EnumMoveState.TR)
                        AddCSV(ref csvLog, MoveState.ToString());
                    else
                        AddCSV(ref csvLog, ControlData.TurnType.ToString());

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
                    logAGVPosition = location.agvPosition;
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
                }

                while (timer.ElapsedMilliseconds < moveControlConfig.CSVLogInterval)
                    Thread.Sleep(1);
            }
        }
        #endregion
    }
}