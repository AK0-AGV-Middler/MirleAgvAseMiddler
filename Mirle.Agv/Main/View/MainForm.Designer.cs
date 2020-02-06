namespace Mirle.Agv.View
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
            this.MiddlerPage = new System.Windows.Forms.ToolStripMenuItem();
            this.IntegrateCommandPage = new System.Windows.Forms.ToolStripMenuItem();
            this.ManualMoveCmdPage = new System.Windows.Forms.ToolStripMenuItem();
            this.JogPage = new System.Windows.Forms.ToolStripMenuItem();
            this.工程師ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.模擬測試ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnPrintScreen = new System.Windows.Forms.Button();
            this.tbxDebugLogMsg = new System.Windows.Forms.TextBox();
            this.btnReDraw = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ckAddress = new System.Windows.Forms.CheckBox();
            this.ckSection = new System.Windows.Forms.CheckBox();
            this.ckBarcode = new System.Windows.Forms.CheckBox();
            this.btnYflip = new System.Windows.Forms.Button();
            this.btnXflip = new System.Windows.Forms.Button();
            this.txtResizePercent = new System.Windows.Forms.TextBox();
            this.txtRotateAngle = new System.Windows.Forms.TextBox();
            this.btnRotate = new System.Windows.Forms.Button();
            this.btnResizePercent = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.btnSwitchBarcodeLine = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tbxTransferStepMsg = new System.Windows.Forms.TextBox();
            this.tbxTransferCommandMsg = new System.Windows.Forms.TextBox();
            this.txtCannotAutoReason = new System.Windows.Forms.Label();
            this.txtCanAuto = new System.Windows.Forms.Label();
            this.gbPerformanceCounter = new System.Windows.Forms.GroupBox();
            this.ucCstId = new Mirle.Agv.UcLabelTextBox();
            this.btnKeyInSoc = new System.Windows.Forms.Button();
            this.ucCharging = new Mirle.Agv.UcLabelTextBox();
            this.numSoc = new System.Windows.Forms.NumericUpDown();
            this.ucSoc = new Mirle.Agv.UcLabelTextBox();
            this.ucLoading = new Mirle.Agv.UcLabelTextBox();
            this.ucPerformanceCounterRam = new Mirle.Agv.UcLabelTextBox();
            this.ucPerformanceCounterCpu = new Mirle.Agv.UcLabelTextBox();
            this.gbReserve = new System.Windows.Forms.GroupBox();
            this.lbxReserveOkSections = new System.Windows.Forms.ListBox();
            this.lbxNeedReserveSections = new System.Windows.Forms.ListBox();
            this.btnAutoManual = new System.Windows.Forms.Button();
            this.txtLastAlarm = new System.Windows.Forms.Label();
            this.btnBuzzOff = new System.Windows.Forms.Button();
            this.btnAlarmReset = new System.Windows.Forms.Button();
            this.gbVehicleLocation = new System.Windows.Forms.GroupBox();
            this.numPositionY = new System.Windows.Forms.NumericUpDown();
            this.ucDistance = new Mirle.Agv.UcLabelTextBox();
            this.numPositionX = new System.Windows.Forms.NumericUpDown();
            this.btnKeyInPosition = new System.Windows.Forms.Button();
            this.ucRealPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucBarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.ucMapAddress = new Mirle.Agv.UcLabelTextBox();
            this.ucMapSection = new Mirle.Agv.UcLabelTextBox();
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.txtAgvcConnection = new System.Windows.Forms.Label();
            this.radOnline = new System.Windows.Forms.RadioButton();
            this.radOffline = new System.Windows.Forms.RadioButton();
            this.txtMiddlerAbnormalReason = new System.Windows.Forms.Label();
            this.txtBatterysAbnormalReason = new System.Windows.Forms.Label();
            this.txtRobotAbnormalReason = new System.Windows.Forms.Label();
            this.txtMainFlowAbnormalReason = new System.Windows.Forms.Label();
            this.txtMoveControlAbnormalReason = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtBatterysAbnormal = new System.Windows.Forms.Label();
            this.txtMainFlowAbnormal = new System.Windows.Forms.Label();
            this.txtMiddlerAbnormal = new System.Windows.Forms.Label();
            this.txtRobotAbnormal = new System.Windows.Forms.Label();
            this.txtMoveControlAbnormal = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtWatchLowPower = new System.Windows.Forms.Label();
            this.txtAskingReserve = new System.Windows.Forms.Label();
            this.txtTrackPosition = new System.Windows.Forms.Label();
            this.txtTransferStep = new System.Windows.Forms.Label();
            this.timeUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tspbCommding = new System.Windows.Forms.ToolStripProgressBar();
            this.tstextClientName = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRemoteIp = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRemotePort = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRealPosX = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRealPosY = new System.Windows.Forms.ToolStripStatusLabel();
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
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.gbPerformanceCounter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSoc)).BeginInit();
            this.gbReserve.SuspendLayout();
            this.gbVehicleLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).BeginInit();
            this.gbConnection.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.系統ToolStripMenuItem,
            this.語言ToolStripMenuItem,
            this.模式ToolStripMenuItem,
            this.工程師ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1904, 24);
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
            this.MiddlerPage,
            this.IntegrateCommandPage,
            this.ManualMoveCmdPage,
            this.JogPage});
            this.模式ToolStripMenuItem.Name = "模式ToolStripMenuItem";
            this.模式ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.模式ToolStripMenuItem.Text = "模式";
            // 
            // VehicleStatusPage
            // 
            this.VehicleStatusPage.Name = "VehicleStatusPage";
            this.VehicleStatusPage.Size = new System.Drawing.Size(210, 22);
            this.VehicleStatusPage.Text = "Parameter";
            this.VehicleStatusPage.Click += new System.EventHandler(this.VehicleStatusPage_Click);
            // 
            // AlarmPage
            // 
            this.AlarmPage.Name = "AlarmPage";
            this.AlarmPage.Size = new System.Drawing.Size(210, 22);
            this.AlarmPage.Text = "Alarm";
            this.AlarmPage.Click += new System.EventHandler(this.AlarmPage_Click);
            // 
            // MiddlerPage
            // 
            this.MiddlerPage.Name = "MiddlerPage";
            this.MiddlerPage.Size = new System.Drawing.Size(210, 22);
            this.MiddlerPage.Text = "CommunicationAgvc";
            this.MiddlerPage.Click += new System.EventHandler(this.MiddlerPage_Click);
            // 
            // IntegrateCommandPage
            // 
            this.IntegrateCommandPage.Name = "IntegrateCommandPage";
            this.IntegrateCommandPage.Size = new System.Drawing.Size(210, 22);
            this.IntegrateCommandPage.Text = "IntegrateCommand";
            this.IntegrateCommandPage.Click += new System.EventHandler(this.IntegrateCommandPage_Click);
            // 
            // ManualMoveCmdPage
            // 
            this.ManualMoveCmdPage.Name = "ManualMoveCmdPage";
            this.ManualMoveCmdPage.Size = new System.Drawing.Size(210, 22);
            this.ManualMoveCmdPage.Text = "ManualMoveCommand";
            this.ManualMoveCmdPage.Click += new System.EventHandler(this.ManualMoveCmdPage_Click);
            // 
            // JogPage
            // 
            this.JogPage.Name = "JogPage";
            this.JogPage.Size = new System.Drawing.Size(210, 22);
            this.JogPage.Text = "JogPitch";
            this.JogPage.Click += new System.EventHandler(this.JogPage_Click);
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
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1904, 1017);
            this.splitContainer1.SplitterDistance = 1178;
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
            this.splitContainer3.Panel2.Controls.Add(this.btnPrintScreen);
            this.splitContainer3.Panel2.Controls.Add(this.tbxDebugLogMsg);
            this.splitContainer3.Panel2.Controls.Add(this.btnReDraw);
            this.splitContainer3.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer3.Panel2.Controls.Add(this.btnYflip);
            this.splitContainer3.Panel2.Controls.Add(this.btnXflip);
            this.splitContainer3.Panel2.Controls.Add(this.txtResizePercent);
            this.splitContainer3.Panel2.Controls.Add(this.txtRotateAngle);
            this.splitContainer3.Panel2.Controls.Add(this.btnRotate);
            this.splitContainer3.Panel2.Controls.Add(this.btnResizePercent);
            this.splitContainer3.Panel2.Controls.Add(this.btnReset);
            this.splitContainer3.Panel2.Controls.Add(this.btnSaveImage);
            this.splitContainer3.Panel2.Controls.Add(this.btnSwitchBarcodeLine);
            this.splitContainer3.Size = new System.Drawing.Size(1178, 1017);
            this.splitContainer3.SplitterDistance = 677;
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
            // btnPrintScreen
            // 
            this.btnPrintScreen.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPrintScreen.ForeColor = System.Drawing.Color.OrangeRed;
            this.btnPrintScreen.Location = new System.Drawing.Point(970, 10);
            this.btnPrintScreen.Name = "btnPrintScreen";
            this.btnPrintScreen.Size = new System.Drawing.Size(205, 71);
            this.btnPrintScreen.TabIndex = 59;
            this.btnPrintScreen.Text = "拍照截圖";
            this.btnPrintScreen.UseVisualStyleBackColor = true;
            this.btnPrintScreen.Click += new System.EventHandler(this.btnPrintScreen_Click);
            // 
            // tbxDebugLogMsg
            // 
            this.tbxDebugLogMsg.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxDebugLogMsg.Location = new System.Drawing.Point(3, 94);
            this.tbxDebugLogMsg.Multiline = true;
            this.tbxDebugLogMsg.Name = "tbxDebugLogMsg";
            this.tbxDebugLogMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxDebugLogMsg.Size = new System.Drawing.Size(1172, 217);
            this.tbxDebugLogMsg.TabIndex = 58;
            // 
            // btnReDraw
            // 
            this.btnReDraw.Location = new System.Drawing.Point(612, 13);
            this.btnReDraw.Name = "btnReDraw";
            this.btnReDraw.Size = new System.Drawing.Size(91, 71);
            this.btnReDraw.TabIndex = 36;
            this.btnReDraw.Text = "ReDraw";
            this.btnReDraw.UseVisualStyleBackColor = true;
            this.btnReDraw.Visible = false;
            this.btnReDraw.Click += new System.EventHandler(this.btnReDraw_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ckAddress);
            this.groupBox1.Controls.Add(this.ckSection);
            this.groupBox1.Controls.Add(this.ckBarcode);
            this.groupBox1.Location = new System.Drawing.Point(382, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(224, 80);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Map Items";
            this.groupBox1.Visible = false;
            // 
            // ckAddress
            // 
            this.ckAddress.AutoSize = true;
            this.ckAddress.Checked = true;
            this.ckAddress.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckAddress.Location = new System.Drawing.Point(139, 21);
            this.ckAddress.Name = "ckAddress";
            this.ckAddress.Size = new System.Drawing.Size(61, 16);
            this.ckAddress.TabIndex = 2;
            this.ckAddress.Text = "Address";
            this.ckAddress.UseVisualStyleBackColor = true;
            // 
            // ckSection
            // 
            this.ckSection.AutoSize = true;
            this.ckSection.Checked = true;
            this.ckSection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckSection.Location = new System.Drawing.Point(75, 21);
            this.ckSection.Name = "ckSection";
            this.ckSection.Size = new System.Drawing.Size(58, 16);
            this.ckSection.TabIndex = 1;
            this.ckSection.Text = "Section";
            this.ckSection.UseVisualStyleBackColor = true;
            // 
            // ckBarcode
            // 
            this.ckBarcode.AutoSize = true;
            this.ckBarcode.Checked = true;
            this.ckBarcode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckBarcode.Location = new System.Drawing.Point(6, 21);
            this.ckBarcode.Name = "ckBarcode";
            this.ckBarcode.Size = new System.Drawing.Size(63, 16);
            this.ckBarcode.TabIndex = 0;
            this.ckBarcode.Text = "Barcode";
            this.ckBarcode.UseVisualStyleBackColor = true;
            // 
            // btnYflip
            // 
            this.btnYflip.Enabled = false;
            this.btnYflip.Location = new System.Drawing.Point(262, 62);
            this.btnYflip.Name = "btnYflip";
            this.btnYflip.Size = new System.Drawing.Size(114, 23);
            this.btnYflip.TabIndex = 33;
            this.btnYflip.Text = "垂直翻轉";
            this.btnYflip.UseVisualStyleBackColor = true;
            this.btnYflip.Visible = false;
            this.btnYflip.Click += new System.EventHandler(this.btnYflip_Click);
            // 
            // btnXflip
            // 
            this.btnXflip.Enabled = false;
            this.btnXflip.Location = new System.Drawing.Point(142, 62);
            this.btnXflip.Name = "btnXflip";
            this.btnXflip.Size = new System.Drawing.Size(114, 23);
            this.btnXflip.TabIndex = 32;
            this.btnXflip.Text = "水平翻轉";
            this.btnXflip.UseVisualStyleBackColor = true;
            this.btnXflip.Visible = false;
            this.btnXflip.Click += new System.EventHandler(this.btnXflip_Click);
            // 
            // txtResizePercent
            // 
            this.txtResizePercent.Enabled = false;
            this.txtResizePercent.Location = new System.Drawing.Point(264, 4);
            this.txtResizePercent.Name = "txtResizePercent";
            this.txtResizePercent.ShortcutsEnabled = false;
            this.txtResizePercent.Size = new System.Drawing.Size(112, 22);
            this.txtResizePercent.TabIndex = 30;
            this.txtResizePercent.Text = "150";
            this.txtResizePercent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtResizePercent.Visible = false;
            // 
            // txtRotateAngle
            // 
            this.txtRotateAngle.Enabled = false;
            this.txtRotateAngle.Location = new System.Drawing.Point(264, 34);
            this.txtRotateAngle.Name = "txtRotateAngle";
            this.txtRotateAngle.ShortcutsEnabled = false;
            this.txtRotateAngle.Size = new System.Drawing.Size(112, 22);
            this.txtRotateAngle.TabIndex = 31;
            this.txtRotateAngle.Text = "1";
            this.txtRotateAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtRotateAngle.Visible = false;
            // 
            // btnRotate
            // 
            this.btnRotate.Enabled = false;
            this.btnRotate.Location = new System.Drawing.Point(142, 32);
            this.btnRotate.Name = "btnRotate";
            this.btnRotate.Size = new System.Drawing.Size(114, 23);
            this.btnRotate.TabIndex = 29;
            this.btnRotate.Text = "旋轉";
            this.btnRotate.UseVisualStyleBackColor = true;
            this.btnRotate.Visible = false;
            this.btnRotate.Click += new System.EventHandler(this.btnRotate_Click);
            // 
            // btnResizePercent
            // 
            this.btnResizePercent.Enabled = false;
            this.btnResizePercent.Location = new System.Drawing.Point(142, 3);
            this.btnResizePercent.Name = "btnResizePercent";
            this.btnResizePercent.Size = new System.Drawing.Size(114, 23);
            this.btnResizePercent.TabIndex = 27;
            this.btnResizePercent.Text = "比例縮放";
            this.btnResizePercent.UseVisualStyleBackColor = true;
            this.btnResizePercent.Visible = false;
            this.btnResizePercent.Click += new System.EventHandler(this.btnResizePercent_Click);
            // 
            // btnReset
            // 
            this.btnReset.Enabled = false;
            this.btnReset.Location = new System.Drawing.Point(3, 32);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(133, 23);
            this.btnReset.TabIndex = 26;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Visible = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.Enabled = false;
            this.btnSaveImage.Location = new System.Drawing.Point(3, 61);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(133, 23);
            this.btnSaveImage.TabIndex = 25;
            this.btnSaveImage.Text = "Save Image";
            this.btnSaveImage.UseVisualStyleBackColor = true;
            this.btnSaveImage.Visible = false;
            // 
            // btnSwitchBarcodeLine
            // 
            this.btnSwitchBarcodeLine.Enabled = false;
            this.btnSwitchBarcodeLine.Location = new System.Drawing.Point(3, 3);
            this.btnSwitchBarcodeLine.Name = "btnSwitchBarcodeLine";
            this.btnSwitchBarcodeLine.Size = new System.Drawing.Size(133, 23);
            this.btnSwitchBarcodeLine.TabIndex = 24;
            this.btnSwitchBarcodeLine.Text = "切換BarcodeLine";
            this.btnSwitchBarcodeLine.UseVisualStyleBackColor = true;
            this.btnSwitchBarcodeLine.Visible = false;
            this.btnSwitchBarcodeLine.Click += new System.EventHandler(this.btnSwitchBarcodeLine_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.AutoScroll = true;
            this.splitContainer2.Panel1.Controls.Add(this.tbxTransferStepMsg);
            this.splitContainer2.Panel1.Controls.Add(this.tbxTransferCommandMsg);
            this.splitContainer2.Panel1.Controls.Add(this.txtCannotAutoReason);
            this.splitContainer2.Panel1.Controls.Add(this.txtCanAuto);
            this.splitContainer2.Panel1.Controls.Add(this.gbPerformanceCounter);
            this.splitContainer2.Panel1.Controls.Add(this.gbReserve);
            this.splitContainer2.Panel1.Controls.Add(this.btnAutoManual);
            this.splitContainer2.Panel1.Controls.Add(this.txtLastAlarm);
            this.splitContainer2.Panel1.Controls.Add(this.btnBuzzOff);
            this.splitContainer2.Panel1.Controls.Add(this.btnAlarmReset);
            this.splitContainer2.Panel1.Controls.Add(this.gbVehicleLocation);
            this.splitContainer2.Panel1.Controls.Add(this.gbConnection);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.AutoScroll = true;
            this.splitContainer2.Panel2.Controls.Add(this.txtMiddlerAbnormalReason);
            this.splitContainer2.Panel2.Controls.Add(this.txtBatterysAbnormalReason);
            this.splitContainer2.Panel2.Controls.Add(this.txtRobotAbnormalReason);
            this.splitContainer2.Panel2.Controls.Add(this.txtMainFlowAbnormalReason);
            this.splitContainer2.Panel2.Controls.Add(this.txtMoveControlAbnormalReason);
            this.splitContainer2.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer2.Size = new System.Drawing.Size(722, 1017);
            this.splitContainer2.SplitterDistance = 660;
            this.splitContainer2.TabIndex = 0;
            // 
            // tbxTransferStepMsg
            // 
            this.tbxTransferStepMsg.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxTransferStepMsg.Location = new System.Drawing.Point(423, 453);
            this.tbxTransferStepMsg.Multiline = true;
            this.tbxTransferStepMsg.Name = "tbxTransferStepMsg";
            this.tbxTransferStepMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxTransferStepMsg.Size = new System.Drawing.Size(287, 184);
            this.tbxTransferStepMsg.TabIndex = 60;
            // 
            // tbxTransferCommandMsg
            // 
            this.tbxTransferCommandMsg.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxTransferCommandMsg.Location = new System.Drawing.Point(3, 390);
            this.tbxTransferCommandMsg.Multiline = true;
            this.tbxTransferCommandMsg.Name = "tbxTransferCommandMsg";
            this.tbxTransferCommandMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxTransferCommandMsg.Size = new System.Drawing.Size(415, 247);
            this.tbxTransferCommandMsg.TabIndex = 59;
            // 
            // txtCannotAutoReason
            // 
            this.txtCannotAutoReason.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtCannotAutoReason.Location = new System.Drawing.Point(422, 74);
            this.txtCannotAutoReason.Name = "txtCannotAutoReason";
            this.txtCannotAutoReason.Size = new System.Drawing.Size(143, 79);
            this.txtCannotAutoReason.TabIndex = 58;
            this.txtCannotAutoReason.Text = "Not Auto Reason";
            // 
            // txtCanAuto
            // 
            this.txtCanAuto.BackColor = System.Drawing.Color.LightGreen;
            this.txtCanAuto.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtCanAuto.Location = new System.Drawing.Point(419, 3);
            this.txtCanAuto.Name = "txtCanAuto";
            this.txtCanAuto.Size = new System.Drawing.Size(143, 65);
            this.txtCanAuto.TabIndex = 57;
            this.txtCanAuto.Text = "可以 Auto";
            this.txtCanAuto.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gbPerformanceCounter
            // 
            this.gbPerformanceCounter.Controls.Add(this.ucCstId);
            this.gbPerformanceCounter.Controls.Add(this.btnKeyInSoc);
            this.gbPerformanceCounter.Controls.Add(this.ucCharging);
            this.gbPerformanceCounter.Controls.Add(this.numSoc);
            this.gbPerformanceCounter.Controls.Add(this.ucSoc);
            this.gbPerformanceCounter.Controls.Add(this.ucLoading);
            this.gbPerformanceCounter.Controls.Add(this.ucPerformanceCounterRam);
            this.gbPerformanceCounter.Controls.Add(this.ucPerformanceCounterCpu);
            this.gbPerformanceCounter.Location = new System.Drawing.Point(218, 74);
            this.gbPerformanceCounter.Name = "gbPerformanceCounter";
            this.gbPerformanceCounter.Size = new System.Drawing.Size(200, 310);
            this.gbPerformanceCounter.TabIndex = 10;
            this.gbPerformanceCounter.TabStop = false;
            this.gbPerformanceCounter.Text = "Performance Counter";
            // 
            // ucCstId
            // 
            this.ucCstId.Location = new System.Drawing.Point(7, 181);
            this.ucCstId.Name = "ucCstId";
            this.ucCstId.Size = new System.Drawing.Size(187, 26);
            this.ucCstId.TabIndex = 42;
            this.ucCstId.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucCstId.TagName = "CstId";
            this.ucCstId.TagValue = "";
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
            // ucCharging
            // 
            this.ucCharging.Location = new System.Drawing.Point(6, 113);
            this.ucCharging.Name = "ucCharging";
            this.ucCharging.Size = new System.Drawing.Size(187, 30);
            this.ucCharging.TabIndex = 3;
            this.ucCharging.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucCharging.TagName = "Charge";
            this.ucCharging.TagValue = "";
            // 
            // numSoc
            // 
            this.numSoc.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numSoc.Location = new System.Drawing.Point(6, 240);
            this.numSoc.Name = "numSoc";
            this.numSoc.Size = new System.Drawing.Size(191, 27);
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
            this.ucSoc.Location = new System.Drawing.Point(7, 81);
            this.ucSoc.Name = "ucSoc";
            this.ucSoc.Size = new System.Drawing.Size(187, 30);
            this.ucSoc.TabIndex = 2;
            this.ucSoc.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucSoc.TagName = "SOC";
            this.ucSoc.TagValue = "";
            // 
            // ucLoading
            // 
            this.ucLoading.Location = new System.Drawing.Point(7, 149);
            this.ucLoading.Name = "ucLoading";
            this.ucLoading.Size = new System.Drawing.Size(187, 26);
            this.ucLoading.TabIndex = 5;
            this.ucLoading.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLoading.TagName = "Loading";
            this.ucLoading.TagValue = "";
            // 
            // ucPerformanceCounterRam
            // 
            this.ucPerformanceCounterRam.Location = new System.Drawing.Point(6, 49);
            this.ucPerformanceCounterRam.Name = "ucPerformanceCounterRam";
            this.ucPerformanceCounterRam.Size = new System.Drawing.Size(187, 30);
            this.ucPerformanceCounterRam.TabIndex = 1;
            this.ucPerformanceCounterRam.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucPerformanceCounterRam.TagName = "RAM";
            this.ucPerformanceCounterRam.TagValue = "";
            // 
            // ucPerformanceCounterCpu
            // 
            this.ucPerformanceCounterCpu.Location = new System.Drawing.Point(6, 19);
            this.ucPerformanceCounterCpu.Name = "ucPerformanceCounterCpu";
            this.ucPerformanceCounterCpu.Size = new System.Drawing.Size(187, 30);
            this.ucPerformanceCounterCpu.TabIndex = 0;
            this.ucPerformanceCounterCpu.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucPerformanceCounterCpu.TagName = "CPU";
            this.ucPerformanceCounterCpu.TagValue = "";
            // 
            // gbReserve
            // 
            this.gbReserve.Controls.Add(this.lbxReserveOkSections);
            this.gbReserve.Controls.Add(this.lbxNeedReserveSections);
            this.gbReserve.Location = new System.Drawing.Point(424, 241);
            this.gbReserve.Name = "gbReserve";
            this.gbReserve.Size = new System.Drawing.Size(286, 206);
            this.gbReserve.TabIndex = 49;
            this.gbReserve.TabStop = false;
            this.gbReserve.Text = "Reserve";
            // 
            // lbxReserveOkSections
            // 
            this.lbxReserveOkSections.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbxReserveOkSections.FormattingEnabled = true;
            this.lbxReserveOkSections.ItemHeight = 19;
            this.lbxReserveOkSections.Location = new System.Drawing.Point(147, 21);
            this.lbxReserveOkSections.Name = "lbxReserveOkSections";
            this.lbxReserveOkSections.ScrollAlwaysVisible = true;
            this.lbxReserveOkSections.Size = new System.Drawing.Size(127, 175);
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
            this.lbxNeedReserveSections.Size = new System.Drawing.Size(135, 175);
            this.lbxNeedReserveSections.TabIndex = 41;
            // 
            // btnAutoManual
            // 
            this.btnAutoManual.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAutoManual.Location = new System.Drawing.Point(218, 3);
            this.btnAutoManual.Name = "btnAutoManual";
            this.btnAutoManual.Size = new System.Drawing.Size(200, 65);
            this.btnAutoManual.TabIndex = 53;
            this.btnAutoManual.Text = "Auto/Manual";
            this.btnAutoManual.UseVisualStyleBackColor = true;
            this.btnAutoManual.Click += new System.EventHandler(this.btnAutoManual_Click);
            // 
            // txtLastAlarm
            // 
            this.txtLastAlarm.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtLastAlarm.Location = new System.Drawing.Point(421, 155);
            this.txtLastAlarm.Name = "txtLastAlarm";
            this.txtLastAlarm.Size = new System.Drawing.Size(141, 83);
            this.txtLastAlarm.TabIndex = 50;
            this.txtLastAlarm.Text = "Last Alarm";
            // 
            // btnBuzzOff
            // 
            this.btnBuzzOff.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBuzzOff.ForeColor = System.Drawing.Color.Brown;
            this.btnBuzzOff.Location = new System.Drawing.Point(568, 131);
            this.btnBuzzOff.Name = "btnBuzzOff";
            this.btnBuzzOff.Size = new System.Drawing.Size(151, 107);
            this.btnBuzzOff.TabIndex = 3;
            this.btnBuzzOff.Text = "Buzz Off";
            this.btnBuzzOff.UseVisualStyleBackColor = true;
            this.btnBuzzOff.Click += new System.EventHandler(this.btnBuzzOff_Click);
            // 
            // btnAlarmReset
            // 
            this.btnAlarmReset.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAlarmReset.ForeColor = System.Drawing.Color.Red;
            this.btnAlarmReset.Location = new System.Drawing.Point(568, 3);
            this.btnAlarmReset.Name = "btnAlarmReset";
            this.btnAlarmReset.Size = new System.Drawing.Size(151, 122);
            this.btnAlarmReset.TabIndex = 2;
            this.btnAlarmReset.Text = "Alarm Reset";
            this.btnAlarmReset.UseVisualStyleBackColor = true;
            this.btnAlarmReset.Click += new System.EventHandler(this.btnAlarmReset_Click);
            // 
            // gbVehicleLocation
            // 
            this.gbVehicleLocation.Controls.Add(this.numPositionY);
            this.gbVehicleLocation.Controls.Add(this.ucDistance);
            this.gbVehicleLocation.Controls.Add(this.numPositionX);
            this.gbVehicleLocation.Controls.Add(this.btnKeyInPosition);
            this.gbVehicleLocation.Controls.Add(this.ucRealPosition);
            this.gbVehicleLocation.Controls.Add(this.ucBarcodePosition);
            this.gbVehicleLocation.Controls.Add(this.ucMapAddress);
            this.gbVehicleLocation.Controls.Add(this.ucMapSection);
            this.gbVehicleLocation.Location = new System.Drawing.Point(3, 106);
            this.gbVehicleLocation.Name = "gbVehicleLocation";
            this.gbVehicleLocation.Size = new System.Drawing.Size(209, 278);
            this.gbVehicleLocation.TabIndex = 1;
            this.gbVehicleLocation.TabStop = false;
            this.gbVehicleLocation.Text = "Vehicle Location";
            // 
            // numPositionY
            // 
            this.numPositionY.Location = new System.Drawing.Point(107, 213);
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
            this.numPositionY.Size = new System.Drawing.Size(94, 22);
            this.numPositionY.TabIndex = 41;
            // 
            // ucDistance
            // 
            this.ucDistance.Location = new System.Drawing.Point(0, 85);
            this.ucDistance.Name = "ucDistance";
            this.ucDistance.Size = new System.Drawing.Size(194, 26);
            this.ucDistance.TabIndex = 6;
            this.ucDistance.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucDistance.TagName = "Dis";
            this.ucDistance.TagValue = "";
            // 
            // numPositionX
            // 
            this.numPositionX.Location = new System.Drawing.Point(9, 213);
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
            this.numPositionX.Size = new System.Drawing.Size(92, 22);
            this.numPositionX.TabIndex = 41;
            // 
            // btnKeyInPosition
            // 
            this.btnKeyInPosition.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnKeyInPosition.Location = new System.Drawing.Point(9, 241);
            this.btnKeyInPosition.Name = "btnKeyInPosition";
            this.btnKeyInPosition.Size = new System.Drawing.Size(192, 27);
            this.btnKeyInPosition.TabIndex = 40;
            this.btnKeyInPosition.Text = "鍵入車輛位置";
            this.btnKeyInPosition.UseVisualStyleBackColor = true;
            this.btnKeyInPosition.Click += new System.EventHandler(this.btnKeyInPosition_Click);
            // 
            // ucRealPosition
            // 
            this.ucRealPosition.Location = new System.Drawing.Point(0, 149);
            this.ucRealPosition.Name = "ucRealPosition";
            this.ucRealPosition.Size = new System.Drawing.Size(194, 26);
            this.ucRealPosition.TabIndex = 5;
            this.ucRealPosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucRealPosition.TagName = "Real.";
            this.ucRealPosition.TagValue = "";
            // 
            // ucBarcodePosition
            // 
            this.ucBarcodePosition.Location = new System.Drawing.Point(0, 117);
            this.ucBarcodePosition.Name = "ucBarcodePosition";
            this.ucBarcodePosition.Size = new System.Drawing.Size(194, 26);
            this.ucBarcodePosition.TabIndex = 3;
            this.ucBarcodePosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucBarcodePosition.TagName = "Bar.";
            this.ucBarcodePosition.TagValue = "";
            // 
            // ucMapAddress
            // 
            this.ucMapAddress.Location = new System.Drawing.Point(0, 53);
            this.ucMapAddress.Name = "ucMapAddress";
            this.ucMapAddress.Size = new System.Drawing.Size(194, 26);
            this.ucMapAddress.TabIndex = 1;
            this.ucMapAddress.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucMapAddress.TagName = "L.Adr";
            this.ucMapAddress.TagValue = "";
            // 
            // ucMapSection
            // 
            this.ucMapSection.Location = new System.Drawing.Point(0, 21);
            this.ucMapSection.Name = "ucMapSection";
            this.ucMapSection.Size = new System.Drawing.Size(194, 26);
            this.ucMapSection.TabIndex = 0;
            this.ucMapSection.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucMapSection.TagName = "L.Sec";
            this.ucMapSection.TagValue = "";
            // 
            // gbConnection
            // 
            this.gbConnection.Controls.Add(this.txtAgvcConnection);
            this.gbConnection.Controls.Add(this.radOnline);
            this.gbConnection.Controls.Add(this.radOffline);
            this.gbConnection.Location = new System.Drawing.Point(3, 3);
            this.gbConnection.Name = "gbConnection";
            this.gbConnection.Size = new System.Drawing.Size(209, 97);
            this.gbConnection.TabIndex = 0;
            this.gbConnection.TabStop = false;
            this.gbConnection.Text = "AGVC Connection";
            // 
            // txtAgvcConnection
            // 
            this.txtAgvcConnection.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAgvcConnection.Location = new System.Drawing.Point(8, 57);
            this.txtAgvcConnection.Name = "txtAgvcConnection";
            this.txtAgvcConnection.Size = new System.Drawing.Size(195, 24);
            this.txtAgvcConnection.TabIndex = 2;
            this.txtAgvcConnection.Text = "Connection";
            this.txtAgvcConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // radOnline
            // 
            this.radOnline.AutoSize = true;
            this.radOnline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radOnline.Location = new System.Drawing.Point(107, 24);
            this.radOnline.Name = "radOnline";
            this.radOnline.Size = new System.Drawing.Size(78, 21);
            this.radOnline.TabIndex = 1;
            this.radOnline.Text = "Connect";
            this.radOnline.UseVisualStyleBackColor = true;
            this.radOnline.CheckedChanged += new System.EventHandler(this.radOnline_CheckedChanged);
            // 
            // radOffline
            // 
            this.radOffline.AutoSize = true;
            this.radOffline.Checked = true;
            this.radOffline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radOffline.Location = new System.Drawing.Point(3, 24);
            this.radOffline.Name = "radOffline";
            this.radOffline.Size = new System.Drawing.Size(98, 21);
            this.radOffline.TabIndex = 0;
            this.radOffline.TabStop = true;
            this.radOffline.Text = "DisConnect";
            this.radOffline.UseVisualStyleBackColor = true;
            this.radOffline.CheckedChanged += new System.EventHandler(this.radOffline_CheckedChanged);
            // 
            // txtMiddlerAbnormalReason
            // 
            this.txtMiddlerAbnormalReason.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtMiddlerAbnormalReason.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMiddlerAbnormalReason.ForeColor = System.Drawing.Color.OrangeRed;
            this.txtMiddlerAbnormalReason.Location = new System.Drawing.Point(422, 28);
            this.txtMiddlerAbnormalReason.Name = "txtMiddlerAbnormalReason";
            this.txtMiddlerAbnormalReason.Size = new System.Drawing.Size(290, 74);
            this.txtMiddlerAbnormalReason.TabIndex = 66;
            this.txtMiddlerAbnormalReason.Text = "通訊的理由";
            // 
            // txtBatterysAbnormalReason
            // 
            this.txtBatterysAbnormalReason.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtBatterysAbnormalReason.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtBatterysAbnormalReason.ForeColor = System.Drawing.Color.OrangeRed;
            this.txtBatterysAbnormalReason.Location = new System.Drawing.Point(192, 203);
            this.txtBatterysAbnormalReason.Name = "txtBatterysAbnormalReason";
            this.txtBatterysAbnormalReason.Size = new System.Drawing.Size(331, 87);
            this.txtBatterysAbnormalReason.TabIndex = 65;
            this.txtBatterysAbnormalReason.Text = "電池卡住的理由";
            // 
            // txtRobotAbnormalReason
            // 
            this.txtRobotAbnormalReason.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtRobotAbnormalReason.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtRobotAbnormalReason.ForeColor = System.Drawing.Color.OrangeRed;
            this.txtRobotAbnormalReason.Location = new System.Drawing.Point(422, 107);
            this.txtRobotAbnormalReason.Name = "txtRobotAbnormalReason";
            this.txtRobotAbnormalReason.Size = new System.Drawing.Size(292, 85);
            this.txtRobotAbnormalReason.TabIndex = 64;
            this.txtRobotAbnormalReason.Text = "上下貨卡住的理由";
            // 
            // txtMainFlowAbnormalReason
            // 
            this.txtMainFlowAbnormalReason.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtMainFlowAbnormalReason.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMainFlowAbnormalReason.ForeColor = System.Drawing.Color.OrangeRed;
            this.txtMainFlowAbnormalReason.Location = new System.Drawing.Point(162, 28);
            this.txtMainFlowAbnormalReason.Name = "txtMainFlowAbnormalReason";
            this.txtMainFlowAbnormalReason.Size = new System.Drawing.Size(250, 74);
            this.txtMainFlowAbnormalReason.TabIndex = 63;
            this.txtMainFlowAbnormalReason.Text = "流程卡住的理由";
            // 
            // txtMoveControlAbnormalReason
            // 
            this.txtMoveControlAbnormalReason.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtMoveControlAbnormalReason.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMoveControlAbnormalReason.ForeColor = System.Drawing.Color.OrangeRed;
            this.txtMoveControlAbnormalReason.Location = new System.Drawing.Point(162, 107);
            this.txtMoveControlAbnormalReason.Name = "txtMoveControlAbnormalReason";
            this.txtMoveControlAbnormalReason.Size = new System.Drawing.Size(250, 85);
            this.txtMoveControlAbnormalReason.TabIndex = 37;
            this.txtMoveControlAbnormalReason.Text = "走行卡住的理由";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtBatterysAbnormal);
            this.groupBox3.Controls.Add(this.txtMainFlowAbnormal);
            this.groupBox3.Controls.Add(this.txtMiddlerAbnormal);
            this.groupBox3.Controls.Add(this.txtRobotAbnormal);
            this.groupBox3.Controls.Add(this.txtMoveControlAbnormal);
            this.groupBox3.Location = new System.Drawing.Point(18, 21);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(138, 181);
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
            // txtMiddlerAbnormal
            // 
            this.txtMiddlerAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtMiddlerAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMiddlerAbnormal.Location = new System.Drawing.Point(6, 49);
            this.txtMiddlerAbnormal.Name = "txtMiddlerAbnormal";
            this.txtMiddlerAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtMiddlerAbnormal.TabIndex = 3;
            this.txtMiddlerAbnormal.Text = "通訊";
            this.txtMiddlerAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtWatchLowPower);
            this.groupBox2.Controls.Add(this.txtAskingReserve);
            this.groupBox2.Controls.Add(this.txtTrackPosition);
            this.groupBox2.Controls.Add(this.txtTransferStep);
            this.groupBox2.Location = new System.Drawing.Point(18, 208);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(159, 116);
            this.groupBox2.TabIndex = 61;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "FlowStatus";
            // 
            // txtWatchLowPower
            // 
            this.txtWatchLowPower.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtWatchLowPower.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtWatchLowPower.Location = new System.Drawing.Point(6, 87);
            this.txtWatchLowPower.Name = "txtWatchLowPower";
            this.txtWatchLowPower.Size = new System.Drawing.Size(150, 23);
            this.txtWatchLowPower.TabIndex = 58;
            this.txtWatchLowPower.Text = "Soc/Gap : 100/50";
            this.txtWatchLowPower.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtAskingReserve
            // 
            this.txtAskingReserve.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtAskingReserve.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAskingReserve.Location = new System.Drawing.Point(3, 64);
            this.txtAskingReserve.Name = "txtAskingReserve";
            this.txtAskingReserve.Size = new System.Drawing.Size(150, 23);
            this.txtAskingReserve.TabIndex = 58;
            this.txtAskingReserve.Text = "ID:Sec001";
            this.txtAskingReserve.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtTrackPosition
            // 
            this.txtTrackPosition.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTrackPosition.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtTrackPosition.Location = new System.Drawing.Point(6, 41);
            this.txtTrackPosition.Name = "txtTrackPosition";
            this.txtTrackPosition.Size = new System.Drawing.Size(150, 23);
            this.txtTrackPosition.TabIndex = 57;
            this.txtTrackPosition.Text = "(StepI, MoveI)";
            this.txtTrackPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtTransferStep
            // 
            this.txtTransferStep.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTransferStep.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtTransferStep.Location = new System.Drawing.Point(3, 17);
            this.txtTransferStep.Name = "txtTransferStep";
            this.txtTransferStep.Size = new System.Drawing.Size(150, 23);
            this.txtTransferStep.TabIndex = 56;
            this.txtTransferStep.Text = "Step : Move";
            this.txtTransferStep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timeUpdateUI
            // 
            this.timeUpdateUI.Enabled = true;
            this.timeUpdateUI.Interval = 250;
            this.timeUpdateUI.Tick += new System.EventHandler(this.timeUpdateUI_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tspbCommding,
            this.tstextClientName,
            this.tstextRemoteIp,
            this.tstextRemotePort,
            this.tstextRealPosX,
            this.tstextRealPosY});
            this.statusStrip1.Location = new System.Drawing.Point(0, 1019);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1904, 22);
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
            // tstextRealPosX
            // 
            this.tstextRealPosX.Name = "tstextRealPosX";
            this.tstextRealPosX.Size = new System.Drawing.Size(90, 17);
            this.tstextRealPosX.Text = "tstextRealPosX";
            // 
            // tstextRealPosY
            // 
            this.tstextRealPosY.Name = "tstextRealPosY";
            this.tstextRealPosY.Size = new System.Drawing.Size(89, 17);
            this.tstextRealPosY.Text = "tstextRealPosY";
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
            this.ClientSize = new System.Drawing.Size(1904, 1041);
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
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.gbPerformanceCounter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numSoc)).EndInit();
            this.gbReserve.ResumeLayout(false);
            this.gbVehicleLocation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).EndInit();
            this.gbConnection.ResumeLayout(false);
            this.gbConnection.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem JogPage;
        private System.Windows.Forms.ToolStripMenuItem AlarmPage;
        private System.Windows.Forms.ToolStripMenuItem MiddlerPage;
        private System.Windows.Forms.ToolStripMenuItem VehicleStatusPage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripMenuItem ManualMoveCmdPage;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Button btnSwitchBarcodeLine;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnSaveImage;
        private System.Windows.Forms.TextBox txtResizePercent;
        private System.Windows.Forms.TextBox txtRotateAngle;
        private System.Windows.Forms.Button btnRotate;
        private System.Windows.Forms.Button btnResizePercent;
        private System.Windows.Forms.Button btnYflip;
        private System.Windows.Forms.Button btnXflip;
        private System.Windows.Forms.GroupBox gbVehicleLocation;
        private System.Windows.Forms.GroupBox gbConnection;
        private System.Windows.Forms.RadioButton radOnline;
        private System.Windows.Forms.RadioButton radOffline;
        private UcLabelTextBox ucRealPosition;
        private UcLabelTextBox ucBarcodePosition;
        private UcLabelTextBox ucMapAddress;
        private UcLabelTextBox ucMapSection;
        private System.Windows.Forms.Button btnBuzzOff;
        private System.Windows.Forms.Button btnAlarmReset;
        private System.Windows.Forms.Button btnKeyInPosition;
        private System.Windows.Forms.ListBox lbxReserveOkSections;
        private System.Windows.Forms.ListBox lbxNeedReserveSections;
        private System.Windows.Forms.Timer timeUpdateUI;
        private System.Windows.Forms.NumericUpDown numPositionY;
        private System.Windows.Forms.NumericUpDown numPositionX;
        private System.Windows.Forms.GroupBox gbPerformanceCounter;
        private UcLabelTextBox ucPerformanceCounterRam;
        private UcLabelTextBox ucPerformanceCounterCpu;
        private System.Windows.Forms.ToolStripMenuItem IntegrateCommandPage;
        private System.Windows.Forms.Label txtLastAlarm;
        private UcLabelTextBox ucSoc;
        private System.Windows.Forms.PictureBox pictureBox1;
        private UcLabelTextBox ucLoading;
        private System.Windows.Forms.Button btnAutoManual;
        private System.Windows.Forms.Label txtTransferStep;
        private System.Windows.Forms.GroupBox gbReserve;
        private System.Windows.Forms.Label txtTrackPosition;
        private System.Windows.Forms.Label txtAskingReserve;
        private System.Windows.Forms.Label txtWatchLowPower;
        private System.Windows.Forms.NumericUpDown numSoc;
        private System.Windows.Forms.Button btnKeyInSoc;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox ckAddress;
        private System.Windows.Forms.CheckBox ckSection;
        private System.Windows.Forms.CheckBox ckBarcode;
        private System.Windows.Forms.Button btnReDraw;
        private UcLabelTextBox ucCharging;
        private UcLabelTextBox ucDistance;
        private System.Windows.Forms.Label txtAgvcConnection;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar tspbCommding;
        private System.Windows.Forms.ToolStripStatusLabel tstextClientName;
        private System.Windows.Forms.ToolStripStatusLabel tstextRemoteIp;
        private System.Windows.Forms.ToolStripStatusLabel tstextRemotePort;
        private System.Windows.Forms.ToolStripStatusLabel tstextRealPosX;
        private System.Windows.Forms.ToolStripStatusLabel tstextRealPosY;
        private UcLabelTextBox ucCstId;
        private System.Windows.Forms.Timer timer_SetupInitialSoc;
        private System.Windows.Forms.Label txtMoveControlAbnormalReason;
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
        private System.Windows.Forms.Label txtMiddlerAbnormal;
        private System.Windows.Forms.Label txtRobotAbnormal;
        private System.Windows.Forms.Label txtMoveControlAbnormal;
        private System.Windows.Forms.Label txtMainFlowAbnormalReason;
        private System.Windows.Forms.Label txtRobotAbnormalReason;
        private System.Windows.Forms.Button btnPrintScreen;
        private System.Windows.Forms.Label txtBatterysAbnormalReason;
        private System.Windows.Forms.Label txtMiddlerAbnormalReason;
    }
}