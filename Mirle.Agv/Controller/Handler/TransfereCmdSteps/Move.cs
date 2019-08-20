using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Controller.Handler.TransCmdsSteps
{
    public class Move : ITransferStatus
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            TransferStep curTransCmd = mainFlowHandler.GetCurTransferStep();
            EnumTransferCommandType type = curTransCmd.GetEnumTransferCommandType();

            switch (type)
            {
                case EnumTransferCommandType.Move:
                    MoveCmdInfo moveCmd = (MoveCmdInfo)curTransCmd;
                    mainFlowHandler.StopCharge();                    
                    mainFlowHandler.CallMoveControlWork(moveCmd);
                    mainFlowHandler.PrepareForAskingReserve(moveCmd);
                    //mainFlowHandler.MiddleAgent_StartAskingReserve();
                    break;
                case EnumTransferCommandType.Load:
                    mainFlowHandler.SetTransCmdsStep(new Load());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferCommandType.Unload:
                    mainFlowHandler.SetTransCmdsStep(new Unload());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferCommandType.Empty:
                default:
                    mainFlowHandler.SetTransCmdsStep(new Idle());
                    mainFlowHandler.DoTransfer();
                    break;
            }
        }

    }
}
