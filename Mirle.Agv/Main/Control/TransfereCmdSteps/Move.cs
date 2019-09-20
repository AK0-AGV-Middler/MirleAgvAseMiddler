using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferSteps;

namespace Mirle.Agv.Controller.Handler.TransCmdsSteps
{
    [Serializable]
    public class Move : ITransferStatus
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            TransferStep curTransferStep = mainFlowHandler.GetCurTransferStep();
            EnumTransferStepType type = curTransferStep.GetTransferStepType();

            switch (type)
            {
                case EnumTransferStepType.Move:
                case EnumTransferStepType.MoveToCharger:
                    MoveCmdInfo moveCmd = (MoveCmdInfo)curTransferStep;
                    if (moveCmd.MovingSections.Count > 0)
                    {
                        if (mainFlowHandler.StopCharge())
                        {
                            mainFlowHandler.IsOverrideStopMove = false;
                            if (mainFlowHandler.CallMoveControlWork(moveCmd))
                            {
                                mainFlowHandler.PrepareForAskingReserve(moveCmd);
                            }                            
                        }
                        break;
                    }
                    else
                    {
                        //原地移動
                        mainFlowHandler.MoveControlHandler_OnMoveFinished(this, EnumMoveComplete.Success);
                        break;
                    }
                case EnumTransferStepType.Load:
                    mainFlowHandler.SetTransCmdsStep(new Load());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferStepType.Unload:
                    mainFlowHandler.SetTransCmdsStep(new Unload());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferStepType.Empty:
                default:
                    mainFlowHandler.SetTransCmdsStep(new Idle());
                    mainFlowHandler.DoTransfer();
                    break;
            }
        }

    }
}
