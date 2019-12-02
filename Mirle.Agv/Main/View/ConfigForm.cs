using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Controller;
using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferSteps;
using Mirle.Agv;
using Mirle.Agv.Controller.Tools;
using System.Reflection;
using Mirle.Agv.Model.Configs;

namespace Mirle.Agv.View
{
    public partial class ConfigForm : Form
    {
        private MainFlowHandler mainFlowHandler;
        private MainFlowConfig mainFlowConfig;
        private MiddlerConfig middlerConfig;

        public ConfigForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
        }

        private void timerUpdateConfigs_Tick(object sender, EventArgs e)
        {
            switch (tabConfigs.SelectedIndex)
            {
                case 0:
                    UpdateMainFlowConfigCv();
                    break;
                case 1:
                    UpdateMainFlowConfigCv();
                    break;
                default:
                    break;
            }
        }

        private void UpdateMainFlowConfigCv()
        {
            try
            {
                mainFlowConfig = mainFlowHandler.GetMainFlowConfig();
                ShowMainFlowConfigCvOnForm(mainFlowConfig);
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void ShowMainFlowConfigCvOnForm(MainFlowConfig mainFlowConfig)
        {
            try
            {
                tbxVisitTransferStepsCv.Text = mainFlowConfig.VisitTransferStepsSleepTimeMs.ToString("F0");
                tbxTrackPositionCv.Text = mainFlowConfig.TrackPositionSleepTimeMs.ToString("F0");
                tbxWatchLowPowerCv.Text = mainFlowConfig.WatchLowPowerSleepTimeMs.ToString("F0");
                tbxReportPositionCv.Text = mainFlowConfig.ReportPositionIntervalMs.ToString("F0");
                tbxStartChargeTimeoutCv.Text = mainFlowConfig.StartChargeWaitingTimeoutMs.ToString("F0");
                tbxStopChargeTimeoutCv.Text = mainFlowConfig.StopChargeWaitingTimeoutMs.ToString("F0");
                tbxPositionRangeCv.Text = mainFlowConfig.RealPositionRangeMm.ToString("F0");
                tbxLoadingChargeCv.Text = mainFlowConfig.LoadingChargeIntervalMs.ToString("F0");
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void ShowMainFlowConfigSvOnForm(MainFlowConfig mainFlowConfig)
        {
            try
            {
                tbxVisitTransferStepsSv.Text = mainFlowConfig.VisitTransferStepsSleepTimeMs.ToString("F0");
                tbxTrackPositionSv.Text = mainFlowConfig.TrackPositionSleepTimeMs.ToString("F0");
                tbxWatchLowPowerSv.Text = mainFlowConfig.WatchLowPowerSleepTimeMs.ToString("F0");
                tbxReportPositionSv.Text = mainFlowConfig.ReportPositionIntervalMs.ToString("F0");
                tbxStartChargeTimeoutSv.Text = mainFlowConfig.StartChargeWaitingTimeoutMs.ToString("F0");
                tbxStopChargeTimeoutSv.Text = mainFlowConfig.StopChargeWaitingTimeoutMs.ToString("F0");
                tbxPositionRangeSv.Text = mainFlowConfig.RealPositionRangeMm.ToString("F0");
                tbxLoadingChargeSv.Text = mainFlowConfig.LoadingChargeIntervalMs.ToString("F0");
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void btnLoadMainFlowConfig_Click(object sender, EventArgs e)
        {
            try
            {
                mainFlowHandler.LoadMainFlowConfig();
                UpdateMainFlowConfigCv();
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void btnSaveMainFlowConfig_Click(object sender, EventArgs e)
        {
            try
            {
                MainFlowConfig tempMainFlowConfig = new MainFlowConfig();
                tempMainFlowConfig.LogConfigPath = mainFlowConfig.LogConfigPath;
                tempMainFlowConfig.VisitTransferStepsSleepTimeMs = int.Parse(tbxVisitTransferStepsSv.Text);
                tempMainFlowConfig.TrackPositionSleepTimeMs = int.Parse(tbxTrackPositionSv.Text);
                tempMainFlowConfig.WatchLowPowerSleepTimeMs = int.Parse(tbxWatchLowPowerSv.Text);
                tempMainFlowConfig.ReportPositionIntervalMs = int.Parse(tbxReportPositionSv.Text);
                tempMainFlowConfig.StartChargeWaitingTimeoutMs = int.Parse(tbxStartChargeTimeoutSv.Text);
                tempMainFlowConfig.StopChargeWaitingTimeoutMs = int.Parse(tbxStopChargeTimeoutSv.Text);
                tempMainFlowConfig.RealPositionRangeMm = int.Parse(tbxPositionRangeSv.Text);
                tempMainFlowConfig.LoadingChargeIntervalMs = int.Parse(tbxLoadingChargeSv.Text);

                mainFlowConfig = tempMainFlowConfig;
                mainFlowHandler.SetMainFlowConfig(mainFlowConfig);
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            try
            {
                mainFlowConfig = mainFlowHandler.GetMainFlowConfig();
                ShowMainFlowConfigCvOnForm(mainFlowConfig);
                ShowMainFlowConfigSvOnForm(mainFlowConfig);
                middlerConfig = mainFlowHandler.GetMiddlerConfig();
                ShowMiddlerConfigCvOnForm(middlerConfig);
                ShowMiddlerConfigSvOnForm(middlerConfig);                    
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.SendToBack();
            this.Hide();
        }

        private void btnReconnect_Click(object sender, EventArgs e)
        {
            try
            {
                MiddleAgent middleAgent = mainFlowHandler.GetMiddleAgent();
                middleAgent.ReConnect();
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void UpdateMiddlerConfigCv()
        {
            try
            {
                middlerConfig = mainFlowHandler.GetMiddlerConfig();
                ShowMiddlerConfigCvOnForm(middlerConfig);
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void ShowMiddlerConfigCvOnForm(MiddlerConfig middlerConfig)
        {
            try
            {
                tbxClientNameCv.Text = middlerConfig.ClientName;
                tbxRemoteIpCv.Text = middlerConfig.RemoteIp;
                tbxRemotePortCv.Text = middlerConfig.RemotePort.ToString("F0");
                tbxLocalIpCv.Text = middlerConfig.LocalIp;
                tbxLocalPortCv.Text = middlerConfig.LocalPort.ToString("F0");
                tbxRetryCountCv.Text = middlerConfig.RetryCount.ToString("F0");
                tbxResrveLengthMeterCv.Text = middlerConfig.ReserveLengthMeter.ToString("F0");
                tbxAskReserveMsCv.Text = middlerConfig.AskReserveIntervalMs.ToString("F0");
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void ShowMiddlerConfigSvOnForm(MiddlerConfig middlerConfig)
        {
            try
            {
                tbxClientNameSv.Text = middlerConfig.ClientName;
                tbxRemoteIpSv.Text = middlerConfig.RemoteIp;
                tbxRemotePortSv.Text = middlerConfig.RemotePort.ToString("F0");
                tbxLocalIpSv.Text = middlerConfig.LocalIp;
                tbxLocalPortSv.Text = middlerConfig.LocalPort.ToString("F0");
                tbxRetryCountSv.Text = middlerConfig.RetryCount.ToString("F0");
                tbxResrveLengthMeterSv.Text = middlerConfig.ReserveLengthMeter.ToString("F0");
                tbxAskReserveMsSv.Text = middlerConfig.AskReserveIntervalMs.ToString("F0");
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void btnLoadMiddlerConfig_Click(object sender, EventArgs e)
        {
            try
            {
                mainFlowHandler.LoadMiddlerConfig();
                UpdateMiddlerConfigCv();
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void btnSaveMiddlerConfig_Click(object sender, EventArgs e)
        {
            try
            {
                MiddlerConfig tempMiddlerConfig = new MiddlerConfig();
                tempMiddlerConfig.ClientNum = middlerConfig.ClientNum;
                tempMiddlerConfig.BcrReadRetryIntervalMs = middlerConfig.BcrReadRetryIntervalMs;
                tempMiddlerConfig.BcrReadRetryTimeoutSec = middlerConfig.BcrReadRetryTimeoutSec;
                tempMiddlerConfig.MaxReadSize = middlerConfig.MaxReadSize;
                tempMiddlerConfig.MaxReconnectionCount = middlerConfig.MaxReconnectionCount;
                tempMiddlerConfig.NeerlyNoMoveRangeMm = middlerConfig.NeerlyNoMoveRangeMm;
                tempMiddlerConfig.ReconnectionIntervalMs = middlerConfig.ReconnectionIntervalMs;
                tempMiddlerConfig.RecvTimeoutMs = middlerConfig.RecvTimeoutMs;
                tempMiddlerConfig.RichTextBoxMaxLines = middlerConfig.RichTextBoxMaxLines;
                tempMiddlerConfig.SendTimeoutMs = middlerConfig.SendTimeoutMs;
                tempMiddlerConfig.SleepTime = middlerConfig.SleepTime;
                tempMiddlerConfig.ClientName = tbxClientNameSv.Text;
                tempMiddlerConfig.RemoteIp = tbxRemoteIpSv.Text;
                tempMiddlerConfig.RemotePort = int.Parse(tbxRemotePortSv.Text);
                tempMiddlerConfig.LocalIp = tbxLocalIpSv.Text;
                tempMiddlerConfig.LocalPort = int.Parse(tbxLocalPortSv.Text);
                tempMiddlerConfig.RetryCount = int.Parse(tbxRetryCountSv.Text);
                tempMiddlerConfig.ReserveLengthMeter = int.Parse(tbxResrveLengthMeterSv.Text);
                tempMiddlerConfig.AskReserveIntervalMs = int.Parse(tbxAskReserveMsSv.Text);

                middlerConfig = tempMiddlerConfig;
                mainFlowHandler.SetMiddlerConfig(middlerConfig);
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }
    }
}
