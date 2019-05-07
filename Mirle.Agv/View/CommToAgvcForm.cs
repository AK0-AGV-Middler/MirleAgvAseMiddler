using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Control;
using System.Threading;

namespace Mirle.Agv.View
{
    public partial class CommToAgvcForm : Form
    {
        private MainFlowHandler mainFlowHandler;

        private Thread thdTestText;
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);

        public CommToAgvcForm(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            InitializeComponent();
            EventInital();
        }

        private void EventInital()
        {
            mainFlowHandler.OnMsgFromAgvcAddHandler(OnMsgFromAgvcHandler);
            mainFlowHandler.OnMsgFromVehicleAddHandler(OnMsgFromVehicleHandler);
            mainFlowHandler.OnMsgToAgvcAddHandler(OnMsgToAgvcHandler);
            mainFlowHandler.OnMsgToVehicleAddHandler(OnMsgToVehicleHandler);
        }

        private void OnMsgToVehicleHandler(object sender, string e)
        {
            TextBoxAppendHead(txbMsgToVehicle, e);
        }

        private void OnMsgToAgvcHandler(object sender, string e)
        {
            TextBoxAppendHead(txbMsgToAgvc, e);
        }

        private void OnMsgFromVehicleHandler(object sender, string e)
        {
            TextBoxAppendHead(txbMsgFromVehicle, e);
        }

        private void OnMsgFromAgvcHandler(object sender, string e)
        {
            TextBoxAppendHead(txbMsgFromAgvc, e);
        }

        public delegate void TextBoxAppendHeadCallback(TextBox textBox, string msg);
        private void TextBoxAppendHead(TextBox textBox, string msg)
        {
            if (textBox.InvokeRequired)
            {
                TextBoxAppendHeadCallback mydel = new TextBoxAppendHeadCallback(TextBoxAppendHead);
                this.Invoke(mydel, new object[] { textBox, msg });
            }
            else
            {
                var timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff]");

                textBox.Text = timeStamp + msg + Environment.NewLine + textBox.Text;
            }
        }

        private void btnCleanAll_Click(object sender, EventArgs e)
        {
            txbMsgFromAgvc.Clear();
            txbMsgFromVehicle.Clear();
            txbMsgToAgvc.Clear();
            txbMsgToVehicle.Clear();
        }

        private void btnTestStart_Click(object sender, EventArgs e)
        {
            PauseEvent.Set();
            ShutdownEvent.Reset();
            thdTestText = new Thread(new ThreadStart(TestText));
            thdTestText.IsBackground = true;
            thdTestText.Start();
        }

        private void TestText()
        {
            while (true)
            {
                PauseEvent.WaitOne(Timeout.Infinite);
                if (ShutdownEvent.WaitOne(0))
                {
                    break;
                }

                var timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff]");
                var thdId = Thread.CurrentThread.ManagedThreadId;
                var msg = timeStamp + $"ThdId[{thdId}]";
                TextBoxAppendHead(txbMsgFromAgvc, msg);
                TextBoxAppendHead(txbMsgFromVehicle, msg);
                TextBoxAppendHead(txbMsgToAgvc, msg);
                TextBoxAppendHead(txbMsgToVehicle, msg);

                SpinWait.SpinUntil(() => false, 1000);
            }

        }

        private void btnTestStop_Click(object sender, EventArgs e)
        {
            ShutdownEvent.Set();
            PauseEvent.Set();
            thdTestText.Join();
        }

        private void btnTestMsg_Click(object sender, EventArgs e)
        {
            //mainFlowHandler.ReconnectToAgvc();            
            mainFlowHandler.MiddlerTestMsg();
        }
    }
}
