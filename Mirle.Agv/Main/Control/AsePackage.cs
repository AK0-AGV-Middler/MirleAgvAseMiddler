using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSDriver.PSDriver;

namespace Mirle.AgvAseMiddler.Controller
{
    public class AsePackage
    {
        public PSWrapperXClass psWrapper;
        public AseMoveControl aseMoveControl;
        public AseIntegrateControl aseIntegrateControl;


        public AsePackage()
        {
            InitialWrapper();
            aseMoveControl = new AseMoveControl(psWrapper);

        }

        private void InitialWrapper()
        {
            throw new NotImplementedException();
        }
    }
}
