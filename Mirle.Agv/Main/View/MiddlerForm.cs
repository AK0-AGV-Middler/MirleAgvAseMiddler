using Google.Protobuf.Collections;
using Mirle.AgvAseMiddler.Controller;
using Mirle.AgvAseMiddler.Controller.Tools;
using Mirle.AgvAseMiddler.Model.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TcpIpClientSample;
using System.Threading.Tasks;
using Mirle.Tools;

namespace Mirle.AgvAseMiddler.View
{
    public partial class MiddlerForm : Form
    {
        private MiddleAgent middleAgent;
        private MiddlerConfig middlerConfig;
        private string CommLogMsg { get; set; } = "";
        private string ConnectionMsg { get; set; } = "";

        public MiddlerForm(MiddleAgent middleAgent)
        {
            InitializeComponent();
            this.middleAgent = middleAgent;
            middlerConfig = middleAgent.GetMiddlerConfig();
            EventInital();
        }

        private void CommunicationForm_Load(object sender, EventArgs e)
        {
            ConfigToUI();
            if (middleAgent.ClientAgent != null)
            {
                if (middleAgent.ClientAgent.IsConnection)
                {
                    toolStripStatusLabel1.Text = "Connect";
                }
            }
        }

        private void ConfigToUI()
        {
            txtRemoteIp.Text = middlerConfig.RemoteIp;
            txtRemotePort.Text = middlerConfig.RemotePort.ToString();
        }

        private void EventInital()
        {
            middleAgent.OnCmdReceiveEvent += SendOrReceiveCmdToTextBox;
            middleAgent.OnCmdSendEvent += SendOrReceiveCmdToTextBox;
            middleAgent.OnConnectionChangeEvent += MiddleAgent_OnConnectionChangeEvent;
        }

