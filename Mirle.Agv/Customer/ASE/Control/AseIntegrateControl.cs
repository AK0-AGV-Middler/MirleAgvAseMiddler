using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.TransferSteps;
using Mirle.AgvAseMiddler.View;
using PSDriver.PSDriver;
using System.Collections.Concurrent;
using Mirle.AgvAseMiddler.Model.Configs;
using Mirle.Tools;
using System.Threading;

namespace Mirle.AgvAseMiddler.Controller
{
    public class AseIntegrateControl : IntegrateControlPlate
    {
        public override event EventHandler<string> OnReadCarrierIdFinishEvent;
        public override event EventHandler<TransferStep> OnRobotInterlockErrorEvent;
        public override event EventHandler<TransferStep> OnRobotCommandFinishEvent;
        public override event EventHandler<TransferStep> OnRobotCommandErrorEvent;
        public override event EventHandler<double> OnBatteryPercentageChangeEvent;

        private PSWrapperXClass psWrapper;
        private PspConnectionConfig pspConnectionConfig;

        private Dictionary<string, PSMessageXClass> myMessageMap = new Dictionary<string, PSMessageXClass>();

        private bool isRobotReportRequestReply = false;
        private bool isCarrierIdReportRequestReply = false;

        private AseVehicleIntegrateStatus aseVehicleIntegrateStatus = (AseVehicleIntegrateStatus)Vehicle.Instance.TheVehicleIntegrateStatus;

        public AseIntegrateControl(PSWrapperXClass psWrapper)
        {
            this.psWrapper = psWrapper;
            LoadConfigs();
            BindEvent();
            psWrapper.Open();
        }

        private void BindEvent()
        {
            try
            {
                psWrapper.OnConnectionStateChange += PsWrapper_OnConnectionStateChange;
                psWrapper.OnPrimaryReceived += PsWrapper_OnPrimaryReceived;
                psWrapper.OnPrimarySent += PsWrapper_OnPrimarySent;
                psWrapper.OnSecondaryReceived += PsWrapper_OnSecondaryReceived;
                psWrapper.OnSecondarySent += PsWrapper_OnSecondarySent;
                psWrapper.OnTransactionError += PsWrapper_OnTransactionError;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsWrapper_OnTransactionError(string errorString, ref PSMessageXClass msg)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsWrapper_OnSecondarySent(ref PSTransactionXClass transaction)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsWrapper_OnSecondaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsWrapper_OnPrimarySent(ref PSTransactionXClass transaction)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsWrapper_OnPrimaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void PsWrapper_OnConnectionStateChange(enumConnectState state)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LoadConfigs()
        {
            try
            {
                pspConnectionConfig = new XmlHandler().ReadXml<PspConnectionConfig>(@"D:\AgvConfigs\PspConnectionConfig.xml");
                psWrapper.Address = pspConnectionConfig.Ip;
                psWrapper.Port = pspConnectionConfig.Port;
                psWrapper.ConnectMode = pspConnectionConfig.IsServer ? enumConnectMode.Passive : enumConnectMode.Active;

                LoadAutoReply();

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LoadAutoReply()
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public override void ClearRobotCommand()
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Type = "P";
                psMessage.Number = "49";
                psMessage.PSMessage = "";

                PrimarySend(psMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        private void PrimarySend(PSMessageXClass psMessage)
        {
            try
            {
                PSTransactionXClass psTransaction = new PSTransactionXClass();
                psTransaction.PSPrimaryMessage = psMessage;
                psWrapper.PrimarySent(ref psTransaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void SecondarySend(PSTransactionXClass psTransaction)
        {
            try
            {
                psWrapper.SecondarySent(ref psTransaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public override bool DoRobotCommand(TransferStep transferStep)
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Number = "45";
                psMessage.Type = "P";
                psMessage.PSMessage = GetRobotMessageFromTransferStep();
                if (psMessage.PSMessage == "") return false;

                PSTransactionXClass psTransaction = new PSTransactionXClass();
                psTransaction.PSPrimaryMessage = psMessage;

                psWrapper.PrimarySent(ref psTransaction);

                return true;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return false;
            }
        }

        private string GetRobotMessageFromTransferStep()
        {
            try
            {
                string result = "";

                string pioPart = "00";  //NoPio

                //if (cbIsPio.Checked)
                //{
                //    if (radLeftPio.Checked)
                //    {
                //        pioPart = "11";
                //    }
                //    else
                //    {
                //        pioPart = "12";
                //    }
                //}

                //string forkSpeed = GetStringFromNumUpDown(numForkSpeed.Value, 3);
                //string fromPort = txtFromPort.Text.Substring(0, 2);
                //string toPort = txtToPort.Text.Substring(0, 2);


                //result = string.Concat(pioPart, forkSpeed, fromPort, toPort);

                return result;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return "";
            }
        }

        public override bool IsRobotCommandExist()
        {
            try
            {
                //isRobotReportRequestReply = false;

                //RobotReportRequest();

                //int timeoutCount = 10;
                //while (true)
                //{
                //    if (isRobotReportRequestReply)
                //    {
                //        break;
                //    }

                //    if (timeoutCount > 0)
                //    {
                //        timeoutCount--;
                //    }
                //    else
                //    {
                //        string exMsg = "IsRobotReportRequestReply timeout";
                //        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, exMsg);
                //        return true;
                //    }
                //}

                return !(aseVehicleIntegrateStatus.AseRobotState == EnumAseRobotState.Idle);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return true;
            }
        }

        private void RobotReportRequest()
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Type = "P";
                psMessage.Number = "31";
                psMessage.PSMessage = "2";

                PrimarySend(psMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public override string ReadCarrierId()
        {
            try
            {
                isCarrierIdReportRequestReply = false;

                CarrierIdReportRequest();

                int timeoutCount = 10;
                while (true)
                {
                    if (isCarrierIdReportRequestReply)
                    {
                        break;
                    }

                    if (timeoutCount > 0)
                    {
                        timeoutCount--;
                    }
                    else
                    {
                        string exMsg = "IsRobotReportRequestReply timeout";
                        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, exMsg);
                        return "ERROR";
                    }

                    SpinWait.SpinUntil(() => false, 100);
                }

                return aseVehicleIntegrateStatus.CarrierSlot.CarrierId;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return "ERROR";
            }
        }

        private void CarrierIdReportRequest()
        {
            throw new NotImplementedException();
        }

        public override bool ResetAllAlarm()
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
            return false;

        }

        public override bool SetAlarm(Alarm alarm)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
            return false;

        }

        public override bool SetAlarmStatus(bool hasAlarm, bool hasWarn)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
            return false;

        }

        public override void SetAutoManualState(EnumIPCStatus status)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public override void SetPercentage(double percentage)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        public override bool StartCharge(EnumChargeDirection chargeDirection)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
            return false;

        }

        public override void StopBuzzer()
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public override bool StopCharge()
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

            return false;
        }

        private void LogException(string source, string exMsg)
        {
            Mirle.Tools.MirleLogger.Instance.Log(new Mirle.Tools.LogFormat("Error", "5", source, "Device", "CarrierID", exMsg));
        }

        private void LogDebug(string source, string msg)
        {
            Mirle.Tools.MirleLogger.Instance.Log(new Mirle.Tools.LogFormat("Debug", "5", source, "Device", "CarrierID", msg));
        }

        private void LogPspWrapper(string source, string msg)
        {
            Mirle.Tools.MirleLogger.Instance.Log(new Mirle.Tools.LogFormat("PspWrapper", "5", source, "Device", "CarrierID", msg));
        }

    }
}
