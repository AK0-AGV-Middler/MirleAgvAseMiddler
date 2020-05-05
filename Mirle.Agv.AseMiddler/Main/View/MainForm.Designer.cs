namespace Mirle.Agv.AseMiddler.View
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.系統ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.啟動ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.登入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.登出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.關閉ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.語言ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.中文ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.模式ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.VehicleStatusPage = new System.Windows.Forms.ToolStripMenuItem();
            this.AlarmPage = new System.Windows.Forms.ToolStripMenuItem();
            this.AgvcConnectorPage = new System.Windows.Forms.ToolStripMenuItem();
            this.MovePage = new System.Windows.Forms.ToolStripMenuItem();
            this.RobotAndChargePage = new System.Windows.Forms.ToolStripMenuItem();
            this.AgvlConnectorPage = new System.Windows.Forms.ToolStripMenuItem();
            this.工程師ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.模擬測試ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tbxDebugLogMsg = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pageBasicState = new System.Windows.Forms.TabPage();
            this.btnRefreshPosition = new System.Windows.Forms.Button();
            this.btnPrintScreen = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtWatchLowPower = new System.Windows.Forms.Label();
            this.txtAskingReserve = new System.Windows.Forms.Label();
            this.txtTrackPosition = new System.Windows.Forms.Label();
            this.txtTransferStep = new System.Windows.Forms.Label();
            this.gbPerformanceCounter = new System.Windows.Forms.GroupBox();
            this.ucRCstId = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucLCstId = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.btnKeyInSoc = new System.Windows.Forms.Button();
            this.ucRobotHome = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucCharging = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.numSoc = new System.Windows.Forms.NumericUpDown();
            this.ucSoc = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.txtAgvcConnection = new System.Windows.Forms.Label();
            this.radAgvcOnline = new System.Windows.Forms.RadioButton();
            this.radAgvcOffline = new System.Windows.Forms.RadioButton();
            this.btnBuzzOff = new System.Windows.Forms.Button();
            this.txtLastAlarm = new System.Windows.Forms.Label();
            this.btnAlarmReset = new System.Windows.Forms.Button();
            this.txtCannotAutoReason = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtAgvlConnection = new System.Windows.Forms.Label();
            this.radAgvlOnline = new System.Windows.Forms.RadioButton();
            this.radAgvlOffline = new System.Windows.Forms.RadioButton();
            this.txtCanAuto = new System.Windows.Forms.Label();
            this.btnAutoManual = new System.Windows.Forms.Button();
            this.pageMoveState = new System.Windows.Forms.TabPage();
            this.btnRefreshMoveState = new System.Windows.Forms.Button();
            this.gbVehicleLocation = new System.Windows.Forms.GroupBox();
            this.numPositionY = new System.Windows.Forms.NumericUpDown();
            this.numPositionX = new System.Windows.Forms.NumericUpDown();
            this.btnKeyInPosition = new System.Windows.Forms.Button();
            this.ucMoveMovingIndex = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveMoveState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePauseStop = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveReserveStop = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveLastAddress = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveIsMoveEnd = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveLastSection = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePositionY = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePositionX = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.pageRobotSate = new System.Windows.Forms.TabPage();
            this.btnRefreshRobotState = new System.Windows.Forms.Button();
            this.ucRobotIsHome = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotLState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotLId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotRobotState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.pageBatteryState = new System.Windows.Forms.TabPage();
            this.btnRefreshBatteryState = new System.Windows.Forms.Button();
            this.ucBatteryCharging = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryTemperature = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryVoltage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryPercentage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.pageVehicleState = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtBatterysAbnormal = new System.Windows.Forms.Label();
            this.txtMainFlowAbnormal = new System.Windows.Forms.Label();
            this.txtAgvcConnectorAbnormal = new System.Windows.Forms.Label();
            this.txtRobotAbnormal = new System.Windows.Forms.Label();
            this.txtMoveControlAbnormal = new System.Windows.Forms.Label();
            this.ucGoNextStep = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucTransferStepType = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucTransferSteps = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucTransferIndex = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.pageReserveInfo = new System.Windows.Forms.TabPage();
            this.gbReserve = new System.Windows.Forms.GroupBox();
            this.lbxReserveOkSections = new System.Windows.Forms.ListBox();
            this.lbxNeedReserveSections = new System.Windows.Forms.ListBox();
            this.pageTransferCommand = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tbxTransferStepMsg = new System.Windows.Forms.TextBox();
            this.tbxTransferCommandMsg = new System.Windows.Forms.TextBox();
            this.timeUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tspbCommding = new System.Windows.Forms.ToolStripProgressBar();
            this.tstextClientName = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRemoteIp = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRemotePort = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextLastPosX = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextLastPosY = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer_SetupInitialSoc = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.pageBasicState.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gbPerformanceCounter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSoc)).BeginInit();
            this.gbConnection.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.pageMoveState.SuspendLayout();
            this.gbVehicleLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).BeginInit();
            this.pageRobotSate.SuspendLayout();
            this.pageBatteryState.SuspendLayout();
            this.pageVehicleState.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.pageReserveInfo.SuspendLayout();
            this.gbReserve.SuspendLayout();
            this.pageTransferCommand.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1375, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 系統ToolStripMenuItem
            // 
            this.系統ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.啟動ToolStripMenuItem,
            this.登入ToolStripMenuItem,
            this.登出ToolStripMenuItem,
            this.關閉ToolStripMenuItem});
            this.系統ToolStripMenuItem.Name = "系統ToolStripMenuItem";
            this.系統ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.系統ToolStripMenuItem.Text = "系統";
            // 
            // 啟動ToolStripMenuItem
            // 
            this.啟動ToolStripMenuItem.Name = "啟動ToolStripMenuItem";
            this.啟動ToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.啟動ToolStripMenuItem.Text = "啟動";
            // 
            // 登入ToolStripMenuItem
            // 
            this.登入ToolStripMenuItem.Name = "登入ToolStripMenuItem";
            this.登入ToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.登入ToolStripMenuItem.Text = "登入";
            // 
            // 登出ToolStripMenuItem
            // 
            this.登出ToolStripMenuItem.Name = "登出ToolStripMenuItem";
            this.登出ToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.登出ToolStripMenuItem.Text = "登出";
            // 
            // 關閉ToolStripMenuItem
            // 
            this.關閉ToolStripMenuItem.Name = "關閉ToolStripMenuItem";
            this.關閉ToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.關閉ToolStripMenuItem.Text = "關閉";
            this.關閉ToolStripMenuItem.Click += new System.EventHandler(this.關閉ToolStripMenuItem_Click);
            // 
            // 語言ToolStripMenuItem
            // 
            this.語言ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.中文ToolStripMenuItem,
            this.englishToolStripMenuItem});
            this.語言ToolStripMenuItem.Name = "語言ToolStripMenuItem";
            this.語言ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.語言ToolStripMenuItem.Text = "語言";
            // 
            // 中文ToolStripMenuItem
            // 
            this.中文ToolStripMenuItem.Name = "中文ToolStripMenuItem";
            this.中文ToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.中文ToolStripMenuItem.Text = "中文";
            // 
            // englishToolStripMenuItem
            // 
            this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            this.englishToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.englishToolStripMenuItem.Text = "English";
            // 
            // 模式ToolStripMenuItem
            // 
            this.模式ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.VehicleStatusPage,
            this.AlarmPage,
            this.AgvcConnectorPage,
            this.MovePage,
            this.RobotAndChargePage,
            this.AgvlConnectorPage});
            this.模式ToolStripMenuItem.Name = "模式ToolStripMenuItem";
            this.模式ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.模式ToolStripMenuItem.Text = "模式";
            // 
            // VehicleStatusPage
            // 
            this.VehicleStatusPage.Name = "VehicleStatusPage";
            this.VehicleStatusPage.Size = new System.Drawing.Size(179, 22);
            this.VehicleStatusPage.Text = "Parameter";
            this.VehicleStatusPage.Click += new System.EventHandler(this.VehicleStatusPage_Click);
            // 
            // AlarmPage
            // 
            this.AlarmPage.Name = "AlarmPage";
            this.AlarmPage.Size = new System.Drawing.Size(179, 22);
            this.AlarmPage.Text = "Alarm";
            this.AlarmPage.Click += new System.EventHandler(this.AlarmPage_Click);
            // 
            // AgvcConnectorPage
            // 
            this.AgvcConnectorPage.Name = "AgvcConnectorPage";
            this.AgvcConnectorPage.Size = new System.Drawing.Size(179, 22);
            this.AgvcConnectorPage.Text = "AgvcConnector";
            this.AgvcConnectorPage.Click += new System.EventHandler(this.AgvcConnectorPage_Click);
            // 
            // MovePage
            // 
            this.MovePage.Name = "MovePage";
            this.MovePage.Size = new System.Drawing.Size(179, 22);
            this.MovePage.Text = "Move";
            this.MovePage.Click += new System.EventHandler(this.ManualMoveCmdPage_Click);
            // 
            // RobotAndChargePage
            // 
            this.RobotAndChargePage.Name = "RobotAndChargePage";
            this.RobotAndChargePage.Size = new System.Drawing.Size(179, 22);
            this.RobotAndChargePage.Text = "Robot and Charge";
            this.RobotAndChargePage.Click += new System.EventHandler(this.RobotControlPage_Click);
            // 
            // AgvlConnectorPage
            // 
            this.AgvlConnectorPage.Name = "AgvlConnectorPage";
            this.AgvlConnectorPage.Size = new System.Drawing.Size(179, 22);
            this.AgvlConnectorPage.Text = "AgvlConnector";
            this.AgvlConnectorPage.Click += new System.EventHandler(this.AgvlConnectorPage_Click);
            // 
            // 工程師ToolStripMenuItem
            // 
            this.工程師ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.模擬測試ToolStripMenuItem});
            this.工程師ToolStripMenuItem.Name = "工程師ToolStripMenuItem";
            this.工程師ToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.工程師ToolStripMenuItem.Text = "工程師";
            // 
            // 模擬測試ToolStripMenuItem
            // 
            this.模擬測試ToolStripMenuItem.Name = "模擬測試ToolStripMenuItem";
            this.模擬測試ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.模擬測試ToolStripMenuItem.Text = "模擬測試";
            this.模擬測試ToolStripMenuItem.Click += new System.EventHandler(this.模擬測試ToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(1375, 737);
            this.splitContainer1.SplitterDistance = 750;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.AutoScroll = true;
            this.splitContainer3.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer3.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tbxDebugLogMsg);
            this.splitContainer3.Size = new System.Drawing.Size(750, 737);
            this.splitContainer3.SplitterDistance = 502;
            this.splitContainer3.SplitterIncrement = 10;
            this.splitContainer3.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            // 
            // tbxDebugLogMsg
            // 
            this.tbxDebugLogMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxDebugLogMsg.Location = new System.Drawing.Point(3, 6);
            this.tbxDebugLogMsg.Multiline = true;
            this.tbxDebugLogMsg.Name = "tbxDebugLogMsg";
            this.tbxDebugLogMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxDebugLogMsg.Size = new System.Drawing.Size(744, 196);
            this.tbxDebugLogMsg.TabIndex = 58;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pageBasicState);
            this.tabControl1.Controls.Add(this.pageMoveState);
            this.tabControl1.Controls.Add(this.pageRobotSate);
            this.tabControl1.Controls.Add(this.pageBatteryState);
            this.tabControl1.Controls.Add(this.pageVehicleState);
            this.tabControl1.Controls.Add(this.pageReserveInfo);
            this.tabControl1.Controls.Add(this.pageTransferCommand);
            this.tabControl1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(615, 709);
            this.tabControl1.TabIndex = 63;
            // 
            // pageBasicState
            // 
            this.pageBasicState.Controls.Add(this.btnRefreshPosition);
            this.pageBasicState.Controls.Add(this.btnPrintScreen);
            this.pageBasicState.Controls.Add(this.groupBox2);
            this.pageBasicState.Controls.Add(this.gbPerformanceCounter);
            this.pageBasicState.Controls.Add(this.gbConnection);
            this.pageBasicState.Controls.Add(this.btnBuzzOff);
            this.pageBasicState.Controls.Add(this.txtLastAlarm);
            this.pageBasicState.Controls.Add(this.btnAlarmReset);
            this.pageBasicState.Controls.Add(this.txtCannotAutoReason);
            this.pageBasicState.Controls.Add(this.groupBox1);
            this.pageBasicState.Controls.Add(this.txtCanAuto);
            this.pageBasicState.Controls.Add(this.btnAutoManual);
            this.pageBasicState.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageBasicState.Location = new System.Drawing.Point(4, 29);
            this.pageBasicState.Name = "pageBasicState";
            this.pageBasicState.Size = new System.Drawing.Size(607, 676);
            this.pageBasicState.TabIndex = 6;
            this.pageBasicState.Text = "Basic";
            this.pageBasicState.UseVisualStyleBackColor = true;
            // 
            // btnRefreshPosition
            // 
            this.btnRefreshPosition.Location = new System.Drawing.Point(6, 416);
            this.btnRefreshPosition.Name = "btnRefreshPosition";
            this.btnRefreshPosition.Size = new System.Drawing.Size(209, 74);
            this.btnRefreshPosition.TabIndex = 60;
            this.btnRefreshPosition.Text = "Refresh Position";
            this.btnRefreshPosition.UseVisualStyleBackColor = true;
            this.btnRefreshPosition.Click += new System.EventHandler(this.btnRefreshPosition_Click);
            // 
            // btnPrintScreen
            // 
            this.btnPrintScreen.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPrintScreen.ForeColor = System.Drawing.Color.OrangeRed;
            this.btnPrintScreen.Location = new System.Drawing.Point(6, 495);
            this.btnPrintScreen.Name = "btnPrintScreen";
            this.btnPrintScreen.Size = new System.Drawing.Size(210, 74);
            this.btnPrintScreen.TabIndex = 59;
            this.btnPrintScreen.Text = "拍照截圖";
            this.btnPrintScreen.UseVisualStyleBackColor = true;
            this.btnPrintScreen.Click += new System.EventHandler(this.btnPrintScreen_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtWatchLowPower);
            this.groupBox2.Controls.Add(this.txtAskingReserve);
            this.groupBox2.Controls.Add(this.txtTrackPosition);
            this.groupBox2.Controls.Add(this.txtTransferStep);
            this.groupBox2.Location = new System.Drawing.Point(6, 282);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(209, 128);
            this.groupBox2.TabIndex = 61;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "FlowStatus";
            // 
            // txtWatchLowPower
            // 
            this.txtWatchLowPower.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtWatchLowPower.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtWatchLowPower.Location = new System.Drawing.Point(31, 93);
            this.txtWatchLowPower.Name = "txtWatchLowPower";
            this.txtWatchLowPower.Size = new System.Drawing.Size(150, 23);
            this.txtWatchLowPower.TabIndex = 58;
            this.txtWatchLowPower.Text = "Soc/Gap : 100/50";
            this.txtWatchLowPower.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtAskingReserve
            // 
            this.txtAskingReserve.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtAskingReserve.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAskingReserve.Location = new System.Drawing.Point(28, 70);
            this.txtAskingReserve.Name = "txtAskingReserve";
            this.txtAskingReserve.Size = new System.Drawing.Size(150, 23);
            this.txtAskingReserve.TabIndex = 58;
            this.txtAskingReserve.Text = "ID:Sec001";
            this.txtAskingReserve.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtTrackPosition
            // 
            this.txtTrackPosition.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTrackPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtTrackPosition.Location = new System.Drawing.Point(31, 47);
            this.txtTrackPosition.Name = "txtTrackPosition";
            this.txtTrackPosition.Size = new System.Drawing.Size(150, 23);
            this.txtTrackPosition.TabIndex = 57;
            this.txtTrackPosition.Text = "(StepI, MoveI)";
            this.txtTrackPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtTransferStep
            // 
            this.txtTransferStep.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTransferStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtTransferStep.Location = new System.Drawing.Point(28, 23);
            this.txtTransferStep.Name = "txtTransferStep";
            this.txtTransferStep.Size = new System.Drawing.Size(150, 23);
            this.txtTransferStep.TabIndex = 56;
            this.txtTransferStep.Text = "Step : Move";
            this.txtTransferStep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gbPerformanceCounter
            // 
            this.gbPerformanceCounter.Controls.Add(this.ucRCstId);
            this.gbPerformanceCounter.Controls.Add(this.ucLCstId);
            this.gbPerformanceCounter.Controls.Add(this.btnKeyInSoc);
            this.gbPerformanceCounter.Controls.Add(this.ucRobotHome);
            this.gbPerformanceCounter.Controls.Add(this.ucCharging);
            this.gbPerformanceCounter.Controls.Add(this.numSoc);
            this.gbPerformanceCounter.Controls.Add(this.ucSoc);
            this.gbPerformanceCounter.Location = new System.Drawing.Point(222, 282);
            this.gbPerformanceCounter.Name = "gbPerformanceCounter";
            this.gbPerformanceCounter.Size = new System.Drawing.Size(208, 315);
            this.gbPerformanceCounter.TabIndex = 10;
            this.gbPerformanceCounter.TabStop = false;
            this.gbPerformanceCounter.Text = "Performance Counter";
            // 
            // ucRCstId
            // 
            this.ucRCstId.Location = new System.Drawing.Point(11, 57);
            this.ucRCstId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRCstId.Name = "ucRCstId";
            this.ucRCstId.Size = new System.Drawing.Size(187, 26);
            this.ucRCstId.TabIndex = 42;
            this.ucRCstId.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucRCstId.TagName = "RCstId";
            this.ucRCstId.TagValue = "";
            // 
            // ucLCstId
            // 
            this.ucLCstId.Location = new System.Drawing.Point(10, 25);
            this.ucLCstId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucLCstId.Name = "ucLCstId";
            this.ucLCstId.Size = new System.Drawing.Size(187, 26);
            this.ucLCstId.TabIndex = 42;
            this.ucLCstId.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLCstId.TagName = "LCstId";
            this.ucLCstId.TagValue = "";
            // 
            // btnKeyInSoc
            // 
            this.btnKeyInSoc.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnKeyInSoc.Location = new System.Drawing.Point(6, 273);
            this.btnKeyInSoc.Name = "btnKeyInSoc";
            this.btnKeyInSoc.Size = new System.Drawing.Size(191, 28);
            this.btnKeyInSoc.TabIndex = 40;
            this.btnKeyInSoc.Text = "校正電量";
            this.btnKeyInSoc.UseVisualStyleBackColor = true;
            this.btnKeyInSoc.Click += new System.EventHandler(this.btnKeyInSoc_Click);
            // 
            // ucRobotHome
            // 
            this.ucRobotHome.Location = new System.Drawing.Point(6, 161);
            this.ucRobotHome.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotHome.Name = "ucRobotHome";
            this.ucRobotHome.Size = new System.Drawing.Size(187, 30);
            this.ucRobotHome.TabIndex = 3;
            this.ucRobotHome.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucRobotHome.TagName = "Robot";
            this.ucRobotHome.TagValue = "";
            // 
            // ucCharging
            // 
            this.ucCharging.Location = new System.Drawing.Point(10, 89);
            this.ucCharging.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucCharging.Name = "ucCharging";
            this.ucCharging.Size = new System.Drawing.Size(187, 30);
            this.ucCharging.TabIndex = 3;
            this.ucCharging.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucCharging.TagName = "Charge";
            this.ucCharging.TagValue = "";
            // 
            // numSoc
            // 
            this.numSoc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numSoc.Location = new System.Drawing.Point(6, 240);
            this.numSoc.Name = "numSoc";
            this.numSoc.Size = new System.Drawing.Size(191, 26);
            this.numSoc.TabIndex = 41;
            this.numSoc.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numSoc.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
            // 
            // ucSoc
            // 
            this.ucSoc.Location = new System.Drawing.Point(10, 125);
            this.ucSoc.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucSoc.Name = "ucSoc";
            this.ucSoc.Size = new System.Drawing.Size(187, 30);
            this.ucSoc.TabIndex = 2;
            this.ucSoc.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucSoc.TagName = "SOC";
            this.ucSoc.TagValue = "";
            // 
            // gbConnection
            // 
            this.gbConnection.Controls.Add(this.txtAgvcConnection);
            this.gbConnection.Controls.Add(this.radAgvcOnline);
            this.gbConnection.Controls.Add(this.radAgvcOffline);
            this.gbConnection.Location = new System.Drawing.Point(6, 12);
            this.gbConnection.Name = "gbConnection";
            this.gbConnection.Size = new System.Drawing.Size(209, 86);
            this.gbConnection.TabIndex = 0;
            this.gbConnection.TabStop = false;
            this.gbConnection.Text = "AGVC Connection";
            // 
            // txtAgvcConnection
            // 
            this.txtAgvcConnection.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAgvcConnection.Location = new System.Drawing.Point(6, 49);
            this.txtAgvcConnection.Name = "txtAgvcConnection";
            this.txtAgvcConnection.Size = new System.Drawing.Size(195, 24);
            this.txtAgvcConnection.TabIndex = 2;
            this.txtAgvcConnection.Text = "Connection";
            this.txtAgvcConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // radAgvcOnline
            // 
            this.radAgvcOnline.AutoSize = true;
            this.radAgvcOnline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radAgvcOnline.Location = new System.Drawing.Point(107, 24);
            this.radAgvcOnline.Name = "radAgvcOnline";
            this.radAgvcOnline.Size = new System.Drawing.Size(78, 21);
            this.radAgvcOnline.TabIndex = 1;
            this.radAgvcOnline.Text = "Connect";
            this.radAgvcOnline.UseVisualStyleBackColor = true;
            this.radAgvcOnline.CheckedChanged += new System.EventHandler(this.radAgvcOnline_CheckedChanged);
            // 
            // radAgvcOffline
            // 
            this.radAgvcOffline.AutoSize = true;
            this.radAgvcOffline.Checked = true;
            this.radAgvcOffline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radAgvcOffline.Location = new System.Drawing.Point(3, 24);
            this.radAgvcOffline.Name = "radAgvcOffline";
            this.radAgvcOffline.Size = new System.Drawing.Size(98, 21);
            this.radAgvcOffline.TabIndex = 0;
            this.radAgvcOffline.TabStop = true;
            this.radAgvcOffline.Text = "DisConnect";
            this.radAgvcOffline.UseVisualStyleBackColor = true;
            this.radAgvcOffline.CheckedChanged += new System.EventHandler(this.radAgvcOffline_CheckedChanged);
            // 
            // btnBuzzOff
            // 
            this.btnBuzzOff.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBuzzOff.ForeColor = System.Drawing.Color.Brown;
            this.btnBuzzOff.Location = new System.Drawing.Point(6, 187);
            this.btnBuzzOff.Name = "btnBuzzOff";
            this.btnBuzzOff.Size = new System.Drawing.Size(209, 89);
            this.btnBuzzOff.TabIndex = 3;
            this.btnBuzzOff.Text = "Buzz Off";
            this.btnBuzzOff.UseVisualStyleBackColor = true;
            this.btnBuzzOff.Click += new System.EventHandler(this.btnBuzzOff_Click);
            // 
            // txtLastAlarm
            // 
            this.txtLastAlarm.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtLastAlarm.Location = new System.Drawing.Point(436, 187);
            this.txtLastAlarm.Name = "txtLastAlarm";
            this.txtLastAlarm.Size = new System.Drawing.Size(209, 92);
            this.txtLastAlarm.TabIndex = 50;
            this.txtLastAlarm.Text = "Last Alarm";
            // 
            // btnAlarmReset
            // 
            this.btnAlarmReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAlarmReset.ForeColor = System.Drawing.Color.Red;
            this.btnAlarmReset.Location = new System.Drawing.Point(221, 184);
            this.btnAlarmReset.Name = "btnAlarmReset";
            this.btnAlarmReset.Size = new System.Drawing.Size(209, 92);
            this.btnAlarmReset.TabIndex = 2;
            this.btnAlarmReset.Text = "Alarm Reset";
            this.btnAlarmReset.UseVisualStyleBackColor = true;
            this.btnAlarmReset.Click += new System.EventHandler(this.btnAlarmReset_Click);
            // 
            // txtCannotAutoReason
            // 
            this.txtCannotAutoReason.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtCannotAutoReason.Location = new System.Drawing.Point(436, 101);
            this.txtCannotAutoReason.Name = "txtCannotAutoReason";
            this.txtCannotAutoReason.Size = new System.Drawing.Size(143, 79);
            this.txtCannotAutoReason.TabIndex = 58;
            this.txtCannotAutoReason.Text = "Not Auto Reason";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtAgvlConnection);
            this.groupBox1.Controls.Add(this.radAgvlOnline);
            this.groupBox1.Controls.Add(this.radAgvlOffline);
            this.groupBox1.Location = new System.Drawing.Point(221, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(209, 86);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "AGVL Connection";
            // 
            // txtAgvlConnection
            // 
            this.txtAgvlConnection.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAgvlConnection.Location = new System.Drawing.Point(6, 49);
            this.txtAgvlConnection.Name = "txtAgvlConnection";
            this.txtAgvlConnection.Size = new System.Drawing.Size(195, 24);
            this.txtAgvlConnection.TabIndex = 2;
            this.txtAgvlConnection.Text = "Connection";
            this.txtAgvlConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // radAgvlOnline
            // 
            this.radAgvlOnline.AutoSize = true;
            this.radAgvlOnline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radAgvlOnline.Location = new System.Drawing.Point(107, 24);
            this.radAgvlOnline.Name = "radAgvlOnline";
            this.radAgvlOnline.Size = new System.Drawing.Size(78, 21);
            this.radAgvlOnline.TabIndex = 1;
            this.radAgvlOnline.Text = "Connect";
            this.radAgvlOnline.UseVisualStyleBackColor = true;
            this.radAgvlOnline.CheckedChanged += new System.EventHandler(this.radAgvlOnline_CheckedChanged);
            // 
            // radAgvlOffline
            // 
            this.radAgvlOffline.AutoSize = true;
            this.radAgvlOffline.Checked = true;
            this.radAgvlOffline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radAgvlOffline.Location = new System.Drawing.Point(3, 24);
            this.radAgvlOffline.Name = "radAgvlOffline";
            this.radAgvlOffline.Size = new System.Drawing.Size(98, 21);
            this.radAgvlOffline.TabIndex = 0;
            this.radAgvlOffline.TabStop = true;
            this.radAgvlOffline.Text = "DisConnect";
            this.radAgvlOffline.UseVisualStyleBackColor = true;
            this.radAgvlOffline.CheckedChanged += new System.EventHandler(this.radAgvlOffline_CheckedChanged);
            // 
            // txtCanAuto
            // 
            this.txtCanAuto.BackColor = System.Drawing.Color.LightGreen;
            this.txtCanAuto.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtCanAuto.Location = new System.Drawing.Point(221, 101);
            this.txtCanAuto.Name = "txtCanAuto";
            this.txtCanAuto.Size = new System.Drawing.Size(209, 80);
            this.txtCanAuto.TabIndex = 57;
            this.txtCanAuto.Text = "可以 Auto";
            this.txtCanAuto.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnAutoManual
            // 
            this.btnAutoManual.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAutoManual.Location = new System.Drawing.Point(6, 104);
            this.btnAutoManual.Name = "btnAutoManual";
            this.btnAutoManual.Size = new System.Drawing.Size(209, 77);
            this.btnAutoManual.TabIndex = 53;
            this.btnAutoManual.Text = "Auto/Manual";
            this.btnAutoManual.UseVisualStyleBackColor = true;
            this.btnAutoManual.Click += new System.EventHandler(this.btnAutoManual_Click);
            // 
            // pageMoveState
            // 
            this.pageMoveState.Controls.Add(this.btnRefreshMoveState);
            this.pageMoveState.Controls.Add(this.gbVehicleLocation);
            this.pageMoveState.Controls.Add(this.ucMoveMovingIndex);
            this.pageMoveState.Controls.Add(this.ucMoveMoveState);
            this.pageMoveState.Controls.Add(this.ucMovePauseStop);
            this.pageMoveState.Controls.Add(this.ucMoveReserveStop);
            this.pageMoveState.Controls.Add(this.ucMoveLastAddress);
            this.pageMoveState.Controls.Add(this.ucMoveIsMoveEnd);
            this.pageMoveState.Controls.Add(this.ucMoveLastSection);
            this.pageMoveState.Controls.Add(this.ucMovePositionY);
            this.pageMoveState.Controls.Add(this.ucMovePositionX);
            this.pageMoveState.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageMoveState.Location = new System.Drawing.Point(4, 22);
            this.pageMoveState.Name = "pageMoveState";
            this.pageMoveState.Padding = new System.Windows.Forms.Padding(3);
            this.pageMoveState.Size = new System.Drawing.Size(607, 683);
            this.pageMoveState.TabIndex = 0;
            this.pageMoveState.Text = "Move";
            this.pageMoveState.UseVisualStyleBackColor = true;
            // 
            // btnRefreshMoveState
            // 
            this.btnRefreshMoveState.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshMoveState.Location = new System.Drawing.Point(221, 335);
            this.btnRefreshMoveState.Name = "btnRefreshMoveState";
            this.btnRefreshMoveState.Size = new System.Drawing.Size(135, 92);
            this.btnRefreshMoveState.TabIndex = 42;
            this.btnRefreshMoveState.Text = "更新走行狀態";
            this.btnRefreshMoveState.UseVisualStyleBackColor = true;
            this.btnRefreshMoveState.Click += new System.EventHandler(this.btnRefreshMoveState_Click);
            // 
            // gbVehicleLocation
            // 
            this.gbVehicleLocation.Controls.Add(this.numPositionY);
            this.gbVehicleLocation.Controls.Add(this.numPositionX);
            this.gbVehicleLocation.Controls.Add(this.btnKeyInPosition);
            this.gbVehicleLocation.Location = new System.Drawing.Point(6, 335);
            this.gbVehicleLocation.Name = "gbVehicleLocation";
            this.gbVehicleLocation.Size = new System.Drawing.Size(209, 93);
            this.gbVehicleLocation.TabIndex = 1;
            this.gbVehicleLocation.TabStop = false;
            this.gbVehicleLocation.Text = "Vehicle Location";
            // 
            // numPositionY
            // 
            this.numPositionY.Location = new System.Drawing.Point(104, 19);
            this.numPositionY.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numPositionY.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numPositionY.Name = "numPositionY";
            this.numPositionY.Size = new System.Drawing.Size(94, 29);
            this.numPositionY.TabIndex = 41;
            // 
            // numPositionX
            // 
            this.numPositionX.Location = new System.Drawing.Point(6, 19);
            this.numPositionX.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numPositionX.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.numPositionX.Name = "numPositionX";
            this.numPositionX.Size = new System.Drawing.Size(92, 29);
            this.numPositionX.TabIndex = 41;
            // 
            // btnKeyInPosition
            // 
            this.btnKeyInPosition.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnKeyInPosition.Location = new System.Drawing.Point(6, 47);
            this.btnKeyInPosition.Name = "btnKeyInPosition";
            this.btnKeyInPosition.Size = new System.Drawing.Size(192, 27);
            this.btnKeyInPosition.TabIndex = 40;
            this.btnKeyInPosition.Text = "鍵入車輛位置";
            this.btnKeyInPosition.UseVisualStyleBackColor = true;
            this.btnKeyInPosition.Click += new System.EventHandler(this.btnKeyInPosition_Click);
            // 
            // ucMoveMovingIndex
            // 
            this.ucMoveMovingIndex.Location = new System.Drawing.Point(285, 152);
            this.ucMoveMovingIndex.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveMovingIndex.Name = "ucMoveMovingIndex";
            this.ucMoveMovingIndex.Size = new System.Drawing.Size(135, 65);
            this.ucMoveMovingIndex.TabIndex = 0;
            this.ucMoveMovingIndex.TagColor = System.Drawing.Color.Black;
            this.ucMoveMovingIndex.TagName = "MovingIndex";
            this.ucMoveMovingIndex.TagValue = "0";
            // 
            // ucMoveMoveState
            // 
            this.ucMoveMoveState.Location = new System.Drawing.Point(144, 10);
            this.ucMoveMoveState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveMoveState.Name = "ucMoveMoveState";
            this.ucMoveMoveState.Size = new System.Drawing.Size(135, 65);
            this.ucMoveMoveState.TabIndex = 0;
            this.ucMoveMoveState.TagColor = System.Drawing.Color.Black;
            this.ucMoveMoveState.TagName = "MoveState";
            this.ucMoveMoveState.TagValue = "Idle";
            // 
            // ucMovePauseStop
            // 
            this.ucMovePauseStop.Location = new System.Drawing.Point(285, 81);
            this.ucMovePauseStop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePauseStop.Name = "ucMovePauseStop";
            this.ucMovePauseStop.Size = new System.Drawing.Size(135, 65);
            this.ucMovePauseStop.TabIndex = 0;
            this.ucMovePauseStop.TagColor = System.Drawing.Color.Black;
            this.ucMovePauseStop.TagName = "Pause Stop";
            this.ucMovePauseStop.TagValue = "Off";
            // 
            // ucMoveReserveStop
            // 
            this.ucMoveReserveStop.Location = new System.Drawing.Point(285, 10);
            this.ucMoveReserveStop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveReserveStop.Name = "ucMoveReserveStop";
            this.ucMoveReserveStop.Size = new System.Drawing.Size(135, 65);
            this.ucMoveReserveStop.TabIndex = 0;
            this.ucMoveReserveStop.TagColor = System.Drawing.Color.Black;
            this.ucMoveReserveStop.TagName = "Reserve Stop";
            this.ucMoveReserveStop.TagValue = "Off";
            // 
            // ucMoveLastAddress
            // 
            this.ucMoveLastAddress.Location = new System.Drawing.Point(3, 81);
            this.ucMoveLastAddress.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveLastAddress.Name = "ucMoveLastAddress";
            this.ucMoveLastAddress.Size = new System.Drawing.Size(135, 65);
            this.ucMoveLastAddress.TabIndex = 0;
            this.ucMoveLastAddress.TagColor = System.Drawing.Color.Black;
            this.ucMoveLastAddress.TagName = "Last Address";
            this.ucMoveLastAddress.TagValue = "10001";
            // 
            // ucMoveIsMoveEnd
            // 
            this.ucMoveIsMoveEnd.Location = new System.Drawing.Point(144, 81);
            this.ucMoveIsMoveEnd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveIsMoveEnd.Name = "ucMoveIsMoveEnd";
            this.ucMoveIsMoveEnd.Size = new System.Drawing.Size(135, 65);
            this.ucMoveIsMoveEnd.TabIndex = 0;
            this.ucMoveIsMoveEnd.TagColor = System.Drawing.Color.Black;
            this.ucMoveIsMoveEnd.TagName = "Is Move End";
            this.ucMoveIsMoveEnd.TagValue = "True";
            // 
            // ucMoveLastSection
            // 
            this.ucMoveLastSection.Location = new System.Drawing.Point(3, 10);
            this.ucMoveLastSection.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveLastSection.Name = "ucMoveLastSection";
            this.ucMoveLastSection.Size = new System.Drawing.Size(135, 65);
            this.ucMoveLastSection.TabIndex = 0;
            this.ucMoveLastSection.TagColor = System.Drawing.Color.Black;
            this.ucMoveLastSection.TagName = "Last Section";
            this.ucMoveLastSection.TagValue = "00101";
            // 
            // ucMovePositionY
            // 
            this.ucMovePositionY.Location = new System.Drawing.Point(3, 234);
            this.ucMovePositionY.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePositionY.Name = "ucMovePositionY";
            this.ucMovePositionY.Size = new System.Drawing.Size(135, 65);
            this.ucMovePositionY.TabIndex = 0;
            this.ucMovePositionY.TagColor = System.Drawing.Color.Black;
            this.ucMovePositionY.TagName = "Y";
            this.ucMovePositionY.TagValue = "-13579";
            // 
            // ucMovePositionX
            // 
            this.ucMovePositionX.Location = new System.Drawing.Point(3, 152);
            this.ucMovePositionX.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePositionX.Name = "ucMovePositionX";
            this.ucMovePositionX.Size = new System.Drawing.Size(135, 65);
            this.ucMovePositionX.TabIndex = 0;
            this.ucMovePositionX.TagColor = System.Drawing.Color.Black;
            this.ucMovePositionX.TagName = "X";
            this.ucMovePositionX.TagValue = "123456";
            // 
            // pageRobotSate
            // 
            this.pageRobotSate.Controls.Add(this.btnRefreshRobotState);
            this.pageRobotSate.Controls.Add(this.ucRobotIsHome);
            this.pageRobotSate.Controls.Add(this.ucRobotSlotRId);
            this.pageRobotSate.Controls.Add(this.ucRobotSlotRState);
            this.pageRobotSate.Controls.Add(this.ucRobotSlotLState);
            this.pageRobotSate.Controls.Add(this.ucRobotSlotLId);
            this.pageRobotSate.Controls.Add(this.ucRobotRobotState);
            this.pageRobotSate.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageRobotSate.Location = new System.Drawing.Point(4, 22);
            this.pageRobotSate.Name = "pageRobotSate";
            this.pageRobotSate.Padding = new System.Windows.Forms.Padding(3);
            this.pageRobotSate.Size = new System.Drawing.Size(607, 683);
            this.pageRobotSate.TabIndex = 1;
            this.pageRobotSate.Text = "Robot";
            this.pageRobotSate.UseVisualStyleBackColor = true;
            // 
            // btnRefreshRobotState
            // 
            this.btnRefreshRobotState.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshRobotState.Location = new System.Drawing.Point(3, 180);
            this.btnRefreshRobotState.Name = "btnRefreshRobotState";
            this.btnRefreshRobotState.Size = new System.Drawing.Size(135, 65);
            this.btnRefreshRobotState.TabIndex = 43;
            this.btnRefreshRobotState.Text = "更新手臂狀態";
            this.btnRefreshRobotState.UseVisualStyleBackColor = true;
            this.btnRefreshRobotState.Click += new System.EventHandler(this.btnRefreshRobotState_Click);
            // 
            // ucRobotIsHome
            // 
            this.ucRobotIsHome.Location = new System.Drawing.Point(3, 88);
            this.ucRobotIsHome.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotIsHome.Name = "ucRobotIsHome";
            this.ucRobotIsHome.Size = new System.Drawing.Size(135, 65);
            this.ucRobotIsHome.TabIndex = 1;
            this.ucRobotIsHome.TagColor = System.Drawing.Color.Black;
            this.ucRobotIsHome.TagName = "Is Home";
            this.ucRobotIsHome.TagValue = "false";
            // 
            // ucRobotSlotRId
            // 
            this.ucRobotSlotRId.Location = new System.Drawing.Point(298, 88);
            this.ucRobotSlotRId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotRId.Name = "ucRobotSlotRId";
            this.ucRobotSlotRId.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotRId.TabIndex = 1;
            this.ucRobotSlotRId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRId.TagName = "Slot R Id";
            this.ucRobotSlotRId.TagValue = "PQR";
            // 
            // ucRobotSlotRState
            // 
            this.ucRobotSlotRState.Location = new System.Drawing.Point(298, 6);
            this.ucRobotSlotRState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotRState.Name = "ucRobotSlotRState";
            this.ucRobotSlotRState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotRState.TabIndex = 1;
            this.ucRobotSlotRState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRState.TagName = "Slot R State";
            this.ucRobotSlotRState.TagValue = "Empty";
            // 
            // ucRobotSlotLState
            // 
            this.ucRobotSlotLState.Location = new System.Drawing.Point(147, 6);
            this.ucRobotSlotLState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotLState.Name = "ucRobotSlotLState";
            this.ucRobotSlotLState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotLState.TabIndex = 1;
            this.ucRobotSlotLState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLState.TagName = "Slot L State";
            this.ucRobotSlotLState.TagValue = "Empty";
            // 
            // ucRobotSlotLId
            // 
            this.ucRobotSlotLId.Location = new System.Drawing.Point(147, 88);
            this.ucRobotSlotLId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotLId.Name = "ucRobotSlotLId";
            this.ucRobotSlotLId.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotLId.TabIndex = 1;
            this.ucRobotSlotLId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLId.TagName = "Slot L Id";
            this.ucRobotSlotLId.TagValue = "ABC";
            // 
            // ucRobotRobotState
            // 
            this.ucRobotRobotState.Location = new System.Drawing.Point(6, 6);
            this.ucRobotRobotState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotRobotState.Name = "ucRobotRobotState";
            this.ucRobotRobotState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotRobotState.TabIndex = 1;
            this.ucRobotRobotState.TagColor = System.Drawing.Color.Black;
            this.ucRobotRobotState.TagName = "Robot State";
            this.ucRobotRobotState.TagValue = "Idle";
            // 
            // pageBatteryState
            // 
            this.pageBatteryState.Controls.Add(this.btnRefreshBatteryState);
            this.pageBatteryState.Controls.Add(this.ucBatteryCharging);
            this.pageBatteryState.Controls.Add(this.ucBatteryTemperature);
            this.pageBatteryState.Controls.Add(this.ucBatteryVoltage);
            this.pageBatteryState.Controls.Add(this.ucBatteryPercentage);
            this.pageBatteryState.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageBatteryState.Location = new System.Drawing.Point(4, 22);
            this.pageBatteryState.Name = "pageBatteryState";
            this.pageBatteryState.Size = new System.Drawing.Size(607, 683);
            this.pageBatteryState.TabIndex = 2;
            this.pageBatteryState.Text = "Battery";
            this.pageBatteryState.UseVisualStyleBackColor = true;
            // 
            // btnRefreshBatteryState
            // 
            this.btnRefreshBatteryState.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshBatteryState.Location = new System.Drawing.Point(3, 353);
            this.btnRefreshBatteryState.Name = "btnRefreshBatteryState";
            this.btnRefreshBatteryState.Size = new System.Drawing.Size(135, 65);
            this.btnRefreshBatteryState.TabIndex = 41;
            this.btnRefreshBatteryState.Text = "更新電池狀態";
            this.btnRefreshBatteryState.UseVisualStyleBackColor = true;
            this.btnRefreshBatteryState.Click += new System.EventHandler(this.AseRobotControlForm_RefreshBatteryState);
            // 
            // ucBatteryCharging
            // 
            this.ucBatteryCharging.Location = new System.Drawing.Point(3, 258);
            this.ucBatteryCharging.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryCharging.Name = "ucBatteryCharging";
            this.ucBatteryCharging.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryCharging.TabIndex = 5;
            this.ucBatteryCharging.TagColor = System.Drawing.Color.Black;
            this.ucBatteryCharging.TagName = "Charging";
            this.ucBatteryCharging.TagValue = "false";
            // 
            // ucBatteryTemperature
            // 
            this.ucBatteryTemperature.Location = new System.Drawing.Point(3, 169);
            this.ucBatteryTemperature.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryTemperature.Name = "ucBatteryTemperature";
            this.ucBatteryTemperature.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryTemperature.TabIndex = 6;
            this.ucBatteryTemperature.TagColor = System.Drawing.Color.Black;
            this.ucBatteryTemperature.TagName = "Temperature";
            this.ucBatteryTemperature.TagValue = "40.5";
            // 
            // ucBatteryVoltage
            // 
            this.ucBatteryVoltage.Location = new System.Drawing.Point(3, 84);
            this.ucBatteryVoltage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryVoltage.Name = "ucBatteryVoltage";
            this.ucBatteryVoltage.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryVoltage.TabIndex = 7;
            this.ucBatteryVoltage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryVoltage.TagName = "Voltage";
            this.ucBatteryVoltage.TagValue = "55.66";
            // 
            // ucBatteryPercentage
            // 
            this.ucBatteryPercentage.Location = new System.Drawing.Point(3, 13);
            this.ucBatteryPercentage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryPercentage.Name = "ucBatteryPercentage";
            this.ucBatteryPercentage.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryPercentage.TabIndex = 3;
            this.ucBatteryPercentage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryPercentage.TagName = "Percentage";
            this.ucBatteryPercentage.TagValue = " 70.0";
            // 
            // pageVehicleState
            // 
            this.pageVehicleState.Controls.Add(this.groupBox3);
            this.pageVehicleState.Controls.Add(this.ucGoNextStep);
            this.pageVehicleState.Controls.Add(this.ucTransferStepType);
            this.pageVehicleState.Controls.Add(this.ucTransferSteps);
            this.pageVehicleState.Controls.Add(this.ucTransferIndex);
            this.pageVehicleState.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageVehicleState.Location = new System.Drawing.Point(4, 22);
            this.pageVehicleState.Name = "pageVehicleState";
            this.pageVehicleState.Size = new System.Drawing.Size(607, 683);
            this.pageVehicleState.TabIndex = 3;
            this.pageVehicleState.Text = "Vehicle";
            this.pageVehicleState.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtBatterysAbnormal);
            this.groupBox3.Controls.Add(this.txtMainFlowAbnormal);
            this.groupBox3.Controls.Add(this.txtAgvcConnectorAbnormal);
            this.groupBox3.Controls.Add(this.txtRobotAbnormal);
            this.groupBox3.Controls.Add(this.txtMoveControlAbnormal);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(140, 181);
            this.groupBox3.TabIndex = 62;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "IsAbnormal";
            // 
            // txtBatterysAbnormal
            // 
            this.txtBatterysAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtBatterysAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtBatterysAbnormal.Location = new System.Drawing.Point(6, 142);
            this.txtBatterysAbnormal.Name = "txtBatterysAbnormal";
            this.txtBatterysAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtBatterysAbnormal.TabIndex = 4;
            this.txtBatterysAbnormal.Text = "電池";
            this.txtBatterysAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtMainFlowAbnormal
            // 
            this.txtMainFlowAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtMainFlowAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMainFlowAbnormal.Location = new System.Drawing.Point(6, 18);
            this.txtMainFlowAbnormal.Name = "txtMainFlowAbnormal";
            this.txtMainFlowAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtMainFlowAbnormal.TabIndex = 1;
            this.txtMainFlowAbnormal.Text = "流程";
            this.txtMainFlowAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtAgvcConnectorAbnormal
            // 
            this.txtAgvcConnectorAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtAgvcConnectorAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAgvcConnectorAbnormal.Location = new System.Drawing.Point(6, 49);
            this.txtAgvcConnectorAbnormal.Name = "txtAgvcConnectorAbnormal";
            this.txtAgvcConnectorAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtAgvcConnectorAbnormal.TabIndex = 3;
            this.txtAgvcConnectorAbnormal.Text = "通訊";
            this.txtAgvcConnectorAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRobotAbnormal
            // 
            this.txtRobotAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtRobotAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtRobotAbnormal.Location = new System.Drawing.Point(6, 111);
            this.txtRobotAbnormal.Name = "txtRobotAbnormal";
            this.txtRobotAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtRobotAbnormal.TabIndex = 2;
            this.txtRobotAbnormal.Text = "手臂";
            this.txtRobotAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtMoveControlAbnormal
            // 
            this.txtMoveControlAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtMoveControlAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMoveControlAbnormal.Location = new System.Drawing.Point(6, 80);
            this.txtMoveControlAbnormal.Name = "txtMoveControlAbnormal";
            this.txtMoveControlAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtMoveControlAbnormal.TabIndex = 0;
            this.txtMoveControlAbnormal.Text = "走行";
            this.txtMoveControlAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucGoNextStep
            // 
            this.ucGoNextStep.Location = new System.Drawing.Point(149, 154);
            this.ucGoNextStep.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucGoNextStep.Name = "ucGoNextStep";
            this.ucGoNextStep.Size = new System.Drawing.Size(135, 65);
            this.ucGoNextStep.TabIndex = 1;
            this.ucGoNextStep.TagColor = System.Drawing.Color.Black;
            this.ucGoNextStep.TagName = "GoNext";
            this.ucGoNextStep.TagValue = "-1";
            // 
            // ucTransferStepType
            // 
            this.ucTransferStepType.Location = new System.Drawing.Point(149, 10);
            this.ucTransferStepType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucTransferStepType.Name = "ucTransferStepType";
            this.ucTransferStepType.Size = new System.Drawing.Size(135, 65);
            this.ucTransferStepType.TabIndex = 1;
            this.ucTransferStepType.TagColor = System.Drawing.Color.Black;
            this.ucTransferStepType.TagName = "TransType";
            this.ucTransferStepType.TagValue = "None";
            // 
            // ucTransferSteps
            // 
            this.ucTransferSteps.Location = new System.Drawing.Point(149, 83);
            this.ucTransferSteps.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucTransferSteps.Name = "ucTransferSteps";
            this.ucTransferSteps.Size = new System.Drawing.Size(135, 65);
            this.ucTransferSteps.TabIndex = 1;
            this.ucTransferSteps.TagColor = System.Drawing.Color.Black;
            this.ucTransferSteps.TagName = "TransferSteps";
            this.ucTransferSteps.TagValue = "-1";
            // 
            // ucTransferIndex
            // 
            this.ucTransferIndex.Location = new System.Drawing.Point(149, 225);
            this.ucTransferIndex.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucTransferIndex.Name = "ucTransferIndex";
            this.ucTransferIndex.Size = new System.Drawing.Size(135, 65);
            this.ucTransferIndex.TabIndex = 1;
            this.ucTransferIndex.TagColor = System.Drawing.Color.Black;
            this.ucTransferIndex.TagName = "TransIndex";
            this.ucTransferIndex.TagValue = "-1";
            // 
            // pageReserveInfo
            // 
            this.pageReserveInfo.Controls.Add(this.gbReserve);
            this.pageReserveInfo.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageReserveInfo.Location = new System.Drawing.Point(4, 22);
            this.pageReserveInfo.Name = "pageReserveInfo";
            this.pageReserveInfo.Padding = new System.Windows.Forms.Padding(3);
            this.pageReserveInfo.Size = new System.Drawing.Size(607, 683);
            this.pageReserveInfo.TabIndex = 4;
            this.pageReserveInfo.Text = "Reserve";
            this.pageReserveInfo.UseVisualStyleBackColor = true;
            // 
            // gbReserve
            // 
            this.gbReserve.Controls.Add(this.lbxReserveOkSections);
            this.gbReserve.Controls.Add(this.lbxNeedReserveSections);
            this.gbReserve.Location = new System.Drawing.Point(6, 9);
            this.gbReserve.Name = "gbReserve";
            this.gbReserve.Size = new System.Drawing.Size(489, 404);
            this.gbReserve.TabIndex = 49;
            this.gbReserve.TabStop = false;
            this.gbReserve.Text = "Reserve";
            // 
            // lbxReserveOkSections
            // 
            this.lbxReserveOkSections.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbxReserveOkSections.FormattingEnabled = true;
            this.lbxReserveOkSections.ItemHeight = 19;
            this.lbxReserveOkSections.Location = new System.Drawing.Point(238, 21);
            this.lbxReserveOkSections.Name = "lbxReserveOkSections";
            this.lbxReserveOkSections.ScrollAlwaysVisible = true;
            this.lbxReserveOkSections.Size = new System.Drawing.Size(235, 365);
            this.lbxReserveOkSections.TabIndex = 42;
            // 
            // lbxNeedReserveSections
            // 
            this.lbxNeedReserveSections.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbxNeedReserveSections.FormattingEnabled = true;
            this.lbxNeedReserveSections.ItemHeight = 19;
            this.lbxNeedReserveSections.Location = new System.Drawing.Point(6, 21);
            this.lbxNeedReserveSections.Name = "lbxNeedReserveSections";
            this.lbxNeedReserveSections.ScrollAlwaysVisible = true;
            this.lbxNeedReserveSections.Size = new System.Drawing.Size(215, 365);
            this.lbxNeedReserveSections.TabIndex = 41;
            // 
            // pageTransferCommand
            // 
            this.pageTransferCommand.Controls.Add(this.groupBox4);
            this.pageTransferCommand.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageTransferCommand.Location = new System.Drawing.Point(4, 22);
            this.pageTransferCommand.Name = "pageTransferCommand";
            this.pageTransferCommand.Size = new System.Drawing.Size(607, 683);
            this.pageTransferCommand.TabIndex = 5;
            this.pageTransferCommand.Text = "TransferCmd";
            this.pageTransferCommand.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tbxTransferStepMsg);
            this.groupBox4.Controls.Add(this.tbxTransferCommandMsg);
            this.groupBox4.Location = new System.Drawing.Point(3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(757, 459);
            this.groupBox4.TabIndex = 60;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Transfer Command";
            // 
            // tbxTransferStepMsg
            // 
            this.tbxTransferStepMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxTransferStepMsg.Location = new System.Drawing.Point(427, 13);
            this.tbxTransferStepMsg.Multiline = true;
            this.tbxTransferStepMsg.Name = "tbxTransferStepMsg";
            this.tbxTransferStepMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxTransferStepMsg.Size = new System.Drawing.Size(324, 247);
            this.tbxTransferStepMsg.TabIndex = 60;
            // 
            // tbxTransferCommandMsg
            // 
            this.tbxTransferCommandMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxTransferCommandMsg.Location = new System.Drawing.Point(6, 23);
            this.tbxTransferCommandMsg.Multiline = true;
            this.tbxTransferCommandMsg.Name = "tbxTransferCommandMsg";
            this.tbxTransferCommandMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxTransferCommandMsg.Size = new System.Drawing.Size(415, 247);
            this.tbxTransferCommandMsg.TabIndex = 59;
            // 
            // timeUpdateUI
            // 
            this.timeUpdateUI.Enabled = true;
            this.timeUpdateUI.Interval = 250;
            this.timeUpdateUI.Tick += new System.EventHandler(this.timeUpdateUI_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 739);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1375, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tspbCommding
            // 
            this.tspbCommding.Name = "tspbCommding";
            this.tspbCommding.Size = new System.Drawing.Size(100, 16);
            // 
            // tstextClientName
            // 
            this.tstextClientName.Name = "tstextClientName";
            this.tstextClientName.Size = new System.Drawing.Size(104, 17);
            this.tstextClientName.Text = "tstextClientName";
            // 
            // tstextRemoteIp
            // 
            this.tstextRemoteIp.Name = "tstextRemoteIp";
            this.tstextRemoteIp.Size = new System.Drawing.Size(93, 17);
            this.tstextRemoteIp.Text = "tstextRemoteIp";
            // 
            // tstextRemotePort
            // 
            this.tstextRemotePort.Name = "tstextRemotePort";
            this.tstextRemotePort.Size = new System.Drawing.Size(105, 17);
            this.tstextRemotePort.Text = "tstextRemotePort";
            // 
            // tstextLastPosX
            // 
            this.tstextLastPosX.Name = "tstextLastPosX";
            this.tstextLastPosX.Size = new System.Drawing.Size(90, 17);
            this.tstextLastPosX.Text = "tstextRealPosX";
            // 
            // tstextLastPosY
            // 
            this.tstextLastPosY.Name = "tstextLastPosY";
            this.tstextLastPosY.Size = new System.Drawing.Size(89, 17);
            this.tstextLastPosY.Text = "tstextRealPosY";
            // 
            // timer_SetupInitialSoc
            // 
            this.timer_SetupInitialSoc.Interval = 50;
            this.timer_SetupInitialSoc.Tick += new System.EventHandler(this.timer_SetupInitialSoc_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1375, 761);
            this.ControlBox = false;
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.pageBasicState.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.gbPerformanceCounter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numSoc)).EndInit();
            this.gbConnection.ResumeLayout(false);
            this.gbConnection.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.pageMoveState.ResumeLayout(false);
            this.gbVehicleLocation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).EndInit();
            this.pageRobotSate.ResumeLayout(false);
            this.pageBatteryState.ResumeLayout(false);
            this.pageVehicleState.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.pageReserveInfo.ResumeLayout(false);
            this.gbReserve.ResumeLayout(false);
            this.pageTransferCommand.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 系統ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 語言ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 模式ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 工程師ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 啟動ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 登入ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 登出ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 關閉ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 中文ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AlarmPage;
        private System.Windows.Forms.ToolStripMenuItem AgvcConnectorPage;
        private System.Windows.Forms.ToolStripMenuItem VehicleStatusPage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem MovePage;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.GroupBox gbVehicleLocation;
        private System.Windows.Forms.GroupBox gbConnection;
        private System.Windows.Forms.RadioButton radAgvcOnline;
        private System.Windows.Forms.RadioButton radAgvcOffline;
        private System.Windows.Forms.Button btnBuzzOff;
        private System.Windows.Forms.Button btnAlarmReset;
        private System.Windows.Forms.Button btnKeyInPosition;
        private System.Windows.Forms.ListBox lbxReserveOkSections;
        private System.Windows.Forms.ListBox lbxNeedReserveSections;
        private System.Windows.Forms.Timer timeUpdateUI;
        private System.Windows.Forms.NumericUpDown numPositionY;
        private System.Windows.Forms.NumericUpDown numPositionX;
        private System.Windows.Forms.GroupBox gbPerformanceCounter;
        private System.Windows.Forms.ToolStripMenuItem RobotAndChargePage;
        private System.Windows.Forms.Label txtLastAlarm;
        private UcLabelTextBox ucSoc;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnAutoManual;
        private System.Windows.Forms.Label txtTransferStep;
        private System.Windows.Forms.GroupBox gbReserve;
        private System.Windows.Forms.Label txtTrackPosition;
        private System.Windows.Forms.Label txtAskingReserve;
        private System.Windows.Forms.Label txtWatchLowPower;
        private System.Windows.Forms.NumericUpDown numSoc;
        private System.Windows.Forms.Button btnKeyInSoc;
        private UcLabelTextBox ucCharging;
        private System.Windows.Forms.Label txtAgvcConnection;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar tspbCommding;
        private System.Windows.Forms.ToolStripStatusLabel tstextClientName;
        private System.Windows.Forms.ToolStripStatusLabel tstextRemoteIp;
        private System.Windows.Forms.ToolStripStatusLabel tstextRemotePort;
        private System.Windows.Forms.ToolStripStatusLabel tstextLastPosX;
        private System.Windows.Forms.ToolStripStatusLabel tstextLastPosY;
        private UcLabelTextBox ucLCstId;
        private System.Windows.Forms.Timer timer_SetupInitialSoc;
        private System.Windows.Forms.ToolStripMenuItem 模擬測試ToolStripMenuItem;
        private System.Windows.Forms.Label txtCannotAutoReason;
        private System.Windows.Forms.Label txtCanAuto;
        private System.Windows.Forms.TextBox tbxDebugLogMsg;
        private System.Windows.Forms.TextBox tbxTransferCommandMsg;
        private System.Windows.Forms.TextBox tbxTransferStepMsg;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label txtBatterysAbnormal;
        private System.Windows.Forms.Label txtMainFlowAbnormal;
        private System.Windows.Forms.Label txtAgvcConnectorAbnormal;
        private System.Windows.Forms.Label txtRobotAbnormal;
        private System.Windows.Forms.Label txtMoveControlAbnormal;
        private System.Windows.Forms.Button btnPrintScreen;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label txtAgvlConnection;
        private System.Windows.Forms.RadioButton radAgvlOnline;
        private System.Windows.Forms.RadioButton radAgvlOffline;
        private System.Windows.Forms.Button btnRefreshPosition;
        private System.Windows.Forms.ToolStripMenuItem AgvlConnectorPage;
        private UcLabelTextBox ucRCstId;
        private UcLabelTextBox ucRobotHome;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pageMoveState;
        private System.Windows.Forms.TabPage pageRobotSate;
        private System.Windows.Forms.TabPage pageBatteryState;
        private System.Windows.Forms.TabPage pageVehicleState;
        private UcVerticalLabelText ucBatteryCharging;
        private UcVerticalLabelText ucBatteryTemperature;
        private UcVerticalLabelText ucBatteryVoltage;
        private UcVerticalLabelText ucBatteryPercentage;
        private System.Windows.Forms.Button btnRefreshBatteryState;
        private UcVerticalLabelText ucMoveLastAddress;
        private UcVerticalLabelText ucMoveLastSection;
        private UcVerticalLabelText ucMovePositionY;
        private UcVerticalLabelText ucMovePositionX;
        private System.Windows.Forms.Button btnRefreshMoveState;
        private UcVerticalLabelText ucMoveIsMoveEnd;
        private UcVerticalLabelText ucMoveReserveStop;
        private UcVerticalLabelText ucMoveMovingIndex;
        private UcVerticalLabelText ucMovePauseStop;
        private UcVerticalLabelText ucRobotIsHome;
        private UcVerticalLabelText ucRobotRobotState;
        private UcVerticalLabelText ucRobotSlotRId;
        private UcVerticalLabelText ucRobotSlotLId;
        private UcVerticalLabelText ucRobotSlotRState;
        private UcVerticalLabelText ucRobotSlotLState;
        private System.Windows.Forms.Button btnRefreshRobotState;
        private UcVerticalLabelText ucGoNextStep;
        private UcVerticalLabelText ucTransferStepType;
        private UcVerticalLabelText ucTransferSteps;
        private UcVerticalLabelText ucTransferIndex;
        private UcVerticalLabelText ucMoveMoveState;
        private System.Windows.Forms.TabPage pageBasicState;
        private System.Windows.Forms.TabPage pageReserveInfo;
        private System.Windows.Forms.TabPage pageTransferCommand;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}