using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferSteps;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mirle.Agv.Controller.Handler.TransCmdsSteps;
using TcpIpClientSample;
using System.Reflection;
using System.Linq;
using ClsMCProtocol;
using System.Diagnostics;
using Google.Protobuf.Collections;

namespace Mirle.Agv.Controller
{
    public class MainFlowHandler
    {
        #region Configs
        private MiddlerConfig middlerConfig;
        private MainFlowConfig mainFlowConfig;
        private MapConfig mapConfig;
        private AlarmConfig alarmConfig;
        #endregion

        #region TransCmds
        private List<TransferStep> transferSteps = new List<TransferStep>();
        private List<TransferStep> lastTransferSteps = new List<TransferStep>();

        public bool GoNextTransferStep { get; set; }
        public int TransferStepsIndex { get; set; }


        public bool IsReportingPosition { get; set; }
        public bool IsReserveMechanism { get; set; } = true;
        private ITransferStatus transferStatus;
        private AgvcTransCmd agvcTransCmd = new AgvcTransCmd();
        private AgvcTransCmd lastAgvcTransCmd = new AgvcTransCmd();
        public MapSection SectionHasFoundPosition { get; set; } = new MapSection();
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
        public event EventHandler<string> OnUpdateLoadPortEvent;
        public event EventHandler<string> OnUpdateUnloadPortEvent;
        #endregion

        #region Models
        public Vehicle theVehicle;
        private bool isIniOk;
        public MapInfo TheMapInfo { get; private set; } = new MapInfo();
        private MCProtocol mcProtocol;
        public ushort ForkCommandNumber { get; set; } = 0;
        public PlcForkCommand PlcForkLoadCommand { get; set; }
        public PlcForkCommand PlcForkUnloadCommand { get; set; }
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

                mainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>("MainFlow.xml");
                LoggerAgent.LogConfigPath = mainFlowConfig.LogConfigPath;
                mapConfig = xmlHandler.ReadXml<MapConfig>("Map.xml");
                middlerConfig = xmlHandler.ReadXml<MiddlerConfig>("Middler.xml");
                alarmConfig = xmlHandler.ReadXml<AlarmConfig>("Alarm.xml");

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
                alarmHandler = new AlarmHandler(alarmConfig);
                mapHandler = new MapHandler(mapConfig);
                TheMapInfo = mapHandler.TheMapInfo;

                middleAgent = new MiddleAgent(this);
                mcProtocol = new MCProtocol();
                mcProtocol.Name = "MCProtocol";
                plcAgent = new PlcAgent(mcProtocol, alarmHandler);

