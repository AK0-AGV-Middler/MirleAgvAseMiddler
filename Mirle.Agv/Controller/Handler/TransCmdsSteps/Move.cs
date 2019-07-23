using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Controller.Handler.TransCmdsSteps
{
    public class Move : ITransCmdsStep
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            TransCmd curTransCmd = mainFlowHandler.GetCurTransCmd();
            EnumTransCmdType type = curTransCmd.GetCommandType();

            switch (type)
            {
                case EnumTransCmdType.Move:                   
                    //TODO:                   
                    //Check if move complete
                    MoveCmdInfo moveCmd = (MoveCmdInfo)curTransCmd;
                    mainFlowHandler.PrepareForAskingReserve(moveCmd);
                    mainFlowHandler.PublishTransferMoveEvent(moveCmd);
                    mainFlowHandler.StartTrackingPosition();
                    mainFlowHandler.MiddleAgent_ResumeAskingReserve();                    
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
