using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Mirle.Agv.Controller;
using Mirle.Agv;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class PlcBatterys
    {
        public EnumBatteryType BatteryType { get; set; } = EnumBatteryType.Yinda;
        public double MeterCurrent { get; set; }
        public double MeterVoltage { get; set; }
        public double MeterWatt { get; set; }
        public double MeterWattHour { get; set; }
        public double GotechMaxVol { get; set; }
        public double GotechMinVol { get; set; }
        public double YindaMaxVol { get; set; }
        public double YindaMinVol { get; set; }

        public ushort Cell_number { get; set; }     
        public List<BatteryCell> BatteryCells = new List<BatteryCell>();     

        public ushort Temperature_sensor_number { get; set; }  
        public double Temperature_1_MOSFET { get; set; }       
        public double Temperature_2_Cell { get; set; }         
        public double Temperature_3_MCU { get; set; }          
        public double BatteryCurrent { get; set; }             
        public double Packet_Voltage { get; set; }             
        public ushort Remain_Capacity { get; set; }            
        public ushort Design_Capacity { get; set; }

        public ushort BatterySOCFormPlc { get; set; }//PLC->IPC
        public ushort BatterySOHFormPlc { get; set; }//PLC->IPC

        public double Battery_Cell_Low_Voltage { get; set; } = 2.8;

        public double FBatteryTemperature { get; set; }
        public double BBatteryTemperature { get; set; }

        public Stopwatch SwBatteryAhSetToZero { get; set; } = new Stopwatch();
        public uint ResetAhTimeout { get; set; } = 10;
        public ushort MaxResetAhCcounter { get; set; } = 50;
        public ushort FullChargeIndex { get; set; } = 50;
        //Port_AutoCharge_Low_SOC
        public double PortAutoChargeLowSoc { get; set; } = 50.00;
        public double PortAutoChargeHighSoc { get; set; } = 90.00;

        public uint Battery_Logger_Interval { get; set; } = 3;
        public uint Batterys_Charging_Time_Out { get; set; } = 10;

        public uint Charging_Off_Delay { get; set; } = 2;
        public bool Charging { get; set; }
        public bool CcModeFlag { get; set; } = false;
        public ushort CcModeCounter { get; set; } = 0;

        private double dblMeterAH = 0;
        public double MeterAh
        {
            get { return dblMeterAH; }
            set
            {
                dblMeterAH = value;
                CountPercentage();
            }
        }
        public double CcModeAh { get; private set; } = 0;//充電達到cc mode時當下的Meter AH值

        public void SetCcModeAh(double setAh, bool realCcMode)
        {
            CcModeAh = setAh;
            if (realCcMode)
            {
                if (!SetMeterAhToZeroFlag)
                {
                    Percentage = 100.00;
                    CcModeCounter++;
                }
                else
                {
                    this.CountPercentage();
                }

            }
            else
            {
                this.CountPercentage();
            }


        }

        public double Percentage { get; private set; } = 0;  //剩餘電量s分比

        private double dblAhWorkingRange = 23.0;
        public double AhWorkingRange//我們使用的電池AH Range (User config)
        {
            get { return dblAhWorkingRange; }
            set
            {
                if (value != 0)
                {
                    dblAhWorkingRange = value;
                    //CcModeAh = dblAHWorkingRange;
                    //SetCcModeAh(dblAhWorkingRange, false);
                    CountPercentage();
                }
            }
        }

        private bool boolSetMeterAhToZeroFlag = false;
        public bool SetMeterAhToZeroFlag//是否正在執行AH Reset flag
        {
            get { return boolSetMeterAhToZeroFlag; }
            set
            {
                if (boolSetMeterAhToZeroFlag != value)
                {
                    boolSetMeterAhToZeroFlag = value;
                    if (value)
                    {
                        SetMeterAhToZeroAh = dblMeterAH;
                        SwBatteryAhSetToZero.Reset();
                        SwBatteryAhSetToZero.Start();
                    }
                    else
                    {
                        SetMeterAhToZeroAh = 0;
                        SwBatteryAhSetToZero.Stop();
                    }
                }
            }
        }
        public double SetMeterAhToZeroAh { get; set; } = 0.0;//執行AH Reset flag前的AH

        //改由毛哥卡CC mode停止,抓FullChargeIndex變化,代表達到CC mode
        public double CCModeStopVoltage { get; set; } = 61.5; //CC Mode充電停止電壓 (User config)
        private Boolean bVoltageAbnormal = false;
        private void CountPercentage()
        {
            if (!boolSetMeterAhToZeroFlag)
            {
                if (Percentage != 100.00)
                {
                    Percentage = ((dblAhWorkingRange - (CcModeAh - dblMeterAH)) / dblAhWorkingRange) * 100;
                    if (Percentage > 99.99)
                    {
                        Percentage = 99.99;
                    }

                    if (Percentage < 0.00)
                    {
                        Percentage = 0.00;
                    }
                }
                else
                {
                    double temp = ((dblAhWorkingRange - (CcModeAh - dblMeterAH)) / dblAhWorkingRange) * 100;
                    if (temp > 99.99)
                    {
                        //keep 100%
                    }
                    else if (Percentage < 0.00)
                    {
                        Percentage = 0.00;
                    }
                    else
                    {
                        Percentage = temp;
                    }
                }


                try
                {
                    if (GotechMinVol != 0 && YindaMinVol != 0)
                    {
                        switch (BatteryType)
                        {
                            case EnumBatteryType.Gotech:
                                TriggerBatteryLowVoltage(GotechMinVol + 1.5, 10);
                                break;
                            case EnumBatteryType.Yinda:
                                TriggerBatteryLowVoltage(YindaMinVol + 1.5, 10);
                                break;

                            default:
                                break;

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }


            }

        }

        private void TriggerBatteryLowVoltage(double MinVol, double SocChange)
        {
            if (MeterVoltage < MinVol)
            {
                if (bVoltageAbnormal == false)
                {
                    bVoltageAbnormal = true;
                    CcModeAh = (MeterAh + AhWorkingRange * (100.0 - SocChange) / 100.00);
                }
                else
                {
                    if (Percentage > SocChange)
                    {
                        CcModeAh = (MeterAh + AhWorkingRange * (100.0 - SocChange) / 100.00);
                    }
                }
            }
            else
            {
                if (bVoltageAbnormal == true)
                {
                    bVoltageAbnormal = false;
                }
            }
        }

    }
}
