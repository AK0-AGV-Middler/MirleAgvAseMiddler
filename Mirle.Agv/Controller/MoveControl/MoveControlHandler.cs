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

namespace Mirle.Agv.Controller
{
    public class MoveControlHandler
    {
        private CreateMoveControlList createMoveControlList;
        public EnumMoveState MoveState { get; private set; }
        public MoveControlConfig moveControlConfig;
        private MapInfo theMapInfo = new MapInfo();
        private Logger logger = LoggerAgent.Instance.GetLooger("MoveControlCSV");
        private Logger flow = LoggerAgent.Instance.GetLooger("MoveControl");

        public ElmoDriver elmoDriver;
        public List<Sr2000Driver> DriverSr2000List = new List<Sr2000Driver>();
        public OntimeReviseConfig ontimeReviseConfig = null;
        private AgvMoveRevise agvRevise;

        public event EventHandler<EnumMoveComplete> OnMoveFinished;

        private List<SectionLine> SectionLineList = new List<SectionLine>();
        private int indexOflisSectionLine = 0;
        public Location location = new Location();

        public List<Command> CommandList { get; private set; } = new List<Command>();
        private int indexOfCmdList = 0;

        private MoveControlParameter controlData = new MoveControlParameter();
        private const int AllowableTheta = 10;
        public List<ReserveData> ReserveList { get; private set; } = new List<ReserveData>();
        Thread threadSCVLog;
        public bool DebugLog { get; set; }
        public List<string[]> deubgCsvLogList = new List<string[]>();
        public List<string> debugFlowLogList = new List<string>();
        private bool isAGVMCommand = false;
        public string MoveCommandID { get; set; } = "";

        public bool SimulationMode { get; set; } = false;
        private bool simulationIsMoving = false;
        private MoveControlSafetyData safetyData = new MoveControlSafetyData();
        public string AGVStopResult { get; set; }

        public MoveControlHandler(string moveControlConfigPath, MapInfo theMapInfo)
        {
            moveControlConfigPath = "MoveControlConfig.xml";
            this.theMapInfo = theMapInfo;
            DebugLog = false;
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

        private void WriteLog(string logMeesage)
        {

            if (DebugLog)
                debugFlowLogList.Add(logMeesage);
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
                        returnAxisData.Deceleration = double.Parse(item.InnerText);
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
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "路徑有問題, path = null or \"\"!\n");
                return;
            }

            XmlDocument doc = new XmlDocument();

            string xmlPath = Path.Combine(Environment.CurrentDirectory, path);

