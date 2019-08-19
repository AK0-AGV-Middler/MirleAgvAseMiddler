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
        public EnumMoveState MoveState { get; private set; }
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
        private int indexOfCmdList = 0;

        private MoveControlParameter controlData = new MoveControlParameter();
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

        public bool DebugFlowMode { get; set; } = false;
        public string AGVStopResult { get; set; }
        private const int debugFlowLogMaxLength = 10000;

        public string DebugFlowLog { get; set; }

        private bool r2000SlowStop = false;

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

        public MoveControlHandler(string moveControlConfigPath, MapInfo theMapInfo)
        {
            moveControlConfigPath = "MoveControlConfig.xml";
            this.theMapInfo = theMapInfo;
            ReadMoveControlConfigXML(moveControlConfigPath);

            InitailSr2000(moveControlConfig.Sr2000ConfigPath);
            elmoDriver = new ElmoDriver(moveControlConfig.ElmoConfigPath);

            ReadOntimeReviseConfigXML(moveControlConfig.OnTimeReviseConfigPath);
            createMoveControlList = new CreateMoveControlList(DriverSr2000List, moveControlConfig);

            agvRevise = new AgvMoveRevise(ontimeReviseConfig, elmoDriver, DriverSr2000List, moveControlConfig.Safety);

            controlData.MoveControlThread = new Thread(MoveControlThread);
            controlData.MoveControlThread.Start();
            MoveState = EnumMoveState.Idle;

            elmoDriver.ElmoStop(EnumAxis.GX);
            elmoDriver.ElmoMove(EnumAxis.GT, 0, 75, EnumMoveType.Absolute);

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
                    case "TurnSpeedSafetyRange":
                        moveControlConfig.TurnSpeedSafetyRange = double.Parse(item.InnerText);
                        break;
                    case "SafteyDistance":
                        ReadSafetyDistanceXML((XmlElement)item);
                        break;
                    case "Safety":
                        ReadSafetyXML((XmlElement)item);
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
                Vehicle.Instance.AVehiclePosition.RealPosition = location.Real.Position;
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
                    WriteLog("MoveControl", "3", device, "", "UpdateDelta中Barcode讀取位置和Encode(座標)差距" +
                        distance.ToString("0.0") + "mm,已超過安全設置的" +
                        moveControlConfig.Safety[EnumMoveControlSafetyType.UpdateDeltaPositionRange].Range.ToString("0") +
                        "mm,因此啟動EMS! Encoder ( " + location.Encoder.Position.X.ToString("0") + ", " + location.Encoder.Position.Y.ToString("0") +
                        " ), Barcoder ( " + location.Barcode.Position.X.ToString("0") + ", " + location.Barcode.Position.Y.ToString("0") +
                        " ), realEncoder : " + location.RealEncoder.ToString("0"));
                    AGVStopResult = "UpdateDelta中Barcode讀取位置和Encode(座標)差距" +
                        distance.ToString("0.0") + "mm,已超過安全設置的" +
                        moveControlConfig.Safety[EnumMoveControlSafetyType.UpdateDeltaPositionRange].Range.ToString("0") +
                        "mm,因此啟動EMS!";

                    EMSControl();
                }
            }

            //AssignEncoderPosition();
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
                        else if (IsSameAngle(agvPosition.BarcodeAngleInMap, agvPosition.AGVAngle, controlData.WheelAngle))
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
                    location.XFLVelocity = controlData.DirFlag ? controlData.VelocityCommand : -controlData.VelocityCommand;
                    location.XRRVelocity = controlData.DirFlag ? controlData.VelocityCommand : -controlData.VelocityCommand;

                    location.ElmoEncoder = location.ElmoEncoder + 0.005 * location.XFLVelocity;
                }

                location.ElmoGetDataTime = DateTime.Now;
            }
        }

        private void UpdateEncoderPositionNowEncoder()
        {
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
            if (!SimulationMode)
            {
                double deltaTime = ((DateTime.Now - location.Barcode.GetDataTime).TotalMilliseconds + location.Barcode.ScanTime) / 1000;
                double moveAngle = location.Encoder.AGVAngle + controlData.WheelAngle;

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
                        WriteLog("MoveControl", "3", device, "", "出彎" + Math.Abs(location.ElmoEncoder - safetyData.TurnOutElmoEncoder).ToString("0") +
                            "mm未讀取到Barcode,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.TurnOut].Range.ToString("0") +
                            "mm,因此啟動EMS!");
                        AGVStopResult = "出彎" + Math.Abs(location.ElmoEncoder - safetyData.TurnOutElmoEncoder).ToString("0") +
                            "mm未讀取到Barcode,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.TurnOut].Range.ToString("0") +
                            "mm,因此啟動EMS!";

                        EMSControl();
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
                        WriteLog("MoveControl", "3", device, "", "直線超過" + Math.Abs(location.ElmoEncoder - safetyData.LastReadBarcodeElmoEncoder).ToString("0") +
                            "mm未讀取到Barcode,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.LineBarcodeInterval].Range.ToString("0") +
                            "mm,因此啟動EMS!");

                        AGVStopResult = "直線超過" + Math.Abs(location.ElmoEncoder - safetyData.LastReadBarcodeElmoEncoder).ToString("0") +
                            "mm未讀取到Barcode,已超過安全設置的" +
                            moveControlConfig.Safety[EnumMoveControlSafetyType.LineBarcodeInterval].Range.ToString("0") +
                            "mm,因此啟動EMS!";

                        EMSControl();
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
                    Vehicle.Instance.AVehiclePosition.RealPosition = location.Real.Position;
                }
            }
        }
        #endregion

        #region CommandControl
        private void TRControl(int wheelAngle, EnumAddressAction type)
        {
            double velocity = moveControlConfig.TurnParameter[type].Velocity;
            double r = moveControlConfig.TurnParameter[type].R;

            WriteLog("MoveControl", "7", device, "", "start, velocity : " + velocity.ToString("0") + ", r : " + r.ToString("0") +
                ", 舵輪將旋轉至 " + wheelAngle.ToString("0") + "度!");
            MoveState = EnumMoveState.TR;

            double xFLVelocity = Math.Abs(location.XFLVelocity);
            double xRRVelocity = Math.Abs(location.XRRVelocity);
            double startEncoder = location.ElmoEncoder;
            double distance = r * 2;


            if (Math.Abs(xFLVelocity - velocity) <= velocity * moveControlConfig.TurnSpeedSafetyRange &&
                Math.Abs(xRRVelocity - velocity) <= velocity * moveControlConfig.TurnSpeedSafetyRange)
            { // Normal
                if (!elmoDriver.MoveCompelete(EnumAxis.GT))
                    WriteLog("MoveControl", "4", device, "", " TR中 GT Moving~");

                elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.TurnParameter[type].AxisParameter.Velocity, EnumMoveType.Absolute,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Acceleration,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Deceleration,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Jerk);
            }
            else if (Math.Abs(xFLVelocity - xRRVelocity) > 2 * velocity * moveControlConfig.TurnSpeedSafetyRange)
            { // GG,不該發生.
                WriteLog("MoveControl", "4", device, "", "左前輪速度和右後輪速度差異過大, XFL vel : " + xFLVelocity.ToString("0") +
                                                         ", XRR vel : " + xRRVelocity.ToString("0"));
                EMSControl();
            }
            else if (xFLVelocity > velocity && xRRVelocity > velocity)
            { // 超速GG, 不該發生.
                WriteLog("MoveControl", "4", device, "", "超速.., XFL vel : " + xFLVelocity.ToString("0") +
                                                         ", XRR vel : " + xRRVelocity.ToString("0"));
                EMSControl();
            }
            else
            { // 太慢 處理??
                WriteLog("MoveControl", "4", device, "", "速度過慢.., XFL vel : " + xFLVelocity.ToString("0") +
                                                         ", XRR vel : " + xRRVelocity.ToString("0"));
                EMSControl();
            }

            while (!elmoDriver.WheelAngleCompare(wheelAngle, moveControlConfig.StartWheelAngleRange) && !SimulationMode)
            {
                // 過半保護.
                UpdatePosition();

                if (controlData.MoveControlStop)
                {
                    StopControl();
                }

                Thread.Sleep(20);
            }

            indexOflisSectionLine++;

            switch (wheelAngle)
            {
                case 0:
                    BeamSensorOnlyOnOff((controlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back), true);
                    break;
                case 90:
                    BeamSensorOnlyOnOff((controlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right), true);
                    break;
                case -90:
                    BeamSensorOnlyOnOff((controlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left), true);
                    break;
                default:
                    WriteLog("MoveControl", "4", device, "", "switch (wheelAngle) default..EMS..");
                    EMSControl();
                    break;
            }

            controlData.WheelAngle = wheelAngle;
            location.Delta = location.Delta + (controlData.DirFlag ? (distance - (location.ElmoEncoder - startEncoder)) :
                                               -(distance - (startEncoder - location.ElmoEncoder)));
            UpdatePosition();

            MoveState = EnumMoveState.Moving;
            WriteLog("MoveControl", "7", device, "", "end.");
        }

        private bool OkToTurnZero(double outerWheelEncoder, double trunZeroEncoder)
        {
            if (controlData.DirFlag)
                return outerWheelEncoder >= trunZeroEncoder;
            else
                return outerWheelEncoder <= trunZeroEncoder;
        }



        private void R2000StopAndSetPosition()
        {
            if (CommandList[indexOfCmdList + 1].CmdType == EnumCommandType.SlowStop &&
                CommandList[indexOfCmdList + 1].Position == null)
            {
                r2000SlowStop = true;
            }
            else
            {
                double stopEncoder = location.ElmoEncoder + (controlData.DirFlag ? 50 : -50);

                while (true)
                {
                    UpdatePosition();
                    if (controlData.DirFlag)
                    {
                        if (location.RealEncoder > stopEncoder)
                            break;
                    }
                    else
                    {
                        if (location.RealEncoder < stopEncoder)
                            break;
                    }

                    Thread.Sleep(moveControlConfig.SleepTime);
                }

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
                double distance = controlData.TrigetEndEncoder - location.RealEncoder;
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
            int moveDirlag = controlData.DirFlag ? 1 : -1;
            double outerWheelEncoder;
            double trunZeroEncoder;
            double startOuterWheelEncoder;
            double startEncoder = location.ElmoEncoder;

            double distance = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R * Math.Sqrt(2);

            MoveState = EnumMoveState.R2000;
            indexOflisSectionLine++;

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
                EMSControl();
                return;
            }

            startOuterWheelEncoder = outerWheelEncoder;
            trunZeroEncoder = outerWheelEncoder + (controlData.DirFlag ? moveControlConfig.TurnParameter[EnumAddressAction.R2000].Distance :
                                                                        -moveControlConfig.TurnParameter[EnumAddressAction.R2000].Distance);

            WriteLog("MoveControl", "7", device, "", "開始旋轉, startOuterWheelEncoder : " + startOuterWheelEncoder.ToString("0.0") +
                                                ", 預計回正OuterWheelEncode : " + trunZeroEncoder.ToString("0.0"));
            // 哲瑀走行是下Abs.
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

                if (controlData.MoveControlStop)
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

            Thread.Sleep(50);
            while (!elmoDriver.MoveCompeleteVirtual(EnumAxisType.Move) /*|| !elmoDriver.MoveCompeleteVirtual(EnumAxisType.Turn)*/)
            {
                if (controlData.MoveControlStop)
                {
                    StopControl();
                    return;
                }

                UpdatePosition();

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            location.Delta = location.Delta + (controlData.DirFlag ? (distance - (location.ElmoEncoder - startEncoder)) :
                                               -(distance - (startEncoder - location.ElmoEncoder)));

            R2000StopAndSetPosition();
            indexOflisSectionLine++;
            UpdatePosition();
            if ((wheelAngle == -1 && controlData.DirFlag) ||
                (wheelAngle == 1 && !controlData.DirFlag))
                location.Real.AGVAngle -= 90;
            else
                location.Real.AGVAngle += 90;

            MoveState = EnumMoveState.Moving;
            WriteLog("MoveControl", "7", device, "", " end.");
            //Test();
        }

        private void VchangeControl(double velocity, bool isTRVChange = false, int TRWheelAngle = 0)
        {
            WriteLog("MoveControl", "7", device, "", " start, Velocity : " + velocity.ToString("0"));

            if (velocity != controlData.VelocityCommand)
            {
                controlData.VelocityCommand = velocity;
                agvRevise.SettingReviseData(velocity, controlData.DirFlag);
                velocity /= moveControlConfig.Move.Velocity;
                elmoDriver.ElmoGroupVelocityChange(EnumAxis.GX, velocity);
            }

            if (isTRVChange)
            {
                switch (TRWheelAngle)
                {
                    case 0:
                        BeamSensorSingleOnOff((controlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back), true);
                        break;
                    case 90:
                        BeamSensorSingleOnOff((controlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right), true);
                        break;
                    case -90:
                        BeamSensorSingleOnOff((controlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left), true);
                        break;
                    default:
                        WriteLog("MoveControl", "4", device, "", "switch (TRWheelAngle) default..EMS..");
                        EMSControl();
                        break;
                }
            }

            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void MoveCommandControl(double velocity, double distance, bool dirFlag, int wheelAngle, bool isFirstMove)
        {
            WriteLog("MoveControl", "7", device, "", "start, 方向 : " + (dirFlag ? "前進" : "後退") + ", distance : " + distance.ToString("0") +
                                                     ", velocity : " + velocity.ToString("0") + ", wheelAngle : " + wheelAngle.ToString("0") +
                                                     (isFirstMove ? ", 為第一次移動,需等待兩秒!" : ""));
            elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.Turn.Velocity, EnumMoveType.Absolute,
                    moveControlConfig.Turn.Acceleration, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Reset();
            timer.Start();
            while (!elmoDriver.WheelAngleCompare(wheelAngle, 1) && timer.ElapsedMilliseconds < moveControlConfig.TurnTimeoutValue && !SimulationMode)
            {
                UpdatePosition();
                Thread.Sleep(moveControlConfig.SleepTime);
            }

            if (!elmoDriver.WheelAngleCompare(wheelAngle, 1) && !SimulationMode)
            {
                WriteLog("MoveControl", "4", device, "", "舵輪旋轉Timeout!");
                return;
            }


            switch (wheelAngle)
            {
                case 0: // 朝前面.
                    BeamSensorSingleOnOff((controlData.DirFlag ? EnumBeamSensorLocate.Front : EnumBeamSensorLocate.Back), true);
                    break;
                case 90: // 朝左.
                    BeamSensorSingleOnOff((controlData.DirFlag ? EnumBeamSensorLocate.Left : EnumBeamSensorLocate.Right), true);
                    break;
                case -90: // 朝右.
                    BeamSensorSingleOnOff((controlData.DirFlag ? EnumBeamSensorLocate.Right : EnumBeamSensorLocate.Left), true);
                    break;
                default:
                    WriteLog("MoveControl", "4", device, "", "switch (wheelAngle) default..EMS..");
                    return;
            }

            if (isFirstMove)
            {
                controlData.TrigetEndEncoder = dirFlag ? distance : -distance;
                timer.Reset();
                timer.Start();
                while (timer.ElapsedMilliseconds < moveControlConfig.MoveStartWaitTime)
                {
                    UpdatePosition();
                    Thread.Sleep(moveControlConfig.SleepTime);
                }
            }
            else
            {
                controlData.TrigetEndEncoder = controlData.TrigetEndEncoder + (dirFlag ? distance : -distance);
                indexOflisSectionLine++;
            }

            timer.Reset();
            timer.Start();
            while (elmoDriver.ElmoGetDisable(EnumAxis.GX) && timer.ElapsedMilliseconds < moveControlConfig.TurnTimeoutValue && !SimulationMode)
            {
                UpdatePosition();
                Thread.Sleep(moveControlConfig.SleepTime);
            }

            if (elmoDriver.ElmoGetDisable(EnumAxis.GX) && !SimulationMode)
            {
                WriteLog("MoveControl", "4", device, "", "Enable Timeout!");
                return;
            }

            controlData.WheelAngle = wheelAngle;
            controlData.DirFlag = dirFlag;
            controlData.VelocityCommand = velocity;
            simulationIsMoving = true;

            if (dirFlag)
                elmoDriver.ElmoMove(EnumAxis.GX, distance, velocity, EnumMoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            else
                elmoDriver.ElmoMove(EnumAxis.GX, -distance, velocity, EnumMoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);


            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void SlowStopControl(MapPosition endPosition)
        {
            WriteLog("MoveControl", "7", device, "", "start");
            elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Reset();
            timer.Start();
            while (!elmoDriver.MoveCompelete(EnumAxis.GX) && timer.ElapsedMilliseconds < moveControlConfig.SlowStopTimeoutValue)
            {
                UpdatePosition();
                Thread.Sleep(moveControlConfig.SleepTime);
            }

            if (!elmoDriver.MoveCompelete(EnumAxis.GX))
            {
                WriteLog("MoveControl", "4", device, "", "SlowStop Timeout!");
                EMSControl();
                return;
            }

            BeamSensorCloseAll();

            if (r2000SlowStop)
            {
                r2000SlowStop = false;
                elmoDriver.SetMoveAxisPosition();
            }

            for (int i = indexOfCmdList + 1; i < CommandList.Count; i++)
            {
                if (CommandList[i].CmdType == EnumCommandType.Move)
                {
                    controlData.DirFlag = CommandList[i].DirFlag;
                    break;
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
                while (!elmoDriver.MoveCompelete(EnumAxis.GX) && timer.ElapsedMilliseconds < moveControlConfig.SlowStopTimeoutValue)
                {
                    UpdatePosition();
                    Thread.Sleep(moveControlConfig.SleepTime);
                }

                if (!elmoDriver.MoveCompelete(EnumAxis.GX))
                {
                    WriteLog("MoveControl", "4", device, "", "SecondCorrectionControl Timeout!");
                    EMSControl();
                    return;
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

            controlData.MoveControlStop = false;
            simulationIsMoving = false;
            WriteLog("MoveControl", "7", device, "", "end");
        }

        private void EMSControl()
        {
            WriteLog("MoveControl", "7", device, "", "start");
            elmoDriver.ElmoStop(EnumAxis.GX);
            elmoDriver.ElmoStop(EnumAxis.GX);
            //elmoDriver.DisableAllAxis();
            MoveState = EnumMoveState.Error;
            MoveFinished(EnumMoveComplete.Fail);
            simulationIsMoving = false;
            WriteLog("MoveControl", "7", device, "", "end");
        }
        #endregion

        #region 檢查觸發.
        private bool CheckGetNextReserve(Command cmd)
        {
            if (cmd.NextRserveCancel)
            {
                if (cmd.ReserveNumber + 1 < ReserveList.Count)
                {
                    if (ReserveList[cmd.ReserveNumber + 1].GetReserve)
                    {
                        indexOfCmdList++;
                        WriteLog("MoveControl", "7", device, "", "取得下段Reserve點, 因此取消此命令~!");
                        return false;
                    }
                    else
                        return true;
                }
                else
                {
                    WriteLog("MoveControl", "7", device, "", "??? 取得下段Reserve要取消此命令,但是Reserve List並無下個Reserve點? ..EMS..");
                    EMSControl();
                    return false;
                }
            }
            else
                return true;
        }

        private bool TriggerCommand(Command cmd)
        {
            if (cmd.ReserveNumber < ReserveList.Count && ReserveList[cmd.ReserveNumber].GetReserve)
            {
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
                            WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 超過Triiger觸發區間,EMS.. dirFlag : " +
                                         (controlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
                                         location.RealEncoder.ToString("0.0") + ", triggerEncoder : " + cmd.TriggerEncoder.ToString("0.0"));
                            EMSControl();
                            return false;
                        }
                        else if (location.RealEncoder > cmd.TriggerEncoder)
                        {
                            if (CheckGetNextReserve(cmd))
                            {
                                WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 觸發, dirFlag : " +
                                          (controlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
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
                            WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 超過Triiger觸發區間,EMS.. dirFlag : " +
                                         (controlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
                                         location.RealEncoder.ToString("0.0") + ", triggerEncoder : " + cmd.TriggerEncoder.ToString("0.0"));
                            EMSControl();
                            return false;
                        }
                        else if (location.RealEncoder < cmd.TriggerEncoder)
                        {
                            if (CheckGetNextReserve(cmd))
                            {
                                WriteLog("MoveControl", "7", device, "", "Command : " + cmd.CmdType.ToString() + ", 觸發, dirFlag : " +
                                          (controlData.DirFlag ? "往前" : "往後") + ", Encoder : " +
                                         location.RealEncoder.ToString("0.0") + ", triggerEncoder : " + cmd.TriggerEncoder.ToString("0.0"));
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        private void ExecuteCommandList()
        {
            if (CommandList.Count != 0 && indexOfCmdList < CommandList.Count && TriggerCommand(CommandList[indexOfCmdList]))
            {
                WriteLog("MoveControl", "7", device, "", "Barcode Position ( " + location.Barcode.Position.X.ToString("0") + ", " + location.Barcode.Position.Y.ToString("0") +
                                                         " ), Real Position ( " + location.Real.Position.X.ToString("0") + ", " + location.Real.Position.Y.ToString("0") +
                                                         " ), Encoder Position ( " + location.Encoder.Position.X.ToString("0") + ", " + location.Encoder.Position.Y.ToString("0") + " )");

                switch (CommandList[indexOfCmdList].CmdType)
                {
                    case EnumCommandType.TR:
                        TRControl(CommandList[indexOfCmdList].WheelAngle, CommandList[indexOfCmdList].TurnType);
                        break;
                    case EnumCommandType.R2000:
                        R2000Control(CommandList[indexOfCmdList].WheelAngle);
                        break;
                    case EnumCommandType.Vchange:
                        VchangeControl(CommandList[indexOfCmdList].Velocity);
                        break;
                    case EnumCommandType.ReviseOpen:
                        if (controlData.OntimeReviseFlag == false)
                        {
                            agvRevise.SettingReviseData(controlData.VelocityCommand, controlData.DirFlag);
                            controlData.OntimeReviseFlag = true;
                        }

                        break;
                    case EnumCommandType.ReviseClose:
                        controlData.OntimeReviseFlag = false;
                        if (CommandList[indexOfCmdList].TurnType == EnumAddressAction.R2000)
                            elmoDriver.ElmoMove(EnumAxis.GT, 0, 20, EnumMoveType.Absolute);
                        else
                            elmoDriver.ElmoStop(EnumAxis.GT);
                        break;
                    case EnumCommandType.Move:
                        MoveCommandControl(CommandList[indexOfCmdList].Velocity, CommandList[indexOfCmdList].Distance, CommandList[indexOfCmdList].DirFlag,
                                           CommandList[indexOfCmdList].WheelAngle, CommandList[indexOfCmdList].IsFirstMove);
                        break;
                    case EnumCommandType.SlowStop:
                        SlowStopControl(CommandList[indexOfCmdList].EndPosition);
                        break;
                    case EnumCommandType.End:
                        SlowStopControl(CommandList[indexOfCmdList].EndPosition);
                        SecondCorrectionControl(CommandList[indexOfCmdList].EndEncoder);
                        break;
                    case EnumCommandType.Stop:
                        StopControl();
                        break;
                    default:
                        break;
                }

                indexOfCmdList++;
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

                    if (!controlData.MoveControlStop && MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
                    {
                        ExecuteCommandList();
                    }

                    if (controlData.OntimeReviseFlag && !controlData.MoveControlStop && MoveState == EnumMoveState.Moving)
                    {
                        ontimeReviseEMSMessage = "";

                        if (agvRevise.OntimeRevise(ref reviseWheelAngle, controlData.WheelAngle, ref ontimeReviseEMSMessage))
                        {
                            if (ontimeReviseEMSMessage != "")
                            {
                                EMSControl();
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

                    if (controlData.MoveControlStop)
                    {
                        StopControl();
                    }

                    Thread.Sleep(moveControlConfig.SleepTime);
                }
            }
            catch
            {

                EMSControl();
            }
        }

        #region 障礙物檢知



        #endregion

        #region BeamSensor切換
        private void BeamSensorSingleOnOff(EnumBeamSensorLocate locate, bool flag)
        {
            WriteLog("MoveControl", "7", device, "", "Beam sensor 切換 : 修改 " + locate.ToString() + " 變更為 " + (flag ? "On" : "Off") + " !");

        }

        private void BeamSensorOnlyOnOff(EnumBeamSensorLocate locate, bool flag)
        {
            WriteLog("MoveControl", "7", device, "", "Beam sensor 切換 : 只剩 " + locate.ToString() + " !");

        }

        private void BeamSensorCloseAll()
        {
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
                WriteLog("MoveControl", "7", device, "", "location.Real == null EMS");
                EMSControl();
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
                controlData.MoveControlStop = true;
            }

            location.Encoder = new AGVPosition();
            location.Encoder.Position = new MapPosition(location.Real.Position.X, location.Real.Position.Y);
            location.Encoder.AGVAngle = location.Real.AGVAngle;
            UpdateEncoderPositionNowEncoder();
            location.Delta = 0;
            WriteLog("MoveControl", "7", device, "", "end");
        }

        private bool CheckListAllOK()
        {
            if (ReserveList == null || ReserveList.Count == 0)
                return false;

            if (SectionLineList == null || SectionLineList.Count == 0)
                return false;

            if (CommandList == null || CommandList.Count == 0)
                return false;

            return true;
        }

        public bool TransferMove(MoveCmdInfo moveCmd)
        {
            WriteLog("MoveControl", "7", device, "", "start");
            //MoveCmdInfo moveCmdColne = moveCmd.DeepClone();
            MoveCmdInfo moveCmdColne = moveCmd;

            string errorMessage = "";

            if ((MoveState != EnumMoveState.Error && MoveState != EnumMoveState.Idle))
            {
                WriteLog("MoveControl", "7", device, "", "移動中,因此無視~!");
                return false;
            }

            List<Command> moveCmdList = new List<Command>();
            List<SectionLine> sectionLineList = new List<SectionLine>();
            List<ReserveData> reserveDataList = new List<ReserveData>();

            if (!createMoveControlList.CreatMoveControlListSectionListReserveList(moveCmd, ref moveCmdList, ref sectionLineList, ref reserveDataList,
                                                                                  location.Real, controlData.WheelAngle, ref errorMessage))
            {
                WriteLog("MoveControl", "7", device, "", "命令分解失敗~!, errorMessage : " + errorMessage);
                return false;
            }

            ResetEncoder(sectionLineList[0].Start, sectionLineList[0].End, sectionLineList[0].DirFlag);

            ReserveList = reserveDataList;
            SectionLineList = sectionLineList;
            indexOflisSectionLine = 0;
            CommandList = moveCmdList;
            indexOfCmdList = 0;
            MoveCommandID = moveCmd.CmdId;
            r2000SlowStop = false;

            // 暫時直接取得所有Reserve
            for (int i = 0; i < reserveDataList.Count; i++)
                reserveDataList[i].GetReserve = true;

            if (!CheckListAllOK())
            {
                WriteLog("MoveControl", "7", device, "", "List 有問題!");
                return false;
            }

            Task.Factory.StartNew(() =>
            {
                elmoDriver.EnableMoveAxis();
            });

            isAGVMCommand = true;
            safetyData = new MoveControlSafetyData();
            MoveState = EnumMoveState.Moving;
            AGVStopResult = "";
            WriteLog("MoveControl", "7", device, "", "sucess! 開始執行動作~!");
            return true;
        }

        public bool CreatMoveControlListSectionListReserveList(MoveCmdInfo moveCmd, ref List<Command> moveCmdList, ref List<SectionLine> sectionLineList,
                                                               ref List<ReserveData> reserveDataList, AGVPosition nowAGV, ref string errorMessage)
        {
            return createMoveControlList.CreatMoveControlListSectionListReserveList(moveCmd, ref moveCmdList, ref sectionLineList,
                ref reserveDataList, nowAGV, controlData.WheelAngle, ref errorMessage);
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
            WriteLog("MoveControl", "7", device, "", "start");
            ReserveList = reserveDataList;
            SectionLineList = sectionLineList;
            indexOflisSectionLine = 0;
            CommandList = moveCmdList;
            indexOfCmdList = 0;
            r2000SlowStop = false;
            MoveCommandID = "DebugForm" + DateTime.Now.ToString("HH:mm:ss");

            ResetEncoder(sectionLineList[0].Start, sectionLineList[0].End, sectionLineList[0].DirFlag);

            if (!CheckListAllOK())
            {
                WriteLog("MoveControl", "7", device, "", "List 有問題!");
                return false;
            }

            Task.Factory.StartNew(() =>
            {
                elmoDriver.EnableMoveAxis();
            });

            isAGVMCommand = false;
            safetyData = new MoveControlSafetyData();
            AGVStopResult = "";
            MoveState = EnumMoveState.Moving;
            WriteLog("MoveControl", "7", device, "", "sucess! 開始執行動作~!");
            return true;
        }

        public void StopFlagOn()
        {
            WriteLog("MoveControl", "7", device, "", "外部控制啟動StopControl!");
            controlData.MoveControlStop = true;
        }

        public void StatusChange()
        {
            WriteLog("MoveControl", "7", device, "", "外部控制強制清除命令!");
            controlData.WheelAngle = 0;
            MoveState = EnumMoveState.Idle;
        }

        public bool AddReservedMapPosition(MapPosition mapPosition)
        {
            bool returnBoolean = false;

            for (int i = 0; i < ReserveList.Count; i++)
            {
                if (mapPosition.X == ReserveList[i].Position.X && mapPosition.Y == ReserveList[i].Position.Y)
                {
                    ReserveList[i].GetReserve = true;
                    returnBoolean = true;
                    break;
                }
                else if (!ReserveList[i].GetReserve)
                    return false;
            }

            return returnBoolean;
        }

        public void AddReservedIndexForDebugModeTest(int index)
        {
            if (ReserveList == null || MoveState == EnumMoveState.Idle)
                return;

            if (index >= 0 && index < ReserveList.Count)
            {
                for (int i = 0; i <= index; i++)
                    ReserveList[i].GetReserve = true;
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

        public int GetCommandIndex()
        {
            return indexOfCmdList;
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
                if (MoveState != EnumMoveState.Idle && indexOfCmdList < CommandList.Count)
                {
                    AddCSV(ref csvLog, CommandList[indexOfCmdList].CmdType.ToString());

                    if (CommandList[indexOfCmdList].Position != null)
                        AddCSV(ref csvLog, CommandList[indexOfCmdList].TriggerEncoder.ToString("0"));
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
