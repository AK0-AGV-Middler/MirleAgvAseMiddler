namespace Mirle.Agv.AseMiddler.View
{
    partial class AseMoveControlForm
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
            this.pageMoveAppend = new System.Windows.Forms.TabPage();
            this.ucMoveMoveState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveLastAddress = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveIsMoveEnd = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveLastSection = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePositionY = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePositionX = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.btnResumeAskPosition = new System.Windows.Forms.Button();
            this.btnRefreshPosition = new System.Windows.Forms.Button();
            this.btnPauseAskPosition = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.boxKeepOrGo = new System.Windows.Forms.ComboBox();
            this.btnSearchMapAddress = new System.Windows.Forms.Button();
            this.txtMapAddress = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.boxIsEnd = new System.Windows.Forms.ComboBox();
            this.boxAddressDirection = new System.Windows.Forms.ComboBox();
            this.btnSendMove = new System.Windows.Forms.Button();
            this.numMoveSpeed = new System.Windows.Forms.NumericUpDown();
            this.numHeadAngle = new System.Windows.Forms.NumericUpDown();
            this.numMovePositionY = new System.Windows.Forms.NumericUpDown();
            this.numMovePositionX = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.pageMoveAppend.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMoveSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeadAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionX)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pageMoveAppend);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1142, 251);
            this.tabControl1.TabIndex = 0;
            // 
            // pageMoveAppend
            // 
            this.pageMoveAppend.Controls.Add(this.ucMoveMoveState);
            this.pageMoveAppend.Controls.Add(this.ucMoveLastAddress);
            this.pageMoveAppend.Controls.Add(this.ucMoveIsMoveEnd);
            this.pageMoveAppend.Controls.Add(this.ucMoveLastSection);
            this.pageMoveAppend.Controls.Add(this.ucMovePositionY);
            this.pageMoveAppend.Controls.Add(this.ucMovePositionX);
            this.pageMoveAppend.Controls.Add(this.btnResumeAskPosition);
            this.pageMoveAppend.Controls.Add(this.btnRefreshPosition);
            this.pageMoveAppend.Controls.Add(this.btnPauseAskPosition);
            this.pageMoveAppend.Controls.Add(this.groupBox1);
            this.pageMoveAppend.Controls.Add(this.groupBox3);
            this.pageMoveAppend.Location = new System.Drawing.Point(4, 22);
            this.pageMoveAppend.Name = "pageMoveAppend";
            this.pageMoveAppend.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.pageMoveAppend.Size = new System.Drawing.Size(1134, 225);
            this.pageMoveAppend.TabIndex = 0;
            this.pageMoveAppend.Text = "MoveAppend";
            this.pageMoveAppend.UseVisualStyleBackColor = true;
            // 
            // ucMoveMoveState
            // 
            this.ucMoveMoveState.Location = new System.Drawing.Point(966, 6);
            this.ucMoveMoveState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveMoveState.Name = "ucMoveMoveState";
            this.ucMoveMoveState.Size = new System.Drawing.Size(135, 65);
            this.ucMoveMoveState.TabIndex = 5;
            this.ucMoveMoveState.TagColor = System.Drawing.Color.Black;
            this.ucMoveMoveState.TagName = "MoveState";
            this.ucMoveMoveState.TagValue = "Idle";
            // 
            // ucMoveLastAddress
            // 
            this.ucMoveLastAddress.Location = new System.Drawing.Point(825, 78);
            this.ucMoveLastAddress.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveLastAddress.Name = "ucMoveLastAddress";
            this.ucMoveLastAddress.Size = new System.Drawing.Size(135, 65);
            this.ucMoveLastAddress.TabIndex = 6;
            this.ucMoveLastAddress.TagColor = System.Drawing.Color.Black;
            this.ucMoveLastAddress.TagName = "Last Address";
            this.ucMoveLastAddress.TagValue = "10001";
            // 
            // ucMoveIsMoveEnd
            // 
            this.ucMoveIsMoveEnd.Location = new System.Drawing.Point(966, 78);
            this.ucMoveIsMoveEnd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveIsMoveEnd.Name = "ucMoveIsMoveEnd";
            this.ucMoveIsMoveEnd.Size = new System.Drawing.Size(135, 65);
            this.ucMoveIsMoveEnd.TabIndex = 7;
            this.ucMoveIsMoveEnd.TagColor = System.Drawing.Color.Black;
            this.ucMoveIsMoveEnd.TagName = "Is Move End";
            this.ucMoveIsMoveEnd.TagValue = "True";
            // 
            // ucMoveLastSection
            // 
            this.ucMoveLastSection.Location = new System.Drawing.Point(825, 6);
            this.ucMoveLastSection.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveLastSection.Name = "ucMoveLastSection";
            this.ucMoveLastSection.Size = new System.Drawing.Size(135, 65);
            this.ucMoveLastSection.TabIndex = 8;
            this.ucMoveLastSection.TagColor = System.Drawing.Color.Black;
            this.ucMoveLastSection.TagName = "Last Section";
            this.ucMoveLastSection.TagValue = "00101";
            // 
            // ucMovePositionY
            // 
            this.ucMovePositionY.Location = new System.Drawing.Point(966, 149);
            this.ucMovePositionY.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePositionY.Name = "ucMovePositionY";
            this.ucMovePositionY.Size = new System.Drawing.Size(135, 65);
            this.ucMovePositionY.TabIndex = 9;
            this.ucMovePositionY.TagColor = System.Drawing.Color.Black;
            this.ucMovePositionY.TagName = "Y";
            this.ucMovePositionY.TagValue = "-13579";
            // 
            // ucMovePositionX
            // 
            this.ucMovePositionX.Location = new System.Drawing.Point(825, 149);
            this.ucMovePositionX.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePositionX.Name = "ucMovePositionX";
            this.ucMovePositionX.Size = new System.Drawing.Size(135, 65);
            this.ucMovePositionX.TabIndex = 10;
            this.ucMovePositionX.TagColor = System.Drawing.Color.Black;
            this.ucMovePositionX.TagName = "X";
            this.ucMovePositionX.TagValue = "123456";
            // 
            // btnResumeAskPosition
            // 
            this.btnResumeAskPosition.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnResumeAskPosition.Location = new System.Drawing.Point(534, 55);
            this.btnResumeAskPosition.Name = "btnResumeAskPosition";
            this.btnResumeAskPosition.Size = new System.Drawing.Size(272, 33);
            this.btnResumeAskPosition.TabIndex = 4;
            this.btnResumeAskPosition.Text = "Resume Ask Position";
            this.btnResumeAskPosition.UseVisualStyleBackColor = true;
            this.btnResumeAskPosition.Click += new System.EventHandler(this.btnResumeAskPosition_Click);
            // 
            // btnRefreshPosition
            // 
            this.btnRefreshPosition.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshPosition.Location = new System.Drawing.Point(534, 159);
            this.btnRefreshPosition.Name = "btnRefreshPosition";
            this.btnRefreshPosition.Size = new System.Drawing.Size(272, 33);
            this.btnRefreshPosition.TabIndex = 4;
            this.btnRefreshPosition.Text = "Refresh  Position";
            this.btnRefreshPosition.UseVisualStyleBackColor = true;
            this.btnRefreshPosition.Click += new System.EventHandler(this.btnRefreshPosition_Click);
            // 
            // btnPauseAskPosition
            // 
            this.btnPauseAskPosition.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPauseAskPosition.Location = new System.Drawing.Point(534, 16);
            this.btnPauseAskPosition.Name = "btnPauseAskPosition";
            this.btnPauseAskPosition.Size = new System.Drawing.Size(272, 33);
            this.btnPauseAskPosition.TabIndex = 4;
            this.btnPauseAskPosition.Text = "Pause Ask Position";
            this.btnPauseAskPosition.UseVisualStyleBackColor = true;
            this.btnPauseAskPosition.Click += new System.EventHandler(this.btnPauseAskPosition_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.boxKeepOrGo);
            this.groupBox1.Controls.Add(this.btnSearchMapAddress);
            this.groupBox1.Controls.Add(this.txtMapAddress);
            this.groupBox1.Location = new System.Drawing.Point(269, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 197);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MapAddress";
            // 
            // boxKeepOrGo
            // 
            this.boxKeepOrGo.FormattingEnabled = true;
            this.boxKeepOrGo.Location = new System.Drawing.Point(6, 42);
            this.boxKeepOrGo.Name = "boxKeepOrGo";
            this.boxKeepOrGo.Size = new System.Drawing.Size(247, 20);
            this.boxKeepOrGo.TabIndex = 7;
            // 
            // btnSearchMapAddress
            // 
            this.btnSearchMapAddress.Location = new System.Drawing.Point(6, 153);
            this.btnSearchMapAddress.Name = "btnSearchMapAddress";
            this.btnSearchMapAddress.Size = new System.Drawing.Size(247, 33);
            this.btnSearchMapAddress.TabIndex = 3;
            this.btnSearchMapAddress.Text = "Search MapAddress";
            this.btnSearchMapAddress.UseVisualStyleBackColor = true;
            this.btnSearchMapAddress.Click += new System.EventHandler(this.btnSearchMapAddress_Click);
            // 
            // txtMapAddress
            // 
            this.txtMapAddress.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtMapAddress.Location = new System.Drawing.Point(6, 15);
            this.txtMapAddress.Name = "txtMapAddress";
            this.txtMapAddress.Size = new System.Drawing.Size(247, 22);
            this.txtMapAddress.TabIndex = 2;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.boxIsEnd);
            this.groupBox3.Controls.Add(this.boxAddressDirection);
            this.groupBox3.Controls.Add(this.btnSendMove);
            this.groupBox3.Controls.Add(this.numMoveSpeed);
            this.groupBox3.Controls.Add(this.numHeadAngle);
            this.groupBox3.Controls.Add(this.numMovePositionY);
            this.groupBox3.Controls.Add(this.numMovePositionX);
            this.groupBox3.Location = new System.Drawing.Point(6, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(257, 198);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Move(P41)";
            // 
            // boxIsEnd
            // 
            this.boxIsEnd.FormattingEnabled = true;
            this.boxIsEnd.Location = new System.Drawing.Point(6, 17);
            this.boxIsEnd.Name = "boxIsEnd";
            this.boxIsEnd.Size = new System.Drawing.Size(245, 20);
            this.boxIsEnd.TabIndex = 6;
            // 
            // boxAddressDirection
            // 
            this.boxAddressDirection.FormattingEnabled = true;
            this.boxAddressDirection.Location = new System.Drawing.Point(6, 127);
            this.boxAddressDirection.Name = "boxAddressDirection";
            this.boxAddressDirection.Size = new System.Drawing.Size(245, 20);
            this.boxAddressDirection.TabIndex = 6;
            // 
            // btnSendMove
            // 
            this.btnSendMove.Location = new System.Drawing.Point(6, 153);
            this.btnSendMove.Name = "btnSendMove";
            this.btnSendMove.Size = new System.Drawing.Size(245, 33);
            this.btnSendMove.TabIndex = 5;
            this.btnSendMove.Text = "Send";
            this.btnSendMove.UseVisualStyleBackColor = true;
            this.btnSendMove.Click += new System.EventHandler(this.btnSendMove_Click);
            // 
            // numMoveSpeed
            // 
            this.numMoveSpeed.Location = new System.Drawing.Point(129, 99);
            this.numMoveSpeed.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numMoveSpeed.Name = "numMoveSpeed";
            this.numMoveSpeed.Size = new System.Drawing.Size(122, 22);
            this.numMoveSpeed.TabIndex = 4;
            this.numMoveSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMoveSpeed.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // numHeadAngle
            // 
            this.numHeadAngle.Location = new System.Drawing.Point(6, 99);
            this.numHeadAngle.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numHeadAngle.Name = "numHeadAngle";
            this.numHeadAngle.Size = new System.Drawing.Size(117, 22);
            this.numHeadAngle.TabIndex = 3;
            this.numHeadAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numHeadAngle.Value = new decimal(new int[] {
            360,
            0,
            0,
            0});
            // 
            // numMovePositionY
            // 
            this.numMovePositionY.Location = new System.Drawing.Point(6, 71);
            this.numMovePositionY.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.numMovePositionY.Minimum = new decimal(new int[] {
            99999999,
            0,
            0,
            -2147483648});
            this.numMovePositionY.Name = "numMovePositionY";
            this.numMovePositionY.Size = new System.Drawing.Size(245, 22);
            this.numMovePositionY.TabIndex = 2;
            this.numMovePositionY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMovePositionY.Value = new decimal(new int[] {
            87654321,
            0,
            0,
            -2147483648});
            // 
            // numMovePositionX
            // 
            this.numMovePositionX.Location = new System.Drawing.Point(6, 43);
            this.numMovePositionX.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.numMovePositionX.Minimum = new decimal(new int[] {
            99999999,
            0,
            0,
            -2147483648});
            this.numMovePositionX.Name = "numMovePositionX";
            this.numMovePositionX.Size = new System.Drawing.Size(245, 22);
            this.numMovePositionX.TabIndex = 1;
            this.numMovePositionX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMovePositionX.Value = new decimal(new int[] {
            12345678,
            0,
            0,
            0});
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox1.Location = new System.Drawing.Point(12, 269);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(1142, 439);
            this.textBox1.TabIndex = 3;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // AseMoveControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1166, 705);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tabControl1);
            this.Name = "AseMoveControlForm";
            this.Text = "AseMoveControlForm";
            this.tabControl1.ResumeLayout(false);
            this.pageMoveAppend.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numMoveSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeadAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionX)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pageMoveAppend;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnSendMove;
        private System.Windows.Forms.NumericUpDown numMoveSpeed;
        private System.Windows.Forms.NumericUpDown numHeadAngle;
        private System.Windows.Forms.NumericUpDown numMovePositionY;
        private System.Windows.Forms.NumericUpDown numMovePositionX;
        private System.Windows.Forms.ComboBox boxAddressDirection;
        private System.Windows.Forms.ComboBox boxIsEnd;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSearchMapAddress;
        private System.Windows.Forms.TextBox txtMapAddress;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnPauseAskPosition;
        private System.Windows.Forms.Button btnResumeAskPosition;
        private System.Windows.Forms.ComboBox boxKeepOrGo;
        private System.Windows.Forms.Button btnRefreshPosition;
        private UcVerticalLabelText ucMoveMoveState;
        private UcVerticalLabelText ucMoveLastAddress;
        private UcVerticalLabelText ucMoveIsMoveEnd;
        private UcVerticalLabelText ucMoveLastSection;
        private UcVerticalLabelText ucMovePositionY;
        private UcVerticalLabelText ucMovePositionX;
    }
}