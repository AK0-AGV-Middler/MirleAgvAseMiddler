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

namespace Mirle.Agv.AseMiddler.Controller
{
    public class AseBuzzerControl
    {
        private MirleLogger mirleLogger = MirleLogger.Instance;

        public event EventHandler<int> OnAlarmCodeSetEvent;
        public event EventHandler<int> OnAlarmCodeResetEvent;
        public event EventHandler OnAlarmCodeAllResetEvent;
        public event EventHandler<PSTransactionXClass> OnPrimarySendEvent;

        public void OnAlarmCodeSet(int alarmCode,bool isSet)
        {          
            try
            {
                if (isSet)
                {
                    OnAlarmCodeSetEvent?.Invoke(this, alarmCode);
                }
                else
                {
                    OnAlarmCodeResetEvent?.Invoke(this, alarmCode);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void OnAlarmCodeAllReset()
        {
            try
            {
                OnAlarmCodeAllResetEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void SetAlarmCode(int alarmCode, bool isSet)
        {
            try
            {
                string isSetString = isSet ? "1" : "0";                
                string alarmCodeString = alarmCode.ToString(new string('0', 6));
                string psMessage = string.Concat(isSetString, alarmCodeString);
                PrimarySend("61", psMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void ResetAllAlarmCode(object sneder,string msg)
        {
            try
            {
                PrimarySend("63", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void BuzzerOff()
        {
            try
            {
                PrimarySend("65", "");
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
