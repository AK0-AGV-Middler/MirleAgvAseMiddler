using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model.Configs
{
    [Serializable]
    public class MiddlerConfig
    {
        public int ClientNum { get; set; }
        public string ClientName { get; set; }
        public string RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public string LocalIp { get; set; }
        public int LocalPort { get; set; }
        public int RecvTimeoutMs { get; set; }
        public int SendTimeoutMs { get; set; }
        public int MaxReadSize { get; set; }
        public int ReconnectionIntervalMs { get; set; }
        public int MaxReconnectionCount { get; set; }
        public int RetryCount { get; set; }
        public int SleepTime { get; set; }
        public int RichTextBoxMaxLines { get; set; }
        public int ReserveLengthMeter { get; set; }
        public int AskReserveIntervalMs { get; set; }
        public int NeerlyNoMoveRangeMm { get; set; }
        public int BcrReadRetryIntervalMs { get; set; }
        public int BcrReadRetryTimeoutSec { get; set; }
    }
}
