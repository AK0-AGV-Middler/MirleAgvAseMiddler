using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using com.mirle.iibg3k0.ttc.Common.TCPIP.DecodRawData;
using Google.Protobuf.Collections;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Concurrent;
using Mirle.Tools;



namespace Mirle.Agv.AseMiddler.Controller
{
    [Serializable]
    public class AgvcConnector
    {
        #region Events
        public event EventHandler<string> OnMessageShowOnMainFormEvent;
        public event EventHandler<AgvcTransCmd> OnInstallTransferCommandEvent;
        public event EventHandler<AgvcOverrideCmd> OnOverrideCommandEvent;
        public event EventHandler<AseMovingGuide> OnAvoideRequestEvent;
        public event EventHandler<string> OnCmdReceiveEvent;
        public event EventHandler<string> OnCmdSendEvent;
        public event EventHandler<bool> OnConnectionChangeEvent;
        public event EventHandler<string> OnReserveOkEvent;
        public event EventHandler<string> OnPassReserveSectionEvent;
        #endregion

        private Vehicle theVehicle = Vehicle.Instance;
        private AgvcConnectorConfig agvcConnectorConfig;
        private AlarmHandler alarmHandler;
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private MainFlowHandler mainFlowHandler;

        private Thread thdAskReserve;
        public EnumThreadStatus AskReserveStatus { get; private set; } = EnumThreadStatus.None;
        public EnumThreadStatus PreAskReserveStatus { get; private set; } = EnumThreadStatus.None;

        private ConcurrentQueue<MapSection> queNeedReserveSections = new ConcurrentQueue<MapSection>();
        private ConcurrentQueue<MapSection> queReserveOkSections = new ConcurrentQueue<MapSection>();
        private bool ReserveOkAskNext { get; set; } = false;
        private ConcurrentBag<MapSection> CbagNeedReserveSections { get; set; } = new ConcurrentBag<MapSection>();
        public bool IsAskReservePause { get; private set; }
        public bool IsAskReserveStop { get; private set; }
        private bool IsWaitReserveReply { get; set; }
        public bool IsAgvcRejectReserve { get; set; }

        public event EventHandler<LogFormat> OnLogMsgEvent;

        public TcpIpAgent ClientAgent { get; private set; }
        public string AgvcConnectorAbnormalMsg { get; set; } = "";

        public AgvcConnector(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            agvcConnectorConfig = mainFlowHandler.GetAgvcConnectorConfig();
            alarmHandler = mainFlowHandler.GetAlarmHandler();
            mirleLogger = MirleLogger.Instance;

            CreatTcpIpClientAgent();
            if (!theVehicle.IsSimulation)
            {
                Connect();
            }
            StartAskReserve();
        }

        #region Initial

