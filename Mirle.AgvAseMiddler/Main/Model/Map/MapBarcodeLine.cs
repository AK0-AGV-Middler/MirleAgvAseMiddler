using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    [Serializable]
    public class MapBarcodeLine
    {
        //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, Offset
        public string Id { get; set; } = "";
        public MapBarcode HeadBarcode { get; set; } = new MapBarcode();
        public MapBarcode TailBarcode { get; set; } = new MapBarcode();
        public MapPosition Offset { get; set; } = new MapPosition();
        public EnumBarcodeMaterial Material { get; set; } = EnumBarcodeMaterial.Iron;

        public EnumBarcodeMaterial BarcodeMaterialParse(string v)
        {
            v = v.Trim();
            return (EnumBarcodeMaterial)Enum.Parse(typeof(EnumBarcodeMaterial), v);
        }
    }
}
