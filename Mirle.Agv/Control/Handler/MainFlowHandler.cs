using Mirle.Agv.Control.Tools;
using Mirle.Agv.Control.Tools.Logger;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mirle.Agv.Control.Handler.TransCmdsSteps;

namespace Mirle.Agv.Control
{
    public class MainFlowHandler : ICmdFinished
    {
        #region Configs

        private string configPath;
        private ConfigHandler configHandler;
        private MiddlerConfigs middlerConfigs;
        private Sr2000Configs sr2000Configs;
        private MainFlowConfigs mainFlowConfigs;
        private MapConfigs mapConfigs;
        private MoveControlConfigs moveControlConfigs;

        #endregion

        #region TransCmds

        private List<TransCmd> transCmds;
        private List<TransCmd> lastTransCmds;
        private ConcurrentQueue<MoveCmdInfo> queWaitForReserve;
        private bool goNextTransCmd;
        public bool GoNextTransCmd
        {
            get { return goNextTransCmd; }
            set { goNextTransCmd = value; }
        }
        private int transCmdsIndex;
        public int TransCmdsIndex
        {
            get { return transCmdsIndex; }
            set { transCmdsIndex = value; }
        }
        private ITransCmdsStep step;

        #endregion

        #region Agent

        private BmsAgent bmsAgent;
        private ElmoAgent elmoAgent;
        private MiddleAgent middleAgent;
        private PlcAgent plcAgent;
        private LoggerAgent loggerAgent;

        #endregion

        #region Handler

        private BatteryHandler batteryHandler;
        private CoupleHandler coupleHandler;
        private MapHandler mapHandler;
        private MoveControlHandler moveControlHandler;
        private RobotControlHandler robotControlHandler;

        #endregion

        #region Threads

        private Thread thdGetsNewTransCmds;
        private Thread thdAskReserve;
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);

        #endregion

        #region Events

        public event EventHandler<InitialEventArgs> OnXXXIntialDoneEvent;

        #endregion

        public Vehicle theVehicle;
        private bool isIniOk;

        public MainFlowHandler()
        {
            isIniOk = true;
            //InitialMainFlowHandler();

            //RunThreads();

            transCmds = new List<TransCmd>();
            lastTransCmds = new List<TransCmd>();
            queWaitForReserve = new ConcurrentQueue<MoveCmdInfo>();
        }

