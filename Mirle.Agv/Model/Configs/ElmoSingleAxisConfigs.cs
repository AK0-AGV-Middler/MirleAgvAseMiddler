using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Configs
{
    public class ElmoSingleAxisConfigs
    {
        public string AxisAlias { get; set; }
        public string AxisName { get; set; }
        public bool IsGroup { get; set; }
        public int AxisID { get; set; }
        public double MotorResolution { get; set; }
        public double PulseUnit;
        public double Velocity;
        public double Acceleration;
        public double Deceleration;
        public double Jerk;
    }
}
