using System;
using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.TransferSteps;

namespace Mirle.AgvAseMiddler.Controller
{
    public class AuoMoveControl : MoveControlPlate
    {
        public override event EventHandler<EnumMoveComplete> OnMoveFinish;
        public override event EventHandler<EnumMoveComplete> OnRetryMoveFinish;

        private MoveControlHandler moveControlHandler;

        public AuoMoveControl(MapInfo mapInfo,AlarmHandler alarmHandler,PlcAgent plcAgent)
        {
            moveControlHandler = new MoveControlHandler(mapInfo, alarmHandler, plcAgent);
            moveControlHandler.OnMoveFinished += MoveControlHandler_OnMoveFinished;
            moveControlHandler.OnRetryMoveFinished += MoveControlHandler_OnRetryMoveFinished;
            MoveState = moveControlHandler.MoveState;
            StopResult = moveControlHandler.AGVStopResult;
        }       

        private void MoveControlHandler_OnRetryMoveFinished(object sender, EnumMoveComplete e)
        {
            OnRetryMoveFinish?.Invoke(this, e);
        }

        private void MoveControlHandler_OnMoveFinished(object sender, EnumMoveComplete e)
        {
            OnMoveFinish?.Invoke(this, e);
        }

        public override void Close()
        {
            moveControlHandler.CloseMoveControlHandler();
        }

        public override bool CanAuto(ref string errorMsg)
        {
            return moveControlHandler.MoveControlCanAuto(ref errorMsg);
        }

        public override bool IsPause()
        {
            return moveControlHandler.ControlData.PauseRequest || moveControlHandler.ControlData.PauseAlready;
        }

        public override bool IsVehicleStop()
        {
            return moveControlHandler.elmoDriver.MoveCompelete(EnumAxis.GX);
        }

        public override bool Move(TransferStep transferStep, ref string errorMsg)
        {
            return moveControlHandler.TransferMove_Override((MoveCmdInfo)transferStep, ref errorMsg);
        }

        public override void RetryMove()
        {
            moveControlHandler.TransferMove_RetryMove();
        }

        public override bool PartMove(MapPosition mapPosition)
        {
            return moveControlHandler.AddReservedMapPosition(mapPosition);
        }

        public override void StopAndClear()
        {
            moveControlHandler.StopAndClear();
        }

        public override void VehcleCancel()
        {
            moveControlHandler.VehcleCancel();
        }

        public override void VehcleContinue()
        {
            moveControlHandler.VehcleContinue();
        }

        public override bool VehclePause()
        {
           return moveControlHandler.VehclePause();
        }

        public MoveControlHandler GetMoveControlHandler() => moveControlHandler;
    }
}