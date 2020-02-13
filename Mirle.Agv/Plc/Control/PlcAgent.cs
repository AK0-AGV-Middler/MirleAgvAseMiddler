//#define  DebugTestThread 
//#define  DebugTest 

using ClsMCProtocol;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Mirle.Tools;

namespace Mirle.Agv.Controller
{
    public class PlcAgent
    {
        #region "const"
        private const int Fork_Command_Format_NG = 270001;
        private const int Fork_Command_Read_timeout = 270002;
        private const int Fork_Not_Busy_timeout = 270003;
        private const int Fork_Command_Executing_timeout = 270004;
        private const int plcBatterys_Charging_Time_Out = 270005;

        private const int ModeChangeError = 270006;
        private const int Fork_Home_Flag_Waiting_timeout = 270007;
        #endregion

        private MCProtocol aMCProtocol;
        private CassetteIDReader aCassetteIDReader = new CassetteIDReader();

        public string PlcId { get; set; } = "AGVPLC";
        public string Ip { get; set; } = "192.168.3.39";
        public string Port { get; set; } = "6000";
        public string LocalIp { get; set; } = "192.168.3.100";
        public string LocalPort { get; set; } = "3001";

        public Int64 ForkCommandReadTimeout { get; set; } = 850000;
        public Int64 ForkCommandBusyTimeout { get; set; } = 850000;
        public Int64 ForkCommandMovingTimeout { get; set; } = 850000;

        private MirleLogger mirleLogger = MirleLogger.Instance;

        private Logger plcAgentLogger;
        private Logger errLogger;
        private Logger portPIOLogger;
        private Logger chargerPIOLogger;
        private Logger plcJogPitchLogger;

        private Logger BatteryLogger;
        //private Logger BatteryPercentage;
        public Boolean IsFirstMeterAhGet { get; set; } = false;

        public PlcVehicle APLCVehicle;
        public VehicleCorrectValue AVehicleCorrectValue = new VehicleCorrectValue();

        public EnumVehicleSafetyAction VehicleSafetyAction_Old { get; set; } = EnumVehicleSafetyAction.Normal;

        private PlcForkCommand eventForkCommand; //發event前 先把executing commnad reference先放過來, 避免外部exevnt處理時發生null問題
        private bool clearExecutingForkCommandFlag = false;

        private Thread plcOtherControlThread = null;
        private Thread plcForkCommandControlThread = null;
        private Thread plcOperationThread = null;

        private Thread TestThread = null;

        private UInt16 beforeBatteryPercentageInteger = 0;
        private UInt32 alarmReadIndex = 0;

        private JogPitchForm jogPitchForm = null;
        private MainForm mainForm = null;
        private List<string> alarmCodeRecordList = new List<string>();
        
        public event EventHandler<PlcForkCommand> OnForkCommandExecutingEvent;
        public event EventHandler<PlcForkCommand> OnForkCommandFinishEvent;
        public event EventHandler<PlcForkCommand> OnForkCommandErrorEvent;
        public event EventHandler<UInt16> OnBatteryPercentageChangeEvent;
        public event EventHandler<string> OnCassetteIDReadFinishEvent;
        public event EventHandler<PlcForkCommand> OnForkCommandInterlockErrorEvent;

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
        private int nowErrorCode = 0;

        public void SendVehicleDecreaseSpeedFlag()
        {

        }

        private AlarmHandler aAlarmHandler = null;

        public void SetOutSideObj(MainForm mainForm)
        {
            this.mainForm = mainForm;
            this.jogPitchForm = mainForm.GetJogPitchForm();
            //this.mainForm = mainForm;  , ref MainForm mainForm
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
                MaxMeterCurrent = this.APLCVehicle.plcBatterys.MeterCurrent;
                MaxMeterVoltage = this.APLCVehicle.plcBatterys.MeterVoltage;
                MaxMeterWatt = this.APLCVehicle.plcBatterys.MeterWatt;
                MaxMeterAh = this.APLCVehicle.plcBatterys.MeterAh;
                MaxCcModeAh = this.APLCVehicle.plcBatterys.CcModeAh;
                //MaxCcModeCounter = this.APLCVehicle.plcBatterys.CcModeCounter;
                //MaxFullChargeIndex = this.APLCVehicle.plcBatterys.FullChargeIndex;
                MaxFBatteryTemperature = this.APLCVehicle.plcBatterys.FBatteryTemperature;
                MaxBBatteryTemperature = this.APLCVehicle.plcBatterys.BBatteryTemperature;

                MinMeterCurrent = this.APLCVehicle.plcBatterys.MeterCurrent;
                MinMeterVoltage = this.APLCVehicle.plcBatterys.MeterVoltage;
                MinMeterWatt = this.APLCVehicle.plcBatterys.MeterWatt;
                MinMeterAh = this.APLCVehicle.plcBatterys.MeterAh;
                MinCcModeAh = this.APLCVehicle.plcBatterys.CcModeAh;
                //MinCcModeCounter = this.APLCVehicle.plcBatterys.CcModeCounter;
                //MinFullChargeIndex = this.APLCVehicle.plcBatterys.FullChargeIndex;
                MinFBatteryTemperature = this.APLCVehicle.plcBatterys.FBatteryTemperature;
                MinBBatteryTemperature = this.APLCVehicle.plcBatterys.BBatteryTemperature;

                BatteryMaxMinIni = false;
            }

