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

namespace Mirle.Agv.AseMiddler.View
{
    public partial class AseMoveControlForm : Form
    {
        public event EventHandler<AseMoveEventArgs> SendMove;
        public event EventHandler<string> OnException;
        public event EventHandler<bool> PauseOrResumeAskPosition;
        private MapInfo mapInfo;
        public string PspLogMsg { get; set; } = "";        

        public AseMoveControlForm(MapInfo mapInfo)
        {
            InitializeComponent();
            InitialBoxAddressDirection();
            InitialBoxIsEnd();
            this.mapInfo = mapInfo;
        }

        private void InitialBoxIsEnd()
        {
            boxIsEnd.DataSource = Enum.GetValues(typeof(EnumAseMoveCommandIsEnd));
            boxIsEnd.SelectedIndex = 0;
        }

        private void InitialBoxAddressDirection()
        {
            boxAddressDirection.DataSource = Enum.GetValues(typeof(EnumAddressDirection));
            boxAddressDirection.SelectedIndex = 0;
            //boxAddressDirection.SelectedItem = EnumAddressDirection.None;
        }

        private void btnSendMove_Click(object sender, EventArgs e)
        {
            try
            {
                AseMoveEventArgs aseMoveEventArgs = GetAseMoveEventArgsFromForm();
                SendMove?.Invoke(this, aseMoveEventArgs);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private AseMoveEventArgs GetAseMoveEventArgsFromForm()
        {
            AseMoveEventArgs aseMoveEventArgs = new AseMoveEventArgs();
            aseMoveEventArgs.AddressDirection = (EnumAddressDirection)boxAddressDirection.SelectedItem;
            aseMoveEventArgs.MapPosition = new MapPosition(Convert.ToDouble(numMovePositionX.Value), Convert.ToDouble(numMovePositionY.Value));
            aseMoveEventArgs.HeadAngle = Convert.ToInt32(numHeadAngle.Value);
            aseMoveEventArgs.Speed = Convert.ToInt32(numMoveSpeed.Value);
            aseMoveEventArgs.isEnd = (EnumAseMoveCommandIsEnd)boxIsEnd.SelectedItem;

            return aseMoveEventArgs;
        }

        private void btnSearchMapAddress_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtMapAddress.Text.Trim())) return;

                string addressId = txtMapAddress.Text.Trim();

                if (mapInfo.addressMap.ContainsKey(addressId))
                {
                    MapAddress mapAddress = mapInfo.addressMap[addressId];
                    UpdateMapAddressUserControls(mapAddress);
                }
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.StackTrace);
            }
        }

        private void UpdateMapAddressUserControls(MapAddress mapAddress)
        {
            boxIsEnd.SelectedIndex = (int)EnumAseMoveCommandIsEnd.Begin;
            numMovePositionX.Value = Convert.ToDecimal(mapAddress.Position.X);
            numMovePositionY.Value = Convert.ToDecimal(mapAddress.Position.Y);
            numHeadAngle.Value = Convert.ToDecimal((int)mapAddress.VehicleHeadAngle);
            boxAddressDirection.SelectedIndex = (int)mapAddress.TransferPortDirection;
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox1.Text = PspLogMsg;
        }

        private void btnPauseAskPosition_Click(object sender, EventArgs e)
        {
            PauseOrResumeAskPosition?.Invoke(this, true);
        }

        private void btnResumeAskPosition_Click(object sender, EventArgs e)
        {
            PauseOrResumeAskPosition?.Invoke(this, false);
        }
    }

    public class AseMoveEventArgs : EventArgs
    {
        public EnumAseMoveCommandIsEnd isEnd { get; set; }
        public EnumAddressDirection AddressDirection { get; set; }
        public MapPosition MapPosition { get; set; } = new MapPosition();
        public int HeadAngle { get; set; }
        public int Speed { get; set; }
    }
}
