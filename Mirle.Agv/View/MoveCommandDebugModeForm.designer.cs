
namespace Mirle.Agv.View
{
    partial class MoveCommandDebugModeForm
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
            this.CommandList = new System.Windows.Forms.ListBox();
            this.ReserveList = new System.Windows.Forms.ListBox();
            this.label_CommandList = new System.Windows.Forms.Label();
            this.label_ReserveList = new System.Windows.Forms.Label();
            this.button_SendList = new System.Windows.Forms.Button();
            this.button_StopMove = new System.Windows.Forms.Button();
            this.button_ClearCommand = new System.Windows.Forms.Button();
            this.timer_UpdateData = new System.Windows.Forms.Timer(this.components);
            this.tbC_Debug = new System.Windows.Forms.TabControl();
            this.tbP_CreateCommand = new System.Windows.Forms.TabPage();
            this.tB_PositionY = new System.Windows.Forms.TextBox();
            this.tB_PositionX = new System.Windows.Forms.TextBox();
            this.button_DebugModeSend = new System.Windows.Forms.Button();
            this.listCmdSpeedLimits = new System.Windows.Forms.ListBox();
            this.listMapSpeedLimits = new System.Windows.Forms.ListBox();
            this.listCmdAddressActions = new System.Windows.Forms.ListBox();
            this.listMapAddressActions = new System.Windows.Forms.ListBox();
            this.listCmdAddressPositions = new System.Windows.Forms.ListBox();
            this.btnAddressPositionsClear = new System.Windows.Forms.Button();
            this.btnRemoveLastAddressPosition = new System.Windows.Forms.Button();
            this.btnPositionXY = new System.Windows.Forms.Button();
            this.btnAddAddressPosition = new System.Windows.Forms.Button();
            this.listMapAddressPositions = new System.Windows.Forms.ListBox();
            this.btnClearMoveCmdInfo = new System.Windows.Forms.Button();
            this.btnClearSpeedLimit = new System.Windows.Forms.Button();
            this.btnRemoveSpeedLimit = new System.Windows.Forms.Button();
            this.btnAddSpeedLimit = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnClearAddressActions = new System.Windows.Forms.Button();
            this.btnRemoveLastAddressAction = new System.Windows.Forms.Button();
            this.btnAddAddressAction = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbP_List = new System.Windows.Forms.TabPage();
            this.tbP_Debug = new System.Windows.Forms.TabPage();
            this.button_DebugListClear = new System.Windows.Forms.Button();
            this.label_DebugList = new System.Windows.Forms.Label();
            this.DebugList = new System.Windows.Forms.ListBox();
            this.tbP_DebugCSV = new System.Windows.Forms.TabPage();
            this.button_DebugCSVClear = new System.Windows.Forms.Button();
            this.label_DebugCSVList = new System.Windows.Forms.Label();
            this.DebugCSVList = new System.Windows.Forms.ListBox();
            this.button_DebugCSV = new System.Windows.Forms.Button();
            this.ucLabelTB_CreateCommandState = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_CreateCommand_BarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTtB_CommandListState = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_BarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_RealPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_Delta = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_RealEncoder = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTextBox1 = new Mirle.Agv.UcLabelTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbC_Debug.SuspendLayout();
            this.tbP_CreateCommand.SuspendLayout();
            this.tbP_List.SuspendLayout();
            this.tbP_Debug.SuspendLayout();
            this.tbP_DebugCSV.SuspendLayout();
            this.SuspendLayout();
            // 
            // CommandList
            // 
            this.CommandList.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.CommandList.FormattingEnabled = true;
            this.CommandList.HorizontalScrollbar = true;
            this.CommandList.ItemHeight = 16;
            this.CommandList.Location = new System.Drawing.Point(6, 27);
            this.CommandList.Name = "CommandList";
            this.CommandList.ScrollAlwaysVisible = true;
            this.CommandList.Size = new System.Drawing.Size(1000, 420);
            this.CommandList.TabIndex = 36;
            // 
            // ReserveList
            // 
            this.ReserveList.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ReserveList.FormattingEnabled = true;
            this.ReserveList.HorizontalScrollbar = true;
            this.ReserveList.ItemHeight = 16;
            this.ReserveList.Location = new System.Drawing.Point(1012, 27);
            this.ReserveList.Name = "ReserveList";
            this.ReserveList.ScrollAlwaysVisible = true;
            this.ReserveList.Size = new System.Drawing.Size(265, 420);
            this.ReserveList.TabIndex = 38;
            this.ReserveList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ReserveList_MouseDoubleClick);
            // 
            // label_CommandList
            // 
            this.label_CommandList.AutoSize = true;
            this.label_CommandList.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_CommandList.Location = new System.Drawing.Point(6, 5);
            this.label_CommandList.Name = "label_CommandList";
            this.label_CommandList.Size = new System.Drawing.Size(127, 19);
            this.label_CommandList.TabIndex = 40;
            this.label_CommandList.Text = "Command List :";
            // 
            // label_ReserveList
            // 
            this.label_ReserveList.AutoSize = true;
            this.label_ReserveList.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_ReserveList.Location = new System.Drawing.Point(1015, 5);
            this.label_ReserveList.Name = "label_ReserveList";
            this.label_ReserveList.Size = new System.Drawing.Size(110, 19);
            this.label_ReserveList.TabIndex = 41;
            this.label_ReserveList.Text = "Reserve List :";
            // 
            // button_SendList
            // 
            this.button_SendList.Location = new System.Drawing.Point(625, 518);
            this.button_SendList.Name = "button_SendList";
            this.button_SendList.Size = new System.Drawing.Size(104, 36);
            this.button_SendList.TabIndex = 44;
            this.button_SendList.Text = "執行移動命令";
            this.button_SendList.UseVisualStyleBackColor = true;
            this.button_SendList.Click += new System.EventHandler(this.button_SendList_Click);
            // 
            // button_StopMove
            // 
            this.button_StopMove.Location = new System.Drawing.Point(1034, 518);
            this.button_StopMove.Name = "button_StopMove";
            this.button_StopMove.Size = new System.Drawing.Size(104, 36);
            this.button_StopMove.TabIndex = 45;
            this.button_StopMove.Text = "Stop";
            this.button_StopMove.UseVisualStyleBackColor = true;
            this.button_StopMove.Click += new System.EventHandler(this.button_StopMove_Click);
            // 
            // button_ClearCommand
            // 
            this.button_ClearCommand.Location = new System.Drawing.Point(1154, 518);
            this.button_ClearCommand.Name = "button_ClearCommand";
            this.button_ClearCommand.Size = new System.Drawing.Size(104, 36);
            this.button_ClearCommand.TabIndex = 46;
            this.button_ClearCommand.Text = "清除命令";
            this.button_ClearCommand.UseVisualStyleBackColor = true;
            this.button_ClearCommand.Click += new System.EventHandler(this.button_ClearCommand_Click);
            // 
            // timer_UpdateData
            // 
            this.timer_UpdateData.Enabled = true;
            this.timer_UpdateData.Interval = 200;
            this.timer_UpdateData.Tick += new System.EventHandler(this.timer_UpdateData_Tick);
            // 
            // tbC_Debug
            // 
            this.tbC_Debug.Controls.Add(this.tbP_CreateCommand);
            this.tbC_Debug.Controls.Add(this.tbP_List);
            this.tbC_Debug.Controls.Add(this.tbP_Debug);
            this.tbC_Debug.Controls.Add(this.tbP_DebugCSV);
            this.tbC_Debug.Location = new System.Drawing.Point(2, 3);
            this.tbC_Debug.Name = "tbC_Debug";
            this.tbC_Debug.SelectedIndex = 0;
            this.tbC_Debug.Size = new System.Drawing.Size(1291, 627);
            this.tbC_Debug.TabIndex = 50;
            // 
            // tbP_CreateCommand
            // 
            this.tbP_CreateCommand.Controls.Add(this.ucLabelTB_CreateCommandState);
            this.tbP_CreateCommand.Controls.Add(this.ucLabelTB_CreateCommand_BarcodePosition);
            this.tbP_CreateCommand.Controls.Add(this.tB_PositionY);
            this.tbP_CreateCommand.Controls.Add(this.tB_PositionX);
            this.tbP_CreateCommand.Controls.Add(this.button_DebugModeSend);
            this.tbP_CreateCommand.Controls.Add(this.listCmdSpeedLimits);
            this.tbP_CreateCommand.Controls.Add(this.listMapSpeedLimits);
            this.tbP_CreateCommand.Controls.Add(this.listCmdAddressActions);
            this.tbP_CreateCommand.Controls.Add(this.listMapAddressActions);
            this.tbP_CreateCommand.Controls.Add(this.listCmdAddressPositions);
            this.tbP_CreateCommand.Controls.Add(this.btnAddressPositionsClear);
            this.tbP_CreateCommand.Controls.Add(this.btnRemoveLastAddressPosition);
            this.tbP_CreateCommand.Controls.Add(this.btnPositionXY);
            this.tbP_CreateCommand.Controls.Add(this.btnAddAddressPosition);
            this.tbP_CreateCommand.Controls.Add(this.listMapAddressPositions);
            this.tbP_CreateCommand.Controls.Add(this.btnClearMoveCmdInfo);
            this.tbP_CreateCommand.Controls.Add(this.btnClearSpeedLimit);
            this.tbP_CreateCommand.Controls.Add(this.btnRemoveSpeedLimit);
            this.tbP_CreateCommand.Controls.Add(this.btnAddSpeedLimit);
            this.tbP_CreateCommand.Controls.Add(this.label4);
            this.tbP_CreateCommand.Controls.Add(this.btnClearAddressActions);
            this.tbP_CreateCommand.Controls.Add(this.btnRemoveLastAddressAction);
            this.tbP_CreateCommand.Controls.Add(this.btnAddAddressAction);
            this.tbP_CreateCommand.Controls.Add(this.label3);
            this.tbP_CreateCommand.Controls.Add(this.label1);
            this.tbP_CreateCommand.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbP_CreateCommand.Location = new System.Drawing.Point(4, 22);
            this.tbP_CreateCommand.Name = "tbP_CreateCommand";
            this.tbP_CreateCommand.Padding = new System.Windows.Forms.Padding(3);
            this.tbP_CreateCommand.Size = new System.Drawing.Size(1283, 601);
            this.tbP_CreateCommand.TabIndex = 0;
            this.tbP_CreateCommand.Text = "產生命令";
            this.tbP_CreateCommand.UseVisualStyleBackColor = true;
            // 
            // tB_PositionY
            // 
            this.tB_PositionY.Location = new System.Drawing.Point(140, 526);
            this.tB_PositionY.Name = "tB_PositionY";
            this.tB_PositionY.Size = new System.Drawing.Size(123, 30);
            this.tB_PositionY.TabIndex = 76;
            // 
            // tB_PositionX
            // 
            this.tB_PositionX.Location = new System.Drawing.Point(18, 526);
            this.tB_PositionX.Name = "tB_PositionX";
            this.tB_PositionX.Size = new System.Drawing.Size(123, 30);
            this.tB_PositionX.TabIndex = 75;
            // 
            // button_DebugModeSend
            // 
            this.button_DebugModeSend.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_DebugModeSend.Location = new System.Drawing.Point(968, 539);
            this.button_DebugModeSend.Name = "button_DebugModeSend";
            this.button_DebugModeSend.Size = new System.Drawing.Size(160, 40);
            this.button_DebugModeSend.TabIndex = 74;
            this.button_DebugModeSend.Text = "DebugModeSend";
            this.button_DebugModeSend.UseVisualStyleBackColor = true;
            this.button_DebugModeSend.Click += new System.EventHandler(this.button_DebugModeSend_Click);
            // 
            // listCmdSpeedLimits
            // 
            this.listCmdSpeedLimits.FormattingEnabled = true;
            this.listCmdSpeedLimits.ItemHeight = 19;
            this.listCmdSpeedLimits.Location = new System.Drawing.Point(1085, 74);
            this.listCmdSpeedLimits.Name = "listCmdSpeedLimits";
            this.listCmdSpeedLimits.ScrollAlwaysVisible = true;
            this.listCmdSpeedLimits.Size = new System.Drawing.Size(180, 441);
            this.listCmdSpeedLimits.TabIndex = 70;
            // 
            // listMapSpeedLimits
            // 
            this.listMapSpeedLimits.FormattingEnabled = true;
            this.listMapSpeedLimits.ItemHeight = 19;
            this.listMapSpeedLimits.Location = new System.Drawing.Point(899, 74);
            this.listMapSpeedLimits.Name = "listMapSpeedLimits";
            this.listMapSpeedLimits.ScrollAlwaysVisible = true;
            this.listMapSpeedLimits.Size = new System.Drawing.Size(180, 441);
            this.listMapSpeedLimits.TabIndex = 68;
            // 
            // listCmdAddressActions
            // 
            this.listCmdAddressActions.FormattingEnabled = true;
            this.listCmdAddressActions.ItemHeight = 19;
            this.listCmdAddressActions.Location = new System.Drawing.Point(706, 75);
            this.listCmdAddressActions.Name = "listCmdAddressActions";
            this.listCmdAddressActions.ScrollAlwaysVisible = true;
            this.listCmdAddressActions.Size = new System.Drawing.Size(180, 441);
            this.listCmdAddressActions.TabIndex = 69;
            // 
            // listMapAddressActions
            // 
            this.listMapAddressActions.FormattingEnabled = true;
            this.listMapAddressActions.ItemHeight = 19;
            this.listMapAddressActions.Location = new System.Drawing.Point(520, 75);
            this.listMapAddressActions.Name = "listMapAddressActions";
            this.listMapAddressActions.ScrollAlwaysVisible = true;
            this.listMapAddressActions.Size = new System.Drawing.Size(180, 441);
            this.listMapAddressActions.TabIndex = 67;
            // 
            // listCmdAddressPositions
            // 
            this.listCmdAddressPositions.FormattingEnabled = true;
            this.listCmdAddressPositions.ItemHeight = 19;
            this.listCmdAddressPositions.Location = new System.Drawing.Point(267, 75);
            this.listCmdAddressPositions.Name = "listCmdAddressPositions";
            this.listCmdAddressPositions.ScrollAlwaysVisible = true;
            this.listCmdAddressPositions.Size = new System.Drawing.Size(243, 441);
            this.listCmdAddressPositions.TabIndex = 66;
            // 
            // btnAddressPositionsClear
            // 
            this.btnAddressPositionsClear.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddressPositionsClear.Location = new System.Drawing.Point(420, 20);
            this.btnAddressPositionsClear.Name = "btnAddressPositionsClear";
            this.btnAddressPositionsClear.Size = new System.Drawing.Size(90, 32);
            this.btnAddressPositionsClear.TabIndex = 65;
            this.btnAddressPositionsClear.Text = "Clear";
            this.btnAddressPositionsClear.UseVisualStyleBackColor = true;
            this.btnAddressPositionsClear.Click += new System.EventHandler(this.btnAddressPositionsClear_Click);
            // 
            // btnRemoveLastAddressPosition
            // 
            this.btnRemoveLastAddressPosition.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRemoveLastAddressPosition.Location = new System.Drawing.Point(324, 20);
            this.btnRemoveLastAddressPosition.Name = "btnRemoveLastAddressPosition";
            this.btnRemoveLastAddressPosition.Size = new System.Drawing.Size(90, 32);
            this.btnRemoveLastAddressPosition.TabIndex = 64;
            this.btnRemoveLastAddressPosition.Text = "Remove";
            this.btnRemoveLastAddressPosition.UseVisualStyleBackColor = true;
            this.btnRemoveLastAddressPosition.Click += new System.EventHandler(this.btnRemoveLastAddressPosition_Click);
            // 
            // btnPositionXY
            // 
            this.btnPositionXY.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPositionXY.Location = new System.Drawing.Point(267, 526);
            this.btnPositionXY.Name = "btnPositionXY";
            this.btnPositionXY.Size = new System.Drawing.Size(180, 32);
            this.btnPositionXY.TabIndex = 63;
            this.btnPositionXY.Text = "Add";
            this.btnPositionXY.UseVisualStyleBackColor = true;
            this.btnPositionXY.Click += new System.EventHandler(this.btnPositionXY_Click);
            // 
            // btnAddAddressPosition
            // 
            this.btnAddAddressPosition.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddAddressPosition.Location = new System.Drawing.Point(228, 20);
            this.btnAddAddressPosition.Name = "btnAddAddressPosition";
            this.btnAddAddressPosition.Size = new System.Drawing.Size(90, 32);
            this.btnAddAddressPosition.TabIndex = 62;
            this.btnAddAddressPosition.Text = "Add";
            this.btnAddAddressPosition.UseVisualStyleBackColor = true;
            this.btnAddAddressPosition.Click += new System.EventHandler(this.btnAddAddressPosition_Click);
            // 
            // listMapAddressPositions
            // 
            this.listMapAddressPositions.FormattingEnabled = true;
            this.listMapAddressPositions.ItemHeight = 19;
            this.listMapAddressPositions.Location = new System.Drawing.Point(18, 74);
            this.listMapAddressPositions.Name = "listMapAddressPositions";
            this.listMapAddressPositions.ScrollAlwaysVisible = true;
            this.listMapAddressPositions.Size = new System.Drawing.Size(243, 441);
            this.listMapAddressPositions.TabIndex = 61;
            // 
            // btnClearMoveCmdInfo
            // 
            this.btnClearMoveCmdInfo.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearMoveCmdInfo.Location = new System.Drawing.Point(1145, 539);
            this.btnClearMoveCmdInfo.Name = "btnClearMoveCmdInfo";
            this.btnClearMoveCmdInfo.Size = new System.Drawing.Size(95, 40);
            this.btnClearMoveCmdInfo.TabIndex = 54;
            this.btnClearMoveCmdInfo.Text = "Clear";
            this.btnClearMoveCmdInfo.UseVisualStyleBackColor = true;
            this.btnClearMoveCmdInfo.Click += new System.EventHandler(this.btnClearMoveCmdInfo_Click);
            // 
            // btnClearSpeedLimit
            // 
            this.btnClearSpeedLimit.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearSpeedLimit.Location = new System.Drawing.Point(1175, 20);
            this.btnClearSpeedLimit.Name = "btnClearSpeedLimit";
            this.btnClearSpeedLimit.Size = new System.Drawing.Size(90, 32);
            this.btnClearSpeedLimit.TabIndex = 51;
            this.btnClearSpeedLimit.Text = "Clear";
            this.btnClearSpeedLimit.UseVisualStyleBackColor = true;
            this.btnClearSpeedLimit.Click += new System.EventHandler(this.btnClearSpeedLimit_Click);
            // 
            // btnRemoveSpeedLimit
            // 
            this.btnRemoveSpeedLimit.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRemoveSpeedLimit.Location = new System.Drawing.Point(1082, 20);
            this.btnRemoveSpeedLimit.Name = "btnRemoveSpeedLimit";
            this.btnRemoveSpeedLimit.Size = new System.Drawing.Size(90, 32);
            this.btnRemoveSpeedLimit.TabIndex = 50;
            this.btnRemoveSpeedLimit.Text = "Remove";
            this.btnRemoveSpeedLimit.UseVisualStyleBackColor = true;
            this.btnRemoveSpeedLimit.Click += new System.EventHandler(this.btnRemoveSpeedLimit_Click);
            // 
            // btnAddSpeedLimit
            // 
            this.btnAddSpeedLimit.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddSpeedLimit.Location = new System.Drawing.Point(990, 20);
            this.btnAddSpeedLimit.Name = "btnAddSpeedLimit";
            this.btnAddSpeedLimit.Size = new System.Drawing.Size(90, 32);
            this.btnAddSpeedLimit.TabIndex = 49;
            this.btnAddSpeedLimit.Text = "Add";
            this.btnAddSpeedLimit.UseVisualStyleBackColor = true;
            this.btnAddSpeedLimit.Click += new System.EventHandler(this.btnAddSpeedLimit_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(893, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 19);
            this.label4.TabIndex = 48;
            this.label4.Text = "SpeedLimit";
            // 
            // btnClearAddressActions
            // 
            this.btnClearAddressActions.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearAddressActions.Location = new System.Drawing.Point(796, 20);
            this.btnClearAddressActions.Name = "btnClearAddressActions";
            this.btnClearAddressActions.Size = new System.Drawing.Size(90, 32);
            this.btnClearAddressActions.TabIndex = 47;
            this.btnClearAddressActions.Text = "Clear";
            this.btnClearAddressActions.UseVisualStyleBackColor = true;
            this.btnClearAddressActions.Click += new System.EventHandler(this.btnClearAddressActions_Click);
            // 
            // btnRemoveLastAddressAction
            // 
            this.btnRemoveLastAddressAction.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRemoveLastAddressAction.Location = new System.Drawing.Point(697, 20);
            this.btnRemoveLastAddressAction.Name = "btnRemoveLastAddressAction";
            this.btnRemoveLastAddressAction.Size = new System.Drawing.Size(90, 32);
            this.btnRemoveLastAddressAction.TabIndex = 46;
            this.btnRemoveLastAddressAction.Text = "Remove";
            this.btnRemoveLastAddressAction.UseVisualStyleBackColor = true;
            this.btnRemoveLastAddressAction.Click += new System.EventHandler(this.btnRemoveLastAddressAction_Click);
            // 
            // btnAddAddressAction
            // 
            this.btnAddAddressAction.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddAddressAction.Location = new System.Drawing.Point(598, 20);
            this.btnAddAddressAction.Name = "btnAddAddressAction";
            this.btnAddAddressAction.Size = new System.Drawing.Size(90, 32);
            this.btnAddAddressAction.TabIndex = 45;
            this.btnAddAddressAction.Text = "Add";
            this.btnAddAddressAction.UseVisualStyleBackColor = true;
            this.btnAddAddressAction.Click += new System.EventHandler(this.btnAddAddressAction_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(517, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 19);
            this.label3.TabIndex = 44;
            this.label3.Text = "Actions";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(18, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 19);
            this.label1.TabIndex = 43;
            this.label1.Text = "Positions";
            // 
            // tbP_List
            // 
            this.tbP_List.Controls.Add(this.ucLabelTtB_CommandListState);
            this.tbP_List.Controls.Add(this.ucLabelTB_BarcodePosition);
            this.tbP_List.Controls.Add(this.ucLabelTB_RealPosition);
            this.tbP_List.Controls.Add(this.ucLabelTB_Delta);
            this.tbP_List.Controls.Add(this.ucLabelTB_RealEncoder);
            this.tbP_List.Controls.Add(this.button_ClearCommand);
            this.tbP_List.Controls.Add(this.label_ReserveList);
            this.tbP_List.Controls.Add(this.label_CommandList);
            this.tbP_List.Controls.Add(this.ReserveList);
            this.tbP_List.Controls.Add(this.button_SendList);
            this.tbP_List.Controls.Add(this.ucLabelTextBox1);
            this.tbP_List.Controls.Add(this.CommandList);
            this.tbP_List.Controls.Add(this.button_StopMove);
            this.tbP_List.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbP_List.Location = new System.Drawing.Point(4, 22);
            this.tbP_List.Name = "tbP_List";
            this.tbP_List.Padding = new System.Windows.Forms.Padding(3);
            this.tbP_List.Size = new System.Drawing.Size(1283, 601);
            this.tbP_List.TabIndex = 1;
            this.tbP_List.Text = "CommandList資料";
            this.tbP_List.UseVisualStyleBackColor = true;
            // 
            // tbP_Debug
            // 
            this.tbP_Debug.Controls.Add(this.button_DebugListClear);
            this.tbP_Debug.Controls.Add(this.label_DebugList);
            this.tbP_Debug.Controls.Add(this.DebugList);
            this.tbP_Debug.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbP_Debug.Location = new System.Drawing.Point(4, 22);
            this.tbP_Debug.Name = "tbP_Debug";
            this.tbP_Debug.Size = new System.Drawing.Size(1283, 601);
            this.tbP_Debug.TabIndex = 2;
            this.tbP_Debug.Text = "Debug";
            this.tbP_Debug.UseVisualStyleBackColor = true;
            // 
            // button_DebugListClear
            // 
            this.button_DebugListClear.Location = new System.Drawing.Point(1192, 9);
            this.button_DebugListClear.Name = "button_DebugListClear";
            this.button_DebugListClear.Size = new System.Drawing.Size(85, 27);
            this.button_DebugListClear.TabIndex = 42;
            this.button_DebugListClear.Text = "清除";
            this.button_DebugListClear.UseVisualStyleBackColor = true;
            // 
            // label_DebugList
            // 
            this.label_DebugList.AutoSize = true;
            this.label_DebugList.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_DebugList.Location = new System.Drawing.Point(6, 13);
            this.label_DebugList.Name = "label_DebugList";
            this.label_DebugList.Size = new System.Drawing.Size(100, 19);
            this.label_DebugList.TabIndex = 41;
            this.label_DebugList.Text = "Debug List :";
            // 
            // DebugList
            // 
            this.DebugList.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.DebugList.FormattingEnabled = true;
            this.DebugList.HorizontalScrollbar = true;
            this.DebugList.ItemHeight = 16;
            this.DebugList.Location = new System.Drawing.Point(6, 46);
            this.DebugList.Name = "DebugList";
            this.DebugList.ScrollAlwaysVisible = true;
            this.DebugList.Size = new System.Drawing.Size(1271, 548);
            this.DebugList.TabIndex = 37;
            // 
            // tbP_DebugCSV
            // 
            this.tbP_DebugCSV.Controls.Add(this.label2);
            this.tbP_DebugCSV.Controls.Add(this.button_DebugCSV);
            this.tbP_DebugCSV.Controls.Add(this.button_DebugCSVClear);
            this.tbP_DebugCSV.Controls.Add(this.label_DebugCSVList);
            this.tbP_DebugCSV.Controls.Add(this.DebugCSVList);
            this.tbP_DebugCSV.Location = new System.Drawing.Point(4, 22);
            this.tbP_DebugCSV.Name = "tbP_DebugCSV";
            this.tbP_DebugCSV.Size = new System.Drawing.Size(1283, 601);
            this.tbP_DebugCSV.TabIndex = 3;
            this.tbP_DebugCSV.Text = "DebugCSV";
            this.tbP_DebugCSV.UseVisualStyleBackColor = true;
            // 
            // button_DebugCSVClear
            // 
            this.button_DebugCSVClear.Location = new System.Drawing.Point(1192, 11);
            this.button_DebugCSVClear.Name = "button_DebugCSVClear";
            this.button_DebugCSVClear.Size = new System.Drawing.Size(85, 27);
            this.button_DebugCSVClear.TabIndex = 43;
            this.button_DebugCSVClear.Text = "清除";
            this.button_DebugCSVClear.UseVisualStyleBackColor = true;
            this.button_DebugCSVClear.Click += new System.EventHandler(this.button_DebugCSVClear_Click);
            // 
            // label_DebugCSVList
            // 
            this.label_DebugCSVList.AutoSize = true;
            this.label_DebugCSVList.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_DebugCSVList.Location = new System.Drawing.Point(6, 13);
            this.label_DebugCSVList.Name = "label_DebugCSVList";
            this.label_DebugCSVList.Size = new System.Drawing.Size(135, 19);
            this.label_DebugCSVList.TabIndex = 42;
            this.label_DebugCSVList.Text = "DebugCSV List :";
            // 
            // DebugCSVList
            // 
            this.DebugCSVList.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.DebugCSVList.FormattingEnabled = true;
            this.DebugCSVList.HorizontalScrollbar = true;
            this.DebugCSVList.ItemHeight = 16;
            this.DebugCSVList.Location = new System.Drawing.Point(6, 78);
            this.DebugCSVList.Name = "DebugCSVList";
            this.DebugCSVList.ScrollAlwaysVisible = true;
            this.DebugCSVList.Size = new System.Drawing.Size(1271, 516);
            this.DebugCSVList.TabIndex = 38;
            // 
            // button_DebugCSV
            // 
            this.button_DebugCSV.Location = new System.Drawing.Point(1020, 11);
            this.button_DebugCSV.Name = "button_DebugCSV";
            this.button_DebugCSV.Size = new System.Drawing.Size(85, 27);
            this.button_DebugCSV.TabIndex = 44;
            this.button_DebugCSV.Text = "開啟";
            this.button_DebugCSV.UseVisualStyleBackColor = true;
            this.button_DebugCSV.Click += new System.EventHandler(this.button_DebugCSV_Click);
            // 
            // ucLabelTB_CreateCommandState
            // 
            this.ucLabelTB_CreateCommandState.Location = new System.Drawing.Point(658, 544);
            this.ucLabelTB_CreateCommandState.Margin = new System.Windows.Forms.Padding(8);
            this.ucLabelTB_CreateCommandState.Name = "ucLabelTB_CreateCommandState";
            this.ucLabelTB_CreateCommandState.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_CreateCommandState.TabIndex = 78;
            this.ucLabelTB_CreateCommandState.UcName = "label1";
            this.ucLabelTB_CreateCommandState.UcValue = "";
            // 
            // ucLabelTB_CreateCommand_BarcodePosition
            // 
            this.ucLabelTB_CreateCommand_BarcodePosition.Location = new System.Drawing.Point(18, 566);
            this.ucLabelTB_CreateCommand_BarcodePosition.Margin = new System.Windows.Forms.Padding(5);
            this.ucLabelTB_CreateCommand_BarcodePosition.Name = "ucLabelTB_CreateCommand_BarcodePosition";
            this.ucLabelTB_CreateCommand_BarcodePosition.Size = new System.Drawing.Size(332, 27);
            this.ucLabelTB_CreateCommand_BarcodePosition.TabIndex = 77;
            this.ucLabelTB_CreateCommand_BarcodePosition.UcName = "label1";
            this.ucLabelTB_CreateCommand_BarcodePosition.UcValue = "";
            // 
            // ucLabelTtB_CommandListState
            // 
            this.ucLabelTtB_CommandListState.Location = new System.Drawing.Point(756, 523);
            this.ucLabelTtB_CommandListState.Margin = new System.Windows.Forms.Padding(13);
            this.ucLabelTtB_CommandListState.Name = "ucLabelTtB_CommandListState";
            this.ucLabelTtB_CommandListState.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTtB_CommandListState.TabIndex = 79;
            this.ucLabelTtB_CommandListState.UcName = "label1";
            this.ucLabelTtB_CommandListState.UcValue = "";
            // 
            // ucLabelTB_BarcodePosition
            // 
            this.ucLabelTB_BarcodePosition.Location = new System.Drawing.Point(309, 523);
            this.ucLabelTB_BarcodePosition.Margin = new System.Windows.Forms.Padding(8);
            this.ucLabelTB_BarcodePosition.Name = "ucLabelTB_BarcodePosition";
            this.ucLabelTB_BarcodePosition.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_BarcodePosition.TabIndex = 50;
            this.ucLabelTB_BarcodePosition.UcName = "label1";
            this.ucLabelTB_BarcodePosition.UcValue = "";
            // 
            // ucLabelTB_RealPosition
            // 
            this.ucLabelTB_RealPosition.Location = new System.Drawing.Point(11, 523);
            this.ucLabelTB_RealPosition.Margin = new System.Windows.Forms.Padding(8);
            this.ucLabelTB_RealPosition.Name = "ucLabelTB_RealPosition";
            this.ucLabelTB_RealPosition.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_RealPosition.TabIndex = 49;
            this.ucLabelTB_RealPosition.UcName = "label1";
            this.ucLabelTB_RealPosition.UcValue = "";
            // 
            // ucLabelTB_Delta
            // 
            this.ucLabelTB_Delta.Location = new System.Drawing.Point(309, 471);
            this.ucLabelTB_Delta.Margin = new System.Windows.Forms.Padding(8);
            this.ucLabelTB_Delta.Name = "ucLabelTB_Delta";
            this.ucLabelTB_Delta.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_Delta.TabIndex = 48;
            this.ucLabelTB_Delta.UcName = "label1";
            this.ucLabelTB_Delta.UcValue = "";
            // 
            // ucLabelTB_RealEncoder
            // 
            this.ucLabelTB_RealEncoder.Location = new System.Drawing.Point(10, 471);
            this.ucLabelTB_RealEncoder.Margin = new System.Windows.Forms.Padding(5);
            this.ucLabelTB_RealEncoder.Name = "ucLabelTB_RealEncoder";
            this.ucLabelTB_RealEncoder.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_RealEncoder.TabIndex = 47;
            this.ucLabelTB_RealEncoder.UcName = "label1";
            this.ucLabelTB_RealEncoder.UcValue = "";
            // 
            // ucLabelTextBox1
            // 
            this.ucLabelTextBox1.Location = new System.Drawing.Point(1802, 238);
            this.ucLabelTextBox1.Margin = new System.Windows.Forms.Padding(5);
            this.ucLabelTextBox1.Name = "ucLabelTextBox1";
            this.ucLabelTextBox1.Size = new System.Drawing.Size(13, 13);
            this.ucLabelTextBox1.TabIndex = 37;
            this.ucLabelTextBox1.UcName = "label1";
            this.ucLabelTextBox1.UcValue = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(8, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 19);
            this.label2.TabIndex = 45;
            this.label2.Text = "label2";
            // 
            // MoveCommandDebugMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1295, 632);
            this.Controls.Add(this.tbC_Debug);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoveCommandDebugMode";
            this.Text = "MoveCommandMonitor";
            this.Load += new System.EventHandler(this.MoveCommandMonitor_Load);
            this.Leave += new System.EventHandler(this.MoveCommandDebugMode_Leave);
            this.tbC_Debug.ResumeLayout(false);
            this.tbP_CreateCommand.ResumeLayout(false);
            this.tbP_CreateCommand.PerformLayout();
            this.tbP_List.ResumeLayout(false);
            this.tbP_List.PerformLayout();
            this.tbP_Debug.ResumeLayout(false);
            this.tbP_Debug.PerformLayout();
            this.tbP_DebugCSV.ResumeLayout(false);
            this.tbP_DebugCSV.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox CommandList;
        private UcLabelTextBox ucLabelTextBox1;
        private System.Windows.Forms.ListBox ReserveList;
        private System.Windows.Forms.Label label_CommandList;
        private System.Windows.Forms.Label label_ReserveList;
        private System.Windows.Forms.Button button_SendList;
        private System.Windows.Forms.Button button_StopMove;
        private System.Windows.Forms.Button button_ClearCommand;
        private System.Windows.Forms.Timer timer_UpdateData;
        private System.Windows.Forms.TabControl tbC_Debug;
        private System.Windows.Forms.TabPage tbP_CreateCommand;
        private System.Windows.Forms.TabPage tbP_List;
        private System.Windows.Forms.Button button_DebugModeSend;
        private System.Windows.Forms.ListBox listCmdSpeedLimits;
        private System.Windows.Forms.ListBox listMapSpeedLimits;
        private System.Windows.Forms.ListBox listCmdAddressActions;
        private System.Windows.Forms.ListBox listMapAddressActions;
        private System.Windows.Forms.ListBox listCmdAddressPositions;
        private System.Windows.Forms.Button btnAddressPositionsClear;
        private System.Windows.Forms.Button btnRemoveLastAddressPosition;
        private System.Windows.Forms.Button btnPositionXY;
        private System.Windows.Forms.Button btnAddAddressPosition;
        private System.Windows.Forms.ListBox listMapAddressPositions;
        private System.Windows.Forms.Button btnClearMoveCmdInfo;
        private System.Windows.Forms.Button btnClearSpeedLimit;
        private System.Windows.Forms.Button btnRemoveSpeedLimit;
        private System.Windows.Forms.Button btnAddSpeedLimit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnClearAddressActions;
        private System.Windows.Forms.Button btnRemoveLastAddressAction;
        private System.Windows.Forms.Button btnAddAddressAction;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tbP_Debug;
        private UcLabelTextBox ucLabelTB_BarcodePosition;
        private UcLabelTextBox ucLabelTB_RealPosition;
        private UcLabelTextBox ucLabelTB_Delta;
        private UcLabelTextBox ucLabelTB_RealEncoder;
        private UcLabelTextBox ucLabelTB_CreateCommand_BarcodePosition;
        private System.Windows.Forms.TextBox tB_PositionY;
        private System.Windows.Forms.TextBox tB_PositionX;
        private UcLabelTextBox ucLabelTB_CreateCommandState;
        private UcLabelTextBox ucLabelTtB_CommandListState;
        private System.Windows.Forms.ListBox DebugList;
        private System.Windows.Forms.TabPage tbP_DebugCSV;
        private System.Windows.Forms.ListBox DebugCSVList;
        private System.Windows.Forms.Button button_DebugListClear;
        private System.Windows.Forms.Label label_DebugList;
        private System.Windows.Forms.Button button_DebugCSVClear;
        private System.Windows.Forms.Label label_DebugCSVList;
        private System.Windows.Forms.Button button_DebugCSV;
        private System.Windows.Forms.Label label2;
    }
}