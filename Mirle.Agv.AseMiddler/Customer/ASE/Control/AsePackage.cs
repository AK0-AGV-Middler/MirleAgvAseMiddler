using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSDriver.PSDriver;
using Mirle.Tools;
using System.Reflection;
using Mirle.Agv.AseMiddler.Model.Configs;
using System.IO;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using System.Threading;
using SimpleWifi;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class AsePackage
    {
        public PSWrapperXClass psWrapper;
        public AseMoveControl aseMoveControl;
        public AseRobotControl aseRobotControl;
        public AseBatteryControl aseBatteryControl;
        public AseBuzzerControl aseBuzzerControl;
        public MirleLogger mirleLogger = MirleLogger.Instance;
        private AsePackageConfig asePackageConfig = new AsePackageConfig();
        private PspConnectionConfig pspConnectionConfig = new PspConnectionConfig();
        private AseBatteryConfig aseBatteryConfig = new AseBatteryConfig();
        private AseMoveConfig aseMoveConfig = new AseMoveConfig();
        private Vehicle theVehicle = Vehicle.Instance;
        public Dictionary<string, PSMessageXClass> psMessageMap = new Dictionary<string, PSMessageXClass>();
        private Thread thdWatchWifiSignalStrength;
        public bool IsWatchWifiSignalStrengthPause { get; set; } = false;
        public bool IsWatchWifiSignalStrengthStop { get; set; } = false;

        public event EventHandler<bool> OnConnectionChangeEvent;
        public event EventHandler<string> AllPspLog;
        public event EventHandler<string> ImportantPspLog;
        public event EventHandler<string> OnStatusChangeReportEvent;
        public event EventHandler<AseMoveStatus> OnPartMoveArrivalEvent;
        public event EventHandler<AseMoveStatus> OnPositionChangeEvent;
        public event EventHandler OnAgvlErrorEvent;
        public event EventHandler ArrivalCharge;
        public event EventHandler<EnumAutoState> OnModeChangeEvent;

        public AsePackage(Dictionary<string, string> gateTypeMap)
        {
            LoadConfigs();
            InitialWrapper();
            InitialAseMoveControl();
            InitialAseRobotControl(gateTypeMap);
            InitialAseBatteryControl();
            InitialAseBuzzerControl();
            //InitialThread();
        }

        private void InitialThread()
        {
            thdWatchWifiSignalStrength = new Thread(WatchWifiSignalStrength);
            thdWatchWifiSignalStrength.IsBackground = true;
            thdWatchWifiSignalStrength.Start();
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "AsePackage : Start WatchWifiSignalStrength");
        }

        private void WatchWifiSignalStrength()
        {
            while (true)
            {
                try
                {
                    if (IsWatchWifiSignalStrengthPause) continue;
                    if (IsWatchWifiSignalStrengthStop) break;

                    if (psWrapper.IsConnected())
                    {
                        SendWifiSignalStrength();
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
                finally
                {
                    SpinWait.SpinUntil(() => (IsWatchWifiSignalStrengthPause || IsWatchWifiSignalStrengthStop), asePackageConfig.WatchWifiSignalIntervalMs);
                }
            }
        }

        private void SendWifiSignalStrength()
        {
            try
            {
                if (Vehicle.Instance.IsAgvcConnect)
                {
                List<AccessPoint> accessPoints = new Wifi().GetAccessPoints();
                if (accessPoints.Any())
                {
                    var connectedAccessPoint = accessPoints.FirstOrDefault(x => x.IsConnected);
                    if (connectedAccessPoint != null)
                    {
                        SendWifiSignalStrength(connectedAccessPoint.SignalStrength);
                        return;
                    }
                }
                }    

                SendWifiSignalStrength(0);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                SendWifiSignalStrength(0);
            }
        }

        private void SendWifiSignalStrength(uint signalStrength)
        {
            try
            {
                PrimarySend("19", signalStrength.ToString().PadLeft(3, '0'));
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void InitialAseBuzzerControl()
        {
            try
            {
                aseBuzzerControl = new AseBuzzerControl();
                aseBuzzerControl.OnPrimarySendEvent += AseControl_OnPrimarySendEvent;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void InitialAseBatteryControl()
        {
            try
            {
                aseBatteryControl = new AseBatteryControl(psWrapper, aseBatteryConfig);
                aseBatteryControl.OnPrimarySendEvent += AseControl_OnPrimarySendEvent;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void InitialAseRobotControl(Dictionary<string, string> gateTypeMap)
        {
            try
            {
                aseRobotControl = new AseRobotControl(gateTypeMap);
                aseRobotControl.OnPrimarySendEvent += AseControl_OnPrimarySendEvent;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void InitialAseMoveControl()
        {
            try
            {
                aseMoveControl = new AseMoveControl(psWrapper, aseMoveConfig);
                aseMoveControl.OnPrimarySendEvent += AseControl_OnPrimarySendEvent;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LoadConfigs()
        {
            XmlHandler xmlHandler = new XmlHandler();
            asePackageConfig = xmlHandler.ReadXml<AsePackageConfig>("AsePackageConfig.xml");
            pspConnectionConfig = xmlHandler.ReadXml<PspConnectionConfig>(asePackageConfig.PspConnectionConfigFilePath);
            theVehicle.PspSpecVersion = pspConnectionConfig.SpecVersion;
            aseBatteryConfig = xmlHandler.ReadXml<AseBatteryConfig>(asePackageConfig.AseBatteryConfigFilePath);
            aseMoveConfig = xmlHandler.ReadXml<AseMoveConfig>(asePackageConfig.AseMoveConfigFilePath);

            if (theVehicle.IsSimulation)
            {
                aseBatteryConfig.WatchBatteryStateInterval = 30 * 1000;
                aseBatteryConfig.WatchBatteryStateIntervalInCharging = 30 * 1000;
                aseMoveConfig.WatchPositionInterval = 200;
            }
        }

        private void InitialWrapper()
        {
            try
            {
                LoadAutoReply();
                LoadPspConnectionConfig();
                BindPsWrapperEvent();

                if (!theVehicle.IsSimulation)
                {
                    psWrapper.Open();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void BindPsWrapperEvent()
        {
            try
            {
                psWrapper.OnConnectionStateChange += PsWrapper_OnConnectionStateChange;
                psWrapper.OnPrimarySent += PsWrapper_OnPrimarySent;
                psWrapper.OnPrimaryReceived += PsWrapper_OnPrimaryReceived;
                psWrapper.OnSecondarySent += PsWrapper_OnSecondarySent;
                psWrapper.OnSecondaryReceived += PsWrapper_OnSecondaryReceived;
                psWrapper.OnTransactionError += PsWrapper_OnTransactionError;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        #region PrimarySend

        private void PsWrapper_OnPrimarySent(ref PSTransactionXClass transaction)
        {
            try
            {
                //string msg = $"PrimarySent : [{transaction.PSPrimaryMessage.ToString()}]";
                //AllPspLog?.Invoke(this, msg);
                //LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public PSTransactionXClass PrimarySend(string index, string message)
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Type = "P";
                psMessage.Number = index.Substring(0, 2);
                psMessage.PSMessage = message;
                PSTransactionXClass psTransaction = new PSTransactionXClass();
                psTransaction.PSPrimaryMessage = psMessage;

                psWrapper.PrimarySent(ref psTransaction);

                string msg = $"PrimarySent : [{psTransaction.PSPrimaryMessage.ToString()}]";
                AllPspLog?.Invoke(this, msg);
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

                return psTransaction;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return null;
            }
        }

        private void AseControl_OnPrimarySendEvent(object sender, PSTransactionXClass psTransaction)
        {
            try
            {
                psWrapper.PrimarySent(ref psTransaction);

                string msg = $"PrimarySent : [{psTransaction.PSPrimaryMessage.ToString()}]";
                AllPspLog?.Invoke(this, msg);
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        private void PsWrapper_OnTransactionError(string errorString, ref PSMessageXClass psMessage)
        {
            if (psMessage == null)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, errorString + "\r\n PsMessage is null");

            }
            else
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, errorString + "\r\n" + psMessage.ToString());

            }
        }

        public void SetLocalDateTime()
        {
            try
            {
                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                PrimarySend("15", timeStamp);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AllAgvlStatusReportRequest()
        {
            try
            {
                PrimarySend("31", "0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void SetTransferCommandInfoRequest()
        {
            try
            {
                List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                foreach (var agvcTransCmd in agvcTransCmds)
                {
                    string transferCommandInfo = GetTransferCommandInfo(agvcTransCmd);
                    PrimarySend("37", transferCommandInfo);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string GetTransferCommandInfo(AgvcTransCmd agvcTransCmd)
        {
            try
            {
                string commandId = agvcTransCmd.CommandId.PadLeft(20, '0');
                string fromPortNum = agvcTransCmd.LoadAddressId.PadLeft(5, '0').Substring(0, 5);
                string toPortNum = agvcTransCmd.UnloadAddressId.PadLeft(5, '0').Substring(0, 5);
                string vehicleSlot = agvcTransCmd.SlotNumber.ToString();
                string lotId = agvcTransCmd.LotId.PadLeft(40, '0').Substring(0, 40);
                string cassetteId = agvcTransCmd.CassetteId;
                return string.Concat(commandId, fromPortNum, toPortNum, vehicleSlot, lotId, cassetteId);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return "";
            }
        }

        #endregion

        #region PrimaryReceived

        private void PsWrapper_OnPrimaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
                string msg = $"PrimaryReceived : [{transaction.ToString()}]";
                AllPspLog?.Invoke(this, msg);
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

                AutoReplyFromPsMessageMap(transaction);
                DealPrimaryReceived(transaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void DealPrimaryReceived(PSTransactionXClass transaction)
        {
            try
            {
                if (transaction.PSPrimaryMessage == null)
                {
                    throw new Exception("Primary message is empty.");
                }

                if (transaction.PSPrimaryMessage.Type.ToUpper() != "P")
                {
                    throw new Exception("Primary message type is not P.");
                }

                switch (transaction.PSPrimaryMessage.Number)
                {
                    case "11":
                        SetVehicleAuto();
                        break;
                    case "13":
                        SetVehicleManual();
                        break;
                    case "21":
                        UpdateMoveStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "23":
                        UpdateRobotStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "25":
                        UpdateCarrierSlotStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "29":
                        UpdateChargeStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "43":
                        ReceiveMoveAppendArrivalReport(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "53":
                        ReceiveRobotCommandFinishedReport(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "61":
                        AlarmReport(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "63":
                        AllAlarmReset();
                        break;
                    case "97":
                        ShowTestMsg();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReceiveRobotCommandFinishedReport(string psMessage)
        {
            string finishedMsg = psMessage.Trim();
            switch (finishedMsg)
            {
                case "Finished":
                    aseRobotControl.OnRobotCommandFinish();
                    break;
                case "InterlockError":
                    aseRobotControl.OnRobotInterlockError();
                    //OnAgvlErrorEvent?.Invoke(this, new EventArgs());
                    break;
                default:
                    aseRobotControl.OnRobotCommandError();
                    OnAgvlErrorEvent?.Invoke(this, new EventArgs());
                    break;
            }
        }

        private void AutoReplyFromPsMessageMap(PSTransactionXClass psTransaction)
        {
            try
            {
                if (!IsPrimaryEmpty(psTransaction))
                {
                    if (IsPrimaryInMessageMap(psTransaction))
                    {
                        ReplyFromMsgMap(psTransaction);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReplyFromMsgMap(PSTransactionXClass psTransaction)
        {
            try
            {
                PSMessageXClass primaryMessage = psTransaction.PSPrimaryMessage;
                int primaryNumber = int.Parse(primaryMessage.Number);
                string secondaryTypeNumber = "S" + (primaryNumber + 1).ToString("00");

                if (psMessageMap.ContainsKey(secondaryTypeNumber))
                {
                    psTransaction.PSSecondaryMessage = psMessageMap[secondaryTypeNumber];
                    psWrapper.SecondarySent(ref psTransaction);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool IsPrimaryInMessageMap(PSTransactionXClass psTransaction)
        {
            try
            {
                if (psMessageMap.Count <= 0)
                {
                    throw new Exception("PsMessageMap is empty");
                }

                PSMessageXClass primaryMessage = psTransaction.PSPrimaryMessage;
                if (primaryMessage.Type.ToUpper() != "P")
                {
                    throw new Exception($"PrimaryReceive transaction.primaryMessage.type is not P.[{primaryMessage.Type}]");
                }

                return psMessageMap.ContainsKey(primaryMessage.Type + primaryMessage.Number);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }

        private bool IsPrimaryEmpty(PSTransactionXClass psTransaction)
        {
            return psTransaction.PSPrimaryMessage == null;
        }

        private void ShowTestMsg()
        {
            ImportantPspLog?.Invoke(this, "A test msg from AGVL.");
        }

        private void ReceiveMoveAppendArrivalReport(string psMessage)
        {
            try
            {
                EnumAseArrival aseArrival = (EnumAseArrival)Enum.Parse(typeof(EnumAseArrival), psMessage.Substring(0, 1));
                switch (aseArrival)
                {
                    case EnumAseArrival.Fail:
                        aseMoveControl.MoveFinished(EnumMoveComplete.Fail);
                        OnAgvlErrorEvent?.Invoke(this, new EventArgs());
                        break;
                    case EnumAseArrival.Arrival:
                        ArrivalPosition(psMessage);
                        break;
                    case EnumAseArrival.EndArrival:
                        ArrivalPosition(psMessage);
                        aseMoveControl.MoveFinished(EnumMoveComplete.Success);
                        break;
                    default:
                        break;
                }

                OnStatusChangeReportEvent?.Invoke(this, $"ReceiveMoveAppendArrivalReport:[{aseArrival}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ArrivalPosition(string psMessage)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

                double x = GetPositionFromPsMessage(psMessage.Substring(1, 9));                
                double y = GetPositionFromPsMessage(psMessage.Substring(10, 9));
                aseMoveStatus.LastMapPosition = new MapPosition(x, y);
                aseMoveStatus.HeadDirection = int.Parse(psMessage.Substring(19, 3));
                aseMoveStatus.MovingDirection = int.Parse(psMessage.Substring(22, 3));
                aseMoveStatus.Speed = int.Parse(psMessage.Substring(25, 4));

                OnPartMoveArrivalEvent?.Invoke(this, aseMoveStatus);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AllAlarmReset()
        {
            try
            {
                aseBuzzerControl.OnAlarmCodeAllReset();

                OnStatusChangeReportEvent?.Invoke(this, $"AllAlarmReset:");

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AlarmReport(string psMessage)
        {
            try
            {
                string isAlarmSet = psMessage.Substring(0, 1).Trim();
                int alarmCode = int.Parse(psMessage.Substring(1, 6));

                aseBuzzerControl.OnAlarmCodeSet(alarmCode, IsValueTrue(isAlarmSet));

                OnStatusChangeReportEvent?.Invoke(this, $"AlarmReport:[{ alarmCode}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateChargeStatus(string psMessage)
        {
            try
            {
                bool isCharging = IsValueTrue(psMessage.Substring(0, 1).ToUpper().Trim());
                if (theVehicle.IsCharging != isCharging)
                {
                    theVehicle.IsCharging = isCharging;
                    OnStatusChangeReportEvent?.Invoke(this, $"UpdateChargeStatus:[{ theVehicle.IsCharging }]");
                    if (isCharging)
                    {
                        if (theVehicle.AseBatteryStatus.Percentage + 3 >= aseBatteryConfig.FullChargePercentage)
                        {
                            aseBatteryControl.FullCharge();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool IsValueTrue(string v)
        {
            switch (v)
            {
                case "1":
                    return true;
                case "0":
                    return false;
                default:
                    throw new Exception($"My bool parse fail. {v}");
            }
        }

        private void UpdateCarrierSlotStatus(string psMessage)
        {
            try
            {
                AseCarrierSlotStatus aseCarrierSlotStatus = new AseCarrierSlotStatus();
                aseCarrierSlotStatus.CarrierSlotStatus = (EnumAseCarrierSlotStatus)Enum.Parse(typeof(EnumAseCarrierSlotStatus), psMessage.Substring(1, 1));
                aseCarrierSlotStatus.CarrierId = psMessage.Substring(2);
                EnumSlotNumber slotNumber = (EnumSlotNumber)Enum.Parse(typeof(EnumSlotNumber), psMessage.Substring(0, 1));
                switch (slotNumber)
                {
                    case EnumSlotNumber.L:
                        theVehicle.AseCarrierSlotL = aseCarrierSlotStatus;
                        break;
                    case EnumSlotNumber.R:
                        theVehicle.AseCarrierSlotR = aseCarrierSlotStatus;
                        break;
                    default:
                        throw new Exception($"PsMessage slot number error.[{psMessage.Substring(0, 1)}]");
                }

                aseRobotControl.OnReadCarrierIdFinish(slotNumber);

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateCarrierSlotStatus:[{slotNumber}][{theVehicle.GetAseCarrierSlotStatus(slotNumber).CarrierSlotStatus}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateRobotStatus(string psMessage)
        {
            try
            {
                AseRobotStatus aseRobotStatus = new AseRobotStatus();
                var robotState = (EnumAseRobotState)Enum.Parse(typeof(EnumAseRobotState), psMessage.Substring(0, 1));
                aseRobotStatus.RobotState = robotState;
                aseRobotStatus.IsHome = IsValueTrue(psMessage.Substring(1, 1));

                theVehicle.AseRobotStatus = aseRobotStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateRobotStatus:[{theVehicle.AseRobotStatus.RobotState}][RobotHome={theVehicle.AseRobotStatus.IsHome}]");

                if (robotState== EnumAseRobotState.Error)
                {
                    OnAgvlErrorEvent?.Invoke(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateMoveStatus(string psMessage)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                var aseMoveState = (EnumAseMoveState)Enum.Parse(typeof(EnumAseMoveState), psMessage.Substring(0, 1));
                aseMoveStatus.AseMoveState = aseMoveState;
                aseMoveStatus.HeadDirection = int.Parse(psMessage.Substring(1, 3));
                theVehicle.AseMoveStatus = aseMoveStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateMoveStatus:[{theVehicle.AseMoveStatus.AseMoveState}]");

                if (aseMoveState== EnumAseMoveState.Error)
                {
                    OnAgvlErrorEvent?.Invoke(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void SetVehicleManual()
        {
            OnModeChangeEvent?.Invoke(this, EnumAutoState.Manual);

            //theVehicle.AutoState = EnumAutoState.Manual;
            //string msg = "AGVL switch to  : Manual";
            //ImportantPspLog?.Invoke(this, msg);
        }

        private void SetVehicleAuto()
        {
            OnModeChangeEvent?.Invoke(this, EnumAutoState.Auto);

            //theVehicle.AutoState = EnumAutoState.Auto;

            //string msg = "AGVL switch to  : Auto";
            //ImportantPspLog?.Invoke(this, msg);
        }

        public void SetVehicleAutoScenario()
        {
            SetLocalDateTime();
            SpinWait.SpinUntil(() => false, 1000);

            AllAgvlStatusReportRequest();
            SpinWait.SpinUntil(() => false, 1000);

            aseMoveControl.SendPositionReportRequest();
            SpinWait.SpinUntil(() => false, 1000);

            aseBatteryControl.SendBatteryStatusRequest();
            SpinWait.SpinUntil(() => false, 1000);

            SetTransferCommandInfoRequest();
        }

        #endregion

        #region SecondarySend

        private void PsWrapper_OnSecondarySent(ref PSTransactionXClass transaction)
        {
            try
            {
                string msg = $"SecodarySent : [{transaction.ToString()}]";
                AllPspLog?.Invoke(this, msg);
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        #endregion

        #region SecondaryReceived

        private void OnSecondaryReceived(PSTransactionXClass transaction)
        {
            try
            {
                if (transaction.PSSecondaryMessage == null)
                {
                    throw new Exception("Secondary message is empty.");
                }

                if (transaction.PSSecondaryMessage.Type.ToUpper() != "S")
                {
                    throw new Exception("Secondary message type is not S.");
                }


                switch (transaction.PSSecondaryMessage.Number)
                {
                    case "34":
                        ReceivePositionReportRequestAck(transaction.PSSecondaryMessage.PSMessage);
                        break;
                    case "36":
                        ReceiveBatteryStatusRequestAck(transaction.PSSecondaryMessage.PSMessage);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReceiveBatteryStatusRequestAck(string psMessage)
        {
            try
            {
                AseBatteryStatus aseBatteryStatus = new AseBatteryStatus(theVehicle.AseBatteryStatus);
                aseBatteryStatus.Percentage = int.Parse(psMessage.Substring(0, 3));
                aseBatteryStatus.Voltage = int.Parse(psMessage.Substring(3, 4)) * 0.01;
                aseBatteryStatus.Temperature = int.Parse(psMessage.Substring(7, 3));
                theVehicle.AseBatteryStatus = aseBatteryStatus;

                if (theVehicle.IsCharging)
                {
                    if (aseBatteryStatus.Percentage + 3 >= aseBatteryConfig.FullChargePercentage)
                {
                    aseBatteryControl.FullCharge();
                }
            }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReceivePositionReportRequestAck(string psMessage)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                aseMoveStatus.AseMoveState = (EnumAseMoveState)Enum.Parse(typeof(EnumAseMoveState), psMessage.Substring(0, 1));
                double x = GetPositionFromPsMessage(psMessage.Substring(1, 9));
                double y = GetPositionFromPsMessage(psMessage.Substring(10, 9));
                aseMoveStatus.LastMapPosition = new MapPosition(x, y);               
                aseMoveStatus.HeadDirection = int.Parse(psMessage.Substring(19, 3));
                aseMoveStatus.MovingDirection = int.Parse(psMessage.Substring(22, 3));
                aseMoveStatus.Speed = int.Parse(psMessage.Substring(25, 4));
                //theVehicle.AseMoveStatus = aseMoveStatus;
                OnPositionChangeEvent?.Invoke(this, aseMoveStatus);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private double GetPositionFromPsMessage(string v)
        {
            string isPositive = v.Substring(0, 1);
            double value = double.Parse(v.Substring(1, 8));
            return IsValuePositive(isPositive) ? value : -value;
        }

        private bool IsValuePositive(string v)
        {
            switch (v)
            {
                case "P":
                    return true;
                case "N":
                    return false;
                default:
                    throw new Exception($"My positive parse fail. {v}");
            }
        }

        private void PsWrapper_OnSecondaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
                string msg = $"SecodaryReceived : [{transaction.ToString()}]";
                AllPspLog?.Invoke(this, msg);
                LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

                OnSecondaryReceived(transaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        #endregion

        #region PsWrapper

        private void PsWrapper_OnConnectionStateChange(enumConnectState state)
        {
            try
            {
                string msg = $"PsWrapper connection state changed.[{state}]";
                if (state == enumConnectState.Connected || state == enumConnectState.Quit)
                {
                    LogPsWrapper(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                    ImportantPspLog?.Invoke(this, msg);
                }
                bool isConnected = psWrapper.ConnectionState == enumConnectState.Connected;
                theVehicle.IsAgvlConnect = isConnected;
                OnConnectionChangeEvent?.Invoke(this, isConnected);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LoadPspConnectionConfig()
        {
            try
            {
                psWrapper = new PSWrapperXClass();
                psWrapper.Address = pspConnectionConfig.Ip;
                psWrapper.Port = pspConnectionConfig.Port;
                psWrapper.ConnectMode = pspConnectionConfig.IsServer ? enumConnectMode.Passive : enumConnectMode.Active;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LoadAutoReply()
        {
            try
            {
                if (File.Exists(asePackageConfig.AutoReplyFilePath))
                {
                    LoadAutoReplyFileToMyMessageMap();
                    //FitPspMessageList();
                    //InitialSingleMsgType();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LoadAutoReplyFileToMyMessageMap()
        {
            try
            {
                string[] allRows = File.ReadAllLines(asePackageConfig.AutoReplyFilePath);

                foreach (string oneRow in allRows)
                {
                    var parts = oneRow.Split(',');
                    string key = parts[0];
                    string description = parts[1];
                    string autoMessage = parts[2];

                    PSMessageXClass psMessage = new PSMessageXClass();
                    psMessage.Type = key.Substring(0, 1);
                    psMessage.Number = key.Substring(1, 2);
                    psMessage.Description = description;
                    psMessage.PSMessage = autoMessage;

                    psMessageMap.Add(key, psMessage);
                }

                PsMessageMapAddVersionInfo();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsMessageMapAddVersionInfo()
        {
            PSMessageXClass versionInfoRequestPsMessage = new PSMessageXClass();
            versionInfoRequestPsMessage.Type = "P";
            versionInfoRequestPsMessage.Number = "17";
            versionInfoRequestPsMessage.Description = "Version Info Request";
            versionInfoRequestPsMessage.PSMessage = "";
            psMessageMap.Add("P17", versionInfoRequestPsMessage);

            string versionInfo = string.Concat("Sw", theVehicle.SoftwareVersion.PadRight(13), "Sp", theVehicle.PspSpecVersion.PadRight(5));
            PSMessageXClass versionInfoRequestAckPsMessage = new PSMessageXClass();
            versionInfoRequestAckPsMessage.Type = "S";
            versionInfoRequestAckPsMessage.Number = "18";
            versionInfoRequestAckPsMessage.Description = "Version Info Request Ack.";
            versionInfoRequestAckPsMessage.PSMessage = versionInfo;
            psMessageMap.Add("S18", versionInfoRequestAckPsMessage);
        }

        public bool IsConnected()
        {
            return psWrapper.ConnectionState == enumConnectState.Connected;
        }

        public void Connect()
        {
            psWrapper.Open();
        }

        public void DisConnect()
        {
            psWrapper.Close();
        }

        #endregion

        #region Logger

        private void LogException(string classMethodName, string exMsg)
        {
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }

        public void LogDebug(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, "Device", "CarrierID", msg));
        }

        public void LogPsWrapper(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("PsWrapper", "5", classMethodName, "Device", "CarrierID", msg));
        }

        #endregion

    }
}
