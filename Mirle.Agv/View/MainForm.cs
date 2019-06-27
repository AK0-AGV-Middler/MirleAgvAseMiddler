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
using Mirle.Agv.Model;

namespace Mirle.Agv.View
{
    public partial class MainForm : Form
    {
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);
        private MainFlowHandler mainFlowHandler;
        private ManualMoveCmdForm manualMoveCmdForm;
        private MiddlerForm middlerForm;
        private MapForm mapForm;
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen blackPen = new Pen(Color.Black, 1);
        private Graphics gra;
        private Panel panelleft;
        private MapInfo theMapInfo;
       

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            theMapInfo = MapInfo.Instance;
            manualMoveCmdForm = new ManualMoveCmdForm(mainFlowHandler);
            middlerForm = new MiddlerForm(mainFlowHandler);
            mapForm = new MapForm();
            gra = mapForm.CreateGraphics();

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
            if (middlerForm.IsDisposed)
            {
                middlerForm = new MiddlerForm(mainFlowHandler);
            }
            middlerForm.TopMost = true;
            middlerForm.WindowState = FormWindowState.Normal;
            middlerForm.Show();

        }

        private void btnRefreshMap_Click(object sender, EventArgs e)
        {
            //DrawSomething();
           // mapForm.DrawTheMap();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            panelleft = splitContainer1.Panel1;
            mapForm.TopLevel = false;
            panelleft.Controls.Add(mapForm);
            mapForm.Show();
        }

        private void 手動測試動令ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (manualMoveCmdForm.IsDisposed)
            {
                manualMoveCmdForm = new ManualMoveCmdForm(mainFlowHandler);
            }
            manualMoveCmdForm.TopMost = true;
            manualMoveCmdForm.WindowState = FormWindowState.Normal;
            manualMoveCmdForm.Show();
        }
    }
}
