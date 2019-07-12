using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class Alarm
    {
        public int Id { get; set; }
        public string ShortName { get; set; } = "Unknow";
        public int WordNum { get; set; }
        public int BitNum { get; set; }
        public int Level { get; set; }
        public string Description { get; set; } = "Unknow";
        public DateTime SetTime { get; set; }
        public DateTime ResetTime { get; set; }
    }
}
