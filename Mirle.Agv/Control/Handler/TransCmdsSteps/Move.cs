﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Control.Handler.TransCmdsSteps
{
    public class Move : ITransCmdsStep
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            TransCmd curTransCmd = mainFlowHandler.GetCurTransCmd();
            var type = curTransCmd.GetType();

            switch (type)
            {
                case EnumTransCmdType.Move:                                     
                    EnqueMoveCmd(mainFlowHandler, curTransCmd);
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

        private static void EnqueMoveCmd(MainFlowHandler mainFlowHandler, TransCmd curTransCmd)
        {
            MoveCmdInfo moveCmd = (MoveCmdInfo)curTransCmd;
            mainFlowHandler.EnqueWaitForReserve(moveCmd);
            if (!moveCmd.IsPrecisePositioning)
            {
                mainFlowHandler.TransCmdsIndex++;
                mainFlowHandler.GoNextTransCmd = true;
            }
        }
    }
}
