using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model.Configs
{
    public class AseBatteryConfig
    {
        public int WatchBatteryStateInterval { get; set; } = 1000;
        public int WatchBatteryStateIntervalInCharging { get; set; } = 500;
    }
}
