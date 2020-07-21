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
        public ushort PlcWord { get; set; } //int -> string
        public ushort PlcBit { get; set; }//int -> string
        public EnumAlarmLevel Level { get; set; } = EnumAlarmLevel.Warn;
        public string Description { get; set; } = "Unknow";
        public DateTime SetTime { get; set; }
        public DateTime ResetTime { get; set; }

        //AlarmCode:
        //MainFLow = 0XXXXX
        //Move = 1XXXXX
        //Plc = 2XXXXX
        //AgvcConnector = 3XXXXX       
    }
}
