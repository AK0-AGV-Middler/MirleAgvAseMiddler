namespace Mirle.Agv
{
    partial class SafetyInformation
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
            this.tB_Range = new System.Windows.Forms.TextBox();
            this.button_RangeSet = new System.Windows.Forms.Button();
            this.label_Range = new System.Windows.Forms.Label();
            this.button_Change = new System.Windows.Forms.Button();
            this.label_Name = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tB_Range
            // 
            this.tB_Range.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tB_Range.Location = new System.Drawing.Point(465, 0);
            this.tB_Range.Name = "tB_Range";
            this.tB_Range.Size = new System.Drawing.Size(98, 30);
            this.tB_Range.TabIndex = 11;
            this.tB_Range.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button_RangeSet
            // 
            this.button_RangeSet.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_RangeSet.Location = new System.Drawing.Point(569, 0);
            this.button_RangeSet.Name = "button_RangeSet";
            this.button_RangeSet.Size = new System.Drawing.Size(79, 30);
            this.button_RangeSet.TabIndex = 10;
            this.button_RangeSet.Text = "設定";
            this.button_RangeSet.UseVisualStyleBackColor = true;
            this.button_RangeSet.Click += new System.EventHandler(this.button_RangeSet_Click);
            // 
            // label_Range
            // 
            this.label_Range.AutoSize = true;
            this.label_Range.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Range.Location = new System.Drawing.Point(209, 6);
            this.label_Range.Name = "label_Range";
            this.label_Range.Size = new System.Drawing.Size(65, 19);
            this.label_Range.TabIndex = 9;
            this.label_Range.Text = "Range :";
            // 
            // button_Change
            // 
            this.button_Change.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_Change.Location = new System.Drawing.Point(105, 0);
            this.button_Change.Name = "button_Change";
            this.button_Change.Size = new System.Drawing.Size(79, 30);
            this.button_Change.TabIndex = 8;
            this.button_Change.Text = "關閉";
            this.button_Change.UseVisualStyleBackColor = true;
            this.button_Change.Click += new System.EventHandler(this.button_Change_Click);
            // 
            // label_Name
            // 
            this.label_Name.AutoSize = true;
            this.label_Name.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Name.Location = new System.Drawing.Point(-1, 6);
            this.label_Name.Name = "label_Name";
            this.label_Name.Size = new System.Drawing.Size(95, 19);
            this.label_Name.TabIndex = 7;
            this.label_Name.Text = "安全保護 :";
            // 
            // SafetyInformation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tB_Range);
            this.Controls.Add(this.button_RangeSet);
            this.Controls.Add(this.label_Range);
            this.Controls.Add(this.button_Change);
            this.Controls.Add(this.label_Name);
            this.Name = "SafetyInformation";
            this.Size = new System.Drawing.Size(647, 29);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tB_Range;
        private System.Windows.Forms.Button button_RangeSet;
        private System.Windows.Forms.Label label_Range;
        private System.Windows.Forms.Button button_Change;
        private System.Windows.Forms.Label label_Name;
    }
}
