using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using com.mirle.iibg3k0.ttc.Common.TCPIP.DecodRawData;
using Google.Protobuf.Collections;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TcpIpClientSample;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Concurrent;
using Mirle.Tools;

namespace Mirle.Agv.Controller
{
    [Serializable]
    public class MiddleAgent
    {
        #region Events
        public event EventHandler<string> OnMessageShowOnMainFormEvent;
        public event EventHandler<AgvcTransCmd> OnInstallTransferCommandEvent;
        public event EventHandler<AgvcOverrideCmd> OnOverrideCommandEvent;
        public event EventHandler<AgvcMoveCmd> OnAvoideRequestEvent;
        public event EventHandler<string> OnCmdReceiveEvent;
        public event EventHandler<string> OnCmdSendEvent;
        public event EventHandler<bool> OnConnectionChangeEvent;
        public event EventHandler<string> OnReserveOkEvent;
        public event EventHandler<string> OnPassReserveSectionEvent;
        #endregion

        private Vehicle theVehicle = Vehicle.Instance;
        private MiddlerConfig middlerConfig;
        private AlarmHandler alarmHandler;
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private MainFlowHandler mainFlowHandler;

        private Thread thdAskReserve;
        private EnumThreadStatus askReserveStatus = EnumThreadStatus.None;
        public EnumThreadStatus AskReserveStatus
        {
            get { return askReserveStatus; }
            private set
            {
                askReserveStatus = value;
                theVehicle.AskReserveStatus = value;
            }
        }
        public EnumThreadStatus PreAskReserveStatus { get; private set; } = EnumThreadStatus.None;

        private ConcurrentQueue<MapSection> queNeedReserveSections = new ConcurrentQueue<MapSection>();
        private ConcurrentQueue<MapSection> queReserveOkSections = new ConcurrentQueue<MapSection>();
        private bool ReserveOkAskNext { get; set; } = false;
        private ConcurrentBag<MapSection> CbagNeedReserveSections { get; set; } = new ConcurrentBag<MapSection>();
        public bool IsAskReservePause { get; private set; }
        public bool IsAskReserveStop { get; private set; }
        private bool IsWaitReserveReply { get; set; }
        public bool IsAgvcRejectReserve { get; set; }

        public TcpIpAgent ClientAgent { get; private set; }
        public string MiddlerAbnormalMsg { get; set; } = "";

        public MiddleAgent(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            middlerConfig = mainFlowHandler.GetMiddlerConfig();
            alarmHandler = mainFlowHandler.GetAlarmHandler();
            mirleLogger = MirleLogger.Instance;

            CreatTcpIpClientAgent();
            Connect();
            StartAskReserve();
        }

        #region Initial

