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

        public event EventHandler<MoveComplete> OnMoveFinished;

        private List<SectionLine> SectionLineList = new List<SectionLine>();
        private int indexOflisSectionLine = 0;
        public Position position = new Position();

        private List<Command> CommandList = new List<Command>();
        private int indexOfCmdList = 0;

        private MoveControlParameter controlData = new MoveControlParameter();
        private const int AllowableTheta = 10;
        private List<ReserveData> ReserveList;
        Thread threadSCVLog;
        public bool DebugLog { get; set; }
        public List<string> debugCsvLogList = new List<string>();
        public List<string> debugFlowLogList = new List<string>();

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

            elmoDriver.ElmoStop(Axis.GX);
            elmoDriver.ElmoMove(Axis.GT, 0, 75, MoveType.Absolute);

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
            string xmlPath = Path.Combine(Environment.CurrentDirectory, path);

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
                double realElmoEncode = position.ElmoEncoder + position.Delta + position.Offset;
                position.RealEncoder = realElmoEncode;

                if ((SectionLineList[indexOflisSectionLine].DirFlag && realElmoEncode < SectionLineList[indexOflisSectionLine].EncoderStart) ||
                   (!SectionLineList[indexOflisSectionLine].DirFlag && realElmoEncode > SectionLineList[indexOflisSectionLine].EncoderStart))
                {
                    if (indexOflisSectionLine == 0)
                        realElmoEncode = SectionLineList[indexOflisSectionLine].EncoderStart;
                    else
                    {
                        indexOflisSectionLine--;
                        UpdateReal();
                        return;
                    }
                }
                else if ((SectionLineList[indexOflisSectionLine].DirFlag && realElmoEncode > SectionLineList[indexOflisSectionLine].EncoderEnd) ||
                        (!SectionLineList[indexOflisSectionLine].DirFlag && realElmoEncode < SectionLineList[indexOflisSectionLine].EncoderEnd))
                {
                    if (indexOflisSectionLine == SectionLineList.Count - 1)
                        realElmoEncode = SectionLineList[indexOflisSectionLine].EncoderEnd;
                    else
                    {
                        indexOflisSectionLine++;
                        UpdateReal();
                        return;
                    }
                }

                position.Real.Position = GetMapPosition(SectionLineList[indexOflisSectionLine], realElmoEncode);
            }
        }

        private bool IsLine(MapPosition start, MapPosition end, MapPosition nextEnd)
        {
            if (start.X == end.X && end.X == nextEnd.X)
                return true;
            else if (start.Y == end.X && end.Y == nextEnd.Y)
                return true;

            return false;
        }

        private void UpdateIndexOflisSectionLine()
        {
            if (SectionLineList.Count > indexOflisSectionLine + 1 && IsLine(SectionLineList[indexOflisSectionLine].Start,
                SectionLineList[indexOflisSectionLine].End, SectionLineList[indexOflisSectionLine + 1].End))
            {
                if (SectionLineList[indexOflisSectionLine + 1].Start.X == SectionLineList[indexOflisSectionLine + 1].End.X)
                {
                    if (SectionLineList[indexOflisSectionLine + 1].Start.Y > position.Barcode.Position.Y &&
                        position.Barcode.Position.Y > SectionLineList[indexOflisSectionLine + 1].End.Y)
                        indexOflisSectionLine++;
                    else if (SectionLineList[indexOflisSectionLine + 1].Start.Y < position.Barcode.Position.Y &&
                        position.Barcode.Position.Y < SectionLineList[indexOflisSectionLine + 1].End.Y)
                        indexOflisSectionLine++;
                }
                else if (SectionLineList[indexOflisSectionLine + 1].Start.Y == SectionLineList[indexOflisSectionLine + 1].End.Y)
                {
                    if (SectionLineList[indexOflisSectionLine + 1].Start.X > position.Barcode.Position.X &&
                        position.Barcode.Position.X > SectionLineList[indexOflisSectionLine + 1].End.X)
                        indexOflisSectionLine++;
                    else if (SectionLineList[indexOflisSectionLine + 1].Start.X < position.Barcode.Position.X &&
                        position.Barcode.Position.X < SectionLineList[indexOflisSectionLine + 1].End.X)
                        indexOflisSectionLine++;
                }
            }
        }

        private void UpdateDelta()
        {
            if (SectionLineList.Count != 0)
            {
                UpdateIndexOflisSectionLine();
                double realEncoder = MapPositionToEncoder(SectionLineList[indexOflisSectionLine], position.Barcode.Position);
                // 要考慮時間差
                position.Delta = realEncoder - (position.ElmoEncoder + position.Offset);
                position.Delta = position.Delta + ((double)position.ScanTime + (DateTime.Now - position.BarcodeGetDataTime).TotalMilliseconds) * (controlData.DirFlag ? position.XFLVelocity : -position.XFLVelocity) / 1000;
            }
        }

        private void UpdatePositionMoving(bool newBarcodeData)
        {
            if (MoveState != EnumMoveState.TR && MoveState != EnumMoveState.R2000 && newBarcodeData)
                UpdateDelta();

            UpdateReal();
        }

        private void UpdatePositionStopping()
        {
            if (position.Real == null && position.Barcode != null)
                position.Real = position.Barcode;
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

        private void UpdatePosition()
        {
            bool newBarcodeData = false;

            ElmoAxisFeedbackData elmoData = elmoDriver.ElmoGetFeedbackData(Axis.XFL);
            if (elmoData != null)
            {
                position.XFLVelocity = elmoData.Feedback_Velocity;
                position.XRRVelocity = elmoDriver.ElmoGetVelocity(Axis.XRR);
                position.ElmoGetDataTime = elmoData.GetDataTime;
                position.ElmoEncoder = elmoData.Feedback_Position +
                (controlData.DirFlag ? position.XFLVelocity : -position.XFLVelocity) *
                ((DateTime.Now - position.ElmoGetDataTime).TotalMilliseconds + moveControlConfig.SleepTime / 2) / 1000;
            }

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

            if (agvPosition != null && !(position.LastBarcodeCount == agvPosition.Count && position.IndexOfSr2000List == index))
            {
                position.Barcode = agvPosition;
                position.ScanTime = agvPosition.ScanTime;
                position.BarcodeGetDataTime = agvPosition.GetDataTime;
                newBarcodeData = true;
            }
            else if (DriverSr2000List.Count == 0 && position.Barcode == null)
            {   // fake data
                MapPosition tempPosition = new MapPosition(0, 0);
                position.Barcode = new AGVPosition(tempPosition, 0, 0, 20, DateTime.Now, 0, 0);
            }

            if (MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.MoveComplete)
            {
                if (MoveState != EnumMoveState.TR && MoveState != EnumMoveState.R2000 && newBarcodeData)
                    UpdateDelta();

                UpdateReal();
            }
            else
            {
                if (position.Real == null && position.Barcode != null)
                    position.Real = position.Barcode;
            }
        }
        #endregion

        #region CommandControl
        private void TRControl(int wheelAngle, EnumAddressAction type)
        {
            MoveState = EnumMoveState.TR;

            double velocity = moveControlConfig.TR[type].Velocity;
            double r = moveControlConfig.TR[type].R;

            if (Math.Abs(position.XFLVelocity - velocity) >= velocity * moveControlConfig.TurnSpeedSafetyRange &&
                Math.Abs(position.XRRVelocity - velocity) >= velocity * moveControlConfig.TurnSpeedSafetyRange)
            { // Normal
                if (!elmoDriver.MoveCompelete(Axis.GT))
                    Console.WriteLine("GT Moving~~");

                elmoDriver.ElmoMove(Axis.GT, wheelAngle, moveControlConfig.TR[type].AxisParameter.Velocity, MoveType.Absolute,
                                    moveControlConfig.TR[type].AxisParameter.Acceleration,
                                    moveControlConfig.TR[type].AxisParameter.Deceleration,
                                    moveControlConfig.TR[type].AxisParameter.Jerk);
            }
            else if (Math.Abs(position.XFLVelocity - position.XRRVelocity) > 2 * velocity * moveControlConfig.TurnSpeedSafetyRange)
            { // GG,不該發生.
                // log..
                EMSControl();
            }
            else if (position.XFLVelocity > velocity && position.XRRVelocity > velocity)
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
                    BeamSensorOnlyOnOff((controlData.DirFlag ? BeamSensorLocate.Front : BeamSensorLocate.Back), true);
                    break;
                case 90:
                    BeamSensorOnlyOnOff((controlData.DirFlag ? BeamSensorLocate.Left : BeamSensorLocate.Right), true);
                    break;
                case -90:
                    BeamSensorOnlyOnOff((controlData.DirFlag ? BeamSensorLocate.Right : BeamSensorLocate.Left), true);
                    break;
                default:
                    // 不該發生 log..
                    EMSControl();
                    break;
            }

            controlData.WheelAngle = wheelAngle;

            MoveState = EnumMoveState.Idle;
            StopControl();
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
            elmoDriver.ElmoGroupVelocityChange(Axis.GX, velocity);

            if (isTRVChange)
            {
                switch (TRWheelAngle)
                {
                    case 0:
                        BeamSensorSingleOnOff((controlData.DirFlag ? BeamSensorLocate.Front : BeamSensorLocate.Back), true);
                        break;
                    case 90:
                        BeamSensorSingleOnOff((controlData.DirFlag ? BeamSensorLocate.Left : BeamSensorLocate.Right), true);
                        break;
                    case -90:
                        BeamSensorSingleOnOff((controlData.DirFlag ? BeamSensorLocate.Right : BeamSensorLocate.Left), true);
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
            elmoDriver.ElmoMove(Axis.GT, wheelAngle, moveControlConfig.Turn.Velocity, MoveType.Absolute,
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
                    BeamSensorSingleOnOff((controlData.DirFlag ? BeamSensorLocate.Front : BeamSensorLocate.Back), true);
                    break;
                case 90: // 朝左.
                    BeamSensorSingleOnOff((controlData.DirFlag ? BeamSensorLocate.Left : BeamSensorLocate.Right), true);
                    break;
                case -90: // 朝右.
                    BeamSensorSingleOnOff((controlData.DirFlag ? BeamSensorLocate.Right : BeamSensorLocate.Left), true);
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
                elmoDriver.ElmoMove(Axis.GX, distance, velocity, MoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            else
                elmoDriver.ElmoMove(Axis.GX, -distance, velocity, MoveType.Relative, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
        }

        private void SlowStopControl(MapPosition endPosition)
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "SlowStop!");
            elmoDriver.ElmoStop(Axis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Reset();
            timer.Start();
            while (!elmoDriver.MoveCompelete(Axis.GX) && timer.ElapsedMilliseconds < moveControlConfig.SlowStopTimeoutValue)
                Thread.Sleep(50);

            if (!elmoDriver.MoveCompelete(Axis.GX))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " : SlowStop Timeout!");
                EMSControl();
                return;
            }

            BeamSensorCloseAll();
        }

        private void SecondCorrectionControl(MapPosition endPosition)
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "Move Compelete !");
            MoveState = EnumMoveState.Idle;
        }

        private void StopControl()
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "Stop!");
            elmoDriver.ElmoStop(Axis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            elmoDriver.ElmoStop(Axis.GT, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);
        }

        private void EMSControl()
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "EMS!");
            elmoDriver.DisableAllAxis();
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
                        if (position.RealEncoder > cmd.TriggerEncoder + cmd.SafetyDistance)
                        {
                            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "超過Triiger觸發區間!");
                            controlData.MoveControlStop = true;
                            return false;
                        }
                        else if (position.RealEncoder > cmd.TriggerEncoder)
                        {
                            if (CheckGetNextReserve(cmd))
                            {
                                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "觸發, Real Encoder : " + position.RealEncoder.ToString("0") +
                                                                           ", command type : " + cmd.CmdType.ToString() + "~");
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                    else
                    {
                        if (position.RealEncoder < cmd.TriggerEncoder - cmd.SafetyDistance)
                        {
                            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "超過Triiger觸發區間!");
                            controlData.MoveControlStop = true;
                            return false;
                        }
                        else if (position.RealEncoder < cmd.TriggerEncoder)
                        {
                            if (CheckGetNextReserve(cmd))
                            {
                                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "觸發, Real Encoder : " + position.RealEncoder.ToString("0") +
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

        private void MoveControlThread()
        {
            double[] reviseWheelAngle = new double[4];
            while (true)
            {
                UpdatePosition();

                if (!controlData.MoveControlStop && MoveState != EnumMoveState.Idle && MoveState != EnumMoveState.MoveComplete)
                {
                    if (CommandList.Count != 0 && indexOfCmdList < CommandList.Count && TriggerCommand(CommandList[indexOfCmdList]))
                    {
                        Console.WriteLine("trigger : encoder = " + position.RealEncoder + " , command = " + CommandList[indexOfCmdList].CmdType.ToString());

                        switch (CommandList[indexOfCmdList].CmdType)
                        {
                            case CommandType.TR:
                                TRControl(CommandList[indexOfCmdList].WheelAngle, CommandList[indexOfCmdList].TRType);
                                break;
                            case CommandType.R2000:
                                R2000Control(CommandList[indexOfCmdList].WheelAngle);
                                break;
                            case CommandType.Vchange:
                                VchangeControl(CommandList[indexOfCmdList].Velocity);
                                break;
                            case CommandType.ReviseOpen:
                                if (controlData.OntimeReviseFlag == false)
                                {
                                    agvRevise.SettingReviseData(controlData.VelocityCommand, controlData.DirFlag);
                                    controlData.OntimeReviseFlag = true;
                                }

                                break;
                            case CommandType.ReviseClose:
                                controlData.OntimeReviseFlag = false;
                                elmoDriver.ElmoStop(Axis.GT);
                                break;
                            case CommandType.Move:
                                MoveCommandControl(CommandList[indexOfCmdList].Velocity, CommandList[indexOfCmdList].Distance, CommandList[indexOfCmdList].DirFlag,
                                                   CommandList[indexOfCmdList].WheelAngle, CommandList[indexOfCmdList].IsFirstMove);
                                break;
                            case CommandType.SlowStop:
                                SlowStopControl(CommandList[indexOfCmdList].EndPosition);
                                break;
                            case CommandType.End:
                                SlowStopControl(CommandList[indexOfCmdList].EndPosition);
                                SecondCorrectionControl(CommandList[indexOfCmdList].EndPosition);
                                break;
                            case CommandType.Stop:
                                StopControl();
                                break;
                            default:
                                break;
                        }

                        indexOfCmdList++;
                    }
                }

                if (controlData.OntimeReviseFlag && !controlData.MoveControlStop && MoveState == EnumMoveState.Moving)
                {
                    if (agvRevise.OntimeRevise(ref reviseWheelAngle, controlData.WheelAngle))
                    {
                        elmoDriver.ElmoMove(Axis.GT, reviseWheelAngle[0], reviseWheelAngle[1], reviseWheelAngle[2], reviseWheelAngle[3],
                                          ontimeReviseConfig.ThetaSpeed, MoveType.Absolute, moveControlConfig.Turn.Acceleration,
                                          moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);
                    }
                }

                if (controlData.MoveControlStop)
                {
                    Console.WriteLine("??");
                    StopControl();
                    controlData.MoveControlStop = false;
                }

                Thread.Sleep(moveControlConfig.SleepTime);
            }
        }

        /// <summary>
        ///  when move finished, call this function to notice other class instance that move is finished with status
        /// </summary>
        private void MoveFinished(MoveComplete status)
        {
            OnMoveFinished?.Invoke(this, status);
        }

        private void ResetEncoder(MapPosition start, MapPosition end, bool dirFlag)
        {
            MapPosition nowPosition;

            double elmoEncoder = elmoDriver.ElmoGetPosition(Axis.XFL);// 更新elmo encoder(走行距離).

            if (position.Real != null)
                nowPosition = position.Real.Position;
            else
            {
                EMSControl();
                // log..
                return;
            }

            if (start.X == end.X)
            {
                if (dirFlag)
                    position.Offset = -elmoEncoder + nowPosition.Y - start.Y;
                else
                    position.Offset = -elmoEncoder + nowPosition.Y - start.Y;
            }
            else if (start.Y == end.Y)
            {
                if (dirFlag)
                    position.Offset = -elmoEncoder + nowPosition.X - start.X;
                else
                    position.Offset = -elmoEncoder + nowPosition.X - start.X;
            }
            else
            {
                // R2000 start. by pass
                controlData.MoveControlStop = true;
            }

            position.Delta = 0;
        }

        public bool TransferMove(MoveCmdInfo moveCmd)
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "接收到AGVM命令~\n");
            //MoveCmdInfo moveCmdColne = moveCmd.DeepClone();
            MoveCmdInfo moveCmdColne = moveCmd;

            string errorMessage = "";

            if ((MoveState != EnumMoveState.MoveComplete && MoveState != EnumMoveState.Idle))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "移動中,因此無視~!\n");
                return false;
            }

            List<Command> moveCmdList = new List<Command>();
            List<SectionLine> sectionLineList = new List<SectionLine>();
            List<ReserveData> reserveDataList = new List<ReserveData>();

            if (!createMoveControlList.CreatMoveControlListSectionListReserveList(moveCmd, ref moveCmdList, ref sectionLineList, ref reserveDataList,
                                                                                  position.Real, ref errorMessage))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "命令分解失敗~!\n");
                return false;
            }

            elmoDriver.SetPosition(Axis.GX, 0);
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

            MoveState = EnumMoveState.Moving;
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "開始執行動作~!\n");
            return true;
        }


        public bool TransferMoveDebugMode(List<Command> moveCmdList, List<SectionLine> sectionLineList, List<ReserveData> reserveDataList)
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "接收到Debug Mode form命令~\n");

            ReserveList = reserveDataList;
            SectionLineList = sectionLineList;
            indexOflisSectionLine = 0;
            CommandList = moveCmdList;
            indexOfCmdList = 0;

            elmoDriver.SetPosition(Axis.GX, 0);
            ResetEncoder(sectionLineList[0].Start, sectionLineList[0].End, sectionLineList[0].DirFlag);

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

        private void BeamSensorSingleOnOff(BeamSensorLocate locate, bool flag)
        {
            flow.SavePureLog("Beam sensor 切換 : 修改 " + locate.ToString() + " 變更為 " + (flag ? "On" : "Off") + " !");

        }

        private void BeamSensorOnlyOnOff(BeamSensorLocate locate, bool flag)
        {
            flow.SavePureLog("Beam sensor 切換 : 只剩 " + locate.ToString() + " !");

        }

        private void BeamSensorCloseAll()
        {
            flow.SavePureLog("Beam sensor 切換 : 全部關掉!");

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

        #region CSV log
        private void WriteLogCSV()
        {
            string csvLog, debugList = "";

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            ElmoAxisFeedbackData feedBackData;
            Axis[] order = new Axis[18] { Axis.XFL, Axis.XFR, Axis.XRL, Axis.XRR,
                                          Axis.TFL, Axis.TFR, Axis.TRL, Axis.TRR,
                                          Axis.VXFL, Axis.VXFR, Axis.VXRL, Axis.VXRR,
                                          Axis.VTFL, Axis.VTFR, Axis.VTRL, Axis.VTRR,
                                          Axis.GX, Axis.GT };
            AGVPosition logAGVPosition;
            ThetaSectionDeviation logThetaDeviation;
            DateTime now;

            bool tempBoolean;
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
                if (csvLogResult)
                    debugList = now.ToString("HH:mm:ss.fff");

                //  State
                csvLog = csvLog + "," + MoveState.ToString();
                if (csvLogResult)
                    debugList = debugList + "\t" + MoveState.ToString();

                //  RealEncoder
                csvLog = csvLog + "," + position.RealEncoder.ToString("0.0");
                if (csvLogResult)
                    debugList = debugList + "\t" + position.RealEncoder.ToString("0.0");

                //  NextCommand	TriggerEncoder
                if (MoveState != EnumMoveState.Idle && indexOfCmdList < CommandList.Count)
                {
                    csvLog = csvLog + "," + CommandList[indexOfCmdList].CmdType.ToString();
                    if (csvLogResult)
                        debugList = debugList + "\t" + CommandList[indexOfCmdList].CmdType.ToString();

                    if (CommandList[indexOfCmdList].Position != null)
                    {
                        csvLog = csvLog + "," + CommandList[indexOfCmdList].TriggerEncoder.ToString();
                        if (csvLogResult)
                            debugList = debugList + "\t" + CommandList[indexOfCmdList].TriggerEncoder.ToString();
                    }
                    else
                    {
                        csvLog = csvLog + ",now";
                        if (csvLogResult)
                            debugList = debugList + "\tnow";
                    }
                }
                else
                {
                    csvLog = csvLog + ",N/A,N/A";
                    if (csvLogResult)
                        debugList = debugList + "\tN/A\tN/A";
                }

                //  Delta
                csvLog = csvLog + "," + position.Delta.ToString("0.0");
                if (csvLogResult)
                    debugList = debugList + "\t" + position.Delta.ToString("0.0");

                //  Offset
                csvLog = csvLog + "," + position.Offset.ToString("0.0");

                //  RealPosition
                logAGVPosition = position.Real;
                if (logAGVPosition != null)
                {
                    csvLog = csvLog + "," + logAGVPosition.Position.X.ToString("0.0") + "," + logAGVPosition.Position.Y.ToString("0.0");
                    if (csvLogResult)
                        debugList = debugList + "\t" + logAGVPosition.Position.X.ToString("0.0") + "\t" + logAGVPosition.Position.Y.ToString("0.0");
                }
                else
                {
                    csvLog = csvLog + ",null,null";
                    if (csvLogResult)
                        debugList = debugList + "\tnull\tnull";
                }

                //  BarcodePosition
                //  X Y
                logAGVPosition = position.Barcode;
                if (logAGVPosition != null)
                {
                    csvLog = csvLog + "," + logAGVPosition.Position.X.ToString("0.0") + "," + logAGVPosition.Position.Y.ToString("0.0");
                    if (csvLogResult)
                        debugList = debugList + "\t" + logAGVPosition.Position.X.ToString("0.0") + "\t" + logAGVPosition.Position.Y.ToString("0.0");
                }
                else
                {
                    csvLog = csvLog + ",null,null";
                    if (csvLogResult)
                        debugList = debugList + "\tnull\tnull";
                }

                //  SR2000
                //  count	scanTime	X	Y	theta   BarcodeAngle    delta	theta   
                for (int i = 0; i < 2; i++)
                {
                    if (DriverSr2000List.Count > i)
                    {
                        logAGVPosition = DriverSr2000List[i].GetAGVPosition();
                        logThetaDeviation = DriverSr2000List[i].GetThetaSectionDeviation();

                        if (logAGVPosition != null)
                        {
                            csvLog = csvLog + "," + logAGVPosition.Count.ToString("0");
                            csvLog = csvLog + "," + logAGVPosition.ScanTime.ToString("0");
                            csvLog = csvLog + "," + logAGVPosition.Position.X.ToString("0.0");
                            csvLog = csvLog + "," + logAGVPosition.Position.Y.ToString("0.0");
                            csvLog = csvLog + "," + logAGVPosition.AGVAngle.ToString("0.0");
                            csvLog = csvLog + "," + logAGVPosition.BarcodeAngleInMap.ToString("0.0");
                        }
                        else
                            csvLog = csvLog + ",N/A,N/A,N/A,N/A,N/A,N/A";

                        if (logThetaDeviation != null)
                        {
                            csvLog = csvLog + "," + logThetaDeviation.Theta.ToString("0.0");
                            csvLog = csvLog + "," + logThetaDeviation.SectionDeviation.ToString("0.0");
                        }
                        else
                            csvLog = csvLog + ",N/A,N/A";
                    }
                    else
                        csvLog = csvLog + ",N/A,N/A,N/A,N/A,N/A,N/A,N/A,N/A";
                }

                //  Elmo
                //  count   position	velocity	toc	disable	moveComplete	error
                for (int i = 0; i < 8; i++)
                {
                    feedBackData = elmoDriver.ElmoGetFeedbackData(order[i]);
                    if (feedBackData != null)
                    {
                        csvLog = csvLog + "," + feedBackData.Count + "," + feedBackData.Feedback_Position + "," +
                                                feedBackData.Feedback_Velocity + "," + feedBackData.Feedback_Torque + "," +
                                                (feedBackData.Disable ? "Disable" : "Enable") + "," +
                                                (feedBackData.StandStill ? "Stop" : "Move") + "," + (feedBackData.ErrorStop ? "Error" : "Normal");

                        if (csvLogResult && (order[i] == Axis.XFL) || order[i] == Axis.TFL)
                            debugList = debugList + "\t" + feedBackData.Feedback_Position + "\t" + feedBackData.Feedback_Velocity;
                    }
                    else
                    {
                        csvLog = csvLog + ",N/A,N/A,N/A,N/A,N/A,N/A,N/A";

                        if (csvLogResult && (order[i] == Axis.XFL) || order[i] == Axis.TFL)
                            debugList = debugList + "\tN/A\tN/A";
                    }
                }

                for (int i = 8; i < 16; i++)
                {
                    feedBackData = elmoDriver.ElmoGetFeedbackData(order[i]);
                    if (feedBackData != null)
                    {
                        csvLog = csvLog + "," + feedBackData.Count + "," + feedBackData.Feedback_Position + "," +
                                                (feedBackData.Disable ? "Disable" : "Enable") + "," +
                                                (feedBackData.StandStill ? "Stop" : "Move") + "," + (feedBackData.ErrorStop ? "Error" : "Normal");
                    }
                    else
                    {
                        csvLog = csvLog + ",N/A,N/A,N/A,N/A,N/A";
                    }
                }

                for (int i = 16; i < 18; i++)
                {
                    tempBoolean = elmoDriver.MoveCompelete(order[i]);
                    csvLog = csvLog + (tempBoolean ? "Disable" : "Enable");
                }

                logger.SavePureLog(csvLog);
                if (csvLogResult)
                    debugCsvLogList.Add(debugList);

                while (timer.ElapsedMilliseconds < moveControlConfig.CSVLogInterval)
                    Thread.Sleep(1);
            }
        }
        #endregion
    }
}
