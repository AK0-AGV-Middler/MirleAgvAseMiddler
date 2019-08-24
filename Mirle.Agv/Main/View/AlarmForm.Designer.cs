﻿namespace Mirle.Agv.View
{
    partial class AlarmForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlarmForm));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBuzzOff = new System.Windows.Forms.Button();
            this.btnAlarmReset = new System.Windows.Forms.Button();
            this.rtbHappeningAlarms = new System.Windows.Forms.RichTextBox();
            this.rtbHistoryAlarms = new System.Windows.Forms.RichTextBox();
            this.btnTestSetAlarm = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.rtbHistoryAlarms, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.rtbHappeningAlarms, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnTestSetAlarm, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.75117F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.24883F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1311, 728);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnBuzzOff);
            this.panel1.Controls.Add(this.btnAlarmReset);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 590);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(649, 135);
            this.panel1.TabIndex = 3;
            // 
            // btnBuzzOff
            // 
            this.btnBuzzOff.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBuzzOff.ForeColor = System.Drawing.Color.Brown;
            this.btnBuzzOff.Location = new System.Drawing.Point(167, 0);
            this.btnBuzzOff.Name = "btnBuzzOff";
            this.btnBuzzOff.Size = new System.Drawing.Size(150, 132);
            this.btnBuzzOff.TabIndex = 7;
            this.btnBuzzOff.Text = "Buzz Off";
            this.btnBuzzOff.UseVisualStyleBackColor = true;
            this.btnBuzzOff.Click += new System.EventHandler(this.btnBuzzOff_Click);
            // 
            // btnAlarmReset
            // 
            this.btnAlarmReset.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAlarmReset.ForeColor = System.Drawing.Color.Red;
            this.btnAlarmReset.Location = new System.Drawing.Point(3, 0);
            this.btnAlarmReset.Name = "btnAlarmReset";
            this.btnAlarmReset.Size = new System.Drawing.Size(158, 132);
            this.btnAlarmReset.TabIndex = 6;
            this.btnAlarmReset.Text = "Alarm Reset";
            this.btnAlarmReset.UseVisualStyleBackColor = true;
            this.btnAlarmReset.Click += new System.EventHandler(this.btnAlarmReset_Click);
            // 
            // rtbHappeningAlarms
            // 
            this.rtbHappeningAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbHappeningAlarms.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rtbHappeningAlarms.ForeColor = System.Drawing.Color.OrangeRed;
            this.rtbHappeningAlarms.Location = new System.Drawing.Point(3, 3);
            this.rtbHappeningAlarms.Name = "rtbHappeningAlarms";
            this.rtbHappeningAlarms.Size = new System.Drawing.Size(649, 581);
            this.rtbHappeningAlarms.TabIndex = 35;
            this.rtbHappeningAlarms.Text = "";
            // 
            // rtbHistoryAlarms
            // 
            this.rtbHistoryAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbHistoryAlarms.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rtbHistoryAlarms.ForeColor = System.Drawing.Color.OrangeRed;
            this.rtbHistoryAlarms.Location = new System.Drawing.Point(658, 3);
            this.rtbHistoryAlarms.Name = "rtbHistoryAlarms";
            this.rtbHistoryAlarms.Size = new System.Drawing.Size(650, 581);
            this.rtbHistoryAlarms.TabIndex = 36;
            this.rtbHistoryAlarms.Text = "";
            // 
            // btnTestSetAlarm
            // 
            this.btnTestSetAlarm.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnTestSetAlarm.ForeColor = System.Drawing.Color.Brown;
            this.btnTestSetAlarm.Location = new System.Drawing.Point(658, 590);
            this.btnTestSetAlarm.Name = "btnTestSetAlarm";
            this.btnTestSetAlarm.Size = new System.Drawing.Size(155, 135);
            this.btnTestSetAlarm.TabIndex = 8;
            this.btnTestSetAlarm.Text = "Test SetAlarm";
            this.btnTestSetAlarm.UseVisualStyleBackColor = true;
            this.btnTestSetAlarm.Click += new System.EventHandler(this.btnTestSetAlarm_Click);
            // 
            // AlarmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1311, 728);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AlarmForm";
            this.Text = "AlarmView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnAlarmReset;
        private System.Windows.Forms.Button btnBuzzOff;
        private System.Windows.Forms.RichTextBox rtbHistoryAlarms;
        private System.Windows.Forms.RichTextBox rtbHappeningAlarms;
        private System.Windows.Forms.Button btnTestSetAlarm;
    }
}