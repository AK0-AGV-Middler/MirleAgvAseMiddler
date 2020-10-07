namespace Mirle.Agv.AseMiddler.View
{
    partial class ConfigForm
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
            this.btnHide = new System.Windows.Forms.Button();
            this.boxVehicleConfigs = new System.Windows.Forms.ComboBox();
            this.btnLoadConfig = new System.Windows.Forms.Button();
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtJsonStringConfig = new System.Windows.Forms.TextBox();
            this.btnCleanTextBox = new System.Windows.Forms.Button();
            this.btnSaveConfigsToFile = new System.Windows.Forms.Button();
            this.btnLoadConfigsFromFile = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnHide
            // 
            this.btnHide.ForeColor = System.Drawing.Color.OrangeRed;
            this.btnHide.Location = new System.Drawing.Point(628, 12);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(75, 39);
            this.btnHide.TabIndex = 1;
            this.btnHide.Text = "X";
            this.btnHide.UseVisualStyleBackColor = true;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // boxVehicleConfigs
            // 
            this.boxVehicleConfigs.FormattingEnabled = true;
            this.boxVehicleConfigs.Location = new System.Drawing.Point(5, 96);
            this.boxVehicleConfigs.Name = "boxVehicleConfigs";
            this.boxVehicleConfigs.Size = new System.Drawing.Size(247, 27);
            this.boxVehicleConfigs.TabIndex = 2;
            this.boxVehicleConfigs.Text = "MainFlowConfig";
            // 
            // btnLoadConfig
            // 
            this.btnLoadConfig.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnLoadConfig.Location = new System.Drawing.Point(6, 26);
            this.btnLoadConfig.Name = "btnLoadConfig";
            this.btnLoadConfig.Size = new System.Drawing.Size(120, 50);
            this.btnLoadConfig.TabIndex = 3;
            this.btnLoadConfig.Text = "Load";
            this.btnLoadConfig.UseVisualStyleBackColor = true;
            this.btnLoadConfig.Click += new System.EventHandler(this.btnLoadConfig_Click);
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Location = new System.Drawing.Point(132, 26);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(120, 50);
            this.btnSaveConfig.TabIndex = 3;
            this.btnSaveConfig.Text = "Save";
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtJsonStringConfig);
            this.groupBox1.Controls.Add(this.boxVehicleConfigs);
            this.groupBox1.Controls.Add(this.btnCleanTextBox);
            this.groupBox1.Controls.Add(this.btnLoadConfigsFromFile);
            this.groupBox1.Controls.Add(this.btnSaveConfigsToFile);
            this.groupBox1.Controls.Add(this.btnSaveConfig);
            this.groupBox1.Controls.Add(this.btnLoadConfig);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(610, 547);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Configs";
            // 
            // txtJsonStringConfig
            // 
            this.txtJsonStringConfig.Location = new System.Drawing.Point(6, 141);
            this.txtJsonStringConfig.Multiline = true;
            this.txtJsonStringConfig.Name = "txtJsonStringConfig";
            this.txtJsonStringConfig.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtJsonStringConfig.Size = new System.Drawing.Size(598, 400);
            this.txtJsonStringConfig.TabIndex = 4;
            // 
            // btnCleanTextBox
            // 
            this.btnCleanTextBox.Location = new System.Drawing.Point(529, 96);
            this.btnCleanTextBox.Name = "btnCleanTextBox";
            this.btnCleanTextBox.Size = new System.Drawing.Size(75, 39);
            this.btnCleanTextBox.TabIndex = 3;
            this.btnCleanTextBox.Text = "Clean";
            this.btnCleanTextBox.UseVisualStyleBackColor = true;
            this.btnCleanTextBox.Click += new System.EventHandler(this.btnCleanTextBox_Click);
            // 
            // btnSaveConfigsToFile
            // 
            this.btnSaveConfigsToFile.Location = new System.Drawing.Point(404, 26);
            this.btnSaveConfigsToFile.Name = "btnSaveConfigsToFile";
            this.btnSaveConfigsToFile.Size = new System.Drawing.Size(140, 50);
            this.btnSaveConfigsToFile.TabIndex = 3;
            this.btnSaveConfigsToFile.Text = "Save To File";
            this.btnSaveConfigsToFile.UseVisualStyleBackColor = true;
            this.btnSaveConfigsToFile.Click += new System.EventHandler(this.btnSaveConfigsToFile_Click);
            // 
            // btnLoadConfigsFromFile
            // 
            this.btnLoadConfigsFromFile.Location = new System.Drawing.Point(258, 26);
            this.btnLoadConfigsFromFile.Name = "btnLoadConfigsFromFile";
            this.btnLoadConfigsFromFile.Size = new System.Drawing.Size(140, 50);
            this.btnLoadConfigsFromFile.TabIndex = 3;
            this.btnLoadConfigsFromFile.Text = "Load From File";
            this.btnLoadConfigsFromFile.UseVisualStyleBackColor = true;
            this.btnLoadConfigsFromFile.Click += new System.EventHandler(this.btnLoadConfigsFromFile_Click);
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 571);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnHide);
            this.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ConfigForm";
            this.Text = "參數設定";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.ComboBox boxVehicleConfigs;
        private System.Windows.Forms.Button btnLoadConfig;
        private System.Windows.Forms.Button btnSaveConfig;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtJsonStringConfig;
        private System.Windows.Forms.Button btnCleanTextBox;
        private System.Windows.Forms.Button btnSaveConfigsToFile;
        private System.Windows.Forms.Button btnLoadConfigsFromFile;
    }
}