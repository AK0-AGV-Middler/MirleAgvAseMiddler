using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class MapBarcodeLine
    {
        //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, Offset
        public string Id { get; set; } = "Empty";
        public MapBarcode HeadBarcode { get; set; } = new MapBarcode();
        public MapBarcode TailBarcode { get; set; } = new MapBarcode();
        public MapPosition Offset { get; set; } = new MapPosition();
    }
}
