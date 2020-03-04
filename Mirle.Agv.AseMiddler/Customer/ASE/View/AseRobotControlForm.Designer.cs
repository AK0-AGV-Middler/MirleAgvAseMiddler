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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtCassetteId = new System.Windows.Forms.TextBox();
            this.boxPioDirection = new System.Windows.Forms.ComboBox();
            this.txtToPort = new System.Windows.Forms.TextBox();
            this.txtFromPort = new System.Windows.Forms.TextBox();
            this.btnSendRobot = new System.Windows.Forms.Button();
            this.numForkSpeed = new System.Windows.Forms.NumericUpDown();
            this.cbIsLoad = new System.Windows.Forms.CheckBox();
            this.cbIsPio = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.pageRobotCommnad.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numForkSpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pageRobotCommnad);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(776, 426);
            this.tabControl1.TabIndex = 0;
            // 
            // pageRobotCommnad
            // 
            this.pageRobotCommnad.Controls.Add(this.groupBox4);
            this.pageRobotCommnad.Location = new System.Drawing.Point(4, 22);
            this.pageRobotCommnad.Name = "pageRobotCommnad";
            this.pageRobotCommnad.Padding = new System.Windows.Forms.Padding(3);
            this.pageRobotCommnad.Size = new System.Drawing.Size(768, 400);
            this.pageRobotCommnad.TabIndex = 0;
            this.pageRobotCommnad.Text = "RobotCommnad";
            this.pageRobotCommnad.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtCassetteId);
            this.groupBox4.Controls.Add(this.boxPioDirection);
            this.groupBox4.Controls.Add(this.txtToPort);
            this.groupBox4.Controls.Add(this.txtFromPort);
            this.groupBox4.Controls.Add(this.btnSendRobot);
            this.groupBox4.Controls.Add(this.numForkSpeed);
            this.groupBox4.Controls.Add(this.cbIsLoad);
            this.groupBox4.Controls.Add(this.cbIsPio);
            this.groupBox4.Location = new System.Drawing.Point(17, 17);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(253, 197);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Robot(P45)";
            // 
            // txtCassetteId
            // 
            this.txtCassetteId.Location = new System.Drawing.Point(6, 121);
            this.txtCassetteId.Name = "txtCassetteId";
            this.txtCassetteId.Size = new System.Drawing.Size(240, 22);
            this.txtCassetteId.TabIndex = 11;
            // 
            // boxPioDirection
            // 
            this.boxPioDirection.FormattingEnabled = true;
            this.boxPioDirection.Location = new System.Drawing.Point(6, 39);
            this.boxPioDirection.Name = "boxPioDirection";
            this.boxPioDirection.Size = new System.Drawing.Size(117, 20);
            this.boxPioDirection.TabIndex = 10;
            // 
            // txtToPort
            // 
            this.txtToPort.Location = new System.Drawing.Point(129, 93);
            this.txtToPort.Name = "txtToPort";
            this.txtToPort.Size = new System.Drawing.Size(117, 22);
            this.txtToPort.TabIndex = 9;
            this.txtToPort.Text = "To";
            // 
            // txtFromPort
            // 
            this.txtFromPort.Location = new System.Drawing.Point(6, 93);
            this.txtFromPort.Name = "txtFromPort";
            this.txtFromPort.Size = new System.Drawing.Size(117, 22);
            this.txtFromPort.TabIndex = 8;
            this.txtFromPort.Text = "From";
            // 
            // btnSendRobot
            // 
            this.btnSendRobot.Location = new System.Drawing.Point(6, 149);
            this.btnSendRobot.Name = "btnSendRobot";
            this.btnSendRobot.Size = new System.Drawing.Size(241, 33);
            this.btnSendRobot.TabIndex = 5;
            this.btnSendRobot.Text = "Send";
            this.btnSendRobot.UseVisualStyleBackColor = true;
            this.btnSendRobot.Click += new System.EventHandler(this.btnSendRobot_Click);
            // 
            // numForkSpeed
            // 
            this.numForkSpeed.Location = new System.Drawing.Point(6, 65);
            this.numForkSpeed.Name = "numForkSpeed";
            this.numForkSpeed.Size = new System.Drawing.Size(117, 22);
            this.numForkSpeed.TabIndex = 3;
            this.numForkSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numForkSpeed.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
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
            // cbIsPio
            // 
            this.cbIsPio.AutoSize = true;
            this.cbIsPio.Location = new System.Drawing.Point(6, 21);
            this.cbIsPio.Name = "cbIsPio";
            this.cbIsPio.Size = new System.Drawing.Size(61, 16);
            this.cbIsPio.TabIndex = 0;
            this.cbIsPio.Text = "Is PIO ?";
            this.cbIsPio.UseVisualStyleBackColor = true;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // AseRobotControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "AseRobotControlForm";
            this.Text = "AseRobotControlForm";
            this.tabControl1.ResumeLayout(false);
            this.pageRobotCommnad.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numForkSpeed)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pageRobotCommnad;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtToPort;
        private System.Windows.Forms.TextBox txtFromPort;
        private System.Windows.Forms.Button btnSendRobot;
        private System.Windows.Forms.NumericUpDown numForkSpeed;
        private System.Windows.Forms.CheckBox cbIsPio;
        private System.Windows.Forms.ComboBox boxPioDirection;
        private System.Windows.Forms.TextBox txtCassetteId;
        private System.Windows.Forms.CheckBox cbIsLoad;
        private System.Windows.Forms.Timer timer1;
    }
}