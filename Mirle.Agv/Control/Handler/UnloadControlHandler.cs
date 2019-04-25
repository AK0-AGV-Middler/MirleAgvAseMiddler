using Mirle.Agv.Model.TransferCmds;
using System;

namespace Mirle.Agv.Control
{
    public class UnloadControlHandler : ITransferHandler
    {
        public event EventHandler<EnumCompleteStatus> OnUnloadFinished;

        /// <summary>
        /// when unload finished, call this function to notice other class instance that unload is finished with status
        /// </summary>
        /// <param name="status"></param>
        private void UnloadFinished(EnumCompleteStatus status)
        {
            if (OnUnloadFinished != null)
            {
                OnUnloadFinished.Invoke(this, status);
            }
        }

        public void DoTransfer(TransCmd transCmd)
        {
            throw new NotImplementedException();
        }
    }
}
