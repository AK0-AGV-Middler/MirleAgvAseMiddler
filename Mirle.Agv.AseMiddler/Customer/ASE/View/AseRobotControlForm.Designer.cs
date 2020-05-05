﻿namespace Mirle.Agv.AseMiddler.View
{
    partial class AseRobotControlForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pageRobotCommnad = new System.Windows.Forms.TabPage();
            this.btnRefreshRobotState = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.boxPioDirection = new System.Windows.Forms.ComboBox();
            this.txtToPort = new System.Windows.Forms.TextBox();
            this.txtPortNumber = new System.Windows.Forms.TextBox();
            this.txtGateType = new System.Windows.Forms.TextBox();
            this.txtFromPort = new System.Windows.Forms.TextBox();
            this.cbIsLoad = new System.Windows.Forms.CheckBox();
            this.btnSendRobot = new System.Windows.Forms.Button();
            this.pageChargeCommand = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnRefreshBatterySate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSearchChargeAddress = new System.Windows.Forms.Button();
            this.btnStopCharge = new System.Windows.Forms.Button();
            this.btnStartCharge = new System.Windows.Forms.Button();
            this.boxChargeDirection = new System.Windows.Forms.ComboBox();
            this.txtChargeAddress = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.ucRobotIsHome = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotLState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotLId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotRobotState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryCharging = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryTemperature = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryVoltage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryPercentage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.tabControl1.SuspendLayout();
            this.pageRobotCommnad.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.pageChargeCommand.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pageRobotCommnad);
            this.tabControl1.Controls.Add(this.pageChargeCommand);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1182, 211);
            this.tabControl1.TabIndex = 0;
            // 
            // pageRobotCommnad
            // 
            this.pageRobotCommnad.Controls.Add(this.btnRefreshRobotState);
            this.pageRobotCommnad.Controls.Add(this.ucRobotIsHome);
            this.pageRobotCommnad.Controls.Add(this.ucRobotSlotRId);
            this.pageRobotCommnad.Controls.Add(this.ucRobotSlotRState);
            this.pageRobotCommnad.Controls.Add(this.ucRobotSlotLState);
            this.pageRobotCommnad.Controls.Add(this.ucRobotSlotLId);
            this.pageRobotCommnad.Controls.Add(this.ucRobotRobotState);
            this.pageRobotCommnad.Controls.Add(this.groupBox4);
            this.pageRobotCommnad.Controls.Add(this.btnSendRobot);
            this.pageRobotCommnad.Location = new System.Drawing.Point(4, 22);
            this.pageRobotCommnad.Name = "pageRobotCommnad";
            this.pageRobotCommnad.Padding = new System.Windows.Forms.Padding(3);
            this.pageRobotCommnad.Size = new System.Drawing.Size(1174, 185);
            this.pageRobotCommnad.TabIndex = 0;
            this.pageRobotCommnad.Text = "RobotCommnad";
            this.pageRobotCommnad.UseVisualStyleBackColor = true;
            // 
            // btnRefreshRobotState
            // 
            this.btnRefreshRobotState.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshRobotState.Location = new System.Drawing.Point(699, 70);
            this.btnRefreshRobotState.Name = "btnRefreshRobotState";
            this.btnRefreshRobotState.Size = new System.Drawing.Size(156, 78);
            this.btnRefreshRobotState.TabIndex = 50;
            this.btnRefreshRobotState.Text = "更新手臂.儲位狀態";
            this.btnRefreshRobotState.UseVisualStyleBackColor = true;
            this.btnRefreshRobotState.Click += new System.EventHandler(this.btnRefreshRobotState_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.boxPioDirection);
            this.groupBox4.Controls.Add(this.txtToPort);
            this.groupBox4.Controls.Add(this.txtPortNumber);
            this.groupBox4.Controls.Add(this.txtGateType);
            this.groupBox4.Controls.Add(this.txtFromPort);
            this.groupBox4.Controls.Add(this.cbIsLoad);
            this.groupBox4.Location = new System.Drawing.Point(6, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(264, 124);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Robot(P45)";
            // 
            // boxPioDirection
            // 
            this.boxPioDirection.FormattingEnabled = true;
            this.boxPioDirection.Location = new System.Drawing.Point(6, 21);
            this.boxPioDirection.Name = "boxPioDirection";
            this.boxPioDirection.Size = new System.Drawing.Size(117, 20);
            this.boxPioDirection.TabIndex = 10;
            // 
            // txtToPort
            // 
            this.txtToPort.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtToPort.Location = new System.Drawing.Point(130, 47);
            this.txtToPort.Name = "txtToPort";
            this.txtToPort.Size = new System.Drawing.Size(117, 22);
            this.txtToPort.TabIndex = 9;
            this.txtToPort.Text = "To";
            // 
            // txtPortNumber
            // 
            this.txtPortNumber.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtPortNumber.Location = new System.Drawing.Point(129, 75);
            this.txtPortNumber.Name = "txtPortNumber";
            this.txtPortNumber.Size = new System.Drawing.Size(117, 22);
            this.txtPortNumber.TabIndex = 8;
            this.txtPortNumber.Text = "1";
            // 
            // txtGateType
            // 
            this.txtGateType.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtGateType.Location = new System.Drawing.Point(6, 75);
            this.txtGateType.Name = "txtGateType";
            this.txtGateType.Size = new System.Drawing.Size(117, 22);
            this.txtGateType.TabIndex = 8;
            this.txtGateType.Text = "0";
            // 
            // txtFromPort
            // 
            this.txtFromPort.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtFromPort.Location = new System.Drawing.Point(6, 47);
            this.txtFromPort.Name = "txtFromPort";
            this.txtFromPort.Size = new System.Drawing.Size(117, 22);
            this.txtFromPort.TabIndex = 8;
            this.txtFromPort.Text = "From";
            // 
            // cbIsLoad
            // 
            this.cbIsLoad.AutoSize = true;
            this.cbIsLoad.Location = new System.Drawing.Point(173, 21);
            this.cbIsLoad.Name = "cbIsLoad";
            this.cbIsLoad.Size = new System.Drawing.Size(67, 16);
            this.cbIsLoad.TabIndex = 0;
            this.cbIsLoad.Text = "Is Load ?";
            this.cbIsLoad.UseVisualStyleBackColor = true;
            // 
            // btnSendRobot
            // 
            this.btnSendRobot.Location = new System.Drawing.Point(17, 136);
            this.btnSendRobot.Name = "btnSendRobot";
            this.btnSendRobot.Size = new System.Drawing.Size(253, 33);
            this.btnSendRobot.TabIndex = 5;
            this.btnSendRobot.Text = "Send";
            this.btnSendRobot.UseVisualStyleBackColor = true;
            this.btnSendRobot.Click += new System.EventHandler(this.btnSendRobot_Click);
            // 
            // pageChargeCommand
            // 
            this.pageChargeCommand.Controls.Add(this.groupBox2);
            this.pageChargeCommand.Controls.Add(this.groupBox1);
            this.pageChargeCommand.Location = new System.Drawing.Point(4, 22);
            this.pageChargeCommand.Name = "pageChargeCommand";
            this.pageChargeCommand.Size = new System.Drawing.Size(1174, 185);
            this.pageChargeCommand.TabIndex = 1;
            this.pageChargeCommand.Text = "ChargeCommand";
            this.pageChargeCommand.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ucBatteryCharging);
            this.groupBox2.Controls.Add(this.ucBatteryTemperature);
            this.groupBox2.Controls.Add(this.btnRefreshBatterySate);
            this.groupBox2.Controls.Add(this.ucBatteryVoltage);
            this.groupBox2.Controls.Add(this.ucBatteryPercentage);
            this.groupBox2.Location = new System.Drawing.Point(294, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(718, 179);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Info";
            // 
            // btnRefreshBatterySate
            // 
            this.btnRefreshBatterySate.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshBatterySate.Location = new System.Drawing.Point(462, 131);
            this.btnRefreshBatterySate.Name = "btnRefreshBatterySate";
            this.btnRefreshBatterySate.Size = new System.Drawing.Size(200, 42);
            this.btnRefreshBatterySate.TabIndex = 3;
            this.btnRefreshBatterySate.Text = "Refresh State";
            this.btnRefreshBatterySate.UseVisualStyleBackColor = true;
            this.btnRefreshBatterySate.Click += new System.EventHandler(this.btnRefreshBatterySate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSearchChargeAddress);
            this.groupBox1.Controls.Add(this.btnStopCharge);
            this.groupBox1.Controls.Add(this.btnStartCharge);
            this.groupBox1.Controls.Add(this.boxChargeDirection);
            this.groupBox1.Controls.Add(this.txtChargeAddress);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(285, 179);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Charge(P47)";
            // 
            // btnSearchChargeAddress
            // 
            this.btnSearchChargeAddress.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSearchChargeAddress.Location = new System.Drawing.Point(6, 53);
            this.btnSearchChargeAddress.Name = "btnSearchChargeAddress";
            this.btnSearchChargeAddress.Size = new System.Drawing.Size(273, 34);
            this.btnSearchChargeAddress.TabIndex = 4;
            this.btnSearchChargeAddress.Text = "Search Address";
            this.btnSearchChargeAddress.UseVisualStyleBackColor = true;
            this.btnSearchChargeAddress.Click += new System.EventHandler(this.btnSearchChargeAddress_Click);
            // 
            // btnStopCharge
            // 
            this.btnStopCharge.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStopCharge.Location = new System.Drawing.Point(140, 131);
            this.btnStopCharge.Name = "btnStopCharge";
            this.btnStopCharge.Size = new System.Drawing.Size(139, 42);
            this.btnStopCharge.TabIndex = 3;
            this.btnStopCharge.Text = "DisCharge";
            this.btnStopCharge.UseVisualStyleBackColor = true;
            // 
            // btnStartCharge
            // 
            this.btnStartCharge.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStartCharge.Location = new System.Drawing.Point(6, 131);
            this.btnStartCharge.Name = "btnStartCharge";
            this.btnStartCharge.Size = new System.Drawing.Size(128, 42);
            this.btnStartCharge.TabIndex = 3;
            this.btnStartCharge.Text = "Charge";
            this.btnStartCharge.UseVisualStyleBackColor = true;
            this.btnStartCharge.Click += new System.EventHandler(this.btnStartCharge_Click);
            // 
            // boxChargeDirection
            // 
            this.boxChargeDirection.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxChargeDirection.FormattingEnabled = true;
            this.boxChargeDirection.Location = new System.Drawing.Point(6, 93);
            this.boxChargeDirection.Name = "boxChargeDirection";
            this.boxChargeDirection.Size = new System.Drawing.Size(273, 32);
            this.boxChargeDirection.TabIndex = 1;
            // 
            // txtChargeAddress
            // 
            this.txtChargeAddress.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtChargeAddress.Location = new System.Drawing.Point(6, 21);
            this.txtChargeAddress.Name = "txtChargeAddress";
            this.txtChargeAddress.Size = new System.Drawing.Size(273, 27);
            this.txtChargeAddress.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox1.Location = new System.Drawing.Point(12, 229);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(1182, 481);
            this.textBox1.TabIndex = 4;
            // 
            // ucRobotIsHome
            // 
            this.ucRobotIsHome.Location = new System.Drawing.Point(276, 83);
            this.ucRobotIsHome.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotIsHome.Name = "ucRobotIsHome";
            this.ucRobotIsHome.Size = new System.Drawing.Size(135, 65);
            this.ucRobotIsHome.TabIndex = 44;
            this.ucRobotIsHome.TagColor = System.Drawing.Color.Black;
            this.ucRobotIsHome.TagName = "Is Home";
            this.ucRobotIsHome.TagValue = "false";
            // 
            // ucRobotSlotRId
            // 
            this.ucRobotSlotRId.Location = new System.Drawing.Point(558, 83);
            this.ucRobotSlotRId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotRId.Name = "ucRobotSlotRId";
            this.ucRobotSlotRId.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotRId.TabIndex = 45;
            this.ucRobotSlotRId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRId.TagName = "Slot R Id";
            this.ucRobotSlotRId.TagValue = "PQR";
            // 
            // ucRobotSlotRState
            // 
            this.ucRobotSlotRState.Location = new System.Drawing.Point(558, 10);
            this.ucRobotSlotRState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotRState.Name = "ucRobotSlotRState";
            this.ucRobotSlotRState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotRState.TabIndex = 46;
            this.ucRobotSlotRState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRState.TagName = "Slot R State";
            this.ucRobotSlotRState.TagValue = "Empty";
            // 
            // ucRobotSlotLState
            // 
            this.ucRobotSlotLState.Location = new System.Drawing.Point(417, 10);
            this.ucRobotSlotLState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotLState.Name = "ucRobotSlotLState";
            this.ucRobotSlotLState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotLState.TabIndex = 47;
            this.ucRobotSlotLState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLState.TagName = "Slot L State";
            this.ucRobotSlotLState.TagValue = "Empty";
            // 
            // ucRobotSlotLId
            // 
            this.ucRobotSlotLId.Location = new System.Drawing.Point(417, 83);
            this.ucRobotSlotLId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotLId.Name = "ucRobotSlotLId";
            this.ucRobotSlotLId.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotLId.TabIndex = 48;
            this.ucRobotSlotLId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLId.TagName = "Slot L Id";
            this.ucRobotSlotLId.TagValue = "ABC";
            // 
            // ucRobotRobotState
            // 
            this.ucRobotRobotState.Location = new System.Drawing.Point(276, 10);
            this.ucRobotRobotState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotRobotState.Name = "ucRobotRobotState";
            this.ucRobotRobotState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotRobotState.TabIndex = 49;
            this.ucRobotRobotState.TagColor = System.Drawing.Color.Black;
            this.ucRobotRobotState.TagName = "Robot State";
            this.ucRobotRobotState.TagValue = "Idle";
            // 
            // ucBatteryCharging
            // 
            this.ucBatteryCharging.Location = new System.Drawing.Point(462, 21);
            this.ucBatteryCharging.Name = "ucBatteryCharging";
            this.ucBatteryCharging.Size = new System.Drawing.Size(200, 59);
            this.ucBatteryCharging.TabIndex = 2;
            this.ucBatteryCharging.TagColor = System.Drawing.Color.Black;
            this.ucBatteryCharging.TagName = "Charging";
            this.ucBatteryCharging.TagValue = "false";
            // 
            // ucBatteryTemperature
            // 
            this.ucBatteryTemperature.Location = new System.Drawing.Point(236, 114);
            this.ucBatteryTemperature.Name = "ucBatteryTemperature";
            this.ucBatteryTemperature.Size = new System.Drawing.Size(200, 59);
            this.ucBatteryTemperature.TabIndex = 2;
            this.ucBatteryTemperature.TagColor = System.Drawing.Color.Black;
            this.ucBatteryTemperature.TagName = "Temperature";
            this.ucBatteryTemperature.TagValue = "40.5";
            // 
            // ucBatteryVoltage
            // 
            this.ucBatteryVoltage.Location = new System.Drawing.Point(236, 21);
            this.ucBatteryVoltage.Name = "ucBatteryVoltage";
            this.ucBatteryVoltage.Size = new System.Drawing.Size(200, 59);
            this.ucBatteryVoltage.TabIndex = 2;
            this.ucBatteryVoltage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryVoltage.TagName = "Voltage";
            this.ucBatteryVoltage.TagValue = "55.66";
            // 
            // ucBatteryPercentage
            // 
            this.ucBatteryPercentage.Location = new System.Drawing.Point(6, 21);
            this.ucBatteryPercentage.Name = "ucBatteryPercentage";
            this.ucBatteryPercentage.Size = new System.Drawing.Size(200, 59);
            this.ucBatteryPercentage.TabIndex = 0;
            this.ucBatteryPercentage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryPercentage.TagName = "Percentage";
            this.ucBatteryPercentage.TagValue = " 70.0";
            // 
            // AseRobotControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1206, 722);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tabControl1);
            this.Name = "AseRobotControlForm";
            this.Text = "AseRobotControlForm";
            this.tabControl1.ResumeLayout(false);
            this.pageRobotCommnad.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.pageChargeCommand.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pageRobotCommnad;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtToPort;
        private System.Windows.Forms.TextBox txtFromPort;
        private System.Windows.Forms.Button btnSendRobot;
        private System.Windows.Forms.ComboBox boxPioDirection;
        private System.Windows.Forms.CheckBox cbIsLoad;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txtPortNumber;
        private System.Windows.Forms.TextBox txtGateType;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabPage pageChargeCommand;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSearchChargeAddress;
        private System.Windows.Forms.Button btnStopCharge;
        private System.Windows.Forms.Button btnStartCharge;
        private System.Windows.Forms.ComboBox boxChargeDirection;
        private System.Windows.Forms.TextBox txtChargeAddress;
        private System.Windows.Forms.GroupBox groupBox2;
        private  UcVerticalLabelText ucBatteryPercentage;
        private  UcVerticalLabelText ucBatteryTemperature;
        private  UcVerticalLabelText ucBatteryVoltage;
        private  UcVerticalLabelText ucBatteryCharging;
        private System.Windows.Forms.Button btnRefreshBatterySate;
        private System.Windows.Forms.Button btnRefreshRobotState;
        private UcVerticalLabelText ucRobotIsHome;
        private UcVerticalLabelText ucRobotSlotRId;
        private UcVerticalLabelText ucRobotSlotRState;
        private UcVerticalLabelText ucRobotSlotLState;
        private UcVerticalLabelText ucRobotSlotLId;
        private UcVerticalLabelText ucRobotRobotState;
    }
}