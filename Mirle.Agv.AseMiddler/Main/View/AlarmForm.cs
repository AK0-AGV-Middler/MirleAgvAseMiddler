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
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Controller;

using System.Reflection;
using System.Threading;
using Mirle.Tools;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class AlarmForm : Form
    {
        private AlarmHandler alarmHandler;
        private MainFlowHandler mainFlowHandler;

        public AlarmForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
            alarmHandler = mainFlowHandler.alarmHandler;
        }

        //public void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        //{
        //    var msgForHappeningAlarms = $"[ID={alarm.Id}][Text={alarm.AlarmText}][{alarm.Level}]\r\n[SetTime={alarm.SetTime.ToString("HH-mm-ss.fff")}][Description={alarm.Description}]";
        //    AppendHappenedingAlarmsMsg(msgForHappeningAlarms);

        //    var msgForHistoryAlarms = $"[Id ={alarm.Id}][Text={alarm.AlarmText}][{alarm.Level}]\r\n[SetTime={alarm.SetTime.ToString("yyyy-MM-dd HH-mm")}]";
        //    AppendHistoryAlarmsMsg(msgForHistoryAlarms);
        //}

        //public void AlarmHandler_OnResetAllAlarmsEvent(object sender, string msg)
        //{
        //    btnAlarmReset.Enabled = false;
        //    AppendHistoryAlarmsMsg(msg);
        //    HappenedingAlarmsMsg = "";
        //    SpinWait.SpinUntil(() =>false, 500);
        //    btnAlarmReset.Enabled = true;
        //}

        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            btnAlarmReset.Enabled = false;
            mainFlowHandler.ResetAllAlarmsFromAgvm();
            Thread.Sleep(500);
            btnAlarmReset.Enabled = true;
        }

        //private void btnBuzzOff_Click(object sender, EventArgs e)
        //{
        //    mainFlowHandler.BuzzOff();
        //}

        private void btnTestSetAlarm_Click(object sender, EventArgs e)
        {
            try
            {
                //Test Set a non-empty alarm
                alarmHandler.SetAlarm(alarmHandler.allAlarms.First(x => x.Key != 0).Key);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.SendToBack();
            this.Hide();
        }

        //private void btnAlarmTest_Click(object sender, EventArgs e)
        //{
        //    var id = Convert.ToInt32(num1.Value);
        //    alarmHandler.SetAlarmFromAgvm(id);
        //}


        //private void AppendHappenedingAlarmsMsg(string msg)
        //{
        //    try
        //    {
        //        HappenedingAlarmsMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\r\n", msg, "\r\n", HappenedingAlarmsMsg);

        //        if (HappenedingAlarmsMsg.Length > 65535)
        //        {
        //            HappenedingAlarmsMsg = HappenedingAlarmsMsg.Substring(65535);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
        //    }
        //}

        //private void AppendHistoryAlarmsMsg(string msg)
        //{
        //    try
        //    {
        //        HistoryAlarmsMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\r\n", msg, "\r\n", HistoryAlarmsMsg);

        //        if (HistoryAlarmsMsg.Length > 65535)
        //        {
        //            HistoryAlarmsMsg = HistoryAlarmsMsg.Substring(65535);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
        //    }
        //}

        private void timeUpdateUI_Tick(object sender, EventArgs e)
        {
            tbxHappendingAlarms.Text = alarmHandler.AlarmLogMsg;
            tbxHistoryAlarms.Text = alarmHandler.AlarmHistoryLogMsg;
        }

        private void LogException(string source, string exMsg)
        {
            MirleLogger.Instance.Log(new LogFormat("Error", "5", source, "Device", "CarrierID", exMsg));

        }
    }
}
