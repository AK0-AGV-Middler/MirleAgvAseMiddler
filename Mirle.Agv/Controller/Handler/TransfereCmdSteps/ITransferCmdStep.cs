namespace Mirle.Agv.Controller.Handler.TransCmdsSteps
{
    public interface ITransferCmdStep
    {
        void DoTransfer(MainFlowHandler mainFlowHandler);
    }
}