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
        private MapForm mapForm;
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Graphics gra;
        private Panel panelleft;

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            commToAgvcForm = new CommToAgvcForm(mainFlowHandler);
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
            if (commToAgvcForm.IsDisposed)
            {
                commToAgvcForm = new CommToAgvcForm(mainFlowHandler);
            }
            commToAgvcForm.TopMost = true;
            commToAgvcForm.WindowState = FormWindowState.Normal;
            commToAgvcForm.Show();
            
        }

        private void btnRefreshMap_Click(object sender, EventArgs e)
        {
            DrawSomething();
        }

        private void DrawSomething()
        {
            Rectangle rectangle = new Rectangle(100, 100, 200, 200);
            gra.DrawArc(bluePen, rectangle, 0, -90);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            panelleft = splitContainer1.Panel1;
            mapForm.TopLevel = false;
            panelleft.Controls.Add(mapForm);
            mapForm.Show();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            DrawSomething();
        }
    }
}
