using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Controller.Tools;
using System.Collections.Concurrent;

namespace Mirle.Agv.Controller
{
    [Serializable]
    public class AlarmHandler
    {
        #region Containers

        public List<Alarm> alarms = new List<Alarm>();
        public Dictionary<int, Alarm> allAlarms = new Dictionary<int, Alarm>();
        public ConcurrentDictionary<int, Alarm> dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();

        #endregion

        #region Events
        #endregion

        private AlarmConfig alarmConfig;
        private LoggerAgent loggerAgent = LoggerAgent.Instance;

        public AlarmHandler(AlarmConfig alarmConfig)
        {
            this.alarmConfig = alarmConfig;
            LoadAlarmFile();
        }

        private void LoadAlarmFile()
        {
            try
            {
                if (string.IsNullOrEmpty(alarmConfig.AlarmFileName))
                {
                    string className = GetType().Name;
                    string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    string classMethodName = className + ":" + methodName;
                    LogFormat logFormat = new LogFormat("Error", "1", classMethodName, "Device", "CarrierID", $"string.IsNullOrEmpty(alarmConfig.AlarmFileName)={string.IsNullOrEmpty(alarmConfig.AlarmFileName)}");
                    loggerAgent.LogMsg("Error", logFormat);

                    return;
                }

                string alarmFullPath = Path.Combine(Environment.CurrentDirectory, alarmConfig.AlarmFileName);
                Dictionary<string, int> dicAlarmIndexes = new Dictionary<string, int>();
                alarms.Clear();
                allAlarms.Clear();

                string[] allRows = File.ReadAllLines(alarmFullPath);
                if (allRows == null || allRows.Length < 2)
                {
                    string className = GetType().Name;
                    string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    string classMethodName = className + ":" + methodName;
                    LogFormat logFormat = new LogFormat("Error", "1", classMethodName, "Device", "CarrierID", "There are no alarms in file");
                    loggerAgent.LogMsg("Error", logFormat);

                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, AlarmText, PlcAddress, PlcBitNumber, Level, Description
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicAlarmIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    Alarm oneRow = new Alarm();
                    oneRow.Id = int.Parse(getThisRow[dicAlarmIndexes["Id"]]);
                    oneRow.AlarmText = getThisRow[dicAlarmIndexes["AlarmText"]];
                    oneRow.PlcAddress = getThisRow[dicAlarmIndexes["PlcAddress"]];
                    oneRow.PlcBitNumber = getThisRow[dicAlarmIndexes["PlcBitNumber"]];
                    oneRow.Level = int.Parse(getThisRow[dicAlarmIndexes["Level"]]);
                    oneRow.Description = getThisRow[dicAlarmIndexes["Description"]];

                    allAlarms.Add(oneRow.Id, oneRow);
                    alarms.Add(oneRow);
                }
            }
            catch (Exception ex)
            {
                string className = GetType().Name;
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string classMethodName = className + ":" + methodName;
                LogFormat logFormat = new LogFormat("Error", "1", classMethodName, "Device", "CarrierID", ex.StackTrace);
                loggerAgent.LogMsg("Error", logFormat);
            }
        }

        public Alarm GetAlarm(int id)
        {
            Alarm alarm;
            if (!allAlarms.ContainsKey(id))
            {
                alarm = new Alarm();
                alarm.Id = id;                
            }
            else
            {
                alarm = allAlarms[id].DeepClone();
            }

            return alarm;
        }

        public void SetAlarm(int id)
        {
            if (dicHappeningAlarms.ContainsKey(id))
            {
                //Already have this alarm in happening list
                return;
            }

            Alarm alarm = GetAlarm(id);
            DateTime timeStamp = DateTime.Now;
            alarm.SetTime = timeStamp;
            dicHappeningAlarms.TryAdd(id, alarm);
            loggerAgent.LogAlarmHistory(alarm);
        }

        public void ResetAlarm(int id)
        {
            if (!dicHappeningAlarms.ContainsKey(id))
            {
                return;
            }

            DateTime resetTime = DateTime.Now;
            dicHappeningAlarms.TryRemove(id, out Alarm alarm);
            alarm.ResetTime = resetTime;
            loggerAgent.LogAlarmHistory(alarm);          
        }

        public void ResetAllAlarms()
        {
            var tempHappeningAlarms = dicHappeningAlarms;
            dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
            DateTime resetTime = DateTime.Now;

            foreach (KeyValuePair<int, Alarm> item in tempHappeningAlarms)
            {
                Alarm alarm = item.Value;
                alarm.ResetTime = resetTime;
                loggerAgent.LogAlarmHistory(alarm);                
            }
        }
    }
}
