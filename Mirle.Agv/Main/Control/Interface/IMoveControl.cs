using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Controller
{
    public interface IMoveControl
    {
        void VehcleCancel();
        void VehcleContinue();
        bool VehclePause();
        void StopAndClear();
        bool Move(TransferStep transferStep, ref string errorMsg);
        void RetryMove();
        bool PartMove(MapPosition mapPosition);

        event EventHandler<EnumMoveComplete> OnMoveFinish;
        event EventHandler<EnumMoveComplete> OnRetryMoveFinish;
    }

    public abstract class MoveControlPlate : IMoveControl
    {
        public abstract event EventHandler<EnumMoveComplete> OnMoveFinish;
        public abstract event EventHandler<EnumMoveComplete> OnRetryMoveFinish;

        public abstract bool Move(TransferStep transferStep, ref string errorMsg);
        public abstract bool PartMove(MapPosition mapPosition);
        public abstract void RetryMove();
        public abstract void StopAndClear();
        public abstract void VehcleCancel();
        public abstract void VehcleContinue();
        public abstract bool VehclePause();

        public EnumMoveState MoveState { get; protected set; }
        public string StopResult { get; set; } = "";
    }

    public class MoveControlFactory
    {
        public MoveControlPlate GetMoveControl(string type, MapInfo mapInfo, AlarmHandler alarmHandler, IntegrateControlPlate integrateControlPlate)
        {
            MoveControlPlate moveControlPlate = null;

            if (type == "AUO")
            {
                AuoIntegrateControl auoIntegrateControl = (AuoIntegrateControl)integrateControlPlate;
                moveControlPlate = new AuoMoveControl(mapInfo, alarmHandler, auoIntegrateControl.GetPlcAgent());
            }            

            return moveControlPlate;
        }
    }
}
