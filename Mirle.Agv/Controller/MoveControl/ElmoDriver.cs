using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using ElmoMotionControlComponents.Drive.EASComponents;
using System.Threading;
using System.Net;
using System.Xml;
using Mirle.Agv.Model;
using System.IO;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Controller.Tools;

namespace Mirle.Agv.Controller
{
    public class ElmoDriver
    {
        Thread Read_BulkRead;

        private Logger elmoLogger = LoggerAgent.Instance.GetLooger("Elmo");

        private ushort MAX_AXIS;
        private string networkInterfaceCard = "";
        private string elmoControlIP = "";
        private int elmoControlPort = 0;
        private int handler = 0;

        private Dictionary<EnumAxis, AxisInfo> allAxis = new Dictionary<EnumAxis, AxisInfo>();
        private List<AxisInfo> allAxisList = new List<AxisInfo>();

        private List<ElmoSingleAxisConfig> elmoAxisConfig = new List<ElmoSingleAxisConfig>();
        private bool connected = false;
        private MMCBulkRead bulkRead;
        private NC_BULKREAD_PRESET_5[] ncBulkRead;

        private int readSleepTime;
        private int writeSleepTime;
        private int ServoOnTimeOut;
        private System.Diagnostics.Stopwatch scanTimeTimer = new System.Diagnostics.Stopwatch();
        private volatile double[] getPosFbk = new double[8];
        private Dictionary<EnumAxisType, AxisData> allType = new Dictionary<EnumAxisType, AxisData>();

        public ElmoDriver(string elmoConfigPath)
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");

            if (elmoConfigPath == null || elmoConfigPath == "")
            {
                elmoLogger.SavePureLog(this.GetType().FullName + " elmoConfigPath error ( = null or = \"\".");
                return;
            }

            try
            {
                ReadMotionParameter(elmoConfigPath);
                if (Connect())
                {
                    connected = true;
                    SetAllAxis();
                    Thread.Sleep(100);
                    DisableAllAxis();
                    EnableAllAxis();

                }
            }
            catch
            {
                elmoLogger.SavePureLog(this.GetType().FullName + " failed!");
                connected = false;
            }
        }

        #region 讀取XML.
        private AxisData ReadMoveTurnData(XmlElement element)
        {
            AxisData returnAxisData = new AxisData();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "MotorResolution":
                        returnAxisData.MotorResolution = double.Parse(item.InnerText);
                        break;
                    case "PulseUnit":
                        returnAxisData.PulseUnit = double.Parse(item.InnerText);
                        break;
                    case "Velocity":
                        returnAxisData.Velocity = double.Parse(item.InnerText);
                        break;
                    case "Acceleration":
                        returnAxisData.Acceleration = double.Parse(item.InnerText);
                        break;
                    case "Deceleration":
                        returnAxisData.Deceleration = double.Parse(item.InnerText);
                        break;
                    case "Jerk":
                        returnAxisData.Jerk = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return returnAxisData;
        }

