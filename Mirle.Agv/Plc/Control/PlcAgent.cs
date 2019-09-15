//#define  DebugTestThread 
//#define  DebugTest 

using ClsMCProtocol;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Mirle.Agv.View;
using System.Runtime.CompilerServices;
using Mirle.Agv.Model.Configs;

namespace Mirle.Agv.Controller
{
    public class PlcAgent
    {
        #region "const"
        private const int Fork_Command_Format_NG = 270001;
        private const int Fork_Command_Read_timeout = 270002;
        private const int Fork_Not_Busy_timeout = 270003;
        private const int Fork_Command_Executing_timeout = 270004;
        private const int Batterys_Charging_Time_Out = 270005;
        #endregion

        private MCProtocol aMCProtocol;
        private CassetteIDReader aCassetteIDReader = new CassetteIDReader();

        public string PlcId { get; set; } = "AGVPLC";
        public string Ip { get; set; } = "192.168.3.39";
        public string Port { get; set; } = "6000";
        public string LocalIp { get; set; } = "192.168.3.100";
        public string LocalPort { get; set; } = "3001";

        public Int64 ForkCommandReadTimeout { get; set; } = 5000;
        public Int64 ForkCommandBusyTimeout { get; set; } = 5000;
        public Int64 ForkCommandMovingTimeout { get; set; } = 120000;

        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private Logger plcAgentLogger;
        private Logger errLogger;
        private Logger portPIOLogger;
        private Logger chargerPIOLogger;

        private Logger BatteryLogger;
        private Logger BatteryPercentage;

        public PlcVehicle APLCVehicle;

        private PlcForkCommand eventForkCommand; //發event前 先把executing commnad reference先放過來, 避免外部exevnt處理時發生null問題
        private bool clearExecutingForkCommandFlag = false;

        private Thread plcOtherControlThread = null;
        private Thread plcForkCommandControlThread = null;

        private Thread TestThread = null;



        private UInt16 beforeBatteryPercentageInteger = 0;
        private UInt32 alarmReadIndex = 0;

        private JogPitchForm jogPitchForm = null;

        public event EventHandler<PlcForkCommand> OnForkCommandExecutingEvent;
        public event EventHandler<PlcForkCommand> OnForkCommandFinishEvent;
        public event EventHandler<PlcForkCommand> OnForkCommandErrorEvent;
        public event EventHandler<UInt16> OnBatteryPercentageChangeEvent;
        public event EventHandler<string> OnCassetteIDReadFinishEvent;

        public event EventHandler<EnumAutoState> OnIpcAutoManualChangeEvent;

        public Boolean IsNeedReadCassetteID { get; set; } = true;
        public Boolean IsFakeForking { get; set; } = false;

        private Boolean boolConnectionState = false;
        public Boolean ConnectionState
        {
            get { return boolConnectionState; }
            //set
            //{
            //    boolConnectionState = value;
            //}
        }

        private AlarmHandler aAlarmHandler = null;

        public void SetOutSideObj(ref JogPitchForm jogPitchForm)
        {
            this.jogPitchForm = jogPitchForm;

            //this.APLCVehicle.Hmi.beforeFromPlcWord = this.APLCVehicle.Hmi.FromPlcWord.DeepClone();
        }

        [Conditional("DebugTestThread")]
        private void OpenTestThread()
        {
            if (TestThread == null)
            {
                TestThread = new Thread(TestThreadRun);
                TestThread.Name = "TestThread";
                TestThread.IsBackground = true;
                TestThread.Start();
            }
        }
        private void TestThreadRun()
        {
            Stopwatch swBatteryLogger = new Stopwatch();
            while (true)
            {
            }
        }
        [Conditional("DebugTest")]
        private void TestFun()
        {

        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private string GetFunName([CallerMemberName] string memberName = "")
        {
            return GetType().Name + ":" + memberName;
        }
        private Stopwatch swBatteryLogger = new Stopwatch();
        private bool BatteryMaxMinIni = true;
        private double MaxMeterCurrent, MinMeterCurrent;
        private double MaxMeterVoltage, MinMeterVoltage;
        private double MaxMeterWatt, MinMeterWatt;
        private double MaxMeterAh, MinMeterAh;
        private double MaxCcModeAh, MinCcModeAh;
        private ushort MaxCcModeCounter, MinCcModeCounter;
        private ushort MaxFullChargeIndex, MinFullChargeIndex;
        private double MaxFBatteryTemperature, MinFBatteryTemperature;
        private double MaxBBatteryTemperature, MinBBatteryTemperature;

        private void ValueCompare<T>(T Source, ref T Distance, int type = 0) where T : System.IComparable<T>
        {
            switch (type)
            {
                case 0:
                    if (Source.CompareTo(Distance) > 0) Distance = Source;
                    break;
                case 1:
                    if (Source.CompareTo(Distance) < 0) Distance = Source;
                    break;
                default:
                    Distance = Source;
                    break;
            }
        }

        private void WriteBatLoggerCsv(ref Stopwatch sw, long ms)
        {
            sw.Start();
            if (BatteryMaxMinIni)
            {
                MaxMeterCurrent = this.APLCVehicle.Batterys.MeterCurrent;
                MaxMeterVoltage = this.APLCVehicle.Batterys.MeterVoltage;
                MaxMeterWatt = this.APLCVehicle.Batterys.MeterWatt;
                MaxMeterAh = this.APLCVehicle.Batterys.MeterAh;
                MaxCcModeAh = this.APLCVehicle.Batterys.CcModeAh;
                //MaxCcModeCounter = this.APLCVehicle.Batterys.CcModeCounter;
                //MaxFullChargeIndex = this.APLCVehicle.Batterys.FullChargeIndex;
                MaxFBatteryTemperature = this.APLCVehicle.Batterys.FBatteryTemperature;
                MaxBBatteryTemperature = this.APLCVehicle.Batterys.BBatteryTemperature;

                MinMeterCurrent = this.APLCVehicle.Batterys.MeterCurrent;
                MinMeterVoltage = this.APLCVehicle.Batterys.MeterVoltage;
                MinMeterWatt = this.APLCVehicle.Batterys.MeterWatt;
                MinMeterAh = this.APLCVehicle.Batterys.MeterAh;
                MinCcModeAh = this.APLCVehicle.Batterys.CcModeAh;
                //MinCcModeCounter = this.APLCVehicle.Batterys.CcModeCounter;
                //MinFullChargeIndex = this.APLCVehicle.Batterys.FullChargeIndex;
                MinFBatteryTemperature = this.APLCVehicle.Batterys.FBatteryTemperature;
                MinBBatteryTemperature = this.APLCVehicle.Batterys.BBatteryTemperature;

                BatteryMaxMinIni = false;
            }

            ValueCompare<double>(this.APLCVehicle.Batterys.MeterCurrent, ref MaxMeterCurrent);
            ValueCompare<double>(this.APLCVehicle.Batterys.MeterVoltage, ref MaxMeterVoltage);
            ValueCompare<double>(this.APLCVehicle.Batterys.MeterWatt, ref MaxMeterWatt);
            ValueCompare<double>(this.APLCVehicle.Batterys.MeterAh, ref MaxMeterAh);
            ValueCompare<double>(this.APLCVehicle.Batterys.CcModeAh, ref MaxCcModeAh);
            //ValueCompare<ushort>(this.APLCVehicle.Batterys.CcModeCounter, ref MaxCcModeCounter);
            //ValueCompare<ushort>(this.APLCVehicle.Batterys.FullChargeIndex, ref MaxFullChargeIndex);
            ValueCompare<double>(this.APLCVehicle.Batterys.FBatteryTemperature, ref MaxFBatteryTemperature);
            ValueCompare<double>(this.APLCVehicle.Batterys.BBatteryTemperature, ref MaxBBatteryTemperature);

            ValueCompare<double>(this.APLCVehicle.Batterys.MeterCurrent, ref MinMeterCurrent, 1);
            ValueCompare<double>(this.APLCVehicle.Batterys.MeterVoltage, ref MinMeterVoltage, 1);
            ValueCompare<double>(this.APLCVehicle.Batterys.MeterWatt, ref MinMeterWatt, 1);
            ValueCompare<double>(this.APLCVehicle.Batterys.MeterAh, ref MinMeterAh, 1);
            ValueCompare<double>(this.APLCVehicle.Batterys.CcModeAh, ref MinCcModeAh, 1);
            //ValueCompare<ushort>(this.APLCVehicle.Batterys.CcModeCounter, ref MinCcModeCounter, 1);
            //ValueCompare<ushort>(this.APLCVehicle.Batterys.FullChargeIndex, ref MinFullChargeIndex, 1);
            ValueCompare<double>(this.APLCVehicle.Batterys.FBatteryTemperature, ref MinFBatteryTemperature, 1);
            ValueCompare<double>(this.APLCVehicle.Batterys.BBatteryTemperature, ref MinBBatteryTemperature, 1);

            if (sw.ElapsedMilliseconds > ms)
            {
                string csvLog = "", Separator = ",";
                DateTime now;
                BatteryMaxMinIni = true;

                now = DateTime.Now;
                csvLog = now.ToString("HH:mm:ss.ff");


                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.MeterCurrent.ToString();
                csvLog = csvLog + Separator + this.MaxMeterCurrent.ToString();
                csvLog = csvLog + Separator + this.MinMeterCurrent.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.MeterVoltage.ToString();
                csvLog = csvLog + Separator + this.MaxMeterVoltage.ToString();
                csvLog = csvLog + Separator + this.MinMeterVoltage.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.MeterWatt.ToString();
                csvLog = csvLog + Separator + this.MaxMeterWatt.ToString();
                csvLog = csvLog + Separator + this.MinMeterWatt.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.MeterAh.ToString();
                csvLog = csvLog + Separator + this.MaxMeterAh.ToString();
                csvLog = csvLog + Separator + this.MinMeterAh.ToString();

                //csvLog = csvLog + Separator + this.APLCVehicle.Batterys.Percentage.ToString();
                //csvLog = csvLog + Separator + this.APLCVehicle.Batterys.AhWorkingRange.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.CcModeAh.ToString();
                csvLog = csvLog + Separator + this.MaxCcModeAh.ToString();
                csvLog = csvLog + Separator + this.MinCcModeAh.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.CcModeCounter.ToString();
                //csvLog = csvLog + Separator + this.MaxCcModeCounter.ToString();
                //csvLog = csvLog + Separator + this.MinCcModeCounter.ToString();

                //csvLog = csvLog + Separator + this.APLCVehicle.Batterys.MaxResetAhCcounter.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.FullChargeIndex.ToString();
                //csvLog = csvLog + Separator + this.MaxFullChargeIndex.ToString();
                //csvLog = csvLog + Separator + this.MinFullChargeIndex.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.FBatteryTemperature.ToString();
                csvLog = csvLog + Separator + this.MaxFBatteryTemperature.ToString();
                csvLog = csvLog + Separator + this.MinFBatteryTemperature.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.BBatteryTemperature.ToString();
                csvLog = csvLog + Separator + this.MaxBBatteryTemperature.ToString();
                csvLog = csvLog + Separator + this.MinBBatteryTemperature.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.Charging.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.BatteryType.ToString();
                for (int i = 1; i <= APLCVehicle.BatteryCellNum; i++)
                {
                    csvLog = csvLog + Separator + this.APLCVehicle.Batterys.BatteryCells[i].Voltage.ToString();
                }
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.Temperature_sensor_number.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.Temperature_1_MOSFET.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.Temperature_2_Cell.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.Temperature_3_MCU.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.BatteryCurrent.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.Packet_Voltage.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.Remain_Capacity.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.Batterys.Design_Capacity.ToString();

                BatteryLogger.SavePureLog(csvLog);

                sw.Stop();
                sw.Reset();
            }
        }
        public PlcAgent(MCProtocol objMCProtocol, AlarmHandler objAlarmHandler)
        {
            OpenTestThread();
            TestFun();

            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            APLCVehicle = Vehicle.Instance.GetPlcVehicle();
            SetupLoggers();

            this.aAlarmHandler = objAlarmHandler;

            this.aMCProtocol = objMCProtocol;
            //PLC_Config.xml
            ReadXml("PLC_Config.xml");
            //ReadXml();
            aMCProtocol.Name = "";
            aMCProtocol.OnDataChangeEvent += MCProtocol_OnDataChangeEvent;
            aMCProtocol.ConnectEvent += MCProtocol_OnConnectEvent;
            aMCProtocol.DisConnectEvent += MCProtocol_OnDisConnectEvent;

            aMCProtocol.Open(this.LocalIp, this.LocalPort);
            aMCProtocol.ConnectToPLC(this.Ip, this.Port);

            aCassetteIDReader.Connect();

            aMCProtocol.Start();

            //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "PLC Connect Start");
            LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "PLC Connect Start");
            LogPlcMsg(loggerAgent, logFormat);
        }

