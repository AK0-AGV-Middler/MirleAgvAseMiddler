using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferSteps;


namespace Mirle.Agv.Controller.Handler.TransCmdsSteps
{
    [Serializable]
    public class Unload : ITransferStatus
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            TransferStep curTransCmd = mainFlowHandler.GetCurTransferStep();
            var type = curTransCmd.GetTransferStepType();

            switch (type)
            {
                case EnumTransferStepType.Move:
                case EnumTransferStepType.MoveToCharger:
                    mainFlowHandler.SetTransCmdsStep(new Move());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferStepType.Load:
                    mainFlowHandler.SetTransCmdsStep(new Load());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferStepType.Unload:
                    //TODO:
                    //resume track position
                    //-> get position
                    //-> send "InPosition" to Plc
                    //-> pause track position
                    //-> send "load" to plc
                    UnloadCmdInfo unloadCmd = (UnloadCmdInfo)curTransCmd;
                    mainFlowHandler.Unload(unloadCmd);
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
