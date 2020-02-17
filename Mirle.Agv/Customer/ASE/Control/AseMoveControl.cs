using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.TransferSteps;

namespace Mirle.AgvAseMiddler.Controller
{
    public class AseMoveControl : MoveControlPlate
    {
        public override event EventHandler<EnumMoveComplete> OnMoveFinish;
        public override event EventHandler<EnumMoveComplete> OnRetryMoveFinish;

        public override bool CanAuto(ref string errorMsg)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override bool IsPause()
        {
            throw new NotImplementedException();
        }

        public override bool IsVehicleStop()
        {
            throw new NotImplementedException();
        }

        public override bool Move(TransferStep transferStep, ref string errorMsg)
        {
            throw new NotImplementedException();
        }

        public override bool PartMove(MapPosition mapPosition)
        {
            throw new NotImplementedException();
        }

        public override void RetryMove()
        {
            throw new NotImplementedException();
        }

        public override void StopAndClear()
        {
            throw new NotImplementedException();
        }

        public override void VehcleCancel()
        {
            throw new NotImplementedException();
        }

        public override void VehcleContinue()
        {
            throw new NotImplementedException();
        }

        public override bool VehclePause()
        {
            throw new NotImplementedException();
        }
    }
}
