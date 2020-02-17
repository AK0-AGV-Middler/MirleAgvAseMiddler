using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    [Serializable]
    public class PlcOperation
    {
        public bool ModeOperation { get; set; }
        public EnumJogVehicleMode ModeVehicle { get; set; }

        public EnumJogElmoFunction JogElmoFunction = EnumJogElmoFunction.No_Use;
        public EnumJogRunMode JogRunMode = EnumJogRunMode.No_Use;
        public EnumJogOperation JogOperation = EnumJogOperation.No_Use;
        public EnumJogTurnSpeed JogTurnSpeed = EnumJogTurnSpeed.No_Use;
        public EnumJogMoveVelocity JogMoveVelocity = EnumJogMoveVelocity.No_Use;

        public bool JogMoveOntimeRevise { get; set; }
        public double JogMaxDistance { get; set; } = 1000f;
        
    }
}
