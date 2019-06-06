using Mirle.Agv.Control.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mirle.Agv.Control.Handler.TransCmdsSteps;
using TcpIpClientSample;

namespace Mirle.Agv.Control
{
    public class MainFlowHandler : ICmdFinished
    {
        #region Configs

        private string rootDir;
        private string configPath;
        private ConfigHandler configHandler;
        private MiddlerConfigs middlerConfigs;
        private Sr2000Configs sr2000Configs;
        private MainFlowConfigs mainFlowConfigs;
        private MapConfigs mapConfigs;
        private MoveControlConfigs moveControlConfigs;
        private BatteryConfigs batteryConfigs;

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
        private AgvcTransCmd agvcTransCmd;
        public MapInfo theMapInfo;

        public MainFlowHandler()
        {
            isIniOk = true;
            rootDir = Environment.CurrentDirectory;
            //InitialMainFlowHandler();

            //RunThreads();

            transCmds = new List<TransCmd>();
            lastTransCmds = new List<TransCmd>();
            queWaitForReserve = new ConcurrentQueue<MoveCmdInfo>();
        }

        public MainFlowHandler(string rootDir)
        {
            isIniOk = true;
            this.rootDir = rootDir;
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
            GetMapInfo();

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

        private void GetMapInfo()
        {
            theMapInfo = MapInfo.Instance;
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
                configPath = Path.Combine(rootDir, "Configs.ini");
                configHandler = new ConfigHandler(configPath);

                mainFlowConfigs = new MainFlowConfigs();
                mainFlowConfigs.LogConfigPath = configHandler.GetString("MainFlow", "LogConfigPath", "Log.ini");
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
                mapConfigs.RootDir = configHandler.GetString("Map", "RootDir", Environment.CurrentDirectory);
                mapConfigs.SectionFileName = configHandler.GetString("Map", "SectionFileName", "ASECTION.csv");
                mapConfigs.AddressFileName = configHandler.GetString("Map", "AddressFileName", "AADDRESS.csv");
                mapConfigs.BarcodeFileName = configHandler.GetString("Map", "BarcodeFileName", "ABARCODE.csv");

                sr2000Configs = new Sr2000Configs();
                int.TryParse(configHandler.GetString("Sr2000", "TrackingInterval", "10"), out int tempTrackingInterval);
                sr2000Configs.TrackingInterval = tempTrackingInterval;

                moveControlConfigs = new MoveControlConfigs();

                batteryConfigs = new BatteryConfigs();
                int.TryParse(configHandler.GetString("Battery", "Percentage", "80"), out int tempPercentage);
                batteryConfigs.Percentage = tempPercentage;
                double.TryParse(configHandler.GetString("Battery", "Voltage", "40"), out double tempVoltage);
                batteryConfigs.Voltage = tempVoltage;
                int.TryParse(configHandler.GetString("Battery", "Temperature", "30"), out int tempTemperature);
                batteryConfigs.Temperature = tempTemperature;
                int.TryParse(configHandler.GetString("Battery", "LowPowerThreshold", "25"), out int tempLowPowerThreshold);
                batteryConfigs.LowPowerThreshold = tempLowPowerThreshold;
                int.TryParse(configHandler.GetString("Battery", "HighTemperatureThreshold", "45"), out int tempHighTemperatureThreshold);
                batteryConfigs.HighTemperatureThreshold = tempHighTemperatureThreshold;

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

                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = false,
                        ItemName = "Handler"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }
            }
        }

        private void VehicleInitial()
        {
            try
            {
                theVehicle = Vehicle.Instance;
                theVehicle.SetupBattery(batteryConfigs);

                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "Vehicle"
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
                        ItemName = "Vehicle"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }
            }

        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'mapHandler
                middleAgent.OnInstallTransferCommandEvent += OnInstallTransferCommand;
                middleAgent.OnInstallTransferCommandEvent += mapHandler.OnInstallTransferCommand;

                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'mapHandler
                middleAgent.OnTransferCancelEvent += OnMiddlerGetsCancelEvent;
                middleAgent.OnTransferCancelEvent += mapHandler.OnMiddlerGetsCancelEvent;

                middleAgent.OnTransferAbortEvent += OnMiddlerGetsAbortEvent;

