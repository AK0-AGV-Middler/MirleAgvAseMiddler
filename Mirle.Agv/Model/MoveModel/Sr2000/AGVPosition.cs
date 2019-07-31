using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class AGVPosition
    {
        public MapPosition Position { get; set; }
        public double AGVAngle { get; set; }
        public double BarcodeAngle { get; set; }
        public int ScanTime { get; set; }
        public DateTime GetDataTime { get; set; }
        public uint Count { get; set; }

        public AGVPosition(MapPosition agvPosition, double agvAngle, double barcodeAngle, int scanTime, DateTime getDataTime, uint count)
        {
            Position = agvPosition;
            AGVAngle = agvAngle;
            BarcodeAngle = barcodeAngle;
            ScanTime = scanTime;
            GetDataTime = getDataTime;
            Count = count;
        }
    }
}
