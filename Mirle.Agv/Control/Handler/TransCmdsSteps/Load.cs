using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Control.Handler.TransCmdsSteps
{
    public class Load : ITransCmdsStep
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            TransCmd curTransCmd = mainFlowHandler.GetCurTransCmd();
            var type = curTransCmd.GetCommandType();

            switch (type)
            {
                case EnumTransCmdType.Move:
                    mainFlowHandler.SetTransCmdsStep(new Move());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransCmdType.Load:
                    LoadCmdInfo loadCmdInfo = (LoadCmdInfo)curTransCmd;
                    mainFlowHandler.Load(loadCmdInfo);
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
