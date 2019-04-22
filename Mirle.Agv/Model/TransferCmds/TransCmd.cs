using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Control.Handler;

namespace Mirle.Agv.Model.TransferCmds
{
    public abstract class TransCmd
    {
        protected ITransferHandler transferHandler;
        protected EnumPartialJobType type;
        public string CmdId { get; set; }

        public TransCmd(ITransferHandler transferHandler)
        {
            this.transferHandler = transferHandler;
        }

        public void DoTransfer()
        {
            transferHandler.DoTransfer(this);
        }

        public EnumPartialJobType GetType()
        {
            return type;
        }
    }
}