        public void CreatTcpIpClientAgent()
        {

            IDecodReceiveRawData RawDataDecoder = new DecodeRawData_Google(unPackWrapperMsg);

            int clientNum = agvcConnectorConfig.ClientNum;
            string clientName = agvcConnectorConfig.ClientName;
            string sRemoteIP = agvcConnectorConfig.RemoteIp;
            int iRemotePort = agvcConnectorConfig.RemotePort;
            string sLocalIP = agvcConnectorConfig.LocalIp;
            int iLocalPort = agvcConnectorConfig.LocalPort;

            int recv_timeout_ms = agvcConnectorConfig.RecvTimeoutMs;                         //等待sendRecv Reply的Time out時間(milliseconds)
            int send_timeout_ms = agvcConnectorConfig.SendTimeoutMs;                         //暫時無用
            int max_readSize = agvcConnectorConfig.MaxReadSize;                              //暫時無用
            int reconnection_interval_ms = agvcConnectorConfig.ReconnectionIntervalMs;       //斷線多久之後再進行一次嘗試恢復連線的動作
            int max_reconnection_count = agvcConnectorConfig.MaxReconnectionCount;           //斷線後最多嘗試幾次重新恢復連線 (若設定為0則不進行自動重新連線)
            int retry_count = agvcConnectorConfig.RetryCount;                                //SendRecv Time out後要再重複發送的次數

            try
            {
                ClientAgent = new TcpIpAgent(clientNum, clientName, sLocalIP, iLocalPort, sRemoteIP, iRemotePort, TcpIpAgent.TCPIP_AGENT_COMM_MODE.CLINET_MODE, recv_timeout_ms, send_timeout_ms, max_readSize, reconnection_interval_ms, max_reconnection_count, retry_count, AppConstants.FrameBuilderType.PC_TYPE_MIRLE);
                ClientAgent.injectDecoder(RawDataDecoder);

                EventInitial();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private static Google.Protobuf.IMessage unPackWrapperMsg(byte[] raw_data)
        {
            WrapperMessage WarpperMsg = ToObject<WrapperMessage>(raw_data);
            return WarpperMsg;
        }
        private static T ToObject<T>(byte[] buf) where T : Google.Protobuf.IMessage<T>, new()
        {
            if (buf == null)
                return default(T);

            Google.Protobuf.MessageParser<T> parser = new Google.Protobuf.MessageParser<T>(() => new T());
            return parser.ParseFrom(buf);
        }
        public AgvcConnectorConfig GetAgvcConnectorConfig()
        {
            return agvcConnectorConfig;
        }
        public bool IsClientAgentNull() => ClientAgent == null;
        public void ReConnect()
        {
            try
            {
                DisConnect();

                Connect();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void DisConnect()
        {
            try
            {
                if (ClientAgent != null)
                {
                    string msg = $"AgvcConnector : Disconnect Stop, [IsNull={IsClientAgentNull()}][IsConnect={IsConnected()}]";
                    LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

                    ClientAgent.stop();
                    //ClientAgent = null;
                }
                else
                {
                    string msg = $"ClientAgent is null cannot disconnect";
                    LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Connect()
        {
            if (ClientAgent != null)
            {
                if (!ClientAgent.IsConnection)
                {
                    //Task.Run(() => ClientAgent.clientConnection());
                    Task.Run(() => ClientAgent.start());
                }
                else
                {
                    string msg = $"Already connect cannot connect again.";
                    LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                }
            }
            else
            {
                CreatTcpIpClientAgent();
                Connect();
                string msg = $"ClientAgent is null cannot connect.";
                LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }

        }
        public void StopClientAgent()
        {
            if (ClientAgent != null)
            {
                if (ClientAgent.IsConnection)
                {
                    Task.Run(() => ClientAgent.stop());
                }
            }
        }
        protected void ClientAgent_OnConnectionChangeEvent(object sender, TcpIpEventArgs e)
        {
            TcpIpAgent agent = sender as TcpIpAgent;
            OnConnectionChangeEvent?.Invoke(this, agent.IsConnection);
            var isConnect = agent.IsConnection ? "連線中" : "斷線";
            OnMessageShowOnMainFormEvent?.Invoke(this, $"AgvcConnector : {agent.Name}與AGVC連線狀態改變 {isConnect}");
        }
        //protected void DoDisconnection(object sender, TcpIpEventArgs e)
        //{
        //    TcpIpAgent agent = sender as TcpIpAgent;
        //    var msg = $"Vh ID:{agent.Name}, disconnection.";
        //    OnConnectionChangeEvent?.Invoke(this, false);
        //    OnMessageShowOnMainFormEvent?.Invoke(this, "AgvcConnector : Dis-Connect");
        //}
        private void EventInitial()
        {
            foreach (var item in Enum.GetValues(typeof(EnumCmdNum)))
            {
                ClientAgent.addTcpIpReceivedHandler((int)item, LogRecvMsg);
                ClientAgent.addTcpIpReceivedHandler((int)item, RecieveCommandMediator);
            }

            ClientAgent.addTcpIpConnectedHandler(ClientAgent_OnConnectionChangeEvent);       //連線時的通知
            ClientAgent.addTcpIpDisconnectedHandler(ClientAgent_OnConnectionChangeEvent);    //斷線時的通知
        }
        private void SendCommandWrapper(WrapperMessage wrapper, bool isReply = false, int delay = 0)
        {
            if (!IsConnected())
            {
                var msg = $"AgvcConnector : 斷線中，無法發送[{wrapper.SeqNum}][id {wrapper.ID}{(EnumCmdNum)wrapper.ID}]資訊";
                OnCmdSendEvent?.Invoke(this, msg);
                msg += wrapper.ToString();
                LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                return;
            }
            else
            {
                LogSendMsg(wrapper);

                if (delay != 0)
                {
                    Task.Run(() =>
                    {
                        SpinWait.SpinUntil(() => false, delay);
                        ClientAgent.TrxTcpIp.SendGoogleMsg(wrapper, isReply);
                    });
                }
                else
                {
                    Task.Run(() => ClientAgent.TrxTcpIp.SendGoogleMsg(wrapper, isReply));
                }
            }
        }
        private void LogRecvMsg(object sender, TcpIpEventArgs e)
        {
            string msg = $"[RECV] [SeqNum = {e.iSeqNum}][{e.iPacketID}][{(EnumCmdNum)int.Parse(e.iPacketID)}][ObjPacket = {e.objPacket}]";
            OnCmdReceiveEvent?.Invoke(this, msg);
            LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
        }
        private void RecieveCommandMediator(object sender, TcpIpEventArgs e)
        {
            EnumCmdNum cmdNum = (EnumCmdNum)int.Parse(e.iPacketID);

            if (theVehicle.AutoState != EnumAutoState.Auto && !IsApplyOnly(cmdNum))
            {
                var msg = $"AgvcConnector : 手動模式下，不接受AGVC命令";
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                return;
            }

            switch (cmdNum)
            {
                case EnumCmdNum.Cmd000_EmptyCommand:
                    break;
                case EnumCmdNum.Cmd31_TransferRequest:
                    Receive_Cmd31_TransferRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd32_TransferCompleteResponse:
                    Receive_Cmd32_TransferCompleteResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd38_GuideInfoResponse:
                    Receive_Cmd38_GuideInfoResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                    Receive_Cmd35_CarrierIdRenameRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd36_TransferEventResponse:
                    Receive_Cmd36_TransferEventResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd37_TransferCancelRequest:
                    Receive_Cmd37_TransferCancelRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd39_PauseRequest:
                    Receive_Cmd39_PauseRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd41_ModeChange:
                    Receive_Cmd41_ModeChange(sender, e);
                    break;
                case EnumCmdNum.Cmd43_StatusRequest:
                    Receive_Cmd43_StatusRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd44_StatusRequest:
                    Receive_Cmd44_StatusRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd51_AvoidRequest:
                    Receive_Cmd51_AvoidRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                    Receive_Cmd52_AvoidCompleteResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd74_AddressTeachResponse:
                    Receive_Cmd74_AddressTeachResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd91_AlarmResetRequest:
                    Receive_Cmd91_AlarmResetRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd94_AlarmResponse:
                    Receive_Cmd94_AlarmResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd131_TransferResponse:
                    break;
                case EnumCmdNum.Cmd132_TransferCompleteReport:
                    break;
                case EnumCmdNum.Cmd133_ControlZoneCancelResponse:
                    break;
                case EnumCmdNum.Cmd134_TransferEventReport:
                    break;
                case EnumCmdNum.Cmd135_CarrierIdRenameResponse:
                    break;
                case EnumCmdNum.Cmd136_TransferEventReport:
                    break;
                case EnumCmdNum.Cmd137_TransferCancelResponse:
                    break;
                case EnumCmdNum.Cmd139_PauseResponse:
                    break;
                case EnumCmdNum.Cmd141_ModeChangeResponse:
                    break;
                case EnumCmdNum.Cmd143_StatusResponse:
                    break;
                case EnumCmdNum.Cmd144_StatusReport:
                    break;
                case EnumCmdNum.Cmd145_PowerOnoffResponse:
                    break;
                case EnumCmdNum.Cmd151_AvoidResponse:
                    break;
                case EnumCmdNum.Cmd152_AvoidCompleteReport:
                    break;
                case EnumCmdNum.Cmd171_RangeTeachResponse:
                    break;
                case EnumCmdNum.Cmd172_RangeTeachCompleteReport:
                    break;
                case EnumCmdNum.Cmd174_AddressTeachReport:
                    break;
                case EnumCmdNum.Cmd191_AlarmResetResponse:
                    break;
                case EnumCmdNum.Cmd194_AlarmReport:
                    break;
                default:
                    break;
            }
        }

        private bool IsApplyOnly(EnumCmdNum cmdNum)
        {
            switch (cmdNum)
            {
                case EnumCmdNum.Cmd000_EmptyCommand:
                    break;
                case EnumCmdNum.Cmd31_TransferRequest:
                    break;
                case EnumCmdNum.Cmd32_TransferCompleteResponse:
                    break;
                case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                    return true;
                case EnumCmdNum.Cmd36_TransferEventResponse:
                    break;
                case EnumCmdNum.Cmd37_TransferCancelRequest:
                    break;
                case EnumCmdNum.Cmd39_PauseRequest:
                    break;
                case EnumCmdNum.Cmd41_ModeChange:
                    break;
                case EnumCmdNum.Cmd43_StatusRequest:
                    return true;
                case EnumCmdNum.Cmd44_StatusRequest:
                    break;
                case EnumCmdNum.Cmd51_AvoidRequest:
                    break;
                case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                    break;
                case EnumCmdNum.Cmd71_RangeTeachRequest:
                    break;
                case EnumCmdNum.Cmd72_RangeTeachCompleteResponse:
                    break;
                case EnumCmdNum.Cmd74_AddressTeachResponse:
                    break;
                case EnumCmdNum.Cmd91_AlarmResetRequest:
                    return true;
                case EnumCmdNum.Cmd94_AlarmResponse:
                    break;
                case EnumCmdNum.Cmd131_TransferResponse:
                    break;
                case EnumCmdNum.Cmd132_TransferCompleteReport:
                    break;
                case EnumCmdNum.Cmd133_ControlZoneCancelResponse:
                    break;
                case EnumCmdNum.Cmd134_TransferEventReport:
                    break;
                case EnumCmdNum.Cmd135_CarrierIdRenameResponse:
                    break;
                case EnumCmdNum.Cmd136_TransferEventReport:
                    break;
                case EnumCmdNum.Cmd137_TransferCancelResponse:
                    break;
                case EnumCmdNum.Cmd139_PauseResponse:
                    break;
                case EnumCmdNum.Cmd141_ModeChangeResponse:
                    break;
                case EnumCmdNum.Cmd143_StatusResponse:
                    break;
                case EnumCmdNum.Cmd144_StatusReport:
                    break;
                case EnumCmdNum.Cmd145_PowerOnoffResponse:
                    break;
                case EnumCmdNum.Cmd151_AvoidResponse:
                    break;
                case EnumCmdNum.Cmd152_AvoidCompleteReport:
                    break;
                case EnumCmdNum.Cmd171_RangeTeachResponse:
                    break;
                case EnumCmdNum.Cmd172_RangeTeachCompleteReport:
                    break;
                case EnumCmdNum.Cmd174_AddressTeachReport:
                    break;
                case EnumCmdNum.Cmd191_AlarmResetResponse:
                    break;
                case EnumCmdNum.Cmd194_AlarmReport:
                    break;
                default:
                    break;
            }

            return false;
        }
        private void LogSendMsg(WrapperMessage wrapper)
        {
            try
            {
                string msg = $"[SEND] [SeqNum = {wrapper.SeqNum}][{wrapper.ID}][{(EnumCmdNum)wrapper.ID}] {wrapper}";
                OnCmdSendEvent?.Invoke(this, msg);
                LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        #endregion

        #region Thd Ask Reserve
        private void AskReserve()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                try
                {
                    #region Pause And Stop Check
                    if (IsAskReservePause) continue;
                    if (IsAskReserveStop) break;
                    #endregion

                    if (queNeedReserveSections.IsEmpty) continue;

                    AskReserveStatus = EnumThreadStatus.Working;

                    if (CanAskReserve())
                    {
                        queNeedReserveSections.TryPeek(out MapSection askReserveSection);
                        if (askReserveSection == null) continue;
                        if (CanDoReserveWork())
                        {
                            Send_Cmd136_AskReserve(askReserveSection);
                            SpinWait.SpinUntil(() => false, 5);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
                finally
                {
                    if (IsAskReserveStop || IsAskReservePause || mainFlowHandler.IsMoveEnd)
                    {
                        SpinWait.SpinUntil(() => false, 50);
                    }
                    else
                    {
                        SpinWait.SpinUntil(() => ReserveOkAskNext, agvcConnectorConfig.AskReserveIntervalMs);
                        ReserveOkAskNext = false;
                        SpinWait.SpinUntil(() => false, 5);
                    }
                }
            }

            AfterAskReserve(sw.ElapsedMilliseconds);
        }
        public void StartAskReserve()
        {
            IsAskReservePause = false;
            IsAskReserveStop = false;
            thdAskReserve = new Thread(new ThreadStart(AskReserve));
            thdAskReserve.IsBackground = true;
            thdAskReserve.Start();
            AskReserveStatus = EnumThreadStatus.Start;
            var msg = $"AgvcConnector : 開始詢問通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void PauseAskReserve()
        {
            IsAskReservePause = true;
            PreAskReserveStatus = AskReserveStatus;
            AskReserveStatus = EnumThreadStatus.Pause;
            var getSectionOk = queNeedReserveSections.TryPeek(out MapSection mapSection);
            var msg = getSectionOk ? $"AgvcConnector : 暫停詢問通行權,[當前詢問路徑={mapSection.Id}]" : $"AgvcConnector : 暫停詢問通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void ResumeAskReserve()
        {
            IsAskReservePause = false;
            AskReserveStatus = PreAskReserveStatus;
            var getSectionOk = queNeedReserveSections.TryPeek(out MapSection mapSection);
            var msg = getSectionOk ? $"AgvcConnector : 恢復詢問通行權,[當前詢問路徑={mapSection.Id}]" : "AgvcConnector : 恢復詢問通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void StopAskReserve()
        {
            IsAskReserveStop = true;
            AskReserveStatus = EnumThreadStatus.Stop;
            ClearAllReserve();
            var msg = $"AgvcConnector : 停止詢問通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        //private void PreAskReserve()
        //{
        //    queNeedReserveSections = new ConcurrentQueue<MapSection>(NeedReserveSections);
        //    queReserveOkSections = new ConcurrentQueue<MapSection>();
        //    askingReserveSection = new MapSection();
        //    if (queNeedReserveSections.IsEmpty)
        //    {
        //        StopAskReserve();
        //    }

        //    var msg = $"AgvcConnector :詢問通行權 前處理[NeedReserveSectionIds={QueMapSectionsToString(queNeedReserveSections)}]";
        //    OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        //}
        private void AfterAskReserve(long total)
        {
            AskReserveStatus = EnumThreadStatus.None;
            var msg = $"MainFlow : 詢問通行權 後處理, [ThreadStatus={AskReserveStatus}][TotalSpendMs={total}]";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }

        public bool CanDoReserveWork()
        {
            return !IsAskReservePause && !IsAskReserveStop && !mainFlowHandler.IsMoveEnd;
        }

        private bool CanAskReserve()
        {
            return mainFlowHandler.IsMoveStep() && mainFlowHandler.CanVehMove() && !IsGotReserveOkSectionsFull() && !mainFlowHandler.IsMoveEnd;
        }
        public bool IsGotReserveOkSectionsFull()
        {
            int reserveOkSectionsTotalLength = GetReserveOkSectionsTotalLength();
            return reserveOkSectionsTotalLength >= agvcConnectorConfig.ReserveLengthMeter * 1000;
        }
        private string QueMapSectionsToString(ConcurrentQueue<MapSection> aQue)
        {
            string sectionIds = "[";
            foreach (var item in aQue) sectionIds = string.Concat(sectionIds, $"({item.Id})");
            sectionIds += "]";
            return sectionIds;
        }

        private int GetReserveOkSectionsTotalLength()
        {
            double result = 0;
            List<MapSection> reserveOkSections = new List<MapSection>(queReserveOkSections);
            foreach (var item in reserveOkSections)
            {
                result += item.HeadToTailDistance;
            }
            return (int)result;
        }
        public void ClearNeedReserveSections()
        {
            queNeedReserveSections = new ConcurrentQueue<MapSection>();
        }
        public void ClearGotReserveOkSections()
        {
            queReserveOkSections = new ConcurrentQueue<MapSection>();
        }
        public MapSection GetAskingReserveSection()
        {
            var getSectionOk = queNeedReserveSections.TryPeek(out MapSection mapSection);
            return getSectionOk ? mapSection : new MapSection();
        }
        public void SetupNeedReserveSections()
        {
            queNeedReserveSections = new ConcurrentQueue<MapSection>(theVehicle.AseMovingGuide.MovingSections);
            var msg = $"AgvcConnector : 更新需要通行權路徑列表[{QueMapSectionsToString(queNeedReserveSections)}]";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void SetupNeedReserveSections(List<MapSection> mapSections)
        {
            queNeedReserveSections = new ConcurrentQueue<MapSection>(mapSections);
            var msg = $"AgvcConnector : 更新需要通行權路徑列表[{QueMapSectionsToString(queNeedReserveSections)}]";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public List<MapSection> GetNeedReserveSections()
        {
            return new List<MapSection>(queNeedReserveSections);
        }
        public List<MapSection> GetReserveOkSections()
        {
            return new List<MapSection>(queReserveOkSections);
        }
        public void DequeueGotReserveOkSections()
        {
            if (queReserveOkSections.IsEmpty)
            {
                var msg = $"AgvcConnector :可通行路徑數量為[{queReserveOkSections.Count}]，無法清除已通過的路徑。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                return;
            }
            else
            {
                queReserveOkSections.TryDequeue(out MapSection passSection);
                string passSectionId = passSection.Id;
                OnPassReserveSectionEvent?.Invoke(this, passSectionId);
                var msg = $"AgvcConnector : 清除已通過路徑[{passSectionId}]。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
        }
        public void OnGetReserveOk(string sectionId)
        {
            if (queNeedReserveSections.IsEmpty)
            {
                var msg = $"AgvcConnector : 收到{sectionId}通行權但延攬失敗，因為未取得通行權路段清單為空";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                return;
            }

            if (!IsAskReserveStop && !mainFlowHandler.IsMoveEnd)
            {
                queNeedReserveSections.TryPeek(out MapSection needReserveSection);

                if (needReserveSection.Id == sectionId || "XXX" == sectionId)
                {
                    queNeedReserveSections.TryDequeue(out MapSection aReserveOkSection);
                    queReserveOkSections.Enqueue(aReserveOkSection);
                    OnReserveOkEvent?.Invoke(this, aReserveOkSection.Id);
                    mainFlowHandler.UpdateMoveControlReserveOkPositions(needReserveSection);
                    ReserveOkAskNext = true;
                }
                else
                {
                    var msg = $"AgvcConnector : 收到{sectionId}通行權但延攬失敗，因為需要通行權路段為{needReserveSection.Id}";
                    OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                }
            }
        }
        public void ClearAllReserve()
        {
            IsAskReservePause = true;
            ClearGotReserveOkSections();
            ClearNeedReserveSections();
            IsAskReservePause = false;
            var msg = $"AgvcConnector : 清除所有通行權。";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void AskGuideAddressesAndSections(MoveCmdInfo moveCmdInfo)
        {
            Send_Cmd138_GuideInfoRequest(theVehicle.AseMoveStatus.LastAddress.Id, moveCmdInfo.EndAddress.Id);
        }
        #endregion

        public void SendAgvcConnectorFormCommands(int cmdNum, Dictionary<string, string> pairs)
        {
            try
            {
                WrapperMessage wrappers = new WrapperMessage();

                var cmdType = (EnumCmdNum)cmdNum;
                switch (cmdType)
                {
                    case EnumCmdNum.Cmd31_TransferRequest:
                        {
                            ID_31_TRANS_REQUEST aCmd = new ID_31_TRANS_REQUEST();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.DestinationAdr = pairs["DestinationAdr"];
                            aCmd.LoadAdr = pairs["LoadAdr"];
                            wrappers.ID = WrapperMessage.TransReqFieldNumber;
                            wrappers.TransReq = aCmd;
                            break;
                        }
                    case EnumCmdNum.Cmd32_TransferCompleteResponse:
                        {
                            ID_32_TRANS_COMPLETE_RESPONSE aCmd = new ID_32_TRANS_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.TranCmpRespFieldNumber;
                            wrappers.TranCmpResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                        {
                            ID_35_CST_ID_RENAME_REQUEST aCmd = new ID_35_CST_ID_RENAME_REQUEST();
                            aCmd.NEWCSTID = pairs["NEWCSTID"];
                            aCmd.OLDCSTID = pairs["OLDCSTID"];

                            wrappers.ID = WrapperMessage.CSTIDRenameReqFieldNumber;
                            wrappers.CSTIDRenameReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd36_TransferEventResponse:
                        {
                            ID_36_TRANS_EVENT_RESPONSE aCmd = new ID_36_TRANS_EVENT_RESPONSE();
                            aCmd.IsBlockPass = PassTypeParse(pairs["IsBlockPass"]);
                            aCmd.IsReserveSuccess = ReserveResultParse(pairs["IsReserveSuccess"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.ImpTransEventRespFieldNumber;
                            wrappers.ImpTransEventResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd37_TransferCancelRequest:
                        {
                            ID_37_TRANS_CANCEL_REQUEST aCmd = new ID_37_TRANS_CANCEL_REQUEST();
                            aCmd.CmdID = pairs["CmdID"];

                            wrappers.ID = WrapperMessage.TransCancelReqFieldNumber;
                            wrappers.TransCancelReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd39_PauseRequest:
                        {
                            ID_39_PAUSE_REQUEST aCmd = new ID_39_PAUSE_REQUEST();
                            aCmd.EventType = PauseEventParse(pairs["EventType"]);
                            aCmd.PauseType = PauseTypeParse(pairs["PauseType"]);

                            wrappers.ID = WrapperMessage.PauseReqFieldNumber;
                            wrappers.PauseReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd41_ModeChange:
                        {
                            ID_41_MODE_CHANGE_REQ aCmd = new ID_41_MODE_CHANGE_REQ();
                            aCmd.OperatingVHMode = OperatingVHModeParse(pairs["OperatingVHMode"]);

                            wrappers.ID = WrapperMessage.ModeChangeReqFieldNumber;
                            wrappers.ModeChangeReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd43_StatusRequest:
                        {
                            ID_43_STATUS_REQUEST aCmd = new ID_43_STATUS_REQUEST();

                            wrappers.ID = WrapperMessage.StatusReqFieldNumber;
                            wrappers.StatusReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd44_StatusRequest:
                        {
                            ID_44_STATUS_CHANGE_RESPONSE aCmd = new ID_44_STATUS_CHANGE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.StatusChangeRespFieldNumber;
                            wrappers.StatusChangeResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd45_PowerOnoffRequest:
                        {
                            ID_45_POWER_OPE_REQ aCmd = new ID_45_POWER_OPE_REQ();
                            aCmd.OperatingPowerMode = OperatingPowerModeParse(pairs["OperatingPowerMode"]);

                            wrappers.ID = WrapperMessage.PowerOpeReqFieldNumber;
                            wrappers.PowerOpeReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd51_AvoidRequest:
                        {
                            ID_51_AVOID_REQUEST aCmd = new ID_51_AVOID_REQUEST();
                            aCmd.GuideAddresses.AddRange(StringSpilter(pairs["GuideAddresses"]));
                            aCmd.GuideSections.AddRange(StringSpilter(pairs["GuideSections"]));

                            wrappers.ID = WrapperMessage.AvoidReqFieldNumber;
                            wrappers.AvoidReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                        {
                            ID_52_AVOID_COMPLETE_RESPONSE aCmd = new ID_52_AVOID_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AvoidCompleteRespFieldNumber;
                            wrappers.AvoidCompleteResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd71_RangeTeachRequest:
                        {
                            ID_71_RANGE_TEACHING_REQUEST aCmd = new ID_71_RANGE_TEACHING_REQUEST();
                            aCmd.FromAdr = pairs["FromAdr"];
                            aCmd.ToAdr = pairs["ToAdr"];

                            wrappers.ID = WrapperMessage.RangeTeachingReqFieldNumber;
                            wrappers.RangeTeachingReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd72_RangeTeachCompleteResponse:
                        {
                            ID_72_RANGE_TEACHING_COMPLETE_RESPONSE aCmd = new ID_72_RANGE_TEACHING_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.RangeTeachingCmpRespFieldNumber;
                            wrappers.RangeTeachingCmpResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd74_AddressTeachResponse:
                        {
                            ID_74_ADDRESS_TEACH_RESPONSE aCmd = new ID_74_ADDRESS_TEACH_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AddressTeachRespFieldNumber;
                            wrappers.AddressTeachResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd91_AlarmResetRequest:
                        {
                            ID_91_ALARM_RESET_REQUEST aCmd = new ID_91_ALARM_RESET_REQUEST();

                            wrappers.ID = WrapperMessage.AlarmResetReqFieldNumber;
                            wrappers.AlarmResetReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd94_AlarmResponse:
                        {
                            ID_94_ALARM_RESPONSE aCmd = new ID_94_ALARM_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);


                            break;
                        }
                    case EnumCmdNum.Cmd131_TransferResponse:
                        {
                            ID_131_TRANS_RESPONSE aCmd = new ID_131_TRANS_RESPONSE();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.NgReason = pairs["NgReason"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.TransRespFieldNumber;
                            wrappers.TransResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd132_TransferCompleteReport:
                        {
                            ID_132_TRANS_COMPLETE_REPORT aCmd = new ID_132_TRANS_COMPLETE_REPORT();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.CmdDistance = int.Parse(pairs["CmdDistance"]);
                            aCmd.CmdPowerConsume = uint.Parse(pairs["CmdPowerConsume"]);
                            aCmd.CmpStatus = CompleteStatusParse(pairs["CmpStatus"]);
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];

                            wrappers.ID = WrapperMessage.TranCmpRepFieldNumber;
                            wrappers.TranCmpRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd134_TransferEventReport:
                        {
                            ID_134_TRANS_EVENT_REP aCmd = new ID_134_TRANS_EVENT_REP();
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];
                            aCmd.EventType = EventTypeParse(pairs["EventType"]);
                            aCmd.DrivingDirection = (DriveDirction)Enum.Parse(typeof(DriveDirction), pairs["DrivingDirection"].Trim());

                            wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                            wrappers.TransEventRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd135_CarrierIdRenameResponse:
                        {
                            ID_135_CST_ID_RENAME_RESPONSE aCmd = new ID_135_CST_ID_RENAME_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.CSTIDRenameRespFieldNumber;
                            wrappers.CSTIDRenameResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd136_TransferEventReport:
                        {
                            ID_136_TRANS_EVENT_REP aCmd = new ID_136_TRANS_EVENT_REP();
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];
                            aCmd.EventType = EventTypeParse(pairs["EventType"]);

                            wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                            wrappers.ImpTransEventRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd137_TransferCancelResponse:
                        {
                            ID_137_TRANS_CANCEL_RESPONSE aCmd = new ID_137_TRANS_CANCEL_RESPONSE();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.TransCancelRespFieldNumber;
                            wrappers.TransCancelResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd139_PauseResponse:
                        {
                            ID_139_PAUSE_RESPONSE aCmd = new ID_139_PAUSE_RESPONSE();
                            aCmd.EventType = PauseEventParse(pairs["EventType"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.PauseRespFieldNumber;
                            wrappers.PauseResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd141_ModeChangeResponse:
                        {
                            ID_141_MODE_CHANGE_RESPONSE aCmd = new ID_141_MODE_CHANGE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.ModeChangeRespFieldNumber;
                            wrappers.ModeChangeResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd143_StatusResponse:
                        {
                            //TODO: 補完屬性
                            ID_143_STATUS_RESPONSE aCmd = new ID_143_STATUS_RESPONSE();
                            aCmd.ActionStatus = VHActionStatusParse(pairs["ActionStatus"]);
                            aCmd.BatteryCapacity = uint.Parse(pairs["BatteryCapacity"]);
                            aCmd.BatteryTemperature = int.Parse(pairs["BatteryTemperature"]);
                            aCmd.BlockingStatus = VhStopSingleParse(pairs["BlockingStatus"]);
                            aCmd.ChargeStatus = VhChargeStatusParse(pairs["ChargeStatus"]);
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];

                            wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                            wrappers.StatusReqResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd144_StatusReport:
                        {
                            //TODO: 補完屬性
                            ID_144_STATUS_CHANGE_REP aCmd = new ID_144_STATUS_CHANGE_REP();
                            aCmd.ActionStatus = VHActionStatusParse(pairs["ActionStatus"]);
                            aCmd.BatteryCapacity = uint.Parse(pairs["BatteryCapacity"]);
                            aCmd.BatteryTemperature = int.Parse(pairs["BatteryTemperature"]);
                            aCmd.BlockingStatus = VhStopSingleParse(pairs["BlockingStatus"]);
                            aCmd.ChargeStatus = VhChargeStatusParse(pairs["ChargeStatus"]);

                            wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                            wrappers.StatueChangeRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd151_AvoidResponse:
                        {
                            ID_151_AVOID_RESPONSE aCmd = new ID_151_AVOID_RESPONSE();
                            aCmd.NgReason = pairs["NgReason"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AvoidRespFieldNumber;
                            wrappers.AvoidResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd152_AvoidCompleteReport:
                        {
                            ID_152_AVOID_COMPLETE_REPORT aCmd = new ID_152_AVOID_COMPLETE_REPORT();
                            aCmd.CmpStatus = int.Parse(pairs["CmpStatus"]);

                            wrappers.ID = WrapperMessage.AvoidCompleteRepFieldNumber;
                            wrappers.AvoidCompleteRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd171_RangeTeachResponse:
                        {
                            ID_171_RANGE_TEACHING_RESPONSE aCmd = new ID_171_RANGE_TEACHING_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.RangeTeachingRespFieldNumber;
                            wrappers.RangeTeachingResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd172_RangeTeachCompleteReport:
                        {
                            ID_172_RANGE_TEACHING_COMPLETE_REPORT aCmd = new ID_172_RANGE_TEACHING_COMPLETE_REPORT();
                            aCmd.CompleteCode = int.Parse(pairs["CompleteCode"]);
                            aCmd.FromAdr = pairs["FromAdr"];
                            aCmd.SecDistance = uint.Parse(pairs["SecDistance"]);
                            aCmd.ToAdr = pairs["ToAdr"];

                            wrappers.ID = WrapperMessage.RangeTeachingCmpRepFieldNumber;
                            wrappers.RangeTeachingCmpRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd174_AddressTeachReport:
                        {
                            ID_174_ADDRESS_TEACH_REPORT aCmd = new ID_174_ADDRESS_TEACH_REPORT();
                            aCmd.Addr = pairs["Addr"];
                            aCmd.Position = int.Parse(pairs["Position"]);

                            wrappers.ID = WrapperMessage.AddressTeachRepFieldNumber;
                            wrappers.AddressTeachRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd191_AlarmResetResponse:
                        {
                            ID_191_ALARM_RESET_RESPONSE aCmd = new ID_191_ALARM_RESET_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AlarmResetRespFieldNumber;
                            wrappers.AlarmResetResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd194_AlarmReport:
                        {
                            ID_194_ALARM_REPORT aCmd = new ID_194_ALARM_REPORT();
                            aCmd.ErrCode = pairs["ErrCode"];
                            aCmd.ErrDescription = pairs["ErrDescription"];
                            aCmd.ErrStatus = ErrorStatusParse(pairs["ErrStatus"]);

                            wrappers.ID = WrapperMessage.AlarmRepFieldNumber;
                            wrappers.AlarmRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd000_EmptyCommand:
                    default:
                        {
                            ID_1_HOST_BASIC_INFO_VERSION_REP aCmd = new ID_1_HOST_BASIC_INFO_VERSION_REP();

                            wrappers.ID = WrapperMessage.HostBasicInfoRepFieldNumber;
                            wrappers.HostBasicInfoRep = aCmd;


                            break;
                        }
                }

                SendCommandWrapper(wrappers);  //似乎是SendFunction底層會咬住等待回應所以開THD去發  
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void TriggerConnect(bool v)
        {
            OnConnectionChangeEvent?.Invoke(this, v);
        }

        private string[] StringSpilter(string v)
        {
            v = v.Trim(new char[] { ' ', '[', ']' });
            if (string.IsNullOrEmpty(v))
            {
                return new string[1] { " " };
            }
            return v.Split(',');
        }

        public void AseBatteryControl_OnBatteryPercentageChangeEvent(object sender, double batteryPercentage)
        {
            BatteryPercentageChangeReport(MethodBase.GetCurrentMethod().Name, (ushort)batteryPercentage);
        }

        private void BatteryPercentageChangeReport(string sender, ushort batteryPercentage)
        {
            Send_Cmd144_StatusChangeReport(sender, batteryPercentage);
        }

        public void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            if (theVehicle.ErrorStatus == VhStopSingle.Off && alarmHandler.HasAlarm)
            {
                theVehicle.ErrorStatus = VhStopSingle.On;
                StatusChangeReport();
            }
            Send_Cmd194_AlarmReport(alarm.Id.ToString(), ErrorStatus.ErrSet);
        }
        public void AlarmHandler_OnPlcResetOneAlarmEvent(object sender, Alarm alarm)
        {
            if (theVehicle.ErrorStatus == VhStopSingle.On && !alarmHandler.HasAlarm)
            {
                theVehicle.ErrorStatus = VhStopSingle.Off;
                StatusChangeReport();
            }
            Send_Cmd194_AlarmReport(alarm.Id.ToString(), ErrorStatus.ErrReset);
        }
        public void AlarmHandler_OnResetAllAlarmsEvent(object sender, string msg)
        {
            if (theVehicle.ErrorStatus == VhStopSingle.On)
            {
                theVehicle.ErrorStatus = VhStopSingle.Off;
                StatusChangeReport();
            }
            Send_Cmd194_AlarmReport("0", ErrorStatus.ErrReset);

            //foreach (var alarm in alarms)
            //{
            //    Send_Cmd194_AlarmReport(alarm.Id.ToString(), ErrorStatus.ErrReset);
            //}
        }

        #region Public Functions

        public void ReportSectionPass()
        {
            Send_Cmd134_TransferEventReport(EventType.AdrPass);
        }
        public void ReportAddressPass()
        {
            if (!IsNeerlyNoMove())
            {
                Send_Cmd134_TransferEventReport(EventType.AdrPass);
            }
        }
        private bool IsNeerlyNoMove()
        {
            var realPos = theVehicle.AseMoveStatus.LastMapPosition;
            var lastAddr = theVehicle.AseMoveStatus.LastAddress;
            if (string.IsNullOrEmpty(lastAddr.Id)) return true;
            return Math.Abs(realPos.X - lastAddr.Position.X) <= agvcConnectorConfig.NeerlyNoMoveRangeMm && Math.Abs(realPos.Y - lastAddr.Position.Y) <= agvcConnectorConfig.NeerlyNoMoveRangeMm;
        }
        public void LoadArrivals(string cmdId)
        {
            Send_Cmd136_TransferEventReport(EventType.LoadArrivals, cmdId);
        }
        public void Loading(string cmdId)
        {
            Send_Cmd136_TransferEventReport(EventType.Vhloading, cmdId);
        }
        public bool IsCstIdReadReplyOk(TransferStep transferStep, EnumCstIdReadResult result)
        {
            Task<bool> actionResult = Send_Cmd136_CstIdReadReport(transferStep, result);
            return actionResult.Result;
        }
        public void TransferComplete(AgvcTransCmd agvcTransCmd)
        {
            AgvcTransCmd cmd = agvcTransCmd;
            Send_Cmd132_TransferCompleteReport(cmd, 0);
        }
        public void LoadComplete(string cmdId)
        {
            AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer[cmdId];
            agvcTransCmd.CommandState = CommandState.UnloadEnroute;
            StatusChangeReport();
            Send_Cmd136_TransferEventReport(EventType.LoadComplete, cmdId);
        }
        public void UnloadArrivals(string cmdId)
        {
            Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, cmdId);
        }
        public void Unloading(string cmdId)
        {
            Send_Cmd136_TransferEventReport(EventType.Vhunloading, cmdId);
        }
        public void UnloadComplete(string cmdId)
        {
            AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer[cmdId];
            agvcTransCmd.CommandState = CommandState.None;
            StatusChangeReport();
            Send_Cmd136_TransferEventReport(EventType.UnloadComplete, cmdId);
        }
        public void MoveArrival()
        {
            Send_Cmd134_TransferEventReport(EventType.AdrOrMoveArrivals);
        }
        public void AvoidComplete()
        {
            Send_Cmd152_AvoidCompleteReport(0);
            mainFlowHandler.GoNextTransferStep = true;
        }
        public void AvoidFail()
        {
            Send_Cmd152_AvoidCompleteReport(1);
        }
        public bool IsAskReserveAlive() => (thdAskReserve != null) && thdAskReserve.IsAlive;
        public void NoCommand()
        {
            theVehicle.ActionStatus = VHActionStatus.NoCommand;
            StatusChangeReport();
        }
        public void Commanding()
        {
            theVehicle.ActionStatus = VHActionStatus.Commanding;
            StatusChangeReport();
        }
        public void ReplyTransferCommand(string cmdId, CommandActionType type, ushort seqNum, int replyCode, string reason)
        {
            Send_Cmd131_TransferResponse(cmdId, type, seqNum, replyCode, reason);
        }
        public void ReplyAvoidCommand(AseMovingGuide aseMovingGuide, int replyCode, string reason)
        {
            Send_Cmd151_AvoidResponse(aseMovingGuide.SeqNum, replyCode, reason);
        }
        public void ChargHandshaking()
        {
            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusHandshaking;
            StatusChangeReport();
        }
        public void Charging()
        {
            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusCharging;
            StatusChangeReport();
        }
        public void ChargeOff()
        {
            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusNone;
            StatusChangeReport();
        }
        public void PauseReply(ushort seqNum, int replyCode, PauseEvent type)
        {
            Send_Cmd139_PauseResponse(seqNum, replyCode, type);
        }
        public void CancelAbortReply(ushort iSeqNum, int replyCode, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            Send_Cmd137_TransferCancelResponse(iSeqNum, replyCode, receive);
        }
        public void DoOverride(ID_31_TRANS_REQUEST transRequest, ushort iSeqNum)
        {
            //AgvcOverrideCmd agvcOverrideCmd = (AgvcOverrideCmd)ConvertAgvcTransCmdIntoPackage(transRequest, iSeqNum);
            //ShowTransferCmdToForm(agvcOverrideCmd);
            //OnOverrideCommandEvent?.Invoke(this, agvcOverrideCmd);
        }
        public void DoBasicTransferCmd(ID_31_TRANS_REQUEST transRequest, ushort iSeqNum)
        {
            AgvcTransCmd agvcTransCmd = ConvertAgvcTransCmdIntoPackage(transRequest, iSeqNum);
            ShowTransferCmdToForm(agvcTransCmd);
            OnInstallTransferCommandEvent?.Invoke(this, agvcTransCmd);
        }
        public void StatusChangeReport()
        {
            Send_Cmd144_StatusChangeReport();
        }
        private void ShowTransferCmdToForm(AgvcTransCmd agvcTransCmd)
        {
            var msg = $"收到{agvcTransCmd.AgvcTransCommandType},loadAdr={agvcTransCmd.LoadAddressId},unloadAdr={agvcTransCmd.UnloadAddressId}.";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public bool IsConnected() => ClientAgent == null ? false : ClientAgent.IsConnection;

        #endregion

        #region Send_Or_Receive_CmdNum
        public void Receive_Cmd94_AlarmResponse(object sender, TcpIpEventArgs e)
        {
            ID_94_ALARM_RESPONSE receive = (ID_94_ALARM_RESPONSE)e.objPacket;
        }
        public void Send_Cmd194_AlarmReport(string alarmCode, ErrorStatus status)
        {
            try
            {
                if (theVehicle.AutoState == EnumAutoState.Auto)
                {
                    ID_194_ALARM_REPORT iD_194_ALARM_REPORT = new ID_194_ALARM_REPORT();
                    iD_194_ALARM_REPORT.ErrCode = alarmCode;
                    iD_194_ALARM_REPORT.ErrStatus = status;

                    WrapperMessage wrappers = new WrapperMessage();
                    wrappers.ID = WrapperMessage.AlarmRepFieldNumber;
                    wrappers.AlarmRep = iD_194_ALARM_REPORT;

                    SendCommandWrapper(wrappers);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }


        public void Receive_Cmd91_AlarmResetRequest(object sender, TcpIpEventArgs e)
        {
            ID_91_ALARM_RESET_REQUEST receive = (ID_91_ALARM_RESET_REQUEST)e.objPacket;

            mainFlowHandler.ResetAllarms();

            int replyCode = 0;
            Send_Cmd191_AlarmResetResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd191_AlarmResetResponse(ushort seqNum, int replyCode)
        {
            try
            {
                ID_191_ALARM_RESET_RESPONSE iD_191_ALARM_RESET_RESPONSE = new ID_191_ALARM_RESET_RESPONSE();
                iD_191_ALARM_RESET_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AlarmResetRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.AlarmResetResp = iD_191_ALARM_RESET_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd74_AddressTeachResponse(object sender, TcpIpEventArgs e)
        {
            ID_74_ADDRESS_TEACH_RESPONSE receive = (ID_74_ADDRESS_TEACH_RESPONSE)e.objPacket;



        }
        public void Send_Cmd174_AddressTeachReport(string addressId, int position)
        {
            try
            {
                //TODO: Teaching port address

                ID_174_ADDRESS_TEACH_REPORT iD_174_ADDRESS_TEACH_REPORT = new ID_174_ADDRESS_TEACH_REPORT();
                iD_174_ADDRESS_TEACH_REPORT.Addr = addressId;
                iD_174_ADDRESS_TEACH_REPORT.Position = position;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AddressTeachRepFieldNumber;
                wrappers.AddressTeachRep = iD_174_ADDRESS_TEACH_REPORT;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd52_AvoidCompleteResponse(object sender, TcpIpEventArgs e)
        {
            ID_52_AVOID_COMPLETE_RESPONSE receive = (ID_52_AVOID_COMPLETE_RESPONSE)e.objPacket;
            if (receive.ReplyCode != 0)
            {
                //Alarm and Log
            }
        }
        public void Send_Cmd152_AvoidCompleteReport(int completeStatus)
        {
            try
            {
                ID_152_AVOID_COMPLETE_REPORT iD_152_AVOID_COMPLETE_REPORT = new ID_152_AVOID_COMPLETE_REPORT();
                iD_152_AVOID_COMPLETE_REPORT.CmpStatus = completeStatus;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AvoidCompleteRepFieldNumber;
                wrappers.AvoidCompleteRep = iD_152_AVOID_COMPLETE_REPORT;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd51_AvoidRequest(object sender, TcpIpEventArgs e)
        {
            ID_51_AVOID_REQUEST receive = (ID_51_AVOID_REQUEST)e.objPacket;
            OnMessageShowOnMainFormEvent?.Invoke(this, $"收到避車指令");
            AseMovingGuide aseMovingGuide = new AseMovingGuide(receive, e.iSeqNum);
            ShowAvoidRequestToForm(aseMovingGuide);
            OnAvoideRequestEvent?.Invoke(this, aseMovingGuide);
        }
        private void ShowAvoidRequestToForm(AseMovingGuide aseMovingGuide)
        {
            var msg = $"收到避車指令,避車終點={aseMovingGuide.ToAddressId}.";

            msg += Environment.NewLine + "避車路徑ID:";
            foreach (var secId in aseMovingGuide.GuideSectionIds)
            {
                msg += $"({secId})";
            }
            msg += Environment.NewLine + "避車過點ID:";
            foreach (var adrId in aseMovingGuide.GuideAddressIds)
            {
                msg += $"({adrId})";
            }
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void Send_Cmd151_AvoidResponse(ushort seqNum, int replyCode, string reason)
        {
            try
            {
                ID_151_AVOID_RESPONSE iD_151_AVOID_RESPONSE = new ID_151_AVOID_RESPONSE();
                iD_151_AVOID_RESPONSE.ReplyCode = replyCode;
                iD_151_AVOID_RESPONSE.NgReason = reason;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AvoidRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.AvoidResp = iD_151_AVOID_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void Receive_Cmd44_StatusRequest(object sender, TcpIpEventArgs e)
        {
            ID_44_STATUS_CHANGE_RESPONSE receive = (ID_44_STATUS_CHANGE_RESPONSE)e.objPacket; // Cmd43's object is empty
        }
        public void Send_Cmd144_StatusChangeReport()
        {
            try
            {
                ID_144_STATUS_CHANGE_REP report = GetCmd144ReportBody();

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = report;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }
        public void Send_Cmd144_StatusChangeReport(string sender, ushort batteryPercentage)
        {
            try
            {
                ID_144_STATUS_CHANGE_REP report = GetCmd144ReportBody();
                report.BatteryCapacity = batteryPercentage;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = report;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }
        private ID_144_STATUS_CHANGE_REP GetCmd144ReportBody()
        {
            ID_144_STATUS_CHANGE_REP report = new ID_144_STATUS_CHANGE_REP();
            report.ModeStatus = VHModeStatusParse(theVehicle.AutoState);
            report.ActionStatus = theVehicle.ActionStatus;
            report.PowerStatus = theVehicle.PowerStatus;
            report.ObstacleStatus = theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Block ? VhStopSingle.On : VhStopSingle.Off;
            report.ReserveStatus = theVehicle.AseMovingGuide.ReserveStop;
            report.BlockingStatus = theVehicle.BlockingStatus;
            report.PauseStatus = theVehicle.AseMovingGuide.PauseStatus;
            report.ErrorStatus = theVehicle.ErrorStatus;
            report.DrivingDirection = theVehicle.DrivingDirection;
            report.BatteryCapacity = (uint)theVehicle.AseBatteryStatus.Percentage;
            report.BatteryTemperature = (int)theVehicle.AseBatteryStatus.Temperature;
            report.ChargeStatus = VhChargeStatusParse(theVehicle.IsCharging);
            report.XAxis = theVehicle.AseMoveStatus.LastMapPosition.X;
            report.YAxis = theVehicle.AseMoveStatus.LastMapPosition.Y;
            report.Speed = theVehicle.AseMoveStatus.Speed;
            AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);
            report.WillPassGuideSection.Clear();
            report.WillPassGuideSection.AddRange(aseMovingGuide.GuideSectionIds);

            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
            report.CurrentSecID = aseMoveStatus.LastSection.Id;
            report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
            report.DirectionAngle = aseMoveStatus.MovingDirection;
            report.VehicleAngle = aseMoveStatus.HeadDirection;

            List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
            report.CmdId1 = agvcTransCmds.Count > 0 ? agvcTransCmds[0].CommandId : "";
            report.CmsState1 = agvcTransCmds.Count > 0 ? agvcTransCmds[0].CommandState : CommandState.None;
            report.CmdId2 = agvcTransCmds.Count > 1 ? agvcTransCmds[1].CommandId : "";
            report.CmsState2 = agvcTransCmds.Count > 1 ? agvcTransCmds[1].CommandState : CommandState.None;

            report.HasCstL = theVehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
            report.CstIdL = theVehicle.AseCarrierSlotL.CarrierId;
            report.HasCstR = theVehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
            report.CstIdR = theVehicle.AseCarrierSlotR.CarrierId;

            return report;
        }

        private void Receive_Cmd43_StatusRequest(object sender, TcpIpEventArgs e)
        {
            ID_43_STATUS_REQUEST receive = (ID_43_STATUS_REQUEST)e.objPacket;
            var receiveTime = receive.SystemTime; //可以記錄AGVC最後發送時間

            Send_Cmd143_StatusResponse(e.iSeqNum);
        }
        public void Send_Cmd143_StatusResponse(ushort seqNum)
        {
            try
            {
                ID_143_STATUS_RESPONSE response = new ID_143_STATUS_RESPONSE();
                response.ModeStatus = VHModeStatusParse(theVehicle.AutoState);
                response.ActionStatus = theVehicle.ActionStatus;
                response.PowerStatus = theVehicle.PowerStatus;
                response.ObstacleStatus = theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Block ? VhStopSingle.On : VhStopSingle.Off;
                response.ReserveStatus = theVehicle.AseMovingGuide.ReserveStop;
                response.BlockingStatus = theVehicle.BlockingStatus;
                response.PauseStatus = theVehicle.AseMovingGuide.PauseStatus;
                response.ErrorStatus = theVehicle.ErrorStatus;
                response.ObstDistance = theVehicle.ObstDistance;
                response.ObstVehicleID = theVehicle.ObstVehicleID;
                response.HasCstL = theVehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
                response.CstIdL = theVehicle.AseCarrierSlotL.CarrierId;
                response.HasCstR = theVehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
                response.CstIdR = theVehicle.AseCarrierSlotR.CarrierId;
                response.ChargeStatus = VhChargeStatusParse(theVehicle.IsCharging);
                response.BatteryCapacity = (uint)theVehicle.AseBatteryStatus.Percentage;
                response.BatteryTemperature = (int)theVehicle.AseBatteryStatus.Temperature;
                response.XAxis = theVehicle.AseMoveStatus.LastMapPosition.X;
                response.YAxis = theVehicle.AseMoveStatus.LastMapPosition.Y;
                response.DirectionAngle = theVehicle.AseMoveStatus.MovingDirection;
                response.VehicleAngle = theVehicle.AseMoveStatus.HeadDirection;
                response.Speed = theVehicle.AseMoveStatus.Speed;
                response.StoppedBlockID = theVehicle.StoppedBlockID;

                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                response.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                response.CurrentSecID = aseMoveStatus.LastSection.Id;
                response.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                response.DrivingDirection = DriveDirctionParse(aseMoveStatus.LastSection.CmdDirection);

                List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                response.CmdId1 = agvcTransCmds.Count > 0 ? agvcTransCmds[0].CommandId : "";
                response.CmsState1 = agvcTransCmds.Count > 0 ? agvcTransCmds[0].CommandState : CommandState.None;
                response.CmdId2 = agvcTransCmds.Count > 1 ? agvcTransCmds[1].CommandId : "";
                response.CmsState2 = agvcTransCmds.Count > 1 ? agvcTransCmds[1].CommandState : CommandState.None;


                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.StatusReqResp = response;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd41_ModeChange(object sender, TcpIpEventArgs e)
        {
            ID_41_MODE_CHANGE_REQ receive = (ID_41_MODE_CHANGE_REQ)e.objPacket;
            int replyCode = 1;
            Send_Cmd141_ModeChangeResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd141_ModeChangeResponse(ushort seqNum, int replyCode)
        {
            try
            {
                ID_141_MODE_CHANGE_RESPONSE iD_141_MODE_CHANGE_RESPONSE = new ID_141_MODE_CHANGE_RESPONSE();
                iD_141_MODE_CHANGE_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ModeChangeRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ModeChangeResp = iD_141_MODE_CHANGE_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd39_PauseRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_39_PAUSE_REQUEST receive = (ID_39_PAUSE_REQUEST)e.objPacket;

                var msg = $"AgvcConnector : 收到[{receive.EventType}]命令。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);

                switch (receive.EventType)
                {
                    case PauseEvent.Continue:
                        mainFlowHandler.AgvcConnector_OnCmdResumeEvent(e.iSeqNum, receive.EventType);
                        break;
                    case PauseEvent.Pause:
                        mainFlowHandler.AgvcConnector_OnCmdPauseEvent(e.iSeqNum, receive.EventType);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Send_Cmd139_PauseResponse(ushort seqNum, int replyCode, PauseEvent eventType)
        {
            try
            {
                ID_139_PAUSE_RESPONSE iD_139_PAUSE_RESPONSE = new ID_139_PAUSE_RESPONSE();
                iD_139_PAUSE_RESPONSE.EventType = eventType;
                iD_139_PAUSE_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.PauseRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.PauseResp = iD_139_PAUSE_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd38_GuideInfoResponse(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_38_GUIDE_INFO_RESPONSE response = (ID_38_GUIDE_INFO_RESPONSE)e.objPacket;
                theVehicle.AseMovingGuide = new AseMovingGuide(response);
                IsAskReservePause = true;
                ClearAllReserve();
                mainFlowHandler.SetupAseMovingGuideMovingSections();
                SetupNeedReserveSections();
                mainFlowHandler.IsMoveEnd = false;
                IsAskReservePause = false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Send_Cmd138_GuideInfoRequest(string fromAddress, string toAddress)
        {
            try
            {
                ID_138_GUIDE_INFO_REQUEST request = new ID_138_GUIDE_INFO_REQUEST();
                FitGuideInfos(request.FromToAdrList, fromAddress, toAddress);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.GuideInfoReqFieldNumber;
                wrappers.GuideInfoReq = request;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void FitGuideInfos(RepeatedField<FromToAdr> fromToAdrList, string fromAddress, string toAddress)
        {
            fromToAdrList.Clear();
            FromToAdr fromToAdr = new FromToAdr();
            fromToAdr.From = fromAddress;
            fromToAdr.To = toAddress;
            fromToAdrList.Add(fromToAdr);
        }

        public void Receive_Cmd37_TransferCancelRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                int replyCode = 0;
                ID_37_TRANS_CANCEL_REQUEST receive = (ID_37_TRANS_CANCEL_REQUEST)e.objPacket;

                var msg = $"AgvcConnector : 收到[{receive.CancelAction}]命令。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);

                var cmdId = receive.CmdID.Trim();

                if (theVehicle.ActionStatus == VHActionStatus.NoCommand)
                {
                    replyCode = 1;
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive);
                    var ngMsg = $"AgvcConnector : 車輛無命令，拒絕[{receive.CancelAction}]命令，";
                    OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                    return;
                }

                if (receive.CancelAction == CancelActionType.CmdEms)
                {
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive);
                    alarmHandler.SetAlarm(000037);
                    mainFlowHandler.StopClearAndReset();
                    return;
                }

                if (!theVehicle.AgvcTransCmdBuffer.ContainsKey(cmdId))
                {
                    replyCode = 1;
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive);
                    var ngMsg = $"AgvcConnector : 車上無ID({cmdId})命令可以取消，拒絕[{receive.CancelAction}]命令，";
                    OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                    return;
                }

                switch (receive.CancelAction)
                {
                    case CancelActionType.CmdCancel:
                    case CancelActionType.CmdAbort:
                        mainFlowHandler.AgvcConnector_OnCmdCancelAbortEvent(e.iSeqNum, receive);
                        break;
                    case CancelActionType.CmdCancelIdMismatch:
                    case CancelActionType.CmdCancelIdReadFailed:
                        alarmHandler.ResetAllAlarms();
                        mainFlowHandler.StopClearAndReset();
                        break;
                    case CancelActionType.CmdNone:
                    default:
                        replyCode = 1;
                        Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Send_Cmd137_TransferCancelResponse(ushort seqNum, int replyCode, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            try
            {
                ID_137_TRANS_CANCEL_RESPONSE iD_137_TRANS_CANCEL_RESPONSE = new ID_137_TRANS_CANCEL_RESPONSE();
                iD_137_TRANS_CANCEL_RESPONSE.CmdID = receive.CmdID;
                iD_137_TRANS_CANCEL_RESPONSE.CancelAction = receive.CancelAction;
                iD_137_TRANS_CANCEL_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransCancelRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.TransCancelResp = iD_137_TRANS_CANCEL_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd36_TransferEventResponse(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_36_TRANS_EVENT_RESPONSE receive = (ID_36_TRANS_EVENT_RESPONSE)e.objPacket;
                if (receive.EventType == EventType.ReserveReq)
                {
                }
                else if (receive.EventType == EventType.Bcrread)
                {
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Send_Cmd136_TransferEventReport(EventType eventType, string cmdId)
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = aseMoveStatus.LastSection.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                iD_136_TRANS_EVENT_REP.CmdID = cmdId;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        public async Task<bool> Send_Cmd136_CstIdReadReport(TransferStep transferStep, EnumCstIdReadResult readResult, bool isLoadComplete = true)
        {

            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                RobotCommand robotCommand = (RobotCommand)transferStep;
                AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(robotCommand.SlotNumber);

                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = EventType.Bcrread;
                iD_136_TRANS_EVENT_REP.CSTID = aseCarrierSlotStatus.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading ? aseCarrierSlotStatus.CarrierId : "";
                iD_136_TRANS_EVENT_REP.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = aseMoveStatus.LastSection.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                iD_136_TRANS_EVENT_REP.BCRReadResult = BCRReadResultParse(readResult);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                LogSendMsg(wrappers);

                ID_36_TRANS_EVENT_RESPONSE response = new ID_36_TRANS_EVENT_RESPONSE();
                string rtnMsg = "";

                TrxTcpIp.ReturnCode returnCode = await Task.Run<TrxTcpIp.ReturnCode>(() => ClientAgent.TrxTcpIp.sendRecv_Google(wrappers, out response, out rtnMsg, agvcConnectorConfig.RecvTimeoutMs, 0));

                if (returnCode == TrxTcpIp.ReturnCode.Normal)
                {
                    if (response.ReplyAction != ReplyActionType.Continue)
                    {
                        OnMessageShowOnMainFormEvent?.Invoke(this, $"Robot取貨異常，處理方式為[{response.ReplyAction}]，CstID變更為[{response.RenameCarrierID}]");
                        alarmHandler.ResetAllAlarms();
                        if (!string.IsNullOrEmpty(response.RenameCarrierID))
                        {
                            aseCarrierSlotStatus.CarrierId = response.RenameCarrierID.Trim();
                            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
                        }
                        LoadComplete(transferStep.CmdId);
                        AbortCommand(response.ReplyAction, transferStep.CmdId);
                        return false;
                    }
                    else
                    {

                        LoadComplete(transferStep.CmdId);
                        OnMessageShowOnMainFormEvent?.Invoke(this, $"Robot取貨完成");
                        return true;
                    }
                }
                else
                {
                    OnMessageShowOnMainFormEvent?.Invoke(this, $"Robot取貨異常，等待回應逾時");
                    alarmHandler.ResetAllAlarms();
                    LoadComplete(transferStep.CmdId);
                    AbortCommand(readResult, transferStep.CmdId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                OnMessageShowOnMainFormEvent?.Invoke(this, $"Robot取貨異常，Exception");
                alarmHandler.ResetAllAlarms();
                LoadComplete(transferStep.CmdId);
                AbortCommand(readResult, transferStep.CmdId);
                return false;
            }
        }
        private void AbortCommand(ReplyActionType replyActionType, string cmdId)
        {
            switch (replyActionType)
            {
                case ReplyActionType.Continue:
                    break;
                case ReplyActionType.Wait:
                    break;
                case ReplyActionType.Retry:
                    break;
                case ReplyActionType.Cancel:
                    theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = CompleteStatus.Cancel;
                    break;
                case ReplyActionType.Abort:
                    theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = CompleteStatus.Abort;
                    break;
                case ReplyActionType.CancelIdMisnatch:
                    theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = CompleteStatus.IdmisMatch;
                    break;
                case ReplyActionType.CancelIdReadFailed:
                    theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = CompleteStatus.IdreadFailed;
                    break;
                case ReplyActionType.CancelPidFailed:
                    theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = CompleteStatus.Cancel;
                    break;
                default:
                    break;
            }
            mainFlowHandler.AbortCommand(cmdId, theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus);
        }
        private void AbortCommand(EnumCstIdReadResult readResult, string cmdId)
        {
            switch (readResult)
            {

                case EnumCstIdReadResult.Mismatch:
                    theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = CompleteStatus.IdmisMatch;
                    break;
                case EnumCstIdReadResult.Fail:
                    theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = CompleteStatus.IdreadFailed;
                    break;
                case EnumCstIdReadResult.Noraml:
                default:
                    theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = CompleteStatus.VehicleAbort;
                    break;
            }

            mainFlowHandler.AbortCommand(cmdId, theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus);
        }

        public void Send_Cmd136_AskReserve(MapSection mapSection)
        {
            var msg = $"詢問{mapSection.Id}通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();
                report.EventType = EventType.ReserveReq;
                FitReserveInfos(report.ReserveInfos, mapSection);
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = report;


                #region Ask reserve and wait reply
                LogSendMsg(wrappers);

                ID_36_TRANS_EVENT_RESPONSE response = new ID_36_TRANS_EVENT_RESPONSE();
                string rtnMsg = "";

                var returnCode = ClientAgent.TrxTcpIp.sendRecv_Google(wrappers, out response, out rtnMsg, agvcConnectorConfig.RecvTimeoutMs, 0);

                if (returnCode == TrxTcpIp.ReturnCode.Normal)
                {
                    OnReceiveReserveReply(response);
                }
                else
                {
                    IsAgvcRejectReserve = true;
                    string xxmsg = $"詢問{mapSection.Id}通行權結果[{returnCode}][{rtnMsg}]";
                    OnMessageShowOnMainFormEvent?.Invoke(this, xxmsg);
                }
                #endregion

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void FitReserveInfos(RepeatedField<ReserveInfo> reserveInfos, MapSection mapSection)
        {
            reserveInfos.Clear();
            ReserveInfo reserveInfo = new ReserveInfo();
            reserveInfo.ReserveSectionID = mapSection.Id;
            if (mapSection.CmdDirection == EnumCommandDirection.Backward)
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirReverse;
            }
            else if (mapSection.CmdDirection == EnumCommandDirection.None)
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirNone;
            }
            else
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirForward;
            }
            reserveInfos.Add(reserveInfo);
        }
        private void OnReceiveReserveReply(ID_36_TRANS_EVENT_RESPONSE receive)
        {
            if (CanDoReserveWork())
            {
                IsAskReservePause = true;
                string sectionId = receive.ReserveInfos[0].ReserveSectionID;
                if (receive.IsReserveSuccess == ReserveResult.Success)
                {
                    IsAgvcRejectReserve = false;
                    string msg = $"收到{sectionId}通行權可行";
                    OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                    if (theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.On)
                    {
                        theVehicle.AseMovingGuide.ReserveStop = VhStopSingle.Off;
                        StatusChangeReport();
                    }
                    if (!IsAskReserveStop && !mainFlowHandler.IsMoveEnd)
                    {
                        OnGetReserveOk(sectionId);
                    }
                }
                else
                {
                    IsAgvcRejectReserve = true;
                    ReserveOkAskNext = false;
                    string msg = $"收到{sectionId}通行權不可行";
                    OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                    if (theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Idle)
                    {
                        if (theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.Off)
                        {
                            theVehicle.AseMovingGuide.ReserveStop = VhStopSingle.On;
                            StatusChangeReport();
                        }
                    }
                }
                IsAskReservePause = false;
            }
            else
            {
                IsAgvcRejectReserve = false;
            }

        }

        public void Receive_Cmd35_CarrierIdRenameRequest(object sender, TcpIpEventArgs e)
        {
            ID_35_CST_ID_RENAME_REQUEST receive = (ID_35_CST_ID_RENAME_REQUEST)e.objPacket;
            var result = false;
            if (theVehicle.AseCarrierSlotL.CarrierId == receive.OLDCSTID.Trim())
            {
                mainFlowHandler.RenameCstId(EnumSlotNumber.L, receive.NEWCSTID);
                result = true;
            }
            else if (theVehicle.AseCarrierSlotL.CarrierId == receive.OLDCSTID.Trim())
            {
                mainFlowHandler.RenameCstId(EnumSlotNumber.R, receive.NEWCSTID);
                result = true;
            }

            int replyCode = result ? 0 : 1;
            Send_Cmd135_CarrierIdRenameResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd135_CarrierIdRenameResponse(ushort seqNum, int replyCode)
        {
            try
            {
                ID_135_CST_ID_RENAME_RESPONSE iD_135_CST_ID_RENAME_RESPONSE = new ID_135_CST_ID_RENAME_RESPONSE();
                iD_135_CST_ID_RENAME_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.CSTIDRenameRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.CSTIDRenameResp = iD_135_CST_ID_RENAME_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void Send_Cmd134_TransferEventReport(EventType type)
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

            try
            {
                ID_134_TRANS_EVENT_REP id_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                id_134_TRANS_EVENT_REP.EventType = type;
                id_134_TRANS_EVENT_REP.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                id_134_TRANS_EVENT_REP.CurrentSecID = aseMoveStatus.LastSection.Id;
                id_134_TRANS_EVENT_REP.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                id_134_TRANS_EVENT_REP.DrivingDirection = DriveDirctionParse(aseMoveStatus.LastSection.CmdDirection);
                id_134_TRANS_EVENT_REP.XAxis = theVehicle.AseMoveStatus.LastMapPosition.X;
                id_134_TRANS_EVENT_REP.YAxis = theVehicle.AseMoveStatus.LastMapPosition.Y;
                id_134_TRANS_EVENT_REP.Speed = theVehicle.AseMoveStatus.Speed;
                id_134_TRANS_EVENT_REP.DirectionAngle = theVehicle.AseMoveStatus.MovingDirection;
                id_134_TRANS_EVENT_REP.VehicleAngle = theVehicle.AseMoveStatus.HeadDirection;

                mirleLogger.Log(new LogFormat("Info", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Angle=[{aseMoveStatus.MovingDirection}]"));

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = id_134_TRANS_EVENT_REP;

                // SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd32_TransferCompleteResponse(object sender, TcpIpEventArgs e)
        {
            ID_32_TRANS_COMPLETE_RESPONSE receive = (ID_32_TRANS_COMPLETE_RESPONSE)e.objPacket;
            theVehicle.ActionStatus = receive.ReplyCode == 0 ? VHActionStatus.NoCommand : VHActionStatus.Commanding;
            StatusChangeReport();
        }
        public void Send_Cmd132_TransferCompleteReport(AgvcTransCmd agvcTransCmd, int delay = 0)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(agvcTransCmd.SlotNumber);

                var msg = $"命令結束，結束狀態{agvcTransCmd.CompleteStatus}，命令編號{agvcTransCmd.CommandId}";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);

                ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT = new ID_132_TRANS_COMPLETE_REPORT();
                iD_132_TRANS_COMPLETE_REPORT.CmdID = agvcTransCmd.CommandId;
                iD_132_TRANS_COMPLETE_REPORT.CSTID = aseCarrierSlotStatus.CarrierId;
                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = agvcTransCmd.CompleteStatus;
                iD_132_TRANS_COMPLETE_REPORT.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                iD_132_TRANS_COMPLETE_REPORT.CurrentSecID = aseMoveStatus.LastSection.Id;
                iD_132_TRANS_COMPLETE_REPORT.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                iD_132_TRANS_COMPLETE_REPORT.CmdPowerConsume = theVehicle.CmdPowerConsume;
                iD_132_TRANS_COMPLETE_REPORT.CmdDistance = theVehicle.CmdDistance;
                iD_132_TRANS_COMPLETE_REPORT.XAxis = theVehicle.AseMoveStatus.LastMapPosition.X;
                iD_132_TRANS_COMPLETE_REPORT.YAxis = theVehicle.AseMoveStatus.LastMapPosition.Y;
                iD_132_TRANS_COMPLETE_REPORT.DirectionAngle = theVehicle.AseMoveStatus.MovingDirection;
                iD_132_TRANS_COMPLETE_REPORT.VehicleAngle = theVehicle.AseMoveStatus.HeadDirection;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TranCmpRepFieldNumber;
                wrappers.TranCmpRep = iD_132_TRANS_COMPLETE_REPORT;

                SendCommandWrapper(wrappers, false, delay);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd31_TransferRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_31_TRANS_REQUEST transRequest = (ID_31_TRANS_REQUEST)e.objPacket;
                OnMessageShowOnMainFormEvent?.Invoke(this, $"收到傳送指令: {transRequest.CommandAction}");

                switch (transRequest.CommandAction)
                {
                    case CommandActionType.Move:
                    case CommandActionType.Load:
                    case CommandActionType.Unload:
                    case CommandActionType.Loadunload:
                    case CommandActionType.Home:
                    case CommandActionType.Movetocharger:
                        DoBasicTransferCmd(transRequest, e.iSeqNum);
                        break;
                    case CommandActionType.Override:
                        DoOverride(transRequest, e.iSeqNum);
                        return;
                    default:
                        var msg = $"拒絕傳送指令: {transRequest.CommandAction}";
                        OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                        Send_Cmd131_TransferResponse(transRequest.CmdID, transRequest.CommandAction, e.iSeqNum, 1, "Unknow command.");
                        return;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }
        public void Send_Cmd131_TransferResponse(string cmdId, CommandActionType commandAction, ushort seqNum, int replyCode, string reason)
        {
            try
            {
                ID_131_TRANS_RESPONSE iD_131_TRANS_RESPONSE = new ID_131_TRANS_RESPONSE();
                iD_131_TRANS_RESPONSE.CmdID = cmdId;
                iD_131_TRANS_RESPONSE.CommandAction = commandAction;
                iD_131_TRANS_RESPONSE.ReplyCode = replyCode;
                iD_131_TRANS_RESPONSE.NgReason = reason;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.TransResp = iD_131_TRANS_RESPONSE;

                SendCommandWrapper(wrappers, true);

                if (replyCode == 0)
                {
                    Commanding();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private AgvcTransCmd ConvertAgvcTransCmdIntoPackage(ID_31_TRANS_REQUEST transRequest, ushort iSeqNum)
        {
            //解析收到的ID_31_TRANS_REQUEST並且填入AgvcTransCmd     

            switch (transRequest.CommandAction)
            {
                case CommandActionType.Move:
                    return new AgvcMoveCmd(transRequest, iSeqNum);
                case CommandActionType.Load:
                    return new AgvcLoadCmd(transRequest, iSeqNum);
                case CommandActionType.Unload:
                    return new AgvcUnloadCmd(transRequest, iSeqNum);
                case CommandActionType.Loadunload:
                    return new AgvcLoadunloadCmd(transRequest, iSeqNum);
                case CommandActionType.Home:
                    break;
                case CommandActionType.Override:
                    return new AgvcOverrideCmd(transRequest, iSeqNum);
                case CommandActionType.Movetocharger:
                    return new AgvcMoveToChargerCmd(transRequest, iSeqNum);
                default:
                    break;
            }

            return new AgvcTransCmd(transRequest, iSeqNum);
        }
        #endregion

        #region EnumParse
        private VhChargeStatus VhChargeStatusParse(string v)
        {
            try
            {
                v = v.Trim();

                return (VhChargeStatus)Enum.Parse(typeof(VhChargeStatus), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return VhChargeStatus.ChargeStatusCharging;
            }
        }
        private VhChargeStatus VhChargeStatusParse(bool charging)
        {
            return charging ? VhChargeStatus.ChargeStatusCharging : VhChargeStatus.ChargeStatusNone;
        }
        private VhStopSingle VhStopSingleParse(string v)
        {
            try
            {
                v = v.Trim();

                return (VhStopSingle)Enum.Parse(typeof(VhStopSingle), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return VhStopSingle.Off;
            }
        }
        private VHActionStatus VHActionStatusParse(string v)
        {
            try
            {
                v = v.Trim();

                return (VHActionStatus)Enum.Parse(typeof(VHActionStatus), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return VHActionStatus.Commanding;
            }
        }
        private EventType EventTypeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (EventType)Enum.Parse(typeof(EventType), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return EventType.AdrOrMoveArrivals;
            }
        }
        private CompleteStatus CompleteStatusParse(string v)
        {
            try
            {
                v = v.Trim();

                return (CompleteStatus)Enum.Parse(typeof(CompleteStatus), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return CompleteStatus.Abort;
            }
        }
        private OperatingPowerMode OperatingPowerModeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (OperatingPowerMode)Enum.Parse(typeof(OperatingPowerMode), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return OperatingPowerMode.OperatingPowerOff;
            }
        }
        private OperatingVHMode OperatingVHModeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (OperatingVHMode)Enum.Parse(typeof(OperatingVHMode), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return OperatingVHMode.OperatingAuto;
            }
        }
        private PauseType PauseTypeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (PauseType)Enum.Parse(typeof(PauseType), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return PauseType.None;
            }
        }
        private PauseEvent PauseEventParse(string v)
        {
            try
            {
                v = v.Trim();

                return (PauseEvent)Enum.Parse(typeof(PauseEvent), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return PauseEvent.Pause;
            }
        }
        //private CMDCancelType CMDCancelTypeParse(string v)
        //{
        //    try
        //    {
        //        v = v.Trim();

        //        return (CMDCancelType)Enum.Parse(typeof(CMDCancelType), v);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
        //        return CMDCancelType.CmdAbort;
        //    }
        //}
        private ReserveResult ReserveResultParse(string v)
        {
            try
            {
                v = v.Trim();

                return (ReserveResult)Enum.Parse(typeof(ReserveResult), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return ReserveResult.Success;
            }
        }
        private PassType PassTypeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (PassType)Enum.Parse(typeof(PassType), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return PassType.Pass;
            }
        }
        private ErrorStatus ErrorStatusParse(string v)
        {
            try
            {
                v = v.Trim();

                return (ErrorStatus)Enum.Parse(typeof(ErrorStatus), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return ErrorStatus.ErrReset;
            }
        }
        //private ActiveType ActiveTypeParse(string v)
        //{
        //    try
        //    {
        //        v = v.Trim();

        //        return (ActiveType)Enum.Parse(typeof(ActiveType), v);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
        //        return ActiveType.Home;
        //    }
        //}
        //private ControlType ControlTypeParse(string v)
        //{
        //    try
        //    {
        //        v = v.Trim();

        //        return (ControlType)Enum.Parse(typeof(ControlType), v);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
        //        return ControlType.Nothing;
        //    }
        //}
        private VhLoadCSTStatus VhLoadCSTStatusParse(bool loading)
        {
            if (loading)
            {
                return VhLoadCSTStatus.Exist;
            }
            else
            {
                return VhLoadCSTStatus.NotExist;
            }
        }
        private BCRReadResult BCRReadResultParse(EnumCstIdReadResult readResult)
        {
            switch (readResult)
            {
                case EnumCstIdReadResult.Mismatch:
                    return BCRReadResult.BcrMisMatch;
                case EnumCstIdReadResult.Fail:
                    return BCRReadResult.BcrReadFail;
                case EnumCstIdReadResult.Noraml:
                default:
                    return BCRReadResult.BcrNormal;
            }
        }
        private VHModeStatus VHModeStatusParse(EnumAutoState autoState)
        {
            switch (autoState)
            {
                case EnumAutoState.Auto:
                    return VHModeStatus.AutoRemote;
                case EnumAutoState.Manual:
                    return VHModeStatus.Manual;
                case EnumAutoState.PreManual:
                    return VHModeStatus.Manual;
                default:
                    return VHModeStatus.None;
            }
        }
        private DriveDirction DriveDirctionParse(EnumCommandDirection cmdDirection)
        {
            try
            {
                switch (cmdDirection)
                {
                    case EnumCommandDirection.None:
                        return DriveDirction.DriveDirNone;
                    case EnumCommandDirection.Forward:
                        return DriveDirction.DriveDirForward;
                    case EnumCommandDirection.Backward:
                        return DriveDirction.DriveDirReverse;
                    default:
                        return DriveDirction.DriveDirNone;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return DriveDirction.DriveDirNone;
            }
        }
        #endregion

        private void LogException(string classMethodName, string exMsg)
        {
            LogFormat logFormat = new LogFormat("Error", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", exMsg);
            OnLogMsgEvent?.Invoke(this, logFormat);
        }

        private void LogDebug(string classMethodName, string msg)
        {
            LogFormat logFormat = new LogFormat("Debug", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", msg);
            OnLogMsgEvent?.Invoke(this, logFormat);
        }

        private void LogComm(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Comm", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", msg));
        }
    }

}
