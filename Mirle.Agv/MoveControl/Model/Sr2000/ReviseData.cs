using Mirle.AgvAseMiddler.Model.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class ReviseParameter
    {
        public double Velocity { get; set; }
        public double ModifyTheta { get; set; }
        public double ModifySectionDeviation { get; set; }
        public EnumLineReviseType ReviseType { get; set; }
        public double ReviseValue { get; set; }
        public double MaxTheta { get; set; }
        public double ThetaCommandSpeed { get; set; }
        public bool OntimeReviseFlag { get; set; }
        public bool DirFlag { get; set; }

        public ReviseParameter(OntimeReviseConfig config, double velocity, bool dirFlag, bool flag = false)
        {
            if (config == null)
                return;

            if (velocity < 0)
                velocity = -velocity;

            Velocity = velocity;

            MaxTheta = 1;
            for (int i = 0; i < config.SpeedToMaxTheta.Count; i++)
            {
                if (velocity < config.SpeedToMaxTheta[i].Speed)
                {
                    MaxTheta = config.SpeedToMaxTheta[i].MaxTheta;
                    break;
                }
            }

            if (velocity > config.MaxVelocity)
                velocity = config.MaxVelocity;
            else if (velocity < config.MinVelocity)
                velocity = config.MinVelocity;

            ModifyTheta = velocity / config.ModifyPriority.Theta;
            ModifySectionDeviation = velocity / config.ModifyPriority.SectionDeviation;
            ReviseType = EnumLineReviseType.None;
            ReviseValue = 0;
            ThetaCommandSpeed = 10;
            DirFlag = dirFlag;
            OntimeReviseFlag = flag;
        }

    }
}