        public void CreatTcpIpClientAgent()
        {

            IDecodReceiveRawData RawDataDecoder = new DecodeRawData_Google(unPackWrapperMsg);

            int clientNum = middlerConfig.ClientNum;
            string clientName = middlerConfig.ClientName;
            string sRemoteIP = middlerConfig.RemoteIp;
            int iRemotePort = middlerConfig.RemotePort;
            string sLocalIP = middlerConfig.LocalIp;
            int iLocalPort = middlerConfig.LocalPort;

            int recv_timeout_ms = middlerConfig.RecvTimeoutMs;                         //等待sendRecv Reply的Time out時間(milliseconds)
            int send_timeout_ms = middlerConfig.SendTimeoutMs;                         //暫時無用
            int max_readSize = middlerConfig.MaxReadSize;                              //暫時無用
            int reconnection_interval_ms = middlerConfig.ReconnectionIntervalMs;       //斷線多久之後再進行一次嘗試恢復連線的動作
            int max_reconnection_count = middlerConfig.MaxReconnectionCount;           //斷線後最多嘗試幾次重新恢復連線 (若設定為0則不進行自動重新連線)
            int retry_count = middlerConfig.RetryCount;                                //SendRecv Time out後要再重複發送的次數

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
        public MiddlerConfig GetMiddlerConfig()
        {
            return middlerConfig;
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
                    mirleLogger.Log( new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Middler : Disconnect Stop, [IsNull={IsClientAgentNull()}][IsConnect={IsConnected()}]"));

                    ClientAgent.stop();
                    //ClientAgent = null;
                }
                else
                {
                    mirleLogger.Log( new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"ClientAgent is null cannot disconnect"));
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
                    mirleLogger.Log(new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Already connect cannot connect again."));
                }
            }
            else
            {
                CreatTcpIpClientAgent();
                Connect();
                mirleLogger.Log(new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"ClientAgent is null cannot connect."));
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
            OnMessageShowOnMainFormEvent?.Invoke(this, $"Middler : {agent.Name}與AGVC連線狀態改變 {isConnect}");
        }
        //protected void DoDisconnection(object sender, TcpIpEventArgs e)
        //{
        //    TcpIpAgent agent = sender as TcpIpAgent;
        //    var msg = $"Vh ID:{agent.Name}, disconnection.";
        //    OnConnectionChangeEvent?.Invoke(this, false);
        //    OnMessageShowOnMainFormEvent?.Invoke(this, "Middler : Dis-Connect");
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
                var msg = $"Middler : 斷線中，無法發送[{wrapper.SeqNum}][id {wrapper.ID}{(EnumCmdNum)wrapper.ID}]資訊";
                OnCmdSendEvent?.Invoke(this, msg);
                msg += wrapper.ToString();
                mirleLogger.Log(new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
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
            mirleLogger.Log(new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        private void RecieveCommandMediator(object sender, TcpIpEventArgs e)
        {
            EnumCmdNum cmdNum = (EnumCmdNum)int.Parse(e.iPacketID);

            if (theVehicle.AutoState != EnumAutoState.Auto && !IsApplyOnly(cmdNum))
            {
                var msg = $"Middler : 手動模式下，不接受AGVC命令";
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
                case EnumCmdNum.Cmd33_ControlZoneCancelRequest:
                    Receive_Cmd33_ControlZoneCancelRequest(sender, e);
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
                case EnumCmdNum.Cmd45_PowerOnoffRequest:
                    Receive_Cmd45_PowerOnoffRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd51_AvoidRequest:
                    Receive_Cmd51_AvoidRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                    Receive_Cmd52_AvoidCompleteResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd71_RangeTeachRequest:
                    Receive_Cmd71_RangeTeachRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd72_RangeTeachCompleteResponse:
                    Receive_Cmd72_RangeTeachCompleteResponse(sender, e);
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
                case EnumCmdNum.Cmd33_ControlZoneCancelRequest:
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
                case EnumCmdNum.Cmd45_PowerOnoffRequest:
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
                mirleLogger.Log(new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
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
            //PreAskReserve();
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
                        SpinWait.SpinUntil(() => ReserveOkAskNext, middlerConfig.AskReserveIntervalMs);
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
            var msg = $"Middler : 開始詢問通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void PauseAskReserve()
        {
            IsAskReservePause = true;
            PreAskReserveStatus = AskReserveStatus;
            AskReserveStatus = EnumThreadStatus.Pause;
            var getSectionOk = queNeedReserveSections.TryPeek(out MapSection mapSection);
            var msg = getSectionOk ? $"Middler : 暫停詢問通行權,[當前詢問路徑={mapSection.Id}]" : $"Middler : 暫停詢問通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void ResumeAskReserve()
        {
            IsAskReservePause = false;
            AskReserveStatus = PreAskReserveStatus;
            var getSectionOk = queNeedReserveSections.TryPeek(out MapSection mapSection);
            var msg = getSectionOk ? $"Middler : 恢復詢問通行權,[當前詢問路徑={mapSection.Id}]" : "Middler : 恢復詢問通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void StopAskReserve()
        {
            IsAskReserveStop = true;
            AskReserveStatus = EnumThreadStatus.Stop;
            ClearAllReserve();
            var msg = $"Middler : 停止詢問通行權";
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

        //    var msg = $"Middler :詢問通行權 前處理[NeedReserveSectionIds={QueMapSectionsToString(queNeedReserveSections)}]";
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
            return /*mainFlowHandler.IsMoveStep() &&*/ /*mainFlowHandler.CanVehMove() &&*/ !IsGotReserveOkSectionsFull() && !mainFlowHandler.IsMoveEnd;
        }
        public bool IsGotReserveOkSectionsFull()
        {
            int reserveOkSectionsTotalLength = GetReserveOkSectionsTotalLength();
            return reserveOkSectionsTotalLength >= middlerConfig.ReserveLengthMeter * 1000;
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
        public void SetupNeedReserveSections(List<MapSection> mapSections)
        {
            queNeedReserveSections = new ConcurrentQueue<MapSection>(mapSections);
            var msg = $"Middler : 更新需要通行權路徑列表[{QueMapSectionsToString(queNeedReserveSections)}]";
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
                var msg = $"Middler :可通行路徑數量為[{queReserveOkSections.Count}]，無法清除已通過的路徑。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                return;
            }
            else
            {
                queReserveOkSections.TryDequeue(out MapSection passSection);
                string passSectionId = passSection.Id;
                OnPassReserveSectionEvent?.Invoke(this, passSectionId);
                var msg = $"Middler : 清除已通過路徑[{passSectionId}]。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
        }
        public void OnGetReserveOk(string sectionId)
        {
            if (queNeedReserveSections.IsEmpty)
            {
                var msg = $"Middler : 收到{sectionId}通行權但延攬失敗，因為未取得通行權路段清單為空";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                return;
            }

            if (!IsAskReserveStop && !mainFlowHandler.IsMoveEnd)
            {
                queNeedReserveSections.TryPeek(out MapSection needReserveSection);

                if (needReserveSection.Id == sectionId || "XXX" == sectionId)
                {
                    mainFlowHandler.UpdateMoveControlReserveOkPositions(needReserveSection);
                    queNeedReserveSections.TryDequeue(out MapSection aReserveOkSection);
                    queReserveOkSections.Enqueue(aReserveOkSection);
                    OnReserveOkEvent?.Invoke(this, aReserveOkSection.Id);
                    ReserveOkAskNext = true;
                }
                else
                {
                    var msg = $"Middler : 收到{sectionId}通行權但延攬失敗，因為需要通行權路段為{needReserveSection.Id}";
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
            var msg = $"Middler : 清除所有通行權。";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        #endregion

        public void SendMiddlerFormCommands(int cmdNum, Dictionary<string, string> pairs)
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
                            aCmd.ActType = ActiveTypeParse(pairs["ActType"]);
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.DestinationAdr = pairs["DestinationAdr"];
                            aCmd.LoadAdr = pairs["LoadAdr"];
                            aCmd.SecDistance = uint.Parse(pairs["SecDistance"]);
                            aCmd.GuideAddressesStartToLoad.AddRange(StringSpilter(pairs["GuideAddressesStartToLoad"]));
                            aCmd.GuideAddressesToDestination.AddRange(StringSpilter(pairs["GuideAddressesToDestination"]));
                            aCmd.GuideSectionsStartToLoad.AddRange(StringSpilter(pairs["GuideSectionsStartToLoad"]));
                            aCmd.GuideSectionsToDestination.AddRange(StringSpilter(pairs["GuideSectionsToDestination"]));

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
                    case EnumCmdNum.Cmd33_ControlZoneCancelRequest:
                        {
                            ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST aCmd = new ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST();
                            aCmd.CancelSecID = pairs["CancelSecID"];
                            aCmd.ControlType = ControlTypeParse(pairs["ControlType"]);

                            wrappers.ID = WrapperMessage.ControlZoneReqFieldNumber;
                            wrappers.ControlZoneReq = aCmd;


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
                            aCmd.ActType = CMDCancelTypeParse(pairs["ActType"]);

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
                            aCmd.ActType = ActiveTypeParse(pairs["ActType"]);
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
                    case EnumCmdNum.Cmd133_ControlZoneCancelResponse:
                        {
                            ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE aCmd = new ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE();
                            aCmd.CancelSecID = pairs["CancelSecID"];
                            aCmd.ControlType = ControlTypeParse(pairs["ControlType"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.ControlZoneRespFieldNumber;
                            wrappers.ControlZoneResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd134_TransferEventReport:
                        {
                            ID_134_TRANS_EVENT_REP aCmd = new ID_134_TRANS_EVENT_REP();
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];
                            aCmd.EventType = EventTypeParse(pairs["EventType"]);
                            aCmd.DrivingDirection = DriveDirctionParse(pairs["DrivingDirection"]);

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
                            aCmd.ActType = CMDCancelTypeParse(pairs["ActType"]);
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
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];

                            wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                            wrappers.StatusReqResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd144_StatusReport:
                        {
                            //TODO: 補完屬性
                            ID_144_STATUS_CHANGE_REP aCmd = new ID_144_STATUS_CHANGE_REP();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.ActionStatus = VHActionStatusParse(pairs["ActionStatus"]);
                            aCmd.BatteryCapacity = uint.Parse(pairs["BatteryCapacity"]);
                            aCmd.BatteryTemperature = int.Parse(pairs["BatteryTemperature"]);
                            aCmd.BlockingStatus = VhStopSingleParse(pairs["BlockingStatus"]);
                            aCmd.ChargeStatus = VhChargeStatusParse(pairs["ChargeStatus"]);
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.CSTID = pairs["CSTID"];

                            wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                            wrappers.StatueChangeRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd145_PowerOnoffResponse:
                        {
                            ID_145_POWER_OPE_RESPONSE aCmd = new ID_145_POWER_OPE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.PowerOpeRespFieldNumber;
                            wrappers.PowerOpeResp = aCmd;


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

        public void IntegrateControl_OnBatteryPercentageChangeEvent(object sender, double batteryPercentage)
        {
            BatteryPercentageChangeReport(MethodBase.GetCurrentMethod().Name, (ushort)batteryPercentage);
        }

        private void BatteryPercentageChangeReport(string sender, ushort batteryPercentage)
        {
            Send_Cmd144_StatusChangeReport(sender, batteryPercentage);
        }

        public void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            if (theVehicle.ErrorStatus == VhStopSingle.StopSingleOff && alarmHandler.HasAlarm)
            {
                theVehicle.ErrorStatus = VhStopSingle.StopSingleOn;
                StatusChangeReport(sender as string);
            }
            Send_Cmd194_AlarmReport(alarm.Id.ToString(), ErrorStatus.ErrSet);
        }
        public void AlarmHandler_OnPlcResetOneAlarmEvent(object sender, Alarm alarm)
        {
            if (theVehicle.ErrorStatus == VhStopSingle.StopSingleOn && !alarmHandler.HasAlarm)
            {
                theVehicle.ErrorStatus = VhStopSingle.StopSingleOff;
                StatusChangeReport(sender as string);
            }
            Send_Cmd194_AlarmReport(alarm.Id.ToString(), ErrorStatus.ErrReset);
        }
        public void AlarmHandler_OnResetAllAlarmsEvent(object sender, string msg)
        {
            if (theVehicle.ErrorStatus == VhStopSingle.StopSingleOn)
            {
                theVehicle.ErrorStatus = VhStopSingle.StopSingleOff;
                StatusChangeReport(sender as string);
            }
            Send_Cmd194_AlarmReport("0", ErrorStatus.ErrReset);

            //foreach (var alarm in alarms)
            //{
            //    Send_Cmd194_AlarmReport(alarm.Id.ToString(), ErrorStatus.ErrReset);
            //}
        }

        #region Public Functions

        public void ReportSectionPass(EventType type)
        {
            Send_Cmd134_TransferEventReport(type);
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
            var realPos = theVehicle.VehicleLocation.RealPosition;
            var lastAddr = theVehicle.VehicleLocation.LastAddress;
            if (string.IsNullOrEmpty(lastAddr.Id)) return true;
            return Math.Abs(realPos.X - lastAddr.Position.X) <= middlerConfig.NeerlyNoMoveRangeMm && Math.Abs(realPos.Y - lastAddr.Position.Y) <= middlerConfig.NeerlyNoMoveRangeMm;
        }
        public void LoadArrivals()
        {
            Send_Cmd136_TransferEventReport(EventType.LoadArrivals);
            Send_Cmd134_TransferEventReport(EventType.LoadArrivals);
        }
        public void Loading()
        {
            Send_Cmd136_TransferEventReport(EventType.Vhloading);
        }
        public bool IsCstIdReadReplyOk(EnumCstIdReadResult result)
        {
            Task<bool> actionResult = Send_Cmd136_CstIdReadReport(result);
            return actionResult.Result;
        }
        public void TransferComplete(AgvcTransCmd agvcTransCmd)
        {
            AgvcTransCmd cmd = agvcTransCmd;
            Send_Cmd132_TransferCompleteReport(cmd, 0);
        }
        public void LoadComplete()
        {
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
            Send_Cmd136_TransferEventReport(EventType.LoadComplete);
        }
        public void UnloadArrivals()
        {
            Send_Cmd136_TransferEventReport(EventType.UnloadArrivals);
            Send_Cmd134_TransferEventReport(EventType.UnloadArrivals);
        }
        public void Unloading()
        {
            Send_Cmd136_TransferEventReport(EventType.Vhunloading);
        }
        public void UnloadComplete()
        {
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
            Send_Cmd136_TransferEventReport(EventType.UnloadComplete);
        }
        public void MoveArrival()
        {
            Send_Cmd134_TransferEventReport(EventType.AdrOrMoveArrivals);
            Send_Cmd136_TransferEventReport(EventType.AdrOrMoveArrivals);
        }
        public void AvoidComplete()
        {
            Send_Cmd152_AvoidCompleteReport(0);
        }
        public void AvoidFail()
        {
            Send_Cmd152_AvoidCompleteReport(1);
        }
        public bool IsAskReserveAlive() => (thdAskReserve != null) && thdAskReserve.IsAlive;
        public void NoCommand()
        {
            theVehicle.ActionStatus = VHActionStatus.NoCommand;
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
        }
        public void Commanding()
        {
            theVehicle.ActionStatus = VHActionStatus.Commanding;
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
        }
        public void ReplyTransferCommand(string cmdId, ActiveType type, ushort seqNum, int replyCode, string reason)
        {
            Send_Cmd131_TransferResponse(cmdId, type, seqNum, replyCode, reason);
        }
        public void ReplyAvoidCommand(AgvcMoveCmd agvcMoveCmd, int replyCode, string reason)
        {
            Send_Cmd151_AvoidResponse(agvcMoveCmd.SeqNum, replyCode, reason);
        }
        public void ChargHandshaking()
        {
            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusHandshaking;
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
        }
        public void Charging()
        {
            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusCharging;
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
        }
        public void ChargeOff()
        {
            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusNone;
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
        }
        public void PauseReply(ushort seqNum, int replyCode, PauseEvent type)
        {
            Send_Cmd139_PauseResponse(seqNum, replyCode, type);
        }
        public void CancelAbortReply(ushort iSeqNum, int replyCode, string cancelCmdId, CMDCancelType actType)
        {
            Send_Cmd137_TransferCancelResponse(iSeqNum, replyCode, cancelCmdId, actType);
        }
        public void DoOverride(ID_31_TRANS_REQUEST transRequest, ushort iSeqNum)
        {
            AgvcOverrideCmd agvcOverrideCmd = (AgvcOverrideCmd)ConvertAgvcTransCmdIntoPackage(transRequest, iSeqNum);
            ShowTransferCmdToForm(agvcOverrideCmd);
            OnOverrideCommandEvent?.Invoke(this, agvcOverrideCmd);
        }
        public void DoBasicTransferCmd(ID_31_TRANS_REQUEST transRequest, ushort iSeqNum)
        {
            AgvcTransCmd agvcTransCmd = ConvertAgvcTransCmdIntoPackage(transRequest, iSeqNum);
            ShowTransferCmdToForm(agvcTransCmd);
            OnInstallTransferCommandEvent?.Invoke(this, agvcTransCmd);
        }
        public void StatusChangeReport(string sender)
        {
            Send_Cmd144_StatusChangeReport(sender);
        }
        private void ShowTransferCmdToForm(AgvcTransCmd agvcTransCmd)
        {
            var msg = $"收到{agvcTransCmd.CommandType},loadAdr={agvcTransCmd.LoadAddressId},unloadAdr={agvcTransCmd.UnloadAddressId}.";
            msg += Environment.NewLine + "LoadSectionIds:";
            foreach (var secId in agvcTransCmd.ToLoadSectionIds)
            {
                msg += $"({secId})";
            }
            msg += Environment.NewLine + "LoadAddressIds:";
            foreach (var adrId in agvcTransCmd.ToLoadAddressIds)
            {
                msg += $"({adrId})";
            }
            msg += Environment.NewLine + "UnloadSectionIds:";
            foreach (var secId in agvcTransCmd.ToUnloadSectionIds)
            {
                msg += $"({secId})";
            }
            msg += Environment.NewLine + "UnloadAddressIds:";
            foreach (var adrId in agvcTransCmd.ToUnloadAddressIds)
            {
                msg += $"({adrId})";
            }
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

        public void Receive_Cmd72_RangeTeachCompleteResponse(object sender, TcpIpEventArgs e)
        {
            ID_72_RANGE_TEACHING_COMPLETE_RESPONSE receive = (ID_72_RANGE_TEACHING_COMPLETE_RESPONSE)e.objPacket;



        }
        public void Send_Cmd172_RangeTeachCompleteReport(int completeCode)
        {
            VehicleLocation vehLocation = theVehicle.VehicleLocation;

            try
            {
                //TODO: After Teaching Complete

                ID_172_RANGE_TEACHING_COMPLETE_REPORT iD_172_RANGE_TEACHING_COMPLETE_REPORT = new ID_172_RANGE_TEACHING_COMPLETE_REPORT();
                iD_172_RANGE_TEACHING_COMPLETE_REPORT.CompleteCode = completeCode;
                iD_172_RANGE_TEACHING_COMPLETE_REPORT.FromAdr = theVehicle.TeachingFromAddress;
                iD_172_RANGE_TEACHING_COMPLETE_REPORT.ToAdr = theVehicle.TeachingToAddress;
                iD_172_RANGE_TEACHING_COMPLETE_REPORT.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.RangeTeachingCmpRepFieldNumber;
                wrappers.RangeTeachingCmpRep = iD_172_RANGE_TEACHING_COMPLETE_REPORT;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd71_RangeTeachRequest(object sender, TcpIpEventArgs e)
        {
            ID_71_RANGE_TEACHING_REQUEST receive = (ID_71_RANGE_TEACHING_REQUEST)e.objPacket;
            //TODO: Teaching Section Address Head/End



            int replyCode = 0;
            Send_Cmd171_RangeTeachResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd171_RangeTeachResponse(ushort seqNum, int replyCode)
        {
            try
            {
                ID_171_RANGE_TEACHING_RESPONSE iD_171_RANGE_TEACHING_RESPONSE = new ID_171_RANGE_TEACHING_RESPONSE();
                iD_171_RANGE_TEACHING_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.RangeTeachingRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.RangeTeachingResp = iD_171_RANGE_TEACHING_RESPONSE;

                SendCommandWrapper(wrappers, true);
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
            AgvcMoveCmd agvcMoveCmd = new AgvcMoveCmd(receive, e.iSeqNum);
            ShowAvoidRequestToForm(agvcMoveCmd);
            OnAvoideRequestEvent?.Invoke(this, agvcMoveCmd);
        }
        private void ShowAvoidRequestToForm(AgvcMoveCmd agvcMoveCmd)
        {
            var msg = $"收到避車指令,避車終點={agvcMoveCmd.UnloadAddressId}.";

            msg += Environment.NewLine + "避車路徑ID:";
            foreach (var secId in agvcMoveCmd.ToUnloadSectionIds)
            {
                msg += $"({secId})";
            }
            msg += Environment.NewLine + "避車過點ID:";
            foreach (var adrId in agvcMoveCmd.ToUnloadAddressIds)
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

        public void Receive_Cmd45_PowerOnoffRequest(object sender, TcpIpEventArgs e)
        {
            ID_45_POWER_OPE_REQ receive = (ID_45_POWER_OPE_REQ)e.objPacket;
            //TODO: PowerOn/PowerOff



            int replyCode = 0;
            Send_Cmd145_PowerOnoffResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd145_PowerOnoffResponse(ushort seqNum, int replyCode)
        {
            try
            {
                ID_145_POWER_OPE_RESPONSE iD_145_POWER_OPE_RESPONSE = new ID_145_POWER_OPE_RESPONSE();
                iD_145_POWER_OPE_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.PowerOpeRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.PowerOpeResp = iD_145_POWER_OPE_RESPONSE;

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
        public void Send_Cmd144_StatusChangeReport(string sender)
        {
            try
            {
                var batterys = theVehicle.TheVehicleIntegrateStatus.Batterys;
                VehicleLocation vehLocation = theVehicle.VehicleLocation;
                AgvcTransCmd agvcTransCmd = mainFlowHandler.GetAgvcTransCmd();

                ID_144_STATUS_CHANGE_REP iD_144_STATUS_CHANGE_REP = new ID_144_STATUS_CHANGE_REP();
                iD_144_STATUS_CHANGE_REP.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_144_STATUS_CHANGE_REP.CurrentSecID = vehLocation.LastSection.Id;
                iD_144_STATUS_CHANGE_REP.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;
                iD_144_STATUS_CHANGE_REP.ModeStatus = theVehicle.ModeStatus;
                iD_144_STATUS_CHANGE_REP.ActionStatus = theVehicle.ActionStatus;
                iD_144_STATUS_CHANGE_REP.PowerStatus = theVehicle.PowerStatus;
                iD_144_STATUS_CHANGE_REP.HasCST = VhLoadCSTStatusParse(!string.IsNullOrWhiteSpace(theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId));
                iD_144_STATUS_CHANGE_REP.ObstacleStatus = theVehicle.ObstacleStatus;
                iD_144_STATUS_CHANGE_REP.ReserveStatus = agvcTransCmd.ReserveStatus;// theVehicle.ReserveStatus;
                iD_144_STATUS_CHANGE_REP.BlockingStatus = theVehicle.BlockingStatus;
                iD_144_STATUS_CHANGE_REP.PauseStatus = agvcTransCmd.PauseStatus;
                iD_144_STATUS_CHANGE_REP.ErrorStatus = theVehicle.ErrorStatus;
                iD_144_STATUS_CHANGE_REP.CmdID = theVehicle.CurAgvcTransCmd.CommandId;
                iD_144_STATUS_CHANGE_REP.CSTID = string.IsNullOrWhiteSpace(theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId) ? "" : theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId;
                iD_144_STATUS_CHANGE_REP.DrivingDirection = theVehicle.DrivingDirection;
                iD_144_STATUS_CHANGE_REP.BatteryCapacity = (uint)batterys.Percentage;
                iD_144_STATUS_CHANGE_REP.BatteryTemperature = (int)batterys.BatteryTemperature;
                iD_144_STATUS_CHANGE_REP.ChargeStatus = VhChargeStatusParse(theVehicle.TheVehicleIntegrateStatus.Batterys.Charging);
                iD_144_STATUS_CHANGE_REP.XAxis = vehLocation.AgvcPosition.X;
                iD_144_STATUS_CHANGE_REP.YAxis = vehLocation.AgvcPosition.Y;
                iD_144_STATUS_CHANGE_REP.DirectionAngle = vehLocation.MoveDirectionAngle;
                iD_144_STATUS_CHANGE_REP.VehicleAngle = vehLocation.VehicleAngle;
                iD_144_STATUS_CHANGE_REP.Speed = vehLocation.Speed;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = iD_144_STATUS_CHANGE_REP;

                var msg = $"[來源{sender}]144 Report. [路徑ID={vehLocation.LastSection.Id}][位置ID={vehLocation.LastAddress.Id}][路徑距離={(uint)vehLocation.LastSection.VehicleDistanceSinceHead}][座標({Convert.ToInt32(vehLocation.RealPosition.X)},{Convert.ToInt32(vehLocation.RealPosition.Y)})][Mode={theVehicle.ModeStatus}][Reserve={agvcTransCmd.ReserveStatus}]";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
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
                var batterys = theVehicle.TheVehicleIntegrateStatus.Batterys;
                VehicleLocation vehLocation = theVehicle.VehicleLocation;
                AgvcTransCmd agvcTransCmd = mainFlowHandler.GetAgvcTransCmd();

                ID_144_STATUS_CHANGE_REP iD_144_STATUS_CHANGE_REP = new ID_144_STATUS_CHANGE_REP();
                iD_144_STATUS_CHANGE_REP.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_144_STATUS_CHANGE_REP.CurrentSecID = vehLocation.LastSection.Id;
                iD_144_STATUS_CHANGE_REP.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;
                iD_144_STATUS_CHANGE_REP.ModeStatus = theVehicle.ModeStatus;
                iD_144_STATUS_CHANGE_REP.ActionStatus = theVehicle.ActionStatus;
                iD_144_STATUS_CHANGE_REP.PowerStatus = theVehicle.PowerStatus;
                iD_144_STATUS_CHANGE_REP.HasCST = VhLoadCSTStatusParse(!string.IsNullOrWhiteSpace(theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId));
                iD_144_STATUS_CHANGE_REP.ObstacleStatus = theVehicle.ObstacleStatus;
                iD_144_STATUS_CHANGE_REP.ReserveStatus = agvcTransCmd.ReserveStatus;// theVehicle.ReserveStatus;
                iD_144_STATUS_CHANGE_REP.BlockingStatus = theVehicle.BlockingStatus;
                iD_144_STATUS_CHANGE_REP.PauseStatus = agvcTransCmd.PauseStatus;
                iD_144_STATUS_CHANGE_REP.ErrorStatus = theVehicle.ErrorStatus;
                iD_144_STATUS_CHANGE_REP.CmdID = theVehicle.CurAgvcTransCmd.CommandId;
                iD_144_STATUS_CHANGE_REP.CSTID = string.IsNullOrWhiteSpace(theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId) ? "" : theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId;
                iD_144_STATUS_CHANGE_REP.DrivingDirection = theVehicle.DrivingDirection;
                iD_144_STATUS_CHANGE_REP.BatteryCapacity = batteryPercentage;
                iD_144_STATUS_CHANGE_REP.BatteryTemperature = (int)batterys.BatteryTemperature;
                iD_144_STATUS_CHANGE_REP.ChargeStatus = VhChargeStatusParse(theVehicle.TheVehicleIntegrateStatus.Batterys.Charging);
                iD_144_STATUS_CHANGE_REP.XAxis = vehLocation.AgvcPosition.X;
                iD_144_STATUS_CHANGE_REP.YAxis = vehLocation.AgvcPosition.Y;
                iD_144_STATUS_CHANGE_REP.DirectionAngle = vehLocation.MoveDirectionAngle;
                iD_144_STATUS_CHANGE_REP.VehicleAngle = vehLocation.VehicleAngle;
                iD_144_STATUS_CHANGE_REP.Speed = vehLocation.Speed;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = iD_144_STATUS_CHANGE_REP;

                var msg = $"[來源{sender}]144 Report. [路徑ID={vehLocation.LastSection.Id}][位置ID={vehLocation.LastAddress.Id}][路徑距離={(uint)vehLocation.LastSection.VehicleDistanceSinceHead}][座標({Convert.ToInt32(vehLocation.RealPosition.X)},{Convert.ToInt32(vehLocation.RealPosition.Y)})][Mode={theVehicle.ModeStatus}][Reserve={agvcTransCmd.ReserveStatus}]";
                mirleLogger.Log(new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", msg));

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        private void Receive_Cmd43_StatusRequest(object sender, TcpIpEventArgs e)
        {
            ID_43_STATUS_REQUEST receive = (ID_43_STATUS_REQUEST)e.objPacket; // Cmd43's object is empty



            Send_Cmd143_StatusResponse(e.iSeqNum);
        }
        public void Send_Cmd143_StatusResponse(ushort seqNum)
        {
            try
            {
                VehicleLocation vehLocation = theVehicle.VehicleLocation;
                var batterys = theVehicle.TheVehicleIntegrateStatus.Batterys;
                AgvcTransCmd agvcTransCmd = mainFlowHandler.GetAgvcTransCmd();

                ID_143_STATUS_RESPONSE iD_143_STATUS_RESPONSE = new ID_143_STATUS_RESPONSE();
                iD_143_STATUS_RESPONSE.ActionStatus = theVehicle.ActionStatus;
                iD_143_STATUS_RESPONSE.BatteryCapacity = (uint)batterys.Percentage;
                iD_143_STATUS_RESPONSE.BatteryTemperature = (int)batterys.BatteryTemperature;
                iD_143_STATUS_RESPONSE.BlockingStatus = theVehicle.BlockingStatus;
                iD_143_STATUS_RESPONSE.ChargeStatus = VhChargeStatusParse(theVehicle.TheVehicleIntegrateStatus.Batterys.Charging);
                iD_143_STATUS_RESPONSE.CmdID = theVehicle.CurAgvcTransCmd.CommandId;
                iD_143_STATUS_RESPONSE.CSTID = theVehicle.TheVehicleIntegrateStatus.CarrierSlot.Loading ? theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId : "";
                iD_143_STATUS_RESPONSE.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_143_STATUS_RESPONSE.CurrentSecID = vehLocation.LastSection.Id;
                iD_143_STATUS_RESPONSE.DrivingDirection = theVehicle.DrivingDirection;
                iD_143_STATUS_RESPONSE.ErrorStatus = theVehicle.ErrorStatus;
                iD_143_STATUS_RESPONSE.HasCST = VhLoadCSTStatusParse(theVehicle.TheVehicleIntegrateStatus.CarrierSlot.Loading);
                iD_143_STATUS_RESPONSE.ModeStatus = theVehicle.ModeStatus;
                iD_143_STATUS_RESPONSE.ObstacleStatus = theVehicle.ObstacleStatus;
                iD_143_STATUS_RESPONSE.ObstDistance = theVehicle.ObstDistance;
                iD_143_STATUS_RESPONSE.ObstVehicleID = theVehicle.ObstVehicleID;
                iD_143_STATUS_RESPONSE.PauseStatus = agvcTransCmd.PauseStatus;
                iD_143_STATUS_RESPONSE.PowerStatus = theVehicle.PowerStatus;
                iD_143_STATUS_RESPONSE.ReserveStatus = agvcTransCmd.ReserveStatus;// theVehicle.ReserveStatus;
                iD_143_STATUS_RESPONSE.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;
                iD_143_STATUS_RESPONSE.StoppedBlockID = theVehicle.StoppedBlockID;
                iD_143_STATUS_RESPONSE.XAxis = vehLocation.AgvcPosition.X;
                iD_143_STATUS_RESPONSE.YAxis = vehLocation.AgvcPosition.Y;
                iD_143_STATUS_RESPONSE.Speed = vehLocation.Speed;
                iD_143_STATUS_RESPONSE.DirectionAngle = vehLocation.MoveDirectionAngle;
                iD_143_STATUS_RESPONSE.VehicleAngle = vehLocation.VehicleAngle;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.StatusReqResp = iD_143_STATUS_RESPONSE;

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
            //TODO: Auto/Manual

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
                int replyCode = 0;
                ID_39_PAUSE_REQUEST receive = (ID_39_PAUSE_REQUEST)e.objPacket;

                var msg = $"Middler : 收到[{receive.EventType}]命令。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);

                if (theVehicle.ActionStatus == VHActionStatus.NoCommand)
                {
                    replyCode = 1;
                    Send_Cmd139_PauseResponse(e.iSeqNum, replyCode, receive.EventType);
                    var ngMsg = $"Middler : 車輛無命令，拒絕[{receive.EventType}]命令，";
                    OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                    return;
                }

                switch (receive.EventType)
                {
                    case PauseEvent.Continue:
                        if (mainFlowHandler.IsVisitTransferStepsPause())
                        {
                            mainFlowHandler.Middler_OnCmdResumeEvent(e.iSeqNum, receive.EventType, receive.ReserveInfos);
                        }
                        else
                        {
                            var ngMsg = $"Middler : 車輛不在[暫停]狀態，拒絕[{PauseEvent.Continue}]命令，";
                            OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                            replyCode = 1;
                            Send_Cmd139_PauseResponse(e.iSeqNum, replyCode, receive.EventType);
                        }
                        break;
                    case PauseEvent.Pause:
                        if (theVehicle.VisitTransferStepsStatus == EnumThreadStatus.Working)
                        {
                            mainFlowHandler.Middler_OnCmdPauseEvent(e.iSeqNum, receive.EventType);
                        }
                        else
                        {
                            var ngMsg = $"Middler : 車輛不在[運作中]狀態，拒絕[{PauseEvent.Pause}]命令，";
                            OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                            replyCode = 1;
                            Send_Cmd139_PauseResponse(e.iSeqNum, replyCode, receive.EventType);
                            return;
                        }
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

        public void Receive_Cmd37_TransferCancelRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                int replyCode = 0;
                ID_37_TRANS_CANCEL_REQUEST receive = (ID_37_TRANS_CANCEL_REQUEST)e.objPacket;

                var msg = $"Middler : 收到[{receive.ActType}]命令。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);

                var cmdId = receive.CmdID.Trim();

                if (theVehicle.ActionStatus == VHActionStatus.NoCommand)
                {
                    replyCode = 1;
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive.CmdID, receive.ActType);
                    var ngMsg = $"Middler : 車輛無命令，拒絕[{receive.ActType}]命令，";
                    OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                    return;
                }

                AgvcTransCmd agvcTransCmd = mainFlowHandler.GetAgvcTransCmd();
                if (agvcTransCmd.CommandId != cmdId)
                {
                    replyCode = 1;
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive.CmdID, receive.ActType);
                    var ngMsg = $"Middler : 當前搬送命令ID({agvcTransCmd.CommandId})與收到取消命令ID({cmdId})不合，拒絕[{receive.ActType}]命令，";
                    OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                    return;
                }

                switch (receive.ActType)
                {
                    case CMDCancelType.CmdCancel:
                    case CMDCancelType.CmdAbort:
                        mainFlowHandler.Middler_OnCmdCancelAbortEvent(e.iSeqNum, cmdId, receive.ActType);
                        break;
                    case CMDCancelType.CmdCancelIdMismatch:
                    case CMDCancelType.CmdCancelIdReadFailed:
                        alarmHandler.ResetAllAlarms();
                        mainFlowHandler.StopAndClear();
                        break;
                    case CMDCancelType.CmdEms:
                        alarmHandler.SetAlarm(000037);
                        mainFlowHandler.StopAndClear();
                        break;
                    case CMDCancelType.CmdNone:
                    default:
                        replyCode = 1;
                        Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive.CmdID, receive.ActType);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Send_Cmd137_TransferCancelResponse(ushort seqNum, int replyCode, string cmdID, CMDCancelType actType)
        {
            try
            {
                ID_137_TRANS_CANCEL_RESPONSE iD_137_TRANS_CANCEL_RESPONSE = new ID_137_TRANS_CANCEL_RESPONSE();
                iD_137_TRANS_CANCEL_RESPONSE.CmdID = cmdID;
                iD_137_TRANS_CANCEL_RESPONSE.ActType = actType;
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
        private CompleteStatus GetCancelCompleteStatus(CMDCancelType replyActiveType, CompleteStatus completeStatus)
        {
            switch (replyActiveType)
            {
                case CMDCancelType.CmdNone:
                    break;
                case CMDCancelType.CmdCancel:
                    return CompleteStatus.CmpStatusCancel;
                case CMDCancelType.CmdAbort:
                    return CompleteStatus.CmpStatusAbort;
                case CMDCancelType.CmdCancelIdMismatch:
                    return CompleteStatus.CmpStatusIdmisMatch;
                case CMDCancelType.CmdCancelIdReadFailed:
                    return CompleteStatus.CmpStatusIdreadFailed;
                default:
                    break;
            }

            return completeStatus;
        }
        private CompleteStatus GetCancelCompleteStatus(EnumCstIdReadResult readResult, CompleteStatus completeStatus)
        {
            switch (readResult)
            {
                case EnumCstIdReadResult.Noraml:
                    break;
                case EnumCstIdReadResult.Mismatch:
                    return CompleteStatus.CmpStatusIdmisMatch;
                case EnumCstIdReadResult.Fail:
                    return CompleteStatus.CmpStatusIdreadFailed;
                default:
                    break;
            }

            return completeStatus;
        }
        public void Send_Cmd136_TransferEventReport(EventType eventType)
        {
            VehicleLocation vehLocation = theVehicle.VehicleLocation;
            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.CSTID = string.IsNullOrWhiteSpace(theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId) ? "" : theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.LastSection.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;

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
        public async Task<bool> Send_Cmd136_CstIdReadReport(EnumCstIdReadResult readResult, bool isLoadComplete = true)
        {
            VehicleLocation vehLocation = theVehicle.VehicleLocation;
            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = EventType.Bcrread;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.LastSection.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;
                iD_136_TRANS_EVENT_REP.BCRReadResult = BCRReadResultParse(readResult);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                #region 2019.12.11 TaskSend
                //SendCommandWrapper(wrappers);
                #endregion

                #region 2019.12.16 SendWait
                LogSendMsg(wrappers);

                ID_36_TRANS_EVENT_RESPONSE response = new ID_36_TRANS_EVENT_RESPONSE();
                string rtnMsg = "";

                TrxTcpIp.ReturnCode returnCode = await Task.Run<TrxTcpIp.ReturnCode>(() => ClientAgent.TrxTcpIp.sendRecv_Google(wrappers, out response, out rtnMsg, middlerConfig.RecvTimeoutMs, 0));

                if (returnCode == TrxTcpIp.ReturnCode.Normal)
                {
                    if (response.ReplyActiveType != CMDCancelType.CmdNone)
                    {
                        OnMessageShowOnMainFormEvent?.Invoke(this, $"Robot取貨異常，處理方式為[{response.ReplyActiveType}]，CstID變更為[{response.RenameCarrierID}]");
                        alarmHandler.ResetAllAlarms();
                        if (!string.IsNullOrEmpty(response.RenameCarrierID))
                        {
                            theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId = response.RenameCarrierID;
                        }
                        if (isLoadComplete) LoadComplete();
                        mainFlowHandler.agvcTransCmd.CompleteStatus = GetCancelCompleteStatus(response.ReplyActiveType, mainFlowHandler.agvcTransCmd.CompleteStatus);
                        mainFlowHandler.StopAndClear();
                        return false;
                    }
                    else
                    {
                        if (isLoadComplete)
                        {
                            LoadComplete();
                            OnMessageShowOnMainFormEvent?.Invoke(this, $"Robot取貨完成");
                        }
                        return true;
                    }
                }
                else
                {
                    OnMessageShowOnMainFormEvent?.Invoke(this, $"Robot取貨異常，等待回應逾時");
                    alarmHandler.ResetAllAlarms();
                    if (isLoadComplete) LoadComplete();
                    mainFlowHandler.agvcTransCmd.CompleteStatus = GetCancelCompleteStatus(readResult, mainFlowHandler.agvcTransCmd.CompleteStatus);
                    mainFlowHandler.StopAndClear();
                    return false;
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                OnMessageShowOnMainFormEvent?.Invoke(this, $"Robot取貨異常，Exception");
                alarmHandler.ResetAllAlarms();
                LoadComplete();
                mainFlowHandler.agvcTransCmd.CompleteStatus = GetCancelCompleteStatus(readResult, mainFlowHandler.agvcTransCmd.CompleteStatus);
                mainFlowHandler.StopAndClear();
                return false;
            }
        }

        public void Send_Cmd136_AskReserve(MapSection mapSection)
        {
            var msg = $"詢問{mapSection.Id}通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            VehicleLocation vehLocation = theVehicle.VehicleLocation;

            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = EventType.ReserveReq;
                FitReserveInfos(iD_136_TRANS_EVENT_REP.ReserveInfos, mapSection);
                iD_136_TRANS_EVENT_REP.CSTID = string.IsNullOrWhiteSpace(theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId) ? "" : theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.LastSection.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;


                #region Ask reserve and wait reply
                LogSendMsg(wrappers);

                ID_36_TRANS_EVENT_RESPONSE response = new ID_36_TRANS_EVENT_RESPONSE();
                string rtnMsg = "";

                var returnCode = ClientAgent.TrxTcpIp.sendRecv_Google(wrappers, out response, out rtnMsg, middlerConfig.RecvTimeoutMs, 0);

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
            if (mapSection.CmdDirection == EnumPermitDirection.Backward)
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirReverse;
            }
            else if (mapSection.CmdDirection == EnumPermitDirection.None)
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
            AgvcTransCmd agvcTransCmd = mainFlowHandler.agvcTransCmd;
            if (CanDoReserveWork())
            {
                IsAskReservePause = true;
                string sectionId = receive.ReserveInfos[0].ReserveSectionID;
                if (receive.IsReserveSuccess == ReserveResult.Success)
                {
                    IsAgvcRejectReserve = false;
                    string msg = $"收到{sectionId}通行權可行";
                    OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                    if (agvcTransCmd.ReserveStatus == VhStopSingle.StopSingleOn)
                    {
                        agvcTransCmd.ReserveStatus = VhStopSingle.StopSingleOff;
                        StatusChangeReport(MethodBase.GetCurrentMethod().Name);
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
                    if (mainFlowHandler.IsMoveStopByNoReserve())
                    {
                        if (agvcTransCmd.ReserveStatus == VhStopSingle.StopSingleOff)
                        {
                            agvcTransCmd.ReserveStatus = VhStopSingle.StopSingleOn;
                            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
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
            if (theVehicle.TheVehicleIntegrateStatus.CarrierSlot.Loading)
            {
                if (theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId == receive.OLDCSTID)
                {
                    mainFlowHandler.RenameCstId(receive.NEWCSTID);
                    result = true;
                }
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
            var location = theVehicle.VehicleLocation;

            try
            {
                ID_134_TRANS_EVENT_REP id_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                id_134_TRANS_EVENT_REP.EventType = type;
                id_134_TRANS_EVENT_REP.CurrentAdrID = location.LastAddress.Id;
                id_134_TRANS_EVENT_REP.CurrentSecID = location.LastSection.Id;
                id_134_TRANS_EVENT_REP.SecDistance = (uint)location.LastSection.VehicleDistanceSinceHead;
                id_134_TRANS_EVENT_REP.DrivingDirection = DriveDirctionParse(location.LastSection.CmdDirection);
                id_134_TRANS_EVENT_REP.XAxis = location.AgvcPosition.X;
                id_134_TRANS_EVENT_REP.YAxis = location.AgvcPosition.Y;
                id_134_TRANS_EVENT_REP.Speed = location.Speed;
                id_134_TRANS_EVENT_REP.DirectionAngle = location.MoveDirectionAngle;
                id_134_TRANS_EVENT_REP.VehicleAngle = location.VehicleAngle;

                mirleLogger.Log(new LogFormat("Info", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Angle=[{location.MoveDirectionAngle}]"));

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = id_134_TRANS_EVENT_REP;



                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd33_ControlZoneCancelRequest(object sender, TcpIpEventArgs e)
        {

            ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST receive = (ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST)e.objPacket;


            switch (receive.ControlType)
            {
                case ControlType.Nothing:
                    break;
                case ControlType.Block:
                    break;
                case ControlType.Hid:
                    break;
                default:
                    break;
            }

            int replyCode = 1;
            Send_Cmd133_ControlZoneCancelResponse(e.iSeqNum, receive.ControlType, receive.CancelSecID, replyCode);
        }
        public void Send_Cmd133_ControlZoneCancelResponse(ushort seqNum, ControlType controlType, string cancelSecID, int replyCode)
        {
            try
            {
                ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE = new ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE();
                iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE.ControlType = controlType;
                iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE.CancelSecID = cancelSecID;
                iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ControlZoneRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ControlZoneResp = iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE;

                SendCommandWrapper(wrappers, true);
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
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
        }
        public void Send_Cmd132_TransferCompleteReport(AgvcTransCmd agvcTransCmd, int delay = 0)
        {
            try
            {
                VehicleLocation vehLocation = theVehicle.VehicleLocation;

                var msg = $"命令結束，結束狀態{agvcTransCmd.CompleteStatus}，命令編號{agvcTransCmd.CommandId}";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);

                ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT = new ID_132_TRANS_COMPLETE_REPORT();
                iD_132_TRANS_COMPLETE_REPORT.CmdID = agvcTransCmd.CommandId;
                iD_132_TRANS_COMPLETE_REPORT.CSTID = string.IsNullOrWhiteSpace(theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId) ? "" : theVehicle.TheVehicleIntegrateStatus.CarrierSlot.CarrierId;
                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = agvcTransCmd.CompleteStatus;
                iD_132_TRANS_COMPLETE_REPORT.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_132_TRANS_COMPLETE_REPORT.CurrentSecID = vehLocation.LastSection.Id;
                iD_132_TRANS_COMPLETE_REPORT.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;
                iD_132_TRANS_COMPLETE_REPORT.CmdPowerConsume = theVehicle.CmdPowerConsume;
                iD_132_TRANS_COMPLETE_REPORT.CmdDistance = theVehicle.CmdDistance;
                iD_132_TRANS_COMPLETE_REPORT.XAxis = vehLocation.AgvcPosition.X;
                iD_132_TRANS_COMPLETE_REPORT.YAxis = vehLocation.AgvcPosition.Y;
                iD_132_TRANS_COMPLETE_REPORT.DirectionAngle = vehLocation.MoveDirectionAngle;
                iD_132_TRANS_COMPLETE_REPORT.VehicleAngle = vehLocation.VehicleAngle;


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
            ID_31_TRANS_REQUEST transRequest = (ID_31_TRANS_REQUEST)e.objPacket;
            OnMessageShowOnMainFormEvent?.Invoke(this, $"收到傳送指令: {transRequest.ActType}");

            switch (transRequest.ActType)
            {
                case ActiveType.Move:
                case ActiveType.Load:
                case ActiveType.Unload:
                case ActiveType.Loadunload:
                case ActiveType.Movetocharger:
                    DoBasicTransferCmd(transRequest, e.iSeqNum);
                    break;
                case ActiveType.Override:
                    DoOverride(transRequest, e.iSeqNum);
                    return;
                case ActiveType.Home:
                case ActiveType.Cstidrename:
                case ActiveType.Mtlhome:
                case ActiveType.Systemout:
                case ActiveType.Systemin:
                case ActiveType.Techingmove:
                case ActiveType.Round:
                default:
                    var msg = $"拒絕傳送指令: {transRequest.ActType}";
                    OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                    Send_Cmd131_TransferResponse(transRequest.CmdID, transRequest.ActType, e.iSeqNum, 1, "Unknow command.");
                    return;
            }

        }
        public void Send_Cmd131_TransferResponse(string cmdId, ActiveType type, ushort seqNum, int replyCode, string reason)
        {
            try
            {
                ID_131_TRANS_RESPONSE iD_131_TRANS_RESPONSE = new ID_131_TRANS_RESPONSE();
                iD_131_TRANS_RESPONSE.CmdID = cmdId;
                iD_131_TRANS_RESPONSE.ActType = type;
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

            switch (transRequest.ActType)
            {
                case ActiveType.Move:
                    return new AgvcMoveCmd(transRequest, iSeqNum);
                case ActiveType.Load:
                    return new AgvcLoadCmd(transRequest, iSeqNum);
                case ActiveType.Unload:
                    return new AgvcUnloadCmd(transRequest, iSeqNum);
                case ActiveType.Loadunload:
                    return new AgvcLoadunloadCmd(transRequest, iSeqNum);
                case ActiveType.Override:
                    AgvcOverrideCmd agvcOverrideCmd = new AgvcOverrideCmd(transRequest, iSeqNum);
                    mirleLogger.Log(new LogFormat("Debug", "9", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", "[替代路徑]物件生成完成"));
                    return agvcOverrideCmd;
                case ActiveType.Movetocharger:
                    return new AgvcMoveToChargerCmd(transRequest, iSeqNum);
                case ActiveType.Cstidrename:
                case ActiveType.Mtlhome:
                case ActiveType.Systemout:
                case ActiveType.Systemin:
                case ActiveType.Techingmove:
                case ActiveType.Round:
                case ActiveType.Home:
                default:
                    return new AgvcTransCmd(transRequest, iSeqNum);
            }
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
            try
            {
                return charging ? VhChargeStatus.ChargeStatusCharging : VhChargeStatus.ChargeStatusNone;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return VhChargeStatus.ChargeStatusNone;
            }
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
                return VhStopSingle.StopSingleOff;
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
        private DriveDirction DriveDirctionParse(string v)
        {
            try
            {
                v = v.Trim();

                return (DriveDirction)Enum.Parse(typeof(DriveDirction), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return DriveDirction.DriveDirForward;
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
                return CompleteStatus.CmpStatusAbort;
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
        private CMDCancelType CMDCancelTypeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (CMDCancelType)Enum.Parse(typeof(CMDCancelType), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return CMDCancelType.CmdAbort;
            }
        }
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
        private ActiveType ActiveTypeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (ActiveType)Enum.Parse(typeof(ActiveType), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return ActiveType.Home;
            }
        }
        private ControlType ControlTypeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (ControlType)Enum.Parse(typeof(ControlType), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return ControlType.Nothing;
            }
        }
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
            try
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
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return BCRReadResult.BcrNormal;
            }
        }

        private DriveDirction DriveDirctionParse(EnumPermitDirection cmdDirection)
        {
            try
            {
                switch (cmdDirection)
                {
                    case EnumPermitDirection.None:
                        return DriveDirction.DriveDirNone;
                    case EnumPermitDirection.Forward:
                        return DriveDirction.DriveDirForward;
                    case EnumPermitDirection.Backward:
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
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }

        private void LogDebug(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, "Device", "CarrierID", msg));
        }
    }

}
