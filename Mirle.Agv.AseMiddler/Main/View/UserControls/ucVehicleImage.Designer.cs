namespace Mirle.Agv.AseMiddler
{
    partial class UcVehicleImage
    {
        /// <summary> 
        /// 設計工具所需的變數.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源.
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true, 否則為 false.</param>
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
        /// 這個方法的內容.
        /// </summary>
        private void InitializeComponent()
        {
            this.picVeh = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picVeh)).BeginInit();
            this.SuspendLayout();
            // 
            // picVeh
            // 
            this.picVeh.BackColor = System.Drawing.Color.Transparent;
            this.picVeh.Image = global::Mirle.Agv.AseMiddler.Properties.Resources.VehHasNoCarrier;
            this.picVeh.Location = new System.Drawing.Point(0, 3);
            this.picVeh.Name = "picVeh";
            this.picVeh.Size = new System.Drawing.Size(40, 28);
            this.picVeh.TabIndex = 0;
            this.picVeh.TabStop = false;
            // 
            // ucVehicleImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.picVeh);
            this.Name = "ucVehicleImage";
            this.Size = new System.Drawing.Size(43, 34);
            ((System.ComponentModel.ISupportInitialize)(this.picVeh)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox picVeh;
    }
}
