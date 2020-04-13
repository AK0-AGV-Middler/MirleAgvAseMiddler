using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    public class AseBatteryStatus
    {
        public double Ah { get; set; } = 0;
        public double Voltage { get; set; } = 0;
        public int Percentage { get; set; } = 0;
        public int Temperature { get; set; } = 0;

        public AseBatteryStatus() { }        

        public AseBatteryStatus(AseBatteryStatus aseBatteryStatus)
        {
            this.Ah = aseBatteryStatus.Ah;
            this.Voltage = aseBatteryStatus.Voltage;
            this.Percentage = aseBatteryStatus.Percentage;
            this.Temperature = aseBatteryStatus.Temperature;
        }
    }
}