            ValueCompare<double>(this.APLCVehicle.plcBatterys.MeterCurrent, ref MaxMeterCurrent);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.MeterVoltage, ref MaxMeterVoltage);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.MeterWatt, ref MaxMeterWatt);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.MeterAh, ref MaxMeterAh);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.CcModeAh, ref MaxCcModeAh);
            //ValueCompare<ushort>(this.APLCVehicle.plcBatterys.CcModeCounter, ref MaxCcModeCounter);
            //ValueCompare<ushort>(this.APLCVehicle.plcBatterys.FullChargeIndex, ref MaxFullChargeIndex);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.FBatteryTemperature, ref MaxFBatteryTemperature);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.BBatteryTemperature, ref MaxBBatteryTemperature);

            ValueCompare<double>(this.APLCVehicle.plcBatterys.MeterCurrent, ref MinMeterCurrent, 1);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.MeterVoltage, ref MinMeterVoltage, 1);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.MeterWatt, ref MinMeterWatt, 1);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.MeterAh, ref MinMeterAh, 1);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.CcModeAh, ref MinCcModeAh, 1);
            //ValueCompare<ushort>(this.APLCVehicle.plcBatterys.CcModeCounter, ref MinCcModeCounter, 1);
            //ValueCompare<ushort>(this.APLCVehicle.plcBatterys.FullChargeIndex, ref MinFullChargeIndex, 1);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.FBatteryTemperature, ref MinFBatteryTemperature, 1);
            ValueCompare<double>(this.APLCVehicle.plcBatterys.BBatteryTemperature, ref MinBBatteryTemperature, 1);




            if (sw.ElapsedMilliseconds > ms)
            {
                string csvLog = "", Separator = ",";
                DateTime now;
                BatteryMaxMinIni = true;

                now = DateTime.Now;
                csvLog = now.ToString("HH:mm:ss.ff");

                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Percentage.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.MeterCurrent.ToString();
                csvLog = csvLog + Separator + this.MaxMeterCurrent.ToString();
                csvLog = csvLog + Separator + this.MinMeterCurrent.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.MeterVoltage.ToString();
                csvLog = csvLog + Separator + this.MaxMeterVoltage.ToString();
                csvLog = csvLog + Separator + this.MinMeterVoltage.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.MeterWatt.ToString();
                csvLog = csvLog + Separator + this.MaxMeterWatt.ToString();
                csvLog = csvLog + Separator + this.MinMeterWatt.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.MeterAh.ToString();
                csvLog = csvLog + Separator + this.MaxMeterAh.ToString();
                csvLog = csvLog + Separator + this.MinMeterAh.ToString();

                //csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Percentage.ToString();
                //csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.AhWorkingRange.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.CcModeAh.ToString();
                csvLog = csvLog + Separator + this.MaxCcModeAh.ToString();
                csvLog = csvLog + Separator + this.MinCcModeAh.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.CcModeCounter.ToString();
                //csvLog = csvLog + Separator + this.MaxCcModeCounter.ToString();
                //csvLog = csvLog + Separator + this.MinCcModeCounter.ToString();

                //csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.MaxResetAhCcounter.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.FullChargeIndex.ToString();
                //csvLog = csvLog + Separator + this.MaxFullChargeIndex.ToString();
                //csvLog = csvLog + Separator + this.MinFullChargeIndex.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.FBatteryTemperature.ToString();
                csvLog = csvLog + Separator + this.MaxFBatteryTemperature.ToString();
                csvLog = csvLog + Separator + this.MinFBatteryTemperature.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.BBatteryTemperature.ToString();
                csvLog = csvLog + Separator + this.MaxBBatteryTemperature.ToString();
                csvLog = csvLog + Separator + this.MinBBatteryTemperature.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Charging.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.BatteryType.ToString();
                for (int i = 1; i <= APLCVehicle.BatteryCellNum; i++)
                {
                    csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.BatteryCells[i].Voltage.ToString();
                }
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Temperature_sensor_number.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Temperature_1_MOSFET.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Temperature_2_Cell.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Temperature_3_MCU.ToString();

                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.BatteryCurrent.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Packet_Voltage.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Remain_Capacity.ToString();
                csvLog = csvLog + Separator + this.APLCVehicle.plcBatterys.Design_Capacity.ToString();

                try
                {
                    csvLog = csvLog + Separator + this.APLCVehicle.RobotHome;
                    csvLog = csvLog + Separator + mainForm.mainFlowHandler.GetCurTransferStep().GetType().ToString();
                    csvLog = csvLog + Separator + Vehicle.Instance.VehicleLocation.LastAddress.Id;
                    csvLog = csvLog + Separator + this.aMCProtocol.get_ItemByTag("PLCBigDataTT01").AsUInt16;
                }
                catch (Exception ex)
                {

                }

                BatteryLogger.LogString(csvLog);

                sw.Stop();
                sw.Reset();
            }
        }
        public PlcAgent(MCProtocol objMCProtocol, AlarmHandler objAlarmHandler)
        {
            OpenTestThread();
            TestFun();

            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            APLCVehicle = (PlcVehicle)Vehicle.Instance.TheVehicleIntegrateStatus;
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
            LogPlcMsg(mirleLogger, logFormat);
        }

        private void SetupLoggers()
        {
            plcAgentLogger = mirleLogger.GetLooger("PlcAgent");
            errLogger = mirleLogger.GetLooger("Error");
            portPIOLogger = mirleLogger.GetLooger("PortPIO");
            chargerPIOLogger = mirleLogger.GetLooger("ChargerPIO");
            plcJogPitchLogger = mirleLogger.GetLooger("PlcJogPitch");

            BatteryLogger = MirleLogger.Instance.GetLooger("BatteryCSV");
            //BatteryPercentage = LoggerAgent.Instance.GetLooger("BatteryPercentage");
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
                            this.APLCVehicle.plcBatterys.AhWorkingRange = Convert.ToDouble(childItem.InnerText);
                            break;
                        case "Ah_Reset_CCmode_Counter":
                            this.APLCVehicle.plcBatterys.MaxResetAhCcounter = Convert.ToUInt16(childItem.InnerText);
                            break;
                        case "Ah_Reset_Timeout":
                            this.APLCVehicle.plcBatterys.ResetAhTimeout = Convert.ToUInt32(childItem.InnerText) * 1000;
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
                            this.APLCVehicle.plcBatterys.PortAutoChargeLowSoc = Convert.ToDouble(childItem.InnerText);
                            break;
                        case "Port_AutoCharge_High_SOC":
                            this.APLCVehicle.plcBatterys.PortAutoChargeHighSoc = Convert.ToDouble(childItem.InnerText);
                            break;

                        case "Battery_Logger_Interval":
                            this.APLCVehicle.plcBatterys.Battery_Logger_Interval = Convert.ToUInt32(Convert.ToDouble(childItem.InnerText) * 1000);
                            break;

                        case "Batterys_Charging_Time_Out":// min
                            this.APLCVehicle.plcBatterys.Batterys_Charging_Time_Out = Convert.ToUInt32(childItem.InnerText) * 60000;
                            break;
                        case "Charging_Off_Delay":
                            this.APLCVehicle.plcBatterys.Charging_Off_Delay = Convert.ToUInt32(childItem.InnerText);
                            break;
                        case "CCMode_Stop_Voltage":
                            this.APLCVehicle.plcBatterys.CCModeStopVoltage = Convert.ToDouble(childItem.InnerText);
                            break;
                        case "Battery_Cell_Low_Voltage":
                            this.APLCVehicle.plcBatterys.Battery_Cell_Low_Voltage = Convert.ToDouble(childItem.InnerText);
                            break;
                        case "Beam_Sensor_Disable_Normal_Speed":
                            if (childItem.InnerText.ToLower() != "true")
                            {
                                this.APLCVehicle.BeamSensorDisableNormalSpeed = false;
                            }
                            else
                            {
                                this.APLCVehicle.BeamSensorDisableNormalSpeed = true;
                            }
                            break;
                    }

                }
            }
        }

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
                            LogPlcMsg(mirleLogger, new LogFormat("PortPIO", "9", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean));
                        }
                        else if (oColParam.Item(i).DataName.ToString().EndsWith("_CPIO"))
                        {
                            //this.chargerPIOLogger.SaveLogFile("PortPIO", "9", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean);
                            LogPlcMsg(mirleLogger, new LogFormat("PortPIO", "9", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean));
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
                                    //errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith Near or Far");
                                    LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith Near or Far"));
                                }
                            }
                            else
                            {
                                //errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
                                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object"));
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
                                    //errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");
                                    LogFormat logFormat = new LogFormat("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");
                                    LogPlcMsg(mirleLogger, logFormat);
                                }
                            }
                            else
                            {
                                //errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
                                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object"));
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
                                    //errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");
                                    LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep"));
                                }
                            }
                            else
                            {
                                //errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
                                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object"));
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
                                //errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBumper object");
                                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBumper object"));
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
                                //errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCEMO object");
                                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCEMO object"));
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("Cell_"))
                        {
                            string[] strarry = oColParam.Item(i).DataName.ToString().Split('_');
                            if (oColParam.Item(i).DataName.ToString() == "Cell_number")
                            {
                                this.APLCVehicle.plcBatterys.Cell_number = oColParam.Item(i).AsUInt16;
                            }
                            else
                            {
                                if (strarry[2] == "Voltage")
                                {
                                    this.APLCVehicle.plcBatterys.BatteryCells[Convert.ToInt16(strarry[1])].Voltage = this.DECToDouble(oColParam.Item(i).AsUInt16, 1, 3);
                                }
                            }
                        }
                        else
                        {
                            switch (oColParam.Item(i).DataName.ToString())
                            {
                                case "BumpAlarmStatus":
                                    this.APLCVehicle.BumperAlarmStatus = aMCProtocol.get_ItemByTag("BumpAlarmStatus").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"BumpAlarmStatus = { this.APLCVehicle.BumperAlarmStatus }"));
                                    break;

                                case "BatteryGotech":
                                    if (oColParam.Item(i).AsBoolean)
                                    {
                                        this.APLCVehicle.plcBatterys.BatteryType = EnumBatteryType.Gotech;
                                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Battery = Gotech"));
                                    }
                                    else
                                    {

                                    }
                                    break;
                                case "BatteryYinda":
                                    if (oColParam.Item(i).AsBoolean)
                                    {
                                        this.APLCVehicle.plcBatterys.BatteryType = EnumBatteryType.Yinda;
                                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Battery = Yinda"));
                                    }
                                    else
                                    {

                                    }
                                    break;
                                case "MeterVoltage":
                                    this.APLCVehicle.plcBatterys.MeterVoltage = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);

                                    switch (this.APLCVehicle.plcBatterys.BatteryType)
                                    {
                                        case EnumBatteryType.Gotech:
                                            if (this.APLCVehicle.plcBatterys.MeterVoltage < this.APLCVehicle.plcBatterys.GotechMinVol + 1.5)
                                            {
                                                if (this.APLCVehicle.plcBatterys.bVoltageAbnormal == false)
                                                {
                                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Trigger low Voltage event."));
                                                    setSOC(10);
                                                    this.APLCVehicle.plcBatterys.bVoltageAbnormal = true;
                                                }
                                            }
                                            else
                                            {
                                                this.APLCVehicle.plcBatterys.bVoltageAbnormal = false;
                                            }
                                            break;
                                        case EnumBatteryType.Yinda:
                                            if (this.APLCVehicle.plcBatterys.MeterVoltage < this.APLCVehicle.plcBatterys.YindaMinVol + 1.5)
                                            {
                                                if (this.APLCVehicle.plcBatterys.bVoltageAbnormal == false)
                                                {
                                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Trigger low Voltage event."));
                                                    setSOC(10);
                                                    this.APLCVehicle.plcBatterys.bVoltageAbnormal = true;
                                                }
                                            }
                                            else
                                            {
                                                this.APLCVehicle.plcBatterys.bVoltageAbnormal = false;
                                            }
                                            break;

                                        default:
                                            break;

                                    }

                                    break;
                                case "MeterCurrent":
                                    this.APLCVehicle.plcBatterys.MeterCurrent = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "MeterWatt":
                                    this.APLCVehicle.plcBatterys.MeterWatt = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "MeterWattHour":
                                    this.APLCVehicle.plcBatterys.MeterWattHour = this.DECToDouble(oColParam.Item(i).AsUInt32, 2);
                                    break;
                                case "MeterAH":
                                    this.APLCVehicle.plcBatterys.MeterAh = this.DECToDouble(oColParam.Item(i).AsUInt32, 2);
                                    if (this.APLCVehicle.plcBatterys.MeterAh != 0.0)
                                    {
                                        if (IsFirstMeterAhGet == false)
                                        {
                                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "Set IsFirstMeterAhGet = true"));
                                            IsFirstMeterAhGet = true;
                                        }
                                        else
                                        {
                                            //第一次讀到非0電表值 過後   都會跑到這邊
                                        }
                                    }
                                    else
                                    {
                                        if (IsFirstMeterAhGet == false)
                                        {
                                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "MeterAH data change but new value is 0.0"));
                                        }
                                    }
                                    break;
                                case "GotechMaxVol":
                                    this.APLCVehicle.plcBatterys.GotechMaxVol = this.DECToDouble(aMCProtocol.get_ItemByTag("GotechMaxVol").AsUInt16, 1, 2);
                                    break;
                                case "GotechMinVol":
                                    this.APLCVehicle.plcBatterys.GotechMinVol = this.DECToDouble(aMCProtocol.get_ItemByTag("GotechMinVol").AsUInt16, 1, 2);
                                    break;
                                case "YindaMaxVol":
                                    this.APLCVehicle.plcBatterys.YindaMaxVol = this.DECToDouble(aMCProtocol.get_ItemByTag("YindaMaxVol").AsUInt16, 1, 2);
                                    break;
                                case "YindaMinVol":
                                    this.APLCVehicle.plcBatterys.YindaMinVol = this.DECToDouble(aMCProtocol.get_ItemByTag("YindaMinVol").AsUInt16, 1, 2);
                                    break;
                                case "FullChargeIndex":
                                    //if (this.APLCVehicle.plcBatterys.FullChargeIndex == 0)
                                    //{
                                    //    //AGV斷電重開
                                    //    //this.APLCVehicle.APlcplcBatterys.CcModeAh = this.APLCVehicle.APlcplcBatterys.CcModeAh - this.APLCVehicle.APlcplcBatterys.AhWorkingRange;
                                    //    this.APLCVehicle.plcBatterys.SetCcModeAh(this.APLCVehicle.plcBatterys.CcModeAh - this.APLCVehicle.plcBatterys.AhWorkingRange, false);
                                    //}
                                    //else
                                    //{
                                    this.APLCVehicle.plcBatterys.CcModeFlag = true;
                                    //CC Mode達到                                
                                    this.APLCVehicle.plcBatterys.FullChargeIndex = oColParam.Item(i).AsUInt16;

                                    //}
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"FullChargeIndex = {this.APLCVehicle.plcBatterys.FullChargeIndex}"));

                                    break;
                                case "HomeStatus":
                                    this.APLCVehicle.RobotHome = aMCProtocol.get_ItemByTag("HomeStatus").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"HomeStatus = {this.APLCVehicle.RobotHome}"));
                                    break;
                                case "ChargeStatus":
                                    if (aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean)
                                    {
                                        this.APLCVehicle.plcBatterys.Charging = aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean;
                                        ChgStasOffDelayFlag = false;
                                    }
                                    else
                                    {
                                        ChgStasOffDelayFlag = true;
                                        ccModeAHSet();
                                    }
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ChargeStatus = {this.APLCVehicle.plcBatterys.Charging}"));

                                    //this.APLCVehicle.plcBatterys.Charging = aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean;
                                    //if (!this.APLCVehicle.plcBatterys.Charging)
                                    //{
                                    //    ccModeAHSet();
                                    //}
                                    break;

                                case "FBatteryTemperature":
                                    //this.APLCVehicle.plcBatterys.FBatteryTemperature = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
                                    {
                                        Int64 value = oColParam.Item(i).AsUInt16;
                                        if (value >= 32768)
                                            this.APLCVehicle.plcBatterys.FBatteryTemperature = Convert.ToDouble(value - 65536);
                                        else
                                            this.APLCVehicle.plcBatterys.FBatteryTemperature = Convert.ToDouble(value);
                                    }
                                    break;
                                case "BBatteryTemperature":
                                    //this.APLCVehicle.plcBatterys.BBatteryTemperature = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
                                    {
                                        Int64 value = oColParam.Item(i).AsUInt16;
                                        if (value >= 32768)
                                            this.APLCVehicle.plcBatterys.FBatteryTemperature = Convert.ToDouble(value - 65536);
                                        else
                                            this.APLCVehicle.plcBatterys.FBatteryTemperature = Convert.ToDouble(value);
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
                                                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"AlarmCode = {AlarmCode + iAlarmOffset}"));

                                                //不區分alarm/warning => alarm CSV裡區分
                                                if (this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmEvent").AsUInt16 == 1)
                                                {
                                                    //set
                                                    //this.setAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16));
                                                    this.setAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16) + iAlarmOffset);
                                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"AlarmEvent = {1}"));
                                                }
                                                else if (this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmEvent").AsUInt16 == 2)
                                                {
                                                    //clear
                                                    //this.resetAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16));
                                                    this.resetAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16) + iAlarmOffset);
                                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"AlarmEvent = {2}"));
                                                }
                                                else
                                                {

                                                }
                                                alarmCodeRecordList.Add(AlarmCode.ToString());
                                            }
                                        }

                                        alarmReadIndex++;
                                        alarmReadIndex = alarmReadIndex % 65535 + 1;
                                        this.aMCProtocol.get_ItemByTag("IPCAlarmWarningIndex").AsUInt16 = (ushort)alarmReadIndex;

                                        if (this.aMCProtocol.WritePLC())
                                        {
                                            //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC success");
                                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC success"));
                                        }
                                        else
                                        {
                                            //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC fail");
                                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC fail"));
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        //this.errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", ex.ToString());
                                        LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", ex.ToString()));
                                    }

                                    //Console.Out.Write("alarm");
                                    break;
                                case "EquipementActionIndex":
                                    this.eqActionIndex = oColParam.Item(i).AsUInt16;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"EquipementActionIndex = {this.eqActionIndex}"));

                                    break;
                                case "ForkReady":
                                    this.APLCVehicle.Robot.ForkReady = aMCProtocol.get_ItemByTag("ForkReady").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkReady = {this.APLCVehicle.Robot.ForkReady}"));

                                    break;
                                case "ForkBusy":
                                    this.APLCVehicle.Robot.ForkBusy = aMCProtocol.get_ItemByTag("ForkBusy").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkBusy = {this.APLCVehicle.Robot.ForkBusy}"));

                                    break;
                                case "ForkCommandNG":
                                    this.APLCVehicle.Robot.ForkNG = aMCProtocol.get_ItemByTag("ForkCommandNG").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkCommandNG = {this.APLCVehicle.Robot.ForkNG}"));

                                    //紀錄 Fork alignment value 
                                    if (this.APLCVehicle.Robot.ForkNG == true)
                                    {
                                        RecordForkAndVeihcleCorrectValue("ForkCommandNG");
                                        ClearForkAndVehicleCorrectValue();
                                    }

                                    break;
                                case "ForkCommandFinish":
                                    this.APLCVehicle.Robot.ForkFinish = aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkCommandFinish = {this.APLCVehicle.Robot.ForkFinish}"));

                                    //紀錄 Fork alignment value 
                                    if (this.APLCVehicle.Robot.ForkFinish == true)
                                    {
                                        RecordForkAndVeihcleCorrectValue("ForkCommandFinish");
                                        ClearForkAndVehicleCorrectValue();
                                    }

                                    break;
                                case "ForkPrePioFail":
                                    this.APLCVehicle.Robot.ForkPrePioFail = aMCProtocol.get_ItemByTag("ForkPrePioFail").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkPrePioFail = {this.APLCVehicle.Robot.ForkPrePioFail}"));

                                    if (Vehicle.Instance.AutoState == EnumAutoState.Auto)
                                    {
                                        if (this.APLCVehicle.Robot.ForkPrePioFail == true)
                                        {
                                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "Prepare to call Interlock Event."));

                                            Task.Run(() =>
                                            {
                                                Thread.Sleep(10000);
                                                this.WritePLCAlarmReset();
                                                Thread.Sleep(5000);
                                                this.WritePLCAlarmReset();
                                                Thread.Sleep(1000);
                                                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "Invoke ForkCommandInterlockErrorEvent trigger. Alarm code :" + String.Join(",", alarmCodeRecordList)));
                                                eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                                OnForkCommandInterlockErrorEvent?.Invoke(this, eventForkCommand);
                                            });
                                        }
                                    }
                                    break;

                                case "ForkBusyFail":
                                    this.APLCVehicle.Robot.ForkBusyFail = aMCProtocol.get_ItemByTag("ForkBusyFail").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkBusyFail = {this.APLCVehicle.Robot.ForkBusyFail}"));
                                    break;

                                case "ForkPostPioFail":
                                    this.APLCVehicle.Robot.ForkPostPioFail = aMCProtocol.get_ItemByTag("ForkPostPioFail").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"ForkPostPioFail = {this.APLCVehicle.Robot.ForkPostPioFail}"));
                                    break;

                                case "StageLoading":
                                    this.APLCVehicle.CarrierSlot.Loading = aMCProtocol.get_ItemByTag("StageLoading").AsBoolean;
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"StageLoading = {this.APLCVehicle.CarrierSlot.Loading}"));
                                    break;

                                case "Temperature_sensor_number":
                                    this.APLCVehicle.plcBatterys.Temperature_sensor_number = aMCProtocol.get_ItemByTag("Temperature_sensor_number").AsUInt16;
                                    break;
                                case "Temperature_1_MOSFET":
                                    this.APLCVehicle.plcBatterys.Temperature_1_MOSFET = this.DECToDouble(aMCProtocol.get_ItemByTag("Temperature_1_MOSFET").AsUInt16, 1, 1);
                                    break;
                                case "Temperature_2_Cell":
                                    this.APLCVehicle.plcBatterys.Temperature_2_Cell = this.DECToDouble(aMCProtocol.get_ItemByTag("Temperature_2_Cell").AsUInt16, 1, 1);
                                    break;
                                case "Temperature_3_MCU":
                                    this.APLCVehicle.plcBatterys.Temperature_3_MCU = this.DECToDouble(aMCProtocol.get_ItemByTag("Temperature_3_MCU").AsUInt16, 1, 1);
                                    break;
                                case "BatteryCurrent":
                                    this.APLCVehicle.plcBatterys.BatteryCurrent = this.DECToDouble(aMCProtocol.get_ItemByTag("BatteryCurrent").AsUInt16, 1, 1);
                                    break;
                                case "Packet_Voltage":
                                    this.APLCVehicle.plcBatterys.Packet_Voltage = this.DECToDouble(aMCProtocol.get_ItemByTag("Packet_Voltage").AsUInt16, 2, 3);
                                    break;
                                case "Remain_Capacity":
                                    this.APLCVehicle.plcBatterys.Remain_Capacity = aMCProtocol.get_ItemByTag("Remain_Capacity").AsUInt16;
                                    break;
                                case "Design_Capacity":
                                    this.APLCVehicle.plcBatterys.Design_Capacity = aMCProtocol.get_ItemByTag("Design_Capacity").AsUInt16;
                                    break;

                                case "BatterySOC_Form_Plc":
                                    this.APLCVehicle.plcBatterys.BatterySOCFormPlc = aMCProtocol.get_ItemByTag("BatterySOC_Form_Plc").AsUInt16;
                                    break;
                                case "BatterySOH_Form_Plc":
                                    this.APLCVehicle.plcBatterys.BatterySOHFormPlc = aMCProtocol.get_ItemByTag("BatterySOH_Form_Plc").AsUInt16;
                                    break;
                                case "DoubleStoreSensor(L)":
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"DoubleStoreSensor(L) = {aMCProtocol.get_ItemByTag("DoubleStoreSensor(L)").AsBoolean}"));
                                    break;
                                case "DoubleStoreSensor(R)":
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"DoubleStoreSensor(R) = {aMCProtocol.get_ItemByTag("DoubleStoreSensor(R)").AsBoolean}"));
                                    break;
                                case "ForkAlignmentResultP":
                                    this.APLCVehicle.Robot.ForkAlignmentP = this.DECToDouble(aMCProtocol.get_ItemByTag("ForkAlignmentResultP").AsUInt32, 2, 2);
                                    break;
                                case "ForkAlignmentResultY":
                                    this.APLCVehicle.Robot.ForkAlignmentY = this.DECToDouble(aMCProtocol.get_ItemByTag("ForkAlignmentResultY").AsUInt32, 2, 2);
                                    break;
                                case "ForkAlignmentResultPhi":
                                    this.APLCVehicle.Robot.ForkAlignmentPhi = this.DECToDouble(aMCProtocol.get_ItemByTag("ForkAlignmentResultPhi").AsUInt32, 2, 2);
                                    break;
                                case "ForkAlignmentResultF":
                                    this.APLCVehicle.Robot.ForkAlignmentF = this.DECToDouble(aMCProtocol.get_ItemByTag("ForkAlignmentResultF").AsUInt32, 2, 2);
                                    break;
                                case "ForkAlignmentResultCode":
                                    this.APLCVehicle.Robot.ForkAlignmentCode = aMCProtocol.get_ItemByTag("ForkAlignmentResultCode").AsUInt16;
                                    break;
                                case "ForkAlignmentResultC":
                                    this.APLCVehicle.Robot.ForkAlignmentC = this.DECToDouble(aMCProtocol.get_ItemByTag("ForkAlignmentResultC").AsUInt32, 2, 2);
                                    break;

                                case "ForkAlignmentResultB":
                                    this.APLCVehicle.Robot.ForkAlignmentB = this.DECToDouble(aMCProtocol.get_ItemByTag("ForkAlignmentResultB").AsUInt32, 2, 2);
                                    break;

                                case "PLCOperateMode":
                                    this.APLCVehicle.JogOperation.ModeOperation = aMCProtocol.get_ItemByTag("PLCOperateMode").AsBoolean;
                                    break;

                                case "PLCVehicleMode":
                                    this.APLCVehicle.JogOperation.ModeVehicle = (EnumJogVehicleMode)aMCProtocol.get_ItemByTag("PLCVehicleMode").AsUInt16;
                                    break;

                                case "PLCJogElmoFunction":
                                    this.APLCVehicle.JogOperation.JogElmoFunction = (EnumJogElmoFunction)aMCProtocol.get_ItemByTag("PLCJogElmoFunction").AsUInt16;
                                    break;
                                case "PLCJogRunMode":
                                    this.APLCVehicle.JogOperation.JogRunMode = (EnumJogRunMode)aMCProtocol.get_ItemByTag("PLCJogRunMode").AsUInt16;
                                    break;
                                case "PLCJogTurnSpeed":
                                    this.APLCVehicle.JogOperation.JogTurnSpeed = (EnumJogTurnSpeed)aMCProtocol.get_ItemByTag("PLCJogTurnSpeed").AsUInt16;
                                    break;
                                case "PLCJogMoveVelocity":
                                    this.APLCVehicle.JogOperation.JogMoveVelocity = (EnumJogMoveVelocity)aMCProtocol.get_ItemByTag("PLCJogMoveVelocity").AsUInt16;
                                    break;
                                case "PLCJogOperation":
                                    this.APLCVehicle.JogOperation.JogOperation = (EnumJogOperation)aMCProtocol.get_ItemByTag("PLCJogOperation").AsUInt16;
                                    break;
                                case "PLCJogMoveOntimeRevise":
                                    this.APLCVehicle.JogOperation.JogMoveOntimeRevise = aMCProtocol.get_ItemByTag("PLCJogMoveOntimeRevise").AsBoolean;
                                    break;
                                case "PLCJogMaxDistance":
                                    this.APLCVehicle.JogOperation.JogMaxDistance = this.DECToDouble(aMCProtocol.get_ItemByTag("PLCJogMaxDistance").AsUInt32, 2, 2);
                                    break;
                                case "PLCJogBatteryReplaceIndex":
                                    this.APLCVehicle.BatteryReplaceIndex = aMCProtocol.get_ItemByTag("PLCJogBatteryReplaceIndex").AsUInt16;
                                    //this.mainForm.mainFlowHandler
                                    break;
                                case "ForkInterfaceT1Timeout":
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Fork Interface T1 timeout change to {aMCProtocol.get_ItemByTag("ForkInterfaceT1Timeout").AsUInt16}."));
                                    SetForkCommandTimeout();

                                    break;
                                case "ForkInterfaceT3Timeout":
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Fork Interface T3 timeout to {aMCProtocol.get_ItemByTag("ForkInterfaceT3Timeout").AsUInt16}."));
                                    SetForkCommandTimeout();

                                    break;
                                case "ForkInterfaceT4Timeout":
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Fork Interface T4 timeout changeto {aMCProtocol.get_ItemByTag("ForkInterfaceT4Timeout").AsUInt16}."));
                                    SetForkCommandTimeout();

                                    break;
                                case "ForkInterfaceT5Timeout":
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Fork Interface T5 timeout change to {aMCProtocol.get_ItemByTag("ForkInterfaceT5Timeout").AsUInt16}."));
                                    SetForkCommandTimeout();

                                    break;
                                case "ForkInterfaceT6Timeout":
                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Fork Interface T6 timeout change to {aMCProtocol.get_ItemByTag("ForkInterfaceT6Timeout").AsUInt16}."));
                                    SetForkCommandTimeout();

                                    break;

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //this.errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName + ":" + ex.ToString());
                        LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", oColParam.Item(i).DataName + ":" + ex.ToString()));
                    }


                }
            }
            catch (Exception ex)
            {
                //this.errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", ex.ToString());
                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", ex.ToString()));
            }
        }


        public void SetForkCommandTimeout()
        {
            try
            {
                long iTimeoutSum = aMCProtocol.get_ItemByTag("ForkInterfaceT1Timeout").AsUInt16 * 100
                                                        + aMCProtocol.get_ItemByTag("ForkInterfaceT3Timeout").AsUInt16 * 100
                                                        + aMCProtocol.get_ItemByTag("ForkInterfaceT4Timeout").AsUInt16 * 100
                                                        + aMCProtocol.get_ItemByTag("ForkInterfaceT5Timeout").AsUInt16 * 100
                                                        + aMCProtocol.get_ItemByTag("ForkInterfaceT6Timeout").AsUInt16 * 100;

                string sTempRecord = String.Concat("Fork interface timeout change: ", aMCProtocol.get_ItemByTag("ForkInterfaceT1Timeout").AsUInt16,
                    ", ", aMCProtocol.get_ItemByTag("ForkInterfaceT3Timeout").AsUInt16,
                    ", ", aMCProtocol.get_ItemByTag("ForkInterfaceT4Timeout").AsUInt16,
                    ", ", aMCProtocol.get_ItemByTag("ForkInterfaceT5Timeout").AsUInt16,
                    ", ", aMCProtocol.get_ItemByTag("ForkInterfaceT6Timeout").AsUInt16);

                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "", sTempRecord));

                if (iTimeoutSum > 850000)
                {
                    ForkCommandReadTimeout = iTimeoutSum + 100000;
                    ForkCommandBusyTimeout = iTimeoutSum + 100000;
                    ForkCommandMovingTimeout = iTimeoutSum + 100000;

                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "", $"Change program timeout time: {iTimeoutSum / 1000}"));
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "", ex.StackTrace));
            }

        }


        public void triggerForkCommandInterlockErrorEvent()
        {
            OnForkCommandInterlockErrorEvent?.Invoke(this, eventForkCommand);
        }

        private void ccModeAHSet()
        {

            if (this.APLCVehicle.plcBatterys.CcModeFlag)
            {
                //this.APLCVehicle.APlcBatterys.CcModeAh = this.DECToDouble(aMCProtocol.get_ItemByTag("MeterAH").AsUInt32, 2);
                this.APLCVehicle.plcBatterys.SetCcModeAh(this.DECToDouble(aMCProtocol.get_ItemByTag("MeterAH").AsUInt32, 2), true);

                //判斷CCModeCounter
                if (this.APLCVehicle.plcBatterys.MaxResetAhCcounter <= this.APLCVehicle.plcBatterys.CcModeCounter)
                {
                    this.APLCVehicle.plcBatterys.CcModeCounter = 0;
                    this.SetMeterAHToZero();
                }
                else
                {
                    //if (this.APLCVehicle.APlcBatterys.CcModeAh > 0.5)
                    //{
                    //    this.APLCVehicle.APlcBatterys.CcModeCounter = 0;
                    //    this.SetMeterAHToZero();
                    //}
                    this.APLCVehicle.plcBatterys.CcModeCounter++;
                }
                this.APLCVehicle.plcBatterys.CcModeFlag = false;

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

        private String DoubleToDECString(double inputNum, int Wordlength, int digit = 0)
        {
            string str = "";

            if (digit <= 0)
            {
                str = "";
            }
            else
            {
                for (int i = 0; i < digit; i++)
                {
                    str += "0";
                }
            }
            str = "#." + str;

            String strInputNum = inputNum.ToString(str);
            strInputNum = strInputNum.Replace(".", "");
            inputNum = Convert.ToDouble(strInputNum);

            // Check inputNum 的大小正確
            switch (Wordlength)
            {
                case 1:
                    if (inputNum < -32768)
                    {
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"InputNumber  {inputNum} out of range."));
                        return "-32768";
                    }
                    if (inputNum > 32767)
                    {
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"InputNumber  {inputNum} out of range."));
                        return "32767";
                    }
                    break;
                case 2:
                    if (inputNum < -2147483648)
                    {
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"InputNumber  {inputNum} out of range."));
                        return "-2147483648";
                    }
                    if (inputNum > 2147483647)
                    {
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"InputNumber  {inputNum} out of range."));
                        return "2147483647";
                    }
                    break;
            }


            switch (Wordlength)
            {
                case 1:
                    if (inputNum < 0)
                    {
                        str = (inputNum + 65536).ToString();
                    }
                    else
                    {
                        str = inputNum.ToString();
                    }
                    break;
                case 2:

                    if (inputNum < 0)
                    {
                        str = (inputNum + 4294967296).ToString();
                    }
                    else
                    {
                        str = inputNum.ToString();
                    }
                    break;
            }

            return str;
        }

        #region 特殊 Log 紀錄
        //紀錄 Fork alignment value 
        private void RecordForkAndVeihcleCorrectValue(String status)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                String strLog1 = new StringBuilder().Append(status)
                    .Append(", Fork alignment value - P: ").Append(APLCVehicle.Robot.ForkAlignmentP)
                    .Append(", Y: ").Append(APLCVehicle.Robot.ForkAlignmentY)
                    .Append(", Phi: ").Append(APLCVehicle.Robot.ForkAlignmentPhi)
                    .Append(", F: ").Append(APLCVehicle.Robot.ForkAlignmentF)
                    .Append(", Code: ").Append(APLCVehicle.Robot.ForkAlignmentCode)
                    .Append(", C: ").Append(APLCVehicle.Robot.ForkAlignmentC)
                    .Append(", B: ").Append(APLCVehicle.Robot.ForkAlignmentB)
                    .ToString();

                String strLog2 = "";
                if (AVehicleCorrectValue.otherMessage.Equals(""))
                {
                    strLog2 = new StringBuilder().Append(status)
                                        .Append(", Vehicle position value - delta X: ").Append(AVehicleCorrectValue.VehicleDeltaX)
                                        .Append(", delta Y: ").Append(AVehicleCorrectValue.VehicleDeltaY)
                                        .Append(", theta: ").Append(AVehicleCorrectValue.VehicleTheta)
                                        .Append(", vehicle head: ").Append(AVehicleCorrectValue.VehicleHead)
                                        .Append(", twice revise distance: ").Append(AVehicleCorrectValue.VehicleTwiceReviseDistance)
                                        .ToString();
                }
                else
                {
                    strLog2 = new StringBuilder().Append(status)
                        .Append(", Vehicle position value - delta X: ").Append(AVehicleCorrectValue.VehicleDeltaX)
                        .Append(", delta Y: ").Append(AVehicleCorrectValue.VehicleDeltaY)
                        .Append(", theta: ").Append(AVehicleCorrectValue.VehicleTheta)
                        .Append(", vehicle head: ").Append(AVehicleCorrectValue.VehicleHead)
                        .Append(", twice revise distance: ").Append(AVehicleCorrectValue.VehicleTwiceReviseDistance)
                        .Append(", other: ").Append(AVehicleCorrectValue.VehicleHead)
                        .ToString();
                }
                String strLog3 = "";
                strLog3 = new StringBuilder().Append(status)
                        .Append(", DoubleStoreSensor(L): ").Append(aMCProtocol.get_ItemByTag("DoubleStoreSensor(L)").AsBoolean)
                        .Append(", DoubleStoreSensor(R): ").Append(aMCProtocol.get_ItemByTag("DoubleStoreSensor(R)").AsBoolean)
                        .Append(", Loading1: ").Append(aMCProtocol.get_ItemByTag("Loading1").AsBoolean)
                        .Append(", Loading2: ").Append(aMCProtocol.get_ItemByTag("Loading2").AsBoolean)
                        .ToString();

                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "Empty", strLog1));
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "Empty", strLog2));
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "Empty", strLog3));

                if (status.Equals("ForkCommandNG"))
                {
                    LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "Empty", strLog1));
                    LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "Empty", strLog2));
                    LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "Empty", strLog3));
                }
            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "Empty", ex.StackTrace));
            }


        }

        private void RecordSafetyValueChanged()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;

            String strLog = new StringBuilder()
                    .Append("Safety Status Change: ").Append(APLCVehicle.VehicleSafetyAction)
                    .ToString();
            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "Empty", strLog));

        }

        #endregion

        public void SimulationPLCConnect()
        {
            MCProtocol_OnConnectEvent("");
        }

        private void MCProtocol_OnConnectEvent(String message)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;
            //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "PLC is connected");
            plcAgentLogger.Log(new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "PLC is connected"));
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
            this.SetForkCommandTimeout();
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
                    double startTime = DateTime.Now.Ticks;
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
                        if (ChgStasOffDelayCount >= this.APLCVehicle.plcBatterys.Charging_Off_Delay && this.APLCVehicle.plcBatterys.Charging)
                        {
                            ChgStasOffDelayCount = 0;
                            this.APLCVehicle.plcBatterys.Charging = aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean;//false
                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "Empty", $"ChargeStatus Set Off. Plc ChargeStatus: {aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean}"));
                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "Empty", $"ChargeStatus Set Off. Batterys.Charging: {this.APLCVehicle.plcBatterys.Charging}"));
                        }
                    }
                    else
                    {
                        ChgStasOffDelayCount = 0;
                    }

                    //Batterys Charging Time Out
                    if (this.APLCVehicle.plcBatterys.Charging)
                    {
                        swChargingTimeOut.Start();
                        if (swChargingTimeOut.ElapsedMilliseconds > this.APLCVehicle.plcBatterys.Batterys_Charging_Time_Out)
                        {
                            BatterysChargingTimeOut = true;
                            if (aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean)
                            {
                                if (ChgStopCommandCount >= 10)
                                {
                                    this.setAlarm(plcBatterys_Charging_Time_Out);
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
                    if ((this.APLCVehicle.plcBatterys.MeterVoltage >= this.APLCVehicle.plcBatterys.CCModeStopVoltage))
                    {
                        if (!CCModeStopVoltageChange)
                        {
                            CCModeStopVoltageChange = true;
                            this.APLCVehicle.plcBatterys.CcModeFlag = true;
                            this.ChargeStopCommand();
                        }
                    }
                    else
                    {
                        CCModeStopVoltageChange = false;
                    }

                    //Battery Logger
                    WriteBatLoggerCsv(ref swBatteryLogger, this.APLCVehicle.plcBatterys.Battery_Logger_Interval);

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
                    if (this.APLCVehicle.plcBatterys.SetMeterAhToZeroFlag)
                    {
                        //判斷歸０完成　=> 電表ＡＨ變成0, 所以原先值SetMeterAHToZeroAH　應該要反映到CCmode AH值
                        if (this.APLCVehicle.plcBatterys.MeterAh < 0.5 && this.APLCVehicle.plcBatterys.MeterAh > -0.5)
                        {
                            //this.APLCVehicle.APlcBatterys.CcModeAh = (0 - this.APLCVehicle.APlcBatterys.SetMeterAHToZeroAH) + this.APLCVehicle.APlcBatterys.CcModeAh;
                            this.APLCVehicle.plcBatterys.SetCcModeAh((0 - this.APLCVehicle.plcBatterys.SetMeterAhToZeroAh) + this.APLCVehicle.plcBatterys.CcModeAh, false);
                            this.APLCVehicle.plcBatterys.SetMeterAhToZeroFlag = false;
                        }
                        else
                        {
                            if (this.APLCVehicle.plcBatterys.SwBatteryAhSetToZero.ElapsedMilliseconds > this.APLCVehicle.plcBatterys.ResetAhTimeout)
                            {
                                //Raise Warning
                                this.APLCVehicle.plcBatterys.SetMeterAhToZeroFlag = false;

                            }
                        }

                    }

                    //Battery SOC => 寫在MeterAH發生變化事件裡
                    //這裡處理發事件
                    //OnBatteryPercentageChangeEvent
                    UInt16 currPercentage = Convert.ToUInt16(this.APLCVehicle.plcBatterys.Percentage);
                    if (currPercentage != this.beforeBatteryPercentageInteger)
                    {
                        this.beforeBatteryPercentageInteger = currPercentage;
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Percentage = {currPercentage}"));
                        BatteryPercentageWriteLog(currPercentage);
                        OnBatteryPercentageChangeEvent?.Invoke(this, currPercentage);
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

                            EnumVehicleSafetyAction result = EnumVehicleSafetyAction.Normal;
                            if (!this.APLCVehicle.BeamSensorDisableNormalSpeed)
                            {
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
                            }
                            else
                            {
                                //BeamSensorDisableNormalSpeed == false => 判斷重寫
                                EnumVehicleSafetyAction frontResult = EnumVehicleSafetyAction.Normal;
                                EnumVehicleSafetyAction backResult = EnumVehicleSafetyAction.Normal;
                                EnumVehicleSafetyAction leftResult = EnumVehicleSafetyAction.Normal;
                                EnumVehicleSafetyAction rightResult = EnumVehicleSafetyAction.Normal;
                                if (APLCVehicle.MoveFront == true)
                                {
                                    //前方
                                    frontResult = decideSafetyActionBySideBeamSensorDisableSpeedNormal(APLCVehicle.listFrontBeamSensor, this.APLCVehicle.FrontBeamSensorDisable);
                                }

                                if (APLCVehicle.MoveBack == true)
                                {
                                    //後方
                                    backResult = decideSafetyActionBySideBeamSensorDisableSpeedNormal(APLCVehicle.listBackBeamSensor, this.APLCVehicle.BackBeamSensorDisable);
                                }

                                if (APLCVehicle.MoveLeft == true)
                                {
                                    //左方
                                    leftResult = decideSafetyActionBySideBeamSensorDisableSpeedNormal(APLCVehicle.listLeftBeamSensor, this.APLCVehicle.LeftBeamSensorDisable);
                                }

                                if (APLCVehicle.MoveRight == true)
                                {
                                    //右方
                                    rightResult = decideSafetyActionBySideBeamSensorDisableSpeedNormal(APLCVehicle.listRightBeamSensor, this.APLCVehicle.RightBeamSensorDisable);
                                }

                                if (frontResult == EnumVehicleSafetyAction.Stop || backResult == EnumVehicleSafetyAction.Stop || leftResult == EnumVehicleSafetyAction.Stop || rightResult == EnumVehicleSafetyAction.Stop)
                                {
                                    result = EnumVehicleSafetyAction.Stop;
                                }
                                else
                                {
                                    if (frontResult == EnumVehicleSafetyAction.LowSpeed || backResult == EnumVehicleSafetyAction.LowSpeed || leftResult == EnumVehicleSafetyAction.LowSpeed || rightResult == EnumVehicleSafetyAction.LowSpeed)
                                    {
                                        result = EnumVehicleSafetyAction.LowSpeed;
                                    }
                                    else
                                    {
                                        result = EnumVehicleSafetyAction.Normal;
                                    }
                                }
                            }
                            this.APLCVehicle.VehicleSafetyAction = result;
                        }
                    }

                    // 紀錄 AGV 車子 Safety 狀態變化
                    if (VehicleSafetyAction_Old != this.APLCVehicle.VehicleSafetyAction)
                    {
                        VehicleSafetyAction_Old = this.APLCVehicle.VehicleSafetyAction;
                        RecordSafetyValueChanged();
                    }


                    // Write related data to plc
                    //this.plcOperationRun_WritePlcDisplayData(plcOperationRun_IpcJogPitchData);
                    double endTime = DateTime.Now.Ticks;
                    //LogPlcMsg(loggerAgent, new LogFormat("PlcJogPitch", "1", functionName, PlcId, "Empty", "Thread cycle time: " + (endTime - startTime) / 10000 + " ms"));
                }
                catch (Exception ex)
                {
                    //this.errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", ex.ToString());
                    LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", ex.ToString()));
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
                if (this.DetectSingleSideBeamSensorDisable(listBeamSensor))
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

        private EnumVehicleSafetyAction decideSafetyActionBySideBeamSensorDisableSpeedNormal(List<PlcBeamSensor> listBeamSensor, Boolean SideBeamSensorDisable)
        {
            EnumVehicleSafetyAction result = EnumVehicleSafetyAction.Normal;

            if (SideBeamSensorDisable)
            {
                return result;
            }
            else
            {
                if (this.DetectSideBeamSensorNear(listBeamSensor))
                {
                    //非disable的beam sensor  其中有一顆以上打到東西
                    result = EnumVehicleSafetyAction.Stop;
                }
                else
                {
                    //非disable的beam sensor都沒有打到東西
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

        /// <summary>
        /// 偵測單顆Beamsensor被關掉暫時不看(比如當單顆sensor壞掉或是沒調整好)
        /// </summary>
        /// <param name="listBeamSensor"></param>
        /// <returns></returns>
        private Boolean DetectSingleSideBeamSensorDisable(List<PlcBeamSensor> listBeamSensor)
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
            //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "PLC is disconnected");
            plcAgentLogger.Log(new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "PLC is disconnected"));
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
                        LogPlcMsg(mirleLogger, logFormat);
                    }
                    else
                    {
                        //plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "Set All Beam Sensor Sleep Off(Awake) Fail");
                        LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", "Set All Beam Sensor Sleep Off(Awake) Fail");
                        LogPlcMsg(mirleLogger, logFormat);

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
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to on success."));
                    }
                    else
                    {
                        //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to on fail.");
                        //loggerAgent.LogMsg("PlcAgent", logFormat);
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to on fail."));
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
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set All Beam Sensor Sleep Off(Awake) Success"));
                    }
                    else
                    {
                        //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set All Beam Sensor Sleep Off(Awake) Fail");
                        //loggerAgent.LogMsg("PlcAgent", logFormat);
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set All Beam Sensor Sleep Off(Awake) Fail"));

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
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to off success."));
                    }
                    else
                    {
                        //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to off fail.");
                        //loggerAgent.LogMsg("PlcAgent", logFormat);
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to off fail."));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Current DateTime success, "));
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Current DateTime fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Current DateTime fail, "));
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

            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "WriteForkCommandInfo: " +
                            "CommandNo - " + commandNo +
                            ", ForkCommandType - " + enumForkCommand +
                            ", StageNo - " + stageNo +
                            ", Direction - " + direction +
                            ", IsEqPio - " + eQIF));

            if (this.aMCProtocol.WritePLC())
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command success, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command success, "));
                return true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command fail, "));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Execute Fork Command(" + aEnumForkCommandExecutionType.ToString() + " = " + Convert.ToString(Onflag) + ") success, "));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Execute Fork Command(" + aEnumForkCommandExecutionType.ToString() + " = " + Convert.ToString(Onflag) + ") fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Execute Fork Command(" + aEnumForkCommandExecutionType.ToString() + " = " + Convert.ToString(Onflag) + ") fail, "));

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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Start Command success, "));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Start Command fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Start Command fail, "));

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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Stop Command success, "));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Stop Command fail, ");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Stop Command fail, "));

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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write AGVCOnline = " + Convert.ToString(OnlineFlag) + " success"));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write AGVCOnline = " + Convert.ToString(OnlineFlag) + " fail");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write AGVCOnline = " + Convert.ToString(OnlineFlag) + " fail"));
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
                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", GetFunName(), this.PlcId, "", liArrayWord.ToString()));
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
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"SetAlarmWarningReportAllReset Success"));
                }
                else
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"SetAlarmWarningReportAllReset fail"));

                if (WriteAlarmWarningStatus(false, false))
                    result = true;

            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", GetFunName(), this.PlcId, "", ex.ToString()));
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
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"Write IPC Alarm Warning Report ({stLevelr} => {word.ToString()}.{bit.ToString()}) = {status.ToString()} Success"));
                        result = true;
                    }
                    else
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"Write IPC Alarm Warning Report ({stLevelr} => {word.ToString()}.{bit.ToString()}) = {status.ToString()} fail"));

                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", GetFunName(), this.PlcId, "", ex.ToString()));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlarmStatus = " + Convert.ToString(alarmStatus) + ", IPCWarningStatus = " + Convert.ToString(warningStatus) + " success"));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlarmStatus = " + Convert.ToString(alarmStatus) + ", IPCWarningStatus = " + Convert.ToString(warningStatus) + " fail");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlarmStatus = " + Convert.ToString(alarmStatus) + ", IPCWarningStatus = " + Convert.ToString(warningStatus) + " fail"));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCReady = " + Convert.ToString(readyStatus) + " success"));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCReady = " + Convert.ToString(readyStatus) + " fail");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCReady = " + Convert.ToString(readyStatus) + " fail"));
            }
            return result;
        }

        public void setSOC(double SOC)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "SetSOC SOC = " + Convert.ToString(SOC) + ", OldCCModeAH = " + APLCVehicle.plcBatterys.CcModeAh.ToString() + ", currentAH = " + APLCVehicle.plcBatterys.MeterAh.ToString()));
            this.APLCVehicle.plcBatterys.SetCcModeAh(this.APLCVehicle.plcBatterys.MeterAh + this.APLCVehicle.plcBatterys.AhWorkingRange * (100.0 - SOC) / 100.00, false);
            //CcModeAh
            BatteryPercentageWriteLog(Convert.ToUInt16(SOC));
            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "SetSOC SOC = " + Convert.ToString(SOC) + ", NewCCModeAH = " + APLCVehicle.plcBatterys.CcModeAh.ToString() + ", currentAH = " + APLCVehicle.plcBatterys.MeterAh.ToString()));

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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCStatus = " + Convert.ToString(aEnumIPCStatus) + " success"));
                result = true;
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCStatus = " + Convert.ToString(aEnumIPCStatus) + " fail");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCStatus = " + Convert.ToString(aEnumIPCStatus) + " fail"));
            }
            return result;
        }
        private void WriteBatterySOC()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;

            // this.aMCProtocol.WritePLCByTagDirectly("BatterySOC", this.APLCVehicle.plcBatterys.Percentage.ToString());

            this.aMCProtocol.get_ItemByTag("BatterySOC").AsUInt16 = Convert.ToUInt16(this.APLCVehicle.plcBatterys.Percentage);

            if (this.aMCProtocol.WritePLC())
            { }
            else
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write Battery SOC = " + Convert.ToString(this.APLCVehicle.plcBatterys.Percentage) + " fail"));
            }
        }

        private UInt16 IPCAliveCounter = 1;
        public void WriteIPCAlive()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            // this.aMCProtocol.WritePLCByTagDirectly("IPCAlive", IPCAliveCounter.ToString());

            //heart beat量大不記log
            this.aMCProtocol.get_ItemByTag("IPCAlive").AsUInt16 = IPCAliveCounter;

            //this.aMCProtocol.WritePLC();
            if (this.aMCProtocol.WritePLC())
            {
            }
            else
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlive = " + Convert.ToString(IPCAliveCounter) + " fail"));
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
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementAction(PLC Alarm Reset) = " + Convert.ToString(10) + " Success"));
                }
                else
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementAction(PLC Alarm Reset) = " + Convert.ToString(10) + " Fail"));
                }


                System.Threading.Thread.Sleep(1000);

                eqActionIndex++;
                eqActionIndex = Convert.ToUInt16(Convert.ToInt32(eqActionIndex) % 65536);
                this.aMCProtocol.get_ItemByTag("EquipementActionIndex").AsUInt16 = eqActionIndex;

                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementActionIndex = " + Convert.ToString(eqActionIndex) + " Success"));
                }
                else
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementActionIndex = " + Convert.ToString(eqActionIndex) + " Fail"));
                }
                alarmCodeRecordList.Clear();
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
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementAction(PLC Buzzser Stop) = " + Convert.ToString(11) + " Success"));
                }
                else
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementAction(PLC Buzzser Stop) = " + Convert.ToString(11) + " Fail"));
                }


                System.Threading.Thread.Sleep(1000);
                eqActionIndex++;
                eqActionIndex = Convert.ToUInt16(Convert.ToInt32(eqActionIndex) % 65536);
                this.aMCProtocol.get_ItemByTag("EquipementActionIndex").AsUInt16 = eqActionIndex;

                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementActionIndex = " + Convert.ToString(eqActionIndex) + " Success"));
                }
                else
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "EquipementActionIndex = " + Convert.ToString(eqActionIndex) + " Fail"));
                }
            });
        }

        public void SetMeterAHToZero()
        {
            this.APLCVehicle.plcBatterys.SetMeterAhToZeroFlag = true;
            //this.this.APLCVehicle.PLCBatterys.SetMeterAHToZeroAH = this.this.APLCVehicle.PLCBatterys.MeterAH;
            Task.Run(() =>
            {
                string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;
                this.aMCProtocol.get_ItemByTag("MeterAHToZero").AsBoolean = true;
                //this.aMCProtocol.WritePLC();
                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Meter AH To Zero Success, "));
                }
                else
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Meter AH To Zero fail, "));
                }


                System.Threading.Thread.Sleep(1000);
                this.aMCProtocol.get_ItemByTag("MeterAHToZero").AsBoolean = false;
                //this.aMCProtocol.WritePLC();
                if (this.aMCProtocol.WritePLC())
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Meter AH To Zero Success, "));
                }
                else
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Meter AH To Zero fail, "));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "WriteVehicleDirection fail, "));
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
                if (this.IsFakeForking)
                {
                    //this.APLCVehicle.Robot.ExecutingCommand = aForkCommand;
                    System.Threading.Thread.Sleep(3000);
                    if (this.APLCVehicle.plcBatterys.Charging == true)
                    {
                        System.Threading.Thread.Sleep(27000);
                    }

                    if (aForkCommand.ForkCommandType == EnumForkCommand.Load)
                    {
                        this.APLCVehicle.CarrierSlot.Loading = true;
                        //this.APLCVehicle.CassetteId = "CA0070";
                        APLCVehicle.CarrierSlot.CarrierId = APLCVehicle.CarrierSlot.FakeCarrierId;
                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", $"CassetteIDRead = {APLCVehicle.CarrierSlot.CarrierId}"));

                        OnCassetteIDReadFinishEvent?.Invoke(this, this.APLCVehicle.CarrierSlot.CarrierId);
                    }
                    else if (aForkCommand.ForkCommandType == EnumForkCommand.Unload)
                    {
                        this.APLCVehicle.CarrierSlot.CarrierId = "";
                        this.APLCVehicle.CarrierSlot.Loading = false;
                    }
                    else
                    {

                    }

                    //eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                    OnForkCommandFinishEvent?.Invoke(this, aForkCommand);
                    //clearExecutingForkCommandFlag = true;
                    this.APLCVehicle.Robot.ExecutingCommand = null;
                    aForkCommand = null;
                    return true;
                }

                if (aForkCommand.ForkCommandState == EnumForkCommandState.Queue)
                {
                    this.APLCVehicle.Robot.ExecutingCommand = aForkCommand;
                    return true;
                }
                else
                {
                    //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, aForkCommand.ForkCommandState = " + Convert.ToString(this.APLCVehicle.APlcRobot.ExecutingCommand.ForkCommandState) + ", is not Queue.");
                    //loggerAgent.LogMsg("PlcAgent", logFormat);
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, aForkCommand.ForkCommandState = " + Convert.ToString(this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState) + ", is not Queue."));
                    return false;
                }
            }
            else
            {
                //LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, executingForkCommand is not null.");
                //loggerAgent.LogMsg("PlcAgent", logFormat);
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, executingForkCommand is not null."));
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
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            string strCassetteID = "ERROR";
            this.aCassetteIDReader.ReadBarcode(ref strCassetteID); //成功或失敗都要發ReadFinishEvent,外部用CassetteID來區別成功或失敗
            this.APLCVehicle.CarrierSlot.CarrierId = strCassetteID;
            CassetteID = strCassetteID;
            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "TriggerCassetteIDReader CassetteID = " + Convert.ToString(APLCVehicle.CarrierSlot.CarrierId) + " Success"));

            OnCassetteIDReadFinishEvent?.Invoke(this, strCassetteID);
        }

        public void testTriggerCassetteIDReader(ref string CassetteID)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;

            string strCassetteID = CassetteID;
            //this.aCassetteIDReader.ReadBarcode(ref strCassetteID); //成功或失敗都要發ReadFinishEvent,外部用CassetteID來區別成功或失敗
            this.APLCVehicle.CarrierSlot.CarrierId = strCassetteID;
            //CassetteID = strCassetteID;
            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "testTriggerCassetteIDReader CassetteID = " + Convert.ToString(APLCVehicle.CarrierSlot.CarrierId) + " Success"));

            OnCassetteIDReadFinishEvent?.Invoke(this, strCassetteID);


        }



        public void ClearExecutingForkCommand()
        {
            clearExecutingForkCommandFlag = true;
        }

        public void plcForkCommandControlRun()
        {
            int iCommandNGCounter = 0;
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;

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

                                #region Not used
                                //if (this.IsFakeForking)
                                //{
                                //    System.Threading.Thread.Sleep(3000);
                                //    if (this.APLCVehicle.plcBatterys.Charging == true)
                                //    {
                                //        System.Threading.Thread.Sleep(27000);
                                //    }

                                //    if (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType == EnumForkCommand.Load)
                                //    {
                                //        this.APLCVehicle.Loading = true;
                                //        //this.APLCVehicle.CassetteId = "CA0070";
                                //        APLCVehicle.CassetteId = APLCVehicle.FakeCassetteId;
                                //        OnCassetteIDReadFinishEvent?.Invoke(this, this.APLCVehicle.CassetteId);
                                //    }
                                //    else if (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType == EnumForkCommand.Unload)
                                //    {
                                //        this.APLCVehicle.CassetteId = "";
                                //        this.APLCVehicle.Loading = false;

                                //    }
                                //    else
                                //    {

                                //    }

                                //    eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                //    OnForkCommandFinishEvent?.Invoke(this, eventForkCommand);
                                //    clearExecutingForkCommandFlag = true;

                                //    break;
                                //}
                                #endregion

                                //送出指令                              
                                if (this.aMCProtocol.get_ItemByTag("ForkReady").AsBoolean && this.aMCProtocol.get_ItemByTag("ForkBusy").AsBoolean == false)
                                {
                                    this.APLCVehicle.Robot.ExecutingCommand.Reason = "";
                                    this.WriteForkCommandInfo(Convert.ToUInt16(this.APLCVehicle.Robot.ExecutingCommand.CommandNo), this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType, this.APLCVehicle.Robot.ExecutingCommand.StageNo, this.APLCVehicle.Robot.ExecutingCommand.Direction, this.APLCVehicle.Robot.ExecutingCommand.IsEqPio, this.APLCVehicle.Robot.ExecutingCommand.ForkSpeed);
                                    System.Threading.Thread.Sleep(500);
                                    this.WriteForkCommandInfo(Convert.ToUInt16(this.APLCVehicle.Robot.ExecutingCommand.CommandNo), this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType, this.APLCVehicle.Robot.ExecutingCommand.StageNo, this.APLCVehicle.Robot.ExecutingCommand.Direction, this.APLCVehicle.Robot.ExecutingCommand.IsEqPio, this.APLCVehicle.Robot.ExecutingCommand.ForkSpeed);
                                    System.Threading.Thread.Sleep(500);
                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, true);
                                    sw.Reset();
                                    sw.Start();
                                    //<-- 2020/01/03 Modify by Ellison
                                    int readTimeoutCounter = 0;
                                    //-->
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
                                            iCommandNGCounter++;
                                            if (iCommandNGCounter > 10)
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
                                                iCommandNGCounter = 0;
                                                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", $"Trigger OnForkCommandErrorEvent because of ForkCommandNG in queue state."));

                                                break;
                                            }

                                        }
                                        else
                                        {
                                            if (sw.ElapsedMilliseconds < ForkCommandReadTimeout)
                                            {

                                                if (clearExecutingForkCommandFlag)
                                                {
                                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "ForkCommandReadTimeout clearExecutingForkCommandFlag = true"));
                                                    break;
                                                }

                                                //<-- 2020/01/03 Modify by Ellison
                                                //System.Threading.Thread.Sleep(20);
                                                SpinWait.SpinUntil(() => false, 5);
                                                readTimeoutCounter++;
                                                if (readTimeoutCounter == 1200)
                                                {
                                                    this.APLCVehicle.Robot.ExecutingCommand.Reason = "";
                                                    this.WriteForkCommandInfo(Convert.ToUInt16(this.APLCVehicle.Robot.ExecutingCommand.CommandNo), this.APLCVehicle.Robot.ExecutingCommand.ForkCommandType, this.APLCVehicle.Robot.ExecutingCommand.StageNo, this.APLCVehicle.Robot.ExecutingCommand.Direction, this.APLCVehicle.Robot.ExecutingCommand.IsEqPio, this.APLCVehicle.Robot.ExecutingCommand.ForkSpeed);

                                                    System.Threading.Thread.Sleep(500);
                                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, true);
                                                    readTimeoutCounter = 0;
                                                }
                                                //-->
                                            }
                                            else
                                            {
                                                //read time out
                                                //this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                                //System.Threading.Thread.Sleep(1000);
                                                //this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, true);

                                                //<-- 2020/01/13 Modify by dean  避免漏寫入硬改寫入三次
                                                this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                                System.Threading.Thread.Sleep(500);
                                                this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                                System.Threading.Thread.Sleep(500);
                                                this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                                //-->
                                                this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                                this.APLCVehicle.Robot.ExecutingCommand.Reason = "Fork Command Read timeout";
                                                //this.aAlarmHandler.SetAlarm(270002);
                                                //this.setAlarm(270002);
                                                this.setAlarm(Fork_Command_Read_timeout);
                                                eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                                OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", $"Trigger OnForkCommandErrorEvent because of Fork_Command_Read_timeout"));

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
                                    //<-- 2020/01/03 Modify by Ellison
                                    readTimeoutCounter = 0;
                                    //-->
                                    while (true)
                                    {
                                        if (this.aMCProtocol.get_ItemByTag("ForkBusy").AsBoolean == false)
                                        {
                                            if (sw.ElapsedMilliseconds < this.ForkCommandBusyTimeout)
                                            {

                                                if (clearExecutingForkCommandFlag)
                                                {
                                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "ForkCommandBusyTimeout clearExecutingForkCommandFlag = true"));
                                                    break;
                                                }

                                                //<-- 2020/01/03 Modify by Ellison
                                                //System.Threading.Thread.Sleep(20);
                                                SpinWait.SpinUntil(() => false, 5);
                                                readTimeoutCounter++;
                                                if (readTimeoutCounter == 1200)
                                                {
                                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                                    System.Threading.Thread.Sleep(1000);
                                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, true);
                                                    readTimeoutCounter = 0;
                                                    break;

                                                }
                                                //-->
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
                                                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", $"Trigger OnForkCommandErrorEvent because of Fork_Not_Busy_timeout"));
                                                break;
                                            }

                                            if (this.aMCProtocol.get_ItemByTag("ForkCommandNG").AsBoolean)
                                            {
                                                iCommandNGCounter++;
                                                if (iCommandNGCounter > 10)
                                                {
                                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, false);
                                                    this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                                    this.APLCVehicle.Robot.ExecutingCommand.Reason = "ForkCommandNG";
                                                    //Raise Alarm
                                                    //this.aAlarmHandler.SetAlarm(270001);
                                                    //this.setAlarm(270001);
                                                    this.setAlarm(Fork_Command_Format_NG);
                                                    eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                                    OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                                    iCommandNGCounter = 0;
                                                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", $"Trigger OnForkCommandErrorEvent because of ForkCommandNG in queue state"));

                                                    break;
                                                }

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
                                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "ForkCommandMovingTimeout clearExecutingForkCommandFlag = true"));
                                            break;
                                        }
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    else
                                    {

                                        this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                        this.APLCVehicle.Robot.ExecutingCommand.Reason = "ForkCommand Moving Timeout";
                                        //Raise Alarm?Warning?   
                                        //this.aAlarmHandler.SetAlarm(270004);
                                        //this.setAlarm(270004);
                                        this.setAlarm(Fork_Command_Executing_timeout);
                                        eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                        OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                        break;
                                    }

                                    if (this.aMCProtocol.get_ItemByTag("ForkCommandNG").AsBoolean)
                                    {
                                        this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                        this.APLCVehicle.Robot.ExecutingCommand.Reason = "ForkCommandNG";
                                        //Raise Alarm
                                        //this.aAlarmHandler.SetAlarm(270001);
                                        //this.setAlarm(270001);
                                        this.setAlarm(Fork_Command_Format_NG);
                                        eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                        OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", $"Trigger OnForkCommandErrorEvent because of ForkCommandNG in executing state."));

                                        break;
                                    }
                                }
                                sw.Stop();
                                sw.Reset();

                                if (this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState == EnumForkCommandState.Error)
                                {
                                    break;
                                }

                                if (this.aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean == true)
                                {
                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, true);
                                    this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Finish;
                                    System.Threading.Thread.Sleep(1000);
                                    this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, false);
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
                                        this.APLCVehicle.CarrierSlot.CarrierId = cassetteID;
                                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, this.PlcId, "", $"CassetteIDRead = {this.APLCVehicle.CarrierSlot.CarrierId}"));

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

                                sw.Stop();
                                sw.Reset();
                                sw.Start();

                                while (true)
                                {

                                    Thread.Sleep(50);
                                    if (this.aMCProtocol.get_ItemByTag("ForkCommandNG").AsBoolean)
                                    {
                                        this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                        this.APLCVehicle.Robot.ExecutingCommand.Reason = "ForkCommandNG";
                                        //Raise Alarm
                                        //this.aAlarmHandler.SetAlarm(270001);
                                        //this.setAlarm(270001);
                                        this.setAlarm(Fork_Command_Format_NG);
                                        eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                        OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                        LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", $"Trigger OnForkCommandErrorEvent because of ForkCommandNG in Finish state"));

                                        break;
                                    }

                                    if (this.APLCVehicle.RobotHome)
                                    {
                                        eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                        OnForkCommandFinishEvent?.Invoke(this, eventForkCommand);
                                        clearExecutingForkCommandFlag = true;
                                        break;
                                    }
                                    else if (sw.Elapsed.TotalSeconds > 30)
                                    {
                                        this.setAlarm(Fork_Home_Flag_Waiting_timeout);

                                        this.APLCVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                        this.APLCVehicle.Robot.ExecutingCommand.Reason = "ForkCommandFinishTimeout";
                                        eventForkCommand = this.APLCVehicle.Robot.ExecutingCommand;
                                        OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                        break;
                                    }

                                }
                                sw.Stop();
                                sw.Reset();
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
                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"ForkCommandOK ExecutingCommand Is NULL,Command_Read_Request False"));
                        }
                        if (this.aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean && bComdIsNullReqComdFinishAck)
                        {
                            bComdIsNullReqComdFinishAck = false;
                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, true);
                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"ForkCommandFinish ExecutingCommand Is NULL,Command_Finish_Ack True"));
                            System.Threading.Thread.Sleep(1000);
                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, false);
                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", $"ForkCommandFinish ExecutingCommand Is NULL,Command_Finish_Ack False"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    //this.errLogger.SaveLogFile("Error", "5", functionName, this.PlcId, "", ex.ToString());

                    //LogFormat logFormat = new LogFormat("Error", "5", functionName, this.PlcId, "", ex.ToString());
                    LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", ex.ToString()));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleCharge On = " + Convert.ToString(true) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleCharge On = " + Convert.ToString(true) + " fail"));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleCharge Off = " + Convert.ToString(false) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleCharge Off = " + Convert.ToString(false) + " fail"));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleInPosition On = " + Convert.ToString(true) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleInPosition On = " + Convert.ToString(true) + " fail"));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleInPosition Off = " + Convert.ToString(false) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set VehicleInPosition Off = " + Convert.ToString(false) + " fail"));
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
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Forc ELMO Servo Off On = " + Convert.ToString(true) + " success"));
                result = true;
            }
            else
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Forc ELMO Servo Off On = " + Convert.ToString(true) + " fail"));
            }
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(120);
                    if (this.aMCProtocol.get_ItemByTag("Force_ELMO_Servo_Off").AsBoolean == false)
                    {
                        this.aMCProtocol.get_ItemByTag("Force_ELMO_Servo_Off").AsBoolean = true;
                        if (this.aMCProtocol.WritePLC())
                        {
                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Forc ELMO Servo Off On = " + Convert.ToString(true) + " success"));
                            result = true;
                        }
                        else
                        {
                            LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Forc ELMO Servo Off On = " + Convert.ToString(true) + " fail"));
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            });
            
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
        private void BatteryPercentageWriteLog(ushort value)
        {
            string strDirectoryFullPath = Path.Combine(Environment.CurrentDirectory, "Log", "BatteryPercentage.log");
            using (StreamWriter sw = new StreamWriter(strDirectoryFullPath))
            {
                sw.Write(value.ToString());
            }
        }
        private void LogPlcMsg(MirleLogger mirleLogger, LogFormat clsLogFormat)
        {
            strlogMsg = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff") + "\t" + clsLogFormat.Message + "\r\n" + strlogMsg;
            if (strlogMsg.Length > LogMsgMaxLength) strlogMsg = strlogMsg.Substring(0, LogMsgMaxLength);
            mirleLogger.Log(clsLogFormat);
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
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "Write Plc Config To XML Success"));
                    result = true;
                }
                else
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", "Write Plc Config To XML Fail"));
                }
            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", ex.ToString()));
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
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", "Write Directional Light (" + atype.ToString() + ") = " + Convert.ToString(status) + " Success"));
                }
                else
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", "Write Directional Light (" + atype.ToString() + ") = " + Convert.ToString(status) + " fail"));
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



        public void SetVehiclePositionValue(String deltaX, String deltaY, String theta, String vehicleHead, String vehicleTwiceReviseDistance, String otherMessage = "")
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name; ;
            try
            {
                this.AVehicleCorrectValue.VehicleDeltaX = deltaX;
                this.AVehicleCorrectValue.VehicleDeltaY = deltaY;
                this.AVehicleCorrectValue.VehicleTheta = theta;
                this.AVehicleCorrectValue.VehicleHead = vehicleHead;
                this.AVehicleCorrectValue.VehicleTwiceReviseDistance = vehicleTwiceReviseDistance;
                this.AVehicleCorrectValue.otherMessage = otherMessage;
            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("Error", "9", functionName, this.PlcId, "", ex.ToString()));
            }
        }


        /// <summary>
        /// 清除紀錄值
        /// </summary>
        private void ClearForkAndVehicleCorrectValue()
        {
            this.AVehicleCorrectValue.VehicleDeltaX = "";
            this.AVehicleCorrectValue.VehicleDeltaY = "";
            this.AVehicleCorrectValue.VehicleTheta = "";
            this.AVehicleCorrectValue.VehicleHead = "";
            this.AVehicleCorrectValue.VehicleTwiceReviseDistance = "";
            this.AVehicleCorrectValue.otherMessage = "";

            this.APLCVehicle.Robot.ForkAlignmentP = 0f;
            this.APLCVehicle.Robot.ForkAlignmentY = 0f;
            this.APLCVehicle.Robot.ForkAlignmentPhi = 0f;
            this.APLCVehicle.Robot.ForkAlignmentF = 0f;
            this.APLCVehicle.Robot.ForkAlignmentCode = 0;
            this.APLCVehicle.Robot.ForkAlignmentC = 0f;
            this.APLCVehicle.Robot.ForkAlignmentB = 0f;

        }


        private void WritePLCBatteryBigData(BatteryLog batteryLog)
        {

            //String datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
            //LogRecord(NLog.LogLevel.Info, "WriteDateTimeCalibrationReport", "DateTime: " + datetime);
            //this.aMCProtocol.get_ItemByTag("YearMonth").AsHex = datetime.Substring(2, 4);
            //this.aMCProtocol.get_ItemByTag("DayHour").AsHex = datetime.Substring(6, 4);
            //this.aMCProtocol.get_ItemByTag("MinSec").AsHex = datetime.Substring(10, 4);

            DateTime dateTime = DateTime.Now;
            string date = dateTime.ToString("yyyyMMddHHmmss");
            ushort iMoveDistance = 0;
            ushort iLoadUnloadCount = 0;
            ushort iChargeCount = 0;


            try
            {
                dateTime = DateTime.ParseExact(batteryLog.ResetTime, "yyyy-MM-dd HH-mm-ss.fff", CultureInfo.InvariantCulture);
                date = dateTime.ToString("yyyyMMddHHmmss");
                iMoveDistance = (ushort)(batteryLog.MoveDistanceTotalM / 1000);
                iLoadUnloadCount = (ushort)batteryLog.LoadUnloadCount;
                iChargeCount = (ushort)batteryLog.ChargeCount;
            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", ex.ToString()));
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", ex.StackTrace));
            }

            this.aMCProtocol.get_ItemByTag("IPCBigDataVehBatteryResetYearMonth").AsHex = date.Substring(2, 2) + date.Substring(4, 2);
            this.aMCProtocol.get_ItemByTag("IPCBigDataVehBatteryResetDayHour").AsHex = date.Substring(6, 2) + date.Substring(8, 2);
            this.aMCProtocol.get_ItemByTag("IPCBigDataVehBatteryResetMinSec").AsHex = date.Substring(10, 2) + date.Substring(12, 2);
            this.aMCProtocol.get_ItemByTag("IPCBigDataVehBatteryMoveDistance").AsUInt16 = iMoveDistance;
            this.aMCProtocol.get_ItemByTag("IPCBigDataVehBatteryLoadUnloadCount").AsUInt16 = iLoadUnloadCount;
            this.aMCProtocol.get_ItemByTag("IPCBigDataVehBatteryChargeCount").AsUInt16 = iChargeCount;

            if (this.aMCProtocol.WritePLC())
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", "Write to PLC Success"));
            }
            else
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", GetFunName(), PlcId, "Empty", "Write to PLC fail"));
            }
        }

        private JogPitchData plcOperationRun_IpcJogPitchData = new JogPitchData();
        private bool plcOperationRun_IpcJogPitchData_CanAuto = false;
        private bool plcOperationRun_bActionRunningStatus = false;
        private EnumPlcOperationStep plcOperationRun_enumNowStep = EnumPlcOperationStep.No_Use;
        private EnumPlcOperationStep plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
        private PlcOperation plcOperationRun_PlcOper = new PlcOperation();  // 為了Plc jog 功能使用
        private PlcOperation plcOperationRun_PlcOperLast = new PlcOperation();  // 為了Plc jog 功能使用
        private PlcOperation plcOperationRun_IpcOper = new PlcOperation();

        public void plcOperationRun_ThreadControl(bool open)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                if (open)
                {
                    plcOperationThread = new Thread(plcOperationRun);
                    plcOperationThread.Start();
                }
                else
                {
                    if (plcOperationThread != null)
                    {
                        plcOperationThread.Abort();
                    }
                }
            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", ex.ToString()));
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", ex.ToString()));
                LogPlcMsg(mirleLogger, new LogFormat("PlcAgent", "1", functionName, PlcId, "", $"Error: action {open} is failed."));
            }

        }

        private void plcOperationRun()
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;
            EnumPlcOperationStep inner_enumLastStep = EnumPlcOperationStep.No_Use;
            plcOperationRun_PlcOper = APLCVehicle.JogOperation.DeepClone();
            plcOperationRun_IpcOper.JogElmoFunction = EnumJogElmoFunction.Enable;

            plcOperationRun_WritePlcDisplayData(plcOperationRun_IpcJogPitchData, plcOperationRun_IpcJogPitchData_CanAuto);

            Thread.Sleep(5000);

            while (true)
            {
                if (Vehicle.Instance.AutoState == EnumAutoState.Manual)
                {
                    // Write related data to plc
                    plcOperationRun_WritePlcDisplayData(plcOperationRun_IpcJogPitchData, plcOperationRun_IpcJogPitchData_CanAuto);
                }

                double startTime = DateTime.Now.Ticks;
                try
                {
                    // 取得目前 JogPitchForm 操作狀態 plcOperationRun_IpcOper
                    plcOperationRun_GetOperationStatusFromJogPitchForm(ref plcOperationRun_IpcOper);
                    Thread.Sleep(1);

                    // 取得目前資訊值
                    plcOperationRun_GetJogPitchDataFromJogPitchForm(ref plcOperationRun_IpcJogPitchData, ref plcOperationRun_IpcJogPitchData_CanAuto);
                    Thread.Sleep(1);

                    plcOperationRun_PlcOperLast = plcOperationRun_PlcOper.DeepClone();
                    plcOperationRun_PlcOper = APLCVehicle.JogOperation.DeepClone();

                    // 取得這次 Thread 所使用的 Last Step
                    inner_enumLastStep = plcOperationRun_enumLastStep;
                    Thread.Sleep(1);

                    // Set base value from plc to jogpitch form
                    plcOperationRun_SetPlcOperValueToJogPitchForm(ref plcOperationRun_PlcOper, ref plcOperationRun_PlcOperLast);
                    Thread.Sleep(1);

                    // get action step from plc 
                    plcOperationRun_GetNowStep(ref plcOperationRun_enumNowStep, ref inner_enumLastStep, ref plcOperationRun_PlcOper, ref plcOperationRun_PlcOperLast);
                    Thread.Sleep(1);

                    if (plcOperationRun_enumNowStep != inner_enumLastStep)
                    {
                        LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", functionName, PlcId, "", $"Get Now Step: {plcOperationRun_enumNowStep.ToString()}"));
                    }


                    // 比較新的訊號: 當新的訊號跟舊的訊號不同時, 先停再說
                    if (plcOperationRun_enumNowStep == inner_enumLastStep)
                    {
                        // do nothing
                    }
                    else
                    {
                        if (inner_enumLastStep != EnumPlcOperationStep.No_Use)
                        {
                            LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", functionName, PlcId, "", $"Prepare Stop Step: Operation NowStep = {plcOperationRun_enumNowStep.ToString()}, LastStep = {plcOperationRun_enumLastStep.ToString()} "));
                            plcOperationRun_ClearAndStopSteps(ref plcOperationRun_IpcJogPitchData, ref inner_enumLastStep, ref plcOperationRun_IpcOper);
                            plcOperationRun_enumLastStep = inner_enumLastStep;
                            Thread.Sleep(1);
                        }

                    }

                    Thread.Sleep(1); // take a break

                    // 執行 PLC signal 功能
                    if (inner_enumLastStep == EnumPlcOperationStep.No_Use)
                    {
                        if (plcOperationRun_enumNowStep != inner_enumLastStep)
                        {
                            LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", functionName, PlcId, "", $"Prepare xecute Step: Operation NowStep = {plcOperationRun_enumNowStep.ToString()}, LastStep = {plcOperationRun_enumLastStep.ToString()} "));
                            plcOperationRun_ExecuteSteps();
                            Thread.Sleep(1);
                        }
                    }
                    else
                    {
                        // do nothing
                    }

                }
                catch (Exception ex)
                {
                    //  發生錯誤時停止所有狀態
                    LogPlcMsg(mirleLogger, new LogFormat("Error", "5", functionName, this.PlcId, "", ex.ToString()));
                    LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", functionName, PlcId, "Error", $"Error: Operation NowStep = {plcOperationRun_enumNowStep.ToString()}, LastStep = {plcOperationRun_enumLastStep.ToString()} "));
                    LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", functionName, PlcId, "Error", ex.ToString()));

                    // 取得目前資訊值
                    plcOperationRun_GetJogPitchDataFromJogPitchForm(ref plcOperationRun_IpcJogPitchData, ref plcOperationRun_IpcJogPitchData_CanAuto);
                    plcOperationRun_ClearAndStopSteps(ref plcOperationRun_IpcJogPitchData, ref plcOperationRun_enumLastStep, ref plcOperationRun_IpcOper);
                    //break;
                }
                double endTime = DateTime.Now.Ticks;
                //LogPlcMsg(loggerAgent, new LogFormat("PlcJogPitch", "1", functionName, PlcId, "Empty", "Thread cycle time: " + (endTime - startTime) / 10000 + " ms"));

                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", functionName, PlcId, "Error", "Thread cycle end: now - " + plcOperationRun_enumNowStep + ", last - " + plcOperationRun_enumLastStep));

                Thread.Sleep(1000);
                // endWhile
            }


        }

        private UInt16 plcOperationRun_ConvertAxisStatusToUint16(ElmoAxisFeedbackData axisData)
        {
            UInt16 iRetVal = 0;

            if (axisData.StandStill ^ axisData.Disable)
            {
                if (axisData.StandStill)
                {
                    iRetVal = (int)EnumElmoStatus.StandStill;
                }
                else if (axisData.Disable)
                {
                    iRetVal = (int)EnumElmoStatus.Disable;
                }
            }
            else
            {
                iRetVal = 0;
            }

            return iRetVal;
        }

        private void plcOperationRun_GetJogPitchDataFromJogPitchForm(ref JogPitchData ipcJogPitchData, ref bool plcOperationRun_IpcJogPitchData_CanAuto)
        {
            // 取得目前資訊值
            ipcJogPitchData.MapX = this.jogPitchForm.jogPitchData.MapX;
            ipcJogPitchData.MapY = this.jogPitchForm.jogPitchData.MapX;
            ipcJogPitchData.Theta = this.jogPitchForm.jogPitchData.Theta;
            ipcJogPitchData.MapTheta = this.jogPitchForm.jogPitchData.MapTheta;
            ipcJogPitchData.SectionDeviation = this.jogPitchForm.jogPitchData.SectionDeviation;
            ipcJogPitchData.ElmoFunctionCompelete = this.jogPitchForm.jogPitchData.ElmoFunctionCompelete;
            ipcJogPitchData.AxisData = this.jogPitchForm.jogPitchData.AxisData;
            plcOperationRun_IpcJogPitchData_CanAuto = this.jogPitchForm.CanAuto;
        }

        private void plcOperationRun_WritePlcDisplayData(JogPitchData plcJogPitchData, bool locationCanAuto)
        {
            try
            {
                String strConcateKey = "";

                this.aMCProtocol.get_ItemByTag("SR2000MapX").AsUInt32 = Convert.ToUInt32(plcOperationRun_WritePlcDisplayData_TryParseNumber("SR2000MapX", plcJogPitchData.MapX, 2, 2));
                this.aMCProtocol.get_ItemByTag("SR2000MapY").AsUInt32 = Convert.ToUInt32(plcOperationRun_WritePlcDisplayData_TryParseNumber("SR2000MapY", plcJogPitchData.MapY, 2, 2));
                this.aMCProtocol.get_ItemByTag("SR2000MapTheta").AsUInt32 = Convert.ToUInt32(plcOperationRun_WritePlcDisplayData_TryParseNumber("SR2000MapTheta", plcJogPitchData.MapTheta, 2, 2));
                this.aMCProtocol.get_ItemByTag("SR2000PathDeviation").AsUInt32 = Convert.ToUInt32(plcOperationRun_WritePlcDisplayData_TryParseNumber("SR2000PathDeviation", plcJogPitchData.SectionDeviation, 2, 2));
                this.aMCProtocol.get_ItemByTag("SR2000ThetaDeviation").AsUInt32 = Convert.ToUInt32(plcOperationRun_WritePlcDisplayData_TryParseNumber("SR2000ThetaDeviation", plcJogPitchData.Theta, 2, 2));
                this.aMCProtocol.get_ItemByTag("IpcJogLocationReady").AsBoolean = locationCanAuto;

                foreach (EnumAxis enumAxis in (EnumAxis[])Enum.GetValues(typeof(EnumAxis)))
                {
                    if (plcJogPitchData.AxisData.ContainsKey(enumAxis))
                    {
                        if (null != plcJogPitchData.AxisData[enumAxis])
                        {
                            strConcateKey = enumAxis.ToString() + "Status";
                            this.aMCProtocol.get_ItemByTag(strConcateKey).AsUInt16 = plcOperationRun_ConvertAxisStatusToUint16(plcJogPitchData.AxisData[enumAxis]);
                            strConcateKey = enumAxis.ToString() + "Position";
                            this.aMCProtocol.get_ItemByTag(strConcateKey).AsUInt32 = Convert.ToUInt32(plcOperationRun_WritePlcDisplayData_TryParseNumber(strConcateKey, plcJogPitchData.AxisData[enumAxis].Feedback_Position, 2, 2));
                            strConcateKey = enumAxis.ToString() + "Tocque";
                            this.aMCProtocol.get_ItemByTag(strConcateKey).AsUInt32 = Convert.ToUInt32(plcOperationRun_WritePlcDisplayData_TryParseNumber(strConcateKey, plcJogPitchData.AxisData[enumAxis].Feedback_Torque, 2, 2));
                        }
                    }
                }

                this.aMCProtocol.get_ItemByTag("IpcVehicleMode").AsUInt16 = (ushort)plcOperationRun_IpcOper.ModeVehicle;
                this.aMCProtocol.get_ItemByTag("IpcJogElmoFunction").AsUInt16 = (ushort)plcOperationRun_IpcOper.JogElmoFunction;
                this.aMCProtocol.get_ItemByTag("IpcJogRunMode").AsUInt16 = (ushort)plcOperationRun_IpcOper.JogRunMode;
                this.aMCProtocol.get_ItemByTag("IpcJogTurnSpeed").AsUInt16 = (ushort)plcOperationRun_IpcOper.JogTurnSpeed;
                this.aMCProtocol.get_ItemByTag("IpcJogMoveVelocity").AsUInt16 = (ushort)plcOperationRun_IpcOper.JogMoveVelocity;
                this.aMCProtocol.get_ItemByTag("IpcJogOperation").AsUInt16 = (ushort)plcOperationRun_IpcOper.JogOperation;
                this.aMCProtocol.get_ItemByTag("IpcJogMoveOntimeRevise").AsBoolean = plcOperationRun_IpcOper.JogMoveOntimeRevise;
                this.aMCProtocol.get_ItemByTag("IpcJogMaxDistance").AsUInt32 = Convert.ToUInt32(plcOperationRun_WritePlcDisplayData_TryParseNumber("IpcJogMaxDistance", plcOperationRun_IpcOper.JogMaxDistance, 2, 2));
                //LogPlcMsg(loggerAgent, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", $"plcOperationRun_IpcOper.JogMaxDistance: {plcOperationRun_IpcOper.JogMaxDistance}"));
                this.aMCProtocol.get_ItemByTag("IpcJogActionStatus").AsBoolean = plcOperationRun_bActionRunningStatus;

                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", $"IpcJogElmoFunction: {plcOperationRun_IpcOper.JogElmoFunction}"));
                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", $"IpcJogRunMode: {plcOperationRun_IpcOper.JogRunMode}"));
                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", $"IpcJogTurnSpeed: {plcOperationRun_IpcOper.JogTurnSpeed}"));
                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", $"IpcJogMoveVelocity: {plcOperationRun_IpcOper.JogMoveVelocity}"));
                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", $"IpcJogOperation: {plcOperationRun_IpcOper.JogOperation}"));
                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", $"IpcJogMaxDistance: {plcOperationRun_IpcOper.JogMaxDistance}"));


                if (this.aMCProtocol.WritePLC())
                {
                    //LogPlcMsg(loggerAgent, new LogFormat("PlcJogPitch", "9", GetFunName(), PlcId, "Empty", "Write data Success."));
                }
                else
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", "Write data fail."));
                }
            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("Error", "5", GetFunName(), this.PlcId, "", ex.ToString()));
                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", ex.ToString()));
            }
        }


        private String plcOperationRun_WritePlcDisplayData_TryParseNumber(String numberName, double inputNum, int Wordlength, int digit = 0)
        {
            String value = "0";
            try
            {
                value = this.DoubleToDECString(inputNum, Wordlength, digit);
            }
            catch (Exception ex)
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", numberName + " : " + inputNum + " is out of range."));
                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", ex.ToString()));
            }

            return value;
        }

        private void plcOperationRun_SetPlcOperValueToJogPitchForm(ref PlcOperation plcOper, ref PlcOperation plcOperLast)
        {
            if (plcOper.JogTurnSpeed != plcOperLast.JogTurnSpeed
                || plcOper.JogMoveVelocity != plcOperLast.JogMoveVelocity
                || plcOper.JogMaxDistance != plcOperLast.JogMaxDistance
                || plcOper.JogMoveOntimeRevise != plcOperLast.JogMoveOntimeRevise)
            {
                //LogPlcMsg(loggerAgent, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error", $"plcOper.JogMaxDistance : {plcOper.JogMaxDistance}"));
                this.jogPitchForm.PlcJog_SettingDelegate(plcOper.JogTurnSpeed, plcOper.JogMoveVelocity, plcOper.JogMaxDistance, plcOper.JogMoveOntimeRevise);
                this.jogPitchForm.PlcJog_RunSettingDelegate();
            }
        }

        private void plcOperationRun_GetOperationStatusFromJogPitchForm(ref PlcOperation plcOperationRun_IpcOper)
        {
            if (Vehicle.Instance.AutoState == EnumAutoState.Auto)
            {
                plcOperationRun_IpcOper.ModeVehicle = EnumJogVehicleMode.Auto;
            }
            else
            {
                plcOperationRun_IpcOper.ModeVehicle = EnumJogVehicleMode.Manual;
            }
            this.jogPitchForm.PlcJog_GetOperationStatus(ref plcOperationRun_IpcOper);
        }


        private void plcOperationRun_GetNowStep(ref EnumPlcOperationStep enumNowStep, ref EnumPlcOperationStep enumLastStep, ref PlcOperation plcOper, ref PlcOperation plcOperLast)
        {
            string functionName = GetType().Name + ":" + System.Reflection.MethodBase.GetCurrentMethod().Name;
            enumNowStep = EnumPlcOperationStep.No_Use;

            if (enumLastStep == EnumPlcOperationStep.No_Use || enumLastStep == EnumPlcOperationStep.JogOperationMoveForward ||
                enumLastStep == EnumPlcOperationStep.JogOperationMoveBackward || enumLastStep == EnumPlcOperationStep.JogOperationTurnLeft ||
                enumLastStep == EnumPlcOperationStep.JogOperationTurnRight || enumLastStep == EnumPlcOperationStep.JogOperationStop)
            {
                // 這些動作必須要判斷現有訊號值 & 上一次訊號值, 來確認動作
            }
            else if (enumLastStep == EnumPlcOperationStep.ElmoEnable ||
                enumLastStep == EnumPlcOperationStep.ElmoDisable ||
                enumLastStep == EnumPlcOperationStep.ElmoAllReset)
            {

            }
            else if (enumLastStep == EnumPlcOperationStep.VehicleModeAuto ||
                enumLastStep == EnumPlcOperationStep.VehicleModeManual)
            {
                enumNowStep = enumLastStep;
            }
            else
            {
                enumNowStep = enumLastStep;
                return;
            }
            LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error",
                    $"Initial   enumNowStep: {enumNowStep.ToString()}, enumLastStep: {enumLastStep.ToString()}"));
            if (plcOper.ModeVehicle != plcOperLast.ModeVehicle)
            {
                LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "Error",
                    $"hahaha   plcOper.ModeVehicle: {plcOper.ModeVehicle}, plcOperLast.ModeVehicle: {plcOperLast.ModeVehicle}"));
                if (plcOper.ModeVehicle == EnumJogVehicleMode.Auto)
                {
                    enumNowStep = EnumPlcOperationStep.VehicleModeAuto;
                }
                else if (plcOper.ModeVehicle == EnumJogVehicleMode.Manual)
                {
                    enumNowStep = EnumPlcOperationStep.VehicleModeManual;
                }
            }

            if (enumNowStep != EnumPlcOperationStep.No_Use)
            {
                return;
            }

            if (plcOper.JogElmoFunction != plcOperLast.JogElmoFunction)
            {
                switch (plcOper.JogElmoFunction)
                {
                    case EnumJogElmoFunction.Enable:
                        enumNowStep = EnumPlcOperationStep.ElmoEnable;
                        break;
                    case EnumJogElmoFunction.Disable:
                        enumNowStep = EnumPlcOperationStep.ElmoDisable;
                        break;
                    case EnumJogElmoFunction.All_Reset:
                        enumNowStep = EnumPlcOperationStep.ElmoAllReset;
                        break;
                    default:
                        enumNowStep = EnumPlcOperationStep.No_Use;
                        break;
                }
            }

            if (enumNowStep != EnumPlcOperationStep.No_Use)
            {
                return;
            }

            if (plcOper.JogRunMode != plcOperLast.JogRunMode)
            {
                switch (plcOper.JogRunMode)
                {
                    case EnumJogRunMode.Normal:
                        enumNowStep = EnumPlcOperationStep.RunModeNormal;
                        break;
                    case EnumJogRunMode.ForwardWheel:
                        enumNowStep = EnumPlcOperationStep.RunModeForwardWheel;
                        break;
                    case EnumJogRunMode.BackwardWheel:
                        enumNowStep = EnumPlcOperationStep.RunModeBackwardWheel;
                        break;
                    case EnumJogRunMode.SpinTurn:
                        enumNowStep = EnumPlcOperationStep.RunModeSpinTurn;
                        break;
                    default:
                        enumNowStep = EnumPlcOperationStep.No_Use;
                        break;
                }

            }

            if (enumNowStep != EnumPlcOperationStep.No_Use)
            {
                return;
            }

            if (plcOper.JogOperation != plcOperLast.JogOperation)
            {
                switch (plcOper.JogOperation)
                {
                    case EnumJogOperation.TurnLeft:
                        enumNowStep = EnumPlcOperationStep.JogOperationTurnLeft;
                        break;

                    case EnumJogOperation.TurnRight:
                        enumNowStep = EnumPlcOperationStep.JogOperationTurnRight;
                        break;

                    case EnumJogOperation.MoveForward:
                        enumNowStep = EnumPlcOperationStep.JogOperationMoveForward;
                        break;

                    case EnumJogOperation.MoveBackward:
                        enumNowStep = EnumPlcOperationStep.JogOperationMoveBackward;
                        break;

                    case EnumJogOperation.Stop:
                        enumNowStep = EnumPlcOperationStep.JogOperationStop;
                        break;

                    default:
                        enumNowStep = EnumPlcOperationStep.No_Use;
                        break;

                }
            }
            else
            {
                enumNowStep = enumLastStep;
            }

            return;
        }

        private void plcOperationRun_ClearAndStopSteps(ref JogPitchData plcJogPitchData, ref EnumPlcOperationStep enumLastStep, ref PlcOperation ipcOper)
        {
            switch (enumLastStep)
            {
                case EnumPlcOperationStep.VehicleModeAuto:
                case EnumPlcOperationStep.VehicleModeManual:
                    // 等做完
                    break;
                case EnumPlcOperationStep.ElmoEnable:
                case EnumPlcOperationStep.ElmoDisable:
                case EnumPlcOperationStep.ElmoAllReset:
                    LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "", $"plcJogPitchData.ElmoFunctionCompelete: {plcJogPitchData.ElmoFunctionCompelete}"));

                    break;

                case EnumPlcOperationStep.RunModeNormal:
                case EnumPlcOperationStep.RunModeForwardWheel:
                case EnumPlcOperationStep.RunModeBackwardWheel:
                case EnumPlcOperationStep.RunModeSpinTurn:
                    // 等做完
                    break;

                case EnumPlcOperationStep.JogOperationTurnLeft:
                case EnumPlcOperationStep.JogOperationTurnRight:
                    this.jogPitchForm.button_JogPitch_Turn_MouseUp(null, null);
                    plcOperationRun_bActionRunningStatus = false;
                    enumLastStep = EnumPlcOperationStep.No_Use;
                    ipcOper.JogOperation = EnumJogOperation.No_Use;
                    break;

                case EnumPlcOperationStep.JogOperationMoveForward:
                case EnumPlcOperationStep.JogOperationMoveBackward:
                    this.jogPitchForm.button_JogPitch_Move_MouseUp(null, null);
                    plcOperationRun_bActionRunningStatus = false;
                    enumLastStep = EnumPlcOperationStep.No_Use;
                    ipcOper.JogOperation = EnumJogOperation.No_Use;
                    break;

                case EnumPlcOperationStep.JogOperationStop:
                    if (plcOperationRun_bActionRunningStatus == false)
                    {
                        enumLastStep = EnumPlcOperationStep.No_Use;
                        ipcOper.JogOperation = EnumJogOperation.No_Use;
                    }
                    break;
                default:
                    //enumLastStep = EnumPlcOperationStep.No_Use;
                    break;


            }
        }

        private void plcOperationRun_ExecuteSteps()
        {
            if (plcOperationRun_enumNowStep == EnumPlcOperationStep.VehicleModeAuto)
            {
                plcOperationRun_enumLastStep = EnumPlcOperationStep.VehicleModeAuto;
                plcOperationRun_bActionRunningStatus = true;
                Task.Run(() =>
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "", $"Change mode start: from Manual to Auto."));
                    bool flag = this.mainForm.SwitchAutoStatus();
                    if (flag)
                    {
                        LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "", $"Change mode end: from Manual to Auto."));
                    }
                    else
                    {
                        LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "", $"Change mode end: from Manual to Auto Error, change back to Manual"));
                    }
                    plcOperationRun_bActionRunningStatus = false;
                    plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                });
            }
            else if (plcOperationRun_enumNowStep == EnumPlcOperationStep.VehicleModeManual)
            {
                plcOperationRun_enumLastStep = EnumPlcOperationStep.VehicleModeManual;
                plcOperationRun_bActionRunningStatus = true;
                Task.Run(() =>
                {
                    LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "", $"Change mode start: from Auto to Manual."));
                    this.mainForm.SwitchAutoStatus();
                    LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "", $"Change mode end: from Auto to Manual."));
                    plcOperationRun_bActionRunningStatus = false;
                    plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                });
            }
            else
            {
                if (Vehicle.Instance.AutoState == EnumAutoState.Manual)
                {
                    switch (plcOperationRun_enumNowStep)
                    {
                        case EnumPlcOperationStep.VehicleModeAuto:
                            break;

                        case EnumPlcOperationStep.VehicleModeManual:
                            break;

                        case EnumPlcOperationStep.ElmoEnable:
                            LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "", $"Execute ElmoEnable"));
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.ElmoEnable;
                            plcOperationRun_IpcOper.JogElmoFunction = EnumJogElmoFunction.Enable;
                            plcOperationRun_bActionRunningStatus = true;
                            Task.Run(() =>
                            {
                                this.jogPitchForm.button_JogPitch_ElmoEnable_Click(null, null);
                                double startTime = DateTime.Now.Ticks;
                                double endTime = DateTime.Now.Ticks;
                                while (true)
                                {
                                    if (this.jogPitchForm.jogPitchData.ElmoFunctionCompelete == true)
                                    {
                                        plcOperationRun_bActionRunningStatus = false;
                                        plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                                        break;
                                    }
                                    endTime = DateTime.Now.Ticks;
                                    if (endTime - startTime > 100000000)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }

                            });
                            break;

                        case EnumPlcOperationStep.ElmoDisable:
                            LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "", $"Execute ElmoDisable"));
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.ElmoDisable;
                            plcOperationRun_IpcOper.JogElmoFunction = EnumJogElmoFunction.Disable;
                            plcOperationRun_bActionRunningStatus = true;
                            Task.Run(() =>
                            {
                                this.jogPitchForm.button_JogPitch_ElmoDisable_Click(null, null);
                                double startTime = DateTime.Now.Ticks;
                                double endTime = DateTime.Now.Ticks;
                                while (true)
                                {
                                    if (this.jogPitchForm.jogPitchData.ElmoFunctionCompelete == true)
                                    {
                                        plcOperationRun_bActionRunningStatus = false;
                                        plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                                        break;
                                    }
                                    endTime = DateTime.Now.Ticks;
                                    if (endTime - startTime > 100000000)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }

                            });
                            break;

                        case EnumPlcOperationStep.ElmoAllReset:
                            LogPlcMsg(mirleLogger, new LogFormat("PlcJogPitch", "1", GetFunName(), PlcId, "", $"Execute ElmoAllReset"));
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.ElmoAllReset;
                            plcOperationRun_IpcOper.JogElmoFunction = EnumJogElmoFunction.All_Reset;
                            plcOperationRun_bActionRunningStatus = true;
                            Task.Run(() =>
                            {
                                this.jogPitchForm.button_JogpitchResetAll_Click(null, null);
                                double startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                                double endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                                while (true)
                                {
                                    if (this.jogPitchForm.jogPitchData.ElmoFunctionCompelete == true)
                                    {
                                        plcOperationRun_bActionRunningStatus = false;
                                        plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                                        break;
                                    }
                                    endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                                    if (endTime - startTime > 11000)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }

                            });
                            break;

                        case EnumPlcOperationStep.RunModeNormal:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.RunModeNormal;
                            //plcOperationRun_IpcOper.JogRunMode = EnumJogRunMode.Normal;
                            plcOperationRun_bActionRunningStatus = true;
                            Task.Run(() =>
                            {
                                this.jogPitchForm.button_JogPitch_Normal_Click(null, null);
                                plcOperationRun_bActionRunningStatus = false;
                                plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                            });
                            break;

                        case EnumPlcOperationStep.RunModeForwardWheel:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.RunModeForwardWheel;
                            //plcOperationRun_IpcOper.JogRunMode = EnumJogRunMode.ForwardWheel;
                            plcOperationRun_bActionRunningStatus = true;
                            Task.Run(() =>
                            {
                                this.jogPitchForm.button_JogPitch_ForwardWheel_Click(null, null);
                                plcOperationRun_bActionRunningStatus = false;
                                plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                            });
                            break;

                        case EnumPlcOperationStep.RunModeBackwardWheel:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.RunModeBackwardWheel;
                            //plcOperationRun_IpcOper.JogRunMode = EnumJogRunMode.BackwardWheel;
                            plcOperationRun_bActionRunningStatus = true;
                            Task.Run(() =>
                            {
                                this.jogPitchForm.button_JogPitch_BackwardWheel_Click(null, null);
                                plcOperationRun_bActionRunningStatus = false;
                                plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                            });
                            break;

                        case EnumPlcOperationStep.RunModeSpinTurn:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.RunModeSpinTurn;
                            //plcOperationRun_IpcOper.JogRunMode = EnumJogRunMode.SpinTurn;
                            plcOperationRun_bActionRunningStatus = true;
                            Task.Run(() =>
                            {
                                this.jogPitchForm.button_JogPitch_SpinTurn_Click(null, null);
                                plcOperationRun_bActionRunningStatus = false;
                                plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                            });
                            break;

                        case EnumPlcOperationStep.JogOperationTurnLeft:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.JogOperationTurnLeft;
                            plcOperationRun_IpcOper.JogOperation = EnumJogOperation.TurnLeft;
                            plcOperationRun_bActionRunningStatus = true;
                            this.jogPitchForm.button_JogPitch_TurnLeft_MouseDown(null, null);
                            break;

                        case EnumPlcOperationStep.JogOperationTurnRight:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.JogOperationTurnRight;
                            plcOperationRun_IpcOper.JogOperation = EnumJogOperation.TurnRight;
                            plcOperationRun_bActionRunningStatus = true;
                            this.jogPitchForm.button_JogPitch_TurnRight_MouseDown(null, null);
                            break;

                        case EnumPlcOperationStep.JogOperationMoveForward:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.JogOperationMoveForward;
                            plcOperationRun_IpcOper.JogOperation = EnumJogOperation.MoveForward;
                            plcOperationRun_bActionRunningStatus = true;
                            this.jogPitchForm.button_JogPitch_Forward_MouseDown(null, null);
                            break;

                        case EnumPlcOperationStep.JogOperationMoveBackward:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.JogOperationMoveBackward;
                            plcOperationRun_IpcOper.JogOperation = EnumJogOperation.MoveBackward;
                            plcOperationRun_bActionRunningStatus = true;
                            this.jogPitchForm.button_JogPitch_Backward_MouseDown(null, null);
                            break;

                        case EnumPlcOperationStep.JogOperationStop:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.JogOperationStop;
                            plcOperationRun_IpcOper.JogOperation = EnumJogOperation.Stop;
                            plcOperationRun_bActionRunningStatus = true;
                            Task.Run(() =>
                            {
                                this.jogPitchForm.button_JogPitch_STOP_Click(null, null);
                                plcOperationRun_bActionRunningStatus = false;
                                plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                            });
                            break;

                        default:
                            plcOperationRun_enumLastStep = EnumPlcOperationStep.No_Use;
                            break;

                    }
                }
            }
        }




        public string displayHmiStatus()
        {
            string temp = this.aMCProtocol.get_ItemByTag("PLCStatus").AsUInt16.ToString();
            return temp;
        }

        public void changeHmiStatus(ushort value)
        {
            this.aMCProtocol.get_ItemByTag("EquipementAction").AsUInt16 = value;
        }

        public MCProtocol GetMCProtocol() => aMCProtocol;
    }
}
