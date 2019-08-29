using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MoveControlSafetyData
    {
        public EnumMoveState LastMoveState { get; set; }
        public EnumMoveState NowMoveState { get; set; }
        public bool IsTurnOut { get; set; }
        public double TurnOutElmoEncoder { get; set; }

        public double LastReadBarcodeElmoEncoder { get; set; }
        public bool LastReadBarcodeReset { get; set; }
        public bool TurningByPass { get; set; }

        public MoveControlSafetyData()
        {
            IsTurnOut = false;
            TurnOutElmoEncoder = 0;
            LastReadBarcodeElmoEncoder = 0;
            NowMoveState = EnumMoveState.Idle;
            LastMoveState = EnumMoveState.Idle;
            LastReadBarcodeReset = true;
        }
    }
}
