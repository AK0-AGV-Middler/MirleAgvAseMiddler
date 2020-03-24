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

namespace Mirle.Agv.AseMiddler.View
{
    public partial class AseMoveControlForm : Form
    {
        public event EventHandler<AseMoveEventArgs> SendMove;
        public event EventHandler<string> OnException;
        private MapInfo mapInfo;

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
