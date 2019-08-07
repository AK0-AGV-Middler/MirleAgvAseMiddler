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
            TransferStep curTransCmd = mainFlowHandler.GetCurTransCmd();
            EnumTransCmdType type = curTransCmd.GetCommandType();

            switch (type)
            {
                case EnumTransCmdType.Move:                   
                    MoveCmdInfo moveCmd = (MoveCmdInfo)curTransCmd;
                    mainFlowHandler.StopCharge();
                    mainFlowHandler.PrepareForAskingReserve(moveCmd);
                    mainFlowHandler.CallMoveControlWork(moveCmd);
                    //mainFlowHandler.StartTrackingPosition();
                    //mainFlowHandler.MiddleAgent_ResumeAskingReserve();
                    mainFlowHandler.MiddleAgent_RestartAskingReserve();
                    break;
                case EnumTransCmdType.Load:
                    mainFlowHandler.SetTransCmdsStep(new Load());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransCmdType.Unload:
                    mainFlowHandler.SetTransCmdsStep(new Unload());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransCmdType.Empty:
                default:
                    mainFlowHandler.SetTransCmdsStep(new Idle());
                    mainFlowHandler.DoTransfer();
                    break;
            }
        }

    }
}
