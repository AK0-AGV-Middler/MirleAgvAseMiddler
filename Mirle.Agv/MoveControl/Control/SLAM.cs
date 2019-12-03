using Mirle.Agv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller
{
    public abstract class SLAM
    {
        protected AGVPosition agvPosition = null;

        virtual public bool InitailSLAM(string configPath)
        {

            return true;
        }

        virtual public void CloseSLAM()
        {

        }

        virtual public AGVPosition GetAGVPosition()
        {

            return null;
        }
    }
}
