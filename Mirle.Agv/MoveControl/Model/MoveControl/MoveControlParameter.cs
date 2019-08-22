using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MoveControlParameter
    {
        public bool DirFlag { get; set; }
        public bool PositionDirFlag { get; set; }
        public int WheelAngle { get; set; }
        public bool MoveControlStop { get; set; }
        public double VelocityCommand { get; set; }
        public double RealVelocity { get; set; }
        public Thread MoveControlThread { get; set; }
        public bool OntimeReviseFlag { get; set; }
        public double TrigetEndEncoder { get; set; }

        public MoveControlParameter()
        {
            DirFlag = true;
            PositionDirFlag = true;
            WheelAngle = 0;
            MoveControlStop = false;
            VelocityCommand = 100;
            OntimeReviseFlag = false;
        }
    }
}
