using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class SpeedAndMaxTheta
    {
        public double Speed { get; set; }
        public double MaxTheta { get; set; }

        public SpeedAndMaxTheta(double speed, double maxTheta)
        {
            Speed = speed;
            MaxTheta = maxTheta;
        }

        public SpeedAndMaxTheta()
        {
            Speed = 0;
            MaxTheta = 0;
        }
    }
}
