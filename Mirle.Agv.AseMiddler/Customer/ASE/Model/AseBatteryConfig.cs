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
        public double CcmodeStopVoltage { get; set; } = 61.0;
        public int BatteryChargingTimeoutM { get; set; } = 10;
        public double CcmodeAh { get; set; } = 0;
        public int WorkingAh { get; set; } = 23;
        public int CcmodeCounter { get; set; } = 0;
        public int AhResetCcmodeCounter { get; set; } = 50;
    }
}
