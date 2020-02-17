using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Model.TransferSteps;

namespace Mirle.AgvAseMiddler.Controller
{
    public interface IRobotControl
    {
        void SetAutoManualState(EnumIPCStatus state);
        void ClearRobotCommand();
        string ReadCarrierId();
        bool DoRobotCommand(TransferStep transferStep);
        bool IsRobotCommandExist();

        event EventHandler<string> OnReadCarrierIdFinishEvent;
        event EventHandler<TransferStep> OnRobotInterlockErrorEvent;
        event EventHandler<TransferStep> OnRobotCommandFinishEvent;
        event EventHandler<TransferStep> OnRobotCommandErrorEvent;
    }
}
