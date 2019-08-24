using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferCmds;
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
        private AgvcTransCmd agvcTransCmd;
        private AgvcTransCmd lastAgvcTransCmd;
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
        public EnumThreadStatus PreVisitTransferStepsStatus { get; private set; } = EnumThreadStatus.None;

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
            LoadAllAlarms();
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
                mapHandler = new MapHandler(mapConfig);
                TheMapInfo = mapHandler.TheMapInfo;

                moveControlHandler = new MoveControlHandler(TheMapInfo, alarmHandler);
                alarmHandler = new AlarmHandler(alarmConfig);

                middleAgent = new MiddleAgent(this);
                mcProtocol = new MCProtocol();
                mcProtocol.Name = "MCProtocol";
                plcAgent = new PlcAgent(mcProtocol, alarmHandler);

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "控制層"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "控制層"));
            }
        }

        private void VehicleInitial()
        {
            try
            {
                theVehicle = Vehicle.Instance;
                theVehicle.CurAgvcTransCmd = agvcTransCmd;
                theVehicle.LastCurAgvcTransCmd = lastAgvcTransCmd;
                theVehicle.CurVehiclePosition.RealPositionRangeMm = mainFlowConfig.RealPositionRangeMm;
                theVehicle.TheMapInfo = TheMapInfo;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "台車"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "台車"));
            }
        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'mapHandler
                middleAgent.OnInstallTransferCommandEvent += MiddleAgent_OnInstallTransferCommandEvent;

                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'mapHandler
                middleAgent.OnTransferCancelEvent += OnMiddlerGetsCancelEvent;
                middleAgent.OnTransferAbortEvent += OnMiddlerGetsAbortEvent;

                //來自MiddleAgent的取得Reserve/BlockZone訊息，通知MainFlow(this)
                middleAgent.OnGetBlockPassEvent += MiddleAgent_OnGetBlockPassEvent;

                //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                moveControlHandler.OnMoveFinished += MoveControlHandler_OnMoveFinished;

                //來自PlcAgent的取放貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                plcAgent.OnForkCommandFinishEvent += PlcAgent_OnForkCommandFinishEvent;

                //來自PlcBattery的電量改變訊息，通知middleAgent
                plcAgent.OnBatteryPercentageChangeEvent += middleAgent.PlcAgent_OnBatteryPercentageChangeEvent;

                //來自PlcBattery的CassetteId讀取訊息，通知middleAgent
                plcAgent.OnCassetteIDReadFinishEvent += middleAgent.PlcAgent_OnCassetteIDReadFinishEvent;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "事件"));
            }
            catch (Exception)
            {
                isIniOk = false;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "事件"));
            }
        }

        private void LoadAllAlarms()
        {
            //TODO: load all alarms
            //throw new NotImplementedException();
        }

        private void VehicleLocationInitial()
        {
            if (IsRealPositionEmpty())
            {
                theVehicle.CurVehiclePosition.RealPosition = TheMapInfo.allMapAddresses.First(x => x.Key != "Empty").Value.Position.DeepClone();
            }
            StartTrackPosition();
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
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , ex.StackTrace));
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
            PreVisitTransferStepsStatus = VisitTransferStepsStatus;
            VisitTransferStepsStatus = EnumThreadStatus.Pause;
            var msg = $"MainFlow : Pause Visit TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        public void ResumeVisitTransferSteps()
        {
            visitTransferStepsPauseEvent.Set();
            var tempStatus = VisitTransferStepsStatus;
            VisitTransferStepsStatus = PreVisitTransferStepsStatus;
            PreVisitTransferStepsStatus = tempStatus;
            var msg = $"MainFlow : Resume Visit TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        public void StopVisitTransferSteps()
        {
            VisitTransferStepsStatus = EnumThreadStatus.Stop;
            var msg = $"MainFlow : Stop Visit TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
              , msg));

            visitTransferStepsShutdownEvent.Set();
            visitTransferStepsPauseEvent.Set();
            StopVehicle();
        }
        private void PreVisitTransferSteps()
        {
            var msg = $"MainFlow : Pre Visit TransferSteps, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));

            TransferStepsIndex = 0;
            theVehicle.CurTrasferStep = GetCurTransferStep();
            GoNextTransferStep = true;
            theVehicle.ActionStatus = VHActionStatus.Commanding;
            middleAgent.Send_Cmd144_StatusChangeReport();
        }
        private void AfterVisitTransferSteps(long total)
        {
            var msg = $"MainFlow : After Visit TransferSteps, [ThreadStatus={VisitTransferStepsStatus}][TotalSpendMs={total}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));

            lastAgvcTransCmd = agvcTransCmd.DeepClone();
            agvcTransCmd = null;
            transferSteps = new List<TransferStep>();
            TransferStepsIndex = 0;
            theVehicle.CurTrasferStep = GetCurTransferStep();
            GoNextTransferStep = false;
            SetTransCmdsStep(new Idle());
            theVehicle.ActionStatus = VHActionStatus.NoCommand;
            VisitTransferStepsStatus = EnumThreadStatus.None;
            middleAgent.Send_Cmd144_StatusChangeReport();
            StartWatchLowPower();
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

                    if (IsLowPower())
                    {
                        StartCharge();
                    }

                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , ex.StackTrace));
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
            WatchLowPowerStatus = EnumThreadStatus.Stop;
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
                    if (transferSteps.Count > 0)
                    {
                        //有搬送命令時，比對當前Position與搬送路徑Sections計算LastSection/LastAddress/Distance
                        var curTransStep = GetCurTransferStep();
                        if (curTransStep.GetTransferStepType() == EnumTransferStepType.Move)
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
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , ex.StackTrace));
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
            TrackPositionStatus = EnumThreadStatus.Stop;

            //if (thdTrackPosition.IsAlive)
            //{
            //    thdTrackPosition.Join();
            //}          

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
            var msg = $"MainFlow : Get Middler TransferCommand, [CmdId={agvcTransCmd.CommandId}][CmdType={agvcTransCmd.EnumCommandType}]";
            OnMessageShowEvent?.Invoke(this, msg);
            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));

            if (!IsAgvcTransferCommandEmpty())
            {
                middleAgent.Send_Cmd131_TransferResponse(agvcTransCmd.SeqNum, 1, "Agv already have transfer command.");
                return;
            }

            try
            {
                this.agvcTransCmd = agvcTransCmd;
                theVehicle.CurAgvcTransCmd = agvcTransCmd;
                StopWatchLowPower();
                SpinWait.SpinUntil(() => false, mainFlowConfig.StopChargeWaitingTimeMs);
                if (WatchLowPowerStatus != EnumThreadStatus.None)
                {
                    //Alarm
                    middleAgent.Send_Cmd131_TransferResponse(agvcTransCmd.SeqNum, 1, "Low Power Chargin.");
                    return;
                }

                middleAgent.Send_Cmd131_TransferResponse(agvcTransCmd.SeqNum, 0, " ");
                SetupTransferSteps();
                transferSteps.Add(new EmptyTransferStep());

                //開始尋訪 trasnferSteps as List<TrasnferStep> 裡的每一步MoveCmdInfo/LoadCmdInfo/UnloadCmdInfo
                StartVisitTransferSteps();

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , ex.StackTrace));
            }
        }
        private void SetupTransferSteps()
        {
            transferSteps = new List<TransferStep>();

            switch (agvcTransCmd.EnumCommandType)
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
                case EnumAgvcTransCommandType.Home:
                    ConvertAgvcHomeCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.Override:
                    ConvertAgvcOverrideCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.Else:
                default:
                    ConvertAgvcElseCmdIntoList(agvcTransCmd);
                    break;
            }
        }
        #region Convert AgvcTransferCommand to TransferSteps
        private void ConvertAgvcElseCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            throw new NotImplementedException();
        }
        private void ConvertAgvcOverrideCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            //TODO: clone old transCmds
            //TODO: separate transCmds into MLMU
            //TODO: override move part.

            //var tempTransCmds = new List<TransCmd>();
            //for (int i = 0; i < transCmds.Count; i++)
            //{

            //}

            //var curSection = theVehicle.AVehLocation.Section.Id;
            //if (agvcTransCmd.ToLoadSections.Length > 0) //curSection at to load sections
            //{
            //    for (int i = 0; i < agvcTransCmd.ToLoadSections.Length; i++)
            //    {

            //    }
            //}
            //else
            //{

            //}
        }
        private void ConvertAgvcHomeCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            //throw new NotImplementedException();
        }
        private void ConvertAgvcLoadUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            ConvertAgvcLoadCmdIntoList(agvcTransCmd);
            ConvertAgvcNextUnloadCmdIntoList(agvcTransCmd);
        }
        private void ConvertAgvcUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToUnloadSections.Count > 0)
            {
                MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
                transferSteps.Add(moveCmd);
            }

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }
        private void ConvertAgvcNextUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToUnloadSections.Count > 0)
            {
                MoveCmdInfo moveCmd = GetMoveToNextUnloadCmdInfo(agvcTransCmd);
                transferSteps.Add(moveCmd);
            }

            UnloadCmdInfo unloadCmd = GetUnloadCmdInfo(agvcTransCmd);
            transferSteps.Add(unloadCmd);
        }
        private void ConvertAgvcLoadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToLoadSections.Count > 0)
            {
                MoveCmdInfo moveCmd = GetMoveToLoadCmdInfo(agvcTransCmd);
                transferSteps.Add(moveCmd);
            }

            LoadCmdInfo loadCmd = GetLoadCmdInfo(agvcTransCmd);
            transferSteps.Add(loadCmd);
        }
        private MoveCmdInfo GetMoveToUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(TheMapInfo);
            moveCmd.CmdId = agvcTransCmd.CommandId;
            moveCmd.CstId = agvcTransCmd.CassetteId;
            moveCmd.AddressIds = agvcTransCmd.ToUnloadAddresses;
            moveCmd.SectionIds = agvcTransCmd.ToUnloadSections;
            moveCmd.SetMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            moveCmd.SetAddressPositions();
            moveCmd.SetAddressActions();
            moveCmd.SetSectionSpeedLimits();
            return moveCmd;
        }
        private MoveCmdInfo GetMoveToNextUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(TheMapInfo);
            moveCmd.CmdId = agvcTransCmd.CommandId;
            moveCmd.CstId = agvcTransCmd.CassetteId;
            moveCmd.AddressIds = agvcTransCmd.ToUnloadAddresses;
            moveCmd.SectionIds = agvcTransCmd.ToUnloadSections;
            moveCmd.SetMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            moveCmd.SetNextUnloadAddressPositions();
            moveCmd.SetAddressActions();
            moveCmd.SetSectionSpeedLimits();
            return moveCmd;
        }
        private LoadCmdInfo GetLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            LoadCmdInfo loadCmd = new LoadCmdInfo();
            loadCmd.CstId = agvcTransCmd.CassetteId;
            loadCmd.CmdId = agvcTransCmd.CommandId;
            loadCmd.LoadAddress = agvcTransCmd.LoadAddress;
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
            unloadCmd.UnloadAddress = agvcTransCmd.UnloadAddress;
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
        private MoveCmdInfo GetMoveToLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(TheMapInfo);
            moveCmd.CmdId = agvcTransCmd.CommandId;
            moveCmd.CstId = agvcTransCmd.CassetteId;
            moveCmd.AddressIds = agvcTransCmd.ToLoadAddresses;
            moveCmd.SectionIds = agvcTransCmd.ToLoadSections;
            moveCmd.SetMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            moveCmd.SetAddressPositions();
            moveCmd.SetAddressActions();
            moveCmd.SetSectionSpeedLimits();

            return moveCmd;
        }
        public AgvcTransCmd GetAgvcTransCmd()
        {
            return agvcTransCmd;
        }
        private void ConvertAgvcMoveCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToUnloadSections.Count > 0)
            {
                MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
                transferSteps.Add(moveCmd);
            }
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
        private void OnMiddlerGetsAbortEvent(object sender, string aCmdId)
        {
            //check cmd-id is match or not
            StopAndClear();
        }
        private void OnMiddlerGetsCancelEvent(object sender, string aCmdId)
        {
            //check cmd-id is match or not
            StopAndClear();
        }
        private bool CanVehUnload()
        {
            // 判斷當前是否可載貨 若否 則發送報告
            MapPosition position = theVehicle.CurVehiclePosition.RealPosition;
            MapAddress unloadAddress = TheMapInfo.allMapAddresses[agvcTransCmd.UnloadAddress];
            var result = mapHandler.IsPositionInThisAddress(position, unloadAddress.Position);
            var msg = $"MainFlow : CanVehUnload, [result={result}][position=({(int)position.X},{(int)position.Y})][loadAddress=({(int)unloadAddress.Position.X},{(int)unloadAddress.Position.Y})]";
            OnMessageShowEvent?.Invoke(this, msg);

            if (result)
            {
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , msg));

            }
            else
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
            }

            return result;
        }
        private bool CanVehLoad()
        {
            // 判斷當前是否可卸貨 若否 則發送報告
            MapPosition position = theVehicle.CurVehiclePosition.RealPosition;
            MapAddress loadAddress = TheMapInfo.allMapAddresses[agvcTransCmd.LoadAddress];
            var result = mapHandler.IsPositionInThisAddress(position, loadAddress.Position);
            var msg = $"MainFlow : CanVehLoad, [result={result}][position=({(int)position.X},{(int)position.Y})][loadAddress=({(int)loadAddress.Position.X},{(int)loadAddress.Position.Y})]";
            OnMessageShowEvent?.Invoke(this, msg);

            if (result)
            {
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , msg));

            }
            else
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
            }

            return result;
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
                //Alarm plcVehicle is not Loading
                return false;
            }
            else if (string.IsNullOrEmpty(plcVeh.CassetteId))
            {
                //CassetteId is null or Empty
                return false;
            }
            else if (plcVeh.CassetteId == "ERROR")
            {
                //CassetteId is Error
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool IsAgvcTransferCommandEmpty()
        {
            return agvcTransCmd == null;
        }
        #endregion

        public void UpdateMoveControlReserveOkPositions(MapSection aReserveOkSection)
        {
            MapPosition pos = aReserveOkSection.CmdDirection == EnumPermitDirection.Forward
                ? aReserveOkSection.TailAddress.Position.DeepClone()
                : aReserveOkSection.HeadAddress.Position.DeepClone();

            bool updateResult = moveControlHandler.AddReservedMapPosition(pos);
            OnMessageShowEvent?.Invoke(this, $"MainFlow :Update MoveControl ReserveOk Position, [UpdateResult={updateResult}][Pos={pos}]");
        }

        public bool IsMoveStep()
        {
            return GetCurrentTransferStepType() == EnumTransferStepType.Move;
        }

        public void MoveControlHandler_OnMoveFinished(object sender, EnumMoveComplete status)
        {
            try
            {
                middleAgent.StopAskReserve();
                middleAgent.ClearGotReserveOkSections();
                theVehicle.CurVehiclePosition.PredictVehicleAngle = (int)theVehicle.CurVehiclePosition.VehicleAngle;

                if (status == EnumMoveComplete.Fail)
                {
                    //TODO: Alarm
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [Status={status}]");
                    StopVisitTransferSteps();
                    return;
                }

                StartCharge();

                if (transferSteps.Count > 0)
                {

                    if (NextTransCmdIsLoad())
                    {
                        middleAgent.ReportLoadArrivals();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [LoadArrival]");
                    }
                    else if (NextTransCmdIsUnload())
                    {
                        middleAgent.UnloadArrivals();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [UnloadArrival]");
                    }
                    else
                    {
                        middleAgent.MoveComplete();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [MoveComplete]");
                    }

                    VisitNextTransCmd();
                }
                else
                {
                    middleAgent.MoveComplete();
                }


            }
            catch (Exception ex)
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Finish, [ex={ex.StackTrace}]");
            }

        }

        public void PlcAgent_OnForkCommandFinishEvent(object sender, PlcForkCommand forkCommand)
        {
            try
            {
                if (transferSteps.Count == 0)
                {
                    return;
                }

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
                        //Alarm : loading is still on
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : ForkCommandFinish,[Type={forkCommand.ForkCommandType}] [Loading={theVehicle.ThePlcVehicle.Loading}]");
                        return;
                    }

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

                VisitNextTransCmd();
            }
            catch (Exception ex)
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : ForkCommandFinish,[ex={ex.StackTrace}]");
            }
        }

        private bool NextTransCmdIsUnload()
        {
            return transferSteps[TransferStepsIndex + 1].GetTransferStepType() == EnumTransferStepType.Unload;
        }

        private bool NextTransCmdIsLoad()
        {
            return transferSteps[TransferStepsIndex + 1].GetTransferStepType() == EnumTransferStepType.Load;
        }

        private bool NextTransCmdIsMove()
        {
            return transferSteps[TransferStepsIndex + 1].GetTransferStepType() == EnumTransferStepType.Move;
        }

        private bool IsLoadUnloadComplete()
        {
            return agvcTransCmd.EnumCommandType == EnumAgvcTransCommandType.LoadUnload;
        }

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
            TransferStep transCmd = new EmptyTransferStep(TheMapInfo);
            if (TransferStepsIndex < transferSteps.Count)
            {
                transCmd = transferSteps[TransferStepsIndex];
            }
            return transCmd;
        }

        public TransferStep GetNextTransCmd()
        {
            TransferStep transCmd = new EmptyTransferStep(TheMapInfo);
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
            //Check if it is in position to unload here
            if (CanVehUnload())
            {
                try
                {
                    if (plcAgent.IsForkCommandExist())
                    {
                        //Alarm : 
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Unload, [plcAgent.IsForkCommandExist={plcAgent.IsForkCommandExist()}]");
                    }
                    else
                    {
                        middleAgent.Send_Cmd136_TransferEventReport(EventType.Vhunloading);
                        PlcForkUnloadCommand = new PlcForkCommand(ForkCommandNumber++, EnumForkCommand.Unload, unloadCmd.StageNum.ToString(), unloadCmd.StageDirection, unloadCmd.IsEqPio, unloadCmd.ForkSpeed);
                        plcAgent.AddForkComand(PlcForkUnloadCommand);
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Unload, [Type={PlcForkLoadCommand.ForkCommandType}][StageNum={PlcForkLoadCommand.StageNo}][IsEqPio={PlcForkLoadCommand.IsEqPio}]");
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.ToString();
                }

            }
            else
            {
                //Alarm:
                OnMessageShowEvent?.Invoke(this, $"MainFlow : Unload, [CanVehUnload={CanVehUnload()}]");

            }
        }

        public void Load(LoadCmdInfo loadCmd)
        {
            //Check if it is in position to load here
            if (CanVehLoad())
            {
                try
                {
                    if (!plcAgent.IsForkCommandExist())
                    {
                        middleAgent.Send_Cmd136_TransferEventReport(EventType.Vhloading);
                        PlcForkLoadCommand = new PlcForkCommand(ForkCommandNumber++, EnumForkCommand.Load, loadCmd.StageNum.ToString(), loadCmd.StageDirection, loadCmd.IsEqPio, loadCmd.ForkSpeed);
                        plcAgent.AddForkComand(PlcForkLoadCommand);
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Load, [Type={PlcForkLoadCommand.ForkCommandType}][StageNum={PlcForkLoadCommand.StageNo}][IsEqPio={PlcForkLoadCommand.IsEqPio}]");
                    }
                    else
                    {
                        //Alarm : 
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.ToString();
                }
            }
        }

        public void ReconnectToAgvc()
        {
            middleAgent.ReConnect();
        }

        public AlarmHandler GetAlarmHandler()
        {
            return this.alarmHandler;
        }

        public MiddleAgent GetMiddleAgent()
        {
            return middleAgent;
        }
        public MiddlerConfig GetMiddlerConfig()
        {
            return middlerConfig;
        }

        public MapHandler GetMapHandler()
        {
            return mapHandler;
        }

        public MoveControlHandler GetMoveControlHandler()
        {
            return moveControlHandler;
        }

        public void CallMoveControlWork(MoveCmdInfo moveCmd)
        {
            string msg = "MainFlow : Call Move Control Work";
            msg += "[AddressPositions=";
            for (int i = 0; i < moveCmd.AddressPositions.Count; i++)
            {
                msg += $"({(int)(moveCmd.AddressPositions[i].X)},{(int)(moveCmd.AddressPositions[i].Y)})";
            }
            msg += "]\n[AddressActions=";
            for (int i = 0; i < moveCmd.AddressActions.Count; i++)
            {
                msg += $"({moveCmd.AddressActions[i]})";
            }
            msg += "]\n[SectionSpeedLimits=";
            for (int i = 0; i < moveCmd.SectionSpeedLimits.Count; i++)
            {
                msg += $"({moveCmd.SectionSpeedLimits[i]})";
            }
            msg += "]";
            OnMessageShowEvent?.Invoke(this, msg);

            if (moveControlHandler.TransferMove(moveCmd))
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : Call Move Control Work, [TransferMove = true]");
            }
            else
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : Call Move Control Work, [TransferMove = false]");
            }
        }

        public void PrepareForAskingReserve(MoveCmdInfo moveCmd)
        {
            middleAgent.StopAskReserve();
            middleAgent.SetupNeedReserveSections(moveCmd);
            middleAgent.StartAskReserve();
            //SetupNeedReserveSections(moveCmd);
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
                        //Middler send vehicle location to agvc
                        //middleAgent.Send_Cmd134_TransferEventReport();
                        SectionHasFoundPosition = movingSections[searchingSectionIndex];

                        curTransCmd.MovingSectionsIndex = searchingSectionIndex;

                        UpdateMiddlerGotReserveOkSections(SectionHasFoundPosition.Id);

                        //UpdatePlcVehicleBeamSensor(SectionHasFoundPosition);

                        break;
                    }
                    else
                    {
                        searchingSectionIndex++;
                    }
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                      , ex.StackTrace));
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, 1);
                }
            }

            if (searchingSectionIndex == movingSections.Count)
            {
                //gxPosition is not in curTransCmd.MovingSections
                //TODO: PublishAlarm and log
            }
        }

        private void UpdatePlcVehicleBeamSensor(MapSection mapSection)
        {
            var plcVeh = theVehicle.ThePlcVehicle;
            plcVeh.FrontBeamSensorDisable = mapSection.FowardBeamSensorDisable;
            plcVeh.BackBeamSensorDisable = mapSection.BackwardBeamSensorDisable;
            plcVeh.LeftBeamSensorDisable = mapSection.LeftBeamSensorDisable;
            plcVeh.RightBeamSensorDisable = mapSection.RightBeamSensorDisable;
        }

        private void UpdateMiddlerGotReserveOkSections(string id)
        {
            var getReserveOkSections = middleAgent.GetReserveOkSections();
            int getReserveOkSectionIndex = getReserveOkSections.FindIndex(x => x.Id == id);
            if (getReserveOkSectionIndex < 0) return;
            for (int i = 0; i < getReserveOkSectionIndex; i++)
            {
                //Remove passed section in ReserveOkSection
                middleAgent.DequeueGotReserveOkSections();
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
                    SectionHasFoundPosition = theVehicle.CurVehiclePosition.LastSection;
                    isInMap = true;
                    if (mapSection.Type == EnumSectionType.Horizontal)
                    {
                        theVehicle.CurVehiclePosition.RealPosition.Y = mapSection.HeadAddress.Position.Y;
                    }
                    else if (mapSection.Type == EnumSectionType.Vertical)
                    {
                        theVehicle.CurVehiclePosition.RealPosition.X = mapSection.HeadAddress.Position.X;
                    }

                    break;
                }
            }

            if (!isInMap)
            {
                var msg = $"MainFlow : Update VehiclePosition Without MoveCmd +++FAIL+++, [Position=({(int)gxPosition.X},{(int)gxPosition.Y})][LastSectionId={theVehicle.CurVehiclePosition.LastSection.Id}]";
                //OnMessageShowEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
            }
            return isInMap;
        }

        public MapBarcode GetMapBarcodeClone(int baracodeNum)
        {
            var dicBarcodes = TheMapInfo.allBarcodes;
            if (dicBarcodes.ContainsKey(baracodeNum))
            {
                //先 Clone 一份避免被改掉內容
                MapBarcode barcode = dicBarcodes[baracodeNum].DeepClone();
                return barcode;
            }
            else
            {
                return null;
            }
        }

        public MCProtocol GetMcProtocol()
        {
            return mcProtocol;
        }

        public PlcAgent GetPlcAgent()
        {
            return plcAgent;
        }

        private void StartCharge()
        {
            MapAddress address = theVehicle.CurVehiclePosition.LastAddress;
            if (address.IsCharger)
            {
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
                        //Alarm : charge direction error
                        return;
                }

                theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusHandshaking;
                middleAgent.Send_Cmd144_StatusChangeReport();
                plcAgent.ChargeStartCommand(chargeDirection);

                SpinWait.SpinUntil(() => false, mainFlowConfig.StartChargeWaitingTimeMs);
                if (!theVehicle.ThePlcVehicle.Batterys.Charging)
                {
                    //Alarm
                }
                else
                {
                    theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusCharging;
                    middleAgent.Send_Cmd144_StatusChangeReport();
                }


                OnMessageShowEvent?.Invoke(this, $"MainFlow : Start Charging, [Id={address.Id}][IsInCouple={address.IsCharger}][IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]");
            }
            //else
            //{
            //    OnMessageShowEvent?.Invoke(this, $"MainFlow : Start Charging,[Id={address.Id}][IsInCouple={address.IsCharger}][IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]");
            //}
        }

        public void StopCharge()
        {
            var isStopChargeOk = true;
            plcAgent.ChargeStopCommand();
            SpinWait.SpinUntil(() => false, mainFlowConfig.StopChargeWaitingTimeMs);

            if (theVehicle.ThePlcVehicle.Batterys.Charging)
            {
                //Alarm
                isStopChargeOk = false;
            }
            else
            {
                theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusNone;
                middleAgent.Send_Cmd144_StatusChangeReport();
            }

            OnMessageShowEvent?.Invoke(this, $"MainFlow : Stop Charge, [IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]");

            if (!isStopChargeOk)
            {
                StopVehicle();
            }
        }

        public void StopAndClear()
        {
            StopVehicle();
            StopVisitTransferSteps();
            middleAgent.ClearNeedReserveSections();
            middleAgent.StopAskReserve();
            middleAgent.ClearGotReserveOkSections();
            agvcTransCmd = null;
            lastTransferSteps = transferSteps;
            transferSteps = new List<TransferStep>();
            TransferStepsIndex = 0;
            //var msg = $"MainFlow : Stop Vehicle, [MoveState={moveControlHandler.MoveState}][IsCharging={theVehicle.ThePlcVehicle.Batterys.Charging}]";
            //OnMessageShowEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //     , msg));

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
                //Log Error
                OnMessageShowEvent?.Invoke(this, $"MainFlow : GetCurrentEnumTransCmdType, [ex={ex.StackTrace}]");
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
            StopVehicle();
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
            transCmd.EnumCommandType = EnumAgvcTransCommandType.LoadUnload;
            transCmd.LoadAddress = "28015";
            transCmd.ToLoadAddresses = new List<string>();
            transCmd.ToLoadSections = new List<string>();

            transCmd.UnloadAddress = "20013";
            transCmd.ToUnloadAddresses = new List<string>();
            transCmd.ToUnloadAddresses.Add("28015");
            transCmd.ToUnloadAddresses.Add("48014");
            transCmd.ToUnloadAddresses.Add("20013");
            transCmd.ToUnloadSections = new List<string>();
            transCmd.ToUnloadSections.Add("0101");
            transCmd.ToUnloadSections.Add("0092");

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


    }
}
