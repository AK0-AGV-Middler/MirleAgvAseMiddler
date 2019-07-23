using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using com.mirle.iibg3k0.ttc.Common.TCPIP.DecodRawData;
using Google.Protobuf.Collections;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TcpIpClientSample;

namespace Mirle.Agv.Controller
{
    public class MiddleAgent
    {
        #region Events

        public event EventHandler<string> OnConnected;
        public event EventHandler<string> OnDisConnected;
        public event EventHandler<AgvcTransCmd> OnInstallTransferCommandEvent;
        public event EventHandler<string> OnCmdReceive;
        public event EventHandler<string> OnCmdSend;
        public event EventHandler<string> OnTransferCancelEvent;
        public event EventHandler<string> OnTransferAbortEvent;
        public event EventHandler<MapSection> OnGetReserveOkEvent;
        public event EventHandler<bool> OnGetBlockPassEvent;

        #endregion

        private List<TransCmd> transCmds = new List<TransCmd>();
        private LoggerAgent theLoggerAgent = LoggerAgent.Instance;
        private Vehicle theVehicle = Vehicle.Instance;
        private MiddlerConfig middlerConfig;
        private MapInfo theMapInfo = new MapInfo();

        private bool IsReserveOk;
        private Thread thdAskReserve;
        private ManualResetEvent askReserveShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent askReservePauseEvent = new ManualResetEvent(true);
        private MapSection needReserveSection = new MapSection();

        public TcpIpAgent ClientAgent { get; private set; }

        public MiddleAgent(MiddlerConfig middlerConfig, MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
            this.middlerConfig = middlerConfig;

            CreatTcpIpClientAgent();
            //StartAskingReserve();
            //PauseAskingReserve();
        }

        private void AskReserve()
        {
            bool askResult = false;
            while (!askResult)
            {
                #region Pause And Stop Check

                askReservePauseEvent.WaitOne(Timeout.Infinite);
                if (askReserveShutdownEvent.WaitOne(0))
                {
                    break;
                }

                #endregion

                askResult = Send_Cmd136_AskReserve();
                if (!askResult)
                {
                    SpinWait.SpinUntil(() => false, middlerConfig.AskReserveInterval);
                }
            }

            if (askResult)
            {
                OnGetReserveOkEvent?.Invoke(this, needReserveSection);
            }
        }

        private void CreatTcpIpClientAgent()
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

                EventInitial();

                ClientAgent.injectDecoder(RawDataDecoder);

