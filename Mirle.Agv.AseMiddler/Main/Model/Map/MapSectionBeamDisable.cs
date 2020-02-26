using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    [Serializable]
    public class MapSectionBeamDisable
    {
        public string SectionId { get; set; } = "";
        public double Min { get; set; }
        public double Max { get; set; }
        public bool FrontDisable { get; set; }
        public bool BackDisable { get; set; }
        public bool LeftDisable { get; set; }
        public bool RightDisable { get; set; }
    }
}
