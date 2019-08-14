using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv
{
    public partial class JogPitchAxis : UserControl
    {
        public JogPitchAxis(string axisName)
        {
            InitializeComponent();
            label_AxisName.Text = axisName;
        }

        public void Update(string position, bool disable, bool standStill, bool error, bool link)
        {
            tB_Position.Text = position;

            pB_Disable.BackColor = disable ? System.Drawing.Color.DarkRed : System.Drawing.Color.LightGray;
            pB_StandStill.BackColor = standStill ? System.Drawing.Color.Green : System.Drawing.Color.LightGray;
            pB_Error.BackColor = error ? System.Drawing.Color.DarkRed : System.Drawing.Color.LightGray;
            pB_Link.BackColor = link ? System.Drawing.Color.Green : System.Drawing.Color.LightGray;
        }
    }
}
