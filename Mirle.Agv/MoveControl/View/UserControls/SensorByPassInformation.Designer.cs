namespace Mirle.AgvAseMiddler
{
    partial class SensorByPassInformation
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
            this.button_Change = new System.Windows.Forms.Button();
            this.label_Name = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_Change
            // 
            this.button_Change.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_Change.Location = new System.Drawing.Point(141, 0);
            this.button_Change.Name = "button_Change";
            this.button_Change.Size = new System.Drawing.Size(79, 30);
            this.button_Change.TabIndex = 10;
            this.button_Change.Text = "關閉";
            this.button_Change.UseVisualStyleBackColor = true;
            this.button_Change.Click += new System.EventHandler(this.button_Change_Click);
            // 
            // label_Name
            // 
            this.label_Name.AutoSize = true;
            this.label_Name.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Name.Location = new System.Drawing.Point(4, 6);
            this.label_Name.Name = "label_Name";
            this.label_Name.Size = new System.Drawing.Size(95, 19);
            this.label_Name.TabIndex = 9;
            this.label_Name.Text = "安全保護 :";
            // 
            // SensorByPassInformation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button_Change);
            this.Controls.Add(this.label_Name);
            this.Name = "SensorByPassInformation";
            this.Size = new System.Drawing.Size(220, 29);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Change;
        private System.Windows.Forms.Label label_Name;
    }
}
