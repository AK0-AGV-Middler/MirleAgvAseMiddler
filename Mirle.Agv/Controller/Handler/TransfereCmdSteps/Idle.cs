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
    public class Idle : ITransferStatus
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            EnumTransferCommandType type = mainFlowHandler.GetCurrentEnumTransferCommandType();
            switch (type)
            {
                case EnumTransferCommandType.Move:
                    mainFlowHandler.SetTransCmdsStep(new Move());
                    mainFlowHandler.DoTransfer();
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
                    mainFlowHandler.IdleVisitNext();
                    break;
            }

        }
    }
}
