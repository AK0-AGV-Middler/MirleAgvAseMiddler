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
    public class AseBatteryControl
    {
        private PSWrapperXClass psWrapper;
        private MirleLogger mirleLogger = MirleLogger.Instance;

        private Vehicle theVehicle = Vehicle.Instance;
        public event EventHandler<double> OnBatteryPercentageChangeEvent;

        public AseBatteryControl(PSWrapperXClass psWrapper)
        {
            this.psWrapper = psWrapper;
        }

        public void SetPercentage(double percentage)
        {
            try
            {
                if (Math.Abs(percentage - theVehicle.AseBatteryStatus.Percentage) >= 1)
                {
                    theVehicle.AseBatteryStatus.Percentage = percentage;
                    OnBatteryPercentageChangeEvent?.Invoke(this, percentage);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public bool StopCharge()
        {
            try
            {
                if (!theVehicle.IsCharging) return true;

                PrimarySend("P47", "0");

                return !theVehicle.IsCharging;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }

        public bool StartCharge(EnumChargeDirection chargeDirection)
        {
            try
            {
                if (theVehicle.IsCharging) return true;

                string chargeDirectionString;
                switch (chargeDirection)
                {                   
                    case EnumChargeDirection.Left:
                        chargeDirectionString = "1";                       
                        break;
                    case EnumChargeDirection.Right:
                        chargeDirectionString = "2";
                        break;
                    case EnumChargeDirection.None:
                    default:
                        throw new Exception($"Start charge command direction error.[{chargeDirection}]");
                }
                PrimarySend("P47", chargeDirectionString);                

                return theVehicle.IsCharging;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
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
