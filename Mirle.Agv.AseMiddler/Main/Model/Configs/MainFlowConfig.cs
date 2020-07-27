using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model.Configs
{
    [Serializable]
    public class MainFlowConfig
    {
        public string LogConfigPath { get; set; }
        public int VisitTransferStepsSleepTimeMs { get; set; }
        public int TrackPositionSleepTimeMs { get; set; } = 5000;
        public int WatchLowPowerSleepTimeMs { get; set; } = 5000;
        public int ReportPositionIntervalMs { get; set; } = 5000;
        public int StartChargeWaitingTimeoutMs { get; set; } = 3000;
        public int StopChargeWaitingTimeoutMs { get; set; } = 3000;
        public int RealPositionRangeMm { get; set; }
        public int LoadingChargeIntervalMs { get; set; }
        public int RobotNgRetryTimes { get; set; }
        public bool IsSimulation { get; set; }
        public string CustomerName { get; set; } = "ASE";
        public bool DualCommandOptimize { get; set; } = false;
        public bool BcrByPass { get; set; } = false;
        public int LowPowerPercentage { get; set; } = 50;
        public int HighPowerPercentage { get; set; } = 90;
        public EnumSlotSelect SlotDisable { get; set; } = EnumSlotSelect.None;
        public int InitialPositionRangeMm { get; set; } = 500;
        public bool UseChargeSystemV2 { get; set; } = false;
        public int IdleReportRangeMm { get; set; } = 100;
        public int IdleReportIntervalMs { get; set; } = 2 * 60 * 1000;
        public int DischargeRetryTimes { get; set; } = 3;
    }
}
