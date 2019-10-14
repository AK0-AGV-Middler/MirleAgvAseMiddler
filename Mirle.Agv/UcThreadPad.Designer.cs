namespace Mirle.Agv
{
    partial class UcThreadPad
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
            this.gbThreadPad = new System.Windows.Forms.GroupBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnResume = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.txtThreadStatus = new System.Windows.Forms.Label();
            this.gbThreadPad.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbThreadPad
            // 
            this.gbThreadPad.Controls.Add(this.txtThreadStatus);
            this.gbThreadPad.Controls.Add(this.btnStart);
            this.gbThreadPad.Controls.Add(this.btnResume);
            this.gbThreadPad.Controls.Add(this.btnStop);
            this.gbThreadPad.Controls.Add(this.btnPause);
            this.gbThreadPad.Location = new System.Drawing.Point(3, 3);
            this.gbThreadPad.Name = "gbThreadPad";
            this.gbThreadPad.Size = new System.Drawing.Size(166, 104);
            this.gbThreadPad.TabIndex = 43;
            this.gbThreadPad.TabStop = false;
            this.gbThreadPad.Text = "Title Text";
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStart.Location = new System.Drawing.Point(0, 21);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(35, 35);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "S";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // btnResume
            // 
            this.btnResume.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnResume.Location = new System.Drawing.Point(82, 21);
            this.btnResume.Name = "btnResume";
            this.btnResume.Size = new System.Drawing.Size(35, 35);
            this.btnResume.TabIndex = 8;
            this.btnResume.Text = "R";
            this.btnResume.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStop.Location = new System.Drawing.Point(123, 21);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(35, 35);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "C";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // btnPause
            // 
            this.btnPause.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPause.Location = new System.Drawing.Point(41, 21);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(35, 35);
            this.btnPause.TabIndex = 7;
            this.btnPause.Text = "P";
            this.btnPause.UseVisualStyleBackColor = true;
            // 
            // txtThreadStatus
            // 
            this.txtThreadStatus.BackColor = System.Drawing.Color.OrangeRed;
            this.txtThreadStatus.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtThreadStatus.Location = new System.Drawing.Point(0, 59);
            this.txtThreadStatus.Name = "txtThreadStatus";
            this.txtThreadStatus.Size = new System.Drawing.Size(158, 38);
            this.txtThreadStatus.TabIndex = 9;
            this.txtThreadStatus.Text = "StatusText";
            this.txtThreadStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UcThreadPad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbThreadPad);
            this.Name = "UcThreadPad";
            this.Size = new System.Drawing.Size(173, 110);
            this.gbThreadPad.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox gbThreadPad;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnResume;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Label txtThreadStatus;
    }
}
