using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferSteps;
using Mirle.Agv.View;
using PSDriver.PSDriver;
using System.Collections.Concurrent;

namespace Mirle.Agv.Controller
{
    public class AseIntegrateControl : IntegrateControlPlate
    {
        public override event EventHandler<string> OnReadCarrierIdFinishEvent;
        public override event EventHandler<TransferStep> OnRobotInterlockErrorEvent;
        public override event EventHandler<TransferStep> OnRobotCommandFinishEvent;
        public override event EventHandler<TransferStep> OnRobotCommandErrorEvent;
        public override event EventHandler<double> OnBatteryPercentageChangeEvent;
                       

        public override void ClearRobotCommand()
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Type = "P";
                psMessage.Number = "49";
                psMessage.PSMessage = "";

                PrimarySendEnqueue(psMessage);                
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        private void PrimarySendEnqueue(PSMessageXClass psMessage)
        {
            throw new NotImplementedException();
        }

        public override bool DoRobotCommand(TransferStep transferStep)
        {
            throw new NotImplementedException();
        }

        public override bool IsRobotCommandExist()
        {
            throw new NotImplementedException();
        }

        public override string ReadCarrierId()
        {
            throw new NotImplementedException();
        }

        public override bool ResetAllAlarm()
        {
            throw new NotImplementedException();
        }

        public override bool SetAlarm(Alarm alarm)
        {
            throw new NotImplementedException();
        }

        public override bool SetAlarmStatus(bool hasAlarm, bool hasWarn)
        {
            throw new NotImplementedException();
        }

        public override void SetAutoManualState(EnumIPCStatus status)
        {
            throw new NotImplementedException();
        }

        public override void SetPercentage(double percentage)
        {
            throw new NotImplementedException();
        }

        public override bool StartCharge(EnumChargeDirection chargeDirection)
        {
            throw new NotImplementedException();
        }

        public override void StopBuzzer()
        {
            throw new NotImplementedException();
        }

        public override bool StopCharge()
        {
            throw new NotImplementedException();
        }

        private void LogException(string source, string exMsg)
        {
            Mirle.Tools.MirleLogger.Instance.Log(new Mirle.Tools.LogFormat("Error", "5", source, "Device", "CarrierID", exMsg));
        }
    }
}
