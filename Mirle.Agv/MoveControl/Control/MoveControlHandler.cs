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

        public ElmoDriver elmoDriver;
        public List<Sr2000Driver> DriverSr2000List = new List<Sr2000Driver>();
        public OntimeReviseConfig ontimeReviseConfig = null;
        private AgvMoveRevise agvRevise;

        public event EventHandler<EnumMoveComplete> OnMoveFinished;

        private List<SectionLine> SectionLineList = new List<SectionLine>();
        private int indexOflisSectionLine = 0;
        public Location location = new Location();
        private EncoderPositionData encoderPositionData = new EncoderPositionData();

        public List<Command> CommandList { get; private set; } = new List<Command>();
        public int IndexOfCmdList { get; private set; } = 0;

        public MoveControlParameter ControlData { get; private set; } = new MoveControlParameter();
        private const int AllowableTheta = 10;
        public List<ReserveData> ReserveList { get; private set; } = new List<ReserveData>();
        Thread threadSCVLog;
        public bool DebugCSVMode { get; set; } = false;
        public List<string[]> deubgCsvLogList = new List<string[]>();
        private bool isAGVMCommand = false;
        public string MoveCommandID { get; set; } = "";

        public bool SimulationMode { get; set; } = false;
        private bool simulationIsMoving = false;
        private MoveControlSafetyData safetyData = new MoveControlSafetyData();

        public bool DebugFlowMode { get; set; } = true;
        public string AGVStopResult { get; set; }
        private const int debugFlowLogMaxLength = 10000;

        public string DebugFlowLog { get; set; }

        private bool r2000SlowStop = false;
        public int WaitReseveIndex { get; set; }

        private void SetDebugFlowLog(string functionName, string message)
        {
            if (DebugFlowMode)
            {
                //functionName = functionName + "                                               ";
                //functionName = functionName.Substring(0, 35);

                DebugFlowLog = DateTime.Now.ToString("HH:mm:ss.fff") + "\t" + functionName + "\t" + message + "\r\n" + DebugFlowLog;
                if (DebugFlowLog.Length > debugFlowLogMaxLength)
                    DebugFlowLog = DebugFlowLog.Substring(0, debugFlowLogMaxLength);
            }
        }

        public MoveControlHandler(MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
            ReadMoveControlConfigXML("MoveControlConfig.xml");

            InitailSr2000(moveControlConfig.Sr2000ConfigPath);
            elmoDriver = new ElmoDriver(moveControlConfig.ElmoConfigPath);

            ReadOntimeReviseConfigXML(moveControlConfig.OnTimeReviseConfigPath);
            createMoveControlList = new CreateMoveControlList(DriverSr2000List, moveControlConfig);

            agvRevise = new AgvMoveRevise(ontimeReviseConfig, elmoDriver, DriverSr2000List, moveControlConfig.Safety);

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
                    case "OntimeReviseSectionDeviation":
                    case "OntimeReviseTheta":
                    case "UpdateDeltaPositionRange":
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
                    case "Bumper":
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
                WriteLog("MoveControl", "3", device, "", "MoveControlConfig 路徑錯誤為null或空值,請檢查小練的xml.");
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
                    case "EQVelocity":
                        moveControlConfig.EQVelocity = double.Parse(item.InnerText);
                        break;
                    case "EQVelocityDistance":
                        moveControlConfig.EQVelocityDistance = double.Parse(item.InnerText);
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
            moveControlConfig.CSVLogInterval = 50;
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
            bool sr2000InitialResult = true;

            foreach (XmlNode item in rootNode.ChildNodes)
            {
                sr2000Config = ConvertXmlElementToSr2000Config((XmlElement)item);
                driverSr2000 = new Sr2000Driver(sr2000Config, theMapInfo);
                if (driverSr2000.GetConnect())
                    DriverSr2000List.Add(driverSr2000);
                else
                    sr2000InitialResult = false;
            }

            if (!sr2000InitialResult)
                WriteLog("Error", "1", device, "", "SR2000 啟動失敗!");
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

        #region ReadLineReviseConfig
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
            if (SectionLineList.Count != 0)
            {
                double realElmoEncode = location.ElmoEncoder + location.Delta + location.Offset;
                location.RealEncoder = realElmoEncode;

                if (SectionLineList[indexOflisSectionLine].DirFlag)
                {
                    if (realElmoEncode > SectionLineList[indexOflisSectionLine].TransferPositionEnd)
                        realElmoEncode = SectionLineList[indexOflisSectionLine].TransferPositionEnd;
                    else if (realElmoEncode < SectionLineList[indexOflisSectionLine].TransferPositionStart)
                        realElmoEncode = SectionLineList[indexOflisSectionLine].TransferPositionStart;
                }
                else
                {
                    if (realElmoEncode < SectionLineList[indexOflisSectionLine].TransferPositionEnd)
                        realElmoEncode = SectionLineList[indexOflisSectionLine].TransferPositionEnd;
                    else if (realElmoEncode > SectionLineList[indexOflisSectionLine].TransferPositionStart)
                        realElmoEncode = SectionLineList[indexOflisSectionLine].TransferPositionStart;
                }

                location.Real.Position = GetMapPosition(SectionLineList[indexOflisSectionLine], realElmoEncode);
                Vehicle.Instance.theVehiclePosition.RealPosition = location.Real.Position;
            }
        }

        private void UpdateDelta()
        {
            double realEncoder = MapPositionToEncoder(SectionLineList[indexOflisSectionLine], location.Barcode.Position);
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

        public double GetAGVAngle(double originAngle)
        {
            if (Math.Abs(originAngle - 0) < AllowableTheta)
                return 0;
            else if (Math.Abs(originAngle - 90) < AllowableTheta)
                return 90;
            else if (Math.Abs(originAngle - -90) < AllowableTheta)
                return -90;
            else if (Math.Abs(originAngle - 180) < AllowableTheta || Math.Abs(originAngle - -180) < AllowableTheta)
                return 180;
            else
                return originAngle;
        }

        public bool IsSameAngle(double barcodeAngleInMap, double agvAngleInMap, int wheelAngle)
        {
            if (Math.Abs(agvAngleInMap - 0) < AllowableTheta)
                agvAngleInMap = 0;
            else if (Math.Abs(agvAngleInMap - 90) < AllowableTheta)
                agvAngleInMap = 90;
            else if (Math.Abs(agvAngleInMap - -90) < AllowableTheta)
                agvAngleInMap = -90;
            else if (Math.Abs(agvAngleInMap - 180) < AllowableTheta || Math.Abs(agvAngleInMap - -180) < AllowableTheta)
                agvAngleInMap = 180;
            else
                return false;

            return (agvAngleInMap + barcodeAngleInMap + wheelAngle) % 180 == 0;
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
                        if (MoveState == EnumMoveState.Idle || MoveState == EnumMoveState.Error)
                            break;
                        else if (IsSameAngle(agvPosition.BarcodeAngleInMap, agvPosition.AGVAngle, ControlData.WheelAngle))
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

                    Vehicle.Instance.theVehiclePosition.BarcodePosition = location.Barcode.Position;
                    return true;
                }
            }
            else
            {
                if (location.Barcode == null)
                {
                    MapPosition tempPosition = new MapPosition(0, 0);
                    location.Barcode = new AGVPosition(tempPosition, 0, 0, 20, DateTime.Now, 0, 0);
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

        private void UpdateEncoderPositionNowEncoder()
        {
            return;
            encoderPositionData.LastMoveEncoder = encoderPositionData.NowMoveEncoder;
            encoderPositionData.NowMoveEncoder = new Dictionary<EnumAxis, double>();
            encoderPositionData.NowTurnEncoder = new Dictionary<EnumAxis, double>();
            List<EnumAxis> moveList = new List<EnumAxis>() { EnumAxis.XFL, EnumAxis.XFR, EnumAxis.XRL, EnumAxis.XRR };
            List<EnumAxis> turnList = new List<EnumAxis>() { EnumAxis.TFL, EnumAxis.TFR, EnumAxis.TRL, EnumAxis.TRR };

            double temp;

            for (int i = 0; i < moveList.Count; i++)
            {
                temp = elmoDriver.ElmoGetPosition(moveList[i], true);
                encoderPositionData.NowMoveEncoder.Add(moveList[i], temp);

                temp = elmoDriver.ElmoGetPosition(turnList[i], true);
                encoderPositionData.NowTurnEncoder.Add(turnList[i], temp);
            }
        }

        private void UpdateEncoderPosition()
        {
            return;
            if (!SimulationMode)
            {
                AGVPosition tempAGVPosition = new AGVPosition();
                double deltaX = ((encoderPositionData.NowMoveEncoder[EnumAxis.XFL] - encoderPositionData.LastMoveEncoder[EnumAxis.XFL]) *
                                                  Math.Cos(-encoderPositionData.NowTurnEncoder[EnumAxis.TFL] * Math.PI / 180) +
                                 (encoderPositionData.NowMoveEncoder[EnumAxis.XFR] - encoderPositionData.LastMoveEncoder[EnumAxis.XFR]) *
                                                  Math.Cos(-encoderPositionData.NowTurnEncoder[EnumAxis.TFR] * Math.PI / 180) -
                                 (encoderPositionData.NowMoveEncoder[EnumAxis.XRL] - encoderPositionData.LastMoveEncoder[EnumAxis.XRL]) *
                                                  Math.Cos(-encoderPositionData.NowTurnEncoder[EnumAxis.TRL] * Math.PI / 180) -
                                 (encoderPositionData.NowMoveEncoder[EnumAxis.XRR] - encoderPositionData.LastMoveEncoder[EnumAxis.XRR]) *
                                                  Math.Cos(-encoderPositionData.NowTurnEncoder[EnumAxis.TRR] * Math.PI / 180)) / 2;

                double deltaY = ((encoderPositionData.NowMoveEncoder[EnumAxis.XFL] - encoderPositionData.LastMoveEncoder[EnumAxis.XFL]) *
                                                  Math.Sin(-encoderPositionData.NowTurnEncoder[EnumAxis.TFL] * Math.PI / 180) +
                                 (encoderPositionData.NowMoveEncoder[EnumAxis.XFR] - encoderPositionData.LastMoveEncoder[EnumAxis.XFR]) *
                                                  Math.Sin(-encoderPositionData.NowTurnEncoder[EnumAxis.TFR] * Math.PI / 180) -
                                 (encoderPositionData.NowMoveEncoder[EnumAxis.XRL] - encoderPositionData.LastMoveEncoder[EnumAxis.XRL]) *
                                                  Math.Sin(-encoderPositionData.NowTurnEncoder[EnumAxis.TRL] * Math.PI / 180) -
                                 (encoderPositionData.NowMoveEncoder[EnumAxis.XRR] - encoderPositionData.LastMoveEncoder[EnumAxis.XRR]) *
                                                  Math.Sin(-encoderPositionData.NowTurnEncoder[EnumAxis.TRR] * Math.PI / 180)) / 2;

                double deltaTheta;
                if (deltaX == 0)
                {
                    if (deltaY < 0)
                        deltaTheta = 90;
                    else if (deltaY > 0)
                        deltaTheta = -90;
                    else
                        deltaTheta = 0;
                }
                else
                    deltaTheta = -Math.Atan(deltaY / deltaX) * 180 / Math.PI;

                tempAGVPosition.AGVAngle = location.Encoder.AGVAngle + deltaTheta;
                //location.Encoder.AGVAngle += deltaTheta;

                deltaX = ((encoderPositionData.NowMoveEncoder[EnumAxis.XFL] - encoderPositionData.LastMoveEncoder[EnumAxis.XFL]) *
                           Math.Cos(-(encoderPositionData.NowTurnEncoder[EnumAxis.TFL] + location.Encoder.AGVAngle) * Math.PI / 180) +
                          (encoderPositionData.NowMoveEncoder[EnumAxis.XFR] - encoderPositionData.LastMoveEncoder[EnumAxis.XFR]) *
                           Math.Cos(-(encoderPositionData.NowTurnEncoder[EnumAxis.TFR] + location.Encoder.AGVAngle) * Math.PI / 180) +
                          (encoderPositionData.NowMoveEncoder[EnumAxis.XRL] - encoderPositionData.LastMoveEncoder[EnumAxis.XRL]) *
                           Math.Cos(-(encoderPositionData.NowTurnEncoder[EnumAxis.TRL] + location.Encoder.AGVAngle) * Math.PI / 180) +
                          (encoderPositionData.NowMoveEncoder[EnumAxis.XRR] - encoderPositionData.LastMoveEncoder[EnumAxis.XRR]) *
                           Math.Cos(-(encoderPositionData.NowTurnEncoder[EnumAxis.TRR] + location.Encoder.AGVAngle) * Math.PI / 180)) / 4;


                deltaY = ((encoderPositionData.NowMoveEncoder[EnumAxis.XFL] - encoderPositionData.LastMoveEncoder[EnumAxis.XFL]) *
                           Math.Sin(-(encoderPositionData.NowTurnEncoder[EnumAxis.TFL] + location.Encoder.AGVAngle) * Math.PI / 180) +
                          (encoderPositionData.NowMoveEncoder[EnumAxis.XFR] - encoderPositionData.LastMoveEncoder[EnumAxis.XFR]) *
                           Math.Sin(-(encoderPositionData.NowTurnEncoder[EnumAxis.TFR] + location.Encoder.AGVAngle) * Math.PI / 180) +
                          (encoderPositionData.NowMoveEncoder[EnumAxis.XRL] - encoderPositionData.LastMoveEncoder[EnumAxis.XRL]) *
                           Math.Sin(-(encoderPositionData.NowTurnEncoder[EnumAxis.TRL] + location.Encoder.AGVAngle) * Math.PI / 180) +
                          (encoderPositionData.NowMoveEncoder[EnumAxis.XRR] - encoderPositionData.LastMoveEncoder[EnumAxis.XRR]) *
                           Math.Sin(-(encoderPositionData.NowTurnEncoder[EnumAxis.TRR] + location.Encoder.AGVAngle) * Math.PI / 180)) / 4;

                tempAGVPosition.Position = new MapPosition(
                location.Encoder.Position.X + deltaX,
                location.Encoder.Position.Y + deltaY);
                //location.Encoder.Position.X += deltaX;
                //location.Encoder.Position.Y += deltaY;
                location.Encoder = tempAGVPosition;
            }
        }

        private void AssignEncoderPosition()
        {
            return;
            if (!SimulationMode)
            {
                double deltaTime = ((DateTime.Now - location.Barcode.GetDataTime).TotalMilliseconds + location.Barcode.ScanTime) / 1000;
                double moveAngle = location.Encoder.AGVAngle + ControlData.WheelAngle;

                location.Encoder = location.Barcode;

                location.Encoder.Position.X += location.XFLVelocity * deltaTime * Math.Cos(-moveAngle * Math.PI / 180);
                location.Encoder.Position.Y += location.XFLVelocity * deltaTime * Math.Sin(-moveAngle * Math.PI / 180);
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

            if (safetyData.IsTurnOut && moveControlConfig.Safety[EnumMoveControlSafetyType.TurnOut].Enable)
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
                    }
                }
            }

            if (safetyData.NowMoveState == EnumMoveState.Moving &&
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
                    }
                }
            }


            safetyData.LastMoveState = safetyData.NowMoveState;
        }

        private void UpdatePosition()
        {
            UpdateElmo();
            UpdateEncoderPositionNowEncoder();
            bool newBarcodeData = UpdateSR2000();

            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
            {
                UpdateEncoderPosition();

                if (MoveState != EnumMoveState.TR && MoveState != EnumMoveState.R2000 && newBarcodeData)
                    UpdateDelta();

                UpdateReal();
                SafetyTurnOutAndLineBarcodeInterval(newBarcodeData);
            }
            else
            {
                if (location.Real == null && newBarcodeData)
                {
                    location.Real = location.Barcode;
                    location.Real.AGVAngle = GetAGVAngle(location.Real.AGVAngle);
                    Vehicle.Instance.theVehiclePosition.RealPosition = location.Real.Position;
                    Vehicle.Instance.theVehiclePosition.VehicleAngle = location.Real.AGVAngle;
                }
            }
        }
        #endregion

        #region CommandControl
        private void TRControl(int wheelAngle, EnumAddressAction type)
        {
            double velocity = moveControlConfig.TurnParameter[type].Velocity;
            double r = moveControlConfig.TurnParameter[type].R;
            double safetyVelocityRange = moveControlConfig.TurnParameter[type].SafetyVelocityRange;

            WriteLog("MoveControl", "7", device, "", "start, velocity : " + velocity.ToString("0") + ", r : " + r.ToString("0") +
                ", 舵輪將旋轉至 " + wheelAngle.ToString("0") + "度!");
            MoveState = EnumMoveState.TR;

            double xFLVelocity = Math.Abs(location.XFLVelocity);
            double xRRVelocity = Math.Abs(location.XRRVelocity);
            double startEncoder = location.ElmoEncoder;
            double distance = r * 2;


            if (Math.Abs(xFLVelocity - velocity) <= safetyVelocityRange &&
                Math.Abs(xRRVelocity - velocity) <= safetyVelocityRange)
            { // Normal
                if (!elmoDriver.MoveCompelete(EnumAxis.GT))
                    WriteLog("MoveControl", "4", device, "", " TR中 GT Moving~");

                elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.TurnParameter[type].AxisParameter.Velocity, EnumMoveType.Absolute,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Acceleration,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Deceleration,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Jerk);
            }
            else if (Math.Abs(xFLVelocity - xRRVelocity) > 2 * safetyVelocityRange)
            { // GG,不該發生.
                EMSControl("左前輪速度和右後輪速度差異過大, XFL vel : " + xFLVelocity.ToString("0") +
                                                         ", XRR vel : " + xRRVelocity.ToString("0"));
            }
            else if (xFLVelocity > velocity && xRRVelocity > velocity)
            { // 超速GG, 不該發生.
                EMSControl("超速.., XFL vel : " + xFLVelocity.ToString("0") +
                                                         ", XRR vel : " + xRRVelocity.ToString("0"));
            }
            else
            { // 太慢 處理??
                EMSControl("速度過慢.., XFL vel : " + xFLVelocity.ToString("0") +
                                                         ", XRR vel : " + xRRVelocity.ToString("0"));
            }

            while (!elmoDriver.WheelAngleCompare(wheelAngle, moveControlConfig.StartWheelAngleRange) && !SimulationMode)
            {
                // 過半保護.
                UpdatePosition();
                SensorSafety();

                if (ControlData.MoveControlStop)
                {
                    StopControl();
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            indexOflisSectionLine++;

            switch (wheelAngle)
            {
                case 0:
                    BeamSensorOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back));
                    break;
                case 90:
                    BeamSensorOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                    break;
                case -90:
                    BeamSensorOnlyOn((ControlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
                    break;
                default:
                    EMSControl("switch (wheelAngle) default..EMS..");
                    break;
            }

            ControlData.WheelAngle = wheelAngle;
            location.Delta = location.Delta + (ControlData.DirFlag ? (distance - (location.ElmoEncoder - startEncoder)) :
                                               -(distance - (startEncoder - location.ElmoEncoder)));
            UpdatePosition();

            MoveState = EnumMoveState.Moving;
            WriteLog("MoveControl", "7", device, "", "end.");
        }

        private bool OkToTurnZero(double outerWheelEncoder, double trunZeroEncoder)
        {
            if (ControlData.DirFlag)
                return outerWheelEncoder >= trunZeroEncoder;
            else
                return outerWheelEncoder <= trunZeroEncoder;
        }

        private void R2000StopAndSetPosition()
        {
            if (CommandList[IndexOfCmdList + 1].CmdType == EnumCommandType.SlowStop &&
                CommandList[IndexOfCmdList + 1].Position == null)
            {
                r2000SlowStop = true;
            }
            else
            {
                elmoDriver.ElmoStop(EnumAxis.GX);
                while (!elmoDriver.MoveCompelete(EnumAxis.GX))
                {
                    UpdatePosition();
                    Thread.Sleep(moveControlConfig.SleepTime);
                }

                UpdatePosition();
                elmoDriver.SetMoveAxisPosition();
                location.Offset = location.RealEncoder - location.Delta - elmoDriver.ElmoGetPosition(EnumAxis.XFL);
                // XFL + offset + delta = real
                double distance = ControlData.TrigetEndEncoder - location.RealEncoder;
                elmoDriver.ElmoMove(EnumAxis.GX, distance, moveControlConfig.Move.Velocity, EnumMoveType.Relative,
                    moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            }
        }

        public void R2000Control(int wheelAngle)
        {
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
            indexOflisSectionLine++;

            double velocity = moveControlConfig.TurnParameter[EnumAddressAction.R2000].Velocity;
            double safetyVelocityRange = moveControlConfig.TurnParameter[EnumAddressAction.R2000].SafetyVelocityRange;
            double xFLVelocity = Math.Abs(location.XFLVelocity);
            double xRRVelocity = Math.Abs(location.XRRVelocity);

            if (Math.Abs(xFLVelocity - velocity) <= safetyVelocityRange &&
                Math.Abs(xRRVelocity - velocity) <= safetyVelocityRange)
            { // Normal
            }
            else if (Math.Abs(xFLVelocity - xRRVelocity) > 2 * safetyVelocityRange)
            { // GG,不該發生.
                EMSControl("左前輪速度和右後輪速度差異過大, XFL vel : " + xFLVelocity.ToString("0") +
                                                         ", XRR vel : " + xRRVelocity.ToString("0"));
                return;
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

            while (!OkToTurnZero(outerWheelEncoder, trunZeroEncoder) && !SimulationMode)
            {
                if (wheelAngle == 1)
                    outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.XFL, true);
                else if (wheelAngle == -1)
                    outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.XFR, true);

                UpdatePosition();
                SensorSafety();

                if (ControlData.MoveControlStop)
                {
                    StopControl();
                    return;
                }

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
                if (ControlData.MoveControlStop)
                {
                    StopControl();
                    return;
                }

                UpdatePosition();
                SensorSafety();

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            location.Delta = location.Delta + (ControlData.DirFlag ? (distance - (location.ElmoEncoder - startEncoder)) :
                                               -(distance - (startEncoder - location.ElmoEncoder)));

            //R2000StopAndSetPosition();
            indexOflisSectionLine++;
            UpdatePosition();
            if ((wheelAngle == -1 && ControlData.DirFlag) ||
                (wheelAngle == 1 && !ControlData.DirFlag))
                location.Real.AGVAngle -= 90;
            else
                location.Real.AGVAngle += 90;

            Vehicle.Instance.theVehiclePosition.VehicleAngle = location.Real.AGVAngle;

            MoveState = EnumMoveState.Moving;
            WriteLog("MoveControl", "7", device, "", " end.");
        }

        private void VchangeControl(double velocity, EnumVChangeType vChangeType, int TRWheelAngle = 0)
        {
            WriteLog("MoveControl", "7", device, "", " start, Velocity : " + velocity.ToString("0"));

            if (velocity != ControlData.RealVelocity)
            {
                if (vChangeType != EnumVChangeType.SensorSlow)
                    ControlData.VelocityCommand = velocity;

                if (!ControlData.SensorSlow || velocity <= moveControlConfig.LowVelocity)
                {
                    ControlData.RealVelocity = velocity;
                    agvRevise.SettingReviseData(velocity, ControlData.DirFlag);
                    velocity /= moveControlConfig.Move.Velocity;
                    elmoDriver.ElmoGroupVelocityChange(EnumAxis.GX, velocity);
                }
            }

            if (vChangeType == EnumVChangeType.TRTurn)
            {
                switch (TRWheelAngle)
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
                while (!elmoDriver.WheelAngleCompare(wheelAngle, 1) && !SimulationMode)
                {
                    UpdatePosition();
                    if (timer.ElapsedMilliseconds > moveControlConfig.TurnTimeoutValue)
                    {
                        WriteLog("MoveControl", "4", device, "", "舵輪旋轉Timeout!");
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
                    break;
                case 90: // 朝左.
                    BeamSensorSingleOn((dirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right));
                    break;
                case -90: // 朝右.
                    BeamSensorSingleOn((dirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left));
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
                    Thread.Sleep(moveControlConfig.SleepTime);
                }

                timer.Reset();
                timer.Start();
                while (elmoDriver.ElmoGetDisable(EnumAxis.GX) && !SimulationMode)
                {
                    UpdatePosition();
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
                indexOflisSectionLine++;
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

            if (!elmoDriver.MoveCompelete(EnumAxis.GX))
                elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Reset();
            timer.Start();
            while (!elmoDriver.MoveCompelete(EnumAxis.GX))
            {
                UpdatePosition();
                if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue)
                {
                    EMSControl("SlowStop Timeout!");
                    return;
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            BeamSensorCloseAll();

            //if (r2000SlowStop)
            //{
            //    r2000SlowStop = false;
            //    elmoDriver.SetMoveAxisPosition();
            //}

            if (nextReserveIndex == -1)
            {
                for (int i = IndexOfCmdList + 1; i < CommandList.Count; i++)
                {
                    if (CommandList[i].CmdType == EnumCommandType.Move)
                    {
                        ControlData.DirFlag = CommandList[i].DirFlag;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 2; i <= 3; i++)
                {
                    if (IndexOfCmdList + i < CommandList.Count)
                    {
                        if (CommandList[IndexOfCmdList + i].Position != null)
                        {
                            if ((ControlData.DirFlag && location.RealEncoder > CommandList[IndexOfCmdList + i].TriggerEncoder) ||
                               (!ControlData.DirFlag && location.RealEncoder < CommandList[IndexOfCmdList + i].TriggerEncoder))
                            {
                                WriteLog("MoveControl", "7", device, "", "Reserve Stop, 由於action : " + CommandList[IndexOfCmdList + i].CmdType.ToString() +
                                                                     "的觸發點已超過目前位置,更改為立即觸發!");
                                CommandList[IndexOfCmdList + i].Position = null;
                            }
                        }
                    }
                }

                double distance = ControlData.TrigetEndEncoder - location.RealEncoder;

                Command temp = createMoveControlList.NewMoveCommand(location.Real.Position, location.RealEncoder, Math.Abs(distance),
                    moveControlConfig.Move.Velocity, ControlData.DirFlag, 0, EnumMoveStartType.ReserveStopMove, nextReserveIndex);
                CommandList.Insert(IndexOfCmdList + 1, temp);

                if (ControlData.VelocityCommand != moveControlConfig.Move.Velocity)
                {
                    temp = createMoveControlList.NewVChangeCommand(null, 0, ControlData.VelocityCommand, ControlData.DirFlag);
                    CommandList.Insert(IndexOfCmdList + 2, temp);
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
                elmoDriver.ElmoMove(EnumAxis.GX, endEncoder - location.RealEncoder, moveControlConfig.EQVelocity, EnumMoveType.Relative,
                                    moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Reset();
                timer.Start();

                Thread.Sleep(moveControlConfig.SleepTime * 2);
                while (!elmoDriver.MoveCompelete(EnumAxis.GX))
                {
                    UpdatePosition();

                    if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue)
                    {
                        EMSControl("SecondCorrectionControl Timeout!");
                        return;
                    }

                    Thread.Sleep(moveControlConfig.SleepTime);
                }
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

            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void EMSControl(string emsResult)
        {
            WriteLog("MoveControl", "7", device, "", "start");
            AGVStopResult = emsResult;
            WriteLog("MoveControl", "7", device, "", "EMS Stop : " + emsResult);

            elmoDriver.ElmoStop(EnumAxis.GX);
            elmoDriver.ElmoStop(EnumAxis.GX);
            //elmoDriver.DisableAllAxis();
            MoveState = EnumMoveState.Error;
            MoveFinished(EnumMoveComplete.Fail);
            simulationIsMoving = false;
            BeamSensorCloseAll();
            WriteLog("MoveControl", "7", device, "", "end");
        }
        #endregion

        #region 檢查觸發.
        private bool CheckGetNextReserve(Command cmd)
        {
            if (cmd.NextRserveCancel)
            {
                if (cmd.NextReserveNumber < ReserveList.Count)
                {
                    if (ReserveList[cmd.NextReserveNumber].GetReserve)
                    {
                        IndexOfCmdList++;
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
            if (cmd.ReserveNumber < ReserveList.Count && (cmd.ReserveNumber == -1 || ReserveList[cmd.ReserveNumber].GetReserve))
            {
                WaitReseveIndex = -1;

                if (cmd.Position == null)
                {
                    if (CheckGetNextReserve(cmd))
                    {
                        WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 觸發,為為立即觸發");
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
            if (CommandList.Count != 0 && IndexOfCmdList < CommandList.Count && TriggerCommand(CommandList[IndexOfCmdList]) && !ControlData.SensorStop)
            {
                WriteLog("MoveControl", "7", device, "", "Barcode Position ( " + location.Barcode.Position.X.ToString("0") + ", " + location.Barcode.Position.Y.ToString("0") +
                                                         " ), Real Position ( " + location.Real.Position.X.ToString("0") + ", " + location.Real.Position.Y.ToString("0") +
                                                         " ), Encoder Position ( " + location.Encoder.Position.X.ToString("0") + ", " + location.Encoder.Position.Y.ToString("0") + " )");

                switch (CommandList[IndexOfCmdList].CmdType)
                {
                    case EnumCommandType.TR:
                        TRControl(CommandList[IndexOfCmdList].WheelAngle, CommandList[IndexOfCmdList].TurnType);
                        break;
                    case EnumCommandType.R2000:
                        R2000Control(CommandList[IndexOfCmdList].WheelAngle);
                        break;
                    case EnumCommandType.Vchange:
                        VchangeControl(CommandList[IndexOfCmdList].Velocity, CommandList[IndexOfCmdList].VChangeType, CommandList[IndexOfCmdList].WheelAngle);
                        break;
                    case EnumCommandType.ReviseOpen:
                        if (ControlData.OntimeReviseFlag == false)
                        {
                            agvRevise.SettingReviseData(ControlData.VelocityCommand, ControlData.DirFlag);
                            ControlData.OntimeReviseFlag = true;
                        }

                        break;
                    case EnumCommandType.ReviseClose:
                        ControlData.OntimeReviseFlag = false;
                        if (CommandList[IndexOfCmdList].TurnType == EnumAddressAction.R2000)
                            elmoDriver.ElmoMove(EnumAxis.GT, 0, 20, EnumMoveType.Absolute);
                        else
                            elmoDriver.ElmoStop(EnumAxis.GT);
                        break;
                    case EnumCommandType.Move:
                        MoveCommandControl(CommandList[IndexOfCmdList].Velocity, CommandList[IndexOfCmdList].Distance, CommandList[IndexOfCmdList].DirFlag,
                                           CommandList[IndexOfCmdList].WheelAngle, CommandList[IndexOfCmdList].MoveType);
                        break;
                    case EnumCommandType.SlowStop:
                        SlowStopControl(CommandList[IndexOfCmdList].EndPosition, CommandList[IndexOfCmdList].NextReserveNumber);
                        break;
                    case EnumCommandType.End:
                        SecondCorrectionControl(CommandList[IndexOfCmdList].EndEncoder);
                        break;
                    case EnumCommandType.Stop:
                        StopControl();
                        break;
                    default:
                        break;
                }

                IndexOfCmdList++;
                ExecuteCommandList();
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

                    if (ControlData.OntimeReviseFlag && !ControlData.MoveControlStop && MoveState == EnumMoveState.Moving)
                    {
                        ontimeReviseEMSMessage = "";

                        //if (agvRevise.OntimeRevise(ref reviseWheelAngle, ControlData.WheelAngle, ref ontimeReviseEMSMessage))
                        if (agvRevise.OntimeReviseWithVelocity(ref reviseWheelAngle, ControlData.WheelAngle, location.XFLVelocity, ref ontimeReviseEMSMessage))
                        {
                            if (ontimeReviseEMSMessage != "")
                            {
                                EMSControl(ontimeReviseEMSMessage);
                                AGVStopResult = ontimeReviseEMSMessage;
                            }
                            else
                            {
                                elmoDriver.ElmoMove(EnumAxis.GT, reviseWheelAngle[0], reviseWheelAngle[1], reviseWheelAngle[2], reviseWheelAngle[3],
                                                  ontimeReviseConfig.ThetaSpeed, EnumMoveType.Absolute, moveControlConfig.Turn.Acceleration,
                                                  moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);
                            }
                        }
                    }

                    if (ControlData.MoveControlStop)
                    {
                        StopControl();
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
                return Vehicle.Instance.GetPlcVehicle().Batterys.Charging;
            else
                return false;
        }

        public bool ForkNotHome()
        {
            if (SimulationMode)
                return false;

            if (moveControlConfig.SensorByPass[EnumSensorSafetyType.ForkHome].Enable)
                return !Vehicle.Instance.GetPlcVehicle().Robot.ForkHome || Vehicle.Instance.GetPlcVehicle().Robot.ForkBusy;
            else
                return false;
        }

        private bool GetBumperState()
        {
            if (SimulationMode)
                return false;

            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.Bumper].Enable)
                return false;

            return Vehicle.Instance.GetPlcVehicle().BumperAlarmStatus;
        }

        private EnumVehicleSafetyAction GetBeamSensorState()
        {
            if (SimulationMode)
                return EnumVehicleSafetyAction.Normal;

            if (!moveControlConfig.SensorByPass[EnumSensorSafetyType.BeamSensor].Enable)
                return EnumVehicleSafetyAction.Normal;

            return Vehicle.Instance.GetPlcVehicle().VehicleSafetyAction;
        }

        private bool CheckNextCommandTrigger()
        {
            if (ControlData.DirFlag)
            {
                return location.RealEncoder > CommandList[IndexOfCmdList + 1].TriggerEncoder;
            }
            else
            {
                return location.RealEncoder < CommandList[IndexOfCmdList + 1].TriggerEncoder;
            }
        }

        private void SensorStartMove(EnumVehicleSafetyAction nowAction)
        {
            /// 狀況1. 下一步是SlowStop type : Reserve Stop)
            ///  不做任何事情.
            /// 狀況2. 下一步是SlowStop 且接反折
            /// 在反折啟動範圍內 : 不做任何事情, 在範圍外 : 當二次修正的方式移動過去
            /// 狀況3. 二修 : 不做事情.
            /// 狀況4. 其餘 : 照常運行
            if (CommandList[IndexOfCmdList + 1].CmdType == EnumCommandType.SlowStop && CheckNextCommandTrigger())
            {
                if (CommandList[IndexOfCmdList + 1].NextReserveNumber != -1)
                {
                    // 狀況1. do nothing
                }
                else if (CommandList[IndexOfCmdList + 2].CmdType == EnumCommandType.End)
                {
                    // 狀況3. do nothing
                }
                else if (CommandList[IndexOfCmdList + 2].CmdType == EnumCommandType.Move)
                {
                    // 狀況2.
                    bool needMove = true;
                    double targetEncoder = 0;
                    if (CommandList[IndexOfCmdList + 2].DirFlag)
                    {
                        if (location.RealEncoder > CommandList[IndexOfCmdList + 2].TriggerEncoder &&
                            location.RealEncoder < CommandList[IndexOfCmdList + 2].TriggerEncoder + CommandList[IndexOfCmdList + 2].SafetyDistance)
                            needMove = false;
                    }
                    else
                    {
                        if (location.RealEncoder < CommandList[IndexOfCmdList + 2].TriggerEncoder &&
                            location.RealEncoder > CommandList[IndexOfCmdList + 2].TriggerEncoder - CommandList[IndexOfCmdList + 2].SafetyDistance)
                            needMove = false;
                    }

                    if (needMove)
                    {
                        targetEncoder = CommandList[IndexOfCmdList + 2].TriggerEncoder +
                            (CommandList[IndexOfCmdList + 2].DirFlag ? CommandList[IndexOfCmdList + 2].SafetyDistance / 2 :
                                                                      -CommandList[IndexOfCmdList + 2].SafetyDistance / 2);

                        elmoDriver.ElmoMove(EnumAxis.GX, targetEncoder - location.RealEncoder, moveControlConfig.EQVelocity, EnumMoveType.Relative,
                             moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

                        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                        timer.Reset();
                        timer.Start();

                        Thread.Sleep(moveControlConfig.SleepTime * 2);
                        while (!elmoDriver.MoveCompelete(EnumAxis.GX))
                        {
                            UpdatePosition();
                            Thread.Sleep(moveControlConfig.SleepTime);

                            if (timer.ElapsedMilliseconds > moveControlConfig.SlowStopTimeoutValue)
                            {
                                EMSControl("SecondCorrectionControl Timeout!");
                                return;
                            }
                        }
                    }
                }
                else
                {
                    EMSControl("Sensor再啟動出現奇怪地方.");
                }
            }
            else
            {
                double distance = ControlData.TrigetEndEncoder - location.RealEncoder;

                Command temp = createMoveControlList.NewMoveCommand(null, 0, Math.Abs(distance),
                    moveControlConfig.Move.Velocity, ControlData.DirFlag, 0, EnumMoveStartType.SensorStopMove);
                CommandList.Insert(IndexOfCmdList, temp);

                if (nowAction == EnumVehicleSafetyAction.Normal || ControlData.VelocityCommand < moveControlConfig.LowVelocity)
                    temp = createMoveControlList.NewVChangeCommand(null, 0, ControlData.VelocityCommand, ControlData.DirFlag);
                else
                    temp = createMoveControlList.NewVChangeCommand(null, 0, moveControlConfig.LowVelocity, ControlData.DirFlag, EnumVChangeType.SensorSlow);

                CommandList.Insert(IndexOfCmdList + 1, temp);
            }
        }

        private void SensorActionToNormal()
        {
            switch (ControlData.SensorState)
            {
                case EnumVehicleSafetyAction.LowSpeed:
                    // 加入升速.
                    if (ControlData.VelocityCommand > moveControlConfig.LowVelocity && !elmoDriver.MoveCompelete(EnumAxis.GX))
                    {
                        Command temp = createMoveControlList.NewVChangeCommand(null, 0, ControlData.VelocityCommand, ControlData.DirFlag);
                        CommandList.Insert(IndexOfCmdList, temp);
                    }

                    break;
                case EnumVehicleSafetyAction.Stop:
                    // 加入啟動.
                    if (MoveState == EnumMoveState.R2000 || MoveState == EnumMoveState.TR)
                    {
                        EMSControl("暫時Bypass TR、R2000中啟動!");
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
                        Command temp = createMoveControlList.NewVChangeCommand(null, 0, moveControlConfig.LowVelocity, ControlData.DirFlag, EnumVChangeType.SensorSlow);
                        CommandList.Insert(IndexOfCmdList, temp);
                    }

                    break;
                case EnumVehicleSafetyAction.Stop:
                    // 加入啟動且降速.
                    if (MoveState == EnumMoveState.R2000 || MoveState == EnumMoveState.TR)
                    {
                        EMSControl("暫時Bypass TR、R2000中啟動!");
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
                    SlowStopControl(null, -1);
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

        private void SensorSafety()
        {
            if (MoveState == EnumMoveState.Idle || MoveState == EnumMoveState.Error)
                return;

            if (IsCharging())
            {
                EMSControl("走行中出現Charging訊號!");
            }
            else if (ForkNotHome())
            {
                EMSControl("走行中Fork不在Home點!");
            }

            if (ControlData.FlowStopRequeset)
            {
                ControlData.MoveControlStop = true;
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
                    Vehicle.Instance.GetPlcVehicle().MoveFront = true;
                    break;
                case EnumBeamSensorLocate.Back:
                    Vehicle.Instance.GetPlcVehicle().MoveBack = true;
                    break;
                case EnumBeamSensorLocate.Left:
                    Vehicle.Instance.GetPlcVehicle().MoveLeft = true;
                    break;
                case EnumBeamSensorLocate.Right:
                    Vehicle.Instance.GetPlcVehicle().MoveRight = true;
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

            Vehicle.Instance.GetPlcVehicle().MoveFront = front;
            Vehicle.Instance.GetPlcVehicle().MoveBack = back;
            Vehicle.Instance.GetPlcVehicle().MoveLeft = left;
            Vehicle.Instance.GetPlcVehicle().MoveRight = right;

            WriteLog("MoveControl", "7", device, "", "Beam sensor 切換 : 只剩 " + locate.ToString() + " On !");
        }

        private void BeamSensorCloseAll()
        {
            Vehicle.Instance.GetPlcVehicle().MoveFront = false;
            Vehicle.Instance.GetPlcVehicle().MoveBack = false;
            Vehicle.Instance.GetPlcVehicle().MoveLeft = false;
            Vehicle.Instance.GetPlcVehicle().MoveRight = false;

            WriteLog("MoveControl", "7", device, "", "Beam sensor 切換 : 全部關掉!");
        }
        #endregion

        #region 外部連結 : 產生List、DebugForm相關、狀態、移動完成.
        /// <summary>
        ///  when move finished, call this function to notice other class instance that move is finished with status
        /// </summary>
        public void MoveFinished(EnumMoveComplete status)
        {
            if (isAGVMCommand)
                OnMoveFinished?.Invoke(this, status);
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
            UpdateEncoderPositionNowEncoder();
            location.Delta = 0;
            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void ResetFlag()
        {
            indexOflisSectionLine = 0;
            IndexOfCmdList = 0;
            r2000SlowStop = false;
            ControlData.SensorState = EnumVehicleSafetyAction.Normal;
            ControlData.SensorStop = false;
            ControlData.SensorSlow = false;
            ControlData.FlowStopRequeset = false;
            ControlData.FlowStop = false;
            ControlData.FlowClear = false;
            safetyData = new MoveControlSafetyData();
            AGVStopResult = "";
            WaitReseveIndex = -1;

            ResetEncoder(SectionLineList[0].Start, SectionLineList[0].End, SectionLineList[0].DirFlag);
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

            if (IsCharging())
            {
                errorMessage = "Charging中";
                return false;
            }
            else if (ForkNotHome())
            {
                errorMessage = "Fork不在Home點";
                return false;
            }

            List<Command> moveCmdList = new List<Command>();
            List<SectionLine> sectionLineList = new List<SectionLine>();
            List<ReserveData> reserveDataList = new List<ReserveData>();

            if (!createMoveControlList.CreateMoveControlListSectionListReserveList(moveCmd, ref moveCmdList, ref sectionLineList, ref reserveDataList,
                                                                                  location.Real, ControlData.WheelAngle, ref errorMessage))
            {
                WriteLog("MoveControl", "7", device, "", "命令分解失敗~!, errorMessage : " + errorMessage);
                return false;
            }

            ReserveList = reserveDataList;
            SectionLineList = sectionLineList;
            CommandList = moveCmdList;
            MoveCommandID = moveCmd.CmdId;
            isAGVMCommand = true;

            ResetFlag();

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

        public bool CreateMoveControlListSectionListReserveList(MoveCmdInfo moveCmd, ref List<Command> moveCmdList, ref List<SectionLine> sectionLineList,
                                                               ref List<ReserveData> reserveDataList, AGVPosition nowAGV, ref string errorMessage)
        {
            if (Vehicle.Instance.AutoState == EnumAutoState.Auto)
            {
                errorMessage = "Auto Mode,拒絕Debug Form命令.";
                return false;
            }

            if (IsCharging())
            {
                errorMessage = "Charging中";
                return false;
            }
            else if (ForkNotHome())
            {
                errorMessage = "Fork不在Home點";
                return false;
            }

            if (createMoveControlList.CreateMoveControlListSectionListReserveList(moveCmd, ref moveCmdList, ref sectionLineList,
                ref reserveDataList, nowAGV, ControlData.WheelAngle, ref errorMessage))
            {
                return true;
            }
            else
                return false;
        }

        public void GetMoveCommandListInfo(List<Command> moveCmdList, ref List<string> logMessage)
        {
            createMoveControlList.GetMoveCommandListInfo(moveCmdList, ref logMessage);
        }

        public bool TurnOutSafetyDistance
        {
            get
            {
                return createMoveControlList.TurnOutSafetyDistance;
            }
            set
            {
                createMoveControlList.TurnOutSafetyDistance = value;
            }
        }

        public bool TransferMoveDebugMode(List<Command> moveCmdList, List<SectionLine> sectionLineList, List<ReserveData> reserveDataList)
        {
            if (Vehicle.Instance.AutoState == EnumAutoState.Auto)
                return false;

            WriteLog("MoveControl", "7", device, "", "start");
            ReserveList = reserveDataList;
            SectionLineList = sectionLineList;
            CommandList = moveCmdList;
            MoveCommandID = "DebugForm" + DateTime.Now.ToString("HH:mm:ss");
            isAGVMCommand = false;

            ResetFlag();

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
            for (int i = 0; i < ReserveList.Count; i++)
            {
                if (mapPosition.X == ReserveList[i].Position.X && mapPosition.Y == ReserveList[i].Position.Y)
                {
                    ReserveList[i].GetReserve = true;
                    WriteLog("MoveControl", "7", device, "", "取得Reserve node : index = " + i.ToString() +
                             "( " + mapPosition.X.ToString("0") + ", " + mapPosition.Y.ToString() + " ) !");
                    return true;
                }
                else if (!ReserveList[i].GetReserve)
                    return false;
            }

            return false;
        }

        public void AddReservedIndexForDebugModeTest(int index)
        {
            if (ReserveList == null || MoveState == EnumMoveState.Idle)
                return;

            if (index >= 0 && index < ReserveList.Count)
            {
                for (int i = 0; i <= index; i++)
                {
                    if (!ReserveList[i].GetReserve)
                        AddReservedMapPosition(ReserveList[i].Position);
                }
            }
        }

        public int GetReserveIndex()
        {
            int count = -1;

            if (ReserveList == null)
                return count;

            for (int i = 0; i < ReserveList.Count; i++)
            {
                if (ReserveList[i].GetReserve)
                    count++;
                else
                    return count;
            }

            return count;
        }
        #endregion

        #region DebugForm Admin


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
                if (MoveState != EnumMoveState.Idle && IndexOfCmdList < CommandList.Count)
                {
                    AddCSV(ref csvLog, CommandList[IndexOfCmdList].CmdType.ToString());

                    if (CommandList[IndexOfCmdList].Position != null)
                        AddCSV(ref csvLog, CommandList[IndexOfCmdList].TriggerEncoder.ToString("0"));
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
