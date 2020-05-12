namespace Mirle.Agv.AseMiddler.View
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
            this.ucRobotIsHome = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotLState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotLId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotRobotState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
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
            this.ucBatteryCharging = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryTemperature = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.btnRefreshBatterySate = new System.Windows.Forms.Button();
            this.ucBatteryVoltage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryPercentage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSearchChargeAddress = new System.Windows.Forms.Button();
            this.btnStopCharge = new System.Windows.Forms.Button();
            this.btnStartCharge = new System.Windows.Forms.Button();
            this.boxChargeDirection = new System.Windows.Forms.ComboBox();
            this.txtChargeAddress = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnPauseAskBattery = new System.Windows.Forms.Button();
            this.btnResumeAskBattery = new System.Windows.Forms.Button();
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
            this.tabControl1.Location = new System.Drawing.Point(14, 15);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1379, 264);
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
            this.pageRobotCommnad.Location = new System.Drawing.Point(4, 24);
            this.pageRobotCommnad.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pageRobotCommnad.Name = "pageRobotCommnad";
            this.pageRobotCommnad.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pageRobotCommnad.Size = new System.Drawing.Size(1371, 236);
            this.pageRobotCommnad.TabIndex = 0;
            this.pageRobotCommnad.Text = "RobotCommnad";
            this.pageRobotCommnad.UseVisualStyleBackColor = true;
            // 
            // btnRefreshRobotState
            // 
            this.btnRefreshRobotState.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshRobotState.Location = new System.Drawing.Point(815, 88);
            this.btnRefreshRobotState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRefreshRobotState.Name = "btnRefreshRobotState";
            this.btnRefreshRobotState.Size = new System.Drawing.Size(182, 98);
            this.btnRefreshRobotState.TabIndex = 50;
            this.btnRefreshRobotState.Text = "更新手臂.儲位狀態";
            this.btnRefreshRobotState.UseVisualStyleBackColor = true;
            this.btnRefreshRobotState.Click += new System.EventHandler(this.btnRefreshRobotState_Click);
            // 
            // ucRobotIsHome
            // 
            this.ucRobotIsHome.Location = new System.Drawing.Point(322, 104);
            this.ucRobotIsHome.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucRobotIsHome.Name = "ucRobotIsHome";
            this.ucRobotIsHome.Size = new System.Drawing.Size(157, 81);
            this.ucRobotIsHome.TabIndex = 44;
            this.ucRobotIsHome.TagColor = System.Drawing.Color.Black;
            this.ucRobotIsHome.TagName = "Is Home";
            this.ucRobotIsHome.TagValue = "false";
            // 
            // ucRobotSlotRId
            // 
            this.ucRobotSlotRId.Location = new System.Drawing.Point(651, 104);
            this.ucRobotSlotRId.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucRobotSlotRId.Name = "ucRobotSlotRId";
            this.ucRobotSlotRId.Size = new System.Drawing.Size(157, 81);
            this.ucRobotSlotRId.TabIndex = 45;
            this.ucRobotSlotRId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRId.TagName = "Slot R Id";
            this.ucRobotSlotRId.TagValue = "PQR";
            // 
            // ucRobotSlotRState
            // 
            this.ucRobotSlotRState.Location = new System.Drawing.Point(651, 12);
            this.ucRobotSlotRState.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucRobotSlotRState.Name = "ucRobotSlotRState";
            this.ucRobotSlotRState.Size = new System.Drawing.Size(157, 81);
            this.ucRobotSlotRState.TabIndex = 46;
            this.ucRobotSlotRState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRState.TagName = "Slot R State";
            this.ucRobotSlotRState.TagValue = "Empty";
            // 
            // ucRobotSlotLState
            // 
            this.ucRobotSlotLState.Location = new System.Drawing.Point(486, 12);
            this.ucRobotSlotLState.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucRobotSlotLState.Name = "ucRobotSlotLState";
            this.ucRobotSlotLState.Size = new System.Drawing.Size(157, 81);
            this.ucRobotSlotLState.TabIndex = 47;
            this.ucRobotSlotLState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLState.TagName = "Slot L State";
            this.ucRobotSlotLState.TagValue = "Empty";
            // 
            // ucRobotSlotLId
            // 
            this.ucRobotSlotLId.Location = new System.Drawing.Point(486, 104);
            this.ucRobotSlotLId.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucRobotSlotLId.Name = "ucRobotSlotLId";
            this.ucRobotSlotLId.Size = new System.Drawing.Size(157, 81);
            this.ucRobotSlotLId.TabIndex = 48;
            this.ucRobotSlotLId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLId.TagName = "Slot L Id";
            this.ucRobotSlotLId.TagValue = "ABC";
            // 
            // ucRobotRobotState
            // 
            this.ucRobotRobotState.Location = new System.Drawing.Point(322, 12);
            this.ucRobotRobotState.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucRobotRobotState.Name = "ucRobotRobotState";
            this.ucRobotRobotState.Size = new System.Drawing.Size(157, 81);
            this.ucRobotRobotState.TabIndex = 49;
            this.ucRobotRobotState.TagColor = System.Drawing.Color.Black;
            this.ucRobotRobotState.TagName = "Robot State";
            this.ucRobotRobotState.TagValue = "Idle";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.boxPioDirection);
            this.groupBox4.Controls.Add(this.txtToPort);
            this.groupBox4.Controls.Add(this.txtPortNumber);
            this.groupBox4.Controls.Add(this.txtGateType);
            this.groupBox4.Controls.Add(this.txtFromPort);
            this.groupBox4.Controls.Add(this.cbIsLoad);
            this.groupBox4.Location = new System.Drawing.Point(7, 8);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox4.Size = new System.Drawing.Size(308, 155);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Robot(P45)";
            // 
            // boxPioDirection
            // 
            this.boxPioDirection.FormattingEnabled = true;
            this.boxPioDirection.Location = new System.Drawing.Point(7, 26);
            this.boxPioDirection.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.boxPioDirection.Name = "boxPioDirection";
            this.boxPioDirection.Size = new System.Drawing.Size(136, 23);
            this.boxPioDirection.TabIndex = 10;
            // 
            // txtToPort
            // 
            this.txtToPort.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtToPort.Location = new System.Drawing.Point(152, 59);
            this.txtToPort.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtToPort.Name = "txtToPort";
            this.txtToPort.Size = new System.Drawing.Size(136, 24);
            this.txtToPort.TabIndex = 9;
            this.txtToPort.Text = "To";
            // 
            // txtPortNumber
            // 
            this.txtPortNumber.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtPortNumber.Location = new System.Drawing.Point(150, 94);
            this.txtPortNumber.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPortNumber.Name = "txtPortNumber";
            this.txtPortNumber.Size = new System.Drawing.Size(136, 24);
            this.txtPortNumber.TabIndex = 8;
            this.txtPortNumber.Text = "1";
            // 
            // txtGateType
            // 
            this.txtGateType.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtGateType.Location = new System.Drawing.Point(7, 94);
            this.txtGateType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGateType.Name = "txtGateType";
            this.txtGateType.Size = new System.Drawing.Size(136, 24);
            this.txtGateType.TabIndex = 8;
            this.txtGateType.Text = "0";
            // 
            // txtFromPort
            // 
            this.txtFromPort.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtFromPort.Location = new System.Drawing.Point(7, 59);
            this.txtFromPort.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtFromPort.Name = "txtFromPort";
            this.txtFromPort.Size = new System.Drawing.Size(136, 24);
            this.txtFromPort.TabIndex = 8;
            this.txtFromPort.Text = "From";
            // 
            // cbIsLoad
            // 
            this.cbIsLoad.AutoSize = true;
            this.cbIsLoad.Location = new System.Drawing.Point(202, 26);
            this.cbIsLoad.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbIsLoad.Name = "cbIsLoad";
            this.cbIsLoad.Size = new System.Drawing.Size(71, 19);
            this.cbIsLoad.TabIndex = 0;
            this.cbIsLoad.Text = "Is Load ?";
            this.cbIsLoad.UseVisualStyleBackColor = true;
            // 
            // btnSendRobot
            // 
            this.btnSendRobot.Location = new System.Drawing.Point(20, 170);
            this.btnSendRobot.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSendRobot.Name = "btnSendRobot";
            this.btnSendRobot.Size = new System.Drawing.Size(295, 41);
            this.btnSendRobot.TabIndex = 5;
            this.btnSendRobot.Text = "Send";
            this.btnSendRobot.UseVisualStyleBackColor = true;
            this.btnSendRobot.Click += new System.EventHandler(this.btnSendRobot_Click);
            // 
            // pageChargeCommand
            // 
            this.pageChargeCommand.Controls.Add(this.groupBox2);
            this.pageChargeCommand.Controls.Add(this.groupBox1);
            this.pageChargeCommand.Location = new System.Drawing.Point(4, 24);
            this.pageChargeCommand.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pageChargeCommand.Name = "pageChargeCommand";
            this.pageChargeCommand.Size = new System.Drawing.Size(1371, 236);
            this.pageChargeCommand.TabIndex = 1;
            this.pageChargeCommand.Text = "ChargeCommand";
            this.pageChargeCommand.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ucBatteryCharging);
            this.groupBox2.Controls.Add(this.ucBatteryTemperature);
            this.groupBox2.Controls.Add(this.btnResumeAskBattery);
            this.groupBox2.Controls.Add(this.btnPauseAskBattery);
            this.groupBox2.Controls.Add(this.btnRefreshBatterySate);
            this.groupBox2.Controls.Add(this.ucBatteryVoltage);
            this.groupBox2.Controls.Add(this.ucBatteryPercentage);
            this.groupBox2.Location = new System.Drawing.Point(343, 4);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(838, 224);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Info";
            // 
            // ucBatteryCharging
            // 
            this.ucBatteryCharging.Location = new System.Drawing.Point(170, 25);
            this.ucBatteryCharging.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucBatteryCharging.Name = "ucBatteryCharging";
            this.ucBatteryCharging.Size = new System.Drawing.Size(157, 81);
            this.ucBatteryCharging.TabIndex = 2;
            this.ucBatteryCharging.TagColor = System.Drawing.Color.Black;
            this.ucBatteryCharging.TagName = "Charging";
            this.ucBatteryCharging.TagValue = "false";
            // 
            // ucBatteryTemperature
            // 
            this.ucBatteryTemperature.Location = new System.Drawing.Point(170, 116);
            this.ucBatteryTemperature.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucBatteryTemperature.Name = "ucBatteryTemperature";
            this.ucBatteryTemperature.Size = new System.Drawing.Size(157, 81);
            this.ucBatteryTemperature.TabIndex = 2;
            this.ucBatteryTemperature.TagColor = System.Drawing.Color.Black;
            this.ucBatteryTemperature.TagName = "Temperature";
            this.ucBatteryTemperature.TagValue = "40.5";
            // 
            // btnRefreshBatterySate
            // 
            this.btnRefreshBatterySate.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshBatterySate.Location = new System.Drawing.Point(333, 25);
            this.btnRefreshBatterySate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRefreshBatterySate.Name = "btnRefreshBatterySate";
            this.btnRefreshBatterySate.Size = new System.Drawing.Size(157, 81);
            this.btnRefreshBatterySate.TabIndex = 3;
            this.btnRefreshBatterySate.Text = "Refresh State";
            this.btnRefreshBatterySate.UseVisualStyleBackColor = true;
            this.btnRefreshBatterySate.Click += new System.EventHandler(this.btnRefreshBatterySate_Click);
            // 
            // ucBatteryVoltage
            // 
            this.ucBatteryVoltage.Location = new System.Drawing.Point(7, 116);
            this.ucBatteryVoltage.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucBatteryVoltage.Name = "ucBatteryVoltage";
            this.ucBatteryVoltage.Size = new System.Drawing.Size(157, 81);
            this.ucBatteryVoltage.TabIndex = 2;
            this.ucBatteryVoltage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryVoltage.TagName = "Voltage";
            this.ucBatteryVoltage.TagValue = "55.66";
            // 
            // ucBatteryPercentage
            // 
            this.ucBatteryPercentage.Location = new System.Drawing.Point(7, 26);
            this.ucBatteryPercentage.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ucBatteryPercentage.Name = "ucBatteryPercentage";
            this.ucBatteryPercentage.Size = new System.Drawing.Size(157, 81);
            this.ucBatteryPercentage.TabIndex = 0;
            this.ucBatteryPercentage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryPercentage.TagName = "Percentage";
            this.ucBatteryPercentage.TagValue = " 70.0";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSearchChargeAddress);
            this.groupBox1.Controls.Add(this.btnStopCharge);
            this.groupBox1.Controls.Add(this.btnStartCharge);
            this.groupBox1.Controls.Add(this.boxChargeDirection);
            this.groupBox1.Controls.Add(this.txtChargeAddress);
            this.groupBox1.Location = new System.Drawing.Point(3, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(332, 224);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Charge(P47)";
            // 
            // btnSearchChargeAddress
            // 
            this.btnSearchChargeAddress.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSearchChargeAddress.Location = new System.Drawing.Point(7, 66);
            this.btnSearchChargeAddress.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSearchChargeAddress.Name = "btnSearchChargeAddress";
            this.btnSearchChargeAddress.Size = new System.Drawing.Size(318, 42);
            this.btnSearchChargeAddress.TabIndex = 4;
            this.btnSearchChargeAddress.Text = "Search Address";
            this.btnSearchChargeAddress.UseVisualStyleBackColor = true;
            this.btnSearchChargeAddress.Click += new System.EventHandler(this.btnSearchChargeAddress_Click);
            // 
            // btnStopCharge
            // 
            this.btnStopCharge.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStopCharge.Location = new System.Drawing.Point(163, 164);
            this.btnStopCharge.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnStopCharge.Name = "btnStopCharge";
            this.btnStopCharge.Size = new System.Drawing.Size(162, 52);
            this.btnStopCharge.TabIndex = 3;
            this.btnStopCharge.Text = "DisCharge";
            this.btnStopCharge.UseVisualStyleBackColor = true;
            this.btnStopCharge.Click += new System.EventHandler(this.btnStopCharge_Click);
            // 
            // btnStartCharge
            // 
            this.btnStartCharge.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStartCharge.Location = new System.Drawing.Point(7, 164);
            this.btnStartCharge.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnStartCharge.Name = "btnStartCharge";
            this.btnStartCharge.Size = new System.Drawing.Size(149, 52);
            this.btnStartCharge.TabIndex = 3;
            this.btnStartCharge.Text = "Charge";
            this.btnStartCharge.UseVisualStyleBackColor = true;
            this.btnStartCharge.Click += new System.EventHandler(this.btnStartCharge_Click);
            // 
            // boxChargeDirection
            // 
            this.boxChargeDirection.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxChargeDirection.FormattingEnabled = true;
            this.boxChargeDirection.Location = new System.Drawing.Point(7, 116);
            this.boxChargeDirection.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.boxChargeDirection.Name = "boxChargeDirection";
            this.boxChargeDirection.Size = new System.Drawing.Size(318, 32);
            this.boxChargeDirection.TabIndex = 1;
            // 
            // txtChargeAddress
            // 
            this.txtChargeAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtChargeAddress.Location = new System.Drawing.Point(7, 26);
            this.txtChargeAddress.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtChargeAddress.Name = "txtChargeAddress";
            this.txtChargeAddress.Size = new System.Drawing.Size(318, 26);
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
            this.textBox1.Location = new System.Drawing.Point(14, 286);
            this.textBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(1378, 600);
            this.textBox1.TabIndex = 4;
            // 
            // btnPauseAskBattery
            // 
            this.btnPauseAskBattery.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPauseAskBattery.Location = new System.Drawing.Point(496, 25);
            this.btnPauseAskBattery.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPauseAskBattery.Name = "btnPauseAskBattery";
            this.btnPauseAskBattery.Size = new System.Drawing.Size(218, 81);
            this.btnPauseAskBattery.TabIndex = 3;
            this.btnPauseAskBattery.Text = "Pause Ask Battery";
            this.btnPauseAskBattery.UseVisualStyleBackColor = true;
            this.btnPauseAskBattery.Click += new System.EventHandler(this.btnPauseAskBattery_Click);
            // 
            // btnResumeAskBattery
            // 
            this.btnResumeAskBattery.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnResumeAskBattery.Location = new System.Drawing.Point(496, 116);
            this.btnResumeAskBattery.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnResumeAskBattery.Name = "btnResumeAskBattery";
            this.btnResumeAskBattery.Size = new System.Drawing.Size(218, 81);
            this.btnResumeAskBattery.TabIndex = 3;
            this.btnResumeAskBattery.Text = "Resume Ask Battery";
            this.btnResumeAskBattery.UseVisualStyleBackColor = true;
            this.btnResumeAskBattery.Click += new System.EventHandler(this.btnResumeAskBattery_Click);
            // 
            // AseRobotControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1407, 902);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
        private System.Windows.Forms.Button btnResumeAskBattery;
        private System.Windows.Forms.Button btnPauseAskBattery;
    }
}