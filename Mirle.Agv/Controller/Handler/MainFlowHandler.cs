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

namespace Mirle.Agv.Controller
{
    public class MainFlowHandler
    {
        #region Configs

        private string rootDir = Environment.CurrentDirectory;
        private string configPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
        private ConfigHandler configHandler;
        private MiddlerConfig middlerConfig;
        private MainFlowConfig mainFlowConfig;
        private MapConfig mapConfig;
        private AlarmConfig alarmConfig;

        #endregion

        #region TransCmds

        private List<TransferStep> transferSteps = new List<TransferStep>();
        private List<TransferStep> lastTransferSteps = new List<TransferStep>();
        private ConcurrentQueue<MapSection> queNeedReserveSections = new ConcurrentQueue<MapSection>();
        private ConcurrentQueue<MapSection> queAskingReserveSections = new ConcurrentQueue<MapSection>();
        // private List<MapSection> gotReserveOkSections = new List<MapSection>();
        private ConcurrentQueue<MapSection> queGotReserveOkSections = new ConcurrentQueue<MapSection>();

        public bool GoNextTransferStep { get; set; }
        public int TransferStepsIndex { get; set; }
        public bool IsReportingPosition { get; set; }
        public bool IsReserveMechanism { get; set; } = true;
        private ITransferCmdStep transferCmdStep;
        private AgvcTransCmd agvcTransCmd;
        private AgvcTransCmd lastAgvcTransCmd;

        #endregion

        #region Agent

        private BmsAgent bmsAgent;
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
        private Thread thdVisitTransCmds;
        private ManualResetEvent visitTransCmdsShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent visitTransCmdsPauseEvent = new ManualResetEvent(true);

        private Thread thdTrackingPosition;
        private ManualResetEvent trackingPositionShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent trackingPositionPauseEvent = new ManualResetEvent(true);
        #endregion

        #region Events
        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;
        public event EventHandler<List<MapPosition>> OnReserveOkEvent;
        public event EventHandler<string> OnAgvcTransferCommandCheckedEvent;
        #endregion

        public Vehicle theVehicle;
        private bool isIniOk;
        private MapInfo theMapInfo = new MapInfo();
        private MCProtocol mcProtocol;
        private ushort forkCommandNumber = 0;

        public MainFlowHandler()
        {
            isIniOk = true;
            rootDir = Environment.CurrentDirectory;
        }

        public MainFlowHandler(string rootDir)
        {
            isIniOk = true;
            this.rootDir = rootDir;
        }

        #region InitialComponents

