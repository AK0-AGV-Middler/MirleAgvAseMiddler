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
using Mirle.Agv.AseMiddler.Model;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class AseRobotControlForm : Form
    {
        public event EventHandler<AseRobotEventArgs> SendRobotCommand;
        public event EventHandler<AseChargeEventArgs> SendChargeCommand;
        public event EventHandler<bool> PauseAskBattery;
        public event EventHandler RefreshBatteryState;
        public event EventHandler RefreshRobotState;
        public event EventHandler<string> OnException;

        public string LCassetteId { get; set; } = "";
        public string RCassetteId { get; set; } = "";

        public string PspLogMsg { get; set; } = "";
        private MapInfo mapInfo;

        public string BatteryPercentage { get; set; } = "";
        public string BatteryAH { get; set; } = "";
        public string BatteryVoltage { get; set; } = "";
        public string BatteryTemperature { get; set; } = "";

        public AseRobotControlForm(MapInfo mapInfo)
        {
            InitializeComponent();
            InitialBoxPioDirection();
            InitialBoxChargeDirection();
            this.mapInfo = mapInfo;
        }

        private void InitialBoxChargeDirection()
        {
            foreach (var item in Enum.GetNames(typeof(EnumAddressDirection)))
            {
                boxChargeDirection.Items.Add(item);
            }
            boxChargeDirection.SelectedItem = EnumAddressDirection.Left.ToString();
        }

        private void InitialBoxPioDirection()
        {
            foreach (var item in Enum.GetNames(typeof(EnumAddressDirection)))
            {
                boxPioDirection.Items.Add(item);
            }
            boxPioDirection.SelectedItem = EnumAddressDirection.None.ToString();
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
            textBox1.Text = PspLogMsg;
            UpdateRobotState();
            UpdateSlotState();
            UpdateBatteryState();
            ucBatteryPercentage.TagValue = BatteryPercentage;
            ucBatteryVoltage.TagValue = BatteryVoltage;
            ucBatteryTemperature.TagValue = BatteryTemperature;
            ucBatteryCharging.TagValue = Vehicle.Instance.IsCharging ? "Yes" : "No";
            ucBatteryCharging.TagColor = Vehicle.Instance.IsCharging ? Color.LightGreen : Color.Pink;
        }

        private void UpdateBatteryState()
        {
            try
            {
                ucBatteryPercentage.TagValue = BatteryPercentage;
                ucBatteryVoltage.TagValue = BatteryVoltage;
                ucBatteryTemperature.TagValue = BatteryTemperature;
                ucBatteryCharging.TagValue = Vehicle.Instance.IsCharging ? "Yes" : "No";
                ucBatteryCharging.TagColor = Vehicle.Instance.IsCharging ? Color.LightGreen : Color.Pink;
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private void UpdateSlotState()
        {
            try
            {
                ucRobotSlotLState.TagValue = Vehicle.Instance.AseCarrierSlotL.CarrierSlotStatus.ToString();
                ucRobotSlotLId.TagValue = Vehicle.Instance.AseCarrierSlotL.CarrierId;
                ucRobotSlotRState.TagValue = Vehicle.Instance.AseCarrierSlotR.CarrierSlotStatus.ToString();
                ucRobotSlotRId.TagValue = Vehicle.Instance.AseCarrierSlotR.CarrierId;
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private void UpdateRobotState()
        {
            try
            {
                ucRobotRobotState.TagValue = Vehicle.Instance.AseRobotStatus.RobotState.ToString();
                ucRobotIsHome.TagValue = Vehicle.Instance.AseRobotStatus.IsHome.ToString();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
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

        private void btnSearchChargeAddress_Click(object sender, EventArgs e)
        {
            try
            {
                string addressId = txtChargeAddress.Text;
                if (mapInfo.addressMap.ContainsKey(addressId))
                {
                    boxChargeDirection.SelectedItem = mapInfo.addressMap[addressId].ChargeDirection.ToString();
                }
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private void btnStartCharge_Click(object sender, EventArgs e)
        {
            try
            {
                AseChargeEventArgs aseChargeEventArgs = new AseChargeEventArgs();
                aseChargeEventArgs.IsCharge = true;
                
                aseChargeEventArgs.ChargeDirection = (EnumAddressDirection)Enum.Parse(typeof(EnumAddressDirection), boxChargeDirection.Text);
                SendChargeCommand?.Invoke(this, aseChargeEventArgs);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private void btnRefreshBatterySate_Click(object sender, EventArgs e)
        {
            RefreshBatteryState?.Invoke(this, e);
        }

        private void btnRefreshRobotState_Click(object sender, EventArgs e)
        {
            RefreshRobotState?.Invoke(this, e);
        }

        private void btnStopCharge_Click(object sender, EventArgs e)
        {
            AseChargeEventArgs aseChargeEventArgs = new AseChargeEventArgs();
            aseChargeEventArgs.IsCharge = false;
            aseChargeEventArgs.ChargeDirection = EnumAddressDirection.None;
            SendChargeCommand?.Invoke(this, aseChargeEventArgs);
        }

        private void btnPauseAskBattery_Click(object sender, EventArgs e)
        {
            PauseAskBattery?.Invoke(this, true);
        }

        private void btnResumeAskBattery_Click(object sender, EventArgs e)
        {
            PauseAskBattery?.Invoke(this, false);
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

    public class AseChargeEventArgs : EventArgs
    {
        public bool IsCharge { get; set; }
        public EnumAddressDirection ChargeDirection { get; set; } = EnumAddressDirection.Left;
    }
}
