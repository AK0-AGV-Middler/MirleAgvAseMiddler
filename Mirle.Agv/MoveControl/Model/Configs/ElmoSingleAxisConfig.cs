using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using ElmoMotionControlComponents.Drive.EASComponents;

namespace Mirle.AgvAseMiddler.Model.Configs
{
    public class ElmoSingleAxisConfig
    {
        public EnumAxis ID { get; set; }
        public string AxisName { get; set; }
        public bool IsGroup { get; set; }
        public double MotorResolution { get; set; }
        public double PulseUnit { get; set; }
        public double Velocity { get; set; }
        public double Acceleration { get; set; }
        public double Deceleration { get; set; }
        public double Jerk { get; set; }
        public bool IsVirtualDevice { get; set; } = false;
        public EnumAxis VirtualDev4ID { get; set; } = EnumAxis.None;
        public List<EnumAxis> GroupOrder { get; set; } = null;
        public List<EnumAxis> CommandOrder { get; set; } = null;
        public EnumAxisType Type { get; set; }

        // ??基本需求??
        public double dbDistance { get; set; } = 5000;
        public double dbEndVelocity { get; set; } = 20000;
        public MC_BUFFERED_MODE_ENUM eBufferMode { get; set; } = MC_BUFFERED_MODE_ENUM.MC_ABORTING_MODE;
        public MC_DIRECTION_ENUM eDirection { get; set; } = MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION;
        public byte ucExecute { get; set; } = 1;
        //Group
        public MC_COORD_SYSTEM_ENUM CoordSystem = MC_COORD_SYSTEM_ENUM.MC_ACS_COORD;
        public NC_TRANSITION_MODE_ENUM TransitionMode = NC_TRANSITION_MODE_ENUM.MC_TM_NONE_MODE;
    }
}
