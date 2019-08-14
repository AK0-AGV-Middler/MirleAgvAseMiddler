using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class AGVTurnParameter
    {
        public double Velocity { get; set; }
        public double R { get; set; }
        public double VChangeSafetyDistance { get; set; } //降速完到入彎點的距離.
        public double CloseReviseDistance { get; set; } //關閉即時修正的距離.
        public AxisData AxisParameter { get; set; }
        public double Distance { get; set; }

        public Dictionary<EnumR2000Parameter, AxisData> R2000Parameter { get; set; } = new Dictionary<EnumR2000Parameter, AxisData>();
    }
}
