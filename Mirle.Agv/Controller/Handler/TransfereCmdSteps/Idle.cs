using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Controller.Handler.TransCmdsSteps
{
    public class Idle : ITransferCmdStep
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            TransferStep curTransCmd = mainFlowHandler.GetCurTransCmd();
            var type = curTransCmd.GetCommandType();

            switch (type)
            {
                case EnumTransCmdType.Move:
                    mainFlowHandler.SetTransCmdsStep(new Move());
                    mainFlowHandler.DoTransfer();
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
                    //TODO:
                    //resume tracking position
                    //-> get position                    
                    //-> pause tracking position
                    mainFlowHandler.ResumeTrackingPosition();
                    SpinWait.SpinUntil(() => false, 50);
                    mainFlowHandler.PauseTrackingPosition();
                    mainFlowHandler.SetTransCmdsStep(new Idle());
                    mainFlowHandler.DoTransfer();
                    break;
            }

        }
    }
}
