using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Controller.Handler.TransCmdsSteps
{
    [Serializable]
    public class Load : ITransferStatus
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            TransferStep curTransCmd = mainFlowHandler.GetCurTransferStep();
            EnumTransferStepType type = curTransCmd.GetTransferStepType();

            switch (type)
            {
                case EnumTransferStepType.Move:
                    mainFlowHandler.SetTransCmdsStep(new Move());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferStepType.Load:
                    LoadCmdInfo loadCmdInfo = (LoadCmdInfo)curTransCmd;
                    mainFlowHandler.Load(loadCmdInfo);
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
