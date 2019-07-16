using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class MapBarcode
    {
        public int Number { get; set; }
        public MapPosition Position { get; set; } = new MapPosition();
        public MapPosition Offset { get; set; } = new MapPosition();
        public int Direction { get; set; }
        public string LineId { get; set; }

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
