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
using Mirle.Agv.AseMiddler.Controller;
using System.IO.MemoryMappedFiles;
using System.IO;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Tools;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class InitialForm : Form
    {
        private Thread thdInitial;
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);
        private InitialConfig initialConfig;

        private MainFlowHandler mainFlowHandler;
        private MainForm mainForm;

        private string mmfPath = Path.Combine(Environment.CurrentDirectory, "MMF.txt");
        private string fileShortName = "Mirle.Agv";

        public InitialForm()
        {
            InitializeComponent();
            MakeSureMmfTextFileExist();
            initialConfig = new XmlHandler().ReadXml<InitialConfig>(@"D:\AgvConfigs\Initial.xml");
            mainFlowHandler = new MainFlowHandler();
            mainFlowHandler.OnComponentIntialDoneEvent += MainFlowHandler_OnComponentIntialDoneEvent;
        }

        private void MainFlowHandler_OnComponentIntialDoneEvent(object sender, InitialEventArgs e)
        {
            if (e.IsOk)
            {
                var timeStamp = DateTime.Now.ToString("[yyyy-MM-dd HH-mm-ss]");
                var msg = timeStamp + e.ItemName + "初始化完成\n";
                ListBoxAppend(lst_StartUpMsg, msg);
                if (e.ItemName == "全部")
                {
                    WriteMmf("OK");
                    SpinWait.SpinUntil(() => false, initialConfig.StartOkShowMs);
                    GoNextForm();
                }
            }
            else
            {
                var timeStamp = DateTime.Now.ToString("[yyyy-MM-dd HH-mm-ss]");
                var msg = timeStamp + e.ItemName + "初始化失敗\n";
                ListBoxAppend(lst_StartUpMsg, msg);
                WriteMmf("NG");

                SpinWait.SpinUntil(() => false, initialConfig.StartNgCloseSec * 1000);
                ThisClose();
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

        private void ThisClose()
        {
            ShutdownEvent.Set();
            PauseEvent.Set();

            Application.Exit();
            Environment.Exit(Environment.ExitCode);
        }

        private void cmd_Close_Click(object sender, EventArgs e)
        {
            ThisClose();
        }

        private void InitialForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ThisClose();
        }

        private void WriteMmf(string msg)
        {
            try
            {
                byte[] msgbytes = Encoding.UTF8.GetBytes(msg.Trim());

                using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(mmfPath, FileMode.Create, "SAVE", msgbytes.Length))
                {
                    Mutex mutex;
                    if (!Mutex.TryOpenExisting(fileShortName, out mutex))
                    {
                        mutex = new Mutex(true, fileShortName);

                    }
                    mutex.WaitOne();

                    using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                    {
                        if (stream.CanWrite)
                        {
                            stream.Write(msgbytes, 0, msgbytes.Length);
                        }
                    }
                    mutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void MakeSureMmfTextFileExist()
        {
            try
            {
                if (!File.Exists(mmfPath))
                {
                    File.CreateText(mmfPath);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
