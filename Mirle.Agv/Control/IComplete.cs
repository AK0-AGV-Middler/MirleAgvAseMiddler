using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Control
{
    public interface IComplete
    {
        void OnTransCmdsFinishedEvent(object sender, EnumCompleteStatus status);
    }
}
