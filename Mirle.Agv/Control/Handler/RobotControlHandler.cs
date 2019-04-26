using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Control
{
    public class RobotControlHandler
    {
        public event EventHandler<EnumCompleteStatus> OnLoadFinished;
        public event EventHandler<EnumCompleteStatus> OnUnloadFinished;


        /// <summary>
        /// when load finished, call this function to notice other class instance that load is finished with status
        /// </summary>
        private void LoadFinished(EnumCompleteStatus status)
        {
            if (OnLoadFinished != null)
            {
                OnLoadFinished.Invoke(this, status);
            }
        }

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

        public void DoLoad(LoadCmdInfo loadCmd)
        {
            throw new NotImplementedException();
        }


        public void DoUnload(UnloadCmdInfo unloadCmd)
        {
            throw new NotImplementedException();
        }

    }
}
