using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Control;

namespace Mirle.Agv.View
{
    public partial class MainForm : Form
    {
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);
        private MainFlowHandler mainFlowHandler;
        private CommToAgvcForm commToAgvcForm;

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            commToAgvcForm = new CommToAgvcForm(mainFlowHandler);
            InitializeComponent();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mainFlowHandler.Stop();
            ShutdownEvent.Set();
            PauseEvent.Set();

            Application.Exit();
            Environment.Exit(Environment.ExitCode);

        }

        private void 通訊ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (commToAgvcForm.IsDisposed)
            {
                commToAgvcForm = new CommToAgvcForm(mainFlowHandler);
            }
            commToAgvcForm.TopMost = true;
            commToAgvcForm.WindowState = FormWindowState.Normal;
            commToAgvcForm.Show();
            
        }
    }
}
