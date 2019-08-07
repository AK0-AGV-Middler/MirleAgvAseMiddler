namespace Mirle.Agv
{
    partial class JogPitchAxis
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label_Disable = new System.Windows.Forms.Label();
            this.label_Position = new System.Windows.Forms.Label();
            this.pB_Disable = new System.Windows.Forms.PictureBox();
            this.tB_Position = new System.Windows.Forms.TextBox();
            this.pB_StandStill = new System.Windows.Forms.PictureBox();
            this.label_StandStill = new System.Windows.Forms.Label();
            this.label_AxisName = new System.Windows.Forms.Label();
            this.pB_Error = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pB_Disable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pB_StandStill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pB_Error)).BeginInit();
            this.SuspendLayout();
            // 
            // label_Disable
            // 
            this.label_Disable.AutoSize = true;
            this.label_Disable.Location = new System.Drawing.Point(29, 23);
            this.label_Disable.Name = "label_Disable";
            this.label_Disable.Size = new System.Drawing.Size(39, 12);
            this.label_Disable.TabIndex = 144;
            this.label_Disable.Text = "Disable";
            // 
            // label_Position
            // 
            this.label_Position.AutoSize = true;
            this.label_Position.Location = new System.Drawing.Point(6, 57);
            this.label_Position.Name = "label_Position";
            this.label_Position.Size = new System.Drawing.Size(42, 12);
            this.label_Position.TabIndex = 86;
            this.label_Position.Text = "Position";
            // 
            // pB_Disable
            // 
            this.pB_Disable.BackColor = System.Drawing.Color.LightGray;
            this.pB_Disable.Location = new System.Drawing.Point(9, 25);
            this.pB_Disable.Name = "pB_Disable";
            this.pB_Disable.Size = new System.Drawing.Size(14, 10);
            this.pB_Disable.TabIndex = 63;
            this.pB_Disable.TabStop = false;
            // 
            // tB_Position
            // 
            this.tB_Position.Location = new System.Drawing.Point(9, 71);
            this.tB_Position.Name = "tB_Position";
            this.tB_Position.ReadOnly = true;
            this.tB_Position.Size = new System.Drawing.Size(74, 22);
            this.tB_Position.TabIndex = 83;
            this.tB_Position.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pB_StandStill
            // 
            this.pB_StandStill.BackColor = System.Drawing.Color.LightGray;
            this.pB_StandStill.Location = new System.Drawing.Point(9, 40);
            this.pB_StandStill.Name = "pB_StandStill";
            this.pB_StandStill.Size = new System.Drawing.Size(14, 10);
            this.pB_StandStill.TabIndex = 64;
            this.pB_StandStill.TabStop = false;
            // 
            // label_StandStill
            // 
            this.label_StandStill.AutoSize = true;
            this.label_StandStill.Location = new System.Drawing.Point(29, 40);
            this.label_StandStill.Name = "label_StandStill";
            this.label_StandStill.Size = new System.Drawing.Size(52, 12);
            this.label_StandStill.TabIndex = 68;
            this.label_StandStill.Text = "Stand Still";
            // 
            // label_AxisName
            // 
            this.label_AxisName.AutoSize = true;
            this.label_AxisName.Location = new System.Drawing.Point(7, 6);
            this.label_AxisName.Name = "label_AxisName";
            this.label_AxisName.Size = new System.Drawing.Size(0, 12);
            this.label_AxisName.TabIndex = 146;
            // 
            // pB_Error
            // 
            this.pB_Error.BackColor = System.Drawing.Color.LightGray;
            this.pB_Error.Location = new System.Drawing.Point(67, 8);
            this.pB_Error.Name = "pB_Error";
            this.pB_Error.Size = new System.Drawing.Size(14, 10);
            this.pB_Error.TabIndex = 147;
            this.pB_Error.TabStop = false;
            // 
            // JogPitchAxis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Controls.Add(this.pB_Error);
            this.Controls.Add(this.label_AxisName);
            this.Controls.Add(this.label_Disable);
            this.Controls.Add(this.pB_Disable);
            this.Controls.Add(this.label_Position);
            this.Controls.Add(this.label_StandStill);
            this.Controls.Add(this.pB_StandStill);
            this.Controls.Add(this.tB_Position);
            this.Name = "JogPitchAxis";
            this.Size = new System.Drawing.Size(92, 100);
            ((System.ComponentModel.ISupportInitialize)(this.pB_Disable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pB_StandStill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pB_Error)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_Disable;
        private System.Windows.Forms.Label label_Position;
        private System.Windows.Forms.PictureBox pB_Disable;
        private System.Windows.Forms.TextBox tB_Position;
        private System.Windows.Forms.PictureBox pB_StandStill;
        private System.Windows.Forms.Label label_StandStill;
        private System.Windows.Forms.Label label_AxisName;
        private System.Windows.Forms.PictureBox pB_Error;
    }
}
