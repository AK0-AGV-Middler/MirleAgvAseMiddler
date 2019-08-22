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
using System.Reflection;

namespace Mirle.Agv.Controller
{
    [Serializable]
    public class AlarmHandler
    {
        #region Containers

        public List<Alarm> alarms = new List<Alarm>();
        public Dictionary<int, Alarm> allAlarms = new Dictionary<int, Alarm>();
        public ConcurrentDictionary<int, Alarm> dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
        public ConcurrentStack<Alarm> stkHappeningAlarms = new ConcurrentStack<Alarm>();
        public event EventHandler<string> OnMessageShowEvent;

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
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , $"string.IsNullOrEmpty(alarmConfig.AlarmFileName)={string.IsNullOrEmpty(alarmConfig.AlarmFileName)}"));
                    return;
                }

                string alarmFullPath = Path.Combine(Environment.CurrentDirectory, alarmConfig.AlarmFileName);
                Dictionary<string, int> dicAlarmIndexes = new Dictionary<string, int>();
                alarms.Clear();
                allAlarms.Clear();

                string[] allRows = File.ReadAllLines(alarmFullPath);
                if (allRows == null || allRows.Length < 2)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , "There are no alarms in file"));
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
                    oneRow.PlcAddress = ushort.Parse(getThisRow[dicAlarmIndexes["PlcAddress"]]);
                    oneRow.PlcBitNumber = ushort.Parse(getThisRow[dicAlarmIndexes["PlcBitNumber"]]);
                    oneRow.Level = EnumAlarmLevelParse(getThisRow[dicAlarmIndexes["Level"]]);
                    oneRow.Description = getThisRow[dicAlarmIndexes["Description"]];

                    allAlarms.Add(oneRow.Id, oneRow);
                    alarms.Add(oneRow);
                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , "Load Alarm File Ok"));
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public Alarm GetAlarmClone(int id)
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

                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"[Id={id}]"));
            }

            return alarm;
        }

        public void SetAlarm(int id)
        {
            if (dicHappeningAlarms.ContainsKey(id))
            {
                var ngMsg = $"AlarmHandler : Set alarm fail, [Id={id}][Already in HappeningAlarms]";
                OnMessageShowEvent?.Invoke(this, ngMsg);

                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ngMsg));

                return;
            }

            Alarm alarm = GetAlarmClone(id);
            DateTime timeStamp = DateTime.Now;
            alarm.SetTime = timeStamp;
            dicHappeningAlarms.TryAdd(id, alarm);
            loggerAgent.LogAlarmHistory(alarm);

            var okMsg = $"AlarmHandler : Set alarm ok, [Id={id}]";
            OnMessageShowEvent?.Invoke(this, okMsg);

            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , okMsg));
        }

        public void ResetAlarm(int id)
        {
            if (!dicHappeningAlarms.ContainsKey(id))
            {
                var ngMsg = $"AlarmHandler : Reset alarm fail, [Id={id}][Not in HappeningAlarms]";
                OnMessageShowEvent?.Invoke(this, ngMsg);

                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ngMsg));
                return;
            }

            DateTime resetTime = DateTime.Now;
            dicHappeningAlarms.TryRemove(id, out Alarm alarm);
            alarm.ResetTime = resetTime;
            loggerAgent.LogAlarmHistory(alarm);

            var okMsg = $"AlarmHandler : Reset alarm ok, [Id={id}]";
            OnMessageShowEvent?.Invoke(this, okMsg);

            loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                , okMsg));
        }

        public void ResetAllAlarms()
        {
            try
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

                var okMsg = $"AlarmHandler : Reset all alarm ok";
                OnMessageShowEvent?.Invoke(this, okMsg);

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , okMsg));

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        private EnumAlarmLevel EnumAlarmLevelParse(string v)
        {
            try
            {
                v = v.Trim();

                return (EnumAlarmLevel)Enum.Parse(typeof(EnumAlarmLevel), v);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , ex.StackTrace));
                return EnumAlarmLevel.Warn;
            }
        }
    }
}
