using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class TRTimeToAngleRange
    {
        public List<double> TimeRange { get; set; } = new List<double>();
        public List<double> AngleRange { get; set; } = new List<double>();
        public List<double> TurnVelocity { get; set; } = new List<double>();
    }
}
