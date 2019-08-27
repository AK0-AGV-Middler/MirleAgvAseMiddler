using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class MapSectionBeamDisable
    {
        public string SectionId { get; set; } = "Empty";
        public double Min { get; set; }
        public double Max { get; set; }
        public bool ForwardDisable { get; set; }
        public bool BackwardDisable { get; set; }
        public bool LeftDisable { get; set; }
        public bool RightDisable { get; set; }
    }
}
