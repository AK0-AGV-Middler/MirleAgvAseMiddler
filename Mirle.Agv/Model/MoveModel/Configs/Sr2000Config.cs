using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Configs
{
    public class Sr2000Config
    {
        public string ID { get; set; }
        public string IP { get; set; }
        public double ReaderToCenterDegree { get; set; }
        public double ReaderToCenterDistance { get; set; }
        public double ReaderSetupAngle { get; set; }
        public MapPosition ViewCenter { get; set; }
        public MapPosition ViewOffset { get; set; }
        public MapPosition Target { get; set; }
        public MapPosition Change { get; set; }
        public double OffsetTheta { get; set; }
        public int TimeOutValue { get; set; }
        public int SleepTime { get; set; }
        public bool LogMode { get; set; }
        public double Up { get; set; }
        public double Down { get; set; }
        public double DistanceSafetyRange { get; set; }
    }
}
