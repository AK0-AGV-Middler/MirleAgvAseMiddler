using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapBarcode
    {
        public int BarcodeNum { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public int Direction { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }

        public int BarcodeDirectionConvert(string v)
        {
            v = v.Trim();

            switch (v)
            {
                case "0":
                    return 0;
                case "90":
                    return 90;
                case "180":
                    return 180;
                case "-90":
                    return -90;
                default:
                    return 0;
            }
        }
    }
}
