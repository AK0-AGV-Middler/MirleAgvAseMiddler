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

namespace Mirle.Agv.View
{
    public partial class AlarmForm : Form
    {
        private AlarmHandler alarmHandler;
        private string historyAlarmsFilePath = Path.Combine(Environment.CurrentDirectory, "Log", "AlarmHistory", "AlarmHistory.log");

        public AlarmForm(AlarmHandler alarmHandler)
        {
            InitializeComponent();
            this.alarmHandler = alarmHandler;
            alarmHandler.SetAlarm(12345);
            RefreshHappeningAlarms();
        }

        private void RefreshHappeningAlarms()
        {
            listHappeningAlarms.Items.Clear();
            var happeningAlarms = alarmHandler.happeningAlarms.ToList();
            for (int i = 0; i < happeningAlarms.Count; i++)
            {
                Alarm alarm = happeningAlarms[i];
                string txtAlarm = $"[{alarm.SetTime.ToString("yyyy/MM/dd_HH/mm/ss.fff")}] [{alarm.Id}] [{alarm.AlarmText}] [{alarm.Level}] [{alarm.Description}]";
                listHappeningAlarms.Items.Add(txtAlarm);
            }
        }

        private void btnResetSelectAlarm_Click(object sender, EventArgs e)
        {
            if (listHappeningAlarms.Items.Count < 1)
            {
                return;
            }

            if (listHappeningAlarms.SelectedIndex < 0)
            {
                listHappeningAlarms.SelectedIndex = 0;
            }

            int selectedAlarmId = GetSelectedAlarmId(listHappeningAlarms.SelectedItem);

            alarmHandler.ResetAlarm(selectedAlarmId);
        }

        private int GetSelectedAlarmId(object selectedItem)
        {
            string textSelectedAlarm = (string)selectedItem;
            return int.Parse(textSelectedAlarm.Split(' ')[1].Trim(new char[] { '[', ']' }));
        }

        private void btnResetIdAlarm_Click(object sender, EventArgs e)
        {
            int selectedAlarmId = (int)numSelectAlarmId.Value;
            alarmHandler.ResetAlarm(selectedAlarmId);
        }

        private void listHappeningAlarms_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedAlarmId = GetSelectedAlarmId(listHappeningAlarms.SelectedItem);

            numSelectAlarmId.Value = selectedAlarmId;
        }

        private void btnResetAllAlarms_Click(object sender, EventArgs e)
        {
            alarmHandler.ResetAllAlarms();
        }

        private void timerRefreshHappenAlarm_Tick(object sender, EventArgs e)
        {
            RefreshHappeningAlarms();
        }

        private void btnSetTestAlarm_Click(object sender, EventArgs e)
        {
            int selectedAlarmId = (int)numSelectAlarmId.Value;
            alarmHandler.SetAlarm(selectedAlarmId);
        }

        private void timerRefreshHistoryAlarm_Tick(object sender, EventArgs e)
        {
          //TODO: Show historyAlarms in form
        }
    }
}
