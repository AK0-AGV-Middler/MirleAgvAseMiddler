using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.View
{
    public partial class JogPitchAxis : UserControl
    {
        public JogPitchAxis(string axisName)
        {
            InitializeComponent();
            label_AxisName.Text = axisName;
        }

        public void Update(string position, bool disable, bool standStill)
        {
            tB_Position.Text = position;
            if (disable)
                pB_Disable.BackColor = System.Drawing.Color.DarkRed;
            else
                pB_Disable.BackColor = System.Drawing.Color.LightGray;

            if (standStill)
                pB_StandStill.BackColor = System.Drawing.Color.Green;
            else
                pB_StandStill.BackColor = System.Drawing.Color.LightGray;
        }
    }
}
