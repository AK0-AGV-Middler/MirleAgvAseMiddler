using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Control.Handler
{
    /// <summary>
    /// 搬送命令處理者(MoveController,LoadController,UnloadController...)
    /// </summary>
    public interface ITransferHandler
    {
        /// <summary>
        /// 執行搬送命令
        /// </summary>
        void DoTransfer(TransCmd transCmd);
    }
}
