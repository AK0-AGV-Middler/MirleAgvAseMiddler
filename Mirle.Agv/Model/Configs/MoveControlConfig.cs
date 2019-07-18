using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Configs
{
    public class MoveControlConfig
    {
        public string Sr2000FileName { get; set; } = "SR2KConfig.xml";
        public string OnTimeReviseFileName { get; set; } = "OntimeReviseConfig.xml";
        public int SleepTime { get; set; } = 10;
    }
}
