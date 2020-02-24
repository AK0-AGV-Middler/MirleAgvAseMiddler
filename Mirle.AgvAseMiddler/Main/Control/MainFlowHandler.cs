using Google.Protobuf.Collections;
using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.Configs;
using Mirle.AgvAseMiddler.Model.TransferSteps;
using Mirle.AgvAseMiddler.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Mirle.Tools;

namespace Mirle.AgvAseMiddler.Controller
{
    public class MainFlowHandler
    {
        #region Configs
        private AgvcConnectorConfig agvcConnectorConfig;
        private MainFlowConfig mainFlowConfig;
        private MapConfig mapConfig;
        private AlarmConfig alarmConfig;
        public BatteryLog batteryLog;
        #endregion

        #region TransCmds
        private List<TransferStep> transferSteps = new List<TransferStep>();

        public bool GoNextTransferStep { get; set; }
        public int TransferStepsIndex { get; private set; } = 0;
        public bool IsOverrideMove { get; set; }
        public bool IsAvoidMove { get; set; }

        public bool IsReportingPosition { get; set; }
        public bool IsReserveMechanism { get; set; } = true;
        #endregion

        #region Controller

        private AgvcConnector agvcConnector;
        private MirleLogger mirleLogger = null;
        private AlarmHandler alarmHandler;
        private MapHandler mapHandler;
        private XmlHandler xmlHandler = new XmlHandler();
        private AsePackage asePackage;

        #endregion

        #region Threads
        private Thread thdVisitTransferSteps;
        public bool IsVisitTransferStepPause { get; set; } = false;

        private Thread thdTrackPosition;
        private ManualResetEvent trackPositionShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent trackPositionPauseEvent = new ManualResetEvent(true);
        public EnumThreadStatus TrackPositionStatus { get; private set; } = EnumThreadStatus.None;

        public EnumThreadStatus PreTrackPositionStatus { get; private set; } = EnumThreadStatus.None;

        private Thread thdWatchLowPower;
        private ManualResetEvent watchLowPowerShutdownEvent = new ManualResetEvent(false);

        private ManualResetEvent watchLowPowerPauseEvent = new ManualResetEvent(true);
        public EnumThreadStatus WatchLowPowerStatus { get; private set; } = EnumThreadStatus.None;
        public EnumThreadStatus PreWatchLowPowerStatus { get; private set; } = EnumThreadStatus.None;
        #endregion

        #region Events
        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;
        public event EventHandler<string> OnMessageShowEvent;
        public event EventHandler<MoveCmdInfo> OnPrepareForAskingReserveEvent;
        public event EventHandler OnMoveArrivalEvent;
        public event EventHandler<AgvcTransCmd> OnTransferCommandCheckedEvent;
        public event EventHandler<AgvcOverrideCmd> OnOverrideCommandCheckedEvent;
        public event EventHandler<AseMovingGuide> OnAvoidCommandCheckedEvent;
        public event EventHandler<TransferStep> OnDoTransferStepEvent;
        public event EventHandler<bool> OnAgvlConnectionChangedEvent;
        #endregion

        #region Models
        public Vehicle theVehicle;
        private bool isIniOk;
        public MapInfo theMapInfo { get; private set; } = new MapInfo();
        public double InitialSoc { get; set; } = 70;
        public EnumCstIdReadResult ReadResult { get; set; } = EnumCstIdReadResult.Noraml;
        public bool NeedRename { get; set; } = false;
        public bool IsMoveEnd { get; set; } = false;
        public bool IsSimulation { get; set; }
        public string MainFlowAbnormalMsg { get; set; }
        public bool IsRetryArrival { get; set; } = false;
        #endregion

        public MainFlowHandler()
        {
            isIniOk = true;
        }

        #region InitialComponents

