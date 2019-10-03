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
using System.Reflection;

namespace Mirle.Agv.Controller
{
    public class ElmoDriver
    {
        Thread Read_BulkRead;

        private LoggerAgent loggerAgent = LoggerAgent.Instance;

        private ushort MAX_AXIS;
        private string networkInterfaceCard = "";
        private string elmoControlIP = "";
        private int elmoControlPort = 0;
        private int handler = 0;

        private AlarmHandler alarmHandler;
        private Dictionary<EnumAxis, AxisInfo> allAxis = new Dictionary<EnumAxis, AxisInfo>();
        private List<AxisInfo> allAxisList = new List<AxisInfo>();

        private List<ElmoSingleAxisConfig> elmoAxisConfig = new List<ElmoSingleAxisConfig>();
        public bool Connected { get; private set; } = false;
        private MMCBulkRead bulkRead;
        private NC_BULKREAD_PRESET_5[] ncBulkRead;

        private int readSleepTime;
        private int writeSleepTime;
        private int ServoOnTimeOut;
        private System.Diagnostics.Stopwatch scanTimeTimer = new System.Diagnostics.Stopwatch();
        private volatile double[] getPosFbk = new double[8];
        private Dictionary<EnumAxisType, AxisData> allType = new Dictionary<EnumAxisType, AxisData>();
        private Dictionary<EnumAxis, int> overflowOffset = new Dictionary<EnumAxis, int>();
        private double offsetValue = 0;
        private string device = "Elmo driver";

        private void SendAlarmCode(int alarmCode)
        {
            try
            {
                WriteLog("Elmo", "3", device, "", "SetAlarm, alarmCode : " + alarmCode.ToString());
                alarmHandler.SetAlarm(alarmCode);
            }
            catch (Exception ex)
            {
                WriteLog("Error", "3", device, "", "SetAlarm失敗, Excption : " + ex.ToString());
            }
        }

        public ElmoDriver(string elmoConfigPath, AlarmHandler alarmHandler)
        {
            this.alarmHandler = alarmHandler;

            if (elmoConfigPath == null || elmoConfigPath == "")
            {
                WriteLog("Elmo", "3", device, "", "MotionParameter 路徑錯誤為null或空值,請檢查MoveControlConfig內的ElmoConfigPath");
                SendAlarmCode(100002);
                return;
            }

            try
            {
                ReadMotionParameter(elmoConfigPath);

                if (Connect())
                {
                    Connected = true;
                    SetAllAxis();
                    Thread.Sleep(100);
                    DisableAllAxis();
                    EnableAllAxis();

                    Thread.Sleep(100);
                    ElmoStop(EnumAxis.GX);
                    ElmoMove(EnumAxis.GT, 0, 75, EnumMoveType.Absolute);
                }
                else
                {
                    SendAlarmCode(100000);
                }
            }
            catch (Exception ex)
            {
                WriteLog("Elmo", "1", device, "", "Excption : " + ex.ToString());
                WriteLog("Error", "1", device, "", "Elmo 連線失敗, Excption : " + ex.ToString());
                SendAlarmCode(100000);
                Connected = false;
            }
        }

        private void WriteLog(string category, string logLevel, string device, string carrierId, string message,
                             [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string classMethodName = GetType().Name + ":" + memberName;
            LogFormat logFormat = new LogFormat(category, logLevel, classMethodName, device, carrierId, message);

            loggerAgent.LogMsg(logFormat.Category, logFormat);
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
            try
            {
                XmlDocument doc = new XmlDocument();

                string xmlPath = Path.Combine(Environment.CurrentDirectory, path);

                if (!File.Exists(xmlPath))
                {
                    WriteLog("Elmo", "3", device, "", "找不到MotionParameter.xml.");
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
            }
            catch (Exception ex)
            {
                WriteLog("Elmo", "3", device, "", "參數讀取失敗,Excption : " + ex.ToString());
                SendAlarmCode(100001);
            }
        }
        #endregion

        #region Connect & SetAllAxis
        private bool Connect()
        {
            IPAddress cardIP, controlIP;

            if (!IPAddress.TryParse(networkInterfaceCard, out cardIP))
            {
                WriteLog("Elmo", "3", device, "", "介面卡IP格式錯誤,請檢查MotionParameter內的NetworkInterfaceCard!");
                return false;
            }

            if (!IPAddress.TryParse(elmoControlIP, out controlIP))
            {
                WriteLog("Elmo", "3", device, "", "控制器IP格式錯誤,請檢查MotionParameter內的ElmoControl!");
                return false;
            }

            if (MMCConnection.ConnectRPC(controlIP, cardIP, elmoControlPort, out handler) != 0)
            {
                WriteLog("Error", "1", device, "", "Elmo 連線失敗!");
                return false;
            }

            WriteLog("Elmo", "9", device, "", "Elmo 連線成功!");
            return true;
        }

        public void SetAllAxis()
        {
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

            offsetValue = allType[EnumAxisType.Move].PulseUnit * Math.Pow(2, 32);

            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.Type == EnumAxisType.Move)
                    overflowOffset.Add(allAxisList[i].Config.ID, 0);
            }

            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup)
                {
                    allAxisList[i].FeedbackData = new ElmoAxisFeedbackData();
                    allAxisList[i].FeedbackData.Disable = true;
                }
            }

