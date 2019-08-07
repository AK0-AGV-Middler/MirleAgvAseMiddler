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
            this.pbPause = new System.Windows.Forms.PictureBox();
            this.pbStop = new System.Windows.Forms.PictureBox();
            this.gbThreadPad = new System.Windows.Forms.GroupBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnResume = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStop)).BeginInit();
            this.gbThreadPad.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbPause
            // 
            this.pbPause.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pbPause.Location = new System.Drawing.Point(134, 3);
            this.pbPause.Name = "pbPause";
            this.pbPause.Size = new System.Drawing.Size(14, 10);
            this.pbPause.TabIndex = 0;
            this.pbPause.TabStop = false;
            // 
            // pbStop
            // 
            this.pbStop.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pbStop.Location = new System.Drawing.Point(154, 3);
            this.pbStop.Name = "pbStop";
            this.pbStop.Size = new System.Drawing.Size(14, 10);
            this.pbStop.TabIndex = 1;
            this.pbStop.TabStop = false;
            // 
            // gbThreadPad
            // 
            this.gbThreadPad.Controls.Add(this.btnStart);
            this.gbThreadPad.Controls.Add(this.btnResume);
            this.gbThreadPad.Controls.Add(this.btnStop);
            this.gbThreadPad.Controls.Add(this.btnPause);
            this.gbThreadPad.Location = new System.Drawing.Point(3, 19);
            this.gbThreadPad.Name = "gbThreadPad";
            this.gbThreadPad.Size = new System.Drawing.Size(165, 143);
            this.gbThreadPad.TabIndex = 43;
            this.gbThreadPad.TabStop = false;
            this.gbThreadPad.Text = "Tracking Position";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(6, 21);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(150, 23);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // btnResume
            // 
            this.btnResume.Location = new System.Drawing.Point(6, 79);
            this.btnResume.Name = "btnResume";
            this.btnResume.Size = new System.Drawing.Size(150, 23);
            this.btnResume.TabIndex = 8;
            this.btnResume.Text = "Resume";
            this.btnResume.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(6, 108);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(150, 23);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(6, 50);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(150, 23);
            this.btnPause.TabIndex = 7;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            // 
            // UcThreadPad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbThreadPad);
            this.Controls.Add(this.pbStop);
            this.Controls.Add(this.pbPause);
            this.Name = "UcThreadPad";
            this.Size = new System.Drawing.Size(177, 168);
            ((System.ComponentModel.ISupportInitialize)(this.pbPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStop)).EndInit();
            this.gbThreadPad.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbPause;
        private System.Windows.Forms.PictureBox pbStop;
        private System.Windows.Forms.GroupBox gbThreadPad;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnResume;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnPause;
    }
}
