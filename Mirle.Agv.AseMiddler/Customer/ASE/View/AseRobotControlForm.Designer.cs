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
            this.txtRCstId = new System.Windows.Forms.TextBox();
            this.txtLCstId = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.boxPioDirection = new System.Windows.Forms.ComboBox();
            this.txtToPort = new System.Windows.Forms.TextBox();
            this.txtPortNumber = new System.Windows.Forms.TextBox();
            this.txtGateType = new System.Windows.Forms.TextBox();
            this.txtFromPort = new System.Windows.Forms.TextBox();
            this.cbIsLoad = new System.Windows.Forms.CheckBox();
            this.btnSendRobot = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.pageRobotCommnad.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pageRobotCommnad);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1182, 211);
            this.tabControl1.TabIndex = 0;
            // 
            // pageRobotCommnad
            // 
            this.pageRobotCommnad.Controls.Add(this.txtRCstId);
            this.pageRobotCommnad.Controls.Add(this.txtLCstId);
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
            // txtRCstId
            // 
            this.txtRCstId.Location = new System.Drawing.Point(276, 92);
            this.txtRCstId.Name = "txtRCstId";
            this.txtRCstId.Size = new System.Drawing.Size(240, 22);
            this.txtRCstId.TabIndex = 11;
            // 
            // txtLCstId
            // 
            this.txtLCstId.Location = new System.Drawing.Point(276, 64);
            this.txtLCstId.Name = "txtLCstId";
            this.txtLCstId.Size = new System.Drawing.Size(240, 22);
            this.txtLCstId.TabIndex = 11;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.boxPioDirection);
            this.groupBox4.Controls.Add(this.txtToPort);
            this.groupBox4.Controls.Add(this.txtPortNumber);
            this.groupBox4.Controls.Add(this.txtGateType);
            this.groupBox4.Controls.Add(this.txtFromPort);
            this.groupBox4.Controls.Add(this.cbIsLoad);
            this.groupBox4.Location = new System.Drawing.Point(17, 17);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(253, 113);
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
            this.txtToPort.Location = new System.Drawing.Point(130, 47);
            this.txtToPort.Name = "txtToPort";
            this.txtToPort.Size = new System.Drawing.Size(117, 22);
            this.txtToPort.TabIndex = 9;
            this.txtToPort.Text = "To";
            // 
            // txtPortNumber
            // 
            this.txtPortNumber.Location = new System.Drawing.Point(129, 75);
            this.txtPortNumber.Name = "txtPortNumber";
            this.txtPortNumber.Size = new System.Drawing.Size(117, 22);
            this.txtPortNumber.TabIndex = 8;
            this.txtPortNumber.Text = "1";
            // 
            // txtGateType
            // 
            this.txtGateType.Location = new System.Drawing.Point(6, 75);
            this.txtGateType.Name = "txtGateType";
            this.txtGateType.Size = new System.Drawing.Size(117, 22);
            this.txtGateType.TabIndex = 8;
            this.txtGateType.Text = "0";
            // 
            // txtFromPort
            // 
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
            this.pageRobotCommnad.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
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
        private System.Windows.Forms.TextBox txtLCstId;
        private System.Windows.Forms.CheckBox cbIsLoad;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txtPortNumber;
        private System.Windows.Forms.TextBox txtGateType;
        private System.Windows.Forms.TextBox txtRCstId;
        private System.Windows.Forms.TextBox textBox1;
    }
}