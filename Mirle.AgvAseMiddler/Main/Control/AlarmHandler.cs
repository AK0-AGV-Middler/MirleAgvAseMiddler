﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.Configs;
 
using System.Collections.Concurrent;
using System.Reflection;
using System.Diagnostics;
using Mirle.Tools;

namespace Mirle.AgvAseMiddler.Controller
{
    [Serializable]
    public class AlarmHandler
    {
        //TODO: 
        //(未實作)將檔案讀入到AlarmForm的History欄位，檢視或檢索。目前無想法。
        #region Containers
        public Dictionary<int, Alarm> allAlarms = new Dictionary<int, Alarm>();
        public ConcurrentDictionary<int, Alarm> dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
        //public ConcurrentQueue<Alarm> queHistoryAlarm = new ConcurrentQueue<Alarm>();
        public Alarm LastAlarm { get; set; } = new Alarm();
        private bool hasAlarm = false;
        public bool HasAlarm
        {
            get { return hasAlarm; }
            set
            {
                if (value != hasAlarm)
                {
                    hasAlarm = value;
                }
            }
        }
        private bool hasWarn = false;
        public bool HasWarn
        {
            get { return hasWarn; }
            set
            {
                if (value != hasWarn)
                {
                    hasWarn = value;
                }
            }
        }
        #endregion

        public event EventHandler<Alarm> OnSetAlarmEvent;
        public event EventHandler<Alarm> OnPlcResetOneAlarmEvent;
        public event EventHandler<string> OnResetAllAlarmsEvent;

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
                    oneRow.PlcWord = ushort.Parse(getThisRow[dicAlarmIndexes["PlcWord"]]);
                    oneRow.PlcBit = ushort.Parse(getThisRow[dicAlarmIndexes["PlcBit"]]);
                    oneRow.Level = EnumAlarmLevelParse(getThisRow[dicAlarmIndexes["Level"]]);
                    oneRow.Description = getThisRow[dicAlarmIndexes["Description"]];

                    allAlarms.Add(oneRow.Id, oneRow);
                }

               mirleLogger.Log( new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , "Load Alarm File Ok"));
            }
            catch (Exception ex)
            {
                mirleLogger.Log( new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }        

        public bool SetAlarm(int id)
        {
            try
            {
                DateTime timeStamp = DateTime.Now;
                Alarm alarm = allAlarms.ContainsKey(id)? allAlarms[id]: new Alarm { Id = id };
                alarm.SetTime = timeStamp;

                if (dicHappeningAlarms.ContainsKey(id))
                {
                    return false;
                }
                else
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
                    OnSetAlarmEvent?.Invoke(this, alarm);
                    return true;
                }
            }
            catch (Exception ex)
            {
                mirleLogger.Log( new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                return false;
            }
        }

        public void ResetAlarm(int id)
        {
            if (!dicHappeningAlarms.ContainsKey(id))
            {
                var ngMsg = $"AlarmHandler : Reset alarm fail, [Id={id}][Not in HappeningAlarms]";

               mirleLogger.Log( new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ngMsg));
                return;
            }
            DateTime timeStamp = DateTime.Now;
            dicHappeningAlarms.TryRemove(id, out Alarm alarm);
            var happeningAlarms = dicHappeningAlarms.Values.ToList();
            HasAlarm = false;
            HasWarn = false;
            foreach (var item in happeningAlarms)
            {
                if (item.Level== EnumAlarmLevel.Alarm)
                {
                    HasAlarm = true;
                }

                if (item.Level == EnumAlarmLevel.Warn)
                {
                    HasWarn = true;
                }
            }
            alarm.ResetTime = timeStamp;
            LogAlarmHistory(alarm);
            OnPlcResetOneAlarmEvent?.Invoke(this, alarm);
        }

        public void ResetAllAlarms()
        {
            try
            {
                DateTime timeStamp = DateTime.Now;
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
                var msg = $"清除所有警報，花費{sw.ElapsedMilliseconds}毫秒";
                OnResetAllAlarmsEvent?.Invoke(this, msg);
                mirleLogger.Log( new LogFormat("AlarmHistory", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , msg));
            }
            catch (Exception ex)
            {
                mirleLogger.Log( new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                  , ex.StackTrace));
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
                    , ex.StackTrace));
            }
        }
    }
}