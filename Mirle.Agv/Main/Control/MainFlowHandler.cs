using ClsMCProtocol;
using Google.Protobuf.Collections;
using Mirle.Agv.Controller.Handler.TransCmdsSteps;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferSteps;
using Mirle.Agv.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TcpIpClientSample;

namespace Mirle.Agv.Controller
{
    public class MainFlowHandler
    {
        #region Configs
        private MiddlerConfig middlerConfig;
        private MainFlowConfig mainFlowConfig;
        private MapConfig mapConfig;
        private AlarmConfig alarmConfig;
        public BatteryLog batteryLog;
        #endregion

        #region TransCmds
        private List<TransferStep> transferSteps = new List<TransferStep>();
        private List<TransferStep> lastTransferSteps = new List<TransferStep>();

        public bool GoNextTransferStep { get; set; }
        public int TransferStepsIndex { get; private set; }
        public bool IsOverrideMove { get; set; }
        public bool IsAvoidMove { get; set; }

        public bool IsReportingPosition { get; set; }
        public bool IsReserveMechanism { get; set; } = true;
        private ITransferStatus transferStatus;
        public AgvcTransCmd agvcTransCmd = new AgvcTransCmd();
        private AgvcTransCmd lastAgvcTransCmd = new AgvcTransCmd();
        public MapSection SectionHasFoundPosition { get; set; } = new MapSection();
        public VehicleLocation CmdEndVehiclePosition { get; set; } = new VehicleLocation();
        #endregion

        #region Agent

        private MiddleAgent middleAgent;
        private PlcAgent plcAgent;
        private LoggerAgent loggerAgent;

        #endregion

        #region Handler

        private AlarmHandler alarmHandler;
        private MapHandler mapHandler;
        private MoveControlHandler moveControlHandler;

        #endregion

        #region Threads
        private Thread thdVisitTransferSteps;
        private ManualResetEvent visitTransferStepsShutdownEvent = new ManualResetEvent(false);

        private ManualResetEvent visitTransferStepsPauseEvent = new ManualResetEvent(true);
        private EnumThreadStatus visitTransferStepsStatus = EnumThreadStatus.None;
        public EnumThreadStatus VisitTransferStepsStatus
        {
            get { return visitTransferStepsStatus; }
            private set
            {
                visitTransferStepsStatus = value;
                theVehicle.VisitTransferStepsStatus = value;
            }
        }
        public EnumThreadStatus VisitTransferStepsStatusBeforePause { get; private set; } = EnumThreadStatus.None;

        private Thread thdTrackPosition;
        private ManualResetEvent trackPositionShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent trackPositionPauseEvent = new ManualResetEvent(true);
        private EnumThreadStatus trackPositionStatus = EnumThreadStatus.None;
        public EnumThreadStatus TrackPositionStatus
        {
            get { return trackPositionStatus; }
            private set
            {
                trackPositionStatus = value;
                theVehicle.TrackPositionStatus = value;
            }
        }

        public EnumThreadStatus PreTrackPositionStatus { get; private set; } = EnumThreadStatus.None;

        private Thread thdWatchLowPower;
        private ManualResetEvent watchLowPowerShutdownEvent = new ManualResetEvent(false);

        private ManualResetEvent watchLowPowerPauseEvent = new ManualResetEvent(true);
        private EnumThreadStatus watchLowPowerStatus = EnumThreadStatus.None;
        public EnumThreadStatus WatchLowPowerStatus
        {
            get { return watchLowPowerStatus; }
            private set
            {
                watchLowPowerStatus = value;
                theVehicle.WatchLowPowerStatus = value;
            }
        }
        public EnumThreadStatus PreWatchLowPowerStatus { get; private set; } = EnumThreadStatus.None;
        #endregion

        #region Events
        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;
        public event EventHandler<string> OnMessageShowEvent;
        public event EventHandler<MoveCmdInfo> OnPrepareForAskingReserveEvent;
        public event EventHandler OnMoveArrivalEvent;
        public event EventHandler<AgvcTransCmd> OnTransferCommandCheckedEvent;
        public event EventHandler<AgvcOverrideCmd> OnOverrideCommandCheckedEvent;
        public event EventHandler<AgvcMoveCmd> OnAvoidCommandCheckedEvent;
        public event EventHandler<TransferStep> OnDoTransferStepEvent;
        #endregion

        #region Models
        public Vehicle theVehicle;
        private bool isIniOk;
        public MapInfo TheMapInfo { get; private set; } = new MapInfo();
        private MCProtocol mcProtocol;
        public ushort ForkCommandNumber { get; set; } = 0;
        public PlcForkCommand PlcForkLoadCommand { get; set; }
        public PlcForkCommand PlcForkUnloadCommand { get; set; }
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
            //ConfigsInitial();
            XmlInitial();
            LoggersInitial();
            ControllersInitial();
            VehicleInitial();
            EventInitial();
            SetTransCmdsStep(new Idle());

            VehicleLocationInitial();

