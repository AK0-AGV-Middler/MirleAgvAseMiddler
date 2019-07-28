using ClsMCProtocol;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Mirle.Agv.Controller
{
    public class PlcAgent
    {
        private MCProtocol aMCProtocol;
        private CassetteIDReader aCassetteIDReader = new CassetteIDReader();

        public string PlcId { get; set; } = "AGVPLC";
        public string Ip { get; set; } = "192.168.3.39";
        public string Port { get; set; } = "6000";
        public string LocalIp { get; set; } = "192.168.3.100";
        public string LocalPort { get; set; } = "3001";

        public long ForkCommandReadTimeout { get; set; } = 5000;
        public long ForkCommandBusyTimeout { get; set; } = 5000;
        public long ForkCommandMovingTimeout { get; set; } = 120000;

        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private Logger plcAgentLogger;
        private Logger errLogger;
        private Logger portPIOLogger;
        private Logger chargerPIOLogger;

        public PlcVehicle thePlcVehicle;

        private PLCForkCommand eventForkCommand; //發event前 先把executing commnad reference先放過來, 避免外部exevnt處理時發生null問題
        private bool clearExecutingForkCommandFlag = false;

        private Thread plcOtherControlThread;
        private Thread plcForkCommandControlThread;

        private ushort beforeBatteryPercentageInteger = 0;
        private uint alarmReadIndex = 0;

        public event EventHandler<PLCForkCommand> OnForkCommandExecutingEvent;
        public event EventHandler<PLCForkCommand> OnForkCommandFinishEvent;
        public event EventHandler<PLCForkCommand> OnForkCommandErrorEvent;
        public event EventHandler<ushort> OnBatteryPercentageChangeEvent;
        public event EventHandler<string> OnCassetteIDReadFinishEvent;

        public bool IsNeedReadCassetteID { get; set; } = true;
        public bool ConnectionState { get; private set;}

        private AlarmHandler alarmHandler;

        public PlcAgent(MCProtocol aMcProtocol, AlarmHandler aAlarmHandler)
        {
            string functionName = "PLCAgent:PLCAgent";

            thePlcVehicle = Vehicle.Instance.GetPlcVehicle();

            SetupLoggers();

            alarmHandler = aAlarmHandler;

            aMCProtocol = aMcProtocol;
            ReadXml("PLC_Config.xml");            //PLC_Config.xml
            aMCProtocol.Name = "McProtocol";

            McProtocolEventInitial();

            aMCProtocol.Open(LocalIp, LocalPort);
            aMCProtocol.ConnectToPLC(Ip, Port);

            aCassetteIDReader.Connect();

            aMCProtocol.Start();

            plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "PLC Connect Start");
        }

        private void McProtocolEventInitial()
        {
            aMCProtocol.OnDataChangeEvent += McProtocol_OnDataChangeEvent;
            aMCProtocol.ConnectEvent += McProtocol_OnConnectEvent;
            aMCProtocol.DisConnectEvent += McProtocol_OnDisConnectEvent;
        }

        private void SetupLoggers()
        {
            plcAgentLogger = loggerAgent.GetLooger("PlcAgent");
            errLogger = loggerAgent.GetLooger("Error");
            portPIOLogger = loggerAgent.GetLooger("PortPIO");
            chargerPIOLogger = loggerAgent.GetLooger("ChargerPIO");
        }

        //讀取XML
        private void ReadXml(string file_address)//Gavin 20190615
        {
            string functionName = "PLCAgent:read_xml";
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
                            PlcId = childItem.InnerText;
                            break;
                        case "IP":
                            Ip = childItem.InnerText;
                            break;
                        case "Port":
                            Port = childItem.InnerText;
                            break;
                        case "LocalIP":
                            LocalIp = childItem.InnerText;
                            break;
                        case "LocalPort":
                            LocalPort = childItem.InnerText;
                            break;
                        case "SOC_AH":
                            thePlcVehicle.Batterys.AhWorkingRange = Convert.ToDouble(childItem.InnerText);
                            break;
                        case "Ah_Reset_CCmode_Counter":
                            thePlcVehicle.Batterys.MaxResetAhCcounter = Convert.ToUInt16(childItem.InnerText);
                            break;
                        case "Ah_Reset_Timeout":
                            thePlcVehicle.Batterys.ResetAhTimeout = Convert.ToUInt32(childItem.InnerText) * 1000;
                            break;
                        case "CassetteIDReaderIP":
                            aCassetteIDReader.Ip = childItem.InnerText;
                            break;
                        case "IsNeedReadCassetteID":
                            //if (childItem.InnerText.ToLower() != "true")
                            //{
                            //    this.IsNeedReadCassetteID = false;
                            //}
                            //else
                            //{
                            //    this.IsNeedReadCassetteID = true;
                            //}
                            IsNeedReadCassetteID = bool.Parse(childItem.InnerText.Trim());

                            break;
                        case "Fork_Command_Read_Timeout":
                            ForkCommandReadTimeout = Convert.ToUInt32(childItem.InnerText) * 1000;
                            break;
                        case "Fork_Command_Busy_Timeout":
                            ForkCommandBusyTimeout = Convert.ToUInt32(childItem.InnerText) * 1000;
                            break;
                        case "Fork_Command_Moving_Timeout":
                            ForkCommandMovingTimeout = Convert.ToUInt32(childItem.InnerText) * 1000;
                            break;
                        case "Port_AutoCharge_Low_SOC":
                            thePlcVehicle.Batterys.PortAutoChargeLowSoc = Convert.ToDouble(childItem.InnerText);
                            break;
                    }

                }
            }
        }

        //private void MCProtocol_OnDataChangeEvent(string sMessage, ClsMCProtocol.clsColParameter oColParam)
        //{
        //    string functionName = "PLCAgent:MCProtocol_OnDataChangeEvent";

        //    try
        //    {
        //        int tagChangeCount = oColParam.Count();
        //        for (int i = 1; i <= tagChangeCount; i++)
        //        {
        //            try
        //            {
        //                //temp = oColParam.Item(1).DataName.ToString() + " = " + oColParam.Item(1).AsBoolean.ToString();
        //                if (oColParam.Item(i).DataName.ToString().EndsWith("_PIO"))
        //                {
        //                    this.portPIOLogger.SaveLogFile("PortPIO", "9", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean);
        //                }
        //                else if (oColParam.Item(i).DataName.ToString().EndsWith("_CPIO"))
        //                {
        //                    this.chargerPIOLogger.SaveLogFile("PortPIO", "9", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean);
        //                }
        //                else if (oColParam.Item(i).DataName.ToString().StartsWith("BeamSensor"))
        //                {
        //                    PlcBeamSensor aBeamSensor = this.APLCVehicle.dicBeamSensor[oColParam.Item(i).DataName.ToString()];
        //                    if (aBeamSensor != null)
        //                    {
        //                        if (oColParam.Item(i).DataName.ToString().EndsWith("Near"))
        //                        {
        //                            aBeamSensor.NearSignal = oColParam.Item(i).AsBoolean;
        //                        }
        //                        else if (oColParam.Item(i).DataName.ToString().EndsWith("Far"))
        //                        {
        //                            aBeamSensor.FarSignal = oColParam.Item(i).AsBoolean;
        //                        }
        //                        else
        //                        {
        //                            this.errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith Near or Far");

        //                        }
        //                    }
        //                    else
        //                    {
        //                        this.errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
        //                    }
        //                }
        //                else if (oColParam.Item(i).DataName.ToString().StartsWith("RBeamSensor"))
        //                {
        //                    //RBeamSensor Sleep read
        //                    PlcBeamSensor aBeamSensor = this.APLCVehicle.dicBeamSensor[oColParam.Item(i).DataName.ToString()];
        //                    if (aBeamSensor != null)
        //                    {
        //                        if (oColParam.Item(i).DataName.ToString().EndsWith("_Sleep"))
        //                        {
        //                            aBeamSensor.ReadSleepSignal = oColParam.Item(i).AsBoolean;
        //                        }
        //                        else
        //                        {
        //                            this.errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");

        //                        }
        //                    }
        //                    else
        //                    {
        //                        this.errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
        //                    }
        //                }
        //                else if (oColParam.Item(i).DataName.ToString().StartsWith("WBeamSensor"))
        //                {
        //                    //WBeamSensor Sleep write
        //                    PlcBeamSensor aBeamSensor = this.APLCVehicle.dicBeamSensor[oColParam.Item(i).DataName.ToString()];
        //                    if (aBeamSensor != null)
        //                    {
        //                        if (oColParam.Item(i).DataName.ToString().EndsWith("_Sleep"))
        //                        {
        //                            aBeamSensor.WriteSleepSignal = oColParam.Item(i).AsBoolean;
        //                        }
        //                        else
        //                        {
        //                            this.errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");

        //                        }
        //                    }
        //                    else
        //                    {
        //                        this.errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
        //                    }
        //                }
        //                else if (oColParam.Item(i).DataName.ToString().StartsWith("Bumper"))
        //                {
        //                    //WBeamSensor Sleep write
        //                    PlcBumper aBumper = this.APLCVehicle.dicBumper[oColParam.Item(i).DataName.ToString()];
        //                    if (aBumper != null)
        //                    {
        //                        aBumper.Signal = oColParam.Item(i).AsBoolean;
        //                    }
        //                    else
        //                    {
        //                        errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " can not find PlcEmo object");
        //                    }
        //                }
        //                else if (oColParam.Item(i).DataName.ToString().StartsWith("EMO_"))
        //                {
        //                    //WBeamSensor Sleep write
        //                    PlcEmo aPLCEMO = this.APLCVehicle.dicPlcEmos[oColParam.Item(i).DataName.ToString()];
        //                    if (aPLCEMO != null)
        //                    {
        //                        aPLCEMO.Signal = oColParam.Item(i).AsBoolean;
        //                    }
        //                    else
        //                    {
        //                        errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", oColParam.Item(i).DataName.ToString() + " can not find PlcEmo object");
        //                    }
        //                }
        //                else
        //                {
        //                    switch (oColParam.Item(i).DataName.ToString())
        //                    {
        //                        case "BatteryGotech":
        //                            if (oColParam.Item(i).AsBoolean)
        //                            {
        //                                this.APLCVehicle.APLCBatterys.BatteryType = EnumBatteryType.Gotech;
        //                            }
        //                            else
        //                            {

        //                            }
        //                            break;
        //                        case "BatteryYinda":
        //                            if (oColParam.Item(i).AsBoolean)
        //                            {
        //                                this.APLCVehicle.APLCBatterys.BatteryType = EnumBatteryType.Yinda;
        //                            }
        //                            else
        //                            {

        //                            }
        //                            break;
        //                        case "MeterVoltage":
        //                            this.APLCVehicle.APLCBatterys.MeterVoltage = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
        //                            break;
        //                        case "MeterCurrent":
        //                            this.APLCVehicle.APLCBatterys.MeterCurrent = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
        //                            break;
        //                        case "MeterWatt":
        //                            this.APLCVehicle.APLCBatterys.MeterWatt = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
        //                            break;
        //                        case "MeterWattHour":
        //                            this.APLCVehicle.APLCBatterys.MeterWattHour = this.DECToDouble(oColParam.Item(i).AsUInt32, 2);
        //                            break;
        //                        case "MeterAH":
        //                            this.APLCVehicle.APLCBatterys.MeterAH = this.DECToDouble(oColParam.Item(i).AsUInt32, 2);
        //                            break;
        //                        case "FullChargeIndex":
        //                            if (this.APLCVehicle.APLCBatterys.FullChargeIndex == 0)
        //                            {
        //                                //AGV斷電重開
        //                                this.APLCVehicle.APLCBatterys.CCModeAH = this.APLCVehicle.APLCBatterys.CCModeAH - this.APLCVehicle.APLCBatterys.AHWorkingRange;
        //                            }
        //                            else
        //                            {
        //                                this.APLCVehicle.APLCBatterys.CCModeFlag = true;
        //                                //CC Mode達到                                
        //                                this.APLCVehicle.APLCBatterys.FullChargeIndex = oColParam.Item(i).AsUInt16;

        //                            }

        //                            break;
        //                        case "ChargeStatus":
        //                            this.APLCVehicle.APLCBatterys.Charging = aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean;
        //                            if (!this.APLCVehicle.APLCBatterys.Charging)
        //                            {
        //                                ccModeAHSet();
        //                            }
        //                            break;

        //                        case "FBatteryTemperature":
        //                            this.APLCVehicle.APLCBatterys.FBatteryTemperature = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
        //                            break;
        //                        case "BBatteryTemperature":
        //                            this.APLCVehicle.APLCBatterys.BBatteryTemperature = this.DECToDouble(oColParam.Item(i).AsUInt16, 1);
        //                            break;
        //                        case "PLCAlarmIndex":
        //                            //7個alarm set/reset
        //                            for (i = 0; i < 7; i++)
        //                            {
        //                                if (this.aMCProtocol.get_ItemByTag("0" + i.ToString().Trim() + "AlarmCode").AsUInt16 != 0)
        //                                {
        //                                    //if (this.aMCProtocol.get_ItemByTag("0" + i.ToString().Trim() + "AlarmLevel").AsUInt16 == 1)
        //                                    //{
        //                                    //    //warning

        //                                    //}
        //                                    //else if (this.aMCProtocol.get_ItemByTag("0" + i.ToString().Trim() + "AlarmLevel").AsUInt16 == 2)
        //                                    //{
        //                                    //    //alarm

        //                                    //}
        //                                    //else
        //                                    //{

        //                                    //}
        //                                    //不區分alarm/warning => alarm CSV裡區分
        //                                    if (this.aMCProtocol.get_ItemByTag("0" + i.ToString().Trim() + "AlarmEvent").AsUInt16 == 1)
        //                                    {
        //                                        //set
        //                                        this.setAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + i.ToString().Trim() + "AlarmCode").AsUInt16));
        //                                    }
        //                                    else if (this.aMCProtocol.get_ItemByTag("0" + i.ToString().Trim() + "AlarmEvent").AsUInt16 == 2)
        //                                    {
        //                                        //clear
        //                                        this.resetAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + i.ToString().Trim() + "AlarmCode").AsUInt16));
        //                                    }
        //                                    else
        //                                    {

        //                                    }
        //                                }
        //                            }
        //                            break;
        //                        case "EquipementActionIndex":
        //                            this.eqActionIndex = oColParam.Item(i).AsUInt16;
        //                            break;
        //                        case "ForkReady":
        //                            this.APLCVehicle.APLCRobot.ForkReady = aMCProtocol.get_ItemByTag("ForkReady").AsBoolean;
        //                            break;
        //                        case "ForkBusy":
        //                            this.APLCVehicle.APLCRobot.ForkBusy = aMCProtocol.get_ItemByTag("ForkBusy").AsBoolean;
        //                            break;
        //                        case "ForkCommandFinish":
        //                            this.APLCVehicle.APLCRobot.ForkFinish = aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean;
        //                            break;
        //                        case "StageLoading":
        //                            this.APLCVehicle.Loading = aMCProtocol.get_ItemByTag("StageLoading").AsBoolean;
        //                            break;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //                this.errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", ex.ToString());
        //            }


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        this.errLogger.SaveLogFile("Error", "1", functionName, this.PLCId, "", ex.ToString());
        //    }
        //}

        private void McProtocol_OnDataChangeEvent(string sMessage, ClsMCProtocol.clsColParameter oColParam)
        {
            string functionName = "PLCAgent:MCProtocol_OnDataChangeEvent";

            try
            {
                int tagChangeCount = oColParam.Count();
                for (int i = 1; i <= tagChangeCount; i++)
                {

                    try
                    {
                        if (oColParam.Item(i).DataName.ToString().EndsWith("_PIO"))
                        {
                            this.portPIOLogger.SaveLogFile("PortPIO", "9", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean);
                        }
                        else if (oColParam.Item(i).DataName.ToString().EndsWith("_CPIO"))
                        {
                            this.chargerPIOLogger.SaveLogFile("PortPIO", "9", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean);
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("BeamSensor"))
                        {
                            PlcBeamSensor aBeamSensor = this.thePlcVehicle.dicBeamSensor[oColParam.Item(i).DataName.ToString()];
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
                                    errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith Near or Far");

                                }
                            }
                            else
                            {
                                errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("RBeamSensor"))
                        {
                            //RBeamSensor Sleep read
                            PlcBeamSensor aBeamSensor = this.thePlcVehicle.dicBeamSensor[oColParam.Item(i).DataName.ToString()];
                            if (aBeamSensor != null)
                            {
                                if (oColParam.Item(i).DataName.ToString().EndsWith("_Sleep"))
                                {
                                    aBeamSensor.ReadSleepSignal = oColParam.Item(i).AsBoolean;
                                }
                                else
                                {
                                    errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");

                                }
                            }
                            else
                            {
                                errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("WBeamSensor"))
                        {
                            //WBeamSensor Sleep write
                            PlcBeamSensor aBeamSensor = this.thePlcVehicle.dicBeamSensor[oColParam.Item(i).DataName.ToString()];
                            if (aBeamSensor != null)
                            {
                                if (oColParam.Item(i).DataName.ToString().EndsWith("_Sleep"))
                                {
                                    aBeamSensor.WriteSleepSignal = oColParam.Item(i).AsBoolean;
                                }
                                else
                                {
                                    errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " PLC Tag ID is not endwith _Sleep");

                                }
                            }
                            else
                            {
                                errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBeamSensor object");
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("Bumper"))
                        {
                            //WBeamSensor Sleep write
                            PlcBumper aBumper = this.thePlcVehicle.dicBumper[oColParam.Item(i).DataName.ToString()];
                            if (aBumper != null)
                            {
                                aBumper.Signal = oColParam.Item(i).AsBoolean;
                            }
                            else
                            {
                                errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCBumper object");
                            }
                        }
                        else if (oColParam.Item(i).DataName.ToString().StartsWith("EMO_"))
                        {
                            //WBeamSensor Sleep write
                            PlcEmo aPLCEMO = this.thePlcVehicle.dicPlcEmo[oColParam.Item(i).DataName.ToString()];
                            if (aPLCEMO != null)
                            {
                                aPLCEMO.Signal = oColParam.Item(i).AsBoolean;
                            }
                            else
                            {
                                errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName.ToString() + " can not find PLCEMO object");
                            }
                        }
                        else
                        {
                            switch (oColParam.Item(i).DataName.ToString())
                            {
                                case "BatteryGotech":
                                    if (oColParam.Item(i).AsBoolean)
                                    {
                                        this.thePlcVehicle.Batterys.BatteryType = EnumBatteryType.Gotech;
                                    }
                                    else
                                    {

                                    }
                                    break;
                                case "BatteryYinda":
                                    if (oColParam.Item(i).AsBoolean)
                                    {
                                        this.thePlcVehicle.Batterys.BatteryType = EnumBatteryType.Yinda;
                                    }
                                    else
                                    {

                                    }
                                    break;
                                case "MeterVoltage":
                                    this.thePlcVehicle.Batterys.MeterVoltage = this.DecToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "MeterCurrent":
                                    this.thePlcVehicle.Batterys.MeterCurrent = this.DecToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "MeterWatt":
                                    this.thePlcVehicle.Batterys.MeterWatt = this.DecToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "MeterWattHour":
                                    this.thePlcVehicle.Batterys.MeterWattHour = this.DecToDouble(oColParam.Item(i).AsUInt32, 2);
                                    break;
                                case "MeterAH":
                                    this.thePlcVehicle.Batterys.MeterAh = this.DecToDouble(oColParam.Item(i).AsUInt32, 2);
                                    break;
                                case "FullChargeIndex":
                                    if (this.thePlcVehicle.Batterys.FullChargeIndex == 0)
                                    {
                                        //AGV斷電重開
                                        this.thePlcVehicle.Batterys.CcModeAh = this.thePlcVehicle.Batterys.CcModeAh - this.thePlcVehicle.Batterys.AhWorkingRange;
                                    }
                                    else
                                    {
                                        this.thePlcVehicle.Batterys.CcModeFlag = true;
                                        //CC Mode達到                                
                                        this.thePlcVehicle.Batterys.FullChargeIndex = oColParam.Item(i).AsUInt16;

                                    }

                                    break;
                                case "ChargeStatus":
                                    this.thePlcVehicle.Batterys.Charging = aMCProtocol.get_ItemByTag("ChargeStatus").AsBoolean;
                                    if (!this.thePlcVehicle.Batterys.Charging)
                                    {
                                        SetupCcModeAh();
                                    }
                                    break;

                                case "FBatteryTemperature":
                                    this.thePlcVehicle.Batterys.FBatteryTemperature = this.DecToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "BBatteryTemperature":
                                    this.thePlcVehicle.Batterys.BBatteryTemperature = this.DecToDouble(oColParam.Item(i).AsUInt16, 1);
                                    break;
                                case "PLCAlarmIndex":
                                    //7個alarm set/reset
                                    try
                                    {
                                        for (int j = 1; j <= 7; j++)
                                        {
                                            if (this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16 != 0)
                                            {
                                                //if (this.aMCProtocol.get_ItemByTag("0" + i.ToString().Trim() + "AlarmLevel").AsUInt16 == 1)
                                                //{
                                                //    //warning

                                                //}
                                                //else if (this.aMCProtocol.get_ItemByTag("0" + i.ToString().Trim() + "AlarmLevel").AsUInt16 == 2)
                                                //{
                                                //    //alarm

                                                //}
                                                //else
                                                //{

                                                //}
                                                //不區分alarm/warning => alarm CSV裡區分
                                                if (this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmEvent").AsUInt16 == 1)
                                                {
                                                    //set
                                                    this.SetAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16));
                                                }
                                                else if (this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmEvent").AsUInt16 == 2)
                                                {
                                                    //clear
                                                    this.ResetAlarm(Convert.ToInt32(this.aMCProtocol.get_ItemByTag("0" + j.ToString().Trim() + "AlarmCode").AsUInt16));
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
                                            plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC success");
                                        }
                                        else
                                        {
                                            plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "AlarmReadIndex = " + alarmReadIndex.ToString() + " write to PLC fail");
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        this.errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", ex.ToString());
                                    }

                                    //Console.Out.Write("alarm");
                                    break;
                                case "EquipementActionIndex":
                                    this.eqActionIndex = oColParam.Item(i).AsUInt16;
                                    break;
                                case "ForkReady":
                                    this.thePlcVehicle.Robot.ForkReady = aMCProtocol.get_ItemByTag("ForkReady").AsBoolean;
                                    break;
                                case "ForkBusy":
                                    this.thePlcVehicle.Robot.ForkBusy = aMCProtocol.get_ItemByTag("ForkBusy").AsBoolean;
                                    break;
                                case "ForkCommandFinish":
                                    this.thePlcVehicle.Robot.ForkFinish = aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean;
                                    break;
                                case "StageLoading":
                                    this.thePlcVehicle.Loading = aMCProtocol.get_ItemByTag("StageLoading").AsBoolean;
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", oColParam.Item(i).DataName + ":" + ex.ToString());

                    }


                }
            }
            catch (Exception ex)
            {
                this.errLogger.SaveLogFile("Error", "1", functionName, this.PlcId, "", ex.ToString());
            }
        }

        private void SetupCcModeAh()
        {

            if (this.thePlcVehicle.Batterys.CcModeFlag)
            {
                this.thePlcVehicle.Batterys.CcModeAh = this.DecToDouble(aMCProtocol.get_ItemByTag("MeterAH").AsUInt32, 2);
                //判斷CCModeCounter
                if (this.thePlcVehicle.Batterys.MaxResetAhCcounter <= this.thePlcVehicle.Batterys.CcModeCounter)
                {
                    this.thePlcVehicle.Batterys.CcModeCounter = 0;
                    this.SetMeterAHToZero();
                }
                else
                {
                    if (this.thePlcVehicle.Batterys.CcModeAh > 0.5)
                    {
                        this.thePlcVehicle.Batterys.CcModeCounter = 0;
                        this.SetMeterAHToZero();
                    }
                    this.thePlcVehicle.Batterys.CcModeCounter++;
                }
                this.thePlcVehicle.Batterys.CcModeFlag = false;

            }
            else
            {
                //
            }

        }

        private double DecToDouble(long inputNum, int length)
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

        private void McProtocol_OnConnectEvent(string message)
        {
            string functionName = "PLCAgent:MCProtocol_OnConnectEvent";
            plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "PLC is connected");
            this.ConnectionState = true;

            plcOtherControlThread = new Thread(PlcOtherControlRun);
            plcForkCommandControlThread = new Thread(PlcForkCommandControlRun);
            plcOtherControlThread.Start();
            plcForkCommandControlThread.Start();

            this.WriteCurrentDateTime();
            this.WriteIPCStatus(EnumIPCStatus.Initial);
            this.WriteAlarmWarningStatus(false, false);
            this.WriteIPCReady(true);
        }

        private int beforeDay = DateTime.Now.Day;
        public void PlcOtherControlRun()
        {
            while (true)
            {
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
                if (this.thePlcVehicle.Batterys.SetMeterAhToZeroFlag)
                {
                    //判斷歸０完成　=> 電表ＡＨ變成0, 所以原先值SetMeterAHToZeroAH　應該要反映到CCmode AH值
                    if (this.thePlcVehicle.Batterys.MeterAh < 0.5 && this.thePlcVehicle.Batterys.MeterAh > -0.5)
                    {
                        this.thePlcVehicle.Batterys.CcModeAh = (0 - this.thePlcVehicle.Batterys.SetMeterAhToZeroAh) + this.thePlcVehicle.Batterys.CcModeAh;
                        this.thePlcVehicle.Batterys.SetMeterAhToZeroFlag = false;
                    }
                    else
                    {
                        if (this.thePlcVehicle.Batterys.SwBatteryAhSetToZero.ElapsedMilliseconds > this.thePlcVehicle.Batterys.ResetAhTimeout)
                        {
                            //Raise Warning
                            this.thePlcVehicle.Batterys.SetMeterAhToZeroFlag = false;

                        }
                    }

                }

                //Battery SOC => 寫在MeterAH發生變化事件裡
                //這裡處理發事件
                //OnBatteryPercentageChangeEvent
                UInt16 currPercentage = Convert.ToUInt16(this.thePlcVehicle.Batterys.Percentage);
                if (currPercentage != this.beforeBatteryPercentageInteger)
                {
                    this.beforeBatteryPercentageInteger = currPercentage;
                    OnBatteryPercentageChangeEvent?.Invoke(this, currPercentage);
                }

                //Safety Action 判斷
                //決定safety action
                //Bumper -> BeamSensor
                //EMO就算Safety Disable也要生效 => MoveControl會直接看EMO訊號,直接disable各軸
                if (thePlcVehicle.SafetyDisable)
                {
                    this.thePlcVehicle.VehicleSafetyAction = EnumVehicleSafetyAction.Normal;
                }
                else
                {
                    //Bumper
                    if (this.DetectBumperDisable())
                    {
                        //有Bumper是disable最多只能慢速
                        if (this.DetectBumper())
                        {
                            this.thePlcVehicle.VehicleSafetyAction = EnumVehicleSafetyAction.Stop;
                        }
                        else
                        {
                            this.thePlcVehicle.VehicleSafetyAction = EnumVehicleSafetyAction.LowSpeed;
                            //續看beam sensor near
                            if (DetectBeamSensorNear())
                            {
                                this.thePlcVehicle.VehicleSafetyAction = EnumVehicleSafetyAction.Stop;
                            }
                            else
                            {
                                //不用去看Far => 因為不能用Normal速度
                                this.thePlcVehicle.VehicleSafetyAction = EnumVehicleSafetyAction.LowSpeed;
                            }
                        }
                    }
                    else
                    {
                        if (this.DetectBumper())
                        {
                            this.thePlcVehicle.VehicleSafetyAction = EnumVehicleSafetyAction.Stop;
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
                            if (thePlcVehicle.BeamSensorAutoSleep)
                            {
                                frontSleepFlag = (!thePlcVehicle.MoveFront) || thePlcVehicle.FrontBeamSensorDisable || thePlcVehicle.SafetyDisable;
                                backSleepFlag = (!thePlcVehicle.MoveBack) || thePlcVehicle.FrontBeamSensorDisable || thePlcVehicle.SafetyDisable;
                                leftSleepFlag = (!thePlcVehicle.MoveLeft) || thePlcVehicle.FrontBeamSensorDisable || thePlcVehicle.SafetyDisable;
                                rightSleepFlag = (!thePlcVehicle.MoveRight) || thePlcVehicle.FrontBeamSensorDisable || thePlcVehicle.SafetyDisable;

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

                            if (thePlcVehicle.MoveFront == true)
                            {
                                //前方
                                result = DecideSafetyActionBySideBeamSensor(result, thePlcVehicle.listFrontBeamSensor, this.thePlcVehicle.FrontBeamSensorDisable);
                            }

                            if (thePlcVehicle.MoveBack == true)
                            {
                                //後方
                                result = DecideSafetyActionBySideBeamSensor(result, thePlcVehicle.listBackBeamSensor, this.thePlcVehicle.BackBeamSensorDisable);
                            }

                            if (thePlcVehicle.MoveLeft == true)
                            {
                                //左方
                                result = DecideSafetyActionBySideBeamSensor(result, thePlcVehicle.listLeftBeamSensor, this.thePlcVehicle.LeftBeamSensorDisable);
                            }

                            if (thePlcVehicle.MoveRight == true)
                            {
                                //右方
                                result = DecideSafetyActionBySideBeamSensor(result, thePlcVehicle.listRightBeamSensor, this.thePlcVehicle.RightBeamSensorDisable);
                            }

                            this.thePlcVehicle.VehicleSafetyAction = result;

                        }
                    }
                }



                System.Threading.Thread.Sleep(1);
            }
        }

        private EnumVehicleSafetyAction DecideSafetyActionBySideBeamSensor(EnumVehicleSafetyAction initAction, List<PlcBeamSensor> listBeamSensor, Boolean SideBeamSensorDisable)
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

        private bool DetectEMO()
        {
            Boolean emoFlag = false;
            foreach (PlcEmo aPlcEmo in thePlcVehicle.listPlcEmo)
            {
                if (aPlcEmo.Disable == false && aPlcEmo.Signal == false)
                {
                    emoFlag = true;
                    break;
                }
            }

            return emoFlag;
        }

        private bool DetectBumper()
        {
            Boolean bumperFlag = false;
            foreach (PlcBumper aPLCBumper in thePlcVehicle.listBumper)
            {
                if (aPLCBumper.Disable == false && aPLCBumper.Signal == false)
                {
                    bumperFlag = true;
                    break;
                }
            }

            return bumperFlag;
        }

        private bool DetectBeamSensorNear()
        {
            Boolean result = false;
            //front
            if (this.thePlcVehicle.MoveFront)
            {
                if (DetectSideBeamSensorNear(this.thePlcVehicle.listFrontBeamSensor))
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
            if (this.thePlcVehicle.MoveBack)
            {
                if (DetectSideBeamSensorNear(this.thePlcVehicle.listBackBeamSensor))
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
            if (this.thePlcVehicle.MoveLeft)
            {
                if (DetectSideBeamSensorNear(this.thePlcVehicle.listLeftBeamSensor))
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
            if (this.thePlcVehicle.MoveRight)
            {
                if (DetectSideBeamSensorNear(this.thePlcVehicle.listRightBeamSensor))
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

        private bool DetectBeamSensorFar()
        {
            Boolean result = false;
            //front
            if (this.thePlcVehicle.MoveFront)
            {
                if (DetectSideBeamSensorFar(this.thePlcVehicle.listFrontBeamSensor))
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
            if (this.thePlcVehicle.MoveBack)
            {
                if (DetectSideBeamSensorFar(this.thePlcVehicle.listBackBeamSensor))
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
            if (this.thePlcVehicle.MoveLeft)
            {
                if (DetectSideBeamSensorFar(this.thePlcVehicle.listLeftBeamSensor))
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
            if (this.thePlcVehicle.MoveRight)
            {
                if (DetectSideBeamSensorFar(this.thePlcVehicle.listRightBeamSensor))
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

        private bool DetectSideBeamSensorNear(List<PlcBeamSensor> listBeamSensor)
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

        private bool DetectSideBeamSensorFar(List<PlcBeamSensor> listBeamSensor)
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

        private bool DetectSideBeamSensorDisable(List<PlcBeamSensor> listBeamSensor)
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

        private bool DetectBumperDisable()
        {
            Boolean disableFlag = false;
            foreach (PlcBumper aPLCBeamSensor in thePlcVehicle.listBumper)
            {
                if (aPLCBeamSensor.Disable == true)
                {
                    disableFlag = true;
                    break;
                }
            }

            return disableFlag;
        }

        private bool DetectEMODisable()
        {
            Boolean disableFlag = false;

            foreach (PlcEmo aPlcEmo in thePlcVehicle.listPlcEmo)
            {
                if (aPlcEmo.Disable == true)
                {
                    disableFlag = true;
                    break;
                }
            }

            return disableFlag;
        }

        private void McProtocol_OnDisConnectEvent(string message)
        {
            string functionName = "PLCAgent:MCProtocol_OnDisConnectEvent";
            this.ConnectionState = false;
            plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "PLC is disconnected");

        }

        public void SetBeamSensorSleepOn(EnumVehicleSide aSide)
        {
            string functionName = "PLCAgent:SetBeamSensorSleep";
            List<PlcBeamSensor> listSideBeamSensor = null;
            switch (aSide)
            {
                case EnumVehicleSide.Forward:
                    listSideBeamSensor = this.thePlcVehicle.listFrontBeamSensor;
                    break;
                case EnumVehicleSide.Backward:
                    listSideBeamSensor = this.thePlcVehicle.listBackBeamSensor;
                    break;
                case EnumVehicleSide.Left:
                    listSideBeamSensor = this.thePlcVehicle.listLeftBeamSensor;
                    break;
                case EnumVehicleSide.Right:
                    listSideBeamSensor = this.thePlcVehicle.listRightBeamSensor;
                    break;
                case EnumVehicleSide.None:
                    //全開

                    break;
            }

            Boolean writeFlag = false;
            if (listSideBeamSensor == null)
            {
                //全開 安全起見 全開
                foreach (PlcBeamSensor aPLCBeamSensor in this.thePlcVehicle.listFrontBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.thePlcVehicle.listBackBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;
                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.thePlcVehicle.listLeftBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.thePlcVehicle.listRightBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                if (writeFlag)
                {
                    if (this.aMCProtocol.WritePLC())
                    {
                        plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "Set All Beam Sensor Sleep Off(Awake) Success");
                    }
                    else
                    {
                        plcAgentLogger.SaveLogFile("PlcAgent", "1", functionName, this.PlcId, "", "Set All Beam Sensor Sleep Off(Awake) Fail");


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
                        LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to on success.");
                        loggerAgent.LogMsg("PlcAgent", logFormat);

                    }
                    else
                    {
                        LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to on fail.");
                        loggerAgent.LogMsg("PlcAgent", logFormat);

                    }
                }
            }
        }

        private bool BeamSensorWriteSleep(PlcBeamSensor aPLCBeamSensor, bool flag)
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
            string functionName = "PLCAgent:SetBeamSensorSleepOff";
            List<PlcBeamSensor> listSideBeamSensor = null;
            switch (aSide)
            {
                case EnumVehicleSide.Forward:
                    listSideBeamSensor = this.thePlcVehicle.listFrontBeamSensor;
                    break;
                case EnumVehicleSide.Backward:
                    listSideBeamSensor = this.thePlcVehicle.listBackBeamSensor;
                    break;
                case EnumVehicleSide.Left:
                    listSideBeamSensor = this.thePlcVehicle.listLeftBeamSensor;
                    break;
                case EnumVehicleSide.Right:
                    listSideBeamSensor = this.thePlcVehicle.listRightBeamSensor;
                    break;
                case EnumVehicleSide.None:
                    //全開

                    break;
            }

            Boolean writeFlag = false;
            if (listSideBeamSensor == null)
            {
                //全開 安全起見 全開
                foreach (PlcBeamSensor aPLCBeamSensor in this.thePlcVehicle.listFrontBeamSensor)
                {

                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.thePlcVehicle.listBackBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;
                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.thePlcVehicle.listLeftBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                foreach (PlcBeamSensor aPLCBeamSensor in this.thePlcVehicle.listRightBeamSensor)
                {
                    if (BeamSensorWriteSleep(aPLCBeamSensor, false)) writeFlag = true;

                }

                if (writeFlag)
                {
                    if (this.aMCProtocol.WritePLC())
                    {
                        LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set All Beam Sensor Sleep Off(Awake) Success");
                        loggerAgent.LogMsg("PlcAgent", logFormat);

                    }
                    else
                    {
                        LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set All Beam Sensor Sleep Off(Awake) Fail");
                        loggerAgent.LogMsg("PlcAgent", logFormat);


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
                        LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to off success.");
                        loggerAgent.LogMsg("PlcAgent", logFormat);

                    }
                    else
                    {
                        LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", " Side Beam Sensor Sleep to off fail.");
                        loggerAgent.LogMsg("PlcAgent", logFormat);

                    }
                }


            }

        }

        public void WriteCurrentDateTime()
        {
            string functionName = "PLCAgent:WriteCurrentDateTime";
            String datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
            //LogRecord(NLog.LogLevel.Info, "WriteDateTimeCalibrationReport", "DateTime: " + datetime);
            this.aMCProtocol.get_ItemByTag("YearMonth").AsHex = datetime.Substring(2, 4);
            this.aMCProtocol.get_ItemByTag("DayHour").AsHex = datetime.Substring(6, 4);
            this.aMCProtocol.get_ItemByTag("MinSec").AsHex = datetime.Substring(10, 4);
            if (this.aMCProtocol.WritePLC())
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Current DateTime success, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);

            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Set Current DateTime fail, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);

            }


        }

        public bool WriteForkCommandInfo(ushort commandNo, EnumForkCommand enumForkCommand, string stageNo, EnumStageDirection direction, bool eQIF, ushort forkSpeed)
        {
            string functionName = "PLCAgent:WriteForkCommand";
            this.aMCProtocol.get_ItemByTag("CommandNo").AsUInt16 = commandNo;

            this.aMCProtocol.get_ItemByTag("OperationType").AsUInt16 = Convert.ToUInt16(enumForkCommand);

            this.aMCProtocol.get_ItemByTag("StageNo").AsUInt16 = Convert.ToUInt16(stageNo);
            this.aMCProtocol.get_ItemByTag("StageDirection").AsUInt16 = Convert.ToUInt16(direction);
            this.aMCProtocol.get_ItemByTag("EQPIO").AsBoolean = eQIF;
            this.aMCProtocol.get_ItemByTag("ForkSpeed").AsUInt16 = forkSpeed;
            if (this.aMCProtocol.WritePLC())
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command success, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);

                return true;
            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Send out a Fork Command fail, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);

                return true;
            }


        }

        public bool WriteForkCommandActionBit(EnumForkCommandExecutionType aEnumForkCommandExecutionType, bool Onflag)
        {
            string functionName = "PLCAgent:WriteForkCommand";
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
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Execute Fork Command(" + aEnumForkCommandExecutionType.ToString() + " = " + Convert.ToString(Onflag) + ") success, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);

                result = true;
            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Execute Fork Command(" + aEnumForkCommandExecutionType.ToString() + " = " + Convert.ToString(Onflag) + ") fail, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);


            }
            return result;

        }

        public bool ChargeStartCommand(EnumChargeDirection aEnumChargeDirection)
        {
            string functionName = "PLCAgent:ChargeStartCommand";
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
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Start Command success, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);

                result = true;
            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Start Command fail, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);


            }
            return result;

        }

        public bool ChargeStopCommand()
        {
            string functionName = "PLCAgent:ChargeStopCommand";
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("LeftChargeRequest").AsBoolean = false;
            this.aMCProtocol.get_ItemByTag("RightChargeRequest").AsBoolean = false;

            if (this.aMCProtocol.WritePLC())
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Stop Command success, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);

                result = true;
            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Charge Stop Command fail, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);


            }
            return result;

        }

        public bool WriteAGVCOnline(bool OnlineFlag)
        {
            string functionName = "PLCAgent:WriteAGVCOnline";
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("AGVCOnline").AsBoolean = OnlineFlag;

            if (this.aMCProtocol.WritePLC())
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write AGVCOnline = " + Convert.ToString(OnlineFlag) + " success");
                loggerAgent.LogMsg("PlcAgent", logFormat);
                result = true;
            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write AGVCOnline = " + Convert.ToString(OnlineFlag) + " fail");
                loggerAgent.LogMsg("PlcAgent", logFormat);

            }
            return result;
        }

        public bool WriteAlarmWarningStatus(bool alarmStatus, Boolean warningStatus)
        {
            string functionName = "PLCAgent:WriteAlarmWarningStatus";
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("IPCAlarmStatus").AsBoolean = alarmStatus;
            this.aMCProtocol.get_ItemByTag("IPCWarningStatus").AsBoolean = warningStatus;
            if (this.aMCProtocol.WritePLC())
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlarmStatus = " + Convert.ToString(alarmStatus) + ", IPCWarningStatus = " + Convert.ToString(warningStatus) + " success");
                loggerAgent.LogMsg("PlcAgent", logFormat);

                result = true;
            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCAlarmStatus = " + Convert.ToString(alarmStatus) + ", IPCWarningStatus = " + Convert.ToString(warningStatus) + " fail");
                loggerAgent.LogMsg("PlcAgent", logFormat);

            }
            return result;
        }

        public bool WriteIPCReady(bool readyStatus)
        {
            string functionName = "PLCAgent:WriteIPCReady";
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("IPCReady").AsBoolean = readyStatus;
            if (this.aMCProtocol.WritePLC())
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCReady = " + Convert.ToString(readyStatus) + " success");
                loggerAgent.LogMsg("PlcAgent", logFormat);

                result = true;
            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCReady = " + Convert.ToString(readyStatus) + " fail");
                loggerAgent.LogMsg("PlcAgent", logFormat);

            }
            return result;
        }

        public bool WriteIPCStatus(EnumIPCStatus aEnumIPCStatus)
        {
            string functionName = "PLCAgent:WriteIPCStatus";
            Boolean result = false;

            this.aMCProtocol.get_ItemByTag("IPCStatus").AsUInt16 = Convert.ToUInt16(aEnumIPCStatus);
            if (this.aMCProtocol.WritePLC())
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCStatus = " + Convert.ToString(aEnumIPCStatus) + " success");
                loggerAgent.LogMsg("PlcAgent", logFormat);

                result = true;
            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "Write IPCStatus = " + Convert.ToString(aEnumIPCStatus) + " fail");
                loggerAgent.LogMsg("PlcAgent", logFormat);
            }
            return result;
        }

        private ushort ipcAliveCounter = 1;
        public void WriteIPCAlive()
        {
            //heart beat量大不記log
            this.aMCProtocol.get_ItemByTag("IPCAlive").AsUInt16 = ipcAliveCounter;

            this.aMCProtocol.WritePLC();
            ipcAliveCounter++;
            ipcAliveCounter = Convert.ToUInt16(Convert.ToInt32(ipcAliveCounter) % 65536);
            //System.Threading.Thread.Sleep(1000);
        }

        private ushort eqActionIndex = 0;

        public void WritePLCAlarmReset()
        {
            Task.Run(() =>
            {
                this.aMCProtocol.get_ItemByTag("EquipementAction").AsUInt16 = 10;
                this.aMCProtocol.WritePLC();
                System.Threading.Thread.Sleep(1000);
                eqActionIndex++;
                eqActionIndex = Convert.ToUInt16(Convert.ToInt32(eqActionIndex) % 65536);
                this.aMCProtocol.get_ItemByTag("EquipementActionIndex").AsUInt16 = eqActionIndex;
                this.aMCProtocol.WritePLC();
            });


        }

        public void WritePLCBuzzserStop()
        {
            Task.Run(() =>
            {
                this.aMCProtocol.get_ItemByTag("EquipementAction").AsUInt16 = 11;
                this.aMCProtocol.WritePLC();
                System.Threading.Thread.Sleep(1000);
                eqActionIndex++;
                eqActionIndex = Convert.ToUInt16(Convert.ToInt32(eqActionIndex) % 65536);
                this.aMCProtocol.get_ItemByTag("EquipementActionIndex").AsUInt16 = eqActionIndex;
                this.aMCProtocol.WritePLC();
            });
        }

        public void SetMeterAHToZero()
        {
            this.thePlcVehicle.Batterys.SetMeterAhToZeroFlag = true;
            //this.this.APLCVehicle.PLCBatterys.SetMeterAHToZeroAH = this.this.APLCVehicle.PLCBatterys.MeterAH;
            Task.Run(() =>
            {
                this.aMCProtocol.get_ItemByTag("MeterAHToZero").AsBoolean = true;
                this.aMCProtocol.WritePLC();
                System.Threading.Thread.Sleep(1000);
                this.aMCProtocol.get_ItemByTag("MeterAHToZero").AsBoolean = false;
                this.aMCProtocol.WritePLC();
            });
        }

        public bool WriteVehicleDirection(bool spinLeft, bool spinRight, bool TraverseLeft, bool TrverseRight, bool SteeringFL, bool SteeringFR, bool SteeringBL, bool SteeringBR, bool Forward, bool Backward)
        {

            string functionName = "PLCAgent:WriteVehicleDirection";
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
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "WriteVehicleDirection fail, ");
                loggerAgent.LogMsg("PlcAgent", logFormat);
            }
            return result;

        }

        private void TaskRunForkCommandStart()
        {

        }

        private Stopwatch swAlive = new Stopwatch();
        //private ForkCommand executingForkCommand = null;
        public bool IsForkCommandExist()
        {
            if (this.thePlcVehicle.Robot.ExecutingCommand == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetErrorReason()
        {
            try
            {
                PLCForkCommand aForkcommand = this.thePlcVehicle.Robot.ExecutingCommand; //避免executingForkCommand同時被clear
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

        public bool AddForkComand(PLCForkCommand aForkCommand)
        {
            string functionName = "PLCAgent:AddForkComand";
            if (this.thePlcVehicle.Robot.ExecutingCommand == null)
            {
                if (aForkCommand.ForkCommandState == EnumForkCommandState.Queue)
                {
                    this.thePlcVehicle.Robot.ExecutingCommand = aForkCommand;
                    return true;
                }
                else
                {
                    LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, aForkCommand.ForkCommandState = " + Convert.ToString(this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandState) + ", is not Queue.");
                    loggerAgent.LogMsg("PlcAgent", logFormat);

                    return false;
                }
            }
            else
            {
                LogFormat logFormat = new LogFormat("PlcAgent", "1", functionName, PlcId, "Empty", "AddForkComand fail, executingForkCommand is not null.");
                loggerAgent.LogMsg("PlcAgent", logFormat);

                return false;
            }
        }

        private void SetAlarm(int alarmCode)
        {
            string functionName = "PLCAgent:setAlarm";
            if (this.alarmHandler != null)
            {
                this.alarmHandler.SetAlarm(alarmCode);
            }
            else
            {

            }

        }

        private void ResetAlarm(int alarmCode)
        {
            if (this.alarmHandler != null)
            {
                this.alarmHandler.ResetAlarm(alarmCode);
            }
            else
            {

            }

        }

        public void TriggerCassetteIDReader(ref string CassetteID)
        {
            string strCassetteID = "ERROR";
            this.aCassetteIDReader.ReadBarcode(ref strCassetteID); //成功或失敗都要發ReadFinishEvent,外部用CassetteID來區別成功或失敗
            this.thePlcVehicle.CassetteID = strCassetteID;
            CassetteID = strCassetteID;
            OnCassetteIDReadFinishEvent?.Invoke(this, strCassetteID);
        }

        public void ClearExecutingForkCommand()
        {
            clearExecutingForkCommandFlag = true;
        }

        public void PlcForkCommandControlRun()
        {
            while (true)
            {
                //Ready
                this.thePlcVehicle.Robot.ForkReady = this.aMCProtocol.get_ItemByTag("ForkReady").AsBoolean;

                //Fork Command
                if (clearExecutingForkCommandFlag)
                {
                    clearExecutingForkCommandFlag = false;
                    this.thePlcVehicle.Robot.ExecutingCommand = null;
                }

                if (this.thePlcVehicle.Robot.ExecutingCommand != null)
                {
                    Stopwatch sw = new Stopwatch();
                    switch (this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandState)
                    {
                        case EnumForkCommandState.Queue:
                            //送出指令                              
                            if (this.aMCProtocol.get_ItemByTag("ForkReady").AsBoolean && this.aMCProtocol.get_ItemByTag("ForkBusy").AsBoolean == false)
                            {
                                this.thePlcVehicle.Robot.ExecutingCommand.Reason = "";
                                this.WriteForkCommandInfo(Convert.ToUInt16(this.thePlcVehicle.Robot.ExecutingCommand.CommandNo), this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandType, this.thePlcVehicle.Robot.ExecutingCommand.StageNo, this.thePlcVehicle.Robot.ExecutingCommand.Direction, this.thePlcVehicle.Robot.ExecutingCommand.Eqif, this.thePlcVehicle.Robot.ExecutingCommand.ForkSpeed);

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
                                        this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                        this.thePlcVehicle.Robot.ExecutingCommand.Reason = "ForkCommandNG";
                                        //Raise Alarm
                                        //this.aAlarmHandler.SetAlarm(270001);
                                        this.SetAlarm(270001);
                                        eventForkCommand = this.thePlcVehicle.Robot.ExecutingCommand;
                                        OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);

                                        break;
                                    }
                                    else
                                    {
                                        if (sw.ElapsedMilliseconds < ForkCommandReadTimeout)
                                        {
                                            System.Threading.Thread.Sleep(20);
                                        }
                                        else
                                        {
                                            //read time out
                                            //this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                            //System.Threading.Thread.Sleep(1000);
                                            //this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, true);
                                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);
                                            this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                            this.thePlcVehicle.Robot.ExecutingCommand.Reason = "Fork Command Read timeout";
                                            //this.aAlarmHandler.SetAlarm(270002);
                                            this.SetAlarm(270002);
                                            eventForkCommand = this.thePlcVehicle.Robot.ExecutingCommand;
                                            OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                            //Raise Alarm
                                            //return;
                                            break;
                                        }

                                    }


                                }
                                sw.Stop();

                                if (this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandState == EnumForkCommandState.Error)
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
                                            System.Threading.Thread.Sleep(20);
                                        }
                                        else
                                        {
                                            this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, false);
                                            this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Error;
                                            this.thePlcVehicle.Robot.ExecutingCommand.Reason = "ForkNotBusy timeout";
                                            //this.aAlarmHandler.SetAlarm(270003);
                                            this.SetAlarm(270003);
                                            eventForkCommand = this.thePlcVehicle.Robot.ExecutingCommand;
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
                                if (this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandState == EnumForkCommandState.Error)
                                {
                                    break;
                                }
                                this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Executing;
                                eventForkCommand = this.thePlcVehicle.Robot.ExecutingCommand;
                                OnForkCommandExecutingEvent?.Invoke(this, eventForkCommand);
                                this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, false);
                                System.Threading.Thread.Sleep(1000);

                            }
                            else
                            {
                                this.thePlcVehicle.Robot.ExecutingCommand.Reason = "ForkReady or ForkBusy is not correct";
                            }

                            break;
                        case EnumForkCommandState.Executing:
                            sw.Reset();
                            sw.Start();
                            while (this.aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean == false)
                            {
                                if (sw.ElapsedMilliseconds < this.ForkCommandMovingTimeout)
                                {
                                    System.Threading.Thread.Sleep(500);
                                }
                                else
                                {

                                    //executingForkCommand.ForkCommandState = EnumForkCommandState.Error;
                                    this.thePlcVehicle.Robot.ExecutingCommand.Reason = "ForkCommand Moving Timeout";
                                    //Raise Alarm?Warning?   
                                    //this.aAlarmHandler.SetAlarm(270004);
                                    this.SetAlarm(270004);
                                    eventForkCommand = this.thePlcVehicle.Robot.ExecutingCommand;
                                    OnForkCommandErrorEvent?.Invoke(this, eventForkCommand);
                                    break;
                                }
                            }
                            sw.Stop();

                            if (this.aMCProtocol.get_ItemByTag("ForkCommandFinish").AsBoolean)
                            {
                                this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, true);
                                System.Threading.Thread.Sleep(1000);
                                this.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, false);
                                this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandState = EnumForkCommandState.Finish;
                            }
                            else
                            {

                            }

                            break;
                        case EnumForkCommandState.Finish:
                            //OnForkCommandFinishEvent?.Invoke(this, executingForkCommand);
                            if (this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandType == EnumForkCommand.Load)
                            {
                                //要讀完CasetteID才算完成
                                if (this.IsNeedReadCassetteID)
                                {
                                    String cassetteID = "ERROR";
                                    this.aCassetteIDReader.ReadBarcode(ref cassetteID); //成功或失敗都要發ReadFinishEvent,外部用CassetteID來區別成功或失敗
                                    this.thePlcVehicle.CassetteID = cassetteID;
                                    OnCassetteIDReadFinishEvent?.Invoke(this, cassetteID);
                                }
                                else
                                {

                                }
                            }
                            else if (this.thePlcVehicle.Robot.ExecutingCommand.ForkCommandType == EnumForkCommand.Unload)
                            {
                                this.thePlcVehicle.CassetteID = "";
                            }
                            else
                            {

                            }

                            ////ForkCommand aForkCommand = executingForkCommand;
                            eventForkCommand = this.thePlcVehicle.Robot.ExecutingCommand;
                            OnForkCommandFinishEvent?.Invoke(this, eventForkCommand);
                            clearExecutingForkCommandFlag = true;
                            break;
                    }

                }
                System.Threading.Thread.Sleep(5);
            }


        }
    }
}
