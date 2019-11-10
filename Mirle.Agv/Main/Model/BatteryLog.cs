using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class BatteryLog
    {
        public double MoveDistanceTotalMm { get; set; }
        public int LoadUnloadCount { get; set; }
        public int ChargeCount { get; set; }
        public double UsingTime { get; set; }
    }
}
