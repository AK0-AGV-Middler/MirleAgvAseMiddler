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
        public string WarningMsg { get; set; } = "No Warning";

        public WarningForm()
        {
            InitializeComponent();
        }

        private void WarningForm_Shown(object sender, EventArgs e)
        {
            string timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff] ");
            label1.Text = timeStamp + WarningMsg;
        }
    }
}
