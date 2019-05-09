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
using Google.Protobuf.Collections;

namespace Mirle.Agv.Control
{
    public class MiddleAgent
    {
        #region Events

        public event EventHandler<AgvcTransCmd> OnMiddlerGetsNewTransCmdsEvent;
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
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.CSTIDRenameReqFieldNumber, Receive_Cmd35);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.ImpTransEventRespFieldNumber, Receive_Cmd36);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.TransCancelReqFieldNumber, Receive_Cmd37);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.PauseReqFieldNumber, Receive_Cmd39);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.ModeChangeReqFieldNumber, Receive_Cmd41);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.StatusReqFieldNumber, Receive_Cmd43);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.StatusChangeRespFieldNumber, Receive_Cmd44);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.PowerOpeReqFieldNumber, Receive_Cmd45);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.AvoidReqFieldNumber, Receive_Cmd51);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.AvoidCompleteRespFieldNumber, Receive_Cmd52);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.RangeTeachingReqFieldNumber, Receive_Cmd71);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.RangeTeachingCmpRespFieldNumber, Receive_Cmd72);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.AddressTeachRespFieldNumber, Receive_Cmd74);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.AlarmResetReqFieldNumber, Receive_Cmd91);
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.AlarmRespFieldNumber, Receive_Cmd94);
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

        public void Receive_Cmd94(object sender, TcpIpEventArgs e)
        {
            ID_94_ALARM_RESPONSE receive = (ID_94_ALARM_RESPONSE)e.objPacket;

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

        }
        public void Send_Cmd194(string alarmCode, ErrorStatus status)
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

                var result = clientAgent.TrxTcpIp.sendRecv_Google(wrappers, out ID_94_ALARM_RESPONSE receive, out string rtnMsg);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_194_ALARM_REPORT.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd91(object sender, TcpIpEventArgs e)
        {
            ID_91_ALARM_RESET_REQUEST receive = (ID_91_ALARM_RESET_REQUEST)e.objPacket;
            //TODO: Reset alarm

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            int replyCode = 0;
            Send_Cmd191(e.iSeqNum, replyCode);
        }
        public void Send_Cmd191(ushort seqNum, int replyCode)
        {
            try
            {
                ID_191_ALARM_RESET_RESPONSE iD_191_ALARM_RESET_RESPONSE = new ID_191_ALARM_RESET_RESPONSE();
                iD_191_ALARM_RESET_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AlarmResetRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.AlarmResetResp = iD_191_ALARM_RESET_RESPONSE;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_191_ALARM_RESET_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd74(object sender, TcpIpEventArgs e)
        {
            ID_74_ADDRESS_TEACH_RESPONSE receive = (ID_74_ADDRESS_TEACH_RESPONSE)e.objPacket;

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

        }
        public void Send_Cmd174(string addressId, int position)
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

                var result = clientAgent.TrxTcpIp.sendRecv_Google(wrappers, out ID_74_ADDRESS_TEACH_RESPONSE receive, out string rtnMsg);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_174_ADDRESS_TEACH_REPORT.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd72(object sender, TcpIpEventArgs e)
        {
            ID_72_RANGE_TEACHING_COMPLETE_RESPONSE receive = (ID_72_RANGE_TEACHING_COMPLETE_RESPONSE)e.objPacket;

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

        }
        public void Send_Cmd172(int completeCode)
        {
            VehLocation vehLocation = theVehicle.GetVehLoacation();

            try
            {
                //TODO: After Teaching Complete

                ID_172_RANGE_TEACHING_COMPLETE_REPORT iD_172_RANGE_TEACHING_COMPLETE_REPORT = new ID_172_RANGE_TEACHING_COMPLETE_REPORT();
                iD_172_RANGE_TEACHING_COMPLETE_REPORT.CompleteCode = completeCode;
                iD_172_RANGE_TEACHING_COMPLETE_REPORT.FromAdr = theVehicle.TeachingFromAddress;
                iD_172_RANGE_TEACHING_COMPLETE_REPORT.ToAdr = theVehicle.TeachingToAddress;
                iD_172_RANGE_TEACHING_COMPLETE_REPORT.SecDistance = (uint)vehLocation.Section.Distance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.RangeTeachingCmpRepFieldNumber;
                wrappers.RangeTeachingCmpRep = iD_172_RANGE_TEACHING_COMPLETE_REPORT;

                var result = clientAgent.TrxTcpIp.sendRecv_Google(wrappers, out ID_72_RANGE_TEACHING_COMPLETE_RESPONSE receive, out string rtnMsg);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_172_RANGE_TEACHING_COMPLETE_REPORT.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd71(object sender, TcpIpEventArgs e)
        {
            ID_71_RANGE_TEACHING_REQUEST receive = (ID_71_RANGE_TEACHING_REQUEST)e.objPacket;
            //TODO: Teaching Section Address Head/End

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            int replyCode = 0;
            Send_Cmd171(e.iSeqNum, replyCode);
        }
        public void Send_Cmd171(ushort seqNum, int replyCode)
        {
            try
            {
                ID_171_RANGE_TEACHING_RESPONSE iD_171_RANGE_TEACHING_RESPONSE = new ID_171_RANGE_TEACHING_RESPONSE();
                iD_171_RANGE_TEACHING_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.RangeTeachingRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.RangeTeachingResp = iD_171_RANGE_TEACHING_RESPONSE;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_171_RANGE_TEACHING_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd52(object sender, TcpIpEventArgs e)
        {
            ID_52_AVOID_COMPLETE_RESPONSE receive = (ID_52_AVOID_COMPLETE_RESPONSE)e.objPacket;

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            int completeStatus = 0;
        }
        public void Send_Cmd152(int completeStatus)
        {
            try
            {
                //TODO: Avoid

                ID_152_AVOID_COMPLETE_REPORT iD_152_AVOID_COMPLETE_REPORT = new ID_152_AVOID_COMPLETE_REPORT();
                iD_152_AVOID_COMPLETE_REPORT.CmpStatus = completeStatus;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AvoidCompleteRepFieldNumber;
                wrappers.AvoidCompleteRep = iD_152_AVOID_COMPLETE_REPORT;

                var result = clientAgent.TrxTcpIp.sendRecv_Google(wrappers, out ID_52_AVOID_COMPLETE_RESPONSE receive, out string rtnMsg);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_152_AVOID_COMPLETE_REPORT.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd51(object sender, TcpIpEventArgs e)
        {
            ID_51_AVOID_REQUEST receive = (ID_51_AVOID_REQUEST)e.objPacket;
            //TODO: Avoid

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            int replyCode = 0;
            string reason = "Empty";
            Send_Cmd151(e.iSeqNum, replyCode, reason);
        }
        public void Send_Cmd151(ushort seqNum, int replyCode, string reason)
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

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_151_AVOID_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd45(object sender, TcpIpEventArgs e)
        {
            ID_45_POWER_OPE_REQ receive = (ID_45_POWER_OPE_REQ)e.objPacket;
            //TODO: PowerOn/PowerOff

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            int replyCode = 0;
            Send_Cmd145(e.iSeqNum, replyCode);
        }
        public void Send_Cmd145(ushort seqNum, int replyCode)
        {
            try
            {
                ID_145_POWER_OPE_RESPONSE iD_145_POWER_OPE_RESPONSE = new ID_145_POWER_OPE_RESPONSE();
                iD_145_POWER_OPE_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.PowerOpeRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.PowerOpeResp = iD_145_POWER_OPE_RESPONSE;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_145_POWER_OPE_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private void Receive_Cmd44(object sender, TcpIpEventArgs e)
        {
            ID_43_STATUS_REQUEST receive = (ID_43_STATUS_REQUEST)e.objPacket; // Cmd43's object is empty

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            Send_Cmd144();
        }
        public void Send_Cmd144()
        {
            Battery battery = theVehicle.GetBattery();
            VehLocation vehLocation = theVehicle.GetVehLoacation();

            try
            {
                ID_144_STATUS_CHANGE_REP iD_144_STATUS_CHANGE_REP = new ID_144_STATUS_CHANGE_REP();
                iD_144_STATUS_CHANGE_REP.CurrentAdrID = vehLocation.Address.Id;
                iD_144_STATUS_CHANGE_REP.CurrentSecID = vehLocation.Section.Id;
                iD_144_STATUS_CHANGE_REP.SecDistance = (uint)vehLocation.Section.Distance;
                iD_144_STATUS_CHANGE_REP.ModeStatus = theVehicle.ModeStatus;
                iD_144_STATUS_CHANGE_REP.ActionStatus = theVehicle.ActionStatus;
                iD_144_STATUS_CHANGE_REP.PowerStatus = theVehicle.PowerStatus;
                iD_144_STATUS_CHANGE_REP.HasCST = theVehicle.HasCst;
                iD_144_STATUS_CHANGE_REP.ObstacleStatus = theVehicle.ObstacleStatus;
                iD_144_STATUS_CHANGE_REP.ReserveStatus = theVehicle.ReserveStatus;
                iD_144_STATUS_CHANGE_REP.BlockingStatus = theVehicle.BlockingStatus;
                iD_144_STATUS_CHANGE_REP.PauseStatus = theVehicle.PauseStatus;
                iD_144_STATUS_CHANGE_REP.ErrorStatus = theVehicle.ErrorStatus;
                iD_144_STATUS_CHANGE_REP.CmdID = theVehicle.CmdID;
                iD_144_STATUS_CHANGE_REP.CSTID = theVehicle.CarrierID;
                iD_144_STATUS_CHANGE_REP.DrivingDirection = theVehicle.DrivingDirection;
                iD_144_STATUS_CHANGE_REP.BatteryCapacity = (uint)battery.Percentage;
                iD_144_STATUS_CHANGE_REP.BatteryTemperature = battery.Temperature;
                iD_144_STATUS_CHANGE_REP.ChargeStatus = theVehicle.ChargeStatus;


                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = iD_144_STATUS_CHANGE_REP;

                var result = clientAgent.TrxTcpIp.sendRecv_Google(wrappers, out ID_44_STATUS_CHANGE_RESPONSE receive, out string rtnMsg);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_144_STATUS_CHANGE_REP.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }

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
                iD_143_STATUS_RESPONSE.HasCST = theVehicle.HasCst;
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

        public void Receive_Cmd41(object sender, TcpIpEventArgs e)
        {
            ID_41_MODE_CHANGE_REQ receive = (ID_41_MODE_CHANGE_REQ)e.objPacket;
            //TODO: Auto/Manual

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            int replyCode = 0;
            Send_Cmd141(e.iSeqNum, replyCode);
        }
        public void Send_Cmd141(ushort seqNum, int replyCode)
        {
            try
            {
                ID_141_MODE_CHANGE_RESPONSE iD_141_MODE_CHANGE_RESPONSE = new ID_141_MODE_CHANGE_RESPONSE();
                iD_141_MODE_CHANGE_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ModeChangeRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ModeChangeResp = iD_141_MODE_CHANGE_RESPONSE;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_141_MODE_CHANGE_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd39(object sender, TcpIpEventArgs e)
        {
            ID_35_CST_ID_RENAME_REQUEST receive = (ID_35_CST_ID_RENAME_REQUEST)e.objPacket;
            //TODO: Pause/Continue+/Reserve

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            int replyCode = 0;
            Send_Cmd139(e.iSeqNum, replyCode);
        }
        public void Send_Cmd139(ushort seqNum, int replyCode)
        {
            try
            {
                ID_139_PAUSE_RESPONSE iD_139_PAUSE_RESPONSE = new ID_139_PAUSE_RESPONSE();
                iD_139_PAUSE_RESPONSE.EventType = theVehicle.Cmd139EventType;
                iD_139_PAUSE_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.PauseRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.PauseResp = iD_139_PAUSE_RESPONSE;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_139_PAUSE_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd37(object sender, TcpIpEventArgs e)
        {

            ID_37_TRANS_CANCEL_REQUEST receive = (ID_37_TRANS_CANCEL_REQUEST)e.objPacket;
            //TODO: Cancel/Abort

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }

            int replyCode = 1;
            Send_Cmd137(e.iSeqNum, replyCode);
        }
        public void Send_Cmd137(ushort seqNum, int replyCode)
        {
            try
            {
                ID_137_TRANS_CANCEL_RESPONSE iD_137_TRANS_CANCEL_RESPONSE = new ID_137_TRANS_CANCEL_RESPONSE();
                iD_137_TRANS_CANCEL_RESPONSE.CmdID = theVehicle.CmdID;
                iD_137_TRANS_CANCEL_RESPONSE.ActType = theVehicle.Cmd137ActType;
                iD_137_TRANS_CANCEL_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransCancelRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.TransCancelResp = iD_137_TRANS_CANCEL_RESPONSE;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_137_TRANS_CANCEL_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd36(object sender, TcpIpEventArgs e)
        {
            ID_36_TRANS_EVENT_RESPONSE receive = (ID_36_TRANS_EVENT_RESPONSE)e.objPacket;
            //Get reserve, block, 

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }


        }
        public void Send_Cmd136(EventType eventType, string[] reserveSections, DriveDirction[] reserveDirections, string requestBlockID, string releaseBlockAdrID)
        {
            VehLocation vehLocation = theVehicle.GetVehLoacation();

            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = eventType;
                GetReserveInfo(reserveSections, reserveDirections, iD_136_TRANS_EVENT_REP.ReserveInfos);
                iD_136_TRANS_EVENT_REP.RequestBlockID = requestBlockID;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicle.CarrierID;
                iD_136_TRANS_EVENT_REP.ReleaseBlockAdrID = releaseBlockAdrID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.Address.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.Section.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.Section.Distance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                var result = clientAgent.TrxTcpIp.sendRecv_Google(wrappers, out ID_36_TRANS_EVENT_RESPONSE receive, out string rtnMsg);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_136_TRANS_EVENT_REP.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }
        private void GetReserveInfo(string[] reserveSections, DriveDirction[] reserveDirections, RepeatedField<ReserveInfo> reserveInfos)
        {
            if (reserveSections.Length > 0)
            {
                for (int i = 0; i < reserveSections.Length; i++)
                {
                    ReserveInfo reserveInfo = new ReserveInfo();
                    reserveInfo.ReserveSectionID = reserveSections[i];
                    reserveInfo.DriveDirction = reserveDirections[i];

                    reserveInfos.Add(reserveInfo);
                }
            }
        }

        public void Receive_Cmd35(object sender, TcpIpEventArgs e)
        {
            ID_35_CST_ID_RENAME_REQUEST receive = (ID_35_CST_ID_RENAME_REQUEST)e.objPacket;
            //TODO: CarrierID rename

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, receive.ToString());
            }


            int replyCode = 0;
            Send_Cmd135(e.iSeqNum, replyCode);
        }
        public void Send_Cmd135(ushort seqNum, int replyCode)
        {
            try
            {
                ID_135_CST_ID_RENAME_RESPONSE iD_135_CST_ID_RENAME_RESPONSE = new ID_135_CST_ID_RENAME_RESPONSE();
                iD_135_CST_ID_RENAME_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.CSTIDRenameRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.CSTIDRenameResp = iD_135_CST_ID_RENAME_RESPONSE;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, true);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_135_CST_ID_RENAME_RESPONSE.ToString());
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Send_Cmd134()
        {
            VehLocation vehLocation = theVehicle.GetVehLoacation();

            try
            {
                ID_134_TRANS_EVENT_REP iD_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                iD_134_TRANS_EVENT_REP.EventType = theVehicle.Cmd134EventType;
                iD_134_TRANS_EVENT_REP.CurrentAdrID = vehLocation.Address.Id;
                iD_134_TRANS_EVENT_REP.CurrentSecID = vehLocation.Section.Id;
                iD_134_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.Section.Distance;
                iD_134_TRANS_EVENT_REP.DrivingDirection = theVehicle.DrivingDirection;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                var resp = clientAgent.TrxTcpIp.SendGoogleMsg(wrappers, false);

                if (OnMsgToAgvcEvent != null)
                {
                    OnMsgToAgvcEvent(this, iD_134_TRANS_EVENT_REP.ToString());
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

            int replyCode = 1;
            Send_Cmd133(e.iSeqNum, receive.ControlType, receive.CancelSecID, replyCode);
        }
        public void Send_Cmd133(ushort seqNum, ControlType controlType, string cancelSecID, int replyCode)
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
            theVehicle.Cmd131ActType = transRequest.ActType;

            if (OnMsgFromAgvcEvent != null)
            {
                OnMsgFromAgvcEvent.Invoke(this, transRequest.ToString());
            }

            if (CanVehDoTransfer(transRequest, e.iSeqNum))
            {
                AgvcTransCmd agvcTransCmd = ConvertAgvcTransCmdIntoPackage(transRequest);
                if (OnMiddlerGetsNewTransCmdsEvent != null)
                {
                    OnMiddlerGetsNewTransCmdsEvent.Invoke(this, agvcTransCmd);
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
                iD_131_TRANS_RESPONSE.ActType = theVehicle.Cmd131ActType;
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

        private bool CanVehDoTransfer(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (theVehicle.GetBattery().IsBatteryLowPower())
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is in low power can not do transfer command.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else if (theVehicle.GetBattery().IsBatteryHighTemperature())
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is in battery temperature too hight can not do transfer command.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }

            var type = transRequest.ActType;
            switch (type)
            {
                case ActiveType.Move:
                    return CanVehMove(transRequest, seqNum);
                case ActiveType.Load:
                    return CanVehLoad(transRequest, seqNum);
                case ActiveType.Unload:
                    return CanVehUnload(transRequest, seqNum);
                case ActiveType.Loadunload:
                    return CanVehLoadunload(transRequest, seqNum);
                case ActiveType.Home:
                    return CanVehHome(transRequest, seqNum);
                case ActiveType.Override:
                    return CanVehOverride(transRequest, seqNum);
                case ActiveType.Mtlhome:
                case ActiveType.Movetomtl:
                case ActiveType.Systemout:
                case ActiveType.Systemin:
                case ActiveType.Techingmove:
                case ActiveType.Round:
                default:
                    int replyCode = 0; // OK
                    string reason = "Empty";
                    Send_Cmd131(seqNum, replyCode, reason);
                    return true;
            }
        }

        private bool CanVehOverride(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (VehInOverrideSection(transRequest))
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131(seqNum, replyCode, reason);
                return true;
            }
            else
            {
                int replyCode = 1; // NG
                string reason = "Vehicle current section not in override guideSections.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
        }

        private bool VehInOverrideSection(ID_31_TRANS_REQUEST transRequest)
        {
            var location = theVehicle.GetVehLoacation();
            var curSectionId = location.Section.Id;

            var isInToLoadSections = false;
            if (transRequest.GuideAddressesStartToLoad != null)
            {
                var toLoadSections = transRequest.GuideSectionsStartToLoad.ToList();
                isInToLoadSections = toLoadSections.Contains(curSectionId);
            }

            var isInToUnloadSections = false;
            if (transRequest.GuideSectionsToDestination != null)
            {
                var toUnloadSections = transRequest.GuideSectionsToDestination.ToList();
                isInToUnloadSections = toUnloadSections.Contains(curSectionId);
            }

            return isInToLoadSections || isInToUnloadSections;
        }

        private bool CanVehHome(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            int replyCode = 0; // OK
            string reason = "Empty";
            Send_Cmd131(seqNum, replyCode, reason);
            return true;
        }

        private bool CanVehLoadunload(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (theVehicle.ActionStatus != VHActionStatus.NoCommand)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is not idle can not do loadunload.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else if (theVehicle.HasCst == VhLoadCSTStatus.Exist)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle has a carrier can not do loadunload.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.LoadAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no load address can not do load.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.DestinationAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no unload address can not do unload.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131(seqNum, replyCode, reason);
                return true;
            }
        }

        private bool CanVehUnload(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (theVehicle.ActionStatus != VHActionStatus.NoCommand)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is not idle can not do unload.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else if (theVehicle.HasCst == VhLoadCSTStatus.NotExist)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle has no carrier can not do unload.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.DestinationAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no unload address can not do unload.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }

            else
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131(seqNum, replyCode, reason);
                return true;
            }
        }

        private bool CanVehLoad(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (theVehicle.ActionStatus != VHActionStatus.NoCommand)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is not idle can not do load.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else if (theVehicle.HasCst == VhLoadCSTStatus.Exist)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle has a carrier can not do load.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.LoadAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no load address can not do load.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131(seqNum, replyCode, reason);
                return true;
            }
        }

        private bool CanVehMove(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (theVehicle.ActionStatus != VHActionStatus.NoCommand)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is not idle can not do move.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.DestinationAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no move-end address can not do move.";
                Send_Cmd131(seqNum, replyCode, reason);
                return false;
            }
            else
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131(seqNum, replyCode, reason);
                return true;
            }
        }

        private AgvcTransCmd ConvertAgvcTransCmdIntoPackage(ID_31_TRANS_REQUEST transRequest)
        {
            //解析收到的ID_31_TRANS_REQUEST並且填入AgvcTransCmd 
            AgvcTransCmd agvcTransCmd = new AgvcTransCmd();
            agvcTransCmd.CmdId = transRequest.CmdID;
            if (transRequest.CSTID != null)
            {
                agvcTransCmd.CarrierId = transRequest.CSTID;
            }
            switch (transRequest.ActType)
            {
                case ActiveType.Move:
                    ConvertAgvcMoveCmdIntoPackage(transRequest, agvcTransCmd);
                    break;
                case ActiveType.Load:
                    ConvertAgvcLoadCmdIntoPackage(transRequest, agvcTransCmd);
                    break;
                case ActiveType.Unload:
                    ConvertAgvcUnloadCmdIntoPackage(transRequest, agvcTransCmd);
                    break;
                case ActiveType.Loadunload:
                    ConvertAgvcLoadunloadCmdIntoPackage(transRequest, agvcTransCmd);
                    break;
                case ActiveType.Home:
                    ConvertAgvcHomeCmdIntoPackage(transRequest, agvcTransCmd);
                    break;
                case ActiveType.Override:
                    ConvertAgvcOverrideCmdIntoPackage(transRequest, agvcTransCmd);
                    break;
                case ActiveType.Mtlhome:
                case ActiveType.Movetomtl:
                case ActiveType.Systemout:
                case ActiveType.Systemin:
                case ActiveType.Techingmove:
                case ActiveType.Round:
                default:
                    agvcTransCmd.CmdType = EnumAgvcTransCmdType.Else;
                    break;
            }

            return agvcTransCmd;
        }

        private void ConvertAgvcOverrideCmdIntoPackage(ID_31_TRANS_REQUEST transRequest, AgvcTransCmd agvcTransCmd)
        {
            throw new NotImplementedException();
        }

        private void ConvertAgvcHomeCmdIntoPackage(ID_31_TRANS_REQUEST transRequest, AgvcTransCmd agvcTransCmd)
        {
            throw new NotImplementedException();
        }

        private static void ConvertAgvcLoadunloadCmdIntoPackage(ID_31_TRANS_REQUEST transRequest, AgvcTransCmd agvcTransCmd)
        {
            agvcTransCmd.CmdType = EnumAgvcTransCmdType.Load;
            if (transRequest.GuideSectionsStartToLoad != null)
            {
                agvcTransCmd.ToLoadSections = transRequest.GuideSectionsStartToLoad.ToArray();
            }
            agvcTransCmd.LoadAddress = transRequest.LoadAdr;
            if (transRequest.GuideAddressesToDestination != null)
            {
                agvcTransCmd.ToUnloadSections = transRequest.GuideAddressesToDestination.ToArray();
            }
            agvcTransCmd.UnloadAddtess = transRequest.DestinationAdr;
        }

        private static void ConvertAgvcUnloadCmdIntoPackage(ID_31_TRANS_REQUEST transRequest, AgvcTransCmd agvcTransCmd)
        {
            agvcTransCmd.CmdType = EnumAgvcTransCmdType.Unload;
            if (transRequest.GuideAddressesToDestination != null)
            {
                agvcTransCmd.ToUnloadSections = transRequest.GuideAddressesToDestination.ToArray();
            }
            agvcTransCmd.UnloadAddtess = transRequest.DestinationAdr;
        }

        private static void ConvertAgvcLoadCmdIntoPackage(ID_31_TRANS_REQUEST transRequest, AgvcTransCmd agvcTransCmd)
        {
            agvcTransCmd.CmdType = EnumAgvcTransCmdType.Load;
            if (transRequest.GuideSectionsStartToLoad != null)
            {
                agvcTransCmd.ToLoadSections = transRequest.GuideSectionsStartToLoad.ToArray();
            }
            agvcTransCmd.LoadAddress = transRequest.LoadAdr;
        }

        private static void ConvertAgvcMoveCmdIntoPackage(ID_31_TRANS_REQUEST transRequest, AgvcTransCmd agvcTransCmd)
        {
            agvcTransCmd.CmdType = EnumAgvcTransCmdType.Move;
            if (transRequest.GuideSectionsStartToLoad != null)
            {
                agvcTransCmd.ToLoadSections = transRequest.GuideSectionsStartToLoad.ToArray();
            }
            agvcTransCmd.UnloadAddtess = transRequest.DestinationAdr;
        }

        public bool GetReserveFromAgvc(string sectionId)
        {
            throw new NotImplementedException();
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
