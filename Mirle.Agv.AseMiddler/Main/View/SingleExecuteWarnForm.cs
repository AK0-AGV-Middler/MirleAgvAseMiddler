using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class SingleExecuteWarnForm : Form
    {
        public SingleExecuteWarnForm()
        {
            InitializeComponent();
        }

        private void cmd_Close_Click(object sender, EventArgs e)
        {
            ThisClose();
        }

        private void SingleExecuteWarnForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ThisClose();
        }

        private void ThisClose()
        {
            Application.Exit();
            Environment.Exit(Environment.ExitCode);
        }     
    }
}
