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
            this.tbP_Middler = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tbxVisitTransferStepsCv = new System.Windows.Forms.TextBox();
            this.tbxTrackPositionCv = new System.Windows.Forms.TextBox();
            this.tbxWatchLowPowerCv = new System.Windows.Forms.TextBox();
            this.tbxReportPositionCv = new System.Windows.Forms.TextBox();
            this.tbxStartChargeTimeoutCv = new System.Windows.Forms.TextBox();
            this.tbxStopChargeTimeoutCv = new System.Windows.Forms.TextBox();
            this.tbxPositionRangeCv = new System.Windows.Forms.TextBox();
            this.timerUpdateConfigs = new System.Windows.Forms.Timer(this.components);
            this.btnHide = new System.Windows.Forms.Button();
            this.tbxVisitTransferStepsSv = new System.Windows.Forms.TextBox();
            this.tbxTrackPositionSv = new System.Windows.Forms.TextBox();
            this.tbxWatchLowPowerSv = new System.Windows.Forms.TextBox();
            this.tbxReportPositionSv = new System.Windows.Forms.TextBox();
            this.tbxStartChargeTimeoutSv = new System.Windows.Forms.TextBox();
            this.tbxStopChargeTimeoutSv = new System.Windows.Forms.TextBox();
            this.tbxPositionRangeSv = new System.Windows.Forms.TextBox();
            this.tabConfigs.SuspendLayout();
            this.tbP_Mainflow.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabConfigs
            // 
            this.tabConfigs.Controls.Add(this.tbP_Mainflow);
            this.tabConfigs.Controls.Add(this.tbP_Middler);
            this.tabConfigs.Location = new System.Drawing.Point(22, 42);
            this.tabConfigs.Name = "tabConfigs";
            this.tabConfigs.SelectedIndex = 0;
            this.tabConfigs.Size = new System.Drawing.Size(661, 466);
            this.tabConfigs.TabIndex = 0;
            // 
            // tbP_Mainflow
            // 
            this.tbP_Mainflow.Controls.Add(this.tableLayoutPanel1);
            this.tbP_Mainflow.Location = new System.Drawing.Point(4, 28);
            this.tbP_Mainflow.Name = "tbP_Mainflow";
            this.tbP_Mainflow.Padding = new System.Windows.Forms.Padding(3);
            this.tbP_Mainflow.Size = new System.Drawing.Size(653, 434);
            this.tbP_Mainflow.TabIndex = 0;
            this.tbP_Mainflow.Text = "主流程";
            this.tbP_Mainflow.UseVisualStyleBackColor = true;
            // 
            // tbP_Middler
            // 
            this.tbP_Middler.Location = new System.Drawing.Point(4, 28);
            this.tbP_Middler.Name = "tbP_Middler";
            this.tbP_Middler.Padding = new System.Windows.Forms.Padding(3);
            this.tbP_Middler.Size = new System.Drawing.Size(653, 434);
            this.tbP_Middler.TabIndex = 1;
            this.tbP_Middler.Text = "通訊";
            this.tbP_Middler.UseVisualStyleBackColor = true;
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
            this.tableLayoutPanel1.Controls.Add(this.btnLoad, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.btnSave, 2, 9);
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(641, 421);
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(194, 40);
            this.label3.TabIndex = 2;
            this.label3.Text = "Visit Transfer Steps";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(194, 40);
            this.label4.TabIndex = 3;
            this.label4.Text = "Track Position";
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
            this.label5.Text = "Watch LowPower";
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
            this.label6.Text = "Report Position";
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
            this.label7.Text = "Start Charge Timeout";
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
            this.label8.Text = "Stop Charge Timeout";
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
            this.label9.Text = "Position Range";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLoad
            // 
            this.btnLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoad.Location = new System.Drawing.Point(203, 363);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(214, 55);
            this.btnLoad.TabIndex = 9;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSave.Location = new System.Drawing.Point(423, 363);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(215, 55);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
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
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 528);
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
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
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
    }
}