                Task.Run(() =>
                {
                    ClientAgent.clientConnection();
                });
            }
            catch (Exception ex)
            {
                var temp = ex.StackTrace;
            }
        }

        public void ReConnect()
        {
            try
            {
                DisConnect();

                CreatTcpIpClientAgent();

            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void DisConnect()
        {
            try
            {
                if (ClientAgent != null)
                {
                    ClientAgent.stop();
                    ClientAgent = null;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private void EventInitial()
        {
            // Add Event Handlers for all the recieved messages
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.TransReqFieldNumber, Receive_Cmd31_TransferRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.TranCmpRespFieldNumber, Receive_Cmd32_TransferCompleteResponse);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.ControlZoneReqFieldNumber, Receive_Cmd33_ControlZoneCancelRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.CSTIDRenameReqFieldNumber, Receive_Cmd35_CarrierIdRenameRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.ImpTransEventRespFieldNumber, Receive_Cmd36_TransferEventResponse);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.TransCancelReqFieldNumber, Receive_Cmd37_TransferCancelRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.PauseReqFieldNumber, Receive_Cmd39_PauseRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.ModeChangeReqFieldNumber, Receive_Cmd41_ModeChange);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.StatusReqFieldNumber, Receive_Cmd43_StatusRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.StatusChangeRespFieldNumber, Receive_Cmd44_StatusRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.PowerOpeReqFieldNumber, Receive_Cmd45_PowerOnoffRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.AvoidReqFieldNumber, Receive_Cmd51_AvoidRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.AvoidCompleteRespFieldNumber, Receive_Cmd52_AvoidCompleteResponse);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.RangeTeachingReqFieldNumber, Receive_Cmd71_RangeTeachRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.RangeTeachingCmpRespFieldNumber, Receive_Cmd72_RangeTeachCompleteResponse);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.AddressTeachRespFieldNumber, Receive_Cmd74_AddressTeachResponse);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.AlarmResetReqFieldNumber, Receive_Cmd91_AlarmResetRequest);
            ClientAgent.addTcpIpReceivedHandler(WrapperMessage.AlarmRespFieldNumber, Receive_Cmd94_AlarmResponse);
            //
            for (int i = 0; i < 200; i++)
            {
                ClientAgent.addTcpIpReceivedHandler(i, RecieveCmdShowOnCommunicationForm);
            }
            //Here need to be careful for the TCPIP
            //

            ClientAgent.addTcpIpConnectedHandler(DoConnection);       //連線時的通知
            ClientAgent.addTcpIpDisconnectedHandler(DoDisconnection); //斷線時的通知

            //OnCmdReceive += MiddleAgent_OnMsgFromAgvcEvent;
        }

        public void SendMiddlerFormConfigCommand(int cmdNum, Dictionary<string, string> pairs)
        {
            try
            {
                WrapperMessage wrappers = new WrapperMessage();
                string msgShow = "";

                var cmdType = (EnumCmdNums)cmdNum;
                switch (cmdType)
                {
                    case EnumCmdNums.Cmd31_TransferRequest:
                        {
                            ID_31_TRANS_REQUEST aCmd = new ID_31_TRANS_REQUEST();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.ActType = ActiveTypeConverter(pairs["ActType"]);
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

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd32_TransferCompleteResponse:
                        {
                            ID_32_TRANS_COMPLETE_RESPONSE aCmd = new ID_32_TRANS_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.TranCmpRespFieldNumber;
                            wrappers.TranCmpResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd33_ControlZoneCancelRequest:
                        {
                            ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST aCmd = new ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST();
                            aCmd.CancelSecID = pairs["CancelSecID"];
                            aCmd.ControlType = ControlTypeConverter(pairs["ControlType"]);

                            wrappers.ID = WrapperMessage.ControlZoneReqFieldNumber;
                            wrappers.ControlZoneReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd35_CarrierIdRenameRequest:
                        {
                            ID_35_CST_ID_RENAME_REQUEST aCmd = new ID_35_CST_ID_RENAME_REQUEST();
                            aCmd.NEWCSTID = pairs["NEWCSTID"];
                            aCmd.OLDCSTID = pairs["OLDCSTID"];

                            wrappers.ID = WrapperMessage.CSTIDRenameReqFieldNumber;
                            wrappers.CSTIDRenameReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd36_TransferEventResponse:
                        {
                            ID_36_TRANS_EVENT_RESPONSE aCmd = new ID_36_TRANS_EVENT_RESPONSE();
                            aCmd.IsBlockPass = PassTypeConverter(pairs["IsBlockPass"]);
                            aCmd.IsReserveSuccess = ReserveResultConverter(pairs["IsReserveSuccess"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.ImpTransEventRespFieldNumber;
                            wrappers.ImpTransEventResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd37_TransferCancelRequest:
                        {
                            ID_37_TRANS_CANCEL_REQUEST aCmd = new ID_37_TRANS_CANCEL_REQUEST();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.ActType = CMDCancelTypeConverter(pairs["ActType"]);

                            wrappers.ID = WrapperMessage.TransCancelReqFieldNumber;
                            wrappers.TransCancelReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd39_PauseRequest:
                        {
                            ID_39_PAUSE_REQUEST aCmd = new ID_39_PAUSE_REQUEST();
                            aCmd.EventType = PauseEventConverter(pairs["EventType"]);
                            aCmd.PauseType = PauseTypeConverter(pairs["PauseType"]);

                            wrappers.ID = WrapperMessage.PauseReqFieldNumber;
                            wrappers.PauseReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd41_ModeChange:
                        {
                            ID_41_MODE_CHANGE_REQ aCmd = new ID_41_MODE_CHANGE_REQ();
                            aCmd.OperatingVHMode = OperatingVHModeConverter(pairs["EventType"]);

                            wrappers.ID = WrapperMessage.ModeChangeReqFieldNumber;
                            wrappers.ModeChangeReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd43_StatusRequest:
                        {
                            ID_43_STATUS_REQUEST aCmd = new ID_43_STATUS_REQUEST();

                            wrappers.ID = WrapperMessage.StatusReqFieldNumber;
                            wrappers.StatusReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd44_StatusRequest:
                        {
                            ID_44_STATUS_CHANGE_RESPONSE aCmd = new ID_44_STATUS_CHANGE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.StatusChangeRespFieldNumber;
                            wrappers.StatusChangeResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd45_PowerOnoffRequest:
                        {
                            ID_45_POWER_OPE_REQ aCmd = new ID_45_POWER_OPE_REQ();
                            aCmd.OperatingPowerMode = OperatingPowerModeConverter(pairs["OperatingPowerMode"]);

                            wrappers.ID = WrapperMessage.PowerOpeReqFieldNumber;
                            wrappers.PowerOpeReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd51_AvoidRequest:
                        {
                            ID_51_AVOID_REQUEST aCmd = new ID_51_AVOID_REQUEST();
                            aCmd.GuideAddresses.AddRange(StringSpilter(pairs["GuideAddresses"]));
                            aCmd.GuideSections.AddRange(StringSpilter(pairs["GuideSections"]));

                            wrappers.ID = WrapperMessage.AvoidReqFieldNumber;
                            wrappers.AvoidReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd52_AvoidCompleteResponse:
                        {
                            ID_52_AVOID_COMPLETE_RESPONSE aCmd = new ID_52_AVOID_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AvoidCompleteRespFieldNumber;
                            wrappers.AvoidCompleteResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd71_RangeTeachRequest:
                        {
                            ID_71_RANGE_TEACHING_REQUEST aCmd = new ID_71_RANGE_TEACHING_REQUEST();
                            aCmd.FromAdr = pairs["FromAdr"];
                            aCmd.ToAdr = pairs["ToAdr"];

                            wrappers.ID = WrapperMessage.RangeTeachingReqFieldNumber;
                            wrappers.RangeTeachingReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd72_RangeTeachCompleteResponse:
                        {
                            ID_72_RANGE_TEACHING_COMPLETE_RESPONSE aCmd = new ID_72_RANGE_TEACHING_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.RangeTeachingCmpRespFieldNumber;
                            wrappers.RangeTeachingCmpResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd74_AddressTeachResponse:
                        {
                            ID_74_ADDRESS_TEACH_RESPONSE aCmd = new ID_74_ADDRESS_TEACH_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AddressTeachRespFieldNumber;
                            wrappers.AddressTeachResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd91_AlarmResetRequest:
                        {
                            ID_91_ALARM_RESET_REQUEST aCmd = new ID_91_ALARM_RESET_REQUEST();

                            wrappers.ID = WrapperMessage.AlarmResetReqFieldNumber;
                            wrappers.AlarmResetReq = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd94_AlarmResponse:
                        {
                            ID_94_ALARM_RESPONSE aCmd = new ID_94_ALARM_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd131_TransferResponse:
                        {
                            ID_131_TRANS_RESPONSE aCmd = new ID_131_TRANS_RESPONSE();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.ActType = ActiveTypeConverter(pairs["ActType"]);
                            aCmd.NgReason = pairs["NgReason"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.TransRespFieldNumber;
                            wrappers.TransResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd132_TransferCompleteReport:
                        {
                            ID_132_TRANS_COMPLETE_REPORT aCmd = new ID_132_TRANS_COMPLETE_REPORT();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.CmdDistance = int.Parse(pairs["CmdDistance"]);
                            aCmd.CmdPowerConsume = uint.Parse(pairs["CmdPowerConsume"]);
                            aCmd.CmpStatus = CompleteStatusConverter(pairs["CmpStatus"]);
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];

                            wrappers.ID = WrapperMessage.TranCmpRepFieldNumber;
                            wrappers.TranCmpRep = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd133_ControlZoneCancelResponse:
                        {
                            ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE aCmd = new ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE();
                            aCmd.CancelSecID = pairs["CancelSecID"];
                            aCmd.ControlType = ControlTypeConverter(pairs["ControlType"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.ControlZoneRespFieldNumber;
                            wrappers.ControlZoneResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd134_TransferEventReport:
                        {
                            ID_134_TRANS_EVENT_REP aCmd = new ID_134_TRANS_EVENT_REP();
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];
                            aCmd.EventType = EventTypeConverter(pairs["EventType"]);
                            aCmd.DrivingDirection = DriveDirctionConverter(pairs["DrivingDirection"]);

                            wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                            wrappers.TransEventRep = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd135_CarrierIdRenameResponse:
                        {
                            ID_135_CST_ID_RENAME_RESPONSE aCmd = new ID_135_CST_ID_RENAME_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.CSTIDRenameRespFieldNumber;
                            wrappers.CSTIDRenameResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd136_TransferEventReport:
                        {
                            ID_136_TRANS_EVENT_REP aCmd = new ID_136_TRANS_EVENT_REP();
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];
                            aCmd.EventType = EventTypeConverter(pairs["EventType"]);

                            wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                            wrappers.ImpTransEventRep = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd137_TransferCancelResponse:
                        {
                            ID_137_TRANS_CANCEL_RESPONSE aCmd = new ID_137_TRANS_CANCEL_RESPONSE();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.ActType = CMDCancelTypeConverter(pairs["ActType"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.TransCancelRespFieldNumber;
                            wrappers.TransCancelResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd139_PauseResponse:
                        {
                            ID_139_PAUSE_RESPONSE aCmd = new ID_139_PAUSE_RESPONSE();
                            aCmd.EventType = PauseEventConverter(pairs["EventType"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.PauseRespFieldNumber;
                            wrappers.PauseResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd141_ModeChangeResponse:
                        {
                            ID_141_MODE_CHANGE_RESPONSE aCmd = new ID_141_MODE_CHANGE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.ModeChangeRespFieldNumber;
                            wrappers.ModeChangeResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd143_StatusResponse:
                        {
                            //TODO: 補完屬性
                            ID_143_STATUS_RESPONSE aCmd = new ID_143_STATUS_RESPONSE();
                            aCmd.ActionStatus = VHActionStatusConverter(pairs["ActionStatus"]);
                            aCmd.BatteryCapacity = uint.Parse(pairs["BatteryCapacity"]);
                            aCmd.BatteryTemperature = int.Parse(pairs["BatteryTemperature"]);
                            aCmd.BlockingStatus = VhStopSingleConverter(pairs["BlockingStatus"]);
                            aCmd.ChargeStatus = VhChargeStatusConverter(pairs["ChargeStatus"]);
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];

                            wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                            wrappers.StatusReqResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd144_StatusReport:
                        {
                            //TODO: 補完屬性
                            ID_144_STATUS_CHANGE_REP aCmd = new ID_144_STATUS_CHANGE_REP();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.ActionStatus = VHActionStatusConverter(pairs["ActionStatus"]);
                            aCmd.BatteryCapacity = uint.Parse(pairs["BatteryCapacity"]);
                            aCmd.BatteryTemperature = int.Parse(pairs["BatteryTemperature"]);
                            aCmd.BlockingStatus = VhStopSingleConverter(pairs["BlockingStatus"]);
                            aCmd.ChargeStatus = VhChargeStatusConverter(pairs["ChargeStatus"]);
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.CSTID = pairs["CSTID"];

                            wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                            wrappers.StatueChangeRep = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd145_PowerOnoffResponse:
                        {
                            ID_145_POWER_OPE_RESPONSE aCmd = new ID_145_POWER_OPE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.PowerOpeRespFieldNumber;
                            wrappers.PowerOpeResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd151_AvoidResponse:
                        {
                            ID_151_AVOID_RESPONSE aCmd = new ID_151_AVOID_RESPONSE();
                            aCmd.NgReason = pairs["NgReason"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AvoidRespFieldNumber;
                            wrappers.AvoidResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd152_AvoidCompleteReport:
                        {
                            ID_152_AVOID_COMPLETE_REPORT aCmd = new ID_152_AVOID_COMPLETE_REPORT();
                            aCmd.CmpStatus = int.Parse(pairs["CmpStatus"]);

                            wrappers.ID = WrapperMessage.AvoidCompleteRepFieldNumber;
                            wrappers.AvoidCompleteRep = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd171_RangeTeachResponse:
                        {
                            ID_171_RANGE_TEACHING_RESPONSE aCmd = new ID_171_RANGE_TEACHING_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.RangeTeachingRespFieldNumber;
                            wrappers.RangeTeachingResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd172_RangeTeachCompleteReport:
                        {
                            ID_172_RANGE_TEACHING_COMPLETE_REPORT aCmd = new ID_172_RANGE_TEACHING_COMPLETE_REPORT();
                            aCmd.CompleteCode = int.Parse(pairs["CompleteCode"]);
                            aCmd.FromAdr = pairs["FromAdr"];
                            aCmd.SecDistance = uint.Parse(pairs["SecDistance"]);
                            aCmd.ToAdr = pairs["ToAdr"];

                            wrappers.ID = WrapperMessage.RangeTeachingCmpRepFieldNumber;
                            wrappers.RangeTeachingCmpRep = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd174_AddressTeachReport:
                        {
                            ID_174_ADDRESS_TEACH_REPORT aCmd = new ID_174_ADDRESS_TEACH_REPORT();
                            aCmd.Addr = pairs["Addr"];
                            aCmd.Position = int.Parse(pairs["Position"]);

                            wrappers.ID = WrapperMessage.AddressTeachRepFieldNumber;
                            wrappers.AddressTeachRep = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd191_AlarmResetResponse:
                        {
                            ID_191_ALARM_RESET_RESPONSE aCmd = new ID_191_ALARM_RESET_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AlarmResetRespFieldNumber;
                            wrappers.AlarmResetResp = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd194_AlarmReport:
                        {
                            ID_194_ALARM_REPORT aCmd = new ID_194_ALARM_REPORT();
                            aCmd.ErrCode = pairs["ErrCode"];
                            aCmd.ErrDescription = pairs["ErrDescription"];
                            aCmd.ErrStatus = ErrorStatusConverter(pairs["ErrStatus"]);

                            wrappers.ID = WrapperMessage.AlarmRepFieldNumber;
                            wrappers.AlarmRep = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                    case EnumCmdNums.Cmd000_EmptyCommand:
                    default:
                        {
                            ID_1_HOST_BASIC_INFO_VERSION_REP aCmd = new ID_1_HOST_BASIC_INFO_VERSION_REP();

                            wrappers.ID = WrapperMessage.HostBasicInfoRepFieldNumber;
                            wrappers.HostBasicInfoRep = aCmd;

                            msgShow = $"[{cmdType}] [{aCmd}]";
                            break;
                        }
                }

                SendCommandWrapper(wrappers);  //似乎是SendFunction底層會咬住等待回應所以開THD去發  

                OnCmdSend?.Invoke(this, msgShow);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }

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

        private void SendCommandWrapper(WrapperMessage wrapper)
        {
            Task.Run(() => ClientAgent.TrxTcpIp.SendGoogleMsg(wrapper));
        }

        public void SetupNeedReserveSections(MapSection needReserveSection)
        {
            PauseAskingReserve();
            this.needReserveSection = needReserveSection.DeepClone();
            ResumeAskingReserve();
        }

        public string GetNeedReserveSectionId()
        {
            return needReserveSection.Id;
        }

        public void PauseAskingReserve()
        {
            askReservePauseEvent.Reset();
        }

        public void ResumeAskingReserve()
        {
            askReservePauseEvent.Set();
        }

        public void StartAskingReserve()
        {
            askReservePauseEvent.Set();
            askReserveShutdownEvent.Reset();
            thdAskReserve = new Thread(new ThreadStart(AskReserve));
            thdAskReserve.IsBackground = true;
            thdAskReserve.Start();
        }

        public void StopAskingReserve()
        {
            askReserveShutdownEvent.Set();
            askReservePauseEvent.Set();

            //if (thdAskReserve.IsAlive)
            //{
            //    thdAskReserve.Abort();
            //    thdAskReserve.Join();
            //}
        }

        #region EnumConverter
        private VhChargeStatus VhChargeStatusConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (VhChargeStatus)Enum.Parse(typeof(VhChargeStatus), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return VhChargeStatus.ChargeStatusCharging;
            }
        }

        private VhStopSingle VhStopSingleConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (VhStopSingle)Enum.Parse(typeof(VhStopSingle), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return VhStopSingle.StopSingleOff;
            }
        }

        private VHActionStatus VHActionStatusConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (VHActionStatus)Enum.Parse(typeof(VHActionStatus), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return VHActionStatus.Commanding;
            }
        }

        private DriveDirction DriveDirctionConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (DriveDirction)Enum.Parse(typeof(DriveDirction), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return DriveDirction.DriveDirForward;
            }
        }

        private EventType EventTypeConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (EventType)Enum.Parse(typeof(EventType), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return EventType.AdrOrMoveArrivals;
            }
        }

        private CompleteStatus CompleteStatusConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (CompleteStatus)Enum.Parse(typeof(CompleteStatus), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return CompleteStatus.CmpStatusAbort;
            }
        }

        private OperatingPowerMode OperatingPowerModeConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (OperatingPowerMode)Enum.Parse(typeof(OperatingPowerMode), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return OperatingPowerMode.OperatingPowerOff;
            }
        }

        private OperatingVHMode OperatingVHModeConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (OperatingVHMode)Enum.Parse(typeof(OperatingVHMode), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return OperatingVHMode.OperatingAuto;
            }
        }

        private PauseType PauseTypeConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (PauseType)Enum.Parse(typeof(PauseType), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return PauseType.None;
            }
        }

        private PauseEvent PauseEventConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (PauseEvent)Enum.Parse(typeof(PauseEvent), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return PauseEvent.Pause;
            }
        }

        private CMDCancelType CMDCancelTypeConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (CMDCancelType)Enum.Parse(typeof(CMDCancelType), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return CMDCancelType.CmdAbout;
            }
        }

        private ReserveResult ReserveResultConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (ReserveResult)Enum.Parse(typeof(ReserveResult), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return ReserveResult.Success;
            }
        }

        private PassType PassTypeConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (PassType)Enum.Parse(typeof(PassType), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return PassType.Pass;
            }
        }

        private ErrorStatus ErrorStatusConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (ErrorStatus)Enum.Parse(typeof(ErrorStatus), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return ErrorStatus.ErrReset;
            }
        }

        private ActiveType ActiveTypeConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (ActiveType)Enum.Parse(typeof(ActiveType), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return ActiveType.Home;
            }
        }

        private ControlType ControlTypeConverter(string v)
        {
            try
            {
                v = v.Trim();

                return (ControlType)Enum.Parse(typeof(ControlType), v);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return ControlType.Nothing;
            }
        }
        #endregion

        private void RecieveCmdShowOnCommunicationForm(object sender, TcpIpEventArgs e)
        {
            var cmdName = (EnumCmdNums)int.Parse(e.iPacketID);
            string msg = $"[{cmdName}][e.iSeqNum = {e.iSeqNum}][e.objPacket = {e.objPacket}]";
            OnCmdReceive?.Invoke(this, msg);
        }

        private void MiddleAgent_OnMsgFromAgvcEvent(object sender, string e)
        {
            string className = GetType().Name;
            string methodName = sender.ToString(); //System.Reflection.MethodBase.GetCurrentMethod().Name;
            string classMethodName = className + ":" + methodName;
            LogFormat logFormat = new LogFormat("Debug", "3", classMethodName, "Device", "CarrierID", e);
            theLoggerAgent.LogMsg("Debug", logFormat);
        }

        protected void DoConnection(object sender, TcpIpEventArgs e)
        {
            TcpIpAgent agent = sender as TcpIpAgent;
            var msg = $"Vh ID:{agent.Name}, connection.";
            Console.WriteLine(msg);
            OnCmdReceive?.Invoke(this, msg);
            OnConnected?.Invoke(this, "Connected");
        }
        protected void DoDisconnection(object sender, TcpIpEventArgs e)
        {
            TcpIpAgent agent = sender as TcpIpAgent;
            var msg = $"Vh ID:{agent.Name}, disconnection.";
            Console.WriteLine(msg);
            OnCmdReceive?.Invoke(this, msg);
            OnDisConnected?.Invoke(this, "Dis-Connect");
        }


        public void ReportLoadArrivals()
        {
            theVehicle.Cmd134EventType = EventType.AdrOrMoveArrivals;
            Send_Cmd134_TransferEventReport();
            Send_Cmd136_TransferEventReport(EventType.LoadArrivals);
        }
        public void UnloadArrivals()
        {
            theVehicle.Cmd134EventType = EventType.AdrOrMoveArrivals;
            Send_Cmd134_TransferEventReport();
            Send_Cmd136_TransferEventReport(EventType.UnloadArrivals);
        }
        public void MoveComplete()
        {
            theVehicle.Cmd134EventType = EventType.AdrOrMoveArrivals;
            Send_Cmd134_TransferEventReport();
            Send_Cmd136_TransferEventReport(EventType.AdrOrMoveArrivals);
            theVehicle.CompleteStatus = CompleteStatus.CmpStatusMove;
            Send_Cmd132_TransferCompleteReport();
        }
        public void LoadComplete()
        {
            Send_Cmd136_TransferEventReport(EventType.LoadComplete);
            theVehicle.CompleteStatus = CompleteStatus.CmpStatusLoad;
            Send_Cmd132_TransferCompleteReport();
        }
        public void LoadCompleteInLoadunload()
        {
            Send_Cmd136_TransferEventReport(EventType.LoadComplete);
        }
        public void UnloadComplete()
        {
            Send_Cmd136_TransferEventReport(EventType.UnloadComplete);
            theVehicle.CompleteStatus = CompleteStatus.CmpStatusUnload;
            Send_Cmd132_TransferCompleteReport();
        }
        public void LoadUnloadComplete()
        {
            Send_Cmd136_TransferEventReport(EventType.UnloadComplete);
            theVehicle.CompleteStatus = CompleteStatus.CmpStatusLoadunload;
            Send_Cmd132_TransferCompleteReport();
        }
        public void MainFlowGetCancel()
        {
            theVehicle.CompleteStatus = CompleteStatus.CmpStatusCancel;
            Send_Cmd132_TransferCompleteReport();
        }
        public void MainFlowGetAbort()
        {
            theVehicle.CompleteStatus = CompleteStatus.CmpStatusAbort;
            Send_Cmd132_TransferCompleteReport();
        }

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

                OnCmdSend?.Invoke(this, "[Cmd194]" + iD_194_ALARM_REPORT.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd91_AlarmResetRequest(object sender, TcpIpEventArgs e)
        {
            ID_91_ALARM_RESET_REQUEST receive = (ID_91_ALARM_RESET_REQUEST)e.objPacket;
            //TODO: Reset alarm



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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd191]" + iD_191_ALARM_RESET_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
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

                OnCmdSend?.Invoke(this, "[Cmd174]" + iD_174_ADDRESS_TEACH_REPORT.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd72_RangeTeachCompleteResponse(object sender, TcpIpEventArgs e)
        {
            ID_72_RANGE_TEACHING_COMPLETE_RESPONSE receive = (ID_72_RANGE_TEACHING_COMPLETE_RESPONSE)e.objPacket;



        }
        public void Send_Cmd172_RangeTeachCompleteReport(int completeCode)
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd172]" + iD_172_RANGE_TEACHING_COMPLETE_REPORT.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd171]" + iD_171_RANGE_TEACHING_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd52_AvoidCompleteResponse(object sender, TcpIpEventArgs e)
        {
            ID_52_AVOID_COMPLETE_RESPONSE receive = (ID_52_AVOID_COMPLETE_RESPONSE)e.objPacket;



            int completeStatus = 0;
        }
        public void Send_Cmd152_AvoidCompleteReport(int completeStatus)
        {
            try
            {
                //TODO: Avoid

                ID_152_AVOID_COMPLETE_REPORT iD_152_AVOID_COMPLETE_REPORT = new ID_152_AVOID_COMPLETE_REPORT();
                iD_152_AVOID_COMPLETE_REPORT.CmpStatus = completeStatus;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AvoidCompleteRepFieldNumber;
                wrappers.AvoidCompleteRep = iD_152_AVOID_COMPLETE_REPORT;

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd152]" + iD_152_AVOID_COMPLETE_REPORT.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd51_AvoidRequest(object sender, TcpIpEventArgs e)
        {
            ID_51_AVOID_REQUEST receive = (ID_51_AVOID_REQUEST)e.objPacket;
            //TODO: Avoid



            int replyCode = 0;
            string reason = "Empty";
            Send_Cmd151_AvoidResponse(e.iSeqNum, replyCode, reason);
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd151]" + iD_151_AVOID_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd145]" + iD_145_POWER_OPE_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private void Receive_Cmd44_StatusRequest(object sender, TcpIpEventArgs e)
        {
            ID_43_STATUS_REQUEST receive = (ID_43_STATUS_REQUEST)e.objPacket; // Cmd43's object is empty



            Send_Cmd144_StatusChangeReport();
        }
        public void Send_Cmd144_StatusChangeReport()
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

                var result = ClientAgent.TrxTcpIp.sendRecv_Google(wrappers, out ID_44_STATUS_CHANGE_RESPONSE receive, out string rtnMsg);

                OnCmdSend?.Invoke(this, "[Cmd144]" + iD_144_STATUS_CHANGE_REP.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }

        }

        private void Receive_Cmd43_StatusRequest(object sender, TcpIpEventArgs e)
        {
            ID_43_STATUS_REQUEST receive = (ID_43_STATUS_REQUEST)e.objPacket; // Cmd43's object is empty



            Send_Cmd143_StatusResponse(e.iSeqNum);
        }
        public void Send_Cmd143_StatusResponse(ushort seqNum)
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd143]" + iD_143_STATUS_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }

        }

        public void Receive_Cmd41_ModeChange(object sender, TcpIpEventArgs e)
        {
            ID_41_MODE_CHANGE_REQ receive = (ID_41_MODE_CHANGE_REQ)e.objPacket;
            //TODO: Auto/Manual



            int replyCode = 0;
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd141]" + iD_141_MODE_CHANGE_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd39_PauseRequest(object sender, TcpIpEventArgs e)
        {
            ID_39_PAUSE_REQUEST receive = (ID_39_PAUSE_REQUEST)e.objPacket;
            //TODO: Pause/Continue+/Reserve



            int replyCode = 0;
            Send_Cmd139_PauseResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd139_PauseResponse(ushort seqNum, int replyCode)
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd139]" + iD_139_PAUSE_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd37_TransferCancelRequest(object sender, TcpIpEventArgs e)
        {

            ID_37_TRANS_CANCEL_REQUEST receive = (ID_37_TRANS_CANCEL_REQUEST)e.objPacket;
            bool result = false;
            switch (receive.ActType)
            {
                case CMDCancelType.CmdCancel:
                    if (CanVehCancel())
                    {
                        result = true;
                        if (OnTransferCancelEvent != null)
                        {
                            OnTransferCancelEvent(this, receive.CmdID);
                        }
                    }
                    break;
                case CMDCancelType.CmdAbout:
                    if (CanVehAbort())
                    {
                        result = true;
                        if (OnTransferAbortEvent != null)
                        {
                            OnTransferAbortEvent(this, receive.CmdID);
                        }
                    }
                    break;
                default:
                    break;
            }


            int replyCode = result ? 0 : 1;
            Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd137_TransferCancelResponse(ushort seqNum, int replyCode)
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd137]" + iD_137_TRANS_CANCEL_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }
        private bool CanVehAbort()
        {
            throw new NotImplementedException();
        }
        private bool CanVehCancel()
        {
            throw new NotImplementedException();
        }

        public void Receive_Cmd36_TransferEventResponse(object sender, TcpIpEventArgs e)
        {
            ID_36_TRANS_EVENT_RESPONSE receive = (ID_36_TRANS_EVENT_RESPONSE)e.objPacket;
            //Get reserve, block, 

            //if (receive.IsReserveSuccess == ReserveResult.Success)
            //{
            //    OnGetReserveOkEvent?.Invoke(this, true);
            //}

            if (receive.IsBlockPass == PassType.Pass)
            {
                OnGetBlockPassEvent?.Invoke(this, true);
            }
        }
        public void Send_Cmd136_TransferEventReport(EventType eventType)
        {
            VehLocation vehLocation = theVehicle.GetVehLoacation();
            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicle.CarrierID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.Address.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.Section.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.Section.Distance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd136]" + iD_136_TRANS_EVENT_REP.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }

        }
        public void Send_Cmd136_RequestBlock(string requestBlockID)
        {
            VehLocation vehLocation = theVehicle.GetVehLoacation();

            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = EventType.BlockReq;
                iD_136_TRANS_EVENT_REP.RequestBlockID = requestBlockID;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicle.CarrierID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.Address.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.Section.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.Section.Distance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd136]" + iD_136_TRANS_EVENT_REP.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }
        public void Send_Cmd136_ReleaseBlock(string releaseBlockAdrID)
        {
            VehLocation vehLocation = theVehicle.GetVehLoacation();

            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = EventType.BlockRelease;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicle.CarrierID;
                iD_136_TRANS_EVENT_REP.ReleaseBlockAdrID = releaseBlockAdrID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.Address.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.Section.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.Section.Distance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd136]" + iD_136_TRANS_EVENT_REP.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }
        public bool Send_Cmd136_AskReserve()
        {
            VehLocation vehLocation = theVehicle.GetVehLoacation();

            try
            {
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                iD_136_TRANS_EVENT_REP.EventType = EventType.ReserveReq;
                FitReserveInfos(iD_136_TRANS_EVENT_REP.ReserveInfos);
                iD_136_TRANS_EVENT_REP.CSTID = theVehicle.CarrierID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = vehLocation.Address.Id;
                iD_136_TRANS_EVENT_REP.CurrentSecID = vehLocation.Section.Id;
                iD_136_TRANS_EVENT_REP.SecDistance = (uint)vehLocation.Section.Distance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                OnCmdSend?.Invoke(this, "[Cmd136]" + iD_136_TRANS_EVENT_REP.ToString());
                // SendCommandWrapper(wrappers);
                ClientAgent.TrxTcpIp.sendRecv_Google(wrappers, out ID_36_TRANS_EVENT_RESPONSE resultObj, out string resultMsg);
                if (string.IsNullOrEmpty(resultMsg))
                {
                    if (resultObj.IsReserveSuccess == ReserveResult.Success)
                    {
                        return true;
                    }
                }

                return false;

            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
                return false;
            }
        }
        private void FitReserveInfos(RepeatedField<ReserveInfo> reserveInfos)
        {
            reserveInfos.Clear();

            ReserveInfo reserveInfo = new ReserveInfo();
            reserveInfo.ReserveSectionID = needReserveSection.Id;
            if (needReserveSection.CmdDirection == EnumPermitDirection.Backward)
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirReverse;
            }
            else if (needReserveSection.CmdDirection == EnumPermitDirection.None)
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
            bool result = theVehicle.CarrierID == receive.OLDCSTID;
            if (result)
            {
                theVehicle.CarrierID = receive.NEWCSTID;
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd135]" + iD_135_CST_ID_RENAME_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Send_Cmd134_TransferEventReport()
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

                var resp = ClientAgent.TrxTcpIp.SendGoogleMsg(wrappers, false);

                OnCmdSend?.Invoke(this, "[Cmd134]" + iD_134_TRANS_EVENT_REP.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd133]" + iD_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd32_TransferCompleteResponse(object sender, TcpIpEventArgs e)
        {
            ID_32_TRANS_COMPLETE_RESPONSE receive = (ID_32_TRANS_COMPLETE_RESPONSE)e.objPacket;

            Send_Cmd132_TransferCompleteReport();

        }
        public void Send_Cmd132_TransferCompleteReport()
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd132]" + iD_132_TRANS_COMPLETE_REPORT.ToString());
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd31_TransferRequest(object sender, TcpIpEventArgs e)
        {
            ID_31_TRANS_REQUEST transRequest = (ID_31_TRANS_REQUEST)e.objPacket;
            theVehicle.Cmd131ActType = transRequest.ActType;

            AgvcTransCmd agvcTransCmd = ConvertAgvcTransCmdIntoPackage(transRequest, e.iSeqNum);

            OnInstallTransferCommandEvent?.Invoke(this, agvcTransCmd);
        }
        public void Send_Cmd131_TransferResponse(ushort seqNum, int replyCode, string reason)
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

                SendCommandWrapper(wrappers);

                OnCmdSend?.Invoke(this, "[Cmd131]" + iD_131_TRANS_RESPONSE.ToString());
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
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else if (theVehicle.GetBattery().IsBatteryHighTemperature())
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is in battery temperature too hight can not do transfer command.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
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
                    Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                    return true;
            }
        }

        private bool CanVehOverride(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (VehInOverrideSection(transRequest))
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return true;
            }
            else
            {
                int replyCode = 1; // NG
                string reason = "Vehicle current section not in override guideSections.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
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
            Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
            return true;
        }

        private bool CanVehLoadunload(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (theVehicle.ActionStatus != VHActionStatus.NoCommand)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is not idle can not do loadunload.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else if (theVehicle.HasCst == VhLoadCSTStatus.Exist)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle has a carrier can not do loadunload.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.LoadAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no load address can not do load.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.DestinationAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no unload address can not do unload.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return true;
            }
        }

        private bool CanVehUnload(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (theVehicle.ActionStatus != VHActionStatus.NoCommand)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is not idle can not do unload.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else if (theVehicle.HasCst == VhLoadCSTStatus.NotExist)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle has no carrier can not do unload.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.DestinationAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no unload address can not do unload.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }

            else
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return true;
            }
        }

        private bool CanVehLoad(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (theVehicle.ActionStatus != VHActionStatus.NoCommand)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is not idle can not do load.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else if (theVehicle.HasCst == VhLoadCSTStatus.Exist)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle has a carrier can not do load.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.LoadAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no load address can not do load.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return true;
            }
        }

        private bool CanVehMove(ID_31_TRANS_REQUEST transRequest, ushort seqNum)
        {
            if (theVehicle.ActionStatus != VHActionStatus.NoCommand)
            {
                int replyCode = 1; // NG
                string reason = "Vehicle is not idle can not do move.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else if (string.IsNullOrEmpty(transRequest.DestinationAdr))
            {
                int replyCode = 1; // NG
                string reason = "Transfer command has no move-end address can not do move.";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return false;
            }
            else
            {
                int replyCode = 0; // OK
                string reason = "Empty";
                Send_Cmd131_TransferResponse(seqNum, replyCode, reason);
                return true;
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
                case ActiveType.Home:
                    return new AgvcHomeCmd(transRequest, iSeqNum);
                case ActiveType.Override:
                    return new AgvcOverrideCmd(transRequest, iSeqNum);
                case ActiveType.Mtlhome:
                case ActiveType.Movetomtl:
                case ActiveType.Systemout:
                case ActiveType.Systemin:
                case ActiveType.Techingmove:
                case ActiveType.Round:
                default:
                    return new AgvcTransCmd(transRequest, iSeqNum);
            }
        }

        public void OnMapBarcodeValuesChangedEvent(object sender, MapBarcodeReader mapBarcodeValues)
        {
            //vehLocation.SetMapBarcodeValues(mapBarcodeValues);
            //TODO: Make a Position change report from mapBarcode and send to AGVC
        }

        public void OnTransCmdsFinishedEvent(object sender, EnumCompleteStatus status)
        {
            //Send Transfer Command Complete Report to Agvc
            theVehicle.CompleteStatus = (CompleteStatus)(int)status;
            Send_Cmd132_TransferCompleteReport();
        }

        public static Google.Protobuf.IMessage unPackWrapperMsg(byte[] raw_data)
        {
            WrapperMessage WarpperMsg = ToObject<WrapperMessage>(raw_data);
            return WarpperMsg;
        }

        public static T ToObject<T>(byte[] buf) where T : Google.Protobuf.IMessage<T>, new()
        {
            if (buf == null)
                return default(T);

            Google.Protobuf.MessageParser<T> parser = new Google.Protobuf.MessageParser<T>(() => new T());
            return parser.ParseFrom(buf);
        }



    }

}