                moveControlHandler = new MoveControlHandler(TheMapInfo, alarmHandler);


                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "控制層"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "控制層"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void VehicleInitial()
        {
            try
            {
                theVehicle = Vehicle.Instance;
                theVehicle.CurAgvcTransCmd = agvcTransCmd;
                theVehicle.LastAgvcTransCmd = lastAgvcTransCmd;
                theVehicle.CurVehiclePosition.RealPositionRangeMm = mainFlowConfig.RealPositionRangeMm;
                theVehicle.TheMapInfo = TheMapInfo;
                theVehicle.ThdMiddleAgent = middleAgent;
                SetupFakeVehicleSoc(70);

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "台車"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "台車"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'mapHandler
                middleAgent.OnInstallTransferCommandEvent += MiddleAgent_OnInstallTransferCommandEvent;

                //來自MiddleAgent的取得Reserve/BlockZone訊息，通知MainFlow(this)
                middleAgent.OnGetBlockPassEvent += MiddleAgent_OnGetBlockPassEvent;

                //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                moveControlHandler.OnMoveFinished += MoveControlHandler_OnMoveFinished;

                //來自PlcAgent的取放貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                plcAgent.OnForkCommandFinishEvent += PlcAgent_OnForkCommandFinishEvent;

                //來自PlcBattery的電量改變訊息，通知middleAgent
                plcAgent.OnBatteryPercentageChangeEvent += middleAgent.PlcAgent_OnBatteryPercentageChangeEvent;

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

                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void VehicleLocationInitial()
        {
            if (IsRealPositionEmpty())
            {
                try
                {
                    theVehicle.CurVehiclePosition.RealPosition = TheMapInfo.allMapAddresses.First(x => x.Key != "").Value.Position.DeepClone();
                }
                catch (Exception ex)
                {
                    theVehicle.CurVehiclePosition.RealPosition = new MapPosition();
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
            }
            StartTrackPosition();
            StartWatchLowPower();
        }
        private bool IsRealPositionEmpty()
        {
            if (theVehicle.CurVehiclePosition.RealPosition == null)
            {
                return true;
            }

            if (theVehicle.CurVehiclePosition.RealPosition.X == 0 && theVehicle.CurVehiclePosition.RealPosition.Y == 0)
            {
                return true;
            }

            return false;
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
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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

            var msg = $"MainFlow : Start Visit TransferStep, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                 , msg));
        }
        public void PauseVisitTransferSteps()
        {
            visitTransferStepsPauseEvent.Reset();
            VisitTransferStepsStatusBeforePause = VisitTransferStepsStatus;
            VisitTransferStepsStatus = EnumThreadStatus.Pause;

            var msg = $"MainFlow : Pause Visit TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        public void ResumeVisitTransferSteps()
        {
            visitTransferStepsPauseEvent.Set();
            VisitTransferStepsStatus = VisitTransferStepsStatusBeforePause;
            var msg = $"MainFlow : Resume Visit TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        public void StopVisitTransferSteps()
        {
            if (alarmHandler.HasAlarm)
            {
                middleAgent.ErrorComplete();
            }

            visitTransferStepsShutdownEvent.Set();
            visitTransferStepsPauseEvent.Set();
            if (VisitTransferStepsStatus != EnumThreadStatus.None)
            {
                VisitTransferStepsStatus = EnumThreadStatus.Stop;
            }

            var msg = $"MainFlow : Stop Visit TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
              , msg));
        }
        private void PreVisitTransferSteps()
        {

            TransferStepsIndex = 0;
            theVehicle.CurTrasferStep = GetCurTransferStep();
            GoNextTransferStep = true;
            middleAgent.Commanding();

            var msg = $"MainFlow : Pre Visit TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        private void AfterVisitTransferSteps(long total)
        {
            VisitTransferStepsStatus = EnumThreadStatus.None;
            lastAgvcTransCmd = agvcTransCmd;
            agvcTransCmd = new AgvcTransCmd();
            //agvcTransCmd = null;
            lastTransferSteps = transferSteps;
            transferSteps = new List<TransferStep>();
            TransferStepsIndex = 0;

            theVehicle.LastAgvcTransCmd = lastAgvcTransCmd;
            theVehicle.CurAgvcTransCmd = agvcTransCmd;
            theVehicle.CurTrasferStep = GetCurTransferStep();

            GoNextTransferStep = false;
            SetTransCmdsStep(new Idle());
            middleAgent.NoCommand();
            //if (theVehicle.AutoState == EnumAutoState.Auto && IsWatchLowPowerStop())
            //{
            //    StartWatchLowPower();
            //}

            var msg = $"MainFlow : After Visit TransferSteps, [ThreadStatus={VisitTransferStepsStatus}][TotalSpendMs={total}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        #endregion

        #region Thd Watch LowPower
        private void WatchLowPower()
        {
            Stopwatch sw = new Stopwatch();
            long total = 0;
            while (transferSteps.Count == 0)
            {
                try
                {
                    sw.Restart();

                    #region Pause And Stop Check
                    watchLowPowerPauseEvent.WaitOne(Timeout.Infinite);
                    if (watchLowPowerShutdownEvent.WaitOne(0)) break;
                    #endregion

                    WatchLowPowerStatus = EnumThreadStatus.Working;

                    if (theVehicle.AutoState == EnumAutoState.Auto && transferSteps.Count > 0)
                    {
                        if (IsLowPower())
                        {
                            StartCharge();
                        }
                    }                   
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
            var msg = $"MainFlow : Start Watch Low-Power, [Power={batterys.Percentage}][LowSocGap={batterys.PortAutoChargeLowSoc}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                 , msg));
        }
        public void PauseWatchLowPower()
        {
            watchLowPowerPauseEvent.Reset();
            PreWatchLowPowerStatus = WatchLowPowerStatus;
            WatchLowPowerStatus = EnumThreadStatus.Pause;
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            var msg = $"MainFlow : Pause Watch Low-Power, [Power={batterys.Percentage}][LowSocGap={batterys.PortAutoChargeLowSoc}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));

        }
        public void ResumeWatchLowPower()
        {
            watchLowPowerPauseEvent.Set();
            var tempStatus = WatchLowPowerStatus;
            WatchLowPowerStatus = PreWatchLowPowerStatus;
            PreWatchLowPowerStatus = tempStatus;
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            var msg = $"MainFlow : Resume Watch Low-Power, [Power={batterys.Percentage}][LowSocGap={batterys.PortAutoChargeLowSoc}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        public void StopWatchLowPower()
        {
            if (WatchLowPowerStatus != EnumThreadStatus.None)
            {
                WatchLowPowerStatus = EnumThreadStatus.Stop;
            }
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            var msg = $"MainFlow : Stop Watch Low-Power, [Power={batterys.Percentage}][LowSocGap={batterys.PortAutoChargeLowSoc}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
              , msg));

            watchLowPowerShutdownEvent.Set();
            watchLowPowerPauseEvent.Set();
        }
        public void AfterWatchLowPower(long total)
        {
            WatchLowPowerStatus = EnumThreadStatus.None;
            var msg = $"MainFlow : After Watch LowPower, [ThreadStatus={WatchLowPowerStatus}][TotalSpendMs={total}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
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

                    var position = theVehicle.CurVehiclePosition.RealPosition;
                    if (position == null) continue;

                    if (mapHandler.IsPositionInThisAddress(position, theVehicle.CurVehiclePosition.LastAddress.Position))
                    {
                        continue;
                    }
                    if (transferSteps.Count > 0)
                    {
                        //有搬送命令時，比對當前Position與搬送路徑Sections計算LastSection/LastAddress/Distance
                        var curTransStep = GetCurTransferStep();
                        if (IsMoveStep())
                        {
                            MoveCmdInfo moveCmd = (MoveCmdInfo)curTransStep;
                            UpdateVehiclePositionWithMoveCmd(moveCmd, position);
                        }
                    }
                    else
                    {
                        //無搬送命令時，比對當前Position與全地圖Sections確定section-distance
                        UpdateVehiclePositionWithoutMoveCmd(position);
                    }
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Start Track Position, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }
        public void PauseTrackPosition()
        {
            trackPositionPauseEvent.Reset();
            PreTrackPositionStatus = TrackPositionStatus;
            TrackPositionStatus = EnumThreadStatus.Pause;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Pause Track Position, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }
        public void ResumeTrackPosition()
        {
            trackPositionPauseEvent.Set();
            var tempStatus = TrackPositionStatus;
            TrackPositionStatus = PreTrackPositionStatus;
            PreTrackPositionStatus = tempStatus;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Resume Track Position, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }
        public void StopTrackPosition()
        {
            trackPositionShutdownEvent.Set();
            trackPositionPauseEvent.Set();
            if (TrackPositionStatus != EnumThreadStatus.None)
            {
                TrackPositionStatus = EnumThreadStatus.Stop;
            }

            OnMessageShowEvent?.Invoke(this, $"MainFlow : Stop Track Position, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }
        private void AfterTrackPosition(long total)
        {
            TrackPositionStatus = EnumThreadStatus.None;
            var msg = $"MainFlow : After Track Position, [ThreadStatus={TrackPositionStatus}][TotalSpendMs={total}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        #endregion

        #region Handle Transfer Command
        private void MiddleAgent_OnInstallTransferCommandEvent(object sender, AgvcTransCmd agvcTransCmd)
        {
            var msg = $"MainFlow : Get Middler TransferCommand, [CmdId={agvcTransCmd.CommandId}][CmdType={agvcTransCmd.CommandType}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));

            try
            {
                if (!IsAgvcTransferCommandEmpty())
                {
                    alarmHandler.SetAlarm(000001);
                    middleAgent.ReplyTransferCommand(agvcTransCmd.SeqNum, 1, "Agv already have a transfer command.");
                    return;
                }

                if (theVehicle.ThePlcVehicle.Loading)
                {
                    var cstId = "";
                    plcAgent.triggerCassetteIDReader(ref cstId);
                }

                if (IsVehicleAlreadyHaveCstCannotLoad(agvcTransCmd.CommandType))
                {
                    alarmHandler.SetAlarm(000016);
                    middleAgent.ReplyTransferCommand(agvcTransCmd.SeqNum, 1, "Agv already have a cst cannot load.");
                    return;
                }

                if (IsVehicleHaveNoCstCannotUnload(agvcTransCmd.CommandType))
                {
                    alarmHandler.SetAlarm(000017);
                    middleAgent.ReplyTransferCommand(agvcTransCmd.SeqNum, 1, "Agv have no cst cannot unload.");
                    return;
                }

                if (!IsAgvcCommandMatchTheMap(agvcTransCmd))
                {
                    alarmHandler.SetAlarm(000018);
                    middleAgent.ReplyTransferCommand(agvcTransCmd.SeqNum, 1, "Guide sections and address are not match the map.");
                    return;
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));

                middleAgent.ReplyTransferCommand(agvcTransCmd.SeqNum, 1, "Guide sections and address are not match the map.");
                return;
            }


            try
            {
                middleAgent.ReplyTransferCommand(agvcTransCmd.SeqNum, 0, "");
                this.agvcTransCmd = agvcTransCmd;
                theVehicle.CurAgvcTransCmd = agvcTransCmd;
                //StopWatchLowPower();
                SetupTransferSteps();
                transferSteps.Add(new EmptyTransferStep());
                //開始尋訪 trasnferSteps as List<TrasnferStep> 裡的每一步MoveCmdInfo/LoadCmdInfo/UnloadCmdInfo
                StartVisitTransferSteps();
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private bool IsAgvcCommandMatchTheMap(AgvcTransCmd agvcTransCmd)
        {
            var curPos = theVehicle.CurVehiclePosition.RealPosition;
            switch (agvcTransCmd.CommandType)
            {
                case EnumAgvcTransCommandType.Move:
                case EnumAgvcTransCommandType.MoveToCharger:
                case EnumAgvcTransCommandType.Unload:
                    return IsAgvcCommandMatchTheMap(curPos, curPos, agvcTransCmd.ToUnloadSectionIds, agvcTransCmd.ToUnloadAddressIds, agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandType);
                case EnumAgvcTransCommandType.Load:
                    return IsAgvcCommandMatchTheMap(curPos, curPos, agvcTransCmd.ToLoadSectionIds, agvcTransCmd.ToLoadAddressIds, agvcTransCmd.LoadAddressId, agvcTransCmd.CommandType);
                case EnumAgvcTransCommandType.LoadUnload:
                    var canMoveToLoad = IsAgvcCommandMatchTheMap(curPos, curPos, agvcTransCmd.ToLoadSectionIds, agvcTransCmd.ToLoadAddressIds, agvcTransCmd.LoadAddressId, agvcTransCmd.CommandType);
                    var loadPos = TheMapInfo.allMapAddresses[agvcTransCmd.LoadAddressId].Position;
                    var canMoveToUnLoad = IsAgvcCommandMatchTheMap(loadPos, loadPos, agvcTransCmd.ToUnloadSectionIds, agvcTransCmd.ToUnloadAddressIds, agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandType);
                    return canMoveToLoad && canMoveToUnLoad;
                case EnumAgvcTransCommandType.Home:
                    break;
                case EnumAgvcTransCommandType.Override:
                    break;
                case EnumAgvcTransCommandType.Else:
                    break;
                default:
                    break;
            }

            return true;
        }

        private bool IsAgvcCommandMatchTheMap(MapPosition moveFirstPosition, MapPosition insituPosition, List<string> toSectionIds, List<string> toAddressIds, string endAddressId, EnumAgvcTransCommandType commandType)
        {
            if (toSectionIds.Count > 0)
            {
                if (!TheMapInfo.allMapSections.ContainsKey(toSectionIds[0]))
                {
                    var msg = $"MainFlow : Is Agvc Command Match The Map +++FAIL+++,[CommandType={commandType}][{toSectionIds[0]} is not in the map]";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , msg));
                    return false;
                }
                if (!mapHandler.IsPositionInThisSection(moveFirstPosition, TheMapInfo.allMapSections[toSectionIds[0]]))
                {
                    var msg = $"MainFlow : Is Agvc Command Match The Map +++FAIL+++,[CommandType={commandType}][curPosition is not in first section {toSectionIds[0]}]";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , msg));
                    return false;
                }
                if (!CheckSectionIdsAndAddressIds(toSectionIds, toAddressIds, endAddressId, commandType))
                {
                    var msg = $"MainFlow : Is Agvc Command Match The Map +++FAIL+++,[CommandType={commandType}]";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , msg));
                    return false;
                }
            }
            else
            {
                if (!CheckInSituSectionIdAndAddressId(insituPosition, toSectionIds, toAddressIds, endAddressId, commandType))
                {
                    var msg = $"MainFlow : Is Agvc Command Match The Map +++FAIL+++,[CommandType={commandType}][InSitu]";
                    OnMessageShowEvent?.Invoke(this, msg);
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , msg));
                    return false;
                }
            }

            return true;
        }

        private bool CheckInSituSectionIdAndAddressId(MapPosition insituPosition, List<string> sectionIds, List<string> addressIds, string lastAddress, EnumAgvcTransCommandType type)
        {
            //測 AddressIds 為空
            if (addressIds.Count > 0)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"MainFlow : CheckInSituSectionIdAndAddressId +++FAIL+++, [{type}][InSitu] Address is not empty."));
                return false;
            }

