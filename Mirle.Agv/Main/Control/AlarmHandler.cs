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
using System.Diagnostics;

namespace Mirle.Agv.Controller
{
    [Serializable]
    public class AlarmHandler
    {
        //TODO: 
        //(未實作)將HistoryAlarms寫入檔案，請參考Logger中交替Que運作方式。
        //(未實作)將檔案讀入到AlarmForm的History欄位，檢視或檢索。目前無想法。
        #region Containers
        public Dictionary<int, Alarm> allAlarms = new Dictionary<int, Alarm>();
        public ConcurrentDictionary<int, Alarm> dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
        public ConcurrentQueue<Alarm> queHistoryAlarm = new ConcurrentQueue<Alarm>();
        public Alarm LastAlarm { get; set; } = new Alarm();
        #endregion

        public event EventHandler<Alarm> OnSetAlarmEvent;
        public event EventHandler<int> OnResetAllAlarmsEvent;

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
            Alarm alarm = new Alarm { Id = id };            

            try
            {
                if (allAlarms.ContainsKey(id))
                {
                    alarm = allAlarms[id].DeepClone();
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , ex.StackTrace));
            }
            return alarm;
        }

        public bool SetAlarm(int id)
        {
            try
            {
                DateTime timeStamp = DateTime.Now;
                Alarm alarm = GetAlarmClone(id);
                alarm.SetTime = timeStamp;

                if (dicHappeningAlarms.ContainsKey(id))
                {
                    //var msg = $"AlarmHandler : Set alarm +++FAIL+++, [Id={id}][Already in HappeningAlarms]";
                    //loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    //    , msg));
                    return false;
                }
                else
                {
                    dicHappeningAlarms.TryAdd(id, alarm);
                    queHistoryAlarm.Enqueue(alarm);
                    LastAlarm = alarm.DeepClone();

                    //loggerAgent.LogAlarmHistory(alarm);
                    OnSetAlarmEvent?.Invoke(this, alarm);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , ex.StackTrace));
                return false;
            }
        }

        public void ResetAlarm(int id)
        {
            //if (!dicHappeningAlarms.ContainsKey(id))
            //{
            //    var ngMsg = $"AlarmHandler : Reset alarm fail, [Id={id}][Not in HappeningAlarms]";
            //    OnMessageShowEvent?.Invoke(this, ngMsg);

            //    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //        , ngMsg));
            //    return;
            //}

            //DateTime resetTime = DateTime.Now;
            //dicHappeningAlarms.TryRemove(id, out Alarm alarm);
            //alarm.ResetTime = resetTime;
            //loggerAgent.LogAlarmHistory(alarm);

            //var okMsg = $"AlarmHandler : Reset alarm ok, [Id={id}]";
            //OnMessageShowEvent?.Invoke(this, okMsg);

            //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //    , okMsg));
        }

        public void ResetAllAlarms()
        {
            try
            {
                int dicHappeningAlarmsCount = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                lock (dicHappeningAlarms)
                {
                    dicHappeningAlarmsCount = dicHappeningAlarms.Count;
                    dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
                    OnResetAllAlarmsEvent?.Invoke(this, dicHappeningAlarmsCount);
                    LastAlarm = new Alarm();
                }
                sw.Stop();
                var msg = $"AlarmHandler : Reset All Alarms, [Count={dicHappeningAlarmsCount}][TimeMs={sw.ElapsedMilliseconds}]";
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , msg));

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
