namespace Mirle.Agv.Controller.Handler.TransCmdsSteps
{
    public interface ITransCmdsStep
    {
        void DoTransfer(MainFlowHandler mainFlowHandler);
    }
}