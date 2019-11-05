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
                default:
                    break;
            }
        }

        private void UpdateMainFlowConfigCv()
        {
            mainFlowConfig = mainFlowHandler.GetMainFlowConfig();
            ShowMainFlowConfigCvOnForm(mainFlowConfig);
        }

        private void ShowMainFlowConfigCvOnForm(MainFlowConfig mainFlowConfig)
        {
            tbxVisitTransferStepsCv.Text = mainFlowConfig.VisitTransferStepsSleepTimeMs.ToString("F0");
            tbxTrackPositionCv.Text = mainFlowConfig.TrackPositionSleepTimeMs.ToString("F0");
            tbxWatchLowPowerCv.Text = mainFlowConfig.WatchLowPowerSleepTimeMs.ToString("F0");
            tbxReportPositionCv.Text = mainFlowConfig.ReportPositionIntervalMs.ToString("F0");
            tbxStartChargeTimeoutCv.Text = mainFlowConfig.StartChargeWaitingTimeoutMs.ToString("F0");
            tbxStopChargeTimeoutCv.Text = mainFlowConfig.StopChargeWaitingTimeoutMs.ToString("F0");
            tbxPositionRangeCv.Text = mainFlowConfig.RealPositionRangeMm.ToString("F0");
        }

        private void ShowMainFlowConfigSvOnForm(MainFlowConfig mainFlowConfig)
        {
            tbxVisitTransferStepsSv.Text = mainFlowConfig.VisitTransferStepsSleepTimeMs.ToString("F0");
            tbxTrackPositionSv.Text = mainFlowConfig.TrackPositionSleepTimeMs.ToString("F0");
            tbxWatchLowPowerSv.Text = mainFlowConfig.WatchLowPowerSleepTimeMs.ToString("F0");
            tbxReportPositionSv.Text = mainFlowConfig.ReportPositionIntervalMs.ToString("F0");
            tbxStartChargeTimeoutSv.Text = mainFlowConfig.StartChargeWaitingTimeoutMs.ToString("F0");
            tbxStopChargeTimeoutSv.Text = mainFlowConfig.StopChargeWaitingTimeoutMs.ToString("F0");
            tbxPositionRangeSv.Text = mainFlowConfig.RealPositionRangeMm.ToString("F0");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            mainFlowHandler.LoadMainFlowConfig();
            UpdateMainFlowConfigCv();
        }

        private void btnSave_Click(object sender, EventArgs e)
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
            mainFlowConfig = mainFlowHandler.GetMainFlowConfig();
            ShowMainFlowConfigCvOnForm(mainFlowConfig);
            ShowMainFlowConfigSvOnForm(mainFlowConfig);
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.SendToBack();
            this.Hide();
        }
    }
}