            if (!File.Exists(xmlPath))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "路徑有問題, 找不到檔案!\n");
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
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "路徑有問題, path = null or \"\"!\n");
                return;
            }

            XmlDocument doc = new XmlDocument();
            Sr2000Config sr2000Config;
            Sr2000Driver driverSr2000;
            string xmlPath = path;

            if (!File.Exists(xmlPath))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "路徑有問題, 找不到檔案!\n");
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
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "SR2000 啟動失敗!\n");
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
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "路徑有問題, path = null or \"\"!\n");
                return;
            }

            XmlDocument doc = new XmlDocument();

            string xmlPath = Path.Combine(Environment.CurrentDirectory, path);

            if (!File.Exists(xmlPath))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "路徑有問題, 找不到檔案!\n");
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
            MapPosition returnPosition = new MapPosition((float)x, (float)y);

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
                // Error.
                Console.WriteLine("Error");
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
                location.Encoder = GetMapPosition(SectionLineList[indexOflisSectionLine], location.RealEncoder);
                Vehicle.Instance.GetVehLoacation().RealPosition = location.Real.Position;
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
                double distance = Math.Sqrt(Math.Pow(location.Encoder.X - location.Barcode.Position.X, 2) + 
                                            Math.Pow(location.Encoder.Y - location.Barcode.Position.Y, 2));
                if (distance > moveControlConfig.Safety[EnumMoveControlSafetyType.UpdateDeltaPositionRange].Range)
                {
                    AGVStopResult = "UpdateDelta中Barcode讀取位置和Encode(座標)差距" +
                        Math.Abs(location.ElmoEncoder - safetyData.TurnOutElmoEncoder).ToString("0") +
                        "mm,已超過安全設置的" +
                        moveControlConfig.Safety[EnumMoveControlSafetyType.UpdateDeltaPositionRange].Range.ToString("0") +
                        "mm,因此啟動EMS!";

                    EMSControl();
                }
            }
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
                        if (IsSameAngle(agvPosition.BarcodeAngleInMap, agvPosition.AGVAngle, controlData.WheelAngle))
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
                    Console.WriteLine("出彎瞬間Encoder : " + safetyData.TurnOutElmoEncoder.ToString("0"));
                    Console.WriteLine("出彎後第一次讀取到Barcde Encoder : " + location.ElmoEncoder.ToString("0"));
                    safetyData.IsTurnOut = false;
                }
                else
                {
                    if (Math.Abs(location.ElmoEncoder - safetyData.TurnOutElmoEncoder) >
                        moveControlConfig.Safety[EnumMoveControlSafetyType.TurnOut].Range)
                    {
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
                if (location.Real == null && newBarcodeData)
                {
                    location.Real = location.Barcode;
                    Vehicle.Instance.GetVehLoacation().RealPosition = location.Real.Position;
                }
            }
        }
        #endregion

        #region CommandControl
        private void TRControl(int wheelAngle, EnumAddressAction type)
        {
            MoveState = EnumMoveState.TR;

            double velocity = moveControlConfig.TurnParameter[type].Velocity;
            double r = moveControlConfig.TurnParameter[type].R;
            double xFLVelocity = Math.Abs(location.XFLVelocity);
            double xRRVelocity = Math.Abs(location.XRRVelocity);
            double startEncoder = location.ElmoEncoder;
            double distance = r * Math.PI / 2;


            if (Math.Abs(xFLVelocity - velocity) <= velocity * moveControlConfig.TurnSpeedSafetyRange &&
                Math.Abs(xRRVelocity - velocity) <= velocity * moveControlConfig.TurnSpeedSafetyRange)
            { // Normal
                if (!elmoDriver.MoveCompelete(EnumAxis.GT))
                    Console.WriteLine("GT Moving~~");

                elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.TurnParameter[type].AxisParameter.Velocity, EnumMoveType.Absolute,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Acceleration,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Deceleration,
                                    moveControlConfig.TurnParameter[type].AxisParameter.Jerk);
            }
            else if (Math.Abs(xFLVelocity - xRRVelocity) > 2 * velocity * moveControlConfig.TurnSpeedSafetyRange)
            { // GG,不該發生.
              // log..
                EMSControl();
            }
            else if (xFLVelocity > velocity && xRRVelocity > velocity)
            { // 超速GG, 不該發生.
                Console.WriteLine("TR 超速");
                // log..
                EMSControl();
            }
            else
            { // 太慢 處理??

                EMSControl();
            }

            indexOflisSectionLine++;

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
                    // 不該發生 log..
                    EMSControl();
                    break;
            }

            location.Delta = location.Delta + (controlData.DirFlag ? (distance - (location.ElmoEncoder - startEncoder)) :
                                               -(distance - (startEncoder - location.ElmoEncoder)));
            UpdatePosition();
            
            controlData.WheelAngle = wheelAngle;
            MoveState = EnumMoveState.Moving;
        }

        private bool OkToTurnZero(double outerWheelEncoder, double trunZeroEncoder)
        {
            if (controlData.DirFlag)
                return outerWheelEncoder >= trunZeroEncoder;
            else
                return outerWheelEncoder <= trunZeroEncoder;
        }

        public void R2000Control(int wheelAngle, double turnZeroOuterWheelDistance = 0)
        {
            AxisData leftMove;
            AxisData leftTurn;
            AxisData rightMove;
            AxisData rightTurn;
            int moveDirlag = controlData.DirFlag ? 1 : -1;
            double outerWheelEncoder;
            double trunZeroEncoder;

            MoveState = EnumMoveState.R2000;

            if (turnZeroOuterWheelDistance == 0)
            {
                EMSControl();
                return;
            }

            if (wheelAngle == 1)
            {
                outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.TFL, true);
                leftTurn = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.InnerWheelTurn];
                rightTurn = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.OuterWheelTurn];

                leftMove = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.InnerWheelMove];
                rightMove = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.OuterWheelMove];
            }
            else if (wheelAngle == -1)
            {
                outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.TFR, true);
                rightTurn = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.InnerWheelTurn];
                leftTurn = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.OuterWheelTurn];

                rightMove = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.InnerWheelMove];
                leftMove = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R2000Parameter[EnumR2000Parameter.OuterWheelMove];
            }
            else
            {
                EMSControl();
                return;
            }

            // 哲瑀走行是下Abs.
            elmoDriver.ElmoMove(EnumAxis.VTFL, leftTurn.Distance * wheelAngle, leftTurn.Velocity, EnumMoveType.Absolute,
                               leftTurn.Acceleration, leftTurn.Deceleration, leftTurn.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTFR, rightTurn.Distance * wheelAngle, rightTurn.Velocity, EnumMoveType.Absolute,
                               rightTurn.Acceleration, rightTurn.Deceleration, rightTurn.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTRL, -leftTurn.Distance * wheelAngle, leftTurn.Velocity, EnumMoveType.Absolute,
                               leftTurn.Acceleration, leftTurn.Deceleration, leftTurn.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTRR, -rightTurn.Distance * wheelAngle, rightTurn.Velocity, EnumMoveType.Absolute,
                               rightTurn.Acceleration, rightTurn.Deceleration, rightTurn.Jerk);

            elmoDriver.ElmoMove(EnumAxis.VXFL, leftMove.Distance * moveDirlag, leftMove.Velocity, EnumMoveType.Relative,
                               leftMove.Acceleration, leftMove.Deceleration, leftMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VXFL, rightMove.Distance * moveDirlag, rightMove.Velocity, EnumMoveType.Relative,
                               rightMove.Acceleration, rightMove.Deceleration, rightMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VXFL, leftMove.Distance * moveDirlag, leftMove.Velocity, EnumMoveType.Relative,
                               leftMove.Acceleration, leftMove.Deceleration, leftMove.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VXFL, rightMove.Distance * moveDirlag, rightMove.Velocity, EnumMoveType.Relative,
                               rightMove.Acceleration, rightMove.Deceleration, rightMove.Jerk);

            trunZeroEncoder = (controlData.DirFlag ? 1 : -1) * turnZeroOuterWheelDistance + outerWheelEncoder;


            while (!OkToTurnZero(outerWheelEncoder, trunZeroEncoder) && !SimulationMode)
            {
                if (wheelAngle == 1)
                    outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.TFL, true);
                else if (wheelAngle == -1)
                    outerWheelEncoder = elmoDriver.ElmoGetPosition(EnumAxis.TFR, true);

                UpdatePosition();

                if (controlData.MoveControlStop)
                {
                    StopControl();
                    return;
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }

            elmoDriver.ElmoMove(EnumAxis.VTFL, 0, leftTurn.Velocity, EnumMoveType.Absolute, leftTurn.Acceleration, leftTurn.Deceleration, leftTurn.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTFR, 0, rightTurn.Velocity, EnumMoveType.Absolute, rightTurn.Acceleration, rightTurn.Deceleration, rightTurn.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTRL, 0, leftTurn.Velocity, EnumMoveType.Absolute, leftTurn.Acceleration, leftTurn.Deceleration, leftTurn.Jerk);
            elmoDriver.ElmoMove(EnumAxis.VTRR, 0, rightTurn.Velocity, EnumMoveType.Absolute, rightTurn.Acceleration, rightTurn.Deceleration, rightTurn.Jerk);

            elmoDriver.ElmoStop(EnumAxis.VXFL, leftMove.Deceleration, leftMove.Jerk);
            elmoDriver.ElmoStop(EnumAxis.VXFL, rightMove.Deceleration, rightMove.Jerk);
            elmoDriver.ElmoStop(EnumAxis.VXFL, leftMove.Deceleration, leftMove.Jerk);
            elmoDriver.ElmoStop(EnumAxis.VXFL, rightMove.Deceleration, rightMove.Jerk);

            while (!elmoDriver.WheelAngleCompare(0, 1))
            {
                if (controlData.MoveControlStop)
                {
                    StopControl();
                    return;
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }


            MoveState = EnumMoveState.Moving;
        }

        private void VchangeControl(double velocity, bool isTRVChange = false, int TRWheelAngle = 0)
        {
            if (velocity == controlData.VelocityCommand)
                return;

            controlData.VelocityCommand = velocity;
            agvRevise.SettingReviseData(velocity, controlData.DirFlag);
            velocity /= moveControlConfig.Move.Velocity;

            Console.WriteLine("V Change : " + velocity.ToString("0.00") + ", " + TRWheelAngle.ToString());
            elmoDriver.ElmoGroupVelocityChange(EnumAxis.GX, velocity);

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
                        // 不該發生 log..
                        EMSControl();
                        break;
                }
            }
        }

        private void MoveCommandControl(double velocity, double distance, bool difFlag, int wheelAngle, bool isFirstMove)
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : 舵輪旋轉至 " + wheelAngle.ToString("0") + " 度!");
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
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : 舵輪旋轉Timeout!");
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
                    // log..
                    return;
            }

            if (isFirstMove)
            {
                timer.Reset();
                timer.Start();
                while (timer.ElapsedMilliseconds < moveControlConfig.MoveStartWaitTime)
                {
                    UpdatePosition();
                    Thread.Sleep(moveControlConfig.SleepTime);
                }
            }
            else
                indexOflisSectionLine++;

            timer.Reset();
            timer.Start();
            while (elmoDriver.ElmoGetDisable(EnumAxis.GX) && timer.ElapsedMilliseconds < moveControlConfig.TurnTimeoutValue && !SimulationMode)
            {
                UpdatePosition();
                Thread.Sleep(moveControlConfig.SleepTime);
            }

            if (elmoDriver.ElmoGetDisable(EnumAxis.GX) && !SimulationMode)
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : Enable Timeout!");
                return;
            }

            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : ElmoMove, Distance : " + distance.ToString("0") +
                                                    ", 方向 : " + (difFlag ? "前進" : "後退") + ", 速度 : " + velocity.ToString("0"));

            controlData.WheelAngle = wheelAngle;
            controlData.DirFlag = difFlag;
            controlData.VelocityCommand = velocity;
            simulationIsMoving = true;

            if (difFlag)
                elmoDriver.ElmoMove(EnumAxis.GX, distance, velocity, EnumMoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            else
                elmoDriver.ElmoMove(EnumAxis.GX, -distance, velocity, EnumMoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
        }

        private void SlowStopControl(MapPosition endPosition)
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "SlowStop!");
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
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : SlowStop Timeout!");
                EMSControl();
                return;
            }

            BeamSensorCloseAll();

            for (int i = indexOfCmdList + 1; i < CommandList.Count; i++)
            {
                if (CommandList[i].CmdType == EnumCommandType.Move)
                {
                    controlData.DirFlag = CommandList[i].DirFlag;
                    break;
                }
            }

            simulationIsMoving = false;
        }

        private void SecondCorrectionControl(double endEncoder)
        {
            UpdatePosition();

            Console.WriteLine("二修 : " + (endEncoder - location.RealEncoder).ToString("0.0"));
            if (Math.Abs(endEncoder - location.RealEncoder) > moveControlConfig.SecondCorrectionX)
            {
                elmoDriver.ElmoMove(EnumAxis.GX, endEncoder - location.RealEncoder, moveControlConfig.EQVelocity, EnumMoveType.Relative,
                                    moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

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
                    flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : SecondCorrectionControl Timeout!");
                    EMSControl();
                    return;
                }
            }

            elmoDriver.DisableMoveAxis();
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "Move Compelete !");
            MoveState = EnumMoveState.Idle;
            MoveFinished(EnumMoveComplete.Success);
        }

        private void StopControl()
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "Stop!");
            elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            elmoDriver.ElmoStop(EnumAxis.GT, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);
            if (MoveState == EnumMoveState.R2000)
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

            controlData.MoveControlStop = false;
            simulationIsMoving = false;
        }

        private void EMSControl()
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "EMS!");
            elmoDriver.ElmoStop(EnumAxis.GX);
            elmoDriver.ElmoStop(EnumAxis.GX);
            //elmoDriver.DisableAllAxis();
            MoveFinished(EnumMoveComplete.Fail);
            simulationIsMoving = false;
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
                        flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "取得下段Reserve點, 因此取消此命令~!");
                        return false;
                    }
                    else
                        return true;
                }
                else
                {
                    flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "??? 取得下段Reserve要取消此命令,但是Reserve List並無下個Reserve點~");
                    controlData.MoveControlStop = true;
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
                        flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "觸發,為為立即觸發~");
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
                            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "超過Triiger觸發區間!");
                            controlData.MoveControlStop = true;
                            return false;
                        }
                        else if (location.RealEncoder > cmd.TriggerEncoder)
                        {
                            if (CheckGetNextReserve(cmd))
                            {
                                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "觸發, Real Encoder : " + location.RealEncoder.ToString("0") +
                                                                           ", command type : " + cmd.CmdType.ToString() + "~");
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
                            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "超過Triiger觸發區間!");
                            controlData.MoveControlStop = true;
                            return false;
                        }
                        else if (location.RealEncoder < cmd.TriggerEncoder)
                        {
                            if (CheckGetNextReserve(cmd))
                            {
                                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "觸發, Real Encoder : " + location.RealEncoder.ToString("0") +
                                                                           ", command type : " + cmd.CmdType.ToString() + "~");
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
                Console.WriteLine("trigger : encoder = " + location.RealEncoder + " , command = " + CommandList[indexOfCmdList].CmdType.ToString());

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
                    Console.WriteLine("??");
                    StopControl();
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }
        }

        #region 障礙物檢知



        #endregion

        #region BeamSensor切換
        private void BeamSensorSingleOnOff(EnumBeamSensorLocate locate, bool flag)
        {
            flow.SavePureLog("Beam sensor 切換 : 修改 " + locate.ToString() + " 變更為 " + (flag ? "On" : "Off") + " !");

        }

        private void BeamSensorOnlyOnOff(EnumBeamSensorLocate locate, bool flag)
        {
            flow.SavePureLog("Beam sensor 切換 : 只剩 " + locate.ToString() + " !");

        }

        private void BeamSensorCloseAll()
        {
            flow.SavePureLog("Beam sensor 切換 : 全部關掉!");

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
                EMSControl();
                // log..
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
                // R2000 start. by pass
                controlData.MoveControlStop = true;
            }

            location.Delta = 0;
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
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "接收到AGVM命令~\n");
            //MoveCmdInfo moveCmdColne = moveCmd.DeepClone();
            MoveCmdInfo moveCmdColne = moveCmd;

            string errorMessage = "";

            if ((MoveState != EnumMoveState.Error && MoveState != EnumMoveState.Idle))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "移動中,因此無視~!\n");
                return false;
            }

            List<Command> moveCmdList = new List<Command>();
            List<SectionLine> sectionLineList = new List<SectionLine>();
            List<ReserveData> reserveDataList = new List<ReserveData>();

            if (!createMoveControlList.CreatMoveControlListSectionListReserveList(moveCmd, ref moveCmdList, ref sectionLineList, ref reserveDataList,
                                                                                  location.Real, ref errorMessage))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "命令分解失敗~!\n");
                return false;
            }

            MoveCommandID = moveCmd.CmdId;
            elmoDriver.SetPosition(EnumAxis.GX, 0);
            ResetEncoder(sectionLineList[0].Start, sectionLineList[0].End, sectionLineList[0].DirFlag);

            // 暫時直接取得所有Reserve
            for (int i = 0; i < reserveDataList.Count; i++)
                reserveDataList[i].GetReserve = true;

            ReserveList = reserveDataList;
            SectionLineList = sectionLineList;
            indexOflisSectionLine = 0;
            CommandList = moveCmdList;
            indexOfCmdList = 0;

            if (!CheckListAllOK())
            {
                Console.WriteLine("List 有問題!");
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
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "開始執行動作~!\n");
            return true;
        }

        public bool CreatMoveControlListSectionListReserveList(MoveCmdInfo moveCmd, ref List<Command> moveCmdList, ref List<SectionLine> sectionLineList,
                                                               ref List<ReserveData> reserveDataList, AGVPosition nowAGV, ref string errorMessage)
        {
            return createMoveControlList.CreatMoveControlListSectionListReserveList(moveCmd, ref moveCmdList, ref sectionLineList, ref reserveDataList, nowAGV, ref errorMessage);
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
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "接收到Debug Mode form命令~\n");
            MoveCommandID = "DebugForm" + DateTime.Now.ToString("HH:mm:ss");
            ReserveList = reserveDataList;
            SectionLineList = sectionLineList;
            indexOflisSectionLine = 0;
            CommandList = moveCmdList;
            indexOfCmdList = 0;

            elmoDriver.SetPosition(EnumAxis.GX, 0);
            ResetEncoder(sectionLineList[0].Start, sectionLineList[0].End, sectionLineList[0].DirFlag);

            if (!CheckListAllOK())
            {
                Console.WriteLine("List 有問題!");
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
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "開始執行動作~!\n");
            return true;
        }

        public void StopFlagOn()
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "外部控制啟動StopControl!\n");
            controlData.MoveControlStop = true;
        }

        public void StatusChange()
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "強制清除命令!\n");
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

                csvLogResult = DebugLog && (MoveState != EnumMoveState.Idle);
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
                    }
                }

                while (timer.ElapsedMilliseconds < moveControlConfig.CSVLogInterval)
                    Thread.Sleep(1);
            }
        }
        #endregion
    }
}
