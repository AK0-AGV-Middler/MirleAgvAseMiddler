using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Controller;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model.Configs;
using System.Threading;
using System.IO;
using System.Reflection;
using TcpIpClientSample;

namespace Mirle.Agv.View
{
    public partial class MiddlerForm : Form
    {
        private MainFlowHandler mainFlowHandler;
        private MiddleAgent middleAgent;
        private MiddlerConfigs middlerConfigs;

        public MiddlerForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
        }

        private void CommunicationForm_Load(object sender, EventArgs e)
        {            
            SetMiddlerAndConfigs();
            EventInital();

            ConfigToUI();
            if (middleAgent.ClientAgent.IsConnection)
            {
                toolStripStatusLabel1.Text = "Connect";
            }
        }

        private void SetMiddlerAndConfigs()
        {
            middlerConfigs = mainFlowHandler.GetMiddlerConfigs();
            middleAgent = mainFlowHandler.GetMiddleAgent();
        }

        private void ConfigToUI()
        {
            txtRemoteIp.Text = middlerConfigs.RemoteIp;
            txtRemotePort.Text = middlerConfigs.RemotePort.ToString();
        }

        private void EventInital()
        {
            middleAgent.OnConnected += ConnectionStatusToToolStrip;
            middleAgent.OnDisConnected += ConnectionStatusToToolStrip;
            middleAgent.OnCmdReceive += SendOrReceiveCmdToRichTextBox;
            middleAgent.OnCmdSend += SendOrReceiveCmdToRichTextBox;
        }

        private void SendOrReceiveCmdToRichTextBox(object sender, string e)
        {
            RichTextBoxAppendHead(richTextBox1, e);
        }

        private void ConnectionStatusToToolStrip(object sender, string e)
        {
            toolStripStatusLabel1.Text = e;
        }

        public delegate void RichTextBoxAppendHeadCallback(RichTextBox richTextBox, string msg);
        public void RichTextBoxAppendHead(RichTextBox richTextBox, string msg)
        {
            if (richTextBox.InvokeRequired)
            {
                RichTextBoxAppendHeadCallback mydel = new RichTextBoxAppendHeadCallback(RichTextBoxAppendHead);
                this.Invoke(mydel, new object[] { richTextBox, msg });
            }
            else
            {
                var timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff] ");

                richTextBox.Text = timeStamp + msg + Environment.NewLine + richTextBox.Text;

                int RichTextBoxMaxLines = middlerConfigs.RichTextBoxMaxLines;

                if (richTextBox.Lines.Count() > RichTextBoxMaxLines)
                {
                    string[] sNewLines = new string[RichTextBoxMaxLines];
                    Array.Copy(richTextBox.Lines, 0, sNewLines, 0, sNewLines.Length);
                    richTextBox.Lines = sNewLines;
                }
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
                msg += $"[{row.Cells[0].Value},{row.Cells[1].Value}]";
                if (!string.IsNullOrEmpty(row.Cells[0].Value.ToString()))
                {
                    pairs[row.Cells[0].Value.ToString()] = row.Cells[1].Value.ToString();
                }
            }
            //RichTextBoxAppendHead(richTextBox1, msg);

            middleAgent.SendMiddlerFormConfigCommand(cmdNum, pairs);
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
            var cmdType = (EnumCmdNums)selectCmdNum;

