﻿using Mirle.Agv.Controller.Tools;
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

namespace Mirle.Agv.Controller
{
    public class MainFlowHandler : ICmdFinished
    {
        #region Configs

        private string rootDir = Environment.CurrentDirectory;
        private string configPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
        private ConfigHandler configHandler;
        private MiddlerConfigs middlerConfigs;
        private Sr2000Configs sr2000Configs;
        private MainFlowConfigs mainFlowConfigs;
        private MapConfigs mapConfigs;
        private MoveControlConfigs moveControlConfigs;
        private BatteryConfigs batteryConfigs;

        #endregion

        #region TransCmds

        private List<TransCmd> transCmds = new List<TransCmd>();

        private List<TransCmd> lastTransCmds = new List<TransCmd>();

        private ConcurrentQueue<MapSection> queNeedReserveSections = new ConcurrentQueue<MapSection>();
        private ConcurrentQueue<MapSection> queGotReserveOkSections = new ConcurrentQueue<MapSection>();

        public bool GoNextTransferStep { get; set; }
        public int TransCmdsIndex { get; set; }

        public bool IsReportingPosition { get; set; }

        private ITransCmdsStep transferCommandStep;

        //private List<MapSection> movingSections = new List<MapSection>();
        private AgvcTransCmd agvcTransCmd;
        //public int MovingSectionsIndex { get; set; } = 0;

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

        private Thread thdGetNewAgvcTransferCommand;
        private Thread thdAskReserve;
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);

        #endregion

        #region Events

        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;
        public event EventHandler<MoveCmdInfo> OnTransferMoveEvent;
        public event EventHandler<List<MapPosition>> OnReserveOkEvent;
        public event EventHandler<string> OnAgvcTransferCommandCheckedEvent;

        #endregion

        #region Alarms

        public Dictionary<int, Alarm> allAlarms = new Dictionary<int, Alarm>();
        public List<Alarm> happeningAlarms = new List<Alarm>();
        public List<Alarm> historyAlarms = new List<Alarm>();

        #endregion

        public Vehicle theVehicle;
        private bool isIniOk;
        private MapInfo theMapInfo = new MapInfo();

        public MainFlowHandler()
        {
            isIniOk = true;
            rootDir = Environment.CurrentDirectory;
            //InitialMainFlowHandler();

            //RunThreads();
        }

        public MainFlowHandler(string rootDir)
        {
            isIniOk = true;
            this.rootDir = rootDir;
            //InitialMainFlowHandler();

            //RunThreads();
        }