        public void InitialMainFlowHandler()
        {
            ConfigsInitial();
            LoggersInitial();
            ControllersInitial();
            VehicleInitial();
            LoadAllAlarms();
            EventInitial();
            SetTransCmdsStep(new Idle());
            //StartTrackingPosition();

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

        private void ConfigsInitial()
        {
            try
            {
                configPath = Path.Combine(rootDir, "Configs.ini");
                configHandler = new ConfigHandler(configPath);

                mainFlowConfig = new MainFlowConfig();
                mainFlowConfig.LogConfigPath = configHandler.GetString("MainFlow", "LogConfigPath", "Log.ini");
                LoggerAgent.LogConfigPath = mainFlowConfig.LogConfigPath;
                int.TryParse(configHandler.GetString("MainFlow", "TransCmdsCheckInterval", "15"), out int tempTransCmdsCheckInterval);
                mainFlowConfig.TransCmdsCheckInterval = tempTransCmdsCheckInterval;
                int.TryParse(configHandler.GetString("MainFlow", "DoTransCmdsInterval", "15"), out int tempDoTransCmdsInterval);
                mainFlowConfig.DoTransCmdsInterval = tempDoTransCmdsInterval;
                int.TryParse(configHandler.GetString("MainFlow", "ReserveLength", "3"), out int tempReserveLength);
                mainFlowConfig.ReserveLength = tempReserveLength;
                int.TryParse(configHandler.GetString("MainFlow", "TrackingPositionInterval", "100"), out int tempTrackingPositionInterval);
                mainFlowConfig.TrackingPositionInterval = tempTrackingPositionInterval;
                int.TryParse(configHandler.GetString("MainFlow", "StopChargeInterval", "100"), out int tempStopChargeInterval);
                mainFlowConfig.StopChargeInterval = tempStopChargeInterval;
                int.TryParse(configHandler.GetString("MainFlow", "StopChargeInterval", "100"), out int tempStartChargeInterval);
                mainFlowConfig.StartChargeInterval = tempStartChargeInterval;

                middlerConfig = new MiddlerConfig();
                int.TryParse(configHandler.GetString("Middler", "ClientNum", "1"), out int tempClientNum);
                middlerConfig.ClientNum = tempClientNum;
                middlerConfig.ClientName = configHandler.GetString("Middler", "ClientName", "AGV01");
                middlerConfig.RemoteIp = configHandler.GetString("Middler", "RemoteIp", "192.168.9.203");
                int.TryParse(configHandler.GetString("Middler", "RemotePort", "10001"), out int tempRemotePort);
                middlerConfig.RemotePort = tempRemotePort;
                middlerConfig.LocalIp = configHandler.GetString("Middler", "LocalIp", "192.168.9.131");
                int.TryParse(configHandler.GetString("Middler", "LocalPort", "5002"), out int tempPort);
                middlerConfig.LocalPort = tempPort;
                int.TryParse(configHandler.GetString("Middler", "RecvTimeoutMs", "10000"), out int tempRecvTimeoutMs);
                middlerConfig.RecvTimeoutMs = tempRecvTimeoutMs;
                int.TryParse(configHandler.GetString("Middler", "SendTimeoutMs", "0"), out int tempSendTimeoutMs);
                middlerConfig.SendTimeoutMs = tempSendTimeoutMs;
                int.TryParse(configHandler.GetString("Middler", "MaxReadSize", "0"), out int tempMaxReadSize);
                middlerConfig.MaxReadSize = tempMaxReadSize;
                int.TryParse(configHandler.GetString("Middler", "ReconnectionIntervalMs", "10000"), out int tempReconnectionIntervalMs);
                middlerConfig.ReconnectionIntervalMs = tempReconnectionIntervalMs;
                int.TryParse(configHandler.GetString("Middler", "MaxReconnectionCount", "10"), out int tempMaxReconnectionCount);
                middlerConfig.MaxReconnectionCount = tempMaxReconnectionCount;
                int.TryParse(configHandler.GetString("Middler", "RetryCount", "2"), out int tempRetryCount);
                middlerConfig.RetryCount = tempRetryCount;
                int.TryParse(configHandler.GetString("Middler", "SleepTime", "10"), out int tempSleepTime);
                middlerConfig.SleepTime = tempSleepTime;
                int.TryParse(configHandler.GetString("Middler", "RichTextBoxMaxLines ", "10"), out int tempRichTextBoxMaxLines);
                middlerConfig.RichTextBoxMaxLines = tempRichTextBoxMaxLines;
                int.TryParse(configHandler.GetString("Middler", "AskReserveInterval ", "1000"), out int tempAskReserveInterval);
                middlerConfig.AskReserveInterval = tempAskReserveInterval;

                mapConfig = new MapConfig();
                mapConfig.SectionFileName = configHandler.GetString("Map", "SectionFileName", "ASECTION.csv");
                mapConfig.AddressFileName = configHandler.GetString("Map", "AddressFileName", "AADDRESS.csv");
                mapConfig.BarcodeFileName = configHandler.GetString("Map", "BarcodeFileName", "LBARCODE.csv");
                mapConfig.OutSectionThreshold = float.Parse(configHandler.GetString("Map", "OutSectionThreshold", "10"));

                alarmConfig = new AlarmConfig();
                alarmConfig.AlarmFileName = configHandler.GetString("Alarm", "AlarmFileName", "AlarmCode.csv");

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

        public void StopVehicle()
        {
            moveControlHandler.StopFlagOn();
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
                mapHandler = new MapHandler(mapConfig);
                theMapInfo = mapHandler.GetMapInfo();

                moveControlHandler = new MoveControlHandler("", theMapInfo);
                alarmHandler = new AlarmHandler(alarmConfig);

                bmsAgent = new BmsAgent();
                middleAgent = new MiddleAgent(middlerConfig, theMapInfo);
                mcProtocol = new MCProtocol();
                mcProtocol.Name = "MCProtocol";
                plcAgent = new PlcAgent(mcProtocol, alarmHandler);

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
                        ItemName = "Controller"
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

                //來自MiddleAgent的取得Reserve/BlockZone訊息，通知MainFlow(this)
                middleAgent.OnGetReserveOkEvent += MiddleAgent_OnGetReserveOkEvent;
                middleAgent.OnGetBlockPassEvent += MiddleAgent_OnGetBlockPassEvent;

                //來自MoveControl的移動結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                moveControlHandler.OnMoveFinished += MoveControlHandler_OnMoveFinished;

                //來自PlcAgent的取放貨結束訊息，通知MainFlow(this)'middleAgent'mapHandler
                plcAgent.OnForkCommandFinishEvent += PlcAgent_OnForkCommandFinishEvent;

                //來自PlcBattery的電量改變訊息，通知middleAgent
                plcAgent.OnBatteryPercentageChangeEvent += middleAgent.PlcAgent_OnBatteryPercentageChangeEvent;

                //來自PlcBattery的CassetteId讀取訊息，通知middleAgent
                plcAgent.OnCassetteIDReadFinishEvent += middleAgent.PlcAgent_OnCassetteIDReadFinishEvent;

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

        private void LoadAllAlarms()
        {
            //TODO: load all alarms
            //throw new NotImplementedException();
        }

        public MapInfo GetMapInfo()
        {
            return theMapInfo;
        }

        #endregion

        private void MiddleAgent_OnGetBlockPassEvent(object sender, bool e)
        {
            //throw new NotImplementedException();
        }

        private void OnMiddlerGetsAbortEvent(object sender, string e)
        {
            theVehicle.GetTransCmd().CmdId = e;
            OnAbortEvent();
        }

        private void OnMiddlerGetsCancelEvent(object sender, string e)
        {
            theVehicle.GetTransCmd().CmdId = e;
            OnCancelEvent();
        }

        private void MiddleAgent_OnInstallTransferCommandEvent(object sender, AgvcTransCmd agvcTransCmd)
        {
            //TODO:
            if (agvcTransCmd != null)
            {
                middleAgent.Send_Cmd131_TransferResponse(agvcTransCmd.SeqNum, 1, "Agv already have transfer command.");
                return;
            }

            try
            {
                this.agvcTransCmd = agvcTransCmd;
                theVehicle.SetAgvcTransCmd(agvcTransCmd);
                if (!CheckTransCmdSectionsAndAddressesMatch(agvcTransCmd))
                {
                    PublishAgvcTransferCommandChecked(agvcTransCmd, false);
                    return;
                }

                middleAgent.Send_Cmd131_TransferResponse(agvcTransCmd.SeqNum, 0, " ");
                PublishAgvcTransferCommandChecked(agvcTransCmd, true);
                theVehicle.SetAgvcTransCmd(agvcTransCmd);
                SetupTransferSteps();

                //開始尋訪 transCmds as List<TransCmd> 裡的每一步MoveCmdInfo/LoadCmdInfo
                RestartVisitTransCmds();

            }
            catch (Exception ex)
            {
                string className = GetType().Name;
                string methodName = MethodBase.GetCurrentMethod().Name;
                string classMethodName = className + ":" + methodName;
                LogFormat logFormat = new LogFormat("Error", "1", classMethodName, "Device", "CarrierID", ex.StackTrace);
                loggerAgent.LogMsg("Error", logFormat);
            }
        }

        private void PublishAgvcTransferCommandChecked(AgvcTransCmd agvcTransCmd, bool isOk)
        {
            string fullMsg = Environment.NewLine;
            PropertyInfo[] infos = agvcTransCmd.GetType().GetProperties();
            foreach (var info in infos)
            {
                if (info.CanWrite)
                {
                    if (info.PropertyType == typeof(List<string>))
                    {
                        var name = info.Name;
                        List<string> aList = (List<string>)info.GetValue(agvcTransCmd);
                        string values = "";
                        for (int i = 0; i < aList.Count; i++)
                        {
                            values += aList[i] + " ";
                        }

                        fullMsg += $"[{name}={values}]" + Environment.NewLine;

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
            switch (agvcTransCmd.CommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToUnloadSections, agvcTransCmd.ToUnloadAddresses, agvcTransCmd.SeqNum);
                case EnumAgvcTransCommandType.Load:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToLoadSections, agvcTransCmd.ToLoadAddresses, agvcTransCmd.SeqNum);
                case EnumAgvcTransCommandType.Unload:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToUnloadSections, agvcTransCmd.ToUnloadAddresses, agvcTransCmd.SeqNum);
                case EnumAgvcTransCommandType.LoadUnload:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToLoadSections, agvcTransCmd.ToLoadAddresses, agvcTransCmd.SeqNum) || IsSectionsAndAddressesMatch(agvcTransCmd.ToUnloadSections, agvcTransCmd.ToUnloadAddresses, agvcTransCmd.SeqNum);
                default:
                    return true;
            }
        }

        private bool IsSectionsAndAddressesMatch(List<string> sections, List<string> addresses, ushort aSeqNum)
        {
            if (sections.Count + 1 != addresses.Count)
            {
                int replyCode = 1; // NG
                string reason = $"guildSections and guildAddresses is not match";
                middleAgent.Send_Cmd131_TransferResponse(aSeqNum, replyCode, reason);
                return false;
            }

            for (int i = 0; i < sections.Count; i++)
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

        private bool IsBasicTransCmds()
        {
            switch (agvcTransCmd.CommandType)
            {
                case EnumAgvcTransCommandType.Move:
                case EnumAgvcTransCommandType.Load:
                case EnumAgvcTransCommandType.Unload:
                case EnumAgvcTransCommandType.LoadUnload:
                    return true;
                case EnumAgvcTransCommandType.Home:
                case EnumAgvcTransCommandType.Override:
                case EnumAgvcTransCommandType.Else:
                default:
                    return false;
            }
        }

        private void SetupTransferSteps()
        {
            transferSteps.Clear();

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
            //throw new NotImplementedException();
        }

        private void ConvertAgvcLoadUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            ConvertAgvcLoadCmdIntoList(agvcTransCmd);
            ConvertAgvcUnloadCmdIntoList(agvcTransCmd);
        }

        private void ConvertAgvcUnloadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToUnloadSections.Count > 0)
            {
                MoveCmdInfo moveCmd = SetMoveToUnloadCmdInfo(agvcTransCmd);
                transferSteps.Add(moveCmd);
            }

            UnloadCmdInfo unloadCmd = new UnloadCmdInfo();
            unloadCmd.CstId = agvcTransCmd.CassetteId;
            unloadCmd.CmdId = agvcTransCmd.CommandId;
            unloadCmd.UnloadAddress = agvcTransCmd.UnloadAddress;
            MapAddress mapAddress = theMapInfo.allMapAddresses[unloadCmd.UnloadAddress];
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
            transferSteps.Add(unloadCmd);
        }

        private MoveCmdInfo SetMoveToUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(theMapInfo);
            moveCmd.CmdId = agvcTransCmd.CommandId;
            moveCmd.CstId = agvcTransCmd.CassetteId;
            moveCmd.AddressIds = agvcTransCmd.ToUnloadAddresses;
            moveCmd.SectionIds = agvcTransCmd.ToUnloadSections;
            moveCmd.SetAddressPositions();
            moveCmd.SetAddressActions();
            moveCmd.SetSectionSpeedLimits();
            moveCmd.SetMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            return moveCmd;
        }

