using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class VehicleCorrectValue
    {
        public string VehicleDeltaX { get; set; } = "";
        public string VehicleDeltaY { get; set; } = "";
        public string VehicleTheta { get; set; } = "";
        public string VehicleHead { get; set; } = "";
        public string VehicleTwiceReviseDistance { get; set; } = "";
        public string otherMessage { get; set; } = "";
    }
}
