using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Configs
{
    [Serializable]
    public class BatteryConfig
    {
        public int Percentage { get; set; }
        public double Voltage { get; set; }
        public int Temperature { get; set; }
        public int LowPowerThreshold { get; set; }
        public int HighTemperatureThreshold { get; set; }
    }
}