            if (isIniOk)
            {
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "全部"));
            }
        }

        private void XmlInitial()
        {
            try
            {
                XmlHandler xmlHandler = new XmlHandler();

                mainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(@"D:\AgvConfigs\MainFlow.xml");
                LoggerAgent.LogConfigPath = mainFlowConfig.LogConfigPath;
                mapConfig = xmlHandler.ReadXml<MapConfig>(@"D:\AgvConfigs\Map.xml");
                middlerConfig = xmlHandler.ReadXml<MiddlerConfig>(@"D:\AgvConfigs\Middler.xml");
                alarmConfig = xmlHandler.ReadXml<AlarmConfig>(@"D:\AgvConfigs\Alarm.xml");
                //GetInitialSoc("BatteryPercentage.log");
                batteryLog = xmlHandler.ReadXml<BatteryLog>(@"D:\AgvConfigs\BatteryLog.xml");
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
                loggerAgent = LoggerAgent.Instance;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "紀錄器"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "紀錄器"));
            }
        }

        private void ControllersInitial()
        {
            try
            {
                alarmHandler = new AlarmHandler(this);
                mapHandler = new MapHandler(mapConfig);
                TheMapInfo = mapHandler.TheMapInfo;
                mcProtocol = new MCProtocol();
                mcProtocol.Name = "MCProtocol";
                plcAgent = new PlcAgent(mcProtocol, alarmHandler);
                moveControlHandler = new MoveControlHandler(TheMapInfo, alarmHandler, plcAgent);
                middleAgent = new MiddleAgent(this);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "控制層"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "控制層"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void VehicleInitial()
        {
            try
            {
                theVehicle = Vehicle.Instance;
                theVehicle.CurAgvcTransCmd = agvcTransCmd;
                theVehicle.VehicleLocation.RealPositionRangeMm = mainFlowConfig.RealPositionRangeMm;
                theVehicle.TheMapInfo = TheMapInfo;
                theVehicle.ThdMiddleAgent = middleAgent;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "台車"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "台車"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'mapHandler
                middleAgent.OnInstallTransferCommandEvent += MiddleAgent_OnInstallTransferCommandEvent;
                middleAgent.OnOverrideCommandEvent += MiddleAgent_OnOverrideCommandEvent;
                middleAgent.OnAvoideRequestEvent += MiddleAgent_OnAvoideRequestEvent;

                //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                moveControlHandler.OnMoveFinished += MoveControlHandler_OnMoveFinished;
                moveControlHandler.OnRetryMoveFinished += MoveControlHandler_OnRetryMoveFinished;

                //來自PlcAgent的取放貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                plcAgent.OnForkCommandInterlockErrorEvent += PlcAgent_OnForkCommandInterlockErrorEvent;
                plcAgent.OnForkCommandFinishEvent += PlcAgent_OnForkCommandFinishEvent;
                plcAgent.OnForkCommandErrorEvent += PlcAgent_OnForkCommandErrorEvent;

                //來自PlcBattery的電量改變訊息，通知middleAgent
                plcAgent.OnBatteryPercentageChangeEvent += middleAgent.PlcAgent_OnBatteryPercentageChangeEvent;
                plcAgent.OnBatteryPercentageChangeEvent += PlcAgent_OnBatteryPercentageChangeEvent;

                //來自PlcBattery的CassetteId讀取訊息，通知middleAgent
                //plcAgent.OnCassetteIDReadFinishEvent += middleAgent.PlcAgent_OnCassetteIDReadFinishEvent;
                plcAgent.OnCassetteIDReadFinishEvent += PlcAgent_OnCassetteIDReadFinishEvent;

                //來自AlarmHandler的SetAlarm/ResetOneAlarm/ResetAllAlarm發生警告，通知MainFlow,middleAgent
                alarmHandler.OnSetAlarmEvent += AlarmHandler_OnSetAlarmEvent;
                alarmHandler.OnSetAlarmEvent += middleAgent.AlarmHandler_OnSetAlarmEvent;

                alarmHandler.OnPlcResetOneAlarmEvent += middleAgent.AlarmHandler_OnPlcResetOneAlarmEvent;

                alarmHandler.OnResetAllAlarmsEvent += AlarmHandler_OnResetAllAlarmsEvent;
                alarmHandler.OnResetAllAlarmsEvent += middleAgent.AlarmHandler_OnResetAllAlarmsEvent;



                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "事件"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "事件"));

                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void VehicleLocationInitial()
        {
            if (IsRealPositionEmpty())
            {
                try
                {
                    theVehicle.VehicleLocation.RealPosition = TheMapInfo.allMapAddresses.First(x => x.Key != "").Value.Position;
                }
                catch (Exception ex)
                {
                    theVehicle.VehicleLocation.RealPosition = new MapPosition();
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
            }
            StartTrackPosition();
            StartWatchLowPower();
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"讀取到的電量為{batteryLog.InitialSoc}"));

        }
        private bool IsRealPositionEmpty()
        {
            if (theVehicle.VehicleLocation.RealPosition == null)
            {
                return true;
            }

            if (theVehicle.VehicleLocation.RealPosition.X == 0 && theVehicle.VehicleLocation.RealPosition.Y == 0)
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
            PreVisitTransferSteps();
            Stopwatch sw = new Stopwatch();
            long total = 0;
            while (TransferStepsIndex < transferSteps.Count)
            {
                try
                {
                    sw.Restart();

                    #region Pause And Stop Check
                    visitTransferStepsPauseEvent.WaitOne(Timeout.Infinite);
                    if (visitTransferStepsShutdownEvent.WaitOne(0)) break;
                    #endregion

                    VisitTransferStepsStatus = EnumThreadStatus.Working;

                    if (GoNextTransferStep)
                    {
                        GoNextTransferStep = false;
                        DoTransfer();
                    }
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.VisitTransferStepsSleepTimeMs);
                    sw.Stop();
                    total += sw.ElapsedMilliseconds;
                }
            }

            //OnTransCmdsFinishedEvent(this, EnumCompleteStatus.TransferComplete);
            AfterVisitTransferSteps(total);
        }
        public void StartVisitTransferSteps()
        {
            visitTransferStepsPauseEvent.Set();
            visitTransferStepsShutdownEvent.Reset();
            thdVisitTransferSteps = new Thread(VisitTransferSteps);
            thdVisitTransferSteps.IsBackground = true;
            thdVisitTransferSteps.Start();
            VisitTransferStepsStatus = EnumThreadStatus.Start;

            var msg = $"MainFlow : 開始搬送流程, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        public void PauseVisitTransferSteps()
        {
            visitTransferStepsPauseEvent.Reset();
            if (agvcTransCmd.PauseStatus == VhStopSingle.StopSingleOff)
            {
                agvcTransCmd.PauseStatus = VhStopSingle.StopSingleOn;
                middleAgent.StatusChangeReport(MethodBase.GetCurrentMethod().Name);
            }
            VisitTransferStepsStatusBeforePause = VisitTransferStepsStatus;
            VisitTransferStepsStatus = EnumThreadStatus.Pause;

            var msg = $"MainFlow : 暫停搬送流程, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        public void ResumeVisitTransferSteps()
        {
            if (agvcTransCmd.PauseStatus == VhStopSingle.StopSingleOn)
            {
                agvcTransCmd.PauseStatus = VhStopSingle.StopSingleOff;
                middleAgent.StatusChangeReport(MethodBase.GetCurrentMethod().Name);
            }
            visitTransferStepsPauseEvent.Set();
            VisitTransferStepsStatus = VisitTransferStepsStatusBeforePause;
            var msg = $"MainFlow : 恢復搬送流程, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        public void StopVisitTransferSteps()
        {
            visitTransferStepsShutdownEvent.Set();
            visitTransferStepsPauseEvent.Set();
            if (VisitTransferStepsStatus != EnumThreadStatus.None)
            {
                VisitTransferStepsStatus = EnumThreadStatus.Stop;
            }

            var msg = $"MainFlow : 停止搬送流程, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        private void PreVisitTransferSteps()
        {
            IsMoveEnd = true;
            TransferStepsIndex = 0;
            GoNextTransferStep = true;

            var msg = $"MainFlow : 搬送流程 前處理, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }
        private void AfterVisitTransferSteps(long total)
        {
            if (agvcTransCmd.PauseStatus == VhStopSingle.StopSingleOn)
            {
                agvcTransCmd.PauseStatus = VhStopSingle.StopSingleOff;
                middleAgent.StatusChangeReport(MethodBase.GetCurrentMethod().Name);
            }

            middleAgent.TransferComplete(agvcTransCmd);

            VisitTransferStepsStatus = EnumThreadStatus.None;
            lastAgvcTransCmd = agvcTransCmd;
            agvcTransCmd = new AgvcTransCmd();
            lastTransferSteps = transferSteps;
            transferSteps = new List<TransferStep>();
            theVehicle.CurAgvcTransCmd = agvcTransCmd;
            GoNextTransferStep = false;
            SetTransCmdsStep(new Idle());
            middleAgent.NoCommand();
            IsMoveEnd = true;
            var msg = $"MainFlow : 搬送流程 後處理, [ThreadStatus={VisitTransferStepsStatus}][TotalSpendMs={total}]";
            OnMessageShowEvent?.Invoke(this, msg);
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
                            LowPowerStartCharge(theVehicle.VehicleLocation.LastAddress);
                        }
                    }
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            var msg = $"MainFlow : 開始監看自動充電, [Power={batterys.Percentage}][LowSocGap={batterys.PortAutoChargeLowSoc}]";
            OnMessageShowEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //     , msg));
        }
        public void PauseWatchLowPower()
        {
            watchLowPowerPauseEvent.Reset();
            PreWatchLowPowerStatus = WatchLowPowerStatus;
            WatchLowPowerStatus = EnumThreadStatus.Pause;
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            var msg = $"MainFlow : 暫停監看自動充電, [Power={batterys.Percentage}][LowSocGap={batterys.PortAutoChargeLowSoc}]";
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
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            var msg = $"MainFlow : 恢復監看自動充電, [Power={batterys.Percentage}][LowSocGap={batterys.PortAutoChargeLowSoc}]";
            OnMessageShowEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //    , msg));
        }
        public void StopWatchLowPower()
        {
            if (WatchLowPowerStatus != EnumThreadStatus.None)
            {
                WatchLowPowerStatus = EnumThreadStatus.Stop;
            }
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            var msg = $"MainFlow : 停止監看自動充電, [Power={batterys.Percentage}][LowSocGap={batterys.PortAutoChargeLowSoc}]";
            OnMessageShowEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //  , msg));

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
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            return batterys.Percentage <= batterys.PortAutoChargeLowSoc;
        }
        private bool IsHighPower()
        {
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            return batterys.Percentage >= batterys.PortAutoChargeHighSoc;
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

                    VehicleLocation vehicleLocation = theVehicle.VehicleLocation;
                    var position = theVehicle.VehicleLocation.RealPosition;
                    if (vehicleLocation.RealPosition == null) continue;
                    //if (IsVehlocStayInSameAddress(vehicleLocation)) continue;

                    if (theVehicle.AutoState == EnumAutoState.Auto)
                    {
                        if (transferSteps.Count > 0)
                        {
                            //有搬送命令時，比對當前Position與搬送路徑Sections計算LastSection/LastAddress/Distance                           
                            if (IsMoveStep())
                            {
                                MoveCmdInfo moveCmd = (MoveCmdInfo)GetCurTransferStep();
                                if (!IsMoveEnd)
                                {
                                    if (UpdateVehiclePositionInMovingStep(moveCmd, vehicleLocation))
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
                        UpdateVehiclePositionManual(vehicleLocation);
                    }
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.TrackPositionSleepTimeMs);
                }

                sw.Stop();
                if (sw.ElapsedMilliseconds > mainFlowConfig.ReportPositionIntervalMs)
                {
                    middleAgent.ReportAddressPass();
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
        private void MiddleAgent_OnInstallTransferCommandEvent(object sender, AgvcTransCmd agvcTransCmd)
        {
            var msg = $"MainFlow : 收到{agvcTransCmd.CommandType}命令{agvcTransCmd.CommandId}。";
            OnMessageShowEvent?.Invoke(this, msg);

            #region 檢查搬送路徑
            try
            {
                if (!IsAgvcTransferCommandEmpty())
                {
                    var reason = $"Agv already have a [{agvcTransCmd.CommandType}] command [{agvcTransCmd.CommandId}].";
                    RejectTransferCommandAndResume(000001, reason, agvcTransCmd);
                    return;
                }

                if (!theVehicle.ThePlcVehicle.Robot.ForkHome)
                {
                    var reason = $"Fork is not at home.";
                    RejectTransferCommandAndResume(000027, reason, agvcTransCmd);
                    return;
                }

                if (IsVehicleAlreadyHaveCstCannotLoad(agvcTransCmd.CommandType))
                {
                    var reason = $"Agv already have a cst [{theVehicle.ThePlcVehicle.CassetteId}] cannot load.";
                    RejectTransferCommandAndResume(000016, reason, agvcTransCmd);
                    return;
                }

                if (IsVehicleHaveNoCstCannotUnload(agvcTransCmd.CommandType))
                {
                    var reason = $"Agv have no cst cannot unload.[loading={theVehicle.ThePlcVehicle.Loading}]";
                    RejectTransferCommandAndResume(000017, reason, agvcTransCmd);
                    return;
                }

                if (!IsAgvcCommandMatchTheMap(agvcTransCmd))
                {
                    var reason = $"Guide sections and address are not match the map.";
                    RejectTransferCommandAndResume(000018, reason, agvcTransCmd);
                    return;
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));

                middleAgent.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetActiveType(), agvcTransCmd.SeqNum, 1, "Guide sections and address are not match the map.");
                return;
            }
            #endregion

            #region 搬送路徑生成
            try
            {
                agvcTransCmd.ForkNgRetryTimes = mainFlowConfig.ForkNgRetryTimes;
                this.agvcTransCmd = agvcTransCmd;
                theVehicle.CurAgvcTransCmd = agvcTransCmd;
                SetupTransferSteps();
                transferSteps.Add(new EmptyTransferStep());
                //開始尋訪 trasnferSteps as List<TrasnferStep> 裡的每一步MoveCmdInfo/LoadCmdInfo/UnloadCmdInfo
                theVehicle.ThePlcVehicle.FakeCassetteId = agvcTransCmd.CassetteId;
                middleAgent.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetActiveType(), agvcTransCmd.SeqNum, 0, "");
                StartVisitTransferSteps();
                var okMsg = $"MainFlow : 接受 {agvcTransCmd.CommandType}命令{agvcTransCmd.CommandId} 確認。";
                OnMessageShowEvent?.Invoke(this, okMsg);
                OnTransferCommandCheckedEvent?.Invoke(this, agvcTransCmd);
            }
            catch (Exception ex)
            {
                middleAgent.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetActiveType(), agvcTransCmd.SeqNum, 1, "");
                var ngMsg = $"MainFlow : 收到 {agvcTransCmd.CommandType}命令{agvcTransCmd.CommandId} 處理失敗。";
                OnMessageShowEvent?.Invoke(this, ngMsg);

                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
            #endregion
        }

        private bool IsAgvcCommandMatchTheMap(AgvcTransCmd agvcTransCmd)
        {
            var curPos = theVehicle.VehicleLocation.RealPosition;
            switch (agvcTransCmd.CommandType)
            {
                case EnumAgvcTransCommandType.Move:
                case EnumAgvcTransCommandType.MoveToCharger:
                case EnumAgvcTransCommandType.Unload:
                    return IsAgvcCommandMatchTheMap(curPos, agvcTransCmd.ToUnloadSectionIds, agvcTransCmd.ToUnloadAddressIds, agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandType);
                case EnumAgvcTransCommandType.Load:
                    return IsAgvcCommandMatchTheMap(curPos, agvcTransCmd.ToLoadSectionIds, agvcTransCmd.ToLoadAddressIds, agvcTransCmd.LoadAddressId, agvcTransCmd.CommandType);
                case EnumAgvcTransCommandType.LoadUnload:
                    var canMoveToLoad = IsAgvcCommandMatchTheMap(curPos, agvcTransCmd.ToLoadSectionIds, agvcTransCmd.ToLoadAddressIds, agvcTransCmd.LoadAddressId, agvcTransCmd.CommandType);
                    var loadPos = TheMapInfo.allMapAddresses[agvcTransCmd.LoadAddressId].Position;
                    var canMoveToUnLoad = IsAgvcCommandMatchTheMap(loadPos, agvcTransCmd.ToUnloadSectionIds, agvcTransCmd.ToUnloadAddressIds, agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandType);
                    return canMoveToLoad && canMoveToUnLoad;
                case EnumAgvcTransCommandType.Override:
                    break;
                case EnumAgvcTransCommandType.Else:
                    break;
                default:
                    break;
            }

            return true;
        }

        private bool IsAgvcCommandMatchTheMap(MapPosition moveFirstPosition, List<string> toSectionIds, List<string> toAddressIds, string endAddressId, EnumAgvcTransCommandType commandType)
        {
            if (toSectionIds.Count > 0)
            {
                return CheckSectionIdsAndAddressIds(toSectionIds, toAddressIds, endAddressId, commandType);
            }

            return true;
        }

        private bool CheckInSituSectionIdAndAddressId(MapPosition insituPosition, List<string> sectionIds, List<string> addressIds, string lastAddress, EnumAgvcTransCommandType type)
        {
            //測 AddressIds 為空
            if (addressIds.Count > 0)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"MainFlow : CheckInSituSectionIdAndAddressId +++FAIL+++, [{type}][InSitu] Address is not empty."));
                return false;
            }

            //測 終點存在於圖資
            if (!TheMapInfo.allMapAddresses.ContainsKey(lastAddress))
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"MainFlow : CheckInSituSectionIdAndAddressId +++FAIL+++, [{type}][InSitu] Address {lastAddress} is not in the map."));
                return false;
            }

            //測 現在還在終點
            if (!mapHandler.IsPositionInThisAddress(insituPosition, TheMapInfo.allMapAddresses[lastAddress].Position))
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"MainFlow : CheckInSituSectionIdAndAddressId +++FAIL+++, [{type}][InSitu] RealPos is not at {lastAddress}."));
                return false;
            }

            return true;
        }

        private bool CheckSectionIdsAndAddressIds(List<string> sectionIds, List<string> addressIds, string lastAddressId, EnumAgvcTransCommandType type)
        {
            //測sectionIds存在
            foreach (var id in sectionIds)
            {
                if (!TheMapInfo.allMapSections.ContainsKey(id))
                {
                    var msg = $"MainFlow : [{type}]命令檢查失敗，地圖沒有ID為[{id}]的路段。";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                      , msg));
                    return false;
                }
            }

            //測addressIds存在
            foreach (var id in addressIds)
            {
                if (!TheMapInfo.allMapAddresses.ContainsKey(id))
                {
                    var msg = $"MainFlow : [{type}]命令檢查失敗，地圖沒有ID為[{id}]的站點。";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                      , msg));
                    return false;
                }
            }

            //測AddressId 屬於 SectionId 內
            for (int i = 0; i < sectionIds.Count; i++)
            {
                var section = TheMapInfo.allMapSections[sectionIds[i]];
                if (!IsAddressIdInMapSection(addressIds[i], section))
                {
                    var msg = $"MainFlow : [{type}]命令檢查失敗，第[{i + 1}]個站點[{addressIds[i]}]不在第[{i + 1}]個路段[{section.Id}]內。";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
                    return false;
                }

                if (!IsAddressIdInMapSection(addressIds[i + 1], section))
                {
                    var msg = $"MainFlow : [{type}]命令檢查失敗，第[{i + 2}]個站點[{addressIds[i + 1]}]不在第[{i + 1}]個路段[{section.Id}]內。";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
                    return false;
                }
            }

            //測相鄰Section共有Address
            for (int i = 1; i < addressIds.Count - 1; i++)
            {
                var preSection = TheMapInfo.allMapSections[sectionIds[i - 1]];
                if (!IsAddressIdInMapSection(addressIds[i], preSection))
                {
                    var msg = $"MainFlow : [{type}]命令檢查失敗，第[{i + 1}]個站點[{addressIds[i]}]不在第[{i}]個路段[{preSection.Id}]內。";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
                    return false;
                }

                var nextSection = TheMapInfo.allMapSections[sectionIds[i]];
                if (!IsAddressIdInMapSection(addressIds[i], nextSection))
                {
                    var msg = $"MainFlow : [{type}]命令檢查失敗，第[{i + 1}]個站點[{addressIds[i]}]不在第[{i + 1}]個路段[{nextSection.Id}]內。";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
                    return false;
                }
            }

            //測終點AddressId 屬於 最後一個Section
            var lastSection = TheMapInfo.allMapSections[sectionIds[sectionIds.Count - 1]];
            if (!IsAddressIdInMapSection(lastAddressId, lastSection))
            {
                var msg = $"MainFlow : [{type}]命令檢查失敗，最後一個站點[{lastAddressId}]不在最後一個路段[{lastSection.Id}]內。";
                OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , msg));
                return false;
            }

            return true;
        }

        private bool IsAddressIdInMapSection(string addressId, MapSection section)
        {
            return section.InsideAddresses.FindIndex(x => x.Id == addressId) > -1;
        }

        private bool IsVehicleHaveNoCstCannotUnload(EnumAgvcTransCommandType commandTyp)
        {
            return commandTyp == EnumAgvcTransCommandType.Unload && theVehicle.ThePlcVehicle.CassetteId == "";
        }
        private bool IsVehicleAlreadyHaveCstCannotLoad(EnumAgvcTransCommandType commandTyp)
        {
            return (commandTyp == EnumAgvcTransCommandType.Load || commandTyp == EnumAgvcTransCommandType.LoadUnload) && theVehicle.ThePlcVehicle.CassetteId != "";
        }

        private void RejectTransferCommandAndResume2(int alarmCode, string reason, AgvcTransCmd agvcTransferCmd)
        {
            try
            {
                alarmHandler.SetAlarm(alarmCode);
                middleAgent.ReplyTransferCommand(agvcTransferCmd.CommandId, agvcTransferCmd.GetActiveType(), agvcTransferCmd.SeqNum, 1, reason);
                reason = $"MainFlow : Reject [{agvcTransferCmd.CommandType}] Command, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                //       , reason));
                if (VisitTransferStepsStatus == EnumThreadStatus.Pause)
                {
                    ResumeVisitTransferSteps();
                    middleAgent.ResumeAskReserve();
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void MiddleAgent_OnOverrideCommandEvent(object sender, AgvcOverrideCmd agvcOverrideCmd)
        {
            var msg = $"MainFlow : 收到[替代路徑]命令[{agvcOverrideCmd.CommandId}]，開始檢查。";
            OnMessageShowEvent?.Invoke(this, msg);

            #region 替代路徑檢查
            try
            {
                middleAgent.PauseAskReserve();

                if (IsAgvcTransferCommandEmpty())
                {
                    var reason = "車輛沒有搬送命令可以執行替代路徑";
                    RejectOverrideCommandAndResume(000019, reason, agvcOverrideCmd);
                    return;
                }

                if (!IsMoveStep())
                {
                    var reason = "車輛不在移動流程，無法執行替代路徑";
                    RejectOverrideCommandAndResume(000020, reason, agvcOverrideCmd);
                    return;
                }

                if (!IsMoveStopByNoReserve() && !agvcTransCmd.IsAvoidComplete)
                {
                    var reason = $"車輛尚未停妥，拒絕執行替代路徑";
                    RejectOverrideCommandAndResume(000021, reason, agvcOverrideCmd);
                    return;
                }


                if (IsNextTransferStepUnload())
                {
                    if (!this.agvcTransCmd.UnloadAddressId.Equals(agvcOverrideCmd.UnloadAddressId))
                    {
                        var reason = $"替代路徑放貨站點[{agvcOverrideCmd.UnloadAddressId}]與原路徑放貨站點[{agvcTransCmd.UnloadAddressId}]不合。";
                        RejectOverrideCommandAndResume(000022, reason, agvcOverrideCmd);
                        return;
                    }

                    if (agvcOverrideCmd.ToUnloadSectionIds.Count == 0)
                    {
                        var reason = "替代路徑清單放貨段為空。";
                        RejectOverrideCommandAndResume(000024, reason, agvcOverrideCmd);
                        return;
                    }

                    if (!IsOverrideCommandMatchTheMapToUnload(agvcOverrideCmd))
                    {
                        var reason = "替代路徑放貨段站點與路段不合圖資";
                        RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
                        return;
                    }
                }
                else if (IsNextTransferStepLoad())
                {
                    if (IsCurCmdTypeLoadUnload())
                    {
                        if (!this.agvcTransCmd.LoadAddressId.Equals(agvcOverrideCmd.LoadAddressId))
                        {
                            var reason = $"替代路徑取貨站點[{agvcOverrideCmd.LoadAddressId}]與原路徑取貨站點[{agvcTransCmd.LoadAddressId}]不合。";
                            RejectOverrideCommandAndResume(000023, reason, agvcOverrideCmd);
                            return;
                        }

                        if (!this.agvcTransCmd.UnloadAddressId.Equals(agvcOverrideCmd.UnloadAddressId))
                        {
                            var reason = $"替代路徑放貨站點[{agvcOverrideCmd.UnloadAddressId}]與原路徑放貨站點[{agvcTransCmd.UnloadAddressId}]不合。";
                            RejectOverrideCommandAndResume(000022, reason, agvcOverrideCmd);
                            return;
                        }

                        if (agvcOverrideCmd.ToLoadSectionIds.Count == 0)
                        {
                            var reason = "替代路徑清單取貨段為空。";
                            RejectOverrideCommandAndResume(000025, reason, agvcOverrideCmd);
                            return;
                        }

                        if (agvcOverrideCmd.ToUnloadSectionIds.Count == 0)
                        {
                            var reason = "替代路徑清單放貨段為空。";
                            RejectOverrideCommandAndResume(000024, reason, agvcOverrideCmd);
                            return;
                        }

                        if (!IsOverrideCommandMatchTheMapToLoad(agvcOverrideCmd))
                        {
                            var reason = "替代路徑取貨段站點與路段不合圖資";
                            RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
                            return;
                        }

                        if (!IsOverrideCommandMatchTheMapToNextUnload(agvcOverrideCmd))
                        {
                            var reason = "替代路徑放貨段站點與路段不合圖資";
                            RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
                            return;
                        }
                    }
                    else
                    {
                        if (!this.agvcTransCmd.LoadAddressId.Equals(agvcOverrideCmd.LoadAddressId))
                        {
                            var reason = $"替代路徑取貨站點[{agvcOverrideCmd.LoadAddressId}]與原路徑取貨站點[{agvcTransCmd.LoadAddressId}]不合。";
                            RejectOverrideCommandAndResume(000023, reason, agvcOverrideCmd);
                            return;
                        }

                        if (agvcOverrideCmd.ToLoadSectionIds.Count == 0)
                        {
                            var reason = "替代路徑清單取貨段為空。";
                            RejectOverrideCommandAndResume(000025, reason, agvcOverrideCmd);
                            return;
                        }

                        if (!IsOverrideCommandMatchTheMapToLoad(agvcOverrideCmd))
                        {
                            var reason = "替代路徑取貨段站點與路段不合圖資";
                            RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
                            return;
                        }
                    }
                }
                else
                {
                    //Move or MoveToCharger
                    if (!agvcTransCmd.UnloadAddressId.Equals(agvcOverrideCmd.UnloadAddressId))
                    {
                        var reason = $"替代路徑移動終點[{agvcOverrideCmd.UnloadAddressId}]與原路徑移動終點[{agvcTransCmd.UnloadAddressId}]不合。";
                        RejectOverrideCommandAndResume(000022, reason, agvcOverrideCmd);
                        return;
                    }

                    if (agvcOverrideCmd.ToUnloadSectionIds.Count == 0)
                    {
                        var reason = "替代路徑清單為空。";
                        RejectOverrideCommandAndResume(000024, reason, agvcOverrideCmd);
                        return;
                    }

                    if (!IsOverrideCommandMatchTheMapToUnload(agvcOverrideCmd))
                    {
                        var reason = "替代路徑中站點與路段不合圖資";
                        RejectOverrideCommandAndResume(000018, reason, agvcOverrideCmd);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));

                var reason = "替代路徑Exception";
                RejectOverrideCommandAndResume(000026, reason, agvcOverrideCmd);
                return;
            }

            #endregion

            #region 替代路徑生成
            try
            {
                //middleAgent.StopAskReserve();
                middleAgent.ClearAllReserve();
                agvcTransCmd.ExchangeSectionsAndAddress(agvcOverrideCmd);
                agvcTransCmd.AvoidEndAddressId = "";
                agvcTransCmd.IsAvoidComplete = false;
                theVehicle.CurAgvcTransCmd = agvcTransCmd;
                SetupOverrideTransferSteps(agvcOverrideCmd);
                transferSteps.Add(new EmptyTransferStep());
                theVehicle.ThePlcVehicle.FakeCassetteId = agvcTransCmd.CassetteId;
                middleAgent.ReplyTransferCommand(agvcOverrideCmd.CommandId, agvcOverrideCmd.GetActiveType(), agvcOverrideCmd.SeqNum, 0, "");
                var okmsg = $"MainFlow : 接受{agvcOverrideCmd.CommandType}命令{agvcOverrideCmd.CommandId}確認。";
                OnMessageShowEvent?.Invoke(this, okmsg);
                OnOverrideCommandCheckedEvent?.Invoke(this, agvcOverrideCmd);
                IsOverrideMove = true;
                IsAvoidMove = false;
                GoNextTransferStep = true;
                ResumeVisitTransferSteps();
            }
            catch (Exception ex)
            {
                StopAndClear();
                var reason = "替代路徑Exception";
                RejectOverrideCommandAndResume(000026, reason, agvcOverrideCmd);
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }

            #endregion
        }

        public bool IsMoveStopByNoReserve()
        {
            return moveControlHandler.elmoDriver.MoveCompelete(EnumAxis.GX) && IsPauseByNoReserve();
        }

        private void RejectTransferCommandAndResume(int alarmCode, string reason, AgvcTransCmd agvcTransferCmd)
        {
            try
            {
                alarmHandler.SetAlarm(alarmCode);
                middleAgent.ReplyTransferCommand(agvcTransferCmd.CommandId, agvcTransferCmd.GetActiveType(), agvcTransferCmd.SeqNum, 1, reason);
                reason = $"MainFlow : 拒絕 {agvcTransferCmd.CommandType} 命令, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                if (VisitTransferStepsStatus == EnumThreadStatus.Pause)
                {
                    ResumeVisitTransferSteps();
                    middleAgent.ResumeAskReserve();
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void RejectOverrideCommandAndResume(int alarmCode, string reason, AgvcOverrideCmd agvcOverrideCmd)
        {
            try
            {
                alarmHandler.SetAlarm(alarmCode);
                middleAgent.ReplyTransferCommand(agvcOverrideCmd.CommandId, agvcOverrideCmd.GetActiveType(), agvcOverrideCmd.SeqNum, 1, reason);
                reason = $"MainFlow : 拒絕 {agvcOverrideCmd.CommandType} 命令, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                middleAgent.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private bool IsOverrideCommandMatchTheMapToNextUnload(AgvcOverrideCmd agvcOverrideCmd)
        {
            var loadPos = TheMapInfo.allMapAddresses[agvcOverrideCmd.LoadAddressId].Position;
            return IsAgvcCommandMatchTheMap(loadPos, agvcOverrideCmd.ToUnloadSectionIds, agvcOverrideCmd.ToUnloadAddressIds, agvcOverrideCmd.UnloadAddressId, agvcOverrideCmd.CommandType);
        }

        private bool IsOverrideCommandMatchTheMapToLoad(AgvcOverrideCmd agvcOverrideCmd)
        {
            var curPos = theVehicle.VehicleLocation.RealPosition;
            return IsAgvcCommandMatchTheMap(curPos, agvcOverrideCmd.ToLoadSectionIds, agvcOverrideCmd.ToLoadAddressIds, agvcOverrideCmd.LoadAddressId, agvcOverrideCmd.CommandType);
        }

        private bool IsOverrideCommandMatchTheMapToUnload(AgvcOverrideCmd agvcOverrideCmd)
        {
            var curPos = theVehicle.VehicleLocation.RealPosition;
            return IsAgvcCommandMatchTheMap(curPos, agvcOverrideCmd.ToUnloadSectionIds, agvcOverrideCmd.ToUnloadAddressIds, agvcOverrideCmd.UnloadAddressId, agvcOverrideCmd.CommandType);
        }

        private bool IsPauseByNoReserve()
        {
            #region IsPauseByNoReserve 2.0
            var waitReserveIndex = moveControlHandler.ControlData.WaitReserveIndex;

            var vehicleStop = moveControlHandler.ControlData.SensorState;

            if (waitReserveIndex > -1 && vehicleStop == EnumVehicleSafetyAction.Stop)
            {
                return true;
            }
            else
            {
                return false;
            }
            #endregion
        }

        private void MiddleAgent_OnAvoideRequestEvent(object sender, AgvcMoveCmd agvcMoveCmd)
        {
            var msg = $"MainFlow : 收到避車命令，終點[{agvcMoveCmd.UnloadAddressId}]，開始檢查。";
            OnMessageShowEvent?.Invoke(this, msg);

            #region 避車檢查
            try
            {
                middleAgent.PauseAskReserve();

                if (IsAgvcTransferCommandEmpty())
                {
                    var reason = "車輛不在搬送命令中，無法避車";
                    RejectAvoidCommandAndResume(000033, reason, agvcMoveCmd);
                    return;
                }

                if (!IsMoveStep())
                {
                    var reason = "車輛不在移動流程，無法避車";
                    RejectAvoidCommandAndResume(000034, reason, agvcMoveCmd);
                    return;
                }

                if (!IsMoveStopByNoReserve() && !agvcTransCmd.IsAvoidComplete)
                {
                    var reason = $"車輛尚未停妥，無法避車";
                    RejectAvoidCommandAndResume(000035, reason, agvcMoveCmd);
                    return;
                }

                if (!IsAvoidCommandMatchTheMap(agvcMoveCmd))
                {
                    var reason = "避車路徑中站點與路段不合圖資";
                    RejectAvoidCommandAndResume(000018, reason, agvcMoveCmd);
                    return;
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));

                var reason = "避車Exception";
                RejectAvoidCommandAndResume(000036, reason, agvcMoveCmd);
            }
            #endregion

            #region 避車命令生成
            try
            {
                //middleAgent.StopAskReserve();
                middleAgent.ClearAllReserve();
                agvcTransCmd.CombineAvoid(agvcMoveCmd);
                agvcTransCmd.IsAvoidComplete = false;
                theVehicle.CurAgvcTransCmd = agvcTransCmd;
                SetupAvoidTransferSteps();
                theVehicle.ThePlcVehicle.FakeCassetteId = agvcTransCmd.CassetteId;
                middleAgent.ReplyAvoidCommand(agvcMoveCmd, 0, "");
                var okmsg = $"MainFlow : 接受避車命令確認，終點[{agvcTransCmd.AvoidEndAddressId}]。";
                OnMessageShowEvent?.Invoke(this, okmsg);
                OnAvoidCommandCheckedEvent?.Invoke(this, agvcMoveCmd);
                IsAvoidMove = true;
                IsOverrideMove = false;
                GoNextTransferStep = true;
                ResumeVisitTransferSteps();
            }
            catch (Exception ex)
            {
                StopAndClear();
                var reason = "避車Exception";
                RejectAvoidCommandAndResume(000036, reason, agvcMoveCmd);
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }

            #endregion
        }

        private bool IsAvoidCommandMatchTheMap(AgvcMoveCmd agvcMoveCmd)
        {
            var curPos = theVehicle.VehicleLocation.RealPosition;
            return IsAgvcCommandMatchTheMap(curPos, agvcMoveCmd.ToUnloadSectionIds, agvcMoveCmd.ToUnloadAddressIds, agvcMoveCmd.UnloadAddressId, agvcMoveCmd.CommandType);
        }

        private void RejectAvoidCommandAndResume(int alarmCode, string reason, AgvcMoveCmd agvcMoveCmd)
        {
            try
            {
                alarmHandler.SetAlarm(alarmCode);
                middleAgent.ReplyAvoidCommand(agvcMoveCmd, 1, reason);
                reason = $"MainFlow : 拒絕避車命令, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                middleAgent.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        #region Convert AgvcTransferCommand to TransferSteps

        private void SetupTransferSteps()
        {
            transferSteps = new List<TransferStep>();

            switch (agvcTransCmd.CommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    ConvertAgvcMoveCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.Load:
                    ConvertAgvcLoadCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.Unload:
                    ConvertAgvcUnloadCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.LoadUnload:
                    ConvertAgvcLoadUnloadCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.MoveToCharger:
                    ConvertAgvcMoveToChargerCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.Override:
                case EnumAgvcTransCommandType.Else:
                default:
                    break;
            }
        }
        private void SetupOverrideTransferSteps(AgvcOverrideCmd agvcOverrideCmd)
        {
            #region 1.0
            var aTransferSteps = new List<TransferStep>();

            switch (agvcTransCmd.CommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    ConvertAgvcMoveCmdIntoList(agvcTransCmd, aTransferSteps);
                    break;
                case EnumAgvcTransCommandType.Load:
                    ConvertAgvcLoadCmdIntoList(agvcTransCmd, aTransferSteps);
                    break;
                case EnumAgvcTransCommandType.Unload:
                    ConvertAgvcUnloadCmdIntoList(agvcTransCmd, aTransferSteps);
                    break;
                case EnumAgvcTransCommandType.LoadUnload:
                    ConvertOverrideAgvcLoadUnloadCmdIntoList(agvcTransCmd, aTransferSteps);
                    break;
                case EnumAgvcTransCommandType.MoveToCharger:
                    ConvertAgvcMoveToChargerCmdIntoList(agvcTransCmd, aTransferSteps);
                    break;
                case EnumAgvcTransCommandType.Override:
                case EnumAgvcTransCommandType.Else:
                default:
                    break;
            }

            transferSteps = aTransferSteps;
            #endregion

            #region 2.0

            #endregion

        }

       

        private void SetupAvoidTransferSteps()
        {
            MoveCmdInfo moveCmd = GetAvoidMoveCmdInfo(agvcTransCmd);
            transferSteps[TransferStepsIndex] = moveCmd;
        }

        private void ConvertAgvcLoadUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            ConvertAgvcLoadCmdIntoList(agvcTransCmd);
            ConvertAgvcNextUnloadCmdIntoList(agvcTransCmd);
        }
        private void ConvertOverrideAgvcLoadUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            ConvertAgvcLoadCmdIntoList(agvcTransCmd);
            if (agvcTransCmd.ToLoadAddressIds.Count == 0)
            {
                ConvertOverrideAgvcNextUnloadCmdIntoList(agvcTransCmd);
            }
            else
            {
                ConvertAgvcNextUnloadCmdIntoList(agvcTransCmd);
            }
        }
        private void ConvertOverrideAgvcLoadUnloadCmdIntoList(AgvcTransCmd agvcTransCmd, List<TransferStep> transferSteps)
        {
            ConvertAgvcLoadCmdIntoList(agvcTransCmd, transferSteps);
            if (theVehicle.ThePlcVehicle.Loading)
            {
                ConvertOverrideAgvcNextUnloadCmdIntoList(agvcTransCmd, transferSteps);
            }
            else
            {
                ConvertAgvcNextUnloadCmdIntoList(agvcTransCmd, transferSteps);
            }
        }

        private void ConvertAgvcUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
            moveCmd.IsMoveEndDoLoadUnload = true;
            transferSteps.Add(moveCmd);

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }
        private void ConvertAgvcUnloadCmdIntoList(AgvcTransCmd agvcTransCmd, List<TransferStep> transferSteps)
        {
            MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
            moveCmd.IsMoveEndDoLoadUnload = true;
            transferSteps.Add(moveCmd);

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }

        private void ConvertAgvcNextUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = GetMoveToNextUnloadCmdInfo(agvcTransCmd);
            moveCmd.IsMoveEndDoLoadUnload = true;
            transferSteps.Add(moveCmd);

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }
        private void ConvertAgvcNextUnloadCmdIntoList(AgvcTransCmd agvcTransCmd, List<TransferStep> transferSteps)
        {
            MoveCmdInfo moveCmd = GetMoveToNextUnloadCmdInfo(agvcTransCmd);
            moveCmd.IsMoveEndDoLoadUnload = true;
            transferSteps.Add(moveCmd);

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }

        private void ConvertOverrideAgvcNextUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
            moveCmd.IsMoveEndDoLoadUnload = true;
            transferSteps.Add(moveCmd);

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }
        private void ConvertOverrideAgvcNextUnloadCmdIntoList(AgvcTransCmd agvcTransCmd, List<TransferStep> transferSteps)
        {
            MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
            moveCmd.IsMoveEndDoLoadUnload = true;
            transferSteps.Add(moveCmd);

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }

        private void ConvertAgvcLoadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = GetMoveToLoadCmdInfo(agvcTransCmd);
            moveCmd.IsMoveEndDoLoadUnload = true;
            transferSteps.Add(moveCmd);

            LoadCmdInfo loadCmd = GetLoadCmdInfo(agvcTransCmd);
            transferSteps.Add(loadCmd);
        }
        private void ConvertAgvcLoadCmdIntoList(AgvcTransCmd agvcTransCmd, List<TransferStep> transferSteps)
        {
            MoveCmdInfo moveCmd = GetMoveToLoadCmdInfo(agvcTransCmd);
            moveCmd.IsMoveEndDoLoadUnload = true;
            transferSteps.Add(moveCmd);

            LoadCmdInfo loadCmd = GetLoadCmdInfo(agvcTransCmd);
            transferSteps.Add(loadCmd);
        }

        private MoveToChargerCmdInfo GetMoveToChargerCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveToChargerCmdInfo moveCmd = new MoveToChargerCmdInfo(this);
            try
            {
                moveCmd.CmdId = agvcTransCmd.CommandId;
                moveCmd.CstId = agvcTransCmd.CassetteId;
                moveCmd.AddressIds = agvcTransCmd.ToUnloadAddressIds;
                moveCmd.SectionIds = agvcTransCmd.ToUnloadSectionIds;
                moveCmd.FilterUselessFirstSection();
                moveCmd.SetupStartAddress();
                moveCmd.EndAddress = TheMapInfo.allMapAddresses[agvcTransCmd.UnloadAddressId];
                moveCmd.StageDirection = EnumStageDirectionParse(moveCmd.EndAddress.PioDirection);
                moveCmd.WheelAngle = GetCurWheelAngle();
                moveCmd.VehicleHeadAngle = GetCurVehicleAngle();
                moveCmd.SetupMovingSectionsAndAddresses();
                moveCmd.MovingSectionsIndex = 0;
                moveCmd.SetupAddressPositions();
                //moveCmd.SetupAddressActions();
                moveCmd.SetupSectionSpeedLimits();
                moveCmd.SetupInfo();
                OnMessageShowEvent?.Invoke(this, moveCmd.Info);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
            return moveCmd;
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

        private MoveCmdInfo GetMoveToUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(this);
            try
            {
                moveCmd.CmdId = agvcTransCmd.CommandId;
                moveCmd.CstId = agvcTransCmd.CassetteId;
                moveCmd.AddressIds = agvcTransCmd.ToUnloadAddressIds;
                moveCmd.SectionIds = agvcTransCmd.ToUnloadSectionIds;
                moveCmd.FilterUselessFirstSection();
                moveCmd.SetupStartAddress();
                moveCmd.EndAddress = TheMapInfo.allMapAddresses[agvcTransCmd.UnloadAddressId];
                moveCmd.StageDirection = EnumStageDirectionParse(moveCmd.EndAddress.PioDirection);
                moveCmd.WheelAngle = GetCurWheelAngle();
                moveCmd.VehicleHeadAngle = GetCurVehicleAngle();
                moveCmd.SetupMovingSectionsAndAddresses();
                moveCmd.SetupAddressPositions();
                //moveCmd.SetupAddressActions();
                moveCmd.SetupSectionSpeedLimits();
                moveCmd.SetupInfo();
                OnMessageShowEvent?.Invoke(this, moveCmd.Info);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
            return moveCmd;
        }
        private MoveCmdInfo GetMoveToNextUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(this);
            try
            {
                moveCmd.CmdId = agvcTransCmd.CommandId;
                moveCmd.CstId = agvcTransCmd.CassetteId;
                moveCmd.AddressIds = agvcTransCmd.ToUnloadAddressIds;
                moveCmd.SectionIds = agvcTransCmd.ToUnloadSectionIds;
                moveCmd.StartAddress = TheMapInfo.allMapAddresses[agvcTransCmd.LoadAddressId];
                moveCmd.EndAddress = TheMapInfo.allMapAddresses[agvcTransCmd.UnloadAddressId];
                moveCmd.StageDirection = EnumStageDirectionParse(moveCmd.EndAddress.PioDirection);
                moveCmd.FilterUselessNextToLoadFirstSection();
                moveCmd.IsLoadPortToUnloadPort = true;
                moveCmd.SetupMovingSectionsAndAddresses();
                moveCmd.MovingSectionsIndex = 0;
                moveCmd.SetupNextUnloadAddressPositions();
                //moveCmd.SetupAddressActions();
                moveCmd.SetupSectionSpeedLimits();
                moveCmd.SetupInfo();
                OnMessageShowEvent?.Invoke(this, moveCmd.Info);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
            return moveCmd;
        }
        private MoveCmdInfo GetMoveToLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(this);
            try
            {
                moveCmd.CmdId = agvcTransCmd.CommandId;
                moveCmd.CstId = agvcTransCmd.CassetteId;
                moveCmd.AddressIds = agvcTransCmd.ToLoadAddressIds;
                moveCmd.SectionIds = agvcTransCmd.ToLoadSectionIds;
                moveCmd.FilterUselessFirstSection();
                moveCmd.SetupStartAddress();
                moveCmd.EndAddress = TheMapInfo.allMapAddresses[agvcTransCmd.LoadAddressId];
                moveCmd.StageDirection = EnumStageDirectionParse(moveCmd.EndAddress.PioDirection);
                moveCmd.WheelAngle = GetCurWheelAngle();
                moveCmd.VehicleHeadAngle = GetCurVehicleAngle();
                moveCmd.SetupMovingSectionsAndAddresses();
                moveCmd.MovingSectionsIndex = 0;
                moveCmd.SetupAddressPositions();
                //moveCmd.SetupAddressActions();
                moveCmd.SetupSectionSpeedLimits();
                moveCmd.SetupInfo();
                OnMessageShowEvent?.Invoke(this, moveCmd.Info);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
            return moveCmd;
        }
        private MoveCmdInfo GetAvoidMoveCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(this);
            try
            {
                moveCmd.CmdId = agvcTransCmd.CommandId;
                moveCmd.CstId = agvcTransCmd.CassetteId;
                moveCmd.AddressIds = agvcTransCmd.ToUnloadAddressIds;
                moveCmd.SectionIds = agvcTransCmd.ToUnloadSectionIds;
                moveCmd.FilterUselessFirstSection();
                moveCmd.SetupStartAddress();
                moveCmd.EndAddress = TheMapInfo.allMapAddresses[agvcTransCmd.AvoidEndAddressId];
                moveCmd.StageDirection = EnumStageDirectionParse(moveCmd.EndAddress.PioDirection);
                moveCmd.WheelAngle = GetCurWheelAngle();
                moveCmd.VehicleHeadAngle = GetCurVehicleAngle();
                moveCmd.SetupMovingSectionsAndAddresses();
                moveCmd.SetupAddressPositions();
                //moveCmd.SetupAddressActions();
                moveCmd.SetupSectionSpeedLimits();
                moveCmd.SetupInfo();
                OnMessageShowEvent?.Invoke(this, moveCmd.Info);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
            return moveCmd;
        }
        private LoadCmdInfo GetLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            LoadCmdInfo loadCmd = new LoadCmdInfo();
            loadCmd.CstId = agvcTransCmd.CassetteId;
            loadCmd.CmdId = agvcTransCmd.CommandId;
            loadCmd.LoadAddress = agvcTransCmd.LoadAddressId;
            MapAddress mapAddress = TheMapInfo.allMapAddresses[loadCmd.LoadAddress];
            loadCmd.IsEqPio = mapAddress.PioDirection == EnumPioDirection.None ? false : true;
            if (mapAddress.CanLeftLoad && !mapAddress.CanRightLoad)
            {
                loadCmd.StageDirection = EnumStageDirection.Left;
                loadCmd.StageNum = 1;
            }
            else if (!mapAddress.CanLeftLoad && mapAddress.CanRightLoad)
            {
                loadCmd.StageDirection = EnumStageDirection.Right;
                loadCmd.StageNum = 2;
            }
            else
            {
                loadCmd.StageDirection = EnumStageDirection.None;
                loadCmd.StageNum = 3;
            }

            return loadCmd;
        }
        private UnloadCmdInfo GetUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            UnloadCmdInfo unloadCmd = new UnloadCmdInfo();
            unloadCmd.CstId = agvcTransCmd.CassetteId;
            unloadCmd.CmdId = agvcTransCmd.CommandId;
            unloadCmd.UnloadAddress = agvcTransCmd.UnloadAddressId;
            MapAddress mapAddress = TheMapInfo.allMapAddresses[unloadCmd.UnloadAddress];
            unloadCmd.IsEqPio = mapAddress.PioDirection == EnumPioDirection.None ? false : true;
            if (mapAddress.CanLeftUnload && !mapAddress.CanRightUnload)
            {
                unloadCmd.StageDirection = EnumStageDirection.Left;
                unloadCmd.StageNum = 1;
            }
            else if (!mapAddress.CanLeftUnload && mapAddress.CanRightUnload)
            {
                unloadCmd.StageDirection = EnumStageDirection.Right;
                unloadCmd.StageNum = 2;
            }
            else
            {
                unloadCmd.StageDirection = EnumStageDirection.None;
                unloadCmd.StageNum = 3;
            }

            return unloadCmd;
        }
        public AgvcTransCmd GetAgvcTransCmd() => agvcTransCmd;
        private void ConvertAgvcMoveCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(moveCmd);
        }
        private void ConvertAgvcMoveCmdIntoList(AgvcTransCmd agvcTransCmd, List<TransferStep> transferSteps)
        {
            MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(moveCmd);
        }
        private void ConvertAgvcMoveToChargerCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveToChargerCmdInfo moveToChargerCmd = GetMoveToChargerCmdInfo(agvcTransCmd);
            transferSteps.Add(moveToChargerCmd);
        }
        private void ConvertAgvcMoveToChargerCmdIntoList(AgvcTransCmd agvcTransCmd, List<TransferStep> transferSteps)
        {
            MoveToChargerCmdInfo moveToChargerCmd = GetMoveToChargerCmdInfo(agvcTransCmd);
            transferSteps.Add(moveToChargerCmd);
        }

        #endregion

        public void IdleVisitNext()
        {
            var msg = $"MainFlow : Idle Visit Next TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            TransferStepsIndex++;
        }
        private void MiddleAgent_OnGetBlockPassEvent(object sender, bool e)
        {
            //throw new NotImplementedException();
        }
        private bool IsUnloadArrival()
        {
            // 判斷當前是否可載貨 若否 則發送報告
            var curAddress = theVehicle.VehicleLocation.LastAddress;
            var unloadAddressId = agvcTransCmd.UnloadAddressId;
            if (curAddress.Id == unloadAddressId)
            {
                if (IsRetryArrival)
                {
                    IsRetryArrival = false;
                }
                else
                {
                    middleAgent.UnloadArrivals();
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
        private bool IsLoadArrival()
        {
            // 判斷當前是否可卸貨 若否 則發送報告
            var curAddress = theVehicle.VehicleLocation.LastAddress;
            var loadAddressId = agvcTransCmd.LoadAddressId;

            if (curAddress.Id == loadAddressId)
            {
                if (IsRetryArrival)
                {
                    IsRetryArrival = false;
                }
                else
                {
                    middleAgent.LoadArrivals();
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
        public bool CanVehMove()
        {
            //battery/emo/beam/etc/reserve
            // 判斷當前是否可移動 若否 則發送報告
            var plcVeh = theVehicle.ThePlcVehicle;
            var result = plcVeh.Robot.ForkHome && !plcVeh.Batterys.Charging;

            if (!result)
            {
                var msg = $"MainFlow : CanVehMove, [RobotHome={plcVeh.Robot.ForkHome}][Charging={plcVeh.Batterys.Charging}]";
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
            }

            return result;
        }
        public bool IsAgvcTransferCommandEmpty()
        {
            return agvcTransCmd.CommandId == "";
        }
        #endregion

        public void UpdateMoveControlReserveOkPositions(MapSection mapSection)
        {
            try
            {
                MapAddress address = mapSection.CmdDirection == EnumPermitDirection.Forward
                    ? mapSection.TailAddress
                    : mapSection.HeadAddress;

                bool updateResult = moveControlHandler.AddReservedMapPosition(address.Position);
                OnMessageShowEvent?.Invoke(this, $"通知MoveControl延攬通行權{mapSection.Id}成功，下一個可行終點為[{address.Id}]({Convert.ToInt32(address.Position.X)},{Convert.ToInt32(address.Position.Y)})。");
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public bool IsMoveStep() => GetCurrentTransferStepType() == EnumTransferStepType.Move || GetCurrentTransferStepType() == EnumTransferStepType.MoveToCharger;

        public void MoveControlHandler_OnMoveFinished(object sender, EnumMoveComplete status)
        {
            try
            {
                IsMoveEnd = true;
                OnMoveArrivalEvent?.Invoke(this, new EventArgs());

                #region Not EnumMoveComplete.Success
                if (status == EnumMoveComplete.Fail)
                {
                    middleAgent.ClearAllReserve();
                    if (IsAvoidMove)
                    {
                        middleAgent.AvoidFail();
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
                    VisitTransferStepsStatus = EnumThreadStatus.PauseComplete;
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : 移動暫停確認");
                    middleAgent.PauseAskReserve();
                    PauseVisitTransferSteps();
                    return;
                }

                if (status == EnumMoveComplete.Cancel)
                {
                    StopAndClear();
                    if (IsAvoidMove)
                    {
                        middleAgent.AvoidFail();
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
                middleAgent.ClearAllReserve();

                MoveCmdInfo moveCmd = (MoveCmdInfo)GetCurTransferStep();

                UpdateVehiclePositionAfterArrival(moveCmd);

                ArrivalStartCharge(moveCmd.EndAddress);

                ArrivalStopCharge(mainFlowConfig.LoadingChargeIntervalMs);

                if (IsAvoidMove)
                {
                    agvcTransCmd.IsAvoidComplete = true;
                    middleAgent.AvoidComplete();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : 避車移動完成");
                }
                else
                {
                    if (transferSteps.Count > 0)
                    {
                        if (IsNextTransferStepIdle())
                        {
                            middleAgent.MoveArrival();
                        }
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 走行移動完成");

                        VisitNextTransferStep();
                    }
                    else
                    {
                        middleAgent.MoveArrival();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : 走行移動完成");
                    }
                }

                IsAvoidMove = false;
                IsOverrideMove = false;

                #endregion
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void MoveControlHandler_OnRetryMoveFinished(object sender, EnumMoveComplete e)
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : 取放貨異常，觸發重試機制，到站。");

                ForkNgRetryArrivalStartCharge();

                IsRetryArrival = true;

                int timeoutCount = 10;
                while (true)
                {
                    if (plcAgent.IsForkCommandExist())
                    {
                        plcAgent.ClearExecutingForkCommand();
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void ForkNgRetryArrivalStartCharge()
        {
            MoveCmdInfo moveCmd = (MoveCmdInfo)GetPreTransferStep();

            ArrivalStartCharge(moveCmd.EndAddress);

            RetryArrivalStopCharge(mainFlowConfig.LoadingChargeIntervalMs);
        }

        private void RetryArrivalStopCharge(int loadingChargeIntervalMs)
        {
            try
            {
                if (GetCurrentTransferStepType() == EnumTransferStepType.Load && loadingChargeIntervalMs > 0)
                {
                    try
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        while (!theVehicle.ThePlcVehicle.Loading)
                        {
                            if (sw.ElapsedMilliseconds >= mainFlowConfig.StopChargeWaitingTimeoutMs)
                            {
                                break;
                            }
                            SpinWait.SpinUntil(() => false, 50);
                        }
                        SpinWait.SpinUntil(() => false, loadingChargeIntervalMs);
                        StopCharge();
                    }
                    catch (Exception ex)
                    {
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void ArrivalStopCharge(int loadingChargeIntervalMs)
        {
            try
            {
                if (IsNextTransferStepLoad() && loadingChargeIntervalMs > 0)
                {
                    //Task.Run(() =>
                    //{
                    try
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        while (!theVehicle.ThePlcVehicle.Loading)
                        {
                            if (sw.ElapsedMilliseconds >= mainFlowConfig.StopChargeWaitingTimeoutMs)
                            {
                                break;
                            }
                            SpinWait.SpinUntil(() => false, 50);
                        }
                        SpinWait.SpinUntil(() => false, loadingChargeIntervalMs);
                        StopCharge();
                    }
                    catch (Exception ex)
                    {
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }
                    //});
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
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
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void PlcAgent_OnForkCommandFinishEvent(object sender, PlcForkCommand forkCommand)
        {
            try
            {
                agvcTransCmd.ForkNgRetryTimes = mainFlowConfig.ForkNgRetryTimes;
                if (forkCommand.ForkCommandType == EnumForkCommand.Load)
                {
                    if (middleAgent.CstIdRead(ReadResult))
                    {
                        //middleAgent.LoadComplete();
                        //OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot取貨完成");
                        VisitNextTransferStep();
                    }
                    //else
                    //{

                    //}

                    //Stopwatch sw = new Stopwatch();
                    //int bcrReadRetryTimeoutTotal = 0;
                    //int bcrReadRetryTimeoutMs = middlerConfig.BcrReadRetryTimeoutSec * 1000;
                    //int bcrReadRetryIntervalMs = middlerConfig.BcrReadRetryIntervalMs;
                    //sw.Start();
                    //while (true)
                    //{
                    //    sw.Stop();
                    //    if (sw.ElapsedMilliseconds > bcrReadRetryIntervalMs)
                    //    {
                    //        middleAgent.CstIdRead(ReadResult);

                    //        sw.Reset();
                    //        bcrReadRetryTimeoutTotal += bcrReadRetryIntervalMs;
                    //        if (bcrReadRetryTimeoutTotal > bcrReadRetryTimeoutMs)
                    //        {
                    //            alarmHandler.SetAlarm(000029);
                    //            break;
                    //        }
                    //    }
                    //    sw.Start();

                    //    SpinWait.SpinUntil(() => false, 100);

                    //    try
                    //    {


                    //        //if (IsCancelByCstIdRead)
                    //        //{
                    //        //    OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot取貨完成，貨物ID讀取異常。");
                    //        //    StopAndClear();
                    //        //    break;
                    //        //}
                    //        //else
                    //        //{
                    //        //    OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot取貨完成");
                    //        //    VisitNextTransferStep();
                    //        //    break;
                    //        //}
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    //    }
                    //}


                }
                else /*if (forkCommand.ForkCommandType == EnumForkCommand.Unload)*/
                {
                    if (theVehicle.ThePlcVehicle.Loading)
                    {
                        alarmHandler.SetAlarm(000007);
                        return;
                    }

                    theVehicle.ThePlcVehicle.CassetteId = "";

                    middleAgent.UnloadComplete();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot放貨完成");
                    VisitNextTransferStep();
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }
        private void PlcAgent_OnForkCommandErrorEvent(object sender, PlcForkCommand e)
        {
            //middleAgent.CstIdRead(ReadResult);
        }
        private void PlcAgent_OnCassetteIDReadFinishEvent(object sender, string cstId)
        {
            try
            {
                #region 2019.12.11
                //if (string.IsNullOrEmpty(cstId) || cstId == "ERROR")
                //{
                //    PauseVisitTransferSteps();
                //    var msg = $"貨物ID讀取失敗";
                //    OnMessageShowEvent?.Invoke(this, msg);
                //    ReadResult = EnumCstIdReadResult.Fail;
                //    alarmHandler.SetAlarm(000004);
                //}
                //else if (!IsAgvcTransferCommandEmpty() && agvcTransCmd.CassetteId != cstId)
                //{
                //    PauseVisitTransferSteps();
                //    var msg = $"貨物ID[{cstId}]，與命令要求貨物ID[{agvcTransCmd.CassetteId}]不合"; ;
                //    OnMessageShowEvent?.Invoke(this, msg);
                //    ReadResult = EnumCstIdReadResult.Mismatch;
                //    alarmHandler.SetAlarm(000028);
                //}
                //else
                //{
                //    var msg = $"貨物ID[{cstId}]讀取成功";
                //    OnMessageShowEvent?.Invoke(this, msg);
                //    ReadResult = EnumCstIdReadResult.Noraml;
                //}
                //middleAgent.CstIdRead(ReadResult);

                #endregion

                #region 2019.12.16 Report to Agvc when ForkFinished

                if (string.IsNullOrEmpty(cstId) || cstId == "ERROR")
                {
                    var msg = $"貨物ID讀取失敗";
                    OnMessageShowEvent?.Invoke(this, msg);
                    ReadResult = EnumCstIdReadResult.Fail;
                    alarmHandler.SetAlarm(000004);
                }
                else if (!IsAgvcTransferCommandEmpty() && agvcTransCmd.CassetteId != cstId)
                {
                    var msg = $"貨物ID[{cstId}]，與命令要求貨物ID[{agvcTransCmd.CassetteId}]不合";
                    OnMessageShowEvent?.Invoke(this, msg);
                    ReadResult = EnumCstIdReadResult.Mismatch;
                    alarmHandler.SetAlarm(000028);
                }
                else
                {
                    var msg = $"貨物ID[{cstId}]讀取成功";
                    OnMessageShowEvent?.Invoke(this, msg);
                    ReadResult = EnumCstIdReadResult.Noraml;
                }
                theVehicle.ThePlcVehicle.CassetteId = cstId;

                #endregion
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }
        private void PlcAgent_OnForkCommandInterlockErrorEvent(object sender, PlcForkCommand e)
        {
            try
            {
                if (GetCurrentTransferStepType() == EnumTransferStepType.Load || GetCurrentTransferStepType() == EnumTransferStepType.Unload)
                {
                    var msg = $"MainFlow : 取放貨異常[InterlockError]，剩餘重試次數[{agvcTransCmd.ForkNgRetryTimes}]";
                    OnMessageShowEvent?.Invoke(this, msg);

                    #region 2019.12.11 Abort
                    //agvcTransCmd.CompleteStatus = CompleteStatus.CmpStatusInterlockError;
                    //StopAndClear();
                    //alarmHandler.ResetAllAlarms();
                    #endregion

                    #region 2019.12.16 Retry

                    if (theVehicle.ThePlcVehicle.Robot.ForkHome)
                    {
                        if (agvcTransCmd.ForkNgRetryTimes > 0)
                        {
                            agvcTransCmd.ForkNgRetryTimes--;
                            if (StopCharge())
                            {
                                alarmHandler.ResetAllAlarms();
                                OnMessageShowEvent?.Invoke(this, $"MainFlow : 取放貨異常，充電已停止，觸發重試機制。");
                                IsRetryArrival = false;
                                moveControlHandler.TransferMove_RetryMove();
                                return;
                            }
                        }
                    }

                    alarmHandler.ResetAllAlarms();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : 取放貨異常，流程放棄。");
                    agvcTransCmd.CompleteStatus = CompleteStatus.CmpStatusInterlockError;
                    StopAndClear();


                    #endregion
                }
            }
            catch (Exception ex)
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : 取放貨異常，異常跳出。");
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public bool IsVisitTransferStepsPause() => VisitTransferStepsStatus == EnumThreadStatus.Pause || VisitTransferStepsStatus == EnumThreadStatus.PauseComplete;

        private bool IsNextTransferStepUnload() => GetNextTransferStepType() == EnumTransferStepType.Unload;
        private bool IsNextTransferStepLoad() => GetNextTransferStepType() == EnumTransferStepType.Load;
        private bool IsNextTransferStepMove() => GetNextTransferStepType() == EnumTransferStepType.Move || GetNextTransferStepType() == EnumTransferStepType.MoveToCharger;
        private bool IsNextTransferStepIdle() => GetNextTransferStepType() == EnumTransferStepType.Empty;

        private bool IsCurCmdTypeLoadUnload() => agvcTransCmd.CommandType == EnumAgvcTransCommandType.LoadUnload;

        private void VisitNextTransferStep()
        {
            TransferStepsIndex++;
            GoNextTransferStep = true;
        }

        public TransferStep GetCurTransferStep()
        {
            TransferStep transferStep = new EmptyTransferStep(this);
            if (TransferStepsIndex < transferSteps.Count)
            {
                transferStep = transferSteps[TransferStepsIndex];
            }
            return transferStep;
        }

        public TransferStep GetPreTransferStep()
        {
            TransferStep transferStep = new EmptyTransferStep(this);
            var preTransferStepsIndex = TransferStepsIndex - 1;
            if (preTransferStepsIndex < transferSteps.Count && preTransferStepsIndex >= 0)
            {
                transferStep = transferSteps[preTransferStepsIndex];
            }
            return transferStep;
        }

        public TransferStep GetNextTransferStep()
        {
            TransferStep transferStep = new EmptyTransferStep(this);
            int nextIndex = TransferStepsIndex + 1;
            if (nextIndex < transferSteps.Count)
            {
                transferStep = transferSteps[nextIndex];
            }
            return transferStep;
        }

        public void SetTransCmdsStep(ITransferStatus step)
        {
            this.transferStatus = step;
        }

        public void DoTransfer()
        {
            transferStatus.DoTransfer(this);
        }

        public void PublishOnDoTransferStepEvent(TransferStep transferStep)
        {
            OnDoTransferStepEvent?.Invoke(this, transferStep);
        }

        public void Unload(UnloadCmdInfo unloadCmd)
        {
            if (theVehicle.ThePlcVehicle.CassetteId == "")
            {
                alarmHandler.SetAlarm(000017);
                return;
            }

            if (IsUnloadArrival())
            {
                try
                {
                    int timeoutCount = 10;
                    while (true)
                    {
                        if (plcAgent.IsForkCommandExist())
                        {
                            plcAgent.ClearExecutingForkCommand();
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
                            agvcTransCmd.CompleteStatus = CompleteStatus.CmpStatusInterlockError;
                            StopAndClear();
                            return;
                        }
                    }

                    if (plcAgent.IsForkCommandExist())
                    {
                        plcAgent.ClearExecutingForkCommand();
                        SpinWait.SpinUntil(() => false, 500);
                    }

                    middleAgent.Unloading();
                    PublishOnDoTransferStepEvent(unloadCmd);
                    PlcForkUnloadCommand = new PlcForkCommand(ForkCommandNumber++, EnumForkCommand.Unload, unloadCmd.StageNum.ToString(), unloadCmd.StageDirection, unloadCmd.IsEqPio, unloadCmd.ForkSpeed);
                    Task.Run(() => plcAgent.AddForkComand(PlcForkUnloadCommand));
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot放貨中, [方向{unloadCmd.StageDirection}][編號={PlcForkLoadCommand.StageNo}][是否PIO={PlcForkLoadCommand.IsEqPio}]");
                    batteryLog.LoadUnloadCount++;
                    SaveBatteryLog();


                    //if (!plcAgent.IsForkCommandExist())
                    //{
                    //}
                    //else
                    //{
                    //    alarmHandler.SetAlarm(000008);
                    //}
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
            }
        }

        public void Load(LoadCmdInfo loadCmd)
        {
            if (theVehicle.ThePlcVehicle.CassetteId != "")
            {
                alarmHandler.SetAlarm(000016);
                return;
            }

            if (IsLoadArrival())
            {
                try
                {
                    int timeoutCount = 10;
                    while (true)
                    {
                        if (plcAgent.IsForkCommandExist())
                        {
                            plcAgent.ClearExecutingForkCommand();
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
                            agvcTransCmd.CompleteStatus = CompleteStatus.CmpStatusInterlockError;
                            StopAndClear();
                            return;
                        }
                    }


                    middleAgent.Loading();
                    PublishOnDoTransferStepEvent(loadCmd);
                    ReadResult = EnumCstIdReadResult.Noraml;
                    PlcForkLoadCommand = new PlcForkCommand(ForkCommandNumber++, EnumForkCommand.Load, loadCmd.StageNum.ToString(), loadCmd.StageDirection, loadCmd.IsEqPio, loadCmd.ForkSpeed);
                    Task.Run(() => plcAgent.AddForkComand(PlcForkLoadCommand));
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Robot取貨中, [方向={loadCmd.StageDirection}][編號={PlcForkLoadCommand.StageNo}][是否PIO={PlcForkLoadCommand.IsEqPio}]");
                    batteryLog.LoadUnloadCount++;
                    SaveBatteryLog();

                    //if (!plcAgent.IsForkCommandExist())
                    //{
                    //}
                    //else
                    //{
                    //    alarmHandler.SetAlarm(000010);
                    //}
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
            }
        }

        #region Simple Getters
        public AlarmHandler GetAlarmHandler() => alarmHandler;
        public MiddleAgent GetMiddleAgent() => middleAgent;
        public MiddlerConfig GetMiddlerConfig() => middlerConfig;
        public MainFlowConfig GetMainFlowConfig() => mainFlowConfig;
        public MapConfig GetMapConfig() => mapConfig;
        public MapHandler GetMapHandler() => mapHandler;
        public MoveControlHandler GetMoveControlHandler() => moveControlHandler;
        public PlcAgent GetPlcAgent() => plcAgent;
        public MCProtocol GetMcProtocol() => mcProtocol;
        public AlarmConfig GetAlarmConfig() => alarmConfig;
        #endregion

        public bool CallMoveControlWork(MoveCmdInfo moveCmd)
        {
            try
            {

                var msg1 = $"MainFlow : 通知MoveControlHandler傳送";
                OnMessageShowEvent?.Invoke(this, msg1);

                string errorMsg = "";
                if (moveControlHandler.TransferMove(moveCmd, ref errorMsg))
                {
                    var msg2 = $"MainFlow : 通知MoveControlHandler傳送，回報可行.";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    PublishOnDoTransferStepEvent(moveCmd);
                    return true;
                }
                else
                {
                    var msg2 = $"MainFlow : 通知MoveControlHandler傳送，回報失敗。{errorMsg}";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    MoveControlHandler_OnMoveFinished(this, EnumMoveComplete.Fail);
                    return false;
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
                return false;
            }
        }
        public bool CallMoveControlOverride(MoveCmdInfo moveCmd)
        {
            try
            {
                var msg1 = $"MainFlow : 通知MoveControlHandler[替代路徑]";
                OnMessageShowEvent?.Invoke(this, msg1);
                string errorMsg = "";
                if (moveControlHandler.TransferMove_Override(moveCmd, ref errorMsg))
                {
                    var msg2 = $"MainFlow : 通知MoveControlHandler[替代路徑]，回報可行.";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    PublishOnDoTransferStepEvent(moveCmd);
                    return true;
                }
                else
                {
                    var msg2 = $"MainFlow : 通知MoveControlHandler[替代路徑]，回報失敗。{errorMsg}";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    MoveControlHandler_OnMoveFinished(this, EnumMoveComplete.Fail);
                    return false;
                }


            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
                return false;
            }
        }
        public bool CallMoveControlAvoid(MoveCmdInfo moveCmd)
        {
            try
            {
                var msg1 = $"MainFlow : 通知MoveControlHandler[避車路徑]";
                OnMessageShowEvent?.Invoke(this, msg1);

                string errorMsg = "";
                if (moveControlHandler.TransferMove_Override(moveCmd, ref errorMsg))
                {
                    var msg2 = $"MainFlow : 通知MoveControlHandler[避車路徑]，回報可行.";
                    OnMessageShowEvent?.Invoke(this, msg1);
                    PublishOnDoTransferStepEvent(moveCmd);
                    return true;
                }
                else
                {
                    var msg2 = $"MainFlow : 通知MoveControlHandler[避車路徑]，回報失敗。{errorMsg}";
                    OnMessageShowEvent?.Invoke(this, msg2);
                    MoveControlHandler_OnMoveFinished(this, EnumMoveComplete.Fail);
                    return false;
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
                return false;
            }
        }

        public void PrepareForAskingReserve(MoveCmdInfo moveCmd)
        {
            try
            {
                #region 1.0
                //middleAgent.StopAskReserve();
                //middleAgent.NeedReserveSections = moveCmd.MovingSections;
                //middleAgent.ReportSectionPass(EventType.AdrPass);
                //OnPrepareForAskingReserveEvent?.Invoke(this, moveCmd);
                //middleAgent.StartAskReserve();
                #endregion

                #region 2.0
                //middleAgent.PauseAskReserve();
                middleAgent.ReportSectionPass(EventType.AdrPass);
                middleAgent.ClearAllReserve();
                middleAgent.SetupNeedReserveSections(moveCmd.MovingSections);
                //middleAgent.ResumeAskReserve();
                #endregion

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private bool UpdateVehiclePositionInMovingStep(MoveCmdInfo moveCmdInfo, VehicleLocation vehicleLocation)
        {
            if (mapHandler.IsPositionInThisAddress(vehicleLocation.RealPosition, vehicleLocation.LastAddress.Position))
            {
                return false;
            }
            List<MapSection> MovingSections = moveCmdInfo.MovingSections;// GetMapSection(moveCmdInfo);
            int searchingSectionIndex = moveCmdInfo.MovingSectionsIndex;
            bool isUpdateSection = false;
            while (searchingSectionIndex < MovingSections.Count)
            {
                try
                {
                    if (mapHandler.IsPositionInThisSection(MovingSections[searchingSectionIndex], vehicleLocation.RealPosition))
                    {
                        while (moveCmdInfo.MovingSectionsIndex < searchingSectionIndex)
                        {
                            batteryLog.MoveDistanceTotalM += (int)(moveCmdInfo.MovingSections[moveCmdInfo.MovingSectionsIndex].HeadToTailDistance / 1000);
                            SaveBatteryLog();
                            moveCmdInfo.MovingSectionsIndex++;
                            FitVehicalLocationAndMoveCmd(moveCmdInfo, vehicleLocation);
                            middleAgent.ReportSectionPass(EventType.AdrPass);
                            isUpdateSection = true;
                        }

                        FitVehicalLocation(moveCmdInfo, vehicleLocation);

                        UpdateMiddlerGotReserveOkSections(MovingSections[searchingSectionIndex].Id);

                        UpdatePlcVehicleBeamSensor();

                        break;
                    }
                    else
                    {
                        searchingSectionIndex++;
                    }
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    break;
                }
            }

            //if (searchingSectionIndex >= MovingSections.Count)
            //{
            //    isUpdateSection = false;
            //    alarmHandler.SetAlarm(000011);
            //    var msg = $"MainFlow : 有命令下，車輛迷航, [Position=({vehicleLocation.RealPosition.X:F2},{vehicleLocation.RealPosition.Y:F2})]";
            //    loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //         , msg));
            //}

            return isUpdateSection;
        }

        private void FitVehicalLocation(MoveCmdInfo moveCmdInfo, VehicleLocation vehicleLocation)
        {
            var section = moveCmdInfo.MovingSections[moveCmdInfo.MovingSectionsIndex];
            VehicleLocation tempLocation = new VehicleLocation(vehicleLocation);
            tempLocation.LastSection = section;

            FindeNeerlyAddressInTheMovingSection(section, ref tempLocation);

            tempLocation.LastSection = TheMapInfo.allMapSections[section.Id];

            tempLocation.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(tempLocation.RealPosition, tempLocation.LastSection.HeadAddress.Position);

            theVehicle.VehicleLocation = new VehicleLocation(tempLocation);
        }

        private void FitVehicalLocationAndMoveCmd(MoveCmdInfo moveCmdInfo, VehicleLocation vehicleLocation)
        {
            var section = moveCmdInfo.MovingSections[moveCmdInfo.MovingSectionsIndex];
            VehicleLocation tempLocation = new VehicleLocation(vehicleLocation);
            tempLocation.LastSection = section;
            if (section.CmdDirection == EnumPermitDirection.Forward)
            {
                tempLocation.LastAddress = section.HeadAddress;
                tempLocation.LastSection.VehicleDistanceSinceHead = 0;
            }
            else
            {
                tempLocation.LastAddress = section.TailAddress;
                tempLocation.LastSection.VehicleDistanceSinceHead = section.HeadToTailDistance;
            }
            theVehicle.VehicleLocation = new VehicleLocation(tempLocation);
        }

        private void FindeNeerlyAddressInTheMovingSection(MapSection mapSection, ref VehicleLocation vehicleLocation)
        {
            try
            {
                double neerlyDistance = 999999;
                foreach (MapAddress mapAddress in mapSection.InsideAddresses)
                {
                    double dis = mapHandler.GetDistance(vehicleLocation.RealPosition, mapAddress.Position);

                    if (dis < neerlyDistance)
                    {
                        neerlyDistance = dis;
                        vehicleLocation.NeerlyAddress = mapAddress;
                    }
                }
                vehicleLocation.LastAddress = vehicleLocation.NeerlyAddress;
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void UpdateVehiclePositionAfterArrival(MoveCmdInfo moveCmd)
        {
            try
            {
                MapSection lastSection = moveCmd.MovingSections.FindLast(x => x.Id != null);
                lastSection = TheMapInfo.allMapSections[lastSection.Id];
                var lastAddress = moveCmd.EndAddress;
                CmdEndVehiclePosition = new VehicleLocation(theVehicle.VehicleLocation);
                CmdEndVehiclePosition.RealPosition = lastAddress.Position;
                CmdEndVehiclePosition.LastAddress = lastAddress;
                CmdEndVehiclePosition.LastSection = lastSection;
                CmdEndVehiclePosition.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(lastAddress.Position, lastSection.HeadAddress.Position);
                theVehicle.VehicleLocation = CmdEndVehiclePosition;

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                      , $"MainFolw : 車輛抵達終點站{moveCmd.EndAddress.Id}，位置更新。"));
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "7", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"MainFolw : 車輛抵達終點站{moveCmd.EndAddress.Id}，位置更新失敗。"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "7", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        private void UpdateVehiclePositionManual(VehicleLocation vehicleLocation)
        {
            MapAddress neerlyAddress = new MapAddress();
            FindeNeerlyAddress(ref vehicleLocation);

            var sectionsWithinNeerlyAddress = new List<MapSection>();
            FindSectionsWithinNeerlyAddress(vehicleLocation.NeerlyAddress.Id, ref sectionsWithinNeerlyAddress);

            foreach (MapSection mapSection in sectionsWithinNeerlyAddress)
            {
                if (mapHandler.IsPositionInThisSection(mapSection, vehicleLocation.RealPosition))
                {
                    vehicleLocation.LastSection = mapSection;
                    vehicleLocation.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(vehicleLocation.RealPosition, mapSection.HeadAddress.Position);
                    break;
                }
            }

            UpdatePlcVehicleBeamSensor();
        }

        private void FindSectionsWithinNeerlyAddress(string neerlyAddressId, ref List<MapSection> sectionsWithinNeerlyAddress)
        {
            foreach (MapSection mapSection in TheMapInfo.allMapSections.Values)
            {
                if (mapSection.InsideAddresses.FindIndex(z => z.Id == neerlyAddressId) > -1)
                {
                    sectionsWithinNeerlyAddress.Add(mapSection);
                }
            }
        }

        private void FindeNeerlyAddress(ref VehicleLocation vehicleLocation)
        {
            try
            {
                double neerlyDistance = 999999;
                foreach (MapAddress mapAddress in TheMapInfo.allMapAddresses.Values)
                {
                    double dis = mapHandler.GetDistance(vehicleLocation.RealPosition, mapAddress.Position);

                    if (dis < neerlyDistance)
                    {
                        neerlyDistance = dis;
                        vehicleLocation.NeerlyAddress = mapAddress;
                    }
                }
                vehicleLocation.LastAddress = vehicleLocation.NeerlyAddress;
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private bool IsVehlocStayInSameAddress(VehicleLocation vehloc)
        {
            if (!string.IsNullOrEmpty(vehloc.LastAddress.Id) && !string.IsNullOrEmpty(vehloc.LastSection.Id))
            {
                if (mapHandler.IsPositionInThisAddress(vehloc.RealPosition, vehloc.LastAddress.Position))
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdatePlcVehicleBeamSensor()
        {
            var plcVeh = theVehicle.ThePlcVehicle;
            var lastSection = theVehicle.VehicleLocation.LastSection;
            var curDistance = lastSection.VehicleDistanceSinceHead;
            var index = lastSection.BeamSensorDisables.FindIndex(x => x.Min <= curDistance && x.Max >= curDistance);
            if (index > -1)
            {
                var beamDisable = lastSection.BeamSensorDisables[index];
                theVehicle.FrontBeamDisable = beamDisable.FrontDisable;
                theVehicle.BackBeamDisable = beamDisable.BackDisable;
                theVehicle.LeftBeamDisable = beamDisable.LeftDisable;
                theVehicle.RightBeamDisable = beamDisable.RightDisable;
            }
            else
            {
                theVehicle.FrontBeamDisable = false;
                theVehicle.BackBeamDisable = false;
                theVehicle.LeftBeamDisable = false;
                theVehicle.RightBeamDisable = false;
            }
        }

        private void UpdateMiddlerGotReserveOkSections(string id)
        {
            int getReserveOkSectionIndex = 0;
            try
            {
                var getReserveOkSections = middleAgent.GetReserveOkSections();
                getReserveOkSectionIndex = getReserveOkSections.FindIndex(x => x.Id == id);
                if (getReserveOkSectionIndex < 0) return;
                for (int i = 0; i < getReserveOkSectionIndex; i++)
                {
                    //Remove passed section in ReserveOkSection
                    middleAgent.DequeueGotReserveOkSections();
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"MainFlow : UpdateMiddlerGotReserveOkSections FAIL [SecId={id}][Index={getReserveOkSectionIndex}]"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }

        }

        private void StartCharge(MapAddress endAddress)
        {
            var address = endAddress;
            var percentage = theVehicle.ThePlcVehicle.Batterys.Percentage;
            var highPercentage = theVehicle.ThePlcVehicle.Batterys.PortAutoChargeHighSoc;

            if (address.IsCharger)
            {
                if (theVehicle.ThePlcVehicle.Batterys.Charging)
                {
                    var msg = $"車子抵達{address.Id},充電方向為{address.ChargeDirection},因充電狀態為{theVehicle.ThePlcVehicle.Batterys.Charging}, 故暫不再送出充電信號";
                    loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                          , msg));
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
                    loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , msg));
                    OnMessageShowEvent?.Invoke(this, msg);
                }

                middleAgent.ChargHandshaking();
                EnumChargeDirection chargeDirection;
                switch (address.ChargeDirection)
                {
                    case EnumChargeDirection.Left:
                        chargeDirection = EnumChargeDirection.Left;
                        break;
                    case EnumChargeDirection.Right:
                        chargeDirection = EnumChargeDirection.Right;
                        break;
                    case EnumChargeDirection.None:
                    default:
                        alarmHandler.SetAlarm(000012);
                        return;
                }
                plcAgent.ChargeStartCommand(chargeDirection);

                Stopwatch sw = new Stopwatch();
                bool isTimeout = false;
                sw.Start();
                while (true)
                {
                    sw.Stop();
                    if (sw.ElapsedMilliseconds > mainFlowConfig.StartChargeWaitingTimeoutMs)
                    {
                        isTimeout = true;
                        break;
                    }
                    sw.Start();
                    if (theVehicle.ThePlcVehicle.Batterys.Charging)
                    {
                        break;
                    }
                    SpinWait.SpinUntil(() => false, 5);
                }

                if (!isTimeout)
                {
                    middleAgent.Charging();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : 到達站點[{address.Id}]充電中。");
                    batteryLog.ChargeCount++;
                    SaveBatteryLog();
                }
                else
                {
                    alarmHandler.SetAlarm(000013);
                }
            }
        }
        private void LowPowerStartCharge(MapAddress lastAddress)
        {
            var address = lastAddress;
            var percentage = theVehicle.ThePlcVehicle.Batterys.Percentage;
            var lowPercentage = theVehicle.ThePlcVehicle.Batterys.PortAutoChargeLowSoc;
            var pos = theVehicle.VehicleLocation.RealPosition;
            if (address.IsCharger && mapHandler.IsPositionInThisAddress(pos, address.Position))
            {
                if (theVehicle.ThePlcVehicle.Batterys.Charging)
                {
                    var msg = $"車子停在{address.Id}且目前沒有傳送命令,充電方向為{address.ChargeDirection},因充電狀態為{theVehicle.ThePlcVehicle.Batterys.Charging}, 故暫不再送出充電信號";
                    loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                          , msg));
                    //OnMessageShowEvent?.Invoke(this, msg);
                    return;
                }
                else
                {
                    var msg = $"車子停在{address.Id}且目前沒有傳送命令,充電方向為{address.PioDirection},因SOC為{percentage:F2} < {lowPercentage:F2}(自動充電門檻值), 故送出充電信號";
                    //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    //     , msg));
                    OnMessageShowEvent?.Invoke(this, msg);
                }


                middleAgent.ChargHandshaking();
                EnumChargeDirection chargeDirection;
                switch (address.ChargeDirection)
                {
                    case EnumChargeDirection.Left:
                        chargeDirection = EnumChargeDirection.Left;
                        break;
                    case EnumChargeDirection.Right:
                        chargeDirection = EnumChargeDirection.Right;
                        break;
                    case EnumChargeDirection.None:
                    default:
                        alarmHandler.SetAlarm(000012);
                        return;
                }
                plcAgent.ChargeStartCommand(chargeDirection);

                Stopwatch sw = new Stopwatch();
                bool isTimeout = false;
                sw.Start();
                while (true)
                {
                    sw.Stop();
                    if (sw.ElapsedMilliseconds > mainFlowConfig.StartChargeWaitingTimeoutMs)
                    {
                        isTimeout = true;
                        break;
                    }
                    sw.Start();
                    if (theVehicle.ThePlcVehicle.Batterys.Charging)
                    {
                        break;
                    }
                    SpinWait.SpinUntil(() => false, 5);
                }

                if (!isTimeout)
                {
                    middleAgent.Charging();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : 充電中, [Address={address.Id}][IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]");
                    batteryLog.ChargeCount++;
                    SaveBatteryLog();
                }
                else
                {
                    alarmHandler.SetAlarm(000013);
                }
            }
        }

        public bool StopCharge()
        {
            var msg = $"MainFlow : 嘗試停止充電, [IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]";
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                 , msg));

            if (!theVehicle.ThePlcVehicle.Batterys.Charging)
            {
                return true;
            }

            if (!mapHandler.IsPositionInThisAddress(theVehicle.VehicleLocation.RealPosition, theVehicle.VehicleLocation.LastAddress.Position))
            {
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , $"MainFlow : Stop charge fail, RealPos is not in LastAddress [Real=({(int)theVehicle.VehicleLocation.RealPosition.X},{(int)theVehicle.VehicleLocation.RealPosition.Y})][LastAddress={theVehicle.VehicleLocation.LastAddress.Id}]"));
                return true;
            }
            var address = theVehicle.VehicleLocation.LastAddress;
            if (address.IsCharger)
            {
                middleAgent.ChargHandshaking();
                plcAgent.ChargeStopCommand();

                Stopwatch sw = new Stopwatch();
                bool isTimeOut = false;
                sw.Start();
                while (true)
                {
                    sw.Stop();
                    if (sw.ElapsedMilliseconds >= mainFlowConfig.StopChargeWaitingTimeoutMs)
                    {
                        isTimeOut = true;
                        break;
                    }
                    sw.Start();
                    if (!theVehicle.ThePlcVehicle.Batterys.Charging)
                    {
                        break;
                    }
                    SpinWait.SpinUntil(() => false, 5);
                }
                sw.Stop();

                if (!isTimeOut)
                {
                    middleAgent.ChargeOff();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Stop Charge, [IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]");
                    return true;
                }
                else
                {
                    alarmHandler.SetAlarm(000014);
                    StopVehicle();
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public void StopAndClear()
        {
            PauseVisitTransferSteps();
            middleAgent.ClearAllReserve();
            StopVehicle();
            StopVisitTransferSteps();
            theVehicle.VehicleLocation.WheelAngle = moveControlHandler.ControlData.WheelAngle;

            if (agvcTransCmd.PauseStatus == VhStopSingle.StopSingleOn)
            {
                agvcTransCmd.PauseStatus = VhStopSingle.StopSingleOff;
                middleAgent.StatusChangeReport(MethodBase.GetCurrentMethod().Name);
            }

            if (!IsInterlockErrorOrBcrReadFail())
            {
                agvcTransCmd.CompleteStatus = CompleteStatus.CmpStatusVehicleAbort;
            }

            if (theVehicle.ThePlcVehicle.Loading && ReadResult == EnumCstIdReadResult.Noraml)
            {
                string cstId = "";
                plcAgent.triggerCassetteIDReader(ref cstId);
            }

            //if (NeedRename)
            //{
            //    NeedRename = false;
            //    theVehicle.ThePlcVehicle.CassetteId = theVehicle.ThePlcVehicle.RenameCassetteId;
            //}
            //else
            //{
            //    if (theVehicle.ThePlcVehicle.Loading && ReadResult == EnumCstIdReadResult.Noraml)
            //    {
            //        string cstId = "";
            //        plcAgent.triggerCassetteIDReader(ref cstId);
            //    }
            //}
            ReadResult = EnumCstIdReadResult.Noraml;

            var msg = $"MainFlow : Stop And Clear";
            OnMessageShowEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //     , msg));
        }

        private bool IsInterlockErrorOrBcrReadFail()
        {
            return agvcTransCmd.CompleteStatus == CompleteStatus.CmpStatusInterlockError || agvcTransCmd.CompleteStatus == CompleteStatus.CmpStatusIdmisMatch || agvcTransCmd.CompleteStatus == CompleteStatus.CmpStatusIdreadFailed;
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                return EnumTransferStepType.Empty;
            }
        }

        public int GetTransferStepsCount()
        {
            return transferSteps.Count;
        }

        public void StopVehicle()
        {
            moveControlHandler.StopAndClear();
            plcAgent.ClearExecutingForkCommand();
            plcAgent.ChargeStopCommand();

            var msg = $"MainFlow : Stop Vehicle, [MoveState={moveControlHandler.MoveState}][IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public bool SetManualToAuto()
        {
            StopAndClear();
            string reason = "";
            if (!moveControlHandler.MoveControlCanAuto(ref reason))
            {
                reason = $"Manual 切換 Auto 失敗，原因： " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                return false;
            }
            else
            {
                string msg = $"Manual 切換 Auto 成功";
                OnMessageShowEvent?.Invoke(this, msg);
                return true;
            }
        }

        private bool IsMoveStateIdle()
        {
            return moveControlHandler.MoveState == EnumMoveState.Idle;
        }

        public void SetupPlcAutoManualState(EnumIPCStatus status)
        {
            plcAgent.WriteIPCStatus(status);
        }

        public void ResetAllarms()
        {
            alarmHandler.ResetAllAlarms();
        }

        public void SetupTestAgvcTransferCmd()
        {
            transferSteps = new List<TransferStep>();
            Random random = new Random();
            AgvcTransCmd transCmd = new AgvcTransCmd();
            transCmd.CommandId = $"test00{random.Next() % 32767}";
            transCmd.CassetteId = "FakeCst001";
            transCmd.CommandType = EnumAgvcTransCommandType.LoadUnload;
            transCmd.LoadAddressId = "28015";
            transCmd.ToLoadAddressIds = new List<string>();
            transCmd.ToLoadSectionIds = new List<string>();

            transCmd.UnloadAddressId = "20013";
            transCmd.ToUnloadAddressIds = new List<string>();
            transCmd.ToUnloadAddressIds.Add("28015");
            transCmd.ToUnloadAddressIds.Add("48014");
            transCmd.ToUnloadAddressIds.Add("20013");
            transCmd.ToUnloadSectionIds = new List<string>();
            transCmd.ToUnloadSectionIds.Add("0101");
            transCmd.ToUnloadSectionIds.Add("0092");

            MiddleAgent_OnInstallTransferCommandEvent(this, transCmd);
        }

        private void AlarmHandler_OnResetAllAlarmsEvent(object sender, string msg)
        {
            plcAgent.WritePLCAlarmReset();
            plcAgent.SetAlarmWarningReportAllReset();
        }

        private void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            if (alarmHandler.HasAlarm && !IsCancelByAgvcCmd())
            {
                //middleAgent.PauseAskReserve();
                //PauseVisitTransferSteps();
                //if (!IsVehicleAbortByAlarm)
                //{
                //    IsVehicleAbortByAlarm = true;
                //}
            }
            if (alarm.PlcWord != 0 || alarm.PlcBit != 0)
            {
                plcAgent.WriteAlarmWarningReport(alarm.Level, alarm.PlcWord, alarm.PlcBit, true);
                plcAgent.WriteAlarmWarningStatus(alarmHandler.HasAlarm, alarmHandler.HasWarn);
            }
        }

        private bool IsCancelByAgvcCmd() => agvcTransCmd.CompleteStatus == CompleteStatus.CmpStatusAbort || agvcTransCmd.CompleteStatus == CompleteStatus.CmpStatusCancel || agvcTransCmd.CompleteStatus == CompleteStatus.CmpStatusIdmisMatch || agvcTransCmd.CompleteStatus == CompleteStatus.CmpStatusIdreadFailed;

        public void SetupVehicleSoc(double percentage)
        {
            plcAgent.setSOC(percentage);
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public void RenameCstId(string newCstId)
        {
            theVehicle.ThePlcVehicle.CassetteId = newCstId;
            theVehicle.CurAgvcTransCmd.CassetteId = newCstId;
            if (transferSteps.Count > 0)
            {
                agvcTransCmd.CassetteId = newCstId;
                foreach (var transferStep in transferSteps)
                {
                    transferStep.CstId = newCstId;
                }
            }
        }

        public void Middler_OnCmdPauseEvent(ushort iSeqNum, PauseEvent type)
        {
            try
            {
                middleAgent.PauseAskReserve();
                PauseVisitTransferSteps();
                if (IsMoveStep())
                {
                    if (moveControlHandler.VehclePause())
                    {
                        var msg = $"MainFlow : 接受[{type}]命令。";
                        OnMessageShowEvent(this, msg);
                        middleAgent.PauseReply(iSeqNum, 0, PauseEvent.Pause);
                        if (agvcTransCmd.PauseStatus == VhStopSingle.StopSingleOff)
                        {
                            agvcTransCmd.PauseStatus = VhStopSingle.StopSingleOn;
                            middleAgent.StatusChangeReport(MethodBase.GetCurrentMethod().Name);
                        }
                    }
                    else
                    {
                        var msg = $"MainFlow : 移動無法暫停，拒絕[{type}]命令。";
                        OnMessageShowEvent(this, msg);
                        middleAgent.PauseReply(iSeqNum, 1, PauseEvent.Pause);
                        middleAgent.ResumeAskReserve();
                        ResumeVisitTransferSteps();
                    }
                }
                else
                {
                    var msg = $"MainFlow : 接受[{type}]命令。";
                    OnMessageShowEvent(this, msg);
                    middleAgent.PauseReply(iSeqNum, 0, PauseEvent.Pause);
                    if (agvcTransCmd.PauseStatus == VhStopSingle.StopSingleOff)
                    {
                        agvcTransCmd.PauseStatus = VhStopSingle.StopSingleOn;
                        middleAgent.StatusChangeReport(MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public void Middler_OnCmdResumeEvent(ushort iSeqNum, PauseEvent type, RepeatedField<ReserveInfo> reserveInfos)
        {
            try
            {
                if (IsMoveStep())
                {
                    if (IsMoveControllPause())
                    {
                        var msg = $"MainFlow : 接受[{type}]命令。";
                        OnMessageShowEvent(this, msg);
                        middleAgent.PauseReply(iSeqNum, 0, PauseEvent.Continue);
                        moveControlHandler.VehcleContinue();
                        ResumeVisitTransferSteps();
                        middleAgent.ResumeAskReserve();
                        IsMoveEnd = false;
                        if (agvcTransCmd.PauseStatus == VhStopSingle.StopSingleOn)
                        {
                            agvcTransCmd.PauseStatus = VhStopSingle.StopSingleOff;
                            middleAgent.StatusChangeReport(MethodBase.GetCurrentMethod().Name);
                        }
                    }
                    else
                    {
                        var msg = $"MainFlow : 移動尚未暫停，拒絕[{type}]命令。";
                        OnMessageShowEvent(this, msg);
                        middleAgent.PauseReply(iSeqNum, 1, PauseEvent.Continue);
                    }
                }
                else
                {
                    var msg = $"MainFlow : 接受[{type}]命令。";
                    OnMessageShowEvent(this, msg);
                    middleAgent.PauseReply(iSeqNum, 0, PauseEvent.Continue);
                    middleAgent.ResumeAskReserve();
                    ResumeVisitTransferSteps();
                    if (agvcTransCmd.PauseStatus == VhStopSingle.StopSingleOn)
                    {
                        agvcTransCmd.PauseStatus = VhStopSingle.StopSingleOff;
                        middleAgent.StatusChangeReport(MethodBase.GetCurrentMethod().Name);
                    }
                }

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        private bool IsMoveControllPause() => moveControlHandler.ControlData.PauseRequest || moveControlHandler.ControlData.PauseAlready;

        private void UpdateMiddlerNeedReserveSections(string reserveSectionID)
        {
            var needReserveSections = middleAgent.GetNeedReserveSections();
            var index = needReserveSections.FindIndex(x => x.Id == reserveSectionID);
            if (index > -1)
            {
                needReserveSections.RemoveAt(index);
                middleAgent.SetupNeedReserveSections(needReserveSections);
            }
        }

        public void Middler_OnCmdCancelAbortEvent(ushort iSeqNum, string cmdId, CMDCancelType actType)
        {
            try
            {
                if (IsMoveStep())
                {
                    if (IsMoveControllPause())
                    {
                        var msg = $"MainFlow : 接受[{actType}]命令。";
                        OnMessageShowEvent(this, msg);
                        middleAgent.CancelAbortReply(iSeqNum, 0, cmdId, actType);

                        moveControlHandler.VehcleCancel();
                        //middleAgent.StopAskReserve();
                        middleAgent.ClearAllReserve();
                        agvcTransCmd.CompleteStatus = actType == CMDCancelType.CmdAbort ? CompleteStatus.CmpStatusAbort : CompleteStatus.CmpStatusCancel;
                        StopVisitTransferSteps();
                        var msg2 = $"MainFlow : 接受[{actType}]命令確認。";
                        OnMessageShowEvent(this, msg2);
                    }
                    else
                    {
                        var msg = $"MainFlow : 移動尚未暫停，拒絕[{actType}]命令。";
                        OnMessageShowEvent(this, msg);
                        middleAgent.CancelAbortReply(iSeqNum, 1, cmdId, actType);
                    }
                }
                else
                {
                    if (IsVisitTransferStepsPause())
                    {
                        var msg = $"MainFlow : 接受[{actType}]命令。";
                        OnMessageShowEvent(this, msg);
                        middleAgent.CancelAbortReply(iSeqNum, 0, cmdId, actType);

                        //middleAgent.StopAskReserve();
                        middleAgent.ClearAllReserve();
                        agvcTransCmd.CompleteStatus = actType == CMDCancelType.CmdAbort ? CompleteStatus.CmpStatusAbort : CompleteStatus.CmpStatusCancel;
                        StopVisitTransferSteps();
                        var msg2 = $"MainFlow : 接受[{actType}]命令確認。";
                        OnMessageShowEvent(this, msg2);
                    }
                    else
                    {
                        var msg = $"MainFlow : 流程尚未暫停，拒絕[{actType}]命令。";
                        OnMessageShowEvent(this, msg);
                        middleAgent.CancelAbortReply(iSeqNum, 1, cmdId, actType);
                    }
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
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
            loggerAgent.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", msg));
        }

        public int GetCurWheelAngle()
        {
            return moveControlHandler.ControlData.WheelAngle;
        }

        public int GetCurVehicleAngle()
        {
            return (int)theVehicle.VehicleLocation.VehicleAngle; //車頭方向角度(0,90,180,-90)  
        }


        public void LoadMainFlowConfig()
        {
            XmlHandler xmlHandler = new XmlHandler();

            mainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(@"D:\AgvConfigs\MainFlow.xml");
        }

        public void SetMainFlowConfig(MainFlowConfig mainFlowConfig)
        {
            this.mainFlowConfig = mainFlowConfig;
            XmlHandler xmlHandler = new XmlHandler();
            xmlHandler.WriteXml(mainFlowConfig, @"D:\AgvConfigs\MainFlow.xml");
        }

        public void LoadMiddlerConfig()
        {
            XmlHandler xmlHandler = new XmlHandler();

            middlerConfig = xmlHandler.ReadXml<MiddlerConfig>(@"D:\AgvConfigs\Middler.xml");
        }

        public void SetMiddlerConfig(MiddlerConfig middlerConfig)
        {
            this.middlerConfig = middlerConfig;
            XmlHandler xmlHandler = new XmlHandler();
            xmlHandler.WriteXml(middlerConfig, @"D:\AgvConfigs\Middler.xml");
        }

        private void PlcAgent_OnBatteryPercentageChangeEvent(object sender, ushort batteryPercentage)
        {
            batteryLog.InitialSoc = batteryPercentage;
            SaveBatteryLog();
        }

        public void SaveBatteryLog()
        {
            XmlHandler xmlHandler = new XmlHandler();
            xmlHandler.WriteXml(batteryLog, @"D:\AgvConfigs\BatteryLog.xml");
        }

        public void ResetBatteryLog()
        {
            BatteryLog tempBatteryLog = new BatteryLog();
            tempBatteryLog.ResetTime = DateTime.Now.ToString("yyyy/MM/dd_HH:mm:ss.fff");
            tempBatteryLog.InitialSoc = batteryLog.InitialSoc;
            batteryLog = tempBatteryLog;
            //TODO: Middler

        }       
    }
}
