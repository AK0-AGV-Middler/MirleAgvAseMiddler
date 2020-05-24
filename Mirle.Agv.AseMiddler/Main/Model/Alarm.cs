using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    [Serializable]
    public class Alarm
    {
        public int Id { get; set; }
        public string AlarmText { get; set; } = "Unknow";
        public EnumAlarmLevel Level { get; set; }
        public string Description { get; set; } = "Unknow";
        public DateTime SetTime { get; set; }
        public DateTime ResetTime { get; set; }
    }
}
