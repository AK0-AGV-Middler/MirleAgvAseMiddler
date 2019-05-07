using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Control.Tools;
using Mirle.Agv.Model.TransferCmds;
using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using TcpIpClientSample;
using Mirle.Agv.Model.Configs;


namespace Mirle.Agv.Control
{
    public class MiddleAgent
    {
        #region Events

        public event EventHandler<List<TransCmd>> OnMiddlerGetsNewTransCmdsEvent;
        public event EventHandler<string> OnMsgFromAgvcEvent;
        public event EventHandler<string> OnMsgToAgvcEvent;
        public event EventHandler<string> OnMsgFromVehicleEvent;
        public event EventHandler<string> OnMsgToVehicleEvent;

        #endregion

        private List<TransCmd> transCmds;
        private LoggerAgent theLoggerAgent;
        private Vehicle theVehicle;
        private MiddlerConfigs middlerConfigs;

        public TcpIpAgent clientAgent { get; private set; }

        public MiddleAgent(MiddlerConfigs middlerConfigs)
        {
            this.middlerConfigs = middlerConfigs;
            transCmds = new List<TransCmd>();

            theLoggerAgent = LoggerAgent.Instance;
            theVehicle = Vehicle.Instance;

            CreatTcpIpClientAgent();

        }

        private void CreatTcpIpClientAgent()
        {
            int clientNum = middlerConfigs.ClientNum;
            string clientName = middlerConfigs.ClientName;
            string sRemoteIP = middlerConfigs.RemoteIp;
            int iRemotePort = middlerConfigs.RemotePort;
            string sLocalIP = middlerConfigs.LocalIp;
            int iLocalPort = middlerConfigs.LocalPort;

            int recv_timeout_ms = middlerConfigs.RecvTimeoutMs;                         //等待sendRecv Reply的Time out時間(milliseconds)
            int send_timeout_ms = middlerConfigs.SendTimeoutMs;                         //暫時無用
            int max_readSize = middlerConfigs.MaxReadSize;                              //暫時無用
            int reconnection_interval_ms = middlerConfigs.ReconnectionIntervalMs;       //斷線多久之後再進行一次嘗試恢復連線的動作
            int max_reconnection_count = middlerConfigs.MaxReconnectionCount;           //斷線後最多嘗試幾次重新恢復連線 (若設定為0則不進行自動重新連線)
            int retry_count = middlerConfigs.RetryCount;                                //SendRecv Time out後要再重複發送的次數

            try
            {
                clientAgent = new TcpIpAgent(clientNum, clientName, sLocalIP, iLocalPort, sRemoteIP, iRemotePort, TcpIpAgent.TCPIP_AGENT_COMM_MODE.CLINET_MODE, recv_timeout_ms, send_timeout_ms, max_readSize, reconnection_interval_ms, max_reconnection_count, retry_count, AppConstants.FrameBuilderType.PC_TYPE_MIRLE);

                EventInitial();
            }
            catch (Exception ex)
            {

                var temp = ex.StackTrace;
            }
        }

