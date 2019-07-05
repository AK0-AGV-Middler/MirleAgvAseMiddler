using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapBarcodeLine
    {
        //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, Direction
        public string Id { get; set; } = "Empty";
        public int BarcodeHeadNum { get; set; }
        public float HeadX { get; set; }
        public float HeadY { get; set; }
        public int BarcodeTailNum { get; set; }
        public float TailX { get; set; }
        public float TailY { get; set; }
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
