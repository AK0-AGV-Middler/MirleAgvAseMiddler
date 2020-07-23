﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model.Configs
{
    public class AsePackageConfig
    {
        public string PspConnectionConfigFilePath { get; set; } = "PspConnectionConfig.xml";
        public string AutoReplyFilePath { get; set; } = "AutoReply.csv";
        public string AseBatteryConfigFilePath { get; set; } = "AseBatteryConfig.xml";
        public string AseMoveConfigFilePath { get; set; } = "AseMoveConfig.xml";
        public int WatchWifiSignalIntervalMs { get; set; } = 20000;
        public bool CanManualDeleteCST { get; set; } = false;
        public int ScheduleIntervalMs { get; set; } = 100;
        public string RemoteControlPauseErrorCode { get; set; } = "123";
        public string RemoteControlResumeErrorCode { get; set; } = "456";
    }
}
