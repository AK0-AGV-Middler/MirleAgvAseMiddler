using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class Location
    {
        public AGVPosition Encoder { get; set; }
        public AGVPosition Barcode { get; set; }
        public AGVPosition BarcodeLeft { get; set; }
        public AGVPosition BarcodeRight { get; set; }
        public double Delta { get; set; }
        public double Offset { get; set; }
        public AGVPosition Real { get; set; }
        public double RealEncoder { get; set; }
        public double ElmoEncoder { get; set; }
        public int ScanTime { get; set; }
        public DateTime BarcodeGetDataTime { get; set; }
        public DateTime ElmoGetDataTime { get; set; }
        public double Velocity { get; set; }
        public bool GXMoveCompelete { get; set; }
        public bool GTMoveCompelete { get; set; }
        public double GTPosition { get; set; }
        public string LastPositingBarcodeID { get; set; }
        public ThetaSectionDeviation ThetaAndSectionDeviation { get; set; }

        public Location()
        {
            LastPositingBarcodeID = "";
            Encoder = null;
            Barcode = null;
            BarcodeLeft = null;
            BarcodeRight = null;
            Delta = 0;
            RealEncoder = 0;
            Real = null;
            ElmoEncoder = 0;
            Offset = 0;
            ThetaAndSectionDeviation = null;
        }
    }
}
