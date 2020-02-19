using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model.Configs
{
    [Serializable]
    public class MainFlowConfig
    {
        public string LogConfigPath { get; set; }
        public int VisitTransferStepsSleepTimeMs { get; set; }
        public int TrackPositionSleepTimeMs { get; set; }
        public int WatchLowPowerSleepTimeMs { get; set; }
        public int ReportPositionIntervalMs { get; set; }
        public int StartChargeWaitingTimeoutMs { get; set; }
        public int StopChargeWaitingTimeoutMs { get; set; }
        public int RealPositionRangeMm { get; set; }
        public int LoadingChargeIntervalMs { get; set; }
        public int RobotNgRetryTimes { get; set; }
        public bool IsSimulation { get; set; }
        public string CustomerName { get; set; } = "ASE";
    }
}
