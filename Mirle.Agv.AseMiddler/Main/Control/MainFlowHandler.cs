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
        private MapConfig mapConfig;
        private AlarmConfig alarmConfig;
        public BatteryLog batteryLog;
        #endregion

        #region TransCmds
        private List<TransferStep> transferSteps = new List<TransferStep>();
        public List<TransferStep> PtransferSteps//200523 dabid+
        {
            get { return transferSteps; }
        }

        public bool GoNextTransferStep { get; set; }
        public int TransferStepsIndex { get; private set; } = 0;
        public bool IsOverrideMove { get; set; }
        public bool IsAvoidMove { get; set; }
        public bool IsAgvcReplySendWaitMessage { get; set; } = false;

        public bool IsArrivalCharge { get; set; } = false;

        #endregion

        #region Controller

        private AgvcConnector agvcConnector;
        private MirleLogger mirleLogger = null;
        private AlarmHandler alarmHandler;
        private MapHandler mapHandler;
        private XmlHandler xmlHandler = new XmlHandler();
        private AsePackage asePackage;
        public UserAgent UserAgent { get; set; } = new UserAgent();

        #endregion

        #region Threads
        private Thread thdVisitTransferSteps;
        public bool IsVisitTransferStepPause { get; set; } = false;

        private Thread thdTrackPosition;
        public bool IsTrackPositionPause { get; set; } = false;

        private Thread thdWatchChargeStage;
        public bool IsWatchChargeStagePause { get; set; } = false;
        #endregion

        #region Events
        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;
        public event EventHandler<string> OnMessageShowEvent;
        public event EventHandler<bool> OnAgvlConnectionChangedEvent;
        public event EventHandler<string> SetAlarmToUI;
        public event EventHandler<string> ResetAllAlarmsToUI;
        #endregion

        #region Models
        public Vehicle Vehicle;
        private bool isIniOk;
        public int InitialSoc { get; set; } = 70;
        public bool IsFirstAhGet { get; set; }
        public EnumCstIdReadResult ReadResult { get; set; } = EnumCstIdReadResult.Normal;
        public string CanAutoMsg { get; set; } = "";
        public DateTime StartChargeTimeStamp { get; set; }
        public DateTime StopChargeTimeStamp { get; set; }

        public LastIdlePosition LastIdlePosition { get; set; } = new LastIdlePosition();

        private ConcurrentQueue<AseMoveStatus> FakeReserveOkAseMoveStatus { get; set; } = new ConcurrentQueue<AseMoveStatus>();
        #endregion

        public MainFlowHandler()
        {
            isIniOk = true;
        }

        #region InitialComponents

        public void InitialMainFlowHandler()
        {
            Vehicle = Vehicle.Instance;
            LoggersInitial();
            XmlInitial();
            VehicleInitial();
            ControllersInitial();
            EventInitial();

            VehicleLocationInitialAndThreadsInitial();

            if (isIniOk)
            {
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "全部"));
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Start Process Ok.");
            }
        }

        private void XmlInitial()
        {
            try
            {
                Vehicle.MainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(@"MainFlow.xml");
                mapConfig = xmlHandler.ReadXml<MapConfig>(@"Map.xml");
                Vehicle.AgvcConnectorConfig = xmlHandler.ReadXml<AgvcConnectorConfig>(@"AgvcConnectorConfig.xml");
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
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "紀錄器缺少 Log.ini"));
            }
        }

        private void ControllersInitial()
        {
            try
            {
                alarmHandler = new AlarmHandler(this);
                mapHandler = new MapHandler(mapConfig);               
                agvcConnector = new AgvcConnector(this);
                asePackage = new AsePackage();
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "控制層"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "控制層"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void VehicleInitial()
        {
            try
            {
                IsFirstAhGet = Vehicle.MainFlowConfig.IsSimulation;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "台車"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "台車"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息, Send to MainFlow(this)'mapHandler
                agvcConnector.OnInstallTransferCommandEvent += AgvcConnector_OnInstallTransferCommandEvent;
                agvcConnector.OnOverrideCommandEvent += AgvcConnector_OnOverrideCommandEvent;
                agvcConnector.OnAvoideRequestEvent += AgvcConnector_OnAvoideRequestEvent;
                agvcConnector.OnLogMsgEvent += LogMsgHandler;
                agvcConnector.OnRenameCassetteIdEvent += AgvcConnector_OnRenameCassetteIdEvent;
                //agvcConnector.OnCassetteIdReadReplyAbortCommandEvent += AgvcConnector_OnCassetteIdReadReplyAbortCommandEvent;
                agvcConnector.OnStopClearAndResetEvent += AgvcConnector_OnStopClearAndResetEvent;
                agvcConnector.OnConnectionChangeEvent += AgvcConnector_OnConnectionChangeEvent;

                agvcConnector.OnAgvcAcceptMoveArrivalEvent += AgvcConnector_OnAgvcAcceptMoveArrivalEvent;
                agvcConnector.OnAgvcAcceptLoadArrivalEvent += AgvcConnector_OnAgvcAcceptLoadArrivalEvent;
                agvcConnector.OnAgvcAcceptLoadCompleteEvent += AgvcConnector_OnAgvcAcceptLoadCompleteEvent;
                agvcConnector.OnAgvcAcceptBcrReadReply += AgvcConnector_OnAgvcContinueBcrReadEvent;
                agvcConnector.OnAgvcAcceptUnloadArrivalEvent += AgvcConnector_OnAgvcAcceptUnloadArrivalEvent;
                agvcConnector.OnAgvcAcceptUnloadCompleteEvent += AgvcConnector_OnAgvcAcceptUnloadCompleteEvent;
                agvcConnector.OnSendRecvTimeoutEvent += AgvcConnector_OnSendRecvTimeoutEvent;
                agvcConnector.OnCstRenameEvent += AgvcConnector_OnCstRenameEvent;

                //來自MoveControl的移動結束訊息, Send to MainFlow(this)'middleAgent'mapHandler
                //asePackage.OnPositionChangeEvent += AsePackage_OnPositionChangeEvent;
                //asePackage.OnPartMoveArrivalEvent += AsePackage_OnPartMoveArrivalEvent;
                //asePackage.OnMoveFinishedEvent += AseMoveControl_OnMoveFinished;
                //asePackage.aseMoveControl.OnRetryMoveFinishEvent += AseMoveControl_OnRetryMoveFinished;
                asePackage.OnUpdateSlotStatusEvent += AsePackage_OnUpdateSlotStatusEvent;

                asePackage.OnModeChangeEvent += AsePackage_OnModeChangeEvent;


                //來自IRobotControl的取放貨結束訊息, Send to MainFlow(this)'middleAgent'mapHandler
                asePackage.OnRobotInterlockErrorEvent += AsePackage_OnRobotInterlockErrorEvent;
                asePackage.OnRobotCommandFinishEvent += AsePackage_OnRobotCommandFinishEvent;
                asePackage.OnRobotCommandErrorEvent += AsePackage_OnRobotCommandErrorEvent;

                //來自IRobot的CarrierId讀取訊息, Send to middleAgent
                //asePackage.OnReadCarrierIdFinishEvent += AsePackage_OnReadCarrierIdFinishEvent;

                //來自IBatterysControl的電量改變訊息, Send to middleAgent
                asePackage.OnBatteryPercentageChangeEvent += agvcConnector.AseBatteryControl_OnBatteryPercentageChangeEvent;
                asePackage.OnBatteryPercentageChangeEvent += AseBatteryControl_OnBatteryPercentageChangeEvent;

                asePackage.OnStatusChangeReportEvent += AsePackage_OnStatusChangeReportEvent;

                asePackage.OnAlarmCodeSetEvent += AsePackage_OnAlarmCodeSetEvent1;
                asePackage.OnAlarmCodeResetEvent += AsePackage_OnAlarmCodeResetEvent;

                asePackage.OnConnectionChangeEvent += AsePackage_OnConnectionChangeEvent;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "事件"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "事件"));

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void VehicleLocationInitialAndThreadsInitial()
        {
            if (Vehicle.MainFlowConfig.IsSimulation)
            {
                try
                {
                    Vehicle.AseMoveStatus.LastMapPosition = Vehicle.Mapinfo.addressMap.First(x => x.Key != "").Value.Position;
                }
                catch (Exception ex)
                {
                    Vehicle.AseMoveStatus.LastMapPosition = new MapPosition();
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
            }
            else
            {
                if (asePackage.IsConnected())
                {
                    asePackage.AllAgvlStatusReportRequest();
                }
            }
            StartVisitTransferSteps();
            StartTrackPosition();
            StartWatchChargeStage();
            var msg = $"讀取到的電量為{batteryLog.InitialSoc}";
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
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
                if (IsVisitTransferStepPause)
                {
                    SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);
                    continue;
                }
                try
                {
                    if (GoNextTransferStep)
                    {
                        GoNextTransferStep = false;
                        if (TransferStepsIndex < 0)
                        {
                            TransferStepsIndex = 0;
                            GoNextTransferStep = true;
                            SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);

                            continue;
                        }
                        if (transferSteps.Count == 0)
                        {
                            if (Vehicle.AgvcTransCmdBuffer.Count > 0)
                            {
                                if (!Vehicle.IsOptimize)
                                    GetTransferCommandToDo();
                            }
                            else
                            {
                                transferSteps.Add(new EmptyTransferStep());
                            }
                            GoNextTransferStep = true;
                            SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);

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
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);
                }
            }
        }

        private void GetTransferCommandToDo()
        {
            transferSteps = new List<TransferStep>();
            TransferStepsIndex = 0;

            if (Vehicle.AgvcTransCmdBuffer.Count > 1)
            {
                if (!Vehicle.MainFlowConfig.DualCommandOptimize)
                {
                    List<AgvcTransCmd> agvcTransCmds = Vehicle.AgvcTransCmdBuffer.Values.ToList();
                    AgvcTransCmd onlyCmd = agvcTransCmds[0];

                    switch (onlyCmd.EnrouteState)
                    {
                        case CommandState.None:
                            TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                            transferSteps.Add(new EmptyTransferStep());
                            break;
                        case CommandState.LoadEnroute:
                            TransferStepsAddMoveCmdInfo(onlyCmd.LoadAddressId, onlyCmd.CommandId);
                            TransferStepsAddLoadCmdInfo(onlyCmd);
                            break;
                        case CommandState.UnloadEnroute:
                            TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                            TransferStepsAddUnloadCmdInfo(onlyCmd);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    List<AgvcTransCmd> agvcTransCmds = Vehicle.AgvcTransCmdBuffer.Values.ToList();
                    AgvcTransCmd cmd001 = agvcTransCmds[0];

                    switch (cmd001.EnrouteState)
                    {
                        case CommandState.None:
                            {
                                TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                transferSteps.Add(new EmptyTransferStep());
                            }
                            break;
                        case CommandState.LoadEnroute:
                            {
                                var disCmd001Load = DistanceFromLastPosition(cmd001.LoadAddressId);

                                AgvcTransCmd cmd002 = agvcTransCmds[1];
                                if (cmd002.EnrouteState == CommandState.LoadEnroute)
                                {
                                    var disCmd002Load = DistanceFromLastPosition(cmd002.LoadAddressId);
                                    if (disCmd001Load <= disCmd002Load)
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd001.LoadAddressId, cmd001.CommandId);
                                        TransferStepsAddLoadCmdInfo(cmd001);
                                    }
                                    else
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd002.LoadAddressId, cmd002.CommandId);
                                        TransferStepsAddLoadCmdInfo(cmd002);
                                    }
                                }
                                else if (cmd002.EnrouteState == CommandState.UnloadEnroute)
                                {
                                    var disCmd002Unload = DistanceFromLastPosition(cmd002.UnloadAddressId);
                                    if (disCmd001Load <= disCmd002Unload)
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd001.LoadAddressId, cmd001.CommandId);
                                        TransferStepsAddLoadCmdInfo(cmd001);
                                    }
                                    else
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd002.UnloadAddressId, cmd002.CommandId);
                                        TransferStepsAddUnloadCmdInfo(cmd002);
                                    }
                                }
                                else
                                {
                                    StopClearAndReset();
                                    SetAlarmFromAgvm(1);
                                }
                            }
                            break;
                        case CommandState.UnloadEnroute:
                            {
                                var disCmd001Unload = DistanceFromLastPosition(cmd001.UnloadAddressId);

                                AgvcTransCmd cmd002 = agvcTransCmds[1];
                                if (cmd002.EnrouteState == CommandState.LoadEnroute)
                                {
                                    var disCmd002Load = DistanceFromLastPosition(cmd002.LoadAddressId);
                                    if (disCmd001Unload <= disCmd002Load)
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                        TransferStepsAddUnloadCmdInfo(cmd001);
                                    }
                                    else
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd002.LoadAddressId, cmd002.CommandId);
                                        TransferStepsAddLoadCmdInfo(cmd002);
                                    }
                                }
                                else if (cmd002.EnrouteState == CommandState.UnloadEnroute)
                                {
                                    var disCmd002Unload = DistanceFromLastPosition(cmd002.UnloadAddressId);
                                    if (disCmd001Unload <= disCmd002Unload)
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                        TransferStepsAddUnloadCmdInfo(cmd001);
                                    }
                                    else
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd002.UnloadAddressId, cmd002.CommandId);
                                        TransferStepsAddUnloadCmdInfo(cmd002);
                                    }
                                }
                                else
                                {
                                    StopClearAndReset();
                                    SetAlarmFromAgvm(1);
                                }
                            }
                            break;
                        default:
                            break;
                    }

                }
            }
            else if (Vehicle.AgvcTransCmdBuffer.Count == 1)
            {
                List<AgvcTransCmd> agvcTransCmds = Vehicle.AgvcTransCmdBuffer.Values.ToList();
                AgvcTransCmd onlyCmd = agvcTransCmds[0];

                switch (onlyCmd.EnrouteState)
                {
                    case CommandState.None:
                        TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                        transferSteps.Add(new EmptyTransferStep());
                        break;
                    case CommandState.LoadEnroute:
                        TransferStepsAddMoveCmdInfo(onlyCmd.LoadAddressId, onlyCmd.CommandId);
                        TransferStepsAddLoadCmdInfo(onlyCmd);
                        break;
                    case CommandState.UnloadEnroute:
                        TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                        TransferStepsAddUnloadCmdInfo(onlyCmd);
                        break;
                    default:
                        break;
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
                    StopCharge();
                    if (moveCmdInfo.EndAddress.Id == Vehicle.AseMoveStatus.LastAddress.Id)
                    {
                        if (Vehicle.MainFlowConfig.IsSimulation)
                        {
                            AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
                        }
                        else
                        {
                            if (Vehicle.IsReAuto)
                            {
                                Vehicle.IsReAuto = false;
                                if (IsAvoidMove)
                                {
                                    asePackage.PartMove(EnumAseMoveCommandIsEnd.End);
                                }
                                else
                                {
                                    var cmdId = moveCmdInfo.CmdId;
                                    var transferCommand = Vehicle.AgvcTransCmdBuffer[cmdId];
                                    if (Vehicle.AgvcTransCmdBuffer.Count == 0)
                                    {
                                        asePackage.PartMove(EnumAseMoveCommandIsEnd.End);
                                    }
                                    else if (Vehicle.AgvcTransCmdBuffer.Count == 1)
                                    {
                                        switch (transferCommand.SlotNumber)
                                        {
                                            case EnumSlotNumber.L:
                                                asePackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Left);
                                                break;
                                            case EnumSlotNumber.R:
                                                asePackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Right);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        //TODO : check if need open both slot.
                                        switch (transferCommand.SlotNumber)
                                        {
                                            case EnumSlotNumber.L:
                                                asePackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Left);
                                                break;
                                            case EnumSlotNumber.R:
                                                asePackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Right);
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
                            }
                        }
                    }
                    else
                    {
                        Vehicle.AseMovingGuide.CommandId = moveCmdInfo.CmdId;
                        agvcConnector.ReportSectionPass();
                        if (!Vehicle.IsCharging)
                        {
                            Vehicle.AseMoveStatus.IsMoveEnd = false;
                            asePackage.PartMove(EnumAseMoveCommandIsEnd.Begin);
                            agvcConnector.AskGuideAddressesAndSections(moveCmdInfo);
                        }
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

            var msg = $"MainFlow : StartVisitTransferSteps";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void PauseVisitTransferSteps()
        {
            IsVisitTransferStepPause = true;
            if (Vehicle.AseMovingGuide.PauseStatus == VhStopSingle.Off)
            {
                Vehicle.AseMovingGuide.PauseStatus = VhStopSingle.On;
                agvcConnector.StatusChangeReport();
            }
            var msg = $"MainFlow : PauseVisitTransferSteps";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void ResumeVisitTransferSteps()
        {
            IsVisitTransferStepPause = false;
            if (Vehicle.AseMovingGuide.PauseStatus == VhStopSingle.On)
            {
                Vehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                agvcConnector.StatusChangeReport();
            }
            var msg = $"MainFlow : ResumeVisitTransferSteps";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void ClearTransferSteps(string cmdId)
        {
            List<TransferStep> tempTransferSteps = new List<TransferStep>();
            foreach (var step in transferSteps)
            {
                if (step.CmdId != cmdId)
                {
                    tempTransferSteps.Add(step);
                }
            }
            transferSteps = tempTransferSteps;

            var msg = $"MainFlow : Clear Finished Command[{cmdId}]";
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

        #region Handle Transfer Command

        private void AgvcConnector_OnInstallTransferCommandEvent(object sender, AgvcTransCmd agvcTransCmd)
        {
            var msg = $"MainFlow : Get [{agvcTransCmd.AgvcTransCommandType}] command [{agvcTransCmd.CommandId}]";
            OnMessageShowEvent?.Invoke(this, msg);

            #region 檢查搬送Command
            try
            {
                switch (agvcTransCmd.AgvcTransCommandType)
                {
                    case EnumAgvcTransCommandType.Move:
                    case EnumAgvcTransCommandType.MoveToCharger:
                        CheckMoveEndAddress(agvcTransCmd.UnloadAddressId);
                        break;
                    case EnumAgvcTransCommandType.Load:
                        CheckLoadPortAddress(agvcTransCmd.LoadAddressId);
                        CheckCstIdDuplicate(agvcTransCmd.CassetteId);
                        agvcTransCmd.SlotNumber = CheckVehicleSlot();
                        break;
                    case EnumAgvcTransCommandType.Unload:
                        CheckUnloadPortAddress(agvcTransCmd.UnloadAddressId);
                        agvcTransCmd.SlotNumber = CheckUnloadCstId(agvcTransCmd.CassetteId);
                        break;
                    case EnumAgvcTransCommandType.LoadUnload:
                        CheckLoadPortAddress(agvcTransCmd.LoadAddressId);
                        CheckUnloadPortAddress(agvcTransCmd.UnloadAddressId);
                        CheckCstIdDuplicate(agvcTransCmd.CassetteId);
                        agvcTransCmd.SlotNumber = CheckVehicleSlot();
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 1, ex.Message);
                string reason = $"MainFlow : Reject {agvcTransCmd.AgvcTransCommandType} Command. {ex.Message}";
                OnMessageShowEvent?.Invoke(this, reason);
                return;
            }
            #endregion

            #region 搬送流程更新
            try
            {
                PauseTransfer();
                Vehicle.AgvcTransCmdBuffer.TryAdd(agvcTransCmd.CommandId, agvcTransCmd);
                agvcConnector.Commanding();
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 0, "");
                asePackage.SetTransferCommandInfoRequest(agvcTransCmd, EnumCommandInfoStep.Begin);
                if (Vehicle.AgvcTransCmdBuffer.Count == 1)
                {
                    InitialTransferSteps(agvcTransCmd);
                    GoNextTransferStep = true;
                }
                ResumeTransfer();
                var okMsg = $"MainFlow : Get  {agvcTransCmd.AgvcTransCommandType}Command{agvcTransCmd.CommandId}  checked .";
                OnMessageShowEvent?.Invoke(this, okMsg);
            }
            catch (Exception ex)
            {
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 1, "");
                var ngMsg = $"MainFlow :  Get  {agvcTransCmd.AgvcTransCommandType}Command{agvcTransCmd.CommandId}  fail .";
                OnMessageShowEvent?.Invoke(this, ngMsg);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            #endregion
        }

        private void CheckCstIdDuplicate(string cassetteId)
        {
            var agvcTransCmdBuffer = Vehicle.AgvcTransCmdBuffer.Values.ToList();
            for (int i = 0; i < agvcTransCmdBuffer.Count; i++)
            {
                if (agvcTransCmdBuffer[i].CassetteId == cassetteId)
                {
                    throw new Exception("Transfer command casette ID duplicate.");
                }
            }
        }

        private EnumSlotNumber CheckVehicleSlot()
        {
            var leftSlotState = Vehicle.AseCarrierSlotL.CarrierSlotStatus;
            var rightSlotState = Vehicle.AseCarrierSlotR.CarrierSlotStatus;
            var agvcTransCmdBuffer = Vehicle.AgvcTransCmdBuffer.Values.ToList();

            switch (Vehicle.MainFlowConfig.SlotDisable)
            {
                case EnumSlotSelect.None:
                    {
                        if (leftSlotState != EnumAseCarrierSlotStatus.Empty && rightSlotState != EnumAseCarrierSlotStatus.Empty)
                        {
                            throw new Exception("Vehicle has no Slot to load.");
                        }
                        else if (Vehicle.AgvcTransCmdBuffer.Count >= 2)
                        {
                            throw new Exception("Vehicle has two other transfer command. Vehicle has no Slot to load.");
                        }
                        else if (Vehicle.AgvcTransCmdBuffer.Count == 1)
                        {
                            var curCmd = Vehicle.AgvcTransCmdBuffer.Values.First();
                            switch (curCmd.EnrouteState)
                            {

                                case CommandState.LoadEnroute:
                                case CommandState.UnloadEnroute:
                                    if (curCmd.SlotNumber == EnumSlotNumber.L)
                                    {
                                        if (rightSlotState != EnumAseCarrierSlotStatus.Empty)
                                        {
                                            throw new Exception("Vehicle has no Slot to load.");
                                        }
                                        else
                                        {
                                            return EnumSlotNumber.R;
                                        }
                                    }
                                    else
                                    {
                                        if (leftSlotState != EnumAseCarrierSlotStatus.Empty)
                                        {
                                            throw new Exception("Vehicle has no Slot to load.");
                                        }
                                        else
                                        {
                                            return EnumSlotNumber.L;
                                        }
                                    }
                                case CommandState.None:
                                default:
                                    if (leftSlotState == EnumAseCarrierSlotStatus.Empty)
                                    {
                                        return EnumSlotNumber.L;
                                    }
                                    else
                                    {
                                        return EnumSlotNumber.R;
                                    }
                            }
                        }
                        else
                        {
                            if (leftSlotState == EnumAseCarrierSlotStatus.Empty)
                            {
                                return EnumSlotNumber.L;
                            }
                            else
                            {
                                return EnumSlotNumber.R;
                            }
                        }
                    }
                case EnumSlotSelect.Left:
                    {
                        if (Vehicle.AgvcTransCmdBuffer.Count >= 2)
                        {
                            throw new Exception("Vehicle has two other transfer command. Vehicle has no Slot to load.");
                        }
                        else
                        {
                            if (rightSlotState != EnumAseCarrierSlotStatus.Empty)
                            {
                                throw new Exception("Left Slot is disable. Vehicle has no Slot to load.");

                            }
                            else
                            {
                                return EnumSlotNumber.R;
                            }
                        }
                    }
                case EnumSlotSelect.Right:
                    {
                        if (Vehicle.AgvcTransCmdBuffer.Count >= 2)
                        {
                            throw new Exception("Vehicle has two other transfer command. Vehicle has no Slot to load.");
                        }
                        else
                        {
                            if (leftSlotState != EnumAseCarrierSlotStatus.Empty)
                            {
                                throw new Exception("Right Slot is disable. Vehicle has no Slot to load.");
                            }
                            else
                            {
                                return EnumSlotNumber.L;
                            }
                        }
                    }
                case EnumSlotSelect.Both:
                    throw new Exception("Both Slot is disable. Can not do transfer command.");
                default:
                    {
                        if (leftSlotState != EnumAseCarrierSlotStatus.Empty && rightSlotState != EnumAseCarrierSlotStatus.Empty)
                        {
                            throw new Exception("Vehicle has no Slot to load.");
                        }
                        else if (Vehicle.AgvcTransCmdBuffer.Count >= 2)
                        {
                            throw new Exception("Vehicle has two other transfer command. Vehicle has no Slot to load.");
                        }
                        else if (Vehicle.AgvcTransCmdBuffer.Count == 1)
                        {
                            var curCmd = Vehicle.AgvcTransCmdBuffer.Values.First();
                            switch (curCmd.EnrouteState)
                            {

                                case CommandState.LoadEnroute:
                                case CommandState.UnloadEnroute:
                                    if (curCmd.SlotNumber == EnumSlotNumber.L)
                                    {
                                        if (rightSlotState != EnumAseCarrierSlotStatus.Empty)
                                        {
                                            throw new Exception("Vehicle has no Slot to load.");
                                        }
                                        else
                                        {
                                            return EnumSlotNumber.R;
                                        }
                                    }
                                    else
                                    {
                                        if (leftSlotState != EnumAseCarrierSlotStatus.Empty)
                                        {
                                            throw new Exception("Vehicle has no Slot to load.");
                                        }
                                        else
                                        {
                                            return EnumSlotNumber.L;
                                        }
                                    }
                                case CommandState.None:
                                default:
                                    if (leftSlotState == EnumAseCarrierSlotStatus.Empty)
                                    {
                                        return EnumSlotNumber.L;
                                    }
                                    else
                                    {
                                        return EnumSlotNumber.R;
                                    }
                            }
                        }
                        else
                        {
                            if (leftSlotState == EnumAseCarrierSlotStatus.Empty)
                            {
                                return EnumSlotNumber.L;
                            }
                            else
                            {
                                return EnumSlotNumber.R;
                            }
                        }
                    }
            }

        }

        private EnumSlotNumber CheckUnloadCstId(string cassetteId)
        {
            if (Vehicle.AseCarrierSlotL.CarrierId.ToUpper().Trim() == cassetteId)
            {
                return EnumSlotNumber.L;
            }
            else if (Vehicle.AseCarrierSlotR.CarrierId.ToUpper().Trim() == cassetteId)
            {
                return EnumSlotNumber.R;
            }
            else
            {
                throw new Exception($"No [{cassetteId}] to unload.");
            }
        }

        private void CheckOverrideAddress(AgvcTransCmd agvcTransCmd)
        {
            return;
        }

        private void CheckUnloadPortAddress(string unloadAddressId)
        {
            CheckMoveEndAddress(unloadAddressId);
            MapAddress unloadAddress = Vehicle.Mapinfo.addressMap[unloadAddressId];
            if (!unloadAddress.IsTransferPort())
            {
                throw new Exception($"{unloadAddressId} can not unload.");
            }
        }

        private void CheckLoadPortAddress(string loadAddressId)
        {
            CheckMoveEndAddress(loadAddressId);
            MapAddress loadAddress = Vehicle.Mapinfo.addressMap[loadAddressId];
            if (!loadAddress.IsTransferPort())
            {
                throw new Exception($"{loadAddressId} can not load.");
            }
        }

        private void CheckMoveEndAddress(string unloadAddressId)
        {
            if (!Vehicle.Mapinfo.addressMap.ContainsKey(unloadAddressId))
            {
                throw new Exception($"{unloadAddressId} is not in the map.");
            }
        }

        private void AgvcConnector_OnOverrideCommandEvent(object sender, AgvcOverrideCmd agvcOverrideCmd)
        {
            var msg = $"MainFlow :  Get [ Override ]Command[{agvcOverrideCmd.CommandId}],  start check .";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        private void AgvcConnector_OnAvoideRequestEvent(object sender, AseMovingGuide aseMovingGuide)
        {
            #region 避車檢查
            try
            {
                var msg = $"MainFlow :  Get Avoid Command, End Adr=[{aseMovingGuide.ToAddressId}],  start check .";
                OnMessageShowEvent?.Invoke(this, msg);

                agvcConnector.PauseAskReserve();

                if (Vehicle.AgvcTransCmdBuffer.Count == 0)
                {
                    throw new Exception("Vehicle has no Command, can not Avoid");
                }

                if (!IsMoveStep())
                {
                    throw new Exception("Vehicle is not moving, can not Avoid");
                }

                if (!IsMoveStopByNoReserve() && !Vehicle.AseMovingGuide.IsAvoidComplete)
                {
                    throw new Exception($"Vehicle is not stop by no reserve, can not Avoid");
                }

                //if (!IsAvoidCommandMatchTheMap(agvcMoveCmd))
                //{
                //    var reason = "避車路徑中 Port Adr 與路段不合圖資";
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

            #region 避車Command生成
            try
            {
                agvcConnector.PauseAskReserve();
                agvcConnector.ClearAllReserve();
                Vehicle.AseMovingGuide = aseMovingGuide;
                SetupAseMovingGuideMovingSections();
                agvcConnector.SetupNeedReserveSections();
                agvcConnector.ReplyAvoidCommand(aseMovingGuide, 0, "");
                var okmsg = $"MainFlow : Get 避車Command checked , 終點[{aseMovingGuide.ToAddressId}].";
                OnMessageShowEvent?.Invoke(this, okmsg);
                IsAvoidMove = true;
                //agvcConnector.AskAllSectionsReserveInOnce();
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                StopClearAndReset();
                var reason = "避車Exception";
                RejectAvoidCommandAndResume(000036, reason, aseMovingGuide);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            #endregion
        }

        private bool IsMoveStopByNoReserve()
        {
            return Vehicle.AseMovingGuide.ReserveStop == VhStopSingle.On;
        }

        private void RejectAvoidCommandAndResume(int alarmCode, string reason, AseMovingGuide aseMovingGuide)
        {
            try
            {
                SetAlarmFromAgvm(alarmCode);
                agvcConnector.ReplyAvoidCommand(aseMovingGuide, 1, reason);
                reason = $"MainFlow : Reject Avoid Command, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Convert AgvcTransferCommand to TransferSteps

        private void InitialTransferSteps(AgvcTransCmd agvcTransCmd)
        {
            TransferStepsIndex = 0;

            switch (agvcTransCmd.AgvcTransCommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandId);
                    transferSteps.Add(new EmptyTransferStep());
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
                    break;
                case EnumAgvcTransCommandType.MoveToCharger:
                    TransferStepsAddMoveToChargerCmdInfo(agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandId);
                    transferSteps.Add(new EmptyTransferStep());
                    break;
                case EnumAgvcTransCommandType.Override:
                case EnumAgvcTransCommandType.Else:
                default:
                    break;
            }
        }

        private void TransferStepsAddUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            UnloadCmdInfo unloadCmdInfo = new UnloadCmdInfo(agvcTransCmd);
            MapAddress unloadAddress = Vehicle.Mapinfo.addressMap[unloadCmdInfo.PortAddressId];
            unloadCmdInfo.GateType = unloadAddress.GateType;
            if (string.IsNullOrEmpty(agvcTransCmd.UnloadPortId) || !unloadAddress.PortIdMap.ContainsKey(agvcTransCmd.UnloadPortId))
            {
                unloadCmdInfo.PortNumber = "1";
            }
            else
            {
                unloadCmdInfo.PortNumber = unloadAddress.PortIdMap[agvcTransCmd.UnloadPortId];
            }
            transferSteps.Add(unloadCmdInfo);
        }

        private void TransferStepsAddLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            LoadCmdInfo loadCmdInfo = new LoadCmdInfo(agvcTransCmd);
            MapAddress loadAddress = Vehicle.Mapinfo.addressMap[loadCmdInfo.PortAddressId];
            loadCmdInfo.GateType = loadAddress.GateType;
            if (string.IsNullOrEmpty(agvcTransCmd.LoadPortId) || !loadAddress.PortIdMap.ContainsKey(agvcTransCmd.LoadPortId))
            {
                loadCmdInfo.PortNumber = "1";
            }
            else
            {
                loadCmdInfo.PortNumber = loadAddress.PortIdMap[agvcTransCmd.LoadPortId];
            }
            transferSteps.Add(loadCmdInfo);
        }

        private void TransferStepsAddMoveCmdInfo(string endAddressId, string cmdId)
        {
            MapAddress endAddress = Vehicle.Mapinfo.addressMap[endAddressId];
            MoveCmdInfo moveCmd = new MoveCmdInfo(endAddress, cmdId);
            transferSteps.Add(moveCmd);
        }

        private void TransferStepsAddMoveToChargerCmdInfo(string endAddressId, string cmdId)
        {
            MapAddress endAddress = Vehicle.Mapinfo.addressMap[endAddressId];
            MoveToChargerCmdInfo moveCmd = new MoveToChargerCmdInfo(endAddress, cmdId);
            transferSteps.Add(moveCmd);
        }

        #endregion

        #endregion

        #region Thd Watch Charge Stage

        private void WatchChargeStage()
        {
            while (true)
            {
                try
                {

                    if (!Vehicle.MainFlowConfig.UseChargeSystemV2)
                    {
                        if (Vehicle.AutoState == EnumAutoState.Auto && IsVehicleIdle() && !Vehicle.IsOptimize)
                        {
                            if (IsLowPower() && !Vehicle.IsCharging)
                            {
                                LowPowerStartCharge(Vehicle.AseMoveStatus.LastAddress);
                            }
                        }
                        if (Vehicle.AseBatteryStatus.Percentage < Vehicle.MainFlowConfig.LowPowerPercentage - 11 && !Vehicle.IsCharging)//200701 dabid+
                        {
                            SetAlarmFromAgvm(2);
                        }
                    }
                    else
                    {
                        if (IsWatchChargeStagePause)
                        {
                            SpinWait.SpinUntil(() => !IsWatchChargeStagePause, Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs);
                            continue;
                        }

                        switch (Vehicle.ChargingStage)
                        {
                            case EnumChargingStage.Idle:
                                {
                                    CheckLowPowerChargeCondition();
                                }
                                break;
                            case EnumChargingStage.ArrivalCharge:
                                {
                                    CheckStartChargeCondition();
                                }
                                break;
                            case EnumChargingStage.WaitChargingOn:
                                {
                                    CheckStartChargeTimeout();
                                }
                                break;
                            case EnumChargingStage.LowPowerCharge:
                                {
                                    CheckStartChargeCondition();
                                }
                                break;
                            case EnumChargingStage.DisCharge:
                                {
                                    StageStopCharge();
                                }
                                break;
                            case EnumChargingStage.WaitChargingOff:
                                {
                                    CheckStopChargeTimeout();
                                }
                                break;
                            default:
                                break;
                        }

                    }

                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs);
                }
            }
        }

        private void CheckStartChargeCondition()
        {
            if (!Vehicle.AseMoveStatus.LastAddress.IsCharger())
            {
                Vehicle.ChargingStage = EnumChargingStage.Idle;
                return;
            }

            if (IsHighPower())
            {
                Vehicle.ChargingStage = EnumChargingStage.Idle;
                return;
            }

            StageStartCharge();
        }

        private void CheckStopChargeTimeout()
        {
            if (Vehicle.CheckStopChargeReplyEnd)
            {
                OnMessageShowEvent?.Invoke(this, $"Stop Charge success.");
                Vehicle.ChargingStage = EnumChargingStage.Idle;
                agvcConnector.ChargeOff();
            }
            else
            {
                var curTime = DateTime.Now;
                if ((curTime - StopChargeTimeStamp).TotalMilliseconds >= Vehicle.MainFlowConfig.StopChargeWaitingTimeoutMs)
                {
                    SetAlarmFromAgvm(000014);
                    AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
                }
            }
        }

        private void CheckStartChargeTimeout()
        {
            if (Vehicle.CheckStartChargeReplyEnd)
            {
                OnMessageShowEvent?.Invoke(this, "Start Charge success.");
                Vehicle.ChargingStage = EnumChargingStage.Idle;
                agvcConnector.Charging();
            }
            else
            {
                var curTime = DateTime.Now;
                if ((curTime - StartChargeTimeStamp).TotalMilliseconds >= Vehicle.MainFlowConfig.StartChargeWaitingTimeoutMs)
                {
                    SetAlarmFromAgvm(000013);
                    Vehicle.IsCharging = true;
                    Vehicle.ChargingStage = EnumChargingStage.DisCharge;
                }
            }
        }

        private void CheckLowPowerChargeCondition()
        {
            if (IsLowPower() && Vehicle.AutoState == EnumAutoState.Auto && IsVehicleIdle() && !Vehicle.IsOptimize && !Vehicle.IsCharging)
            {
                Vehicle.ChargingStage = EnumChargingStage.LowPowerCharge;
            }
        }

        private bool IsVehicleIdle()
        {
            if (transferSteps.Count == 0)
            {
                return true;
            }
            else if (transferSteps.Count == 1)
            {
                if (GetCurrentTransferStepType() == EnumTransferStepType.Empty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void StartWatchChargeStage()
        {
            thdWatchChargeStage = new Thread(WatchChargeStage);
            thdWatchChargeStage.IsBackground = true;
            thdWatchChargeStage.Start();
            OnMessageShowEvent?.Invoke(this, $"StartWatchChargeStage");
        }

        public void StageStartCharge()
        {
            OnMessageShowEvent?.Invoke(this, $@"Stage Start Charge.");

            agvcConnector.ChargHandshaking();

            if (Vehicle.MainFlowConfig.IsSimulation)
            {
                Vehicle.ChargingStage = EnumChargingStage.Idle;
                Vehicle.IsCharging = true;
                agvcConnector.Charging();
                return;
            }

            Vehicle.CheckStartChargeReplyEnd = false;
            StartChargeTimeStamp = DateTime.Now;

            Vehicle.ChargingStage = EnumChargingStage.WaitChargingOn;
            asePackage.StartCharge(EnumAddressDirection.Right);
        }

        public void StageStopCharge()
        {
            OnMessageShowEvent?.Invoke(this, $@"Stage Stop Charge.");

            agvcConnector.ChargHandshaking();

            if (Vehicle.MainFlowConfig.IsSimulation)
            {
                Vehicle.ChargingStage = EnumChargingStage.Idle;
                Vehicle.IsCharging = false;
                agvcConnector.ChargeOff();
                return;
            }

            Vehicle.CheckStartChargeReplyEnd = false;
            StopChargeTimeStamp = DateTime.Now;

            Vehicle.ChargingStage = EnumChargingStage.WaitChargingOff;
            asePackage.StopCharge();
        }

        private bool IsLowPower()
        {
            return Vehicle.AseBatteryStatus.Percentage <= Vehicle.MainFlowConfig.LowPowerPercentage;
        }

        private bool IsHighPower()
        {
            return Vehicle.AseBatteryStatus.Percentage >= Vehicle.MainFlowConfig.HighPowerPercentage;
        }

        public void StartCharge()
        {
            StartCharge(Vehicle.AseMoveStatus.LastAddress);
        }

        private void StartCharge(MapAddress endAddress)
        {
            try
            {
                IsArrivalCharge = true;
                var address = endAddress;
                var percentage = Vehicle.AseBatteryStatus.Percentage;
                var highPercentage = Vehicle.MainFlowConfig.HighPowerPercentage;

                if (address.IsCharger())
                {
                    if (IsHighPower())
                    {
                        var msg = $"Vehicle arrival {address.Id},Charge Direction = {address.ChargeDirection},Precentage = {percentage} > {highPercentage}(High Threshold),  thus NOT send charge command.";
                        OnMessageShowEvent?.Invoke(this, msg);
                        return;
                    }

                    agvcConnector.ChargHandshaking();
                    Vehicle.IsCharging = true;
                    agvcConnector.Charging();

                    OnMessageShowEvent?.Invoke(this, $@"Start Charge, Vehicle arrival {address.Id},Charge Direction = {address.ChargeDirection},Precentage = {percentage}.");

                    if (Vehicle.MainFlowConfig.IsSimulation) return;

                    Vehicle.CheckStartChargeReplyEnd = false;
                    asePackage.StartCharge(address.ChargeDirection);

                    SpinWait.SpinUntil(() => Vehicle.CheckStartChargeReplyEnd, 30 * 1000);

                    if (Vehicle.CheckStartChargeReplyEnd)
                    {
                        OnMessageShowEvent?.Invoke(this, "Start Charge success.");
                        batteryLog.ChargeCount++;
                    }
                    else
                    {
                        Vehicle.IsCharging = false;
                        SetAlarmFromAgvm(000013);
                        asePackage.ChargeStatusRequest();
                        SpinWait.SpinUntil(() => false, 500);
                        asePackage.StopCharge();
                    }

                    Vehicle.CheckStartChargeReplyEnd = true;
                    IsArrivalCharge = false;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                IsArrivalCharge = false;
            }
        }

        private void LowPowerStartCharge(MapAddress lastAddress)
        {
            try
            {
                if (IsArrivalCharge) return;

                var address = lastAddress;
                var percentage = Vehicle.AseBatteryStatus.Percentage;
                var pos = Vehicle.AseMoveStatus.LastMapPosition;
                if (address.IsCharger())
                {
                    if (Vehicle.IsCharging)
                    {
                        return;
                    }
                    else
                    {
                        string msg = $"Addr = {address.Id},and no transfer command,Charge Direction = {address.PioDirection},Precentage = {percentage} < {Vehicle.MainFlowConfig.LowPowerPercentage}(Low Threshold), SEND chsrge command";
                        OnMessageShowEvent?.Invoke(this, msg);
                    }

                    agvcConnector.ChargHandshaking();

                    Vehicle.IsCharging = true;
                    agvcConnector.Charging();

                    OnMessageShowEvent?.Invoke(this, $@"Start Charge, Vehicle arrival {address.Id},Charge Direction = {address.ChargeDirection},Precentage = {percentage}.");

                    if (Vehicle.MainFlowConfig.IsSimulation) return;

                    Vehicle.CheckStartChargeReplyEnd = false;
                    asePackage.StartCharge(address.ChargeDirection);

                    SpinWait.SpinUntil(() => Vehicle.CheckStartChargeReplyEnd, 30 * 1000);

                    if (Vehicle.CheckStartChargeReplyEnd)
                    {
                        OnMessageShowEvent?.Invoke(this, "Start Charge success.");
                        batteryLog.ChargeCount++;
                    }
                    else
                    {
                        Vehicle.IsCharging = false;
                        SetAlarmFromAgvm(000013);
                        asePackage.ChargeStatusRequest();
                        SpinWait.SpinUntil(() => false, 500);
                        asePackage.StopCharge();
                    }

                    Vehicle.CheckStartChargeReplyEnd = true;
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
                OnMessageShowEvent?.Invoke(this, $@"MainFlow :  Try STOP charge.[IsCharging = {Vehicle.IsCharging}]");

                AseMoveStatus moveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                var address = moveStatus.LastAddress;
                var pos = moveStatus.LastMapPosition;
                if (address.IsCharger())
                {
                    agvcConnector.ChargHandshaking();

                    if (Vehicle.MainFlowConfig.IsSimulation)
                    {
                        Vehicle.IsCharging = false;
                        return;
                    }

                    //in starting charge
                    if (!Vehicle.CheckStartChargeReplyEnd) Thread.Sleep(Vehicle.MainFlowConfig.StopChargeWaitingTimeoutMs);

                    asePackage.StopCharge();

                    SpinWait.SpinUntil(() => !Vehicle.IsCharging, Vehicle.MainFlowConfig.StopChargeWaitingTimeoutMs);

                    asePackage.ChargeStatusRequest();
                    SpinWait.SpinUntil(() => false, 500);

                    if (!Vehicle.IsCharging)
                    {
                        agvcConnector.ChargeOff();
                        OnMessageShowEvent?.Invoke(this, $"Stop Charge success.");
                    }
                    else
                    {
                        SetAlarmFromAgvm(000014);
                        AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Thd Track Position

        private void TrackPosition()
        {
            while (true)
            {
                try
                {
                    if (IsTrackPositionPause)
                    {
                        SpinWait.SpinUntil(() => !IsTrackPositionPause, Vehicle.MainFlowConfig.TrackPositionSleepTimeMs);
                        continue;
                    }

                    if (!Vehicle.MainFlowConfig.IsSimulation)
                    {
                        if (asePackage.ReceivePositionArgsQueue.Any())
                        {
                            asePackage.ReceivePositionArgsQueue.TryDequeue(out AsePositionArgs positionArgs);
                            DealAsePositionArgs(positionArgs);
                        }
                    }
                    else
                    {
                        //if (FakeReserveOkAseMoveStatus.Any())
                        //{
                        //    if (Vehicle.AseMovingGuide.ReserveStop == VhStopSingle.On)
                        //    {
                        //        Vehicle.AseMovingGuide.ReserveStop = VhStopSingle.Off;
                        //        agvcConnector.StatusChangeReport();
                        //    }
                        //    FakeMoveToReserveOkPositions();
                        //}                      

                        if (asePackage.ReceivePositionArgsQueue.Any())
                        {
                            asePackage.ReceivePositionArgsQueue.TryDequeue(out AsePositionArgs positionArgs);
                            DealAsePositionArgs(positionArgs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.TrackPositionSleepTimeMs);
                }
            }
        }

        private void DealAsePositionArgs(AsePositionArgs positionArgs)
        {
            try
            {
                AseMovingGuide movingGuide = new AseMovingGuide(Vehicle.AseMovingGuide);
                AseMoveStatus moveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                moveStatus.LastMapPosition = positionArgs.MapPosition;

                if (movingGuide.GuideSectionIds.Any())
                {
                    if (positionArgs.Arrival == EnumAseArrival.EndArrival)
                    {
                        moveStatus.NearlyAddress = Vehicle.Mapinfo.addressMap[movingGuide.ToAddressId];
                        moveStatus.NearlySection = movingGuide.MovingSections.Last();
                        moveStatus.NearlySection.VehicleDistanceSinceHead = moveStatus.NearlySection.HeadAddress.MyDistance(moveStatus.NearlyAddress.Position);
                        OnMessageShowEvent?.Invoke(this, $"Update Position. [LastSection = {moveStatus.LastSection.Id}][LastAddress = {moveStatus.LastAddress.Id}] to [NearlySection = {moveStatus.NearlySection.Id}][NearlyAddress = {moveStatus.NearlyAddress.Id}]");
                    }
                    else
                    {
                        if (LastIdlePosition.Position.MyDistance(positionArgs.MapPosition) <= Vehicle.MainFlowConfig.IdleReportRangeMm)
                        {
                            if ((DateTime.Now - LastIdlePosition.TimeStamp).TotalMilliseconds >= Vehicle.MainFlowConfig.IdleReportIntervalMs)
                            {
                                UpdateLastIdlePositionAndTimeStamp(positionArgs);
                                SetAlarmFromAgvm(55);
                            }

                            return;
                        }
                        else
                        {
                            UpdateLastIdlePositionAndTimeStamp(positionArgs);

                            var nearlyDistance = 999999;
                            foreach (string addressId in movingGuide.GuideAddressIds)
                            {
                                MapAddress mapAddress = Vehicle.Mapinfo.addressMap[addressId];
                                var dis = moveStatus.LastMapPosition.MyDistance(mapAddress.Position);

                                if (dis < nearlyDistance)
                                {
                                    nearlyDistance = dis;
                                    moveStatus.NearlyAddress = mapAddress;
                                }
                            }

                            foreach (string sectionId in movingGuide.GuideSectionIds)
                            {
                                MapSection mapSection = Vehicle.Mapinfo.sectionMap[sectionId];
                                if (mapSection.InSection(moveStatus.LastAddress.Id))
                                {
                                    moveStatus.NearlySection = mapSection;
                                }
                            }
                            moveStatus.NearlySection.VehicleDistanceSinceHead = moveStatus.NearlyAddress.MyDistance(moveStatus.NearlySection.HeadAddress.Position);

                            if (moveStatus.NearlyAddress.Id != moveStatus.LastAddress.Id)
                            {
                                OnMessageShowEvent?.Invoke(this, $"Update Position. [LastSection = {moveStatus.LastSection.Id}][LastAddress = {moveStatus.LastAddress.Id}] to [NearlySection = {moveStatus.NearlySection.Id}][NearlyAddress = {moveStatus.NearlyAddress.Id}]");
                            }
                        }
                    }

                    moveStatus.LastAddress = moveStatus.NearlyAddress;
                    moveStatus.LastSection = moveStatus.NearlySection;
                    moveStatus.HeadDirection = positionArgs.HeadAngle;
                    moveStatus.MovingDirection = positionArgs.MovingDirection;
                    moveStatus.Speed = positionArgs.Speed;
                    Vehicle.AseMoveStatus = moveStatus;
                    agvcConnector.ReportSectionPass();

                    UpdateAgvcConnectorGotReserveOkSections(moveStatus.LastSection.Id);

                    for (int i = 0; i < movingGuide.MovingSections.Count; i++)
                    {
                        if (movingGuide.MovingSections[i].Id == moveStatus.LastSection.Id)
                        {
                            Vehicle.AseMovingGuide.MovingSectionsIndex = i;
                        }
                    }

                    switch (positionArgs.Arrival)
                    {
                        case EnumAseArrival.Fail:
                            AseMoveControl_OnMoveFinished(new object(), EnumMoveComplete.Fail);
                            break;
                        case EnumAseArrival.Arrival:
                            break;
                        case EnumAseArrival.EndArrival:
                            AseMoveControl_OnMoveFinished(new object(), EnumMoveComplete.Success);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    moveStatus.NearlyAddress = Vehicle.Mapinfo.addressMap.Values.ToList().OrderBy(address => address.MyDistance(positionArgs.MapPosition)).First();
                    moveStatus.NearlySection = Vehicle.Mapinfo.sectionMap.Values.ToList().FirstOrDefault(section => section.InSection(moveStatus.NearlyAddress));
                    moveStatus.NearlySection.VehicleDistanceSinceHead = moveStatus.NearlySection.HeadAddress.MyDistance(positionArgs.MapPosition);
                    moveStatus.LastMapPosition = positionArgs.MapPosition;
                    if (moveStatus.NearlyAddress.Id != moveStatus.LastAddress.Id)
                    {
                        OnMessageShowEvent?.Invoke(this, $"Update Position. [LastSection = {moveStatus.LastSection.Id}][LastAddress = {moveStatus.LastAddress.Id}] to [NearlySection = {moveStatus.NearlySection.Id}][NearlyAddress = {moveStatus.NearlyAddress.Id}]");
                    }
                    moveStatus.LastAddress = moveStatus.NearlyAddress;
                    moveStatus.LastSection = moveStatus.NearlySection;
                    moveStatus.HeadDirection = positionArgs.HeadAngle;
                    moveStatus.MovingDirection = positionArgs.MovingDirection;
                    moveStatus.Speed = positionArgs.Speed;
                    Vehicle.AseMoveStatus = moveStatus;
                    agvcConnector.ReportSectionPass();

                    UpdateAgvcConnectorGotReserveOkSections(moveStatus.LastSection.Id);

                    for (int i = 0; i < movingGuide.MovingSections.Count; i++)
                    {
                        if (movingGuide.MovingSections[i].Id == moveStatus.LastSection.Id)
                        {
                            Vehicle.AseMovingGuide.MovingSectionsIndex = i;
                        }
                    }

                    switch (positionArgs.Arrival)
                    {
                        case EnumAseArrival.Fail:
                            AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Fail);
                            break;
                        case EnumAseArrival.Arrival:
                            break;
                        case EnumAseArrival.EndArrival:
                            AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateLastIdlePositionAndTimeStamp(AsePositionArgs positionArgs)
        {
            LastIdlePosition lastIdlePosition = new LastIdlePosition();
            lastIdlePosition.Position = positionArgs.MapPosition;
            lastIdlePosition.TimeStamp = DateTime.Now;
            LastIdlePosition = lastIdlePosition;
        }

        public void StartTrackPosition()
        {
            thdTrackPosition = new Thread(TrackPosition);
            thdTrackPosition.IsBackground = true;
            thdTrackPosition.Start();
        }

        private void FakeMoveToReserveOkPositions()
        {
            Vehicle.AseMoveStatus.AseMoveState = EnumAseMoveState.Working;
            SpinWait.SpinUntil(() => false, 2000);
            FakeReserveOkAseMoveStatus.TryDequeue(out AseMoveStatus targetAseMoveStatus);
            AseMoveStatus tempMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);

            if (targetAseMoveStatus.LastSection.Id != tempMoveStatus.LastSection.Id)
            {
                tempMoveStatus.LastSection = targetAseMoveStatus.LastSection;
                GetFakeSectionDistance(tempMoveStatus);
                Vehicle.AseMoveStatus = tempMoveStatus;
                agvcConnector.ReportSectionPass();

                SpinWait.SpinUntil(() => false, 1000);

                tempMoveStatus.LastMapPosition = targetAseMoveStatus.LastMapPosition;
                tempMoveStatus.Speed = targetAseMoveStatus.Speed;
                tempMoveStatus.HeadDirection = targetAseMoveStatus.HeadDirection;
                tempMoveStatus.LastAddress = targetAseMoveStatus.LastAddress;
                GetFakeSectionDistance(tempMoveStatus);
                Vehicle.AseMoveStatus = tempMoveStatus;
                agvcConnector.ReportSectionPass();

                Vehicle.AseMovingGuide.MovingSectionsIndex++;
                UpdateAgvcConnectorGotReserveOkSections(targetAseMoveStatus.LastSection.Id);
                SpinWait.SpinUntil(() => false, 1000);
            }

            if (targetAseMoveStatus.LastAddress.Id != Vehicle.AseMoveStatus.LastAddress.Id)
            {
                SpinWait.SpinUntil(() => false, 1000);

                tempMoveStatus.LastMapPosition = targetAseMoveStatus.LastMapPosition;
                tempMoveStatus.Speed = targetAseMoveStatus.Speed;
                tempMoveStatus.HeadDirection = targetAseMoveStatus.HeadDirection;
                tempMoveStatus.LastAddress = targetAseMoveStatus.LastAddress;
                GetFakeSectionDistance(tempMoveStatus);
                Vehicle.AseMoveStatus = tempMoveStatus;
                agvcConnector.ReportSectionPass();
            }

            if (FakeReserveOkAseMoveStatus.IsEmpty)
            {
                Vehicle.AseMoveStatus.AseMoveState = EnumAseMoveState.Idle;
            }

            if (targetAseMoveStatus.IsMoveEnd)
            {
                AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
                Vehicle.AseMoveStatus.IsMoveEnd = true;
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

        #endregion

        public bool CanVehMove()
        {
            if (Vehicle.IsCharging)//dabid
            {
                StopCharge();
            }
            return Vehicle.AseRobotStatus.IsHome && !Vehicle.IsCharging;
        }

        public void AgvcConnector_GetReserveOkUpdateMoveControlNextPartMovePosition(MapSection mapSection, EnumIsExecute keepOrGo)
        {
            try
            {
                int sectionIndex = Vehicle.AseMovingGuide.GuideSectionIds.FindIndex(x => x == mapSection.Id);
                MapAddress address = Vehicle.Mapinfo.addressMap[Vehicle.AseMovingGuide.GuideAddressIds[sectionIndex + 1]];

                bool isEnd = false;
                EnumAddressDirection addressDirection = EnumAddressDirection.None;
                if (address.Id == Vehicle.AseMovingGuide.ToAddressId)
                {
                    addressDirection = address.TransferPortDirection;
                    isEnd = true;
                }
                int headAngle = (int)address.VehicleHeadAngle;
                int speed = (int)mapSection.Speed;

                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
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
                    if (isEnd)
                    {
                        var cmdId = GetCurTransferStep().CmdId;
                        if (Vehicle.AgvcTransCmdBuffer.ContainsKey(cmdId))
                        {
                            var transferCommand = Vehicle.AgvcTransCmdBuffer[cmdId];
                            //TODO : check if need open both slot.
                            switch (transferCommand.EnrouteState)
                            {
                                case CommandState.None:
                                    asePackage.PartMove(address.Position, headAngle, speed, EnumAseMoveCommandIsEnd.End, keepOrGo, EnumSlotSelect.None);
                                    break;
                                case CommandState.LoadEnroute:
                                case CommandState.UnloadEnroute:
                                    if (transferCommand.SlotNumber == EnumSlotNumber.L)
                                    {
                                        asePackage.PartMove(address.Position, headAngle, speed, EnumAseMoveCommandIsEnd.End, keepOrGo, EnumSlotSelect.Left);
                                    }
                                    else
                                    {
                                        asePackage.PartMove(address.Position, headAngle, speed, EnumAseMoveCommandIsEnd.End, keepOrGo, EnumSlotSelect.Right);
                                    }
                                    break;
                                default:
                                    asePackage.PartMove(address.Position, headAngle, speed, EnumAseMoveCommandIsEnd.End, keepOrGo, EnumSlotSelect.None);
                                    break;
                            }
                        }
                        else
                        {
                            asePackage.PartMove(address.Position, headAngle, speed, EnumAseMoveCommandIsEnd.End, keepOrGo, EnumSlotSelect.None);
                        }
                    }
                    else
                    {
                        asePackage.PartMove(address.Position, headAngle, speed, EnumAseMoveCommandIsEnd.None, keepOrGo, EnumSlotSelect.None);
                    }
                }

                OnMessageShowEvent?.Invoke(this, $"Send to MoveControl get reserve {mapSection.Id} ok , next end point [{address.Id}]({Convert.ToInt32(address.Position.X)},{Convert.ToInt32(address.Position.Y)}).");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public bool IsMoveStep() => GetCurrentTransferStepType() == EnumTransferStepType.Move || GetCurrentTransferStepType() == EnumTransferStepType.MoveToCharger;

        public void AseMoveControl_OnMoveFinished(object sender, EnumMoveComplete status)
        {
            try
            {
                Vehicle.AseMoveStatus.IsMoveEnd = true;
                #region Not EnumMoveComplete.Success
                if (status == EnumMoveComplete.Fail)
                {
                    SetAlarmFromAgvm(6);
                    agvcConnector.ClearAllReserve();
                    Vehicle.AseMovingGuide = new AseMovingGuide();
                    if (IsAvoidMove)
                    {
                        agvcConnector.AvoidFail();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Avoid Fail. ");
                        IsAvoidMove = false;
                    }
                    else if (IsOverrideMove)
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow :  Override Move Fail. ");
                        IsOverrideMove = false;
                    }
                    else
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Fail. ");
                    }
                    AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
                    return;
                }

                if (status == EnumMoveComplete.Pause)
                {
                    //VisitTransferStepsStatus = EnumThreadStatus.PauseComplete;
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Pause.  checked ");
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
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Avoid Move Cancel checked ");
                        return;
                    }
                    else
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Cancel checked ");
                        return;
                    }
                }
                #endregion

                #region EnumMoveComplete.Success
                Vehicle.IsReAuto = false;

                agvcConnector.ClearAllReserve();


                if (IsAvoidMove)
                {
                    Vehicle.AseMovingGuide = new AseMovingGuide();
                    Vehicle.AseMovingGuide.IsAvoidComplete = true;
                    agvcConnector.AvoidComplete();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Avoid Move End Ok.");
                }
                else
                {

                    if (IsMoveStep())
                    {
                        MoveCmdInfo moveCmdInfo = (MoveCmdInfo)GetCurTransferStep();
                        ArrivalStartCharge(moveCmdInfo.EndAddress);
                        Vehicle.AseMovingGuide = new AseMovingGuide();

                        if (IsNextTransferStepIdle())
                        {
                            TransferComplete(moveCmdInfo.CmdId);
                            OnMessageShowEvent?.Invoke(this, $"MainFlow : Move End Ok.");
                        }
                        else
                        {
                            VisitNextTransferStep();
                        }
                    }
                    else
                    {
                        ArrivalStartCharge(Vehicle.AseMoveStatus.LastAddress);
                        VisitNextTransferStep();
                    }
                }

                IsAvoidMove = false;
                IsOverrideMove = false;

                #endregion
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void TransferComplete(string cmdId)
        {
            try
            {
                IsVisitTransferStepPause = true;
                AgvcTransCmd agvcTransCmd = Vehicle.AgvcTransCmdBuffer[cmdId];
                agvcTransCmd.EnrouteState = CommandState.None;
                ClearTransferSteps(cmdId);
                ConcurrentDictionary<string, AgvcTransCmd> tempTransCmdBuffer = new ConcurrentDictionary<string, AgvcTransCmd>();
                foreach (var transCmd in Vehicle.AgvcTransCmdBuffer.Values.ToList())
                {
                    if (transCmd.CommandId != cmdId)
                    {
                        tempTransCmdBuffer.TryAdd(transCmd.CommandId, transCmd);
                    }
                }
                Vehicle.AgvcTransCmdBuffer = tempTransCmdBuffer;
                ReportAgvcTransferComplete(agvcTransCmd);
                OptimizeTransferStepsAfterTransferComplete();
                if (Vehicle.AgvcTransCmdBuffer.Count == 0)
                {
                    agvcConnector.NoCommand();
                }
                asePackage.SetTransferCommandInfoRequest(agvcTransCmd, EnumCommandInfoStep.End);
                GoNextTransferStep = true;
                IsVisitTransferStepPause = false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ArrivalStartCharge(MapAddress endAddress)
        {
            try
            {
                try
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(50);
                        StartCharge(endAddress);
                    });

                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

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

        private void ReportAgvcTransferComplete(AgvcTransCmd agvcTransCmd)
        {
            agvcConnector.TransferComplete(agvcTransCmd);
        }

        private void GetPioDirection(RobotCommand robotCommand)
        {
            try
            {
                MapAddress portAddress = Vehicle.Mapinfo.addressMap[robotCommand.PortAddressId];
                robotCommand.PioDirection = portAddress.PioDirection;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #region Load Unload

        public void Load(LoadCmdInfo loadCmd)
        {
            try
            {
                GetPioDirection(loadCmd);

                AseCarrierSlotStatus aseCarrierSlotStatus = Vehicle.GetAseCarrierSlotStatus(loadCmd.SlotNumber);

                OnMessageShowEvent?.Invoke(this, $"PreLoadSlotCheck, [slotNum={aseCarrierSlotStatus.SlotNumber}][slotState={aseCarrierSlotStatus.CarrierSlotStatus}][loadCmdId={loadCmd.CmdId}]");

                if (aseCarrierSlotStatus.CarrierSlotStatus != EnumAseCarrierSlotStatus.Empty)
                {
                    SetAlarmFromAgvm(000016);
                    return;
                }

                ReportLoadArrival(loadCmd);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReportLoadArrival(LoadCmdInfo loadCmdInfo)
        {
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Load Arrival, [Port Adr = {loadCmdInfo.PortAddressId}][PioDirection = {loadCmdInfo.PioDirection}][Port Num = {loadCmdInfo.PortNumber}][GateType = {loadCmdInfo.GateType}]");
            agvcConnector.ReportLoadArrival(loadCmdInfo.CmdId);
        }

        private void AgvcConnector_OnAgvcAcceptLoadArrivalEvent(object sender, EventArgs e)
        {
            var loadCmd = (LoadCmdInfo)GetCurTransferStep();

            AseCarrierSlotStatus aseCarrierSlotStatus = Vehicle.GetAseCarrierSlotStatus(loadCmd.SlotNumber);

            try
            {
                agvcConnector.Loading(loadCmd.CmdId, loadCmd.SlotNumber);
                if (loadCmd.SlotNumber == EnumSlotNumber.L)
                {
                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                }
                else
                {
                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                }

                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    SimulationLoad(loadCmd, aseCarrierSlotStatus);
                }
                else
                {
                    Task.Run(() => asePackage.DoRobotCommand(loadCmd));
                    OnMessageShowEvent?.Invoke(this, $"Loading, [Direction={loadCmd.PioDirection}][SlotNum={loadCmd.SlotNumber}][Load Adr={loadCmd.PortAddressId}][Load Port Num={loadCmd.PortNumber}]");
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        private void SimulationLoad(LoadCmdInfo loadCmd, AseCarrierSlotStatus aseCarrierSlotStatus)
        {
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Loading, [Direction={loadCmd.PioDirection}][SlotNum={loadCmd.SlotNumber}][PortNum={loadCmd.PortNumber}]");
            SpinWait.SpinUntil(() => false, 3000);
            aseCarrierSlotStatus.CarrierId = loadCmd.CassetteId;
            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
            if (loadCmd.SlotNumber == EnumSlotNumber.L)
            {
                Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
            }
            else
            {
                Vehicle.RightReadResult = BCRReadResult.BcrNormal;
            }
            //AsePackage_OnReadCarrierIdFinishEvent(this, aseCarrierSlotStatus.SlotNumber);
            SpinWait.SpinUntil(() => false, 2000);
            AsePackage_OnRobotCommandFinishEvent(this, (RobotCommand)GetCurTransferStep());
        }

        public void Unload(UnloadCmdInfo unloadCmd)
        {
            //OptimizeTransferSteps();

            GetPioDirection(unloadCmd);

            AseCarrierSlotStatus aseCarrierSlotStatus = Vehicle.GetAseCarrierSlotStatus(unloadCmd.SlotNumber);

            if (aseCarrierSlotStatus.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty)
            {
                SetAlarmFromAgvm(000017);
                return;
            }

            ReportUnloadArrival(unloadCmd);
        }

        private void AgvcConnector_OnAgvcAcceptUnloadArrivalEvent(object sender, EventArgs e)
        {
            try
            {
                UnloadCmdInfo unloadCmd = (UnloadCmdInfo)GetCurTransferStep();
                AseCarrierSlotStatus aseCarrierSlotStatus = Vehicle.GetAseCarrierSlotStatus(unloadCmd.SlotNumber);

                agvcConnector.Unloading(unloadCmd.CmdId, unloadCmd.SlotNumber);

                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    SimulationUnload(unloadCmd, aseCarrierSlotStatus);
                }
                else
                {
                    Task.Run(() => asePackage.DoRobotCommand(unloadCmd));
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Unloading, [Direction{unloadCmd.PioDirection}][SlotNum={unloadCmd.SlotNumber}][Unload Adr={unloadCmd.PortAddressId}][Unload Port Num={unloadCmd.PortNumber}]");
                }
                batteryLog.LoadUnloadCount++;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        private void ReportUnloadArrival(UnloadCmdInfo unloadCmd)
        {
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Unload Arrival, [Port Adr = {unloadCmd.PortAddressId}][PioDirection = {unloadCmd.PioDirection}][PortNumber = {unloadCmd.PortNumber}]GateType = {unloadCmd.GateType}]");
            agvcConnector.ReportUnloadArrival(unloadCmd.CmdId);
        }

        private void SimulationUnload(UnloadCmdInfo unloadCmd, AseCarrierSlotStatus aseCarrierSlotStatus)
        {
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Unloading, [Direction{unloadCmd.PioDirection}][SlotNum={unloadCmd.SlotNumber}][PortNum={unloadCmd.PortNumber}]");
            SpinWait.SpinUntil(() => false, 3000);
            aseCarrierSlotStatus.CarrierId = "";
            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
            SpinWait.SpinUntil(() => false, 2000);
            AsePackage_OnRobotCommandFinishEvent(this, (RobotCommand)GetCurTransferStep());
        }

        private void AsePackage_OnRobotCommandErrorEvent(object sender, RobotCommand robotCommand)
        {
            OnMessageShowEvent?.Invoke(this, "AseRobotControl_OnRobotCommandErrorEvent");
            AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
        }

        public void AsePackage_OnRobotCommandFinishEvent(object sender, RobotCommand robotCommand)
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, "AseRobotContorl_OnRobotCommandFinishEvent");
                EnumTransferStepType transferStepType = robotCommand.GetTransferStepType();
                if (transferStepType == EnumTransferStepType.Load)
                {
                    agvcConnector.ReadResult = ReadResult;
                    ReportAgvcLoadComplete(robotCommand.CmdId);
                }
                else if (transferStepType == EnumTransferStepType.Unload)
                {
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Unload Complete");

                    var slotNumber = robotCommand.SlotNumber;
                    AseCarrierSlotStatus aseCarrierSlotStatus = Vehicle.GetAseCarrierSlotStatus(slotNumber);
                    switch (aseCarrierSlotStatus.CarrierSlotStatus)
                    {
                        case EnumAseCarrierSlotStatus.Empty:
                            ReportAgvcUnloadComplete(robotCommand.CmdId);
                            break;
                        case EnumAseCarrierSlotStatus.Loading:
                        case EnumAseCarrierSlotStatus.ReadFail:
                            if (Vehicle.AgvcTransCmdBuffer.ContainsKey(robotCommand.CmdId))
                            {
                                Vehicle.AgvcTransCmdBuffer[robotCommand.CmdId].CompleteStatus = CompleteStatus.VehicleAbort;
                                TransferComplete(robotCommand.CmdId);
                            }
                            else
                            {
                                OnMessageShowEvent?.Invoke(this, $"AseRobotContorl_OnRobotCommandFinishEvent, can not find command [ID = {robotCommand.CmdId}]. Reset all.");
                                StopClearAndReset();
                            }
                            break;
                        case EnumAseCarrierSlotStatus.PositionError:
                            SetAlarmFromAgvm(51);
                            AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);

                            break;
                    }
                }
                else
                {
                    OnMessageShowEvent?.Invoke(this, $"[{transferStepType}]");
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AgvcConnector_OnAgvcAcceptLoadCompleteEvent(object sender, EventArgs e)
        {
            try
            {
                RobotCommand robotCommand = (RobotCommand)GetCurTransferStep();
                EnumSlotNumber slotNumber = robotCommand.SlotNumber;
                AseCarrierSlotStatus slotStatus = slotNumber == EnumSlotNumber.L ? Vehicle.AseCarrierSlotL : Vehicle.AseCarrierSlotR;

                if (!Vehicle.MainFlowConfig.BcrByPass)
                {
                    switch (slotStatus.CarrierSlotStatus)
                    {
                        case EnumAseCarrierSlotStatus.Empty:
                            {
                                OnMessageShowEvent?.Invoke(this, $"After Load Complete, CST ID is empty.");

                                slotStatus.CarrierId = "";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;

                                }

                                SetAlarmFromAgvm(000051);
                            }
                            break;
                        case EnumAseCarrierSlotStatus.Loading:
                            if (robotCommand.CassetteId.Trim() == slotStatus.CarrierId.Trim())
                            {
                                OnMessageShowEvent?.Invoke(this, $"CST ID = [{slotStatus.CarrierId.Trim()}] read ok.");

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                                }
                                else
                                {
                                    Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                                }
                            }
                            else
                            {
                                OnMessageShowEvent?.Invoke(this, $"Read CST ID = [{slotStatus.CarrierId}], unmatch command CST ID = [{robotCommand.CassetteId}]");

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.LeftReadResult = BCRReadResult.BcrMisMatch;
                                }
                                else
                                {
                                    Vehicle.RightReadResult = BCRReadResult.BcrMisMatch;
                                }

                                SetAlarmFromAgvm(000028);
                            }
                            break;
                        case EnumAseCarrierSlotStatus.ReadFail:
                            {
                                slotStatus.CarrierId = "ReadFail";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }
                                SetAlarmFromAgvm(000004);
                            }
                            break;
                        case EnumAseCarrierSlotStatus.PositionError:
                            {
                                OnMessageShowEvent?.Invoke(this, $"CST Position Error.");

                                slotStatus.CarrierId = "PositionError";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.PositionError;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }

                                SetAlarmFromAgvm(000051);
                                AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);

                                return;
                            }
                        default:
                            break;
                    }
                }
                else
                {
                    switch (slotStatus.CarrierSlotStatus)
                    {
                        case EnumAseCarrierSlotStatus.Empty:
                            {
                                OnMessageShowEvent?.Invoke(this, $"Load Complete, BcrByPass, slot is empty.");

                                slotStatus.CarrierId = "";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;

                                }
                            }
                            break;
                        case EnumAseCarrierSlotStatus.ReadFail:
                        case EnumAseCarrierSlotStatus.Loading:
                            {
                                OnMessageShowEvent?.Invoke(this, $"Load Complete, BcrByPass, loading is true.");
                                slotStatus.CarrierId = robotCommand.CassetteId.Trim();
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                                }
                            }
                            break;
                        case EnumAseCarrierSlotStatus.PositionError:
                            {
                                OnMessageShowEvent?.Invoke(this, $"CST Position Error.");

                                slotStatus.CarrierId = "PositionError";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.PositionError;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }

                                SetAlarmFromAgvm(000051);
                                AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);

                                return;
                            }
                        default:
                            break;
                    }
                }

                ReportAgvcBcrRead();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReportAgvcUnloadComplete(string cmdId)
        {
            try
            {
                var robotCmdInfo = (RobotCommand)GetCurTransferStep();
                var slotNumber = robotCmdInfo.SlotNumber;
                AseCarrierSlotStatus aseCarrierSlotStatus = slotNumber == EnumSlotNumber.L ? Vehicle.AseCarrierSlotL : Vehicle.AseCarrierSlotR;


                switch (aseCarrierSlotStatus.CarrierSlotStatus)
                {
                    case EnumAseCarrierSlotStatus.Empty:
                        OnMessageShowEvent?.Invoke(this, $"Slot [{slotNumber}] unload success.");
                        break;
                    case EnumAseCarrierSlotStatus.Loading:
                    case EnumAseCarrierSlotStatus.ReadFail:
                    case EnumAseCarrierSlotStatus.PositionError:
                        SetAlarmFromAgvm(7);
                        OnMessageShowEvent?.Invoke(this, $"Slot [{slotNumber}] unload fail. [CST ID = {aseCarrierSlotStatus.CarrierId}][{aseCarrierSlotStatus.CarrierSlotStatus}]");
                        break;
                    default:
                        break;
                }

                agvcConnector.UnloadComplete(cmdId);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void ReportAgvcLoadComplete(string cmdId)
        {
            agvcConnector.LoadComplete(cmdId);
        }
        private void ReportAgvcBcrRead()
        {
            agvcConnector.SendRecv_Cmd136_CstIdReadReport();
        }

        private void AsePackage_OnRobotInterlockErrorEvent(object sender, RobotCommand robotCommand)
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, "AseRobotControl_OnRobotInterlockErrorEvent");
                ResetAllAlarmsFromAgvm();
                var curCmdId = robotCommand.CmdId;
                if (Vehicle.AgvcTransCmdBuffer.ContainsKey(curCmdId))
                {
                    Vehicle.AgvcTransCmdBuffer[curCmdId].CompleteStatus = CompleteStatus.InterlockError;
                    TransferComplete(curCmdId);
                }
                else
                {
                    OnMessageShowEvent?.Invoke(this, $"AseRobotControl_OnRobotInterlockErrorEvent, can not find command [ID = {curCmdId}]. Reset all.");
                    StopClearAndReset();
                }
            }
            catch (Exception ex)
            {
                OnMessageShowEvent?.Invoke(this, $"InterlockError Exception.");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AgvcConnector_OnSendRecvTimeoutEvent(object sender, EventArgs e)
        {
            SetAlarmFromAgvm(38);
            //AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
        }

        private void AgvcConnector_OnAgvcContinueBcrReadEvent(object sender, EventArgs e)
        {
            OptimizeTransferStepsAfterLoadEnd();
        }

        private void AgvcConnector_OnAgvcAcceptMoveArrivalEvent(object sender, EventArgs e)
        {
            OnMessageShowEvent(this, "MoveArrival");
            if (transferSteps.Count > 0)
            {
                VisitNextTransferStep();
            }
        }

        private void AgvcConnector_OnAgvcAcceptUnloadCompleteEvent(object sender, EventArgs e)
        {
            try
            {
                var transferStep = GetCurTransferStep();
                TransferComplete(transferStep.CmdId);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void OptimizeTransferStepsAfterLoadEnd()
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, $"Optimize Transfer Steps");

                Vehicle.IsOptimize = true;
                IsVisitTransferStepPause = true;

                var curCmdId = GetCurTransferStep().CmdId;
                var curCmd = Vehicle.AgvcTransCmdBuffer[curCmdId];
                var curStep = GetCurTransferStep();
                transferSteps = new List<TransferStep>();
                TransferStepsIndex = 1;
                transferSteps.Add(curStep);

                if (!Vehicle.MainFlowConfig.DualCommandOptimize)
                {
                    if (curCmd.AgvcTransCommandType == EnumAgvcTransCommandType.LoadUnload)
                    {
                        curCmd.EnrouteState = CommandState.UnloadEnroute;
                        TransferStepsAddMoveCmdInfo(curCmd.UnloadAddressId, curCmdId);
                        TransferStepsAddUnloadCmdInfo(curCmd);
                    }
                    else
                    {
                        transferSteps = new List<TransferStep>();
                        TransferStepsIndex = 0;
                        TransferComplete(curCmdId);
                    }
                }
                else
                {
                    if (Vehicle.AgvcTransCmdBuffer.Count <= 1)
                    {
                        if (curCmd.AgvcTransCommandType == EnumAgvcTransCommandType.LoadUnload)
                        {
                            curCmd.EnrouteState = CommandState.UnloadEnroute;
                            TransferStepsAddMoveCmdInfo(curCmd.UnloadAddressId, curCmdId);
                            TransferStepsAddUnloadCmdInfo(curCmd);
                        }
                        else
                        {
                            transferSteps = new List<TransferStep>();
                            TransferStepsIndex = 0;
                            TransferComplete(curCmdId);
                        }
                    }
                    else
                    {
                        if (curCmd.GetCommandActionType() == CommandActionType.Load)
                        {
                            transferSteps = new List<TransferStep>();
                            TransferStepsIndex = 0;
                            TransferComplete(curCmdId);
                        }
                        else
                        {
                            curCmd.EnrouteState = CommandState.UnloadEnroute;
                            var disCurCmdUnload = DistanceFromLastPosition(curCmd.UnloadAddressId);

                            var transferCommands = Vehicle.AgvcTransCmdBuffer.Values.ToList();
                            int nextCmdIndex = transferCommands[0].CommandId == curCmdId ? 1 : 0;
                            var nextCmd = transferCommands[nextCmdIndex];
                            var nextCmdId = nextCmd.CommandId;

                            if (nextCmd.EnrouteState == CommandState.LoadEnroute)
                            {
                                var disNextCmdLoad = DistanceFromLastPosition(nextCmd.LoadAddressId);
                                if (disCurCmdUnload <= disNextCmdLoad)
                                {
                                    TransferStepsAddMoveCmdInfo(curCmd.UnloadAddressId, curCmdId);
                                    TransferStepsAddUnloadCmdInfo(curCmd);
                                }
                                else
                                {
                                    TransferStepsAddMoveCmdInfo(nextCmd.LoadAddressId, nextCmdId);
                                    TransferStepsAddLoadCmdInfo(nextCmd);
                                }
                            }
                            else if (nextCmd.EnrouteState == CommandState.UnloadEnroute)
                            {
                                var disNextCmdUnload = DistanceFromLastPosition(nextCmd.UnloadAddressId);
                                if (disCurCmdUnload <= disNextCmdUnload)
                                {
                                    TransferStepsAddMoveCmdInfo(curCmd.UnloadAddressId, curCmdId);
                                    TransferStepsAddUnloadCmdInfo(curCmd);
                                }
                                else
                                {
                                    TransferStepsAddMoveCmdInfo(nextCmd.UnloadAddressId, nextCmdId);
                                    TransferStepsAddUnloadCmdInfo(nextCmd);
                                }
                            }
                        }
                    }
                }

                Vehicle.IsOptimize = false;

                GoNextTransferStep = true;
                IsVisitTransferStepPause = false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private int DistanceFromLastPosition(string addressId)
        {
            var lastPosition = Vehicle.AseMoveStatus.LastMapPosition;
            var addressPosition = Vehicle.Mapinfo.addressMap[addressId].Position;
            return (int)mapHandler.GetDistance(lastPosition, addressPosition);
        }

        private void OptimizeTransferStepsAfterTransferComplete()
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, $"OptimizeTransferStepsAfterTransferComplete");
                Vehicle.IsOptimize = true;
                transferSteps = new List<TransferStep>();
                TransferStepsIndex = 0;

                if (Vehicle.AgvcTransCmdBuffer.Count > 1)
                {
                    if (!Vehicle.MainFlowConfig.DualCommandOptimize)
                    {
                        List<AgvcTransCmd> agvcTransCmds = Vehicle.AgvcTransCmdBuffer.Values.ToList();
                        AgvcTransCmd onlyCmd = agvcTransCmds[0];

                        switch (onlyCmd.EnrouteState)
                        {
                            case CommandState.None:
                                TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                                transferSteps.Add(new EmptyTransferStep());
                                break;
                            case CommandState.LoadEnroute:
                                TransferStepsAddMoveCmdInfo(onlyCmd.LoadAddressId, onlyCmd.CommandId);
                                TransferStepsAddLoadCmdInfo(onlyCmd);
                                break;
                            case CommandState.UnloadEnroute:
                                TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                                TransferStepsAddUnloadCmdInfo(onlyCmd);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        List<AgvcTransCmd> agvcTransCmds = Vehicle.AgvcTransCmdBuffer.Values.ToList();
                        AgvcTransCmd cmd001 = agvcTransCmds[0];

                        switch (cmd001.EnrouteState)
                        {
                            case CommandState.None:
                                {
                                    TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                    transferSteps.Add(new EmptyTransferStep());
                                }
                                break;
                            case CommandState.LoadEnroute:
                                {
                                    var disCmd001Load = DistanceFromLastPosition(cmd001.LoadAddressId);

                                    AgvcTransCmd cmd002 = agvcTransCmds[1];
                                    if (cmd002.EnrouteState == CommandState.LoadEnroute)
                                    {
                                        var disCmd002Load = DistanceFromLastPosition(cmd002.LoadAddressId);
                                        if (disCmd001Load <= disCmd002Load)
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd001.LoadAddressId, cmd001.CommandId);
                                            TransferStepsAddLoadCmdInfo(cmd001);
                                        }
                                        else
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd002.LoadAddressId, cmd002.CommandId);
                                            TransferStepsAddLoadCmdInfo(cmd002);
                                        }
                                    }
                                    else if (cmd002.EnrouteState == CommandState.UnloadEnroute)
                                    {
                                        var disCmd002Unload = DistanceFromLastPosition(cmd002.UnloadAddressId);
                                        if (disCmd001Load <= disCmd002Unload)
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd001.LoadAddressId, cmd001.CommandId);
                                            TransferStepsAddLoadCmdInfo(cmd001);
                                        }
                                        else
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd002.UnloadAddressId, cmd002.CommandId);
                                            TransferStepsAddUnloadCmdInfo(cmd002);
                                        }
                                    }
                                    else
                                    {
                                        StopClearAndReset();
                                        SetAlarmFromAgvm(1);
                                    }
                                }
                                break;
                            case CommandState.UnloadEnroute:
                                {
                                    var disCmd001Unload = DistanceFromLastPosition(cmd001.UnloadAddressId);

                                    AgvcTransCmd cmd002 = agvcTransCmds[1];
                                    if (cmd002.EnrouteState == CommandState.LoadEnroute)
                                    {
                                        var disCmd002Load = DistanceFromLastPosition(cmd002.LoadAddressId);
                                        if (disCmd001Unload <= disCmd002Load)
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                            TransferStepsAddUnloadCmdInfo(cmd001);
                                        }
                                        else
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd002.LoadAddressId, cmd002.CommandId);
                                            TransferStepsAddLoadCmdInfo(cmd002);
                                        }
                                    }
                                    else if (cmd002.EnrouteState == CommandState.UnloadEnroute)
                                    {
                                        var disCmd002Unload = DistanceFromLastPosition(cmd002.UnloadAddressId);
                                        if (disCmd001Unload <= disCmd002Unload)
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                            TransferStepsAddUnloadCmdInfo(cmd001);
                                        }
                                        else
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd002.UnloadAddressId, cmd002.CommandId);
                                            TransferStepsAddUnloadCmdInfo(cmd002);
                                        }
                                    }
                                    else
                                    {
                                        StopClearAndReset();
                                        SetAlarmFromAgvm(1);
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                else if (Vehicle.AgvcTransCmdBuffer.Count == 1)
                {
                    List<AgvcTransCmd> agvcTransCmds = Vehicle.AgvcTransCmdBuffer.Values.ToList();
                    AgvcTransCmd onlyCmd = agvcTransCmds[0];

                    switch (onlyCmd.EnrouteState)
                    {
                        case CommandState.None:
                            TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                            transferSteps.Add(new EmptyTransferStep());
                            break;
                        case CommandState.LoadEnroute:
                            TransferStepsAddMoveCmdInfo(onlyCmd.LoadAddressId, onlyCmd.CommandId);
                            TransferStepsAddLoadCmdInfo(onlyCmd);
                            break;
                        case CommandState.UnloadEnroute:
                            TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                            TransferStepsAddUnloadCmdInfo(onlyCmd);
                            break;
                        default:
                            break;
                    }
                }

                Vehicle.IsOptimize = false;

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AgvcConnector_OnCstRenameEvent(object sender, EnumSlotNumber slotNumber)
        {
            try
            {
                AseCarrierSlotStatus slotStatus = slotNumber == EnumSlotNumber.L ? Vehicle.AseCarrierSlotL : Vehicle.AseCarrierSlotR;
                asePackage.CstRename(slotStatus);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }


        #endregion

        #region Simple Getters
        public AlarmHandler GetAlarmHandler() => alarmHandler;
        public AgvcConnector GetAgvcConnector() => agvcConnector;
        public AlarmConfig GetAlarmConfig() => alarmConfig;
        public AsePackage GetAsePackage() => asePackage;
        #endregion

        public void SetupAseMovingGuideMovingSections()
        {
            try
            {
                //StopCharge();
                AseMovingGuide aseMovingGuide = new AseMovingGuide(Vehicle.AseMovingGuide);
                aseMovingGuide.MovingSections.Clear();
                for (int i = 0; i < Vehicle.AseMovingGuide.GuideSectionIds.Count; i++)
                {
                    MapSection mapSection = new MapSection();
                    string sectionId = aseMovingGuide.GuideSectionIds[i].Trim();
                    string addressId = aseMovingGuide.GuideAddressIds[i + 1].Trim();
                    if (!Vehicle.Mapinfo.sectionMap.ContainsKey(sectionId))
                    {
                        throw new Exception($"Map info has no this section ID.[{sectionId}]");
                    }
                    else if (!Vehicle.Mapinfo.addressMap.ContainsKey(addressId))
                    {
                        throw new Exception($"Map info has no this address ID.[{addressId}]");
                    }

                    mapSection = Vehicle.Mapinfo.sectionMap[sectionId];
                    mapSection.CmdDirection = addressId == mapSection.TailAddress.Id ? EnumCommandDirection.Forward : EnumCommandDirection.Backward;
                    aseMovingGuide.MovingSections.Add(mapSection);
                }
                Vehicle.AseMovingGuide = aseMovingGuide;
            }
            catch (Exception ex)
            {
                OnMessageShowEvent?.Invoke(this, ex.Message);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                Vehicle.AseMovingGuide.MovingSections = new List<MapSection>();
                SetAlarmFromAgvm(18);
                StopClearAndReset();
            }
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        private void AgvcConnector_OnStopClearAndResetEvent(object sender, EventArgs e)
        {
            StopClearAndReset();
        }

        public void StopClearAndReset()
        {
            try
            {
                PauseTransfer();
                agvcConnector.ClearAllReserve();
                Vehicle.AseMovingGuide = new AseMovingGuide();
                StopVehicle();
                ReadResult = EnumCstIdReadResult.Fail;//dabid

                var transferCommands = Vehicle.AgvcTransCmdBuffer.Values.ToList();
                foreach (var transCmd in transferCommands)
                {
                    Vehicle.AgvcTransCmdBuffer[transCmd.CommandId].CompleteStatus = GetStopAndClearCompleteStatus(transCmd.CompleteStatus);
                    TransferComplete(transCmd.CommandId);
                }

                if (Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading || Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
                {
                    asePackage.ReadCarrierId();
                }

                if (Vehicle.AseMovingGuide.PauseStatus == VhStopSingle.On)
                {
                    Vehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                    agvcConnector.StatusChangeReport();
                }

                var msg = $"MainFlow : Stop And Clear";
                OnMessageShowEvent?.Invoke(this, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private CompleteStatus GetStopAndClearCompleteStatus(CompleteStatus completeStatus)
        {
            switch (completeStatus)
            {
                case CompleteStatus.Move:
                case CompleteStatus.Load:
                case CompleteStatus.Unload:
                case CompleteStatus.Loadunload:
                case CompleteStatus.MoveToCharger:
                    return CompleteStatus.VehicleAbort;
                case CompleteStatus.Cancel:
                case CompleteStatus.Abort:
                case CompleteStatus.VehicleAbort:
                case CompleteStatus.IdmisMatch:
                case CompleteStatus.IdreadFailed:
                case CompleteStatus.InterlockError:
                case CompleteStatus.LongTimeInaction:
                case CompleteStatus.ForceFinishByOp:
                default:
                    return completeStatus;
            }
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return EnumTransferStepType.Empty;
            }
        }

        public int GetTransferStepsCount()
        {
            return transferSteps.Count;
        }

        public void StopVehicle()
        {
            asePackage.MoveStop();
            asePackage.ClearRobotCommand();
            asePackage.StopCharge();

            var msg = $"MainFlow : Stop Vehicle, [MoveState={Vehicle.AseMoveStatus.AseMoveState}][IsCharging={Vehicle.IsCharging}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void SetupVehicleSoc(int percentage)
        {
            asePackage.SetPercentage(percentage);
        }

        private void AgvcConnector_OnRenameCassetteIdEvent(object sender, AseCarrierSlotStatus e)
        {
            try
            {
                AgvcTransCmd agvcTransCmd = Vehicle.AgvcTransCmdBuffer.Values.First(x => x.SlotNumber == e.SlotNumber);
                agvcTransCmd.CassetteId = e.CarrierId;
                Vehicle.AgvcTransCmdBuffer[agvcTransCmd.CommandId] = agvcTransCmd;

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
                asePackage.MovePause();
                var msg = $"MainFlow : Get [{type}]Command.";
                OnMessageShowEvent(this, msg);
                agvcConnector.PauseReply(iSeqNum, 0, PauseEvent.Pause);
                if (Vehicle.AseMovingGuide.PauseStatus == VhStopSingle.Off)
                {
                    Vehicle.AseMovingGuide.PauseStatus = VhStopSingle.On;
                    agvcConnector.StatusChangeReport();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AgvcConnector_OnCmdResumeEvent(ushort iSeqNum, PauseEvent type)
        {
            try
            {
                var msg = $"MainFlow : Get [{type}]Command.";
                OnMessageShowEvent(this, msg);
                agvcConnector.PauseReply(iSeqNum, 0, PauseEvent.Continue);
                asePackage.MoveContinue();
                ResumeVisitTransferSteps();
                agvcConnector.ResumeAskReserve();
                if (Vehicle.AseMovingGuide.PauseStatus == VhStopSingle.Off)
                {
                    Vehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                    agvcConnector.StatusChangeReport();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void AgvcConnector_OnCmdCancelAbortEvent(ushort iSeqNum, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            try
            {
                var msg = $"MainFlow : Get [{receive.CancelAction}] Command.";
                OnMessageShowEvent(this, msg);
                PauseTransfer();
                agvcConnector.CancelAbortReply(iSeqNum, 0, receive);

                string abortCmdId = receive.CmdID.Trim();
                var step = GetCurTransferStep();
                bool IsAbortCurCommand = GetCurTransferStep().CmdId == abortCmdId;
                var targetAbortCmd = Vehicle.AgvcTransCmdBuffer[abortCmdId];

                if (IsAbortCurCommand)
                {
                    agvcConnector.ClearAllReserve();
                    asePackage.MoveStop();
                    targetAbortCmd.CompleteStatus = GetCompleteStatusFromCancelRequest(receive.CancelAction);
                    TransferComplete(targetAbortCmd.CommandId);
                }
                else
                {

                    targetAbortCmd.EnrouteState = CommandState.None;
                    targetAbortCmd.CompleteStatus = GetCompleteStatusFromCancelRequest(receive.CancelAction);

                    ConcurrentDictionary<string, AgvcTransCmd> tempTransCmdBuffer = new ConcurrentDictionary<string, AgvcTransCmd>();
                    foreach (var transCmd in Vehicle.AgvcTransCmdBuffer.Values.ToList())
                    {
                        if (transCmd.CommandId != abortCmdId)
                        {
                            tempTransCmdBuffer.TryAdd(transCmd.CommandId, transCmd);
                        }
                    }
                    Vehicle.AgvcTransCmdBuffer = tempTransCmdBuffer;
                    agvcConnector.StatusChangeReport();
                    ReportAgvcTransferComplete(targetAbortCmd);

                    asePackage.SetTransferCommandInfoRequest(targetAbortCmd, EnumCommandInfoStep.End);

                    if (Vehicle.AgvcTransCmdBuffer.Count == 0)
                    {
                        agvcConnector.NoCommand();
                        GoNextTransferStep = true;
                        IsVisitTransferStepPause = false;
                    }
                }

                ResumeTransfer();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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

            Vehicle.MainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(@"MainFlow.xml");
        }

        public void SetMainFlowConfig(MainFlowConfig mainFlowConfig)
        {
            Vehicle.MainFlowConfig = mainFlowConfig;
            xmlHandler.WriteXml(Vehicle.MainFlowConfig, @"MainFlow.xml");
        }

        public void LoadAgvcConnectorConfig()
        {
            Vehicle.AgvcConnectorConfig = xmlHandler.ReadXml<AgvcConnectorConfig>(@"AgvcConnector.xml");
        }

        public void SetAgvcConnectorConfig(AgvcConnectorConfig agvcConnectorConfig)
        {
            Vehicle.AgvcConnectorConfig = agvcConnectorConfig;
            xmlHandler.WriteXml(Vehicle.AgvcConnectorConfig, @"AgvcConnector.xml");
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ReadCarrierId()
        {
            try
            {
                asePackage.ReadCarrierId();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void BuzzOff()
        {
            try
            {
                asePackage.BuzzerOff();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AsePackage_OnUpdateSlotStatusEvent(object sender, AseCarrierSlotStatus slotStatus)
        {
            try
            {
                if (slotStatus.ManualDeleteCST)
                {
                    slotStatus.CarrierId = "";
                    slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                    switch (slotStatus.SlotNumber)
                    {
                        case EnumSlotNumber.L:
                            Vehicle.AseCarrierSlotL = slotStatus;
                            break;
                        case EnumSlotNumber.R:
                            Vehicle.AseCarrierSlotR = slotStatus;
                            break;
                    }

                    agvcConnector.Send_Cmd136_CstRemove(slotStatus.SlotNumber);
                }
                else
                {
                    switch (slotStatus.SlotNumber)
                    {
                        case EnumSlotNumber.L:
                            Vehicle.AseCarrierSlotL = slotStatus;
                            break;
                        case EnumSlotNumber.R:
                            Vehicle.AseCarrierSlotR = slotStatus;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AsePackage_OnModeChangeEvent(object sender, EnumAutoState autoState)
        {
            try
            {
                if (Vehicle.AutoState != autoState)
                {
                    switch (autoState)
                    {
                        case EnumAutoState.Auto:
                            StopClearAndReset();
                            asePackage.SetVehicleAutoScenario();
                            ResetAllAlarmsFromAgvm();
                            Vehicle.IsReAuto = true;
                            Thread.Sleep(500);
                            CheckCanAuto();
                            UpdateSlotStatus();
                            break;
                        case EnumAutoState.Manual:
                            StopClearAndReset();
                            asePackage.RequestVehicleToManual();
                            break;
                        case EnumAutoState.None:
                            break;
                        default:
                            break;
                    }
                }

                Vehicle.AutoState = autoState;
                agvcConnector.StatusChangeReport();

                OnMessageShowEvent?.Invoke(this, $"Switch to {autoState}");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void CheckCanAuto()
        {

            if (Vehicle.AseMoveStatus.LastSection == null || string.IsNullOrEmpty(Vehicle.AseMoveStatus.LastSection.Id))
            {
                CanAutoMsg = "Section Lost";
                throw new Exception("CheckCanAuto fail. Section Lost.");
            }
            else if (Vehicle.AseMoveStatus.LastAddress == null || string.IsNullOrEmpty(Vehicle.AseMoveStatus.LastAddress.Id))
            {
                CanAutoMsg = "Address Lost";
                throw new Exception("CheckCanAuto fail. Address Lost.");
            }
            else if (Vehicle.AseMoveStatus.AseMoveState != EnumAseMoveState.Idle && Vehicle.AseMoveStatus.AseMoveState != EnumAseMoveState.Block)
            {
                CanAutoMsg = $"Move State = {Vehicle.AseMoveStatus.AseMoveState}";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }
            else if (Vehicle.AseMoveStatus.LastAddress.MyDistance(Vehicle.AseMoveStatus.LastMapPosition) >= Vehicle.MainFlowConfig.InitialPositionRangeMm)
            {
                SetAlarmFromAgvm(54);
                CanAutoMsg = $"Initial Positon Too Far.";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }

            var aseRobotStatus = Vehicle.AseRobotStatus;
            if (aseRobotStatus.RobotState != EnumAseRobotState.Idle)
            {
                CanAutoMsg = $"Robot State = {aseRobotStatus.RobotState}";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }
            else if (!aseRobotStatus.IsHome)
            {
                CanAutoMsg = $"Robot IsHome = {aseRobotStatus.IsHome}";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }

            CanAutoMsg = "OK";
        }

        private void UpdateSlotStatus()
        {
            try
            {
                AseCarrierSlotStatus leftSlotStatus = new AseCarrierSlotStatus(Vehicle.AseCarrierSlotL);
                AseCarrierSlotStatus rightSlotStatus = new AseCarrierSlotStatus(Vehicle.AseCarrierSlotR);

                switch (leftSlotStatus.CarrierSlotStatus)
                {
                    case EnumAseCarrierSlotStatus.Empty:
                        {
                            leftSlotStatus.CarrierId = "";
                            leftSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                            Vehicle.AseCarrierSlotL = leftSlotStatus;
                            Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                        }
                        break;
                    case EnumAseCarrierSlotStatus.Loading:
                        {
                            if (string.IsNullOrEmpty(leftSlotStatus.CarrierId.Trim()))
                            {
                                leftSlotStatus.CarrierId = "ReadFail";
                                leftSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                                Vehicle.AseCarrierSlotL = leftSlotStatus;
                                Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                            }
                            else
                            {
                                Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                            }
                        }
                        break;
                    case EnumAseCarrierSlotStatus.PositionError:
                        {
                            SetAlarmFromAgvm(51);
                            AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
                        }
                        return;
                    case EnumAseCarrierSlotStatus.ReadFail:
                        {
                            leftSlotStatus.CarrierId = "ReadFail";
                            leftSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                            Vehicle.AseCarrierSlotL = leftSlotStatus;
                            Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                        }
                        break;
                    default:
                        break;
                }

                switch (rightSlotStatus.CarrierSlotStatus)
                {
                    case EnumAseCarrierSlotStatus.Empty:
                        {
                            rightSlotStatus.CarrierId = "";
                            rightSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                            Vehicle.AseCarrierSlotR = rightSlotStatus;
                            Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                        }
                        break;
                    case EnumAseCarrierSlotStatus.Loading:
                        {
                            if (string.IsNullOrEmpty(rightSlotStatus.CarrierId.Trim()))
                            {
                                rightSlotStatus.CarrierId = "ReadFail";
                                rightSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                                Vehicle.AseCarrierSlotR = rightSlotStatus;
                                Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                            }
                            else
                            {
                                Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                            }
                        }
                        break;
                    case EnumAseCarrierSlotStatus.PositionError:
                        {
                            SetAlarmFromAgvm(51);
                            AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
                        }
                        return;
                    case EnumAseCarrierSlotStatus.ReadFail:
                        {
                            rightSlotStatus.CarrierId = "ReadFail";
                            rightSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                            Vehicle.AseCarrierSlotR = rightSlotStatus;
                            Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                        }
                        break;
                    default:
                        break;
                }

                agvcConnector.CSTStatusReport();//200625 dabid#
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AgvcConnector_OnConnectionChangeEvent(object sender, bool e)
        {
            Vehicle.IsAgvcConnect = e;
        }

        private void AsePackage_OnAlarmCodeResetEvent(object sender, int e)
        {
            ResetAllAlarmsFromAgvl();
        }

        private void AsePackage_OnAlarmCodeSetEvent1(object sender, int id)
        {
            SetAlarmFromAgvl(id);
        }

        private void AsePackage_OnStatusChangeReportEvent(object sender, string e)
        {
            OnMessageShowEvent?.Invoke(this, e);
            agvcConnector.StatusChangeReport();
        }

        private void AsePackage_OnConnectionChangeEvent(object sender, bool e)
        {
            OnAgvlConnectionChangedEvent?.Invoke(this, e);
        }

        #endregion

        #region Set / Reset Alarm

        public void SetAlarmFromAgvm(int errorCode)
        {
            if (!alarmHandler.dicHappeningAlarms.ContainsKey(errorCode))
            {
                alarmHandler.SetAlarm(errorCode);
                asePackage.SetAlarmCode(errorCode, true);
                var IsAlarm = alarmHandler.IsAlarm(errorCode);

                agvcConnector.SetlAlarmToAgvc(errorCode, IsAlarm);
                var alarmText = alarmHandler.GetAlarmText(errorCode);
                SetAlarmToUI?.Invoke(this, alarmText);
            }
        }

        public void SetAlarmFromAgvl(int errorCode)
        {
            if (!alarmHandler.dicHappeningAlarms.ContainsKey(errorCode))
            {
                alarmHandler.SetAlarm(errorCode);
                var IsAlarm = alarmHandler.IsAlarm(errorCode);

                agvcConnector.SetlAlarmToAgvc(errorCode, IsAlarm);
                var alarmText = alarmHandler.GetAlarmText(errorCode);
                SetAlarmToUI?.Invoke(this, alarmText);
            }
        }

        public void ResetAllAlarmsFromAgvm()
        {
            alarmHandler.ResetAllAlarms();
            asePackage.ResetAllAlarmCode();
            agvcConnector.ResetAllAlarmsToAgvc();
        }

        public void ResetAllAlarmsFromAgvc()
        {
            alarmHandler.ResetAllAlarms();
            asePackage.ResetAllAlarmCode();
        }

        public void ResetAllAlarmsFromAgvl()
        {
            alarmHandler.ResetAllAlarms();
            agvcConnector.ResetAllAlarmsToAgvc();
        }

        #endregion

        #region Log

        private void LogException(string classMethodName, string exMsg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("Error", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", exMsg));
            }
            catch (Exception)
            {
            }
        }

        private void LogDebug(string classMethodName, string msg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", msg));
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

    public class LastIdlePosition
    {
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public MapPosition Position { get; set; } = new MapPosition();
    }
}
