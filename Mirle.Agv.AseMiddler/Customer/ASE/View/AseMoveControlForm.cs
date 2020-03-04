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

        public AseMoveControlForm()
        {
            InitializeComponent();
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
            aseMoveEventArgs.IsEnd = cbIsEnd.Checked;
            aseMoveEventArgs.MapPosition = new MapPosition(Convert.ToDouble(numMovePositionX.Value), Convert.ToDouble(numMovePositionY.Value));
            aseMoveEventArgs.HeadAngle = Convert.ToInt32(numHeadAngle.Value);
            aseMoveEventArgs.Speed = Convert.ToInt32(numMoveSpeed.Value);

            return aseMoveEventArgs;
        }
    }

    public class AseMoveEventArgs : EventArgs
    {
        public bool IsEnd { get; set; }
        public MapPosition MapPosition { get; set; } = new MapPosition();
        public int HeadAngle { get; set; }
        public int Speed { get; set; }
    }
}
