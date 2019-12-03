using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model.Configs
{
    public class SLAMConfig
    {
        public EnumSLAMType UsingSLAMType { get; set; }
        public string SLAM_Nav350ConfigPath { get; set; }
        public string SLAM_SickConfigPath { get; set; }
        public string SLAM_R2SConfigPath { get; set; }

        public SLAMConfig()
        {
            UsingSLAMType = EnumSLAMType.None;
            SLAM_Nav350ConfigPath = "";
            SLAM_SickConfigPath = "";
            SLAM_R2SConfigPath = "";
        }
    }
}
