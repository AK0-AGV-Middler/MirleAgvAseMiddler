using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.AgvAseMiddler.Controller;
using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.TransferSteps;
using Mirle.AgvAseMiddler;
 
using System.Reflection;
using Mirle.AgvAseMiddler.Model.Configs;
using Mirle.Tools;

namespace Mirle.AgvAseMiddler.View
{
    public partial class ConfigForm : Form
    {
        private MainFlowHandler mainFlowHandler;
        private MainFlowConfig mainFlowConfig;
        private AgvcConnectorConfig agvcConnectorConfig;

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
                    UpdateAgvcConnectorConfigCv();
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            try
            {
                mainFlowConfig = mainFlowHandler.GetMainFlowConfig();
                ShowMainFlowConfigCvOnForm(mainFlowConfig);
                ShowMainFlowConfigSvOnForm(mainFlowConfig);
                agvcConnectorConfig = mainFlowHandler.GetAgvcConnectorConfig();
                ShowAgvcConnectorConfigCvOnForm(agvcConnectorConfig);
                ShowAgvcConnectorConfigSvOnForm(agvcConnectorConfig);                    
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                AgvcConnector agvcConnector = mainFlowHandler.GetAgvcConnector();
                agvcConnector.ReConnect();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void UpdateAgvcConnectorConfigCv()
        {
            try
            {
                agvcConnectorConfig = mainFlowHandler.GetAgvcConnectorConfig();
                ShowAgvcConnectorConfigCvOnForm(agvcConnectorConfig);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void ShowAgvcConnectorConfigCvOnForm(AgvcConnectorConfig agvcConnectorConfig)
        {
            try
            {
                tbxClientNameCv.Text = agvcConnectorConfig.ClientName;
                tbxRemoteIpCv.Text = agvcConnectorConfig.RemoteIp;
                tbxRemotePortCv.Text = agvcConnectorConfig.RemotePort.ToString("F0");
                tbxLocalIpCv.Text = agvcConnectorConfig.LocalIp;
                tbxLocalPortCv.Text = agvcConnectorConfig.LocalPort.ToString("F0");
                tbxRetryCountCv.Text = agvcConnectorConfig.RetryCount.ToString("F0");
                tbxResrveLengthMeterCv.Text = agvcConnectorConfig.ReserveLengthMeter.ToString("F0");
                tbxAskReserveMsCv.Text = agvcConnectorConfig.AskReserveIntervalMs.ToString("F0");
                tbxRecvTimeoutMsCv.Text = agvcConnectorConfig.RecvTimeoutMs.ToString("F0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void ShowAgvcConnectorConfigSvOnForm(AgvcConnectorConfig agvcConnectorConfig)
        {
            try
            {
                tbxClientNameSv.Text = agvcConnectorConfig.ClientName;
                tbxRemoteIpSv.Text = agvcConnectorConfig.RemoteIp;
                tbxRemotePortSv.Text = agvcConnectorConfig.RemotePort.ToString("F0");
                tbxLocalIpSv.Text = agvcConnectorConfig.LocalIp;
                tbxLocalPortSv.Text = agvcConnectorConfig.LocalPort.ToString("F0");
                tbxRetryCountSv.Text = agvcConnectorConfig.RetryCount.ToString("F0");
                tbxResrveLengthMeterSv.Text = agvcConnectorConfig.ReserveLengthMeter.ToString("F0");
                tbxAskReserveMsSv.Text = agvcConnectorConfig.AskReserveIntervalMs.ToString("F0");
                tbxRecvTimeoutMsSv.Text = agvcConnectorConfig.RecvTimeoutMs.ToString("F0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void btnLoadAgvcConnectorConfig_Click(object sender, EventArgs e)
        {
            try
            {
                mainFlowHandler.LoadAgvcConnectorConfig();
                UpdateAgvcConnectorConfigCv();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void btnSaveAgvcConnectorConfig_Click(object sender, EventArgs e)
        {
            try
            {
                AgvcConnectorConfig tempAgvcConnectorConfig = new AgvcConnectorConfig();
                tempAgvcConnectorConfig.ClientNum = agvcConnectorConfig.ClientNum;
                tempAgvcConnectorConfig.BcrReadRetryIntervalMs = agvcConnectorConfig.BcrReadRetryIntervalMs;
                tempAgvcConnectorConfig.BcrReadRetryTimeoutSec = agvcConnectorConfig.BcrReadRetryTimeoutSec;
                tempAgvcConnectorConfig.MaxReadSize = agvcConnectorConfig.MaxReadSize;
                tempAgvcConnectorConfig.MaxReconnectionCount = agvcConnectorConfig.MaxReconnectionCount;
                tempAgvcConnectorConfig.NeerlyNoMoveRangeMm = agvcConnectorConfig.NeerlyNoMoveRangeMm;
                tempAgvcConnectorConfig.ReconnectionIntervalMs = agvcConnectorConfig.ReconnectionIntervalMs;
                tempAgvcConnectorConfig.RecvTimeoutMs = int.Parse(tbxRecvTimeoutMsSv.Text);
                tempAgvcConnectorConfig.RichTextBoxMaxLines = agvcConnectorConfig.RichTextBoxMaxLines;
                tempAgvcConnectorConfig.SendTimeoutMs = agvcConnectorConfig.SendTimeoutMs;
                tempAgvcConnectorConfig.SleepTime = agvcConnectorConfig.SleepTime;
                tempAgvcConnectorConfig.ClientName = tbxClientNameSv.Text;
                tempAgvcConnectorConfig.RemoteIp = tbxRemoteIpSv.Text;
                tempAgvcConnectorConfig.RemotePort = int.Parse(tbxRemotePortSv.Text);
                tempAgvcConnectorConfig.LocalIp = tbxLocalIpSv.Text;
                tempAgvcConnectorConfig.LocalPort = int.Parse(tbxLocalPortSv.Text);
                tempAgvcConnectorConfig.RetryCount = int.Parse(tbxRetryCountSv.Text);
                tempAgvcConnectorConfig.ReserveLengthMeter = int.Parse(tbxResrveLengthMeterSv.Text);
                tempAgvcConnectorConfig.AskReserveIntervalMs = int.Parse(tbxAskReserveMsSv.Text);                

                agvcConnectorConfig = tempAgvcConnectorConfig;
                mainFlowHandler.SetAgvcConnectorConfig(agvcConnectorConfig);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LogException(string source,string exMsg)
        {
            MirleLogger.Instance.Log(new LogFormat("Error", "5", source, "Device", "CarrierID", exMsg));

        }
    }
}
