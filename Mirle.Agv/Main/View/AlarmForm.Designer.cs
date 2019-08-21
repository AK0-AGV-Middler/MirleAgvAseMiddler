namespace Mirle.Agv.View
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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.listHappeningAlarms = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnResetAllAlarms = new System.Windows.Forms.Button();
            this.btnResetIdAlarm = new System.Windows.Forms.Button();
            this.numSelectAlarmId = new System.Windows.Forms.NumericUpDown();
            this.btnResetSelectAlarm = new System.Windows.Forms.Button();
            this.timerRefreshHappenAlarm = new System.Windows.Forms.Timer(this.components);
            this.timerRefreshHistoryAlarm = new System.Windows.Forms.Timer(this.components);
            this.btnSetTestAlarm = new System.Windows.Forms.Button();
            this.listHistoryAlarms = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSelectAlarmId)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.listHappeningAlarms, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.listHistoryAlarms, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.75117F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.24883F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1195, 596);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // listHappeningAlarms
            // 
            this.listHappeningAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listHappeningAlarms.FormattingEnabled = true;
            this.listHappeningAlarms.HorizontalScrollbar = true;
            this.listHappeningAlarms.ItemHeight = 12;
            this.listHappeningAlarms.Location = new System.Drawing.Point(3, 3);
            this.listHappeningAlarms.Name = "listHappeningAlarms";
            this.listHappeningAlarms.ScrollAlwaysVisible = true;
            this.listHappeningAlarms.Size = new System.Drawing.Size(591, 475);
            this.listHappeningAlarms.TabIndex = 0;
            this.listHappeningAlarms.SelectedIndexChanged += new System.EventHandler(this.listHappeningAlarms_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSetTestAlarm);
            this.panel1.Controls.Add(this.btnResetAllAlarms);
            this.panel1.Controls.Add(this.btnResetIdAlarm);
            this.panel1.Controls.Add(this.numSelectAlarmId);
            this.panel1.Controls.Add(this.btnResetSelectAlarm);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 484);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(591, 109);
            this.panel1.TabIndex = 3;
            // 
            // btnResetAllAlarms
            // 
            this.btnResetAllAlarms.Location = new System.Drawing.Point(3, 68);
            this.btnResetAllAlarms.Name = "btnResetAllAlarms";
            this.btnResetAllAlarms.Size = new System.Drawing.Size(252, 32);
            this.btnResetAllAlarms.TabIndex = 5;
            this.btnResetAllAlarms.Text = "Reset All Alarms";
            this.btnResetAllAlarms.UseVisualStyleBackColor = true;
            this.btnResetAllAlarms.Click += new System.EventHandler(this.btnResetAllAlarms_Click);
            // 
            // btnResetIdAlarm
            // 
            this.btnResetIdAlarm.Location = new System.Drawing.Point(89, 36);
            this.btnResetIdAlarm.Name = "btnResetIdAlarm";
            this.btnResetIdAlarm.Size = new System.Drawing.Size(166, 27);
            this.btnResetIdAlarm.TabIndex = 4;
            this.btnResetIdAlarm.Text = "Reset #Id Alarm";
            this.btnResetIdAlarm.UseVisualStyleBackColor = true;
            this.btnResetIdAlarm.Click += new System.EventHandler(this.btnResetIdAlarm_Click);
            // 
            // numSelectAlarmId
            // 
            this.numSelectAlarmId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.numSelectAlarmId.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numSelectAlarmId.Location = new System.Drawing.Point(3, 36);
            this.numSelectAlarmId.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numSelectAlarmId.Name = "numSelectAlarmId";
            this.numSelectAlarmId.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.numSelectAlarmId.Size = new System.Drawing.Size(80, 27);
            this.numSelectAlarmId.TabIndex = 3;
            this.numSelectAlarmId.Value = new decimal(new int[] {
            12345,
            0,
            0,
            0});
            // 
            // btnResetSelectAlarm
            // 
            this.btnResetSelectAlarm.Location = new System.Drawing.Point(3, 3);
            this.btnResetSelectAlarm.Name = "btnResetSelectAlarm";
            this.btnResetSelectAlarm.Size = new System.Drawing.Size(252, 32);
            this.btnResetSelectAlarm.TabIndex = 2;
            this.btnResetSelectAlarm.Text = "Reset Select Alarm";
            this.btnResetSelectAlarm.UseVisualStyleBackColor = true;
            this.btnResetSelectAlarm.Click += new System.EventHandler(this.btnResetSelectAlarm_Click);
            // 
            // timerRefreshHappenAlarm
            // 
            this.timerRefreshHappenAlarm.Enabled = true;
            this.timerRefreshHappenAlarm.Tick += new System.EventHandler(this.timerRefreshHappenAlarm_Tick);
            // 
            // timerRefreshHistoryAlarm
            // 
            this.timerRefreshHistoryAlarm.Enabled = true;
            this.timerRefreshHistoryAlarm.Interval = 1500;
            this.timerRefreshHistoryAlarm.Tick += new System.EventHandler(this.timerRefreshHistoryAlarm_Tick);
            // 
            // btnSetTestAlarm
            // 
            this.btnSetTestAlarm.Location = new System.Drawing.Point(476, 68);
            this.btnSetTestAlarm.Name = "btnSetTestAlarm";
            this.btnSetTestAlarm.Size = new System.Drawing.Size(112, 32);
            this.btnSetTestAlarm.TabIndex = 6;
            this.btnSetTestAlarm.Text = "Set Test Alarm";
            this.btnSetTestAlarm.UseVisualStyleBackColor = true;
            this.btnSetTestAlarm.Click += new System.EventHandler(this.btnSetTestAlarm_Click);
            // 
            // listHistoryAlarms
            // 
            this.listHistoryAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listHistoryAlarms.FormattingEnabled = true;
            this.listHistoryAlarms.ItemHeight = 12;
            this.listHistoryAlarms.Location = new System.Drawing.Point(600, 3);
            this.listHistoryAlarms.Name = "listHistoryAlarms";
            this.listHistoryAlarms.Size = new System.Drawing.Size(592, 475);
            this.listHistoryAlarms.TabIndex = 1;
            // 
            // AlarmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1195, 596);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AlarmForm";
            this.Text = "AlarmView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numSelectAlarmId)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListBox listHappeningAlarms;
        private System.Windows.Forms.Timer timerRefreshHappenAlarm;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnResetAllAlarms;
        private System.Windows.Forms.Button btnResetIdAlarm;
        private System.Windows.Forms.NumericUpDown numSelectAlarmId;
        private System.Windows.Forms.Button btnResetSelectAlarm;
        private System.Windows.Forms.Timer timerRefreshHistoryAlarm;
        private System.Windows.Forms.Button btnSetTestAlarm;
        private System.Windows.Forms.ListBox listHistoryAlarms;
    }
}