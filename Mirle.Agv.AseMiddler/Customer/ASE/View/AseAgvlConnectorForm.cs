using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Controller;
using PSDriver.PSDriver;
using Mirle.Tools;
using System.Reflection;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class AseAgvlConnectorForm : Form
    {
        public string PspLogMsg { get; set; } = "";
        private AsePackage asePackage;

        public event EventHandler<string> OnException;
        private MirleLogger mirleLogger = MirleLogger.Instance;

        public AseAgvlConnectorForm(AsePackage asePackage)
        {
            InitializeComponent();
            this.asePackage = asePackage;           
            FitPspMessageList();
            InitialSingleMsgType();
            asePackage.AllPspLog += AsePackage_AllPspLog;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox1.Text = PspLogMsg;
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.SendToBack();
            this.Hide();
        }

        private void FitPspMessageList()
        {
            try
            {
                if (asePackage.psMessageMap.Count > 0)
                {
                    foreach (var item in asePackage.psMessageMap)
                    {
                        string pspMessageListItem = string.Concat(item.Key, ",", item.Value.Description);
                        cbPspMessageList.Items.Add(pspMessageListItem);
                    }
                }
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private void InitialSingleMsgType()
        {
            cbPsMessageType.DataSource = Enum.GetValues(typeof(PsMessageType));
            cbPsMessageType.SelectedItem = PsMessageType.P;
        }

        private void btnSingleMessageSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbPsMessageType.Text.Equals("P"))
                {
                    string typeAndNumber = numPsMessageNumber.Value.ToString("00");
                    asePackage.PrimarySend(typeAndNumber, txtPsMessageText.Text);
                }              
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private void AsePackage_AllPspLog(object sender, string e)
        {
            AppendPspLogMsg(e);
        }

        private void AppendPspLogMsg(string msg)
        {
            try
            {
                PspLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", msg, "\r\n", PspLogMsg);

                if (PspLogMsg.Length > 65535)
                {
                    PspLogMsg = PspLogMsg.Substring(65535);
                }
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private void cbPspMessageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedItem = cbPspMessageList.SelectedItem.ToString().Split(',')[0];
                PSMessageXClass pspMessage = asePackage.psMessageMap[selectedItem];
                cbPsMessageType.SelectedItem = pspMessage.Type == "P" ? PsMessageType.P : PsMessageType.S;
                numPsMessageNumber.Value = decimal.Parse(pspMessage.Number);
                txtPsMessageText.Text = pspMessage.PSMessage;
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private void LogPspLog(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("PsWrapper", "5", classMethodName, "Device", "CarrierID", msg));
        }

    }
}
