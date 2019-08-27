using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Configs
{
    [Serializable]
    public class MapConfig
    {
        public string SectionFileName { get; set; }
        public string AddressFileName { get; set; }
        public string BarcodeFileName { get; set; }
        public string SectionBeamDisablePathFileName { get; set; }      
    }
}