                //來自MoveControl的Barcode更新訊息，通知MainFlow(this)'middleAgent'mapHandler
                moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += OnMapBarcodeValuesChangedEvent;
                moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += middleAgent.OnMapBarcodeValuesChangedEvent;
                moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += mapHandler.OnMapBarcodeValuesChangedEvent;
                moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += moveControlHandler.OnMapBarcodeValuesChangedEvent;

                //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                moveControlHandler.OnMoveFinished += OnTransCmdsFinishedEvent;
                moveControlHandler.OnMoveFinished += mapHandler.OnTransCmdsFinishedEvent;

                //來自RobotControl的取貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                robotControlHandler.OnLoadFinished += OnTransCmdsFinishedEvent;
                robotControlHandler.OnLoadFinished += mapHandler.OnTransCmdsFinishedEvent;

                //來自RobotControl的放貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                robotControlHandler.OnUnloadFinished += OnTransCmdsFinishedEvent;
                robotControlHandler.OnUnloadFinished += mapHandler.OnTransCmdsFinishedEvent;



                if (OnXXXIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "事件"
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
                        ItemName = "事件"
                    };
                    OnXXXIntialDoneEvent(this, args);
                }
            }
        }

        private void OnMiddlerGetsAbortEvent(object sender, string e)
        {
            theVehicle.CmdID = e;
            OnAbortEvent();
        }

        private void OnMiddlerGetsCancelEvent(object sender, string e)
        {
            theVehicle.CmdID = e;
            OnCancelEvent();
        }

        private void OnInstallTransferCommand(object sender, AgvcTransCmd agvcTransCmd)
        {
            try
            {
                this.agvcTransCmd = agvcTransCmd;
                if (!CheckTransCmdSectionsAndAddressesMatch(agvcTransCmd))
                {
                    return;
                }

                if (GenralTransCmds()) // Move/Load/Unload/LoadUnload
                {
                    ConvertAgvcTransCmdIntoList();
                    transCmds.Add(new EmptyTransCmd());

                    thdGetsNewTransCmds.Start();

                    thdAskReserve.Start();
                    //thdAskReserve等待thdGetsNewTransCmds一起完結
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                string className = GetType().Name;
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string classMethodName = className + ":" + methodName;
                LogFormat logFormat = new LogFormat("Error", "1", classMethodName, "Device", "CarrierID", ex.StackTrace);
                loggerAgent.LogMsg("Error", logFormat);
            }
        }

        private bool CheckTransCmdSectionsAndAddressesMatch(AgvcTransCmd agvcTransCmd)
        {
            switch (agvcTransCmd.CmdType)
            {
                case EnumAgvcTransCmdType.Move:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToUnloadSections, agvcTransCmd.ToUnloadAddresses, agvcTransCmd.SeqNum);
                case EnumAgvcTransCmdType.Load:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToLoadSections, agvcTransCmd.ToLoadAddresses, agvcTransCmd.SeqNum);
                case EnumAgvcTransCmdType.Unload:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToUnloadSections, agvcTransCmd.ToUnloadAddresses, agvcTransCmd.SeqNum);
                case EnumAgvcTransCmdType.LoadUnload:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToLoadSections, agvcTransCmd.ToLoadAddresses, agvcTransCmd.SeqNum) || IsSectionsAndAddressesMatch(agvcTransCmd.ToUnloadSections, agvcTransCmd.ToUnloadAddresses, agvcTransCmd.SeqNum);
                default:
                    return true;
            }
        }

        private bool IsSectionsAndAddressesMatch(string[] sections, string[] addresses, ushort aSeqNum)
        {
            if (sections.Length + 1 != addresses.Length)
            {
                int replyCode = 1; // NG
                string reason = $"guildSections and guildAddresses is not match";
                middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                return false;
            }

            for (int i = 0; i < sections.Length; i++)
            {
                if (!theMapInfo.dicMapSections.ContainsKey(sections[i]))
                {
                    int replyCode = 1; // NG
                    string reason = $"{sections[i]} is not in the map.";
                    middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                    return false;
                }

                var tempSection = theMapInfo.dicMapSections[sections[i]];

                if (!theMapInfo.dicMapAddresses.ContainsKey(addresses[i]))
                {
                    int replyCode = 1; // NG
                    string reason = $"{addresses[i]} is not in the map.";
                    middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                    return false;
                }

                if (!theMapInfo.dicMapAddresses.ContainsKey(addresses[i + 1]))
                {
                    int replyCode = 1; // NG
                    string reason = $"{addresses[i+1]} is not in the map.";
                    middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                    return false;
                }

                if (tempSection.FromAddress == addresses[i])
                {
                    if (tempSection.ToAddress != addresses[i + 1])
                    {
                        int replyCode = 1; // NG
                        string reason = $"guildSections and guildAddresses is not match";
                        middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                        return false;
                    }
                }
                else if (tempSection.ToAddress == addresses[i])
                {
                    if (tempSection.FromAddress != addresses[i + 1])
                    {
                        int replyCode = 1; // NG
                        string reason = $"guildSections and guildAddresses is not match";
                        middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                        return false;
                    }
                }
                else
                {
                    int replyCode = 1; // NG
                    string reason = $"guildSections and guildAddresses is not match";
                    middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                    return false;
                }
            }
            return true;
        }


        private bool GenralTransCmds()
        {
            var type = agvcTransCmd.CmdType;
            return (type == EnumAgvcTransCmdType.Move) || type == EnumAgvcTransCmdType.Load || (type == EnumAgvcTransCmdType.Unload) || (type == EnumAgvcTransCmdType.LoadUnload);
        }

        private void ConvertAgvcTransCmdIntoList()
        {
            transCmds.Clear();

            switch (agvcTransCmd.CmdType)
            {
                case EnumAgvcTransCmdType.Move:
                    ConvertAgvcMoveCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCmdType.Load:
                    ConvertAgvcLoadCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCmdType.Unload:
                    ConvertAgvcUnloadCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCmdType.LoadUnload:
                    ConvertAgvcLoadUnloadCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCmdType.Home:
                    ConvertAgvcHomeCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCmdType.Override:
                    ConvertAgvcOverrideCmdIntoList(agvcTransCmd);
                    break;
                case EnumAgvcTransCmdType.Else:
                default:
                    ConvertAgvcElseCmdIntoList(agvcTransCmd);
                    break;
            }
        }

        private void ConvertAgvcElseCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            throw new NotImplementedException();
        }

        private void ConvertAgvcOverrideCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            //TODO: clone old transCmds
            //TODO: separate transCmds into MLMU
            //TODO: override move part.

            var tempTransCmds = new List<TransCmd>();
            for (int i = 0; i < transCmds.Count; i++)
            {

            }

            var curSection = theVehicle.GetVehLoacation().Section.Id;
            if (agvcTransCmd.ToLoadSections.Length > 0) //curSection at to load sections
            {
                for (int i = 0; i < agvcTransCmd.ToLoadSections.Length; i++)
                {

                }
            }
            else
            {

            }
        }

        private void ConvertAgvcHomeCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            throw new NotImplementedException();
        }

        private void ConvertAgvcLoadUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            ConvertAgvcLoadCmdIntoList(agvcTransCmd);
            ConvertAgvcUnloadCmdIntoList(agvcTransCmd);
        }

        private void ConvertAgvcUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToUnloadSections.Length > 0)
            {
                for (int i = 0; i < agvcTransCmd.ToUnloadSections.Length; i++)
                {
                    MoveCmdInfo moveCmd = new MoveCmdInfo();
                    moveCmd.CmdId = agvcTransCmd.CmdId;
                    moveCmd.MoveEndAddress = agvcTransCmd.UnloadAddtess;
                    var section = theMapInfo.dicMapSections[agvcTransCmd.ToUnloadSections[i]];
                    moveCmd.Section = section;
                    moveCmd.TotalMoveLength += section.Distance;
                    moveCmd.IsPrecisePositioning = (i == agvcTransCmd.ToUnloadSections.Length - 1);
                    transCmds.Add(moveCmd);
                }
            }

            UnloadCmdInfo unloadCmd = new UnloadCmdInfo();
            unloadCmd.CarrierId = agvcTransCmd.CarrierId;
            unloadCmd.CmdId = agvcTransCmd.CmdId;
            unloadCmd.UnloadAddress = agvcTransCmd.UnloadAddtess;

            transCmds.Add(unloadCmd);
        }

        private void ConvertAgvcLoadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToLoadSections.Length > 0)
            {
                for (int i = 0; i < agvcTransCmd.ToLoadSections.Length; i++)
                {
                    MoveCmdInfo moveCmd = new MoveCmdInfo();
                    moveCmd.CmdId = agvcTransCmd.CmdId;
                    moveCmd.MoveEndAddress = agvcTransCmd.LoadAddress;
                    var section = theMapInfo.dicMapSections[agvcTransCmd.ToLoadSections[i]];
                    moveCmd.Section = section;
                    moveCmd.TotalMoveLength += section.Distance;
                    moveCmd.IsPrecisePositioning = (i == agvcTransCmd.ToLoadSections.Length - 1);
                    transCmds.Add(moveCmd);
                }
            }

            LoadCmdInfo loadCmd = new LoadCmdInfo();
            loadCmd.CarrierId = agvcTransCmd.CarrierId;
            loadCmd.CmdId = agvcTransCmd.CmdId;
            loadCmd.LoadAddress = agvcTransCmd.LoadAddress;

            transCmds.Add(loadCmd);
        }

        private void ConvertAgvcMoveCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToUnloadSections.Length > 0)
            {
                for (int i = 0; i < agvcTransCmd.ToUnloadSections.Length; i++)
                {
                    MoveCmdInfo moveCmd = new MoveCmdInfo();
                    moveCmd.CmdId = agvcTransCmd.CmdId;
                    moveCmd.MoveEndAddress = agvcTransCmd.UnloadAddtess;
                    var section = theMapInfo.dicMapSections[agvcTransCmd.ToUnloadSections[i]];
                    moveCmd.Section = section;
                    moveCmd.TotalMoveLength += section.Distance;
                    moveCmd.IsPrecisePositioning = (i == agvcTransCmd.ToUnloadSections.Length - 1);
                    transCmds.Add(moveCmd);
                }
            }
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
            theVehicle.SetVehicleStop();
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
                    OnMoveFinishedEvent();
                    break;
                case EnumCompleteStatus.Load:
                    OnLoadFinishedEvent();
                    break;
                case EnumCompleteStatus.Unload:
                    OnUnloadFinishedEvent();
                    break;
                case EnumCompleteStatus.LoadUnload:
                    OnLoadunloadFinishedEvent();
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
                    OnCancelEvent();
                    break;
                case EnumCompleteStatus.Abort:
                    OnAbortEvent();
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

        private void OnAbortEvent()
        {
            Stop();
            middleAgent.MainFlowGetAbort();
        }

        private void OnCancelEvent()
        {
            Stop();
            middleAgent.MainFlowGetCancel();
        }

        private void OnMoveFinishedEvent()
        {
            if (NextTransCmdIsLoad())
            {
                middleAgent.ReportLoadArrivals();
                VisitNextTransCmd();
            }
            else if (NextTransCmdIsUnload())
            {
                middleAgent.UnloadArrivals();
                VisitNextTransCmd();
            }
            else
            {
                middleAgent.MoveComplete();
            }
        }

        private bool NextTransCmdIsUnload()
        {
            return transCmds[transCmdsIndex + 1].GetType() == EnumTransCmdType.Unload;
        }

        private bool NextTransCmdIsLoad()
        {
            return transCmds[transCmdsIndex + 1].GetType() == EnumTransCmdType.Load;
        }

        private void OnLoadFinishedEvent()
        {
            if (CanCarrierIdRead())
            {
                //update carrierId
            }
            else
            {
                //carrierId = unknow
            }

            if (NextTransCmdIsMove())
            {
                middleAgent.LoadCompleteInLoadunload();
                VisitNextTransCmd();
            }
            else
            {
                middleAgent.LoadComplete();
            }
        }

        private bool NextTransCmdIsMove()
        {
            return transCmds[transCmdsIndex + 1].GetType() == EnumTransCmdType.Move;
        }

        private void OnUnloadFinishedEvent()
        {
            if (IsLoadUnloadComplete())
            {
                middleAgent.LoadUnloadComplete();
            }
            else
            {
                middleAgent.UnloadComplete();
            }

        }

        private bool IsLoadUnloadComplete()
        {
            return agvcTransCmd.CmdType == EnumAgvcTransCmdType.LoadUnload;
        }

        private void OnLoadunloadFinishedEvent()
        {
            middleAgent.LoadUnloadComplete();
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
            middleAgent.Send_Cmd131_TransferResponse(20, 1, "SomeReason");
        }

        public MiddleAgent GetMiddleAgent()
        {
            return middleAgent;
        }

        public MapHandler GetMapHandler()
        {
            return mapHandler;
        }
    }
}
