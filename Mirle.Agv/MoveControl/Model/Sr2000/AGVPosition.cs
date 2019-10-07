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
        public MapPosition BarcodeCenter { get; set; }
        public double AGVAngle { get; set; }
        public double BarcodeAngle { get; set; }
        public int ScanTime { get; set; }
        public DateTime GetDataTime { get; set; }
        public uint Count { get; set; }
        public double BarcodeAngleInMap { get; set; }
        public EnumBarcodeMaterial Type { get; set; }

        public AGVPosition(MapPosition agvPosition, MapPosition barcodeCenter, double agvAngle, double barcodeAngle, int scanTime, DateTime getDataTime, uint count, double barcodeAngleInMap, EnumBarcodeMaterial type)
        {
            Position = agvPosition;
            AGVAngle = agvAngle;
            BarcodeCenter = barcodeCenter;
            BarcodeAngle = barcodeAngle;
            ScanTime = scanTime;
            GetDataTime = getDataTime;
            Count = count;
            BarcodeAngleInMap = barcodeAngleInMap;
            Type = type;
        }

        public AGVPosition()
        {
            Position = new MapPosition();
            BarcodeCenter = new MapPosition();
        }
    }
}
