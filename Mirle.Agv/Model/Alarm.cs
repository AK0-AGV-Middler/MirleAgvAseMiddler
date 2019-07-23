using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class Alarm
    {
        public int Id { get; set; }
        public string AlarmText { get; set; } = "Unknow";
        public string PlcAddress { get; set; } //int -> string
        public string PlcBitNumber { get; set; }//int -> string
        public int Level { get; set; }
        public string Description { get; set; } = "Unknow";
        public DateTime SetTime { get; set; }
        public DateTime ResetTime { get; set; }

        //AlarmCode:
        //MainFLow = 0XXXXX
        //Move = 1XXXXX
        //Plc = 2XXXXX
        //Middler = 3XXXXX
    }
}
