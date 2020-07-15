using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using Mirle.Tools;
using PSDriver.PSDriver;
using SimpleWifi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class AsePackage
    {
        public PSWrapperXClass psWrapper;
        public AseRobotControl aseRobotControl;
        public MirleLogger mirleLogger = MirleLogger.Instance;
        public AsePackageConfig asePackageConfig = new AsePackageConfig();
        public PspConnectionConfig pspConnectionConfig = new PspConnectionConfig();
        public AseBatteryConfig aseBatteryConfig = new AseBatteryConfig();
        public AseMoveConfig aseMoveConfig = new AseMoveConfig();
        public Dictionary<string, PSMessageXClass> psMessageMap = new Dictionary<string, PSMessageXClass>();
        public Vehicle Vehicle { get; set; } = Vehicle.Instance;
        public string LocalLogMsg { get; set; } = "";
        public string MoveStopResult { get; set; } = "";

        public string ABCDE { get; set; } = "";

        private Thread thdWatchWifiSignalStrength;
        public bool IsWatchWifiSignalStrengthPause { get; set; } = false;
        public uint WifiSignalStrength { get; set; } = 0;

        private Thread thdSchedule;
        public bool IsSchedulePause { get; set; } = false;

        private Thread thdWatchPosition;
        public bool IsWatchPositionPause { get; private set; } = false;

        private Thread thdWatchBatteryState;
        public bool IsWatchBatteryStatusPause { get; private set; } = false;

        public ConcurrentQueue<PSMessageXClass> PrimarySendQueue { get; set; } = new ConcurrentQueue<PSMessageXClass>();
        public ConcurrentQueue<PSTransactionXClass> SecondarySendQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<PSTransactionXClass> PrimaryReceiveQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<PSTransactionXClass> DealPrimaryReceiveQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<PSTransactionXClass> SecondaryReceiveQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        private List<PSTransactionXClass> primaryReceiveTransactions;

        public event EventHandler<bool> OnConnectionChangeEvent;
        public event EventHandler<string> ImportantPspLog;
        public event EventHandler<string> OnStatusChangeReportEvent;
        public event EventHandler<AseMoveStatus> OnPartMoveArrivalEvent;
        public event EventHandler<AseMoveStatus> OnPositionChangeEvent;
        public event EventHandler<EnumAutoState> OnModeChangeEvent;
        public event EventHandler<AseCarrierSlotStatus> OnUpdateSlotStatusEvent;
        public event EventHandler<EnumMoveComplete> OnMoveFinishedEvent;
        public event EventHandler<int> OnAlarmCodeSetEvent;
        public event EventHandler<int> OnAlarmCodeResetEvent;
        public event EventHandler OnAlarmCodeAllResetEvent;
        public event EventHandler<double> OnBatteryPercentageChangeEvent;

        public AsePackage(Dictionary<string, string> gateTypeMap)
        {
            LoadConfigs();
            InitialWrapper();
            InitialAseRobotControl(gateTypeMap);
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

            thdWatchPosition = new Thread(WatchPosition);
            thdWatchPosition.IsBackground = true;
            thdWatchPosition.Start();

            thdWatchBatteryState = new Thread(WatchBatteryState);
            thdWatchBatteryState.IsBackground = true;
            thdWatchBatteryState.Start();
        }

        private void InitialAseBuzzerControl()
        {
            try
            {
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

        private void LoadConfigs()
        {
            XmlHandler xmlHandler = new XmlHandler();
            asePackageConfig = xmlHandler.ReadXml<AsePackageConfig>("AsePackageConfig.xml");
            pspConnectionConfig = xmlHandler.ReadXml<PspConnectionConfig>(asePackageConfig.PspConnectionConfigFilePath);
            Vehicle.PspSpecVersion = pspConnectionConfig.SpecVersion;
            aseBatteryConfig = xmlHandler.ReadXml<AseBatteryConfig>(asePackageConfig.AseBatteryConfigFilePath);
            aseMoveConfig = xmlHandler.ReadXml<AseMoveConfig>(asePackageConfig.AseMoveConfigFilePath);

            if (Vehicle.MainFlowConfig.IsSimulation)
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

                if (!Vehicle.MainFlowConfig.IsSimulation)
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
                if (Model.Vehicle.Instance.IsAgvcConnect)
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

        private void WatchPosition()
        {
            while (true)
            {
                try
                {
                    if (IsWatchPositionPause)
                    {
                        SpinWait.SpinUntil(() => !IsWatchPositionPause, aseMoveConfig.WatchPositionInterval);
                        continue;
                    }

                    if (psWrapper.IsConnected())
                    {
                        SendPositionReportRequest();
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, aseMoveConfig.WatchPositionInterval);
                }
            }
        }

        public void SendPositionReportRequest()
        {
            try
            {
                PrimarySendEnqueue("P33", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }


        private void WatchBatteryState()
        {
            while (true)
            {
                try
                {
                    if (IsWatchBatteryStatusPause)
                    {
                        SpinWait.SpinUntil(() => !IsWatchPositionPause, aseBatteryConfig.WatchBatteryStateInterval);
                        continue;
                    }

                    if (psWrapper.IsConnected())
                    {
                        SendBatteryStatusRequest();
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                finally
                {
                    if (Vehicle.IsCharging)
                    {
                        SpinWait.SpinUntil(() => false, aseBatteryConfig.WatchBatteryStateIntervalInCharging);
                    }
                    else
                    {
                        SpinWait.SpinUntil(() => false, aseBatteryConfig.WatchBatteryStateInterval);
                    }
                }
            }
        }

        public void PauseWatchBatteryState()
        {
            IsWatchBatteryStatusPause = true;
        }

        public void ResumeWatchBatteryState()
        {
            IsWatchBatteryStatusPause = false;
        }

        public void SendBatteryStatusRequest()
        {
            try
            {
                PrimarySendEnqueue("P35", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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

        public void DoRobotCommand(string robotCommandInfo)
        {
            try
            {
                PrimarySendEnqueue("P45", robotCommandInfo);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SetAlarmCode(int alarmCode, bool isSet)
        {
            try
            {
                string isSetString = isSet ? "1" : "0";
                string alarmCodeString = alarmCode.ToString(new string('0', 6));
                string psMessage = string.Concat(isSetString, alarmCodeString);
                PrimarySendEnqueue("P61", psMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ResetAllAlarmCode(object sneder, EventArgs eventArgs)
        {
            try
            {
                PrimarySendEnqueue("P63", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            SetAlarmCode(alarm.Id, true);
        }

        public void BuzzerOff()
        {
            try
            {
                PrimarySendEnqueue("P65", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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

                if (psMessage.Length == 1)
                {
                    MoveFinished(EnumMoveComplete.Fail);
                    return;
                }

                ImportantPspLog?.Invoke(this, $"ReceiveMoveAppendArrivalReport {psMessage.Substring(0, 1)}");

                EnumAseArrival aseArrival = GetArrivalStatus(psMessage.Substring(0, 1));

                ImportantPspLog?.Invoke(this, $"ReceiveMoveAppendArrivalReport {aseArrival}");

                switch (aseArrival)
                {
                    case EnumAseArrival.Fail:
                        MoveFinished(EnumMoveComplete.Fail);
                        break;
                    case EnumAseArrival.Arrival:
                        ArrivalPosition(psMessage);
                        break;
                    case EnumAseArrival.EndArrival:
                        ArrivalPosition(psMessage);
                        MoveFinished(EnumMoveComplete.Success);
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
                MoveFinished(EnumMoveComplete.Fail);
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
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);

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
                OnStatusChangeReportEvent?.Invoke(this, $"AllAlarmReset:");

                OnAlarmCodeAllResetEvent?.Invoke(this, new EventArgs());
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

                if (isAlarmSet)
                {
                    OnAlarmCodeSetEvent?.Invoke(this, alarmCode);
                }
                else
                {
                    OnAlarmCodeResetEvent?.Invoke(this, alarmCode);
                }
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
                    Vehicle.CheckStartChargeReplyEnd = true;
                }
                Vehicle.IsCharging = isCharging;
                OnStatusChangeReportEvent?.Invoke(this, $"Local Update Charge Status :[{ Vehicle.IsCharging }]");
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
                    if (manualDeleteCst)
                    {
                        OnUpdateSlotStatusEvent?.Invoke(this, aseCarrierSlotStatus);
                        return;
                    }

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

                Vehicle.AseRobotStatus = aseRobotStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateRobotStatus:[{Vehicle.AseRobotStatus.RobotState}][RobotHome={Vehicle.AseRobotStatus.IsHome}]");
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
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                aseMoveStatus.AseMoveState = GetMoveStatus(psMessage.Substring(0, 1));
                aseMoveStatus.HeadDirection = GetIntTryParse(psMessage.Substring(1, 3));
                Vehicle.AseMoveStatus = aseMoveStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateMoveStatus:[{Vehicle.AseMoveStatus.AseMoveState}]");
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
            AllAgvlStatusReportRequest();
            SpinWait.SpinUntil(() => false, 50);

            SendPositionReportRequest();
            SpinWait.SpinUntil(() => false, 50);

            SendBatteryStatusRequest();
            SpinWait.SpinUntil(() => false, 50);
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

        public void OnAlarmCodeSet(int alarmCode, bool isSet)
        {
            try
            {
                if (isSet)
                {
                    OnAlarmCodeSetEvent?.Invoke(this, alarmCode);
                }
                else
                {
                    OnAlarmCodeResetEvent?.Invoke(this, alarmCode);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                AseBatteryStatus aseBatteryStatus = new AseBatteryStatus(Vehicle.AseBatteryStatus);
                aseBatteryStatus.Percentage = GetIntTryParse(psMessage.Substring(0, 3));
                aseBatteryStatus.Voltage = GetIntTryParse(psMessage.Substring(3, 4)) * 0.01;
                aseBatteryStatus.Temperature = GetIntTryParse(psMessage.Substring(7, 3));
                //200522 dabid+ To Report 144 While Percentage diff
                if (Vehicle.AseBatteryStatus.Percentage != aseBatteryStatus.Percentage)
                {
                    SetPercentage(aseBatteryStatus.Percentage);
                }

                Vehicle.AseBatteryStatus = aseBatteryStatus;

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
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
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

            string versionInfo = string.Concat("Sw", Vehicle.SoftwareVersion.PadRight(13), "Sp", Vehicle.PspSpecVersion.PadRight(5));
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

        #region Battery Control

        public void SetPercentage(int percentage)
        {
            try
            {
                if (Math.Abs(percentage - Vehicle.AseBatteryStatus.Percentage) >= 1)
                {
                    Vehicle.AseBatteryStatus.Percentage = percentage;
                    OnBatteryPercentageChangeEvent?.Invoke(this, percentage);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void StopCharge()
        {
            try
            {
                //if (!theVehicle.IsCharging) return;

                PrimarySendEnqueue("P47", "0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void StartCharge(EnumAddressDirection chargeDirection)
        {
            try
            {
                //if (theVehicle.CheckStartChargeReplyEnd) return; //200702 dabid-
                string chargeDirectionString;
                switch (chargeDirection)
                {
                    case EnumAddressDirection.Left:
                        chargeDirectionString = "1";
                        break;
                    case EnumAddressDirection.Right:
                        chargeDirectionString = "2";
                        break;
                    case EnumAddressDirection.None:
                    default:
                        throw new Exception($"Start charge command direction error.[{chargeDirection}]");
                }
                PrimarySendEnqueue("P47", chargeDirectionString);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }


        #endregion

        #region Move Control

        public void MoveFinished(EnumMoveComplete enumMoveComplete)
        {
            try
            {
                OnMoveFinishedEvent?.Invoke(this, enumMoveComplete);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PartMove(MapPosition mapPosition, int headAngle, int speed, EnumAseMoveCommandIsEnd isEnd, EnumIsExecute keepOrGo, EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            try
            {
                string isEndString = ((int)isEnd).ToString();
                string positionX = GetPositionString(mapPosition.X);
                string positionY = GetPositionString(mapPosition.Y);
                string thetaString = GetNumberToString(headAngle, 3);
                string speedString = GetNumberToString(speed, 4);
                string openSlotString = ((int)openSlot).ToString();
                string keepOrGoString = keepOrGo.ToString().Substring(0, 1).ToUpper();
                string message = string.Concat(isEndString, positionX, positionY, thetaString, speedString, openSlotString, keepOrGoString);

                PrimarySendEnqueue("P41", message);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PartMove(EnumAseMoveCommandIsEnd enumAseMoveCommandIsEnd, EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Model.Vehicle.Instance.AseMoveStatus);
                string beginString = ((int)enumAseMoveCommandIsEnd).ToString();
                string positionX = GetPositionString(aseMoveStatus.LastAddress.Position.X);
                string positionY = GetPositionString(aseMoveStatus.LastAddress.Position.Y);
                string thetaString = GetNumberToString((int)aseMoveStatus.LastAddress.VehicleHeadAngle, 3);
                string speedString = GetNumberToString((int)aseMoveStatus.LastSection.Speed, 4);
                string openSlotString = ((int)openSlot).ToString();
                string keepOrGoString = "G";
                string message = string.Concat(beginString, positionX, positionY, thetaString, speedString, openSlotString, keepOrGoString);

                PrimarySendEnqueue("P41", message);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string GetPositionString(double x)
        {
            try
            {
                string result = x >= 0 ? "P" : "N";
                string number = GetNumberToString((int)(Math.Abs(x)), 8);
                result = string.Concat(result, number);
                return result;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        private string GetNumberToString(int value, ushort digit)
        {
            try
            {
                string valueFormat = new string('0', digit);

                return value.ToString(valueFormat);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        public void MoveStop()
        {
            try
            {
                PrimarySendEnqueue("P51", "2");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void MoveContinue()
        {
            try
            {
                PrimarySendEnqueue("P51", "1");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void MovePause()
        {
            try
            {
                PrimarySendEnqueue("P51", "0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void RefreshMoveState()
        {
            try
            {
                PrimarySendEnqueue("P31", "1");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
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
