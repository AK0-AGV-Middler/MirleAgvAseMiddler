using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    public class AseRobotStatus
    {
        public EnumAseRobotState RobotState { get; set; } = EnumAseRobotState.Idle;
        public bool IsHome { get; set; } = true;

        public AseRobotStatus() { }

        public AseRobotStatus(AseRobotStatus aseRobotStatus)
        {
            this.RobotState = aseRobotStatus.RobotState;
            this.IsHome = aseRobotStatus.IsHome;
        }
    }
}
