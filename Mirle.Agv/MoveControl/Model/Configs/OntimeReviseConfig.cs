using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model.Configs
{
    public class OntimeReviseConfig
    {
        public double MaxVelocity { get; set; }
        public double MinVelocity { get; set; }
        public double ThetaSpeed { get; set; }
        public ThetaSectionDeviation LinePriority { get; set; }
        public ThetaSectionDeviation HorizontalPriority { get; set; }
        public ThetaSectionDeviation ModifyPriority { get; set; }
        public ThetaSectionDeviation Return0ThetaPriority { get; set; }
        public List<SpeedAndMaxTheta> SpeedToMaxTheta { get; set; }
        public SafetyData OneTimeRevise { get; set; }

        public OntimeReviseConfig()
        {
            SpeedToMaxTheta = new List<SpeedAndMaxTheta>();
        }
    }
}
