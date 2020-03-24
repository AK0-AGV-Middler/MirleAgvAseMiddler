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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pageMoveAppend = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.boxIsEnd = new System.Windows.Forms.ComboBox();
            this.boxAddressDirection = new System.Windows.Forms.ComboBox();
            this.btnSendMove = new System.Windows.Forms.Button();
            this.numMoveSpeed = new System.Windows.Forms.NumericUpDown();
            this.numHeadAngle = new System.Windows.Forms.NumericUpDown();
            this.numMovePositionY = new System.Windows.Forms.NumericUpDown();
            this.numMovePositionX = new System.Windows.Forms.NumericUpDown();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.txtMapAddress = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSearchMapAddress = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.pageMoveAppend.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMoveSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeadAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionX)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pageMoveAppend);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(776, 426);
            this.tabControl1.TabIndex = 0;
            // 
            // pageMoveAppend
            // 
            this.pageMoveAppend.Controls.Add(this.groupBox1);
            this.pageMoveAppend.Controls.Add(this.groupBox3);
            this.pageMoveAppend.Location = new System.Drawing.Point(4, 22);
            this.pageMoveAppend.Name = "pageMoveAppend";
            this.pageMoveAppend.Padding = new System.Windows.Forms.Padding(3);
            this.pageMoveAppend.Size = new System.Drawing.Size(768, 400);
            this.pageMoveAppend.TabIndex = 0;
            this.pageMoveAppend.Text = "MoveAppend";
            this.pageMoveAppend.UseVisualStyleBackColor = true;
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
            // txtMapAddress
            // 
            this.txtMapAddress.Location = new System.Drawing.Point(6, 15);
            this.txtMapAddress.Name = "txtMapAddress";
            this.txtMapAddress.Size = new System.Drawing.Size(247, 22);
            this.txtMapAddress.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSearchMapAddress);
            this.groupBox1.Controls.Add(this.txtMapAddress);
            this.groupBox1.Location = new System.Drawing.Point(269, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 197);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MapAddress";
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
            // AseMoveControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "AseMoveControlForm";
            this.Text = "AseMoveControlForm";
            this.tabControl1.ResumeLayout(false);
            this.pageMoveAppend.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numMoveSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeadAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionX)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ComboBox boxIsEnd;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSearchMapAddress;
        private System.Windows.Forms.TextBox txtMapAddress;
    }
}