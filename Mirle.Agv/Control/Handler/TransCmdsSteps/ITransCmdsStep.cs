namespace Mirle.Agv.Control.Handler.TransCmdsSteps
{
    public interface ITransCmdsStep
    {
        void DoTransfer(MainFlowHandler mainFlowHandler);
    }
}