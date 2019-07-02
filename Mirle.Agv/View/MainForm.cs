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
using Mirle.Agv.Model.TransferCmds;

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
        private Panel panelLeft;
        private Panel panelRightUp;
        private Panel panelRightDown;
        private MapInfo theMapInfo = MapInfo.Instance;

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitialForms();
            InitialPanels();
            InitialEvents();
            mapForm.Show();
        }

        private void InitialPanels()
        {
            panelLeft = splitContainer1.Panel1;
            panelLeft.Controls.Add(mapForm);

            panelRightUp = splitContainer2.Panel1;

            panelRightDown = splitContainer2.Panel2;
        }

        private void InitialEvents()
        {
            mainFlowHandler.OnAgvcTransferCommandCheckedEvent += MainFlowHandler_OnAgvcTransferCommandCheckedEvent;
        }

        private void MainFlowHandler_OnAgvcTransferCommandCheckedEvent(object sender, string msg)
        {
            //RichTextBoxAppendHead(richTextBox1, msg);
        }

        private void InitialForms()
        {
            manualMoveCmdForm = new ManualMoveCmdForm(mainFlowHandler);
            manualMoveCmdForm.TopMost = true;
            manualMoveCmdForm.WindowState = FormWindowState.Normal;

            middlerForm = new MiddlerForm(mainFlowHandler);
            middlerForm.TopMost = true;
            middlerForm.WindowState = FormWindowState.Normal;

            mapForm = new MapForm();
            mapForm.TopLevel = false;
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
            middlerForm.Show();

        }

        private void btnRefreshMap_Click(object sender, EventArgs e)
        {
            //DrawSomething();
            // mapForm.DrawTheMap();
        }


        private void 手動測試動令ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (manualMoveCmdForm.IsDisposed)
            {
                manualMoveCmdForm = new ManualMoveCmdForm(mainFlowHandler);
            }
            manualMoveCmdForm.Show();
        }

        public delegate void RichTextBoxAppendHeadCallback(RichTextBox richTextBox, string msg);
        public void RichTextBoxAppendHead(RichTextBox richTextBox, string msg)
        {
            if (richTextBox.InvokeRequired)
            {
                RichTextBoxAppendHeadCallback mydel = new RichTextBoxAppendHeadCallback(RichTextBoxAppendHead);
                this.Invoke(mydel, new object[] { richTextBox, msg });
            }
            else
            {
                var timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff] ");

                richTextBox.Text = timeStamp + msg + Environment.NewLine + richTextBox.Text;

                if (richTextBox.Lines.Count() > 25)
                {
                    string[] sNewLines = new string[25];
                    Array.Copy(richTextBox.Lines, 0, sNewLines, 0, sNewLines.Length);
                    richTextBox.Lines = sNewLines;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mainFlowHandler.FakeCmdTest();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MapSection testSection1 = theMapInfo.dicMapSections["sec001"];
            Panel panel = new Panel();
            panel.BackColor = Color.Green;
            mapForm.AddSectionPanel(testSection1, panel);
        }
    }
}
