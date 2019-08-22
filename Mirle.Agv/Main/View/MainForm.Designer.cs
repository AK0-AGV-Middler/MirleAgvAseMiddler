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
            this.btnClearAgvcTransferCmd = new System.Windows.Forms.Button();
            this.btnAutoManual = new System.Windows.Forms.Button();
            this.btnTransferComplete = new System.Windows.Forms.Button();
            this.txtLastAlarm = new System.Windows.Forms.Label();
            this.btnUnloadFinish = new System.Windows.Forms.Button();
            this.btnLoadFinish = new System.Windows.Forms.Button();
            this.lbxAskReserveSection = new System.Windows.Forms.ListBox();
            this.btnAutoApplyReserve = new System.Windows.Forms.Button();
            this.gbAskReserve = new System.Windows.Forms.GroupBox();
            this.picAskReserve = new System.Windows.Forms.PictureBox();
            this.btnStartAskingReserve = new System.Windows.Forms.Button();
            this.btnResumeAskingReserve = new System.Windows.Forms.Button();
            this.btnStopAskingReserve = new System.Windows.Forms.Button();
            this.btnPauseAskingReserve = new System.Windows.Forms.Button();
            this.btnSetTestTransferCmd = new System.Windows.Forms.Button();
            this.gbVisitTransCmds = new System.Windows.Forms.GroupBox();
            this.picVisitTransferCmd = new System.Windows.Forms.PictureBox();
            this.btnStartVisitTransCmds = new System.Windows.Forms.Button();
            this.btnResumeVisitTransCmds = new System.Windows.Forms.Button();
            this.btnStopVisitTransCmds = new System.Windows.Forms.Button();
            this.btnPauseVisitTransCmds = new System.Windows.Forms.Button();
            this.btnMoveFinish = new System.Windows.Forms.Button();
            this.gbPerformanceCounter = new System.Windows.Forms.GroupBox();
            this.ucSoc = new Mirle.Agv.UcLabelTextBox();
            this.ucPerformanceCounterRam = new Mirle.Agv.UcLabelTextBox();
            this.ucPerformanceCounterCpu = new Mirle.Agv.UcLabelTextBox();
            this.gbTrackingPosition = new System.Windows.Forms.GroupBox();
            this.picTrackingPosition = new System.Windows.Forms.PictureBox();
            this.btnStartTrackingPosition = new System.Windows.Forms.Button();
            this.btnResumeTrackingPostiion = new System.Windows.Forms.Button();
            this.btnStopTrackingPosition = new System.Windows.Forms.Button();
            this.btnPauseTrackingPosition = new System.Windows.Forms.Button();
            this.btnStopVehicle = new System.Windows.Forms.Button();
            this.lbxReserveOkSections = new System.Windows.Forms.ListBox();
            this.lbxNeedReserveSections = new System.Windows.Forms.ListBox();
            this.btnBuzzOff = new System.Windows.Forms.Button();
            this.numPositionY = new System.Windows.Forms.NumericUpDown();
            this.btnAlarmReset = new System.Windows.Forms.Button();
            this.gbVehicleLocation = new System.Windows.Forms.GroupBox();
            this.ucLoading = new Mirle.Agv.UcLabelTextBox();
            this.ucRealPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucBarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.ucMapAddress = new Mirle.Agv.UcLabelTextBox();
            this.ucMapSection = new Mirle.Agv.UcLabelTextBox();
            this.numPositionX = new System.Windows.Forms.NumericUpDown();
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.radOnline = new System.Windows.Forms.RadioButton();
            this.radOffline = new System.Windows.Forms.RadioButton();
            this.btnSetPosition = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnTestSomething = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtTransferStep = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.gbAskReserve.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAskReserve)).BeginInit();
            this.gbVisitTransCmds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picVisitTransferCmd)).BeginInit();
            this.gbPerformanceCounter.SuspendLayout();
            this.gbTrackingPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTrackingPosition)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).BeginInit();
            this.gbVehicleLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).BeginInit();
            this.gbConnection.SuspendLayout();
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
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1161, 1024);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
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
            this.btnYflip.Location = new System.Drawing.Point(262, 62);
            this.btnYflip.Name = "btnYflip";
            this.btnYflip.Size = new System.Drawing.Size(114, 23);
            this.btnYflip.TabIndex = 33;
            this.btnYflip.Text = "垂直翻轉";
            this.btnYflip.UseVisualStyleBackColor = true;
            this.btnYflip.Click += new System.EventHandler(this.btnYflip_Click);
            // 
            // btnXflip
            // 
            this.btnXflip.Location = new System.Drawing.Point(142, 62);
            this.btnXflip.Name = "btnXflip";
            this.btnXflip.Size = new System.Drawing.Size(114, 23);
            this.btnXflip.TabIndex = 32;
            this.btnXflip.Text = "水平翻轉";
            this.btnXflip.UseVisualStyleBackColor = true;
            this.btnXflip.Click += new System.EventHandler(this.btnXflip_Click);
            // 
            // txtResizePercent
            // 
            this.txtResizePercent.Location = new System.Drawing.Point(264, 4);
            this.txtResizePercent.Name = "txtResizePercent";
            this.txtResizePercent.ShortcutsEnabled = false;
            this.txtResizePercent.Size = new System.Drawing.Size(112, 22);
            this.txtResizePercent.TabIndex = 30;
            this.txtResizePercent.Text = "150";
            this.txtResizePercent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtRotateAngle
            // 
            this.txtRotateAngle.Location = new System.Drawing.Point(264, 34);
            this.txtRotateAngle.Name = "txtRotateAngle";
            this.txtRotateAngle.ShortcutsEnabled = false;
            this.txtRotateAngle.Size = new System.Drawing.Size(112, 22);
            this.txtRotateAngle.TabIndex = 31;
            this.txtRotateAngle.Text = "1";
            this.txtRotateAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnRotate
            // 
            this.btnRotate.Location = new System.Drawing.Point(142, 32);
            this.btnRotate.Name = "btnRotate";
            this.btnRotate.Size = new System.Drawing.Size(114, 23);
            this.btnRotate.TabIndex = 29;
            this.btnRotate.Text = "旋轉";
            this.btnRotate.UseVisualStyleBackColor = true;
            this.btnRotate.Click += new System.EventHandler(this.btnRotate_Click);
            // 
            // btnResizePercent
            // 
            this.btnResizePercent.Location = new System.Drawing.Point(142, 3);
            this.btnResizePercent.Name = "btnResizePercent";
            this.btnResizePercent.Size = new System.Drawing.Size(114, 23);
            this.btnResizePercent.TabIndex = 27;
            this.btnResizePercent.Text = "比例縮放";
            this.btnResizePercent.UseVisualStyleBackColor = true;
            this.btnResizePercent.Click += new System.EventHandler(this.btnResizePercent_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(3, 32);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(133, 23);
            this.btnReset.TabIndex = 26;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.Location = new System.Drawing.Point(3, 61);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(133, 23);
            this.btnSaveImage.TabIndex = 25;
            this.btnSaveImage.Text = "Save Image";
            this.btnSaveImage.UseVisualStyleBackColor = true;
            // 
            // btnSwitchBarcodeLine
            // 
            this.btnSwitchBarcodeLine.Location = new System.Drawing.Point(3, 3);
            this.btnSwitchBarcodeLine.Name = "btnSwitchBarcodeLine";
            this.btnSwitchBarcodeLine.Size = new System.Drawing.Size(133, 23);
            this.btnSwitchBarcodeLine.TabIndex = 24;
            this.btnSwitchBarcodeLine.Text = "切換BarcodeLine";
            this.btnSwitchBarcodeLine.UseVisualStyleBackColor = true;
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
            this.splitContainer2.Panel1.Controls.Add(this.btnClearAgvcTransferCmd);
            this.splitContainer2.Panel1.Controls.Add(this.btnAutoManual);
            this.splitContainer2.Panel1.Controls.Add(this.btnTransferComplete);
            this.splitContainer2.Panel1.Controls.Add(this.txtLastAlarm);
            this.splitContainer2.Panel1.Controls.Add(this.btnUnloadFinish);
            this.splitContainer2.Panel1.Controls.Add(this.btnLoadFinish);
            this.splitContainer2.Panel1.Controls.Add(this.lbxAskReserveSection);
            this.splitContainer2.Panel1.Controls.Add(this.btnAutoApplyReserve);
            this.splitContainer2.Panel1.Controls.Add(this.gbAskReserve);
            this.splitContainer2.Panel1.Controls.Add(this.btnSetTestTransferCmd);
            this.splitContainer2.Panel1.Controls.Add(this.gbVisitTransCmds);
            this.splitContainer2.Panel1.Controls.Add(this.btnMoveFinish);
            this.splitContainer2.Panel1.Controls.Add(this.gbPerformanceCounter);
            this.splitContainer2.Panel1.Controls.Add(this.gbTrackingPosition);
            this.splitContainer2.Panel1.Controls.Add(this.btnStopVehicle);
            this.splitContainer2.Panel1.Controls.Add(this.lbxReserveOkSections);
            this.splitContainer2.Panel1.Controls.Add(this.lbxNeedReserveSections);
            this.splitContainer2.Panel1.Controls.Add(this.btnBuzzOff);
            this.splitContainer2.Panel1.Controls.Add(this.numPositionY);
            this.splitContainer2.Panel1.Controls.Add(this.btnAlarmReset);
            this.splitContainer2.Panel1.Controls.Add(this.gbVehicleLocation);
            this.splitContainer2.Panel1.Controls.Add(this.numPositionX);
            this.splitContainer2.Panel1.Controls.Add(this.gbConnection);
            this.splitContainer2.Panel1.Controls.Add(this.btnSetPosition);
            this.splitContainer2.Panel1.Controls.Add(this.label3);
            this.splitContainer2.Panel1.Controls.Add(this.label4);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.AutoScroll = true;
            this.splitContainer2.Panel2.Controls.Add(this.btnTestSomething);
            this.splitContainer2.Size = new System.Drawing.Size(722, 1017);
            this.splitContainer2.SplitterDistance = 677;
            this.splitContainer2.TabIndex = 0;
            // 
            // btnClearAgvcTransferCmd
            // 
            this.btnClearAgvcTransferCmd.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearAgvcTransferCmd.ForeColor = System.Drawing.Color.Red;
            this.btnClearAgvcTransferCmd.Location = new System.Drawing.Point(411, 131);
            this.btnClearAgvcTransferCmd.Name = "btnClearAgvcTransferCmd";
            this.btnClearAgvcTransferCmd.Size = new System.Drawing.Size(151, 105);
            this.btnClearAgvcTransferCmd.TabIndex = 54;
            this.btnClearAgvcTransferCmd.Text = "Clear Transfer Command";
            this.btnClearAgvcTransferCmd.UseVisualStyleBackColor = true;
            this.btnClearAgvcTransferCmd.Click += new System.EventHandler(this.btnClearAgvcTransferCmd_Click);
            // 
            // btnAutoManual
            // 
            this.btnAutoManual.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAutoManual.Location = new System.Drawing.Point(218, 3);
            this.btnAutoManual.Name = "btnAutoManual";
            this.btnAutoManual.Size = new System.Drawing.Size(187, 65);
            this.btnAutoManual.TabIndex = 53;
            this.btnAutoManual.Text = "Auto/Manual";
            this.btnAutoManual.UseVisualStyleBackColor = true;
            this.btnAutoManual.Click += new System.EventHandler(this.btnAutoManual_Click);
            // 
            // btnTransferComplete
            // 
            this.btnTransferComplete.Location = new System.Drawing.Point(619, 609);
            this.btnTransferComplete.Name = "btnTransferComplete";
            this.btnTransferComplete.Size = new System.Drawing.Size(91, 45);
            this.btnTransferComplete.TabIndex = 51;
            this.btnTransferComplete.Text = "Transfer Complete";
            this.btnTransferComplete.UseVisualStyleBackColor = true;
            this.btnTransferComplete.Click += new System.EventHandler(this.btnTransferComplete_Click);
            // 
            // txtLastAlarm
            // 
            this.txtLastAlarm.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtLastAlarm.Location = new System.Drawing.Point(571, 167);
            this.txtLastAlarm.Name = "txtLastAlarm";
            this.txtLastAlarm.Size = new System.Drawing.Size(148, 60);
            this.txtLastAlarm.TabIndex = 50;
            this.txtLastAlarm.Text = "Last Alarm";
            // 
            // btnUnloadFinish
            // 
            this.btnUnloadFinish.Location = new System.Drawing.Point(619, 554);
            this.btnUnloadFinish.Name = "btnUnloadFinish";
            this.btnUnloadFinish.Size = new System.Drawing.Size(91, 45);
            this.btnUnloadFinish.TabIndex = 49;
            this.btnUnloadFinish.Text = "UnloadFinish";
            this.btnUnloadFinish.UseVisualStyleBackColor = true;
            this.btnUnloadFinish.Click += new System.EventHandler(this.btnUnloadFinish_Click);
            // 
            // btnLoadFinish
            // 
            this.btnLoadFinish.Location = new System.Drawing.Point(619, 503);
            this.btnLoadFinish.Name = "btnLoadFinish";
            this.btnLoadFinish.Size = new System.Drawing.Size(91, 45);
            this.btnLoadFinish.TabIndex = 48;
            this.btnLoadFinish.Text = "LoadFinish";
            this.btnLoadFinish.UseVisualStyleBackColor = true;
            this.btnLoadFinish.Click += new System.EventHandler(this.btnLoadFinish_Click);
            // 
            // lbxAskReserveSection
            // 
            this.lbxAskReserveSection.FormattingEnabled = true;
            this.lbxAskReserveSection.ItemHeight = 12;
            this.lbxAskReserveSection.Items.AddRange(new object[] {
            "Empty"});
            this.lbxAskReserveSection.Location = new System.Drawing.Point(330, 453);
            this.lbxAskReserveSection.Name = "lbxAskReserveSection";
            this.lbxAskReserveSection.ScrollAlwaysVisible = true;
            this.lbxAskReserveSection.Size = new System.Drawing.Size(134, 172);
            this.lbxAskReserveSection.TabIndex = 47;
            // 
            // btnAutoApplyReserve
            // 
            this.btnAutoApplyReserve.Location = new System.Drawing.Point(393, 408);
            this.btnAutoApplyReserve.Name = "btnAutoApplyReserve";
            this.btnAutoApplyReserve.Size = new System.Drawing.Size(165, 23);
            this.btnAutoApplyReserve.TabIndex = 46;
            this.btnAutoApplyReserve.Text = "Auto Apply Reserve Once";
            this.btnAutoApplyReserve.UseVisualStyleBackColor = true;
            this.btnAutoApplyReserve.Click += new System.EventHandler(this.btnAutoApplyReserve_Click);
            // 
            // gbAskReserve
            // 
            this.gbAskReserve.Controls.Add(this.picAskReserve);
            this.gbAskReserve.Controls.Add(this.btnStartAskingReserve);
            this.gbAskReserve.Controls.Add(this.btnResumeAskingReserve);
            this.gbAskReserve.Controls.Add(this.btnStopAskingReserve);
            this.gbAskReserve.Controls.Add(this.btnPauseAskingReserve);
            this.gbAskReserve.Location = new System.Drawing.Point(393, 251);
            this.gbAskReserve.Name = "gbAskReserve";
            this.gbAskReserve.Size = new System.Drawing.Size(165, 154);
            this.gbAskReserve.TabIndex = 43;
            this.gbAskReserve.TabStop = false;
            this.gbAskReserve.Text = "Ask Reserve";
            // 
            // picAskReserve
            // 
            this.picAskReserve.Location = new System.Drawing.Point(6, 135);
            this.picAskReserve.Name = "picAskReserve";
            this.picAskReserve.Size = new System.Drawing.Size(150, 16);
            this.picAskReserve.TabIndex = 56;
            this.picAskReserve.TabStop = false;
            // 
            // btnStartAskingReserve
            // 
            this.btnStartAskingReserve.Location = new System.Drawing.Point(6, 21);
            this.btnStartAskingReserve.Name = "btnStartAskingReserve";
            this.btnStartAskingReserve.Size = new System.Drawing.Size(150, 23);
            this.btnStartAskingReserve.TabIndex = 5;
            this.btnStartAskingReserve.Text = "Start Asking Reserve";
            this.btnStartAskingReserve.UseVisualStyleBackColor = true;
            this.btnStartAskingReserve.Click += new System.EventHandler(this.btnStartAskingReserve_Click);
            // 
            // btnResumeAskingReserve
            // 
            this.btnResumeAskingReserve.Location = new System.Drawing.Point(6, 79);
            this.btnResumeAskingReserve.Name = "btnResumeAskingReserve";
            this.btnResumeAskingReserve.Size = new System.Drawing.Size(150, 23);
            this.btnResumeAskingReserve.TabIndex = 8;
            this.btnResumeAskingReserve.Text = "Resume Asking Reserve";
            this.btnResumeAskingReserve.UseVisualStyleBackColor = true;
            this.btnResumeAskingReserve.Click += new System.EventHandler(this.btnResumeAskingReserve_Click);
            // 
            // btnStopAskingReserve
            // 
            this.btnStopAskingReserve.Location = new System.Drawing.Point(6, 108);
            this.btnStopAskingReserve.Name = "btnStopAskingReserve";
            this.btnStopAskingReserve.Size = new System.Drawing.Size(150, 23);
            this.btnStopAskingReserve.TabIndex = 6;
            this.btnStopAskingReserve.Text = "Stop Asking Reserve";
            this.btnStopAskingReserve.UseVisualStyleBackColor = true;
            this.btnStopAskingReserve.Click += new System.EventHandler(this.btnStopAskingReserve_Click);
            // 
            // btnPauseAskingReserve
            // 
            this.btnPauseAskingReserve.Location = new System.Drawing.Point(6, 50);
            this.btnPauseAskingReserve.Name = "btnPauseAskingReserve";
            this.btnPauseAskingReserve.Size = new System.Drawing.Size(150, 23);
            this.btnPauseAskingReserve.TabIndex = 7;
            this.btnPauseAskingReserve.Text = "Pause Asking Reserve";
            this.btnPauseAskingReserve.UseVisualStyleBackColor = true;
            this.btnPauseAskingReserve.Click += new System.EventHandler(this.btnPauseAskingReserve_Click);
            // 
            // btnSetTestTransferCmd
            // 
            this.btnSetTestTransferCmd.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSetTestTransferCmd.ForeColor = System.Drawing.Color.Green;
            this.btnSetTestTransferCmd.Location = new System.Drawing.Point(574, 339);
            this.btnSetTestTransferCmd.Name = "btnSetTestTransferCmd";
            this.btnSetTestTransferCmd.Size = new System.Drawing.Size(136, 108);
            this.btnSetTestTransferCmd.TabIndex = 45;
            this.btnSetTestTransferCmd.Text = "Set Test Transfer Cmd";
            this.btnSetTestTransferCmd.UseVisualStyleBackColor = true;
            this.btnSetTestTransferCmd.Click += new System.EventHandler(this.btnSetTestTransferCmd_Click);
            // 
            // gbVisitTransCmds
            // 
            this.gbVisitTransCmds.Controls.Add(this.txtTransferStep);
            this.gbVisitTransCmds.Controls.Add(this.picVisitTransferCmd);
            this.gbVisitTransCmds.Controls.Add(this.btnStartVisitTransCmds);
            this.gbVisitTransCmds.Controls.Add(this.btnResumeVisitTransCmds);
            this.gbVisitTransCmds.Controls.Add(this.btnStopVisitTransCmds);
            this.gbVisitTransCmds.Controls.Add(this.btnPauseVisitTransCmds);
            this.gbVisitTransCmds.Location = new System.Drawing.Point(8, 453);
            this.gbVisitTransCmds.Name = "gbVisitTransCmds";
            this.gbVisitTransCmds.Size = new System.Drawing.Size(165, 182);
            this.gbVisitTransCmds.TabIndex = 43;
            this.gbVisitTransCmds.TabStop = false;
            this.gbVisitTransCmds.Text = "Visit Trans Cmds";
            // 
            // picVisitTransferCmd
            // 
            this.picVisitTransferCmd.Location = new System.Drawing.Point(6, 137);
            this.picVisitTransferCmd.Name = "picVisitTransferCmd";
            this.picVisitTransferCmd.Size = new System.Drawing.Size(150, 16);
            this.picVisitTransferCmd.TabIndex = 55;
            this.picVisitTransferCmd.TabStop = false;
            // 
            // btnStartVisitTransCmds
            // 
            this.btnStartVisitTransCmds.Location = new System.Drawing.Point(6, 21);
            this.btnStartVisitTransCmds.Name = "btnStartVisitTransCmds";
            this.btnStartVisitTransCmds.Size = new System.Drawing.Size(150, 23);
            this.btnStartVisitTransCmds.TabIndex = 5;
            this.btnStartVisitTransCmds.Text = "Start Visit Trans Cmds";
            this.btnStartVisitTransCmds.UseVisualStyleBackColor = true;
            this.btnStartVisitTransCmds.Click += new System.EventHandler(this.btnStartVisitTransCmds_Click);
            // 
            // btnResumeVisitTransCmds
            // 
            this.btnResumeVisitTransCmds.Location = new System.Drawing.Point(6, 79);
            this.btnResumeVisitTransCmds.Name = "btnResumeVisitTransCmds";
            this.btnResumeVisitTransCmds.Size = new System.Drawing.Size(150, 23);
            this.btnResumeVisitTransCmds.TabIndex = 8;
            this.btnResumeVisitTransCmds.Text = "Resume Visit Trans Cmds";
            this.btnResumeVisitTransCmds.UseVisualStyleBackColor = true;
            this.btnResumeVisitTransCmds.Click += new System.EventHandler(this.btnResumeVisitTransCmds_Click);
            // 
            // btnStopVisitTransCmds
            // 
            this.btnStopVisitTransCmds.Location = new System.Drawing.Point(6, 108);
            this.btnStopVisitTransCmds.Name = "btnStopVisitTransCmds";
            this.btnStopVisitTransCmds.Size = new System.Drawing.Size(150, 23);
            this.btnStopVisitTransCmds.TabIndex = 6;
            this.btnStopVisitTransCmds.Text = "Stop Visit Trans Cmds";
            this.btnStopVisitTransCmds.UseVisualStyleBackColor = true;
            this.btnStopVisitTransCmds.Click += new System.EventHandler(this.btnStopVisitTransCmds_Click);
            // 
            // btnPauseVisitTransCmds
            // 
            this.btnPauseVisitTransCmds.Location = new System.Drawing.Point(6, 50);
            this.btnPauseVisitTransCmds.Name = "btnPauseVisitTransCmds";
            this.btnPauseVisitTransCmds.Size = new System.Drawing.Size(150, 23);
            this.btnPauseVisitTransCmds.TabIndex = 7;
            this.btnPauseVisitTransCmds.Text = "Pause Visit Trans Cmds";
            this.btnPauseVisitTransCmds.UseVisualStyleBackColor = true;
            this.btnPauseVisitTransCmds.Click += new System.EventHandler(this.btnPauseVisitTransCmds_Click);
            // 
            // btnMoveFinish
            // 
            this.btnMoveFinish.Location = new System.Drawing.Point(619, 453);
            this.btnMoveFinish.Name = "btnMoveFinish";
            this.btnMoveFinish.Size = new System.Drawing.Size(91, 45);
            this.btnMoveFinish.TabIndex = 0;
            this.btnMoveFinish.Text = "MoveFinish";
            this.btnMoveFinish.UseVisualStyleBackColor = true;
            this.btnMoveFinish.Click += new System.EventHandler(this.btnMoveFinish_Click);
            // 
            // gbPerformanceCounter
            // 
            this.gbPerformanceCounter.Controls.Add(this.ucSoc);
            this.gbPerformanceCounter.Controls.Add(this.ucPerformanceCounterRam);
            this.gbPerformanceCounter.Controls.Add(this.ucPerformanceCounterCpu);
            this.gbPerformanceCounter.Location = new System.Drawing.Point(12, 268);
            this.gbPerformanceCounter.Name = "gbPerformanceCounter";
            this.gbPerformanceCounter.Size = new System.Drawing.Size(200, 128);
            this.gbPerformanceCounter.TabIndex = 10;
            this.gbPerformanceCounter.TabStop = false;
            this.gbPerformanceCounter.Text = "Performance Counter";
            // 
            // ucSoc
            // 
            this.ucSoc.Location = new System.Drawing.Point(7, 91);
            this.ucSoc.Name = "ucSoc";
            this.ucSoc.Size = new System.Drawing.Size(187, 30);
            this.ucSoc.TabIndex = 2;
            this.ucSoc.TagName = "SOC";
            this.ucSoc.TagValue = "";
            // 
            // ucPerformanceCounterRam
            // 
            this.ucPerformanceCounterRam.Location = new System.Drawing.Point(7, 55);
            this.ucPerformanceCounterRam.Name = "ucPerformanceCounterRam";
            this.ucPerformanceCounterRam.Size = new System.Drawing.Size(187, 30);
            this.ucPerformanceCounterRam.TabIndex = 1;
            this.ucPerformanceCounterRam.TagName = "RAM";
            this.ucPerformanceCounterRam.TagValue = "";
            // 
            // ucPerformanceCounterCpu
            // 
            this.ucPerformanceCounterCpu.Location = new System.Drawing.Point(6, 19);
            this.ucPerformanceCounterCpu.Name = "ucPerformanceCounterCpu";
            this.ucPerformanceCounterCpu.Size = new System.Drawing.Size(187, 30);
            this.ucPerformanceCounterCpu.TabIndex = 0;
            this.ucPerformanceCounterCpu.TagName = "CPU";
            this.ucPerformanceCounterCpu.TagValue = "";
            // 
            // gbTrackingPosition
            // 
            this.gbTrackingPosition.Controls.Add(this.picTrackingPosition);
            this.gbTrackingPosition.Controls.Add(this.btnStartTrackingPosition);
            this.gbTrackingPosition.Controls.Add(this.btnResumeTrackingPostiion);
            this.gbTrackingPosition.Controls.Add(this.btnStopTrackingPosition);
            this.gbTrackingPosition.Controls.Add(this.btnPauseTrackingPosition);
            this.gbTrackingPosition.Location = new System.Drawing.Point(222, 251);
            this.gbTrackingPosition.Name = "gbTrackingPosition";
            this.gbTrackingPosition.Size = new System.Drawing.Size(165, 154);
            this.gbTrackingPosition.TabIndex = 42;
            this.gbTrackingPosition.TabStop = false;
            this.gbTrackingPosition.Text = "Tracking Position";
            // 
            // picTrackingPosition
            // 
            this.picTrackingPosition.Location = new System.Drawing.Point(6, 135);
            this.picTrackingPosition.Name = "picTrackingPosition";
            this.picTrackingPosition.Size = new System.Drawing.Size(150, 16);
            this.picTrackingPosition.TabIndex = 56;
            this.picTrackingPosition.TabStop = false;
            // 
            // btnStartTrackingPosition
            // 
            this.btnStartTrackingPosition.Location = new System.Drawing.Point(6, 21);
            this.btnStartTrackingPosition.Name = "btnStartTrackingPosition";
            this.btnStartTrackingPosition.Size = new System.Drawing.Size(150, 23);
            this.btnStartTrackingPosition.TabIndex = 5;
            this.btnStartTrackingPosition.Text = "Start Tracking Position";
            this.btnStartTrackingPosition.UseVisualStyleBackColor = true;
            this.btnStartTrackingPosition.Click += new System.EventHandler(this.btnStartTrackingPosition_Click);
            // 
            // btnResumeTrackingPostiion
            // 
            this.btnResumeTrackingPostiion.Location = new System.Drawing.Point(6, 79);
            this.btnResumeTrackingPostiion.Name = "btnResumeTrackingPostiion";
            this.btnResumeTrackingPostiion.Size = new System.Drawing.Size(150, 23);
            this.btnResumeTrackingPostiion.TabIndex = 8;
            this.btnResumeTrackingPostiion.Text = "Resume Tracking Position";
            this.btnResumeTrackingPostiion.UseVisualStyleBackColor = true;
            this.btnResumeTrackingPostiion.Click += new System.EventHandler(this.btnResumeTrackingPostiion_Click);
            // 
            // btnStopTrackingPosition
            // 
            this.btnStopTrackingPosition.Location = new System.Drawing.Point(6, 108);
            this.btnStopTrackingPosition.Name = "btnStopTrackingPosition";
            this.btnStopTrackingPosition.Size = new System.Drawing.Size(150, 23);
            this.btnStopTrackingPosition.TabIndex = 6;
            this.btnStopTrackingPosition.Text = "Stop Tracking Position";
            this.btnStopTrackingPosition.UseVisualStyleBackColor = true;
            this.btnStopTrackingPosition.Click += new System.EventHandler(this.btnStopTrackingPosition_Click);
            // 
            // btnPauseTrackingPosition
            // 
            this.btnPauseTrackingPosition.Location = new System.Drawing.Point(6, 50);
            this.btnPauseTrackingPosition.Name = "btnPauseTrackingPosition";
            this.btnPauseTrackingPosition.Size = new System.Drawing.Size(150, 23);
            this.btnPauseTrackingPosition.TabIndex = 7;
            this.btnPauseTrackingPosition.Text = "Pause Tracking Position";
            this.btnPauseTrackingPosition.UseVisualStyleBackColor = true;
            this.btnPauseTrackingPosition.Click += new System.EventHandler(this.btnPauseTrackingPosition_Click);
            // 
            // btnStopVehicle
            // 
            this.btnStopVehicle.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStopVehicle.ForeColor = System.Drawing.Color.Red;
            this.btnStopVehicle.Location = new System.Drawing.Point(411, 3);
            this.btnStopVehicle.Name = "btnStopVehicle";
            this.btnStopVehicle.Size = new System.Drawing.Size(151, 122);
            this.btnStopVehicle.TabIndex = 4;
            this.btnStopVehicle.Text = "StopVehicle";
            this.btnStopVehicle.UseVisualStyleBackColor = true;
            // 
            // lbxReserveOkSections
            // 
            this.lbxReserveOkSections.FormattingEnabled = true;
            this.lbxReserveOkSections.ItemHeight = 12;
            this.lbxReserveOkSections.Location = new System.Drawing.Point(470, 453);
            this.lbxReserveOkSections.Name = "lbxReserveOkSections";
            this.lbxReserveOkSections.ScrollAlwaysVisible = true;
            this.lbxReserveOkSections.Size = new System.Drawing.Size(134, 172);
            this.lbxReserveOkSections.TabIndex = 42;
            // 
            // lbxNeedReserveSections
            // 
            this.lbxNeedReserveSections.FormattingEnabled = true;
            this.lbxNeedReserveSections.ItemHeight = 12;
            this.lbxNeedReserveSections.Location = new System.Drawing.Point(179, 454);
            this.lbxNeedReserveSections.Name = "lbxNeedReserveSections";
            this.lbxNeedReserveSections.ScrollAlwaysVisible = true;
            this.lbxNeedReserveSections.Size = new System.Drawing.Size(144, 172);
            this.lbxNeedReserveSections.TabIndex = 41;
            // 
            // btnBuzzOff
            // 
            this.btnBuzzOff.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBuzzOff.ForeColor = System.Drawing.Color.Brown;
            this.btnBuzzOff.Location = new System.Drawing.Point(568, 131);
            this.btnBuzzOff.Name = "btnBuzzOff";
            this.btnBuzzOff.Size = new System.Drawing.Size(151, 33);
            this.btnBuzzOff.TabIndex = 3;
            this.btnBuzzOff.Text = "Buzz Off";
            this.btnBuzzOff.UseVisualStyleBackColor = true;
            this.btnBuzzOff.Click += new System.EventHandler(this.btnBuzzOff_Click);
            // 
            // numPositionY
            // 
            this.numPositionY.Location = new System.Drawing.Point(164, 411);
            this.numPositionY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numPositionY.Name = "numPositionY";
            this.numPositionY.Size = new System.Drawing.Size(92, 22);
            this.numPositionY.TabIndex = 41;
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
            this.gbVehicleLocation.Controls.Add(this.ucLoading);
            this.gbVehicleLocation.Controls.Add(this.ucRealPosition);
            this.gbVehicleLocation.Controls.Add(this.ucBarcodePosition);
            this.gbVehicleLocation.Controls.Add(this.ucMapAddress);
            this.gbVehicleLocation.Controls.Add(this.ucMapSection);
            this.gbVehicleLocation.Location = new System.Drawing.Point(12, 68);
            this.gbVehicleLocation.Name = "gbVehicleLocation";
            this.gbVehicleLocation.Size = new System.Drawing.Size(200, 177);
            this.gbVehicleLocation.TabIndex = 1;
            this.gbVehicleLocation.TabStop = false;
            this.gbVehicleLocation.Text = "VehicleLocation";
            // 
            // ucLoading
            // 
            this.ucLoading.Location = new System.Drawing.Point(0, 145);
            this.ucLoading.Name = "ucLoading";
            this.ucLoading.Size = new System.Drawing.Size(194, 26);
            this.ucLoading.TabIndex = 5;
            this.ucLoading.TagName = "Loading";
            this.ucLoading.TagValue = "";
            // 
            // ucRealPosition
            // 
            this.ucRealPosition.Location = new System.Drawing.Point(0, 117);
            this.ucRealPosition.Name = "ucRealPosition";
            this.ucRealPosition.Size = new System.Drawing.Size(194, 26);
            this.ucRealPosition.TabIndex = 5;
            this.ucRealPosition.TagName = "Real Pos";
            this.ucRealPosition.TagValue = "";
            // 
            // ucBarcodePosition
            // 
            this.ucBarcodePosition.Location = new System.Drawing.Point(0, 85);
            this.ucBarcodePosition.Name = "ucBarcodePosition";
            this.ucBarcodePosition.Size = new System.Drawing.Size(194, 26);
            this.ucBarcodePosition.TabIndex = 3;
            this.ucBarcodePosition.TagName = "Barcode Pos";
            this.ucBarcodePosition.TagValue = "";
            // 
            // ucMapAddress
            // 
            this.ucMapAddress.Location = new System.Drawing.Point(0, 53);
            this.ucMapAddress.Name = "ucMapAddress";
            this.ucMapAddress.Size = new System.Drawing.Size(194, 26);
            this.ucMapAddress.TabIndex = 1;
            this.ucMapAddress.TagName = "Last Address";
            this.ucMapAddress.TagValue = "";
            // 
            // ucMapSection
            // 
            this.ucMapSection.Location = new System.Drawing.Point(0, 21);
            this.ucMapSection.Name = "ucMapSection";
            this.ucMapSection.Size = new System.Drawing.Size(194, 26);
            this.ucMapSection.TabIndex = 0;
            this.ucMapSection.TagName = "Last Section";
            this.ucMapSection.TagValue = "";
            // 
            // numPositionX
            // 
            this.numPositionX.Location = new System.Drawing.Point(39, 412);
            this.numPositionX.Name = "numPositionX";
            this.numPositionX.Size = new System.Drawing.Size(92, 22);
            this.numPositionX.TabIndex = 41;
            // 
            // gbConnection
            // 
            this.gbConnection.Controls.Add(this.radOnline);
            this.gbConnection.Controls.Add(this.radOffline);
            this.gbConnection.Location = new System.Drawing.Point(12, 14);
            this.gbConnection.Name = "gbConnection";
            this.gbConnection.Size = new System.Drawing.Size(200, 48);
            this.gbConnection.TabIndex = 0;
            this.gbConnection.TabStop = false;
            this.gbConnection.Text = "Connection";
            // 
            // radOnline
            // 
            this.radOnline.AutoSize = true;
            this.radOnline.Location = new System.Drawing.Point(68, 21);
            this.radOnline.Name = "radOnline";
            this.radOnline.Size = new System.Drawing.Size(54, 16);
            this.radOnline.TabIndex = 1;
            this.radOnline.TabStop = true;
            this.radOnline.Text = "Online";
            this.radOnline.UseVisualStyleBackColor = true;
            // 
            // radOffline
            // 
            this.radOffline.AutoSize = true;
            this.radOffline.Location = new System.Drawing.Point(6, 21);
            this.radOffline.Name = "radOffline";
            this.radOffline.Size = new System.Drawing.Size(56, 16);
            this.radOffline.TabIndex = 0;
            this.radOffline.TabStop = true;
            this.radOffline.Text = "Offline";
            this.radOffline.UseVisualStyleBackColor = true;
            // 
            // btnSetPosition
            // 
            this.btnSetPosition.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSetPosition.Location = new System.Drawing.Point(268, 411);
            this.btnSetPosition.Name = "btnSetPosition";
            this.btnSetPosition.Size = new System.Drawing.Size(115, 23);
            this.btnSetPosition.TabIndex = 40;
            this.btnSetPosition.Text = "鍵入車輛位置";
            this.btnSetPosition.UseVisualStyleBackColor = true;
            this.btnSetPosition.Click += new System.EventHandler(this.btnSetPosition_Click_1);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(13, 411);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 22);
            this.label3.TabIndex = 21;
            this.label3.Text = "X";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(137, 411);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(22, 22);
            this.label4.TabIndex = 23;
            this.label4.Text = "Y";
            // 
            // btnTestSomething
            // 
            this.btnTestSomething.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnTestSomething.ForeColor = System.Drawing.Color.Green;
            this.btnTestSomething.Location = new System.Drawing.Point(8, 11);
            this.btnTestSomething.Name = "btnTestSomething";
            this.btnTestSomething.Size = new System.Drawing.Size(187, 73);
            this.btnTestSomething.TabIndex = 55;
            this.btnTestSomething.Text = "Test Button";
            this.btnTestSomething.UseVisualStyleBackColor = true;
            this.btnTestSomething.Click += new System.EventHandler(this.btnTestSomething_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txtTransferStep
            // 
            this.txtTransferStep.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTransferStep.Location = new System.Drawing.Point(6, 156);
            this.txtTransferStep.Name = "txtTransferStep";
            this.txtTransferStep.Size = new System.Drawing.Size(150, 23);
            this.txtTransferStep.TabIndex = 56;
            this.txtTransferStep.Text = "TransferStep";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
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
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.gbAskReserve.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picAskReserve)).EndInit();
            this.gbVisitTransCmds.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picVisitTransferCmd)).EndInit();
            this.gbPerformanceCounter.ResumeLayout(false);
            this.gbTrackingPosition.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picTrackingPosition)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).EndInit();
            this.gbVehicleLocation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).EndInit();
            this.gbConnection.ResumeLayout(false);
            this.gbConnection.PerformLayout();
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
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
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
        private System.Windows.Forms.Button btnStartTrackingPosition;
        private System.Windows.Forms.Button btnStopVehicle;
        private System.Windows.Forms.Button btnResumeTrackingPostiion;
        private System.Windows.Forms.Button btnPauseTrackingPosition;
        private System.Windows.Forms.Button btnStopTrackingPosition;
        private System.Windows.Forms.Button btnSetPosition;
        private System.Windows.Forms.ListBox lbxReserveOkSections;
        private System.Windows.Forms.ListBox lbxNeedReserveSections;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NumericUpDown numPositionY;
        private System.Windows.Forms.NumericUpDown numPositionX;
        private System.Windows.Forms.GroupBox gbTrackingPosition;
        private System.Windows.Forms.GroupBox gbPerformanceCounter;
        private UcLabelTextBox ucPerformanceCounterRam;
        private UcLabelTextBox ucPerformanceCounterCpu;
        private System.Windows.Forms.ToolStripMenuItem PlcPage;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnMoveFinish;
        private System.Windows.Forms.GroupBox gbVisitTransCmds;
        private System.Windows.Forms.Button btnStartVisitTransCmds;
        private System.Windows.Forms.Button btnResumeVisitTransCmds;
        private System.Windows.Forms.Button btnStopVisitTransCmds;
        private System.Windows.Forms.Button btnPauseVisitTransCmds;
        private System.Windows.Forms.Button btnSetTestTransferCmd;
        private System.Windows.Forms.GroupBox gbAskReserve;
        private System.Windows.Forms.Button btnStartAskingReserve;
        private System.Windows.Forms.Button btnResumeAskingReserve;
        private System.Windows.Forms.Button btnStopAskingReserve;
        private System.Windows.Forms.Button btnPauseAskingReserve;
        private System.Windows.Forms.Button btnAutoApplyReserve;
        private System.Windows.Forms.ListBox lbxAskReserveSection;
        private System.Windows.Forms.Button btnLoadFinish;
        private System.Windows.Forms.Button btnUnloadFinish;
        private System.Windows.Forms.Label txtLastAlarm;
        private System.Windows.Forms.Button btnTransferComplete;
        private UcLabelTextBox ucSoc;
        private System.Windows.Forms.PictureBox pictureBox1;
        private UcLabelTextBox ucLoading;
        private System.Windows.Forms.Button btnAutoManual;
        private System.Windows.Forms.Button btnClearAgvcTransferCmd;
        private System.Windows.Forms.PictureBox picAskReserve;
        private System.Windows.Forms.PictureBox picVisitTransferCmd;
        private System.Windows.Forms.PictureBox picTrackingPosition;
        private System.Windows.Forms.Button btnTestSomething;
        private System.Windows.Forms.Label txtTransferStep;
    }
}