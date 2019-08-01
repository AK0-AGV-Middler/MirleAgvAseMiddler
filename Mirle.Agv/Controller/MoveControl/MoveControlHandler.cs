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
        private EnumMoveState moveState;
        public MoveControlConfig moveControlConfig;
        private MapInfo theMapInfo = new MapInfo();
        private Logger logger = LoggerAgent.Instance.GetLooger("MoveControlCSV");
        private Logger flow = LoggerAgent.Instance.GetLooger("MoveControl");

        public ElmoDriver elmoDriver;
        public List<Sr2000Driver> DriverSr2000List = new List<Sr2000Driver>();
        public OntimeReviseConfig ontimeReviseConfig = null;
        private ReviseParameter reviseParameter;

        public event EventHandler<EnumMoveComplete> OnMoveFinished;

        private List<SectionLine> SectionLineList = new List<SectionLine>();
        private int indexOflisSectionLine = 0;
        public Position position = new Position();

        private List<Command> CommandList = new List<Command>();
        private int indexOfCmdList = 0;

        private MoveControlParameter controlData = new MoveControlParameter();
        private const int AllowableTheta = 10;
        private List<ReserveData> ReserveList;

        public MoveControlHandler(string moveControlConfigPath, MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
            moveControlConfig = new MoveControlConfig();

            InitailSr2000(moveControlConfig.Sr2000ConfigPath);
            elmoDriver = new ElmoDriver("MotionParameter.xml");

            ReadOntimeReviseConfigXML(moveControlConfig.OnTimeReviseConfigPath);
            createMoveControlList = new CreateMoveControlList(DriverSr2000List, moveControlConfig);

            controlData.MoveControlThread = new Thread(MoveControlThread);
            controlData.MoveControlThread.Start();
            moveState = EnumMoveState.Idle;

            elmoDriver.ElmoStop(EnumAxis.GX);
            elmoDriver.ElmoMove(EnumAxis.GT, 0, 75, EnumMoveType.Absolute);
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
            reviseParameter = new ReviseParameter(ontimeReviseConfig, 100);
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

        #region OntimeRevise
        public void SettingReviseData(double velocity)
        {
            reviseParameter = new ReviseParameter(ontimeReviseConfig, velocity);
        }

        public void SettingReviseDirFlag(bool dirFlag)
        {
            controlData.DirFlag = dirFlag;
        }

        private bool CheckWorse(double value, double oldValue)
        {
            if (oldValue == 0)
                return false;
            else if (oldValue > 0)
                return value > oldValue;
            else
                return value < oldValue;
        }

        private bool LineRevise(ref double[] wheelTheta, double theta, double sectionDeviation)
        {
            if ((reviseParameter.ReviseType == EnumLineReviseType.Theta || theta > reviseParameter.ModifyTheta || theta < -reviseParameter.ModifyTheta) &&
                sectionDeviation < reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                sectionDeviation > -reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
            {
                if ((theta < reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta &&
                     theta > -reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta))
                {
                    reviseParameter.ReviseType = EnumLineReviseType.None;
                    wheelTheta = new double[4] { 0, 0, 0, 0 };
                    return true;
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.Theta;
                    reviseParameter.ReviseValue = theta;
                    double turnTheta = theta / reviseParameter.ModifyTheta / ontimeReviseConfig.LinePriority.Theta * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    turnTheta = controlData.DirFlag ? -turnTheta : turnTheta;
                    wheelTheta = new double[4] { turnTheta, turnTheta, -turnTheta, -turnTheta };
                    return true;
                }
            }
            else if (reviseParameter.ReviseType == EnumLineReviseType.SectionDeviation || sectionDeviation > reviseParameter.ModifySectionDeviation
                                                                                   || sectionDeviation < -reviseParameter.ModifySectionDeviation)
            {
                if (sectionDeviation < reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                    sectionDeviation > -reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
                {
                    reviseParameter.ReviseType = EnumLineReviseType.None;
                    wheelTheta = new double[4] { 0, 0, 0, 0 };
                    return true;
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.SectionDeviation;
                    reviseParameter.ReviseValue = sectionDeviation;
                    double turnTheta = sectionDeviation / reviseParameter.ModifySectionDeviation / ontimeReviseConfig.LinePriority.SectionDeviation * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    turnTheta = controlData.DirFlag ? turnTheta : -turnTheta;
                    wheelTheta = new double[4] { turnTheta, turnTheta, turnTheta, turnTheta };
                    return true;
                }
            }
            else
            {
                reviseParameter.ReviseType = EnumLineReviseType.None;
                wheelTheta = new double[4] { 0, 0, 0, 0 };
                return true;
            }
        }

        private bool LineRevise_Old(ref double[] wheelTheta, double theta, double sectionDeviation)
        {
            if ((reviseParameter.ReviseType == EnumLineReviseType.Theta || theta > reviseParameter.ModifyTheta || -theta < -reviseParameter.ModifyTheta) &&
                sectionDeviation > reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation ||
                -sectionDeviation < -reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
            {
                if (reviseParameter.ReviseType == EnumLineReviseType.Theta)
                {
                    if (CheckWorse(theta, reviseParameter.ReviseValue))
                    {
                        reviseParameter.ReviseType = EnumLineReviseType.None;
                        // log 變糟糕,修反or直線性太差.
                        wheelTheta = new double[4] { 0, 0, 0, 0 };
                        Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation + ", 0 0 0 0 (變糟)");
                        return true;
                    }

                    if ((theta < reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta &&
                        -theta > -reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta))
                    {
                        reviseParameter.ReviseType = EnumLineReviseType.None;
                        wheelTheta = new double[4] { 0, 0, 0, 0 };
                        Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation + ", 0 0 0 0 (修好)");
                        return true;
                    }
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.Theta;
                    reviseParameter.ReviseValue = theta;
                    double turnTheta;

                    turnTheta = theta / reviseParameter.ModifyTheta / ontimeReviseConfig.LinePriority.Theta * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    turnTheta = controlData.DirFlag ? -turnTheta : turnTheta;
                    wheelTheta = new double[4] { turnTheta, turnTheta, -turnTheta, -turnTheta };
                    Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation + ", (修Theta)前輪 : " + turnTheta.ToString("0.00"));
                    return true;
                }
            }
            else if (reviseParameter.ReviseType == EnumLineReviseType.SectionDeviation || sectionDeviation > reviseParameter.ModifySectionDeviation
                                                                                   || -sectionDeviation < -reviseParameter.ModifySectionDeviation)
            {
                if (reviseParameter.ReviseType == EnumLineReviseType.SectionDeviation)
                {
                    if (CheckWorse(sectionDeviation, reviseParameter.ReviseValue))
                    {
                        reviseParameter.ReviseType = EnumLineReviseType.None;
                        // log 變糟糕,修反or直線性太差.
                        wheelTheta = new double[4] { 0, 0, 0, 0 };
                        Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation + ", 0 0 0 0 (變糟)");
                        return true;
                    }

                    if (sectionDeviation < reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                        -sectionDeviation > -reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
                    {
                        reviseParameter.ReviseType = EnumLineReviseType.None;
                        wheelTheta = new double[4] { 0, 0, 0, 0 };
                        Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation + ", 0 0 0 0 (修好)");
                        return true;
                    }
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.SectionDeviation;
                    reviseParameter.ReviseValue = sectionDeviation;
                    double turnTheta;

                    turnTheta = sectionDeviation / reviseParameter.ModifySectionDeviation / ontimeReviseConfig.LinePriority.SectionDeviation * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    turnTheta = controlData.DirFlag ? -turnTheta : turnTheta;
                    wheelTheta = new double[4] { turnTheta, turnTheta, turnTheta, turnTheta };
                    Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation + ", (修sectionDeviation)前輪 : " + turnTheta.ToString("0.00"));
                    return true;
                }
            }
            else
            {
                reviseParameter.ReviseType = EnumLineReviseType.None;
                wheelTheta = new double[4] { 0, 0, 0, 0 };
                return true;
            }

            return true;
        }

        private bool HorizontalRevise(ref double[] wheelTheta, double theta, double sectionDeviation, int wheelAngle)
        {
            if ((reviseParameter.ReviseType == EnumLineReviseType.Theta || theta > reviseParameter.ModifyTheta || -theta < -reviseParameter.ModifyTheta) &&
                sectionDeviation < reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                sectionDeviation > -reviseParameter.ModifySectionDeviation * ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
            {
                if ((theta < reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta &&
                    -theta > -reviseParameter.ModifyTheta / ontimeReviseConfig.Return0ThetaPriority.Theta))
                {
                    reviseParameter.ReviseType = EnumLineReviseType.None;
                    wheelTheta = new double[4] { 0, 0, 0, 0 };
                    Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation.ToString("0.00") + ", 0 0 0 0 (修好)");
                    return true;
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.Theta;
                    reviseParameter.ReviseValue = theta;
                    double turnTheta = theta / reviseParameter.ModifyTheta / ontimeReviseConfig.LinePriority.Theta * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    turnTheta = controlData.DirFlag ? -turnTheta : turnTheta;
                    wheelTheta = new double[4] { -turnTheta, turnTheta, -turnTheta, turnTheta };
                    Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation.ToString("0.00") + ", (修Theta)前輪 : " + turnTheta.ToString("0.00"));
                    return true;
                }
            }
            else if (reviseParameter.ReviseType == EnumLineReviseType.SectionDeviation || sectionDeviation > reviseParameter.ModifySectionDeviation
                                                                                   || -sectionDeviation < -reviseParameter.ModifySectionDeviation)
            {
                if (sectionDeviation < reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation &&
                    -sectionDeviation > -reviseParameter.ModifySectionDeviation / ontimeReviseConfig.Return0ThetaPriority.SectionDeviation)
                {
                    reviseParameter.ReviseType = EnumLineReviseType.None;
                    wheelTheta = new double[4] { 0, 0, 0, 0 };
                    Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation.ToString("0.00") + ", 0 0 0 0 (修好)");
                    return true;
                }
                else
                {
                    reviseParameter.ReviseType = EnumLineReviseType.SectionDeviation;
                    reviseParameter.ReviseValue = sectionDeviation;
                    double turnTheta = sectionDeviation / reviseParameter.ModifySectionDeviation / ontimeReviseConfig.LinePriority.SectionDeviation * reviseParameter.MaxTheta;

                    if (turnTheta > reviseParameter.MaxTheta)
                        turnTheta = reviseParameter.MaxTheta;
                    else if (turnTheta < -reviseParameter.MaxTheta)
                        turnTheta = -reviseParameter.MaxTheta;

                    turnTheta = controlData.DirFlag ? turnTheta : -turnTheta;
                    turnTheta = wheelAngle == -90 ? -turnTheta : turnTheta;
                    wheelTheta = new double[4] { turnTheta, turnTheta, turnTheta, turnTheta };
                    Console.WriteLine("theta : " + theta.ToString("0.00") + ", sectionDeviation : " + sectionDeviation.ToString("0.00") + ", (修sectionDeviation)前輪 : " + turnTheta.ToString("0.00"));
                    return true;
                }
            }
            else
            {
                reviseParameter.ReviseType = EnumLineReviseType.None;
                wheelTheta = new double[4] { 0, 0, 0, 0 };
                return true;
            }
        }

        public bool OntimeRevise(ref double[] wheelTheta, int wheelAngle = 0)
        {
            ThetaSectionDeviation reviseData = null;

            while (!elmoDriver.MoveCompelete(EnumAxis.GT))
                return false;

            for (int i = 0; i < DriverSr2000List.Count; i++)
            {
                reviseData = DriverSr2000List[i].GetThetaSectionDeviation();
                if (reviseData != null)
                {
                    // log??
                    break;
                }
            }

            if (reviseData == null)
            {
                wheelTheta = new double[4] { 0, 0, 0, 0 };
                return true;
            }
            else if (reviseParameter == null)
            {
                // log, 不該發生.
                return false;
            }
            else
            {
                if (wheelAngle == 0)
                    return LineRevise(ref wheelTheta, reviseData.Theta, reviseData.SectionDeviation);
                else
                    return HorizontalRevise(ref wheelTheta, reviseData.Theta, reviseData.SectionDeviation, wheelAngle);
            }
        }

        #endregion

        #region 更新Real Delta所用的小Function
        private MapPosition GetMapPosition(SectionLine sectionLine, double encode)
        {
            double x = sectionLine.Start.X + (sectionLine.End.X - sectionLine.Start.X) * (encode - sectionLine.EncoderStart) / (sectionLine.EncoderEnd - sectionLine.EncoderStart);
            double y = sectionLine.Start.Y + (sectionLine.End.Y - sectionLine.Start.Y) * (encode - sectionLine.EncoderStart) / (sectionLine.EncoderEnd - sectionLine.EncoderStart);
            MapPosition returnPosition = new MapPosition((float)x, (float)y);

            return returnPosition;
        }

        private bool IsBetweenIn(float target, float start, float end)
        {
            if (start > end)
                return start > target && target > end;
            else if (start < end)
                return start < target && target < end;
            else
                return false;
        }

        private double DistanceToAddresss(MapPosition target, MapPosition start, MapPosition end)
        {
            double distanceToStart = Math.Sqrt(Math.Pow(target.X - start.X, 2) + Math.Pow(target.Y - start.Y, 2));
            double distanceToEnd = Math.Sqrt(Math.Pow(target.X - end.X, 2) + Math.Pow(target.Y - end.Y, 2));

            if (distanceToStart < distanceToEnd)
                return distanceToStart;
            else
                return distanceToEnd;
        }

        private double GetDistanceToSection(int index)
        {
            if (SectionLineList[index].Start.X != SectionLineList[index].End.X && SectionLineList[index].Start.Y != SectionLineList[index].End.Y)
            {
                controlData.MoveControlThread.Abort();
                return -1;
            }

            if (SectionLineList[index].Start.X == SectionLineList[index].End.X)
            {
                if (IsBetweenIn(position.Barcode.Y, SectionLineList[index].Start.Y, SectionLineList[index].End.Y))
                { // 投影到Section的直線,且介於section起點終點之間.
                    return Math.Abs(SectionLineList[index].Start.X - position.Barcode.X);
                }
                else
                { // 不再Section之間,距離 = 到Start或End之距離.
                    return DistanceToAddresss(position.Barcode, SectionLineList[index].Start, SectionLineList[index].End);
                }
            }
            else if (SectionLineList[index].Start.Y == SectionLineList[index].End.Y)
            {
                if (IsBetweenIn(position.Barcode.X, SectionLineList[index].Start.X, SectionLineList[index].End.X))
                { // 投影到Section的直線,且介於section起點終點之間.
                    return Math.Abs(SectionLineList[index].Start.Y - position.Barcode.Y);
                }
                else
                { // 不再Section之間,距離 = 到Start或End之距離.
                    return DistanceToAddresss(position.Barcode, SectionLineList[index].Start, SectionLineList[index].End);
                }
            }
            else
            {
                controlData.MoveControlThread.Abort();
                return -1;
            }
        }

        private int ClosingSection()
        {
            double[] distance = new double[3] { -1, -1, -1 };
            int minIndex = -1;

            if (indexOflisSectionLine > 0)
                distance[0] = GetDistanceToSection(indexOflisSectionLine - 1);

            distance[1] = GetDistanceToSection(indexOflisSectionLine);

            if (indexOflisSectionLine < SectionLineList.Count - 1)
                distance[2] = GetDistanceToSection(indexOflisSectionLine + 1);

            for (int i = 0; i < 3; i++)
            {
                if (distance[i] != -1)
                {
                    if (minIndex == -1 || distance[minIndex] > distance[i])
                        minIndex = i;
                }
            }

            return indexOflisSectionLine + minIndex - 1;
        }

        private double MapPositionToEncoder(SectionLine sectionLine)
        {
            if (sectionLine.Start.X != sectionLine.End.X && sectionLine.Start.Y != sectionLine.End.Y)
            {
                // Error.
                return 0;
            }

            if (sectionLine.Start.X == sectionLine.End.X)
            {
                return sectionLine.EncoderStart + (sectionLine.EncoderEnd - sectionLine.EncoderStart) *
                      (position.Barcode.Y - sectionLine.Start.Y) / (sectionLine.End.Y - sectionLine.Start.Y);
            }
            else if (sectionLine.Start.Y == sectionLine.End.Y)
            {
                return sectionLine.EncoderStart + (sectionLine.EncoderEnd - sectionLine.EncoderStart) *
                      (position.Barcode.X - sectionLine.Start.X) / (sectionLine.End.X - sectionLine.Start.X);
            }
            else
            {
                // Error.
                return 0;
            }
        }

        #endregion

        #region Position { barcode, elmo encoder, delta, real }更新.
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

                position.Real = GetMapPosition(SectionLineList[indexOflisSectionLine], realElmoEncode);
                Vehicle.Instance.GetVehLoacation().RealPosition = position.Real;
            }
        }

        private void UpdateDelta()
        {
            if (SectionLineList.Count != 0)
            {
                int sectionIndex = ClosingSection();
                double realEncoder = MapPositionToEncoder(SectionLineList[sectionIndex]);
                // 要考慮時間差
                position.Delta = realEncoder - (position.ElmoEncoder + position.Offset);
                position.Delta = position.Delta + ((double)position.ScanTime + (position.BarcodeGetDataTime - position.ElmoGetDataTime).TotalMilliseconds) * (controlData.DirFlag ? position.XFLVelocity : -position.XFLVelocity) / 1000;
            }
        }

        private void UpdatePosition()
        {
            AGVPosition agvPosition = null;
            int index = 0;

            ElmoAxisFeedbackData elmoData = elmoDriver.ElmoGetFeedbackData(EnumAxis.XFL);

            if (elmoData == null)
                return;

            position.ElmoEncoder = elmoData.Feedback_Position;// 更新elmo encoder(走行距離).
            position.XFLVelocity = elmoData.Feedback_Velocity;
            position.XRRVelocity = elmoDriver.ElmoGetVelocity(EnumAxis.XRR);
            position.ElmoGetDataTime = elmoData.GetDataTime;

            if (moveState != EnumMoveState.Idle && moveState != EnumMoveState.MoveComplete)
            {
                //position.ElmoEncoder = elmoData.Feedback_Position +
                //(controlData.DirFlag ? position.XFLVelocity : -position.XFLVelocity) * (DateTime.Now - position.ElmoGetDataTime).TotalMilliseconds / 1000;
                // 置中. testing.
                position.ElmoEncoder = elmoData.Feedback_Position +
                (controlData.DirFlag ? position.XFLVelocity : -position.XFLVelocity) *
                ((DateTime.Now - position.ElmoGetDataTime).TotalMilliseconds + moveControlConfig.SleepTime / 2 + 200) / 1000;
            }
            else
                position.ElmoEncoder = elmoData.Feedback_Position;

            for (; index < DriverSr2000List.Count; index++)
            {
                agvPosition = DriverSr2000List[index].GetAGVPosition();
                if (agvPosition != null)
                    break;
            }

            if (agvPosition != null && !(position.LastBarcodeCount == agvPosition.Count && position.IndexOfSr2000List == index))
            {
                position.Barcode = agvPosition.Position;
                position.ScanTime = agvPosition.ScanTime;
                position.BarcodeGetDataTime = agvPosition.GetDataTime;

                if (moveState != EnumMoveState.Idle && moveState != EnumMoveState.MoveComplete)
                {
                    if (moveState != EnumMoveState.TR && moveState != EnumMoveState.R2000)
                        UpdateDelta();
                    else
                        ; // ???

                    UpdateReal();
                }
            }
        }
        #endregion

        #region CommandControl
        private void TRControl(double velocity, double r, int wheelAngle)
        {
            //...
            moveState = EnumMoveState.TR;
            double encoderTRStart = position.RealEncoder;
            Console.WriteLine("v : " + velocity.ToString("0") + ", r : " + r.ToString("0") + ", angle : " + wheelAngle.ToString());
            Console.WriteLine("v(FL) : " + position.XFLVelocity.ToString("0") + ", v(RR) : " + position.XRRVelocity.ToString("0") + ", range : " + moveControlConfig.TurnSpeedSafetyRange.ToString());

            //wheelAngle = 90;
            if (Math.Abs(position.XFLVelocity - velocity) >= velocity * moveControlConfig.TurnSpeedSafetyRange &&
                Math.Abs(position.XRRVelocity - velocity) >= velocity * moveControlConfig.TurnSpeedSafetyRange)
            { // Normal
                elmoDriver.ElmoMove(EnumAxis.GT, wheelAngle, moveControlConfig.TR.Velocity, EnumMoveType.Absolute,
                                    moveControlConfig.TR.Acceleration, moveControlConfig.TR.Deceleration, moveControlConfig.TR.Jerk);
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

            while (!elmoDriver.WheelAngleCompare(wheelAngle, moveControlConfig.StartWheelAngleRange))
            {
                // 過半保護.

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
            Console.WriteLine("TR Compelete!");
            StopControl();
            SecondCorrectionControl(null);
            //moveState = EnumMoveState.Moving;　// 讓她不會執行list內容和修正.
        }

        private void R2000Control(double velocity, double r/*, R2000Config r2000config*/)
        {

        }

        private void VchangeControl(double velocity, bool isTRVChange = false, int TRWheelAngle = 0)
        {
            velocity /= moveControlConfig.AGVMaxVelocity;
            controlData.VelocityCommand = velocity;

            reviseParameter = new ReviseParameter(ontimeReviseConfig, velocity, true);

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

            Thread.Sleep(50);
            while (!elmoDriver.MoveCompelete(EnumAxis.GT))
            {
                Thread.Sleep(50);
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
            BeamSensorCloseAll();
        }

        private void SecondCorrectionControl(MapPosition endPosition)
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "Move Compelete !");
            moveState = EnumMoveState.MoveComplete;
        }

        private void StopControl()
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "Stop!");
            elmoDriver.ElmoStop(EnumAxis.GX, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            elmoDriver.ElmoStop(EnumAxis.GT, moveControlConfig.Turn.Deceleration, moveControlConfig.Turn.Jerk);
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

                if (!controlData.MoveControlStop && moveState != EnumMoveState.Idle && moveState != EnumMoveState.MoveComplete)
                {
                    if (CommandList.Count != 0 && indexOfCmdList < CommandList.Count && TriggerCommand(CommandList[indexOfCmdList]))
                    {
                        Console.WriteLine("trigger : encoder = " + position.RealEncoder + " , command = " + CommandList[indexOfCmdList].CmdType.ToString());

                        switch (CommandList[indexOfCmdList].CmdType)
                        {
                            case EnumCommandType.TR:
                                TRControl(CommandList[indexOfCmdList].Velocity, CommandList[indexOfCmdList].Distance, CommandList[indexOfCmdList].WheelAngle);
                                break;
                            case EnumCommandType.R2000:
                                R2000Control(CommandList[indexOfCmdList].Velocity, CommandList[indexOfCmdList].Distance);
                                break;
                            case EnumCommandType.Vchange:
                                VchangeControl(CommandList[indexOfCmdList].Velocity);
                                break;
                            case EnumCommandType.ReviseOpen:
                                if (reviseParameter.OntimeReviseFlag == false)
                                    reviseParameter = new ReviseParameter(ontimeReviseConfig, controlData.VelocityCommand, true);
                                break;
                            case EnumCommandType.ReviseClose:
                                reviseParameter.OntimeReviseFlag = false;
                                elmoDriver.ElmoMove(EnumAxis.GT, 0, ontimeReviseConfig.ThetaSpeed, EnumMoveType.Absolute);
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
                                SecondCorrectionControl(CommandList[indexOfCmdList].EndPosition);
                                break;
                            case EnumCommandType.Stop:
                                StopControl();
                                break;
                            default:
                                break;
                        }

                        indexOfCmdList++;
                    }
                }

                if (reviseParameter.OntimeReviseFlag && !controlData.MoveControlStop && moveState == EnumMoveState.Moving)
                {
                    if (OntimeRevise(ref reviseWheelAngle, controlData.WheelAngle))
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

        /// <summary>
        ///  when move finished, call this function to notice other class instance that move is finished with status
        /// </summary>
        private void MoveFinished(EnumMoveComplete status)
        {
            OnMoveFinished?.Invoke(this, status);
        }

        private void ResetEncoder(MapPosition start, MapPosition end, bool dirFlag)
        {
            AGVPosition agvPosition = null;
            MapPosition nowPosition;

            double elmoEncoder = elmoDriver.ElmoGetPosition(EnumAxis.XFL);// 更新elmo encoder(走行距離).

            if (position.Real != null)
            {
                nowPosition = position.Real;
            }
            else
            {
                for (int i = 0; i < DriverSr2000List.Count; i++)
                {
                    agvPosition = DriverSr2000List[i].GetAGVPosition();
                    if (agvPosition != null)
                        break;
                }
            }

            if (agvPosition == null)
                return;
            else
                nowPosition = agvPosition.Position;

            if (start.X == end.X)
            {
                if (dirFlag)
                    position.Offset = -elmoEncoder + nowPosition.Y - start.Y;
                else
                    position.Offset = -elmoEncoder + nowPosition.Y + start.Y;
            }
            else if (start.Y == end.Y)
            {
                if (dirFlag)
                    position.Offset = -elmoEncoder + nowPosition.X - start.X;
                else
                    position.Offset = -elmoEncoder + nowPosition.X + start.X;
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

            if ((moveState != EnumMoveState.MoveComplete && moveState != EnumMoveState.Idle))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "移動中,因此無視~!\n");
                return false;
            }

            List<Command> moveCmdList = new List<Command>();
            List<SectionLine> sectionLineList = new List<SectionLine>();
            List<ReserveData> reserveDataList = new List<ReserveData>();

            if (!createMoveControlList.CreatMoveControlListSectionListReserveList(moveCmd, ref moveCmdList, ref sectionLineList, ref reserveDataList, null))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "命令分解失敗~!\n");
                return false;
            }

            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "命令分解成功~!\n");

            if (sectionLineList.Count > 0)
            {
                elmoDriver.SetPosition(EnumAxis.GX, 0);
                ResetEncoder(sectionLineList[0].Start, sectionLineList[0].End, sectionLineList[0].DirFlag);
            }
            else
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "命令資料為空(不該發生)~!\n");
                return false;
            }

            // 暫時直接取得所有Reserve
            for (int i = 0; i < reserveDataList.Count; i++)
                //if (i != 1)
                reserveDataList[i].GetReserve = true;

            ReserveList = reserveDataList;
            SectionLineList = sectionLineList;
            indexOflisSectionLine = 0;
            CommandList = moveCmdList;
            indexOfCmdList = 0;

            moveState = EnumMoveState.Moving;
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
            moveState = EnumMoveState.Idle;
        }

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

        private void WriteLogCSC()
        {

            logger.SavePureLog("A,B,C");
        }
    }
}
