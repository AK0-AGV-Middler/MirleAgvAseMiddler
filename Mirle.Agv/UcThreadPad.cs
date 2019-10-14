using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Mirle.Agv
{
    public partial class UcThreadPad : UserControl
    {       
        public UcThreadPad()
        {
            InitializeComponent();
        }

        public delegate void DelSetupControlText(string msg);
        public void SetupTitleText(string txt)
        {
            if (this.InvokeRequired)
            {
                DelSetupControlText del = new DelSetupControlText(SetupTitleText);
                del(txt);
            }
            else
            {
                gbThreadPad.Text = txt;
            }
        }

        public void SetupStatusText(string txt)
        {
            if (this.InvokeRequired)
            {
                DelSetupControlText del = new DelSetupControlText(SetupStatusText);
                del(txt);
            }
            else
            {
                txtThreadStatus.Text = txt;
            }
        }

        public delegate void DelSetupStatusColor(Color color);
        public void SetupStatusColor(Color color)
        {
            if (this.InvokeRequired)
            {
                DelSetupStatusColor del = new DelSetupStatusColor(SetupStatusColor);
                del(color);
            }
            else
            {
                txtThreadStatus.BackColor = color;
            }
        }
    }
}
