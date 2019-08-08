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
        private AGVMoveRevise agvRevise;

        public event EventHandler<EnumMoveComplete> OnMoveFinished;

        private List<SectionLine> SectionLineList = new List<SectionLine>();
        private int indexOflisSectionLine = 0;
        public Location location = new Location();

        private List<Command> CommandList = new List<Command>();
        private int indexOfCmdList = 0;

        private MoveControlParameter controlData = new MoveControlParameter();
        private const int AllowableTheta = 10;
        private List<ReserveData> ReserveList;
        Thread threadSCVLog;
        public bool DebugLog { get; set; }
        public List<string[]> deubgCsvLogList = new List<string[]>();
        public List<string> debugFlowLogList = new List<string>();
        private bool isAGVMCommand = false;

        public MoveControlHandler(string moveControlConfigPath, MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
            DebugLog = false;
            moveControlConfig = new MoveControlConfig();

            InitailSr2000(moveControlConfig.Sr2000ConfigPath);
            elmoDriver = new ElmoDriver("MotionParameter.xml");

            ReadOntimeReviseConfigXML(moveControlConfig.OnTimeReviseConfigPath);
            createMoveControlList = new CreateMoveControlList(DriverSr2000List, moveControlConfig);

            agvRevise = new AGVMoveRevise(ontimeReviseConfig, elmoDriver, DriverSr2000List);

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
            else if (DriverSr2000List.Count == 0 && location.Barcode == null)
            {   // fake data
                MapPosition tempPosition = new MapPosition(0, 0);
                location.Barcode = new AGVPosition(tempPosition, 0, 0, 20, DateTime.Now, 0, 0);
                return true;
            }

            return false;
        }

        private void UpdateElmo()
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

        private void UpdatePosition()
        {
            UpdateElmo();
            bool newBarcodeData = UpdateSR2000();

            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
            {
                if (MoveState != EnumMoveState.TR && MoveState != EnumMoveState.R2000 && newBarcodeData)
                    UpdateDelta();

                UpdateReal();
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

            double velocity = moveControlConfig.TR[type].Velocity;
            double r = moveControlConfig.TR[type].R;
            double xFLVelocity = Math.Abs(location.XFLVelocity);
            double xRRVelocity = Math.Abs(location.XRRVelocity);


            if (Math.Abs(xFLVelocity - velocity) <= velocity * moveControlConfig.TurnSpeedSafetyRange &&
                Math.Abs(xRRVelocity - velocity) <= velocity * moveControlConfig.TurnSpeedSafetyRange)
            { // Normal
                if (!elmoDriver.MoveCompelete(EnumAxis.GT))
                    Console.WriteLine("GT Moving~~");

                elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.TR[type].AxisParameter.Velocity, EnumMoveType.Absolute,
                                    moveControlConfig.TR[type].AxisParameter.Acceleration,
                                    moveControlConfig.TR[type].AxisParameter.Deceleration,
                                    moveControlConfig.TR[type].AxisParameter.Jerk);
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

            while (!elmoDriver.WheelAngleCompare(wheelAngle, moveControlConfig.StartWheelAngleRange))
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

            controlData.WheelAngle = wheelAngle;
            MoveState = EnumMoveState.Moving;
        }

        private void R2000Control(int TRWheelAngle)
        {

        }

        private void VchangeControl(double velocity, bool isTRVChange = false, int TRWheelAngle = 0)
        {
            if (velocity == controlData.VelocityCommand)
                return;

            controlData.VelocityCommand = velocity;
            agvRevise.SettingReviseData(velocity, controlData.DirFlag);
            velocity /= moveControlConfig.AGVMaxVelocity;


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
            while (!elmoDriver.WheelAngleCompare(wheelAngle, 1) && timer.ElapsedMilliseconds < moveControlConfig.TurnTimeoutValue)
                Thread.Sleep(50);

            if (!elmoDriver.WheelAngleCompare(wheelAngle, 1))
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
                Thread.Sleep(5000);

            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : ElmoMove, Distance : " + distance.ToString("0") +
                                                    ", 方向 : " + (difFlag ? "前進" : "後退") + ", 速度 : " + velocity.ToString("0"));

            controlData.WheelAngle = wheelAngle;
            controlData.DirFlag = difFlag;
            controlData.VelocityCommand = velocity;

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
                Thread.Sleep(50);

            if (!elmoDriver.MoveCompelete(EnumAxis.GX))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : SlowStop Timeout!");
                EMSControl();
                return;
            }

            BeamSensorCloseAll();
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
                    Thread.Sleep(50);

                if (!elmoDriver.MoveCompelete(EnumAxis.GX))
                {
                    flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : SecondCorrectionControl Timeout!");
                    EMSControl();
                    return;
                }
            }

            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "Move Compelete !");
            MoveState = EnumMoveState.Idle;
            MoveFinished(EnumMoveComplete.Success);
        }

        private void StopControl()
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "Stop!");
            elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            elmoDriver.ElmoStop(EnumAxis.GT, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);
            controlData.MoveControlStop = false;
        }

        private void EMSControl()
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "EMS!");
            elmoDriver.DisableAllAxis();
            MoveFinished(EnumMoveComplete.Fail);
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
                        TRControl(CommandList[indexOfCmdList].WheelAngle, CommandList[indexOfCmdList].TRType);
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
            while (true)
            {
                UpdatePosition();

                if (!controlData.MoveControlStop && MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.Error)
                {
                    ExecuteCommandList();
                }

                if (controlData.OntimeReviseFlag && !controlData.MoveControlStop && MoveState == EnumMoveState.Moving)
                {
                    if (agvRevise.OntimeRevise(ref reviseWheelAngle, controlData.WheelAngle))
                    {
                        elmoDriver.ElmoMove(EnumAxis.GT, reviseWheelAngle[0], reviseWheelAngle[1], reviseWheelAngle[2], reviseWheelAngle[3],
                                          ontimeReviseConfig.ThetaSpeed, EnumMoveType.Absolute, moveControlConfig.Turn.Acceleration,
                                          moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);
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

            for (int i = 0; i < moveCmdColne.SectionSpeedLimits.Count; i++)
            {
                if (moveCmdColne.SectionSpeedLimits[i] > moveControlConfig.AGVMaxVelocity)
                    moveCmdColne.SectionSpeedLimits[i] = moveControlConfig.AGVMaxVelocity;
            }

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

            elmoDriver.SetPosition(EnumAxis.GX, 0);
            ResetEncoder(sectionLineList[0].Start, sectionLineList[0].End, sectionLineList[0].DirFlag);

            // 暫時直接取得所有Reserve
            for (int i = 0; i < reserveDataList.Count; i++)
                //if (i != 1)
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

            isAGVMCommand = true;
            MoveState = EnumMoveState.Moving;
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

            isAGVMCommand = false;
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

            if (MoveState == EnumMoveState.Idle || ReserveList == null)
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
            if (MoveState == EnumMoveState.Idle)
                return -1;

            return indexOfCmdList;
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