        private void MiddleAgent_OnConnectionChangeEvent(object sender, bool isConnect)
        {
            try
            {
                if (isConnect)
                {
                    ConnectionMsg = "連線中";
                }
                else
                {
                    ConnectionMsg = "已斷線";
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void SendOrReceiveCmdToTextBox(object sender, string msg)
        {
            try
            {
                AppendCommLogMsg(msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AppendCommLogMsg(string msg)
        {
            try
            {
                CommLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", msg, "\r\n", CommLogMsg);

                if (CommLogMsg.Length > 65535)
                {
                    CommLogMsg = CommLogMsg.Substring(65535);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            int numOfCmdItems = dataGridView1.Rows.Count - 1;
            if (numOfCmdItems < 0)
            {
                return;
            }
            int cmdNum = int.Parse(cbSend.Text.Split('_')[0].Substring(3));
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            string msg = $"[{cbSend.Text}] ";
            for (int i = 0; i < numOfCmdItems; i++)
            {
                var row = dataGridView1.Rows[i];
                msg = string.Concat(msg, $"[{row.Cells[0].Value},{row.Cells[1].Value}]");
                if (!string.IsNullOrEmpty(row.Cells[0].Value.ToString()))
                {
                    pairs[row.Cells[0].Value.ToString()] = row.Cells[1].Value.ToString();
                }
            }
            AppendCommLogMsg(msg);

            middleAgent.SendMiddlerFormCommands(cmdNum, pairs);
        }

        private void cbSend_SelectedValueChanged(object sender, EventArgs e)
        {
            string selectCmd = cbSend.Text.Split('_')[0].Substring(3);
            int selectCmdNum = int.Parse(selectCmd);
            SetDataGridViewFromCmdNum(selectCmdNum);
        }

        private void SetDataGridViewFromCmdNum(int selectCmdNum)
        {

            PropertyInfo[] infos;
            var cmdType = (EnumCmdNum)selectCmdNum;

            switch (cmdType)
            {
                case EnumCmdNum.Cmd000_EmptyCommand:
                    ID_2_BASIC_INFO_VERSION_RESPONSE cmd002 = new ID_2_BASIC_INFO_VERSION_RESPONSE();
                    infos = cmd002.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd002);
                    break;
                case EnumCmdNum.Cmd31_TransferRequest:
                    ID_31_TRANS_REQUEST cmd31 = new ID_31_TRANS_REQUEST();
                    cmd31.CmdID = "Cmd001";
                    cmd31.CSTID = "Cst001";
                    cmd31.ActType = ActiveType.Move;
                    cmd31.DestinationAdr = "Adr001";
                    cmd31.LoadAdr = "Adr002";
                    infos = cmd31.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd31);
                    break;
                case EnumCmdNum.Cmd32_TransferCompleteResponse:
                    ID_32_TRANS_COMPLETE_RESPONSE cmd32 = new ID_32_TRANS_COMPLETE_RESPONSE();
                    cmd32.ReplyCode = 0;
                    infos = cmd32.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd32);
                    break;
                case EnumCmdNum.Cmd33_ControlZoneCancelRequest:
                    ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST cmd33 = new ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST();
                    cmd33.CancelSecID = "Sec001";
                    cmd33.ControlType = ControlType.Nothing;
                    infos = cmd33.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd33);
                    break;
                case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                    ID_35_CST_ID_RENAME_REQUEST cmd35 = new ID_35_CST_ID_RENAME_REQUEST();
                    cmd35.OLDCSTID = "Cst001";
                    cmd35.NEWCSTID = "Cst002";
                    infos = cmd35.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd35);
                    break;
                case EnumCmdNum.Cmd36_TransferEventResponse:
                    ID_36_TRANS_EVENT_RESPONSE cmd36 = new ID_36_TRANS_EVENT_RESPONSE();
                    cmd36.IsBlockPass = PassType.Pass;
                    cmd36.IsReserveSuccess = ReserveResult.Success;
                    cmd36.ReplyCode = 0;
                    infos = cmd36.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd36);
                    break;
                case EnumCmdNum.Cmd37_TransferCancelRequest:
                    ID_37_TRANS_CANCEL_REQUEST cmd37 = new ID_37_TRANS_CANCEL_REQUEST();
                    cmd37.CmdID = "Cmd001";
                    cmd37.ActType = CMDCancelType.CmdAbort;
                    infos = cmd37.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd37);
                    break;
                case EnumCmdNum.Cmd39_PauseRequest:
                    ID_39_PAUSE_REQUEST cmd39 = new ID_39_PAUSE_REQUEST();
                    cmd39.EventType = PauseEvent.Continue;
                    cmd39.PauseType = PauseType.None;
                    infos = cmd39.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd39);
                    break;
                case EnumCmdNum.Cmd41_ModeChange:
                    ID_41_MODE_CHANGE_REQ cmd41 = new ID_41_MODE_CHANGE_REQ();
                    cmd41.OperatingVHMode = OperatingVHMode.OperatingAuto;
                    infos = cmd41.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd41);
                    break;
                case EnumCmdNum.Cmd43_StatusRequest:
                    ID_43_STATUS_REQUEST cmd43 = new ID_43_STATUS_REQUEST();
                    infos = cmd43.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd43);
                    break;
                case EnumCmdNum.Cmd44_StatusRequest:
                    ID_44_STATUS_CHANGE_RESPONSE cmd44 = new ID_44_STATUS_CHANGE_RESPONSE();
                    cmd44.ReplyCode = 0;
                    infos = cmd44.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd44);
                    break;
                case EnumCmdNum.Cmd45_PowerOnoffRequest:
                    ID_45_POWER_OPE_REQ cmd45 = new ID_45_POWER_OPE_REQ();
                    cmd45.OperatingPowerMode = OperatingPowerMode.OperatingPowerOn;
                    infos = cmd45.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd45);
                    break;
                case EnumCmdNum.Cmd51_AvoidRequest:
                    ID_51_AVOID_REQUEST cmd51 = new ID_51_AVOID_REQUEST();
                    infos = cmd51.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd51);
                    break;
                case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                    ID_52_AVOID_COMPLETE_RESPONSE cmd52 = new ID_52_AVOID_COMPLETE_RESPONSE();
                    cmd52.ReplyCode = 0;
                    infos = cmd52.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd52);
                    break;
                case EnumCmdNum.Cmd71_RangeTeachRequest:
                    ID_71_RANGE_TEACHING_REQUEST cmd71 = new ID_71_RANGE_TEACHING_REQUEST();
                    cmd71.FromAdr = "Adr001";
                    cmd71.ToAdr = "Adr002";
                    infos = cmd71.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd71);
                    break;
                case EnumCmdNum.Cmd72_RangeTeachCompleteResponse:
                    ID_72_RANGE_TEACHING_COMPLETE_RESPONSE cmd72 = new ID_72_RANGE_TEACHING_COMPLETE_RESPONSE();
                    cmd72.ReplyCode = 0;
                    infos = cmd72.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd72);
                    break;
                case EnumCmdNum.Cmd74_AddressTeachResponse:
                    ID_74_ADDRESS_TEACH_RESPONSE cmd74 = new ID_74_ADDRESS_TEACH_RESPONSE();
                    cmd74.ReplyCode = 0;
                    infos = cmd74.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd74);
                    break;
                case EnumCmdNum.Cmd91_AlarmResetRequest:
                    ID_91_ALARM_RESET_REQUEST cmd91 = new ID_91_ALARM_RESET_REQUEST();
                    infos = cmd91.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd91);
                    break;
                case EnumCmdNum.Cmd94_AlarmResponse:
                    ID_94_ALARM_RESPONSE cmd94 = new ID_94_ALARM_RESPONSE();
                    cmd94.ReplyCode = 0;
                    infos = cmd94.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd94);
                    break;
                case EnumCmdNum.Cmd131_TransferResponse:
                    ID_131_TRANS_RESPONSE cmd131 = new ID_131_TRANS_RESPONSE();
                    cmd131.CmdID = "Cmd001";
                    cmd131.ActType = ActiveType.Move;
                    cmd131.NgReason = "";
                    cmd131.ReplyCode = 0;
                    infos = cmd131.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd131);
                    break;
                case EnumCmdNum.Cmd132_TransferCompleteReport:
                    ID_132_TRANS_COMPLETE_REPORT cmd132 = new ID_132_TRANS_COMPLETE_REPORT();
                    cmd132.CmdID = "Cmd001";
                    infos = cmd132.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd132);
                    break;
                case EnumCmdNum.Cmd133_ControlZoneCancelResponse:
                    ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE cmd133 = new ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE();
                    cmd133.CancelSecID = "Sec001";
                    cmd133.ControlType = ControlType.Block;
                    cmd133.ReplyCode = 0;
                    infos = cmd133.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd133);
                    break;
                case EnumCmdNum.Cmd134_TransferEventReport:
                    ID_134_TRANS_EVENT_REP cmd134 = new ID_134_TRANS_EVENT_REP();
                    cmd134.CurrentAdrID = "Adr001";
                    cmd134.CurrentSecID = "Sec001";
                    cmd134.DrivingDirection = DriveDirction.DriveDirForward;
                    cmd134.EventType = EventType.AdrPass;
                    cmd134.SecDistance = 12345;
                    infos = cmd134.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd134);
                    break;
                case EnumCmdNum.Cmd135_CarrierIdRenameResponse:
                    ID_135_CST_ID_RENAME_RESPONSE cmd135 = new ID_135_CST_ID_RENAME_RESPONSE();
                    cmd135.ReplyCode = 0;
                    infos = cmd135.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd135);
                    break;
                case EnumCmdNum.Cmd136_TransferEventReport:
                    ID_136_TRANS_EVENT_REP cmd136 = new ID_136_TRANS_EVENT_REP();
                    cmd136.CSTID = "Cst001";
                    cmd136.CurrentAdrID = "Adr001";
                    cmd136.CurrentSecID = "Sec001";
                    cmd136.EventType = EventType.AdrPass;
                    infos = cmd136.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd136);
                    break;
                case EnumCmdNum.Cmd137_TransferCancelResponse:
                    ID_137_TRANS_CANCEL_RESPONSE cmd137 = new ID_137_TRANS_CANCEL_RESPONSE();
                    cmd137.ReplyCode = 0;
                    infos = cmd137.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd137);
                    break;
                case EnumCmdNum.Cmd139_PauseResponse:
                    ID_139_PAUSE_RESPONSE cmd139 = new ID_139_PAUSE_RESPONSE();
                    cmd139.ReplyCode = 0;
                    infos = cmd139.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd139);
                    break;
                case EnumCmdNum.Cmd141_ModeChangeResponse:
                    ID_141_MODE_CHANGE_RESPONSE cmd141 = new ID_141_MODE_CHANGE_RESPONSE();
                    cmd141.ReplyCode = 0;
                    infos = cmd141.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd141);
                    break;
                case EnumCmdNum.Cmd143_StatusResponse:
                    ID_143_STATUS_RESPONSE cmd143 = new ID_143_STATUS_RESPONSE();
                    cmd143.CmdID = "Cmd001";
                    infos = cmd143.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd143);
                    break;
                case EnumCmdNum.Cmd144_StatusReport:
                    ID_144_STATUS_CHANGE_REP cmd144 = new ID_144_STATUS_CHANGE_REP();
                    cmd144.CmdID = "Cmd001";
                    infos = cmd144.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd144);
                    break;
                case EnumCmdNum.Cmd145_PowerOnoffResponse:
                    ID_145_POWER_OPE_RESPONSE cmd145 = new ID_145_POWER_OPE_RESPONSE();
                    cmd145.ReplyCode = 0;
                    infos = cmd145.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd145);
                    break;
                case EnumCmdNum.Cmd151_AvoidResponse:
                    ID_151_AVOID_RESPONSE cmd151 = new ID_151_AVOID_RESPONSE();
                    cmd151.ReplyCode = 0;
                    infos = cmd151.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd151);
                    break;
                case EnumCmdNum.Cmd152_AvoidCompleteReport:
                    ID_152_AVOID_COMPLETE_REPORT cmd152 = new ID_152_AVOID_COMPLETE_REPORT();
                    cmd152.CmpStatus = 0;
                    infos = cmd152.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd152);
                    break;
                case EnumCmdNum.Cmd171_RangeTeachResponse:
                    ID_171_RANGE_TEACHING_RESPONSE cmd171 = new ID_171_RANGE_TEACHING_RESPONSE();
                    cmd171.ReplyCode = 0;
                    infos = cmd171.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd171);
                    break;
                case EnumCmdNum.Cmd172_RangeTeachCompleteReport:
                    ID_172_RANGE_TEACHING_COMPLETE_REPORT cmd172 = new ID_172_RANGE_TEACHING_COMPLETE_REPORT();
                    cmd172.CompleteCode = 0;
                    infos = cmd172.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd172);
                    break;
                case EnumCmdNum.Cmd174_AddressTeachReport:
                    ID_174_ADDRESS_TEACH_REPORT cmd174 = new ID_174_ADDRESS_TEACH_REPORT();
                    cmd174.Addr = "Adr001";
                    infos = cmd174.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd174);
                    break;
                case EnumCmdNum.Cmd191_AlarmResetResponse:
                    ID_191_ALARM_RESET_RESPONSE cmd191 = new ID_191_ALARM_RESET_RESPONSE();
                    cmd191.ReplyCode = 0;
                    infos = cmd191.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd191);
                    break;
                case EnumCmdNum.Cmd194_AlarmReport:
                default:
                    ID_194_ALARM_REPORT cmd194 = new ID_194_ALARM_REPORT();
                    cmd194.ErrCode = "";
                    cmd194.ErrDescription = "";
                    cmd194.ErrStatus = ErrorStatus.ErrSet;
                    infos = cmd194.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd194);
                    break;
            }
        }

        private void SetDataGridViewFromInfos(PropertyInfo[] infos, object obj)
        {
            dataGridView1.Rows.Clear();

            foreach (var info in infos)
            {
                if (info.CanWrite)
                {
                    var name = info.Name;
                    var value = info.GetValue(obj);
                    string[] row = { name, value.ToString() };
                    dataGridView1.Rows.Add(row);
                }
            }

            foreach (var info in infos)
            {
                if (info.PropertyType == typeof(RepeatedField<string>))
                {
                    var name = info.Name;
                    var value = info.GetValue(obj);
                    string[] row = { name, value.ToString() };
                    dataGridView1.Rows.Add(row);
                }
            }
        }

        private void btnIsClientAgentNull_Click(object sender, EventArgs e)
        {
            if (middleAgent.IsClientAgentNull())
            {
                AppendCommLogMsg("ClientAgent is null.");
            }
            else
            {
                AppendCommLogMsg("ClientAgent is not null.");
            }
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            middleAgent.DisConnect();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                //middlerConfig.RemoteIp = txtRemoteIp.Text;
                //middlerConfig.RemotePort = int.Parse(txtRemotePort.Text);
                //SaveMiddlerConfigs();

                //middleAgent.ReConnect();
                middleAgent.Connect();

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            middleAgent.StopClientAgent();
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.SendToBack();
            this.Hide();
        }

        private void timerUI_Tick(object sender, EventArgs e)
        {           
            try
            {
                tbxCommLogMsg.Text = CommLogMsg;
                toolStripStatusLabel1.Text = ConnectionMsg;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }        

        private void LogException(string classMethodName,string exMsg)
        {
            MirleLogger.Instance.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }
    }
}
