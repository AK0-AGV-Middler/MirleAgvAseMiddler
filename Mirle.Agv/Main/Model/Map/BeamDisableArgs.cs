using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class BeamDisableArgs : EventArgs
    {
        public EnumBeamDirection Direction { get; set; } = EnumBeamDirection.Front;
        public bool IsDisable { get; set; } = false;

        public BeamDisableArgs(EnumBeamDirection beamDirection,bool isDisable)
        {
            Direction = beamDirection;
            IsDisable = isDisable;
        }
    }
}
