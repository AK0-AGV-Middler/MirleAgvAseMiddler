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
        public event EventHandler<PSTransactionXClass> OnPrimarySendEvent;

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
                    if (IsWatchBatteryStatusPause)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
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

        public void PauseWatchBatteryState()
        {
            IsWatchBatteryStatusPause = true;
        }

        public void ResumeWatchBatteryState()
        {
            IsWatchBatteryStatusPause = false;
        }

        public void SendBatteryStatusRequest()
        {
            try
            {
                PrimarySend("35", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SendChargeStatusRequest()
        {
            try
            {
                PrimarySend("31", "4");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        #endregion

        public void SetPercentage(int percentage)
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

        public void StopCharge()
        {
            try
            {
                if (!theVehicle.IsCharging) return;

                PrimarySend("47", "0");

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void StartCharge(EnumAddressDirection chargeDirection)
        {
            try
            {
                if (theVehicle.IsCharging) return;

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
                PrimarySend("47", chargeDirectionString);

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
                theVehicle.AseBatteryStatus.Percentage = aseBatteryConfig.FullChargePercentage;

                OnBatteryPercentageChangeEvent?.Invoke(this, 100);

                StopCharge();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
