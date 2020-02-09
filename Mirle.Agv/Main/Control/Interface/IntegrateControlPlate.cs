using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferSteps;


namespace Mirle.Agv.Controller
{
    public abstract class IntegrateControlPlate : IRobotControl, IBatterysControl, IBuzzerControl
    {
        public abstract event EventHandler<string> OnReadCarrierIdFinishEvent;
        public abstract event EventHandler<TransferStep> OnRobotInterlockErrorEvent;
        public abstract event EventHandler<TransferStep> OnRobotCommandFinishEvent;
        public abstract event EventHandler<TransferStep> OnRobotCommandErrorEvent;
        public abstract event EventHandler<double> OnBatteryPercentageChangeEvent;

        public abstract void ClearRobotCommand();
        public abstract bool DoRobotCommand(TransferStep transferStep);
        public abstract bool IsRobotCommandExist();
        public abstract string ReadCarrierId();
        public abstract bool ResetAllAlarm();
        public abstract bool SetAlarm(Alarm alarm);
        public abstract bool SetAlarmStatus(bool hasAlarm, bool hasWarn);
        public abstract void SetAutoManualState(EnumIPCStatus status);
        public abstract void SetPercentage(double percentage);
        public abstract bool StartCharge(EnumChargeDirection chargeDirection);
        public abstract void StopBuzzer();
        public abstract bool StopCharge();
    }

    public class IntegrateControlFactory
    {
        public IntegrateControlPlate GetIntegrateControl(string type,ClsMCProtocol.MCProtocol mcProtocol,AlarmHandler alarmHandler)
        {
            IntegrateControlPlate integrateControlPlate = null;

            if (type == "AUO")
            {
                integrateControlPlate = new AuoIntegrateControl(mcProtocol, alarmHandler);
            }
            else if (type == "ASE")
            {
                integrateControlPlate = new AseIntegrateControl();
            }

            return integrateControlPlate;
        }
    }
}
