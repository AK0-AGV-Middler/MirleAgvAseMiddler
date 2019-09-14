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
    public partial class WarningForm : Form
    {
        private string warningMsg = "No Warning";
        public string WarningMsg
        {
            get { return warningMsg; }
            set
            {
                string timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff]");
                warningMsg = value;
                textBox1.Text = timeStamp + Environment.NewLine + warningMsg;
            }
        }

        public WarningForm()
        {
            InitializeComponent();
        }

        private void WarningForm_Shown(object sender, EventArgs e)
        {
            //string timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff] ");
            //label1.Text = timeStamp + WarningMsg;
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.SendToBack();
        }
    }
}
