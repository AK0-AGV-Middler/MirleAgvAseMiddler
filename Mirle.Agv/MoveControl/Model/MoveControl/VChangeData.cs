using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class VChangeData
    {
        public double StartEncoder { get; set; }
        public double StartVelocity { get; set; }
        public double EndEncoder { get; set; }
        public double VelocityCommand { get; set; }
        public EnumVChangeType Type { get; set; }

        public VChangeData( double startEncoder, double startVelocity, double endEncoder, double velocityCommand, EnumVChangeType type)
        {
            StartEncoder = startEncoder;
            StartVelocity = startVelocity;
            EndEncoder = endEncoder;
            VelocityCommand = velocityCommand;
            Type = type;
        }
    }
}
