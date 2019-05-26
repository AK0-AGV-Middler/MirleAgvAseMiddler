using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapRowBarcode
    {
        //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, Type

        public string Id { get; set; }
        public float BarcodeHeadNum { get; set; }
        public float HeadX { get; set; }
        public float HeadY { get; set; }
        public float BarcodeTailNum { get; set; }
        public float TailX { get; set; }
        public float TailY { get; set; }
        public EnumRowBarcodeType Type { get; set; }

    }
}
