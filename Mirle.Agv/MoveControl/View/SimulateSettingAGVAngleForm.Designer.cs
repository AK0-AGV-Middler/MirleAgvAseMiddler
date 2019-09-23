namespace Mirle.Agv.View
{
    partial class SimulateSettingAGVAngleForm
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
            this.button_SettingAGVAngle = new System.Windows.Forms.Button();
            this.cB_AGVAngle = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button_SettingAGVAngle
            // 
            this.button_SettingAGVAngle.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_SettingAGVAngle.Location = new System.Drawing.Point(155, 32);
            this.button_SettingAGVAngle.Name = "button_SettingAGVAngle";
            this.button_SettingAGVAngle.Size = new System.Drawing.Size(88, 30);
            this.button_SettingAGVAngle.TabIndex = 1;
            this.button_SettingAGVAngle.Text = "設定";
            this.button_SettingAGVAngle.UseVisualStyleBackColor = true;
            this.button_SettingAGVAngle.Click += new System.EventHandler(this.button_SettingAGVAngle_Click);
            // 
            // cB_AGVAngle
            // 
            this.cB_AGVAngle.FormattingEnabled = true;
            this.cB_AGVAngle.Items.AddRange(new object[] {
            "0",
            "90",
            "-90",
            "180"});
            this.cB_AGVAngle.Location = new System.Drawing.Point(49, 37);
            this.cB_AGVAngle.Name = "cB_AGVAngle";
            this.cB_AGVAngle.Size = new System.Drawing.Size(91, 20);
            this.cB_AGVAngle.TabIndex = 132;
            // 
            // SimulateSettingAGVAngle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 90);
            this.ControlBox = false;
            this.Controls.Add(this.cB_AGVAngle);
            this.Controls.Add(this.button_SettingAGVAngle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SimulateSettingAGVAngle";
            this.Text = "SimulateSettingAGVAngle";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button_SettingAGVAngle;
        private System.Windows.Forms.ComboBox cB_AGVAngle;
    }
}