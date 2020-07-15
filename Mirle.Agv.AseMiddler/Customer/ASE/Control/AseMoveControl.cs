using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using PSDriver.PSDriver;
using Mirle.Tools;
using System.Reflection;
using Mirle.Agv.AseMiddler.Model.Configs;
using System.Threading;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class AseMoveControl
    {
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private PSWrapperXClass psWrapper;

        private AseMoveConfig aseMoveConfig;

        public event EventHandler<EnumMoveComplete> OnMoveFinishedEvent;
        public event EventHandler<PSTransactionXClass> OnPrimarySendEvent;

        public string StopResult { get; set; } = "";

        public AseMoveControl(PSWrapperXClass psWrapper, AseMoveConfig aseMoveConfig)
        {
            this.psWrapper = psWrapper;
            this.aseMoveConfig = aseMoveConfig;
        }

        public void MoveFinished(EnumMoveComplete enumMoveComplete)
        {
            try
            {
                OnMoveFinishedEvent?.Invoke(this, enumMoveComplete);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }   

        private void LogException(string classMethodName, string exMsg)
        {
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }

    }
}
