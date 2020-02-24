using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    [Serializable]
    public class MapAddressOffset
    {
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double OffsetTheta { get; set; }
    }
}
