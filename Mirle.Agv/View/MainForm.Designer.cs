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
            this.listAskingReserveSections = new System.Windows.Forms.ListBox();
            this.gbPerformanceCounter = new System.Windows.Forms.GroupBox();
            this.gbTrackingPosition = new System.Windows.Forms.GroupBox();
            this.btnStartTrackingPosition = new System.Windows.Forms.Button();
            this.btnResumeTrackingPostiion = new System.Windows.Forms.Button();
            this.btnStopTrackingPosition = new System.Windows.Forms.Button();
            this.btnPauseTrackingPosition = new System.Windows.Forms.Button();
            this.btnStopVehicle = new System.Windows.Forms.Button();
            this.listReserveOkSections = new System.Windows.Forms.ListBox();
            this.listNeedReserveSections = new System.Windows.Forms.ListBox();
            this.btnBuzzOff = new System.Windows.Forms.Button();
            this.numPositionY = new System.Windows.Forms.NumericUpDown();
            this.btnAlarmReset = new System.Windows.Forms.Button();
            this.gbVehicleLocation = new System.Windows.Forms.GroupBox();
            this.numPositionX = new System.Windows.Forms.NumericUpDown();
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.radOnline = new System.Windows.Forms.RadioButton();
            this.radOffline = new System.Windows.Forms.RadioButton();
            this.btnSetPosition = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ucPerformanceCounterRam = new Mirle.Agv.UcLabelTextBox();
            this.ucPerformanceCounterCpu = new Mirle.Agv.UcLabelTextBox();
            this.ucRealPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucDeltaPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucBarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.ucEncoderPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucMapAddress = new Mirle.Agv.UcLabelTextBox();
            this.ucMapSection = new Mirle.Agv.UcLabelTextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.gbPerformanceCounter.SuspendLayout();
            this.gbTrackingPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).BeginInit();
            this.gbVehicleLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).BeginInit();
            this.gbConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
            this.JogPage.Size = new System.Drawing.Size(180, 22);
            this.JogPage.Text = "JogPitch";
            this.JogPage.Click += new System.EventHandler(this.JogPage_Click);
            // 
            // AlarmPage
            // 
            this.AlarmPage.Name = "AlarmPage";
            this.AlarmPage.Size = new System.Drawing.Size(180, 22);
            this.AlarmPage.Text = "Alarm";
            this.AlarmPage.Click += new System.EventHandler(this.AlarmPage_Click);
            // 
            // MiddlerPage
            // 
            this.MiddlerPage.Name = "MiddlerPage";
            this.MiddlerPage.Size = new System.Drawing.Size(180, 22);
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
            this.VehicleStatusPage.Size = new System.Drawing.Size(180, 22);
            this.VehicleStatusPage.Text = "車輛狀態";
            // 
            // PlcPage
            // 
            this.PlcPage.Name = "PlcPage";
            this.PlcPage.Size = new System.Drawing.Size(180, 22);
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
            // richTextBox1
            // 
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
            this.splitContainer2.Panel1.Controls.Add(this.listAskingReserveSections);
            this.splitContainer2.Panel1.Controls.Add(this.gbPerformanceCounter);
            this.splitContainer2.Panel1.Controls.Add(this.gbTrackingPosition);
            this.splitContainer2.Panel1.Controls.Add(this.btnStopVehicle);
            this.splitContainer2.Panel1.Controls.Add(this.listReserveOkSections);
            this.splitContainer2.Panel1.Controls.Add(this.listNeedReserveSections);
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
            this.splitContainer2.Panel2.Controls.Add(this.button1);
            this.splitContainer2.Size = new System.Drawing.Size(722, 1017);
            this.splitContainer2.SplitterDistance = 677;
            this.splitContainer2.TabIndex = 0;
            // 
            // listAskingReserveSections
            // 
            this.listAskingReserveSections.FormattingEnabled = true;
            this.listAskingReserveSections.ItemHeight = 12;
            this.listAskingReserveSections.Location = new System.Drawing.Point(329, 454);
            this.listAskingReserveSections.Name = "listAskingReserveSections";
            this.listAskingReserveSections.ScrollAlwaysVisible = true;
            this.listAskingReserveSections.Size = new System.Drawing.Size(144, 172);
            this.listAskingReserveSections.TabIndex = 43;
            // 
            // gbPerformanceCounter
            // 
            this.gbPerformanceCounter.Controls.Add(this.ucPerformanceCounterRam);
            this.gbPerformanceCounter.Controls.Add(this.ucPerformanceCounterCpu);
            this.gbPerformanceCounter.Location = new System.Drawing.Point(12, 296);
            this.gbPerformanceCounter.Name = "gbPerformanceCounter";
            this.gbPerformanceCounter.Size = new System.Drawing.Size(200, 100);
            this.gbPerformanceCounter.TabIndex = 10;
            this.gbPerformanceCounter.TabStop = false;
            this.gbPerformanceCounter.Text = "Performance Counter";
            // 
            // gbTrackingPosition
            // 
            this.gbTrackingPosition.Controls.Add(this.btnStartTrackingPosition);
            this.gbTrackingPosition.Controls.Add(this.btnResumeTrackingPostiion);
            this.gbTrackingPosition.Controls.Add(this.btnStopTrackingPosition);
            this.gbTrackingPosition.Controls.Add(this.btnPauseTrackingPosition);
            this.gbTrackingPosition.Location = new System.Drawing.Point(8, 454);
            this.gbTrackingPosition.Name = "gbTrackingPosition";
            this.gbTrackingPosition.Size = new System.Drawing.Size(165, 143);
            this.gbTrackingPosition.TabIndex = 42;
            this.gbTrackingPosition.TabStop = false;
            this.gbTrackingPosition.Text = "Tracking Position";
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
            this.btnStopVehicle.Location = new System.Drawing.Point(222, 72);
            this.btnStopVehicle.Name = "btnStopVehicle";
            this.btnStopVehicle.Size = new System.Drawing.Size(75, 23);
            this.btnStopVehicle.TabIndex = 4;
            this.btnStopVehicle.Text = "StopVehicle";
            this.btnStopVehicle.UseVisualStyleBackColor = true;
            // 
            // listReserveOkSections
            // 
            this.listReserveOkSections.FormattingEnabled = true;
            this.listReserveOkSections.ItemHeight = 12;
            this.listReserveOkSections.Location = new System.Drawing.Point(479, 453);
            this.listReserveOkSections.Name = "listReserveOkSections";
            this.listReserveOkSections.ScrollAlwaysVisible = true;
            this.listReserveOkSections.Size = new System.Drawing.Size(134, 172);
            this.listReserveOkSections.TabIndex = 42;
            // 
            // listNeedReserveSections
            // 
            this.listNeedReserveSections.FormattingEnabled = true;
            this.listNeedReserveSections.ItemHeight = 12;
            this.listNeedReserveSections.Location = new System.Drawing.Point(179, 454);
            this.listNeedReserveSections.Name = "listNeedReserveSections";
            this.listNeedReserveSections.ScrollAlwaysVisible = true;
            this.listNeedReserveSections.Size = new System.Drawing.Size(144, 172);
            this.listNeedReserveSections.TabIndex = 41;
            // 
            // btnBuzzOff
            // 
            this.btnBuzzOff.Location = new System.Drawing.Point(222, 43);
            this.btnBuzzOff.Name = "btnBuzzOff";
            this.btnBuzzOff.Size = new System.Drawing.Size(75, 23);
            this.btnBuzzOff.TabIndex = 3;
            this.btnBuzzOff.Text = "BuzzOff";
            this.btnBuzzOff.UseVisualStyleBackColor = true;
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
            this.btnAlarmReset.Location = new System.Drawing.Point(222, 14);
            this.btnAlarmReset.Name = "btnAlarmReset";
            this.btnAlarmReset.Size = new System.Drawing.Size(75, 23);
            this.btnAlarmReset.TabIndex = 2;
            this.btnAlarmReset.Text = "AlarmReset";
            this.btnAlarmReset.UseVisualStyleBackColor = true;
            // 
            // gbVehicleLocation
            // 
            this.gbVehicleLocation.Controls.Add(this.ucRealPosition);
            this.gbVehicleLocation.Controls.Add(this.ucDeltaPosition);
            this.gbVehicleLocation.Controls.Add(this.ucBarcodePosition);
            this.gbVehicleLocation.Controls.Add(this.ucEncoderPosition);
            this.gbVehicleLocation.Controls.Add(this.ucMapAddress);
            this.gbVehicleLocation.Controls.Add(this.ucMapSection);
            this.gbVehicleLocation.Location = new System.Drawing.Point(12, 68);
            this.gbVehicleLocation.Name = "gbVehicleLocation";
            this.gbVehicleLocation.Size = new System.Drawing.Size(200, 222);
            this.gbVehicleLocation.TabIndex = 1;
            this.gbVehicleLocation.TabStop = false;
            this.gbVehicleLocation.Text = "VehicleLocation";
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
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ucPerformanceCounterRam
            // 
            this.ucPerformanceCounterRam.Location = new System.Drawing.Point(7, 55);
            this.ucPerformanceCounterRam.Name = "ucPerformanceCounterRam";
            this.ucPerformanceCounterRam.Size = new System.Drawing.Size(187, 30);
            this.ucPerformanceCounterRam.TabIndex = 1;
            this.ucPerformanceCounterRam.UcName = "RAM";
            this.ucPerformanceCounterRam.UcValue = "";
            // 
            // ucPerformanceCounterCpu
            // 
            this.ucPerformanceCounterCpu.Location = new System.Drawing.Point(6, 19);
            this.ucPerformanceCounterCpu.Name = "ucPerformanceCounterCpu";
            this.ucPerformanceCounterCpu.Size = new System.Drawing.Size(187, 30);
            this.ucPerformanceCounterCpu.TabIndex = 0;
            this.ucPerformanceCounterCpu.UcName = "CPU";
            this.ucPerformanceCounterCpu.UcValue = "";
            // 
            // ucRealPosition
            // 
            this.ucRealPosition.Location = new System.Drawing.Point(0, 181);
            this.ucRealPosition.Name = "ucRealPosition";
            this.ucRealPosition.Size = new System.Drawing.Size(194, 26);
            this.ucRealPosition.TabIndex = 5;
            this.ucRealPosition.UcName = "label1";
            this.ucRealPosition.UcValue = "";
            // 
            // ucDeltaPosition
            // 
            this.ucDeltaPosition.Location = new System.Drawing.Point(0, 149);
            this.ucDeltaPosition.Name = "ucDeltaPosition";
            this.ucDeltaPosition.Size = new System.Drawing.Size(194, 26);
            this.ucDeltaPosition.TabIndex = 4;
            this.ucDeltaPosition.UcName = "label1";
            this.ucDeltaPosition.UcValue = "";
            // 
            // ucBarcodePosition
            // 
            this.ucBarcodePosition.Location = new System.Drawing.Point(0, 117);
            this.ucBarcodePosition.Name = "ucBarcodePosition";
            this.ucBarcodePosition.Size = new System.Drawing.Size(194, 26);
            this.ucBarcodePosition.TabIndex = 3;
            this.ucBarcodePosition.UcName = "label1";
            this.ucBarcodePosition.UcValue = "";
            // 
            // ucEncoderPosition
            // 
            this.ucEncoderPosition.Location = new System.Drawing.Point(0, 85);
            this.ucEncoderPosition.Name = "ucEncoderPosition";
            this.ucEncoderPosition.Size = new System.Drawing.Size(194, 26);
            this.ucEncoderPosition.TabIndex = 2;
            this.ucEncoderPosition.UcName = "label1";
            this.ucEncoderPosition.UcValue = "";
            // 
            // ucMapAddress
            // 
            this.ucMapAddress.Location = new System.Drawing.Point(0, 53);
            this.ucMapAddress.Name = "ucMapAddress";
            this.ucMapAddress.Size = new System.Drawing.Size(194, 26);
            this.ucMapAddress.TabIndex = 1;
            this.ucMapAddress.UcName = "label1";
            this.ucMapAddress.UcValue = "";
            // 
            // ucMapSection
            // 
            this.ucMapSection.Location = new System.Drawing.Point(0, 21);
            this.ucMapSection.Name = "ucMapSection";
            this.ucMapSection.Size = new System.Drawing.Size(194, 26);
            this.ucMapSection.TabIndex = 0;
            this.ucMapSection.UcName = "label1";
            this.ucMapSection.UcValue = "";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1920, 1024);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(57, 102);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
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
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.gbPerformanceCounter.ResumeLayout(false);
            this.gbTrackingPosition.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).EndInit();
            this.gbVehicleLocation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).EndInit();
            this.gbConnection.ResumeLayout(false);
            this.gbConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
        private System.Windows.Forms.PictureBox pictureBox1;
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
        private UcLabelTextBox ucDeltaPosition;
        private UcLabelTextBox ucBarcodePosition;
        private UcLabelTextBox ucEncoderPosition;
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
        private System.Windows.Forms.ListBox listReserveOkSections;
        private System.Windows.Forms.ListBox listNeedReserveSections;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NumericUpDown numPositionY;
        private System.Windows.Forms.NumericUpDown numPositionX;
        private System.Windows.Forms.ListBox listAskingReserveSections;
        private System.Windows.Forms.GroupBox gbTrackingPosition;
        private System.Windows.Forms.GroupBox gbPerformanceCounter;
        private UcLabelTextBox ucPerformanceCounterRam;
        private UcLabelTextBox ucPerformanceCounterCpu;
        private System.Windows.Forms.ToolStripMenuItem PlcPage;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
    }
}