            switch (cmdType)
            {
                case EnumCmdNums.Cmd000_EmptyCommand:
                    ID_2_BASIC_INFO_VERSION_RESPONSE cmd002 = new ID_2_BASIC_INFO_VERSION_RESPONSE();
                    infos = cmd002.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd002);
                    break;
                case EnumCmdNums.Cmd31_TransferRequest:
                    ID_31_TRANS_REQUEST cmd31 = new ID_31_TRANS_REQUEST();
                    cmd31.CmdID = "Cmd001";
                    cmd31.CSTID = "Cst001";
                    cmd31.ActType = ActiveType.Move;
                    cmd31.DestinationAdr = "Adr001";
                    cmd31.LoadAdr = "Adr002";
                    infos = cmd31.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd31);
                    break;
                case EnumCmdNums.Cmd32_TransferCompleteResponse:
                    ID_32_TRANS_COMPLETE_RESPONSE cmd32 = new ID_32_TRANS_COMPLETE_RESPONSE();
                    cmd32.ReplyCode = 0;
                    infos = cmd32.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd32);
                    break;
                case EnumCmdNums.Cmd33_ControlZoneCancelRequest:
                    ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST cmd33 = new ID_33_CONTROL_ZONE_REPUEST_CANCEL_REQUEST();
                    cmd33.CancelSecID = "Sec001";
                    cmd33.ControlType = ControlType.Nothing;
                    infos = cmd33.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd33);
                    break;
                case EnumCmdNums.Cmd35_CarrierIdRenameRequest:
                    ID_35_CST_ID_RENAME_REQUEST cmd35 = new ID_35_CST_ID_RENAME_REQUEST();
                    cmd35.OLDCSTID = "Cst001";
                    cmd35.NEWCSTID = "Cst002";
                    infos = cmd35.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd35);
                    break;
                case EnumCmdNums.Cmd36_TransferEventResponse:
                    ID_36_TRANS_EVENT_RESPONSE cmd36 = new ID_36_TRANS_EVENT_RESPONSE();
                    cmd36.IsBlockPass = PassType.Pass;
                    cmd36.IsReserveSuccess = ReserveResult.Success;
                    cmd36.ReplyCode = 0;
                    infos = cmd36.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd36);
                    break;
                case EnumCmdNums.Cmd37_TransferCancelRequest:
                    ID_37_TRANS_CANCEL_REQUEST cmd37 = new ID_37_TRANS_CANCEL_REQUEST();
                    cmd37.CmdID = "Cmd001";
                    cmd37.ActType = CMDCancelType.CmdAbout;
                    infos = cmd37.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd37);
                    break;
                case EnumCmdNums.Cmd39_PauseRequest:
                    ID_39_PAUSE_REQUEST cmd39 = new ID_39_PAUSE_REQUEST();
                    cmd39.EventType = PauseEvent.Continue;
                    cmd39.PauseType = PauseType.None;
                    infos = cmd39.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd39);
                    break;
                case EnumCmdNums.Cmd41_ModeChange:
                    ID_41_MODE_CHANGE_REQ cmd41 = new ID_41_MODE_CHANGE_REQ();
                    cmd41.OperatingVHMode = OperatingVHMode.OperatingAuto;
                    infos = cmd41.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd41);
                    break;
                case EnumCmdNums.Cmd43_StatusRequest:
                    ID_43_STATUS_REQUEST cmd43 = new ID_43_STATUS_REQUEST();
                    infos = cmd43.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd43);
                    break;
                case EnumCmdNums.Cmd44_StatusRequest:
                    ID_44_STATUS_CHANGE_RESPONSE cmd44 = new ID_44_STATUS_CHANGE_RESPONSE();
                    cmd44.ReplyCode = 0;
                    infos = cmd44.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd44);
                    break;
                case EnumCmdNums.Cmd45_PowerOnoffRequest:
                    ID_45_POWER_OPE_REQ cmd45 = new ID_45_POWER_OPE_REQ();
                    cmd45.OperatingPowerMode = OperatingPowerMode.OperatingPowerOn;
                    infos = cmd45.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd45);
                    break;
                case EnumCmdNums.Cmd51_AvoidRequest:
                    ID_51_AVOID_REQUEST cmd51 = new ID_51_AVOID_REQUEST();
                    infos = cmd51.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd51);
                    break;
                case EnumCmdNums.Cmd52_AvoidCompleteResponse:
                    ID_52_AVOID_COMPLETE_RESPONSE cmd52 = new ID_52_AVOID_COMPLETE_RESPONSE();
                    cmd52.ReplyCode = 0;
                    infos = cmd52.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd52);
                    break;
                case EnumCmdNums.Cmd71_RangeTeachRequest:
                    ID_71_RANGE_TEACHING_REQUEST cmd71 = new ID_71_RANGE_TEACHING_REQUEST();
                    cmd71.FromAdr = "Adr001";
                    cmd71.ToAdr = "Adr002";
                    infos = cmd71.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd71);
                    break;
                case EnumCmdNums.Cmd72_RangeTeachCompleteResponse:
                    ID_72_RANGE_TEACHING_COMPLETE_RESPONSE cmd72 = new ID_72_RANGE_TEACHING_COMPLETE_RESPONSE();
                    cmd72.ReplyCode = 0;
                    infos = cmd72.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd72);
                    break;
                case EnumCmdNums.Cmd74_AddressTeachResponse:
                    ID_74_ADDRESS_TEACH_RESPONSE cmd74 = new ID_74_ADDRESS_TEACH_RESPONSE();
                    cmd74.ReplyCode = 0;
                    infos = cmd74.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd74);
                    break;
                case EnumCmdNums.Cmd91_AlarmResetRequest:
                    ID_91_ALARM_RESET_REQUEST cmd91 = new ID_91_ALARM_RESET_REQUEST();
                    infos = cmd91.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd91);
                    break;
                case EnumCmdNums.Cmd94_AlarmResponse:
                    ID_94_ALARM_RESPONSE cmd94 = new ID_94_ALARM_RESPONSE();
                    cmd94.ReplyCode = 0;
                    infos = cmd94.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd94);
                    break;
                case EnumCmdNums.Cmd131_TransferResponse:
                    ID_131_TRANS_RESPONSE cmd131 = new ID_131_TRANS_RESPONSE();
                    cmd131.CmdID = "Cmd001";
                    cmd131.ActType = ActiveType.Move;
                    cmd131.NgReason = "Empty";
                    cmd131.ReplyCode = 0;
                    infos = cmd131.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd131);
                    break;
                case EnumCmdNums.Cmd132_TransferCompleteReport:
                    ID_132_TRANS_COMPLETE_REPORT cmd132 = new ID_132_TRANS_COMPLETE_REPORT();
                    cmd132.CmdID = "Cmd001";
                    infos = cmd132.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd132);
                    break;
                case EnumCmdNums.Cmd133_ControlZoneCancelResponse:
                    ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE cmd133 = new ID_133_CONTROL_ZONE_REPUEST_CANCEL_RESPONSE();
                    cmd133.CancelSecID = "Sec001";
                    cmd133.ControlType = ControlType.Block;
                    cmd133.ReplyCode = 0;
                    infos = cmd133.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd133);
                    break;
                case EnumCmdNums.Cmd134_TransferEventReport:
                    ID_134_TRANS_EVENT_REP cmd134 = new ID_134_TRANS_EVENT_REP();
                    cmd134.CurrentAdrID = "Adr001";
                    cmd134.CurrentSecID = "Sec001";
                    cmd134.DrivingDirection = DriveDirction.DriveDirForward;
                    cmd134.EventType = EventType.AdrPass;
                    cmd134.SecDistance = 12345;
                    infos = cmd134.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd134);
                    break;
                case EnumCmdNums.Cmd135_CarrierIdRenameResponse:
                    ID_135_CST_ID_RENAME_RESPONSE cmd135 = new ID_135_CST_ID_RENAME_RESPONSE();
                    cmd135.ReplyCode = 0;
                    infos = cmd135.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd135);
                    break;
                case EnumCmdNums.Cmd136_TransferEventReport:
                    ID_136_TRANS_EVENT_REP cmd136 = new ID_136_TRANS_EVENT_REP();
                    cmd136.CSTID = "Cst001";
                    cmd136.CurrentAdrID = "Adr001";
                    cmd136.CurrentSecID = "Sec001";
                    cmd136.EventType = EventType.AdrPass;
                    infos = cmd136.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd136);
                    break;
                case EnumCmdNums.Cmd137_TransferCancelResponse:
                    ID_137_TRANS_CANCEL_RESPONSE cmd137 = new ID_137_TRANS_CANCEL_RESPONSE();
                    cmd137.ReplyCode = 0;
                    infos = cmd137.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd137);
                    break;
                case EnumCmdNums.Cmd139_PauseResponse:
                    ID_139_PAUSE_RESPONSE cmd139 = new ID_139_PAUSE_RESPONSE();
                    cmd139.ReplyCode = 0;
                    infos = cmd139.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd139);
                    break;
                case EnumCmdNums.Cmd141_ModeChangeResponse:
                    ID_141_MODE_CHANGE_RESPONSE cmd141 = new ID_141_MODE_CHANGE_RESPONSE();
                    cmd141.ReplyCode = 0;
                    infos = cmd141.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd141);
                    break;
                case EnumCmdNums.Cmd143_StatusResponse:
                    ID_143_STATUS_RESPONSE cmd143 = new ID_143_STATUS_RESPONSE();
                    cmd143.CmdID = "Cmd001";
                    infos = cmd143.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd143);
                    break;
                case EnumCmdNums.Cmd144_StatusReport:
                    ID_144_STATUS_CHANGE_REP cmd144 = new ID_144_STATUS_CHANGE_REP();
                    cmd144.CmdID = "Cmd001";
                    infos = cmd144.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd144);
                    break;
                case EnumCmdNums.Cmd145_PowerOnoffResponse:
                    ID_145_POWER_OPE_RESPONSE cmd145 = new ID_145_POWER_OPE_RESPONSE();
                    cmd145.ReplyCode = 0;
                    infos = cmd145.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd145);
                    break;
                case EnumCmdNums.Cmd151_AvoidResponse:
                    ID_151_AVOID_RESPONSE cmd151 = new ID_151_AVOID_RESPONSE();
                    cmd151.ReplyCode = 0;
                    infos = cmd151.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd151);
                    break;
                case EnumCmdNums.Cmd152_AvoidCompleteReport:
                    ID_152_AVOID_COMPLETE_REPORT cmd152 = new ID_152_AVOID_COMPLETE_REPORT();
                    cmd152.CmpStatus = 0;
                    infos = cmd152.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd152);
                    break;
                case EnumCmdNums.Cmd171_RangeTeachResponse:
                    ID_171_RANGE_TEACHING_RESPONSE cmd171 = new ID_171_RANGE_TEACHING_RESPONSE();
                    cmd171.ReplyCode = 0;
                    infos = cmd171.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd171);
                    break;
                case EnumCmdNums.Cmd172_RangeTeachCompleteReport:
                    ID_172_RANGE_TEACHING_COMPLETE_REPORT cmd172 = new ID_172_RANGE_TEACHING_COMPLETE_REPORT();
                    cmd172.CompleteCode = 0;
                    infos = cmd172.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd172);
                    break;
                case EnumCmdNums.Cmd174_AddressTeachReport:
                    ID_174_ADDRESS_TEACH_REPORT cmd174 = new ID_174_ADDRESS_TEACH_REPORT();
                    cmd174.Addr = "Adr001";
                    infos = cmd174.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd174);
                    break;
                case EnumCmdNums.Cmd191_AlarmResetResponse:
                    ID_191_ALARM_RESET_RESPONSE cmd191 = new ID_191_ALARM_RESET_RESPONSE();
                    cmd191.ReplyCode = 0;
                    infos = cmd191.GetType().GetProperties();
                    SetDataGridViewFromInfos(infos, cmd191);
                    break;
                case EnumCmdNums.Cmd194_AlarmReport:
                default:
                    ID_194_ALARM_REPORT cmd194 = new ID_194_ALARM_REPORT();
                    cmd194.ErrCode = "Empty";
                    cmd194.ErrDescription = "Empty";
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
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                middlerConfigs.RemoteIp = txtRemoteIp.Text;
                middlerConfigs.RemotePort = int.Parse(txtRemotePort.Text);
                SaveMiddlerConfigs();

                middleAgent.ReConnect();

            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private void SaveMiddlerConfigs()
        {
            string iniPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
            var configHandler = new ConfigHandler(iniPath);

            configHandler.SetString("Middler", "RemoteIp", middlerConfigs.RemoteIp);
            configHandler.SetString("Middler", "RemotePort", middlerConfigs.RemotePort.ToString());
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            middleAgent.DisConnect();
        }

       
    }
}