        private void ConvertAgvcLoadCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToLoadSections.Count > 0)
            {
                MoveCmdInfo moveCmd = SetMoveToLoadCmdInfo(agvcTransCmd);
                transferSteps.Add(moveCmd);
            }

            LoadCmdInfo loadCmd = new LoadCmdInfo();
            loadCmd.CstId = agvcTransCmd.CassetteId;
            loadCmd.CmdId = agvcTransCmd.CommandId;
            loadCmd.LoadAddress = agvcTransCmd.LoadAddress;
            MapAddress mapAddress = theMapInfo.allMapAddresses[loadCmd.LoadAddress];
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

            transferSteps.Add(loadCmd);
        }

        private MoveCmdInfo SetMoveToLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(theMapInfo);
            moveCmd.CmdId = agvcTransCmd.CommandId;
            moveCmd.CstId = agvcTransCmd.CassetteId;
            moveCmd.AddressIds = agvcTransCmd.ToLoadAddresses;
            moveCmd.SectionIds = agvcTransCmd.ToLoadSections;
            moveCmd.SetAddressPositions();
            moveCmd.SetAddressActions();
            moveCmd.SetSectionSpeedLimits();
            moveCmd.SetMovingSections();
            moveCmd.MovingSectionsIndex = 0;
            return moveCmd;
        }

        private void ConvertAgvcMoveCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToUnloadSections.Count > 0)
            {
                MoveCmdInfo moveCmd = SetMoveToUnloadCmdInfo(agvcTransCmd);
                transferSteps.Add(moveCmd);
            }
        }

        private void VisitTransCmds()
        {
            PreVisitTransCmds();

            while (TransferStepsIndex < transferSteps.Count)
            {
                #region Pause And Stop Check

                visitTransCmdsPauseEvent.WaitOne(Timeout.Infinite);
                if (visitTransCmdsShutdownEvent.WaitOne(0))
                {
                    break;
                }

                #endregion

                if (GoNextTransferStep)
                {
                    GoNextTransferStep = false;
                    DoTransfer();
                }

                if (CanAskNextReserveSection())
                {
                    queNeedReserveSections.TryPeek(out MapSection needReserveSection);
                    if (middleAgent.GetNeedReserveSectionId() != needReserveSection.Id)
                    {
                        middleAgent.SetupNeedReserveSections(needReserveSection);
                        middleAgent.StartAskingReserve();
                    }
                }

                SpinWait.SpinUntil(() => false, mainFlowConfig.DoTransCmdsInterval);
            }

            //OnTransCmdsFinishedEvent(this, EnumCompleteStatus.TransferComplete);
            AfterVisitTransCmds();
        }

        public void RestartVisitTransCmds()
        {
            StopVisitTransCmds();
            StartVisitTransCmds();
        }

        public void StartVisitTransCmds()
        {
            visitTransCmdsPauseEvent.Set();
            visitTransCmdsShutdownEvent.Reset();
            thdVisitTransCmds = new Thread(VisitTransCmds);
            thdVisitTransCmds.IsBackground = true;
            thdVisitTransCmds.Start();
        }

        public void PauseVisitTransCmds()
        {
            visitTransCmdsPauseEvent.Reset();
            SetTransCmdsStep(new Idle());
        }

        public void ResumeVisitTransCmds()
        {
            visitTransCmdsPauseEvent.Set();
        }

        public void StopVisitTransCmds()
        {
            visitTransCmdsShutdownEvent.Set();
            visitTransCmdsPauseEvent.Set();

            theVehicle.SetVehicleStop();
            if (thdVisitTransCmds != null && thdVisitTransCmds.IsAlive)
            {
                thdVisitTransCmds.Join();
            }

            AfterVisitTransCmds();

        }

        private void AfterVisitTransCmds()
        {
            lastAgvcTransCmd = agvcTransCmd;
            agvcTransCmd = null;
            theVehicle.SetAgvcTransCmd(new AgvcTransCmd());
            transferSteps.Clear();
            TransferStepsIndex = 0;
            GoNextTransferStep = false;
            SetTransCmdsStep(new Idle());
            theVehicle.ActionStatus = VHActionStatus.NoCommand;
            middleAgent.Send_Cmd144_StatusChangeReport();
        }

        private void PreVisitTransCmds()
        {
            TransferStepsIndex = 0;
            GoNextTransferStep = true;
            visitTransCmdsPauseEvent.Set();
            visitTransCmdsShutdownEvent.Reset();
            theVehicle.ActionStatus = VHActionStatus.Commanding;
            middleAgent.Send_Cmd144_StatusChangeReport();
        }

        private bool CanVehUnload()
        {
            // 判斷當前是否可載貨 若否 則發送報告
            //throw new NotImplementedException();
            return true;
        }

        private bool CanVehLoad()
        {
            // 判斷當前是否可卸貨 若否 則發送報告
            MapPosition position = theVehicle.GetVehLoacation().RealPosition;
            //TODO: Check if it is in position to Load

            return true;
        }

        private bool CanVehMove()
        {
            //battery/emo/beam/etc/reserve
            // 判斷當前是否可移動 若否 則發送報告
            //throw new NotImplementedException();
            return true;
        }

        private bool CanCassetteIdRead()
        {
            // 判斷當前貨物的ID是否可正確讀取 若否 則發送報告
            if (!theVehicle.GetPlcVehicle().Loading)
            {
                //Alarm plcVehicle is not Loading
                return false;
            }
            else if (!string.IsNullOrEmpty(theVehicle.GetPlcVehicle().CassetteId))
            {
                //CassetteId is null or Empty
                return false;
            }
            else if (theVehicle.GetPlcVehicle().CassetteId == "ERROR")
            {
                //CassetteId is Error
                return false;
            }
            else
            {
                return true;
            }
        }

        private void TrackingPosition()
        {
            while (true)
            {
                #region Pause And Stop Check

                trackingPositionPauseEvent.WaitOne(Timeout.Infinite);
                if (trackingPositionShutdownEvent.WaitOne(0))
                {
                    break;
                }

                #endregion

                var position = moveControlHandler.position.Real;

                if (transferSteps.Count > 0)
                {
                    //有搬送命令時，比對當前Position與搬送路徑Sections確定section-distance
                    var curTransCmd = GetCurTransCmd();
                    if (curTransCmd.GetCommandType() == EnumTransCmdType.Move)
                    {
                        MoveCmdInfo moveCmd = (MoveCmdInfo)curTransCmd;
                        MoveCmdInfoUpdatePosition(moveCmd, position);
                    }
                }
                else
                {
                    //無搬送命令時，比對當前Position與全地圖Sections確定section-distance
                    MoveCmdInfoUpdatePosition(position);
                }

                SpinWait.SpinUntil(() => false, mainFlowConfig.DoTransCmdsInterval);
            }

        }

        public void StartTrackingPosition()
        {
            trackingPositionPauseEvent.Set();
            trackingPositionShutdownEvent.Reset();
            thdTrackingPosition = new Thread(TrackingPosition);
            thdTrackingPosition.IsBackground = true;
            thdTrackingPosition.Start();
        }

        public void PauseTrackingPosition()
        {
            trackingPositionPauseEvent.Reset();
        }

        public void ResumeTrackingPosition()
        {
            trackingPositionPauseEvent.Set();
        }

        public void StopTrackingPosition()
        {
            trackingPositionShutdownEvent.Set();
            trackingPositionPauseEvent.Set();

            if (thdTrackingPosition.IsAlive)
            {
                thdTrackingPosition.Join();
            }
        }

        private void MiddleAgent_OnGetReserveOkEvent(object sender, MapSection reserveOkSection)
        {
            queNeedReserveSections.TryPeek(out MapSection needReserveSection);
            if (needReserveSection.Id == reserveOkSection.Id)
            {
                queNeedReserveSections.TryDequeue(out MapSection aReserveOkSection);
                queGotReserveOkSections.Enqueue(aReserveOkSection);
                PublishReserveOkEvent();
            }
        }

        public void MiddleAgent_ResumeAskingReserve()
        {
            middleAgent.ResumeAskingReserve();
        }

        private void PublishReserveOkEvent()
        {
            //if (gotReserveOkSections.Count < 1)
            //{
            //    return;
            //}
            //List<MapPosition> reserveOkPositions = new List<MapPosition>();
            //for (int i = 0; i < gotReserveOkSections.Count; i++)
            //{
            //    MapSection mapSection = gotReserveOkSections[i];
            //    MapAddress mapAddress = new MapAddress();
            //    if (mapSection.CmdDirection == EnumPermitDirection.Backward)
            //    {
            //        mapAddress = mapSection.TailAddress.DeepClone();
            //    }
            //    else
            //    {
            //        mapAddress = mapSection.HeadAddress.DeepClone();
            //    }
            //    MapPosition mapPosition = new MapPosition(mapAddress.Position.X, mapAddress.Position.Y);
            //    reserveOkPositions.Add(mapPosition);
            //}

            //OnReserveOkEvent?.Invoke(this, reserveOkPositions);

            if (queGotReserveOkSections.Count < 1)
            {
                return;
            }
            List<MapPosition> reserveOkPositions = new List<MapPosition>();
            List<MapSection> reserveOkSections = queGotReserveOkSections.ToList();
            for (int i = 0; i < reserveOkSections.Count; i++)
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

        private bool CanAskNextReserveSection()
        {
            return IsMoveStep() && CanVehMove() && !IsGotReserveOkSectionsFull() && IsQueNeedReserveSectionsNotEmpty();
        }

        private bool IsQueGotReserveOkSectionsFull()
        {
            return queGotReserveOkSections.Count >= mainFlowConfig.ReserveLength;
        }

        private bool IsGotReserveOkSectionsFull()
        {
            return queGotReserveOkSections.Count >= mainFlowConfig.ReserveLength;
        }

        private bool IsMoveStep()
        {
            return GetCurTransCmd().GetCommandType() == EnumTransCmdType.Move;
        }

        private bool IsQueNeedReserveSectionsNotEmpty()
        {
            return !queNeedReserveSections.IsEmpty;
        }

        public void MoveControlHandler_OnMoveFinished(object sender, EnumMoveComplete status)
        {
            //TODO: if error stop ,
            if (status == EnumMoveComplete.Fail)
            {
                //Alarm
                StopVisitTransCmds();
                lastTransferSteps = transferSteps;
                transferSteps = new List<TransferStep>();
                return;
            }

            StartCharge();

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
            StopVisitTransCmds();
        }

        private void OnCancelEvent()
        {
            StopVisitTransCmds();
            middleAgent.MainFlowGetCancel();
        }

        private void PlcAgent_OnForkCommandFinishEvent(object sender, PlcForkCommand forkCommand)
        {
            if (forkCommand.ForkCommandType == EnumForkCommand.Load)
            {
                if (!CanCassetteIdRead())
                {
                    return;
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
            else if (forkCommand.ForkCommandType == EnumForkCommand.Unload)
            {
                if (theVehicle.GetPlcVehicle().Loading)
                {
                    //Alarm : loading is still on
                    return;
                }

                if (IsLoadUnloadComplete())
                {
                    middleAgent.LoadUnloadComplete();
                }
                else
                {
                    middleAgent.UnloadComplete();
                }
            }
            else if (forkCommand.ForkCommandType == EnumForkCommand.Home)
            {
                //TODO: RobotHomeComplete
            }
        }

        private bool NextTransCmdIsUnload()
        {
            return transferSteps[TransferStepsIndex + 1].GetCommandType() == EnumTransCmdType.Unload;
        }

        private bool NextTransCmdIsLoad()
        {
            return transferSteps[TransferStepsIndex + 1].GetCommandType() == EnumTransCmdType.Load;
        }

        private bool NextTransCmdIsMove()
        {
            return transferSteps[TransferStepsIndex + 1].GetCommandType() == EnumTransCmdType.Move;
        }

        private bool IsLoadUnloadComplete()
        {
            return agvcTransCmd.CommandType == EnumAgvcTransCommandType.LoadUnload;
        }

        private void OnLoadunloadFinishedEvent()
        {
            middleAgent.LoadUnloadComplete();
        }

        private void VisitNextTransCmd()
        {
            if (TransferStepsIndex == transferSteps.Count - 1)
            {
                //Middler send transfer complete to agvc
            }

            TransferStepsIndex++;
            GoNextTransferStep = true;
        }

        public TransferStep GetCurTransCmd()
        {
            TransferStep transCmd = new EmptyTransCmd(theMapInfo);
            if (TransferStepsIndex < transferSteps.Count)
            {
                transCmd = transferSteps[TransferStepsIndex];
            }
            return transCmd;
        }

        public TransferStep GetNextTransCmd()
        {
            TransferStep transCmd = new EmptyTransCmd(theMapInfo);
            int nextIndex = TransferStepsIndex + 1;
            if (nextIndex < transferSteps.Count)
            {
                transCmd = transferSteps[nextIndex];
            }
            return transCmd;
        }

        public void SetTransCmdsStep(ITransferCmdStep step)
        {
            this.transferCmdStep = step;
        }

        public void DoTransfer()
        {
            transferCmdStep.DoTransfer(this);
        }

        public void Unload(UnloadCmdInfo unloadCmd)
        {
            //Check if it is in position to unload here
            if (CanVehUnload())
            {
                try
                {
                    if (!plcAgent.IsForkCommandExist())
                    {
                        middleAgent.Send_Cmd136_TransferEventReport(EventType.Vhunloading);
                        PlcForkCommand aForkCommand = new PlcForkCommand(forkCommandNumber++, EnumForkCommand.Unload, unloadCmd.StageNum.ToString(), unloadCmd.StageDirection, unloadCmd.IsEqPio, unloadCmd.ForkSpeed);
                        plcAgent.AddForkComand(aForkCommand);
                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {

                    var msg = ex.ToString();
                }

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
                        PlcForkCommand aForkCommand = new PlcForkCommand(forkCommandNumber++, EnumForkCommand.Load, loadCmd.StageNum.ToString(), loadCmd.StageDirection, loadCmd.IsEqPio, loadCmd.ForkSpeed);
                        plcAgent.AddForkComand(aForkCommand);
                    }
                    else
                    {

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

        public MapHandler GetMapHandler()
        {
            return mapHandler;
        }

        public MoveControlHandler GetMoveControlHandler()
        {
            return moveControlHandler;
        }

        public MiddlerConfig GetMiddlerConfigs()
        {
            return middlerConfig;
        }

        public void PublishTransferMoveEvent(MoveCmdInfo moveCmd)
        {
            moveControlHandler.TransferMove(moveCmd);
        }

        public void PrepareForAskingReserve(MoveCmdInfo moveCmd)
        {
            SetupNeedReserveSections(moveCmd);
        }

        private void SetupNeedReserveSections(MoveCmdInfo moveCmd)
        {
            queNeedReserveSections = new ConcurrentQueue<MapSection>();
            for (int i = 0; i < moveCmd.MovingSections.Count; i++)
            {
                MapSection section = moveCmd.MovingSections[i].DeepClone();
                queNeedReserveSections.Enqueue(section);
            }
        }

        public MapSection TrackingSection { get; set; } = new MapSection();

        private void MoveCmdInfoUpdatePosition(MoveCmdInfo curTransCmd, MapPosition gxPosition)
        {
            List<MapSection> movingSections = curTransCmd.MovingSections;
            int searchingSectionIndex = curTransCmd.MovingSectionsIndex;
            while (searchingSectionIndex < movingSections.Count)
            {
                if (mapHandler.IsPositionInThisSection(gxPosition, movingSections[searchingSectionIndex]))
                {
                    TrackingSection = movingSections[searchingSectionIndex];
                    //Middler send vehicle location to agvc
                    middleAgent.Send_Cmd134_TransferEventReport();
                    while (searchingSectionIndex > curTransCmd.MovingSectionsIndex)
                    {
                        var peek = queGotReserveOkSections.TryPeek(out MapSection mapSection);
                        var curSection = movingSections[curTransCmd.MovingSectionsIndex];
                        if (mapSection.Id == curSection.Id)
                        {
                            //Remove passed section in ReserveOkSection
                            queGotReserveOkSections.TryDequeue(out MapSection passSection);
                        }
                        else
                        {
                            //TODO : SetAlarm : reserveOkSection and curSection unmatch
                        }

                        curTransCmd.MovingSectionsIndex++;
                    }


                    break;
                }
                searchingSectionIndex++;
            }

            if (searchingSectionIndex == movingSections.Count)
            {
                //gxPosition is not in curTransCmd.MovingSections
                //TODO: PublishAlarm and log
            }

        }

        private void MoveCmdInfoUpdatePosition(MapPosition gxPosition)
        {
            bool isInMap = false;
            foreach (var item in theMapInfo.allMapSections)
            {
                MapSection mapSection = item.Value;
                mapSection.CmdDirection = EnumPermitDirection.Forward;
                if (mapHandler.IsPositionInThisSection(gxPosition, mapSection))
                {
                    TrackingSection = theVehicle.GetVehLoacation().Section;
                    isInMap = true;
                    //middleAgent.Send_Cmd134_TransferEventReport();
                    break;
                }
            }

            if (!isInMap)
            {
                //TODO: send alarm and log Position is not in Map
            }
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

        public List<MapSection> GetNeedReserveSections()
        {
            return queNeedReserveSections.ToList().DeepClone();
        }

        public List<MapSection> GetAskingReserveSections()
        {
            return queAskingReserveSections.ToList().DeepClone();
        }

        public List<MapSection> GetReserveOkSections()
        {
            return queGotReserveOkSections.ToList().DeepClone();
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
            MapAddress address = theVehicle.GetVehLoacation().Address;
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

                while (!theVehicle.GetPlcVehicle().Batterys.Charging)
                {                
                    SpinWait.SpinUntil(() => false, mainFlowConfig.StartChargeInterval);
                }

                theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusCharging;
                middleAgent.Send_Cmd144_StatusChangeReport();
            }
        }

        public void StopCharge()
        {
            plcAgent.ChargeStopCommand();

            while (theVehicle.GetPlcVehicle().Batterys.Charging)
            {            
                SpinWait.SpinUntil(() => false, mainFlowConfig.StopChargeInterval);
            }

            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusNone;
            middleAgent.Send_Cmd144_StatusChangeReport();
        }

    }
}