            //測 終點存在於圖資
            if (!TheMapInfo.allMapAddresses.ContainsKey(lastAddress))
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"MainFlow : CheckInSituSectionIdAndAddressId +++FAIL+++, [{type}][InSitu] Address {lastAddress} is not in the map."));
                return false;
            }

            //測 現在還在終點
            if (!mapHandler.IsPositionInThisAddress(insituPosition, TheMapInfo.allMapAddresses[lastAddress].Position))
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
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
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                      , $"MainFlow : CheckSectionIdsAndAddressIds +++FAIL+++, [{type}] Section {id} is not in the map."));
                    return false;
                }
            }

            //測addressIds存在
            foreach (var id in addressIds)
            {
                if (!TheMapInfo.allMapAddresses.ContainsKey(id))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                      , $"MainFlow : CheckSectionIdsAndAddressIds +++FAIL+++, [{type}] Address {id} is not in the map."));
                    return false;
                }
            }

            //測AddressId 屬於 SectionId 內
            for (int i = 0; i < sectionIds.Count; i++)
            {
                var section = TheMapInfo.allMapSections[sectionIds[i]];
                if (!IsAddressIdInMapSection(addressIds[i], section))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"MainFlow : CheckSectionIdsAndAddressIds +++FAIL+++, [{type}] the Address {addressIds[i]} is not in Section {section.Id}"));
                    return false;
                }

                if (!IsAddressIdInMapSection(addressIds[i + 1], section))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"MainFlow : CheckSectionIdsAndAddressIds +++FAIL+++, [{type}] the Address {addressIds[i + 1]} is not in Section {section.Id}"));
                    return false;
                }
            }

            //測相鄰ToUnloadSection共有ToUnloadAddress
            for (int i = 1; i < addressIds.Count - 1; i++)
            {
                var preSection = TheMapInfo.allMapSections[sectionIds[i - 1]];
                if (!IsAddressIdInMapSection(addressIds[i], preSection))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"MainFlow : CheckSectionIdsAndAddressIds +++FAIL+++, [{type}] the Address {addressIds[i]} is not in Section {preSection.Id}"));
                    return false;
                }

                var nextSection = TheMapInfo.allMapSections[sectionIds[i]];
                if (!IsAddressIdInMapSection(addressIds[i], nextSection))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"MainFlow : CheckSectionIdsAndAddressIds +++FAIL+++,  [{type}] the Address {addressIds[i]} is not in Section {nextSection.Id}"));
                    return false;
                }
            }

            //測UnloadAddressId 屬於 最後一個ToUnloadSection
            var lastSection = TheMapInfo.allMapSections[sectionIds[sectionIds.Count - 1]];
            if (!IsAddressIdInMapSection(lastAddressId, lastSection))
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"MainFlow : CheckSectionIdsAndAddressIds +++FAIL+++,  [{type}] the Address {lastAddressId} is not in Section {lastSection.Id}"));
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
                case EnumAgvcTransCommandType.Home:
                    ConvertAgvcHomeCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.Override:               
                    break;           
                case EnumAgvcTransCommandType.Else:
                default:
                    ConvertAgvcElseCmdIntoList(agvcTransCmd);
                    break;
            }
        }
        private void ConvertAgvcElseCmdIntoList(AgvcTransCmd agvcTransCmd)
        {

        }       
        private void ConvertAgvcHomeCmdIntoList(AgvcTransCmd agvcTransCmd)
        {

        }
        private void ConvertAgvcLoadUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            ConvertAgvcLoadCmdIntoList(agvcTransCmd);
            ConvertAgvcNextUnloadCmdIntoList(agvcTransCmd);
        }
        private void ConvertAgvcUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(moveCmd);

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }
        private void ConvertAgvcNextUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = GetMoveToNextUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(moveCmd);

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }
        private void ConvertAgvcLoadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = GetMoveToLoadCmdInfo(agvcTransCmd);
            transferSteps.Add(moveCmd);

            LoadCmdInfo loadCmd = GetLoadCmdInfo(agvcTransCmd);
            transferSteps.Add(loadCmd);
        }
        private MoveToChargerCmdInfo GetMoveToChargerCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveToChargerCmdInfo moveCmd = new MoveToChargerCmdInfo(this);
            moveCmd.CmdId = agvcTransCmd.CommandId;
            moveCmd.CstId = agvcTransCmd.CassetteId;
            moveCmd.AddressIds = agvcTransCmd.ToUnloadAddressIds;
            moveCmd.SectionIds = agvcTransCmd.ToUnloadSectionIds;
            moveCmd.EndAddressId = agvcTransCmd.UnloadAddressId;
            moveCmd.SetupMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            moveCmd.SetupAddressPositions();
            moveCmd.SetupAddressActions();
            moveCmd.SetupSectionSpeedLimits();
            moveCmd.SetupInfo();
            OnMessageShowEvent(this, moveCmd.Info);
            return moveCmd;
        }
        private MoveCmdInfo GetMoveToUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(this);
            moveCmd.CmdId = agvcTransCmd.CommandId;
            moveCmd.CstId = agvcTransCmd.CassetteId;
            moveCmd.AddressIds = agvcTransCmd.ToUnloadAddressIds;
            moveCmd.SectionIds = agvcTransCmd.ToUnloadSectionIds;
            moveCmd.EndAddressId = agvcTransCmd.UnloadAddressId;
            moveCmd.SetupMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            moveCmd.SetupAddressPositions();
            moveCmd.SetupAddressActions();
            moveCmd.SetupSectionSpeedLimits();
            moveCmd.SetupInfo();
            OnMessageShowEvent(this, moveCmd.Info);
            return moveCmd;
        }
        private MoveCmdInfo GetMoveToNextUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(this);
            moveCmd.CmdId = agvcTransCmd.CommandId;
            moveCmd.CstId = agvcTransCmd.CassetteId;
            moveCmd.AddressIds = agvcTransCmd.ToUnloadAddressIds;
            moveCmd.SectionIds = agvcTransCmd.ToUnloadSectionIds;
            moveCmd.EndAddressId = agvcTransCmd.UnloadAddressId;
            moveCmd.StartAddressId = agvcTransCmd.LoadAddressId;
            moveCmd.SetupMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            moveCmd.SetupNextUnloadAddressPositions();
            moveCmd.SetupAddressActions();
            moveCmd.SetupSectionSpeedLimits();
            moveCmd.SetupInfo();
            OnMessageShowEvent(this, moveCmd.Info);
            return moveCmd;
        }
        private MoveCmdInfo GetMoveToLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(this);
            moveCmd.CmdId = agvcTransCmd.CommandId;
            moveCmd.CstId = agvcTransCmd.CassetteId;
            moveCmd.AddressIds = agvcTransCmd.ToLoadAddressIds;
            moveCmd.SectionIds = agvcTransCmd.ToLoadSectionIds;
            moveCmd.EndAddressId = agvcTransCmd.LoadAddressId;
            moveCmd.SetupMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            moveCmd.SetupAddressPositions();
            moveCmd.SetupAddressActions();
            moveCmd.SetupSectionSpeedLimits();
            moveCmd.SetupInfo();
            OnMessageShowEvent(this, moveCmd.Info);
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
        public AgvcTransCmd GetAgvcTransCmd()
        {
            return agvcTransCmd;
        }
        private void ConvertAgvcMoveCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(moveCmd);
        }
        private void ConvertAgvcMoveToChargerCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            MoveToChargerCmdInfo moveToChargerCmd = GetMoveToChargerCmdInfo(agvcTransCmd);
            transferSteps.Add(moveToChargerCmd);
        }
        #endregion

        public void IdleVisitNext()
        {
            var msg = $"MainFlow : Idle Visit Next TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
            TransferStepsIndex++;
            theVehicle.CurTrasferStep = GetCurTransferStep();
        }
        private void MiddleAgent_OnGetBlockPassEvent(object sender, bool e)
        {
            //throw new NotImplementedException();
        }
        private bool IsUnloadArrival()
        {
            // 判斷當前是否可載貨 若否 則發送報告
            var curAddress = theVehicle.CurVehiclePosition.LastAddress;
            var unloadAddressId = agvcTransCmd.UnloadAddressId;
            if (curAddress.Id == unloadAddressId)
            {
                middleAgent.UnloadArrivals();
                var msg = $"MainFlow : UnloadArrvial, [curAddressPos=({(int)curAddress.Position.X},{(int)curAddress.Position.Y})][unloadAddressId={unloadAddressId}]";
                OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , msg));
                return true;
            }
            else
            {
                alarmHandler.SetAlarm(000009);
                return false;
            }
        }
        private bool IsLoadArrival()
        {
            // 判斷當前是否可卸貨 若否 則發送報告
            var curAddress = theVehicle.CurVehiclePosition.LastAddress;
            var loadAddressId = agvcTransCmd.LoadAddressId;

            if (curAddress.Id == loadAddressId)
            {
                middleAgent.LoadArrivals();
                var msg = $"MainFlow : LoadArrival, [curAddressPos=({(int)curAddress.Position.X},{(int)curAddress.Position.Y})][loadAddressId={loadAddressId}]";
                OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , msg));
                return true;
            }
            else
            {
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
            }

            return result;
        }
        private bool CanCassetteIdRead()
        {
            var plcVeh = theVehicle.ThePlcVehicle;
            // 判斷當前貨物的ID是否可正確讀取 若否 則發送報告
            if (!plcVeh.Loading)
            {
                alarmHandler.SetAlarm(000003);
                return false;
            }
            else if (string.IsNullOrEmpty(plcVeh.CassetteId))
            {
                //CassetteId is null or Empty
                alarmHandler.SetAlarm(000004);
                return false;
            }
            else if (plcVeh.CassetteId == "ERROR")
            {
                //CassetteId is Error
                alarmHandler.SetAlarm(000005);
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool IsAgvcTransferCommandEmpty()
        {
            return agvcTransCmd.CommandId == "";
        }
        #endregion

        public void UpdateMoveControlReserveOkPositions(MapSection aReserveOkSection)
        {
            MoveCmdInfo moveCmd = (MoveCmdInfo)GetCurTransferStep();
            var reserveOkSection = moveCmd.MovingSections.Find(x => x.Id == aReserveOkSection.Id);

            if (reserveOkSection == null)
            {
                var msg = $"MovcCmdInfo : Setup Moving Sections +++FAIL+++, [ReserveOkSection({aReserveOkSection.Id}) is not in MovingSections]";
                LoggerAgent.Instance.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", msg));
                StopVisitTransferSteps();
                return;
            }

            MapPosition pos = reserveOkSection.CmdDirection == EnumPermitDirection.Forward
                ? reserveOkSection.TailAddress.Position.DeepClone()
                : reserveOkSection.HeadAddress.Position.DeepClone();

            bool updateResult = moveControlHandler.AddReservedMapPosition(pos);
            OnMessageShowEvent?.Invoke(this, $"MainFlow :Update MoveControl ReserveOk Position, [UpdateResult={updateResult}][Pos={pos}]");
        }

        public bool IsMoveStep() => GetCurrentTransferStepType() == EnumTransferStepType.Move || GetCurrentTransferStepType() == EnumTransferStepType.MoveToCharger;

        public void MoveControlHandler_OnMoveFinished(object sender, EnumMoveComplete status)
        {
            try
            {
                middleAgent.StopAskReserve();

                //theVehicle.CurVehiclePosition.WheelAngle = (int)theVehicle.CurVehiclePosition.VehicleAngle;
                //if (VisitTransferStepsStatus == EnumThreadStatus.Stop || VisitTransferStepsStatus == EnumThreadStatus.None)
                //{
                //    //Visit Transfer Steps has stop or clear
                //    loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID",
                //        $"MainFlow : MoveControlHandler_OnMoveFinished, [FinishStatus={status}][VisitTransferStepsStatus={VisitTransferStepsStatus}]"));
                //    return;
                //}

                if (status == EnumMoveComplete.Fail)
                {
                    middleAgent.ClearGotReserveOkSections(); //Already stop ask reserve and clear need and asking section(s)
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [FAIL]");
                    alarmHandler.SetAlarm(000006);
                    StopVisitTransferSteps();
                    return;
                }

                if (status == EnumMoveComplete.Pause)
                {
                    VisitTransferStepsStatus = EnumThreadStatus.PauseComplete;
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Pause, [Pause]");
                    middleAgent.PauseComplete();
                    return;
                }

                #region EnumMoveComplete.Success

                middleAgent.ClearGotReserveOkSections();

                //UpdateVehiclePositionWithMoveCmd((MoveCmdInfo)GetCurTransferStep(), theVehicle.CurVehiclePosition.RealPosition);
                UpdateLastAddressAfterArrival((MoveCmdInfo)GetCurTransferStep());

                StartCharge();

                if (transferSteps.Count > 0)
                {
                    if (NextTransCmdIsLoad())
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [LoadArrival]");
                    }
                    else if (NextTransCmdIsUnload())
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [UnloadArrival]");
                    }
                    else
                    {
                        if (GetCurrentTransferStepType() == EnumTransferStepType.MoveToCharger)
                        {
                            middleAgent.MoveToChargerComplete();
                        }
                        else
                        {
                            middleAgent.MoveComplete();
                        }

                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [{GetCurrentTransferStepType()} Complete]");
                    }

                    VisitNextTransCmd();
                }
                else
                {
                    if (GetCurrentTransferStepType() == EnumTransferStepType.MoveToCharger)
                    {
                        middleAgent.MoveToChargerComplete();
                    }
                    else
                    {
                        middleAgent.MoveComplete();
                    }

                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [{GetCurrentTransferStepType()} Complete]");
                }

                #endregion
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void PlcAgent_OnForkCommandFinishEvent(object sender, PlcForkCommand forkCommand)
        {
            try
            {
                //if (VisitTransferStepsStatus == EnumThreadStatus.Stop || VisitTransferStepsStatus == EnumThreadStatus.None)
                //{
                //    //Visit Transfer Steps has stop or clear
                //    loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID",
                //        $"MainFlow : PlcAgent_OnForkCommandFinishEvent, [Type={forkCommand.ForkCommandType}][VisitTransferStepsStatus={VisitTransferStepsStatus}]"));
                //    return;
                //}

                if (forkCommand.ForkCommandType == EnumForkCommand.Load)
                {
                    if (!CanCassetteIdRead())
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : ForkCommandFinish,[Type={forkCommand.ForkCommandType}] [CanCassetteIdRead={CanCassetteIdRead()}]");
                        return;
                    }

                    if (NextTransCmdIsMove())
                    {
                        middleAgent.LoadCompleteInLoadunload();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : ForkCommandFinish,[Type={forkCommand.ForkCommandType}] [NextTransCmdIsMove={NextTransCmdIsMove()}]");
                    }
                    else
                    {
                        middleAgent.LoadComplete();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : ForkCommandFinish,[Type={forkCommand.ForkCommandType}] [LoadComplete]");
                    }
                }
                else if (forkCommand.ForkCommandType == EnumForkCommand.Unload)
                {
                    if (theVehicle.ThePlcVehicle.Loading)
                    {
                        alarmHandler.SetAlarm(000007);
                        return;
                    }

                    theVehicle.ThePlcVehicle.CassetteId = "";
                    if (IsLoadUnloadComplete())
                    {
                        middleAgent.LoadUnloadComplete();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : ForkCommandFinish,[Type={forkCommand.ForkCommandType}] [LoadUnloadComplete]");
                    }
                    else
                    {
                        middleAgent.UnloadComplete();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : ForkCommandFinish,[Type={forkCommand.ForkCommandType}] [UnloadComplete]");
                    }
                }
                else if (forkCommand.ForkCommandType == EnumForkCommand.Home)
                {
                    //TODO: RobotHomeComplete
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : ForkCommandFinish,[Type={forkCommand.ForkCommandType}] [RobotHomeComplete]");
                }
                //middleAgent.Send_Cmd144_StatusChangeReport();               

                VisitNextTransCmd();
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private bool NextTransCmdIsUnload() => GetNextTransferStepType() == EnumTransferStepType.Unload;

        private bool NextTransCmdIsLoad() => GetNextTransferStepType() == EnumTransferStepType.Load;

        private bool NextTransCmdIsMove() => GetNextTransferStepType() == EnumTransferStepType.Move || GetNextTransferStepType() == EnumTransferStepType.MoveToCharger;

        private bool IsLoadUnloadComplete() => agvcTransCmd.CommandType == EnumAgvcTransCommandType.LoadUnload;

        private void OnLoadunloadFinishedEvent()
        {
            middleAgent.LoadUnloadComplete();
        }

        private void VisitNextTransCmd()
        {
            TransferStepsIndex++;
            theVehicle.CurTrasferStep = GetCurTransferStep();
            GoNextTransferStep = true;
        }

        public TransferStep GetCurTransferStep()
        {
            TransferStep transCmd = new EmptyTransferStep(this);
            if (TransferStepsIndex < transferSteps.Count)
            {
                transCmd = transferSteps[TransferStepsIndex];
            }
            return transCmd;
        }

        public TransferStep GetNextTransCmd()
        {
            TransferStep transCmd = new EmptyTransferStep(this);
            int nextIndex = TransferStepsIndex + 1;
            if (nextIndex < transferSteps.Count)
            {
                transCmd = transferSteps[nextIndex];
            }
            return transCmd;
        }

        public void SetTransCmdsStep(ITransferStatus step)
        {
            this.transferStatus = step;
        }

        public void DoTransfer()
        {
            transferStatus.DoTransfer(this);
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
                    if (!plcAgent.IsForkCommandExist())
                    {
                        middleAgent.Unloading();
                        PlcForkUnloadCommand = new PlcForkCommand(ForkCommandNumber++, EnumForkCommand.Unload, unloadCmd.StageNum.ToString(), unloadCmd.StageDirection, unloadCmd.IsEqPio, unloadCmd.ForkSpeed);
                        plcAgent.AddForkComand(PlcForkUnloadCommand);
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Unload, [Type={PlcForkLoadCommand.ForkCommandType}][StageNum={PlcForkLoadCommand.StageNo}][IsEqPio={PlcForkLoadCommand.IsEqPio}]");
                    }
                    else
                    {
                        alarmHandler.SetAlarm(000008);
                    }
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                    if (!plcAgent.IsForkCommandExist())
                    {
                        middleAgent.Loading();
                        PlcForkLoadCommand = new PlcForkCommand(ForkCommandNumber++, EnumForkCommand.Load, loadCmd.StageNum.ToString(), loadCmd.StageDirection, loadCmd.IsEqPio, loadCmd.ForkSpeed);
                        plcAgent.AddForkComand(PlcForkLoadCommand);
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Load, [Type={PlcForkLoadCommand.ForkCommandType}][StageNum={PlcForkLoadCommand.StageNo}][IsEqPio={PlcForkLoadCommand.IsEqPio}]");
                    }
                    else
                    {
                        alarmHandler.SetAlarm(000010);
                    }
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
            }
        }

        public void ReconnectToAgvc()
        {
            middleAgent.ReConnect();
        }

        #region Simple Getters
        public AlarmHandler GetAlarmHandler() => alarmHandler;
        public MiddleAgent GetMiddleAgent() => middleAgent;
        public MiddlerConfig GetMiddlerConfig() => middlerConfig;
        public MapConfig GetMapConfig() => mapConfig;
        public MapHandler GetMapHandler() => mapHandler;
        public MoveControlHandler GetMoveControlHandler() => moveControlHandler;
        public PlcAgent GetPlcAgent() => plcAgent;
        public MCProtocol GetMcProtocol() => mcProtocol;
        #endregion

        public bool CallMoveControlWork(MoveCmdInfo moveCmd)
        {
            //string msg = "MainFlow : Call Move Control Work";
            //msg += "[AddressPositions=";
            //for (int i = 0; i < moveCmd.AddressPositions.Count; i++)
            //{
            //    msg += $"({(int)(moveCmd.AddressPositions[i].X)},{(int)(moveCmd.AddressPositions[i].Y)})";
            //}
            //msg += "]\n[AddressActions=";
            //for (int i = 0; i < moveCmd.AddressActions.Count; i++)
            //{
            //    msg += $"({moveCmd.AddressActions[i]})";
            //}
            //msg += "]\n[SectionSpeedLimits=";
            //for (int i = 0; i < moveCmd.SectionSpeedLimits.Count; i++)
            //{
            //    msg += $"({moveCmd.SectionSpeedLimits[i]})";
            //}
            //msg += "]";
            //OnMessageShowEvent?.Invoke(this, msg);

            if (moveControlHandler.TransferMove(moveCmd.DeepClone()))
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : Call Move Control Work  OK.");
                return true;
            }
            else
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : Call Move Control Work +++FAIL+++");
                MoveControlHandler_OnMoveFinished(this, EnumMoveComplete.Fail);
                return false;
            }
        }

        public void PrepareForAskingReserve(MoveCmdInfo moveCmd)
        {
            middleAgent.StopAskReserve();
            middleAgent.SetupNeedReserveSections(moveCmd);
            middleAgent.StartAskReserve();
        }

        private void UpdateVehiclePositionWithMoveCmd(MoveCmdInfo curTransCmd, MapPosition gxPosition)
        {
            List<MapSection> movingSections = curTransCmd.MovingSections;
            int searchingSectionIndex = curTransCmd.MovingSectionsIndex;

            while (searchingSectionIndex < movingSections.Count)
            {
                try
                {
                    if (mapHandler.IsPositionInThisSection(gxPosition, movingSections[searchingSectionIndex]))
                    {
                        curTransCmd.MovingSectionsIndex = searchingSectionIndex;

                        UpdateMiddlerGotReserveOkSections(movingSections[searchingSectionIndex].Id);

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
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, 1);
                }
            }

            if (searchingSectionIndex == movingSections.Count)
            {
                alarmHandler.SetAlarm(000011);
            }
        }

        private void UpdateLastAddressAfterArrival(MoveCmdInfo moveCmd)
        {
            try
            {
                VehiclePosition vehiclePosition = new VehiclePosition();
                vehiclePosition.BarcodePosition = theVehicle.CurVehiclePosition.BarcodePosition;
                vehiclePosition.RealPosition = TheMapInfo.allMapAddresses[moveCmd.EndAddressId].Position;
                vehiclePosition.LastAddress = TheMapInfo.allMapAddresses[moveCmd.EndAddressId].DeepClone();
                vehiclePosition.LastSection = moveCmd.MovingSections[moveCmd.MovingSections.Count - 1].DeepClone();
                vehiclePosition.LastSection.Distance = Math.Sqrt(mapHandler.GetDistance(vehiclePosition.RealPosition, vehiclePosition.LastSection.HeadAddress.Position));
                vehiclePosition.RealPositionRangeMm = theVehicle.CurVehiclePosition.RealPositionRangeMm;
                vehiclePosition.VehicleAngle = theVehicle.CurVehiclePosition.VehicleAngle;
                vehiclePosition.WheelAngle = theVehicle.CurVehiclePosition.WheelAngle;
                theVehicle.CurVehiclePosition = vehiclePosition;

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                      , $"MainFolw : UpdateLastAddressAfterArrival. [LastAddress = {theVehicle.CurVehiclePosition.LastAddress}][EndAddressId={moveCmd.EndAddressId}]"));
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"MainFolw : UpdateLastAddressAfterArrival +++FAIL+++"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        private void UpdatePlcVehicleBeamSensor()
        {
            var plcVeh = theVehicle.ThePlcVehicle;
            var lastSection = theVehicle.CurVehiclePosition.LastSection.DeepClone();
            var curDistance = lastSection.Distance;
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"MainFlow : UpdateMiddlerGotReserveOkSections FAIL [SecId={id}][Index={getReserveOkSectionIndex}]"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }

        }

        private bool UpdateVehiclePositionWithoutMoveCmd(MapPosition gxPosition)
        {
            if (gxPosition == null) return false;

            bool isInMap = false;
            foreach (var item in TheMapInfo.allMapSections)
            {
                MapSection mapSection = item.Value;
                if (mapHandler.IsPositionInThisSection(gxPosition, mapSection))
                {
                    isInMap = true;
                    if (mapSection.Type == EnumSectionType.Horizontal)
                    {
                        theVehicle.CurVehiclePosition.RealPosition.Y = mapSection.HeadAddress.Position.Y;
                    }
                    else if (mapSection.Type == EnumSectionType.Vertical)
                    {
                        theVehicle.CurVehiclePosition.RealPosition.X = mapSection.HeadAddress.Position.X;
                    }

                    UpdatePlcVehicleBeamSensor();

                    break;
                }
            }

            if (!isInMap)
            {
                var msg = $"MainFlow : Update VehiclePosition Without MoveCmd +++Get Lost+++, [Position=({(int)gxPosition.X},{(int)gxPosition.Y})]";
                //OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
            }

            return isInMap;
        }

        private void StartCharge()
        {
            if (theVehicle.ThePlcVehicle.Batterys.Charging)
            {
                var msg = $"MainFlow : Start charge+++FAIL+++, IsCharging [Charging={true}]";
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                      , msg));
                OnMessageShowEvent?.Invoke(this, msg);
                return;
            }

            if (IsHighPower())
            {
                var msg = $"MainFlow : Start charge +++FAIL+++, IsHighPower [Percent={(int)theVehicle.ThePlcVehicle.Batterys.Percentage}][HighSoc={(int)theVehicle.ThePlcVehicle.Batterys.PortAutoChargeHighSoc}]";
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
                OnMessageShowEvent?.Invoke(this, msg);
                return;
            }

            if (!mapHandler.IsPositionInThisAddress(theVehicle.CurVehiclePosition.RealPosition, theVehicle.CurVehiclePosition.LastAddress.Position))
            {
                var msg = $"MainFlow : Start charge fail, RealPos is not in LastAddress [Real=({(int)theVehicle.CurVehiclePosition.RealPosition.X},{(int)theVehicle.CurVehiclePosition.RealPosition.Y})][LastAddress={theVehicle.CurVehiclePosition.LastAddress.Id}]";
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                 , msg));
                OnMessageShowEvent?.Invoke(this, msg);
                return;
            }

            {
                var msg = $"MainFlow : Start charge, [IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]";
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
                OnMessageShowEvent?.Invoke(this, msg);
            }


            var address = theVehicle.CurVehiclePosition.LastAddress;

            if (address.IsCharger)
            {
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
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Start Charging, [Id={address.Id}][IsInCouple={address.IsCharger}][IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]");
                }
                else
                {
                    alarmHandler.SetAlarm(000013);
                }
            }
        }

        public bool StopCharge()
        {
            var msg = $"MainFlow : Stop Charge, [IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]";
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                 , msg));


            if (!theVehicle.ThePlcVehicle.Batterys.Charging)
            {
                return true;
            }

            if (!mapHandler.IsPositionInThisAddress(theVehicle.CurVehiclePosition.RealPosition, theVehicle.CurVehiclePosition.LastAddress.Position))
            {
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , $"MainFlow : Stop charge fail, RealPos is not in LastAddress [Real=({(int)theVehicle.CurVehiclePosition.RealPosition.X},{(int)theVehicle.CurVehiclePosition.RealPosition.Y})][LastAddress={theVehicle.CurVehiclePosition.LastAddress.Id}]"));
                return true;
            }
            var address = theVehicle.CurVehiclePosition.LastAddress;
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
            PauseMainFlowThreads();
            middleAgent.PauseAskReserve();
            StopVehicle();
            middleAgent.StopAskReserve();
            StopMainFlowThreads();
            middleAgent.ClearAskReserve();

            if (theVehicle.ThePlcVehicle.Loading)
            {
                string cstId = "";
                plcAgent.triggerCassetteIDReader(ref cstId);
                if (cstId == "ERROR")
                {
                    cstId = "";
                }
                theVehicle.ThePlcVehicle.CassetteId = cstId;
            }
            StartTrackPosition();
            StartWatchLowPower();

            var msg = $"MainFlow : Stop And Clear, [VisitTransferStepsStatus={VisitTransferStepsStatus}][WatchLowPowerStatus={WatchLowPowerStatus}][TrackPositionStatus={TrackPositionStatus}][AskReserveStatus={theVehicle.AskReserveStatus}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                 , msg));
        }

        private void StopMainFlowThreads()
        {
            StopTrackPosition();
            StopVisitTransferSteps();
            StopWatchLowPower();
        }

        private void PauseMainFlowThreads()
        {
            PauseVisitTransferSteps();
            PauseWatchLowPower();
            PauseTrackPosition();
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                 , msg));
        }

        public bool SetManualToAuto()
        {
            StopAndClear();
            if (!IsMoveControlHandlerReadyToAuto())
            {
                var msg = $"MainFlow : Set Manual To Auto +++FAIL+++, [MoveState={moveControlHandler.MoveState}][IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}][LocationNull={!moveControlHandler.IsLocationRealNotNull()}]";
                OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
                return false;
            }

            if (!UpdateVehiclePositionWithoutMoveCmd(theVehicle.CurVehiclePosition.RealPosition))
            {
                var realpos = theVehicle.CurVehiclePosition.RealPosition;
                var msg = $"MainFlow : Set Manual To Auto +++FAIL+++, [Position=({(int)realpos.X},{(int)realpos.Y})]";
                OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
                return false;
            }
            else
            {
                var realpos = theVehicle.CurVehiclePosition.RealPosition;
                var msg = $"MainFlow : Set Manual To Auto +++Ok+++, [Position=({(int)realpos.X},{(int)realpos.Y})][MoveState={moveControlHandler.MoveState}][IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]";
                OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
                return true;
            }
        }

        private bool IsMoveControlHandlerReadyToAuto()
        {
            return moveControlHandler.MoveState == EnumMoveState.Idle && moveControlHandler.IsLocationRealNotNull();
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
            plcAgent.WritePLCAlarmReset();
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
        public void SetupTestMoveCmd(List<MapSection> mapSections)
        {
            transferSteps = new List<TransferStep>();
            MoveCmdInfo moveCmd = new MoveCmdInfo();
            moveCmd.MovingSections = mapSections.DeepClone();
            transferSteps.Add(moveCmd);
            transferSteps.Add(new EmptyTransferStep());
            TransferStepsIndex = 0;
        }

        private void AlarmHandler_OnResetAllAlarmsEvent(object sender, List<Alarm> alarms)
        {
            plcAgent.SetAlarmWarningReportAllReset();
        }

        private void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            plcAgent.WriteAlarmWarningReport(alarm.Level, alarm.PlcWord, alarm.PlcBit, true);
            plcAgent.WriteAlarmWarningStatus(alarmHandler.HasAlarm, alarmHandler.HasWarn);
        }

        private void PlcAgent_OnCassetteIDReadFinishEvent(object sender, string cstId)
        {
            if (cstId == "ERROR")
            {
                //Id Read Fail
                theVehicle.ThePlcVehicle.CassetteId = "";
                middleAgent.CstIdRead(EnumCstIdReadResult.Fail);
            }
            else if (!IsAgvcTransferCommandEmpty())
            {
                if (agvcTransCmd.CassetteId != cstId)
                {
                    //Id Read Mismatch
                    theVehicle.ThePlcVehicle.CassetteId = cstId;
                    middleAgent.CstIdRead(EnumCstIdReadResult.Mismatch);
                }
                else
                {
                    //Id Read Normal
                    theVehicle.ThePlcVehicle.CassetteId = cstId;
                    middleAgent.CstIdRead(EnumCstIdReadResult.Noraml);
                }
            }
            else
            {
                //Id Read Normal
                theVehicle.ThePlcVehicle.CassetteId = cstId;
                middleAgent.CstIdRead(EnumCstIdReadResult.Noraml);
            }
        }

        public void SetupFakeVehicleSoc(double percentage)
        {
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            batterys.SetCcModeAh(batterys.MeterAh + batterys.AhWorkingRange * (100.0 - percentage) / 100.00, false);
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

        public void Middler_OnCmdPauseEvent(ushort iSeqNum, PauseType pauseType)
        {
            if (false/*moveControlHandler.CanVehPause()*/)
            {
                if (IsMoveStep())
                {
                    middleAgent.PauseReply(iSeqNum, 0, PauseEvent.Pause);
                    PauseVisitTransferSteps();
                    middleAgent.PauseAskReserve();
                    moveControlHandler.VehclePause();
                }
                else
                {
                    middleAgent.PauseReply(iSeqNum, 1, PauseEvent.Pause);
                }
            }
            else
            {
                middleAgent.PauseReply(iSeqNum, 1, PauseEvent.Pause);
            }
        }

        public void Middler_OnCmdResumeEvent(ushort iSeqNum, PauseType pauseType, RepeatedField<ReserveInfo> reserveInfos)
        {
            if (pauseType == PauseType.Reserve)
            {
                List<MapSection> reserveOkSections = new List<MapSection>();
                foreach (var reserveInfo in reserveInfos)
                {
                    if (TheMapInfo.allMapSections.ContainsKey(reserveInfo.ReserveSectionID))
                    {
                        UpdateMiddlerNeedReserveSections(reserveInfo.ReserveSectionID);
                        var reserveOkSection = TheMapInfo.allMapSections[reserveInfo.ReserveSectionID];
                        reserveOkSection.CmdDirection = reserveInfo.DriveDirction == DriveDirction.DriveDirForward ? EnumPermitDirection.Forward : EnumPermitDirection.Backward;
                        reserveOkSections.Add(reserveOkSection);
                        UpdateMoveControlReserveOkPositions(reserveOkSection);
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : ResumeReserveOk, [ReserveOkSectionId = {reserveOkSection.Id}]");
                    }
                    else
                    {
                        middleAgent.PauseReply(iSeqNum, 1, PauseEvent.Continue);
                    }
                }
                middleAgent.ClearAskingReserveSection();
                middleAgent.SetupReserveOkSections(reserveOkSections);
            }

            middleAgent.PauseReply(iSeqNum, 0, PauseEvent.Continue);
            ResumeVisitTransferSteps();
            middleAgent.ResumeAskReserve();
            moveControlHandler.VehcleContinue();
            middleAgent.ResumeComplete();
        }

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
            {
                middleAgent.CancelAbortReply(iSeqNum, 1, cmdId, actType);
                var msg = $"MainFlow : OnCmdCancelAbortEvent +++FALSE+++, Can not cancel or abort";
                OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , msg));
                return;
            }

            if (transferSteps.Count == 0)
            {
                middleAgent.CancelAbortReply(iSeqNum, 1, cmdId, actType);
                var msg = $"MainFlow : OnCmdCancelAbortEvent +++FALSE+++, [transferSteps={transferSteps.Count}][CmdId={cmdId}][Type={actType}]";
                OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , msg));
                return;
            }

            if (agvcTransCmd.CommandId != cmdId)
            {
                middleAgent.CancelAbortReply(iSeqNum, 1, cmdId, actType);
                var msg = $"MainFlow : OnCmdCancelAbortEvent  +++FALSE+++, [TransferCmdId={agvcTransCmd.CommandId}][CancelAbortCmdId={cmdId}][Type={actType}]";
                OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , msg));
                return;
            }

            middleAgent.CancelAbortReply(iSeqNum, 0, cmdId, actType);
            StopAndClear();
            switch (actType)
            {
                case CMDCancelType.CmdNone:
                    break;
                case CMDCancelType.CmdCancel:
                    middleAgent.CancelComplete();
                    break;
                case CMDCancelType.CmdAbort:
                    middleAgent.AbortComplete();
                    break;
                case CMDCancelType.CmdCancelIdMismatch:
                    break;
                case CMDCancelType.CmdCancelIdReadFailed:
                    break;
                default:
                    break;
            }

            if (theVehicle.AutoState == EnumAutoState.Auto && IsWatchLowPowerStop())
            {
                StartWatchLowPower();
            }
        }

    }
}
