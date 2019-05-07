using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class Battery
    {
        public int Percentage { get; set; }
        public double Voltage { get; set; }
        public int Temperature { get; set; }

        public Battery(int per, double vol)
        {
            Percentage = per;
            Voltage = vol;
            Temperature = 40;
        }
    }
}