        private void SetupLoggers()
        {
            plcAgentLogger = loggerAgent.GetLooger("PlcAgent");
            errLogger = loggerAgent.GetLooger("Error");
            portPIOLogger = loggerAgent.GetLooger("PortPIO");
            chargerPIOLogger = loggerAgent.GetLooger("ChargerPIO");

            BatteryLogger = LoggerAgent.Instance.GetLooger("BatteryCSV");
            BatteryPercentage = LoggerAgent.Instance.GetLooger("BatteryPercentage");

        }

        //讀取XML
        private void ReadXml(string file_address)//Gavin 20190615
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            XmlDocument doc = new XmlDocument();
            doc.Load(file_address);
            var rootNode = doc.DocumentElement;     // <Motion>

            foreach (XmlNode item in rootNode.ChildNodes)
            {
                //預期只有一台PLC,多台要改寫
                XmlElement element = (XmlElement)item; // <Params> = each axis
                foreach (XmlNode childItem in element.ChildNodes)
                {
                    switch (childItem.Name)
                    {
                        case "ID":
                            this.PlcId = childItem.InnerText;
                            break;
                        case "IP":
                            this.Ip = childItem.InnerText;
                            break;
                        case "Port":
                            this.Port = childItem.InnerText;
                            break;
                        case "LocalIP":
                            this.LocalIp = childItem.InnerText;
                            break;
                        case "LocalPort":
                            this.LocalPort = childItem.InnerText;
                            break;
                        case "SOC_AH":
                            this.APLCVehicle.Batterys.AhWorkingRange = Convert.ToDouble(childItem.InnerText);
                            break;
                        case "Ah_Reset_CCmode_Counter":
                            this.APLCVehicle.Batterys.MaxResetAhCcounter = Convert.ToUInt16(childItem.InnerText);
                            break;
                        case "Ah_Reset_Timeout":
                            this.APLCVehicle.Batterys.ResetAhTimeout = Convert.ToUInt32(childItem.InnerText) * 1000;
                            break;
                        case "CassetteIDReaderIP":
                            aCassetteIDReader.Ip = childItem.InnerText;
                            break;
                        case "IsNeedReadCassetteID":
                            if (childItem.InnerText.ToLower() != "true")
                            {
                                this.IsNeedReadCassetteID = false;
                            }
                            else
                            {
                                this.IsNeedReadCassetteID = true;
                            }
                            break;
                        case "Fork_Command_Read_Timeout":
                            this.ForkCommandReadTimeout = Convert.ToUInt32(childItem.InnerText) * 1000;
                            break;
                        case "Fork_Command_Busy_Timeout":
                            this.ForkCommandBusyTimeout = Convert.ToUInt32(childItem.InnerText) * 1000;
                            break;
                        case "Fork_Command_Moving_Timeout":
                            this.ForkCommandMovingTimeout = Convert.ToUInt32(childItem.InnerText) * 1000;
                            break;

                        case "Port_AutoCharge_Low_SOC":
                            this.APLCVehicle.Batterys.PortAutoChargeLowSoc = Convert.ToDouble(childItem.InnerText);
                            break;
                        case "Port_AutoCharge_High_SOC":
                            this.APLCVehicle.Batterys.PortAutoChargeHighSoc = Convert.ToDouble(childItem.InnerText);
                            break;

                        case "Battery_Logger_Interval":
                            this.APLCVehicle.Batterys.Battery_Logger_Interval = Convert.ToUInt32(Convert.ToDouble(childItem.InnerText) * 1000);
                            break;

                        case "Batterys_Charging_Time_Out":// min
                            this.APLCVehicle.Batterys.Batterys_Charging_Time_Out = Convert.ToUInt32(childItem.InnerText) * 60000;
                            break;
                        case "Charging_Off_Delay":
                            this.APLCVehicle.Batterys.Charging_Off_Delay = Convert.ToUInt32(childItem.InnerText);
                            break;
                        case "CCMode_Stop_Voltage":
                            this.APLCVehicle.Batterys.CCModeStopVoltage = Convert.ToDouble(childItem.InnerText);
                            break;
                        case "Battery_Cell_Low_Voltage":
                            this.APLCVehicle.Batterys.Battery_Cell_Low_Voltage = Convert.ToDouble(childItem.InnerText);
                            break;
                    }

                }
            }
        }
        //private void ReadXml()
        //{
        //    XmlHandler xmlHandler = new XmlHandler();
        //    plcConfig = xmlHandler.ReadXml<PlcConfig>("Plc.xml");
        //}

