using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model.Configs
{
    public class PlcConfig
    {
        public string ID { get; set; } = "AGVPLC";
        public string IP { get; set; } = "192.168.1.39";
        public string Port { get; set; } = "6000";
        public string LocalIp { get; set; } = "192.168.1.120";
        public string LocalPort { get; set; } = "3018";
        public string CassetteIDReaderIP { get; set; } = "192.168.1.123";
        public double Battery_Cell_Low_Voltage { get; set; } = 2.8;
        public double CCMode_Stop_Voltage { get; set; } = 61;
        public uint Charging_Off_Delay { get; set; } = 0;
        public uint Battery_Logger_Interval { get; set; } = 3000; //*1000
        public uint Batterys_Charging_Time_Out { get; set; } = 600000; // min //*60000
        public double SOC_AH { get; set; } = 23;
        public ushort Ah_Reset_CCmode_Counter { get; set; } = 50;
        public double Port_AutoCharge_Low_SOC { get; set; } = 50;
        public double Port_AutoCharge_High_SOC { get; set; } = 70;
        public uint Ah_Reset_Timeout { get; set; } = 10000; //*1000   
        public bool IsNeedReadCassetteID { get; set; } = true;
        public uint Fork_Command_Read_Timeout { get; set; } = 50000;//*1000
        public uint Fork_Command_Busy_Timeout { get; set; } = 30000;//*1000
        public uint Fork_Command_Moving_Timeout { get; set; } = 120000;//*1000
    }
}