        private void ReadIP(XmlElement element)
        {
            string splitString;
            string[] splitResult;
            AxisData tempAxisData;

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "NetworkInterfaceCard":
                        networkInterfaceCard = item.InnerText;
                        break;
                    case "ElmoControl":
                        splitString = item.InnerText;
                        splitResult = splitString.Split(':');

                        if (splitResult != null && splitResult.Count() == 2)
                        {
                            elmoControlIP = splitResult[0];
                            elmoControlPort = int.Parse(splitResult[1]);
                        }
                        else
                        {
                            // log...
                        }

                        break;
                    case "ReadSleepTime":
                        readSleepTime = Int16.Parse(item.InnerText);
                        if (readSleepTime < 0)
                            readSleepTime = 10;

                        break;
                    case "WriteSleepTime":
                        writeSleepTime = Int16.Parse(item.InnerText);
                        if (readSleepTime < 0)
                            readSleepTime = 10;

                        break;
                    case "ServoOnTimeOut":
                        ServoOnTimeOut = Int16.Parse(item.InnerText);
                        break;
                    case "Move":
                        tempAxisData = ReadMoveTurnData((XmlElement)item);
                        allType.Add(EnumAxisType.Move, tempAxisData);
                        break;
                    case "Turn":
                        tempAxisData = ReadMoveTurnData((XmlElement)item);
                        allType.Add(EnumAxisType.Turn, tempAxisData);
                        break;
                    default:
                        // log...
                        break;
                }
            }
        }

        private List<EnumAxis> ReadOrder(XmlElement element)
        {
            List<EnumAxis> stringList = new List<EnumAxis>();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "ID":
                        stringList.Add((EnumAxis)Enum.Parse(typeof(EnumAxis), item.InnerText));
                        break;
                    default:
                        //log...
                        break;
                }
            }

            return stringList;
        }

        private void ReadParams(XmlElement element)
        {
            ElmoSingleAxisConfig tempAxisConfig = new ElmoSingleAxisConfig();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "ID":
                        tempAxisConfig.ID = (EnumAxis)Enum.Parse(typeof(EnumAxis), item.InnerText);
                        break;
                    case "AxisName":
                        tempAxisConfig.AxisName = item.InnerText;
                        break;
                    case "VirtualDev4ID":
                        tempAxisConfig.VirtualDev4ID = (EnumAxis)Enum.Parse(typeof(EnumAxis), item.InnerText);
                        break;
                    case "GroupOrder":
                        tempAxisConfig.GroupOrder = ReadOrder((XmlElement)item);
                        break;
                    case "CommandOrder":
                        tempAxisConfig.CommandOrder = ReadOrder((XmlElement)item);
                        break;
                    case "Type":
                        tempAxisConfig.Type = (EnumAxisType)Enum.Parse(typeof(EnumAxisType), item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            tempAxisConfig.IsGroup = (tempAxisConfig.GroupOrder != null);
            tempAxisConfig.IsVirtualDevice = (tempAxisConfig.VirtualDev4ID != EnumAxis.None);

            elmoAxisConfig.Add(tempAxisConfig);
        }

        private void ReadMotionParameter(string path)
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");
            XmlDocument doc = new XmlDocument();

            string xmlPath = Path.Combine(Environment.CurrentDirectory, path);

            if (!File.Exists(xmlPath))
            {

                return;
            }


            doc.Load(xmlPath);
            var rootNode = doc.DocumentElement;
            foreach (XmlNode item in rootNode.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Params":
                        ReadParams((XmlElement)item);
                        break;
                    case "Config":
                        ReadIP((XmlElement)item);
                        break;
                    default:
                        // log...
                        break;
                }
            }

            for (int i = 0; i < elmoAxisConfig.Count; i++)
            {
                elmoAxisConfig[i].MotorResolution = allType[elmoAxisConfig[i].Type].MotorResolution;
                elmoAxisConfig[i].Acceleration = allType[elmoAxisConfig[i].Type].Acceleration;
                elmoAxisConfig[i].Deceleration = allType[elmoAxisConfig[i].Type].Deceleration;
                elmoAxisConfig[i].Jerk = allType[elmoAxisConfig[i].Type].Jerk;
                elmoAxisConfig[i].Velocity = allType[elmoAxisConfig[i].Type].Velocity;
                elmoAxisConfig[i].PulseUnit = allType[elmoAxisConfig[i].Type].PulseUnit;

                if (elmoAxisConfig[i].IsVirtualDevice)
                {
                    for (int j = 0; j < elmoAxisConfig.Count; j++)
                        if (elmoAxisConfig[i].VirtualDev4ID == elmoAxisConfig[j].ID)
                            elmoAxisConfig[j].VirtualDev4ID = elmoAxisConfig[i].ID;
                }
            }

            elmoLogger.SavePureLog(this.GetType().FullName + " sucess!");
        }
        #endregion

        private bool Connect()
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");
            IPAddress cardIP, controlIP;

            if (!IPAddress.TryParse(networkInterfaceCard, out cardIP))
            {
                elmoLogger.SavePureLog(this.GetType().FullName + " 介面卡IP格式錯誤!");
                return false;
            }

            if (!IPAddress.TryParse(elmoControlIP, out controlIP))
            {
                elmoLogger.SavePureLog(this.GetType().FullName + " 控制器IP格式錯誤!");
                return false;
            }

            if (MMCConnection.ConnectRPC(controlIP, cardIP, elmoControlPort, out handler) != 0)
            {
                elmoLogger.SavePureLog(this.GetType().FullName + " 連線失敗!");
                return false;
            }
            
            elmoLogger.SavePureLog(this.GetType().FullName + " 連線成功!");
            return true;
        }

        public void SetAllAxis()
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");
            MMC_MOTIONPARAMS_EX_SINGLE singleAxisParameter;
            AxisInfo tempAxisInfo;

            for (int i = 0; i < elmoAxisConfig.Count; i++)
            {
                singleAxisParameter = new MMC_MOTIONPARAMS_EX_SINGLE();

                singleAxisParameter.dbAcceleration = elmoAxisConfig[i].Acceleration;
                singleAxisParameter.dbDeceleration = elmoAxisConfig[i].Deceleration;
                singleAxisParameter.dbJerk = elmoAxisConfig[i].Jerk;
                singleAxisParameter.dbVelocity = elmoAxisConfig[i].Velocity;
                singleAxisParameter.dbDistance = elmoAxisConfig[i].dbDistance;
                singleAxisParameter.dbEndVelocity = elmoAxisConfig[i].dbEndVelocity;
                singleAxisParameter.eBufferMode = elmoAxisConfig[i].eBufferMode;
                singleAxisParameter.eDirection = elmoAxisConfig[i].eDirection;
                singleAxisParameter.ucExecute = elmoAxisConfig[i].ucExecute;

                if (elmoAxisConfig[i].IsGroup)
                {
                    tempAxisInfo = new AxisInfo();
                    tempAxisInfo.Config = elmoAxisConfig[i];
                    tempAxisInfo.GroupAxis = new MMCGroupAxis(elmoAxisConfig[i].ID.ToString(), handler);
                    tempAxisInfo.GroupAxis.CoordSystem = elmoAxisConfig[i].CoordSystem;
                    tempAxisInfo.GroupAxis.TransitionMode = elmoAxisConfig[i].TransitionMode;
                    tempAxisInfo.GroupAxis.Acceleration = elmoAxisConfig[i].Acceleration * Math.Sqrt(elmoAxisConfig[i].GroupOrder.Count());
                    tempAxisInfo.GroupAxis.Deceleration = elmoAxisConfig[i].Deceleration * Math.Sqrt(elmoAxisConfig[i].GroupOrder.Count());
                    tempAxisInfo.GroupAxis.Jerk = elmoAxisConfig[i].Jerk * Math.Sqrt(elmoAxisConfig[i].GroupOrder.Count());

                    tempAxisInfo.GroupOlderToCommandOlder = new int[tempAxisInfo.Config.CommandOrder.Count()];
                    for (int j = 0; j < tempAxisInfo.Config.CommandOrder.Count(); j++)
                    {
                        for (int k = 0; k < tempAxisInfo.Config.GroupOrder.Count(); k++)
                        {
                            if (tempAxisInfo.Config.CommandOrder[j] == tempAxisInfo.Config.GroupOrder[k])
                            {
                                tempAxisInfo.GroupOlderToCommandOlder[j] = k;
                                break;
                            }
                        }
                    }

                    allAxis.Add(elmoAxisConfig[i].ID, tempAxisInfo);
                    allAxisList.Add(tempAxisInfo);
                }
                else
                {
                    tempAxisInfo = new AxisInfo();
                    tempAxisInfo.Config = elmoAxisConfig[i];
                    tempAxisInfo.SingleAxis = new MMCSingleAxis(elmoAxisConfig[i].ID.ToString(), handler);
                    tempAxisInfo.SingleAxis.SetDefaultParams(singleAxisParameter);
                    allAxis.Add(elmoAxisConfig[i].ID, tempAxisInfo);
                    allAxisList.Add(tempAxisInfo);
                }
            }

            elmoLogger.SavePureLog(this.GetType().FullName + " set Bulk!");
            #region Bulkread set
            MAX_AXIS = (ushort)allAxisList.Count();
            ushort[] nodeList = new ushort[MAX_AXIS];
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup)
                    nodeList[i] = allAxisList[i].GroupAxis.AxisReference;
                else
                    nodeList[i] = allAxisList[i].SingleAxis.AxisReference;
            }

            bulkRead = new MMCBulkRead(handler);
            bulkRead.Init(NC_BULKREAD_PRESET_ENUM.eNC_BULKREAD_PRESET_5, NC_BULKREAD_CONFIG_ENUM.eBULKREAD_CONFIG_2, nodeList, MAX_AXIS);
            bulkRead.Config();
            ncBulkRead = bulkRead.Preset_5;
            #endregion

            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup)
                {
                    allAxisList[i].FeedbackData = new ElmoAxisFeedbackData();
                    allAxisList[i].FeedbackData.Disable = true;
                }
            }

            elmoLogger.SavePureLog(this.GetType().FullName + " set Bulk end!");
            /////////////////////////////Bulkread Start////////////////////////////
            //Total
            Read_BulkRead = new Thread(BulkReadThread);
            Read_BulkRead.Start();

            elmoLogger.SavePureLog(this.GetType().FullName + " end!");
        }

        private void BulkReadThread()
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");

            uint count = 0;
            scanTimeTimer.Reset();
            scanTimeTimer.Start();

            ElmoAxisFeedbackData tempFeedbackData;
            DateTime GetDataTime;

            while (connected)
            {
                scanTimeTimer.Reset();
                scanTimeTimer.Start();
                bulkRead.Perform();
                GetDataTime = DateTime.Now;

                for (int i = 0; i < MAX_AXIS; i++)
                {
                    if (allAxisList[i].Config.ID != EnumAxis.GT && allAxisList[i].Config.ID != EnumAxis.GX)
                    {
                        tempFeedbackData = new ElmoAxisFeedbackData();
                        tempFeedbackData.Feedback_Velocity = Convert.ToInt32(ncBulkRead[i].aVel * allAxisList[i].Config.PulseUnit);//速度
                        tempFeedbackData.Feedback_Position_Error = ncBulkRead[i].iPosFollowingErr;//位置誤差
                        tempFeedbackData.Feedback_Torque = ncBulkRead[i].aTorque / 10;//扭力值
                        tempFeedbackData.Feedback_Now_Mode = ncBulkRead[i].eOpMode.ToString();//當下模式
                        tempFeedbackData.Feedback_Position = ncBulkRead[i].aPos * allAxisList[i].Config.PulseUnit;//位置
                        

                        tempFeedbackData.StandStill = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.STAND_STILL)) != 0);
                        tempFeedbackData.Inmotion = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.DISCRETE_MOTION)) != 0);
                        tempFeedbackData.Disable = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.DISABLED)) != 0);
                        tempFeedbackData.Homing = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.HOMING)) != 0);
                        tempFeedbackData.Stopping = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.STOPPING)) != 0);
                        tempFeedbackData.ErrorStop = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.ERROR_STOP)) != 0);

                        tempFeedbackData.Count = count;
                        tempFeedbackData.GetDataTime = GetDataTime;
                        allAxisList[i].FeedbackData = tempFeedbackData;
                    }
                }

                count++;
                scanTimeTimer.Stop();//碼錶停止
                double result1 = scanTimeTimer.Elapsed.TotalMilliseconds;
                
                Thread.Sleep(readSleepTime);
            }

            elmoLogger.SavePureLog(this.GetType().FullName + " close!");
        }

        #region Enable Disable fucntion

        private void EnableRealAxis(EnumAxis axis, bool onOff)
        {
            string msg = "";
            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            try
            {
                if (onOff)
                {
                    msg = "Servo on";
                    if (allAxis[axis].FeedbackData.Disable)
                        allAxis[axis].SingleAxis.PowerOn(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                    if (!allAxis[allAxis[axis].Config.VirtualDev4ID].FeedbackData.Disable && !allAxis[axis].Linking) //Link Real and Virtual Axis
                    {
                        msg = "Link Virtual Axis";

                        servoOnTimer.Reset();
                        servoOnTimer.Start();

                        while (allAxis[axis].FeedbackData.Disable && servoOnTimer.ElapsedMilliseconds < ServoOnTimeOut)
                            Thread.Sleep(10);

                        if (allAxis[axis].FeedbackData.Disable)
                        {
                            //log...
                        }                            
                        else
                        {
                            allAxis[axis].SingleAxis.LinkAxis(1, 2, 3, 4, allAxis[allAxis[axis].Config.VirtualDev4ID].SingleAxis.AxisReference, 0);
                            allAxis[axis].Linking = true;
                        }
                    }
                }
                else
                {
                    msg = "Servo off";
                    if (!allAxis[axis].FeedbackData.Disable)
                        allAxis[axis].SingleAxis.PowerOff(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                    allAxis[axis].Linking = false;
                }
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        private void EnableVirtualAxis(EnumAxis axis, bool onOff)
        {
            string msg = "";
            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            try
            {
                if (onOff)
                {
                    msg = "Servo on";
                    if (allAxis[axis].FeedbackData.Disable)
                    {
                        allAxis[axis].SingleAxis.PowerOn(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                        servoOnTimer.Reset();
                        servoOnTimer.Start();

                        while (allAxis[axis].FeedbackData.Disable && servoOnTimer.ElapsedMilliseconds < ServoOnTimeOut)
                            Thread.Sleep(10);

                        if (allAxis[axis].FeedbackData.Disable)
                            ;//log...
                    }

                    EnableAxis(allAxis[axis].Config.VirtualDev4ID);
                }
                else
                {
                    msg = "Servo off";
                    if (!allAxis[axis].FeedbackData.Disable)
                        allAxis[axis].SingleAxis.PowerOff(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                    allAxis[allAxis[axis].Config.VirtualDev4ID].Linking = false;
                }
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        private void EnableGroupAxis(EnumAxis axis, bool onOff)
        {
            string msg = "";
            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            try
            {
                if (onOff)
                {
                    msg = "Servo on";

                    if (allAxis[axis].FeedbackData.Disable)
                    {
                        for (int i = 0; i < allAxis[axis].Config.GroupOrder.Count(); i++)
                        {
                            EnableAxis(allAxis[axis].Config.GroupOrder[0]);
                            servoOnTimer.Reset();
                            servoOnTimer.Start();

                            while (allAxis[axis].FeedbackData.Disable && servoOnTimer.ElapsedMilliseconds < ServoOnTimeOut)
                                Thread.Sleep(10);

                            if (allAxis[axis].FeedbackData.Disable)
                                ;//log...
                        }

                        allAxis[axis].GroupAxis.GroupEnable();
                        allAxis[axis].FeedbackData.Disable = false;
                    }
                }
                else
                {
                    msg = "Servo off";

                    if (!allAxis[axis].FeedbackData.Disable)
                        allAxis[axis].GroupAxis.GroupDisable();

                    allAxis[axis].FeedbackData.Disable = true;
                }
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        public void DisableAxis(EnumAxis axis)
        {
            if (!connected)
                return;

            if (allAxis[axis].Config.IsGroup)
                EnableGroupAxis(axis, false);
            else if (allAxis[axis].Config.IsVirtualDevice)
                EnableVirtualAxis(axis, false);
            else
                EnableRealAxis(axis, false);
        }

        public void EnableAxis(EnumAxis axis)
        {
            if (!connected)
                return;

            if (allAxis[axis].Config.IsGroup)
                EnableGroupAxis(axis, true);
            else if (allAxis[axis].Config.IsVirtualDevice)
                EnableVirtualAxis(axis, true);
            else
                EnableRealAxis(axis, true);
        }

        public void DisableAllAxis()
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");

            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();
            // Disable group
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup)
                {
                    DisableAxis(allAxisList[i].Config.ID);
                    Thread.Sleep(200);
                }
            }

            // Disable 剩下的.
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsGroup && allAxisList[i].Config.IsVirtualDevice)
                {
                    DisableAxis(allAxisList[i].Config.ID);
                    Thread.Sleep(200);
                }
            }

            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsGroup && !allAxisList[i].Config.IsVirtualDevice)
                {
                    DisableAxis(allAxisList[i].Config.ID);
                    Thread.Sleep(200);
                }
            }

            elmoLogger.SavePureLog(this.GetType().FullName + " end!");
        }

        public void EnableAllAxis()
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");

            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            // Servo on real Axis
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsVirtualDevice && !allAxisList[i].Config.IsGroup)
                {
                    EnableAxis(allAxisList[i].Config.ID);

                    servoOnTimer.Reset();
                    servoOnTimer.Start();

                    while (allAxisList[i].FeedbackData.Disable && servoOnTimer.ElapsedMilliseconds < ServoOnTimeOut)
                        Thread.Sleep(10);

                    if (allAxisList[i].FeedbackData.Disable)
                        ;//log...
                }
            }

            // Servo on virtual Axis (本身會等待Enable(因為要Link,外面不用再等).
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsVirtualDevice)
                    EnableAxis(allAxisList[i].Config.ID);
            }

            // Servo on group Axis
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup)
                    EnableAxis(allAxisList[i].Config.ID);
            }

            elmoLogger.SavePureLog(this.GetType().FullName + " end!");
        }

        public void DisableMoveAxis(EnumAxis axis)
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");
            // 0.0
            elmoLogger.SavePureLog(this.GetType().FullName + " end!");
        }

        public void EnableMoveAxis(EnumAxis axis)
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");
            // 0.0
            elmoLogger.SavePureLog(this.GetType().FullName + " end!");
        }

        #endregion

        public void ResetError(EnumAxis axis)
        {
            string msg = "";

            try
            {
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                msg = axis.ToString() + " reset error ";

                if (allAxis[axis].Config.IsGroup)
                    allAxis[axis].GroupAxis.GroupReset();
                else
                    allAxis[axis].SingleAxis.Reset();
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        public void ResetErrorAll()
        {
            elmoLogger.SavePureLog(this.GetType().FullName + " start!");
            // reset error group
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup)
                    ResetError(allAxisList[i].Config.ID);
            }

            // reset 剩下的.
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsGroup)
                    ResetError(allAxisList[i].Config.ID);
            }

            elmoLogger.SavePureLog(this.GetType().FullName + " end!");
        }

        // 對外開放 比對四軸角度: 回傳true false, 給目標四軸的角度跟允許誤差.
        public bool WheelAngleCompare(double angle_FL, double angle_FR, double angle_RL, double angle_RR, double range)
        {
            string msg = "";

            try
            {
                if (!connected)
                    return false;

                return Math.Abs(allAxis[EnumAxis.TFL].FeedbackData.Feedback_Position - angle_FL) < range &&
                       Math.Abs(allAxis[EnumAxis.TFR].FeedbackData.Feedback_Position - angle_FR) < range &&
                       Math.Abs(allAxis[EnumAxis.TRL].FeedbackData.Feedback_Position - angle_RL) < range &&
                       Math.Abs(allAxis[EnumAxis.TRR].FeedbackData.Feedback_Position - angle_RR) < range;
            }
            catch (MMCException ex)
            {


                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                return false;
            }
        }

        public bool WheelAngleCompare(double angle_ALL, double range)
        {
            return WheelAngleCompare(angle_ALL, angle_ALL, angle_ALL, angle_ALL, range);
        }

        private void ElmoMoveGroupAxisAbsolute(EnumAxis axis, double distance_FL, double distance_FR, double distance_RL, double distance_RR,
                                                  double velocity, double acceleration, double deceleration, double jerk)
        {
            string msg = "";

            try
            {
                if (allAxis[axis].FeedbackData.Disable)
                    return;


                if (axis == EnumAxis.GX)
                    return;

                if (axis == EnumAxis.GT)
                {
                    if (WheelAngleCompare(distance_FL, distance_FR, distance_RL, distance_RR, 0.1))
                        return;
                }

                msg = axis.ToString() + " Move ";
                double sqrt = Math.Sqrt(allAxis[axis].Config.GroupOrder.Count());
                velocity *= sqrt;
                acceleration *= sqrt;
                deceleration *= sqrt;
                jerk *= sqrt;

                double[] orgionArray = { distance_FL, distance_FR, distance_RL, distance_RR };
                double[] realArray = { orgionArray[allAxis[axis].GroupOlderToCommandOlder[0]], orgionArray[allAxis[axis].GroupOlderToCommandOlder[1]],
                                       orgionArray[allAxis[axis].GroupOlderToCommandOlder[2]], orgionArray[allAxis[axis].GroupOlderToCommandOlder[3]] };

                float[] Transition = { 0, 0, 0, 0 };
                allAxis[axis].GroupAxis.GroupSetOverride(1, 1, 1, 0);//Speed 100%

                allAxis[axis].GroupAxis.MoveLinearAbsolute(
                          (float)velocity, (float)acceleration, (float)deceleration, (float)jerk,
                          realArray,
                          MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE,
                          MC_COORD_SYSTEM_ENUM.MC_ACS_COORD,
                          NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE,
                          Transition, 1, 1);
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        private void ElmoMoveGroupAxisRelative(EnumAxis axis, double distance_FL, double distance_FR, double distance_RL, double distance_RR,
                                                  double velocity, double acceleration, double deceleration, double jerk)
        {
            string msg = "";

            try
            {
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                if (distance_FL == 0 && distance_FR == 0 && distance_RL == 0 && distance_RR == 0)
                    return;

                msg = axis.ToString() + " Move ";
                double sqrt = Math.Sqrt(allAxis[axis].Config.GroupOrder.Count());
                velocity *= sqrt;
                acceleration *= sqrt;
                deceleration *= sqrt;
                jerk *= sqrt;

                double[] orgionArray = { distance_FL, distance_FR, distance_RL, distance_RR };
                double[] realArray = { orgionArray[allAxis[axis].GroupOlderToCommandOlder[0]], orgionArray[allAxis[axis].GroupOlderToCommandOlder[1]],
                                       orgionArray[allAxis[axis].GroupOlderToCommandOlder[2]], orgionArray[allAxis[axis].GroupOlderToCommandOlder[3]] };

                float[] Transition = { 0, 0, 0, 0 };
                allAxis[axis].GroupAxis.GroupSetOverride(1, 1, 1, 0);//Speed 100%

                allAxis[axis].GroupAxis.MoveLinearRelative(
                          (float)velocity, (float)acceleration, (float)deceleration, (float)jerk,
                          realArray,
                          MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE,
                          MC_COORD_SYSTEM_ENUM.MC_ACS_COORD,
                          NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE,
                          Transition, 1, 1);
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        private void ElmoMoveSingleAxisRelative(EnumAxis axis, double distance, double velocity, double acceleration, double deceleration, double jerk)
        {
            string msg = "";

            try
            {
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                msg = axis.ToString() + " Move ";
                // 卡同position
                //0.0
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }


        private void ElmoMoveSingleAxisAbsolute(EnumAxis axis, double distance, double velocity, double acceleration, double deceleration, double jerk)
        {
            string msg = "";

            try
            {
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                msg = axis.ToString() + " Move ";
                // 卡同position
                //0.0
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        private void ElmoStopGroupAxis(EnumAxis axis, double deceleration, double jerk)
        {
            string msg = "";

            try
            {
                msg = axis.ToString() + " Stop ";
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                double sqrt = Math.Sqrt(allAxis[axis].Config.GroupOrder.Count());
                deceleration *= sqrt;
                jerk *= sqrt;

                allAxis[axis].GroupAxis.GroupStop((float)deceleration, (float)jerk, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        private void ElmoStopSingleAxis(EnumAxis axis, double deceleration, double jerk)
        {
            string msg = "";

            try
            {
                msg = axis.ToString() + " Stop ";
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                allAxis[axis].SingleAxis.Stop((float)deceleration, (float)jerk, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        // 對外開放 可Group(四軸同距離)可Single(虛實皆可以), acc dec jerk 可不填,不填使用MotionParameter預設值.
        public void ElmoMove(EnumAxis axis, double distance, double velocity, EnumMoveType type, double acceleration = -1, double deceleration = -1, double jerk = -1)
        {
            if (!connected)
                return;

            if (acceleration == -1)
                acceleration = allAxis[axis].Config.Acceleration;

            if (deceleration == -1)
                deceleration = allAxis[axis].Config.Deceleration;

            if (jerk == -1)
                jerk = allAxis[axis].Config.Jerk;

            if (allAxis[axis].Config.IsGroup)
            {
                if (type == EnumMoveType.Absolute)
                    ElmoMoveGroupAxisAbsolute(axis, distance, distance, distance, distance, velocity, acceleration, deceleration, jerk);
                else if (type == EnumMoveType.Relative)
                    ElmoMoveGroupAxisRelative(axis, distance, distance, distance, distance, velocity, acceleration, deceleration, jerk);
            }
            else
            {
                if (type == EnumMoveType.Absolute)
                    ElmoMoveSingleAxisAbsolute(axis, distance, velocity, acceleration, deceleration, jerk);
                else if (type == EnumMoveType.Relative)
                    ElmoMoveSingleAxisRelative(axis, distance, velocity, acceleration, deceleration, jerk);
            }
        }

        // 對外開放 必須是Group, acc dec jerk 可不填,不填使用MotionParameter預設值.
        public void ElmoMove(EnumAxis axis, double distance_FL, double distance_FR, double distance_RL, double distance_RR,
                                        double velocity, EnumMoveType type, double acceleration = -1, double deceleration = -1, double jerk = -1)
        {
            if (!connected || !allAxis[axis].Config.IsGroup)
                return;

            if (acceleration == -1)
                acceleration = allAxis[axis].Config.Acceleration;

            if (deceleration == -1)
                deceleration = allAxis[axis].Config.Deceleration;

            if (jerk == -1)
                jerk = allAxis[axis].Config.Jerk;

            if (type == EnumMoveType.Absolute)
                ElmoMoveGroupAxisAbsolute(axis, distance_FL, distance_FR, distance_RL, distance_RR, velocity, acceleration, deceleration, jerk);
            else if (type == EnumMoveType.Relative)
                ElmoMoveGroupAxisRelative(axis, distance_FL, distance_FR, distance_RL, distance_RR, velocity, acceleration, deceleration, jerk);
        }

        // 對外開放 可Group(四軸同距離)可Single(虛實皆可以), dec jerk 可不填,不填使用MotionParameter預設值.
        public void ElmoStop(EnumAxis axis, double deceleration = -1, double jerk = -1)
        {
            if (!connected || !allAxis[axis].Config.IsGroup)
                return;

            if (deceleration == -1)
                deceleration = allAxis[axis].Config.Deceleration;

            if (jerk == -1)
                jerk = allAxis[axis].Config.Jerk;

            if (allAxis[axis].Config.IsGroup)
                ElmoStopGroupAxis(axis, deceleration, jerk);
            else
                ElmoStopSingleAxis(axis, deceleration, jerk);
        }

        // 對外開放 必須是Group Change Velocity
        public void ElmoGroupVelocityChange(EnumAxis axis, double velocityRatio)
        {
            string msg = "";

            try
            {
                if (!connected || allAxis[axis].FeedbackData.Disable || !allAxis[axis].Config.IsGroup)
                    return;

                if (velocityRatio > 1 || velocityRatio < 0)
                    return;

                msg = axis.ToString() + " Velocity Change ";

                allAxis[axis].GroupAxis.GroupSetOverride((float)velocityRatio, 1, 1, 0); // acc, jerk 先不調整, 調整的位置估算太難做.
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
            }
        }

        // 對外開放 讀取position, 只能轉向4實體和 走行"前左","後右"
        public double ElmoGetPosition(EnumAxis axis)
        {
            string msg = "";

            try
            {
                if (!connected)
                    return -1;

                if (axis != EnumAxis.XFL && axis != EnumAxis.XRR &&
                    axis != EnumAxis.TFL && axis != EnumAxis.TFR && axis != EnumAxis.TRL && axis != EnumAxis.TRR)
                    return -1;

                return allAxis[axis].FeedbackData.Feedback_Position;
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
                return -1;
            }
        }

        // 對外開放 讀取velocity.
        public double ElmoGetVelocity(EnumAxis axis)
        {
            string msg = "";

            try
            {
                if (!connected)
                    return -1;

                if (allAxis[axis].Config.IsGroup)
                    return -1;

                return allAxis[axis].FeedbackData.Feedback_Velocity;
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
                return -1;
            }
        }

        public ElmoAxisFeedbackData ElmoGetFeedbackData(EnumAxis axis)
        {
            string msg = "";

            try
            {
                if (!connected)
                    return null;

                return allAxis[axis].FeedbackData;
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
                return null;
            }
        }

        public bool MoveCompelete(EnumAxis axis)
        {
            string msg = "";

            try
            {
                if (!connected)
                    return true;

                if (allAxis[axis].Config.IsGroup)
                {
                    return allAxis[allAxis[axis].Config.GroupOrder[0]].FeedbackData.StandStill &&
                           allAxis[allAxis[axis].Config.GroupOrder[1]].FeedbackData.StandStill &&
                           allAxis[allAxis[axis].Config.GroupOrder[2]].FeedbackData.StandStill &&
                           allAxis[allAxis[axis].Config.GroupOrder[3]].FeedbackData.StandStill;
                }
                else
                    return allAxis[axis].FeedbackData.StandStill;
            }
            catch (MMCException ex)
            {
                msg = this.GetType().FullName + msg + " : Fail, " + (ex.MMCError).ToString();
                elmoLogger.SavePureLog(msg);
                return true;
            }
        }

        public void SetPosition(EnumAxis axis, double position)
        {
            try
            {
                return;

                if (allAxis[axis].Config.Type != EnumAxisType.Move)
                    return;

                if (allAxis[axis].Config.IsGroup)
                {
                    // ??
                }
                else if (allAxis[axis].Config.IsVirtualDevice)
                {
                    allAxis[axis].SingleAxis.SetParameter(position, MMC_PARAMETER_LIST_ENUM.MMC_ACTUAL_POS_UU_PARAM, 1);
                }
                else
                {
                    allAxis[axis].SingleAxis.SetOpMode(OPM402.OPM402_HOMING_MODE);
                    Thread.Sleep(200);

                    //allAxis[axis].SingleAxis.HomeDS402Ex(
                    //    position,
                    //    10 * eq.Param.MOTION[Axis].MOTOR_RES,
                    //    100 * eq.Param.MOTION[Axis].MOTOR_RES,
                    //    (float)eq.Param.MOTION[Axis].HOME_VELOCITY_HIGH,
                    //    (float)eq.Param.MOTION[Axis].HOME_VELOCITY_LOW,
                    //    0,
                    //    0,
                    //    MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE,
                    //        37, //增量式設35   絕對式設37
                    //    100000,
                    //    0,
                    //    1,
                    //    array);
                    //Thread.Sleep(200);

                    allAxis[axis].SingleAxis.SetOpMode(OPM402.OPM402_CYCLIC_SYNC_POSITION_MODE);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