        public void InitialMainFlowHandler()
        {
            ConfigsInitial();
            LoggersInitial();
            ControllersInitial();
            VehicleInitial();
            LoadAllAlarms();
            EventInitial();
            ThreadInitial();

            if (isIniOk)
            {
                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "全部"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }
            }
        }

        private void LoadAllAlarms()
        {
            //TODO: load all alarms
            //throw new NotImplementedException();
        }

        private void ThreadInitial()
        {
            thdGetNewAgvcTransferCommand = new Thread(new ThreadStart(VisitTransCmds));
            thdGetNewAgvcTransferCommand.IsBackground = true;

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
                int.TryParse(configHandler.GetString("Middler", "RichTextBoxMaxLines ", "10"), out int tempRichTextBoxMaxLines);
                middlerConfigs.RichTextBoxMaxLines = tempRichTextBoxMaxLines;

                mapConfigs = new MapConfigs();
                //mapConfigs.RootDir = configHandler.GetString("Map", "RootDir", Environment.CurrentDirectory);
                mapConfigs.RootDir = Environment.CurrentDirectory;
                mapConfigs.SectionFileName = configHandler.GetString("Map", "SectionFileName", "ASECTION.csv");
                mapConfigs.AddressFileName = configHandler.GetString("Map", "AddressFileName", "AADDRESS.csv");
                mapConfigs.BarcodeFileName = configHandler.GetString("Map", "BarcodeFileName", "LBARCODE.csv");
                mapConfigs.OutSectionThreshold = float.Parse(configHandler.GetString("Map", "OutSectionThreshold", "10"));

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

                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "讀寫設定檔"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }
            }
            catch (Exception)
            {
                isIniOk = false;
                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = false,
                        ItemName = "讀寫設定檔"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }
            }
        }

        private void LoggersInitial()
        {
            try
            {
                loggerAgent = LoggerAgent.Instance;

                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "Logger"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }

            }
            catch (Exception)
            {
                isIniOk = false;
                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = false,
                        ItemName = "Logger"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }

            }
        }

        private void ControllersInitial()
        {
            try
            {
                mapHandler = new MapHandler(mapConfigs);
                theMapInfo = mapHandler.GetMapInfo();

                batteryHandler = new BatteryHandler();
                coupleHandler = new CoupleHandler();
                moveControlHandler = new MoveControlHandler(moveControlConfigs, sr2000Configs, theMapInfo);
                robotControlHandler = new RobotControlHandler();


                bmsAgent = new BmsAgent();
                elmoAgent = new ElmoAgent();
                middleAgent = new MiddleAgent(middlerConfigs, theMapInfo);
                plcAgent = new PlcAgent();

                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "Controller"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }

            }
            catch (Exception ex)
            {
                var temp = ex.StackTrace;
                isIniOk = false;
                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = false,
                        ItemName = "Agent"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }
            }
        }

        private void VehicleInitial()
        {
            try
            {
                theVehicle = Vehicle.Instance;
                theVehicle.SetMapInfo(theMapInfo);
                theVehicle.SetupBattery(batteryConfigs);

                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "Vehicle"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }
            }
            catch (Exception)
            {
                isIniOk = false;
                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = false,
                        ItemName = "Vehicle"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }
            }

        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'mapHandler
                middleAgent.OnInstallTransferCommandEvent += MiddleAgent_OnInstallTransferCommandEvent;
                //middleAgent.OnInstallTransferCommandEvent += mapHandler.OnInstallTransferCommand;
                //OnTransferMoveEvent += moveControlHandler.MainFlow_OnTransferMoveEven;

                //來自middleAgent的NewTransCmds訊息，通知MainFlow(this)'mapHandler
                middleAgent.OnTransferCancelEvent += OnMiddlerGetsCancelEvent;
                //middleAgent.OnTransferCancelEvent += mapHandler.OnMiddlerGetsCancelEvent;

                middleAgent.OnTransferAbortEvent += OnMiddlerGetsAbortEvent;

                //來自MoveControl的Barcode更新訊息，通知MainFlow(this)'middleAgent'mapHandler
                //moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += OnMapBarcodeValuesChangedEvent;
                //moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += middleAgent.OnMapBarcodeValuesChangedEvent;
                //moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += mapHandler.OnMapBarcodeValuesChangedEvent;
                //moveControlHandler.sr2000Agent.OnMapBarcodeValuesChange += moveControlHandler.OnMapBarcodeValuesChangedEvent;

                middleAgent.OnGetReserveOkEvent += MiddleAgent_OnGetReserveOkEvent;
                middleAgent.OnGetBlockPassEvent += MiddleAgent_OnGetBlockPassEvent;

                //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                moveControlHandler.OnMoveFinished += MoveControlHandler_OnMoveFinished;
                //moveControlHandler.OnMoveFinished += mapHandler.OnTransCmdsFinishedEvent;

                //來自RobotControl的取貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                robotControlHandler.OnLoadFinished += RobotControlHandler_OnLoadFinished;
                //robotControlHandler.OnLoadFinished += mapHandler.OnTransCmdsFinishedEvent;

                //來自RobotControl的放貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                robotControlHandler.OnUnloadFinished += RobotControlHandler_OnUnloadFinished;
                //robotControlHandler.OnUnloadFinished += mapHandler.OnTransCmdsFinishedEvent;



                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = true,
                        ItemName = "事件"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }

            }
            catch (Exception)
            {
                isIniOk = false;

                if (OnComponentIntialDoneEvent != null)
                {
                    var args = new InitialEventArgs
                    {
                        IsOk = false,
                        ItemName = "事件"
                    };
                    OnComponentIntialDoneEvent(this, args);
                }
            }
        }

        public MapInfo GetMapInfo()
        {
            return theMapInfo;
        }

        private void MiddleAgent_OnGetBlockPassEvent(object sender, bool e)
        {
            throw new NotImplementedException();
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

        private void MiddleAgent_OnInstallTransferCommandEvent(object sender, AgvcTransCmd agvcTransCmd)
        {
            try
            {
                this.agvcTransCmd = agvcTransCmd;
                if (!CheckTransCmdSectionsAndAddressesMatch(agvcTransCmd))
                {
                    SendAgvcTransferCommandChecked(agvcTransCmd, false);
                    return;
                }

                SendAgvcTransferCommandChecked(agvcTransCmd, true);
                AgvcTransferCommandIntoTransferSteps();
                transCmds.Add(new EmptyTransCmd());

                //開始尋訪 transCmds as List<TransCmd> 裡的每一步MoveCmdInfo/LoadCmdInfo
                thdGetNewAgvcTransferCommand.Start();


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

        private void SendAgvcTransferCommandChecked(AgvcTransCmd agvcTransCmd, bool isOk)
        {
            string fullMsg = Environment.NewLine;
            PropertyInfo[] infos = agvcTransCmd.GetType().GetProperties();
            foreach (var info in infos)
            {
                if (info.CanWrite)
                {
                    if (info.PropertyType == typeof(string[]))
                    {
                        var name = info.Name;
                        string arrayMsg = "";
                        string[] array1 = (string[])info.GetValue(agvcTransCmd);
                        for (int i = 0; i < array1.Length; i++)
                        {
                            arrayMsg += array1[i] + " ";
                        }

                        fullMsg += $"[{name}={arrayMsg}]" + Environment.NewLine;

                    }
                    else
                    {
                        var name = info.Name;
                        var value = info.GetValue(agvcTransCmd);
                        fullMsg += $"[{name}={value}]" + Environment.NewLine;

                    }
                }
            }

            OnAgvcTransferCommandCheckedEvent?.Invoke(this, fullMsg);
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
                if (!theMapInfo.allMapSections.ContainsKey(sections[i]))
                {
                    int replyCode = 1; // NG
                    string reason = $"{sections[i]} is not in the map.";
                    middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                    return false;
                }

                var tempSection = theMapInfo.allMapSections[sections[i]];

                if (!theMapInfo.allMapAddresses.ContainsKey(addresses[i]))
                {
                    int replyCode = 1; // NG
                    string reason = $"{addresses[i]} is not in the map.";
                    middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                    return false;
                }

                if (!theMapInfo.allMapAddresses.ContainsKey(addresses[i + 1]))
                {
                    int replyCode = 1; // NG
                    string reason = $"{addresses[i + 1]} is not in the map.";
                    middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                    return false;
                }

                if (tempSection.HeadAddress.Id == addresses[i])
                {
                    if (tempSection.TailAddress.Id != addresses[i + 1])
                    {
                        int replyCode = 1; // NG
                        string reason = $"guildSections and guildAddresses is not match";
                        middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                        return false;
                    }
                }
                else if (tempSection.TailAddress.Id == addresses[i])
                {
                    if (tempSection.HeadAddress.Id != addresses[i + 1])
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
            switch (agvcTransCmd.CmdType)
            {
                case EnumAgvcTransCmdType.Move:
                case EnumAgvcTransCmdType.Load:
                case EnumAgvcTransCmdType.Unload:
                case EnumAgvcTransCmdType.LoadUnload:
                    return true;
                case EnumAgvcTransCmdType.Home:
                case EnumAgvcTransCmdType.Override:
                case EnumAgvcTransCmdType.Else:
                default:
                    return false;
            }
        }

        private void AgvcTransferCommandIntoTransferSteps()
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

            //var tempTransCmds = new List<TransCmd>();
            //for (int i = 0; i < transCmds.Count; i++)
            //{

            //}

            //var curSection = theVehicle.GetVehLoacation().Section.Id;
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
                MoveCmdInfo moveCmd = SetMoveToUnloadCmdInfo(agvcTransCmd);
                transCmds.Add(moveCmd);
            }

            UnloadCmdInfo unloadCmd = new UnloadCmdInfo();
            unloadCmd.CstId = agvcTransCmd.CarrierId;
            unloadCmd.CmdId = agvcTransCmd.CmdId;
            unloadCmd.UnloadAddress = agvcTransCmd.UnloadAddtess;

            transCmds.Add(unloadCmd);
        }

        private MoveCmdInfo SetMoveToUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo();
            moveCmd.CmdId = agvcTransCmd.CmdId;
            moveCmd.CstId = agvcTransCmd.CarrierId;
            moveCmd.AddressIds = moveCmd.SetListIds(agvcTransCmd.ToUnloadAddresses);
            moveCmd.SectionIds = moveCmd.SetListIds(agvcTransCmd.ToUnloadSections);
            moveCmd.SetAddressPositions();
            moveCmd.SetAddressActions();
            moveCmd.SetSectionSpeedLimits();
            moveCmd.SetMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            return moveCmd;
        }

        private void ConvertAgvcLoadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToLoadSections.Length > 0)
            {
                MoveCmdInfo moveCmd = SetMoveToLoadCmdInfo(agvcTransCmd);
                transCmds.Add(moveCmd);
            }

            LoadCmdInfo loadCmd = new LoadCmdInfo();
            loadCmd.CstId = agvcTransCmd.CarrierId;
            loadCmd.CmdId = agvcTransCmd.CmdId;
            loadCmd.LoadAddress = agvcTransCmd.LoadAddress;

            transCmds.Add(loadCmd);
        }

        private MoveCmdInfo SetMoveToLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo();
            moveCmd.CmdId = agvcTransCmd.CmdId;
            moveCmd.CstId = agvcTransCmd.CarrierId;
            moveCmd.AddressIds = moveCmd.SetListIds(agvcTransCmd.ToLoadAddresses);
            moveCmd.SectionIds = moveCmd.SetListIds(agvcTransCmd.ToLoadSections);
            moveCmd.SetAddressPositions();
            moveCmd.SetAddressActions();
            moveCmd.SetSectionSpeedLimits();
            moveCmd.SetMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            return moveCmd;
        }

        private void ConvertAgvcMoveCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToUnloadSections.Length > 0)
            {
                MoveCmdInfo moveCmd = SetMoveToUnloadCmdInfo(agvcTransCmd);
                transCmds.Add(moveCmd);
            }
        }

        private void VisitTransCmds()
        {
            PreVisitTransCmds();

            while (TransCmdsIndex < transCmds.Count)
            {
                #region Pause And Stop Check

                PauseEvent.WaitOne(Timeout.Infinite);
                if (ShutdownEvent.WaitOne(0))
                {
                    break;
                }

                #endregion

                if (GoNextTransferStep)
                {
                    GoNextTransferStep = false;
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
            TransCmdsIndex = 0;
            GoNextTransferStep = false;
            SetTransCmdsStep(new Idle());
        }

        private void PreVisitTransCmds()
        {
            TransCmdsIndex = 0;
            GoNextTransferStep = true;
            PauseEvent.Set();
            ShutdownEvent.Reset();
        }

        private bool IsQueGotReserveOkSectionsFull()
        {
            return queGotReserveOkSections.Count >= mainFlowConfigs.ReserveLength;
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
            while (GetCurTransCmd().GetCommandType() == EnumTransCmdType.Move)
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
                    middleAgent.AskAgvcReserveSections(queNeedReserveSections);
                }

                SpinWait.SpinUntil(() => false, mainFlowConfigs.AskReserveInterval);
            }
        }

        private void MiddleAgent_OnGetReserveOkEvent(object sender, bool e)
        {
            if (e)
            {
                queNeedReserveSections.TryDequeue(out MapSection reserveOkSection);
                queGotReserveOkSections.Enqueue(reserveOkSection);
                PublishReserveOkEvent();
            }
        }


        private void PublishReserveOkEvent()
        {
            if (queGotReserveOkSections.Count < 1)
            {
                return;
            }
            List<MapPosition> reserveOkPositions = new List<MapPosition>();
            MapSection[] reserveOkSections = queGotReserveOkSections.ToArray();
            for (int i = 0; i < reserveOkSections.Length; i++)
            {
                MapSection mapSection = reserveOkSections[i];
                MapAddress mapAddress = new MapAddress();
                if (mapSection.CmdDirection == EnumPermitDirection.Backward)
                {
                    mapAddress = mapSection.TailAddress.DeepClone();
                }
                else
                {
                    mapAddress = mapSection.HeadAddress.DeepClone();
                }
                MapPosition mapPosition = new MapPosition(mapAddress.Position.X, mapAddress.Position.Y);
                reserveOkPositions.Add(mapPosition);
            }

            OnReserveOkEvent?.Invoke(this, reserveOkPositions);
        }

        private bool CanAskReserve()
        {
            return CanVehMove() && !IsQueGotReserveOkSectionsFull() && IsQueNeedReserveSectionsNotEmpty();
        }

        private bool IsQueNeedReserveSectionsNotEmpty()
        {
            return !queNeedReserveSections.IsEmpty;
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
            if (thdGetNewAgvcTransferCommand.IsAlive)
            {
                thdGetNewAgvcTransferCommand.Join();
            }
            if (thdAskReserve.IsAlive)
            {
                thdAskReserve.Join();
            }
            SetTransCmdsStep(new Idle());
        }

        public void MoveControlHandler_OnMoveFinished(object sender, EnumCompleteStatus status)
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

        private void RobotControlHandler_OnUnloadFinished(object sender, EnumCompleteStatus e)
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

        private void RobotControlHandler_OnLoadFinished(object sender, EnumCompleteStatus e)
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

        private bool NextTransCmdIsUnload()
        {
            return transCmds[TransCmdsIndex + 1].GetCommandType() == EnumTransCmdType.Unload;
        }

        private bool NextTransCmdIsLoad()
        {
            return transCmds[TransCmdsIndex + 1].GetCommandType() == EnumTransCmdType.Load;
        }

        private bool NextTransCmdIsMove()
        {
            return transCmds[TransCmdsIndex + 1].GetCommandType() == EnumTransCmdType.Move;
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
            if (TransCmdsIndex < transCmds.Count)
            {
                TransCmdsIndex++;
                GoNextTransferStep = true;
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
            TransCmd transCmd = new EmptyTransCmd(theMapInfo);
            if (TransCmdsIndex < transCmds.Count)
            {
                transCmd = transCmds[TransCmdsIndex];
            }
            return transCmd;
        }

        public TransCmd GetNextTransCmd()
        {
            TransCmd transCmd = new EmptyTransCmd(theMapInfo);
            int nextIndex = TransCmdsIndex + 1;
            if (nextIndex < transCmds.Count)
            {
                transCmd = transCmds[nextIndex];
            }
            return transCmd;
        }

        public void SetTransCmdsStep(ITransCmdsStep step)
        {
            this.transferCommandStep = step;
        }

        public void DoTransfer()
        {
            transferCommandStep.DoTransfer(this);
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

        public void ReconnectToAgvc()
        {
            middleAgent.ReConnect();
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

        public MiddlerConfigs GetMiddlerConfigs()
        {
            return middlerConfigs;
        }

        public void PublishTransferMoveEvent(MoveCmdInfo moveCmd)
        {
            OnTransferMoveEvent?.Invoke(this, moveCmd);
        }

        public void StartAskingReserve(MoveCmdInfo moveCmd)
        {
            FitPrepareForReserveSections(moveCmd);
            thdAskReserve.Start();
        }

        private void FitPrepareForReserveSections(MoveCmdInfo moveCmd)
        {
            queNeedReserveSections = new ConcurrentQueue<MapSection>();
            for (int i = 0; i < moveCmd.MovingSections.Count; i++)
            {
                MapSection section = moveCmd.MovingSections[i];
                queNeedReserveSections.Enqueue(section);
            }
        }

        public void ReportVehiclePosition(MapPosition gxPosition)
        {
            VehLocation theVehicleLocation = theVehicle.GetVehLoacation();
            theVehicleLocation.EncoderGxPosition = gxPosition;

            TransCmd curTransCmd = GetCurTransCmd();
            if (curTransCmd.GetCommandType() == EnumTransCmdType.Move)
            {
                MoveCmdInfoUpdatePosition((MoveCmdInfo)curTransCmd, gxPosition);
            }
            middleAgent.Send_Cmd134_TransferEventReport();


        }

        private void MoveCmdInfoUpdatePosition(MoveCmdInfo curTransCmd, MapPosition gxPosition)
        {
            List<MapSection> movingSections = curTransCmd.MovingSections;
            int movingSectionIndex = curTransCmd.MovingSectionsIndex;
            if (movingSectionIndex + 1 == movingSections.Count)
            {
                //vehicle is in the last section of this moveCmdInfo.
                MapSection currentSection = movingSections[movingSectionIndex];
                MapAddress headAdr = currentSection.HeadAddress;
                MapAddress tailAdr = currentSection.TailAddress;
                switch (currentSection.Type)
                {
                    case EnumSectionType.Horizontal:
                        {
                            if (IsOutsideSection(gxPosition.Y, headAdr.Position.Y))
                            {
                                //TODO:
                                //Stop the vehicle.
                                //Send alarm to agvc.
                                return;
                            }

                            float distance = 0;
                            if (currentSection.CmdDirection == EnumPermitDirection.Backward)
                            {
                                distance = tailAdr.Position.X - gxPosition.X;
                            }
                            else
                            {
                                distance = gxPosition.X - headAdr.Position.X;
                            }

                            var location = theVehicle.GetVehLoacation();

                            if (distance > 0.95 * currentSection.Distance) //0.95 can config
                            {
                                //算是進入終點區間
                                //TODO: 
                                //二次定位
                                EnumTransCmdType type = transCmds[TransCmdsIndex + 1].GetCommandType();
                                if (type == EnumTransCmdType.Load)
                                {
                                    middleAgent.Send_Cmd136_TransferEventReport(EventType.LoadArrivals);
                                }
                                else if (type == EnumTransCmdType.Unload)
                                {
                                    middleAgent.Send_Cmd136_TransferEventReport(EventType.UnloadArrivals);
                                }
                                else
                                {
                                    middleAgent.Send_Cmd136_TransferEventReport(EventType.AdrOrMoveArrivals);
                                }


                            }
                            else
                            {
                                //還未到終點
                                location.Section.Distance = distance;
                                middleAgent.Send_Cmd134_TransferEventReport();
                            }

                        }
                        break;
                    case EnumSectionType.Vertical:
                        break;
                    case EnumSectionType.R2000:
                        break;
                    case EnumSectionType.None:
                    default:
                        break;
                }


            }
            else
            {
                MapSection currentSection = movingSections[movingSectionIndex];
                MapSection nextSection = movingSections[movingSectionIndex + 1];
            }

        }

        private bool IsOutsideSection(float num1, float num2)
        {
            return Math.Abs(num1 - num2) > mapConfigs.OutSectionThreshold;
        }

        public MapBarcode GetMapBarcode(int baracodeNum)
        {
            var dicBarcodes = theMapInfo.allBarcodes;
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

        public void FakeCmdTest()
        {
            AgvcTransCmd fakeCmd = new AgvcTransCmd();
            fakeCmd.CmdId = "Cmd101";
            fakeCmd.CarrierId = "Cst101";
            fakeCmd.CmdType = EnumAgvcTransCmdType.Move;
            fakeCmd.LoadAddress = "Adr103";
            fakeCmd.ToLoadSections = new string[] { "Sec101", "Sec102" };
            fakeCmd.ToLoadAddresses = new string[] { "Adr101", "Adr102", "Adr103" };
            fakeCmd.UnloadAddtess = "Adr203";
            fakeCmd.ToUnloadSections = new string[] { "Sec201", "Sec202" };
            fakeCmd.ToUnloadAddresses = new string[] { "Adr201", "Adr202", "Adr203" };

            SendAgvcTransferCommandChecked(fakeCmd, true);
        }

        public void LogAlarm(Alarm alarm)
        {
            if (loggerAgent == null)
            {
                return;
            }

            loggerAgent.LogAlarm(alarm);
        }

        public void AlarmSet(int aAlarmId)
        {
            if (allAlarms.Count < 1)
            {
                string className = GetType().Name;
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name; //sender.ToString();
                string classMethodName = className + ":" + methodName;
                LogFormat logFormat = new LogFormat("Error", "3", classMethodName, "Device", "CarrierID", $"Allalarms is empty");
                loggerAgent.LogMsg("Error", logFormat);

                return;
            }

            if (!allAlarms.ContainsKey(aAlarmId))
            {
                string className = GetType().Name;
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name; //sender.ToString();
                string classMethodName = className + ":" + methodName;
                LogFormat logFormat = new LogFormat("Error", "3", classMethodName, "Device", "CarrierID", $"No such alarmId({aAlarmId})");
                loggerAgent.LogMsg("Error", logFormat);

                return;
            }

            DateTime alarmSetTime = DateTime.Now;
            Alarm alarm = allAlarms[aAlarmId].DeepClone();
            alarm.SetTime = alarmSetTime;
            //通知AGVC

            //通知PLC

            //紀錄
            loggerAgent.LogAlarm(alarm);
        }
    }
}