        public void ReconnectToAgvc()
        {
            try
            {
                clientAgent.stop();
                clientAgent = null;
                CreatTcpIpClientAgent();

            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        /// <summary>
        /// 註冊要監聽的事件
        /// </summary>
        void EventInitial()
        {
            // Add Event Handlers for all the recieved messages
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.TransReqFieldNumber, Receive_Cmd31);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.TranCmpRespFieldNumber, Receive_Cmd32);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.ControlZoneReqFieldNumber, Receive_Cmd33);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.CSTIDRenameReqFieldNumber, str35_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.ImpTransEventRespFieldNumber, str36_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.TransCancelReqFieldNumber, str37_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.PauseReqFieldNumber, str39_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.ModeChangeReqFieldNumber, str41_Recieve);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.StatusReqFieldNumber, Receive_Cmd43);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.StatusChangeRespFieldNumber, str44_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.PowerOpeReqFieldNumber, str45_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.AvoidReqFieldNumber, str51_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.AvoidCompleteRespFieldNumber, str52_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.RangeTeachingReqFieldNumber, str71_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.RangeTeachingCmpRespFieldNumber, str72_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.AddressTeachRespFieldNumber, str74_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.AlarmResetReqFieldNumber, str91_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.AlarmRespFieldNumber, str94_Receive);
            //
            //Here need to be careful for the TCPIP
            //

            clientAgent.addTcpIpConnectedHandler(DoConnection);       //連線時的通知
            clientAgent.addTcpIpDisconnectedHandler(DoDisconnection); //斷線時的通知
        }

        protected void DoConnection(object sender, TcpIpEventArgs e)
        {
            TcpIpAgent agent = sender as TcpIpAgent;
            Console.WriteLine("Vh ID:{0}, connection.", agent.Name);
        }
        protected void DoDisconnection(object sender, TcpIpEventArgs e)
        {
            TcpIpAgent agent = sender as TcpIpAgent;
            Console.WriteLine("Vh ID:{0}, disconnection.", agent.Name);
        }

        private void Receive_Cmd43(object sender, TcpIpEventArgs e)
        {
            ID_43_STATUS_REQUEST receive = (ID_43_STATUS_REQUEST)e.objPacket; // Cmd43's object is empty

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            Send_Cmd143(e.iSeqNum);
        }
        public void Send_Cmd143(ushort seqNum)
        {
            Battery battery = theVehicle.GetBattery();
            TransCmd transCmd = theVehicle.GetTransCmd();
            VehLocation vehLocation = theVehicle.GetVehLoacation();

            try
            {
                ID_143_STATUS_RESPONSE iD_143_STATUS_RESPONSE = new ID_143_STATUS_RESPONSE();
                iD_143_STATUS_RESPONSE.ActionStatus = theVehicle.ActionStatus;
                iD_143_STATUS_RESPONSE.BatteryCapacity = (uint)battery.Percentage;
                iD_143_STATUS_RESPONSE.BatteryTemperature = battery.Temperature;
                iD_143_STATUS_RESPONSE.BlockingStatus = theVehicle.BlockingStatus;
                iD_143_STATUS_RESPONSE.ChargeStatus = theVehicle.ChargeStatus;
                iD_143_STATUS_RESPONSE.CmdID = transCmd.CmdId;
                iD_143_STATUS_RESPONSE.CSTID = theVehicle.CarrierID;
                iD_143_STATUS_RESPONSE.CurrentAdrID = vehLocation.Address.Id;
                iD_143_STATUS_RESPONSE.CurrentSecID = vehLocation.Section.Id;
                iD_143_STATUS_RESPONSE.DrivingDirection = theVehicle.DrivingDirection;
                iD_143_STATUS_RESPONSE.ErrorStatus = theVehicle.ErrorStatus;
                iD_143_STATUS_RESPONSE.HasCST = theVehicle.HasCarrier();
                iD_143_STATUS_RESPONSE.ModeStatus = theVehicle.ModeStatus;
                iD_143_STATUS_RESPONSE.ObstacleStatus = theVehicle.ObstacleStatus;
                iD_143_STATUS_RESPONSE.ObstDistance = theVehicle.ObstDistance;
                iD_143_STATUS_RESPONSE.ObstVehicleID = theVehicle.ObstVehicleID;
                iD_143_STATUS_RESPONSE.PauseStatus = theVehicle.PauseStatus;
                iD_143_STATUS_RESPONSE.PowerStatus = theVehicle.PowerStatus;
                iD_143_STATUS_RESPONSE.ReserveStatus = theVehicle.ReserveStatus;
                iD_143_STATUS_RESPONSE.SecDistance = (uint)vehLocation.Section.Distance;
                iD_143_STATUS_RESPONSE.StoppedBlockID = theVehicle.StoppedBlockID;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.StatusReqResp = iD_143_STATUS_RESPONSE;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_143_STATUS_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }

        }

        public void Receive_Cmd33(object sender, TcpIpEventArgs e)
        {

            ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST receive = (ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST)e.objPacket;

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

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

            Send_Cmd133(e.iSeqNum, receive.ControlType,receive.CancelSecID);
        }
        public void Send_Cmd133(ushort seqNum, ControlType controlType, string cancelSecID)
        {
            try
            {
                ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE = new ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE();
                iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE.ControlType = controlType;
                iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE.CancelSecID = cancelSecID;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ControlZoneRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ControlZoneResp = iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd32(object sender, TcpIpEventArgs e)
        {
            ID_32_TRANS_COMPLETE_RESPONSE receive = (ID_32_TRANS_COMPLETE_RESPONSE)e.objPacket;

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }
        }
        public void Send_Cmd132()
        {
            TransCmd transCmd = theVehicle.GetTransCmd();
            VehLocation vehLocation = theVehicle.GetVehLoacation();

            try
            {
                ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT = new ID_132_TRANS_COMPLETE_REPORT();
                iD_132_TRANS_COMPLETE_REPORT.CmdID = transCmd.CmdId;
                iD_132_TRANS_COMPLETE_REPORT.CSTID = theVehicle.CarrierID;
                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = theVehicle.CompleteStatus;
                iD_132_TRANS_COMPLETE_REPORT.CurrentAdrID = vehLocation.Address.Id;
                iD_132_TRANS_COMPLETE_REPORT.CurrentSecID = vehLocation.Section.Id;
                iD_132_TRANS_COMPLETE_REPORT.SecDistance = (uint)vehLocation.Section.Distance;
                iD_132_TRANS_COMPLETE_REPORT.CmdPowerConsume = theVehicle.CmdPowerConsume;
                iD_132_TRANS_COMPLETE_REPORT.CmdDistance = theVehicle.CmdDistance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TranCmpRepFieldNumber;                
                wrappers.TranCmpRep = iD_132_TRANS_COMPLETE_REPORT;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_132_TRANS_COMPLETE_REPORT.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd31(object sender, TcpIpEventArgs e)
        {
            ID_31_TRANS_REQUEST transRequest = (ID_31_TRANS_REQUEST)e.objPacket;
            //TODO : check if this cmd can work
            //TODO : change into list<TransCmd>
            //TODO : Notify everyone that new cmd31 receive

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, transRequest.ToString());
            }



            if (CanVehDoTransfer())
            {
                ConvertAgvcTransCmdIntoList(transRequest);
                if (OnMiddlerGetsNewTransCmdsEvent != null)
                {
                    OnMiddlerGetsNewTransCmdsEvent.Invoke(this, transCmds);
                }
            }
        }
        public void Send_Cmd131(ushort seqNum, int replyCode, string reason)
        {
            TransCmd transCmd = theVehicle.GetTransCmd();

            try
            {
                ID_131_TRANS_RESPONSE iD_131_TRANS_RESPONSE = new ID_131_TRANS_RESPONSE();
                iD_131_TRANS_RESPONSE.CmdID = transCmd.CmdId;
                iD_131_TRANS_RESPONSE.ActType = theVehicle.ActType;
                iD_131_TRANS_RESPONSE.ReplyCode = replyCode;
                iD_131_TRANS_RESPONSE.NgReason = reason;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.TransResp = iD_131_TRANS_RESPONSE;


                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_131_TRANS_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private bool CanVehDoTransfer()
        {
            throw new NotImplementedException();
        }

        private void ConvertAgvcTransCmdIntoList(ID_31_TRANS_REQUEST transRequest)
        {
            //解析收到的AgvcTransCmd並且填入TransCmds(list)
            throw new NotImplementedException();
        }

        public bool GetReserveFromAgvc(string sectionId)
        {
            throw new NotImplementedException();
        }

        public void ClearTransCmds()
        {
            transCmds.Clear();
        }

        public bool IsTransCmds()
        {
            return transCmds.Count > 0;
        }

        public List<TransCmd> GetTransCmds()
        {
            List<TransCmd> tempTransCmds = transCmds.ToList();
            return tempTransCmds;
        }

        public void OnMapBarcodeValuesChangedEvent(object sender, MapBarcodeReader mapBarcodeValues)
        {
            //vehLocation.SetMapBarcodeValues(mapBarcodeValues);
            //TODO: Make a Position change report from mapBarcode and send to AGVC
        }

        public void OnTransCmdsFinishedEvent(object sender, EnumCompleteStatus status)
        {
            //Send Transfer Command Complete Report to Agvc
            throw new NotImplementedException();
        }

        public void TestMsg()
        {
            var msg = "This is a test msg to agvc.";

            if (OnMsgToAgvcEvent != null)
            {
                OnMsgToAgvcEvent.Invoke(this, msg);
            }
        }
    }
}