        private void MCProtocol_OnDataChangeEvent(string sMessage, ClsMCProtocol.clsColParameter oColParam)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            try
            {
                int tagChangeCount = oColParam.Count();
                for (int i = 1; i <= tagChangeCount; i++)
                {

                    try
                    {
                        if (oColParam.Item(i).DataName.ToString().EndsWith("_PIO"))
                        {
                            //this.portPIOLogger.SaveLogFile("PortPIO", "9", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean);
                            LogPlcMsg(loggerAgent, new LogFormat("PortPIO", "9", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean));
                        }
                        else if (oColParam.Item(i).DataName.ToString().EndsWith("_CPIO"))
                        {
                            //this.chargerPIOLogger.SaveLogFile("PortPIO", "9", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean);
                            LogPlcMsg(loggerAgent, new LogFormat("PortPIO", "9", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean));
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("BeamSensor"))
                        {
                            PlcBeamSensor aBeamSensor = this.APLCVehicle.dicBeamSensor[oColParam.Item(i).DataName.ToString()];
                            if (aBeamSensor != null)
                            {
                                if (oColParam.Item(i).DataName.ToString().EndsWith("Near"))
                                {
                                    aBeamSensor.NearSignal = oColParam.Item(i).AsBoolean;
                                }
                                else if (oColParam.Item(i).DataName.ToString().EndsWith("Far"))
                                {
                                    aBeamSensor.FarSignal = oColParam.Item(i).AsBoolean;
                                }
                                else
                                {
                                    //errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith Near or Far");
                                    LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith Near or Far"));
                                }
                            }
                            else
                            {
                                //errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
                                LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object"));
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("RBeamSensor"))
                        {
                            //RBeamSensor Sleep read
                            PlcBeamSensor aBeamSensor = this.APLCVehicle.dicBeamSensor[oColParam.Item(i).DataName.ToString()];
                            if (aBeamSensor != null)
                            {
                                if (oColParam.Item(i).DataName.ToString().EndsWith("_Sleep"))
                                {
                                    aBeamSensor.ReadSleepSignal = oColParam.Item(i).AsBoolean;
                                }
                                else
                                {
                                    //errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");
                                    LogFormat logFormat = new LogFormat("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");
                                    LogPlcMsg(loggerAgent, logFormat);
                                }
                            }
                            else
                            {
                                //errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
                                LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object"));
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("WBeamSensor"))
                        {
                            //WBeamSensor Sleep write
                            PlcBeamSensor aBeamSensor = this.APLCVehicle.dicBeamSensor[oColParam.Item(i).DataName.ToString()];
                            if (aBeamSensor != null)
                            {
                                if (oColParam.Item(i).DataName.ToString().EndsWith("_Sleep"))
                                {
                                    aBeamSensor.WriteSleepSignal = oColParam.Item(i).AsBoolean;
                                }
                                else
                                {
                                    //errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");
                                    LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep"));
                                }
                            }
                            else
                            {
                                //errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
                                LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object"));
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("Bumper"))
                        {

                            PlcBumper aBumper = this.APLCVehicle.dicBumper[oColParam.Item(i).DataName.ToString()];
                            if (aBumper != null)
                            {
                                aBumper.Signal = oColParam.Item(i).AsBoolean;
                            }
                            else
                            {
                                //errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBumper object");
                                LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBumper object"));
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("EMO_"))
                        {

                            PlcEmo aPLCEMO = this.APLCVehicle.dicPlcEmo[oColParam.Item(i).DataName.ToString()];
                            if (aPLCEMO != null)
                            {
                                aPLCEMO.Signal = oColParam.Item(i).AsBoolean;
                            }
                            else
                            {
                                //errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCEMO object");
                                LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCEMO object"));
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("Cell_"))
                        {
                            string[] strarry = oColParam.Item(i).DataName.ToString().Split('_');
                            if (oColParam.Item(i).DataName.ToString() == "Cell_number")
                            {
                                this.APLCVehicle.Batterys.Cell_number = oColParam.Item(i).AsUInt16;
                            }
                            else
                            {
                                if (strarry[2] == "Voltage")
                                {
                                    this.APLCVehicle.Batterys.BatteryCells[Convert.ToInt16(strarry[1])].Voltage = this.DECToDouble(oColParam.Item(i).AsUInt16, 1, 3);
                                }
                            }
                        }
                        else
                        {
                            switch (oColParam.Item(i).DataName.ToString())
                            {
                                case "BumpAlarmStatus":
                                    this.APLCVehicle.BumperAlarmStatus = aMCProtocol.get_ItemByTag("BumpAlarmStatus").AsBoolean;
                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"BumpAlarmStatus = { this.APLCVehicle.BumperAlarmStatus }"));
                                    break;

                                case "BatteryGotech":
                                    if (oColParam.Item(i).AsBoolean)
                                    {
                                        this.APLCVehicle.Batterys.BatteryType = EnumBatteryType.Gotech;
                                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Battery = Gotech"));
                                    }
                                    else
                                    {

                                    }
                                    break;
                                case "BatteryYinda":
                                    if (oColParam.Item(i).AsBoolean)
                                    {
                                        this.APLCVehicle.Batterys.BatteryType = EnumBatteryType.Yinda;
                                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Battery = Yinda"));
                                    }
                                    else
                                    {

                                    }
                                    break;
                                case "MeterVoltage":
                                    this.APLCVehicle.Batterys.MeterVoltage = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "MeterCurrent":
                                    this.APLCVehicle.Batterys.MeterCurrent = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "MeterWatt":
                                    this.APLCVehicle.Batterys.MeterWatt = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "MeterWattHour":
                                    this.APLCVehicle.Batterys.MeterWattHour = this.DECToDouble(oColParam.Item(i).AsUInt32, 2);
                                    break;
                                case "MeterAH":
                                    this.APLCVehicle.Batterys.MeterAh = this.DECToDouble(oColParam.Item(i).AsUInt32, 2);
                                    break;
                                case "FullChargeIndex":
                                    //if (this.APLCVehicle.Batterys.FullChargeIndex == 0)
                                    //{
                                    //    //AGV斷電重開
                                    //    //this.APLCVehicle.APlcBatterys.CcModeAh = this.APLCVehicle.APlcBatterys.CcModeAh - this.APLCVehicle.APlcBatterys.AhWorkingRange;
                                    //    this.APLCVehicle.Batterys.SetCcModeAh(this.APLCVehicle.Batterys.CcModeAh - this.APLCVehicle.Batterys.AhWorkingRange, false);
                                    //}
                                    //else
                                    //{
                                    this.APLCVehicle.Batterys.CcModeFlag = true;
                                    //CC Mode達到                                
                                    this.APLCVehicle.Batterys.FullChargeIndex = oColParam.Item(i).AsUInt16;

                                    //}
                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"FullChargeIndex = {this.APLCVehicle.Batterys.FullChargeIndex}"));

                                    break;
                                case "HomeStatus":
                                    this.APLCVehicle.Robot.ForkHome = aMCProtocol.get_ItemByTag("HomeStatus").AsBoolean;
                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"HomeStatus = {this.APLCVehicle.Robot.ForkHome}"));
                                    break;
                                case "ChargeStatus":
                                    if (aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean)
                                    {
                                        this.APLCVehicle.Batterys.Charging = aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean;
                                        ChgStasOffDelayFlag = false;
                                    }
                                    else
                                    {
                                        ChgStasOffDelayFlag = true;
                                        ccModeAHSet();
                                    }
                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ChargeStatus = {this.APLCVehicle.Batterys.Charging}"));

                                    //this.APLCVehicle.Batterys.Charging = aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean;
                                    //if (!this.APLCVehicle.Batterys.Charging)
                                    //{
                                    //    ccModeAHSet();
                                    //}
                                    break;

                                case "FBatteryTemperature":
                                    //this.APLCVehicle.Batterys.FBatteryTemperature = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
                                    {
                                        Int64 value = oColParam.Item(i).AsUInt16;
                                        if (value >= 32768)
                                            this.APLCVehicle.Batterys.FBatteryTemperature = Convert.ToDouble(value - 65536);
                                        else
                                            this.APLCVehicle.Batterys.FBatteryTemperature = Convert.ToDouble(value);
                                    }
                                    break;
                                case "BBatteryTemperature":
                                    //this.APLCVehicle.Batterys.BBatteryTemperature = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
                                    {
                                        Int64 value = oColParam.Item(i).AsUInt16;
                                        if (value >= 32768)
                                            this.APLCVehicle.Batterys.FBatteryTemperature = Convert.ToDouble(value - 65536);
                                        else
                                            this.APLCVehicle.Batterys.FBatteryTemperature = Convert.ToDouble(value);
                                    }
                                    break;
                                case "PLCAlarmIndex":
                                    //7個alarm set/reset
                                    try
                                    {
                                        int iAlarmOffset = 200000;
                                        for (int j = 1; j <= 7; j++)
                                        {
                                            if (this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16 != 0)
                                            {
                                                UInt16 AlarmCode = this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16;
                                                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"AlarmCode = {AlarmCode + iAlarmOffset}"));

                                                //不區分alarm/warning => alarm CSV裡區分
                                                if (this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmEvent").AsUInt16 == 1)
                                                {
                                                    //set
                                                    //this.setAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16));
                                                    this.setAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16) + iAlarmOffset);
                                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"AlarmEvent = {1}"));
                                                }
                                                else if (this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmEvent").AsUInt16 == 2)
                                                {
                                                    //clear
                                                    //this.resetAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16));
                                                    this.resetAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16) + iAlarmOffset);
                                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"AlarmEvent = {2}"));
                                                }
                                                else
                                                {

                                                }
                                            }
                                        }

                                        alarmReadIndex++;
                                        alarmReadIndex = alarmReadIndex % 65535 + 1;
                                        if (this.aMCProtocol.WritePLC())
                                        {
                                            //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC success");
                                            LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC success"));
                                        }
                                        else
                                        {
                                            //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC fail");
                                            LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC fail"));
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        //this.errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", ex.ToString());
                                        LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", ex.ToString()));
                                    }

                                    //Console.Out.Write("alarm");
                                    break;
                                case "EquipementActionIndex":
                                    this.eqActionIndex = oColParam.Item(i).AsUInt16;
                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"EquipementActionIndex = {this.eqActionIndex}"));

                                    break;
                                case "ForkReady":
                                    this.APLCVehicle.Robot.ForkReady = aMCProtocol.get_ItemByTag("ForkReady").AsBoolean;
                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkReady = {this.APLCVehicle.Robot.ForkReady}"));

                                    break;
                                case "ForkBusy":
                                    this.APLCVehicle.Robot.ForkBusy = aMCProtocol.get_ItemByTag("ForkBusy").AsBoolean;
                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkBusy = {this.APLCVehicle.Robot.ForkBusy}"));

                                    break;
                                case "ForkCommandFinish":
                                    this.APLCVehicle.Robot.ForkFinish = aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean;
                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkCommandFinish = {this.APLCVehicle.Robot.ForkFinish}"));

                                    break;
                                case "StageLoading":
                                    this.APLCVehicle.Loading = aMCProtocol.get_ItemByTag("StageLoading").AsBoolean;
                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"StageLoading = {this.APLCVehicle.Loading}"));
                                    break;

                                case "Temperature_sensor_number":
                                    this.APLCVehicle.Batterys.Temperature_sensor_number = aMCProtocol.get_ItemByTag("Temperature_sensor_number").AsUInt16;
                                    break;
                                case "Temperature_1_MOSFET":
                                    this.APLCVehicle.Batterys.Temperature_1_MOSFET = this.DECToDouble(aMCProtocol.get_ItemByTag("Temperature_1_MOSFET").AsUInt16, 1, 1);
                                    break;
                                case "Temperature_2_Cell":
                                    this.APLCVehicle.Batterys.Temperature_2_Cell = this.DECToDouble(aMCProtocol.get_ItemByTag("Temperature_2_Cell").AsUInt16, 1, 1);
                                    break;
                                case "Temperature_3_MCU":
                                    this.APLCVehicle.Batterys.Temperature_3_MCU = this.DECToDouble(aMCProtocol.get_ItemByTag("Temperature_3_MCU").AsUInt16, 1, 1);
                                    break;
                                case "BatteryCurrent":
                                    this.APLCVehicle.Batterys.BatteryCurrent = this.DECToDouble(aMCProtocol.get_ItemByTag("BatteryCurrent").AsUInt16, 1, 1);
                                    break;
                                case "Packet_Voltage":
                                    this.APLCVehicle.Batterys.Packet_Voltage = this.DECToDouble(aMCProtocol.get_ItemByTag("Packet_Voltage").AsUInt16, 2, 3);
                                    break;
                                case "Remain_Capacity":
                                    this.APLCVehicle.Batterys.Remain_Capacity = aMCProtocol.get_ItemByTag("Remain_Capacity").AsUInt16;
                                    break;
                                case "Design_Capacity":
                                    this.APLCVehicle.Batterys.Design_Capacity = aMCProtocol.get_ItemByTag("Design_Capacity").AsUInt16;
                                    break;

                                case "BatterySOC_Form_Plc":
                                    this.APLCVehicle.Batterys.BatterySOCFormPlc = aMCProtocol.get_ItemByTag("BatterySOC_Form_Plc").AsUInt16;
                                    break;
                                case "BatterySOH_Form_Plc":
                                    this.APLCVehicle.Batterys.BatterySOHFormPlc = aMCProtocol.get_ItemByTag("BatterySOH_Form_Plc").AsUInt16;
                                    break;

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //this.errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName + ":" + ex.ToString());
                        LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName + ":" + ex.ToString()));
                    }


                }
            }
            catch (Exception ex)
            {
                //this.errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", ex.ToString());
                LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", ex.ToString()));
            }
        }
        private void ccModeAHSet()
        {

            if (this.APLCVehicle.Batterys.CcModeFlag)
            {
                //this.APLCVehicle.APlcBatterys.CcModeAh = this.DECToDouble(aMCProtocol.get_ItemByTag("MeterAH").AsUInt32, 2);
                this.APLCVehicle.Batterys.SetCcModeAh(this.DECToDouble(aMCProtocol.get_ItemByTag("MeterAH").AsUInt32, 2), true);

                //判斷CCModeCounter
                if (this.APLCVehicle.Batterys.MaxResetAhCcounter <= this.APLCVehicle.Batterys.CcModeCounter)
                {
                    this.APLCVehicle.Batterys.CcModeCounter = 0;
                    this.SetMeterAHToZero();
                }
                else
                {
                    //if (this.APLCVehicle.APlcBatterys.CcModeAh > 0.5)
                    //{
                    //    this.APLCVehicle.APlcBatterys.CcModeCounter = 0;
                    //    this.SetMeterAHToZero();
                    //}
                    this.APLCVehicle.Batterys.CcModeCounter++;
                }
                this.APLCVehicle.Batterys.CcModeFlag = false;

            }
            else
            {
                //
            }

        }
        private double DECToDouble(Int64 inputNum, int Wordlength, int digit = 2)
        {
            double result = 0.00;
            string str = "1";
            if (digit <= 0)
            {
                str = "1";
            }
            else
            {
                for (int i = 0; i < digit; i++)
                {
                    str += "0";
                }
            }
            double d = Convert.ToDouble(str);
            switch (Wordlength)
            {
                case 1:
                    if (inputNum >= 32768)
                    {
                        result = Convert.ToDouble(inputNum - 65536) / d;
                    }
                    else
                    {
                        result = Convert.ToDouble(inputNum) / d;
                    }
                    break;
                case 2:

                    if (inputNum >= 2147483648)
                    {
                        result = Convert.ToDouble(inputNum - 4294967296) / d;
                    }
                    else
                    {
                        result = Convert.ToDouble(inputNum) / d;
                    }
                    break;

            }


            return result;
        }
        private double DECToDouble(Int64 inputNum, int length)
        {
            double returnValue = 0.0;
            switch (length)
            {
                case 1:
                    if (inputNum >= 32768)
                    {
                        returnValue = Convert.ToDouble(inputNum - 65536) / 100.00;
                    }
                    else
                    {
                        returnValue = Convert.ToDouble(inputNum) / 100.00;
                    }
                    break;
                case 2:

                    if (inputNum >= 2147483648)
                    {
                        returnValue = Convert.ToDouble(inputNum - 4294967296) / 100.00;
                    }
                    else
                    {
                        returnValue = Convert.ToDouble(inputNum) / 100.00;
                    }
                    break;
                    //returnValue = (2147483648.0 - Convert.ToDouble(inputNum)) / 100.00;
            }
            return returnValue;
        }

        private void MCProtocol_OnConnectEvent(String message)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "PLC is connected");
            this.boolConnectionState = true;

            if (plcOtherControlThread == null)
            {
                plcOtherControlThread = new Thread(plcOtherControlRun);
                plcOtherControlThread.Start();
            }
            if (plcForkCommandControlThread == null)
            {
                plcForkCommandControlThread = new Thread(plcForkCommandControlRun);
                plcForkCommandControlThread.Start();
            }

            //plcOtherControlThread = new Thread(plcOtherControlRun);
            //plcForkCommandControlThread = new Thread(plcForkCommandControlRun);
            //plcOtherControlThread.Start();
            //plcForkCommandControlThread.Start();

            this.WriteCurrentDateTime();
            this.WriteIPCStatus(EnumIPCStatus.Initial);
            this.WriteAlarmWarningStatus(false, false);
            this.WriteIPCReady(true);
        }

        private int beforeDay = DateTime.Now.Day;
        private bool beforeEMOStatus;
        private Stopwatch swChargingTimeOut = new Stopwatch();
        private bool ChgStasOffDelayFlag = false;

        //處理非Fork command需要即時的邏輯
        public void plcOtherControlRun()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            Stopwatch sw500msClock = new Stopwatch();
            bool Clock1secWrite = false, b500msHigh = false, b500msLow = true;
            bool BatterysChargingTimeOut = false;
            bool CCModeStopVoltageChange = false;

            EnumAutoState IpcStatus;
            bool IpcStatusAutoIni = false;
            bool IpcStatusManualIni = true;

            uint WriteBatterySOCCount = 0;
            uint ChgStasOffDelayCount = 0;
            uint ChgStopCommandCount = 0;

            while (true)
            {
                try //20190730_Rudy 新增try catch
                {
                    //========Clock Working========
                    sw500msClock.Start();
                    if (sw500msClock.ElapsedMilliseconds >= 500)
                    {
                        b500msHigh ^= b500msLow;
                        b500msLow ^= b500msHigh;
                        b500msHigh ^= b500msLow;
                        Clock1secWrite = true;
                        sw500msClock.Stop();
                        sw500msClock.Reset();
                    }
                    if (b500msHigh && Clock1secWrite == true)
                    {
                        Clock1secWrite = false;
                        WriteBatterySOCCount++;
                        ChgStasOffDelayCount++;
                        ChgStopCommandCount++;
                    }
                    //========Clock Working========

                    //Write Battery SOC 
                    if (WriteBatterySOCCount >= 1)
                    {
                        WriteBatterySOCCount = 0;
                        WriteBatterySOC();
                    }

                    //Charging Off Delay
                    if (ChgStasOffDelayFlag)
                    {
                        if (ChgStasOffDelayCount >= this.APLCVehicle.Batterys.Charging_Off_Delay && this.APLCVehicle.Batterys.Charging)
                        {
                            ChgStasOffDelayCount = 0;
                            this.APLCVehicle.Batterys.Charging = aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean;//false
                            LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "Empty", "ChargeStatus Set Off"));
                        }
                    }
                    else
                    {
                        ChgStasOffDelayCount = 0;
                    }

                    //Batterys Charging Time Out
                    if (this.APLCVehicle.Batterys.Charging)
                    {
                        swChargingTimeOut.Start();
                        if (swChargingTimeOut.ElapsedMilliseconds > this.APLCVehicle.Batterys.Batterys_Charging_Time_Out)
                        {
                            BatterysChargingTimeOut = true;
                            if (aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean)
                            {
                                if (ChgStopCommandCount >= 10)
                                {
                                    this.setAlarm(Batterys_Charging_Time_Out);
                                    ChgStopCommandCount = 0;
                                    this.ChargeStopCommand();
                                }
                            }
                        }
                    }
                    else
                    {
                        ChgStopCommandCount = 10;
                        BatterysChargingTimeOut = false;
                        swChargingTimeOut.Stop();
                        swChargingTimeOut.Reset();
                    }

                    //CCModeStopVoltage
                    if ((this.APLCVehicle.Batterys.MeterVoltage >= this.APLCVehicle.Batterys.CCModeStopVoltage))
                    {
                        if (!CCModeStopVoltageChange)
                        {
                            CCModeStopVoltageChange = true;
                            this.APLCVehicle.Batterys.CcModeFlag = true;
                            this.ChargeStopCommand();
                        }
                    }
                    else
                    {
                        CCModeStopVoltageChange = false;
                    }

                    //Battery Logger
                    WriteBatLoggerCsv(ref swBatteryLogger, this.APLCVehicle.Batterys.Battery_Logger_Interval);

                    //EMO
                    APLCVehicle.PlcEmoStatus = DetectEMO();
                    if (APLCVehicle.PlcEmoStatus && (beforeEMOStatus != APLCVehicle.PlcEmoStatus))
                    {
                        beforeEMOStatus = APLCVehicle.PlcEmoStatus;
                        WriteForkCommandInfo(0, EnumForkCommand.None, "0", EnumStageDirection.None, true, 100);//待測試(看需要麼)
                        ClearExecutingForkCommand();
                    }

                    //heartbeat
                    swAlive.Start();
                    if (swAlive.ElapsedMilliseconds > 1000)
                    {
                        WriteIPCAlive();
                        swAlive.Stop();
                        swAlive.Reset();
                    }

                    //時間寫入(跨日)
                    int currDay = DateTime.Now.Day;
                    if (currDay != beforeDay)
                    {
                        beforeDay = currDay;
                        this.WriteCurrentDateTime();
                    }

                    //判斷Meter歸０完成
                    if (this.APLCVehicle.Batterys.SetMeterAhToZeroFlag)
                    {
                        //判斷歸０完成　=> 電表ＡＨ變成0, 所以原先值SetMeterAHToZeroAH　應該要反映到CCmode AH值
                        if (this.APLCVehicle.Batterys.MeterAh < 0.5 && this.APLCVehicle.Batterys.MeterAh > -0.5)
                        {
                            //this.APLCVehicle.APlcBatterys.CcModeAh = (0 - this.APLCVehicle.APlcBatterys.SetMeterAHToZeroAH) + this.APLCVehicle.APlcBatterys.CcModeAh;
                            this.APLCVehicle.Batterys.SetCcModeAh((0 - this.APLCVehicle.Batterys.SetMeterAhToZeroAh) + this.APLCVehicle.Batterys.CcModeAh, false);
                            this.APLCVehicle.Batterys.SetMeterAhToZeroFlag = false;
                        }
                        else
                        {
                            if (this.APLCVehicle.Batterys.SwBatteryAhSetToZero.ElapsedMilliseconds > this.APLCVehicle.Batterys.ResetAhTimeout)
                            {
                                //Raise Warning
                                this.APLCVehicle.Batterys.SetMeterAhToZeroFlag = false;

                            }
                        }

                    }

                    //Battery SOC => 寫在MeterAH發生變化事件裡
                    //這裡處理發事件
                    //OnBatteryPercentageChangeEvent
                    UInt16 currPercentage = Convert.ToUInt16(this.APLCVehicle.Batterys.Percentage);
                    if (currPercentage != this.beforeBatteryPercentageInteger)
                    {
                        this.beforeBatteryPercentageInteger = currPercentage;
                        OnBatteryPercentageChangeEvent?.Invoke(this, currPercentage);
                        LogPlcMsg(loggerAgent, new LogFormat("BatteryPercentage", "1", functionName, this.PlcId, "", $"Percentage = {currPercentage}"));
                    }

                    //IPC Auto、Manual 初始化
                    //方向燈控制
                    IpcStatus = Vehicle.Instance.AutoState;
                    if (IpcStatus == EnumAutoState.Auto)
                    {
                        if (IpcStatusAutoIni)
                        {
                            IpcStatusAutoIni = false;
                            //IpcModeToAutoInitial();
                            OnIpcAutoManualChangeEvent?.Invoke(this, EnumAutoState.Auto);
                        }
                        IpcAutoModeDirectionalLightControl();
                        IpcStatusManualIni = true;
                    }
                    else
                    {
                        if (IpcStatusManualIni)
                        {
                            IpcStatusManualIni = false;
                            //APLCVehicle.MoveFront = false;
                            //APLCVehicle.MoveBack = false;
                            //APLCVehicle.MoveLeft = false;
                            //APLCVehicle.MoveRight = false;
                            OnIpcAutoManualChangeEvent?.Invoke(this, EnumAutoState.Manual);
                            WriteDirectionalLight(EnumDirectionalLightType.None);
                        }
                        IpcAutoModeDirectionalLightControl();
                        IpcStatusAutoIni = true;
                    }

                    //Safety Action 判斷
                    //決定safety action
                    //Bumper -> BeamSensor
                    //EMO就算Safety Disable也要生效 => MoveControl會直接看EMO訊號,直接disable各軸    
                    if (APLCVehicle.SafetyDisable)
                    {
                        this.APLCVehicle.VehicleSafetyAction = EnumVehicleSafetyAction.Normal;
                    }
                    else
                    {
                        if (this.DetectBumper())
                        {
                            this.APLCVehicle.VehicleSafetyAction = EnumVehicleSafetyAction.Stop;
                        }
                        else
                        {
                            //Side Beam Sensor
                            Boolean frontSleepFlag = false;
                            Boolean backSleepFlag = false;
                            Boolean leftSleepFlag = false;
                            Boolean rightSleepFlag = false;

                            //順便決定beam sensor sleep的範圍
                            EnumVehicleSafetyAction result = EnumVehicleSafetyAction.Normal;
                            if (APLCVehicle.BeamSensorAutoSleep)
                            {
                                frontSleepFlag = (!APLCVehicle.MoveFront) || APLCVehicle.FrontBeamSensorDisable || APLCVehicle.SafetyDisable;
                                backSleepFlag = (!APLCVehicle.MoveBack) || APLCVehicle.FrontBeamSensorDisable || APLCVehicle.SafetyDisable;
                                leftSleepFlag = (!APLCVehicle.MoveLeft) || APLCVehicle.FrontBeamSensorDisable || APLCVehicle.SafetyDisable;
                                rightSleepFlag = (!APLCVehicle.MoveRight) || APLCVehicle.FrontBeamSensorDisable || APLCVehicle.SafetyDisable;

                                if (frontSleepFlag)
                                {
                                    this.SetBeamSensorSleepOn(EnumVehicleSide.Forward);
                                }
                                else
                                {
                                    this.SetBeamSensorSleepOff(EnumVehicleSide.Forward);
                                }

                                if (backSleepFlag)
                                {
                                    this.SetBeamSensorSleepOn(EnumVehicleSide.Backward);
                                }
                                else
                                {
                                    this.SetBeamSensorSleepOff(EnumVehicleSide.Backward);
                                }

                                if (leftSleepFlag)
                                {
                                    this.SetBeamSensorSleepOn(EnumVehicleSide.Left);
                                }
                                else
                                {
                                    this.SetBeamSensorSleepOff(EnumVehicleSide.Left);
                                }

                                if (rightSleepFlag)
                                {
                                    this.SetBeamSensorSleepOn(EnumVehicleSide.Right);
                                }
                                else
                                {
                                    this.SetBeamSensorSleepOff(EnumVehicleSide.Right);
                                }
                            }
                            else
                            {
                                this.SetBeamSensorSleepOff(EnumVehicleSide.Forward);
                                this.SetBeamSensorSleepOff(EnumVehicleSide.Backward);
                                this.SetBeamSensorSleepOff(EnumVehicleSide.Left);
                                this.SetBeamSensorSleepOff(EnumVehicleSide.Right);
                            }

                            if (APLCVehicle.MoveFront == true)
                            {
                                //前方
                                result = decideSafetyActionBySideBeamSensor(result, APLCVehicle.listFrontBeamSensor, this.APLCVehicle.FrontBeamSensorDisable);
                            }

                            if (APLCVehicle.MoveBack == true)
                            {
                                //後方
                                result = decideSafetyActionBySideBeamSensor(result, APLCVehicle.listBackBeamSensor, this.APLCVehicle.BackBeamSensorDisable);
                            }

                            if (APLCVehicle.MoveLeft == true)
                            {
                                //左方
                                result = decideSafetyActionBySideBeamSensor(result, APLCVehicle.listLeftBeamSensor, this.APLCVehicle.LeftBeamSensorDisable);
                            }

                            if (APLCVehicle.MoveRight == true)
                            {
                                //右方
                                result = decideSafetyActionBySideBeamSensor(result, APLCVehicle.listRightBeamSensor, this.APLCVehicle.RightBeamSensorDisable);
                            }

                            this.APLCVehicle.VehicleSafetyAction = result;
                        }
                    }


                }
                catch (Exception ex)
                {
                    //this.errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", ex.ToString());
                    LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", ex.ToString()));
                }
                System.Threading.Thread.Sleep(1);
            }
        }

        private EnumVehicleSafetyAction decideSafetyActionBySideBeamSensor(EnumVehicleSafetyAction initAction, List<PlcBeamSensor> listBeamSensor, Boolean SideBeamSensorDisable)
        {
            EnumVehicleSafetyAction result = initAction;
            //前方
            if (SideBeamSensorDisable == true) //圖資要求不看beam sensor
            {
                if (result == EnumVehicleSafetyAction.Normal)
                {
                    result = EnumVehicleSafetyAction.LowSpeed;
                }
                else
                {
                    //維持
                }
            }
            else
            {
                if (this.DetectSideBeamSensorDisable(listBeamSensor))
                {
                    //最多只能LowSpeed
                    //只須看near
                    if (this.DetectSideBeamSensorNear(listBeamSensor))
                    {
                        result = EnumVehicleSafetyAction.Stop;
                    }
                    else
                    {
                        result = EnumVehicleSafetyAction.LowSpeed;
                    }
                }
                else
                {
                    if (this.DetectSideBeamSensorNear(listBeamSensor))
                    {
                        result = EnumVehicleSafetyAction.Stop;
                    }
                    else
                    {
                        if (this.DetectSideBeamSensorFar(listBeamSensor))
                        {
                            result = EnumVehicleSafetyAction.LowSpeed;
                        }
                        else
                        {
                            //維持
                        }
                    }
                }
            }
            return result;
        }

        private Boolean DetectEMO()
        {
            Boolean emoFlag = false;
            foreach (PlcEmo aPlcEmo in APLCVehicle.listPlcEmo)
            {
                if (aPlcEmo.Disable == false && aPlcEmo.Signal == false)
                {
                    emoFlag = true;
                    break;
                }
            }

            return emoFlag;
        }

        private Boolean DetectBumper()
        {
            //Boolean bumperFlag = false;
            //foreach (PlcBumper aPLCBumper in APLCVehicle.listBumper)
            //{
            //    if (aPLCBumper.Disable == false && aPLCBumper.Signal == false)
            //    {
            //        bumperFlag = true;
            //        break;
            //    }
            //}
            //return bumperFlag;
            //<-- 2019//07/29 modify by ellison
            //改成看BumperAlarmStatus單一點位

            return this.APLCVehicle.BumperAlarmStatus;


        }

        private Boolean DetectBeamSensorNear()
        {
            Boolean result = false;
            //front
            if (this.APLCVehicle.MoveFront)
            {
                if (DetectSideBeamSensorNear(this.APLCVehicle.listFrontBeamSensor))
                {
                    result = true;
                    return result;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            //back
            if (this.APLCVehicle.MoveBack)
            {
                if (DetectSideBeamSensorNear(this.APLCVehicle.listBackBeamSensor))
                {
                    result = true;
                    return result;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            //left
            if (this.APLCVehicle.MoveLeft)
            {
                if (DetectSideBeamSensorNear(this.APLCVehicle.listLeftBeamSensor))
                {
                    result = true;
                    return result;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            //right
            if (this.APLCVehicle.MoveRight)
            {
                if (DetectSideBeamSensorNear(this.APLCVehicle.listRightBeamSensor))
                {
                    result = true;
                    return result;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        private Boolean DetectBeamSensorFar()
        {
            Boolean result = false;
            //front
            if (this.APLCVehicle.MoveFront)
            {
                if (DetectSideBeamSensorFar(this.APLCVehicle.listFrontBeamSensor))
                {
                    result = true;
                    return result;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            //back
            if (this.APLCVehicle.MoveBack)
            {
                if (DetectSideBeamSensorFar(this.APLCVehicle.listBackBeamSensor))
                {
                    result = true;
                    return result;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            //left
            if (this.APLCVehicle.MoveLeft)
            {
                if (DetectSideBeamSensorFar(this.APLCVehicle.listLeftBeamSensor))
                {
                    result = true;
                    return result;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            //right
            if (this.APLCVehicle.MoveRight)
            {
                if (DetectSideBeamSensorFar(this.APLCVehicle.listRightBeamSensor))
                {
                    result = true;
                    return result;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        private Boolean DetectSideBeamSensorNear(List<PlcBeamSensor> listBeamSensor)
        {
            Boolean nearFlag = false;
            foreach (PlcBeamSensor aPLCBeamSensor in listBeamSensor)
            {
                if (aPLCBeamSensor.Disable == false && aPLCBeamSensor.NearSignal == false)
                {
                    nearFlag = true;
                    break;
                }
            }

            return nearFlag;
        }

        private Boolean DetectSideBeamSensorFar(List<PlcBeamSensor> listBeamSensor)
        {
            Boolean farFlag = false;
            foreach (PlcBeamSensor aPLCBeamSensor in listBeamSensor)
            {
                if (aPLCBeamSensor.Disable == false && aPLCBeamSensor.FarSignal == false)
                {
                    farFlag = true;
                    break;
                }
            }

            return farFlag;
        }

        private Boolean DetectSideBeamSensorDisable(List<PlcBeamSensor> listBeamSensor)
        {
            Boolean disableFlag = false;
            foreach (PlcBeamSensor aPLCBeamSensor in listBeamSensor)
            {
                if (aPLCBeamSensor.Disable == true)
                {
                    disableFlag = true;
                    break;
                }
            }

            return disableFlag;
        }


        private void MCProtocol_OnDisConnectEvent(String message)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            this.boolConnectionState = false;
            plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "PLC is disconnected");
        }

        public void SetBeamSensorSleepOn(EnumVehicleSide aSide)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            List<PlcBeamSensor> listSideBeamSensor = null;
            switch (aSide)
            {
                case EnumVehicleSide.Forward:
                    listSideBeamSensor = this.APLCVehicle.listFrontBeamSensor;
                    break;
                case EnumVehicleSide.Backward:
                    listSideBeamSensor = this.APLCVehicle.listBackBeamSensor;
                    break;
                case EnumVehicleSide.Left:
                    listSideBeamSensor = this.APLCVehicle.listLeftBeamSensor;
                    break;
                case EnumVehicleSide.Right:
                    listSideBeamSensor = this.APLCVehicle.listRightBeamSensor;
                    break;
                case EnumVehicleSide.None:
                    //全開

                    break;
            }

            Boolean writeFlag = false;
            if (listSideBeamSensor == null)
            {
                //全開 安全起見 全開
                foreach (PlcBeamSensor aPLCBeamSensor in this.APLCVehicle.listFrontBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.APLCVehicle.listBackBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;
                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.APLCVehicle.listLeftBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.APLCVehicle.listRightBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                if (writeFlag)
                {
                    if (this.aMCProtocol.WritePLC())
                    {
                        //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "Set All Beam Sensor Sleep Off(Awake) Success");
                        LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "Set All Beam Sensor Sleep Off(Awake) Success");
                        LogPlcMsg(loggerAgent, logFormat);
                    }
                    else
                    {
                        //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "Set All Beam Sensor Sleep Off(Awake) Fail");
                        LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "Set All Beam Sensor Sleep Off(Awake) Fail");
                        LogPlcMsg(loggerAgent, logFormat);

                    }
                }


            }
            else
            {
                foreach (PlcBeamSensor aPLCBeamSensor in listSideBeamSensor)
                {

                    if (BeamSensorWriteSleep(aPLCBeamSensor, true)) writeFlag = true;

                }

                if (writeFlag)
                {
                    if (this.aMCProtocol.WritePLC())
                    {
                        //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to on success.");
                        //loggerAgent.LogMsg("PlcAgent", logFormat);
                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to on success."));
                    }
                    else
                    {
                        //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to on fail.");
                        //loggerAgent.LogMsg("PlcAgent", logFormat);
                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to on fail."));
                    }
                }
            }
        }

        private Boolean BeamSensorWriteSleep(PlcBeamSensor aPLCBeamSensor, Boolean flag)
        {
            Boolean writeFlag = false;
            if ((this.aMCProtocol.get_ItemByTag(aPLCBeamSensor.PlcReadSleepTagId).AsBoolean != flag) && (aPLCBeamSensor.BeforeWriteSleep != flag))
            {
                this.aMCProtocol.get_ItemByTag(aPLCBeamSensor.PlcWriteSleepTagId).AsBoolean = flag;
                aPLCBeamSensor.BeforeWriteSleep = flag;
                writeFlag = true;
            }
            return writeFlag;
        }

        public void SetBeamSensorSleepOff(EnumVehicleSide aSide)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            List<PlcBeamSensor> listSideBeamSensor = null;
            switch (aSide)
            {
                case EnumVehicleSide.Forward:
                    listSideBeamSensor = this.APLCVehicle.listFrontBeamSensor;
                    break;
                case EnumVehicleSide.Backward:
                    listSideBeamSensor = this.APLCVehicle.listBackBeamSensor;
                    break;
                case EnumVehicleSide.Left:
                    listSideBeamSensor = this.APLCVehicle.listLeftBeamSensor;
                    break;
                case EnumVehicleSide.Right:
                    listSideBeamSensor = this.APLCVehicle.listRightBeamSensor;
                    break;
                case EnumVehicleSide.None:
                    //全開

                    break;
            }

            Boolean writeFlag = false;
            if (listSideBeamSensor == null)
            {
                //全開 安全起見 全開
                foreach (PlcBeamSensor aPLCBeamSensor in this.APLCVehicle.listFrontBeamSensor)
                {

                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.APLCVehicle.listBackBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;
                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.APLCVehicle.listLeftBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.APLCVehicle.listRightBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                if (writeFlag)
                {
                    if (this.aMCProtocol.WritePLC())
                    {
                        //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set All Beam Sensor Sleep Off(Awake) Success");
                        //loggerAgent.LogMsg("PlcAgent", logFormat);
                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set All Beam Sensor Sleep Off(Awake) Success"));
                    }
                    else
                    {
                        //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set All Beam Sensor Sleep Off(Awake) Fail");
                        //loggerAgent.LogMsg("PlcAgent", logFormat);
                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set All Beam Sensor Sleep Off(Awake) Fail"));

                    }
                }


            }
            else
            {
                foreach (PlcBeamSensor aPLCBeamSensor in listSideBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;
                }

                if (writeFlag)
                {
                    if (this.aMCProtocol.WritePLC())
                    {
                        //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to off success.");
                        //loggerAgent.LogMsg("PlcAgent", logFormat);
                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to off success."));
                    }
                    else
                    {
                        //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to off fail.");
                        //loggerAgent.LogMsg("PlcAgent", logFormat);
                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to off fail."));
                    }
                }


            }

        }



        public void WriteCurrentDateTime()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            String datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
            //LogRecord(NLog.LogLevel.Info, "WriteDateTimeCalibrationReport", "DateTime: " + datetime);
            this.aMCProtocol.get_ItemByTag("YearMonth").AsHex = datetime.Substring(2, 4);
            this.aMCProtocol.get_ItemByTag("DayHour").AsHex = datetime.Substring(6, 4);
            this.aMCProtocol.get_ItemByTag("MinSec").AsHex = datetime.Substring(10, 4);
            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Current DateTime success, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Current DateTime success, "));
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Current DateTime fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Current DateTime fail, "));
            }


        }



        public Boolean WriteForkCommandInfo(ushort commandNo, EnumForkCommand enumForkCommand, String stageNo, EnumStageDirection direction, Boolean eQIF, UInt16 forkSpeed)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            this.aMCProtocol.get_ItemByTag("CommandNo").AsUInt16 = commandNo;

            this.aMCProtocol.get_ItemByTag("OperationType").AsUInt16 = Convert.ToUInt16(enumForkCommand);

            this.aMCProtocol.get_ItemByTag("StageNo").AsUInt16 = Convert.ToUInt16(stageNo);
            this.aMCProtocol.get_ItemByTag("StageDirection").AsUInt16 = Convert.ToUInt16(direction);
            this.aMCProtocol.get_ItemByTag("EQPIO").AsBoolean = eQIF;
            this.aMCProtocol.get_ItemByTag("ForkSpeed").AsUInt16 = forkSpeed;
            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command success, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command success, "));
                return true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command fail, "));
                return true;
            }


        }



        public Boolean WriteForkCommandActionBit(EnumForkCommandExecutionType aEnumForkCommandExecutionType, Boolean Onflag)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            switch (aEnumForkCommandExecutionType)
            {
                case EnumForkCommandExecutionType.Command_Read_Request:
                    this.aMCProtocol.get_ItemByTag("ReadCommandRequest").AsBoolean = Onflag;
                    break;
                case EnumForkCommandExecutionType.Command_Start:
                    this.aMCProtocol.get_ItemByTag("CommandStart").AsBoolean = Onflag;
                    break;
                case EnumForkCommandExecutionType.Command_Finish_Ack:
                    this.aMCProtocol.get_ItemByTag("CommandFinishAck").AsBoolean = Onflag;
                    break;
            }

            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Execute Fork Command(" + aEnumForkCommandExecutionType.ToString() + " = " + Convert.ToString(Onflag) + ") success, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Execute Fork Command(" + aEnumForkCommandExecutionType.ToString() + " = " + Convert.ToString(Onflag) + ") success, "));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Execute Fork Command(" + aEnumForkCommandExecutionType.ToString() + " = " + Convert.ToString(Onflag) + ") fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Execute Fork Command(" + aEnumForkCommandExecutionType.ToString() + " = " + Convert.ToString(Onflag) + ") fail, "));

            }
            return result;

        }

        public Boolean ChargeStartCommand(EnumChargeDirection aEnumChargeDirection)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            switch (aEnumChargeDirection)
            {
                case EnumChargeDirection.Left:
                    this.aMCProtocol.get_ItemByTag("LeftChargeRequest").AsBoolean = true;
                    break;
                case EnumChargeDirection.Right:
                    this.aMCProtocol.get_ItemByTag("RightChargeRequest").AsBoolean = true;
                    break;

            }

            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Start Command success, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Start Command success, "));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Start Command fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Start Command fail, "));

            }
            return result;

        }

        public Boolean ChargeStopCommand()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("LeftChargeRequest").AsBoolean = false;
            this.aMCProtocol.get_ItemByTag("RightChargeRequest").AsBoolean = false;

            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Stop Command success, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Stop Command success, "));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Stop Command fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Stop Command fail, "));

            }
            return result;

        }

        public Boolean WriteAGVCOnline(Boolean OnlineFlag)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("AGVCOnline").AsBoolean = OnlineFlag;

            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write AGVCOnline = " + Convert.ToString(OnlineFlag) + " success");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write AGVCOnline = " + Convert.ToString(OnlineFlag) + " success"));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write AGVCOnline = " + Convert.ToString(OnlineFlag) + " fail");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write AGVCOnline = " + Convert.ToString(OnlineFlag) + " fail"));
            }
            return result;
        }
        public bool SetAlarmWarningReportAllReset()
        {
            bool result = false;
            List<string> liArrayWord = new List<string>() { "10" };
            List<string> liWarningWord = new List<string>() { "20" };
            string stLevelr = "";
            try
            {
                string strItem = "";

                foreach (string word in liArrayWord)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        strItem = $"Alarm_{word}_{i}";
                        this.aMCProtocol.get_ItemByTag(strItem).AsBoolean = false;
                    }
                }

                foreach (string word in liWarningWord)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        strItem = $"Warning_{word}_{i}";
                        this.aMCProtocol.get_ItemByTag(strItem).AsBoolean = false;
                    }
                }
                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"SetAlarmWarningReportAllReset Success"));
                }
                else
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"SetAlarmWarningReportAllReset fail"));

                if (WriteAlarmWarningStatus(false, false))
                    result = true;

            }
            catch (Exception ex)
            {
                LogPlcMsg(loggerAgent, new LogFormat("Error", "1", GetFunName(), this.PlcId, "", ex.ToString()));
            }
            liArrayWord.Clear();
            liWarningWord.Clear();
            return result;
        }
        public bool WriteAlarmWarningReport(EnumAlarmLevel level, ushort word, ushort bit, bool status)
        {
            bool result = false;
            string stLevelr = "";
            switch (level)
            {
                case EnumAlarmLevel.Alarm:
                    stLevelr = "Alarm";
                    break;
                case EnumAlarmLevel.Warn:
                    stLevelr = "Warning";
                    break;
                default:
                    stLevelr = "Warning";
                    break;
            }
            string strItem = $"{stLevelr}_{word.ToString()}_{bit.ToString()}";
            try
            {
                if (this.aMCProtocol.get_ItemByTag(strItem) != null)
                {
                    this.aMCProtocol.get_ItemByTag(strItem).AsBoolean = status;
                    if (this.aMCProtocol.WritePLC())
                    {
                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"Write IPC Alarm Warning Report ({stLevelr} => {word.ToString()}.{bit.ToString()}) = {status.ToString()} Success"));
                        result = true;
                    }
                    else
                        LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"Write IPC Alarm Warning Report ({stLevelr} => {word.ToString()}.{bit.ToString()}) = {status.ToString()} fail"));

                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LogPlcMsg(loggerAgent, new LogFormat("Error", "1", GetFunName(), this.PlcId, "", ex.ToString()));
            }
            return result;
        }
        public Boolean WriteAlarmWarningStatus(Boolean alarmStatus, Boolean warningStatus)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("IPCAlarmStatus").AsBoolean = alarmStatus;
            this.aMCProtocol.get_ItemByTag("IPCWarningStatus").AsBoolean = warningStatus;
            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlarmStatus = " + Convert.ToString(alarmStatus) + ", IPCWarningStatus = " + Convert.ToString(warningStatus) + " success");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlarmStatus = " + Convert.ToString(alarmStatus) + ", IPCWarningStatus = " + Convert.ToString(warningStatus) + " success"));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlarmStatus = " + Convert.ToString(alarmStatus) + ", IPCWarningStatus = " + Convert.ToString(warningStatus) + " fail");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlarmStatus = " + Convert.ToString(alarmStatus) + ", IPCWarningStatus = " + Convert.ToString(warningStatus) + " fail"));
            }
            return result;
        }

        public Boolean WriteIPCReady(Boolean readyStatus)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("IPCReady").AsBoolean = readyStatus;
            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCReady = " + Convert.ToString(readyStatus) + " success");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCReady = " + Convert.ToString(readyStatus) + " success"));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCReady = " + Convert.ToString(readyStatus) + " fail");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCReady = " + Convert.ToString(readyStatus) + " fail"));
            }
            return result;
        }

        public Boolean WriteIPCStatus(EnumIPCStatus aEnumIPCStatus)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("IPCStatus").AsUInt16 = Convert.ToUInt16(aEnumIPCStatus);

            this.APLCVehicle.IPcStatus = Convert.ToUInt16(aEnumIPCStatus);

            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCStatus = " + Convert.ToString(aEnumIPCStatus) + " success");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCStatus = " + Convert.ToString(aEnumIPCStatus) + " success"));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCStatus = " + Convert.ToString(aEnumIPCStatus) + " fail");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCStatus = " + Convert.ToString(aEnumIPCStatus) + " fail"));
            }
            return result;
        }
        private void WriteBatterySOC()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            this.aMCProtocol.get_ItemByTag("BatterySOC").AsUInt16 = Convert.ToUInt16(this.APLCVehicle.Batterys.Percentage);

            if (this.aMCProtocol.WritePLC())
            { }
            else
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write Battery SOC = " + Convert.ToString(this.APLCVehicle.Batterys.Percentage) + " fail"));
            }
        }

        private UInt16 IPCAliveCounter = 1;
        public void WriteIPCAlive()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            //heart beat量大不記log
            this.aMCProtocol.get_ItemByTag("IPCAlive").AsUInt16 = IPCAliveCounter;

            //this.aMCProtocol.WritePLC();
            if (this.aMCProtocol.WritePLC())
            {
            }
            else
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlive = " + Convert.ToString(IPCAliveCounter) + " fail"));
            }

            IPCAliveCounter++;
            IPCAliveCounter = Convert.ToUInt16(Convert.ToInt32(IPCAliveCounter) % 65536);
            //System.Threading.Thread.Sleep(1000);
        }

        private UInt16 eqActionIndex = 0;

        public void WritePLCAlarmReset()
        {
            Task.Run(() =>
            {
                string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;

                this.aMCProtocol.get_ItemByTag("EquipementAction").AsUInt16 = 10;

                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementAction(PLC Alarm Reset) = " + Convert.ToString(10) + " Success"));
                }
                else
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementAction(PLC Alarm Reset) = " + Convert.ToString(10) + " Fail"));
                }


                System.Threading.Thread.Sleep(1000);

                eqActionIndex++;
                eqActionIndex = Convert.ToUInt16(Convert.ToInt32(eqActionIndex) % 65536);
                this.aMCProtocol.get_ItemByTag("EquipementActionIndex").AsUInt16 = eqActionIndex;

                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementActionIndex = " + Convert.ToString(eqActionIndex) + " Success"));
                }
                else
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementActionIndex = " + Convert.ToString(eqActionIndex) + " Fail"));
                }

            });


        }

        public void WritePLCBuzzserStop()
        {
            Task.Run(() =>
            {
                string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;
                this.aMCProtocol.get_ItemByTag("EquipementAction").AsUInt16 = 11;

                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementAction(PLC Alarm Reset) = " + Convert.ToString(11) + " Success"));
                }
                else
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementAction(PLC Alarm Reset) = " + Convert.ToString(11) + " Fail"));
                }


                System.Threading.Thread.Sleep(1000);
                eqActionIndex++;
                eqActionIndex = Convert.ToUInt16(Convert.ToInt32(eqActionIndex) % 65536);
                this.aMCProtocol.get_ItemByTag("EquipementActionIndex").AsUInt16 = eqActionIndex;

                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementActionIndex = " + Convert.ToString(eqActionIndex) + " Success"));
                }
                else
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementActionIndex = " + Convert.ToString(eqActionIndex) + " Fail"));
                }
            });
        }

        public void SetMeterAHToZero()
        {
            this.APLCVehicle.Batterys.SetMeterAhToZeroFlag = true;
            //this.this.APLCVehicle.PLCBatterys.SetMeterAHToZeroAH = this.this.APLCVehicle.PLCBatterys.MeterAH;
            Task.Run(() =>
            {
                string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;
                this.aMCProtocol.get_ItemByTag("MeterAHToZero").AsBoolean = true;
                //this.aMCProtocol.WritePLC();
                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Meter AH To Zero Success, "));
                }
                else
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Meter AH To Zero fail, "));
                }


                System.Threading.Thread.Sleep(1000);
                this.aMCProtocol.get_ItemByTag("MeterAHToZero").AsBoolean = false;
                //this.aMCProtocol.WritePLC();
                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Meter AH To Zero Success, "));
                }
                else
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Meter AH To Zero fail, "));
                }
            });
        }

        public Boolean WriteVehicleDirection(Boolean spinLeft, Boolean spinRight, Boolean TraverseLeft, Boolean TrverseRight, Boolean SteeringFL, Boolean SteeringFR, Boolean SteeringBL, Boolean SteeringBR, Boolean Forward, Boolean Backward)
        {

            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("SpinTurn(L)").AsBoolean = spinLeft;
            this.aMCProtocol.get_ItemByTag("SpinTurn(R)").AsBoolean = spinRight;
            this.aMCProtocol.get_ItemByTag("Traverse(L)").AsBoolean = TraverseLeft;
            this.aMCProtocol.get_ItemByTag("Traverse(R)").AsBoolean = TrverseRight;
            this.aMCProtocol.get_ItemByTag("Steering(FL)").AsBoolean = SteeringFL;
            this.aMCProtocol.get_ItemByTag("Steering(FR)").AsBoolean = SteeringFR;
            this.aMCProtocol.get_ItemByTag("Steering(BL)").AsBoolean = SteeringBL;
            this.aMCProtocol.get_ItemByTag("Steering(BR)").AsBoolean = SteeringBR;
            this.aMCProtocol.get_ItemByTag("Forward").AsBoolean = Forward;
            this.aMCProtocol.get_ItemByTag("Backward").AsBoolean = Backward;

            if (this.aMCProtocol.WritePLC())
            {
                //本function成功不記log
                //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PLCId, "", "Charge Start Command success, ");
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "WriteVehicleDirection fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "WriteVehicleDirection fail, "));
            }
            return result;

        }

        private void TaskRunForkCommandStart()
        {

        }

        private Stopwatch swAlive = new Stopwatch();
        //private ForkCommand executingForkCommand = null;
        public Boolean IsForkCommandExist()
        {
            if (this.APLCVehicle.Robot.ExecutingCommand == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public String getErrorReason()
        {
            try
            {
                PlcForkCommand aForkcommand = this.APLCVehicle.Robot.ExecutingCommand; //避免executingForkCommand同時被clear
                if (aForkcommand != null)
                {
                    return aForkcommand.Reason;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {

                return "";
            }

        }

        public Boolean AddForkComand(PlcForkCommand aForkCommand)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            if (this.APLCVehicle.Robot.ExecutingCommand == null)
            {
                if (aForkCommand.ForkCommandState == EnumForkCommandState.Queue)
                {
                    this.APLCVehicle.Robot.ExecutingCommand = aForkCommand;
                    return true;
                }
                else
                {
                    //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, aForkCommand.ForkCommandState = " + Convert.ToString(this.APLCVehicle.APlcRobot.ExecutingCommand.ForkCommandState) + ", is not Queue.");
                    //loggerAgent.LogMsg("PlcAgent", logFormat);
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, aForkCommand.ForkCommandState = " + Convert.ToString(this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState) + ", is not Queue."));
                    return false;
                }
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, executingForkCommand is not null.");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, executingForkCommand is not null."));
                return false;
            }
        }

        private void setAlarm(int alarmCode)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            if (this.aAlarmHandler != null)
            {
                this.aAlarmHandler.SetAlarm(alarmCode);
            }
            else
            {

            }

        }

        private void resetAlarm(int alarmCode)
        {
            if (this.aAlarmHandler != null)
            {
                this.aAlarmHandler.ResetAlarm(alarmCode);
            }
            else
            {

            }

        }

        public void triggerCassetteIDReader(ref string CassetteID)
        {
            string strCassetteID = "ERROR";
            this.aCassetteIDReader.ReadBarcode(ref strCassetteID); //成功或失敗都要發ReadFinishEvent,外部用CassetteID來區別成功或失敗
            this.APLCVehicle.CassetteId = strCassetteID;
            CassetteID = strCassetteID;
            OnCassetteIDReadFinishEvent?.Invoke(this, strCassetteID);


            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;
            LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "TriggerCassetteIDReader CassetteID = " + Convert.ToString(APLCVehicle.CassetteId) + " Success"));

        }


        public void ClearExecutingForkCommand()
        {
            clearExecutingForkCommandFlag = true;
        }

        public void plcForkCommandControlRun()
        {

            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            while (true)
            {
                try
                {
                    //Ready
                    this.APLCVehicle.Robot.ForkReady = this.aMCProtocol.get_ItemByTag("ForkReady").AsBoolean;

                    //Fork Command
                    if (clearExecutingForkCommandFlag)
                    {
                        clearExecutingForkCommandFlag = false;
                        this.APLCVehicle.Robot.ExecutingCommand = null;
                    }

                    if (this.APLCVehicle.Robot.ExecutingCommand != null)
                    {
                        Stopwatch sw = new Stopwatch();
                        switch (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState)
                        {
                            case EnumForkCommandState.Queue:

                                if (this.IsFakeForking)
                                {
                                    System.Threading.Thread.Sleep(3000);
                                    if (this.APLCVehicle.Batterys.Charging == true)
                                    {
                                        System.Threading.Thread.Sleep(27000);
                                    }

                                    if (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType == EnumForkCommand.Load)
                                    {
                                        this.APLCVehicle.Loading = true;
                                        this.APLCVehicle.CassetteId = "CA0070";
                                        OnCassetteIDReadFinishEvent?.Invoke(this, this.APLCVehicle.CassetteId);
                                    }
                                    else if (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType == EnumForkCommand.Unload)
                                    {
                                        this.APLCVehicle.CassetteId = "";
                                        this.APLCVehicle.Loading = false;

                                    }
                                    else
                                    {

                                    }

                                    eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                    OnForkCommandFinishEvent?.Invoke(this, eventForkCommand);
                                    clearExecutingForkCommandFlag = true;

                                    break;
                                }

                                //送出指令                              
                                if (this.aMCProtocol.get_ItemByTag("ForkReady").AsBoolean && this.aMCProtocol.get_ItemByTag("ForkBusy").AsBoolean == false)
                                {
                                    this.APLCVehicle.Robot.ExecutingCommand.Reason = "";
                                    this.WriteForkCommandInfo(Convert.ToUInt16(this.APLCVehicle.Robot.ExecutingCommand.CommandNo), this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType, this.APLCVehicle.Robot.ExecutingCommand.StageNo, this.APLCVehicle.Robot.ExecutingCommand.Direction, this.APLCVehicle.Robot.ExecutingCommand.IsEqPio, this.APLCVehicle.Robot.ExecutingCommand.ForkSpeed);

                                    System.Threading.Thread.Sleep(500);
                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, true);
                                    sw.Reset();
                                    sw.Start();
                                    while (true)
                                    {
                                        if (this.aMCProtocol.get_ItemByTag("ForkCommandOK").AsBoolean)
                                        {
                                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                            System.Threading.Thread.Sleep(1000);
                                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, true);
                                            break;
                                        }
                                        else if (this.aMCProtocol.get_ItemByTag("ForkCommandNG").AsBoolean)
                                        {
                                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                            this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                            this.APLCVehicle.Robot.ExecutingCommand.Reason = "ForkCommandNG";
                                            //Raise Alarm
                                            //this.aAlarmHandler.SetAlarm(270001);
                                            //this.setAlarm(270001);
                                            this.setAlarm(Fork_Command_Format_NG);
                                            eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                            OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);

                                            break;
                                        }
                                        else
                                        {
                                            if (sw.ElapsedMilliseconds < ForkCommandReadTimeout)
                                            {
                                                if (clearExecutingForkCommandFlag)
                                                {
                                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "ForkCommandReadTimeout clearExecutingForkCommandFlag = true"));
                                                    break;
                                                }
                                                System.Threading.Thread.Sleep(20);
                                            }
                                            else
                                            {
                                                //read time out
                                                //this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                                //System.Threading.Thread.Sleep(1000);
                                                //this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, true);
                                                this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                                this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                                this.APLCVehicle.Robot.ExecutingCommand.Reason = "Fork Command Read timeout";
                                                //this.aAlarmHandler.SetAlarm(270002);
                                                //this.setAlarm(270002);
                                                this.setAlarm(Fork_Command_Read_timeout);
                                                eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                                OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                                //Raise Alarm
                                                //return;
                                                break;
                                            }

                                        }


                                    }
                                    sw.Stop();

                                    if (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState == EnumForkCommandState.Error)
                                    {
                                        break;
                                    }

                                    sw.Reset();
                                    sw.Start();
                                    while (true)
                                    {
                                        if (this.aMCProtocol.get_ItemByTag("ForkBusy").AsBoolean == false)
                                        {
                                            if (sw.ElapsedMilliseconds < this.ForkCommandBusyTimeout)
                                            {
                                                if (clearExecutingForkCommandFlag)
                                                {
                                                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "ForkCommandBusyTimeout clearExecutingForkCommandFlag = true"));
                                                    break;
                                                }
                                                System.Threading.Thread.Sleep(20);
                                            }
                                            else
                                            {
                                                this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, false);
                                                this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                                this.APLCVehicle.Robot.ExecutingCommand.Reason = "ForkNotBusy timeout";
                                                //this.aAlarmHandler.SetAlarm(270003);
                                                //this.setAlarm(270003);
                                                this.setAlarm(Fork_Not_Busy_timeout);
                                                eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                                OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    sw.Stop();
                                    if (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState == EnumForkCommandState.Error)
                                    {
                                        break;
                                    }
                                    this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Executing;
                                    eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                    OnForkCommandExecutingEvent?.Invoke(this, eventForkCommand);
                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, false);
                                    System.Threading.Thread.Sleep(1000);

                                }
                                else
                                {
                                    this.APLCVehicle.Robot.ExecutingCommand.Reason = "ForkReady or ForkBusy is not correct";
                                }

                                break;
                            case EnumForkCommandState.Executing:
                                sw.Reset();
                                sw.Start();
                                while (this.aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean == false)
                                {
                                    if (sw.ElapsedMilliseconds < this.ForkCommandMovingTimeout)
                                    {
                                        if (clearExecutingForkCommandFlag)
                                        {
                                            LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "ForkCommandMovingTimeout clearExecutingForkCommandFlag = true"));
                                            break;
                                        }
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    else
                                    {

                                        //executingForkCommand.ForkCommandState = EnumForkCommandState.Error;
                                        this.APLCVehicle.Robot.ExecutingCommand.Reason = "ForkCommand Moving Timeout";
                                        //Raise Alarm?Warning?   
                                        //this.aAlarmHandler.SetAlarm(270004);
                                        //this.setAlarm(270004);
                                        this.setAlarm(Fork_Command_Executing_timeout);
                                        eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                        OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                        break;
                                    }
                                }
                                sw.Stop();
                                sw.Reset();
                                if (this.aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean)
                                {
                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, true);
                                    System.Threading.Thread.Sleep(1000);
                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, false);
                                    this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Finish;
                                }
                                else
                                {

                                }

                                break;
                            case EnumForkCommandState.Finish:
                                //OnForkCommandFinishEvent?.Invoke(this, executingForkCommand);
                                if (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType == EnumForkCommand.Load)
                                {
                                    //要讀完CasetteID才算完成
                                    if (this.IsNeedReadCassetteID)
                                    {
                                        String cassetteID = "ERROR";
                                        this.aCassetteIDReader.ReadBarcode(ref cassetteID); //成功或失敗都要發ReadFinishEvent,外部用CassetteID來區別成功或失敗
                                        this.APLCVehicle.CassetteId = cassetteID;
                                        OnCassetteIDReadFinishEvent?.Invoke(this, cassetteID);
                                    }
                                    else
                                    {

                                    }
                                }
                                else if (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType == EnumForkCommand.Unload)
                                {
                                    //this.APLCVehicle.CassetteId = "";
                                }
                                else
                                {

                                }

                                ////ForkCommand aForkCommand = executingForkCommand;
                                eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                OnForkCommandFinishEvent?.Invoke(this, eventForkCommand);
                                clearExecutingForkCommandFlag = true;
                                break;
                        }
                        bComdIsNullReqForkComdOK = true;
                        bComdIsNullReqComdFinishAck = true;
                    }
                    else
                    {
                        if (this.aMCProtocol.get_ItemByTag("ForkCommandOK").AsBoolean && bComdIsNullReqForkComdOK)
                        {
                            bComdIsNullReqForkComdOK = false;
                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                            LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"ForkCommandOK ExecutingCommand Is NULL,Command_Read_Request False"));
                        }
                        if (this.aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean && bComdIsNullReqComdFinishAck)
                        {
                            bComdIsNullReqComdFinishAck = false;
                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, true);
                            LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"ForkCommandFinish ExecutingCommand Is NULL,Command_Finish_Ack True"));
                            System.Threading.Thread.Sleep(1000);
                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, false);
                            LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"ForkCommandFinish ExecutingCommand Is NULL,Command_Finish_Ack False"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    //this.errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", ex.ToString());

                    //LogFormat logFormat = new LogFormat("Error", "1", functionName, this.PlcId, "", ex.ToString());
                    LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", ex.ToString()));
                }
                System.Threading.Thread.Sleep(5);
            }
        }
        private bool bComdIsNullReqForkComdOK = false, bComdIsNullReqComdFinishAck = false;

        public Boolean SetVehicleChargeOn()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("VehicleCharge").AsBoolean = true;
            if (this.aMCProtocol.WritePLC())
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleCharge On = " + Convert.ToString(true) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleCharge On = " + Convert.ToString(true) + " fail"));
            }
            return result;
        }
        public Boolean SetVehicleChargeOff()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("VehicleCharge").AsBoolean = false;
            if (this.aMCProtocol.WritePLC())
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleCharge Off = " + Convert.ToString(false) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleCharge Off = " + Convert.ToString(false) + " fail"));
            }
            return result;
        }

        public Boolean SetVehicleInPositionOn()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("VehicleInPosition").AsBoolean = true;
            if (this.aMCProtocol.WritePLC())
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleInPosition On = " + Convert.ToString(true) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleInPosition On = " + Convert.ToString(true) + " fail"));
            }
            return result;
        }
        public Boolean SetVehicleInPositionOff()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("VehicleInPosition").AsBoolean = false;
            if (this.aMCProtocol.WritePLC())
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleInPosition Off = " + Convert.ToString(false) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleInPosition Off = " + Convert.ToString(false) + " fail"));
            }
            return result;
        }

        public Boolean SetForcELMOServoOffOn()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            Boolean result = false;
            this.aMCProtocol.get_ItemByTag("Force_ELMO_Servo_Off").AsBoolean = true;
            if (this.aMCProtocol.WritePLC())
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Forc ELMO Servo Off On = " + Convert.ToString(true) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Forc ELMO Servo Off On = " + Convert.ToString(true) + " fail"));
            }
            return result;
        }

        private string strlogMsg = "";
        private const int LogMsgMaxLength = 65535;//65535
        public string logMsg
        {
            get
            {
                return strlogMsg;
            }
        }
        private void LogPlcMsg(LoggerAgent clsLoggerAgent, LogFormat clsLogFormat)
        {
            strlogMsg = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "\t" + clsLogFormat.Message + "\r\n" + strlogMsg;
            if (strlogMsg.Length > LogMsgMaxLength) strlogMsg = strlogMsg.Substring(0, LogMsgMaxLength);
            clsLoggerAgent.LogMsg(clsLogFormat.Category, clsLogFormat);
        }

        public bool WritePlcConfigToXML(Dictionary<string, string> dicSetValue, string file_address = "PLC_Config.xml")
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            bool searchStatus = false;
            bool result = false;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file_address);
                var rootNode = doc.DocumentElement;
                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    XmlElement element = (XmlElement)item;
                    foreach (XmlNode childItem in element.ChildNodes)
                    {
                        if (dicSetValue.ContainsKey(childItem.Name))
                        {
                            childItem.InnerText = dicSetValue[childItem.Name];
                            searchStatus = true;
                        }
                    }
                }
                if (searchStatus)
                {
                    doc.Save(file_address);
                    ReadXml("PLC_Config.xml");
                    //ReadXml();
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "Write Plc Config To XML Success"));
                    result = true;
                }
                else
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "Write Plc Config To XML Fail"));
                }
            }
            catch (Exception ex)
            {
                LogPlcMsg(loggerAgent, new LogFormat("Error", "1", functionName, this.PlcId, "", ex.ToString()));
            }
            return result;
        }
        //public void WritePlcConfigToXML()
        //{
        //    XmlHandler xmlHandler = new XmlHandler();
        //    xmlHandler.WriteXml(plcConfig, "Plc.xml");
        //}

        private bool LogVehicleMove_SpinL, LogVehicleMove_SpinR;
        private bool LogVehicleMove_SteerFR, LogVehicleMove_SteerFL;
        private bool LogVehicleMove_RTraverse, LogVehicleMove_LTraverse;
        private bool LogVehicleMove_SteerBR, LogVehicleMove_SteerBL;
        private bool LogVehicleMove_Forward, LogVehicleMove_Backward;

        private void WriteDirectionalLight(EnumDirectionalLightType atype, bool status = false)
        {
            string SpinTurnL = "SpinTurn(L)", SpinTurnR = "SpinTurn(R)";
            string SteerFR = "Steering(FR)", SteerFL = "Steering(FL)";
            string TraverseR = "Traverse(R)", TraverseL = "Traverse(L)";
            string SteerBR = "Steering(BR)", SteerBL = "Steering(BL)";

            string VehicleMove = "VehicleMove", Forward = "Forward", Backward = "Backward";

            bool bWriteStatus = false;
            switch (atype)
            {
                case EnumDirectionalLightType.None:
                    if (LogVehicleMove_SpinL != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SpinTurnL).AsBoolean = status;
                        LogVehicleMove_SpinL = status;
                        bWriteStatus = true;
                    }
                    if (LogVehicleMove_SpinR != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SpinTurnR).AsBoolean = status;
                        LogVehicleMove_SpinR = status;
                        bWriteStatus = true;
                    }
                    if (LogVehicleMove_SteerFR != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SteerFR).AsBoolean = status;
                        LogVehicleMove_SteerFR = status;
                        bWriteStatus = true;
                    }
                    if (LogVehicleMove_SteerFL != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SteerFL).AsBoolean = status;
                        LogVehicleMove_SteerFL = status;
                        bWriteStatus = true;
                    }
                    if (LogVehicleMove_RTraverse != status)
                    {
                        this.aMCProtocol.get_ItemByTag(TraverseR).AsBoolean = status;
                        LogVehicleMove_RTraverse = status;
                        bWriteStatus = true;
                    }
                    if (LogVehicleMove_LTraverse != status)
                    {
                        this.aMCProtocol.get_ItemByTag(TraverseL).AsBoolean = status;
                        LogVehicleMove_LTraverse = status;
                        bWriteStatus = true;
                    }
                    if (LogVehicleMove_SteerBR != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SteerBR).AsBoolean = status;
                        LogVehicleMove_SteerBR = status;
                        bWriteStatus = true;
                    }
                    if (LogVehicleMove_SteerBL != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SteerBL).AsBoolean = status;
                        LogVehicleMove_SteerBL = status;
                        bWriteStatus = true;
                    }
                    if (LogVehicleMove_Backward != status)
                    {
                        this.aMCProtocol.get_ItemByTag(Backward).AsBoolean = status;
                        LogVehicleMove_Backward = status;
                        bWriteStatus = true;
                    }
                    if (LogVehicleMove_Forward != status)
                    {
                        this.aMCProtocol.get_ItemByTag(Forward).AsBoolean = status;
                        LogVehicleMove_Forward = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.SpinL:
                    if (LogVehicleMove_SpinL != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SpinTurnL).AsBoolean = status;
                        LogVehicleMove_SpinL = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.SpinR:
                    if (LogVehicleMove_SpinR != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SpinTurnR).AsBoolean = status;
                        LogVehicleMove_SpinR = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.SteerFR:
                    if (LogVehicleMove_SteerFR != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SteerFR).AsBoolean = status;
                        LogVehicleMove_SteerFR = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.SteerFL:
                    if (LogVehicleMove_SteerFL != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SteerFL).AsBoolean = status;
                        LogVehicleMove_SteerFL = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.RTraverse:
                    if (LogVehicleMove_RTraverse != status)
                    {
                        this.aMCProtocol.get_ItemByTag(TraverseR).AsBoolean = status;
                        LogVehicleMove_RTraverse = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.LTraverse:
                    if (LogVehicleMove_LTraverse != status)
                    {
                        this.aMCProtocol.get_ItemByTag(TraverseL).AsBoolean = status;
                        LogVehicleMove_LTraverse = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.SteerBR:
                    if (LogVehicleMove_SteerBR != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SteerBR).AsBoolean = status;
                        LogVehicleMove_SteerBR = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.SteerBL:
                    if (LogVehicleMove_SteerBL != status)
                    {
                        this.aMCProtocol.get_ItemByTag(SteerBL).AsBoolean = status;
                        LogVehicleMove_SteerBL = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.Backward:
                    if (LogVehicleMove_Backward != status)
                    {
                        this.aMCProtocol.get_ItemByTag(Backward).AsBoolean = status;
                        LogVehicleMove_Backward = status;
                        bWriteStatus = true;
                    }
                    break;
                case EnumDirectionalLightType.Forward:
                    if (LogVehicleMove_Forward != status)
                    {
                        this.aMCProtocol.get_ItemByTag(Forward).AsBoolean = status;
                        LogVehicleMove_Forward = status;
                        bWriteStatus = true;
                    }
                    break;

                default:
                    break;
            }
            if (bWriteStatus)
            {
                bool MoveFlag = LogVehicleMove_Forward ||
                                LogVehicleMove_Backward ||
                                LogVehicleMove_SpinL ||
                                LogVehicleMove_SpinR ||
                                LogVehicleMove_SteerFR ||
                                LogVehicleMove_SteerFL ||
                                LogVehicleMove_RTraverse ||
                                LogVehicleMove_LTraverse ||
                                LogVehicleMove_SteerBR ||
                                LogVehicleMove_SteerBL;

                if (MoveFlag)
                    this.aMCProtocol.get_ItemByTag(VehicleMove).AsBoolean = true;
                else
                    this.aMCProtocol.get_ItemByTag(VehicleMove).AsBoolean = false;

                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", "Write Directional Light (" + atype.ToString() + ") = " + Convert.ToString(status) + " Success"));
                }
                else
                {
                    LogPlcMsg(loggerAgent, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", "Write Directional Light (" + atype.ToString() + ") = " + Convert.ToString(status) + " fail"));
                }
            }
        }
        private void IpcModeToAutoInitial()
        {
            APLCVehicle.SafetyDisable = false;
            APLCVehicle.BeamSensorAutoSleep = false;

            APLCVehicle.FrontBeamSensorDisable = false;
            APLCVehicle.BackBeamSensorDisable = false;
            APLCVehicle.LeftBeamSensorDisable = false;
            APLCVehicle.RightBeamSensorDisable = false;

            foreach (PlcBeamSensor beamSensor in APLCVehicle.listFrontBeamSensor)
            {
                beamSensor.Disable = false;
            }
            foreach (PlcBeamSensor beamSensor in APLCVehicle.listBackBeamSensor)
            {
                beamSensor.Disable = false;
            }
            foreach (PlcBeamSensor beamSensor in APLCVehicle.listLeftBeamSensor)
            {
                beamSensor.Disable = false;
            }
            foreach (PlcBeamSensor beamSensor in APLCVehicle.listRightBeamSensor)
            {
                beamSensor.Disable = false;
            }

            //Robot
            ClearExecutingForkCommand();
            //WriteForkCommandInfo(0, EnumForkCommand.None, "0", EnumStageDirection.None, true, 100);
        }
        private void IpcAutoModeDirectionalLightControl()
        {
            if (APLCVehicle.Forward)
            {
                WriteDirectionalLight(EnumDirectionalLightType.Forward, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.Forward);
            }
            if (APLCVehicle.Backward)
            {
                WriteDirectionalLight(EnumDirectionalLightType.Backward, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.Backward);
            }
            if (APLCVehicle.SpinTurnLeft)
            {
                WriteDirectionalLight(EnumDirectionalLightType.SpinL, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.SpinL);
            }
            if (APLCVehicle.SpinTurnRight)
            {
                WriteDirectionalLight(EnumDirectionalLightType.SpinR, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.SpinR);
            }
            if (APLCVehicle.TraverseLeft)
            {
                WriteDirectionalLight(EnumDirectionalLightType.LTraverse, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.LTraverse);
            }
            if (APLCVehicle.TraverseRight)
            {
                WriteDirectionalLight(EnumDirectionalLightType.RTraverse, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.RTraverse);
            }
            if (APLCVehicle.SteeringBL)
            {
                WriteDirectionalLight(EnumDirectionalLightType.SteerBL, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.SteerBL);
            }
            if (APLCVehicle.SteeringBR)
            {
                WriteDirectionalLight(EnumDirectionalLightType.SteerBR, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.SteerBR);
            }

            if (APLCVehicle.SteeringFR)
            {
                WriteDirectionalLight(EnumDirectionalLightType.SteerFR, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.SteerFR);
            }
            if (APLCVehicle.SteeringFL)
            {
                WriteDirectionalLight(EnumDirectionalLightType.SteerFL, true);
            }
            else
            {
                WriteDirectionalLight(EnumDirectionalLightType.SteerFL);
            }

        }


    }
}
