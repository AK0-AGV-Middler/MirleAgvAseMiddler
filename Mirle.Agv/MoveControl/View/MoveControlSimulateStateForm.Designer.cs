namespace Mirle.AgvAseMiddler.View
{
    partial class MoveControlSimulateStateForm
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
            this.gB_SimulateBeamSensor = new System.Windows.Forms.GroupBox();
            this.radioButton_BeamSensor_Stop = new System.Windows.Forms.RadioButton();
            this.radioButton_BeamSensor_Normal = new System.Windows.Forms.RadioButton();
            this.radioButton_BeamSensor_LowSpeed = new System.Windows.Forms.RadioButton();
            this.gB_SimulateBumpSensor = new System.Windows.Forms.GroupBox();
            this.radioButton_BumpSensor_Stop = new System.Windows.Forms.RadioButton();
            this.radioButton_BumpSensor_Normal = new System.Windows.Forms.RadioButton();
            this.gB_SimulateAxisState = new System.Windows.Forms.GroupBox();
            this.radioButton_SimulateAxisError = new System.Windows.Forms.RadioButton();
            this.radioButton_SimulateAxisNormal = new System.Windows.Forms.RadioButton();
            this.gB_SimulateCharging = new System.Windows.Forms.GroupBox();
            this.radioButton_SimulateChargingYes = new System.Windows.Forms.RadioButton();
            this.radioButton_SimulateChargingNo = new System.Windows.Forms.RadioButton();
            this.radioButton_SimulateForkState = new System.Windows.Forms.GroupBox();
            this.radioButton_SimulateForkNotHome = new System.Windows.Forms.RadioButton();
            this.radioButton_SimulateForkHome = new System.Windows.Forms.RadioButton();
            this.gB_SimulateMCommand = new System.Windows.Forms.GroupBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_Continue = new System.Windows.Forms.Button();
            this.button_Pause = new System.Windows.Forms.Button();
            this.gB_SimulateBeamSensor.SuspendLayout();
            this.gB_SimulateBumpSensor.SuspendLayout();
            this.gB_SimulateAxisState.SuspendLayout();
            this.gB_SimulateCharging.SuspendLayout();
            this.radioButton_SimulateForkState.SuspendLayout();
            this.gB_SimulateMCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // gB_SimulateBeamSensor
            // 
            this.gB_SimulateBeamSensor.Controls.Add(this.radioButton_BeamSensor_Stop);
            this.gB_SimulateBeamSensor.Controls.Add(this.radioButton_BeamSensor_Normal);
            this.gB_SimulateBeamSensor.Controls.Add(this.radioButton_BeamSensor_LowSpeed);
            this.gB_SimulateBeamSensor.Location = new System.Drawing.Point(12, 17);
            this.gB_SimulateBeamSensor.Name = "gB_SimulateBeamSensor";
            this.gB_SimulateBeamSensor.Size = new System.Drawing.Size(90, 122);
            this.gB_SimulateBeamSensor.TabIndex = 0;
            this.gB_SimulateBeamSensor.TabStop = false;
            this.gB_SimulateBeamSensor.Text = "BeamSensor";
            // 
            // radioButton_BeamSensor_Stop
            // 
            this.radioButton_BeamSensor_Stop.AutoSize = true;
            this.radioButton_BeamSensor_Stop.Location = new System.Drawing.Point(9, 93);
            this.radioButton_BeamSensor_Stop.Name = "radioButton_BeamSensor_Stop";
            this.radioButton_BeamSensor_Stop.Size = new System.Drawing.Size(44, 16);
            this.radioButton_BeamSensor_Stop.TabIndex = 2;
            this.radioButton_BeamSensor_Stop.TabStop = true;
            this.radioButton_BeamSensor_Stop.Text = "Stop";
            this.radioButton_BeamSensor_Stop.UseVisualStyleBackColor = true;
            this.radioButton_BeamSensor_Stop.CheckedChanged += new System.EventHandler(this.radioButton_BeamSensor_CheckedChanged);
            // 
            // radioButton_BeamSensor_Normal
            // 
            this.radioButton_BeamSensor_Normal.AutoSize = true;
            this.radioButton_BeamSensor_Normal.Location = new System.Drawing.Point(9, 25);
            this.radioButton_BeamSensor_Normal.Name = "radioButton_BeamSensor_Normal";
            this.radioButton_BeamSensor_Normal.Size = new System.Drawing.Size(58, 16);
            this.radioButton_BeamSensor_Normal.TabIndex = 1;
            this.radioButton_BeamSensor_Normal.TabStop = true;
            this.radioButton_BeamSensor_Normal.Text = "Normal";
            this.radioButton_BeamSensor_Normal.UseVisualStyleBackColor = true;
            this.radioButton_BeamSensor_Normal.CheckedChanged += new System.EventHandler(this.radioButton_BeamSensor_CheckedChanged);
            // 
            // radioButton_BeamSensor_LowSpeed
            // 
            this.radioButton_BeamSensor_LowSpeed.AutoSize = true;
            this.radioButton_BeamSensor_LowSpeed.Location = new System.Drawing.Point(9, 57);
            this.radioButton_BeamSensor_LowSpeed.Name = "radioButton_BeamSensor_LowSpeed";
            this.radioButton_BeamSensor_LowSpeed.Size = new System.Drawing.Size(72, 16);
            this.radioButton_BeamSensor_LowSpeed.TabIndex = 0;
            this.radioButton_BeamSensor_LowSpeed.TabStop = true;
            this.radioButton_BeamSensor_LowSpeed.Text = "LowSpeed";
            this.radioButton_BeamSensor_LowSpeed.UseVisualStyleBackColor = true;
            this.radioButton_BeamSensor_LowSpeed.CheckedChanged += new System.EventHandler(this.radioButton_BeamSensor_CheckedChanged);
            // 
            // gB_SimulateBumpSensor
            // 
            this.gB_SimulateBumpSensor.Controls.Add(this.radioButton_BumpSensor_Stop);
            this.gB_SimulateBumpSensor.Controls.Add(this.radioButton_BumpSensor_Normal);
            this.gB_SimulateBumpSensor.Location = new System.Drawing.Point(109, 17);
            this.gB_SimulateBumpSensor.Name = "gB_SimulateBumpSensor";
            this.gB_SimulateBumpSensor.Size = new System.Drawing.Size(90, 122);
            this.gB_SimulateBumpSensor.TabIndex = 3;
            this.gB_SimulateBumpSensor.TabStop = false;
            this.gB_SimulateBumpSensor.Text = "BumpSensor";
            // 
            // radioButton_BumpSensor_Stop
            // 
            this.radioButton_BumpSensor_Stop.AutoSize = true;
            this.radioButton_BumpSensor_Stop.Location = new System.Drawing.Point(9, 57);
            this.radioButton_BumpSensor_Stop.Name = "radioButton_BumpSensor_Stop";
            this.radioButton_BumpSensor_Stop.Size = new System.Drawing.Size(44, 16);
            this.radioButton_BumpSensor_Stop.TabIndex = 2;
            this.radioButton_BumpSensor_Stop.TabStop = true;
            this.radioButton_BumpSensor_Stop.Text = "Stop";
            this.radioButton_BumpSensor_Stop.UseVisualStyleBackColor = true;
            this.radioButton_BumpSensor_Stop.CheckedChanged += new System.EventHandler(this.radioButton_BumpSensor_CheckedChanged);
            // 
            // radioButton_BumpSensor_Normal
            // 
            this.radioButton_BumpSensor_Normal.AutoSize = true;
            this.radioButton_BumpSensor_Normal.Location = new System.Drawing.Point(9, 25);
            this.radioButton_BumpSensor_Normal.Name = "radioButton_BumpSensor_Normal";
            this.radioButton_BumpSensor_Normal.Size = new System.Drawing.Size(58, 16);
            this.radioButton_BumpSensor_Normal.TabIndex = 1;
            this.radioButton_BumpSensor_Normal.TabStop = true;
            this.radioButton_BumpSensor_Normal.Text = "Normal";
            this.radioButton_BumpSensor_Normal.UseVisualStyleBackColor = true;
            this.radioButton_BumpSensor_Normal.CheckedChanged += new System.EventHandler(this.radioButton_BumpSensor_CheckedChanged);
            // 
            // gB_SimulateAxisState
            // 
            this.gB_SimulateAxisState.Controls.Add(this.radioButton_SimulateAxisError);
            this.gB_SimulateAxisState.Controls.Add(this.radioButton_SimulateAxisNormal);
            this.gB_SimulateAxisState.Location = new System.Drawing.Point(205, 17);
            this.gB_SimulateAxisState.Name = "gB_SimulateAxisState";
            this.gB_SimulateAxisState.Size = new System.Drawing.Size(90, 122);
            this.gB_SimulateAxisState.TabIndex = 4;
            this.gB_SimulateAxisState.TabStop = false;
            this.gB_SimulateAxisState.Text = "AxisState";
            // 
            // radioButton_SimulateAxisError
            // 
            this.radioButton_SimulateAxisError.AutoSize = true;
            this.radioButton_SimulateAxisError.Location = new System.Drawing.Point(9, 57);
            this.radioButton_SimulateAxisError.Name = "radioButton_SimulateAxisError";
            this.radioButton_SimulateAxisError.Size = new System.Drawing.Size(48, 16);
            this.radioButton_SimulateAxisError.TabIndex = 2;
            this.radioButton_SimulateAxisError.TabStop = true;
            this.radioButton_SimulateAxisError.Text = "Error";
            this.radioButton_SimulateAxisError.UseVisualStyleBackColor = true;
            this.radioButton_SimulateAxisError.CheckedChanged += new System.EventHandler(this.radioButton_SimulateAxisState_CheckedChanged);
            // 
            // radioButton_SimulateAxisNormal
            // 
            this.radioButton_SimulateAxisNormal.AutoSize = true;
            this.radioButton_SimulateAxisNormal.Location = new System.Drawing.Point(9, 25);
            this.radioButton_SimulateAxisNormal.Name = "radioButton_SimulateAxisNormal";
            this.radioButton_SimulateAxisNormal.Size = new System.Drawing.Size(58, 16);
            this.radioButton_SimulateAxisNormal.TabIndex = 1;
            this.radioButton_SimulateAxisNormal.TabStop = true;
            this.radioButton_SimulateAxisNormal.Text = "Normal";
            this.radioButton_SimulateAxisNormal.UseVisualStyleBackColor = true;
            this.radioButton_SimulateAxisNormal.CheckedChanged += new System.EventHandler(this.radioButton_SimulateAxisState_CheckedChanged);
            // 
            // gB_SimulateCharging
            // 
            this.gB_SimulateCharging.Controls.Add(this.radioButton_SimulateChargingYes);
            this.gB_SimulateCharging.Controls.Add(this.radioButton_SimulateChargingNo);
            this.gB_SimulateCharging.Location = new System.Drawing.Point(301, 17);
            this.gB_SimulateCharging.Name = "gB_SimulateCharging";
            this.gB_SimulateCharging.Size = new System.Drawing.Size(90, 122);
            this.gB_SimulateCharging.TabIndex = 4;
            this.gB_SimulateCharging.TabStop = false;
            this.gB_SimulateCharging.Text = "ChargingState";
            // 
            // radioButton_SimulateChargingYes
            // 
            this.radioButton_SimulateChargingYes.AutoSize = true;
            this.radioButton_SimulateChargingYes.Location = new System.Drawing.Point(6, 57);
            this.radioButton_SimulateChargingYes.Name = "radioButton_SimulateChargingYes";
            this.radioButton_SimulateChargingYes.Size = new System.Drawing.Size(40, 16);
            this.radioButton_SimulateChargingYes.TabIndex = 2;
            this.radioButton_SimulateChargingYes.TabStop = true;
            this.radioButton_SimulateChargingYes.Text = "Yes";
            this.radioButton_SimulateChargingYes.UseVisualStyleBackColor = true;
            this.radioButton_SimulateChargingYes.CheckedChanged += new System.EventHandler(this.radioButton_SimulateChargingState_CheckedChanged);
            // 
            // radioButton_SimulateChargingNo
            // 
            this.radioButton_SimulateChargingNo.AutoSize = true;
            this.radioButton_SimulateChargingNo.Location = new System.Drawing.Point(6, 25);
            this.radioButton_SimulateChargingNo.Name = "radioButton_SimulateChargingNo";
            this.radioButton_SimulateChargingNo.Size = new System.Drawing.Size(37, 16);
            this.radioButton_SimulateChargingNo.TabIndex = 1;
            this.radioButton_SimulateChargingNo.TabStop = true;
            this.radioButton_SimulateChargingNo.Text = "No";
            this.radioButton_SimulateChargingNo.UseVisualStyleBackColor = true;
            this.radioButton_SimulateChargingNo.CheckedChanged += new System.EventHandler(this.radioButton_SimulateChargingState_CheckedChanged);
            // 
            // radioButton_SimulateForkState
            // 
            this.radioButton_SimulateForkState.Controls.Add(this.radioButton_SimulateForkNotHome);
            this.radioButton_SimulateForkState.Controls.Add(this.radioButton_SimulateForkHome);
            this.radioButton_SimulateForkState.Location = new System.Drawing.Point(397, 17);
            this.radioButton_SimulateForkState.Name = "radioButton_SimulateForkState";
            this.radioButton_SimulateForkState.Size = new System.Drawing.Size(90, 122);
            this.radioButton_SimulateForkState.TabIndex = 4;
            this.radioButton_SimulateForkState.TabStop = false;
            this.radioButton_SimulateForkState.Text = "ForkState";
            // 
            // radioButton_SimulateForkNotHome
            // 
            this.radioButton_SimulateForkNotHome.AutoSize = true;
            this.radioButton_SimulateForkNotHome.Location = new System.Drawing.Point(6, 57);
            this.radioButton_SimulateForkNotHome.Name = "radioButton_SimulateForkNotHome";
            this.radioButton_SimulateForkNotHome.Size = new System.Drawing.Size(68, 16);
            this.radioButton_SimulateForkNotHome.TabIndex = 2;
            this.radioButton_SimulateForkNotHome.TabStop = true;
            this.radioButton_SimulateForkNotHome.Text = "NotHome";
            this.radioButton_SimulateForkNotHome.UseVisualStyleBackColor = true;
            this.radioButton_SimulateForkNotHome.CheckedChanged += new System.EventHandler(this.radioButton_SimulateForkState_CheckedChanged);
            // 
            // radioButton_SimulateForkHome
            // 
            this.radioButton_SimulateForkHome.AutoSize = true;
            this.radioButton_SimulateForkHome.Location = new System.Drawing.Point(6, 25);
            this.radioButton_SimulateForkHome.Name = "radioButton_SimulateForkHome";
            this.radioButton_SimulateForkHome.Size = new System.Drawing.Size(51, 16);
            this.radioButton_SimulateForkHome.TabIndex = 1;
            this.radioButton_SimulateForkHome.TabStop = true;
            this.radioButton_SimulateForkHome.Text = "Home";
            this.radioButton_SimulateForkHome.UseVisualStyleBackColor = true;
            this.radioButton_SimulateForkHome.CheckedChanged += new System.EventHandler(this.radioButton_SimulateForkState_CheckedChanged);
            // 
            // gB_SimulateMCommand
            // 
            this.gB_SimulateMCommand.Controls.Add(this.button_Cancel);
            this.gB_SimulateMCommand.Controls.Add(this.button_Continue);
            this.gB_SimulateMCommand.Controls.Add(this.button_Pause);
            this.gB_SimulateMCommand.Location = new System.Drawing.Point(493, 17);
            this.gB_SimulateMCommand.Name = "gB_SimulateMCommand";
            this.gB_SimulateMCommand.Size = new System.Drawing.Size(110, 122);
            this.gB_SimulateMCommand.TabIndex = 5;
            this.gB_SimulateMCommand.TabStop = false;
            this.gB_SimulateMCommand.Text = "MCommand";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(19, 86);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_Continue
            // 
            this.button_Continue.Location = new System.Drawing.Point(19, 51);
            this.button_Continue.Name = "button_Continue";
            this.button_Continue.Size = new System.Drawing.Size(75, 23);
            this.button_Continue.TabIndex = 1;
            this.button_Continue.Text = "Continue";
            this.button_Continue.UseVisualStyleBackColor = true;
            this.button_Continue.Click += new System.EventHandler(this.button_Continue_Click);
            // 
            // button_Pause
            // 
            this.button_Pause.Location = new System.Drawing.Point(19, 18);
            this.button_Pause.Name = "button_Pause";
            this.button_Pause.Size = new System.Drawing.Size(75, 23);
            this.button_Pause.TabIndex = 0;
            this.button_Pause.Text = "Pause";
            this.button_Pause.UseVisualStyleBackColor = true;
            this.button_Pause.Click += new System.EventHandler(this.button_Pause_Click);
            // 
            // MoveControlSimulateStateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 149);
            this.Controls.Add(this.gB_SimulateMCommand);
            this.Controls.Add(this.radioButton_SimulateForkState);
            this.Controls.Add(this.gB_SimulateCharging);
            this.Controls.Add(this.gB_SimulateAxisState);
            this.Controls.Add(this.gB_SimulateBumpSensor);
            this.Controls.Add(this.gB_SimulateBeamSensor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoveControlSimulateStateForm";
            this.ShowInTaskbar = false;
            this.Text = "MoveControlSimulateStateForm";
            this.gB_SimulateBeamSensor.ResumeLayout(false);
            this.gB_SimulateBeamSensor.PerformLayout();
            this.gB_SimulateBumpSensor.ResumeLayout(false);
            this.gB_SimulateBumpSensor.PerformLayout();
            this.gB_SimulateAxisState.ResumeLayout(false);
            this.gB_SimulateAxisState.PerformLayout();
            this.gB_SimulateCharging.ResumeLayout(false);
            this.gB_SimulateCharging.PerformLayout();
            this.radioButton_SimulateForkState.ResumeLayout(false);
            this.radioButton_SimulateForkState.PerformLayout();
            this.gB_SimulateMCommand.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gB_SimulateBeamSensor;
        private System.Windows.Forms.RadioButton radioButton_BeamSensor_Stop;
        private System.Windows.Forms.RadioButton radioButton_BeamSensor_Normal;
        private System.Windows.Forms.RadioButton radioButton_BeamSensor_LowSpeed;
        private System.Windows.Forms.GroupBox gB_SimulateBumpSensor;
        private System.Windows.Forms.RadioButton radioButton_BumpSensor_Stop;
        private System.Windows.Forms.RadioButton radioButton_BumpSensor_Normal;
        private System.Windows.Forms.GroupBox gB_SimulateAxisState;
        private System.Windows.Forms.RadioButton radioButton_SimulateAxisError;
        private System.Windows.Forms.RadioButton radioButton_SimulateAxisNormal;
        private System.Windows.Forms.GroupBox gB_SimulateCharging;
        private System.Windows.Forms.RadioButton radioButton_SimulateChargingYes;
        private System.Windows.Forms.RadioButton radioButton_SimulateChargingNo;
        private System.Windows.Forms.GroupBox radioButton_SimulateForkState;
        private System.Windows.Forms.RadioButton radioButton_SimulateForkNotHome;
        private System.Windows.Forms.RadioButton radioButton_SimulateForkHome;
        private System.Windows.Forms.GroupBox gB_SimulateMCommand;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_Continue;
        private System.Windows.Forms.Button button_Pause;
    }
}