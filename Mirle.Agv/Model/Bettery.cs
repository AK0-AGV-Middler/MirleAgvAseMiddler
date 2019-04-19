using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class Battery
    {
        private int percentage;
        private double voltage;

        public Battery(int per, double vol)
        {
            this.percentage = per;
            this.voltage = vol;
        }
    }
}