        public void InitialMainFlowHandler()
        {
            XmlInitial();
            LoggersInitial();
            VehicleInitial();
            ControllersInitial();            
            EventInitial();

            VehicleLocationInitialAndThreadsInitial();

            if (isIniOk)
            {
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "全部"));
            }
        }

        private void XmlInitial()
        {
            try
            {
                mainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(@"MainFlow.xml");
                mapConfig = xmlHandler.ReadXml<MapConfig>(@"Map.xml");
                agvcConnectorConfig = xmlHandler.ReadXml<AgvcConnectorConfig>(@"AgvcConnectorConfig.xml");
                alarmConfig = xmlHandler.ReadXml<AlarmConfig>(@"Alarm.xml");
                batteryLog = xmlHandler.ReadXml<BatteryLog>(@"BatteryLog.xml");
                InitialSoc = batteryLog.InitialSoc;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "讀寫設定檔"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "讀寫設定檔"));
            }
        }

        private void LoggersInitial()
        {
            try
            {
                string loggerConfigPath = "Log.ini";
                if (File.Exists(loggerConfigPath))
                {
                    mirleLogger = MirleLogger.Instance;
                }
                else
                {
                    throw new Exception();
                }

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "紀錄器"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "紀錄器缺少Log.ini"));
            }
        }

        private void ControllersInitial()
        {
            try
            {
                alarmHandler = new AlarmHandler(this);
                mapHandler = new MapHandler(mapConfig);
                theMapInfo = mapHandler.theMapInfo;
                agvcConnector = new AgvcConnector(this);
                asePackage = new AsePackage(theMapInfo.portNumberMap);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "控制層"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "控制層"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void VehicleInitial()
        {
            try
            {
                theVehicle = Vehicle.Instance;
                theVehicle.IsSimulation = mainFlowConfig.IsSimulation;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "台車"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "台車"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'mapHandler
                agvcConnector.OnInstallTransferCommandEvent += AgvcConnector_OnInstallTransferCommandEvent;
                agvcConnector.OnOverrideCommandEvent += AgvcConnector_OnOverrideCommandEvent;
                agvcConnector.OnAvoideRequestEvent += AgvcConnector_OnAvoideRequestEvent;

                //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                asePackage.aseMoveControl.OnMoveFinishEvent += AseMoveControl_OnMoveFinished;
                asePackage.aseMoveControl.OnRetryMoveFinishEvent += AseMoveControl_OnRetryMoveFinished;

                //來自IRobotControl的取放貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                asePackage.aseRobotControl.OnRobotInterlockErrorEvent += AseRobotControl_OnRobotInterlockErrorEvent;
                asePackage.aseRobotControl.OnRobotCommandFinishEvent += AseRobotContorl_OnRobotCommandFinishEvent;
                asePackage.aseRobotControl.OnRobotCommandErrorEvent += AseRobotControl_OnRobotCommandErrorEvent;

                //來自IRobot的CarrierId讀取訊息，通知middleAgent
                asePackage.aseRobotControl.OnReadCarrierIdFinishEvent += AseRobotControl_OnReadCarrierIdFinishEvent;

                //來自IBatterysControl的電量改變訊息，通知middleAgent
                asePackage.aseBatteryControl.OnBatteryPercentageChangeEvent += agvcConnector.AseBatteryControl_OnBatteryPercentageChangeEvent;
                asePackage.aseBatteryControl.OnBatteryPercentageChangeEvent += AseBatteryControl_OnBatteryPercentageChangeEvent;

                //來自AlarmHandler的SetAlarm/ResetOneAlarm/ResetAllAlarm發生警告，通知MainFlow,middleAgent
                alarmHandler.OnSetAlarmEvent += AlarmHandler_OnSetAlarmEvent;
                alarmHandler.OnSetAlarmEvent += agvcConnector.AlarmHandler_OnSetAlarmEvent;

                alarmHandler.OnPlcResetOneAlarmEvent += agvcConnector.AlarmHandler_OnPlcResetOneAlarmEvent;

                alarmHandler.OnResetAllAlarmsEvent += AlarmHandler_OnResetAllAlarmsEvent;
                alarmHandler.OnResetAllAlarmsEvent += agvcConnector.AlarmHandler_OnResetAllAlarmsEvent;

                theVehicle.OnAutoStateChangeEvent += TheVehicle_OnAutoStateChangeEvent;

                asePackage.OnMessageShowEvent += AsePackage_OnMessageShowEvent;
                asePackage.OnConnectionChangeEvent += AsePackage_OnConnectionChangeEvent;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "事件"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "事件"));

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AsePackage_OnConnectionChangeEvent(object sender, bool e)
        {
            OnAgvlConnectionChangedEvent?.Invoke(this, e);
        }

        private void TheVehicle_OnAutoStateChangeEvent(object sender, EnumAutoState autoState)
        {
            switch (autoState)
            {
                case EnumAutoState.Auto:
                    MainFlowAbnormalMsg = "";
                    alarmHandler.ResetAllAlarms();
                    ReadCarrierId();
                    break;
                case EnumAutoState.Manual:
                    StopClearAndReset();
                    break;
                case EnumAutoState.PreManual:
                    break;
                default:
                    break;
            }
            agvcConnector.StatusChangeReport();
        }

        private void VehicleLocationInitialAndThreadsInitial()
        {
            if (IsRealPositionEmpty())
            {
                try
                {
                    theVehicle.AseMoveStatus.LastMapPosition = theMapInfo.allMapAddresses.First(x => x.Key != "").Value.Position;
                }
                catch (Exception ex)
                {
                    theVehicle.AseMoveStatus.LastMapPosition = new MapPosition();
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
            }
            StartTrackPosition();
            StartWatchLowPower();
            var msg = $"讀取到的電量為{batteryLog.InitialSoc}";
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
        }
        private bool IsRealPositionEmpty()
        {
            if (theVehicle.AseMoveStatus.LastMapPosition == null)
            {
                return true;
            }

            if (theVehicle.AseMoveStatus.LastMapPosition.X == 0 && theVehicle.AseMoveStatus.LastMapPosition.Y == 0)
            {
                return true;
            }

            return false;
        }

        public void ReloadConfig()
        {
            XmlInitial();
        }

        #endregion

        #region Thd Visit TransferSteps
        private void VisitTransferSteps()
        {
            while (true)
            {
                if (IsVisitTransferStepPause) continue;

                try
                {
                    if (GoNextTransferStep)
                    {
                        GoNextTransferStep = false;
                        //DoTransfer();
                        if (transferSteps.Count == 0)
                        {
                            transferSteps.Add(new EmptyTransferStep());
                            continue;
                        }
                        if (TransferStepsIndex < 0)
                        {
                            TransferStepsIndex = 0;
                            GoNextTransferStep = true;
                            continue;
                        }
                        if (TransferStepsIndex < transferSteps.Count)
                        {
                            DoTransferStep(transferSteps[TransferStepsIndex]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.VisitTransferStepsSleepTimeMs);
                }
            }
        }

        private void DoTransferStep(TransferStep transferStep)
        {
            switch (transferStep.GetTransferStepType())
            {
                case EnumTransferStepType.Move:
                case EnumTransferStepType.MoveToCharger:
                    agvcConnector.AskGuideAddressesAndSections(transferStep);
                    break;
                case EnumTransferStepType.Load:
                    Load((LoadCmdInfo)transferStep);
                    break;
                case EnumTransferStepType.Unload:
                    Unload((UnloadCmdInfo)transferStep);
                    break;
                case EnumTransferStepType.Empty:
                    CheckTransferSteps();
                    break;
                default:
                    break;
            }
        }

        private void CheckTransferSteps()
        {
            foreach (var transferStep in transferSteps)
            {
                if (transferStep.GetTransferStepType() != EnumTransferStepType.Empty)
                {
                    if (TransferStepsIndex == transferSteps.Count - 1)
                    {
                        transferSteps = new List<TransferStep>();
                        transferSteps.Add(new EmptyTransferStep());
                        return;
                    }
                    else
                    {
                        VisitNextTransferStep();
                        return;
                    }
                }
            }

            if (transferSteps.Count > 1)
            {
                transferSteps = new List<TransferStep>();
                transferSteps.Add(new EmptyTransferStep());
                TransferStepsIndex = 0;
            }

        }

        public void StartVisitTransferSteps()
        {
            TransferStepsIndex = 0;
            thdVisitTransferSteps = new Thread(VisitTransferSteps);
            thdVisitTransferSteps.IsBackground = true;
            thdVisitTransferSteps.Start();

            var msg = $"MainFlow : 開始搬送流程";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        public void PauseVisitTransferSteps()
        {
            IsVisitTransferStepPause = true;
            if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.Off)
            {
                theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.On;
                agvcConnector.StatusChangeReport();
            }
            var msg = $"MainFlow : 暫停搬送流程";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        public void ResumeVisitTransferSteps()
        {
            IsVisitTransferStepPause = false;
            if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.On)
            {
                theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                agvcConnector.StatusChangeReport();
            }
            var msg = $"MainFlow : 恢復搬送流程";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void ClearTransferSteps()
        {
            IsVisitTransferStepPause = true;
            GoNextTransferStep = false;
            foreach (var cmdId in theVehicle.AgvcTransCmdBuffer.Keys)
            {
                foreach (var transferStep in transferSteps)
                {
                    if (transferStep.CmdId == cmdId)
                    {
                        transferSteps.Remove(transferStep);
                        TransferStepsIndex--;
                    }
                }
            }
            transferSteps = new List<TransferStep>();
            transferSteps.Add(new EmptyTransferStep());
            GoNextTransferStep = true;
            IsVisitTransferStepPause = false;

            var msg = $"MainFlow : 清除搬送流程";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void ClearTransferSteps(string cmdId)
        {
            IsVisitTransferStepPause = true;
            GoNextTransferStep = false;
            foreach (var transferStep in transferSteps)
            {
                if (transferStep.CmdId == cmdId)
                {
                    transferSteps.Remove(transferStep);
                    TransferStepsIndex--;
                }
            }
            if (transferSteps.Count == 0)
            {
                transferSteps.Add(new EmptyTransferStep());
            }
            GoNextTransferStep = true;
            IsVisitTransferStepPause = false;

            var msg = $"MainFlow : 清除已完成命令";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        //private void AfterVisitTransferSteps()
        //{
        //    if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.On)
        //    {
        //        theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
        //        agvcConnector.StatusChangeReport();
        //    }

        //    agvcConnector.TransferComplete(agvcTransCmd);

        //    VisitTransferStepsStatus = EnumThreadStatus.None;
        //    lastAgvcTransCmd = agvcTransCmd;
        //    agvcTransCmd = new AgvcTransCmd();
        //    transferSteps = new List<TransferStep>();
        //    GoNextTransferStep = false;
        //    SetTransCmdsStep(new Idle());
        //    agvcConnector.NoCommand();
        //    IsMoveEnd = true;
        //    var msg = $"MainFlow : 結束搬送流程";
        //    OnMessageShowEvent?.Invoke(this, msg);
        //}
        #endregion

        #region Thd Watch LowPower
        private void WatchLowPower()
        {
            Stopwatch sw = new Stopwatch();
            long total = 0;
            while (true)
            {
                try
                {
                    sw.Restart();

                    #region Pause And Stop Check
                    watchLowPowerPauseEvent.WaitOne(Timeout.Infinite);
                    if (watchLowPowerShutdownEvent.WaitOne(0)) break;
                    #endregion

                    WatchLowPowerStatus = EnumThreadStatus.Working;

                    if (theVehicle.AutoState == EnumAutoState.Auto && transferSteps.Count == 0)
                    {
                        if (IsLowPower())
                        {
                            LowPowerStartCharge(theVehicle.AseMoveStatus.LastAddress);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.WatchLowPowerSleepTimeMs);
                    sw.Stop();
                    total += sw.ElapsedMilliseconds;
                }
            }
            sw.Stop();
            AfterWatchLowPower(total);
        }
        public void StartWatchLowPower()
        {
            watchLowPowerPauseEvent.Set();
            watchLowPowerShutdownEvent.Reset();
            thdWatchLowPower = new Thread(WatchLowPower);
            thdWatchLowPower.IsBackground = true;
            thdWatchLowPower.Start();
            WatchLowPowerStatus = EnumThreadStatus.Start;
            var msg = $"MainFlow : 開始監看自動充電, [Power={theVehicle.AseBatteryStatus.Percentage}][LowSocGap={theVehicle.AutoChargeLowThreshold}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        public void PauseWatchLowPower()
        {
            watchLowPowerPauseEvent.Reset();
            PreWatchLowPowerStatus = WatchLowPowerStatus;
            WatchLowPowerStatus = EnumThreadStatus.Pause;
            var msg = $"MainFlow : 暫停監看自動充電, [Power={theVehicle.AseBatteryStatus.Percentage}][LowSocGap={theVehicle.AutoChargeLowThreshold}]";
            OnMessageShowEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //    , msg));

        }
        public void ResumeWatchLowPower()
        {
            watchLowPowerPauseEvent.Set();
            var tempStatus = WatchLowPowerStatus;
            WatchLowPowerStatus = PreWatchLowPowerStatus;
            PreWatchLowPowerStatus = tempStatus;
            var msg = $"MainFlow : 恢復監看自動充電, [Power={theVehicle.AseBatteryStatus.Percentage}][LowSocGap={theVehicle.AutoChargeLowThreshold}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        public void StopWatchLowPower()
        {
            if (WatchLowPowerStatus != EnumThreadStatus.None)
            {
                WatchLowPowerStatus = EnumThreadStatus.Stop;
            }
            var msg = $"MainFlow : 停止監看自動充電, [Power={theVehicle.AseBatteryStatus.Percentage}][LowSocGap={theVehicle.AutoChargeLowThreshold}]";
            OnMessageShowEvent?.Invoke(this, msg);
            watchLowPowerShutdownEvent.Set();
            watchLowPowerPauseEvent.Set();
        }
        public void AfterWatchLowPower(long total)
        {
            WatchLowPowerStatus = EnumThreadStatus.None;
            var msg = $"MainFlow : 監看自動充電 後處理, [ThreadStatus={WatchLowPowerStatus}][TotalSpendMs={total}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        private bool IsLowPower()
        {
            return theVehicle.AseBatteryStatus.Percentage <= theVehicle.AutoChargeLowThreshold;
        }
        private bool IsHighPower()
        {
            return theVehicle.AseBatteryStatus.Percentage >= theVehicle.AutoChargeHighThreshold;
        }
        private bool IsWatchLowPowerStop()
        {
            return WatchLowPowerStatus == EnumThreadStatus.Stop || WatchLowPowerStatus == EnumThreadStatus.StopComplete || WatchLowPowerStatus == EnumThreadStatus.None;
        }
        #endregion

        #region Thd Track Position
        private void TrackPosition()
        {
            Stopwatch sw = new Stopwatch();
            long total = 0;
            while (true)
            {
                try
                {
                    sw.Start();

                    #region Pause And Stop Check
                    trackPositionPauseEvent.WaitOne(Timeout.Infinite);
                    if (trackPositionShutdownEvent.WaitOne(0)) break;
                    #endregion

                    TrackPositionStatus = EnumThreadStatus.Working;

                    AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                    var position = theVehicle.AseMoveStatus.LastMapPosition;
                    if (theVehicle.AseMoveStatus.LastMapPosition == null) continue;
                    //if (IsVehlocStayInSameAddress(vehicleLocation)) continue;

                    if (theVehicle.AutoState == EnumAutoState.Auto)
                    {
                        if (transferSteps.Count > 0)
                        {
                            //有搬送命令時，比對當前Position與搬送路徑Sections計算LastSection/LastAddress/Distance                           
                            if (IsMoveStep())
                            {
                                if (!IsMoveEnd)
                                {
                                    if (UpdateVehiclePositionInMovingStep())
                                    {
                                        sw.Reset();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //無搬送命令時，比對當前Position與全地圖Sections確定section-distance
                        UpdateVehiclePositionManual();
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.TrackPositionSleepTimeMs);
                }

                sw.Stop();
                if (sw.ElapsedMilliseconds > mainFlowConfig.ReportPositionIntervalMs)
                {
                    agvcConnector.ReportAddressPass();
                    total += sw.ElapsedMilliseconds;
                    sw.Reset();
                }
            }

            AfterTrackPosition(total);
        }
        public void StartTrackPosition()
        {
            trackPositionPauseEvent.Set();
            trackPositionShutdownEvent.Reset();
            thdTrackPosition = new Thread(TrackPosition);
            thdTrackPosition.IsBackground = true;
            thdTrackPosition.Start();
            TrackPositionStatus = EnumThreadStatus.Start;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : 開始追蹤座標, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }
        public void PauseTrackPosition()
        {
            trackPositionPauseEvent.Reset();
            PreTrackPositionStatus = TrackPositionStatus;
            TrackPositionStatus = EnumThreadStatus.Pause;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : 暫停追蹤座標, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }
        public void ResumeTrackPosition()
        {
            trackPositionPauseEvent.Set();
            var tempStatus = TrackPositionStatus;
            TrackPositionStatus = PreTrackPositionStatus;
            PreTrackPositionStatus = tempStatus;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : 恢復追蹤座標, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }
        public void StopTrackPosition()
        {
            trackPositionShutdownEvent.Set();
            trackPositionPauseEvent.Set();
            if (TrackPositionStatus != EnumThreadStatus.None)
            {
                TrackPositionStatus = EnumThreadStatus.Stop;
            }

            OnMessageShowEvent?.Invoke(this, $"MainFlow : 停止追蹤座標, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }
        private void AfterTrackPosition(long total)
        {
            TrackPositionStatus = EnumThreadStatus.None;
            var msg = $"MainFlow : 追蹤座標 後處理, [ThreadStatus={TrackPositionStatus}][TotalSpendMs={total}]";
            OnMessageShowEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //    , msg));
        }
        #endregion

        #region Handle Transfer Command
        private void AgvcConnector_OnInstallTransferCommandEvent(object sender, AgvcTransCmd agvcTransCmd)
        {
            var msg = $"MainFlow : 收到{agvcTransCmd.AgvcTransCommandType}命令{agvcTransCmd.CommandId}。";
            OnMessageShowEvent?.Invoke(this, msg);

            #region 檢查搬送命令
            try
            {
                //檢查車上命令數
                if (theVehicle.AgvcTransCmdBuffer.Count >= 2)
                {
                    string cmdIds = "";
                    foreach (var cmdId in theVehicle.AgvcTransCmdBuffer.Keys)
                    {
                        cmdIds += $"[{cmdId}]";
                    }
                    var reason = $"Agv already have two command.{cmdIds}";
                    throw new Exception(reason);
                }
                else if (theVehicle.AgvcTransCmdBuffer.Count == 1)
                {
                    if (theVehicle.AgvcTransCmdBuffer.Values.First().SlotNumber == EnumSlotNumber.A)
                    {
                        agvcTransCmd.SlotNumber = EnumSlotNumber.B;
                    }
                    else
                    {
                        agvcTransCmd.SlotNumber = EnumSlotNumber.A;
                    }
                }
                else
                {
                    agvcTransCmd.SlotNumber = EnumSlotNumber.A;
                }

                switch (agvcTransCmd.AgvcTransCommandType)
                {
                    case EnumAgvcTransCommandType.Move:
                    case EnumAgvcTransCommandType.MoveToCharger:
                        CheckMoveEndAddress(agvcTransCmd.UnloadAddressId);
                        break;
                    case EnumAgvcTransCommandType.Load:
                        CheckLoadPortAddress(agvcTransCmd.LoadAddressId);
                        break;
                    case EnumAgvcTransCommandType.Unload:
                        CheckUnloadPortAddress(agvcTransCmd.UnloadAddressId);
                        break;
                    case EnumAgvcTransCommandType.LoadUnload:
                        CheckLoadPortAddress(agvcTransCmd.LoadAddressId);
                        CheckUnloadPortAddress(agvcTransCmd.UnloadAddressId);
                        break;
                    case EnumAgvcTransCommandType.Override:
                        CheckOverrideAddress(agvcTransCmd);
                        break;
                    case EnumAgvcTransCommandType.Else:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 1, ex.Message);
                string reason = $"MainFlow : 拒絕 {agvcTransCmd.AgvcTransCommandType} 命令. {ex.Message}";
                OnMessageShowEvent?.Invoke(this, reason);
                return;
            }
            #endregion

            #region 搬送流程更新
            try
            {
                PauseVisitTransferSteps();
                agvcTransCmd.RobotNgRetryTimes = mainFlowConfig.RobotNgRetryTimes;
                SetupTransferSteps(agvcTransCmd);
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 0, "");
                VisitNextTransferStep();
                ResumeVisitTransferSteps();
                asePackage.InstallTransferCommand(agvcTransCmd);
                var okMsg = $"MainFlow : 接受 {agvcTransCmd.AgvcTransCommandType}命令{agvcTransCmd.CommandId} 確認。";
                OnMessageShowEvent?.Invoke(this, okMsg);
                OnTransferCommandCheckedEvent?.Invoke(this, agvcTransCmd);
            }
            catch (Exception ex)
            {
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 1, "");
                var ngMsg = $"MainFlow : 收到 {agvcTransCmd.AgvcTransCommandType}命令{agvcTransCmd.CommandId} 處理失敗。";
                OnMessageShowEvent?.Invoke(this, ngMsg);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
            #endregion
        }

        private void CheckOverrideAddress(AgvcTransCmd agvcTransCmd)
        {
            throw new NotImplementedException();
        }

        private void CheckUnloadPortAddress(string unloadAddressId)
        {
            CheckMoveEndAddress(unloadAddressId);
            MapAddress unloadAddress = theMapInfo.allMapAddresses[unloadAddressId];
            if (!(unloadAddress.CanLeftUnload || unloadAddress.CanRightUnload))
            {
                throw new Exception($"{unloadAddressId} can not unload.");
            }
        }

        private void CheckLoadPortAddress(string loadAddressId)
        {
            CheckMoveEndAddress(loadAddressId);
            MapAddress loadAddress = theMapInfo.allMapAddresses[loadAddressId];
            if (!(loadAddress.CanLeftLoad || loadAddress.CanRightLoad))
            {
                throw new Exception($"{loadAddressId} can not load.");
            }
        }

        private void CheckMoveEndAddress(string unloadAddressId)
        {
            if (!theMapInfo.allMapAddresses.ContainsKey(unloadAddressId))
            {
                throw new Exception($"{unloadAddressId} is not in the map.");
            }
        }

        private void AgvcConnector_OnOverrideCommandEvent(object sender, AgvcOverrideCmd agvcOverrideCmd)
        {
            var msg = $"MainFlow : 收到[替代路徑]命令[{agvcOverrideCmd.CommandId}]，開始檢查。";
            OnMessageShowEvent?.Invoke(this, msg);

            //#region 替代路徑檢查
            //try
            //{
            //    agvcConnector.PauseAskReserve();

            //    if (IsAgvcTransferCommandEmpty())
            //    {
            //        var reason = "車輛沒有搬送命令可以執行替代路徑";
            //        RejectOverrideCommandAndResume(000019, reason, agvcOverrideCmd);
            //        return;
            //    }

            //    if (!IsMoveStep())
            //    {
            //        var reason = "車輛不在移動流程，無法執行替代路徑";
            //        RejectOverrideCommandAndResume(000020, reason, agvcOverrideCmd);
            //        return;
            //    }

            //    if (!IsMoveStopByNoReserve() && !agvcTransCmd.IsAvoidComplete)
            //    {
            //        var reason = $"車輛尚未停妥，拒絕執行替代路徑";
            //        RejectOverrideCommandAndResume(000021, reason, agvcOverrideCmd);
            //        return;
            //    }


            //    if (IsNextTransferStepUnload())
            //    {
            //        if (!this.agvcTransCmd.UnloadAddressId.Equals(agvcOverrideCmd.UnloadAddressId))
            //        {
            //            var reason = $"替代路徑放貨站點[{agvcOverrideCmd.UnloadAddressId}]與原路徑放貨站點[{agvcTransCmd.UnloadAddressId}]不合。";
            //            RejectOverrideCommandAndResume(000022, reason, agvcOverrideCmd);
            //            return;
            //        }

            //        if (agvcOverrideCmd.ToUnloadSectionIds.Count == 0)
            //        {
            //            var reason = "替代路徑清單放貨段為空。";
            //            RejectOverrideCommandAndResume(000024, reason, agvcOverrideCmd);
            //            return;
            //        }

            //        if (!IsOverrideCommandMatchTheMapToUnload(agvcOverrideCmd))
            //        {
            //            var reason = "替代路徑放貨段站點與路段不合圖資";
            //            RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
            //            return;
            //        }
            //    }
            //    else if (IsNextTransferStepLoad())
            //    {
            //        if (IsCurCmdTypeLoadUnload())
            //        {
            //            if (!this.agvcTransCmd.LoadAddressId.Equals(agvcOverrideCmd.LoadAddressId))
            //            {
            //                var reason = $"替代路徑取貨站點[{agvcOverrideCmd.LoadAddressId}]與原路徑取貨站點[{agvcTransCmd.LoadAddressId}]不合。";
            //                RejectOverrideCommandAndResume(000023, reason, agvcOverrideCmd);
            //                return;
            //            }

            //            if (!this.agvcTransCmd.UnloadAddressId.Equals(agvcOverrideCmd.UnloadAddressId))
            //            {
            //                var reason = $"替代路徑放貨站點[{agvcOverrideCmd.UnloadAddressId}]與原路徑放貨站點[{agvcTransCmd.UnloadAddressId}]不合。";
            //                RejectOverrideCommandAndResume(000022, reason, agvcOverrideCmd);
            //                return;
            //            }

            //            if (agvcOverrideCmd.ToLoadSectionIds.Count == 0)
            //            {
            //                var reason = "替代路徑清單取貨段為空。";
            //                RejectOverrideCommandAndResume(000025, reason, agvcOverrideCmd);
            //                return;
            //            }

            //            if (agvcOverrideCmd.ToUnloadSectionIds.Count == 0)
            //            {
            //                var reason = "替代路徑清單放貨段為空。";
            //                RejectOverrideCommandAndResume(000024, reason, agvcOverrideCmd);
            //                return;
            //            }

            //            if (!IsOverrideCommandMatchTheMapToLoad(agvcOverrideCmd))
            //            {
            //                var reason = "替代路徑取貨段站點與路段不合圖資";
            //                RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
            //                return;
            //            }

            //            if (!IsOverrideCommandMatchTheMapToNextUnload(agvcOverrideCmd))
            //            {
            //                var reason = "替代路徑放貨段站點與路段不合圖資";
            //                RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
            //                return;
            //            }
            //        }
            //        else
            //        {
            //            if (!this.agvcTransCmd.LoadAddressId.Equals(agvcOverrideCmd.LoadAddressId))
            //            {
            //                var reason = $"替代路徑取貨站點[{agvcOverrideCmd.LoadAddressId}]與原路徑取貨站點[{agvcTransCmd.LoadAddressId}]不合。";
            //                RejectOverrideCommandAndResume(000023, reason, agvcOverrideCmd);
            //                return;
            //            }

            //            if (agvcOverrideCmd.ToLoadSectionIds.Count == 0)
            //            {
            //                var reason = "替代路徑清單取貨段為空。";
            //                RejectOverrideCommandAndResume(000025, reason, agvcOverrideCmd);
            //                return;
            //            }

            //            if (!IsOverrideCommandMatchTheMapToLoad(agvcOverrideCmd))
            //            {
            //                var reason = "替代路徑取貨段站點與路段不合圖資";
            //                RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
            //                return;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //Move or MoveToCharger
            //        if (!agvcTransCmd.UnloadAddressId.Equals(agvcOverrideCmd.UnloadAddressId))
            //        {
            //            var reason = $"替代路徑移動終點[{agvcOverrideCmd.UnloadAddressId}]與原路徑移動終點[{agvcTransCmd.UnloadAddressId}]不合。";
            //            RejectOverrideCommandAndResume(000022, reason, agvcOverrideCmd);
            //            return;
            //        }

            //        if (agvcOverrideCmd.ToUnloadSectionIds.Count == 0)
            //        {
            //            var reason = "替代路徑清單為空。";
            //            RejectOverrideCommandAndResume(000024, reason, agvcOverrideCmd);
            //            return;
            //        }

            //        if (!IsOverrideCommandMatchTheMapToUnload(agvcOverrideCmd))
            //        {
            //            var reason = "替代路徑中站點與路段不合圖資";
            //            RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
            //            return;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);

            //    var reason = "替代路徑Exception";
            //    RejectOverrideCommandAndResume(000026, reason, agvcOverrideCmd);
            //    return;
            //}

            //#endregion

            //#region 替代路徑生成
            //try
            //{
            //    //middleAgent.StopAskReserve();
            //    agvcConnector.ClearAllReserve();
            //    agvcTransCmd.ExchangeSectionsAndAddress(agvcOverrideCmd);
            //    agvcTransCmd.AvoidEndAddressId = "";
            //    agvcTransCmd.IsAvoidComplete = false;
            //    SetupOverrideTransferSteps(agvcOverrideCmd);
            //    transferSteps.Add(new EmptyTransferStep());
            //    //theVehicle.TheVehicleIntegrateStatus.CarrierSlot.FakeCarrierId = agvcTransCmd.CassetteId;
            //    agvcConnector.ReplyTransferCommand(agvcOverrideCmd.CommandId, agvcOverrideCmd.GetCommandActionType(), agvcOverrideCmd.SeqNum, 0, "");
            //    var okmsg = $"MainFlow : 接受{agvcOverrideCmd.AgvcTransCommandType}命令{agvcOverrideCmd.CommandId}確認。";
            //    OnMessageShowEvent?.Invoke(this, okmsg);
            //    OnOverrideCommandCheckedEvent?.Invoke(this, agvcOverrideCmd);
            //    IsOverrideMove = true;
            //    IsAvoidMove = false;
            //    GoNextTransferStep = true;
            //    ResumeVisitTransferSteps();
            //}
            //catch (Exception ex)
            //{
            //    StopAndClear();
            //    var reason = "替代路徑Exception";
            //    RejectOverrideCommandAndResume(000026, reason, agvcOverrideCmd);
            //    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            //}

            //#endregion
        }

        private void RejectTransferCommandAndResume(int alarmCode, string reason, AgvcTransCmd agvcTransferCmd)
        {
            try
            {
                alarmHandler.SetAlarm(alarmCode);
                agvcConnector.ReplyTransferCommand(agvcTransferCmd.CommandId, agvcTransferCmd.GetCommandActionType(), agvcTransferCmd.SeqNum, 1, reason);
                reason = $"MainFlow : 拒絕 {agvcTransferCmd.AgvcTransCommandType} 命令, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                if (IsVisitTransferStepPause)
                {
                    ResumeVisitTransferSteps();
                    agvcConnector.ResumeAskReserve();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void RejectOverrideCommandAndResume(int alarmCode, string reason, AgvcOverrideCmd agvcOverrideCmd)
        {
            try
            {
                alarmHandler.SetAlarm(alarmCode);
                agvcConnector.ReplyTransferCommand(agvcOverrideCmd.CommandId, agvcOverrideCmd.GetCommandActionType(), agvcOverrideCmd.SeqNum, 1, reason);
                reason = $"MainFlow : 拒絕 {agvcOverrideCmd.AgvcTransCommandType} 命令, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AgvcConnector_OnAvoideRequestEvent(object sender, AseMovingGuide aseMovingGuide)
        {
            var msg = $"MainFlow : 收到避車命令，終點[{aseMovingGuide.ToAddressId}]，開始檢查。";
            OnMessageShowEvent?.Invoke(this, msg);

            #region 避車檢查
            try
            {
                agvcConnector.PauseAskReserve();

                if (IsAgvcTransferCommandEmpty())
                {
                    throw new Exception("車輛不在搬送命令中，無法避車");
                }

                if (!IsMoveStep())
                {
                    throw new Exception("車輛不在移動流程，無法避車");
                }

                if (!IsMoveStopByNoReserve() && !theVehicle.AseMovingGuide.IsAvoidComplete)
                {
                    throw new Exception($"車輛尚未停妥，無法避車");
                }

                //if (!IsAvoidCommandMatchTheMap(agvcMoveCmd))
                //{
                //    var reason = "避車路徑中站點與路段不合圖資";
                //    RejectAvoidCommandAndResume(000018, reason, agvcMoveCmd);
                //    return;
                //}
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                RejectAvoidCommandAndResume(000036, ex.Message, aseMovingGuide);
            }
            #endregion

            #region 避車命令生成
            try
            {
                agvcConnector.PauseAskReserve();
                agvcConnector.ClearAllReserve();
                theVehicle.AseMovingGuide = aseMovingGuide;
                SetupAseMovingGuideMovingSections();
                agvcConnector.SetupNeedReserveSections();
                agvcConnector.ReplyAvoidCommand(aseMovingGuide, 0, "");
                var okmsg = $"MainFlow : 接受避車命令確認，終點[{aseMovingGuide.ToAddressId}]。";
                OnMessageShowEvent?.Invoke(this, okmsg);
                OnAvoidCommandCheckedEvent?.Invoke(this, aseMovingGuide);
                IsAvoidMove = true;
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                StopClearAndReset();
                var reason = "避車Exception";
                RejectAvoidCommandAndResume(000036, reason, aseMovingGuide);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

            #endregion
        }

        private bool IsMoveStopByNoReserve()
        {
            return theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.On;
        }

        private void RejectAvoidCommandAndResume(int alarmCode, string reason, AseMovingGuide aseMovingGuide)
        {
            try
            {
                alarmHandler.SetAlarm(alarmCode);
                agvcConnector.ReplyAvoidCommand(aseMovingGuide, 1, reason);
                reason = $"MainFlow : 拒絕避車命令, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }        

        #region Convert AgvcTransferCommand to TransferSteps

        private void SetupTransferSteps(AgvcTransCmd agvcTransCmd)
        {
            switch (agvcTransCmd.AgvcTransCommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandId);
                    break;
                case EnumAgvcTransCommandType.Load:
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.LoadAddressId, agvcTransCmd.CommandId);
                    TransferStepsAddLoadCmdInfo(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.Unload:
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandId);
                    TransferStepsAddUnloadCmdInfo(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.LoadUnload:
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.LoadAddressId, agvcTransCmd.CommandId);
                    TransferStepsAddLoadCmdInfo(agvcTransCmd);
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandId);
                    TransferStepsAddUnloadCmdInfo(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.MoveToCharger:
                    TransferStepsAddMoveToChargerCmdInfo(agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandId);
                    break;
                case EnumAgvcTransCommandType.Override:
                case EnumAgvcTransCommandType.Else:
                default:
                    break;
            }

            transferSteps.Add(new EmptyTransferStep(agvcTransCmd.CommandId));
        }
        private void TransferStepsAddUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            UnloadCmdInfo unloadCmdInfo = new UnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmdInfo);
        }
        private void TransferStepsAddLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            LoadCmdInfo loadCmdInfo = new LoadCmdInfo(agvcTransCmd);
            transferSteps.Add(loadCmdInfo);
        }
        private void TransferStepsAddMoveCmdInfo(string endAddressId, string cmdId)
        {
            MapAddress endAddress = theMapInfo.allMapAddresses[endAddressId];
            MoveCmdInfo moveCmd = new MoveCmdInfo(endAddress, cmdId);
            transferSteps.Add(moveCmd);
        }
        private void TransferStepsAddMoveToChargerCmdInfo(string endAddressId, string cmdId)
        {
            MapAddress endAddress = theMapInfo.allMapAddresses[endAddressId];
            MoveToChargerCmdInfo moveCmd = new MoveToChargerCmdInfo(endAddress, cmdId);
            transferSteps.Add(moveCmd);
        }
        private MoveToChargerCmdInfo GetMoveToChargerCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MapAddress endAddress = theMapInfo.allMapAddresses[agvcTransCmd.UnloadAddressId];
            return new MoveToChargerCmdInfo(endAddress, agvcTransCmd.CommandId);
        }

        private EnumStageDirection EnumStageDirectionParse(EnumPioDirection pioDirection)
        {
            switch (pioDirection)
            {
                case EnumPioDirection.Left:
                    return EnumStageDirection.Left;
                case EnumPioDirection.Right:
                    return EnumStageDirection.Right;
                case EnumPioDirection.None:
                    return EnumStageDirection.None;
                default:
                    return EnumStageDirection.None;
            }
        }

        #endregion

        private void AgvcConnector_OnGetBlockPassEvent(object sender, bool e)
        {
            //throw new NotImplementedException();
        }
        private bool IsUnloadArrival(UnloadCmdInfo unloadCmd)
        {
            // 判斷當前是否可載貨 若否 則發送報告
            var curAddress = theVehicle.AseMoveStatus.LastAddress;
            var unloadAddressId = unloadCmd.PortAddressId;
            if (curAddress.Id == unloadAddressId)
            {
                if (IsRetryArrival)
                {
                    IsRetryArrival = false;
                }
                else
                {
                    agvcConnector.UnloadArrivals(unloadCmd.CmdId);
                }
                var msg = $"MainFlow : 到達放貨站,[Port={unloadAddressId}]";
                OnMessageShowEvent?.Invoke(this, msg);
                return true;
            }
            else
            {
                IsRetryArrival = false;
                alarmHandler.SetAlarm(000009);
                return false;
            }
        }
        private bool IsLoadArrival(LoadCmdInfo loadCmdInfo)
        {
            // 判斷當前是否可卸貨 若否 則發送報告
            var curAddress = theVehicle.AseMoveStatus.LastAddress;
            var loadAddressId = loadCmdInfo.PortAddressId;

            if (curAddress.Id == loadAddressId)
            {
                if (IsRetryArrival)
                {
                    IsRetryArrival = false;
                }
                else
                {
                    agvcConnector.LoadArrivals(loadCmdInfo.CmdId);
                }

                var msg = $"MainFlow : 到達取貨站, [Port={loadAddressId}]";
                OnMessageShowEvent?.Invoke(this, msg);
                return true;
            }
            else
            {
                IsRetryArrival = false;
                alarmHandler.SetAlarm(000015);
                return false;
            }
        }
        public bool IsAgvcTransferCommandEmpty()
        {
            return theVehicle.AgvcTransCmdBuffer.Count == 0;
        }

       
        #endregion

        public bool CanVehMove()
        {
            return theVehicle.AseRobotStatus.IsHome && !theVehicle.IsCharging;
        }

        public void UpdateMoveControlReserveOkPositions(MapSection mapSection)
        {
            try
            {
                MapAddress address = mapSection.CmdDirection == EnumPermitDirection.Forward
                    ? mapSection.TailAddress
                    : mapSection.HeadAddress;

                bool isEnd = address.Id == theVehicle.AseMovingGuide.ToAddressId;
                int theta = (int)address.VehicleHeadAngle;
                int speed = (int)mapSection.Speed;

                asePackage.aseMoveControl.PartMove(isEnd, address.Position, theta, speed);
                OnMessageShowEvent?.Invoke(this, $"通知MoveControl延攬通行權{mapSection.Id}成功，下一個可行終點為[{address.Id}]({Convert.ToInt32(address.Position.X)},{Convert.ToInt32(address.Position.Y)})。");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public bool IsMoveStep() => GetCurrentTransferStepType() == EnumTransferStepType.Move || GetCurrentTransferStepType() == EnumTransferStepType.MoveToCharger;

        public void AseMoveControl_OnMoveFinished(object sender, EnumMoveComplete status)
        {
            try
            {
                IsMoveEnd = true;
                OnMoveArrivalEvent?.Invoke(this, new EventArgs());

                #region Not EnumMoveComplete.Success
                if (status == EnumMoveComplete.Fail)
                {
                    agvcConnector.ClearAllReserve();
                    if (IsAvoidMove)
                    {
                        agvcConnector.AvoidFail();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 避車移動異常終止");
                        IsAvoidMove = false;
                        return;
                    }
                    else if (IsOverrideMove)
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 替代路徑移動異常終止");
                        IsOverrideMove = false;
                        return;
                    }
                    else
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 移動異常終止");
                        return;
                    }
                }

                if (status == EnumMoveComplete.Pause)
                {
                    //VisitTransferStepsStatus = EnumThreadStatus.PauseComplete;
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : 移動暫停確認");
                    agvcConnector.PauseAskReserve();
                    PauseVisitTransferSteps();
                    return;
                }

                if (status == EnumMoveComplete.Cancel)
                {
                    StopClearAndReset();
                    if (IsAvoidMove)
                    {
                        agvcConnector.AvoidFail();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 避車移動取消確認");
                        return;
                    }
                    else
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 移動取消確認");
                        return;
                    }
                }
                #endregion

                #region EnumMoveComplete.Success
                agvcConnector.ClearAllReserve();

                MoveCmdInfo moveCmd = (MoveCmdInfo)GetCurTransferStep();

                UpdateVehiclePositionAfterArrival(moveCmd);

                ArrivalStartCharge(moveCmd.EndAddress);

                if (IsAvoidMove)
                {
                    theVehicle.AseMovingGuide.IsAvoidComplete = true;
                    agvcConnector.AvoidComplete();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : 避車移動完成");
                }
                else
                {
                    agvcConnector.MoveArrival();
                    if (IsNextTransferStepIdle())
                    {
                        TransferComplete(moveCmd.CmdId);
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 走行移動完成");
                    }

                    VisitNextTransferStep();
                }

                IsAvoidMove = false;
                IsOverrideMove = false;

                #endregion
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void TransferComplete(string cmdId)
        {
            try
            {
                AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer[cmdId];
                agvcConnector.TransferComplete(agvcTransCmd);
                ClearTransferSteps(cmdId);
                theVehicle.AgvcTransCmdBuffer.Remove(cmdId);
                if (theVehicle.AgvcTransCmdBuffer.Count == 0)
                {
                    agvcConnector.NoCommand();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AseMoveControl_OnRetryMoveFinished(object sender, EnumMoveComplete e)
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : 取放貨異常，觸發重試機制，到站。");

                ForkNgRetryArrivalStartCharge();

                IsRetryArrival = true;

                int timeoutCount = 10;
                while (true)
                {
                    if (asePackage.aseRobotControl.IsRobotCommandExist())
                    {
                        asePackage.aseRobotControl.ClearRobotCommand();
                        SpinWait.SpinUntil(() => false, 200);
                    }
                    else
                    {
                        break;
                    }

                    if (timeoutCount > 0)
                    {
                        timeoutCount--;
                    }
                    else
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 取放貨異常，觸發重試機制，到站。無法清除ForkCommand");
                        return;
                    }
                }

                GoNextTransferStep = true;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void ForkNgRetryArrivalStartCharge()
        {
            MoveCmdInfo moveCmd = (MoveCmdInfo)GetPreTransferStep();

            ArrivalStartCharge(moveCmd.EndAddress);

        }

        private void ArrivalStartCharge(MapAddress endAddress)
        {
            try
            {
                try
                {
                    StartCharge(endAddress);
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AseRobotControl_OnRobotCommandErrorEvent(object sender, TransferStep transferStep)
        {
        }

        public void AseRobotContorl_OnRobotCommandFinishEvent(object sender, TransferStep transferStep)
        {
            try
            {
                EnumTransferStepType transferStepType = transferStep.GetTransferStepType();

                if (transferStepType == EnumTransferStepType.Load)
                {
                    if (agvcConnector.IsCstIdReadReplyOk(transferStep, ReadResult))
                    {
                        if (IsNextTransferStepIdle())
                        {
                            TransferComplete(transferStep.CmdId);
                        }
                        VisitNextTransferStep();
                    }
                }
                else if (transferStepType == EnumTransferStepType.Unload)
                {
                    var slotNumber = theVehicle.AgvcTransCmdBuffer[transferStep.CmdId].SlotNumber;
                    AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(slotNumber);
                    aseCarrierSlotStatus.CarrierId = "";
                    aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;

                    agvcConnector.UnloadComplete(transferStep.CmdId);
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot放貨完成");
                    TransferComplete(transferStep.CmdId);
                    VisitNextTransferStep();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AseRobotControl_OnReadCarrierIdFinishEvent(object sender, EnumSlotNumber slotNumber)
        {
            try
            {
                #region 2019.12.16 Report to Agvc when ForkFinished

                AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(slotNumber);

                if (aseCarrierSlotStatus.CarrierSlotStatus == EnumAseCarrierSlotStatus.ReadFail)
                {
                    var ngMsg = $"貨物ID讀取失敗";
                    OnMessageShowEvent?.Invoke(this, ngMsg);
                    ReadResult = EnumCstIdReadResult.Fail;
                    alarmHandler.SetAlarm(000004);
                    return;
                }
                else if (!IsAgvcTransferCommandEmpty())
                {
                    AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer[GetCurTransferStep().CmdId];
                    if (agvcTransCmd.CassetteId != aseCarrierSlotStatus.CarrierId)
                    {
                        var ngMsg = $"貨物ID[{aseCarrierSlotStatus.CarrierId}]，與命令要求貨物ID[{agvcTransCmd.CassetteId}]不合";
                        OnMessageShowEvent?.Invoke(this, ngMsg);
                        ReadResult = EnumCstIdReadResult.Mismatch;
                        alarmHandler.SetAlarm(000028);
                        return;
                    }
                }

                var msg = $"貨物ID[{aseCarrierSlotStatus.CarrierId}]讀取成功";
                OnMessageShowEvent?.Invoke(this, msg);
                ReadResult = EnumCstIdReadResult.Noraml;

                #endregion
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AseRobotControl_OnRobotInterlockErrorEvent(object sender, TransferStep transferStep)
        {
            try
            {
                EnumTransferStepType transferType = transferStep.GetTransferStepType();
                if (transferType == EnumTransferStepType.Load || transferType == EnumTransferStepType.Unload)
                {
                    RobotCommand robotCommand = (RobotCommand)transferStep;
                    var msg = $"MainFlow : 取放貨異常[InterlockError]，剩餘重試次數[{robotCommand.RobotNgRetryTimes}]";
                    OnMessageShowEvent?.Invoke(this, msg);

                    #region 2019.12.16 Retry                    
                    if (theVehicle.AseRobotStatus.IsHome)
                    {
                        if (robotCommand.RobotNgRetryTimes > 0)
                        {
                            robotCommand.RobotNgRetryTimes--;
                            if (StopCharge())
                            {
                                alarmHandler.ResetAllAlarms();
                                OnMessageShowEvent?.Invoke(this, $"MainFlow : 取放貨異常，充電已停止，觸發重試機制。");
                                LogRetry(robotCommand.RobotNgRetryTimes);
                                IsRetryArrival = false;
                                asePackage.aseMoveControl.RetryMove();
                                return;
                            }
                        }
                    }

                    alarmHandler.ResetAllAlarms();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : 取放貨異常，流程放棄。");
                    if (transferType == EnumTransferStepType.Load)
                    {
                        AbortCommand(transferStep.CmdId, CompleteStatus.InterlockError);
                    }
                    else
                    {
                        theVehicle.AgvcTransCmdBuffer[transferStep.CmdId].CompleteStatus = CompleteStatus.InterlockError;
                        AbortAllAgvcTransCmdInBuffer();
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : 取放貨異常，異常跳出。");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LogRetry(int forkNgRetryTimes)
        {
            try
            {
                var msg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), ",\t", alarmHandler.LastAlarm.AlarmText, ",\t", forkNgRetryTimes);
                mirleLogger.LogString("RetryLog", msg);
            }
            catch (Exception)
            {
            }
        }

        private bool IsNextTransferStepUnload() => GetNextTransferStepType() == EnumTransferStepType.Unload;
        private bool IsNextTransferStepLoad() => GetNextTransferStepType() == EnumTransferStepType.Load;
        private bool IsNextTransferStepMove() => GetNextTransferStepType() == EnumTransferStepType.Move || GetNextTransferStepType() == EnumTransferStepType.MoveToCharger;
        private bool IsNextTransferStepIdle() => GetNextTransferStepType() == EnumTransferStepType.Empty;

        private void VisitNextTransferStep()
        {
            TransferStepsIndex++;
            GoNextTransferStep = true;
        }

        public TransferStep GetCurTransferStep()
        {
            TransferStep transferStep = new EmptyTransferStep();
            if (TransferStepsIndex < transferSteps.Count)
            {
                transferStep = transferSteps[TransferStepsIndex];
            }
            return transferStep;
        }

        public TransferStep GetPreTransferStep()
        {
            TransferStep transferStep = new EmptyTransferStep();
            var preTransferStepsIndex = TransferStepsIndex - 1;
            if (preTransferStepsIndex < transferSteps.Count && preTransferStepsIndex >= 0)
            {
                transferStep = transferSteps[preTransferStepsIndex];
            }
            return transferStep;
        }

        public void PublishOnDoTransferStepEvent(TransferStep transferStep)
        {
            OnDoTransferStepEvent?.Invoke(this, transferStep);
        }

        public void Unload(UnloadCmdInfo unloadCmd)
        {
            AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(unloadCmd.SlotNumber);

            if (aseCarrierSlotStatus.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty)
            {
                alarmHandler.SetAlarm(000017);
                return;
            }

            if (IsUnloadArrival(unloadCmd))
            {
                try
                {
                    int timeoutCount = 10;
                    while (true)
                    {
                        if (asePackage.aseRobotControl.IsRobotCommandExist())
                        {
                            asePackage.aseRobotControl.ClearRobotCommand();
                            SpinWait.SpinUntil(() => false, 200);
                        }
                        else
                        {
                            break;
                        }

                        if (timeoutCount > 0)
                        {
                            timeoutCount--;
                        }
                        else
                        {
                            alarmHandler.ResetAllAlarms();
                            var errorMsg = $"MainFlow : 放貨異常，無法清除Robot命令，流程放棄。";
                            OnMessageShowEvent?.Invoke(this, errorMsg);
                            MainFlowAbnormalMsg = errorMsg;
                            AbortCommand(unloadCmd.CmdId, CompleteStatus.InterlockError);
                            return;
                        }
                    }

                    agvcConnector.Unloading(unloadCmd.CmdId);
                    PublishOnDoTransferStepEvent(unloadCmd);
                    Task.Run(() => asePackage.aseRobotControl.DoRobotCommand(unloadCmd));
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot放貨中, [方向{unloadCmd.PioDirection}][編號={unloadCmd.SlotNumber}][是否PIO={unloadCmd.IsEqPio}]");
                    //batteryLog.LoadUnloadCount++;
                    //SaveBatteryLog();
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
            }
        }

        public void Load(LoadCmdInfo loadCmd)
        {
            FitRobotCommand(loadCmd);

            AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(loadCmd.SlotNumber);

            if (aseCarrierSlotStatus.CarrierSlotStatus != EnumAseCarrierSlotStatus.Empty)
            {
                alarmHandler.SetAlarm(000016);
                return;
            }

            if (IsLoadArrival(loadCmd))
            {
                try
                {
                    int timeoutCount = 10;
                    while (true)
                    {
                        if (asePackage.aseRobotControl.IsRobotCommandExist())
                        {
                            asePackage.aseRobotControl.ClearRobotCommand();
                            SpinWait.SpinUntil(() => false, 200);
                        }
                        else
                        {
                            break;
                        }

                        if (timeoutCount > 0)
                        {
                            timeoutCount--;
                        }
                        else
                        {
                            alarmHandler.ResetAllAlarms();
                            var errorMsg = $"MainFlow : 取貨異常，無法清除Robot命令，流程放棄。";
                            MainFlowAbnormalMsg = errorMsg;
                            OnMessageShowEvent?.Invoke(this, errorMsg);
                            AbortCommand(loadCmd.CmdId, CompleteStatus.InterlockError);
                            return;
                        }
                    }


                    agvcConnector.Loading(loadCmd.CmdId);
                    PublishOnDoTransferStepEvent(loadCmd);
                    ReadResult = EnumCstIdReadResult.Noraml;
                    Task.Run(() => asePackage.aseRobotControl.DoRobotCommand(loadCmd));
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot取貨中, [方向={loadCmd.PioDirection}][編號={loadCmd.SlotNumber}][是否PIO={loadCmd.IsEqPio}]");
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
            }
        }

        public void AbortCommand(string cmdId, CompleteStatus completeStatus)
        {
            ClearTransferSteps(cmdId);
            var agvcTransCmd = theVehicle.AgvcTransCmdBuffer[cmdId];
            theVehicle.AgvcTransCmdBuffer.Remove(cmdId);
            agvcTransCmd.CompleteStatus = completeStatus;
            agvcConnector.TransferComplete(agvcTransCmd);
        }

        public void AbortAllAgvcTransCmdInBuffer()
        {
            ClearTransferSteps();
            foreach (var agvcTransCmd in theVehicle.AgvcTransCmdBuffer.Values)
            {
                if (!IsInterlockErrorOrBcrReadFail(agvcTransCmd))
                {
                    agvcTransCmd.CompleteStatus = CompleteStatus.VehicleAbort;
                }
                AbortCommand(agvcTransCmd.CommandId, agvcTransCmd.CompleteStatus);
            }
        }

        private void FitRobotCommand(RobotCommand robotCommand)
        {
            MapAddress portAddress = theMapInfo.allMapAddresses[robotCommand.PortAddressId];
            robotCommand.IsEqPio = portAddress.PioDirection != EnumPioDirection.None;
            robotCommand.PioDirection = portAddress.PioDirection;
        }

        #region Simple Getters
        public AlarmHandler GetAlarmHandler() => alarmHandler;
        public AgvcConnector GetAgvcConnector() => agvcConnector;
        public AgvcConnectorConfig GetAgvcConnectorConfig() => agvcConnectorConfig;
        public MainFlowConfig GetMainFlowConfig() => mainFlowConfig;
        public MapConfig GetMapConfig() => mapConfig;
        public MapHandler GetMapHandler() => mapHandler;
        public AlarmConfig GetAlarmConfig() => alarmConfig;
        public AsePackage GetAsePackage() => asePackage;
        public AseMoveControl GetAseMoveControl() => asePackage.aseMoveControl;
        public string GetMoveControlStopResult() => asePackage.aseMoveControl.StopResult;
        #endregion

        public bool CallMoveControlWork(MoveCmdInfo moveCmd)
        {
            try
            {

                var msg1 = $"MainFlow : 通知MoveControl傳送";
                OnMessageShowEvent?.Invoke(this, msg1);

                string errorMsg = "";
                if (asePackage.aseMoveControl.Move(moveCmd, ref errorMsg))
                {
                    var msg2 = $"MainFlow : 通知MoveControl傳送，回報可行.";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    PublishOnDoTransferStepEvent(moveCmd);
                    return true;
                }
                else
                {
                    var msg2 = $"MainFlow : 通知MoveControl傳送，回報失敗。{errorMsg}";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Fail);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return false;
            }
        }
        public bool CallMoveControlOverride(MoveCmdInfo moveCmd)
        {
            try
            {
                var msg1 = $"MainFlow : 通知MoveControl[替代路徑]";
                OnMessageShowEvent?.Invoke(this, msg1);
                string errorMsg = "";
                if (asePackage.aseMoveControl.Move(moveCmd, ref errorMsg))
                {
                    var msg2 = $"MainFlow : 通知MoveControl[替代路徑]，回報可行.";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    PublishOnDoTransferStepEvent(moveCmd);
                    return true;
                }
                else
                {
                    var msg2 = $"MainFlow : 通知MoveControl[替代路徑]，回報失敗。{errorMsg}";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Fail);
                    return false;
                }


            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return false;
            }
        }
        public bool CallMoveControlAvoid(MoveCmdInfo moveCmd)
        {
            try
            {
                var msg1 = $"MainFlow : 通知MoveControl[避車路徑]";
                OnMessageShowEvent?.Invoke(this, msg1);

                string errorMsg = "";
                if (asePackage.aseMoveControl.Move(moveCmd, ref errorMsg))
                {
                    var msg2 = $"MainFlow : 通知MoveControl[避車路徑]，回報可行.";
                    OnMessageShowEvent?.Invoke(this, msg1);
                    PublishOnDoTransferStepEvent(moveCmd);
                    return true;
                }
                else
                {
                    var msg2 = $"MainFlow : 通知MoveControl[避車路徑]，回報失敗。{errorMsg}";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Fail);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return false;
            }
        }

        public void SetupAseMovingGuideMovingSections()
        {
            try
            {
                theVehicle.AseMovingGuide.MovingSections = new List<MapSection>();
                for (int i = 0; i < theVehicle.AseMovingGuide.GuideSectionIds.Count; i++)
                {
                    MapSection mapSection = new MapSection();
                    string sectionId = theVehicle.AseMovingGuide.GuideSectionIds[i];
                    string addressId = theVehicle.AseMovingGuide.GuideAddressIds[i + 1];
                    if (!theMapInfo.allMapSections.ContainsKey(sectionId))
                    {
                        throw new Exception($"Map info has no this section ID.[{sectionId}]");
                    }
                    else if (!theMapInfo.allMapAddresses.ContainsKey(addressId))
                    {
                        throw new Exception($"Map info has no this address ID.[{addressId}]");
                    }

                    mapSection = theMapInfo.allMapSections[sectionId];
                    mapSection.CmdDirection = addressId == mapSection.TailAddress.Id ? EnumPermitDirection.Forward : EnumPermitDirection.Backward;
                    theVehicle.AseMovingGuide.MovingSections.Add(mapSection);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                theVehicle.AseMovingGuide.MovingSections = new List<MapSection>();
            }
        }

        private bool UpdateVehiclePositionInMovingStep()
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            if (mapHandler.IsPositionInThisAddress(aseMoveStatus.LastMapPosition, aseMoveStatus.LastAddress.Position))
            {
                return false;
            }
            AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);
            List<MapSection> MovingSections = aseMovingGuide.MovingSections;
            if (MovingSections.Count <= 0) return false;
            int searchingSectionIndex = aseMovingGuide.MovingSectionsIndex;
            bool isUpdateSection = false;
            while (searchingSectionIndex < MovingSections.Count)
            {
                try
                {
                    if (mapHandler.IsPositionInThisSection(MovingSections[searchingSectionIndex], theVehicle.AseMoveStatus.LastMapPosition))
                    {
                        while (aseMovingGuide.MovingSectionsIndex < searchingSectionIndex)
                        {
                            batteryLog.MoveDistanceTotalM += (int)(aseMovingGuide.MovingSections[aseMovingGuide.MovingSectionsIndex].HeadToTailDistance / 1000);
                            SaveBatteryLog();
                            aseMovingGuide.MovingSectionsIndex++;
                            FitVehicalLocationAndMoveCmd();
                            agvcConnector.ReportSectionPass();
                            isUpdateSection = true;
                        }

                        FitVehicalLocation();

                        UpdateAgvcConnectorGotReserveOkSections(MovingSections[searchingSectionIndex].Id);

                        //if (mainFlowConfig.CustomerName == "AUO")
                        //{
                        //    UpdatePlcVehicleBeamSensor();
                        //}

                        break;
                    }
                    else
                    {
                        searchingSectionIndex++;
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                    break;
                }
            }
            return isUpdateSection;
        }

        private void FitVehicalLocation()
        {
            AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);
            var section = aseMovingGuide.MovingSections[aseMovingGuide.MovingSectionsIndex];
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            FindeNeerlyAddressInTheMovingSection(section, ref aseMoveStatus);
            aseMoveStatus.LastSection = theMapInfo.allMapSections[section.Id];
            aseMoveStatus.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(theVehicle.AseMoveStatus.LastMapPosition, aseMoveStatus.LastSection.HeadAddress.Position);
            theVehicle.AseMoveStatus = aseMoveStatus;
        }

        private void FitVehicalLocationAndMoveCmd()
        {
            AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);
            MapSection section = aseMovingGuide.MovingSections[aseMovingGuide.MovingSectionsIndex];
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            aseMoveStatus.LastSection = section;
            if (section.CmdDirection == EnumPermitDirection.Forward)
            {
                aseMoveStatus.LastAddress = section.HeadAddress;
                aseMoveStatus.LastSection.VehicleDistanceSinceHead = 0;
            }
            else
            {
                aseMoveStatus.LastAddress = section.TailAddress;
                aseMoveStatus.LastSection.VehicleDistanceSinceHead = section.HeadToTailDistance;
            }
            theVehicle.AseMoveStatus = aseMoveStatus;
        }

        private void FindeNeerlyAddressInTheMovingSection(MapSection mapSection, ref AseMoveStatus aseMoveStatus)
        {
            try
            {
                double neerlyDistance = 999999;
                foreach (MapAddress mapAddress in mapSection.InsideAddresses)
                {
                    double dis = mapHandler.GetDistance(theVehicle.AseMoveStatus.LastMapPosition, mapAddress.Position);

                    if (dis < neerlyDistance)
                    {
                        neerlyDistance = dis;
                        aseMoveStatus.NeerlyAddress = mapAddress;
                    }
                }
                aseMoveStatus.LastAddress = aseMoveStatus.NeerlyAddress;

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void UpdateVehiclePositionAfterArrival(MoveCmdInfo moveCmd)
        {
            try
            {
                MapSection lastSection = new MapSection();
                if (theVehicle.AseMovingGuide.MovingSections.Count > 0)
                {
                    var lastMoveSection = theVehicle.AseMovingGuide.MovingSections.FindLast(x => x.Id != null);
                    lastSection = theMapInfo.allMapSections[lastMoveSection.Id];
                    lastSection.CmdDirection = lastMoveSection.CmdDirection;
                }
                else
                {
                    lastSection = theVehicle.AseMoveStatus.LastSection;
                }

                var lastAddress = moveCmd.EndAddress;
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                aseMoveStatus.LastAddress = moveCmd.EndAddress;
                aseMoveStatus.LastMapPosition = lastAddress.Position;
                aseMoveStatus.LastSection = lastSection;
                aseMoveStatus.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(lastAddress.Position, lastSection.HeadAddress.Position);
                theVehicle.AseMoveStatus = aseMoveStatus;

                var msg = $"車輛抵達終點站{moveCmd.EndAddress.Id}，位置更新。";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"車輛抵達終點站{moveCmd.EndAddress.Id}，位置更新失敗。");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void UpdateVehiclePositionManual()
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            FindeNeerlyAddress(ref aseMoveStatus);

            var sectionsWithinNeerlyAddress = new List<MapSection>();
            FindSectionsWithinNeerlyAddress(aseMoveStatus.NeerlyAddress.Id, ref sectionsWithinNeerlyAddress);

            foreach (MapSection mapSection in sectionsWithinNeerlyAddress)
            {
                if (mapHandler.IsPositionInThisSection(mapSection, theVehicle.AseMoveStatus.LastMapPosition))
                {
                    aseMoveStatus.LastSection = mapSection;
                    aseMoveStatus.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(theVehicle.AseMoveStatus.LastMapPosition, mapSection.HeadAddress.Position);
                    break;
                }
            }
            //if (mainFlowConfig.CustomerName == "AUO")
            //{
            //    UpdatePlcVehicleBeamSensor();
            //}
            theVehicle.AseMoveStatus = aseMoveStatus;
        }

        private void FindSectionsWithinNeerlyAddress(string neerlyAddressId, ref List<MapSection> sectionsWithinNeerlyAddress)
        {
            foreach (MapSection mapSection in theMapInfo.allMapSections.Values)
            {
                if (mapSection.InsideAddresses.FindIndex(z => z.Id == neerlyAddressId) > -1)
                {
                    sectionsWithinNeerlyAddress.Add(mapSection);
                }
            }
        }

        private void FindeNeerlyAddress(ref AseMoveStatus aseMoveStatus)
        {
            try
            {
                double neerlyDistance = 999999;
                foreach (MapAddress mapAddress in theMapInfo.allMapAddresses.Values)
                {
                    double dis = mapHandler.GetDistance(aseMoveStatus.LastMapPosition, mapAddress.Position);

                    if (dis < neerlyDistance)
                    {
                        neerlyDistance = dis;
                        aseMoveStatus.NeerlyAddress = mapAddress;
                    }
                }
                aseMoveStatus.LastAddress = aseMoveStatus.NeerlyAddress;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        //private bool IsVehlocStayInSameAddress(VehicleLocation vehloc)
        //{
        //    if (!string.IsNullOrEmpty(vehloc.LastAddress.Id) && !string.IsNullOrEmpty(vehloc.LastSection.Id))
        //    {
        //        if (mapHandler.IsPositionInThisAddress(vehloc.RealPosition, vehloc.LastAddress.Position))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        private void UpdatePlcVehicleBeamSensor()
        {
            //var plcVeh = (PlcVehicle)theVehicle.TheVehicleIntegrateStatus;
            //var lastSection = theVehicle.AseMoveStatus.LastSection;
            //var curDistance = lastSection.VehicleDistanceSinceHead;
            //var index = lastSection.BeamSensorDisables.FindIndex(x => x.Min <= curDistance && x.Max >= curDistance);
            //if (index > -1)
            //{
            //    var beamDisable = lastSection.BeamSensorDisables[index];
            //    plcVeh.FrontBeamSensorDisable = beamDisable.FrontDisable;
            //    plcVeh.BackBeamSensorDisable = beamDisable.BackDisable;
            //    plcVeh.LeftBeamSensorDisable = beamDisable.LeftDisable;
            //    plcVeh.RightBeamSensorDisable = beamDisable.RightDisable;
            //}
            //else
            //{
            //    plcVeh.FrontBeamSensorDisable = false;
            //    plcVeh.BackBeamSensorDisable = false;
            //    plcVeh.LeftBeamSensorDisable = false;
            //    plcVeh.RightBeamSensorDisable = false;
            //}
        }

        private void UpdateAgvcConnectorGotReserveOkSections(string id)
        {
            int getReserveOkSectionIndex = 0;
            try
            {
                var getReserveOkSections = agvcConnector.GetReserveOkSections();
                getReserveOkSectionIndex = getReserveOkSections.FindIndex(x => x.Id == id);
                if (getReserveOkSectionIndex < 0) return;
                for (int i = 0; i < getReserveOkSectionIndex; i++)
                {
                    //Remove passed section in ReserveOkSection
                    agvcConnector.DequeueGotReserveOkSections();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"FAIL [SecId={id}][Index={getReserveOkSectionIndex}]");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        private void StartCharge(MapAddress endAddress)
        {
            try
            {
                var address = endAddress;
                var percentage = theVehicle.AseBatteryStatus.Percentage;
                var highPercentage = theVehicle.AutoChargeHighThreshold;

                if (address.IsCharger)
                {
                    if (theVehicle.IsCharging)
                    {
                        var msg = $"車子抵達{address.Id},充電方向為{address.ChargeDirection},因充電狀態為{theVehicle.IsCharging}, 故暫不再送出充電信號";
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                        return;
                    }

                    if (IsHighPower())
                    {
                        var msg = $"車子抵達{address.Id},充電方向為{address.ChargeDirection},因SOC為{percentage:F2} > {highPercentage:F2}(高水位門檻值), 故暫不充電";
                        OnMessageShowEvent?.Invoke(this, msg);
                        return;
                    }
                    else
                    {
                        var msg = $"車子抵達{address.Id},充電方向為{address.ChargeDirection},因SOC為{percentage:F2} < {highPercentage:F2}(高水位門檻值), 故送出充電信號";
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                        OnMessageShowEvent?.Invoke(this, msg);
                    }

                    agvcConnector.ChargHandshaking();

                    int timeoutCount = 10;
                    do
                    {
                        if (theVehicle.IsCharging) break;
                        timeoutCount--;
                        asePackage.aseBatteryControl.StartCharge(address.ChargeDirection);
                        SpinWait.SpinUntil(() => theVehicle.IsCharging, 100);
                    } while (timeoutCount >= 0);

                    if (!theVehicle.IsCharging)
                    {
                        alarmHandler.SetAlarm(000013);
                    }
                    else
                    {
                        agvcConnector.Charging();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 到達站點[{address.Id}]充電中。");
                        //batteryLog.ChargeCount++;
                        //SaveBatteryLog();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void LowPowerStartCharge(MapAddress lastAddress)
        {
            try
            {
                var address = lastAddress;
                var percentage = theVehicle.AseBatteryStatus.Percentage;
                var pos = theVehicle.AseMoveStatus.LastMapPosition;
                if (address.IsCharger && mapHandler.IsPositionInThisAddress(pos, address.Position))
                {
                    if (theVehicle.IsCharging)
                    {
                        return;
                    }
                    else
                    {
                        var msg = $"車子停在{address.Id}且目前沒有傳送命令,充電方向為{address.PioDirection},因SOC為{percentage:F2} < {theVehicle.AutoChargeLowThreshold:F2}(自動充電門檻值), 故送出充電信號";
                        OnMessageShowEvent?.Invoke(this, msg);
                    }

                    agvcConnector.ChargHandshaking();

                    int timeoutCount = 10;
                    do
                    {
                        if (theVehicle.IsCharging) break;
                        timeoutCount--;
                        asePackage.aseBatteryControl.StartCharge(address.ChargeDirection);
                        SpinWait.SpinUntil(() => theVehicle.IsCharging, 100);
                    } while (timeoutCount >= 0);

                    if (!theVehicle.IsCharging)
                    {
                        alarmHandler.SetAlarm(000013);
                    }
                    else
                    {
                        agvcConnector.Charging();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 充電中, [Address={address.Id}][IsCharging={theVehicle.IsCharging}]");
                        //batteryLog.ChargeCount++;
                        //SaveBatteryLog();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public bool StopCharge()
        {
            try
            {
                var beginMsg = $"MainFlow : 嘗試停止充電, [IsCharging={theVehicle.IsCharging}]";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, beginMsg);

                if (!theVehicle.IsCharging) return true;

                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                //if (!mapHandler.IsPositionInThisAddress(aseMoveStatus.LastMapPosition, aseMoveStatus.LastAddress.Position))
                //{
                //    var msg = $"Stop charge fail, RealPos is not in LastAddress [Real=({(int)aseMoveStatus.LastMapPosition.X},{(int)aseMoveStatus.LastMapPosition.Y})][LastAddress={aseMoveStatus.LastAddress.Id}]";
                //    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                //    return true;
                //}
                var address = aseMoveStatus.LastAddress;
                if (address.IsCharger)
                {
                    agvcConnector.ChargHandshaking();
                    int timeoutCount = 10;
                    do
                    {
                        if (!theVehicle.IsCharging) break;
                        timeoutCount--;
                        asePackage.aseBatteryControl.StopCharge();
                        SpinWait.SpinUntil(() => !theVehicle.IsCharging, 100);
                    } while (timeoutCount >= 0);

                    if (theVehicle.IsCharging)
                    {
                        alarmHandler.SetAlarm(000014);
                        StopVehicle();
                        return false;
                    }
                    else
                    {
                        agvcConnector.ChargeOff();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Stop Charge, [IsCharging={theVehicle.IsCharging}]");
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return false;
            }
        }

        public void StopClearAndReset()
        {
            try
            {
                PauseVisitTransferSteps();
                agvcConnector.ClearAllReserve();
                StopVehicle();
                AbortAllAgvcTransCmdInBuffer();

                if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.On)
                {
                    theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                    agvcConnector.StatusChangeReport();
                }

                if (theVehicle.AseCarrierSlotA.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading || theVehicle.AseCarrierSlotB.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
                {
                    asePackage.aseRobotControl.ReadCarrierId();
                }

                ReadResult = EnumCstIdReadResult.Noraml;

                var msg = $"MainFlow : Stop And Clear";
                OnMessageShowEvent?.Invoke(this, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private bool IsInterlockErrorOrBcrReadFail(AgvcTransCmd agvcTransCmd)
        {
            return agvcTransCmd.CompleteStatus == CompleteStatus.InterlockError || agvcTransCmd.CompleteStatus == CompleteStatus.IdmisMatch || agvcTransCmd.CompleteStatus == CompleteStatus.IdreadFailed;
        }

        public EnumTransferStepType GetCurrentTransferStepType()
        {
            try
            {
                if (transferSteps.Count > 0)
                {
                    if (TransferStepsIndex < transferSteps.Count)
                    {
                        return transferSteps[TransferStepsIndex].GetTransferStepType();
                    }
                }

                return EnumTransferStepType.Empty;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return EnumTransferStepType.Empty;
            }
        }
        public EnumTransferStepType GetNextTransferStepType()
        {
            try
            {
                if (transferSteps.Count > 0)
                {
                    if (TransferStepsIndex + 1 < transferSteps.Count)
                    {
                        return transferSteps[TransferStepsIndex + 1].GetTransferStepType();
                    }
                }

                return EnumTransferStepType.Empty;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return EnumTransferStepType.Empty;
            }
        }

        public int GetTransferStepsCount()
        {
            return transferSteps.Count;
        }

        public void StopVehicle()
        {
            asePackage.aseMoveControl.StopAndClear();
            asePackage.aseRobotControl.ClearRobotCommand();
            asePackage.aseBatteryControl.StopCharge();

            var msg = $"MainFlow : Stop Vehicle, [MoveState={theVehicle.AseMoveStatus.AseMoveState}][IsCharging={theVehicle.IsCharging}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public bool SetManualToAuto()
        {
            StopClearAndReset();
            //string reason = "";
            //if (!moveControlPlate.CanAuto(ref reason))
            //{
            //    reason = $"Manual 切換 Auto 失敗，原因： " + reason;
            //    OnMessageShowEvent?.Invoke(this, reason);
            //    return false;
            //}
            //else
            //{
            //    string msg = $"Manual 切換 Auto 成功";
            //    OnMessageShowEvent?.Invoke(this, msg);
            //    return true;
            //}

            string msg = $"Manual 切換 Auto 成功";
            OnMessageShowEvent?.Invoke(this, msg);
            return true;

        }

        public void ResetAllarms()
        {
            alarmHandler.ResetAllAlarms();
        }

        private void AlarmHandler_OnResetAllAlarmsEvent(object sender, string msg)
        {
            asePackage.aseBuzzerControl.ResetAllAlarmCode();
        }

        private void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            asePackage.aseBuzzerControl.SetAlarmCode(alarm.Id, true);
        }

        public void SetupVehicleSoc(double percentage)
        {
            asePackage.aseBatteryControl.SetPercentage(percentage);
        }

        private void GetInitialSoc(string v)
        {
            try
            {
                string filePath = Path.Combine(Environment.CurrentDirectory, "log", v);
                if (File.Exists(filePath))
                {
                    var text = File.ReadAllText(filePath);
                    bool isParse = double.TryParse(text, out double result);
                    if (isParse)
                    {
                        InitialSoc = result;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void RenameCstId(EnumSlotNumber slotNumber, string newCstId)
        {
            try
            {
                AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(slotNumber);
                aseCarrierSlotStatus.CarrierId = newCstId;
                aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;

                AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer.Values.First(x => x.SlotNumber == slotNumber);
                agvcTransCmd.CassetteId = newCstId;
                theVehicle.AgvcTransCmdBuffer[agvcTransCmd.CommandId] = agvcTransCmd;

                if (transferSteps.Count > 0)
                {
                    foreach (var transferStep in transferSteps)
                    {
                        if (transferStep.CmdId == agvcTransCmd.CommandId && IsRobCommand(transferStep))
                        {
                            ((RobotCommand)transferStep).CassetteId = newCstId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool IsRobCommand(TransferStep transferStep) => transferStep.GetTransferStepType() == EnumTransferStepType.Load || transferStep.GetTransferStepType() == EnumTransferStepType.Unload;

        public void AgvcConnector_OnCmdPauseEvent(ushort iSeqNum, PauseEvent type)
        {
            try
            {
                agvcConnector.PauseAskReserve();
                PauseVisitTransferSteps();
                asePackage.aseMoveControl.VehclePause();
                var msg = $"MainFlow : 接受[{type}]命令。";
                OnMessageShowEvent(this, msg);
                agvcConnector.PauseReply(iSeqNum, 0, PauseEvent.Pause);
                if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.Off)
                {
                    theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.On;
                    agvcConnector.StatusChangeReport();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void AgvcConnector_OnCmdResumeEvent(ushort iSeqNum, PauseEvent type)
        {
            try
            {
                var msg = $"MainFlow : 接受[{type}]命令。";
                OnMessageShowEvent(this, msg);
                agvcConnector.PauseReply(iSeqNum, 0, PauseEvent.Continue);
                asePackage.aseMoveControl.VehcleContinue();
                ResumeVisitTransferSteps();
                agvcConnector.ResumeAskReserve();
                if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.Off)
                {
                    theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                    agvcConnector.StatusChangeReport();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private bool IsMoveControllPause()
        {
            return (theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Pause || theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Pausing);
        }

        private bool IsMoveControlStop()
        {
            return (theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Idle || theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Stoping);
        }

        private void UpdateAgvcConnectorNeedReserveSections(string reserveSectionID)
        {
            var needReserveSections = agvcConnector.GetNeedReserveSections();
            var index = needReserveSections.FindIndex(x => x.Id == reserveSectionID);
            if (index > -1)
            {
                needReserveSections.RemoveAt(index);
                agvcConnector.SetupNeedReserveSections(needReserveSections);
            }
        }

        public void AgvcConnector_OnCmdCancelAbortEvent(ushort iSeqNum, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            try
            {
                var msg = $"MainFlow : 接受[{receive.CancelAction}]命令。";
                OnMessageShowEvent(this, msg);
                agvcConnector.CancelAbortReply(iSeqNum, 0, receive);
                //theVehicle.AseMovingGuide = new AseMovingGuide();
                //asePackage.aseMoveControl.VehcleCancel();
                //agvcConnector.ClearAllReserve();
                //agvcTransCmd.CompleteStatus = receive.CancelAction == CancelActionType.CmdAbort ? CompleteStatus.Abort : CompleteStatus.Cancel;
                //StopVisitTransferSteps();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public bool IsPositionInThisAddress(MapPosition realPosition, MapPosition addressPosition)
        {
            return mapHandler.IsPositionInThisAddress(realPosition, addressPosition);
        }

        public bool IsAddressInThisSection(MapSection mapSection, MapAddress mapAddress)
        {
            return mapSection.InsideAddresses.FindIndex(x => x.Id == mapAddress.Id) > -1;
        }

        public void LogDuel()
        {
            var msg = "DuelStartSectionHappend";
            OnMessageShowEvent?.Invoke(this, msg);
            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
        }

        public void LoadMainFlowConfig()
        {

            mainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(@"D:\AgvConfigs\MainFlow.xml");
        }

        public void SetMainFlowConfig(MainFlowConfig mainFlowConfig)
        {
            this.mainFlowConfig = mainFlowConfig;
            xmlHandler.WriteXml(mainFlowConfig, @"D:\AgvConfigs\MainFlow.xml");
        }

        public void LoadAgvcConnectorConfig()
        {
            agvcConnectorConfig = xmlHandler.ReadXml<AgvcConnectorConfig>(@"D:\AgvConfigs\AgvcConnector.xml");
        }

        public void SetAgvcConnectorConfig(AgvcConnectorConfig agvcConnectorConfig)
        {
            this.agvcConnectorConfig = agvcConnectorConfig;
            xmlHandler.WriteXml(this.agvcConnectorConfig, @"D:\AgvConfigs\AgvcConnector.xml");
        }

        private void AseBatteryControl_OnBatteryPercentageChangeEvent(object sender, double batteryPercentage)
        {
            try
            {
                batteryLog.InitialSoc = (int)batteryPercentage;
                SaveBatteryLog();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void ReadCarrierId()
        {
            try
            {
                asePackage.aseRobotControl.ReadCarrierId();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void BuzzOff()
        {
            try
            {
                asePackage.aseBuzzerControl.BuzzerOff();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void ResetMoveControlStopResult()
        {
            try
            {
                asePackage.aseMoveControl.StopResult = "";
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void SaveBatteryLog()
        {
            xmlHandler.WriteXml(batteryLog, @"BatteryLog.xml");
        }

        public void ResetBatteryLog()
        {
            BatteryLog tempBatteryLog = new BatteryLog();
            tempBatteryLog.ResetTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff");
            tempBatteryLog.InitialSoc = batteryLog.InitialSoc;
            batteryLog = tempBatteryLog;
            //TODO: AgvcConnector

        }

        private void AsePackage_OnMessageShowEvent(object sender, string e)
        {
            OnMessageShowEvent?.Invoke(this, e);
        }

        private void LogException(string classMethodName, string exMsg)
        {
            try
            {
                mirleLogger.Log(new Mirle.Tools.LogFormat("Error", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", exMsg));
            }
            catch (Exception)
            {
            }
        }

        private void LogDebug(string classMethodName, string msg)
        {
            try
            {
                mirleLogger.Log(new Mirle.Tools.LogFormat("Debug", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", msg));
            }
            catch (Exception)
            {
            }
        }


    }
}
