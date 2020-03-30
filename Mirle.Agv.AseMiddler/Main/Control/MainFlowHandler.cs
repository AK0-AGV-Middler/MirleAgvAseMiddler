using Google.Protobuf.Collections;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using Mirle.Agv.AseMiddler.View;
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
using com.mirle.iibg3k0.ttc.Common;
using System.Collections.Concurrent;

namespace Mirle.Agv.AseMiddler.Controller
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
        public bool IsFirstAhGet { get; set; }
        public EnumCstIdReadResult ReadResult { get; set; } = EnumCstIdReadResult.Noraml;
        public bool NeedRename { get; set; } = false;
        public bool IsSimulation { get; set; }
        public string MainFlowAbnormalMsg { get; set; }
        public bool IsRetryArrival { get; set; } = false;

        private ConcurrentQueue<AseMoveStatus> FakeReserveOkAseMoveStatus { get; set; } = new ConcurrentQueue<AseMoveStatus>();
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
                //mirleLogger.CreateXmlLogger(batteryLog, @"BatteryLog.xml");
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
                asePackage = new AsePackage(theMapInfo.gateTypeMap);
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
                IsFirstAhGet = theVehicle.IsSimulation;

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
                agvcConnector.OnLogMsgEvent += LogMsgHandler;
                agvcConnector.OnRenameCassetteIdEvent += AgvcConnector_OnRenameCassetteIdEvent;
                agvcConnector.OnCassetteIdReadReplyAbortCommandEvent += AgvcConnector_OnCassetteIdReadReplyAbortCommandEvent;
                agvcConnector.OnStopClearAndResetEvent += AgvcConnector_OnStopClearAndResetEvent;

                //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                asePackage.aseMoveControl.OnMoveFinishedEvent += AseMoveControl_OnMoveFinished;
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

                alarmHandler.OnResetAllAlarmsEvent += asePackage.aseBuzzerControl.ResetAllAlarmCode;
                alarmHandler.OnResetAllAlarmsEvent += agvcConnector.AlarmHandler_OnResetAllAlarmsEvent;

                theVehicle.OnAutoStateChangeEvent += TheVehicle_OnAutoStateChangeEvent;

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
                    asePackage.SetVehicleAutoScenario();
                    alarmHandler.ResetAllAlarms();
                    IntoAuto();
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
                    theVehicle.AseMoveStatus.LastMapPosition = theMapInfo.addressMap.First(x => x.Key != "").Value.Position;
                }
                catch (Exception ex)
                {
                    theVehicle.AseMoveStatus.LastMapPosition = new MapPosition();
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
            }
            StartVisitTransferSteps();
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
                        if (TransferStepsIndex < 0)
                        {
                            TransferStepsIndex = 0;
                            GoNextTransferStep = true;
                            continue;
                        }
                        if (transferSteps.Count == 0)
                        {
                            transferSteps.Add(new EmptyTransferStep());
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
                    MoveCmdInfo moveCmdInfo = (MoveCmdInfo)transferStep;
                    if (moveCmdInfo.EndAddress.Id == theVehicle.AseMoveStatus.LastAddress.Id)
                    {
                        AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
                    }
                    else
                    {                        
                        theVehicle.AseMovingGuide.commandId = moveCmdInfo.CmdId;
                        agvcConnector.ReportSectionPass();
                        asePackage.aseMoveControl.PartMove(theVehicle.AseMoveStatus);
                        theVehicle.AseMoveStatus.IsMoveEnd = false;
                        agvcConnector.AskGuideAddressesAndSections(moveCmdInfo);
                    }
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
            if (TransferStepsIndex + 1 < transferSteps.Count)
            {
                transferSteps.RemoveAt(TransferStepsIndex);
            }

            GoNextTransferStep = true;
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
            TransferStepsIndex = 0;
            transferSteps = new List<TransferStep>();
            GoNextTransferStep = true;
            IsVisitTransferStepPause = false;

            var msg = $"MainFlow : 清除搬送流程";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void ClearTransferSteps(string cmdId)
        {
            IsVisitTransferStepPause = true;
            GoNextTransferStep = false;
            int transferStepCountBeforeRemove = transferSteps.Count;
            transferSteps.RemoveAll(x => x.CmdId == cmdId);
            TransferStepsIndex = TransferStepsIndex + transferSteps.Count - transferStepCountBeforeRemove;
            if (transferSteps.Count == 0)
            {
                TransferStepsIndex = -1;
                transferSteps.Add(new EmptyTransferStep());
            }
            GoNextTransferStep = true;
            IsVisitTransferStepPause = false;

            var msg = $"MainFlow : 清除已完成命令[{cmdId}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        private void PauseTransfer()
        {
            agvcConnector.PauseAskReserve();
            PauseVisitTransferSteps();
        }

        private void ResumeTransfer()
        {
            ResumeVisitTransferSteps();
            agvcConnector.ResumeAskReserve();
        }

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
                    if (theVehicle.AseMoveStatus.LastMapPosition == null) continue;
                    //if (IsVehlocStayInSameAddress(vehicleLocation)) continue;

                    if (theVehicle.AutoState == EnumAutoState.Auto)
                    {
                        if (transferSteps.Count > 0)
                        {
                            //有搬送命令時，比對當前Position與搬送路徑Sections計算LastSection/LastAddress/Distance                           
                            if (IsMoveStep())
                            {
                                if (!theVehicle.AseMoveStatus.IsMoveEnd)
                                {
                                    if (theVehicle.IsSimulation)
                                    {
                                        if (FakeReserveOkAseMoveStatus.Any())
                                        {
                                            if (theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.On)
                                            {
                                                theVehicle.AseMovingGuide.ReserveStop = VhStopSingle.Off;
                                                agvcConnector.StatusChangeReport();
                                            }
                                            MoveToReserveOkPositions();
                                            sw.Reset();
                                        }
                                    }
                                    else if (UpdateVehiclePositionInMovingStep())
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

        private void MoveToReserveOkPositions()
        {
            theVehicle.AseMoveStatus.AseMoveState = EnumAseMoveState.Working;
            SpinWait.SpinUntil(() => false, 2000);
            FakeReserveOkAseMoveStatus.TryDequeue(out AseMoveStatus targetAseMoveStatus);
            AseMoveStatus tempMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

            if (targetAseMoveStatus.LastSection.Id != tempMoveStatus.LastSection.Id)
            {
                tempMoveStatus.LastSection = targetAseMoveStatus.LastSection;
                GetFakeSectionDistance(tempMoveStatus);
                theVehicle.AseMoveStatus = tempMoveStatus;
                agvcConnector.ReportSectionPass();

                SpinWait.SpinUntil(() => false, 1000);

                tempMoveStatus.LastMapPosition = targetAseMoveStatus.LastMapPosition;
                tempMoveStatus.Speed = targetAseMoveStatus.Speed;
                tempMoveStatus.HeadDirection = targetAseMoveStatus.HeadDirection;
                tempMoveStatus.LastAddress = targetAseMoveStatus.LastAddress;
                GetFakeSectionDistance(tempMoveStatus);
                theVehicle.AseMoveStatus = tempMoveStatus;
                agvcConnector.ReportSectionPass();

                theVehicle.AseMovingGuide.MovingSectionsIndex++;
                UpdateAgvcConnectorGotReserveOkSections(targetAseMoveStatus.LastSection.Id);
                SpinWait.SpinUntil(() => false, 1000);
            }

            if (targetAseMoveStatus.LastAddress.Id != theVehicle.AseMoveStatus.LastAddress.Id)
            {
                SpinWait.SpinUntil(() => false, 1000);

                tempMoveStatus.LastMapPosition = targetAseMoveStatus.LastMapPosition;
                tempMoveStatus.Speed = targetAseMoveStatus.Speed;
                tempMoveStatus.HeadDirection = targetAseMoveStatus.HeadDirection;
                tempMoveStatus.LastAddress = targetAseMoveStatus.LastAddress;
                GetFakeSectionDistance(tempMoveStatus);
                theVehicle.AseMoveStatus = tempMoveStatus;
                agvcConnector.ReportSectionPass();
            }

            if (FakeReserveOkAseMoveStatus.IsEmpty)
            {
                theVehicle.AseMoveStatus.AseMoveState = EnumAseMoveState.Idle;
            }

            if (targetAseMoveStatus.IsMoveEnd)
            {
                AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
                theVehicle.AseMoveStatus.IsMoveEnd = true;
            }
        }

        private static void GetFakeSectionDistance(AseMoveStatus tempMoveStatus)
        {
            if (tempMoveStatus.LastAddress.Id == tempMoveStatus.LastSection.TailAddress.Id)
            {
                tempMoveStatus.LastSection.VehicleDistanceSinceHead = tempMoveStatus.LastSection.HeadToTailDistance;
            }
            else
            {
                tempMoveStatus.LastSection.VehicleDistanceSinceHead = 0;
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
                    if (theVehicle.AgvcTransCmdBuffer.Values.First().SlotNumber == EnumSlotNumber.L)
                    {
                        agvcTransCmd.SlotNumber = EnumSlotNumber.R;
                    }
                    else
                    {
                        agvcTransCmd.SlotNumber = EnumSlotNumber.L;
                    }
                }
                else
                {
                    agvcTransCmd.SlotNumber = EnumSlotNumber.L;
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
                PauseTransfer();
                agvcTransCmd.CommandState = CommandState.LoadEnroute;
                theVehicle.AgvcTransCmdBuffer.Add(agvcTransCmd.CommandId, agvcTransCmd);
                agvcTransCmd.RobotNgRetryTimes = mainFlowConfig.RobotNgRetryTimes;
                SetupTransferSteps(agvcTransCmd);
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 0, "");
                if (theVehicle.AgvcTransCmdBuffer.Count != 2) GoNextTransferStep = true;
                ResumeTransfer();
                asePackage.SetTransferCommandInfoRequest();
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
            MapAddress unloadAddress = theMapInfo.addressMap[unloadAddressId];
            if (!unloadAddress.IsTransferPort())
            {
                throw new Exception($"{unloadAddressId} can not unload.");
            }
        }

        private void CheckLoadPortAddress(string loadAddressId)
        {
            CheckMoveEndAddress(loadAddressId);
            MapAddress loadAddress = theMapInfo.addressMap[loadAddressId];
            if (!loadAddress.IsTransferPort())
            {
                throw new Exception($"{loadAddressId} can not load.");
            }
        }

        private void CheckMoveEndAddress(string unloadAddressId)
        {
            if (!theMapInfo.addressMap.ContainsKey(unloadAddressId))
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
            #region 避車檢查
            try
            {
                var msg = $"MainFlow : 收到避車命令，終點[{aseMovingGuide.ToAddressId}]，開始檢查。";
                OnMessageShowEvent?.Invoke(this, msg);

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

            //transferSteps.Add(new EmptyTransferStep(agvcTransCmd.CommandId));
        }

        private void TransferStepsAddUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            UnloadCmdInfo unloadCmdInfo = new UnloadCmdInfo(agvcTransCmd);
            MapAddress unloadAddress = theMapInfo.addressMap[unloadCmdInfo.PortAddressId];
            unloadCmdInfo.GateType = unloadAddress.GateType;
            if (unloadAddress.PortIdMap.ContainsKey(agvcTransCmd.UnloadPortId))
            {
                unloadCmdInfo.PortNumber = unloadAddress.PortIdMap[agvcTransCmd.UnloadPortId];
            }
            else
            {
                unloadCmdInfo.PortNumber = "1";
            }
            transferSteps.Add(unloadCmdInfo);
        }

        private void TransferStepsAddLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            LoadCmdInfo loadCmdInfo = new LoadCmdInfo(agvcTransCmd);
            MapAddress loadAddress = theMapInfo.addressMap[loadCmdInfo.PortAddressId];
            loadCmdInfo.GateType = loadAddress.GateType;
            if (loadAddress.PortIdMap.ContainsKey(agvcTransCmd.LoadPortId))
            {
                loadCmdInfo.PortNumber = loadAddress.PortIdMap[agvcTransCmd.LoadPortId];
            }
            else
            {
                loadCmdInfo.PortNumber = "1";
            }
            transferSteps.Add(loadCmdInfo);
        }

        private void TransferStepsAddMoveCmdInfo(string endAddressId, string cmdId)
        {
            MapAddress endAddress = theMapInfo.addressMap[endAddressId];
            MoveCmdInfo moveCmd = new MoveCmdInfo(endAddress, cmdId);
            transferSteps.Add(moveCmd);
        }

        private void TransferStepsAddMoveToChargerCmdInfo(string endAddressId, string cmdId)
        {
            MapAddress endAddress = theMapInfo.addressMap[endAddressId];
            MoveToChargerCmdInfo moveCmd = new MoveToChargerCmdInfo(endAddress, cmdId);
            transferSteps.Add(moveCmd);
        }

        private MoveToChargerCmdInfo GetMoveToChargerCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MapAddress endAddress = theMapInfo.addressMap[agvcTransCmd.UnloadAddressId];
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

        public bool CanVehMove()
        {
            return theVehicle.AseRobotStatus.IsHome && !theVehicle.IsCharging;
        }

        public void AgvcConnector_GetReserveOkUpdateMoveControlNextPartMovePosition(MapSection mapSection)
        {
            try
            {
                int sectionIndex = theVehicle.AseMovingGuide.GuideSectionIds.FindIndex(x => x == mapSection.Id);
                MapAddress address = theMapInfo.addressMap[theVehicle.AseMovingGuide.GuideAddressIds[sectionIndex + 1]];

                bool isEnd = false;
                EnumAddressDirection addressDirection = EnumAddressDirection.None;
                if (address.Id == theVehicle.AseMovingGuide.ToAddressId)
                {
                    addressDirection = address.TransferPortDirection;
                    isEnd = true;
                }
                int headAngle = (int)address.VehicleHeadAngle;
                int speed = (int)mapSection.Speed;

                if (theVehicle.IsSimulation)
                {
                    AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                    aseMoveStatus.AseMoveState = isEnd ? EnumAseMoveState.Idle : EnumAseMoveState.Working;
                    aseMoveStatus.LastAddress = address;
                    aseMoveStatus.LastMapPosition = address.Position;
                    aseMoveStatus.LastSection = mapSection;
                    aseMoveStatus.HeadDirection = headAngle;
                    aseMoveStatus.Speed = speed;
                    aseMoveStatus.IsMoveEnd = isEnd;
                    FakeReserveOkAseMoveStatus.Enqueue(aseMoveStatus);
                }
                else
                {
                    EnumAseMoveCommandIsEnd moveCommandIsEnd = isEnd ? EnumAseMoveCommandIsEnd.End : EnumAseMoveCommandIsEnd.None;
                    asePackage.aseMoveControl.PartMove(addressDirection, address.Position, headAngle, speed, moveCommandIsEnd);
                }

                OnMessageShowEvent?.Invoke(this, $"通知MoveControl延攬通行權{mapSection.Id}成功，下一個可行終點為[{address.Id}]({Convert.ToInt32(address.Position.X)},{Convert.ToInt32(address.Position.Y)})。");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void SimulationArrival(bool isEnd, MapPosition position, int theta, int speed)
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            aseMoveStatus.LastMapPosition = position;
            aseMoveStatus.HeadDirection = theta;
            aseMoveStatus.Speed = speed;
            theVehicle.AseMoveStatus = aseMoveStatus;

            if (isEnd)
            {
                SpinWait.SpinUntil(() => false, 1000);
                AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
            }
        }

        public bool IsMoveStep() => GetCurrentTransferStepType() == EnumTransferStepType.Move || GetCurrentTransferStepType() == EnumTransferStepType.MoveToCharger;

        public void AseMoveControl_OnMoveFinished(object sender, EnumMoveComplete status)
        {
            try
            {
                theVehicle.AseMoveStatus.IsMoveEnd = true;
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

                if (IsAvoidMove)
                {
                    var endAddress = theMapInfo.addressMap[theVehicle.AseMovingGuide.ToAddressId];
                    UpdateVehiclePositionAfterArrival(endAddress);
                    ArrivalStartCharge(endAddress);
                    theVehicle.AseMovingGuide.IsAvoidComplete = true;
                    agvcConnector.AvoidComplete();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : 避車移動完成");
                }
                else
                {
                    MoveCmdInfo moveCmdInfo = (MoveCmdInfo)GetCurTransferStep();
                    UpdateVehiclePositionAfterArrival(moveCmdInfo.EndAddress);
                    ArrivalStartCharge(moveCmdInfo.EndAddress);
                    agvcConnector.MoveArrival();
                    if (IsNextTransferStepIdle())
                    {
                        TransferComplete(moveCmdInfo.CmdId);
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
                asePackage.SetTransferCommandInfoRequest();
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
                var robotCmdInfo = (RobotCommand)GetCurTransferStep();
                if (robotCmdInfo.SlotNumber != slotNumber) return;
                if (robotCmdInfo.GetTransferStepType() == EnumTransferStepType.Unload) return;

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
                    if (theVehicle.IsSimulation)
                    {
                        SimulationUnload(unloadCmd, aseCarrierSlotStatus);
                    }
                    else
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
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot放貨中, [方向{unloadCmd.PioDirection}][儲位={unloadCmd.SlotNumber}][放貨站={unloadCmd.PortAddressId}]");

                    }
                    batteryLog.LoadUnloadCount++;
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
                    if (theVehicle.IsSimulation)
                    {
                        SimulationLoad(loadCmd, aseCarrierSlotStatus);
                    }
                    else
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
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot取貨中, [方向={loadCmd.PioDirection}][儲位={loadCmd.SlotNumber}][取貨站={loadCmd.PortAddressId}]");
                    }

                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
            }
        }

        private void SimulationLoad(LoadCmdInfo loadCmd, AseCarrierSlotStatus aseCarrierSlotStatus)
        {
            agvcConnector.Loading(loadCmd.CmdId);
            PublishOnDoTransferStepEvent(loadCmd);
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot取貨中, [方向={loadCmd.PioDirection}][編號={loadCmd.SlotNumber}]");
            SpinWait.SpinUntil(() => false, 3000);
            aseCarrierSlotStatus.CarrierId = loadCmd.CassetteId;
            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
            AseRobotControl_OnReadCarrierIdFinishEvent(this, aseCarrierSlotStatus.SlotNumber);
            SpinWait.SpinUntil(() => false, 2000);
            AseRobotContorl_OnRobotCommandFinishEvent(this, GetCurTransferStep());
        }

        private void SimulationUnload(UnloadCmdInfo unloadCmd, AseCarrierSlotStatus aseCarrierSlotStatus)
        {
            agvcConnector.Unloading(unloadCmd.CmdId);
            PublishOnDoTransferStepEvent(unloadCmd);
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot放貨中, [方向{unloadCmd.PioDirection}][編號={unloadCmd.SlotNumber}]");
            SpinWait.SpinUntil(() => false, 3000);
            aseCarrierSlotStatus.CarrierId = "";
            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
            SpinWait.SpinUntil(() => false, 2000);
            AseRobotContorl_OnRobotCommandFinishEvent(this, GetCurTransferStep());
        }

        private void AgvcConnector_OnCassetteIdReadReplyAbortCommandEvent(object sender, string e)
        {
            try
            {
                AbortCommand(e, theVehicle.AgvcTransCmdBuffer[e].CompleteStatus);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AbortCommand(string cmdId, CompleteStatus completeStatus)
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
            foreach (var agvcTransCmd in theVehicle.AgvcTransCmdBuffer.Values.ToList())
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
            MapAddress portAddress = theMapInfo.addressMap[robotCommand.PortAddressId];
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

        public void SetupAseMovingGuideMovingSections()
        {
            try
            {
                StopCharge();
                theVehicle.AseMovingGuide.MovingSections = new List<MapSection>();
                for (int i = 0; i < theVehicle.AseMovingGuide.GuideSectionIds.Count; i++)
                {
                    MapSection mapSection = new MapSection();
                    string sectionId = theVehicle.AseMovingGuide.GuideSectionIds[i];
                    string addressId = theVehicle.AseMovingGuide.GuideAddressIds[i + 1];
                    if (!theMapInfo.sectionMap.ContainsKey(sectionId))
                    {
                        throw new Exception($"Map info has no this section ID.[{sectionId}]");
                    }
                    else if (!theMapInfo.addressMap.ContainsKey(addressId))
                    {
                        throw new Exception($"Map info has no this address ID.[{addressId}]");
                    }

                    mapSection = theMapInfo.sectionMap[sectionId];
                    mapSection.CmdDirection = addressId == mapSection.TailAddress.Id ? EnumCommandDirection.Forward : EnumCommandDirection.Backward;
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
                            theVehicle.AseMovingGuide.MovingSectionsIndex++;
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
            aseMoveStatus.LastSection = theMapInfo.sectionMap[section.Id];
            aseMoveStatus.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(theVehicle.AseMoveStatus.LastMapPosition, aseMoveStatus.LastSection.HeadAddress.Position);
            theVehicle.AseMoveStatus = aseMoveStatus;
        }

        private void FitVehicalLocationAndMoveCmd()
        {
            AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);
            MapSection section = aseMovingGuide.MovingSections[aseMovingGuide.MovingSectionsIndex];
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            aseMoveStatus.LastSection = section;
            if (section.CmdDirection == EnumCommandDirection.Forward)
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

        private void UpdateVehiclePositionAfterArrival(MapAddress lastAddress)
        {
            try
            {
                MapSection lastSection = new MapSection();
                if (theVehicle.AseMovingGuide.MovingSections.Count > 0)
                {
                    var lastMoveSection = theVehicle.AseMovingGuide.MovingSections.FindLast(x => x.Id != null);
                    lastSection = theMapInfo.sectionMap[lastMoveSection.Id];
                    lastSection.CmdDirection = lastMoveSection.CmdDirection;
                }
                else
                {
                    lastSection = theVehicle.AseMoveStatus.LastSection;
                }


                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                aseMoveStatus.LastAddress = lastAddress;
                aseMoveStatus.LastMapPosition = lastAddress.Position;
                aseMoveStatus.LastSection = lastSection;
                aseMoveStatus.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(lastAddress.Position, lastSection.HeadAddress.Position);
                theVehicle.AseMoveStatus = aseMoveStatus;

                var msg = $"車輛抵達終點站{lastAddress.Id}，位置更新。";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"車輛抵達終點站{lastAddress.Id}，位置更新失敗。");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void UpdateVehiclePositionManual()
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
            foreach (MapSection mapSection in theMapInfo.sectionMap.Values)
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
                foreach (MapAddress mapAddress in theMapInfo.addressMap.Values)
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

                if (address.IsCharger())
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
                    if (theVehicle.IsSimulation)
                    {
                        theVehicle.IsCharging = true;
                    }

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
                        batteryLog.ChargeCount++;
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
                if (address.IsCharger() && mapHandler.IsPositionInThisAddress(pos, address.Position))
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
                    if (theVehicle.IsSimulation)
                    {
                        theVehicle.IsCharging = true;
                    }

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
                        batteryLog.ChargeCount++;
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
                if (address.IsCharger())
                {
                    agvcConnector.ChargHandshaking();

                    if (theVehicle.IsSimulation)
                    {
                        theVehicle.IsCharging = false;
                    }

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

        private void AgvcConnector_OnStopClearAndResetEvent(object sender, EventArgs e)
        {
            StopClearAndReset();
        }

        public void IntoAuto()
        {
            try
            {
                ResumeTransfer();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void StopClearAndReset()
        {
            try
            {
                PauseTransfer();
                agvcConnector.ClearAllReserve();
                theVehicle.AseMovingGuide = new AseMovingGuide();
                StopVehicle();
                AbortAllAgvcTransCmdInBuffer();

                if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.On)
                {
                    theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                    agvcConnector.StatusChangeReport();
                }

                if (theVehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading || theVehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
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

        private void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            asePackage.aseBuzzerControl.SetAlarmCode(alarm.Id, true);
        }

        public void SetupVehicleSoc(double percentage)
        {
            asePackage.aseBatteryControl.SetPercentage(percentage);
        }

        private void AgvcConnector_OnRenameCassetteIdEvent(object sender, AseCarrierSlotStatus e)
        {
            try
            {
                AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer.Values.First(x => x.SlotNumber == e.SlotNumber);
                agvcTransCmd.CassetteId = e.CarrierId;
                theVehicle.AgvcTransCmdBuffer[agvcTransCmd.CommandId] = agvcTransCmd;

                if (transferSteps.Count > 0)
                {
                    foreach (var transferStep in transferSteps)
                    {
                        if (transferStep.CmdId == agvcTransCmd.CommandId && IsRobCommand(transferStep))
                        {
                            ((RobotCommand)transferStep).CassetteId = e.CarrierId;
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
                PauseTransfer();
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
                PauseTransfer();
                agvcConnector.CancelAbortReply(iSeqNum, 0, receive);

                string abortCmdId = receive.CmdID;
                var step = GetCurTransferStep();
                if (step.CmdId == abortCmdId)
                {
                    if (IsMoveStep())
                    {
                        theVehicle.AseMovingGuide = new AseMovingGuide();
                    }
                    else
                    {
                        asePackage.aseRobotControl.ClearRobotCommand();
                    }
                }

                AbortCommand(abortCmdId, GetCompleteStatusFromCancelRequest(receive.CancelAction));
                ResumeTransfer();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private CompleteStatus GetCompleteStatusFromCancelRequest(CancelActionType cancelAction)
        {
            switch (cancelAction)
            {
                case CancelActionType.CmdCancel:
                    return CompleteStatus.Cancel;
                case CancelActionType.CmdCancelIdMismatch:
                    return CompleteStatus.IdmisMatch;
                case CancelActionType.CmdCancelIdReadFailed:
                    return CompleteStatus.IdreadFailed;
                case CancelActionType.CmdNone:
                case CancelActionType.CmdEms:
                case CancelActionType.CmdAbort:
                default:
                    return CompleteStatus.Abort;
            }
        }




        #region Save/Load Configs

        public void LoadMainFlowConfig()
        {

            mainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(@"MainFlow.xml");
        }

        public void SetMainFlowConfig(MainFlowConfig mainFlowConfig)
        {
            this.mainFlowConfig = mainFlowConfig;
            xmlHandler.WriteXml(mainFlowConfig, @"MainFlow.xml");
        }

        public void LoadAgvcConnectorConfig()
        {
            agvcConnectorConfig = xmlHandler.ReadXml<AgvcConnectorConfig>(@"AgvcConnector.xml");
        }

        public void SetAgvcConnectorConfig(AgvcConnectorConfig agvcConnectorConfig)
        {
            this.agvcConnectorConfig = agvcConnectorConfig;
            xmlHandler.WriteXml(this.agvcConnectorConfig, @"AgvcConnector.xml");
        }

        #endregion

        #region AsePackage

        private void AseBatteryControl_OnBatteryPercentageChangeEvent(object sender, double batteryPercentage)
        {
            try
            {
                batteryLog.InitialSoc = (int)batteryPercentage;
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

        private void AsePackage_OnMessageShowEvent(object sender, string e)
        {
            OnMessageShowEvent?.Invoke(this, e);
        }

        #endregion

        public void ResetBatteryLog()
        {
            BatteryLog tempBatteryLog = new BatteryLog();
            tempBatteryLog.ResetTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff");
            tempBatteryLog.InitialSoc = batteryLog.InitialSoc;
            batteryLog = tempBatteryLog;
            //TODO: AgvcConnector
        }

        #region Log

        private void LogException(string classMethodName, string exMsg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("Error", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", exMsg));
            }
            catch (Exception)
            {
            }
        }

        private void LogDebug(string classMethodName, string msg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", msg));
            }
            catch (Exception)
            {
            }
        }

        private void LogMsgHandler(object sender, LogFormat e)
        {
            mirleLogger.Log(e);
        }

        #endregion



    }
}