            /////////////////////////////Bulkread Start////////////////////////////
            Read_BulkRead = new Thread(BulkReadThread);
            Read_BulkRead.Start();
        }
        #endregion

        private void GetOffsetIndex(EnumAxis axis, ref ElmoAxisFeedbackData newData, double oldPosition)
        {
            newData.Feedback_Position = newData.Feedback_Position + overflowOffset[axis] * offsetValue;

            if (Math.Abs(newData.Feedback_Position - oldPosition) > offsetValue / 2)
            {
                if (oldPosition > newData.Feedback_Position)
                {
                    overflowOffset[axis]++;
                    newData.Feedback_Position += offsetValue;
                }
                else
                {
                    overflowOffset[axis]--;
                    newData.Feedback_Position -= offsetValue;
                }
            }
        }

        private void BulkReadThread()
        {
            uint count = 0;
            scanTimeTimer.Reset();
            scanTimeTimer.Start();

            ElmoAxisFeedbackData tempFeedbackData;
            DateTime GetDataTime;

            while (Connected)
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
                        tempFeedbackData.Feedback_Position_Error = ncBulkRead[i].iPosFollowingErr * allAxisList[i].Config.PulseUnit;//位置誤差
                        tempFeedbackData.Feedback_Torque = ncBulkRead[i].aTorque / 10;//扭力值
                        tempFeedbackData.Feedback_Now_Mode = ncBulkRead[i].eOpMode.ToString();//當下模式
                        tempFeedbackData.Feedback_Position = ncBulkRead[i].aPos * allAxisList[i].Config.PulseUnit;//位置

                        if (allAxisList[i].Config.Type == EnumAxisType.Move)
                            GetOffsetIndex(allAxisList[i].Config.ID, ref tempFeedbackData, allAxisList[i].FeedbackData.Feedback_Position);

                        tempFeedbackData.StandStill = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.STAND_STILL)) != 0);
                        tempFeedbackData.Inmotion = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.DISCRETE_MOTION)) != 0);
                        tempFeedbackData.Disable = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.DISABLED)) != 0);
                        tempFeedbackData.Homing = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.HOMING)) != 0);
                        tempFeedbackData.Stopping = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.STOPPING)) != 0);
                        tempFeedbackData.ErrorStop = ((ncBulkRead[i].ulAxisStatus & (Int32)(MC_STATE_SINGLE.ERROR_STOP)) != 0);

                        tempFeedbackData.ErrorCode = Convert.ToInt32(ncBulkRead[i].usLastEmcyErrorCode.usVar.ToString(), 10);

                        if (tempFeedbackData.ErrorCode != 0 && tempFeedbackData.ErrorCode != allAxisList[i].FeedbackData.ErrorCode)
                            WriteLog("Elmo", "2", device, "", "Axis : " + allAxisList[i].Config.ID.ToString() + ", Error code :  " + tempFeedbackData.ErrorCode.ToString());

                        tempFeedbackData.Count = count;
                        tempFeedbackData.GetDataTime = GetDataTime;

                        if (allAxisList[i].NeedAssignLastCommandPosition && tempFeedbackData.StandStill)
                        {
                            allAxisList[i].LastCommandPosition = tempFeedbackData.Feedback_Position;
                            allAxisList[i].NeedAssignLastCommandPosition = false;
                        }

                        allAxisList[i].FeedbackData = tempFeedbackData;
                    }
                }

                count++;
                scanTimeTimer.Stop();//碼錶停止
                double result1 = scanTimeTimer.Elapsed.TotalMilliseconds;

                Thread.Sleep(readSleepTime);
            }
        }

        #region Enable Disable fucntion
        private void EnableRealAxis(EnumAxis axis, bool onOff,
                                   [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            try
            {
                if (onOff)
                {
                    if (allAxis[axis].FeedbackData.Disable)
                        allAxis[axis].SingleAxis.PowerOn(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                    if (!allAxis[allAxis[axis].Config.VirtualDev4ID].FeedbackData.Disable && !allAxis[axis].Linking) //Link Real and Virtual EnumAxis
                    {
                        servoOnTimer.Reset();
                        servoOnTimer.Start();

                        while (allAxis[axis].FeedbackData.Disable && servoOnTimer.ElapsedMilliseconds < ServoOnTimeOut)
                            Thread.Sleep(10);

                        if (allAxis[axis].FeedbackData.Disable)
                            WriteLog("Elmo", "4", device, memberName, "Enable timeout.");
                        else
                        {
                            allAxis[axis].SingleAxis.LinkAxis(1, 2, 3, 4, allAxis[allAxis[axis].Config.VirtualDev4ID].SingleAxis.AxisReference, 0);
                            allAxis[axis].Linking = true;
                        }
                    }
                }
                else
                {
                    if (!allAxis[axis].FeedbackData.Disable)
                        allAxis[axis].SingleAxis.PowerOff(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                    allAxis[axis].Linking = false;
                }
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        private void SetVirtualPositionToZero(EnumAxis axis,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected)
                    return;

                if (!allAxis[axis].Config.IsVirtualDevice)
                    return;

                allAxis[axis].SingleAxis.SetParameter(0, MMC_PARAMETER_LIST_ENUM.MMC_ACTUAL_POS_UU_PARAM, 1);
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        private void EnableVirtualAxis(EnumAxis axis, bool onOff,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            try
            {
                if (onOff)
                {
                    if (allAxis[axis].FeedbackData.Disable)
                    {
                        allAxis[axis].SingleAxis.PowerOn(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                        servoOnTimer.Reset();
                        servoOnTimer.Start();

                        while (allAxis[axis].FeedbackData.Disable && servoOnTimer.ElapsedMilliseconds < ServoOnTimeOut)
                            Thread.Sleep(10);

                        if (allAxis[axis].FeedbackData.Disable)
                            WriteLog("Elmo", "4", device, memberName, "Enable timeout.");
                    }

                    EnableAxis(allAxis[axis].Config.VirtualDev4ID);
                }
                else
                {
                    if (!allAxis[axis].FeedbackData.Disable)
                    {
                        allAxis[axis].SingleAxis.PowerOff(MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);
                        Thread.Sleep(100);
                    }

                    SetVirtualPositionToZero(axis);
                    allAxis[allAxis[axis].Config.VirtualDev4ID].Linking = false;
                }
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        private void EnableGroupAxis(EnumAxis axis, bool onOff,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            try
            {
                if (onOff)
                {
                    for (int i = 0; i < allAxis[axis].Config.GroupOrder.Count(); i++)
                    {
                        if (allAxis[allAxis[axis].Config.GroupOrder[0]].FeedbackData.Disable)
                        {
                            EnableAxis(allAxis[axis].Config.GroupOrder[0]);
                            servoOnTimer.Reset();
                            servoOnTimer.Start();

                            while (allAxis[axis].FeedbackData.Disable && servoOnTimer.ElapsedMilliseconds < ServoOnTimeOut)
                                Thread.Sleep(10);

                            if (allAxis[axis].FeedbackData.Disable)
                                WriteLog("Elmo", "4", device, memberName, "Enable timeout.");
                        }
                    }

                    allAxis[axis].GroupAxis.GroupEnable();
                    allAxis[axis].FeedbackData.Disable = false;
                }
                else
                {
                    allAxis[axis].GroupAxis.GroupDisable();

                    allAxis[axis].FeedbackData.Disable = true;
                }
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        private void DisableAxis(EnumAxis axis,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (!Connected)
                return;

            if (allAxis[axis].Config.IsGroup)
                EnableGroupAxis(axis, false, memberName);
            else if (allAxis[axis].Config.IsVirtualDevice)
                EnableVirtualAxis(axis, false, memberName);
            else
                EnableRealAxis(axis, false, memberName);
        }

        private void EnableAxis(EnumAxis axis,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (!Connected)
                return;

            if (allAxis[axis].Config.IsGroup)
                EnableGroupAxis(axis, true, memberName);
            else if (allAxis[axis].Config.IsVirtualDevice)
                EnableVirtualAxis(axis, true, memberName);
            else
                EnableRealAxis(axis, true, memberName);
        }

        public void DisableAllAxis([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            WriteLog("Elmo", "7", device, memberName, "Start.");

            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();
            // Disable group
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup)
                {
                    DisableAxis(allAxisList[i].Config.ID, memberName);
                    Thread.Sleep(200);
                }
            }

            // Disable 剩下的.
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsGroup && allAxisList[i].Config.IsVirtualDevice)
                {
                    DisableAxis(allAxisList[i].Config.ID, memberName);
                    Thread.Sleep(200);
                }
            }

            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsGroup && !allAxisList[i].Config.IsVirtualDevice)
                {
                    DisableAxis(allAxisList[i].Config.ID, memberName);
                    Thread.Sleep(200);
                }
            }

            WriteLog("Elmo", "7", device, memberName, "End.");
        }

        public void EnableAllAxis([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            WriteLog("Elmo", "7", device, memberName, "Start.");

            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            // Servo on real EnumAxis
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsVirtualDevice && !allAxisList[i].Config.IsGroup)
                {
                    EnableAxis(allAxisList[i].Config.ID, memberName);

                    servoOnTimer.Reset();
                    servoOnTimer.Start();

                    while (allAxisList[i].FeedbackData.Disable && servoOnTimer.ElapsedMilliseconds < ServoOnTimeOut)
                        Thread.Sleep(10);

                    if (allAxisList[i].FeedbackData.Disable)
                        WriteLog("Elmo", "4", device, memberName, "Enable timeout.");
                }
            }

            // Servo on group EnumAxis
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup)
                    EnableAxis(allAxisList[i].Config.ID, memberName);
            }

            // Servo on virtual EnumAxis (本身會等待Enable(因為要Link,外面不用再等).
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsVirtualDevice)
                    EnableAxis(allAxisList[i].Config.ID, memberName);
            }

            WriteLog("Elmo", "7", device, memberName, "End.");
        }

        public void DisableMoveAxis([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            WriteLog("Elmo", "7", device, memberName, "Start.");

            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            // Disable group
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup && allAxisList[i].Config.Type == EnumAxisType.Move)
                {
                    DisableAxis(allAxisList[i].Config.ID, memberName);
                    Thread.Sleep(100);
                }
            }

            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsGroup && !allAxisList[i].Config.IsVirtualDevice && allAxisList[i].Config.Type == EnumAxisType.Move)
                {
                    DisableAxis(allAxisList[i].Config.ID, memberName);
                    Thread.Sleep(400);
                }
            }

            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsGroup && allAxisList[i].Config.IsVirtualDevice && allAxisList[i].Config.Type == EnumAxisType.Move)
                {
                    DisableAxis(allAxisList[i].Config.ID, memberName);
                    Thread.Sleep(100);
                }
            }

            WriteLog("Elmo", "7", device, memberName, "End.");
        }

        public void EnableMoveAxis([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            WriteLog("Elmo", "7", device, memberName, "Start.");

            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();

            // Servo on real EnumAxis
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsVirtualDevice && !allAxisList[i].Config.IsGroup && allAxisList[i].Config.Type == EnumAxisType.Move)
                {
                    EnableAxis(allAxisList[i].Config.ID, memberName);

                    servoOnTimer.Reset();
                    servoOnTimer.Start();

                    while (allAxisList[i].FeedbackData.Disable && servoOnTimer.ElapsedMilliseconds < ServoOnTimeOut)
                        Thread.Sleep(10);

                    if (allAxisList[i].FeedbackData.Disable)
                        WriteLog("Elmo", "4", device, memberName, "Enable timeout.");
                }
            }

            // Servo on group EnumAxis
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsGroup && allAxisList[i].Config.Type == EnumAxisType.Move)
                    EnableAxis(allAxisList[i].Config.ID, memberName);
            }

            // Servo on real VirtualAxis
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].Config.IsVirtualDevice && allAxisList[i].Config.Type == EnumAxisType.Move)
                    EnableAxis(allAxisList[i].Config.ID, memberName);
            }

            WriteLog("Elmo", "7", device, memberName, "End.");
        }

        #endregion

        private void ResetError(EnumAxis axis,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (allAxis[axis].Config.IsGroup)
                    allAxis[axis].GroupAxis.GroupReset();
                else
                {
                    if (allAxis[axis].FeedbackData.ErrorStop)
                        allAxis[axis].SingleAxis.Reset();
                }
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        public void ResetErrorAll([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            WriteLog("Elmo", "7", device, memberName, "Start.");
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

            WriteLog("Elmo", "7", device, memberName, "End.");
        }

        // 對外開放 比對四軸角度: 回傳true false, 給目標四軸的角度跟允許誤差.
        public bool WheelAngleCompare(double angle_FL, double angle_FR, double angle_RL, double angle_RR, double range,
                                      [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected)
                    return false;

                return Math.Abs(allAxis[EnumAxis.TFL].FeedbackData.Feedback_Position - angle_FL) < range &&
                       Math.Abs(allAxis[EnumAxis.TFR].FeedbackData.Feedback_Position - angle_FR) < range &&
                       Math.Abs(allAxis[EnumAxis.TRL].FeedbackData.Feedback_Position - angle_RL) < range &&
                       Math.Abs(allAxis[EnumAxis.TRR].FeedbackData.Feedback_Position - angle_RR) < range;
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
                return false;
            }
        }

        public bool WheelAngleCompare(double angle_ALL, double range,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return WheelAngleCompare(angle_ALL, angle_ALL, angle_ALL, angle_ALL, range, memberName);
        }

        #region Move VChange
        private void ElmoMoveGroupAxisAbsolute(EnumAxis axis, double distance_FL, double distance_FR, double distance_RL, double distance_RR,
                                                  double velocity, double acceleration, double deceleration, double jerk,
                                                  [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                //if (allAxis[axis].FeedbackData.Disable)
                //    return;

                if (axis == EnumAxis.GX)
                    return;

                if (axis == EnumAxis.GT)
                {
                    if (Math.Abs(distance_FL - allAxis[EnumAxis.TFL].LastCommandPosition) < 0.1 &&
                        Math.Abs(distance_FR - allAxis[EnumAxis.TFR].LastCommandPosition) < 0.1 &&
                        Math.Abs(distance_RL - allAxis[EnumAxis.TRL].LastCommandPosition) < 0.1 &&
                        Math.Abs(distance_RR - allAxis[EnumAxis.TRR].LastCommandPosition) < 0.1)
                    {
                        return;
                    }
                }

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

                WriteLog("Elmo", "5", device, memberName, axis.ToString() + " distance_FL : " + distance_FL.ToString("0.00") +
                            ", distance_FR : " + distance_FR.ToString("0.00") +
                            ", distance_RL : " + distance_RL.ToString("0.00") +
                            ", distance_RR : " + distance_RR.ToString("0.00") +
                            ", velocity : " + velocity.ToString("0") + ", acc : " + acceleration.ToString("0") +
                            ", dec : " + deceleration.ToString("0") + ", jerk : " + jerk.ToString("0"));
                allAxis[axis].GroupAxis.MoveLinearAbsolute(
                              (float)velocity, (float)acceleration, (float)deceleration, (float)jerk,
                              realArray,
                              MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE,// MC_ABORTING_MODE
                              MC_COORD_SYSTEM_ENUM.MC_ACS_COORD,
                              NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE,
                              Transition, 1, 1);

                allAxis[EnumAxis.TFL].LastCommandPosition = distance_FL;
                allAxis[EnumAxis.TFR].LastCommandPosition = distance_FR;
                allAxis[EnumAxis.TRL].LastCommandPosition = distance_RL;
                allAxis[EnumAxis.TRR].LastCommandPosition = distance_RR;
            }
            catch (MMCException ex)
            {
                for (int i = 0; i < allAxis[axis].Config.GroupOrder.Count; i++)
                    allAxis[allAxis[axis].Config.GroupOrder[i]].NeedAssignLastCommandPosition = true;
                WriteLog("Elmo", "3", device, memberName, "LastCommand ::" +
                             "TFL : " + allAxis[EnumAxis.TFL].LastCommandPosition.ToString("0.00") +
                            ",TFR : " + allAxis[EnumAxis.TFR].LastCommandPosition.ToString("0.00") +
                            ",TRL : " + allAxis[EnumAxis.TRL].LastCommandPosition.ToString("0.00") +
                            ",TRR : " + allAxis[EnumAxis.TRR].LastCommandPosition.ToString("0.00") + "\r\n" +
                            ex.ToString());
            }
        }

        private void ElmoMoveGroupAxisRelative(EnumAxis axis, double distance_FL, double distance_FR, double distance_RL, double distance_RR,
                                                  double velocity, double acceleration, double deceleration, double jerk,
                                                  [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                //if (allAxis[axis].FeedbackData.Disable)
                //    return;

                if (distance_FL == 0 && distance_FR == 0 && distance_RL == 0 && distance_RR == 0)
                    return;

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

                WriteLog("Elmo", "5", device, memberName, axis.ToString() + " distance_FL : " + distance_FL.ToString("0.00") +
                            ", distance_FR : " + distance_FR.ToString("0.00") +
                            ", distance_RL : " + distance_RL.ToString("0.00") +
                            ", distance_RR : " + distance_RR.ToString("0.00") +
                            ", velocity : " + velocity.ToString("0") + ", acc : " + acceleration.ToString("0") +
                            ", dec : " + deceleration.ToString("0") + ", jerk : " + jerk.ToString("0"));
                allAxis[axis].GroupAxis.MoveLinearRelative(
                              (float)velocity, (float)acceleration, (float)deceleration, (float)jerk,
                              realArray,
                              MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE,
                              MC_COORD_SYSTEM_ENUM.MC_ACS_COORD,
                              NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE,
                              Transition, 1, 1);

                allAxis[allAxis[axis].Config.CommandOrder[0]].LastCommandPosition += distance_FL;
                allAxis[allAxis[axis].Config.CommandOrder[1]].LastCommandPosition += distance_FR;
                allAxis[allAxis[axis].Config.CommandOrder[2]].LastCommandPosition += distance_RL;
                allAxis[allAxis[axis].Config.CommandOrder[3]].LastCommandPosition += distance_RR;
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        private void ElmoMoveSingleAxisRelative(EnumAxis axis, double distance, double velocity, double acceleration,
                                                double deceleration, double jerk,
                                                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                if (distance == 0)
                    return;

                WriteLog("Elmo", "5", device, memberName, axis.ToString() + " distance : " + distance.ToString("0.00") +
                            ", velocity : " + velocity.ToString("0") + ", acc : " + acceleration.ToString("0") +
                            ", dec : " + deceleration.ToString("0") + ", jerk : " + jerk.ToString("0"));
                allAxis[axis].SingleAxis.MoveRelative(
                              distance, (float)velocity,
                              (float)acceleration,
                              (float)deceleration,
                              (float)jerk,
                              MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION,
                              MC_BUFFERED_MODE_ENUM.MC_BUFFERED_MODE);

                allAxis[axis].LastCommandPosition += distance;
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        private void ElmoMoveSingleAxisAbsolute(EnumAxis axis, double distance, double velocity, double acceleration,
                                                double deceleration, double jerk,
                                                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                if (!allAxis[axis].Config.IsVirtualDevice && Math.Abs(distance - allAxis[axis].LastCommandPosition) < 0.1)
                    return;


                WriteLog("Elmo", "5", device, memberName, axis.ToString() + " distance : " + distance.ToString("0.00") +
                            ", velocity : " + velocity.ToString("0") + ", acc : " + acceleration.ToString("0") +
                            ", dec : " + deceleration.ToString("0") + ", jerk : " + jerk.ToString("0"));
                allAxis[axis].SingleAxis.MoveAbsolute(
                              distance, (float)velocity,
                              (float)acceleration,
                              (float)deceleration,
                              (float)jerk,
                              MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION,
                              MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                allAxis[axis].LastCommandPosition = distance;
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        private void ElmoStopGroupAxis(EnumAxis axis, double deceleration, double jerk,
                                      [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                double sqrt = Math.Sqrt(allAxis[axis].Config.GroupOrder.Count());
                deceleration *= sqrt;
                jerk *= sqrt;

                WriteLog("Elmo", "5", device, memberName, axis.ToString() + " dec : " + deceleration.ToString("0.00") +
                                                                            ", jerk : " + jerk.ToString("0"));
                allAxis[axis].GroupAxis.GroupStop((float)deceleration, (float)jerk, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                for (int i = 0; i < allAxis[axis].Config.GroupOrder.Count; i++)
                    allAxis[allAxis[axis].Config.GroupOrder[i]].NeedAssignLastCommandPosition = true;
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        private void ElmoStopSingleAxis(EnumAxis axis, double deceleration, double jerk,
                                        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (allAxis[axis].FeedbackData.Disable)
                    return;

                WriteLog("Elmo", "5", device, memberName, axis.ToString() + " dec : " + deceleration.ToString("0.00") +
                                                                            ", jerk : " + jerk.ToString("0"));
                allAxis[axis].SingleAxis.Stop((float)deceleration, (float)jerk, MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE);

                allAxis[axis].NeedAssignLastCommandPosition = true;
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }

        // 對外開放 可Group(四軸同距離)可Single(虛實皆可以), acc dec jerk 可不填,不填使用MotionParameter預設值.
        public void ElmoMove(EnumAxis axis, double distance, double velocity, EnumMoveType type,
                             double acceleration = -1, double deceleration = -1, double jerk = -1,
                             [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (!Connected)
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
                    ElmoMoveGroupAxisAbsolute(axis, distance, distance, distance, distance, velocity, acceleration, deceleration, jerk, memberName);
                else if (type == EnumMoveType.Relative)
                    ElmoMoveGroupAxisRelative(axis, distance, distance, distance, distance, velocity, acceleration, deceleration, jerk, memberName);
            }
            else
            {
                if (type == EnumMoveType.Absolute)
                    ElmoMoveSingleAxisAbsolute(axis, distance, velocity, acceleration, deceleration, jerk, memberName);
                else if (type == EnumMoveType.Relative)
                    ElmoMoveSingleAxisRelative(axis, distance, velocity, acceleration, deceleration, jerk, memberName);
            }
        }

        // 對外開放 必須是Group, acc dec jerk 可不填,不填使用MotionParameter預設值.
        public void ElmoMove(EnumAxis axis, double distance_FL, double distance_FR, double distance_RL, double distance_RR,
                            double velocity, EnumMoveType type, double acceleration = -1, double deceleration = -1, double jerk = -1,
                            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (!Connected || !allAxis[axis].Config.IsGroup)
                return;

            if (acceleration == -1)
                acceleration = allAxis[axis].Config.Acceleration;

            if (deceleration == -1)
                deceleration = allAxis[axis].Config.Deceleration;

            if (jerk == -1)
                jerk = allAxis[axis].Config.Jerk;

            if (type == EnumMoveType.Absolute)
                ElmoMoveGroupAxisAbsolute(axis, distance_FL, distance_FR, distance_RL, distance_RR, velocity, acceleration, deceleration, jerk, memberName);
            else if (type == EnumMoveType.Relative)
                ElmoMoveGroupAxisRelative(axis, distance_FL, distance_FR, distance_RL, distance_RR, velocity, acceleration, deceleration, jerk, memberName);
        }

        // 對外開放 可Group(四軸同距離)可Single(虛實皆可以), dec jerk 可不填,不填使用MotionParameter預設值.
        public void ElmoStop(EnumAxis axis, double deceleration = -1, double jerk = -1,
                            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (!Connected)
                return;

            if (deceleration == -1)
                deceleration = allAxis[axis].Config.Deceleration;

            if (jerk == -1)
                jerk = allAxis[axis].Config.Jerk;

            if (allAxis[axis].Config.IsGroup)
                ElmoStopGroupAxis(axis, deceleration, jerk, memberName);
            else
                ElmoStopSingleAxis(axis, deceleration, jerk, memberName);
        }

        // 對外開放 必須是Group Change Velocity
        public void ElmoGroupVelocityChange(EnumAxis axis, double velocityRatio,
                                           [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected || /*allAxis[axis].FeedbackData.Disable || */!allAxis[axis].Config.IsGroup)
                    return;

                if (velocityRatio > 1 || velocityRatio < 0)
                    return;

                WriteLog("Elmo", "5", device, memberName, axis.ToString() + " VChange : " + velocityRatio.ToString("0.00"));
                allAxis[axis].GroupAxis.GroupSetOverride((float)velocityRatio, 1, 1, 0); // acc, jerk 先不調整, 調整的位置估算太難做.
                //allAxis[axis].GroupAxis.Execute = 1;
            }
            catch (MMCException ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
            }
        }
        #endregion

        // 對外開放 讀取position, 只能轉向4實體和 走行"前左","後右"
        public double ElmoGetPosition(EnumAxis axis, bool hasTimeOffset = false,
                                     [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected)
                    return -1;

                if (allAxis[axis].Config.IsGroup)
                {
                    double position = 0;
                    for (int i = 0; i < allAxis[axis].Config.GroupOrder.Count; i++)
                        position += allAxis[allAxis[axis].Config.GroupOrder[i]].FeedbackData.Feedback_Position;

                    return position / allAxis[axis].Config.GroupOrder.Count;
                }
                else
                    return allAxis[axis].FeedbackData.Feedback_Position;
            }
            catch (Exception ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
                return -1;
            }
        }

        // 對外開放 讀取velocity.
        public double ElmoGetVelocity(EnumAxis axis, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected)
                    return -1;

                if (allAxis[axis].Config.IsGroup)
                {
                    double vel = 0;
                    for (int i = 0; i < allAxis[axis].Config.GroupOrder.Count; i++)
                        vel += allAxis[allAxis[axis].Config.GroupOrder[i]].FeedbackData.Feedback_Velocity;

                    return vel / allAxis[axis].Config.GroupOrder.Count;
                }
                else
                    return allAxis[axis].FeedbackData.Feedback_Velocity;
            }
            catch (Exception ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
                return -1;
            }
        }

        public bool ElmoGetDisable(EnumAxis axis, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected)
                    return true;

                return allAxis[axis].FeedbackData.Disable;
            }
            catch (Exception ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
                return true;
            }
        }

        public ElmoAxisFeedbackData ElmoGetFeedbackData(EnumAxis axis, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected)
                    return null;

                return allAxis[axis].FeedbackData;
            }
            catch (Exception ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
                return null;
            }
        }

        public bool MoveCompelete(EnumAxis axis, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected)
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
            catch (Exception ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
                return true;
            }
        }

        public bool MoveCompeleteVirtual(EnumAxisType type, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected)
                    return true;

                for (int i = 0; i < MAX_AXIS; i++)
                {
                    if (allAxisList[i].Config.IsVirtualDevice && allAxisList[i].Config.Type == type)
                    {
                        if (!allAxisList[i].FeedbackData.StandStill)
                            return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
                return true;
            }
        }

        public bool GetLink(EnumAxis axis, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            try
            {
                if (!Connected)
                    return true;

                if (allAxis[axis].Config.IsVirtualDevice)
                    return allAxis[allAxis[axis].Config.VirtualDev4ID].Linking;
                else
                    return false;
            }
            catch (Exception ex)
            {
                WriteLog("Elmo", "3", device, memberName, "Excption : " + ex.ToString());
                return false;
            }
        }

        public bool CheckAxisNoError()
        {
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (!allAxisList[i].Config.IsGroup && allAxisList[i].FeedbackData.ErrorStop)
                    return false;
            }

            return true;
        }

        public bool CheckAxisEnableAndLinked()
        {
            for (int i = 0; i < MAX_AXIS; i++)
            {
                if (allAxisList[i].FeedbackData.Disable)
                    return false;

                if (allAxisList[i].Config.IsVirtualDevice && !allAxisList[i].Linking)
                    return false;
            }

            return true;
        }
    }
}
