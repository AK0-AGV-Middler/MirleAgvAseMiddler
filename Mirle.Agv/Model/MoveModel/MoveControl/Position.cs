using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class Position
    {
        public MapPosition Encoder { get; set; }
        public MapPosition Barcode { get; set; }
        public double Delta { get; set; }
        public double Offset { get; set; }
        public MapPosition Real { get; set; }
        public double RealEncoder { get; set; }
        public double ElmoEncoder { get; set; }
        public int IndexOfSr2000List { get; set; }
        public uint LastBarcodeCount { get; set; }
        public int ScanTime { get; set; }
        public DateTime BarcodeGetDataTime { get; set; }
        public DateTime ElmoGetDataTime { get; set; }
        public double XFLVelocity { get; set; }
        public double XRRVelocity { get; set; }

        public Position()
        {
            Encoder = new MapPosition();
            Barcode = new MapPosition();
            Delta = 0;
            RealEncoder = 0;
            Real = null;
            ElmoEncoder = 0;
            Offset = 0;
            IndexOfSr2000List = -1;
            LastBarcodeCount = 100;
        }
    }
}
