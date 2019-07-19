namespace Mirle.Agv.View
{
    partial class ManualMoveCmdForm
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
            this.label3 = new System.Windows.Forms.Label();
            this.btnRemoveLastAddressAction = new System.Windows.Forms.Button();
            this.btnAddAddressAction = new System.Windows.Forms.Button();
            this.btnClearAddressActions = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnClearMoveCmdInfo = new System.Windows.Forms.Button();
            this.btnCheckMoveCmdInfo = new System.Windows.Forms.Button();
            this.btnSendMoveCmdInfo = new System.Windows.Forms.Button();
            this.btnSetIds = new System.Windows.Forms.Button();
            this.txtCstId = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.listMapAddressPositions = new System.Windows.Forms.ListBox();
            this.btnAddressPositionsClear = new System.Windows.Forms.Button();
            this.btnRemoveLastAddressPosition = new System.Windows.Forms.Button();
            this.btnAddAddressPosition = new System.Windows.Forms.Button();
            this.listCmdAddressPositions = new System.Windows.Forms.ListBox();
            this.listCmdAddressActions = new System.Windows.Forms.ListBox();
            this.listMapAddressActions = new System.Windows.Forms.ListBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.listMapSpeedLimits = new System.Windows.Forms.ListBox();
            this.listCmdSpeedLimits = new System.Windows.Forms.ListBox();
            this.btnAddSpeedLimit = new System.Windows.Forms.Button();
            this.btnRemoveSpeedLimit = new System.Windows.Forms.Button();
            this.btnClearSpeedLimit = new System.Windows.Forms.Button();
            this.txtCmdId = new System.Windows.Forms.TextBox();
            this.btnClearIds = new System.Windows.Forms.Button();
            this.btnPositionXY = new System.Windows.Forms.Button();
            this.numPositionX = new System.Windows.Forms.NumericUpDown();
            this.numPositionY = new System.Windows.Forms.NumericUpDown();
            this.btnStopVehicle = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(395, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 16);
            this.label3.TabIndex = 11;
            this.label3.Text = "Actions";
            // 
            // btnRemoveLastAddressAction
            // 
            this.btnRemoveLastAddressAction.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRemoveLastAddressAction.Location = new System.Drawing.Point(608, 22);
            this.btnRemoveLastAddressAction.Name = "btnRemoveLastAddressAction";
            this.btnRemoveLastAddressAction.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveLastAddressAction.TabIndex = 13;
            this.btnRemoveLastAddressAction.Text = "Remove";
            this.btnRemoveLastAddressAction.UseVisualStyleBackColor = true;
            this.btnRemoveLastAddressAction.Click += new System.EventHandler(this.btnRemoveLastAddressAction_Click);
            // 
            // btnAddAddressAction
            // 
            this.btnAddAddressAction.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddAddressAction.Location = new System.Drawing.Point(527, 22);
            this.btnAddAddressAction.Name = "btnAddAddressAction";
            this.btnAddAddressAction.Size = new System.Drawing.Size(75, 23);
            this.btnAddAddressAction.TabIndex = 12;
            this.btnAddAddressAction.Text = "Add";
            this.btnAddAddressAction.UseVisualStyleBackColor = true;
            this.btnAddAddressAction.Click += new System.EventHandler(this.btnAddAddressAction_Click);
            // 
            // btnClearAddressActions
            // 
            this.btnClearAddressActions.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearAddressActions.Location = new System.Drawing.Point(689, 22);
            this.btnClearAddressActions.Name = "btnClearAddressActions";
            this.btnClearAddressActions.Size = new System.Drawing.Size(75, 23);
            this.btnClearAddressActions.TabIndex = 14;
            this.btnClearAddressActions.Text = "Clear";
            this.btnClearAddressActions.UseVisualStyleBackColor = true;
            this.btnClearAddressActions.Click += new System.EventHandler(this.btnClearAddressActions_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(787, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 16);
            this.label4.TabIndex = 15;
            this.label4.Text = "SpeedLimit";
            // 
            // btnClearMoveCmdInfo
            // 
            this.btnClearMoveCmdInfo.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearMoveCmdInfo.Location = new System.Drawing.Point(1116, 756);
            this.btnClearMoveCmdInfo.Name = "btnClearMoveCmdInfo";
            this.btnClearMoveCmdInfo.Size = new System.Drawing.Size(75, 23);
            this.btnClearMoveCmdInfo.TabIndex = 22;
            this.btnClearMoveCmdInfo.Text = "Clear";
            this.btnClearMoveCmdInfo.UseVisualStyleBackColor = true;
            this.btnClearMoveCmdInfo.Click += new System.EventHandler(this.btnClearMoveCmdInfo_Click);
            // 
            // btnCheckMoveCmdInfo
            // 
            this.btnCheckMoveCmdInfo.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnCheckMoveCmdInfo.Location = new System.Drawing.Point(1035, 756);
            this.btnCheckMoveCmdInfo.Name = "btnCheckMoveCmdInfo";
            this.btnCheckMoveCmdInfo.Size = new System.Drawing.Size(75, 23);
            this.btnCheckMoveCmdInfo.TabIndex = 21;
            this.btnCheckMoveCmdInfo.Text = "Check";
            this.btnCheckMoveCmdInfo.UseVisualStyleBackColor = true;
            this.btnCheckMoveCmdInfo.Click += new System.EventHandler(this.btnCheckMoveCmdInfo_Click);
            // 
            // btnSendMoveCmdInfo
            // 
            this.btnSendMoveCmdInfo.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSendMoveCmdInfo.Location = new System.Drawing.Point(954, 756);
            this.btnSendMoveCmdInfo.Name = "btnSendMoveCmdInfo";
            this.btnSendMoveCmdInfo.Size = new System.Drawing.Size(75, 23);
            this.btnSendMoveCmdInfo.TabIndex = 20;
            this.btnSendMoveCmdInfo.Text = "Send";
            this.btnSendMoveCmdInfo.UseVisualStyleBackColor = true;
            this.btnSendMoveCmdInfo.Click += new System.EventHandler(this.btnSendMoveCmdInfo_Click);
            // 
            // btnSetIds
            // 
            this.btnSetIds.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSetIds.Location = new System.Drawing.Point(328, 756);
            this.btnSetIds.Name = "btnSetIds";
            this.btnSetIds.Size = new System.Drawing.Size(75, 23);
            this.btnSetIds.TabIndex = 27;
            this.btnSetIds.Text = "Set";
            this.btnSetIds.UseVisualStyleBackColor = true;
            this.btnSetIds.Click += new System.EventHandler(this.btnSetIds_Click);
            // 
            // txtCstId
            // 
            this.txtCstId.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtCstId.Location = new System.Drawing.Point(222, 756);
            this.txtCstId.Name = "txtCstId";
            this.txtCstId.Size = new System.Drawing.Size(100, 27);
            this.txtCstId.TabIndex = 26;
            this.txtCstId.Text = "Cst001";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(175, 759);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 16);
            this.label5.TabIndex = 24;
            this.label5.Text = "CstId";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label6.Location = new System.Drawing.Point(12, 759);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 16);
            this.label6.TabIndex = 23;
            this.label6.Text = "CmdId";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Positions";
            // 
            // listMapAddressPositions
            // 
            this.listMapAddressPositions.FormattingEnabled = true;
            this.listMapAddressPositions.ItemHeight = 12;
            this.listMapAddressPositions.Location = new System.Drawing.Point(12, 51);
            this.listMapAddressPositions.Name = "listMapAddressPositions";
            this.listMapAddressPositions.ScrollAlwaysVisible = true;
            this.listMapAddressPositions.Size = new System.Drawing.Size(180, 364);
            this.listMapAddressPositions.TabIndex = 31;
            // 
            // btnAddressPositionsClear
            // 
            this.btnAddressPositionsClear.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddressPositionsClear.Location = new System.Drawing.Point(304, 22);
            this.btnAddressPositionsClear.Name = "btnAddressPositionsClear";
            this.btnAddressPositionsClear.Size = new System.Drawing.Size(75, 23);
            this.btnAddressPositionsClear.TabIndex = 34;
            this.btnAddressPositionsClear.Text = "Clear";
            this.btnAddressPositionsClear.UseVisualStyleBackColor = true;
            this.btnAddressPositionsClear.Click += new System.EventHandler(this.btnAddressPositionsClear_Click);
            // 
            // btnRemoveLastAddressPosition
            // 
            this.btnRemoveLastAddressPosition.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRemoveLastAddressPosition.Location = new System.Drawing.Point(223, 22);
            this.btnRemoveLastAddressPosition.Name = "btnRemoveLastAddressPosition";
            this.btnRemoveLastAddressPosition.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveLastAddressPosition.TabIndex = 33;
            this.btnRemoveLastAddressPosition.Text = "Remove";
            this.btnRemoveLastAddressPosition.UseVisualStyleBackColor = true;
            this.btnRemoveLastAddressPosition.Click += new System.EventHandler(this.btnRemoveLastAddressPosition_Click);
            // 
            // btnAddAddressPosition
            // 
            this.btnAddAddressPosition.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddAddressPosition.Location = new System.Drawing.Point(142, 22);
            this.btnAddAddressPosition.Name = "btnAddAddressPosition";
            this.btnAddAddressPosition.Size = new System.Drawing.Size(75, 23);
            this.btnAddAddressPosition.TabIndex = 32;
            this.btnAddAddressPosition.Text = "Add";
            this.btnAddAddressPosition.UseVisualStyleBackColor = true;
            this.btnAddAddressPosition.Click += new System.EventHandler(this.btnAddAddressPosition_Click);
            // 
            // listCmdAddressPositions
            // 
            this.listCmdAddressPositions.FormattingEnabled = true;
            this.listCmdAddressPositions.ItemHeight = 12;
            this.listCmdAddressPositions.Location = new System.Drawing.Point(198, 51);
            this.listCmdAddressPositions.Name = "listCmdAddressPositions";
            this.listCmdAddressPositions.ScrollAlwaysVisible = true;
            this.listCmdAddressPositions.Size = new System.Drawing.Size(180, 364);
            this.listCmdAddressPositions.TabIndex = 35;
            // 
            // listCmdAddressActions
            // 
            this.listCmdAddressActions.FormattingEnabled = true;
            this.listCmdAddressActions.ItemHeight = 12;
            this.listCmdAddressActions.Location = new System.Drawing.Point(584, 51);
            this.listCmdAddressActions.Name = "listCmdAddressActions";
            this.listCmdAddressActions.ScrollAlwaysVisible = true;
            this.listCmdAddressActions.Size = new System.Drawing.Size(180, 364);
            this.listCmdAddressActions.TabIndex = 38;
            // 
            // listMapAddressActions
            // 
            this.listMapAddressActions.FormattingEnabled = true;
            this.listMapAddressActions.ItemHeight = 12;
            this.listMapAddressActions.Location = new System.Drawing.Point(398, 51);
            this.listMapAddressActions.Name = "listMapAddressActions";
            this.listMapAddressActions.ScrollAlwaysVisible = true;
            this.listMapAddressActions.Size = new System.Drawing.Size(180, 364);
            this.listMapAddressActions.TabIndex = 37;
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(69, 1016);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(300, 24);
            this.comboBox1.TabIndex = 36;
            // 
            // listMapSpeedLimits
            // 
            this.listMapSpeedLimits.FormattingEnabled = true;
            this.listMapSpeedLimits.ItemHeight = 12;
            this.listMapSpeedLimits.Location = new System.Drawing.Point(788, 51);
            this.listMapSpeedLimits.Name = "listMapSpeedLimits";
            this.listMapSpeedLimits.ScrollAlwaysVisible = true;
            this.listMapSpeedLimits.Size = new System.Drawing.Size(180, 364);
            this.listMapSpeedLimits.TabIndex = 37;
            // 
            // listCmdSpeedLimits
            // 
            this.listCmdSpeedLimits.FormattingEnabled = true;
            this.listCmdSpeedLimits.ItemHeight = 12;
            this.listCmdSpeedLimits.Location = new System.Drawing.Point(974, 51);
            this.listCmdSpeedLimits.Name = "listCmdSpeedLimits";
            this.listCmdSpeedLimits.ScrollAlwaysVisible = true;
            this.listCmdSpeedLimits.Size = new System.Drawing.Size(180, 364);
            this.listCmdSpeedLimits.TabIndex = 38;
            // 
            // btnAddSpeedLimit
            // 
            this.btnAddSpeedLimit.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddSpeedLimit.Location = new System.Drawing.Point(917, 22);
            this.btnAddSpeedLimit.Name = "btnAddSpeedLimit";
            this.btnAddSpeedLimit.Size = new System.Drawing.Size(75, 23);
            this.btnAddSpeedLimit.TabIndex = 17;
            this.btnAddSpeedLimit.Text = "Add";
            this.btnAddSpeedLimit.UseVisualStyleBackColor = true;
            this.btnAddSpeedLimit.Click += new System.EventHandler(this.btnAddSpeedLimit_Click);
            // 
            // btnRemoveSpeedLimit
            // 
            this.btnRemoveSpeedLimit.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRemoveSpeedLimit.Location = new System.Drawing.Point(998, 22);
            this.btnRemoveSpeedLimit.Name = "btnRemoveSpeedLimit";
            this.btnRemoveSpeedLimit.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveSpeedLimit.TabIndex = 18;
            this.btnRemoveSpeedLimit.Text = "Remove";
            this.btnRemoveSpeedLimit.UseVisualStyleBackColor = true;
            this.btnRemoveSpeedLimit.Click += new System.EventHandler(this.btnRemoveSpeedLimit_Click);
            // 
            // btnClearSpeedLimit
            // 
            this.btnClearSpeedLimit.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearSpeedLimit.Location = new System.Drawing.Point(1079, 22);
            this.btnClearSpeedLimit.Name = "btnClearSpeedLimit";
            this.btnClearSpeedLimit.Size = new System.Drawing.Size(75, 23);
            this.btnClearSpeedLimit.TabIndex = 19;
            this.btnClearSpeedLimit.Text = "Clear";
            this.btnClearSpeedLimit.UseVisualStyleBackColor = true;
            this.btnClearSpeedLimit.Click += new System.EventHandler(this.btnClearSpeedLimit_Click);
            // 
            // txtCmdId
            // 
            this.txtCmdId.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtCmdId.Location = new System.Drawing.Point(69, 756);
            this.txtCmdId.Name = "txtCmdId";
            this.txtCmdId.Size = new System.Drawing.Size(100, 27);
            this.txtCmdId.TabIndex = 25;
            this.txtCmdId.Text = "Cmd001";
            // 
            // btnClearIds
            // 
            this.btnClearIds.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearIds.Location = new System.Drawing.Point(409, 756);
            this.btnClearIds.Name = "btnClearIds";
            this.btnClearIds.Size = new System.Drawing.Size(75, 23);
            this.btnClearIds.TabIndex = 29;
            this.btnClearIds.Text = "Clear";
            this.btnClearIds.UseVisualStyleBackColor = true;
            this.btnClearIds.Click += new System.EventHandler(this.btnClearIds_Click);
            // 
            // btnPositionXY
            // 
            this.btnPositionXY.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPositionXY.Location = new System.Drawing.Point(12, 477);
            this.btnPositionXY.Name = "btnPositionXY";
            this.btnPositionXY.Size = new System.Drawing.Size(180, 23);
            this.btnPositionXY.TabIndex = 32;
            this.btnPositionXY.Text = "Add";
            this.btnPositionXY.UseVisualStyleBackColor = true;
            this.btnPositionXY.Click += new System.EventHandler(this.btnPositionXY_Click);
            // 
            // numPositionX
            // 
            this.numPositionX.Location = new System.Drawing.Point(12, 421);
            this.numPositionX.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numPositionX.Name = "numPositionX";
            this.numPositionX.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.numPositionX.Size = new System.Drawing.Size(180, 22);
            this.numPositionX.TabIndex = 39;
            // 
            // numPositionY
            // 
            this.numPositionY.Location = new System.Drawing.Point(12, 449);
            this.numPositionY.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numPositionY.Name = "numPositionY";
            this.numPositionY.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.numPositionY.Size = new System.Drawing.Size(180, 22);
            this.numPositionY.TabIndex = 40;
            // 
            // btnStopVehicle
            // 
            this.btnStopVehicle.Location = new System.Drawing.Point(954, 717);
            this.btnStopVehicle.Name = "btnStopVehicle";
            this.btnStopVehicle.Size = new System.Drawing.Size(237, 33);
            this.btnStopVehicle.TabIndex = 41;
            this.btnStopVehicle.Text = "Stop";
            this.btnStopVehicle.UseVisualStyleBackColor = true;
            this.btnStopVehicle.Click += new System.EventHandler(this.btnStopVehicle_Click);
            // 
            // ManualMoveCmdForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1214, 797);
            this.Controls.Add(this.btnStopVehicle);
            this.Controls.Add(this.numPositionY);
            this.Controls.Add(this.numPositionX);
            this.Controls.Add(this.listCmdSpeedLimits);
            this.Controls.Add(this.listMapSpeedLimits);
            this.Controls.Add(this.listCmdAddressActions);
            this.Controls.Add(this.listMapAddressActions);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.listCmdAddressPositions);
            this.Controls.Add(this.btnAddressPositionsClear);
            this.Controls.Add(this.btnRemoveLastAddressPosition);
            this.Controls.Add(this.btnPositionXY);
            this.Controls.Add(this.btnAddAddressPosition);
            this.Controls.Add(this.listMapAddressPositions);
            this.Controls.Add(this.btnClearIds);
            this.Controls.Add(this.btnSetIds);
            this.Controls.Add(this.txtCstId);
            this.Controls.Add(this.txtCmdId);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnClearMoveCmdInfo);
            this.Controls.Add(this.btnCheckMoveCmdInfo);
            this.Controls.Add(this.btnSendMoveCmdInfo);
            this.Controls.Add(this.btnClearSpeedLimit);
            this.Controls.Add(this.btnRemoveSpeedLimit);
            this.Controls.Add(this.btnAddSpeedLimit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnClearAddressActions);
            this.Controls.Add(this.btnRemoveLastAddressAction);
            this.Controls.Add(this.btnAddAddressAction);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Name = "ManualMoveCmdForm";
            this.Text = "ManualMoveCmdForm";
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRemoveLastAddressAction;
        private System.Windows.Forms.Button btnAddAddressAction;
        private System.Windows.Forms.Button btnClearAddressActions;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnClearMoveCmdInfo;
        private System.Windows.Forms.Button btnCheckMoveCmdInfo;
        private System.Windows.Forms.Button btnSendMoveCmdInfo;
        private System.Windows.Forms.Button btnSetIds;
        private System.Windows.Forms.TextBox txtCstId;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listMapAddressPositions;
        private System.Windows.Forms.Button btnAddressPositionsClear;
        private System.Windows.Forms.Button btnRemoveLastAddressPosition;
        private System.Windows.Forms.Button btnAddAddressPosition;
        private System.Windows.Forms.ListBox listCmdAddressPositions;
        private System.Windows.Forms.ListBox listCmdAddressActions;
        private System.Windows.Forms.ListBox listMapAddressActions;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ListBox listMapSpeedLimits;
        private System.Windows.Forms.ListBox listCmdSpeedLimits;
        private System.Windows.Forms.Button btnAddSpeedLimit;
        private System.Windows.Forms.Button btnRemoveSpeedLimit;
        private System.Windows.Forms.Button btnClearSpeedLimit;
        private System.Windows.Forms.TextBox txtCmdId;
        private System.Windows.Forms.Button btnClearIds;
        private System.Windows.Forms.Button btnPositionXY;
        private System.Windows.Forms.NumericUpDown numPositionX;
        private System.Windows.Forms.NumericUpDown numPositionY;
        private System.Windows.Forms.Button btnStopVehicle;
    }
}