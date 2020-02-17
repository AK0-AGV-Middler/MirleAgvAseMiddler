using Mirle.AgvAseMiddler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Controller
{
    public class SLAM_Sick : SLAM
    {
        override public bool InitailSLAM(string configPath)
        {

            return true;
        }

        override public void CloseSLAM()
        {

        }

        override public AGVPosition GetAGVPosition()
        {

            return null;
        }
    }
}
