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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPositionX = new System.Windows.Forms.TextBox();
            this.txtPositionY = new System.Windows.Forms.TextBox();
            this.btnAddAddressPosition = new System.Windows.Forms.Button();
            this.btnRemoveLastAddressPosition = new System.Windows.Forms.Button();
            this.txtAddressPositions = new System.Windows.Forms.TextBox();
            this.btnAddressPositionsClear = new System.Windows.Forms.Button();
            this.txtAddressActions = new System.Windows.Forms.TextBox();
            this.txtSectionSpeedLimits = new System.Windows.Forms.TextBox();
            this.cbAction = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnRemoveLastAddressAction = new System.Windows.Forms.Button();
            this.btnAddAddressAction = new System.Windows.Forms.Button();
            this.btnAddressActionsClear = new System.Windows.Forms.Button();
            this.btnClearSpeedLimits = new System.Windows.Forms.Button();
            this.btnRemoveLastSpeedLimits = new System.Windows.Forms.Button();
            this.btnAddSpeedLimit = new System.Windows.Forms.Button();
            this.txtSpeedLimit = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnClearMoveCmdInfo = new System.Windows.Forms.Button();
            this.btnCheckMoveCmdInfo = new System.Windows.Forms.Button();
            this.btnSendMoveCmdInfo = new System.Windows.Forms.Button();
            this.btnClearIds = new System.Windows.Forms.Button();
            this.btnSetIds = new System.Windows.Forms.Button();
            this.txtCstId = new System.Windows.Forms.TextBox();
            this.txtCmdId = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(24, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "PositionX";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(205, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "PositionY";
            // 
            // txtPositionX
            // 
            this.txtPositionX.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtPositionX.Location = new System.Drawing.Point(99, 22);
            this.txtPositionX.Name = "txtPositionX";
            this.txtPositionX.Size = new System.Drawing.Size(100, 27);
            this.txtPositionX.TabIndex = 2;
            // 
            // txtPositionY
            // 
            this.txtPositionY.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtPositionY.Location = new System.Drawing.Point(280, 22);
            this.txtPositionY.Name = "txtPositionY";
            this.txtPositionY.Size = new System.Drawing.Size(100, 27);
            this.txtPositionY.TabIndex = 3;
            // 
            // btnAddAddressPosition
            // 
            this.btnAddAddressPosition.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddAddressPosition.Location = new System.Drawing.Point(386, 22);
            this.btnAddAddressPosition.Name = "btnAddAddressPosition";
            this.btnAddAddressPosition.Size = new System.Drawing.Size(75, 23);
            this.btnAddAddressPosition.TabIndex = 4;
            this.btnAddAddressPosition.Text = "Add";
            this.btnAddAddressPosition.UseVisualStyleBackColor = true;
            this.btnAddAddressPosition.Click += new System.EventHandler(this.btnAddAddressPosition_Click);
            // 
            // btnRemoveLastAddressPosition
            // 
            this.btnRemoveLastAddressPosition.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRemoveLastAddressPosition.Location = new System.Drawing.Point(467, 22);
            this.btnRemoveLastAddressPosition.Name = "btnRemoveLastAddressPosition";
            this.btnRemoveLastAddressPosition.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveLastAddressPosition.TabIndex = 5;
            this.btnRemoveLastAddressPosition.Text = "Remove";
            this.btnRemoveLastAddressPosition.UseVisualStyleBackColor = true;
            this.btnRemoveLastAddressPosition.Click += new System.EventHandler(this.btnRemoveLastAddressPosition_Click);
            // 
            // txtAddressPositions
            // 
            this.txtAddressPositions.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAddressPositions.Location = new System.Drawing.Point(27, 59);
            this.txtAddressPositions.Multiline = true;
            this.txtAddressPositions.Name = "txtAddressPositions";
            this.txtAddressPositions.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAddressPositions.Size = new System.Drawing.Size(515, 49);
            this.txtAddressPositions.TabIndex = 6;
            // 
            // btnAddressPositionsClear
            // 
            this.btnAddressPositionsClear.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddressPositionsClear.Location = new System.Drawing.Point(548, 22);
            this.btnAddressPositionsClear.Name = "btnAddressPositionsClear";
            this.btnAddressPositionsClear.Size = new System.Drawing.Size(75, 23);
            this.btnAddressPositionsClear.TabIndex = 7;
            this.btnAddressPositionsClear.Text = "Clear";
            this.btnAddressPositionsClear.UseVisualStyleBackColor = true;
            this.btnAddressPositionsClear.Click += new System.EventHandler(this.btnAddressPositionsClear_Click);
            // 
            // txtAddressActions
            // 
            this.txtAddressActions.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAddressActions.Location = new System.Drawing.Point(27, 165);
            this.txtAddressActions.Multiline = true;
            this.txtAddressActions.Name = "txtAddressActions";
            this.txtAddressActions.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAddressActions.Size = new System.Drawing.Size(515, 49);
            this.txtAddressActions.TabIndex = 8;
            // 
            // txtSectionSpeedLimits
            // 
            this.txtSectionSpeedLimits.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtSectionSpeedLimits.Location = new System.Drawing.Point(27, 271);
            this.txtSectionSpeedLimits.Multiline = true;
            this.txtSectionSpeedLimits.Name = "txtSectionSpeedLimits";
            this.txtSectionSpeedLimits.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSectionSpeedLimits.Size = new System.Drawing.Size(515, 49);
            this.txtSectionSpeedLimits.TabIndex = 9;
            // 
            // cbAction
            // 
            this.cbAction.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cbAction.FormattingEnabled = true;
            this.cbAction.Location = new System.Drawing.Point(80, 122);
            this.cbAction.Name = "cbAction";
            this.cbAction.Size = new System.Drawing.Size(300, 24);
            this.cbAction.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(24, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 16);
            this.label3.TabIndex = 11;
            this.label3.Text = "Action";
            // 
            // btnRemoveLastAddressAction
            // 
            this.btnRemoveLastAddressAction.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRemoveLastAddressAction.Location = new System.Drawing.Point(467, 122);
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
            this.btnAddAddressAction.Location = new System.Drawing.Point(386, 122);
            this.btnAddAddressAction.Name = "btnAddAddressAction";
            this.btnAddAddressAction.Size = new System.Drawing.Size(75, 23);
            this.btnAddAddressAction.TabIndex = 12;
            this.btnAddAddressAction.Text = "Add";
            this.btnAddAddressAction.UseVisualStyleBackColor = true;
            this.btnAddAddressAction.Click += new System.EventHandler(this.btnAddAddressAction_Click);
            // 
            // btnAddressActionsClear
            // 
            this.btnAddressActionsClear.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddressActionsClear.Location = new System.Drawing.Point(548, 122);
            this.btnAddressActionsClear.Name = "btnAddressActionsClear";
            this.btnAddressActionsClear.Size = new System.Drawing.Size(75, 23);
            this.btnAddressActionsClear.TabIndex = 14;
            this.btnAddressActionsClear.Text = "Clear";
            this.btnAddressActionsClear.UseVisualStyleBackColor = true;
            this.btnAddressActionsClear.Click += new System.EventHandler(this.btnAddressActionsClear_Click);
            // 
            // btnClearSpeedLimits
            // 
            this.btnClearSpeedLimits.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearSpeedLimits.Location = new System.Drawing.Point(548, 228);
            this.btnClearSpeedLimits.Name = "btnClearSpeedLimits";
            this.btnClearSpeedLimits.Size = new System.Drawing.Size(75, 23);
            this.btnClearSpeedLimits.TabIndex = 19;
            this.btnClearSpeedLimits.Text = "Clear";
            this.btnClearSpeedLimits.UseVisualStyleBackColor = true;
            this.btnClearSpeedLimits.Click += new System.EventHandler(this.btnClearSpeedLimits_Click);
            // 
            // btnRemoveLastSpeedLimits
            // 
            this.btnRemoveLastSpeedLimits.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRemoveLastSpeedLimits.Location = new System.Drawing.Point(467, 228);
            this.btnRemoveLastSpeedLimits.Name = "btnRemoveLastSpeedLimits";
            this.btnRemoveLastSpeedLimits.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveLastSpeedLimits.TabIndex = 18;
            this.btnRemoveLastSpeedLimits.Text = "Remove";
            this.btnRemoveLastSpeedLimits.UseVisualStyleBackColor = true;
            this.btnRemoveLastSpeedLimits.Click += new System.EventHandler(this.btnRemoveLastSpeedLimits_Click);
            // 
            // btnAddSpeedLimit
            // 
            this.btnAddSpeedLimit.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddSpeedLimit.Location = new System.Drawing.Point(386, 228);
            this.btnAddSpeedLimit.Name = "btnAddSpeedLimit";
            this.btnAddSpeedLimit.Size = new System.Drawing.Size(75, 23);
            this.btnAddSpeedLimit.TabIndex = 17;
            this.btnAddSpeedLimit.Text = "Add";
            this.btnAddSpeedLimit.UseVisualStyleBackColor = true;
            this.btnAddSpeedLimit.Click += new System.EventHandler(this.btnAddSpeedLimit_Click);
            // 
            // txtSpeedLimit
            // 
            this.txtSpeedLimit.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtSpeedLimit.Location = new System.Drawing.Point(109, 228);
            this.txtSpeedLimit.Name = "txtSpeedLimit";
            this.txtSpeedLimit.Size = new System.Drawing.Size(100, 27);
            this.txtSpeedLimit.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(24, 231);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 16);
            this.label4.TabIndex = 15;
            this.label4.Text = "SpeedLimit";
            // 
            // btnClearMoveCmdInfo
            // 
            this.btnClearMoveCmdInfo.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearMoveCmdInfo.Location = new System.Drawing.Point(710, 415);
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
            this.btnCheckMoveCmdInfo.Location = new System.Drawing.Point(629, 415);
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
            this.btnSendMoveCmdInfo.Location = new System.Drawing.Point(548, 415);
            this.btnSendMoveCmdInfo.Name = "btnSendMoveCmdInfo";
            this.btnSendMoveCmdInfo.Size = new System.Drawing.Size(75, 23);
            this.btnSendMoveCmdInfo.TabIndex = 20;
            this.btnSendMoveCmdInfo.Text = "Send";
            this.btnSendMoveCmdInfo.UseVisualStyleBackColor = true;
            this.btnSendMoveCmdInfo.Click += new System.EventHandler(this.btnSendMoveCmdInfo_Click);
            // 
            // btnClearIds
            // 
            this.btnClearIds.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearIds.Location = new System.Drawing.Point(548, 334);
            this.btnClearIds.Name = "btnClearIds";
            this.btnClearIds.Size = new System.Drawing.Size(75, 23);
            this.btnClearIds.TabIndex = 29;
            this.btnClearIds.Text = "Clear";
            this.btnClearIds.UseVisualStyleBackColor = true;
            this.btnClearIds.Click += new System.EventHandler(this.btnClearIds_Click);
            // 
            // btnSetIds
            // 
            this.btnSetIds.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSetIds.Location = new System.Drawing.Point(386, 334);
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
            this.txtCstId.Location = new System.Drawing.Point(234, 334);
            this.txtCstId.Name = "txtCstId";
            this.txtCstId.Size = new System.Drawing.Size(100, 27);
            this.txtCstId.TabIndex = 26;
            // 
            // txtCmdId
            // 
            this.txtCmdId.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtCmdId.Location = new System.Drawing.Point(81, 334);
            this.txtCmdId.Name = "txtCmdId";
            this.txtCmdId.Size = new System.Drawing.Size(100, 27);
            this.txtCmdId.TabIndex = 25;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(187, 337);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 16);
            this.label5.TabIndex = 24;
            this.label5.Text = "CstId";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label6.Location = new System.Drawing.Point(24, 337);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 16);
            this.label6.TabIndex = 23;
            this.label6.Text = "CmdId";
            // 
            // ManualMoveCmdForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnClearIds);
            this.Controls.Add(this.btnSetIds);
            this.Controls.Add(this.txtCstId);
            this.Controls.Add(this.txtCmdId);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnClearMoveCmdInfo);
            this.Controls.Add(this.btnCheckMoveCmdInfo);
            this.Controls.Add(this.btnSendMoveCmdInfo);
            this.Controls.Add(this.btnClearSpeedLimits);
            this.Controls.Add(this.btnRemoveLastSpeedLimits);
            this.Controls.Add(this.btnAddSpeedLimit);
            this.Controls.Add(this.txtSpeedLimit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnAddressActionsClear);
            this.Controls.Add(this.btnRemoveLastAddressAction);
            this.Controls.Add(this.btnAddAddressAction);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbAction);
            this.Controls.Add(this.txtSectionSpeedLimits);
            this.Controls.Add(this.txtAddressActions);
            this.Controls.Add(this.btnAddressPositionsClear);
            this.Controls.Add(this.txtAddressPositions);
            this.Controls.Add(this.btnRemoveLastAddressPosition);
            this.Controls.Add(this.btnAddAddressPosition);
            this.Controls.Add(this.txtPositionY);
            this.Controls.Add(this.txtPositionX);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ManualMoveCmdForm";
            this.Text = "ManualMoveCmdForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPositionX;
        private System.Windows.Forms.TextBox txtPositionY;
        private System.Windows.Forms.Button btnAddAddressPosition;
        private System.Windows.Forms.Button btnRemoveLastAddressPosition;
        private System.Windows.Forms.TextBox txtAddressPositions;
        private System.Windows.Forms.Button btnAddressPositionsClear;
        private System.Windows.Forms.TextBox txtAddressActions;
        private System.Windows.Forms.TextBox txtSectionSpeedLimits;
        private System.Windows.Forms.ComboBox cbAction;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRemoveLastAddressAction;
        private System.Windows.Forms.Button btnAddAddressAction;
        private System.Windows.Forms.Button btnAddressActionsClear;
        private System.Windows.Forms.Button btnClearSpeedLimits;
        private System.Windows.Forms.Button btnRemoveLastSpeedLimits;
        private System.Windows.Forms.Button btnAddSpeedLimit;
        private System.Windows.Forms.TextBox txtSpeedLimit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnClearMoveCmdInfo;
        private System.Windows.Forms.Button btnCheckMoveCmdInfo;
        private System.Windows.Forms.Button btnSendMoveCmdInfo;
        private System.Windows.Forms.Button btnClearIds;
        private System.Windows.Forms.Button btnSetIds;
        private System.Windows.Forms.TextBox txtCstId;
        private System.Windows.Forms.TextBox txtCmdId;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}