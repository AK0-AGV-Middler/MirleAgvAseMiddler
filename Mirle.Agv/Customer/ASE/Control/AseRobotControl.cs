using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.TransferSteps;
using PSDriver.PSDriver;
using Mirle.Tools;
using System.Reflection;
using System.Threading;

namespace Mirle.AgvAseMiddler.Controller
{
    public class AseRobotControl
    {
        private PSWrapperXClass psWrapper;
        private MirleLogger mirleLogger = MirleLogger.Instance;

        public event EventHandler<string> OnReadCarrierIdFinishEvent;
        public event EventHandler<TransferStep> OnRobotInterlockErrorEvent;
        public event EventHandler<TransferStep> OnRobotCommandFinishEvent;
        public event EventHandler<TransferStep> OnRobotCommandErrorEvent;

        private Vehicle theVehicle = Vehicle.Instance;
        public RobotCommand RobotCommand { get; set; }

        public AseRobotControl(PSWrapperXClass psWrapper)
        {
            this.psWrapper = psWrapper;
        }

        public void OnReadCarrierIdFinish(string slotNumber)
        {
            OnReadCarrierIdFinishEvent?.Invoke(this, slotNumber);
        }

        public void OnRobotInterlockError()
        {
            OnRobotInterlockErrorEvent?.Invoke(this, RobotCommand);
        }

        public void OnRobotCommandFinish()
        {
            OnRobotCommandFinishEvent?.Invoke(this, RobotCommand);
        }

        public void OnRobotCommandError()
        {
            OnRobotCommandErrorEvent?.Invoke(this, RobotCommand);
        }

        public void ClearRobotCommand()
        {
            try
            {
                PrimarySend("P49", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public string ReadCarrierId()
        {
            try
            {
                PrimarySend("P31", "3");
                return "";
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return "";
            }
        }

        public bool DoRobotCommand(RobotCommand robotCommand)
        {
            try
            {
                RobotCommand = robotCommand;
                string robotCommandString = GetRobotCommandString();
                PSTransactionXClass psTransaction = PrimarySend("P45", robotCommandString);
                int timeoutCount = 10;
                while (timeoutCount >= 0)
                {
                    timeoutCount--;

                    SpinWait.SpinUntil(() => !string.IsNullOrEmpty(psTransaction.PSSecondaryMessage.Type), 100);
                }

                if (timeoutCount < 0)
                {
                    throw new Exception($"Receive robot command reply timeout.");
                }

                switch (psTransaction.PSSecondaryMessage.PSMessage.Substring(0, 1))
                {
                    case "0":
                        return true;
                    case "1":
                        throw new Exception($"From port is empty");
                    case "2":
                        throw new Exception($"To port is full");
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }

        private string GetRobotCommandString()
        {
            try
            {
                string isPio = RobotCommand.IsEqPio ? "1" : "0";
                string pioDirection = ((int)RobotCommand.PioDirection).ToString();
                string robotSpeed = RobotCommand.ForkSpeed.ToString(new string('0', 3));
                string fromPort = "";
                string toPort = "";
                switch (RobotCommand.GetTransferStepType())
                {
                    case EnumTransferStepType.Load:
                        fromPort = RobotCommand.PortAddress.Substring(0, 2);
                        toPort = RobotCommand.SlotNumber.PadLeft(2, '0');
                        break;
                    case EnumTransferStepType.Unload:
                        fromPort = RobotCommand.SlotNumber.PadLeft(2, '0');
                        toPort = RobotCommand.PortAddress.Substring(0, 2);
                        break;
                    case EnumTransferStepType.Move:
                    case EnumTransferStepType.MoveToCharger:
                    case EnumTransferStepType.Empty:
                    default:
                        throw new Exception($"Robot command type error.[{RobotCommand.GetTransferStepType()}]");
                }

                return string.Concat(isPio,pioDirection,robotSpeed,fromPort,toPort);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        public bool IsRobotCommandExist()
        {
            if (theVehicle.AseRobotStatus.IsHome)
            {
                if (theVehicle.AseRobotStatus.RobotState == EnumAseRobotState.Idle)
                {
                    return false;
                }
            }
            return true;
        }

        private PSTransactionXClass PrimarySend(string index, string message)
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Type = index.Substring(0, 1);
                psMessage.Number = index.Substring(1, 2);
                psMessage.PSMessage = message;
                PSTransactionXClass psTransaction = new PSTransactionXClass();
                psTransaction.PSPrimaryMessage = psMessage;

                psWrapper.PrimarySent(ref psTransaction);
                return psTransaction;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return null;
            }
        }

        private void LogException(string classMethodName, string exMsg)
        {
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }

        private void LogDebug(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, "Device", "CarrierID", msg));
        }

    }
}
