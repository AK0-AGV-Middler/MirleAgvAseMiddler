using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class EncoderPositionData
    {
        public Dictionary<EnumAxis, double> NowTurnEncoder { get; set; }
        public Dictionary<EnumAxis, double> LastMoveEncoder { get; set; }
        public Dictionary<EnumAxis, double> NowMoveEncoder { get; set; }
    }
}
