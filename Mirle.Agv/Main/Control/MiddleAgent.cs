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
        #endregion

        private LoggerAgent theLoggerAgent = LoggerAgent.Instance;
        private Vehicle theVehicle = Vehicle.Instance;
        private MiddlerConfig middlerConfig;
        private AlarmHandler alarmHandler;
        private LoggerAgent loggerAgent;
        private MainFlowHandler mainFlowHandler;

        private Thread thdAskReserve;
        private ManualResetEvent askReserveShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent askReservePauseEvent = new ManualResetEvent(true);
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
        public List<MapSection> NeedReserveSections { get; set; } = new List<MapSection>();

        private ConcurrentQueue<MapSection> queNeedReserveSections = new ConcurrentQueue<MapSection>();
        private ConcurrentQueue<MapSection> queReserveOkSections = new ConcurrentQueue<MapSection>();
        private MapSection askingReserveSection = new MapSection();
        //public bool IsPauseAskReserve { get; private set; } = false;
        private EnumCstIdReadResult readResult = EnumCstIdReadResult.Noraml;

        public TcpIpAgent ClientAgent { get; private set; }
        public bool IsCancelByCstIdRead { get; set; } = false;

        //private MapSection lastReportSection = new MapSection();

        public MiddleAgent(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            middlerConfig = mainFlowHandler.GetMiddlerConfig();
            alarmHandler = mainFlowHandler.GetAlarmHandler();
            loggerAgent = LoggerAgent.Instance;

            CreatTcpIpClientAgent();
            Connect();
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }
        public void DisConnect()
        {
            try
            {
                if (ClientAgent != null)
                {
                    loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Middler : Disconnect Stop, [IsNull={IsClientAgentNull()}][IsConnect={IsConnected()}]"));

                    ClientAgent.stop();
                    //ClientAgent = null;
                }
                else
                {
                    loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"ClientAgent is null cannot disconnect"));
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                    loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Already connect cannot connect again."));
                }
            }
            else
            {
                CreatTcpIpClientAgent();
                Connect();
                loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"ClientAgent is null cannot connect."));
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
                ClientAgent.addTcpIpReceivedHandler((int)item, RecieveCmmandLog);
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
                loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
                return;
            }
            else
            {
                string msg = $"[SEND] [SeqNum = {wrapper.SeqNum}][{wrapper.ID}][{(EnumCmdNum)wrapper.ID}] {wrapper}";
                OnCmdSendEvent?.Invoke(this, msg);
                loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));

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
        private void RecieveCmmandLog(object sender, TcpIpEventArgs e)
        {
            string msg = $"[RECV] [SeqNum = {e.iSeqNum}][{e.iPacketID}][{(EnumCmdNum)int.Parse(e.iPacketID)}][ObjPacket = {e.objPacket}]";
            OnCmdReceiveEvent?.Invoke(this, msg);
            theLoggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , msg));
        }
        private void RecieveCommandMediator(object sender, TcpIpEventArgs e)
        {
            EnumCmdNum cmdNum = (EnumCmdNum)int.Parse(e.iPacketID);

            if (theVehicle.AutoState != EnumAutoState.Auto && !IsApplyOnly(cmdNum))
            {
                var msg = $"Middler : 手動模式下，不接受AGVC命令";
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , msg));
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

        #endregion

        #region Thd Ask Reserve
        private void AskReserve()
        {
            PreAskReserve();
            Stopwatch sw = new Stopwatch();
            long total = 0;
            while (!queNeedReserveSections.IsEmpty)
            {
                try
                {
                    sw.Restart();

                    #region Pause And Stop Check
                    askReservePauseEvent.WaitOne(Timeout.Infinite);
                    if (askReserveShutdownEvent.WaitOne(0)) break;
                    #endregion

                    AskReserveStatus = EnumThreadStatus.Working;
                    if (CanAskReserve())
                    {
                        queNeedReserveSections.TryPeek(out MapSection needReserveSection);
                        askingReserveSection = needReserveSection == null ? new MapSection() : needReserveSection;
                        Send_Cmd136_AskReserve();
                    }
                }
                catch (Exception ex)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, middlerConfig.AskReserveIntervalMs);
                    sw.Stop();
                    total += sw.ElapsedMilliseconds;
                }
            }
            AfterAskReserve(total);
        }
        public void RestartAskReserve()
        {
            StopAskReserve();
            StartAskReserve();
        }
        public void StartAskReserve()
        {
            askReservePauseEvent.Set();
            askReserveShutdownEvent.Reset();
            thdAskReserve = new Thread(new ThreadStart(AskReserve));
            thdAskReserve.IsBackground = true;
            thdAskReserve.Start();
            AskReserveStatus = EnumThreadStatus.Start;
            var msg = $"Middler : 開始詢問通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //         , msg));
        }
        public void PauseAskReserve()
        {
            askReservePauseEvent.Reset();
            PreAskReserveStatus = AskReserveStatus;
            AskReserveStatus = EnumThreadStatus.Pause;
            var msg = $"Middler : 暫停詢問通行權, [AskingReserveSectionId={askingReserveSection.Id}]";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //     , msg));
        }
        public void ResumeAskReserve()
        {
            askReservePauseEvent.Set();
            var tempStatus = AskReserveStatus;
            AskReserveStatus = PreAskReserveStatus;
            PreAskReserveStatus = tempStatus;
            var msg = $"Middler : 恢復詢問通行權, [AskingReserveSectionId={askingReserveSection.Id}]";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //     , msg));

        }
        public void StopAskReserve()
        {
            askReserveShutdownEvent.Set();
            askReservePauseEvent.Set();

            if (AskReserveStatus != EnumThreadStatus.None)
            {
                AskReserveStatus = EnumThreadStatus.Stop;
            }

            ClearAskReserve();

            var msg = $"Middler : 停止詢問通行權, [AskingReserveSectionId={askingReserveSection.Id}]";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //   , msg));
        }
        private void PreAskReserve()
        {
            queNeedReserveSections = new ConcurrentQueue<MapSection>(NeedReserveSections);
            queReserveOkSections = new ConcurrentQueue<MapSection>();
            askingReserveSection = new MapSection();
            if (queNeedReserveSections.IsEmpty)
            {
                StopAskReserve();
            }

            var msg = $"Middler :詢問通行權 前處理[NeedReserveSectionIds={QueMapSectionsToString(queNeedReserveSections)}]";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        private void AfterAskReserve(long total)
        {
            queNeedReserveSections = new ConcurrentQueue<MapSection>();
            askingReserveSection = new MapSection();
            AskReserveStatus = EnumThreadStatus.None;
            AgvcTransCmd agvcTransCmd = mainFlowHandler.GetAgvcTransCmd();
            agvcTransCmd.ReserveStatus = VhStopSingle.StopSingleOff;
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
            var msg = $"MainFlow : 詢問通行權 後處理, [ThreadStatus={AskReserveStatus}][TotalSpendMs={total}]";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }

        private bool CanAskReserve()
        {
            return mainFlowHandler.IsMoveStep() && mainFlowHandler.CanVehMove() && !IsGotReserveOkSectionsFull();
        }
        public bool IsGotReserveOkSectionsFull()
        {
            int reserveOkSectionsTotalLength = GetReserveOkSectionsTotalLength();
            return reserveOkSectionsTotalLength >= middlerConfig.ReserveLengthMeter * 1000;
        }
        private string QueMapSectionsToString(ConcurrentQueue<MapSection> aQue)
        {
            string sectionIds = "[";
            foreach (var item in aQue) sectionIds += $"({item.Id})";
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
            var msg = $"Middler : 清除未取得通行權路徑清單, 共[{queNeedReserveSections.Count}]筆";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void ClearAskingReserveSection()
        {
            askingReserveSection = new MapSection();
            var msg = $"Middler : 清除正在詢問通行權路徑[{askingReserveSection.Id}]。";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void ClearGotReserveOkSections()
        {
            queReserveOkSections = new ConcurrentQueue<MapSection>();
            var msg = $"Middler : 清除已取得通行權路徑清單, 共[{queReserveOkSections.Count}]筆";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public MapSection GetAskingReserveSection()
        {
            return askingReserveSection;
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
            if (queReserveOkSections.Count == 0)
            {
                var msg = $"Middler :可通行路徑數量為[{queReserveOkSections.Count}]，無法清除已通過的路徑。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                return;
            }
            else
            {
                queReserveOkSections.TryDequeue(out MapSection passSection);
                var msg = $"Middler : 清除已通過路徑[{passSection.Id}]。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
        }
        public void SetupReserveOkSections(List<MapSection> reserveOkSections)
        {
            queReserveOkSections = new ConcurrentQueue<MapSection>(reserveOkSections);
            var msg = $"Middler : 更新可通行路徑列表[{QueMapSectionsToString(queReserveOkSections)}]";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }

        public void OnGetReserveOk()
        {

            if (queNeedReserveSections.Count == 0)
            {
                var msg = $"Middler : 延攬{askingReserveSection.Id}通行權失敗 , 未取得通行權路徑列表 is Empty";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                return;
            }

            queNeedReserveSections.TryPeek(out MapSection needReserveSection);
            if (needReserveSection.Id == askingReserveSection.Id)
            {
                queNeedReserveSections.TryDequeue(out MapSection aReserveOkSection);
                queReserveOkSections.Enqueue(aReserveOkSection);
                mainFlowHandler.UpdateMoveControlReserveOkPositions(aReserveOkSection);
                var msg = $"Middler : 延攬{askingReserveSection.Id}通行權成功";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
            else
            {
                var msg = $"Middler : 延攬{askingReserveSection.Id}通行權失敗, 未取得通行權路徑為{needReserveSection.Id}";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
        }
        public void DequeueNeedReserveSections()
        {
            if (queNeedReserveSections.Count == 0)
            {
                var msg = $"Middler : 清除需要通行權路段失敗，需要通行權路段清單為空。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
            else
            {
                queNeedReserveSections.TryDequeue(out MapSection dequeueSection);
                var msg = $"Middler : 清除需要通行權路段[{dequeueSection.Id}]成功。";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
        }
        public void ClearAskReserve()
        {
            ClearAskingReserveSection();
            ClearGotReserveOkSections();
            ClearNeedReserveSections();
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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

        public void PlcAgent_OnBatteryPercentageChangeEvent(object sender, ushort e)
        {
            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
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

        public void ReportAddressPass(MoveCmdInfo moveCmdInfo)
        {
            theVehicle.Cmd134EventType = EventType.AdrPass;
            Send_Cmd134_TransferEventReport(EventType.AdrPass);
        }

        public void ReportAddressPass()
        {
            if (!IsNeerlyNoMove())
            {
                //lastReportSection = theVehicle.CurVehiclePosition.LastSection.DeepClone();
                theVehicle.Cmd134EventType = EventType.AdrPass;
                Send_Cmd134_TransferEventReport();
            }
        }
        private bool IsNeerlyNoMove()
        {
            var realPos = theVehicle.VehicleLocation.RealPosition;
            var lastAddr = theVehicle.VehicleLocation.LastAddress;
            if (string.IsNullOrEmpty(lastAddr.Id)) return true;
            return Math.Abs(realPos.X - lastAddr.Position.X) <= middlerConfig.NeerlyNoMoveRangeMm && Math.Abs(realPos.Y - lastAddr.Position.Y) <= middlerConfig.NeerlyNoMoveRangeMm;
            //return (lastReportSection.Id == theVehicle.CurVehiclePosition.LastSection.Id) &&
            //    (Math.Abs(lastReportSection.Distance - theVehicle.CurVehiclePosition.LastSection.Distance) < middlerConfig.NeerlyNoMoveRangeMm);
        }
        public void LoadArrivals()
        {
            theVehicle.Cmd134EventType = EventType.LoadArrivals;
            Send_Cmd136_TransferEventReport(EventType.LoadArrivals);
            Send_Cmd134_TransferEventReport();
        }
        public void Loading()
        {
            Send_Cmd136_TransferEventReport(EventType.Vhloading);
        }
        public void CstIdRead(EnumCstIdReadResult result)
        {
            readResult = result;
            Send_Cmd136_CstIdReadReport(result);
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
            theVehicle.Cmd134EventType = EventType.UnloadArrivals;
            Send_Cmd136_TransferEventReport(EventType.UnloadArrivals);
            Send_Cmd134_TransferEventReport();
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
            theVehicle.Cmd134EventType = EventType.AdrOrMoveArrivals;
            Send_Cmd134_TransferEventReport();
            Send_Cmd136_TransferEventReport(EventType.AdrOrMoveArrivals);
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
                //TODO: Report alram
                ID_194_ALARM_REPORT iD_194_ALARM_REPORT = new ID_194_ALARM_REPORT();
                iD_194_ALARM_REPORT.ErrCode = alarmCode;
                iD_194_ALARM_REPORT.ErrStatus = status;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AlarmRepFieldNumber;
                wrappers.AlarmRep = iD_194_ALARM_REPORT;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void Receive_Cmd51_AvoidRequest(object sender, TcpIpEventArgs e)
        {
            ID_51_AVOID_REQUEST receive = (ID_51_AVOID_REQUEST)e.objPacket;
            //TODO: Avoid
            OnMessageShowOnMainFormEvent?.Invoke(this, $"收到避車指令");
            AgvcMoveCmd agvcMoveCmd = ConvertAvoidRequseIntoPackage(receive, e.iSeqNum);
            ShowAvoidRequestToForm(agvcMoveCmd);
            OnAvoideRequestEvent?.Invoke(this, agvcMoveCmd);

            //int replyCode = 0;
            //string reason = "";
            //Send_Cmd151_AvoidResponse(e.iSeqNum, replyCode, reason);
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

        private AgvcMoveCmd ConvertAvoidRequseIntoPackage(ID_51_AVOID_REQUEST receive, ushort iSeqNum)
        {
            return new AgvcMoveCmd(receive, iSeqNum);
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                PlcBatterys batterys = theVehicle.ThePlcVehicle.Batterys;
                VehicleLocation vehLocation = theVehicle.VehicleLocation;
                AgvcTransCmd agvcTransCmd = mainFlowHandler.GetAgvcTransCmd();

                ID_144_STATUS_CHANGE_REP iD_144_STATUS_CHANGE_REP = new ID_144_STATUS_CHANGE_REP();
                iD_144_STATUS_CHANGE_REP.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_144_STATUS_CHANGE_REP.CurrentSecID = vehLocation.LastSection.Id;
                iD_144_STATUS_CHANGE_REP.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;
                iD_144_STATUS_CHANGE_REP.ModeStatus = theVehicle.ModeStatus;
                iD_144_STATUS_CHANGE_REP.ActionStatus = theVehicle.ActionStatus;
                iD_144_STATUS_CHANGE_REP.PowerStatus = theVehicle.PowerStatus;
                iD_144_STATUS_CHANGE_REP.HasCST = VhLoadCSTStatusParse(!string.IsNullOrWhiteSpace(theVehicle.ThePlcVehicle.CassetteId));
                iD_144_STATUS_CHANGE_REP.ObstacleStatus = theVehicle.ObstacleStatus;
                iD_144_STATUS_CHANGE_REP.ReserveStatus = agvcTransCmd.ReserveStatus;// theVehicle.ReserveStatus;
                iD_144_STATUS_CHANGE_REP.BlockingStatus = theVehicle.BlockingStatus;
                iD_144_STATUS_CHANGE_REP.PauseStatus = agvcTransCmd.PauseStatus;
                iD_144_STATUS_CHANGE_REP.ErrorStatus = theVehicle.ErrorStatus;
                iD_144_STATUS_CHANGE_REP.CmdID = theVehicle.CurAgvcTransCmd.CommandId;
                iD_144_STATUS_CHANGE_REP.CSTID = string.IsNullOrWhiteSpace(theVehicle.ThePlcVehicle.CassetteId) ? "" : theVehicle.ThePlcVehicle.CassetteId;
                iD_144_STATUS_CHANGE_REP.DrivingDirection = theVehicle.DrivingDirection;
                iD_144_STATUS_CHANGE_REP.BatteryCapacity = (uint)batterys.Percentage;
                iD_144_STATUS_CHANGE_REP.BatteryTemperature = (int)batterys.FBatteryTemperature;
                iD_144_STATUS_CHANGE_REP.ChargeStatus = VhChargeStatusParse(theVehicle.ThePlcVehicle.Batterys.Charging);


                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = iD_144_STATUS_CHANGE_REP;

                var msg = $"[來源{sender}]144 Report. [路徑ID={vehLocation.LastSection.Id}][位置ID={vehLocation.LastAddress.Id}][路徑距離={(uint)vehLocation.LastSection.VehicleDistanceSinceHead}][座標({Convert.ToInt32(vehLocation.RealPosition.X)},{Convert.ToInt32(vehLocation.RealPosition.Y)})][Mode={theVehicle.ModeStatus}][Reserve={agvcTransCmd.ReserveStatus}]";
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", msg));

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                PlcBatterys batterys = theVehicle.ThePlcVehicle.Batterys;
                AgvcTransCmd agvcTransCmd = mainFlowHandler.GetAgvcTransCmd();

                ID_143_STATUS_RESPONSE iD_143_STATUS_RESPONSE = new ID_143_STATUS_RESPONSE();
                iD_143_STATUS_RESPONSE.ActionStatus = theVehicle.ActionStatus;
                iD_143_STATUS_RESPONSE.BatteryCapacity = (uint)batterys.Percentage;
                iD_143_STATUS_RESPONSE.BatteryTemperature = (int)batterys.FBatteryTemperature;
                iD_143_STATUS_RESPONSE.BlockingStatus = theVehicle.BlockingStatus;
                iD_143_STATUS_RESPONSE.ChargeStatus = VhChargeStatusParse(theVehicle.ThePlcVehicle.Batterys.Charging);
                iD_143_STATUS_RESPONSE.CmdID = theVehicle.CurAgvcTransCmd.CommandId;
                iD_143_STATUS_RESPONSE.CSTID = theVehicle.ThePlcVehicle.Loading ? theVehicle.ThePlcVehicle.CassetteId : "";
                iD_143_STATUS_RESPONSE.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_143_STATUS_RESPONSE.CurrentSecID = vehLocation.LastSection.Id;
                iD_143_STATUS_RESPONSE.DrivingDirection = theVehicle.DrivingDirection;
                iD_143_STATUS_RESPONSE.ErrorStatus = theVehicle.ErrorStatus;
                iD_143_STATUS_RESPONSE.HasCST = VhLoadCSTStatusParse(theVehicle.ThePlcVehicle.Loading);
                iD_143_STATUS_RESPONSE.ModeStatus = theVehicle.ModeStatus;
                iD_143_STATUS_RESPONSE.ObstacleStatus = theVehicle.ObstacleStatus;
                iD_143_STATUS_RESPONSE.ObstDistance = theVehicle.ObstDistance;
                iD_143_STATUS_RESPONSE.ObstVehicleID = theVehicle.ObstVehicleID;
                iD_143_STATUS_RESPONSE.PauseStatus = agvcTransCmd.PauseStatus;
                iD_143_STATUS_RESPONSE.PowerStatus = theVehicle.PowerStatus;
                iD_143_STATUS_RESPONSE.ReserveStatus = agvcTransCmd.ReserveStatus;// theVehicle.ReserveStatus;
                iD_143_STATUS_RESPONSE.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;
                iD_143_STATUS_RESPONSE.StoppedBlockID = theVehicle.StoppedBlockID;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.StatusReqResp = iD_143_STATUS_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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

                if (theVehicle.ActionStatus == VHActionStatus.NoCommand)
                {
                    replyCode = 1;
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive.CmdID, receive.ActType);
                    var ngMsg = $"Middler : 車輛無命令，拒絕[{receive.ActType}]命令，";
                    OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                    return;
                }

                AgvcTransCmd agvcTransCmd = mainFlowHandler.GetAgvcTransCmd();
                if (agvcTransCmd.CommandId != receive.CmdID)
                {
                    replyCode = 1;
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive.CmdID, receive.ActType);
                    var ngMsg = $"Middler : 當前搬送命令ID({agvcTransCmd.CommandId})與收到取消命令ID({receive.CmdID})不合，拒絕[{receive.ActType}]命令，";
                    OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                    return;
                }

                switch (receive.ActType)
                {
                    case CMDCancelType.CmdCancel:
                    case CMDCancelType.CmdAbort:
                        mainFlowHandler.Middler_OnCmdCancelAbortEvent(e.iSeqNum, receive.CmdID, receive.ActType);
                        break;
                    case CMDCancelType.CmdCancelIdMismatch:
                    case CMDCancelType.CmdCancelIdReadFailed:
                        if (IsCancelByCstIdRead)
                        {
                            // mainFlowHandler.Middler_OnCmdCarrierIdReadCancelAbortEvent(e.iSeqNum, receive.CmdID, receive.ActType,receive.);
                        }
                        break;
                    //break;
                    case CMDCancelType.CmdNone:
                    default:
                        replyCode = 1;
                        Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive.CmdID, receive.ActType);
                        break;
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void Receive_Cmd36_TransferEventResponse(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_36_TRANS_EVENT_RESPONSE receive = (ID_36_TRANS_EVENT_RESPONSE)e.objPacket;
                AgvcTransCmd agvcTransCmd = mainFlowHandler.agvcTransCmd;
                if (receive.EventType == EventType.ReserveReq)
                {
                    if (receive.IsReserveSuccess == ReserveResult.Success)
                    {
                        string msg = $"取得{askingReserveSection.Id}通行權成功";
                        if (agvcTransCmd.ReserveStatus == VhStopSingle.StopSingleOn)
                        {
                            agvcTransCmd.ReserveStatus = VhStopSingle.StopSingleOff;
                            StatusChangeReport(MethodBase.GetCurrentMethod().Name);
                        }
                        OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                        OnGetReserveOk();
                    }
                    else
                    {
                        string msg = $"取得{askingReserveSection.Id}通行權失敗";
                        OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                        if (mainFlowHandler.IsPauseByNoReserve())
                        {
                            if (agvcTransCmd.ReserveStatus == VhStopSingle.StopSingleOff)
                            {
                                agvcTransCmd.ReserveStatus = VhStopSingle.StopSingleOn;
                                StatusChangeReport(MethodBase.GetCurrentMethod().Name);
                            }
                            string msg2 = $"上報AGVC，因{askingReserveSection.Id}通行權無法取得停等中。";
                            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                        }
                    }
                }
                else if (receive.EventType == EventType.Bcrread)
                {
                    theVehicle.ThePlcVehicle.RenameCassetteId = string.IsNullOrEmpty(receive.RenameCarrierID) ? "" : receive.RenameCarrierID;
                    mainFlowHandler.NeedRename = !string.IsNullOrEmpty(receive.RenameCarrierID);

                    switch (receive.ReplyActiveType)
                    {
                        case CMDCancelType.CmdCancel:
                        case CMDCancelType.CmdAbort:
                        case CMDCancelType.CmdCancelIdMismatch:
                        case CMDCancelType.CmdCancelIdReadFailed:
                            agvcTransCmd.CompleteStatus = GetCancelCompleteStatus(receive.ReplyActiveType, agvcTransCmd.CompleteStatus);
                            mainFlowHandler.IsCancelByCstIdRead = true;
                            mainFlowHandler.IsBcrReadReply = true;
                            return;
                        case CMDCancelType.CmdNone:
                        default:
                            mainFlowHandler.IsBcrReadReply = true;
                            return;
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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

        public void Send_Cmd136_TransferEventReport(EventType eventType)
        {
            VehicleLocation vehLocation = theVehicle.VehicleLocation;
            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.CSTID = string.IsNullOrWhiteSpace(theVehicle.ThePlcVehicle.CassetteId) ? "" : theVehicle.ThePlcVehicle.CassetteId;
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }

        }
        public void Send_Cmd136_CstIdReadReport(EnumCstIdReadResult readResult)
        {
            VehicleLocation vehLocation = theVehicle.VehicleLocation;
            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = EventType.Bcrread;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicle.ThePlcVehicle.CassetteId;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.LastSection.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;
                iD_136_TRANS_EVENT_REP.BCRReadResult = BCRReadResultParse(readResult);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }

        }
        //public void Send_Cmd136_RequestBlock(string requestBlockID)
        //{
        //    VehiclePosition vehLocation = theVehicle.CurVehiclePosition;

        //    try
        //    {
        //        ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
        //        iD_136_TRANS_EVENT_REP.EventType = EventType.BlockReq;
        //        iD_136_TRANS_EVENT_REP.RequestBlockID = requestBlockID;
        //        iD_136_TRANS_EVENT_REP.CSTID = string.IsNullOrWhiteSpace(theVehicle.ThePlcVehicle.CassetteId) ? "" : theVehicle.ThePlcVehicle.CassetteId;
        //        iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.LastAddress.Id;
        //        iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.LastSection.Id;
        //        iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.LastSection.Distance;

        //        WrapperMessage wrappers = new WrapperMessage();
        //        wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
        //        wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

        //        SendCommandWrapper(wrappers);
        //    }
        //    catch (Exception ex)
        //    {
        //        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
        //    }
        //}
        //public void Send_Cmd136_ReleaseBlock(string releaseBlockAdrID)
        //{
        //    VehiclePosition vehLocation = theVehicle.CurVehiclePosition;

        //    try
        //    {
        //        ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
        //        iD_136_TRANS_EVENT_REP.EventType = EventType.BlockRelease;
        //        iD_136_TRANS_EVENT_REP.CSTID = string.IsNullOrWhiteSpace(theVehicle.ThePlcVehicle.CassetteId) ? "" : theVehicle.ThePlcVehicle.CassetteId;
        //        iD_136_TRANS_EVENT_REP.ReleaseBlockAdrID = releaseBlockAdrID;
        //        iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.LastAddress.Id;
        //        iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.LastSection.Id;
        //        iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.LastSection.Distance;

        //        WrapperMessage wrappers = new WrapperMessage();
        //        wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
        //        wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

        //        SendCommandWrapper(wrappers);
        //    }
        //    catch (Exception ex)
        //    {
        //        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
        //    }
        //}
        public void Send_Cmd136_AskReserve()
        {
            var msg = $"嘗試取得{askingReserveSection.Id}通行權";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            VehicleLocation vehLocation = theVehicle.VehicleLocation;

            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = EventType.ReserveReq;
                FitReserveInfos(iD_136_TRANS_EVENT_REP.ReserveInfos);
                iD_136_TRANS_EVENT_REP.CSTID = string.IsNullOrWhiteSpace(theVehicle.ThePlcVehicle.CassetteId) ? "" : theVehicle.ThePlcVehicle.CassetteId;
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }
        private void FitReserveInfos(RepeatedField<ReserveInfo> reserveInfos)
        {
            reserveInfos.Clear();

            ReserveInfo reserveInfo = new ReserveInfo();
            reserveInfo.ReserveSectionID = askingReserveSection.Id;
            if (askingReserveSection.CmdDirection == EnumPermitDirection.Backward)
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirReverse;
            }
            else if (askingReserveSection.CmdDirection == EnumPermitDirection.None)
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirNone;
            }
            else
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirForward;
            }

            reserveInfos.Add(reserveInfo);

        }

        public void Receive_Cmd35_CarrierIdRenameRequest(object sender, TcpIpEventArgs e)
        {
            ID_35_CST_ID_RENAME_REQUEST receive = (ID_35_CST_ID_RENAME_REQUEST)e.objPacket;
            var result = false;
            if (theVehicle.ThePlcVehicle.Loading)
            {
                if (theVehicle.ThePlcVehicle.CassetteId == receive.OLDCSTID)
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void Send_Cmd134_TransferEventReport()
        {
            VehicleLocation location = theVehicle.VehicleLocation;

            try
            {
                ID_134_TRANS_EVENT_REP iD_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                iD_134_TRANS_EVENT_REP.EventType = theVehicle.Cmd134EventType;
                iD_134_TRANS_EVENT_REP.CurrentAdrID = location.LastAddress.Id;
                iD_134_TRANS_EVENT_REP.CurrentSecID = location.LastSection.Id;
                iD_134_TRANS_EVENT_REP.SecDistance = (uint)location.LastSection.VehicleDistanceSinceHead;
                iD_134_TRANS_EVENT_REP.DrivingDirection = DriveDirctionParse(location.LastSection.CmdDirection);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }
        private void Send_Cmd134_TransferEventReport(EventType type)
        {
            var location = theVehicle.VehicleLocation;

            try
            {

                ID_134_TRANS_EVENT_REP iD_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                iD_134_TRANS_EVENT_REP.EventType = type;
                iD_134_TRANS_EVENT_REP.CurrentAdrID = location.LastAddress.Id;
                iD_134_TRANS_EVENT_REP.CurrentSecID = location.LastSection.Id;
                iD_134_TRANS_EVENT_REP.SecDistance = (uint)location.LastSection.VehicleDistanceSinceHead;
                iD_134_TRANS_EVENT_REP.DrivingDirection = DriveDirctionParse(location.LastSection.CmdDirection);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                iD_132_TRANS_COMPLETE_REPORT.CSTID = string.IsNullOrWhiteSpace(theVehicle.ThePlcVehicle.CassetteId) ? "" : theVehicle.ThePlcVehicle.CassetteId;
                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = agvcTransCmd.CompleteStatus;
                iD_132_TRANS_COMPLETE_REPORT.CurrentAdrID = vehLocation.LastAddress.Id;
                iD_132_TRANS_COMPLETE_REPORT.CurrentSecID = vehLocation.LastSection.Id;
                iD_132_TRANS_COMPLETE_REPORT.SecDistance = (uint)vehLocation.LastSection.VehicleDistanceSinceHead;
                iD_132_TRANS_COMPLETE_REPORT.CmdPowerConsume = theVehicle.CmdPowerConsume;
                iD_132_TRANS_COMPLETE_REPORT.CmdDistance = theVehicle.CmdDistance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TranCmpRepFieldNumber;
                wrappers.TranCmpRep = iD_132_TRANS_COMPLETE_REPORT;

                SendCommandWrapper(wrappers, false, delay);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                    return new AgvcOverrideCmd(transRequest, iSeqNum);
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                return DriveDirction.DriveDirNone;
            }
        }
        #endregion
    }

}
