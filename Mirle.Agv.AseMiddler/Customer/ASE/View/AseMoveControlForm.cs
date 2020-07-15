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
        public event EventHandler RefreshMoveStatusAndPosition;
        private MapInfo mapInfo;
        public string PspLogMsg { get; set; } = "";        

        public AseMoveControlForm(MapInfo mapInfo)
        {
            InitializeComponent();
            InitialBoxAddressDirection();
            InitialBoxIsEnd();
            InitialBoxKeepOrGo();
            this.mapInfo = mapInfo;
        }

        private void InitialBoxKeepOrGo()
        {
            boxKeepOrGo.DataSource = Enum.GetValues(typeof(EnumIsExecute));
            boxKeepOrGo.SelectedIndex = (int)EnumIsExecute.Keep;
        }

        private void InitialBoxIsEnd()
        {
            boxIsEnd.DataSource = Enum.GetValues(typeof(EnumAseMoveCommandIsEnd));
            boxIsEnd.SelectedIndex = (int)EnumAseMoveCommandIsEnd.None;
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
                OnException?.Invoke(this, ex.Message);
            }
        }

        private AseMoveEventArgs GetAseMoveEventArgsFromForm()
        {
            AseMoveEventArgs aseMoveEventArgs = new AseMoveEventArgs();
            aseMoveEventArgs.AddressDirection = (EnumAddressDirection)boxAddressDirection.SelectedItem;
            aseMoveEventArgs.MapPosition = new MapPosition(Convert.ToDouble(numMovePositionX.Value), Convert.ToDouble(numMovePositionY.Value));
            aseMoveEventArgs.HeadAngle = Convert.ToInt32(numHeadAngle.Value);
            aseMoveEventArgs.Speed = Convert.ToInt32(numMoveSpeed.Value);
            aseMoveEventArgs.IsEnd = (EnumAseMoveCommandIsEnd)boxIsEnd.SelectedItem;
            aseMoveEventArgs.KeepOrGo = (EnumIsExecute)boxKeepOrGo.SelectedItem;

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
                OnException?.Invoke(this, ex.Message);
            }
        }

        private void UpdateMapAddressUserControls(MapAddress mapAddress)
        {
            boxIsEnd.SelectedIndex = (int)EnumAseMoveCommandIsEnd.None;
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
                OnException?.Invoke(this, ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox1.Text = PspLogMsg;
            UpdateMoveState();
        }

        private void UpdateMoveState()
        {
            try
            {
                var theVehicle = Vehicle.Instance;
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);

                var lastPos = aseMoveStatus.LastMapPosition;
                string lastPosX = lastPos.X.ToString("F2");
                ucMovePositionX.TagValue = lastPosX;
                string lastPosY = lastPos.Y.ToString("F2");
                ucMovePositionY.TagValue = lastPosY;

                var lastAddress = aseMoveStatus.LastAddress;
                ucMoveLastAddress.TagValue = lastAddress.Id;

                var lastSection = aseMoveStatus.LastSection;
                ucMoveLastSection.TagValue = lastSection.Id;

                ucMoveIsMoveEnd.TagValue = aseMoveStatus.IsMoveEnd.ToString();

                ucMoveMoveState.TagValue = aseMoveStatus.AseMoveState.ToString();

            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex.Message);
            }

        }

        private void btnPauseAskPosition_Click(object sender, EventArgs e)
        {
            PauseOrResumeAskPosition?.Invoke(this, true);
        }

        private void btnResumeAskPosition_Click(object sender, EventArgs e)
        {
            PauseOrResumeAskPosition?.Invoke(this, false);
        }

        private void btnRefreshPosition_Click(object sender, EventArgs e)
        {
            RefreshMoveStatusAndPosition?.Invoke(this, new EventArgs());
        }
    }

    public class AseMoveEventArgs : EventArgs
    {
        public EnumAseMoveCommandIsEnd IsEnd { get; set; }
        public EnumAddressDirection AddressDirection { get; set; }
        public MapPosition MapPosition { get; set; } = new MapPosition();
        public int HeadAngle { get; set; }
        public int Speed { get; set; }
        public EnumIsExecute KeepOrGo { get; set; } =  EnumIsExecute.Keep;
    }
}
