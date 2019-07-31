using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class AxisData
    {
        public double Acceleration { get; set; }
        public double Deceleration { get; set; }
        public double Jerk { get; set; }
        public double Velocity { get; set; }
        public double MotorResolution { get; set; }
        public double PulseUnit { get; set; }

        public AxisData()
        {
            Acceleration = 0;
            Deceleration = 0;
            Jerk = 0;
        }

        public AxisData(double acc, double dec, double jerk, double velocity = 100)
        {
            Acceleration = acc;
            Deceleration = dec;
            Jerk = jerk;
            Velocity = velocity;
        }
    }
}
