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
        //private string configPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
        //private ConfigHandler configHandler;
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
        public long DoTransferTimeout { get; private set; } = 99999999;
        public long SetupReserveTimeout { get; private set; } = 99999999;
        public long DoTransferLoopTimeout { get; private set; } = 99999999;
        public long SearchSectionTimeout { get; private set; } = 99999999;
        public long SearchSectionLoopTimeout { get; private set; } = 99999999;
        public long TrackingPositionTimeout { get; private set; } = 99999999;
        public long StartChargeTimeout { get; private set; } = 5000;
        public long StopChargeTimeout { get; private set; } = 10000;

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
        private Thread thdVisitTransCmds;
        private ManualResetEvent visitTransCmdsShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent visitTransCmdsPauseEvent = new ManualResetEvent(true);
        public EnumThreadStatus VisitTransCmdsStatus { get; private set; } = new EnumThreadStatus();
        public EnumThreadStatus PreVisitTransCmdsStatus { get; private set; } = new EnumThreadStatus();

        private Thread thdTrackingPosition;
        private ManualResetEvent trackingPositionShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent trackingPositionPauseEvent = new ManualResetEvent(true);
        public EnumThreadStatus TrackingPositionStatus { get; private set; } = new EnumThreadStatus();
        public EnumThreadStatus PreTrackingPositionStatus { get; private set; } = new EnumThreadStatus();
        #endregion

        #region Events
        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;
        public event EventHandler<List<MapPosition>> OnReserveOkEvent;
        public event EventHandler<string> OnMessageShowEvent;
        #endregion

        public Vehicle theVehicle;
        private bool isIniOk;
        private MapInfo theMapInfo = new MapInfo();
        private MCProtocol mcProtocol;
        public ushort ForkCommandNumber { get; set; } = 0;
        public PlcForkCommand PlcForkLoadCommand { get; set; }
        public PlcForkCommand PlcForkUnloadCommand { get; set; }

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

        private void VehicleLocationInitial()
        {
            if (theVehicle.AVehiclePosition.RealPosition == null)
            {
                theVehicle.AVehiclePosition.RealPosition = theMapInfo.allMapAddresses["adr001"].Position.DeepClone();
            }
            StartTrackingPosition();
        }

        #region ConfigInitial
        //private void ConfigsInitial()
        //{
        //    try
        //    {
        //        //configPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
        //        //configHandler = new ConfigHandler(configPath);

        //        //mainFlowConfig = new MainFlowConfig();
        //        //mainFlowConfig.LogConfigPath = configHandler.GetString("MainFlow", "LogConfigPath", "Log.ini");
        //        //LoggerAgent.LogConfigPath = mainFlowConfig.LogConfigPath;
        //        //int.TryParse(configHandler.GetString("MainFlow", "TransCmdsCheckInterval", "15"), out int tempTransCmdsCheckInterval);
        //        //mainFlowConfig.TransCmdsCheckInterval = tempTransCmdsCheckInterval;
        //        //int.TryParse(configHandler.GetString("MainFlow", "DoTransCmdsInterval", "15"), out int tempDoTransCmdsInterval);
        //        //mainFlowConfig.DoTransCmdsInterval = tempDoTransCmdsInterval;
        //        //int.TryParse(configHandler.GetString("MainFlow", "ReserveLength", "3"), out int tempReserveLength);
        //        //mainFlowConfig.ReserveLength = tempReserveLength;
        //        //int.TryParse(configHandler.GetString("MainFlow", "TrackingPositionInterval", "100"), out int tempTrackingPositionInterval);
        //        //mainFlowConfig.TrackingPositionInterval = tempTrackingPositionInterval;
        //        //int.TryParse(configHandler.GetString("MainFlow", "StopChargeInterval", "100"), out int tempStopChargeInterval);
        //        //mainFlowConfig.StopChargeInterval = tempStopChargeInterval;
        //        //int.TryParse(configHandler.GetString("MainFlow", "StopChargeInterval", "100"), out int tempStartChargeInterval);
        //        //mainFlowConfig.StartChargeInterval = tempStartChargeInterval;

        //        //middlerConfig = new MiddlerConfig();
        //        //int.TryParse(configHandler.GetString("Middler", "ClientNum", "1"), out int tempClientNum);
        //        //middlerConfig.ClientNum = tempClientNum;
        //        //middlerConfig.ClientName = configHandler.GetString("Middler", "ClientName", "AGV01");
        //        //middlerConfig.RemoteIp = configHandler.GetString("Middler", "RemoteIp", "192.168.9.203");
        //        //int.TryParse(configHandler.GetString("Middler", "RemotePort", "10001"), out int tempRemotePort);
        //        //middlerConfig.RemotePort = tempRemotePort;
        //        //middlerConfig.LocalIp = configHandler.GetString("Middler", "LocalIp", "192.168.9.131");
        //        //int.TryParse(configHandler.GetString("Middler", "LocalPort", "5002"), out int tempPort);
        //        //middlerConfig.LocalPort = tempPort;
        //        //int.TryParse(configHandler.GetString("Middler", "RecvTimeoutMs", "10000"), out int tempRecvTimeoutMs);
        //        //middlerConfig.RecvTimeoutMs = tempRecvTimeoutMs;
        //        //int.TryParse(configHandler.GetString("Middler", "SendTimeoutMs", "0"), out int tempSendTimeoutMs);
        //        //middlerConfig.SendTimeoutMs = tempSendTimeoutMs;
        //        //int.TryParse(configHandler.GetString("Middler", "MaxReadSize", "0"), out int tempMaxReadSize);
        //        //middlerConfig.MaxReadSize = tempMaxReadSize;
        //        //int.TryParse(configHandler.GetString("Middler", "ReconnectionIntervalMs", "10000"), out int tempReconnectionIntervalMs);
        //        //middlerConfig.ReconnectionIntervalMs = tempReconnectionIntervalMs;
        //        //int.TryParse(configHandler.GetString("Middler", "MaxReconnectionCount", "10"), out int tempMaxReconnectionCount);
        //        //middlerConfig.MaxReconnectionCount = tempMaxReconnectionCount;
        //        //int.TryParse(configHandler.GetString("Middler", "RetryCount", "2"), out int tempRetryCount);
        //        //middlerConfig.RetryCount = tempRetryCount;
        //        //int.TryParse(configHandler.GetString("Middler", "SleepTime", "10"), out int tempSleepTime);
        //        //middlerConfig.SleepTime = tempSleepTime;
        //        //int.TryParse(configHandler.GetString("Middler", "RichTextBoxMaxLines ", "10"), out int tempRichTextBoxMaxLines);
        //        //middlerConfig.RichTextBoxMaxLines = tempRichTextBoxMaxLines;
        //        //int.TryParse(configHandler.GetString("Middler", "AskReserveIntervalMs ", "1000"), out int tempAskReserveInterval);
        //        //middlerConfig.AskReserveIntervalMs = tempAskReserveInterval;

        //        //mapConfig = new MapConfig();
        //        //mapConfig.SectionFileName = configHandler.GetString("Map", "SectionFileName", "ASECTION.csv");
        //        //mapConfig.AddressFileName = configHandler.GetString("Map", "AddressFileName", "AADDRESS.csv");
        //        //mapConfig.BarcodeFileName = configHandler.GetString("Map", "BarcodeFileName", "LBARCODE.csv");
        //        //mapConfig.OutSectionThreshold = double.Parse(configHandler.GetString("Map", "OutSectionThreshold", "10"));

        //        //alarmConfig = new AlarmConfig();
        //        //alarmConfig.AlarmFileName = configHandler.GetString("Alarm", "AlarmFileName", "AlarmCode.csv");

        //        if (OnComponentIntialDoneEvent != null)
        //        {
        //            var args = new InitialEventArgs
        //            {
        //                IsOk = true,
        //                ItemName = "讀寫設定檔"
        //            };
        //            OnComponentIntialDoneEvent(this, args);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        isIniOk = false;
        //        if (OnComponentIntialDoneEvent != null)
        //        {
        //            var args = new InitialEventArgs
        //            {
        //                IsOk = false,
        //                ItemName = "讀寫設定檔"
        //            };
        //            OnComponentIntialDoneEvent(this, args);
        //        }
        //    }
        //}
        #endregion

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
                theMapInfo = mapHandler.GetMapInfo();

                moveControlHandler = new MoveControlHandler(theMapInfo);
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
                theVehicle.SetMapInfo(theMapInfo);

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
            if (this.agvcTransCmd != null)
            {
                middleAgent.Send_Cmd131_TransferResponse(agvcTransCmd.SeqNum, 1, "Agv already have transfer command.");
                return;
            }

            OnMessageShowEvent?.Invoke(this, $"MainFlow : Get AgvcTransCmd, [Type={agvcTransCmd.EnumCommandType}]");

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
                transferSteps.Add(new EmptyTransCmd());

                //開始尋訪 transCmds as List<TransCmd> 裡的每一步MoveCmdInfo/LoadCmdInfo
                RestartVisitTransCmds();

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , ex.StackTrace));
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

            OnMessageShowEvent?.Invoke(this, "MainFlow : " + fullMsg);
        }

        private bool CheckTransCmdSectionsAndAddressesMatch(AgvcTransCmd agvcTransCmd)
        {
            switch (agvcTransCmd.EnumCommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToUnloadSections, agvcTransCmd.ToUnloadAddresses, agvcTransCmd.SeqNum);
                case EnumAgvcTransCommandType.Load:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToLoadSections, agvcTransCmd.ToLoadAddresses, agvcTransCmd.SeqNum);
                case EnumAgvcTransCommandType.Unload:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToUnloadSections, agvcTransCmd.ToUnloadAddresses, agvcTransCmd.SeqNum);
                case EnumAgvcTransCommandType.LoadUnload:
                    return IsSectionsAndAddressesMatch(agvcTransCmd.ToLoadSections, agvcTransCmd.ToLoadAddresses, agvcTransCmd.SeqNum) && IsSectionsAndAddressesMatch(agvcTransCmd.ToUnloadSections, agvcTransCmd.ToUnloadAddresses, agvcTransCmd.SeqNum);
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

        public void SetTestTransferCmd()
        {
            transferSteps = new List<TransferStep>();

            AgvcTransCmd transCmd = new AgvcTransCmd();
            transCmd.CommandId = "test001";
            transCmd.EnumCommandType = EnumAgvcTransCommandType.LoadUnload;
            transCmd.SeqNum = 12345;
            transCmd.LoadAddress = "adr010";
            transCmd.ToLoadAddresses = new List<string>();
            transCmd.ToLoadAddresses.Add("adr001");
            transCmd.ToLoadAddresses.Add("adr010");
            transCmd.ToLoadSections = new List<string>();
            transCmd.ToLoadSections.Add("sec001");

            transCmd.UnloadAddress = "adr011";
            transCmd.ToUnloadAddresses = new List<string>();
            transCmd.ToUnloadAddresses.Add("adr010");
            transCmd.ToUnloadAddresses.Add("adr002");
            transCmd.ToUnloadAddresses.Add("adr011");
            transCmd.ToUnloadSections = new List<string>();
            transCmd.ToUnloadSections.Add("sec002");
            transCmd.ToUnloadSections.Add("sec003");

            MiddleAgent_OnInstallTransferCommandEvent(this, transCmd);
        }

        private bool IsBasicTransCmds()
        {
            switch (agvcTransCmd.EnumCommandType)
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
            MoveCmdInfo moveCmd = new MoveCmdInfo(theMapInfo);
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
            MoveCmdInfo moveCmd = new MoveCmdInfo(theMapInfo);
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

            return loadCmd;
        }

        private UnloadCmdInfo GetUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
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

            return unloadCmd;
        }

        private MoveCmdInfo GetMoveToLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo(theMapInfo);
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

        private void ConvertAgvcMoveCmdIntoList(AgvcTransCmd agvcTransCmd)
        {
            if (agvcTransCmd.ToUnloadSections.Count > 0)
            {
                MoveCmdInfo moveCmd = GetMoveToUnloadCmdInfo(agvcTransCmd);
                transferSteps.Add(moveCmd);
            }
        }

        public void IdleVisitNext()
        {
            TransferStepsIndex++;
        }

        private void VisitTransCmds()
        {
            PreVisitTransCmds();
            Stopwatch sw = new Stopwatch();
            long total = 0;
            while (TransferStepsIndex < transferSteps.Count)
            {
                try
                {
                    #region Pause And Stop Check
                    visitTransCmdsPauseEvent.WaitOne(Timeout.Infinite);
                    if (visitTransCmdsShutdownEvent.WaitOne(0))
                    {
                        break;
                    }
                    #endregion

                    VisitTransCmdsStatus = EnumThreadStatus.Working;
                    sw.Start();

                    if (GoNextTransferStep)
                    {
                        GoNextTransferStep = false;
                        DoTransfer();
                    }                                       

                    sw.Stop();
                    total += sw.ElapsedMilliseconds;

                    sw.Reset();
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , ex.StackTrace));
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.DoTransCmdsInterval);
                }
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
            VisitTransCmdsStatus = EnumThreadStatus.Start;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Start Visit TransCmds, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]");
        }
        public void PauseVisitTransCmds()
        {
            visitTransCmdsPauseEvent.Reset();
            PreVisitTransCmdsStatus = VisitTransCmdsStatus;
            VisitTransCmdsStatus = EnumThreadStatus.Pause;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Pause Visit TransCmds, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]");
        }
        public void ResumeVisitTransCmds()
        {
            visitTransCmdsPauseEvent.Set();
            var tempStatus = VisitTransCmdsStatus;
            VisitTransCmdsStatus = PreVisitTransCmdsStatus;
            PreVisitTransCmdsStatus = tempStatus;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Resume Visit TransCmds, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]");
        }
        public void StopVisitTransCmds()
        {
            VisitTransCmdsStatus = EnumThreadStatus.Stop;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Stop Visit TransCmds, [StepIndex={TransferStepsIndex}][TotalSteps={transferSteps.Count}]");

            visitTransCmdsShutdownEvent.Set();
            visitTransCmdsPauseEvent.Set();
            theVehicle.SetVehicleStop();
            //if (thdVisitTransCmds != null && thdVisitTransCmds.IsAlive)
            //{
            //    thdVisitTransCmds.Join();
            //}

        }
        private void PreVisitTransCmds()
        {
            TransferStepsIndex = 0;
            GoNextTransferStep = true;
            theVehicle.ActionStatus = VHActionStatus.Commanding;
            middleAgent.Send_Cmd144_StatusChangeReport();
        }
        private void AfterVisitTransCmds()
        {
            lastAgvcTransCmd = agvcTransCmd;
            agvcTransCmd = null;
            theVehicle.SetAgvcTransCmd(new AgvcTransCmd());
            transferSteps = new List<TransferStep>();
            TransferStepsIndex = 0;
            GoNextTransferStep = false;
            SetTransCmdsStep(new Idle());
            theVehicle.ActionStatus = VHActionStatus.NoCommand;
            VisitTransCmdsStatus = EnumThreadStatus.None;
            middleAgent.Send_Cmd144_StatusChangeReport();
        }

        private bool CanVehUnload()
        {
            // 判斷當前是否可載貨 若否 則發送報告
            MapPosition position = theVehicle.AVehiclePosition.RealPosition;
            MapAddress unloadAddress = theMapInfo.allMapAddresses[agvcTransCmd.UnloadAddress];
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
            MapPosition position = theVehicle.AVehiclePosition.RealPosition;
            MapAddress loadAddress = theMapInfo.allMapAddresses[agvcTransCmd.LoadAddress];
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
            var plcVeh = theVehicle.GetPlcVehicle();
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
            var plcVeh = theVehicle.GetPlcVehicle();
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

        private void TrackingPosition()
        {
            Stopwatch sw = new Stopwatch();

            while (true)
            {
                try
                {
                    sw.Start();
                    #region Pause And Stop Check

                    trackingPositionPauseEvent.WaitOne(Timeout.Infinite);
                    if (trackingPositionShutdownEvent.WaitOne(0))
                    {
                        break;
                    }

                    #endregion
                    TrackingPositionStatus = EnumThreadStatus.Working;
                    var position = theVehicle.AVehiclePosition.RealPosition;
                    if (transferSteps.Count > 0)
                    {
                        //有搬送命令時，比對當前Position與搬送路徑Sections確定section-distance
                        var curTransCmd = GetCurTransferStep();
                        if (curTransCmd.GetEnumTransferCommandType() == EnumTransferCommandType.Move)
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
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , ex.StackTrace));
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.TrackingPositionInterval);
                }

                sw.Stop();
                if (sw.ElapsedMilliseconds > mainFlowConfig.ReportPositionInterval)
                {
                    middleAgent.ReportAddressPass();
                    sw.Reset();
                }
            }

        }
        public void StartTrackingPosition()
        {
            trackingPositionPauseEvent.Set();
            trackingPositionShutdownEvent.Reset();
            thdTrackingPosition = new Thread(TrackingPosition);
            thdTrackingPosition.IsBackground = true;
            thdTrackingPosition.Start();
            TrackingPositionStatus = EnumThreadStatus.Start;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Start Tracking Position, [TrackingPositionStatus={TrackingPositionStatus}][PreTrackingPositionStatus={PreTrackingPositionStatus}]");
        }
        public void PauseTrackingPosition()
        {
            trackingPositionPauseEvent.Reset();
            PreTrackingPositionStatus = TrackingPositionStatus;
            TrackingPositionStatus = EnumThreadStatus.Pause;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Pause Tracking Position, [TrackingPositionStatus={TrackingPositionStatus}][PreTrackingPositionStatus={PreTrackingPositionStatus}]");
        }
        public void ResumeTrackingPosition()
        {
            trackingPositionPauseEvent.Set();
            var tempStatus = TrackingPositionStatus;
            TrackingPositionStatus = PreTrackingPositionStatus;
            PreTrackingPositionStatus = tempStatus;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Resume Tracking Position, [TrackingPositionStatus={TrackingPositionStatus}][PreTrackingPositionStatus={PreTrackingPositionStatus}]");
        }
        public void StopTrackingPosition()
        {
            trackingPositionShutdownEvent.Set();
            trackingPositionPauseEvent.Set();
            TrackingPositionStatus = EnumThreadStatus.Stop;

            //if (thdTrackingPosition.IsAlive)
            //{
            //    thdTrackingPosition.Join();
            //}
            TrackingPositionStatus = EnumThreadStatus.None;

            OnMessageShowEvent?.Invoke(this, $"MainFlow : Stop Tracking Position, [TrackingPositionStatus={TrackingPositionStatus}][PreTrackingPositionStatus={PreTrackingPositionStatus}]");
        }

        public void MiddleAgent_StartAskingReserve()
        {
            middleAgent.StartAskingReserve();
        }

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
            return GetCurrentEnumTransferCommandType() == EnumTransferCommandType.Move;
        }
        public void SetupTestMoveCmd(List<MapSection> mapSections)
        {
            transferSteps = new List<TransferStep>();
            MoveCmdInfo moveCmd = new MoveCmdInfo();
            moveCmd.MovingSections = mapSections.DeepClone();
            transferSteps.Add(moveCmd);
            transferSteps.Add(new EmptyTransCmd());
            TransferStepsIndex = 0;
        }       

        public void MoveControlHandler_OnMoveFinished(object sender, EnumMoveComplete status)
        {
            try
            {
                middleAgent.StopAskingReserve();
                middleAgent.ClearGotReserveOkSections();
                theVehicle.AVehiclePosition.PredictVehicleAngle = (int)theVehicle.AVehiclePosition.VehicleAngle;

                if (status == EnumMoveComplete.Fail)
                {
                    //TODO: Alarm

                    StopVisitTransCmds();
                    AfterVisitTransCmds();
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

        private void OnAbortEvent()
        {
            StopVisitTransCmds();
            AfterVisitTransCmds();
        }

        private void OnCancelEvent()
        {
            StopVisitTransCmds();
            AfterVisitTransCmds();
            middleAgent.MainFlowGetCancel();
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
                    if (theVehicle.GetPlcVehicle().Loading)
                    {
                        //Alarm : loading is still on
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : ForkCommandFinish,[Type={forkCommand.ForkCommandType}] [Loading={theVehicle.GetPlcVehicle().Loading}]");
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
            return transferSteps[TransferStepsIndex + 1].GetEnumTransferCommandType() == EnumTransferCommandType.Unload;
        }

        private bool NextTransCmdIsLoad()
        {
            return transferSteps[TransferStepsIndex + 1].GetEnumTransferCommandType() == EnumTransferCommandType.Load;
        }

        private bool NextTransCmdIsMove()
        {
            return transferSteps[TransferStepsIndex + 1].GetEnumTransferCommandType() == EnumTransferCommandType.Move;
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
            //if (TransferStepsIndex == transferSteps.Count - 1)
            //{
            //    //Middler send transfer complete to agvc
            //}

            TransferStepsIndex++;
            GoNextTransferStep = true;
        }

        public TransferStep GetCurTransferStep()
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
            middleAgent.StopAskingReserve();
            middleAgent.SetupNeedReserveSections(moveCmd);
            middleAgent.StartAskingReserve();
            //SetupNeedReserveSections(moveCmd);
        }        

        private void MoveCmdInfoUpdatePosition(MoveCmdInfo curTransCmd, MapPosition gxPosition)
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
            var plcVeh = theVehicle.GetPlcVehicle();
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

        private void MoveCmdInfoUpdatePosition(MapPosition gxPosition)
        {
            if (gxPosition == null) return;

            bool isInMap = false;
            foreach (var item in theMapInfo.allMapSections)
            {
                MapSection mapSection = item.Value;
                mapSection.CmdDirection = EnumPermitDirection.Forward;
                if (mapHandler.IsPositionInThisSection(gxPosition, mapSection))
                {
                    SectionHasFoundPosition = theVehicle.AVehiclePosition.LastSection;
                    isInMap = true;
                    if (mapSection.Type == EnumSectionType.Horizontal)
                    {
                        theVehicle.AVehiclePosition.RealPosition.Y = mapSection.HeadAddress.Position.Y;
                    }
                    else if (mapSection.Type == EnumSectionType.Vertical)
                    {
                        theVehicle.AVehiclePosition.RealPosition.X = mapSection.HeadAddress.Position.X;
                    }
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
            MapAddress address = theVehicle.AVehiclePosition.LastAddress;
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

                SpinWait.SpinUntil(() => false, mainFlowConfig.StartChargeInterval);
                if (!theVehicle.GetPlcVehicle().Batterys.Charging)
                {
                    //Alarm
                }
                else
                {
                    theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusCharging;
                    middleAgent.Send_Cmd144_StatusChangeReport();
                }             
               

                OnMessageShowEvent?.Invoke(this, $"MainFlow : Start Charging, [Id={address.Id}][IsInCouple={address.IsCharger}][IsCharging={theVehicle.GetPlcVehicle().Batterys.Charging}]");
            }
            else
            {
                OnMessageShowEvent?.Invoke(this, $"MainFlow : Start Charging,[Id={address.Id}][IsInCouple={address.IsCharger}][IsCharging={theVehicle.GetPlcVehicle().Batterys.Charging}]");
            }
        }

        public void StopCharge()
        {
            var isStopChargeOk = true;
            plcAgent.ChargeStopCommand();
            SpinWait.SpinUntil(() => false, mainFlowConfig.StopChargeInterval);

            if (theVehicle.GetPlcVehicle().Batterys.Charging)
            {
                //Alarm
                isStopChargeOk = false;
            }
            else
            {
                theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusNone;
                middleAgent.Send_Cmd144_StatusChangeReport();
            }            

            OnMessageShowEvent?.Invoke(this, $"MainFlow : Stop Charge, [IsCharging={theVehicle.GetPlcVehicle().Batterys.Charging}]");

            if (!isStopChargeOk)
            {
                StopVehicle();
            }
        }

        public void ClearAgvcTransferCmd()
        {
            StopVehicle();
            StopVisitTransCmds();
            middleAgent.StopAskingReserve();
            middleAgent.ClearGotReserveOkSections();
            this.agvcTransCmd = null;
            lastTransferSteps = transferSteps;
            transferSteps = new List<TransferStep>();
            TransferStepsIndex = 0;
        }

        public EnumTransferCommandType GetCurrentEnumTransferCommandType()
        {
            try
            {
                if (transferSteps.Count > 0)
                {
                    if (TransferStepsIndex < transferSteps.Count)
                    {
                        return transferSteps[TransferStepsIndex].GetEnumTransferCommandType();
                    }
                }

                return EnumTransferCommandType.Empty;
            }
            catch (Exception ex)
            {
                //Log Error
                OnMessageShowEvent?.Invoke(this, $"MainFlow : GetCurrentEnumTransCmdType, [ex={ex.StackTrace}]");
                return EnumTransferCommandType.Empty;
            }
        }

        public int GetTransferStepCount()
        {
            return transferSteps.Count;
        }

        public void StopVehicle()
        {
            moveControlHandler.StopFlagOn();
            //Should I clean transfer cmd and stop asking reserve?
        }
    }
}
