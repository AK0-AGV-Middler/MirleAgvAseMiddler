using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;


namespace Mirle.Agv.Controller.Handler.TransCmdsSteps
{
    public class Unload : ITransferCmdStep
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
                    //TODO:
                    //resume tracking position
                    //-> get position
                    //-> send "InPosition" to Plc
                    //-> pause tracking position
                    //-> send "load" to plc
                    UnloadCmdInfo unloadCmd = (UnloadCmdInfo)curTransCmd;
                    mainFlowHandler.Unload(unloadCmd);
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
