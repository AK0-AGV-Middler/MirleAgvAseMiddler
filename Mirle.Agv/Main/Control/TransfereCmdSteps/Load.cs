using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Model.TransferSteps;

namespace Mirle.AgvAseMiddler.Controller.Handler.TransCmdsSteps
{
    [Serializable]
    public class Load : ITransferStatus
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            TransferStep curTransferStep = mainFlowHandler.GetCurTransferStep();
            EnumTransferStepType type = curTransferStep.GetTransferStepType();

            switch (type)
            {
                case EnumTransferStepType.Move:
                case EnumTransferStepType.MoveToCharger:
                    mainFlowHandler.SetTransCmdsStep(new Move());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferStepType.Load:
                    LoadCmdInfo loadCmdInfo = (LoadCmdInfo)curTransferStep;
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
