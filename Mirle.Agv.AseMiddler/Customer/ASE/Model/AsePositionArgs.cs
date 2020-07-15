using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    public class AsePositionArgs : EventArgs
    {
        public EnumAseArrival Arrival { get; set; } = EnumAseArrival.Fail;
        public MapPosition MapPosition { get; set; } = new MapPosition();
    }
}
