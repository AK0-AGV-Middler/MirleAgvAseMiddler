namespace Mirle.Agv
{
    partial class ConfigsNameAndValue
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
            this.tB_Value = new System.Windows.Forms.TextBox();
            this.button_Set = new System.Windows.Forms.Button();
            this.label_Name = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tB_Value
            // 
            this.tB_Value.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tB_Value.Location = new System.Drawing.Point(267, 2);
            this.tB_Value.Name = "tB_Value";
            this.tB_Value.Size = new System.Drawing.Size(91, 27);
            this.tB_Value.TabIndex = 14;
            this.tB_Value.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button_Set
            // 
            this.button_Set.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_Set.Location = new System.Drawing.Point(358, 0);
            this.button_Set.Name = "button_Set";
            this.button_Set.Size = new System.Drawing.Size(55, 30);
            this.button_Set.TabIndex = 13;
            this.button_Set.Text = "設定";
            this.button_Set.UseVisualStyleBackColor = true;
            this.button_Set.Click += new System.EventHandler(this.button_Set_Click);
            // 
            // label_Name
            // 
            this.label_Name.AutoSize = true;
            this.label_Name.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Name.Location = new System.Drawing.Point(3, 7);
            this.label_Name.Name = "label_Name";
            this.label_Name.Size = new System.Drawing.Size(56, 16);
            this.label_Name.TabIndex = 12;
            this.label_Name.Text = "Range :";
            // 
            // ConfigsNameAndValue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tB_Value);
            this.Controls.Add(this.button_Set);
            this.Controls.Add(this.label_Name);
            this.Name = "ConfigsNameAndValue";
            this.Size = new System.Drawing.Size(414, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tB_Value;
        private System.Windows.Forms.Button button_Set;
        private System.Windows.Forms.Label label_Name;
    }
}