        public void InitialMainFlowHandler()
        {
            ConfigsInitial();
            LoggersInitial();

            AgentInitial();
            HandlerInitial();

            VehicleInitial();

            EventInitial();

            ThreadInitial();

            if (isIniOk)
            {
                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "全部"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }
            }
        }

        private void ThreadInitial()
        {
            thdGetsNewTransCmds = new Thread(new ThreadStart(VisitTransCmds));
            thdGetsNewTransCmds.IsBackground = true;

            thdAskReserve = new Thread(new ThreadStart(AskReserve));
            thdAskReserve.IsBackground = true;
        }

        private void ConfigsInitial()
        {
            try
            {
                configPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
                configHandler = new ConfigHandler(configPath);

                mainFlowConfigs = new MainFlowConfigs();
                var tempLogConfigPath = configHandler.GetString("MainFlow", "LogConfigPath", "Log.ini");
                mainFlowConfigs.LogConfigPath = Path.Combine(Environment.CurrentDirectory, tempLogConfigPath);
                LoggerAgent.LogConfigPath = mainFlowConfigs.LogConfigPath;
                int.TryParse(configHandler.GetString("MainFlow", "TransCmdsCheckInterval", "15"), out int tempTransCmdsCheckInterval);
                mainFlowConfigs.TransCmdsCheckInterval = tempTransCmdsCheckInterval;
                int.TryParse(configHandler.GetString("MainFlow", "DoTransCmdsInterval", "15"), out int tempDoTransCmdsInterval);
                mainFlowConfigs.DoTransCmdsInterval = tempDoTransCmdsInterval;
                int.TryParse(configHandler.GetString("MainFlow", "ReserveLength", "3"), out int tempReserveLength);
                mainFlowConfigs.ReserveLength = tempReserveLength;
                int.TryParse(configHandler.GetString("MainFlow", "AskReserveInterval", "15"), out int tempAskReserveInterval);
                mainFlowConfigs.AskReserveInterval = tempAskReserveInterval;

                middlerConfigs = new MiddlerConfigs();
                int.TryParse(configHandler.GetString("Middler", "ClientNum", "1"), out int tempClientNum);
                middlerConfigs.ClientNum = tempClientNum;
                middlerConfigs.ClientName = configHandler.GetString("Middler", "ClientName", "AGV01");
                middlerConfigs.RemoteIp = configHandler.GetString("Middler", "RemoteIp", "192.168.9.203");
                int.TryParse(configHandler.GetString("Middler", "RemotePort", "10001"), out int tempRemotePort);
                middlerConfigs.RemotePort = tempRemotePort;
                middlerConfigs.LocalIp = configHandler.GetString("Middler", "LocalIp", "192.168.9.131");
                int.TryParse(configHandler.GetString("Middler", "LocalPort", "5002"), out int tempPort);
                middlerConfigs.LocalPort = tempPort;
                int.TryParse(configHandler.GetString("Middler", "RecvTimeoutMs", "10000"), out int tempRecvTimeoutMs);
                middlerConfigs.RecvTimeoutMs = tempRecvTimeoutMs;
                int.TryParse(configHandler.GetString("Middler", "SendTimeoutMs", "0"), out int tempSendTimeoutMs);
                middlerConfigs.SendTimeoutMs = tempSendTimeoutMs;
                int.TryParse(configHandler.GetString("Middler", "MaxReadSize", "0"), out int tempMaxReadSize);
                middlerConfigs.MaxReadSize = tempMaxReadSize;
                int.TryParse(configHandler.GetString("Middler", "ReconnectionIntervalMs", "10000"), out int tempReconnectionIntervalMs);
                middlerConfigs.ReconnectionIntervalMs = tempReconnectionIntervalMs;
                int.TryParse(configHandler.GetString("Middler", "MaxReconnectionCount", "10"), out int tempMaxReconnectionCount);
                middlerConfigs.MaxReconnectionCount = tempMaxReconnectionCount;
                int.TryParse(configHandler.GetString("Middler", "RetryCount", "2"), out int tempRetryCount);
                middlerConfigs.RetryCount = tempRetryCount;
                int.TryParse(configHandler.GetString("Middler", "SleepTime", "10"), out int tempSleepTime);
                middlerConfigs.SleepTime = tempSleepTime;

                mapConfigs = new MapConfigs();
                mapConfigs.SectionFilePath = configHandler.GetString("Map", "SectionFilePath", "ASECTION.csv");
                mapConfigs.AddressFilePath = configHandler.GetString("Map", "AddressFilePath", "AADDRESS.csv");

                sr2000Configs = new Sr2000Configs();
                int.TryParse(configHandler.GetString("Sr2000", "TrackingInterval", "10"), out int tempTrackingInterval);
                sr2000Configs.TrackingInterval = tempTrackingInterval;

                moveControlConfigs = new MoveControlConfigs();

                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "讀寫設定檔"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }
            }
            catch (Exception)
            {
                isIniOk = false;
                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = false,
                        ItemName = "讀寫設定檔"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }
            }
        }

        private void LoggersInitial()
        {
            try
            {
                //TODO : make abstract class with an logger and its bean and a function do log, make 4 level subclass imp this abstract class
                loggerAgent = LoggerAgent.Instance;

                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "Logger"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }

            }
            catch (Exception)
            {
                isIniOk = false;
                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = false,
                        ItemName = "Logger"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }

            }
        }

        private void AgentInitial()
        {
            try
            {
                bmsAgent = new BmsAgent();
                elmoAgent = new ElmoAgent();
                middleAgent = new MiddleAgent(middlerConfigs);
                plcAgent = new PlcAgent();

                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "Agent"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }

            }
            catch (Exception ex)
            {
                var temp = ex.StackTrace;
                isIniOk = false;
                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = false,
                        ItemName = "Agent"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }
            }
        }

        private void HandlerInitial()
        {
            try
            {
                batteryHandler = new BatteryHandler();
                coupleHandler = new CoupleHandler();
                mapHandler = new MapHandler(mapConfigs);
                moveControlHandler = new MoveControlHandler(moveControlConfigs, sr2000Configs);
                robotControlHandler = new RobotControlHandler();

                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "Handler"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }

            }
            catch (Exception)
            {
                isIniOk = false;
                var args = new InitialEventArgs
                {
                    IsOk = false,
                    ItemName = "Handler"
                };
                OnXXXIntialDoneEvent(this, args);
            }
        }

        private void VehicleInitial()
        {
            try
            {
                theVehicle = Vehicle.Instance;

                var args = new InitialEventArgs
                {
                    IsOk = true,
                    ItemName = "Vehicle"
                };
                OnXXXIntialDoneEvent(this, args);
            }
            catch (Exception)
            {
                isIniOk = false;
                var args = new InitialEventArgs
                {
                    IsOk = false,
                    ItemName = "Handler"
                };
                OnXXXIntialDoneEvent(this, args);
            }

        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'

                middleAgent.OnMiddlerGetsNewTransCmdsEvent += OnMiddlerGetsNewTransCmds;
                middleAgent.OnMiddlerGetsNewTransCmdsEvent += mapHandler.OnMiddlerGetsNewTransCmds;

                //來自MoveControl的Barcode更新訊息，通知MainFlow(this)'middleAgent'mapHandler
                moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += OnMapBarcodeValuesChangedEvent;
                moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += middleAgent.OnMapBarcodeValuesChangedEvent;
                moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += mapHandler.OnMapBarcodeValuesChangedEvent;
                moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += moveControlHandler.OnMapBarcodeValuesChangedEvent;

                //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                moveControlHandler.OnMoveFinished += OnTransCmdsFinishedEvent;
                moveControlHandler.OnMoveFinished += middleAgent.OnTransCmdsFinishedEvent;
                moveControlHandler.OnMoveFinished += mapHandler.OnTransCmdsFinishedEvent;

                //來自RobotControl的取貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                robotControlHandler.OnLoadFinished += OnTransCmdsFinishedEvent;
                robotControlHandler.OnLoadFinished += middleAgent.OnTransCmdsFinishedEvent;
                robotControlHandler.OnLoadFinished += mapHandler.OnTransCmdsFinishedEvent;

                //來自RobotControl的放貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                robotControlHandler.OnUnloadFinished += OnTransCmdsFinishedEvent;
                robotControlHandler.OnUnloadFinished += middleAgent.OnTransCmdsFinishedEvent;
                robotControlHandler.OnUnloadFinished += mapHandler.OnTransCmdsFinishedEvent;

                var args = new InitialEventArgs
                {
                    IsOk = true,
                    ItemName = "事件"
                };
                OnXXXIntialDoneEvent(this, args);

            }
            catch (Exception)
            {
                isIniOk = false;
                var args = new InitialEventArgs
                {
                    IsOk = false,
                    ItemName = "事件"
                };
                OnXXXIntialDoneEvent(this, args);

            }
        }

        private void OnMiddlerGetsNewTransCmds(object sender, List<TransCmd> transCmds)
        {
            try
            {
                if (CanGetNewTransCmds())
                {
                    this.transCmds = transCmds;
                    transCmds.Add(new EmptyTransCmd());
                    middleAgent.ClearTransCmds();

                    thdGetsNewTransCmds.Start();

                    thdAskReserve.Start();
                    //thdAskReserve等待thdGetsNewTransCmds一起完結
                }

            }
            catch (Exception ex)
            {
                string className = GetType().Name;
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string classMethodName = className + ":" + methodName;
                LogFormat logFormat = new LogFormat("Error", "1", classMethodName, "Device", "CarrierID", ex.StackTrace);
                loggerAgent.LogError(logFormat);
            }
        }

        private bool CanGetNewTransCmds()
        {
            // 判斷當前是否可接收新的搬貨命令 若否 則發送報告
            throw new NotImplementedException();
        }

        private void VisitTransCmds()
        {
            PreVisitTransCmds();

            while (transCmdsIndex < transCmds.Count)
            {
                #region Pause And Stop Check

                PauseEvent.WaitOne(Timeout.Infinite);
                if (ShutdownEvent.WaitOne(0))
                {
                    break;
                }

                #endregion

                if (goNextTransCmd)
                {
                    goNextTransCmd = false;
                    DoTransfer();

                    #region Comment
                    //TransCmd transCmd = transCmds[index];
                    //switch (transCmd.GetType())
                    //{
                    //    case EnumTransCmdType.Move:
                    //        MoveCmdInfo moveCmd = (MoveCmdInfo)transCmd;
                    //        queWaitForReserve.Enqueue(moveCmd);
                    //        goNextTransCmd = !moveCmd.IsPrecisePositioning;
                    //        //TODO
                    //        //MoveComplete(MoveToEnd will set goNextTransCmd into true and go on
                    //        break;
                    //    case EnumTransCmdType.Load:
                    //        LoadCmdInfo loadCmdInfo = (LoadCmdInfo)transCmd;
                    //        //TODO
                    //        //command PLC to DoLoad
                    //        //LoadComplete will set goNextTransCmd into true and go on
                    //        robotControlHandler.DoLoad(loadCmdInfo);
                    //        break;
                    //    case EnumTransCmdType.Unload:
                    //        UnloadCmdInfo unloadCmdInfo = (UnloadCmdInfo)transCmd;
                    //        //TODO
                    //        //command PLC to DoLoad
                    //        //LoadComplete will set goNextTransCmd into true and go on
                    //        robotControlHandler.DoUnload(unloadCmdInfo);
                    //        break;
                    //    default:
                    //        break;
                    //}

                    //transCmdsIndex++;
                    #endregion
                }

                SpinWait.SpinUntil(() => false, mainFlowConfigs.DoTransCmdsInterval);
            }

            //OnTransCmdsFinishedEvent(this, EnumCompleteStatus.TransferComplete);
            AfterVisitTransCmds();
        }

        private void AfterVisitTransCmds()
        {
            transCmds.Clear();
            transCmdsIndex = 0;
            goNextTransCmd = false;
            SetTransCmdsStep(new Idle());
        }

        private void PreVisitTransCmds()
        {
            transCmdsIndex = 0;
            goNextTransCmd = true;
            PauseEvent.Set();
            ShutdownEvent.Reset();
        }

        private bool IsReadyToMoveCmdQueFull()
        {
            return moveControlHandler.GetAmountOfQueReadyCmds() >= mainFlowConfigs.ReserveLength;
        }

        private bool CanVehUnload()
        {
            // 判斷當前是否可載貨 若否 則發送報告
            throw new NotImplementedException();
        }

        private bool CanVehLoad()
        {
            // 判斷當前是否可卸貨 若否 則發送報告
            throw new NotImplementedException();
        }

        private bool CanVehMove()
        {
            //battery/emo/beam/etc/reserve
            // 判斷當前是否可移動 若否 則發送報告
            throw new NotImplementedException();
        }

        private bool CanCarrierIdRead()
        {
            // 判斷當前貨物的ID是否可正確讀取 若否 則發送報告
            throw new NotImplementedException();
        }

        private void AskReserve()
        {
            while (transCmdsIndex < transCmds.Count)
            {
                #region Pause And Stop Check

                PauseEvent.WaitOne(Timeout.Infinite);
                if (ShutdownEvent.WaitOne(0))
                {
                    break;
                }

                #endregion

                if (CanAskReserve())
                {
                    queWaitForReserve.TryPeek(out MoveCmdInfo peek);
                    if (middleAgent.GetReserveFromAgvc(peek.Section.Id))
                    {
                        theVehicle.UpdateStatus(peek);
                        moveControlHandler.DoTransfer(peek);
                        queWaitForReserve.TryDequeue(out MoveCmdInfo aMoveCmd);
                    }
                }

                SpinWait.SpinUntil(() => false, mainFlowConfigs.AskReserveInterval);
            }
        }

        private bool CanAskReserve()
        {
            return CanVehMove() && !IsReadyToMoveCmdQueFull() && IsWaitForReserveQueNotEmpty();
        }

        private bool IsWaitForReserveQueNotEmpty()
        {
            return !queWaitForReserve.IsEmpty;
        }

        public void Pause()
        {
            PauseEvent.Reset();
            SetTransCmdsStep(new Idle());
        }

        public void Resume()
        {
            PauseEvent.Set();
        }

        public void Stop()
        {
            ShutdownEvent.Set();
            PauseEvent.Set();            
            if (thdGetsNewTransCmds.IsAlive)
            {
                thdGetsNewTransCmds.Join();
            }
            if (thdAskReserve.IsAlive)
            {
                thdAskReserve.Join();
            }
            SetTransCmdsStep(new Idle());
        }

        public void UpdateMapBarcode(MapBarcodeReader mapBarcode)
        {
            theVehicle.UpdateStatus(mapBarcode);
        }

        public void OnMapBarcodeValuesChangedEvent(object sender, MapBarcodeReader mapBarcodeValues)
        {
            theVehicle.UpdateStatus(mapBarcodeValues);
        }

        public void OnTransCmdsFinishedEvent(object sender, EnumCompleteStatus status)
        {
            switch (status)
            {
                case EnumCompleteStatus.Move:
                    VisitNextTransCmd();
                    break;
                case EnumCompleteStatus.Load:
                    if (CanCarrierIdRead())
                    {
                        VisitNextTransCmd();
                    }
                    break;
                case EnumCompleteStatus.Unload:
                    VisitNextTransCmd();
                    break;
                case EnumCompleteStatus.LoadUnload:
                    break;
                case EnumCompleteStatus.Home:
                    break;
                case EnumCompleteStatus.MtlHome:
                    break;
                case EnumCompleteStatus.MoveToMtl:
                    break;
                case EnumCompleteStatus.SystemOut:
                    break;
                case EnumCompleteStatus.SystemIn:
                    break;
                case EnumCompleteStatus.Cancel:
                    break;
                case EnumCompleteStatus.Abort:
                    break;
                case EnumCompleteStatus.VehicleAbort:
                    break;
                case EnumCompleteStatus.IdMissmatch:
                    break;
                case EnumCompleteStatus.IdReadFail:
                    break;
                case EnumCompleteStatus.InterlockError:
                    break;
                default:
                    break;
            }
        }

        private void VisitNextTransCmd()
        {
            if (transCmdsIndex < transCmds.Count)
            {
                transCmdsIndex++;
                goNextTransCmd = true;
            }
            else
            {
                Stop();
                SetLasTransCmds();
                //Send Transfer Complete to Middler
            }
        }

        private void SetLasTransCmds()
        {
            lastTransCmds.Clear();
            for (int i = 0; i < transCmds.Count; i++)
            {
                lastTransCmds.Add(transCmds[i]);
            }
            transCmds.Clear();
        }

        public TransCmd GetCurTransCmd()
        {
            TransCmd transCmd = new EmptyTransCmd();
            if (transCmdsIndex < transCmds.Count)
            {
                transCmd = transCmds[transCmdsIndex];
            }
            return transCmd;
        }

        public TransCmd GetNextTransCmd()
        {
            TransCmd transCmd = new EmptyTransCmd();
            int nextIndex = transCmdsIndex + 1;
            if (nextIndex < transCmds.Count)
            {
                transCmd = transCmds[nextIndex];
            }
            return transCmd;
        }

        public void SetTransCmdsStep(ITransCmdsStep step)
        {
            this.step = step;
        }

        public void DoTransfer()
        {
            step.DoTransfer(this);
        }

        public void EnqueWaitForReserve(MoveCmdInfo moveCmd)
        {
            if (CanVehMove())
            {
                queWaitForReserve.Enqueue(moveCmd);
            }
        }

        public void Unload(UnloadCmdInfo unloadCmd)
        {
            if (CanVehUnload())
            {
                robotControlHandler.DoUnload(unloadCmd);
            }
        }

        public void Load(LoadCmdInfo loadCmd)
        {
            if (CanVehLoad())
            {
                robotControlHandler.DoLoad(loadCmd);
            }
        }

        public void OnMsgFromAgvcAddHandler(EventHandler<string> eventHandler)
        {
            middleAgent.OnMsgFromAgvcEvent += eventHandler;
        }
        public void OnMsgToAgvcAddHandler(EventHandler<string> eventHandler)
        {
            middleAgent.OnMsgToAgvcEvent += eventHandler;
        }
        public void OnMsgFromVehicleAddHandler(EventHandler<string> eventHandler)
        {
            middleAgent.OnMsgFromVehicleEvent += eventHandler;
        }
        public void OnMsgToVehicleAddHandler(EventHandler<string> eventHandler)
        {
            middleAgent.OnMsgToVehicleEvent += eventHandler;
        }

        public void ReconnectToAgvc()
        {
            middleAgent.ReconnectToAgvc();
        }

        public void MiddlerTestMsg()
        {
           // middleAgent.TestMsg();
            middleAgent.Send_Cmd131(20, 1, "SomeReason");
        }

        

    }
}
