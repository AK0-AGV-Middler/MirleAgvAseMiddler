using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.Configs;

namespace Mirle.Agv.Model
{
    public class Battery
    {
        public int Percentage { get; set; }
        public double Voltage { get; set; }
        public int Temperature { get; set; }
        public int LowPowerThreshold { get; set; }
        public int HighTemperatureThreshold { get; set; }

        private BatteryConfigs batteryConfigs;

        public Battery()
        {
        }

        public bool IsBatteryLowPower()
        {
            return Percentage < LowPowerThreshold;
        }

        public bool IsBatteryHighTemperature()
        {
            return Temperature > HighTemperatureThreshold;
        }

        public void SetupBattery(BatteryConfigs batteryConfigs)
        {
            this.batteryConfigs = batteryConfigs;

            Percentage = batteryConfigs.Percentage;
            Voltage = batteryConfigs.Voltage;
            Temperature = batteryConfigs.Temperature;
            LowPowerThreshold = batteryConfigs.LowPowerThreshold;
            HighTemperatureThreshold = batteryConfigs.HighTemperatureThreshold;
        }
    }
}