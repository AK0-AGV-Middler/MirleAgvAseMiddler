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
    public class AuoIntegrateControl : IntegrateControlPlate
    {
        private PlcAgent plcAgent;
        private TransferStep transferStep;
        private ushort forkCommandNumber = 0;

        public override event EventHandler<string> OnReadCarrierIdFinishEvent;
        public override event EventHandler<TransferStep> OnRobotInterlockErrorEvent;
        public override event EventHandler<TransferStep> OnRobotCommandFinishEvent;
        public override event EventHandler<TransferStep> OnRobotCommandErrorEvent;
        public override event EventHandler<double> OnBatteryPercentageChangeEvent;

        public AuoIntegrateControl(ClsMCProtocol.MCProtocol mcProtocol, AlarmHandler alarmHandler)
        {
            plcAgent = new PlcAgent(mcProtocol, alarmHandler);
            plcAgent.OnBatteryPercentageChangeEvent += PlcAgent_OnBatteryPercentageChangeEvent;
            plcAgent.OnForkCommandInterlockErrorEvent += PlcAgent_OnForkCommandInterlockErrorEvent;
            plcAgent.OnForkCommandFinishEvent += PlcAgent_OnForkCommandFinishEvent;
            plcAgent.OnForkCommandErrorEvent += PlcAgent_OnForkCommandErrorEvent;
            plcAgent.OnCassetteIDReadFinishEvent += PlcAgent_OnCassetteIDReadFinishEvent;
        }

        private void PlcAgent_OnCassetteIDReadFinishEvent(object sender, string e)
        {
            OnReadCarrierIdFinishEvent?.Invoke(this, e);
        }

        private void PlcAgent_OnForkCommandErrorEvent(object sender, PlcForkCommand e)
        {
            if (transferStep != null) OnRobotCommandErrorEvent?.Invoke(this, transferStep);
        }

        private void PlcAgent_OnForkCommandFinishEvent(object sender, PlcForkCommand e)
        {
            if (transferStep != null) OnRobotCommandFinishEvent?.Invoke(this, transferStep);
        }

        private void PlcAgent_OnForkCommandInterlockErrorEvent(object sender, PlcForkCommand e)
        {
            if (transferStep != null) OnRobotInterlockErrorEvent?.Invoke(this, transferStep);
        }

        private void PlcAgent_OnBatteryPercentageChangeEvent(object sender, ushort e)
        {
            OnBatteryPercentageChangeEvent?.Invoke(this, e);
        }

        public override void ClearRobotCommand()
        {
            plcAgent.ClearExecutingForkCommand();
            transferStep = null;
        }

        public override bool DoRobotCommand(TransferStep transferStep)
        {
            this.transferStep = transferStep;
            return plcAgent.AddForkComand(GetForkCommand(transferStep));
        }

        private PlcForkCommand GetForkCommand(TransferStep transferStep)
        {
            EnumForkCommand forkCommandType = EnumForkCommand.None;
            string stageNo = "";
            EnumStageDirection stageDirection = EnumStageDirection.None;
            bool isPio = false;
            ushort forkSpeed = 0;

            if (transferStep.GetTransferStepType() == EnumTransferStepType.Load)
            {
                LoadCmdInfo loadCmdInfo = (LoadCmdInfo)transferStep;
                forkCommandType = EnumForkCommand.Load;
                stageNo = loadCmdInfo.StageNum.ToString();
                isPio = loadCmdInfo.IsEqPio;
                forkSpeed = loadCmdInfo.ForkSpeed;
            }
            else if (transferStep.GetTransferStepType() == EnumTransferStepType.Unload)
            {
                UnloadCmdInfo unloadCmdInfo = (UnloadCmdInfo)transferStep;
                forkCommandType = EnumForkCommand.Unload;
                stageNo = unloadCmdInfo.StageNum.ToString();
                isPio = unloadCmdInfo.IsEqPio;
                forkSpeed = unloadCmdInfo.ForkSpeed;
            }
            else
            {
                return null;
            }
            return new PlcForkCommand(forkCommandNumber++, forkCommandType, stageNo, stageDirection, isPio, forkSpeed);
        }

        public override bool IsRobotCommandExist()
        {
            return plcAgent.IsForkCommandExist();
        }

        public override string ReadCarrierId()
        {
            string carrierId = "";
            plcAgent.triggerCassetteIDReader(ref carrierId);
            return carrierId;
        }

        public override bool ResetAllAlarm()
        {
            plcAgent.WritePLCAlarmReset();
            return plcAgent.SetAlarmWarningReportAllReset();
        }

        public override bool SetAlarm(Alarm alarm)
        {
            if (alarm.PlcWord != 0 || alarm.PlcBit != 0)
            {
                return plcAgent.WriteAlarmWarningReport(alarm.Level, alarm.PlcWord, alarm.PlcBit, true);
            }
            else
            {
                return false;
            }
        }

        public override bool SetAlarmStatus(bool hasAlarm, bool hasWarn)
        {
          return  plcAgent.WriteAlarmWarningStatus(hasAlarm, hasWarn);
        }

        public override void SetAutoManualState(EnumIPCStatus status)
        {
            plcAgent.WriteIPCStatus(status);
        }

        public void SetOutsideObjects(MainForm mainForm)
        {
            plcAgent.SetOutSideObj(mainForm);
        }

        public override void SetPercentage(double percentage)
        {
            plcAgent.setSOC(percentage);
        }

        public override bool StartCharge(EnumChargeDirection chargeDirection)
        {
           return plcAgent.ChargeStartCommand(chargeDirection);
        }

        public override bool StopCharge()
        {
            return plcAgent.ChargeStopCommand();
        }

        public PlcAgent GetPlcAgent() => plcAgent;

        public bool IsIsFirstMeterAhGet()
        {
            return plcAgent.IsFirstMeterAhGet;
        }

        public override void StopBuzzer()
        {
            plcAgent.WritePLCBuzzserStop();
        }
    }
}
