using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller
{
    public interface ICmdFinished
    {
        void MoveControlHandler_OnMoveFinished(object sender, EnumCompleteStatus status);
    }
}
