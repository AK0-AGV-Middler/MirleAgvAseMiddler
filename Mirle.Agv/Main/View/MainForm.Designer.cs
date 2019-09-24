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
            this.JogPage = new System.Windows.Forms.ToolStripMenuItem();
            this.AlarmPage = new System.Windows.Forms.ToolStripMenuItem();
            this.MiddlerPage = new System.Windows.Forms.ToolStripMenuItem();
            this.ManualMoveCmdPage = new System.Windows.Forms.ToolStripMenuItem();
            this.VehicleStatusPage = new System.Windows.Forms.ToolStripMenuItem();
            this.PlcPage = new System.Windows.Forms.ToolStripMenuItem();
            this.工程師ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.cbSimulationMode = new System.Windows.Forms.CheckBox();
            this.btnReDraw = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ckAddress = new System.Windows.Forms.CheckBox();
            this.ckSection = new System.Windows.Forms.CheckBox();
            this.ckBarcode = new System.Windows.Forms.CheckBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
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
            this.gbPerformanceCounter = new System.Windows.Forms.GroupBox();
            this.btnKeyInSoc = new System.Windows.Forms.Button();
            this.ucCharging = new Mirle.Agv.UcLabelTextBox();
            this.numSoc = new System.Windows.Forms.NumericUpDown();
            this.ucSoc = new Mirle.Agv.UcLabelTextBox();
            this.ucPerformanceCounterRam = new Mirle.Agv.UcLabelTextBox();
            this.ucPerformanceCounterCpu = new Mirle.Agv.UcLabelTextBox();
            this.rtbTransferStep = new System.Windows.Forms.RichTextBox();
            this.rtbAgvcTransCmd = new System.Windows.Forms.RichTextBox();
            this.gbReserve = new System.Windows.Forms.GroupBox();
            this.btnGetReserveOkClear = new System.Windows.Forms.Button();
            this.btnAskReserveClear = new System.Windows.Forms.Button();
            this.lbxReserveOkSections = new System.Windows.Forms.ListBox();
            this.lbxAskReserveSection = new System.Windows.Forms.ListBox();
            this.btnNeedReserveClear = new System.Windows.Forms.Button();
            this.lbxNeedReserveSections = new System.Windows.Forms.ListBox();
            this.btnStopAndClear = new System.Windows.Forms.Button();
            this.btnAutoManual = new System.Windows.Forms.Button();
            this.txtLastAlarm = new System.Windows.Forms.Label();
            this.btnStopVehicle = new System.Windows.Forms.Button();
            this.btnBuzzOff = new System.Windows.Forms.Button();
            this.btnAlarmReset = new System.Windows.Forms.Button();
            this.gbVehicleLocation = new System.Windows.Forms.GroupBox();
            this.numPositionY = new System.Windows.Forms.NumericUpDown();
            this.ucDistance = new Mirle.Agv.UcLabelTextBox();
            this.numPositionX = new System.Windows.Forms.NumericUpDown();
            this.btnKeyInPosition = new System.Windows.Forms.Button();
            this.ucLoading = new Mirle.Agv.UcLabelTextBox();
            this.ucRealPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucBarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.ucMapAddress = new Mirle.Agv.UcLabelTextBox();
            this.ucMapSection = new Mirle.Agv.UcLabelTextBox();
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.txtAgvcConnection = new System.Windows.Forms.Label();
            this.radOnline = new System.Windows.Forms.RadioButton();
            this.radOffline = new System.Windows.Forms.RadioButton();
            this.btnLoadOk = new System.Windows.Forms.Button();
            this.btnMoveOk = new System.Windows.Forms.Button();
            this.ucBarPos = new Mirle.Agv.UcLabelTextBox();
            this.btnSemiAutoManual = new System.Windows.Forms.Button();
            this.txtBarNum = new System.Windows.Forms.TextBox();
            this.gbVisitTransferSteps = new System.Windows.Forms.GroupBox();
            this.txtTransferStep = new System.Windows.Forms.Label();
            this.picVisitTransferSteps = new System.Windows.Forms.PictureBox();
            this.btnStartVisitTransferSteps = new System.Windows.Forms.Button();
            this.btnResumeVisitTransferSteps = new System.Windows.Forms.Button();
            this.btnStopVisitTransferSteps = new System.Windows.Forms.Button();
            this.btnPauseVisitTransferSteps = new System.Windows.Forms.Button();
            this.gbTrackPosition = new System.Windows.Forms.GroupBox();
            this.txtTrackPosition = new System.Windows.Forms.Label();
            this.picTrackPosition = new System.Windows.Forms.PictureBox();
            this.btnStartTrackPosition = new System.Windows.Forms.Button();
            this.btnResumeTrackPostiion = new System.Windows.Forms.Button();
            this.btnStopTrackPosition = new System.Windows.Forms.Button();
            this.btnPauseTrackPosition = new System.Windows.Forms.Button();
            this.gbWatchLowPower = new System.Windows.Forms.GroupBox();
            this.txtWatchLowPower = new System.Windows.Forms.Label();
            this.picWatchLowPower = new System.Windows.Forms.PictureBox();
            this.btnStartWatchLowPower = new System.Windows.Forms.Button();
            this.btnResumeWatchLowPower = new System.Windows.Forms.Button();
            this.btnStopWatchLowPower = new System.Windows.Forms.Button();
            this.btnPauseWatchLowPower = new System.Windows.Forms.Button();
            this.gbAskReserve = new System.Windows.Forms.GroupBox();
            this.txtAskingReserve = new System.Windows.Forms.Label();
            this.picAskReserve = new System.Windows.Forms.PictureBox();
            this.btnStartAskReserve = new System.Windows.Forms.Button();
            this.btnResumeAskReserve = new System.Windows.Forms.Button();
            this.btnStopAskReserve = new System.Windows.Forms.Button();
            this.btnPauseAskReserve = new System.Windows.Forms.Button();
            this.btnAutoApplyReserveOnce = new System.Windows.Forms.Button();
            this.timeUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tspbCommding = new System.Windows.Forms.ToolStripProgressBar();
            this.tstextClientName = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRemoteIp = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRemotePort = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRealPosX = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRealPosY = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnUnloadOk = new System.Windows.Forms.Button();
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
            this.gbVisitTransferSteps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picVisitTransferSteps)).BeginInit();
            this.gbTrackPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTrackPosition)).BeginInit();
            this.gbWatchLowPower.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picWatchLowPower)).BeginInit();
            this.gbAskReserve.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAskReserve)).BeginInit();
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
            this.JogPage,
            this.AlarmPage,
            this.MiddlerPage,
            this.ManualMoveCmdPage,
            this.VehicleStatusPage,
            this.PlcPage});
            this.模式ToolStripMenuItem.Name = "模式ToolStripMenuItem";
            this.模式ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.模式ToolStripMenuItem.Text = "模式";
            // 
            // JogPage
            // 
            this.JogPage.Name = "JogPage";
            this.JogPage.Size = new System.Drawing.Size(208, 22);
            this.JogPage.Text = "JogPitch";
            this.JogPage.Click += new System.EventHandler(this.JogPage_Click);
            // 
            // AlarmPage
            // 
            this.AlarmPage.Name = "AlarmPage";
            this.AlarmPage.Size = new System.Drawing.Size(208, 22);
            this.AlarmPage.Text = "Alarm";
            this.AlarmPage.Click += new System.EventHandler(this.AlarmPage_Click);
            // 
            // MiddlerPage
            // 
            this.MiddlerPage.Name = "MiddlerPage";
            this.MiddlerPage.Size = new System.Drawing.Size(208, 22);
            this.MiddlerPage.Text = "通訊";
            this.MiddlerPage.Click += new System.EventHandler(this.MiddlerPage_Click);
            // 
            // ManualMoveCmdPage
            // 
            this.ManualMoveCmdPage.Name = "ManualMoveCmdPage";
            this.ManualMoveCmdPage.Size = new System.Drawing.Size(208, 22);
            this.ManualMoveCmdPage.Text = "半自動命令DebugMode";
            this.ManualMoveCmdPage.Click += new System.EventHandler(this.ManualMoveCmdPage_Click);
            // 
            // VehicleStatusPage
            // 
            this.VehicleStatusPage.Name = "VehicleStatusPage";
            this.VehicleStatusPage.Size = new System.Drawing.Size(208, 22);
            this.VehicleStatusPage.Text = "車輛狀態";
            // 
            // PlcPage
            // 
            this.PlcPage.Name = "PlcPage";
            this.PlcPage.Size = new System.Drawing.Size(208, 22);
            this.PlcPage.Text = "Plc";
            this.PlcPage.Click += new System.EventHandler(this.PlcPage_Click);
            // 
            // 工程師ToolStripMenuItem
            // 
            this.工程師ToolStripMenuItem.Name = "工程師ToolStripMenuItem";
            this.工程師ToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.工程師ToolStripMenuItem.Text = "工程師";
            this.工程師ToolStripMenuItem.Click += new System.EventHandler(this.工程師ToolStripMenuItem_Click);
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
            this.splitContainer3.Panel2.Controls.Add(this.cbSimulationMode);
            this.splitContainer3.Panel2.Controls.Add(this.btnReDraw);
            this.splitContainer3.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer3.Panel2.Controls.Add(this.richTextBox1);
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
            // cbSimulationMode
            // 
            this.cbSimulationMode.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cbSimulationMode.ForeColor = System.Drawing.Color.OrangeRed;
            this.cbSimulationMode.Location = new System.Drawing.Point(731, 14);
            this.cbSimulationMode.Name = "cbSimulationMode";
            this.cbSimulationMode.Size = new System.Drawing.Size(121, 43);
            this.cbSimulationMode.TabIndex = 3;
            this.cbSimulationMode.Text = "模擬測試";
            this.cbSimulationMode.UseVisualStyleBackColor = true;
            this.cbSimulationMode.CheckedChanged += new System.EventHandler(this.cbSimulationMode_CheckedChanged);
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
            // richTextBox1
            // 
            this.richTextBox1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.richTextBox1.Location = new System.Drawing.Point(3, 91);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(1172, 233);
            this.richTextBox1.TabIndex = 34;
            this.richTextBox1.Text = "";
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
            this.splitContainer2.Panel1.Controls.Add(this.gbPerformanceCounter);
            this.splitContainer2.Panel1.Controls.Add(this.rtbTransferStep);
            this.splitContainer2.Panel1.Controls.Add(this.rtbAgvcTransCmd);
            this.splitContainer2.Panel1.Controls.Add(this.gbReserve);
            this.splitContainer2.Panel1.Controls.Add(this.btnStopAndClear);
            this.splitContainer2.Panel1.Controls.Add(this.btnAutoManual);
            this.splitContainer2.Panel1.Controls.Add(this.txtLastAlarm);
            this.splitContainer2.Panel1.Controls.Add(this.btnStopVehicle);
            this.splitContainer2.Panel1.Controls.Add(this.btnBuzzOff);
            this.splitContainer2.Panel1.Controls.Add(this.btnAlarmReset);
            this.splitContainer2.Panel1.Controls.Add(this.gbVehicleLocation);
            this.splitContainer2.Panel1.Controls.Add(this.gbConnection);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.AutoScroll = true;
            this.splitContainer2.Panel2.Controls.Add(this.btnUnloadOk);
            this.splitContainer2.Panel2.Controls.Add(this.btnLoadOk);
            this.splitContainer2.Panel2.Controls.Add(this.btnMoveOk);
            this.splitContainer2.Panel2.Controls.Add(this.ucBarPos);
            this.splitContainer2.Panel2.Controls.Add(this.btnSemiAutoManual);
            this.splitContainer2.Panel2.Controls.Add(this.txtBarNum);
            this.splitContainer2.Panel2.Controls.Add(this.gbVisitTransferSteps);
            this.splitContainer2.Panel2.Controls.Add(this.gbTrackPosition);
            this.splitContainer2.Panel2.Controls.Add(this.gbWatchLowPower);
            this.splitContainer2.Panel2.Controls.Add(this.gbAskReserve);
            this.splitContainer2.Panel2.Controls.Add(this.btnAutoApplyReserveOnce);
            this.splitContainer2.Size = new System.Drawing.Size(722, 1017);
            this.splitContainer2.SplitterDistance = 677;
            this.splitContainer2.TabIndex = 0;
            // 
            // gbPerformanceCounter
            // 
            this.gbPerformanceCounter.Controls.Add(this.btnKeyInSoc);
            this.gbPerformanceCounter.Controls.Add(this.ucCharging);
            this.gbPerformanceCounter.Controls.Add(this.numSoc);
            this.gbPerformanceCounter.Controls.Add(this.ucSoc);
            this.gbPerformanceCounter.Controls.Add(this.ucPerformanceCounterRam);
            this.gbPerformanceCounter.Controls.Add(this.ucPerformanceCounterCpu);
            this.gbPerformanceCounter.Location = new System.Drawing.Point(218, 74);
            this.gbPerformanceCounter.Name = "gbPerformanceCounter";
            this.gbPerformanceCounter.Size = new System.Drawing.Size(200, 239);
            this.gbPerformanceCounter.TabIndex = 10;
            this.gbPerformanceCounter.TabStop = false;
            this.gbPerformanceCounter.Text = "Performance Counter";
            // 
            // btnKeyInSoc
            // 
            this.btnKeyInSoc.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnKeyInSoc.Location = new System.Drawing.Point(6, 205);
            this.btnKeyInSoc.Name = "btnKeyInSoc";
            this.btnKeyInSoc.Size = new System.Drawing.Size(191, 28);
            this.btnKeyInSoc.TabIndex = 40;
            this.btnKeyInSoc.Text = "校正電量";
            this.btnKeyInSoc.UseVisualStyleBackColor = true;
            this.btnKeyInSoc.Click += new System.EventHandler(this.btnKeyInSoc_Click);
            // 
            // ucCharging
            // 
            this.ucCharging.Location = new System.Drawing.Point(7, 127);
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
            this.numSoc.Location = new System.Drawing.Point(6, 172);
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
            this.ucSoc.Location = new System.Drawing.Point(7, 91);
            this.ucSoc.Name = "ucSoc";
            this.ucSoc.Size = new System.Drawing.Size(187, 30);
            this.ucSoc.TabIndex = 2;
            this.ucSoc.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucSoc.TagName = "SOC";
            this.ucSoc.TagValue = "";
            // 
            // ucPerformanceCounterRam
            // 
            this.ucPerformanceCounterRam.Location = new System.Drawing.Point(7, 55);
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
            // rtbTransferStep
            // 
            this.rtbTransferStep.AcceptsTab = true;
            this.rtbTransferStep.Location = new System.Drawing.Point(486, 466);
            this.rtbTransferStep.Name = "rtbTransferStep";
            this.rtbTransferStep.Size = new System.Drawing.Size(224, 200);
            this.rtbTransferStep.TabIndex = 56;
            this.rtbTransferStep.Text = "";
            this.rtbTransferStep.WordWrap = false;
            // 
            // rtbAgvcTransCmd
            // 
            this.rtbAgvcTransCmd.AcceptsTab = true;
            this.rtbAgvcTransCmd.Location = new System.Drawing.Point(486, 260);
            this.rtbAgvcTransCmd.Name = "rtbAgvcTransCmd";
            this.rtbAgvcTransCmd.Size = new System.Drawing.Size(224, 200);
            this.rtbAgvcTransCmd.TabIndex = 55;
            this.rtbAgvcTransCmd.Text = "";
            this.rtbAgvcTransCmd.WordWrap = false;
            // 
            // gbReserve
            // 
            this.gbReserve.Controls.Add(this.btnGetReserveOkClear);
            this.gbReserve.Controls.Add(this.btnAskReserveClear);
            this.gbReserve.Controls.Add(this.lbxReserveOkSections);
            this.gbReserve.Controls.Add(this.lbxAskReserveSection);
            this.gbReserve.Controls.Add(this.btnNeedReserveClear);
            this.gbReserve.Controls.Add(this.lbxNeedReserveSections);
            this.gbReserve.Location = new System.Drawing.Point(9, 466);
            this.gbReserve.Name = "gbReserve";
            this.gbReserve.Size = new System.Drawing.Size(468, 202);
            this.gbReserve.TabIndex = 49;
            this.gbReserve.TabStop = false;
            this.gbReserve.Text = "Reserve";
            // 
            // btnGetReserveOkClear
            // 
            this.btnGetReserveOkClear.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnGetReserveOkClear.Location = new System.Drawing.Point(305, 21);
            this.btnGetReserveOkClear.Name = "btnGetReserveOkClear";
            this.btnGetReserveOkClear.Size = new System.Drawing.Size(144, 27);
            this.btnGetReserveOkClear.TabIndex = 49;
            this.btnGetReserveOkClear.Text = "取得通行權清單";
            this.btnGetReserveOkClear.UseVisualStyleBackColor = true;
            this.btnGetReserveOkClear.Click += new System.EventHandler(this.btnGetReserveOkClear_Click);
            // 
            // btnAskReserveClear
            // 
            this.btnAskReserveClear.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAskReserveClear.Location = new System.Drawing.Point(155, 21);
            this.btnAskReserveClear.Name = "btnAskReserveClear";
            this.btnAskReserveClear.Size = new System.Drawing.Size(144, 27);
            this.btnAskReserveClear.TabIndex = 49;
            this.btnAskReserveClear.Text = "詢問中";
            this.btnAskReserveClear.UseVisualStyleBackColor = true;
            this.btnAskReserveClear.Click += new System.EventHandler(this.btnAskReserveClear_Click);
            // 
            // lbxReserveOkSections
            // 
            this.lbxReserveOkSections.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbxReserveOkSections.FormattingEnabled = true;
            this.lbxReserveOkSections.ItemHeight = 19;
            this.lbxReserveOkSections.Location = new System.Drawing.Point(305, 54);
            this.lbxReserveOkSections.Name = "lbxReserveOkSections";
            this.lbxReserveOkSections.ScrollAlwaysVisible = true;
            this.lbxReserveOkSections.Size = new System.Drawing.Size(144, 137);
            this.lbxReserveOkSections.TabIndex = 42;
            // 
            // lbxAskReserveSection
            // 
            this.lbxAskReserveSection.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbxAskReserveSection.FormattingEnabled = true;
            this.lbxAskReserveSection.ItemHeight = 19;
            this.lbxAskReserveSection.Items.AddRange(new object[] {
            "Empty"});
            this.lbxAskReserveSection.Location = new System.Drawing.Point(155, 54);
            this.lbxAskReserveSection.Name = "lbxAskReserveSection";
            this.lbxAskReserveSection.ScrollAlwaysVisible = true;
            this.lbxAskReserveSection.Size = new System.Drawing.Size(144, 137);
            this.lbxAskReserveSection.TabIndex = 47;
            // 
            // btnNeedReserveClear
            // 
            this.btnNeedReserveClear.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnNeedReserveClear.Location = new System.Drawing.Point(5, 21);
            this.btnNeedReserveClear.Name = "btnNeedReserveClear";
            this.btnNeedReserveClear.Size = new System.Drawing.Size(144, 27);
            this.btnNeedReserveClear.TabIndex = 48;
            this.btnNeedReserveClear.Text = "需要通行權清單";
            this.btnNeedReserveClear.UseVisualStyleBackColor = true;
            this.btnNeedReserveClear.Click += new System.EventHandler(this.btnNeedReserveClear_Click);
            // 
            // lbxNeedReserveSections
            // 
            this.lbxNeedReserveSections.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbxNeedReserveSections.FormattingEnabled = true;
            this.lbxNeedReserveSections.ItemHeight = 19;
            this.lbxNeedReserveSections.Location = new System.Drawing.Point(5, 54);
            this.lbxNeedReserveSections.Name = "lbxNeedReserveSections";
            this.lbxNeedReserveSections.ScrollAlwaysVisible = true;
            this.lbxNeedReserveSections.Size = new System.Drawing.Size(144, 137);
            this.lbxNeedReserveSections.TabIndex = 41;
            // 
            // btnStopAndClear
            // 
            this.btnStopAndClear.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStopAndClear.ForeColor = System.Drawing.Color.Red;
            this.btnStopAndClear.Location = new System.Drawing.Point(422, 131);
            this.btnStopAndClear.Name = "btnStopAndClear";
            this.btnStopAndClear.Size = new System.Drawing.Size(140, 123);
            this.btnStopAndClear.TabIndex = 54;
            this.btnStopAndClear.Text = "Stop and Clear";
            this.btnStopAndClear.UseVisualStyleBackColor = true;
            this.btnStopAndClear.Click += new System.EventHandler(this.btnStopAndClear_Click);
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
            this.txtLastAlarm.Location = new System.Drawing.Point(568, 198);
            this.txtLastAlarm.Name = "txtLastAlarm";
            this.txtLastAlarm.Size = new System.Drawing.Size(148, 52);
            this.txtLastAlarm.TabIndex = 50;
            this.txtLastAlarm.Text = "Last Alarm";
            // 
            // btnStopVehicle
            // 
            this.btnStopVehicle.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStopVehicle.ForeColor = System.Drawing.Color.Red;
            this.btnStopVehicle.Location = new System.Drawing.Point(422, 3);
            this.btnStopVehicle.Name = "btnStopVehicle";
            this.btnStopVehicle.Size = new System.Drawing.Size(140, 122);
            this.btnStopVehicle.TabIndex = 4;
            this.btnStopVehicle.Text = "StopVehicle";
            this.btnStopVehicle.UseVisualStyleBackColor = true;
            this.btnStopVehicle.Click += new System.EventHandler(this.btnStopVehicle_Click);
            // 
            // btnBuzzOff
            // 
            this.btnBuzzOff.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBuzzOff.ForeColor = System.Drawing.Color.Brown;
            this.btnBuzzOff.Location = new System.Drawing.Point(568, 131);
            this.btnBuzzOff.Name = "btnBuzzOff";
            this.btnBuzzOff.Size = new System.Drawing.Size(151, 64);
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
            this.gbVehicleLocation.Controls.Add(this.ucLoading);
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
            // ucLoading
            // 
            this.ucLoading.Location = new System.Drawing.Point(0, 181);
            this.ucLoading.Name = "ucLoading";
            this.ucLoading.Size = new System.Drawing.Size(194, 26);
            this.ucLoading.TabIndex = 5;
            this.ucLoading.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLoading.TagName = "Loading";
            this.ucLoading.TagValue = "";
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
            // btnLoadOk
            // 
            this.btnLoadOk.Location = new System.Drawing.Point(6, 239);
            this.btnLoadOk.Name = "btnLoadOk";
            this.btnLoadOk.Size = new System.Drawing.Size(67, 33);
            this.btnLoadOk.TabIndex = 59;
            this.btnLoadOk.Text = "LoadOk";
            this.btnLoadOk.UseVisualStyleBackColor = true;
            this.btnLoadOk.Visible = false;
            this.btnLoadOk.Click += new System.EventHandler(this.btnLoadOk_Click);
            // 
            // btnMoveOk
            // 
            this.btnMoveOk.Location = new System.Drawing.Point(6, 200);
            this.btnMoveOk.Name = "btnMoveOk";
            this.btnMoveOk.Size = new System.Drawing.Size(67, 33);
            this.btnMoveOk.TabIndex = 58;
            this.btnMoveOk.Text = "MoveOk";
            this.btnMoveOk.UseVisualStyleBackColor = true;
            this.btnMoveOk.Visible = false;
            this.btnMoveOk.Click += new System.EventHandler(this.btnMoveOk_Click);
            // 
            // ucBarPos
            // 
            this.ucBarPos.Location = new System.Drawing.Point(154, 228);
            this.ucBarPos.Name = "ucBarPos";
            this.ucBarPos.Size = new System.Drawing.Size(185, 26);
            this.ucBarPos.TabIndex = 42;
            this.ucBarPos.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucBarPos.TagName = "Bar Pos";
            this.ucBarPos.TagValue = "";
            // 
            // btnSemiAutoManual
            // 
            this.btnSemiAutoManual.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSemiAutoManual.Location = new System.Drawing.Point(519, 252);
            this.btnSemiAutoManual.Name = "btnSemiAutoManual";
            this.btnSemiAutoManual.Size = new System.Drawing.Size(200, 65);
            this.btnSemiAutoManual.TabIndex = 56;
            this.btnSemiAutoManual.Text = "Semi - Auto/Manual";
            this.btnSemiAutoManual.UseVisualStyleBackColor = true;
            this.btnSemiAutoManual.Visible = false;
            this.btnSemiAutoManual.Click += new System.EventHandler(this.btnSemiAutoManual_Click);
            // 
            // txtBarNum
            // 
            this.txtBarNum.Location = new System.Drawing.Point(174, 200);
            this.txtBarNum.Name = "txtBarNum";
            this.txtBarNum.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtBarNum.Size = new System.Drawing.Size(165, 22);
            this.txtBarNum.TabIndex = 57;
            this.txtBarNum.TextChanged += new System.EventHandler(this.txtBarNum_TextChanged);
            // 
            // gbVisitTransferSteps
            // 
            this.gbVisitTransferSteps.Controls.Add(this.txtTransferStep);
            this.gbVisitTransferSteps.Controls.Add(this.picVisitTransferSteps);
            this.gbVisitTransferSteps.Controls.Add(this.btnStartVisitTransferSteps);
            this.gbVisitTransferSteps.Controls.Add(this.btnResumeVisitTransferSteps);
            this.gbVisitTransferSteps.Controls.Add(this.btnStopVisitTransferSteps);
            this.gbVisitTransferSteps.Controls.Add(this.btnPauseVisitTransferSteps);
            this.gbVisitTransferSteps.Location = new System.Drawing.Point(6, 13);
            this.gbVisitTransferSteps.Name = "gbVisitTransferSteps";
            this.gbVisitTransferSteps.Size = new System.Drawing.Size(165, 182);
            this.gbVisitTransferSteps.TabIndex = 43;
            this.gbVisitTransferSteps.TabStop = false;
            this.gbVisitTransferSteps.Text = "Visit TransferSteps";
            // 
            // txtTransferStep
            // 
            this.txtTransferStep.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTransferStep.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtTransferStep.Location = new System.Drawing.Point(6, 156);
            this.txtTransferStep.Name = "txtTransferStep";
            this.txtTransferStep.Size = new System.Drawing.Size(150, 23);
            this.txtTransferStep.TabIndex = 56;
            this.txtTransferStep.Text = "Step : Move";
            this.txtTransferStep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picVisitTransferSteps
            // 
            this.picVisitTransferSteps.Location = new System.Drawing.Point(6, 137);
            this.picVisitTransferSteps.Name = "picVisitTransferSteps";
            this.picVisitTransferSteps.Size = new System.Drawing.Size(150, 16);
            this.picVisitTransferSteps.TabIndex = 55;
            this.picVisitTransferSteps.TabStop = false;
            // 
            // btnStartVisitTransferSteps
            // 
            this.btnStartVisitTransferSteps.Location = new System.Drawing.Point(6, 21);
            this.btnStartVisitTransferSteps.Name = "btnStartVisitTransferSteps";
            this.btnStartVisitTransferSteps.Size = new System.Drawing.Size(150, 23);
            this.btnStartVisitTransferSteps.TabIndex = 5;
            this.btnStartVisitTransferSteps.Text = "Start Visit TransferSteps";
            this.btnStartVisitTransferSteps.UseVisualStyleBackColor = true;
            this.btnStartVisitTransferSteps.Click += new System.EventHandler(this.btnStartVisitTransferSteps_Click);
            // 
            // btnResumeVisitTransferSteps
            // 
            this.btnResumeVisitTransferSteps.Location = new System.Drawing.Point(6, 79);
            this.btnResumeVisitTransferSteps.Name = "btnResumeVisitTransferSteps";
            this.btnResumeVisitTransferSteps.Size = new System.Drawing.Size(150, 23);
            this.btnResumeVisitTransferSteps.TabIndex = 8;
            this.btnResumeVisitTransferSteps.Text = "Resume Visit TransferSteps";
            this.btnResumeVisitTransferSteps.UseVisualStyleBackColor = true;
            this.btnResumeVisitTransferSteps.Click += new System.EventHandler(this.btnResumeVisitTransferSteps_Click);
            // 
            // btnStopVisitTransferSteps
            // 
            this.btnStopVisitTransferSteps.Location = new System.Drawing.Point(6, 108);
            this.btnStopVisitTransferSteps.Name = "btnStopVisitTransferSteps";
            this.btnStopVisitTransferSteps.Size = new System.Drawing.Size(150, 23);
            this.btnStopVisitTransferSteps.TabIndex = 6;
            this.btnStopVisitTransferSteps.Text = "Stop Visit TransferSteps";
            this.btnStopVisitTransferSteps.UseVisualStyleBackColor = true;
            this.btnStopVisitTransferSteps.Click += new System.EventHandler(this.btnStopVisitTransferSteps_Click);
            // 
            // btnPauseVisitTransferSteps
            // 
            this.btnPauseVisitTransferSteps.Location = new System.Drawing.Point(6, 50);
            this.btnPauseVisitTransferSteps.Name = "btnPauseVisitTransferSteps";
            this.btnPauseVisitTransferSteps.Size = new System.Drawing.Size(150, 23);
            this.btnPauseVisitTransferSteps.TabIndex = 7;
            this.btnPauseVisitTransferSteps.Text = "Pause Visit TransferSteps";
            this.btnPauseVisitTransferSteps.UseVisualStyleBackColor = true;
            this.btnPauseVisitTransferSteps.Click += new System.EventHandler(this.btnPauseVisitTransferSteps_Click);
            // 
            // gbTrackPosition
            // 
            this.gbTrackPosition.Controls.Add(this.txtTrackPosition);
            this.gbTrackPosition.Controls.Add(this.picTrackPosition);
            this.gbTrackPosition.Controls.Add(this.btnStartTrackPosition);
            this.gbTrackPosition.Controls.Add(this.btnResumeTrackPostiion);
            this.gbTrackPosition.Controls.Add(this.btnStopTrackPosition);
            this.gbTrackPosition.Controls.Add(this.btnPauseTrackPosition);
            this.gbTrackPosition.Location = new System.Drawing.Point(177, 12);
            this.gbTrackPosition.Name = "gbTrackPosition";
            this.gbTrackPosition.Size = new System.Drawing.Size(165, 182);
            this.gbTrackPosition.TabIndex = 42;
            this.gbTrackPosition.TabStop = false;
            this.gbTrackPosition.Text = "Track Position";
            // 
            // txtTrackPosition
            // 
            this.txtTrackPosition.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTrackPosition.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtTrackPosition.Location = new System.Drawing.Point(6, 154);
            this.txtTrackPosition.Name = "txtTrackPosition";
            this.txtTrackPosition.Size = new System.Drawing.Size(150, 23);
            this.txtTrackPosition.TabIndex = 57;
            this.txtTrackPosition.Text = "(StepI, MoveI)";
            this.txtTrackPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picTrackPosition
            // 
            this.picTrackPosition.Location = new System.Drawing.Point(6, 135);
            this.picTrackPosition.Name = "picTrackPosition";
            this.picTrackPosition.Size = new System.Drawing.Size(150, 16);
            this.picTrackPosition.TabIndex = 56;
            this.picTrackPosition.TabStop = false;
            // 
            // btnStartTrackPosition
            // 
            this.btnStartTrackPosition.Location = new System.Drawing.Point(6, 21);
            this.btnStartTrackPosition.Name = "btnStartTrackPosition";
            this.btnStartTrackPosition.Size = new System.Drawing.Size(150, 23);
            this.btnStartTrackPosition.TabIndex = 5;
            this.btnStartTrackPosition.Text = "Start Track Position";
            this.btnStartTrackPosition.UseVisualStyleBackColor = true;
            this.btnStartTrackPosition.Click += new System.EventHandler(this.btnStartTrackPosition_Click);
            // 
            // btnResumeTrackPostiion
            // 
            this.btnResumeTrackPostiion.Location = new System.Drawing.Point(6, 79);
            this.btnResumeTrackPostiion.Name = "btnResumeTrackPostiion";
            this.btnResumeTrackPostiion.Size = new System.Drawing.Size(150, 23);
            this.btnResumeTrackPostiion.TabIndex = 8;
            this.btnResumeTrackPostiion.Text = "Resume Track Position";
            this.btnResumeTrackPostiion.UseVisualStyleBackColor = true;
            this.btnResumeTrackPostiion.Click += new System.EventHandler(this.btnResumeTrackPostiion_Click);
            // 
            // btnStopTrackPosition
            // 
            this.btnStopTrackPosition.Location = new System.Drawing.Point(6, 108);
            this.btnStopTrackPosition.Name = "btnStopTrackPosition";
            this.btnStopTrackPosition.Size = new System.Drawing.Size(150, 23);
            this.btnStopTrackPosition.TabIndex = 6;
            this.btnStopTrackPosition.Text = "Stop Track Position";
            this.btnStopTrackPosition.UseVisualStyleBackColor = true;
            this.btnStopTrackPosition.Click += new System.EventHandler(this.btnStopTrackPosition_Click);
            // 
            // btnPauseTrackPosition
            // 
            this.btnPauseTrackPosition.Location = new System.Drawing.Point(6, 50);
            this.btnPauseTrackPosition.Name = "btnPauseTrackPosition";
            this.btnPauseTrackPosition.Size = new System.Drawing.Size(150, 23);
            this.btnPauseTrackPosition.TabIndex = 7;
            this.btnPauseTrackPosition.Text = "Pause Track Position";
            this.btnPauseTrackPosition.UseVisualStyleBackColor = true;
            this.btnPauseTrackPosition.Click += new System.EventHandler(this.btnPauseTrackPosition_Click);
            // 
            // gbWatchLowPower
            // 
            this.gbWatchLowPower.Controls.Add(this.txtWatchLowPower);
            this.gbWatchLowPower.Controls.Add(this.picWatchLowPower);
            this.gbWatchLowPower.Controls.Add(this.btnStartWatchLowPower);
            this.gbWatchLowPower.Controls.Add(this.btnResumeWatchLowPower);
            this.gbWatchLowPower.Controls.Add(this.btnStopWatchLowPower);
            this.gbWatchLowPower.Controls.Add(this.btnPauseWatchLowPower);
            this.gbWatchLowPower.Location = new System.Drawing.Point(519, 13);
            this.gbWatchLowPower.Name = "gbWatchLowPower";
            this.gbWatchLowPower.Size = new System.Drawing.Size(165, 182);
            this.gbWatchLowPower.TabIndex = 43;
            this.gbWatchLowPower.TabStop = false;
            this.gbWatchLowPower.Text = "Watch LowPower";
            // 
            // txtWatchLowPower
            // 
            this.txtWatchLowPower.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtWatchLowPower.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtWatchLowPower.Location = new System.Drawing.Point(6, 154);
            this.txtWatchLowPower.Name = "txtWatchLowPower";
            this.txtWatchLowPower.Size = new System.Drawing.Size(150, 23);
            this.txtWatchLowPower.TabIndex = 58;
            this.txtWatchLowPower.Text = "Soc/Gap : 100/50";
            this.txtWatchLowPower.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picWatchLowPower
            // 
            this.picWatchLowPower.Location = new System.Drawing.Point(6, 135);
            this.picWatchLowPower.Name = "picWatchLowPower";
            this.picWatchLowPower.Size = new System.Drawing.Size(150, 16);
            this.picWatchLowPower.TabIndex = 56;
            this.picWatchLowPower.TabStop = false;
            // 
            // btnStartWatchLowPower
            // 
            this.btnStartWatchLowPower.Location = new System.Drawing.Point(6, 21);
            this.btnStartWatchLowPower.Name = "btnStartWatchLowPower";
            this.btnStartWatchLowPower.Size = new System.Drawing.Size(150, 23);
            this.btnStartWatchLowPower.TabIndex = 5;
            this.btnStartWatchLowPower.Text = "Start Watch LowPower";
            this.btnStartWatchLowPower.UseVisualStyleBackColor = true;
            this.btnStartWatchLowPower.Click += new System.EventHandler(this.btnStartWatchLowPower_Click);
            // 
            // btnResumeWatchLowPower
            // 
            this.btnResumeWatchLowPower.Location = new System.Drawing.Point(6, 79);
            this.btnResumeWatchLowPower.Name = "btnResumeWatchLowPower";
            this.btnResumeWatchLowPower.Size = new System.Drawing.Size(150, 23);
            this.btnResumeWatchLowPower.TabIndex = 8;
            this.btnResumeWatchLowPower.Text = "ResumeWatch LowPower";
            this.btnResumeWatchLowPower.UseVisualStyleBackColor = true;
            this.btnResumeWatchLowPower.Click += new System.EventHandler(this.btnResumeWatchLowPower_Click);
            // 
            // btnStopWatchLowPower
            // 
            this.btnStopWatchLowPower.Location = new System.Drawing.Point(6, 108);
            this.btnStopWatchLowPower.Name = "btnStopWatchLowPower";
            this.btnStopWatchLowPower.Size = new System.Drawing.Size(150, 23);
            this.btnStopWatchLowPower.TabIndex = 6;
            this.btnStopWatchLowPower.Text = "Stop Watch LowPower";
            this.btnStopWatchLowPower.UseVisualStyleBackColor = true;
            this.btnStopWatchLowPower.Click += new System.EventHandler(this.btnStopWatchLowPower_Click);
            // 
            // btnPauseWatchLowPower
            // 
            this.btnPauseWatchLowPower.Location = new System.Drawing.Point(6, 50);
            this.btnPauseWatchLowPower.Name = "btnPauseWatchLowPower";
            this.btnPauseWatchLowPower.Size = new System.Drawing.Size(150, 23);
            this.btnPauseWatchLowPower.TabIndex = 7;
            this.btnPauseWatchLowPower.Text = "Pause Watch LowPower";
            this.btnPauseWatchLowPower.UseVisualStyleBackColor = true;
            this.btnPauseWatchLowPower.Click += new System.EventHandler(this.btnPauseWatchLowPower_Click);
            // 
            // gbAskReserve
            // 
            this.gbAskReserve.Controls.Add(this.txtAskingReserve);
            this.gbAskReserve.Controls.Add(this.picAskReserve);
            this.gbAskReserve.Controls.Add(this.btnStartAskReserve);
            this.gbAskReserve.Controls.Add(this.btnResumeAskReserve);
            this.gbAskReserve.Controls.Add(this.btnStopAskReserve);
            this.gbAskReserve.Controls.Add(this.btnPauseAskReserve);
            this.gbAskReserve.Location = new System.Drawing.Point(348, 13);
            this.gbAskReserve.Name = "gbAskReserve";
            this.gbAskReserve.Size = new System.Drawing.Size(165, 182);
            this.gbAskReserve.TabIndex = 43;
            this.gbAskReserve.TabStop = false;
            this.gbAskReserve.Text = "Ask Reserve";
            // 
            // txtAskingReserve
            // 
            this.txtAskingReserve.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtAskingReserve.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAskingReserve.Location = new System.Drawing.Point(6, 154);
            this.txtAskingReserve.Name = "txtAskingReserve";
            this.txtAskingReserve.Size = new System.Drawing.Size(150, 23);
            this.txtAskingReserve.TabIndex = 58;
            this.txtAskingReserve.Text = "ID:Sec001";
            this.txtAskingReserve.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picAskReserve
            // 
            this.picAskReserve.Location = new System.Drawing.Point(6, 137);
            this.picAskReserve.Name = "picAskReserve";
            this.picAskReserve.Size = new System.Drawing.Size(150, 16);
            this.picAskReserve.TabIndex = 56;
            this.picAskReserve.TabStop = false;
            // 
            // btnStartAskReserve
            // 
            this.btnStartAskReserve.Location = new System.Drawing.Point(6, 21);
            this.btnStartAskReserve.Name = "btnStartAskReserve";
            this.btnStartAskReserve.Size = new System.Drawing.Size(150, 23);
            this.btnStartAskReserve.TabIndex = 5;
            this.btnStartAskReserve.Text = "Start Ask Reserve";
            this.btnStartAskReserve.UseVisualStyleBackColor = true;
            this.btnStartAskReserve.Click += new System.EventHandler(this.btnStartAskReserve_Click);
            // 
            // btnResumeAskReserve
            // 
            this.btnResumeAskReserve.Location = new System.Drawing.Point(6, 79);
            this.btnResumeAskReserve.Name = "btnResumeAskReserve";
            this.btnResumeAskReserve.Size = new System.Drawing.Size(150, 23);
            this.btnResumeAskReserve.TabIndex = 8;
            this.btnResumeAskReserve.Text = "Resume Ask Reserve";
            this.btnResumeAskReserve.UseVisualStyleBackColor = true;
            this.btnResumeAskReserve.Click += new System.EventHandler(this.btnResumeAskReserve_Click);
            // 
            // btnStopAskReserve
            // 
            this.btnStopAskReserve.Location = new System.Drawing.Point(6, 108);
            this.btnStopAskReserve.Name = "btnStopAskReserve";
            this.btnStopAskReserve.Size = new System.Drawing.Size(150, 23);
            this.btnStopAskReserve.TabIndex = 6;
            this.btnStopAskReserve.Text = "Stop Ask Reserve";
            this.btnStopAskReserve.UseVisualStyleBackColor = true;
            this.btnStopAskReserve.Click += new System.EventHandler(this.btnStopAskReserve_Click);
            // 
            // btnPauseAskReserve
            // 
            this.btnPauseAskReserve.Location = new System.Drawing.Point(6, 50);
            this.btnPauseAskReserve.Name = "btnPauseAskReserve";
            this.btnPauseAskReserve.Size = new System.Drawing.Size(150, 23);
            this.btnPauseAskReserve.TabIndex = 7;
            this.btnPauseAskReserve.Text = "Pause Ask Reserve";
            this.btnPauseAskReserve.UseVisualStyleBackColor = true;
            this.btnPauseAskReserve.Click += new System.EventHandler(this.btnPauseAskReserve_Click);
            // 
            // btnAutoApplyReserveOnce
            // 
            this.btnAutoApplyReserveOnce.Location = new System.Drawing.Point(345, 200);
            this.btnAutoApplyReserveOnce.Name = "btnAutoApplyReserveOnce";
            this.btnAutoApplyReserveOnce.Size = new System.Drawing.Size(165, 36);
            this.btnAutoApplyReserveOnce.TabIndex = 46;
            this.btnAutoApplyReserveOnce.Text = "Auto Apply Reserve Once";
            this.btnAutoApplyReserveOnce.UseVisualStyleBackColor = true;
            this.btnAutoApplyReserveOnce.Click += new System.EventHandler(this.btnAutoApplyReserveOnce_Click);
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
            // btnUnloadOk
            // 
            this.btnUnloadOk.Location = new System.Drawing.Point(6, 278);
            this.btnUnloadOk.Name = "btnUnloadOk";
            this.btnUnloadOk.Size = new System.Drawing.Size(67, 33);
            this.btnUnloadOk.TabIndex = 60;
            this.btnUnloadOk.Text = "UnloadOk";
            this.btnUnloadOk.UseVisualStyleBackColor = true;
            this.btnUnloadOk.Visible = false;
            this.btnUnloadOk.Click += new System.EventHandler(this.btnUnloadOk_Click);
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
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
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
            this.gbVisitTransferSteps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picVisitTransferSteps)).EndInit();
            this.gbTrackPosition.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picTrackPosition)).EndInit();
            this.gbWatchLowPower.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picWatchLowPower)).EndInit();
            this.gbAskReserve.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picAskReserve)).EndInit();
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
        private System.Windows.Forms.Button btnStartTrackPosition;
        private System.Windows.Forms.Button btnStopVehicle;
        private System.Windows.Forms.Button btnResumeTrackPostiion;
        private System.Windows.Forms.Button btnPauseTrackPosition;
        private System.Windows.Forms.Button btnStopTrackPosition;
        private System.Windows.Forms.Button btnKeyInPosition;
        private System.Windows.Forms.ListBox lbxReserveOkSections;
        private System.Windows.Forms.ListBox lbxNeedReserveSections;
        private System.Windows.Forms.Timer timeUpdateUI;
        private System.Windows.Forms.NumericUpDown numPositionY;
        private System.Windows.Forms.NumericUpDown numPositionX;
        private System.Windows.Forms.GroupBox gbTrackPosition;
        private System.Windows.Forms.GroupBox gbPerformanceCounter;
        private UcLabelTextBox ucPerformanceCounterRam;
        private UcLabelTextBox ucPerformanceCounterCpu;
        private System.Windows.Forms.ToolStripMenuItem PlcPage;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.GroupBox gbVisitTransferSteps;
        private System.Windows.Forms.Button btnStartVisitTransferSteps;
        private System.Windows.Forms.Button btnResumeVisitTransferSteps;
        private System.Windows.Forms.Button btnStopVisitTransferSteps;
        private System.Windows.Forms.Button btnPauseVisitTransferSteps;
        private System.Windows.Forms.GroupBox gbAskReserve;
        private System.Windows.Forms.Button btnStartAskReserve;
        private System.Windows.Forms.Button btnResumeAskReserve;
        private System.Windows.Forms.Button btnStopAskReserve;
        private System.Windows.Forms.Button btnPauseAskReserve;
        private System.Windows.Forms.Button btnAutoApplyReserveOnce;
        private System.Windows.Forms.ListBox lbxAskReserveSection;
        private System.Windows.Forms.Label txtLastAlarm;
        private UcLabelTextBox ucSoc;
        private System.Windows.Forms.PictureBox pictureBox1;
        private UcLabelTextBox ucLoading;
        private System.Windows.Forms.Button btnAutoManual;
        private System.Windows.Forms.Button btnStopAndClear;
        private System.Windows.Forms.PictureBox picAskReserve;
        private System.Windows.Forms.PictureBox picVisitTransferSteps;
        private System.Windows.Forms.PictureBox picTrackPosition;
        private System.Windows.Forms.Label txtTransferStep;
        private System.Windows.Forms.GroupBox gbReserve;
        private System.Windows.Forms.Button btnGetReserveOkClear;
        private System.Windows.Forms.Button btnAskReserveClear;
        private System.Windows.Forms.Button btnNeedReserveClear;
        private System.Windows.Forms.Label txtTrackPosition;
        private System.Windows.Forms.Label txtAskingReserve;
        private System.Windows.Forms.RichTextBox rtbAgvcTransCmd;
        private System.Windows.Forms.RichTextBox rtbTransferStep;
        private System.Windows.Forms.GroupBox gbWatchLowPower;
        private System.Windows.Forms.Label txtWatchLowPower;
        private System.Windows.Forms.PictureBox picWatchLowPower;
        private System.Windows.Forms.Button btnStartWatchLowPower;
        private System.Windows.Forms.Button btnResumeWatchLowPower;
        private System.Windows.Forms.Button btnStopWatchLowPower;
        private System.Windows.Forms.Button btnPauseWatchLowPower;
        private System.Windows.Forms.NumericUpDown numSoc;
        private System.Windows.Forms.Button btnKeyInSoc;
        private UcLabelTextBox ucBarPos;
        private System.Windows.Forms.TextBox txtBarNum;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox ckAddress;
        private System.Windows.Forms.CheckBox ckSection;
        private System.Windows.Forms.CheckBox ckBarcode;
        private System.Windows.Forms.Button btnReDraw;
        private UcLabelTextBox ucCharging;
        private System.Windows.Forms.Button btnSemiAutoManual;
        private UcLabelTextBox ucDistance;
        private System.Windows.Forms.Label txtAgvcConnection;
        private System.Windows.Forms.CheckBox cbSimulationMode;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar tspbCommding;
        private System.Windows.Forms.ToolStripStatusLabel tstextClientName;
        private System.Windows.Forms.ToolStripStatusLabel tstextRemoteIp;
        private System.Windows.Forms.ToolStripStatusLabel tstextRemotePort;
        private System.Windows.Forms.ToolStripStatusLabel tstextRealPosX;
        private System.Windows.Forms.ToolStripStatusLabel tstextRealPosY;
        private System.Windows.Forms.Button btnLoadOk;
        private System.Windows.Forms.Button btnMoveOk;
        private System.Windows.Forms.Button btnUnloadOk;
    }
}