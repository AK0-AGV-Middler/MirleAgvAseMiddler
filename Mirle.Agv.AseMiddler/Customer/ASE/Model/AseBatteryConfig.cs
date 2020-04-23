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
        public int LastPercentage { get; set; } = 70;
        public int BatteryChargingTimeoutM { get; set; } = 10;
    }
}
