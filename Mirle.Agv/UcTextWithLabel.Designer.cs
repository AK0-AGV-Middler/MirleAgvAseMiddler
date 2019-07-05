namespace Mirle.Agv.View
{
    partial class UcTextWithLabel
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ucLabel = new System.Windows.Forms.Label();
            this.ucTextBox = new System.Windows.Forms.TextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ucLabel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ucTextBox);
            this.splitContainer1.Size = new System.Drawing.Size(240, 30);
            this.splitContainer1.SplitterDistance = 119;
            this.splitContainer1.TabIndex = 0;
            // 
            // ucLabel
            // 
            this.ucLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucLabel.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ucLabel.Location = new System.Drawing.Point(0, 0);
            this.ucLabel.Name = "ucLabel";
            this.ucLabel.Size = new System.Drawing.Size(119, 30);
            this.ucLabel.TabIndex = 0;
            this.ucLabel.Text = "EncoderPosition";
            this.ucLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucTextBox
            // 
            this.ucTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucTextBox.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ucTextBox.Location = new System.Drawing.Point(0, 0);
            this.ucTextBox.Name = "ucTextBox";
            this.ucTextBox.ReadOnly = true;
            this.ucTextBox.Size = new System.Drawing.Size(117, 27);
            this.ucTextBox.TabIndex = 0;
            this.ucTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // UcTextWithLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "UcTextWithLabel";
            this.Size = new System.Drawing.Size(240, 30);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label ucLabel;
        private System.Windows.Forms.TextBox ucTextBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}
