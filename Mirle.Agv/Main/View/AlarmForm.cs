using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.IO;
using Mirle.Agv.Model;
using Mirle.Agv.Controller;
using Mirle.Agv.Controller.Tools;
using System.Reflection;

namespace Mirle.Agv.View
{
    public partial class AlarmForm : Form
    {
        private AlarmHandler alarmHandler;
        private MainFlowHandler mainFlowHandler;
        //private string historyAlarmsFilePath = Path.Combine(Environment.CurrentDirectory, "Log", "AlarmHistory", "AlarmHistory.log");

        public AlarmForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
            alarmHandler = mainFlowHandler.GetAlarmHandler();
            alarmHandler.OnResetAllAlarmsEvent += AlarmHandler_OnResetAllAlarmsEvent;
            alarmHandler.OnSetAlarmEvent += AlarmHandler_OnSetAlarmEvent;
            alarmHandler.OnPlcResetOneAlarmEvent += AlarmHandler_OnPlcResetOneAlarmEvent;
        }

        private void AlarmHandler_OnPlcResetOneAlarmEvent(object sender, Alarm alarm)
        {
            //var xx = rtbHappeningAlarms.Find($"{alarm.Id}");
        }

        private void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            var msgForHappeningAlarms = $"[ID={alarm.Id}][Text={alarm.AlarmText}][{alarm.Level}][{alarm.SetTime.ToString("HH/mm/ss.fff")}][Description={alarm.Description}]";
            RichTextBoxAppendHead(rtbHappeningAlarms, msgForHappeningAlarms);

            var msgForHistoryAlarms = $"[Id ={alarm.Id}][Text={alarm.AlarmText}][{alarm.Level}][{alarm.SetTime.ToString("yyyy/MM/dd_HH/mm")}]";
            RichTextBoxAppendHead(rtbHistoryAlarms, msgForHistoryAlarms);
        }

        private void AlarmHandler_OnResetAllAlarmsEvent(object sender, int count)
        {
            btnAlarmReset.Enabled = false;
            rtbHappeningAlarms.Clear();
            btnAlarmReset.Enabled = true;
        }

        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            btnAlarmReset.Enabled = false;
            mainFlowHandler.ResetAllarms();
            btnAlarmReset.Enabled = true;
        }

        private void btnBuzzOff_Click(object sender, EventArgs e)
        {
            mainFlowHandler.GetPlcAgent().WritePLCBuzzserStop();
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
                msg = msg + Environment.NewLine;
                richTextBox.Text = string.Concat(timeStamp, msg, richTextBox.Text);

                int RichTextBoxMaxLines = 10000;  // middlerConfig.RichTextBoxMaxLines;

                if (richTextBox.Lines.Count() > RichTextBoxMaxLines)
                {
                    string[] sNewLines = new string[RichTextBoxMaxLines];
                    Array.Copy(richTextBox.Lines, 0, sNewLines, 0, sNewLines.Length);
                    richTextBox.Lines = sNewLines;
                }
            }
        }

        private void btnTestSetAlarm_Click(object sender, EventArgs e)
        {
            try
            {
                //Test Set a non-empty alarm
                alarmHandler.SetAlarm(alarmHandler.allAlarms.First(x => x.Key != 0).Key);
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , ex.StackTrace));
            }
        }
    }
}
