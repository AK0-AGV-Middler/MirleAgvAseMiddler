using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.AseMiddler.Model.TransferSteps;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class AseRobotControlForm : Form
    {
        public event EventHandler<AseRobotEventArgs> SendRobotCommand;
        public event EventHandler<string> OnException;

        public string LCassetteId { get; set; } = "";
        public string RCassetteId { get; set; } = "";

        public string PspLogMsg { get; set; } = "";

        public AseRobotControlForm()
        {
            InitializeComponent();
            InitialBoxPioDirection();
        }

        private void InitialBoxPioDirection()
        {
            foreach (var item in Enum.GetNames(typeof(EnumAddressDirection)))
            {
                boxPioDirection.Items.Add(item);
            }
        }

        private void btnSendRobot_Click(object sender, EventArgs e)
        {
            try
            {
                AseRobotEventArgs aseRobotEventArgs = GetRobotCommandFromForm();
                SendRobotCommand?.Invoke(this, aseRobotEventArgs);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private AseRobotEventArgs GetRobotCommandFromForm()
        {
            AseRobotEventArgs aseRobotEventArgs = new AseRobotEventArgs();
            aseRobotEventArgs.IsLoad = cbIsLoad.Checked;
            aseRobotEventArgs.PioDirection = (EnumAddressDirection)Enum.Parse(typeof(EnumAddressDirection), boxPioDirection.Text);
            aseRobotEventArgs.FromPort = txtFromPort.Text;
            aseRobotEventArgs.ToPort = txtToPort.Text;
            aseRobotEventArgs.GateType = txtGateType.Text;
            aseRobotEventArgs.PortNumber = txtPortNumber.Text;
            return aseRobotEventArgs;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            txtLCstId.Text = LCassetteId;
            txtRCstId.Text = RCassetteId;
            textBox1.Text = PspLogMsg;
        }

        public void AsePackage_AllPspLog(object sender, string e)
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
    }

    public class AseRobotEventArgs : EventArgs
    {
        public bool IsLoad { get; set; }
        public EnumAddressDirection PioDirection { get; set; } = EnumAddressDirection.None;
        public string FromPort { get; set; } = "";
        public string ToPort { get; set; } = "";
        public string GateType { get; set; } = "0";
        public string PortNumber { get; set; } = "1";
    }
}
