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
        private MirleLogger mirleLogger = MirleLogger.Instance;

        public event EventHandler<EnumSlotNumber> OnReadCarrierIdFinishEvent;
        public event EventHandler<TransferStep> OnRobotInterlockErrorEvent;
        public event EventHandler<TransferStep> OnRobotCommandFinishEvent;
        public event EventHandler<TransferStep> OnRobotCommandErrorEvent;
        public event EventHandler<PSTransactionXClass> OnPrimarySendEvent;

        private Vehicle theVehicle = Vehicle.Instance;
        public RobotCommand RobotCommand { get; set; }
        private Dictionary<string, string> gateTypeMap = new Dictionary<string, string>();

        public AseRobotControl(Dictionary<string, string> gateTypeMap)
        {
            this.gateTypeMap = gateTypeMap;
        }

        public void OnReadCarrierIdFinish(EnumSlotNumber slotNumber)
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
                PrimarySend("49", "");
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
                PrimarySend("31", "3");
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
                PSTransactionXClass psTransaction = PrimarySend("45", robotCommandString);
                //int timeoutCount = 10;
                //while (timeoutCount >= 0)
                //{
                //    timeoutCount--;

                //    SpinWait.SpinUntil(() => !string.IsNullOrEmpty(psTransaction.PSSecondaryMessage.Type), 1500);
                //}

                //if (timeoutCount < 0)
                //{
                //    throw new Exception($"Receive robot command reply timeout.");
                //}

                //switch (psTransaction.PSSecondaryMessage.PSMessage.Substring(0, 1))
                //{
                //    case "0":
                //        return true;
                //    case "1":
                //        throw new Exception($"From port is empty");
                //    case "2":
                //        throw new Exception($"To port is full");
                //    default:
                //        return false;
                //}
                return true;
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
                string pioDirection = ((int)RobotCommand.PioDirection).ToString();
                string fromPort = "";
                string toPort = "";
                switch (RobotCommand.GetTransferStepType())
                {
                    case EnumTransferStepType.Load:
                        fromPort = RobotCommand.PortAddressId.PadLeft(5, '0');
                        toPort = RobotCommand.SlotNumber.ToString().PadLeft(5, '0');
                        break;
                    case EnumTransferStepType.Unload:
                        fromPort = RobotCommand.SlotNumber.ToString().PadLeft(5, '0');
                        toPort = RobotCommand.PortAddressId.PadLeft(5, '0');
                        break;
                    case EnumTransferStepType.Move:
                    case EnumTransferStepType.MoveToCharger:
                    case EnumTransferStepType.Empty:
                    default:
                        throw new Exception($"Robot command type error.[{RobotCommand.GetTransferStepType()}]");
                }

                string gateType = RobotCommand.GateType.Substring(0, 1);
                string portNumber = RobotCommand.PortNumber.Substring(0, 1);

                return string.Concat(pioDirection, fromPort, toPort, gateType, portNumber).PadRight(24, '0');
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

        public void RefreshRobotState()
        {
            try
            {
                PrimarySend("31", "2");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void RefreshCarrierSlotState()
        {
            try
            {
                PrimarySend("31", "3");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private PSTransactionXClass PrimarySend(string index, string message)
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Type = "P";
                psMessage.Number = index;
                psMessage.PSMessage = message;
                PSTransactionXClass psTransaction = new PSTransactionXClass();
                psTransaction.PSPrimaryMessage = psMessage;

                OnPrimarySendEvent?.Invoke(this, psTransaction);
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
