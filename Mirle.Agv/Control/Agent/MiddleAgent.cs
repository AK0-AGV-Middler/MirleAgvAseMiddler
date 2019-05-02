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
        public event EventHandler<List<TransCmd>> OnMiddlerGetsNewTransCmdsEvent;

        private List<TransCmd> transCmds;
        //private VehLocation vehLocation;
        private LoggerAgent theLoggerAgent;
        private Vehicle theVehicle;
        private MiddlerConfigs middlerConfigs;

        private TcpIpAgent clientAgent;

        public MiddleAgent(MiddlerConfigs middlerConfigs)
        {
            this.middlerConfigs = middlerConfigs;
            transCmds = new List<TransCmd>();
            //vehLocation = new VehLocation();
            theLoggerAgent = LoggerAgent.Instance;
            theVehicle = Vehicle.Instance;

            EventInitial();
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

            clientAgent = new TcpIpAgent(clientNum, clientName,
                sLocalIP, iLocalPort, sRemoteIP, iRemotePort,
                TcpIpAgent.TCPIP_AGENT_COMM_MODE.CLINET_MODE
                  , recv_timeout_ms, send_timeout_ms, max_readSize, reconnection_interval_ms,
                  max_reconnection_count, retry_count, AppConstants.FrameBuilderType.PC_TYPE_MIRLE);
        }

        /// <summary>
        /// 註冊要監聽的事件
        /// </summary>
        void EventInitial()
        {
            // Add Event Handlers for all the recieved messages
            clientAgent.addTcpIpReceivedHandler(WrapperMessage.TransReqFieldNumber, Receive_Cmd31);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.TranCmpRespFieldNumber, str32_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.ControlZoneReqFieldNumber, str33_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.CSTIDRenameReqFieldNumber, str35_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.ImpTransEventRespFieldNumber, str36_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.TransCancelReqFieldNumber, str37_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.PauseReqFieldNumber, str39_Receive);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.ModeChangeReqFieldNumber, str41_Recieve);
            //clientAgent.addTcpIpReceivedHandler(WrapperMessage.StatusReqFieldNumber, str43_Receive);
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


        public void Receive_Cmd31(object sender, TcpIpEventArgs e)
        {
            ID_31_TRANS_REQUEST transRequest = (ID_31_TRANS_REQUEST)e.objPacket;


            if (CanVehDoTransfer())
            {
                ConvertAgvcTransCmdIntoList(transRequest);
                if (OnMiddlerGetsNewTransCmdsEvent != null)
                {
                    OnMiddlerGetsNewTransCmdsEvent.Invoke(this, transCmds);
                }
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
    }
}
