using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapBarcode
    {
        //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, Direction
        public string Id { get; set; } = "Empty";
        public float BarcodeHeadNum { get; set; }
        public float HeadX { get; set; }
        public float HeadY { get; set; }
        public float BarcodeTailNum { get; set; }
        public float TailX { get; set; }
        public float TailY { get; set; }
        public int Direction { get; set; }
    }
}
