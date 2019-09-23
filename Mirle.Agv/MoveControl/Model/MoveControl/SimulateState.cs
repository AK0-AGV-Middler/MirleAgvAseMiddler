using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class SimulateState
    {
        public EnumVehicleSafetyAction BeamSensorState { get; set; } = EnumVehicleSafetyAction.Normal;
        public EnumVehicleSafetyAction BumpSensorState { get; set; } = EnumVehicleSafetyAction.Normal;
        public bool AxisNormal { get; set; } = true;
        public bool IsCharging { get; set; } = false;
        public bool ForkNotHome { get; set; } = false;
    }
}
