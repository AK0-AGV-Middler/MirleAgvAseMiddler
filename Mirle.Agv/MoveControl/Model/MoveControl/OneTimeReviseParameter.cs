using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class OneTimeReviseParameter
    {
        public double ReviseStartEncoder { get; set; }

        public double[] WheelTheta_ReviseTheta { get; set; }
        public double[] WheelTheta_ReviseSectionDeviation { get; set; }
        public EnumOneTimeReviseState State { get; set; }

        public OneTimeReviseParameter()
        {
            State = EnumOneTimeReviseState.NoRevise;
        }
    }
}
