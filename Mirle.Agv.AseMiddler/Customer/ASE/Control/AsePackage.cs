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
using System.Collections.Concurrent;

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
        public Vehicle Vehicle { get; set; } = Vehicle.Instance;
        public string LocalLogMsg { get; set; } = "";

        private Thread thdWatchWifiSignalStrength;
        public bool IsWatchWifiSignalStrengthPause { get; set; } = false;
        public uint WifiSignalStrength { get; set; } = 0;

        private Thread thdSchedule;
        public bool IsSchedulePause { get; set; } = false;

        public ConcurrentQueue<PSMessageXClass> PrimarySendQueue { get; set; } = new ConcurrentQueue<PSMessageXClass>();
        public ConcurrentQueue<PSTransactionXClass> SecondarySendQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<PSTransactionXClass> PrimaryReceiveQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<PSTransactionXClass> DealPrimaryReceiveQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<PSTransactionXClass> SecondaryReceiveQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        private List<PSTransactionXClass> primaryReceiveTransactions;

        public event EventHandler<bool> OnConnectionChangeEvent;
        public event EventHandler<string> AllPspLog;
        public event EventHandler<string> ImportantPspLog;
        public event EventHandler<string> OnStatusChangeReportEvent;
        public event EventHandler<AseMoveStatus> OnPartMoveArrivalEvent;
        public event EventHandler<AseMoveStatus> OnPositionChangeEvent;
        public event EventHandler OnAgvlErrorEvent;
        public event EventHandler<EnumAutoState> OnModeChangeEvent;
        public event EventHandler<AseCarrierSlotStatus> OnUpdateSlotStatusEvent;

        public AsePackage(Dictionary<string, string> gateTypeMap)
        {
            LoadConfigs();
            InitialWrapper();
            InitialAseMoveControl();
            InitialAseRobotControl(gateTypeMap);
            InitialAseBatteryControl();
            InitialAseBuzzerControl();
            InitialThreads();
        }

        private void InitialThreads()
        {
            thdWatchWifiSignalStrength = new Thread(WatchWifiSignalStrength);
            thdWatchWifiSignalStrength.IsBackground = true;
            thdWatchWifiSignalStrength.Start();

            thdSchedule = new Thread(Schedule);
            thdSchedule.IsBackground = true;
            thdSchedule.Start();
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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

            if (theVehicle.MainFlowConfig.IsSimulation)
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
                psWrapper.T3 = pspConnectionConfig.T6Timeout;
                psWrapper.T6 = pspConnectionConfig.T6Timeout;
                psWrapper.LinkTestIntervalMs = pspConnectionConfig.LinkTestIntervalMs;

                if (!theVehicle.MainFlowConfig.IsSimulation)
                {
                    psWrapper.Open();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #region Threads

        private void WatchWifiSignalStrength()
        {
            while (true)
            {
                try
                {
                    if (IsWatchWifiSignalStrengthPause)
                    {
                        SpinWait.SpinUntil(() => false, asePackageConfig.WatchWifiSignalIntervalMs);

                        continue;
                    }

                    if (psWrapper.IsConnected())
                    {
                        SendWifiSignalStrength();
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, asePackageConfig.WatchWifiSignalIntervalMs);
                }
            }
        }

        private void SendWifiSignalStrength()
        {
            try
            {
                if (Vehicle.Instance.IsAgvcConnect)
                {
                    List<AccessPoint> accessPoints = new Wifi().GetAccessPoints().ToList();
                    if (accessPoints.Any())
                    {
                        foreach (var item in accessPoints)
                        {
                            if (item.IsConnected)
                            {
                                WifiSignalStrength = Math.Max(item.SignalStrength, 10);
                            }
                        }
                    }
                }
                else
                {
                    WifiSignalStrength = 0;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            finally
            {
                SendWifiSignalStrength(WifiSignalStrength);
            }
        }

        private void SendWifiSignalStrength(uint signalStrength)
        {
            try
            {
                PrimarySendEnqueue("P19", signalStrength.ToString().PadLeft(3, '0'));
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Schedule()
        {
            while (true)
            {
                try
                {

                    if (IsSchedulePause)
                    {
                        SpinWait.SpinUntil(() => !IsSchedulePause, asePackageConfig.ScheduleIntervalMs);

                        continue;
                    }

                    if (psWrapper.IsConnected())
                    {
                        if (PrimarySendQueue.Any())
                        {
                            PrimarySendQueue.TryDequeue(out PSMessageXClass psMessageObj);
                            PrimarySend(psMessageObj);
                        }

                        if (SecondarySendQueue.Any())
                        {
                            SecondarySendQueue.TryDequeue(out PSTransactionXClass psTransaction);
                            SecondarySend(psTransaction);
                        }

                        CheckPrimaryReceiveQueue();

                        if (DealPrimaryReceiveQueue.Any())
                        {
                            DealPrimaryReceiveQueue.TryDequeue(out PSTransactionXClass psTransaction);
                            DealPrimaryReceived(psTransaction);
                        }

                        if (SecondaryReceiveQueue.Any())
                        {
                            SecondaryReceiveQueue.TryDequeue(out PSTransactionXClass psTransaction);
                            DealSecondaryReceived(psTransaction);
                        }

                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, asePackageConfig.ScheduleIntervalMs);
                }
            }
        }

        #endregion

        #region PrimarySend

        private void PsWrapper_OnPrimarySent(ref PSTransactionXClass transaction)
        {
            try
            {
                //string msg = $"PrimarySent : [{transaction.PSPrimaryMessage.ToString()}]";
                //AllPspLog?.Invoke(this, msg);
                //LogPsWrapper(msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PrimarySendEnqueue(string index, string message)
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Type = index.Substring(0, 1);
                psMessage.Number = index.Substring(1, 2);
                psMessage.PSMessage = message;

                PrimarySendQueue.Enqueue(psMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PrimarySend(PSMessageXClass psMessageObj)
        {
            try
            {
                PSTransactionXClass psTransaction = new PSTransactionXClass();
                psTransaction.PSPrimaryMessage = psMessageObj;

                psWrapper.PrimarySent(ref psTransaction);

                LogPsWrapper($"PSEND : [{psTransaction.PSPrimaryMessage.ToString()}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AseControl_OnPrimarySendEvent(object sender, PSTransactionXClass psTransaction)
        {
            try
            {
                var primaryMessage = psTransaction.PSPrimaryMessage;
                PrimarySendEnqueue(primaryMessage.Type + primaryMessage.Number, primaryMessage.PSMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        public void SetLocalDateTime()
        {
            try
            {
                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                PrimarySendEnqueue("P15", timeStamp);
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
                PrimarySendEnqueue("P31", "0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SetTransferCommandInfoRequest(AgvcTransCmd transCmd, EnumCommandInfoStep commandInfoStep)
        {
            try
            {
                string commandStep = ((int)commandInfoStep).ToString();
                string commandId = transCmd.CommandId.PadLeft(20, '0');
                string fromPortNum = transCmd.LoadAddressId.PadLeft(5, '0').Substring(0, 5);
                string toPortNum = transCmd.UnloadAddressId.PadLeft(5, '0').Substring(0, 5);
                string vehicleSlot = transCmd.SlotNumber.ToString();
                string lotId = transCmd.LotId.PadLeft(40, '0').Substring(0, 40);
                string cassetteId = transCmd.CassetteId;
                string transferCommandInfo = string.Concat(commandStep, commandId, fromPortNum, toPortNum, vehicleSlot, lotId, cassetteId); ;
                PrimarySendEnqueue("P37", transferCommandInfo);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        #endregion

        #region PrimaryReceived

        private void CheckPrimaryReceiveQueue()
        {
            lock (PrimaryReceiveQueue)
            {
                primaryReceiveTransactions = PrimaryReceiveQueue.ToList();
                PrimaryReceiveQueue = new ConcurrentQueue<PSTransactionXClass>();
            }

            if (primaryReceiveTransactions.Any())
            {
                foreach (PSTransactionXClass psTransaction in primaryReceiveTransactions)
                {
                    AutoReplyFromPsMessageMap(psTransaction);
                }
            }
        }

        private void PsWrapper_OnPrimaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
                LogPsWrapper($"PRECV : [{transaction.PSPrimaryMessage.ToString()}]");
                PrimaryReceiveQueue.Enqueue(transaction);

                //AutoReplyFromPsMessageMap(transaction);
                //DealPrimaryReceived(transaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void ReceiveRobotCommandFinishedReport(string psMessage)
        {
            try
            {
                string finishedMsg = psMessage.Trim();
                switch (finishedMsg)
                {
                    case "Finished":
                        aseRobotControl.OnRobotCommandFinish();
                        break;
                    case "InterlockError":
                        aseRobotControl.OnRobotInterlockError();
                        break;
                    case "RobotError":
                        aseRobotControl.OnRobotCommandError();
                        break;
                    default:
                        throw new Exception($"Can not parse robot command finished report.[{finishedMsg}]");
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
                aseRobotControl.OnRobotCommandError();
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

                    DealPrimaryReceiveQueue.Enqueue(psTransaction);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
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
                    //psWrapper.SecondarySent(ref psTransaction);
                    SecondarySendQueue.Enqueue(psTransaction);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
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
                if (psMessage.Length < 0) return;
                   
                if(psMessage.Length == 1)
                {
                    aseMoveControl.MoveFinished(EnumMoveComplete.Fail);
                    return;
                }

                ImportantPspLog?.Invoke(this, $"ReceiveMoveAppendArrivalReport {psMessage.Substring(0, 1)}");

                EnumAseArrival aseArrival = GetArrivalStatus(psMessage.Substring(0, 1));

                ImportantPspLog?.Invoke(this, $"ReceiveMoveAppendArrivalReport {aseArrival}");

                switch (aseArrival)
                {
                    case EnumAseArrival.Fail:
                        aseMoveControl.MoveFinished(EnumMoveComplete.Fail);
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
                string msg = "Move Arrival, " + ex.Message;
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                ImportantPspLog?.Invoke(this, msg);
                aseMoveControl.MoveFinished(EnumMoveComplete.Fail);
            }
        }

        private EnumAseArrival GetArrivalStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumAseArrival.Fail;
                case "1":
                    return EnumAseArrival.Arrival;
                case "2":
                    return EnumAseArrival.EndArrival;
                default:
                    throw new Exception($"Can not parse arrival report.[{v}]");
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
                aseMoveStatus.HeadDirection = GetIntTryParse(psMessage.Substring(19, 3));
                aseMoveStatus.MovingDirection = int.Parse(psMessage.Substring(22, 3));
                aseMoveStatus.Speed = int.Parse(psMessage.Substring(25, 4));

                OnPartMoveArrivalEvent?.Invoke(this, aseMoveStatus);
            }
            catch (Exception ex)
            {
                string msg = "Arrival Position, " + ex.Message;
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                ImportantPspLog?.Invoke(this, msg);
            }
        }

        private int GetIntTryParse(string v)
        {
            try
            {
                return int.Parse(v);
            }
            catch (Exception)
            {
                throw new Exception($"Can not parse int.[{v}]");
            }
        }

        private double GetPositionFromPsMessage(string v)
        {
            try
            {
                string isPositive = v.Substring(0, 1);
                double value = double.Parse(v.Substring(1, 8));
                return IsValuePositive(isPositive) ? value : -value;
            }
            catch (Exception)
            {
                throw new Exception($"Can not parse position report.[{v}]");
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
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void AlarmReport(string psMessage)
        {
            try
            {
                bool isAlarmSet = psMessage.Substring(0, 1) == "1";
                int alarmCode = int.Parse(psMessage.Substring(1, 6));

                aseBuzzerControl.OnAlarmCodeSet(alarmCode, isAlarmSet);

                OnStatusChangeReportEvent?.Invoke(this, $"AlarmReport:[{ alarmCode}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void UpdateChargeStatus(string psMessage)
        {
            try
            {
                bool isCharging = psMessage.Substring(0, 1) == "1";
                if (isCharging)
                {
                    theVehicle.CheckStartChargeReplyEnd = true;
                }
                theVehicle.IsCharging = isCharging;
                OnStatusChangeReportEvent?.Invoke(this, $"Local Update Charge Status :[{ theVehicle.IsCharging }]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void UpdateCarrierSlotStatus(string psMessage)
        {
            EnumSlotNumber slotNumber = EnumSlotNumber.L;
            AseCarrierSlotStatus aseCarrierSlotStatus = new AseCarrierSlotStatus();

            try
            {
                if (!asePackageConfig.CanManualDeleteCST)
                {
                    slotNumber = psMessage.Substring(0, 1) == "L" ? EnumSlotNumber.L : EnumSlotNumber.R;
                    aseCarrierSlotStatus.SlotNumber = slotNumber;

                    aseCarrierSlotStatus.CarrierSlotStatus = GetCarrierSlotStatus(psMessage.Substring(1, 1));
                    aseCarrierSlotStatus.CarrierId = psMessage.Substring(2);
                    if (aseCarrierSlotStatus.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
                    {
                        if (string.IsNullOrEmpty(aseCarrierSlotStatus.CarrierId.Trim()))
                        {
                            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                        }
                        else if (aseCarrierSlotStatus.CarrierId == "ReadIdFail")
                        {
                            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                        }
                        else if (aseCarrierSlotStatus.CarrierId == "PositionError")
                        {
                            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.PositionError;
                        }
                    }

                    OnUpdateSlotStatusEvent?.Invoke(this, aseCarrierSlotStatus);
                }
                else
                {
                    slotNumber = psMessage.Substring(1, 1) == "L" ? EnumSlotNumber.L : EnumSlotNumber.R;
                    aseCarrierSlotStatus.SlotNumber = slotNumber;

                    bool manualDeleteCst = psMessage.Substring(0, 1) == "1";
                    aseCarrierSlotStatus.ManualDeleteCST = manualDeleteCst;

                    aseCarrierSlotStatus.CarrierSlotStatus = GetCarrierSlotStatus(psMessage.Substring(2, 1));
                    aseCarrierSlotStatus.CarrierId = psMessage.Substring(3);
                    if (aseCarrierSlotStatus.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
                    {
                        if (string.IsNullOrEmpty(aseCarrierSlotStatus.CarrierId.Trim()))
                        {
                            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                        }
                        else if (aseCarrierSlotStatus.CarrierId == "ReadIdFail")
                        {
                            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                        }
                        else if (aseCarrierSlotStatus.CarrierId == "PositionError")
                        {
                            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.PositionError;
                        }
                    }

                    OnUpdateSlotStatusEvent?.Invoke(this, aseCarrierSlotStatus);
                }
            }
            catch (Exception ex)
            {
                string msg = "Carrier Slot Report, " + ex.Message;
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                ImportantPspLog?.Invoke(this, msg);
            }
        }

        private EnumAseCarrierSlotStatus GetCarrierSlotStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumAseCarrierSlotStatus.Empty;
                case "1":
                    return EnumAseCarrierSlotStatus.Loading;
                case "2":
                    return EnumAseCarrierSlotStatus.PositionError;
                default:
                    throw new Exception($"Can not parse position report.[{v}]");
            }
        }

        private void UpdateRobotStatus(string psMessage)
        {
            try
            {
                AseRobotStatus aseRobotStatus = new AseRobotStatus();
                aseRobotStatus.RobotState = GetRobotStatus(psMessage.Substring(0, 1));
                aseRobotStatus.IsHome = psMessage.Substring(1, 1) == "1";

                theVehicle.AseRobotStatus = aseRobotStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateRobotStatus:[{theVehicle.AseRobotStatus.RobotState}][RobotHome={theVehicle.AseRobotStatus.IsHome}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private EnumAseRobotState GetRobotStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumAseRobotState.Idle;
                case "1":
                    return EnumAseRobotState.Busy;
                case "2":
                    return EnumAseRobotState.Error;
                default:
                    return EnumAseRobotState.Error;
            }
        }

        private void UpdateMoveStatus(string psMessage)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                aseMoveStatus.AseMoveState = GetMoveStatus(psMessage.Substring(0, 1));
                aseMoveStatus.HeadDirection = GetIntTryParse(psMessage.Substring(1, 3));
                theVehicle.AseMoveStatus = aseMoveStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateMoveStatus:[{theVehicle.AseMoveStatus.AseMoveState}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);

            }
        }

        private EnumAseMoveState GetMoveStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumAseMoveState.Idle;
                case "1":
                    return EnumAseMoveState.Working;
                case "2":
                    return EnumAseMoveState.Pausing;
                case "3":
                    return EnumAseMoveState.Pause;
                case "4":
                    return EnumAseMoveState.Stoping;
                case "5":
                    return EnumAseMoveState.Block;
                case "6":
                    return EnumAseMoveState.Error;
                default:
                    return EnumAseMoveState.Error;
            }
        }

        private void SetVehicleManual()
        {
            OnModeChangeEvent?.Invoke(this, EnumAutoState.Manual);
            ImportantPspLog?.Invoke(this, $"ModeChange : Manual");
        }

        private void SetVehicleAuto()
        {
            OnModeChangeEvent?.Invoke(this, EnumAutoState.Auto);
            ImportantPspLog?.Invoke(this, $"ModeChange : Auto");
        }

        public void SetVehicleAutoScenario()
        {
            //SetLocalDateTime();
            //SpinWait.SpinUntil(() => false, 1000);

            AllAgvlStatusReportRequest();
            SpinWait.SpinUntil(() => false, 50);

            aseMoveControl.SendPositionReportRequest();
            SpinWait.SpinUntil(() => false, 50);

            aseBatteryControl.SendBatteryStatusRequest();
            SpinWait.SpinUntil(() => false, 50);

            var transferCommands = theVehicle.AgvcTransCmdBuffer.Values.ToList();
            for (int i = 0; i < transferCommands.Count; i++)
            {
                SetTransferCommandInfoRequest(transferCommands[i], EnumCommandInfoStep.Begin);
            }
        }

        public void RequestVehicleToManual()
        {
            try
            {
                PrimarySendEnqueue("P13", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        public void CstRename(AseCarrierSlotStatus slotStatus)
        {
            try
            {
                string slotNumber = slotStatus.SlotNumber.ToString().Substring(0, 1);
                string cstRenameString = string.Concat("0", slotNumber, "1", slotStatus.CarrierId);
                PrimarySendEnqueue("P25", cstRenameString);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        #endregion

        #region SecondarySend

        private void PsWrapper_OnSecondarySent(ref PSTransactionXClass transaction)
        {
            try
            {
                LogPsWrapper($"SSEND : [{transaction.PSSecondaryMessage.ToString()}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SecondarySend(PSTransactionXClass psTransaction)
        {
            try
            {
                psWrapper.SecondarySent(ref psTransaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region SecondaryReceived

        private void DealSecondaryReceived(PSTransactionXClass transaction)
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
                aseBatteryStatus.Percentage = GetIntTryParse(psMessage.Substring(0, 3));
                aseBatteryStatus.Voltage = GetIntTryParse(psMessage.Substring(3, 4)) * 0.01;
                aseBatteryStatus.Temperature = GetIntTryParse(psMessage.Substring(7, 3));
                //200522 dabid+ To Report 144 While Percentage diff
                if (theVehicle.AseBatteryStatus.Percentage != aseBatteryStatus.Percentage)
                {
                    aseBatteryControl.SetPercentage(aseBatteryStatus.Percentage);
                }

                theVehicle.AseBatteryStatus = aseBatteryStatus;

                //if (theVehicle.IsCharging)
                //{
                //    if (aseBatteryStatus.Percentage + 3 >= aseBatteryConfig.FullChargePercentage)
                //    {
                //        aseBatteryControl.FullCharge();
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void ReceivePositionReportRequestAck(string psMessage)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                aseMoveStatus.AseMoveState = GetMoveStatus(psMessage.Substring(0, 1));
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
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
                LogPsWrapper($"SRECV : [{transaction.PSSecondaryMessage.ToString()}]");

                SecondaryReceiveQueue.Enqueue(transaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        #endregion

        #region PsWrapper

        private void PsWrapper_OnTransactionError(string errorString, ref PSMessageXClass psMessage)
        {
            if (psMessage == null) return;

            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, errorString + "\r\n" + psMessage.ToString());
            ImportantPspLog?.Invoke(this, $"PsWrapper_OnTransactionError. P{psMessage.Number}. AGVL DisConnect");
            Thread.Sleep(50);
            PrimarySendEnqueue(psMessage.Type + psMessage.Number, psMessage.PSMessage);

            //if (psMessage == null)
            //{
            //    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, errorString + "\r\n PsMessage is null");
            //    ImportantPspLog?.Invoke(this, $"PsWrapper_OnTransactionError. Null.");
            //}
            //else
            //{
            //    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, errorString + "\r\n" + psMessage.ToString());
            //    ImportantPspLog?.Invoke(this, $"PsWrapper_OnTransactionError. P{psMessage.Number}. AGVL DisConnect");
            //    OnAgvlErrorEvent.Invoke(this, new EventArgs());
            //    //psWrapper.Close();
            //}
        }

        private void PsWrapper_OnConnectionStateChange(enumConnectState state)
        {
            try
            {
                string msg = $"PsWrapper connection state changed.[{state}]";
                if (state == enumConnectState.Connected || state == enumConnectState.Quit)
                {
                    LogPsWrapper(msg);
                    ImportantPspLog?.Invoke(this, msg);
                }
                OnConnectionChangeEvent?.Invoke(this, psWrapper.ConnectionState == enumConnectState.Connected);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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

        public void AppendPspLogMsg(string msg)
        {
            try
            {
                LocalLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", msg, "\r\n", LocalLogMsg);

                if (LocalLogMsg.Length > 65535)
                {
                    LocalLogMsg = LocalLogMsg.Substring(65535);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LogException(string classMethodName, string exMsg)
        {
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }

        public void LogDebug(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, "Device", "CarrierID", msg));
        }

        public void LogPsWrapper(string msg)
        {
            mirleLogger.Log(new LogFormat("PsWrapper", "5", "AsePackage", "Device", "CarrierID", msg));
            AppendPspLogMsg(msg);
        }

        #endregion

    }
}
