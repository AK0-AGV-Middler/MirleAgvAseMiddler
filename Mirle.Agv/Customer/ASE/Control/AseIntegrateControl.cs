using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferSteps;
using Mirle.Agv.View;

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
    }
}
