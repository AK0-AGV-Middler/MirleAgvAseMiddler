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
            TransferStep curTransCmd = mainFlowHandler.GetCurTransferStep();
            EnumTransferStepType type = curTransCmd.GetTransferStepType();

            switch (type)
            {
                case EnumTransferStepType.Move:
                case EnumTransferStepType.MoveToCharger:
                    MoveCmdInfo moveCmd = (MoveCmdInfo)curTransCmd;
                    if (mainFlowHandler.StopCharge())
                    {
                        if (mainFlowHandler.CallMoveControlWork(moveCmd))
                        {
                            mainFlowHandler.PrepareForAskingReserve(moveCmd);
                        }                        
                    }                   
                    break;
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
