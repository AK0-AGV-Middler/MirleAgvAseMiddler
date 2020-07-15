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
using System.Threading;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class AseRobotControl
    {
        public event EventHandler<EnumSlotNumber> OnReadCarrierIdFinishEvent;
        public event EventHandler<RobotCommand> OnRobotInterlockErrorEvent;
        public event EventHandler<RobotCommand> OnRobotCommandFinishEvent;
        public event EventHandler<RobotCommand> OnRobotCommandErrorEvent;
        public AseRobotControl(Dictionary<string, string> gateTypeMap)
        {

        }
    }
}
