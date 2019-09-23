using Mirle.Agv.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.View
{
    public partial class SimulateSettingAGVAngleForm : Form
    {
        MoveControlHandler moveControl;

        public SimulateSettingAGVAngleForm(MoveControlHandler moveControl)
        {
            this.moveControl = moveControl;
            InitializeComponent();
            cB_AGVAngle.SelectedIndex = 0;
        }

        private void button_SettingAGVAngle_Click(object sender, EventArgs e)
        {
            int angle = Int16.Parse((string)cB_AGVAngle.SelectedItem);
            moveControl.location.Real.AGVAngle = angle;
            this.Close();
        }
    }
}
