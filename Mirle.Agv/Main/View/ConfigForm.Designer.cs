namespace Mirle.Agv.View
{
    partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabConfigs = new System.Windows.Forms.TabControl();
            this.tbP_Mainflow = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnLoadMainFlowConfig = new System.Windows.Forms.Button();
            this.btnSaveMainFlowConfig = new System.Windows.Forms.Button();
            this.tbxVisitTransferStepsCv = new System.Windows.Forms.TextBox();
            this.tbxTrackPositionCv = new System.Windows.Forms.TextBox();
            this.tbxWatchLowPowerCv = new System.Windows.Forms.TextBox();
            this.tbxReportPositionCv = new System.Windows.Forms.TextBox();
            this.tbxStartChargeTimeoutCv = new System.Windows.Forms.TextBox();
            this.tbxStopChargeTimeoutCv = new System.Windows.Forms.TextBox();
            this.tbxPositionRangeCv = new System.Windows.Forms.TextBox();
            this.tbxVisitTransferStepsSv = new System.Windows.Forms.TextBox();
            this.tbxTrackPositionSv = new System.Windows.Forms.TextBox();
            this.tbxWatchLowPowerSv = new System.Windows.Forms.TextBox();
            this.tbxReportPositionSv = new System.Windows.Forms.TextBox();
            this.tbxStartChargeTimeoutSv = new System.Windows.Forms.TextBox();
            this.tbxStopChargeTimeoutSv = new System.Windows.Forms.TextBox();
            this.tbxPositionRangeSv = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbxLoadingChargeCv = new System.Windows.Forms.TextBox();
            this.tbxLoadingChargeSv = new System.Windows.Forms.TextBox();
            this.tbP_Middler = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnReconnect = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.btnLoadMiddlerConfig = new System.Windows.Forms.Button();
            this.btnSaveMiddlerConfig = new System.Windows.Forms.Button();
            this.tbxClientNameCv = new System.Windows.Forms.TextBox();
            this.tbxRemoteIpCv = new System.Windows.Forms.TextBox();
            this.tbxRemotePortCv = new System.Windows.Forms.TextBox();
            this.tbxLocalIpCv = new System.Windows.Forms.TextBox();
            this.tbxLocalPortCv = new System.Windows.Forms.TextBox();
            this.tbxRetryCountCv = new System.Windows.Forms.TextBox();
            this.tbxResrveLengthMeterCv = new System.Windows.Forms.TextBox();
            this.tbxClientNameSv = new System.Windows.Forms.TextBox();
            this.tbxRemoteIpSv = new System.Windows.Forms.TextBox();
            this.tbxRemotePortSv = new System.Windows.Forms.TextBox();
            this.tbxLocalIpSv = new System.Windows.Forms.TextBox();
            this.tbxLocalPortSv = new System.Windows.Forms.TextBox();
            this.tbxRetryCountSv = new System.Windows.Forms.TextBox();
            this.tbxResrveLengthMeterSv = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.tbxAskReserveMsCv = new System.Windows.Forms.TextBox();
            this.tbxAskReserveMsSv = new System.Windows.Forms.TextBox();
            this.timerUpdateConfigs = new System.Windows.Forms.Timer(this.components);
            this.btnHide = new System.Windows.Forms.Button();
            this.tabConfigs.SuspendLayout();
            this.tbP_Mainflow.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tbP_Middler.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabConfigs
            // 
            this.tabConfigs.Controls.Add(this.tbP_Mainflow);
            this.tabConfigs.Controls.Add(this.tbP_Middler);
            this.tabConfigs.Location = new System.Drawing.Point(22, 42);
            this.tabConfigs.Name = "tabConfigs";
            this.tabConfigs.SelectedIndex = 0;
            this.tabConfigs.Size = new System.Drawing.Size(661, 489);
            this.tabConfigs.TabIndex = 0;
            // 
            // tbP_Mainflow
            // 
            this.tbP_Mainflow.Controls.Add(this.tableLayoutPanel1);
            this.tbP_Mainflow.Location = new System.Drawing.Point(4, 28);
            this.tbP_Mainflow.Name = "tbP_Mainflow";
            this.tbP_Mainflow.Padding = new System.Windows.Forms.Padding(3);
            this.tbP_Mainflow.Size = new System.Drawing.Size(653, 457);
            this.tbP_Mainflow.TabIndex = 0;
            this.tbP_Mainflow.Text = "主流程";
            this.tbP_Mainflow.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.btnLoadMainFlowConfig, 1, 10);
            this.tableLayoutPanel1.Controls.Add(this.btnSaveMainFlowConfig, 2, 10);
            this.tableLayoutPanel1.Controls.Add(this.tbxVisitTransferStepsCv, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbxTrackPositionCv, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbxWatchLowPowerCv, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.tbxReportPositionCv, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.tbxStartChargeTimeoutCv, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.tbxStopChargeTimeoutCv, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.tbxPositionRangeCv, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.tbxVisitTransferStepsSv, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbxTrackPositionSv, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbxWatchLowPowerSv, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.tbxReportPositionSv, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.tbxStartChargeTimeoutSv, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.tbxStopChargeTimeoutSv, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.tbxPositionRangeSv, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.label10, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.tbxLoadingChargeCv, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.tbxLoadingChargeSv, 2, 8);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 11;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(641, 450);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(203, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(214, 40);
            this.label1.TabIndex = 0;
            this.label1.Text = "Current Value";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(194, 40);
            this.label3.TabIndex = 2;
            this.label3.Text = "Visit Transfer Steps Ms";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(423, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(215, 40);
            this.label2.TabIndex = 1;
            this.label2.Text = "Set Value";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(194, 40);
            this.label4.TabIndex = 3;
            this.label4.Text = "Track Position Ms";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(194, 40);
            this.label5.TabIndex = 4;
            this.label5.Text = "Watch LowPower Ms";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 160);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(194, 40);
            this.label6.TabIndex = 5;
            this.label6.Text = "Report Position Ms";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(3, 200);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(194, 40);
            this.label7.TabIndex = 6;
            this.label7.Text = "Start Charge Timeout Ms";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(3, 240);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(194, 40);
            this.label8.TabIndex = 7;
            this.label8.Text = "Stop Charge Timeout Ms";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(3, 280);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(194, 40);
            this.label9.TabIndex = 8;
            this.label9.Text = "Position Range Mm";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLoadMainFlowConfig
            // 
            this.btnLoadMainFlowConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadMainFlowConfig.Location = new System.Drawing.Point(203, 403);
            this.btnLoadMainFlowConfig.Name = "btnLoadMainFlowConfig";
            this.btnLoadMainFlowConfig.Size = new System.Drawing.Size(214, 44);
            this.btnLoadMainFlowConfig.TabIndex = 9;
            this.btnLoadMainFlowConfig.Text = "Load";
            this.btnLoadMainFlowConfig.UseVisualStyleBackColor = true;
            this.btnLoadMainFlowConfig.Click += new System.EventHandler(this.btnLoadMainFlowConfig_Click);
            // 
            // btnSaveMainFlowConfig
            // 
            this.btnSaveMainFlowConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveMainFlowConfig.Location = new System.Drawing.Point(423, 403);
            this.btnSaveMainFlowConfig.Name = "btnSaveMainFlowConfig";
            this.btnSaveMainFlowConfig.Size = new System.Drawing.Size(215, 44);
            this.btnSaveMainFlowConfig.TabIndex = 10;
            this.btnSaveMainFlowConfig.Text = "Save";
            this.btnSaveMainFlowConfig.UseVisualStyleBackColor = true;
            this.btnSaveMainFlowConfig.Click += new System.EventHandler(this.btnSaveMainFlowConfig_Click);
            // 
            // tbxVisitTransferStepsCv
            // 
            this.tbxVisitTransferStepsCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxVisitTransferStepsCv.Location = new System.Drawing.Point(203, 46);
            this.tbxVisitTransferStepsCv.Name = "tbxVisitTransferStepsCv";
            this.tbxVisitTransferStepsCv.ReadOnly = true;
            this.tbxVisitTransferStepsCv.Size = new System.Drawing.Size(214, 27);
            this.tbxVisitTransferStepsCv.TabIndex = 65;
            this.tbxVisitTransferStepsCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxTrackPositionCv
            // 
            this.tbxTrackPositionCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTrackPositionCv.Location = new System.Drawing.Point(203, 86);
            this.tbxTrackPositionCv.Name = "tbxTrackPositionCv";
            this.tbxTrackPositionCv.ReadOnly = true;
            this.tbxTrackPositionCv.Size = new System.Drawing.Size(214, 27);
            this.tbxTrackPositionCv.TabIndex = 66;
            this.tbxTrackPositionCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxWatchLowPowerCv
            // 
            this.tbxWatchLowPowerCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxWatchLowPowerCv.Location = new System.Drawing.Point(203, 126);
            this.tbxWatchLowPowerCv.Name = "tbxWatchLowPowerCv";
            this.tbxWatchLowPowerCv.ReadOnly = true;
            this.tbxWatchLowPowerCv.Size = new System.Drawing.Size(214, 27);
            this.tbxWatchLowPowerCv.TabIndex = 67;
            this.tbxWatchLowPowerCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxReportPositionCv
            // 
            this.tbxReportPositionCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxReportPositionCv.Location = new System.Drawing.Point(203, 166);
            this.tbxReportPositionCv.Name = "tbxReportPositionCv";
            this.tbxReportPositionCv.ReadOnly = true;
            this.tbxReportPositionCv.Size = new System.Drawing.Size(214, 27);
            this.tbxReportPositionCv.TabIndex = 68;
            this.tbxReportPositionCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxStartChargeTimeoutCv
            // 
            this.tbxStartChargeTimeoutCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxStartChargeTimeoutCv.Location = new System.Drawing.Point(203, 206);
            this.tbxStartChargeTimeoutCv.Name = "tbxStartChargeTimeoutCv";
            this.tbxStartChargeTimeoutCv.ReadOnly = true;
            this.tbxStartChargeTimeoutCv.Size = new System.Drawing.Size(214, 27);
            this.tbxStartChargeTimeoutCv.TabIndex = 69;
            this.tbxStartChargeTimeoutCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxStopChargeTimeoutCv
            // 
            this.tbxStopChargeTimeoutCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxStopChargeTimeoutCv.Location = new System.Drawing.Point(203, 246);
            this.tbxStopChargeTimeoutCv.Name = "tbxStopChargeTimeoutCv";
            this.tbxStopChargeTimeoutCv.ReadOnly = true;
            this.tbxStopChargeTimeoutCv.Size = new System.Drawing.Size(214, 27);
            this.tbxStopChargeTimeoutCv.TabIndex = 70;
            this.tbxStopChargeTimeoutCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxPositionRangeCv
            // 
            this.tbxPositionRangeCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxPositionRangeCv.Location = new System.Drawing.Point(203, 286);
            this.tbxPositionRangeCv.Name = "tbxPositionRangeCv";
            this.tbxPositionRangeCv.ReadOnly = true;
            this.tbxPositionRangeCv.Size = new System.Drawing.Size(214, 27);
            this.tbxPositionRangeCv.TabIndex = 71;
            this.tbxPositionRangeCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxVisitTransferStepsSv
            // 
            this.tbxVisitTransferStepsSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxVisitTransferStepsSv.Location = new System.Drawing.Point(423, 46);
            this.tbxVisitTransferStepsSv.Name = "tbxVisitTransferStepsSv";
            this.tbxVisitTransferStepsSv.Size = new System.Drawing.Size(215, 27);
            this.tbxVisitTransferStepsSv.TabIndex = 72;
            // 
            // tbxTrackPositionSv
            // 
            this.tbxTrackPositionSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTrackPositionSv.Location = new System.Drawing.Point(423, 86);
            this.tbxTrackPositionSv.Name = "tbxTrackPositionSv";
            this.tbxTrackPositionSv.Size = new System.Drawing.Size(215, 27);
            this.tbxTrackPositionSv.TabIndex = 73;
            // 
            // tbxWatchLowPowerSv
            // 
            this.tbxWatchLowPowerSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxWatchLowPowerSv.Location = new System.Drawing.Point(423, 126);
            this.tbxWatchLowPowerSv.Name = "tbxWatchLowPowerSv";
            this.tbxWatchLowPowerSv.Size = new System.Drawing.Size(215, 27);
            this.tbxWatchLowPowerSv.TabIndex = 74;
            // 
            // tbxReportPositionSv
            // 
            this.tbxReportPositionSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxReportPositionSv.Location = new System.Drawing.Point(423, 166);
            this.tbxReportPositionSv.Name = "tbxReportPositionSv";
            this.tbxReportPositionSv.Size = new System.Drawing.Size(215, 27);
            this.tbxReportPositionSv.TabIndex = 75;
            // 
            // tbxStartChargeTimeoutSv
            // 
            this.tbxStartChargeTimeoutSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxStartChargeTimeoutSv.Location = new System.Drawing.Point(423, 206);
            this.tbxStartChargeTimeoutSv.Name = "tbxStartChargeTimeoutSv";
            this.tbxStartChargeTimeoutSv.Size = new System.Drawing.Size(215, 27);
            this.tbxStartChargeTimeoutSv.TabIndex = 76;
            // 
            // tbxStopChargeTimeoutSv
            // 
            this.tbxStopChargeTimeoutSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxStopChargeTimeoutSv.Location = new System.Drawing.Point(423, 246);
            this.tbxStopChargeTimeoutSv.Name = "tbxStopChargeTimeoutSv";
            this.tbxStopChargeTimeoutSv.Size = new System.Drawing.Size(215, 27);
            this.tbxStopChargeTimeoutSv.TabIndex = 77;
            // 
            // tbxPositionRangeSv
            // 
            this.tbxPositionRangeSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxPositionRangeSv.Location = new System.Drawing.Point(423, 286);
            this.tbxPositionRangeSv.Name = "tbxPositionRangeSv";
            this.tbxPositionRangeSv.Size = new System.Drawing.Size(215, 27);
            this.tbxPositionRangeSv.TabIndex = 78;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(3, 320);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(194, 40);
            this.label10.TabIndex = 79;
            this.label10.Text = "Loading Charge Ms";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbxLoadingChargeCv
            // 
            this.tbxLoadingChargeCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxLoadingChargeCv.Location = new System.Drawing.Point(203, 326);
            this.tbxLoadingChargeCv.Name = "tbxLoadingChargeCv";
            this.tbxLoadingChargeCv.ReadOnly = true;
            this.tbxLoadingChargeCv.Size = new System.Drawing.Size(214, 27);
            this.tbxLoadingChargeCv.TabIndex = 80;
            this.tbxLoadingChargeCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxLoadingChargeSv
            // 
            this.tbxLoadingChargeSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxLoadingChargeSv.Location = new System.Drawing.Point(423, 326);
            this.tbxLoadingChargeSv.Name = "tbxLoadingChargeSv";
            this.tbxLoadingChargeSv.Size = new System.Drawing.Size(215, 27);
            this.tbxLoadingChargeSv.TabIndex = 81;
            // 
            // tbP_Middler
            // 
            this.tbP_Middler.Controls.Add(this.tableLayoutPanel2);
            this.tbP_Middler.Location = new System.Drawing.Point(4, 28);
            this.tbP_Middler.Name = "tbP_Middler";
            this.tbP_Middler.Padding = new System.Windows.Forms.Padding(3);
            this.tbP_Middler.Size = new System.Drawing.Size(653, 457);
            this.tbP_Middler.TabIndex = 1;
            this.tbP_Middler.Text = "通訊";
            this.tbP_Middler.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnReconnect, 0, 10);
            this.tableLayoutPanel2.Controls.Add(this.label11, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label12, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label13, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.label14, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label15, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.label16, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.label17, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.label18, 0, 6);
            this.tableLayoutPanel2.Controls.Add(this.label19, 0, 7);
            this.tableLayoutPanel2.Controls.Add(this.btnLoadMiddlerConfig, 1, 10);
            this.tableLayoutPanel2.Controls.Add(this.btnSaveMiddlerConfig, 2, 10);
            this.tableLayoutPanel2.Controls.Add(this.tbxClientNameCv, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.tbxRemoteIpCv, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.tbxRemotePortCv, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.tbxLocalIpCv, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.tbxLocalPortCv, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.tbxRetryCountCv, 1, 6);
            this.tableLayoutPanel2.Controls.Add(this.tbxResrveLengthMeterCv, 1, 7);
            this.tableLayoutPanel2.Controls.Add(this.tbxClientNameSv, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.tbxRemoteIpSv, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.tbxRemotePortSv, 2, 3);
            this.tableLayoutPanel2.Controls.Add(this.tbxLocalIpSv, 2, 4);
            this.tableLayoutPanel2.Controls.Add(this.tbxLocalPortSv, 2, 5);
            this.tableLayoutPanel2.Controls.Add(this.tbxRetryCountSv, 2, 6);
            this.tableLayoutPanel2.Controls.Add(this.tbxResrveLengthMeterSv, 2, 7);
            this.tableLayoutPanel2.Controls.Add(this.label20, 0, 8);
            this.tableLayoutPanel2.Controls.Add(this.tbxAskReserveMsCv, 1, 8);
            this.tableLayoutPanel2.Controls.Add(this.tbxAskReserveMsSv, 2, 8);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 11;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(641, 450);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // btnReconnect
            // 
            this.btnReconnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReconnect.Location = new System.Drawing.Point(3, 403);
            this.btnReconnect.Name = "btnReconnect";
            this.btnReconnect.Size = new System.Drawing.Size(194, 44);
            this.btnReconnect.TabIndex = 82;
            this.btnReconnect.Text = "Reconnect";
            this.btnReconnect.UseVisualStyleBackColor = true;
            this.btnReconnect.Click += new System.EventHandler(this.btnReconnect_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label11.Location = new System.Drawing.Point(203, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(214, 40);
            this.label11.TabIndex = 0;
            this.label11.Text = "Current Value";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label12.Location = new System.Drawing.Point(3, 40);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(194, 40);
            this.label12.TabIndex = 2;
            this.label12.Text = "Client Name";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Location = new System.Drawing.Point(423, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(215, 40);
            this.label13.TabIndex = 1;
            this.label13.Text = "Set Value";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(3, 80);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(194, 40);
            this.label14.TabIndex = 3;
            this.label14.Text = "Remote Ip";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Location = new System.Drawing.Point(3, 120);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(194, 40);
            this.label15.TabIndex = 4;
            this.label15.Text = "Remote Port";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(3, 160);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(194, 40);
            this.label16.TabIndex = 5;
            this.label16.Text = "Local Ip";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label17.Location = new System.Drawing.Point(3, 200);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(194, 40);
            this.label17.TabIndex = 6;
            this.label17.Text = "Local Port";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label18.Location = new System.Drawing.Point(3, 240);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(194, 40);
            this.label18.TabIndex = 7;
            this.label18.Text = "Retry Count";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label19.Location = new System.Drawing.Point(3, 280);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(194, 40);
            this.label19.TabIndex = 8;
            this.label19.Text = "Reserve Length Meter";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLoadMiddlerConfig
            // 
            this.btnLoadMiddlerConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadMiddlerConfig.Location = new System.Drawing.Point(203, 403);
            this.btnLoadMiddlerConfig.Name = "btnLoadMiddlerConfig";
            this.btnLoadMiddlerConfig.Size = new System.Drawing.Size(214, 44);
            this.btnLoadMiddlerConfig.TabIndex = 9;
            this.btnLoadMiddlerConfig.Text = "Load";
            this.btnLoadMiddlerConfig.UseVisualStyleBackColor = true;
            this.btnLoadMiddlerConfig.Click += new System.EventHandler(this.btnLoadMiddlerConfig_Click);
            // 
            // btnSaveMiddlerConfig
            // 
            this.btnSaveMiddlerConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveMiddlerConfig.Location = new System.Drawing.Point(423, 403);
            this.btnSaveMiddlerConfig.Name = "btnSaveMiddlerConfig";
            this.btnSaveMiddlerConfig.Size = new System.Drawing.Size(215, 44);
            this.btnSaveMiddlerConfig.TabIndex = 10;
            this.btnSaveMiddlerConfig.Text = "Save";
            this.btnSaveMiddlerConfig.UseVisualStyleBackColor = true;
            this.btnSaveMiddlerConfig.Click += new System.EventHandler(this.btnSaveMiddlerConfig_Click);
            // 
            // tbxClientNameCv
            // 
            this.tbxClientNameCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxClientNameCv.Location = new System.Drawing.Point(203, 46);
            this.tbxClientNameCv.Name = "tbxClientNameCv";
            this.tbxClientNameCv.ReadOnly = true;
            this.tbxClientNameCv.Size = new System.Drawing.Size(214, 27);
            this.tbxClientNameCv.TabIndex = 65;
            this.tbxClientNameCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxRemoteIpCv
            // 
            this.tbxRemoteIpCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxRemoteIpCv.Location = new System.Drawing.Point(203, 86);
            this.tbxRemoteIpCv.Name = "tbxRemoteIpCv";
            this.tbxRemoteIpCv.ReadOnly = true;
            this.tbxRemoteIpCv.Size = new System.Drawing.Size(214, 27);
            this.tbxRemoteIpCv.TabIndex = 66;
            this.tbxRemoteIpCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxRemotePortCv
            // 
            this.tbxRemotePortCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxRemotePortCv.Location = new System.Drawing.Point(203, 126);
            this.tbxRemotePortCv.Name = "tbxRemotePortCv";
            this.tbxRemotePortCv.ReadOnly = true;
            this.tbxRemotePortCv.Size = new System.Drawing.Size(214, 27);
            this.tbxRemotePortCv.TabIndex = 67;
            this.tbxRemotePortCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxLocalIpCv
            // 
            this.tbxLocalIpCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxLocalIpCv.Location = new System.Drawing.Point(203, 166);
            this.tbxLocalIpCv.Name = "tbxLocalIpCv";
            this.tbxLocalIpCv.ReadOnly = true;
            this.tbxLocalIpCv.Size = new System.Drawing.Size(214, 27);
            this.tbxLocalIpCv.TabIndex = 68;
            this.tbxLocalIpCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxLocalPortCv
            // 
            this.tbxLocalPortCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxLocalPortCv.Location = new System.Drawing.Point(203, 206);
            this.tbxLocalPortCv.Name = "tbxLocalPortCv";
            this.tbxLocalPortCv.ReadOnly = true;
            this.tbxLocalPortCv.Size = new System.Drawing.Size(214, 27);
            this.tbxLocalPortCv.TabIndex = 69;
            this.tbxLocalPortCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxRetryCountCv
            // 
            this.tbxRetryCountCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxRetryCountCv.Location = new System.Drawing.Point(203, 246);
            this.tbxRetryCountCv.Name = "tbxRetryCountCv";
            this.tbxRetryCountCv.ReadOnly = true;
            this.tbxRetryCountCv.Size = new System.Drawing.Size(214, 27);
            this.tbxRetryCountCv.TabIndex = 70;
            this.tbxRetryCountCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxResrveLengthMeterCv
            // 
            this.tbxResrveLengthMeterCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxResrveLengthMeterCv.Location = new System.Drawing.Point(203, 286);
            this.tbxResrveLengthMeterCv.Name = "tbxResrveLengthMeterCv";
            this.tbxResrveLengthMeterCv.ReadOnly = true;
            this.tbxResrveLengthMeterCv.Size = new System.Drawing.Size(214, 27);
            this.tbxResrveLengthMeterCv.TabIndex = 71;
            this.tbxResrveLengthMeterCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxClientNameSv
            // 
            this.tbxClientNameSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxClientNameSv.Location = new System.Drawing.Point(423, 46);
            this.tbxClientNameSv.Name = "tbxClientNameSv";
            this.tbxClientNameSv.Size = new System.Drawing.Size(215, 27);
            this.tbxClientNameSv.TabIndex = 72;
            // 
            // tbxRemoteIpSv
            // 
            this.tbxRemoteIpSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxRemoteIpSv.Location = new System.Drawing.Point(423, 86);
            this.tbxRemoteIpSv.Name = "tbxRemoteIpSv";
            this.tbxRemoteIpSv.Size = new System.Drawing.Size(215, 27);
            this.tbxRemoteIpSv.TabIndex = 73;
            // 
            // tbxRemotePortSv
            // 
            this.tbxRemotePortSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxRemotePortSv.Location = new System.Drawing.Point(423, 126);
            this.tbxRemotePortSv.Name = "tbxRemotePortSv";
            this.tbxRemotePortSv.Size = new System.Drawing.Size(215, 27);
            this.tbxRemotePortSv.TabIndex = 74;
            // 
            // tbxLocalIpSv
            // 
            this.tbxLocalIpSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxLocalIpSv.Location = new System.Drawing.Point(423, 166);
            this.tbxLocalIpSv.Name = "tbxLocalIpSv";
            this.tbxLocalIpSv.Size = new System.Drawing.Size(215, 27);
            this.tbxLocalIpSv.TabIndex = 75;
            // 
            // tbxLocalPortSv
            // 
            this.tbxLocalPortSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxLocalPortSv.Location = new System.Drawing.Point(423, 206);
            this.tbxLocalPortSv.Name = "tbxLocalPortSv";
            this.tbxLocalPortSv.Size = new System.Drawing.Size(215, 27);
            this.tbxLocalPortSv.TabIndex = 76;
            // 
            // tbxRetryCountSv
            // 
            this.tbxRetryCountSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxRetryCountSv.Location = new System.Drawing.Point(423, 246);
            this.tbxRetryCountSv.Name = "tbxRetryCountSv";
            this.tbxRetryCountSv.Size = new System.Drawing.Size(215, 27);
            this.tbxRetryCountSv.TabIndex = 77;
            // 
            // tbxResrveLengthMeterSv
            // 
            this.tbxResrveLengthMeterSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxResrveLengthMeterSv.Location = new System.Drawing.Point(423, 286);
            this.tbxResrveLengthMeterSv.Name = "tbxResrveLengthMeterSv";
            this.tbxResrveLengthMeterSv.Size = new System.Drawing.Size(215, 27);
            this.tbxResrveLengthMeterSv.TabIndex = 78;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label20.Location = new System.Drawing.Point(3, 320);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(194, 40);
            this.label20.TabIndex = 79;
            this.label20.Text = "Ask Reserve Ms";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbxAskReserveMsCv
            // 
            this.tbxAskReserveMsCv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxAskReserveMsCv.Location = new System.Drawing.Point(203, 326);
            this.tbxAskReserveMsCv.Name = "tbxAskReserveMsCv";
            this.tbxAskReserveMsCv.ReadOnly = true;
            this.tbxAskReserveMsCv.Size = new System.Drawing.Size(214, 27);
            this.tbxAskReserveMsCv.TabIndex = 80;
            this.tbxAskReserveMsCv.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbxAskReserveMsSv
            // 
            this.tbxAskReserveMsSv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxAskReserveMsSv.Location = new System.Drawing.Point(423, 326);
            this.tbxAskReserveMsSv.Name = "tbxAskReserveMsSv";
            this.tbxAskReserveMsSv.Size = new System.Drawing.Size(215, 27);
            this.tbxAskReserveMsSv.TabIndex = 81;
            // 
            // timerUpdateConfigs
            // 
            this.timerUpdateConfigs.Enabled = true;
            this.timerUpdateConfigs.Interval = 200;
            this.timerUpdateConfigs.Tick += new System.EventHandler(this.timerUpdateConfigs_Tick);
            // 
            // btnHide
            // 
            this.btnHide.ForeColor = System.Drawing.Color.OrangeRed;
            this.btnHide.Location = new System.Drawing.Point(628, 12);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(75, 39);
            this.btnHide.TabIndex = 1;
            this.btnHide.Text = "X";
            this.btnHide.UseVisualStyleBackColor = true;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 571);
            this.ControlBox = false;
            this.Controls.Add(this.btnHide);
            this.Controls.Add(this.tabConfigs);
            this.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ConfigForm";
            this.Text = "參數設定";
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.tabConfigs.ResumeLayout(false);
            this.tbP_Mainflow.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tbP_Middler.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabConfigs;
        private System.Windows.Forms.TabPage tbP_Mainflow;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnLoadMainFlowConfig;
        private System.Windows.Forms.Button btnSaveMainFlowConfig;
        private System.Windows.Forms.TabPage tbP_Middler;
        private System.Windows.Forms.TextBox tbxVisitTransferStepsCv;
        private System.Windows.Forms.TextBox tbxTrackPositionCv;
        private System.Windows.Forms.TextBox tbxWatchLowPowerCv;
        private System.Windows.Forms.TextBox tbxReportPositionCv;
        private System.Windows.Forms.TextBox tbxStartChargeTimeoutCv;
        private System.Windows.Forms.TextBox tbxStopChargeTimeoutCv;
        private System.Windows.Forms.TextBox tbxPositionRangeCv;
        private System.Windows.Forms.Timer timerUpdateConfigs;
        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.TextBox tbxVisitTransferStepsSv;
        private System.Windows.Forms.TextBox tbxTrackPositionSv;
        private System.Windows.Forms.TextBox tbxWatchLowPowerSv;
        private System.Windows.Forms.TextBox tbxReportPositionSv;
        private System.Windows.Forms.TextBox tbxStartChargeTimeoutSv;
        private System.Windows.Forms.TextBox tbxStopChargeTimeoutSv;
        private System.Windows.Forms.TextBox tbxPositionRangeSv;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbxLoadingChargeCv;
        private System.Windows.Forms.TextBox tbxLoadingChargeSv;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnReconnect;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button btnLoadMiddlerConfig;
        private System.Windows.Forms.Button btnSaveMiddlerConfig;
        private System.Windows.Forms.TextBox tbxClientNameCv;
        private System.Windows.Forms.TextBox tbxRemoteIpCv;
        private System.Windows.Forms.TextBox tbxRemotePortCv;
        private System.Windows.Forms.TextBox tbxLocalIpCv;
        private System.Windows.Forms.TextBox tbxLocalPortCv;
        private System.Windows.Forms.TextBox tbxRetryCountCv;
        private System.Windows.Forms.TextBox tbxResrveLengthMeterCv;
        private System.Windows.Forms.TextBox tbxClientNameSv;
        private System.Windows.Forms.TextBox tbxRemoteIpSv;
        private System.Windows.Forms.TextBox tbxRemotePortSv;
        private System.Windows.Forms.TextBox tbxLocalIpSv;
        private System.Windows.Forms.TextBox tbxLocalPortSv;
        private System.Windows.Forms.TextBox tbxRetryCountSv;
        private System.Windows.Forms.TextBox tbxResrveLengthMeterSv;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox tbxAskReserveMsCv;
        private System.Windows.Forms.TextBox tbxAskReserveMsSv;
    }
}