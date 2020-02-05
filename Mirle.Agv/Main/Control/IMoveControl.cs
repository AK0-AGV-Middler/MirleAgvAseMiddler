using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller
{
    public interface IMoveControl
    {
        void VehcleCancel();
        bool IsPause();
        bool IsVehicleStop();
        void VehcleContinue();
        bool VehclePause();
        void StopAndClear();
        bool CanAuto(ref string errorMsg);
        bool Move(TransferStep transferStep, ref string errorMsg);
        void RetryMove();
        bool PartMove(MapPosition mapPosition);
        void Close();

        event EventHandler<EnumMoveComplete> OnMoveFinish;
        event EventHandler<EnumMoveComplete> OnRetryMoveFinish;
    }

    public abstract class MoveControlPlate : IMoveControl
    {
        public abstract event EventHandler<EnumMoveComplete> OnMoveFinish;
        public abstract event EventHandler<EnumMoveComplete> OnRetryMoveFinish;

        public abstract bool CanAuto(ref string errorMsg);
        public abstract bool IsPause();
        public abstract bool IsVehicleStop();
        public abstract bool Move(TransferStep transferStep, ref string errorMsg);
        public abstract bool PartMove(MapPosition mapPosition);
        public abstract void RetryMove();
        public abstract void StopAndClear();
        public abstract void VehcleCancel();
        public abstract void VehcleContinue();
        public abstract bool VehclePause();
        public abstract void Close();

        public EnumMoveState MoveState { get; protected set; }
        public string StopResult { get; set; }
    }

    public class MoveControlFactory
    {
        public MoveControlPlate GetMoveControl(string type, MapInfo mapInfo, AlarmHandler alarmHandler, PlcAgent plcAgent)
        {
            MoveControlPlate moveControl = null;

            if (type == "AUO")
            {
                moveControl = new AuoMoveControl(mapInfo,alarmHandler,plcAgent);
            }

            return moveControl;
        }
    }
}
