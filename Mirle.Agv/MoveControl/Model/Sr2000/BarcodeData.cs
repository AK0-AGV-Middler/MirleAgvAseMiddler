using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class BarcodeData
    {
        public int ID { get; set; }
        public MapPosition ViewPosition { get; set; }
        public MapPosition MapPosition { get; set; }
        public MapPosition MapPositionOffset { get; set; }
        public int LineBarcodeAngle { get; set; }
        public string LineId { get; set; }

        public BarcodeData(int id, double x, double y)
        {
            ID = id;
            ViewPosition = new MapPosition((float)x, (float)y);
        }
    }
}
