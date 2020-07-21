using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
 
using System.Collections.Concurrent;
using System.Reflection;
using System.Diagnostics;
using Mirle.Tools;

namespace Mirle.Agv.AseMiddler.Controller
{
    [Serializable]
    public class AlarmHandler
    {
        //TODO: 
        //(未實作)將檔案讀入到AlarmForm的History欄位, 檢視或檢索.目前無想法.
        #region Containers
        public Dictionary<int, Alarm> allAlarms = new Dictionary<int, Alarm>();
        public ConcurrentDictionary<int, Alarm> dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
        //public ConcurrentQueue<Alarm> queHistoryAlarm = new ConcurrentQueue<Alarm>();
        public Alarm LastAlarm { get; set; } = new Alarm();
        public bool HasAlarm { get; set; } = false;
        public bool HasWarn { get; set; } = false;
        #endregion

        public event EventHandler<Alarm> SetAlarmToUI;
        public event EventHandler<string> ResetAllAlarmsToUI;

        public event EventHandler<Alarm> SetAlarmToAgvl;
        public event EventHandler<Alarm> SetAlarmToAgvc;
        public event EventHandler ResetAllAlarmsToAgvl;
        public event EventHandler ResetAllAlarmsToAgvc;

        private MainFlowHandler mainFlowHandler;
        private AlarmConfig alarmConfig;
        private MirleLogger mirleLogger = MirleLogger.Instance;

        public AlarmHandler(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            this.alarmConfig = mainFlowHandler.GetAlarmConfig();
            LoadAlarmFile();
        }

        private void LoadAlarmFile()
        {
            try
            {
                if (string.IsNullOrEmpty(alarmConfig.AlarmFileName))
                {
                    mirleLogger.Log( new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , $"string.IsNullOrEmpty(alarmConfig.AlarmFileName)={string.IsNullOrEmpty(alarmConfig.AlarmFileName)}"));
                    return;
                }

                string alarmFullPath = Path.Combine(Environment.CurrentDirectory, alarmConfig.AlarmFileName);
                Dictionary<string, int> dicAlarmIndexes = new Dictionary<string, int>();
                allAlarms.Clear();

                string[] allRows = File.ReadAllLines(alarmFullPath);
                if (allRows == null || allRows.Length < 2)
                {
                    mirleLogger.Log( new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
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
                    oneRow.Level = EnumAlarmLevelParse(getThisRow[dicAlarmIndexes["Level"]]);
                    oneRow.Description = getThisRow[dicAlarmIndexes["Description"]];

                    allAlarms.Add(oneRow.Id, oneRow);
                }

               mirleLogger.Log( new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , "Load Alarm File Ok"));
            }
            catch (Exception ex)
            {
                mirleLogger.Log( new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.Message));
            }
        }        

        public void SetAlarmFromAgvm(int id)
        {
            if (!dicHappeningAlarms.ContainsKey(id))
            {
                SetAlarm(id);
                Alarm alarm = allAlarms.ContainsKey(id) ? allAlarms[id] : new Alarm { Id = id };
                SetAlarmToAgvl?.Invoke(this, alarm);
                SetAlarmToAgvc?.Invoke(this, alarm);
                SetAlarmToUI?.Invoke(this, alarm);
            }           
        }

        public void SetAlarmFromAgvl(int id)
        {
            if (!dicHappeningAlarms.ContainsKey(id))
            {
                SetAlarm(id);
                Alarm alarm = allAlarms.ContainsKey(id) ? allAlarms[id] : new Alarm { Id = id };
                SetAlarmToAgvc?.Invoke(this, alarm);
                SetAlarmToUI?.Invoke(this, alarm);
            }            
        }

        private void SetAlarm(int id)
        {
            try
            {
                DateTime timeStamp = DateTime.Now;
                Alarm alarm = allAlarms.ContainsKey(id) ? allAlarms[id] : new Alarm { Id = id };
                alarm.SetTime = timeStamp;

                if (!dicHappeningAlarms.ContainsKey(id))
                {
                    dicHappeningAlarms.TryAdd(id, alarm);
                    LogAlarmHistory(alarm);
                    LastAlarm = alarm;
                    switch (alarm.Level)
                    {
                        case EnumAlarmLevel.Alarm:
                            HasAlarm = true;
                            break;
                        case EnumAlarmLevel.Warn:
                        default:
                            HasWarn = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.Message));
            }
        }

        public void ResetAllAlarmsFromAgvm()
        {
            ResetAllAlarms();
            ResetAllAlarmsToAgvc?.Invoke(this, new EventArgs());
            ResetAllAlarmsToAgvl?.Invoke(this, new EventArgs());
        }

        public void ResetAllAlarmsFromAgvl()
        {
            ResetAllAlarms();
            ResetAllAlarmsToAgvc?.Invoke(this, new EventArgs());
        }

        public void ResetAllAlarmFromAgvc()
        {
            ResetAllAlarms();
            ResetAllAlarmsToAgvl?.Invoke(this, new EventArgs());
        }

        public void ResetAllAlarms()
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                lock (dicHappeningAlarms)
                {
                    dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
                    HasAlarm = false;
                    HasWarn = false;
                    LastAlarm = new Alarm();
                }
                sw.Stop();
                var msg = $"ResetAllAlarms, cost {sw.ElapsedMilliseconds} ms";
                ResetAllAlarmsToUI?.Invoke(this, msg);
                mirleLogger.Log(new LogFormat("AlarmHistory", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , msg));
            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.Message));
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
                mirleLogger.Log( new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , ex.Message));
                return EnumAlarmLevel.Warn;
            }
        }       

        private void LogAlarmHistory(Alarm alarm)
        {
            try
            {
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff");
                string msg = $"{timeStamp},{alarm.Id},{alarm.AlarmText},{alarm.Level},{alarm.SetTime},{alarm.ResetTime},{alarm.Description}";

                mirleLogger.LogString("AlarmHistory", msg);
            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.Message));
            }
        }

        
    }
}
