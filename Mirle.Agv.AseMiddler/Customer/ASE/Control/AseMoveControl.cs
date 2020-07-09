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
        private Thread thdWatchPosition;
        public bool IsWatchPositionPause { get; private set; } = true;
        public bool IsWatchPositionStop { get; private set; } = false;

        public event EventHandler<EnumMoveComplete> OnMoveFinishedEvent;
        public event EventHandler<PSTransactionXClass> OnPrimarySendEvent;

        public string StopResult { get; set; } = "";

        public AseMoveControl(PSWrapperXClass psWrapper, AseMoveConfig aseMoveConfig)
        {
            this.psWrapper = psWrapper;
            this.aseMoveConfig = aseMoveConfig;
            InitialThread();
        }

        #region Thread

        private void InitialThread()
        {
            thdWatchPosition = new Thread(WatchPosition);
            thdWatchPosition.IsBackground = true;
            thdWatchPosition.Start();
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "AsePackage : 開始監看詢問電量");
        }

        private void WatchPosition()
        {
            while (true)
            {
                try
                {
                    if (IsWatchPositionPause)
                    {
                        SpinWait.SpinUntil(() => false, aseMoveConfig.WatchPositionInterval);
                        continue;
                    }
                    if (IsWatchPositionStop) break;

                    SendPositionReportRequest();
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, aseMoveConfig.WatchPositionInterval);
                }
            }
        }

        public void SendPositionReportRequest()
        {
            try
            {
                PrimarySend("P33", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PausePositionWatcher()
        {
            IsWatchPositionPause = true;
        }

        public void ResumePositionWatcher()
        {
            IsWatchPositionPause = false;
        }

        #endregion

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

        public void PartMove(MapPosition mapPosition, int headAngle, int speed, EnumAseMoveCommandIsEnd isEnd, EnumKeepOrGo keepOrGo, EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            ResumePositionWatcher();
            try
            {
                string isEndString = ((int)isEnd).ToString();
                string positionX = GetPositionString(mapPosition.X);
                string positionY = GetPositionString(mapPosition.Y);
                string thetaString = GetNumberToString(headAngle, 3);
                string speedString = GetNumberToString(speed, 4);
                string openSlotString = ((int)openSlot).ToString();
                string keepOrGoString = keepOrGo.ToString().Substring(0, 1).ToUpper();
                string message = string.Concat(isEndString, positionX, positionY, thetaString, speedString, openSlotString, keepOrGoString);

                PrimarySend("P41", message);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PartMove(EnumAseMoveCommandIsEnd enumAseMoveCommandIsEnd,EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.Instance.AseMoveStatus);
                string beginString = ((int)enumAseMoveCommandIsEnd).ToString();
                string positionX = GetPositionString(aseMoveStatus.LastAddress.Position.X);
                string positionY = GetPositionString(aseMoveStatus.LastAddress.Position.Y);
                string thetaString = GetNumberToString((int)aseMoveStatus.LastAddress.VehicleHeadAngle, 3);
                string speedString = GetNumberToString((int)aseMoveStatus.LastSection.Speed, 4);
                string openSlotString = ((int)openSlot).ToString();
                string keepOrGoString = "G";
                string message = string.Concat(beginString, positionX, positionY, thetaString, speedString, openSlotString, keepOrGoString);

                PrimarySend("P41", message);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string GetPositionString(double x)
        {
            try
            {
                string result = x >= 0 ? "P" : "N";
                string number = GetNumberToString((int)(Math.Abs(x)), 8);
                result = string.Concat(result, number);
                return result;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        private string GetNumberToString(int value, ushort digit)
        {
            try
            {
                string valueFormat = new string('0', digit);

                return value.ToString(valueFormat);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        public void RetryMove()
        {

        }

        public void StopAndClear()
        {
            try
            {
                PrimarySend("P51", "2");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void VehcleCancel()
        {
            try
            {
                PrimarySend("P51", "2");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void VehcleContinue()
        {
            try
            {
                PrimarySend("P51", "1");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void VehclePause()
        {
            try
            {
                PrimarySend("P51", "0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void RefreshMoveState()
        {
            try
            {
                PrimarySend("P31", "1");
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
                psMessage.Type = index.Substring(0,1);
                psMessage.Number = index.Substring(1,2);
                psMessage.PSMessage = message;
                PSTransactionXClass psTransaction = new PSTransactionXClass();
                psTransaction.PSPrimaryMessage = psMessage;

                OnPrimarySendEvent?.Invoke(this, psTransaction);
                return psTransaction;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
