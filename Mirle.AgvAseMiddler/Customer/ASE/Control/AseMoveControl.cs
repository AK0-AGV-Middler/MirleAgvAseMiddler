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

namespace Mirle.AgvAseMiddler.Controller
{
    public class AseMoveControl
    {
        private PSWrapperXClass psWrapper;
        private MirleLogger mirleLogger = MirleLogger.Instance;

        public event EventHandler<EnumMoveComplete> OnMoveFinishEvent;
        public event EventHandler<EnumMoveComplete> OnRetryMoveFinishEvent;      

        public string StopResult { get; set; } = "";

        public AseMoveControl(PSWrapperXClass psWrapper)
        {
            this.psWrapper = psWrapper;
        }

        public void OnMoveFinish(EnumMoveComplete enumMoveComplete)
        {
            try
            {
                OnMoveFinishEvent?.Invoke(this, enumMoveComplete);
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

        public bool Move(TransferStep transferStep, ref string errorMsg)
        {
            throw new NotImplementedException();
        }

        public void PartMove(bool isEnd, MapPosition mapPosition, int theta, int speed)
        {
            try
            {
                string message = isEnd ? "1" : "0";
                string positionX = GetPositionString(mapPosition.X);
                string positionY = GetPositionString(mapPosition.Y);
                string thetaString = GetNumberToString(theta, 3);
                string speedString = GetNumberToString(speed, 4);
                message = string.Concat(message, positionX, positionY, thetaString, speedString);

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
                string number = GetNumberToString((int)(Math.Abs(x)),8);
                result = string.Concat(result, number);
                return result;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return "";
            }
        }

        private string GetNumberToString(int value,ushort digit)
        {
            try
            {
                string valueFormat = new string('0',digit);
                
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
