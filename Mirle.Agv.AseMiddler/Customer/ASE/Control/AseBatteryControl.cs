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
using Mirle.Agv.AseMiddler.Model.Configs;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class AseBatteryControl
    {
        private Vehicle theVehicle = Vehicle.Instance;
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private PSWrapperXClass psWrapper;
        private AseBatteryConfig aseBatteryConfig;
        private Thread thdWatchBatteryState;
        public bool IsWatchBatteryStatusPause { get; private set; } = false;
        public bool IsWatchBatteryStatusStop { get; private set; } = false;

        public event EventHandler<double> OnBatteryPercentageChangeEvent;

        public AseBatteryControl(PSWrapperXClass psWrapper, AseBatteryConfig aseBatteryConfig)
        {
            this.psWrapper = psWrapper;
            this.aseBatteryConfig = aseBatteryConfig;          
            InitialThread();
        }

        #region Thread

        private void InitialThread()
        {
            thdWatchBatteryState = new Thread(WatchBatteryState);
            thdWatchBatteryState.IsBackground = true;
            thdWatchBatteryState.Start();
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "AsePackage : 開始監看詢問電量");
        }

        private void WatchBatteryState()
        {
            while (true)
            {
                try
                {
                    if (IsWatchBatteryStatusPause) continue;
                    if (IsWatchBatteryStatusStop) break;

                    if (psWrapper.IsConnected())
                    {
                        SendBatteryStatusRequest();
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
                finally
                {
                    if (theVehicle.IsCharging)
                    {
                        SpinWait.SpinUntil(() => false, aseBatteryConfig.WatchBatteryStateIntervalInCharging);
                    }
                    else
                    {
                        SpinWait.SpinUntil(() => false, aseBatteryConfig.WatchBatteryStateInterval);
                    }                   
                }
            }
        }

        public void SendBatteryStatusRequest()
        {
            try
            {
                PrimarySend("P35", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        #endregion

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

        public bool StartCharge(EnumAddressDirection chargeDirection)
        {
            try
            {
                if (theVehicle.IsCharging) return true;

                string chargeDirectionString;
                switch (chargeDirection)
                {
                    case EnumAddressDirection.Left:
                        chargeDirectionString = "1";
                        break;
                    case EnumAddressDirection.Right:
                        chargeDirectionString = "2";
                        break;
                    case EnumAddressDirection.None:
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

        public void UpdateBatteryStatus()
        {
            try
            {
                AseBatteryStatus aseBatteryStatus = new AseBatteryStatus(theVehicle.AseBatteryStatus);

                if (aseBatteryStatus.Voltage >= aseBatteryConfig.CcmodeStopVoltage)
                {
                    StopCharge();
                    FullCharge();
                    return;
                }

                double tempPercentage = (aseBatteryConfig.WorkingAh - (aseBatteryConfig.CcmodeAh - aseBatteryStatus.Ah)) / aseBatteryConfig.WorkingAh * 100;
                tempPercentage = Math.Max(aseBatteryStatus.Percentage, 0);

                if (tempPercentage >= 100)
                {
                    StopCharge();
                    FullCharge();
                    return;
                }

                if (aseBatteryStatus.Percentage == 100)
                {
                    if (tempPercentage <= 99.98)
                    {
                        aseBatteryStatus.Percentage = tempPercentage;
                        theVehicle.AseBatteryStatus = aseBatteryStatus;
                    }
                }
                else
                {
                    aseBatteryStatus.Percentage = tempPercentage;
                    theVehicle.AseBatteryStatus = aseBatteryStatus;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void FullCharge()
        {
            try
            {
                AseBatteryStatus aseBatteryStatus = new AseBatteryStatus(theVehicle.AseBatteryStatus);
                theVehicle.AseBatteryStatus.Percentage = 100;

                aseBatteryConfig.CcmodeAh = aseBatteryStatus.Ah;
                aseBatteryConfig.CcmodeCounter++;
                if (aseBatteryConfig.CcmodeCounter >= aseBatteryConfig.AhResetCcmodeCounter)
                {
                    SendResetCcmodeAh();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void SendResetCcmodeAh()
        {
             //TODO : Set AGVL battery ah to 0
             //TODO : After AGVL set ah to 0, fix middler ccmode ah to 0 in case ah<->percentage error
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
