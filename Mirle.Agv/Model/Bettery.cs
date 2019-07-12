using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.Configs;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class Battery
    {
        public int Percentage { get; set; }
        public double Voltage { get; set; }
        public int Temperature { get; set; }
        public int LowPowerThreshold { get; set; }
        public int HighTemperatureThreshold { get; set; }

        private BatteryConfig batteryConfig;

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

        public void SetupBattery(BatteryConfig aBatteryConfig)
        {
            this.batteryConfig = aBatteryConfig;

            Percentage = aBatteryConfig.Percentage;
            Voltage = aBatteryConfig.Voltage;
            Temperature = aBatteryConfig.Temperature;
            LowPowerThreshold = aBatteryConfig.LowPowerThreshold;
            HighTemperatureThreshold = aBatteryConfig.HighTemperatureThreshold;
        }
    }
}