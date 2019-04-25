using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Control
{
    public class LoadControlHandler : ITransferHandler
    {
        public event EventHandler<EnumCompleteStatus> OnLoadFinished;

        /// <summary>
        /// when load finished, call this function to notice other class instance that load is finished with status
        /// </summary>
        private void LoadFinished(EnumCompleteStatus status)
        {
            if (OnLoadFinished!=null)
            {
                OnLoadFinished.Invoke(this, status);
            }
        }

        public void DoTransfer(TransCmd transCmd)
        {
            throw new NotImplementedException();
        }
    }
}
