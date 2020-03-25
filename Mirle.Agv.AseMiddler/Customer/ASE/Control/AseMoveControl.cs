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
        public event EventHandler<EnumMoveComplete> OnRetryMoveFinishEvent;
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
                    if (IsWatchPositionPause) continue;
                    if (IsWatchPositionStop) break;

                    if (psWrapper.IsConnected())
                    {
                        SendPositionReportRequest();
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                PausePositionWatcher();
                OnMoveFinishedEvent?.Invoke(this, enumMoveComplete);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void OnRetryMoveFinish(EnumMoveComplete enumMoveComplete)
        {
            try
            {
                OnRetryMoveFinishEvent?.Invoke(this, enumMoveComplete);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void PartMove(AseMoveStatus aseMoveStatus)
        {
            try
            {
                string beginString = "2";
               
                string positionX = GetPositionString(aseMoveStatus.LastAddress.Position.X);
                string positionY = GetPositionString(aseMoveStatus.LastAddress.Position.Y);
                string thetaString = GetNumberToString((int)aseMoveStatus.LastAddress.VehicleHeadAngle, 3);
                string speedString = GetNumberToString((int)aseMoveStatus.LastSection.Speed, 4);
                string pioDirection = ((int)aseMoveStatus.LastAddress.PioDirection).ToString();
                string  message = string.Concat(beginString, positionX, positionY, thetaString, speedString, pioDirection);

                PrimarySend("P41", message);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void PartMove(EnumAddressDirection addressDirection, MapPosition mapPosition, int headAngle, int speed, EnumAseMoveCommandIsEnd isEnd)
        {
            ResumePositionWatcher();
            try
            {
                string isEndString = ((int)isEnd).ToString();
                string positionX = GetPositionString(mapPosition.X);
                string positionY = GetPositionString(mapPosition.Y);
                string thetaString = GetNumberToString(headAngle, 3);
                string speedString = GetNumberToString(speed, 4);
                string pioDirection = ((int)addressDirection).ToString();
                string message = string.Concat(isEndString, positionX, positionY, thetaString, speedString, pioDirection);

                PrimarySend("P41", message);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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

                OnPrimarySendEvent?.Invoke(this, psTransaction);
                //psWrapper.PrimarySent(ref psTransaction);
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
