using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Mirle.Agv.Controller;

namespace Mirle.Agv.View
{
    public partial class InitialForm : Form
    {
        private Thread thdInitial;
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);

        private MainFlowHandler mainFlowHandler;
        private MainForm mainForm;

        public InitialForm()
        {
            InitializeComponent();
            mainFlowHandler = new MainFlowHandler();
            mainFlowHandler.OnComponentIntialDoneEvent += MainFlowHandler_OnComponentIntialDoneEvent;
        }

        private void MainFlowHandler_OnComponentIntialDoneEvent(object sender, InitialEventArgs e)
        {
            if (e.IsOk)
            {
                var timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss]");
                var msg = timeStamp + e.ItemName + "初始化完成\n";                
                ListBoxAppend(lst_StartUpMsg, msg);
                if (e.ItemName=="全部")
                {
                    SpinWait.SpinUntil(() => false, 1000);
                    GoNextForm();
                }
            }
            else
            {
                var timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss]");
                var msg = timeStamp + e.ItemName + "初始化失敗\n";
                ListBoxAppend(lst_StartUpMsg, msg);
            }
           
        }

        private void GoNextForm()
        {
            if (InvokeRequired)
            {
                Action del = new Action(GoNextForm);
                Invoke(del);
            }
            else
            {
                this.Hide();
                mainForm = new MainForm(mainFlowHandler);
                mainForm.Show();
            }
        }

        public delegate void ListBoxAppendCallback(ListBox listBox, string msg);
        private void ListBoxAppend(ListBox listBox, string msg)
        {
            if (listBox.InvokeRequired)
            {
                ListBoxAppendCallback mydel = new ListBoxAppendCallback(ListBoxAppend);
                this.Invoke(mydel, new object[] { listBox, msg });
            }
            else
            {
                listBox.Items.Add(msg);
            }
        }

        private void InitialForm_Shown(object sender, EventArgs e)
        {
            thdInitial = new Thread(new ThreadStart(ForInitial));
            thdInitial.IsBackground = true;
            thdInitial.Start();
        }

        private void ForInitial()
        {
            SpinWait.SpinUntil(() => false, 10);
            mainFlowHandler.InitialMainFlowHandler();
        }

        private void cmd_Close_Click(object sender, EventArgs e)
        {
            ShutdownEvent.Set();
            PauseEvent.Set();

            Application.Exit();
            Environment.Exit(Environment.ExitCode);
        }

        private void InitialForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShutdownEvent.Set();
            PauseEvent.Set();

            Application.Exit();
            Environment.Exit(Environment.ExitCode);
        }
    }
}
