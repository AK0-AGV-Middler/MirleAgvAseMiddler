
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
            this.Button_AutoCreate = new System.Windows.Forms.Button();
            this.button_AddReadPosition = new System.Windows.Forms.Button();
            this.label_LockResult = new System.Windows.Forms.Label();
            this.ucLabelTB_CreateCommandState = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_CreateCommand_BarcodePosition = new Mirle.Agv.UcLabelTextBox();
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
            this.button_SimulateState = new System.Windows.Forms.Button();
            this.label_FlowStop = new System.Windows.Forms.Label();
            this.label_Psuse = new System.Windows.Forms.Label();
            this.label_WaitReserveIndex = new System.Windows.Forms.Label();
            this.label_BeamState = new System.Windows.Forms.Label();
            this.label_BumpState = new System.Windows.Forms.Label();
            this.label_FlowStop_Label = new System.Windows.Forms.Label();
            this.label_Psuse_Label = new System.Windows.Forms.Label();
            this.label_WaitReserveIndex_Label = new System.Windows.Forms.Label();
            this.label_BeamState_Label = new System.Windows.Forms.Label();
            this.label_BumpState_Label = new System.Windows.Forms.Label();
            this.label_SensorState_Label = new System.Windows.Forms.Label();
            this.label_LoopTime_Label = new System.Windows.Forms.Label();
            this.label_LoopTime = new System.Windows.Forms.Label();
            this.label_SensorState = new System.Windows.Forms.Label();
            this.label_AlarmMessage = new System.Windows.Forms.Label();
            this.label_AlarmMessageName = new System.Windows.Forms.Label();
            this.label_MoveCommandID = new System.Windows.Forms.Label();
            this.label_MoveCommandIDLabel = new System.Windows.Forms.Label();
            this.cB_GetAllReserve = new System.Windows.Forms.CheckBox();
            this.ucLabelTB_EncoderPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_Velocity = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_EncoderOffset = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_ElmoEncoder = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTtB_CommandListState = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_BarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_RealPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_Delta = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_RealEncoder = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTextBox1 = new Mirle.Agv.UcLabelTextBox();
            this.tbP_Debug = new System.Windows.Forms.TabPage();
            this.button_DebugListClear = new System.Windows.Forms.Button();
            this.label_DebugList = new System.Windows.Forms.Label();
            this.DebugList = new System.Windows.Forms.ListBox();
            this.tbP_DebugCSV = new System.Windows.Forms.TabPage();
            this.button_CSVListDisViewRang = new System.Windows.Forms.Button();
            this.button_CSVListShowAll = new System.Windows.Forms.Button();
            this.dataGridView_CSVList = new System.Windows.Forms.DataGridView();
            this.time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.moveState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.realEncoder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nextCommand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.triggerEncoder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.delta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.offset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.realX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.realY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.barcodeX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.barcodeY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.encoderPositionX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.encoderPositionY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LGetDataTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LScanTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LMapX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LMapY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LMapTheta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LBarcodeAngle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LDelta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LTheta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LBarcode1ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000LBarcode2ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RGetDataTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RScanTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RMapX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RMapY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RMapTheta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RBarcodeAngle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RDelta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RTheta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RBarcode1ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sr2000RBarcode2ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button_DebugCSV = new System.Windows.Forms.Button();
            this.button_DebugCSVClear = new System.Windows.Forms.Button();
            this.label_DebugCSVList = new System.Windows.Forms.Label();
            this.tP_Admin = new System.Windows.Forms.TabPage();
            this.button_SimulationModeChange = new System.Windows.Forms.Button();
            this.label_SimulationMode = new System.Windows.Forms.Label();
            this.tbxLogView_MoveControlDebugMessage = new System.Windows.Forms.TextBox();
            this.tbC_Debug.SuspendLayout();
            this.tbP_CreateCommand.SuspendLayout();
            this.tbP_List.SuspendLayout();
            this.tbP_Debug.SuspendLayout();
            this.tbP_DebugCSV.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_CSVList)).BeginInit();
            this.tP_Admin.SuspendLayout();
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
            this.CommandList.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
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
            this.button_SendList.Location = new System.Drawing.Point(1104, 475);
            this.button_SendList.Name = "button_SendList";
            this.button_SendList.Size = new System.Drawing.Size(173, 36);
            this.button_SendList.TabIndex = 44;
            this.button_SendList.Text = "執行移動命令";
            this.button_SendList.UseVisualStyleBackColor = true;
            this.button_SendList.Click += new System.EventHandler(this.button_SendList_Click);
            // 
            // button_StopMove
            // 
            this.button_StopMove.Location = new System.Drawing.Point(1034, 561);
            this.button_StopMove.Name = "button_StopMove";
            this.button_StopMove.Size = new System.Drawing.Size(104, 36);
            this.button_StopMove.TabIndex = 45;
            this.button_StopMove.Text = "Stop";
            this.button_StopMove.UseVisualStyleBackColor = true;
            this.button_StopMove.Click += new System.EventHandler(this.button_StopMove_Click);
            // 
            // button_ClearCommand
            // 
            this.button_ClearCommand.Location = new System.Drawing.Point(1159, 561);
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
            this.tbC_Debug.Controls.Add(this.tP_Admin);
            this.tbC_Debug.Location = new System.Drawing.Point(2, 3);
            this.tbC_Debug.Name = "tbC_Debug";
            this.tbC_Debug.SelectedIndex = 0;
            this.tbC_Debug.Size = new System.Drawing.Size(1291, 627);
            this.tbC_Debug.TabIndex = 50;
            // 
            // tbP_CreateCommand
            // 
            this.tbP_CreateCommand.Controls.Add(this.Button_AutoCreate);
            this.tbP_CreateCommand.Controls.Add(this.button_AddReadPosition);
            this.tbP_CreateCommand.Controls.Add(this.label_LockResult);
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
            // Button_AutoCreate
            // 
            this.Button_AutoCreate.AutoEllipsis = true;
            this.Button_AutoCreate.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Button_AutoCreate.Location = new System.Drawing.Point(777, 547);
            this.Button_AutoCreate.Name = "Button_AutoCreate";
            this.Button_AutoCreate.Size = new System.Drawing.Size(179, 40);
            this.Button_AutoCreate.TabIndex = 82;
            this.Button_AutoCreate.Text = "自動產生動作速度";
            this.Button_AutoCreate.UseVisualStyleBackColor = true;
            this.Button_AutoCreate.Click += new System.EventHandler(this.Button_AutoCreate_Click);
            // 
            // button_AddReadPosition
            // 
            this.button_AddReadPosition.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_AddReadPosition.Location = new System.Drawing.Point(358, 564);
            this.button_AddReadPosition.Name = "button_AddReadPosition";
            this.button_AddReadPosition.Size = new System.Drawing.Size(125, 29);
            this.button_AddReadPosition.TabIndex = 81;
            this.button_AddReadPosition.Text = "Add目前位置";
            this.button_AddReadPosition.UseVisualStyleBackColor = true;
            this.button_AddReadPosition.Click += new System.EventHandler(this.button_AddReadPosition_Click);
            // 
            // label_LockResult
            // 
            this.label_LockResult.AutoSize = true;
            this.label_LockResult.ForeColor = System.Drawing.Color.Red;
            this.label_LockResult.Location = new System.Drawing.Point(907, 522);
            this.label_LockResult.Name = "label_LockResult";
            this.label_LockResult.Size = new System.Drawing.Size(112, 19);
            this.label_LockResult.TabIndex = 80;
            this.label_LockResult.Text = "Lock Result : ";
            // 
            // ucLabelTB_CreateCommandState
            // 
            this.ucLabelTB_CreateCommandState.Location = new System.Drawing.Point(494, 557);
            this.ucLabelTB_CreateCommandState.Margin = new System.Windows.Forms.Padding(8);
            this.ucLabelTB_CreateCommandState.Name = "ucLabelTB_CreateCommandState";
            this.ucLabelTB_CreateCommandState.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_CreateCommandState.TabIndex = 78;
            this.ucLabelTB_CreateCommandState.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_CreateCommandState.TagName = "label1";
            this.ucLabelTB_CreateCommandState.TagValue = "";
            // 
            // ucLabelTB_CreateCommand_BarcodePosition
            // 
            this.ucLabelTB_CreateCommand_BarcodePosition.Location = new System.Drawing.Point(18, 566);
            this.ucLabelTB_CreateCommand_BarcodePosition.Margin = new System.Windows.Forms.Padding(5);
            this.ucLabelTB_CreateCommand_BarcodePosition.Name = "ucLabelTB_CreateCommand_BarcodePosition";
            this.ucLabelTB_CreateCommand_BarcodePosition.Size = new System.Drawing.Size(332, 27);
            this.ucLabelTB_CreateCommand_BarcodePosition.TabIndex = 77;
            this.ucLabelTB_CreateCommand_BarcodePosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_CreateCommand_BarcodePosition.TagName = "label1";
            this.ucLabelTB_CreateCommand_BarcodePosition.TagValue = "";
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
            this.button_DebugModeSend.Location = new System.Drawing.Point(968, 547);
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
            this.listMapSpeedLimits.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listMapSpeedLimits_MouseDoubleClick);
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
            this.listMapAddressActions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listMapAddressActions_MouseDoubleClick);
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
            this.btnPositionXY.Size = new System.Drawing.Size(124, 32);
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
            this.listMapAddressPositions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listMapAddressPositions_MouseDoubleClick);
            // 
            // btnClearMoveCmdInfo
            // 
            this.btnClearMoveCmdInfo.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearMoveCmdInfo.Location = new System.Drawing.Point(1145, 547);
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
            this.tbP_List.Controls.Add(this.button_SimulateState);
            this.tbP_List.Controls.Add(this.label_FlowStop);
            this.tbP_List.Controls.Add(this.label_Psuse);
            this.tbP_List.Controls.Add(this.label_WaitReserveIndex);
            this.tbP_List.Controls.Add(this.label_BeamState);
            this.tbP_List.Controls.Add(this.label_BumpState);
            this.tbP_List.Controls.Add(this.label_FlowStop_Label);
            this.tbP_List.Controls.Add(this.label_Psuse_Label);
            this.tbP_List.Controls.Add(this.label_WaitReserveIndex_Label);
            this.tbP_List.Controls.Add(this.label_BeamState_Label);
            this.tbP_List.Controls.Add(this.label_BumpState_Label);
            this.tbP_List.Controls.Add(this.label_SensorState_Label);
            this.tbP_List.Controls.Add(this.label_LoopTime_Label);
            this.tbP_List.Controls.Add(this.label_LoopTime);
            this.tbP_List.Controls.Add(this.label_SensorState);
            this.tbP_List.Controls.Add(this.label_AlarmMessage);
            this.tbP_List.Controls.Add(this.label_AlarmMessageName);
            this.tbP_List.Controls.Add(this.label_MoveCommandID);
            this.tbP_List.Controls.Add(this.label_MoveCommandIDLabel);
            this.tbP_List.Controls.Add(this.cB_GetAllReserve);
            this.tbP_List.Controls.Add(this.button_ClearCommand);
            this.tbP_List.Controls.Add(this.label_ReserveList);
            this.tbP_List.Controls.Add(this.label_CommandList);
            this.tbP_List.Controls.Add(this.ucLabelTB_EncoderPosition);
            this.tbP_List.Controls.Add(this.ucLabelTB_Velocity);
            this.tbP_List.Controls.Add(this.ucLabelTB_EncoderOffset);
            this.tbP_List.Controls.Add(this.ucLabelTB_ElmoEncoder);
            this.tbP_List.Controls.Add(this.ucLabelTtB_CommandListState);
            this.tbP_List.Controls.Add(this.ucLabelTB_BarcodePosition);
            this.tbP_List.Controls.Add(this.ucLabelTB_RealPosition);
            this.tbP_List.Controls.Add(this.ucLabelTB_Delta);
            this.tbP_List.Controls.Add(this.ucLabelTB_RealEncoder);
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
            // button_SimulateState
            // 
            this.button_SimulateState.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_SimulateState.Location = new System.Drawing.Point(619, 487);
            this.button_SimulateState.Name = "button_SimulateState";
            this.button_SimulateState.Size = new System.Drawing.Size(143, 29);
            this.button_SimulateState.TabIndex = 114;
            this.button_SimulateState.Text = "模擬狀態";
            this.button_SimulateState.UseVisualStyleBackColor = true;
            this.button_SimulateState.Visible = false;
            this.button_SimulateState.Click += new System.EventHandler(this.button_SimulateState_Click);
            // 
            // label_FlowStop
            // 
            this.label_FlowStop.AutoSize = true;
            this.label_FlowStop.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_FlowStop.ForeColor = System.Drawing.Color.Red;
            this.label_FlowStop.Location = new System.Drawing.Point(544, 508);
            this.label_FlowStop.Name = "label_FlowStop";
            this.label_FlowStop.Size = new System.Drawing.Size(45, 19);
            this.label_FlowStop.TabIndex = 113;
            this.label_FlowStop.Text = "State";
            // 
            // label_Psuse
            // 
            this.label_Psuse.AutoSize = true;
            this.label_Psuse.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Psuse.ForeColor = System.Drawing.Color.Red;
            this.label_Psuse.Location = new System.Drawing.Point(445, 508);
            this.label_Psuse.Name = "label_Psuse";
            this.label_Psuse.Size = new System.Drawing.Size(45, 19);
            this.label_Psuse.TabIndex = 112;
            this.label_Psuse.Text = "State";
            // 
            // label_WaitReserveIndex
            // 
            this.label_WaitReserveIndex.AutoSize = true;
            this.label_WaitReserveIndex.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_WaitReserveIndex.ForeColor = System.Drawing.Color.Red;
            this.label_WaitReserveIndex.Location = new System.Drawing.Point(339, 508);
            this.label_WaitReserveIndex.Name = "label_WaitReserveIndex";
            this.label_WaitReserveIndex.Size = new System.Drawing.Size(45, 19);
            this.label_WaitReserveIndex.TabIndex = 111;
            this.label_WaitReserveIndex.Text = "State";
            // 
            // label_BeamState
            // 
            this.label_BeamState.AutoSize = true;
            this.label_BeamState.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_BeamState.ForeColor = System.Drawing.Color.Red;
            this.label_BeamState.Location = new System.Drawing.Point(229, 508);
            this.label_BeamState.Name = "label_BeamState";
            this.label_BeamState.Size = new System.Drawing.Size(45, 19);
            this.label_BeamState.TabIndex = 110;
            this.label_BeamState.Text = "State";
            // 
            // label_BumpState
            // 
            this.label_BumpState.AutoSize = true;
            this.label_BumpState.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_BumpState.ForeColor = System.Drawing.Color.Red;
            this.label_BumpState.Location = new System.Drawing.Point(148, 508);
            this.label_BumpState.Name = "label_BumpState";
            this.label_BumpState.Size = new System.Drawing.Size(45, 19);
            this.label_BumpState.TabIndex = 109;
            this.label_BumpState.Text = "State";
            // 
            // label_FlowStop_Label
            // 
            this.label_FlowStop_Label.AutoSize = true;
            this.label_FlowStop_Label.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_FlowStop_Label.ForeColor = System.Drawing.Color.Black;
            this.label_FlowStop_Label.Location = new System.Drawing.Point(529, 482);
            this.label_FlowStop_Label.Name = "label_FlowStop_Label";
            this.label_FlowStop_Label.Size = new System.Drawing.Size(79, 19);
            this.label_FlowStop_Label.TabIndex = 108;
            this.label_FlowStop_Label.Text = "FlowStop";
            // 
            // label_Psuse_Label
            // 
            this.label_Psuse_Label.AutoSize = true;
            this.label_Psuse_Label.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Psuse_Label.ForeColor = System.Drawing.Color.Black;
            this.label_Psuse_Label.Location = new System.Drawing.Point(445, 482);
            this.label_Psuse_Label.Name = "label_Psuse_Label";
            this.label_Psuse_Label.Size = new System.Drawing.Size(51, 19);
            this.label_Psuse_Label.TabIndex = 107;
            this.label_Psuse_Label.Text = "Pause";
            // 
            // label_WaitReserveIndex_Label
            // 
            this.label_WaitReserveIndex_Label.AutoSize = true;
            this.label_WaitReserveIndex_Label.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_WaitReserveIndex_Label.ForeColor = System.Drawing.Color.Black;
            this.label_WaitReserveIndex_Label.Location = new System.Drawing.Point(312, 482);
            this.label_WaitReserveIndex_Label.Name = "label_WaitReserveIndex_Label";
            this.label_WaitReserveIndex_Label.Size = new System.Drawing.Size(107, 19);
            this.label_WaitReserveIndex_Label.TabIndex = 106;
            this.label_WaitReserveIndex_Label.Text = "Wait Reserve";
            // 
            // label_BeamState_Label
            // 
            this.label_BeamState_Label.AutoSize = true;
            this.label_BeamState_Label.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_BeamState_Label.ForeColor = System.Drawing.Color.Black;
            this.label_BeamState_Label.Location = new System.Drawing.Point(229, 482);
            this.label_BeamState_Label.Name = "label_BeamState_Label";
            this.label_BeamState_Label.Size = new System.Drawing.Size(51, 19);
            this.label_BeamState_Label.TabIndex = 105;
            this.label_BeamState_Label.Text = "Beam";
            // 
            // label_BumpState_Label
            // 
            this.label_BumpState_Label.AutoSize = true;
            this.label_BumpState_Label.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_BumpState_Label.ForeColor = System.Drawing.Color.Black;
            this.label_BumpState_Label.Location = new System.Drawing.Point(148, 482);
            this.label_BumpState_Label.Name = "label_BumpState_Label";
            this.label_BumpState_Label.Size = new System.Drawing.Size(53, 19);
            this.label_BumpState_Label.TabIndex = 104;
            this.label_BumpState_Label.Text = "Bump";
            // 
            // label_SensorState_Label
            // 
            this.label_SensorState_Label.AutoSize = true;
            this.label_SensorState_Label.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_SensorState_Label.ForeColor = System.Drawing.Color.Black;
            this.label_SensorState_Label.Location = new System.Drawing.Point(30, 482);
            this.label_SensorState_Label.Name = "label_SensorState_Label";
            this.label_SensorState_Label.Size = new System.Drawing.Size(94, 19);
            this.label_SensorState_Label.TabIndex = 98;
            this.label_SensorState_Label.Text = "SensorState";
            // 
            // label_LoopTime_Label
            // 
            this.label_LoopTime_Label.AutoSize = true;
            this.label_LoopTime_Label.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_LoopTime_Label.ForeColor = System.Drawing.Color.Black;
            this.label_LoopTime_Label.Location = new System.Drawing.Point(936, 482);
            this.label_LoopTime_Label.Name = "label_LoopTime_Label";
            this.label_LoopTime_Label.Size = new System.Drawing.Size(84, 19);
            this.label_LoopTime_Label.TabIndex = 97;
            this.label_LoopTime_Label.Text = "loopTime:";
            // 
            // label_LoopTime
            // 
            this.label_LoopTime.AutoSize = true;
            this.label_LoopTime.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_LoopTime.ForeColor = System.Drawing.Color.Red;
            this.label_LoopTime.Location = new System.Drawing.Point(1023, 482);
            this.label_LoopTime.Name = "label_LoopTime";
            this.label_LoopTime.Size = new System.Drawing.Size(56, 19);
            this.label_LoopTime.TabIndex = 96;
            this.label_LoopTime.Text = "XXms";
            // 
            // label_SensorState
            // 
            this.label_SensorState.AutoSize = true;
            this.label_SensorState.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_SensorState.ForeColor = System.Drawing.Color.Red;
            this.label_SensorState.Location = new System.Drawing.Point(49, 508);
            this.label_SensorState.Name = "label_SensorState";
            this.label_SensorState.Size = new System.Drawing.Size(45, 19);
            this.label_SensorState.TabIndex = 93;
            this.label_SensorState.Text = "State";
            // 
            // label_AlarmMessage
            // 
            this.label_AlarmMessage.AutoSize = true;
            this.label_AlarmMessage.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_AlarmMessage.ForeColor = System.Drawing.Color.Red;
            this.label_AlarmMessage.Location = new System.Drawing.Point(188, 459);
            this.label_AlarmMessage.Name = "label_AlarmMessage";
            this.label_AlarmMessage.Size = new System.Drawing.Size(0, 19);
            this.label_AlarmMessage.TabIndex = 87;
            // 
            // label_AlarmMessageName
            // 
            this.label_AlarmMessageName.AutoSize = true;
            this.label_AlarmMessageName.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_AlarmMessageName.Location = new System.Drawing.Point(17, 453);
            this.label_AlarmMessageName.Name = "label_AlarmMessageName";
            this.label_AlarmMessageName.Size = new System.Drawing.Size(133, 19);
            this.label_AlarmMessageName.TabIndex = 86;
            this.label_AlarmMessageName.Text = "Alarm Message :";
            // 
            // label_MoveCommandID
            // 
            this.label_MoveCommandID.AutoSize = true;
            this.label_MoveCommandID.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_MoveCommandID.Location = new System.Drawing.Point(847, 3);
            this.label_MoveCommandID.Name = "label_MoveCommandID";
            this.label_MoveCommandID.Size = new System.Drawing.Size(57, 19);
            this.label_MoveCommandID.TabIndex = 85;
            this.label_MoveCommandID.Text = "Empty";
            // 
            // label_MoveCommandIDLabel
            // 
            this.label_MoveCommandIDLabel.AutoSize = true;
            this.label_MoveCommandIDLabel.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_MoveCommandIDLabel.Location = new System.Drawing.Point(676, 4);
            this.label_MoveCommandIDLabel.Name = "label_MoveCommandIDLabel";
            this.label_MoveCommandIDLabel.Size = new System.Drawing.Size(165, 19);
            this.label_MoveCommandIDLabel.TabIndex = 84;
            this.label_MoveCommandIDLabel.Text = "Move Command ID :";
            // 
            // cB_GetAllReserve
            // 
            this.cB_GetAllReserve.AutoSize = true;
            this.cB_GetAllReserve.Checked = true;
            this.cB_GetAllReserve.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cB_GetAllReserve.Location = new System.Drawing.Point(1159, 3);
            this.cB_GetAllReserve.Name = "cB_GetAllReserve";
            this.cB_GetAllReserve.Size = new System.Drawing.Size(123, 23);
            this.cB_GetAllReserve.TabIndex = 80;
            this.cB_GetAllReserve.Text = "取得所有點";
            this.cB_GetAllReserve.UseVisualStyleBackColor = true;
            // 
            // ucLabelTB_EncoderPosition
            // 
            this.ucLabelTB_EncoderPosition.Location = new System.Drawing.Point(512, 568);
            this.ucLabelTB_EncoderPosition.Margin = new System.Windows.Forms.Padding(37, 33, 37, 33);
            this.ucLabelTB_EncoderPosition.Name = "ucLabelTB_EncoderPosition";
            this.ucLabelTB_EncoderPosition.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_EncoderPosition.TabIndex = 88;
            this.ucLabelTB_EncoderPosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_EncoderPosition.TagName = "label1";
            this.ucLabelTB_EncoderPosition.TagValue = "";
            // 
            // ucLabelTB_Velocity
            // 
            this.ucLabelTB_Velocity.Location = new System.Drawing.Point(512, 532);
            this.ucLabelTB_Velocity.Margin = new System.Windows.Forms.Padding(22, 21, 22, 21);
            this.ucLabelTB_Velocity.Name = "ucLabelTB_Velocity";
            this.ucLabelTB_Velocity.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_Velocity.TabIndex = 83;
            this.ucLabelTB_Velocity.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_Velocity.TagName = "label1";
            this.ucLabelTB_Velocity.TagValue = "";
            // 
            // ucLabelTB_EncoderOffset
            // 
            this.ucLabelTB_EncoderOffset.Location = new System.Drawing.Point(1027, 515);
            this.ucLabelTB_EncoderOffset.Margin = new System.Windows.Forms.Padding(13);
            this.ucLabelTB_EncoderOffset.Name = "ucLabelTB_EncoderOffset";
            this.ucLabelTB_EncoderOffset.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_EncoderOffset.TabIndex = 82;
            this.ucLabelTB_EncoderOffset.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_EncoderOffset.TagName = "label1";
            this.ucLabelTB_EncoderOffset.TagValue = "";
            // 
            // ucLabelTB_ElmoEncoder
            // 
            this.ucLabelTB_ElmoEncoder.Location = new System.Drawing.Point(767, 532);
            this.ucLabelTB_ElmoEncoder.Margin = new System.Windows.Forms.Padding(13);
            this.ucLabelTB_ElmoEncoder.Name = "ucLabelTB_ElmoEncoder";
            this.ucLabelTB_ElmoEncoder.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_ElmoEncoder.TabIndex = 81;
            this.ucLabelTB_ElmoEncoder.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_ElmoEncoder.TagName = "label1";
            this.ucLabelTB_ElmoEncoder.TagValue = "";
            // 
            // ucLabelTtB_CommandListState
            // 
            this.ucLabelTtB_CommandListState.Location = new System.Drawing.Point(768, 568);
            this.ucLabelTtB_CommandListState.Margin = new System.Windows.Forms.Padding(13);
            this.ucLabelTtB_CommandListState.Name = "ucLabelTtB_CommandListState";
            this.ucLabelTtB_CommandListState.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTtB_CommandListState.TabIndex = 79;
            this.ucLabelTtB_CommandListState.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTtB_CommandListState.TagName = "label1";
            this.ucLabelTtB_CommandListState.TagValue = "";
            // 
            // ucLabelTB_BarcodePosition
            // 
            this.ucLabelTB_BarcodePosition.Location = new System.Drawing.Point(257, 568);
            this.ucLabelTB_BarcodePosition.Margin = new System.Windows.Forms.Padding(8);
            this.ucLabelTB_BarcodePosition.Name = "ucLabelTB_BarcodePosition";
            this.ucLabelTB_BarcodePosition.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_BarcodePosition.TabIndex = 50;
            this.ucLabelTB_BarcodePosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_BarcodePosition.TagName = "label1";
            this.ucLabelTB_BarcodePosition.TagValue = "";
            // 
            // ucLabelTB_RealPosition
            // 
            this.ucLabelTB_RealPosition.Location = new System.Drawing.Point(7, 568);
            this.ucLabelTB_RealPosition.Margin = new System.Windows.Forms.Padding(8);
            this.ucLabelTB_RealPosition.Name = "ucLabelTB_RealPosition";
            this.ucLabelTB_RealPosition.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_RealPosition.TabIndex = 49;
            this.ucLabelTB_RealPosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_RealPosition.TagName = "label1";
            this.ucLabelTB_RealPosition.TagValue = "";
            // 
            // ucLabelTB_Delta
            // 
            this.ucLabelTB_Delta.Location = new System.Drawing.Point(257, 532);
            this.ucLabelTB_Delta.Margin = new System.Windows.Forms.Padding(8);
            this.ucLabelTB_Delta.Name = "ucLabelTB_Delta";
            this.ucLabelTB_Delta.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_Delta.TabIndex = 48;
            this.ucLabelTB_Delta.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_Delta.TagName = "label1";
            this.ucLabelTB_Delta.TagValue = "";
            // 
            // ucLabelTB_RealEncoder
            // 
            this.ucLabelTB_RealEncoder.Location = new System.Drawing.Point(7, 532);
            this.ucLabelTB_RealEncoder.Margin = new System.Windows.Forms.Padding(5);
            this.ucLabelTB_RealEncoder.Name = "ucLabelTB_RealEncoder";
            this.ucLabelTB_RealEncoder.Size = new System.Drawing.Size(250, 27);
            this.ucLabelTB_RealEncoder.TabIndex = 47;
            this.ucLabelTB_RealEncoder.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_RealEncoder.TagName = "label1";
            this.ucLabelTB_RealEncoder.TagValue = "";
            // 
            // ucLabelTextBox1
            // 
            this.ucLabelTextBox1.Location = new System.Drawing.Point(1802, 238);
            this.ucLabelTextBox1.Margin = new System.Windows.Forms.Padding(5);
            this.ucLabelTextBox1.Name = "ucLabelTextBox1";
            this.ucLabelTextBox1.Size = new System.Drawing.Size(13, 13);
            this.ucLabelTextBox1.TabIndex = 37;
            this.ucLabelTextBox1.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTextBox1.TagName = "label1";
            this.ucLabelTextBox1.TagValue = "";
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
            this.tbP_DebugCSV.Controls.Add(this.button_CSVListDisViewRang);
            this.tbP_DebugCSV.Controls.Add(this.button_CSVListShowAll);
            this.tbP_DebugCSV.Controls.Add(this.dataGridView_CSVList);
            this.tbP_DebugCSV.Controls.Add(this.button_DebugCSV);
            this.tbP_DebugCSV.Controls.Add(this.button_DebugCSVClear);
            this.tbP_DebugCSV.Controls.Add(this.label_DebugCSVList);
            this.tbP_DebugCSV.Location = new System.Drawing.Point(4, 22);
            this.tbP_DebugCSV.Name = "tbP_DebugCSV";
            this.tbP_DebugCSV.Size = new System.Drawing.Size(1283, 601);
            this.tbP_DebugCSV.TabIndex = 3;
            this.tbP_DebugCSV.Text = "DebugCSV";
            this.tbP_DebugCSV.UseVisualStyleBackColor = true;
            // 
            // button_CSVListDisViewRang
            // 
            this.button_CSVListDisViewRang.Location = new System.Drawing.Point(702, 11);
            this.button_CSVListDisViewRang.Name = "button_CSVListDisViewRang";
            this.button_CSVListDisViewRang.Size = new System.Drawing.Size(96, 27);
            this.button_CSVListDisViewRang.TabIndex = 48;
            this.button_CSVListDisViewRang.Text = "隱藏區域開啟";
            this.button_CSVListDisViewRang.UseVisualStyleBackColor = true;
            this.button_CSVListDisViewRang.Click += new System.EventHandler(this.button_CSVListDisViewRang_Click);
            // 
            // button_CSVListShowAll
            // 
            this.button_CSVListShowAll.Location = new System.Drawing.Point(858, 11);
            this.button_CSVListShowAll.Name = "button_CSVListShowAll";
            this.button_CSVListShowAll.Size = new System.Drawing.Size(85, 27);
            this.button_CSVListShowAll.TabIndex = 47;
            this.button_CSVListShowAll.Text = "顯示全部";
            this.button_CSVListShowAll.UseVisualStyleBackColor = true;
            this.button_CSVListShowAll.Click += new System.EventHandler(this.button_CSVListShowAll_Click);
            // 
            // dataGridView_CSVList
            // 
            this.dataGridView_CSVList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_CSVList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.time,
            this.moveState,
            this.realEncoder,
            this.nextCommand,
            this.triggerEncoder,
            this.delta,
            this.offset,
            this.realX,
            this.realY,
            this.barcodeX,
            this.barcodeY,
            this.encoderPositionX,
            this.encoderPositionY,
            this.sr2000LCount,
            this.sr2000LGetDataTime,
            this.sr2000LScanTime,
            this.sr2000LMapX,
            this.sr2000LMapY,
            this.sr2000LMapTheta,
            this.sr2000LBarcodeAngle,
            this.sr2000LDelta,
            this.sr2000LTheta,
            this.sr2000LBarcode1ID,
            this.sr2000LBarcode2ID,
            this.sr2000RCount,
            this.sr2000RGetDataTime,
            this.sr2000RScanTime,
            this.sr2000RMapX,
            this.sr2000RMapY,
            this.sr2000RMapTheta,
            this.sr2000RBarcodeAngle,
            this.sr2000RDelta,
            this.sr2000RTheta,
            this.sr2000RBarcode1ID,
            this.sr2000RBarcode2ID});
            this.dataGridView_CSVList.Cursor = System.Windows.Forms.Cursors.Default;
            this.dataGridView_CSVList.Location = new System.Drawing.Point(6, 44);
            this.dataGridView_CSVList.Name = "dataGridView_CSVList";
            this.dataGridView_CSVList.RowTemplate.Height = 24;
            this.dataGridView_CSVList.Size = new System.Drawing.Size(1271, 551);
            this.dataGridView_CSVList.TabIndex = 46;
            this.dataGridView_CSVList.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_CSVList_ColumnHeaderMouseClick);
            this.dataGridView_CSVList.ColumnHeaderMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_CSVList_ColumnHeaderMouseDoubleClick);
            // 
            // time
            // 
            this.time.Frozen = true;
            this.time.HeaderText = "Time";
            this.time.Name = "time";
            // 
            // moveState
            // 
            this.moveState.HeaderText = "State";
            this.moveState.Name = "moveState";
            this.moveState.Width = 50;
            // 
            // realEncoder
            // 
            this.realEncoder.HeaderText = "RealEncoder";
            this.realEncoder.Name = "realEncoder";
            // 
            // nextCommand
            // 
            this.nextCommand.HeaderText = "Command";
            this.nextCommand.Name = "nextCommand";
            // 
            // triggerEncoder
            // 
            this.triggerEncoder.HeaderText = "Trigger";
            this.triggerEncoder.Name = "triggerEncoder";
            // 
            // delta
            // 
            this.delta.HeaderText = "Delta";
            this.delta.Name = "delta";
            // 
            // offset
            // 
            this.offset.HeaderText = "offset";
            this.offset.Name = "offset";
            // 
            // realX
            // 
            this.realX.HeaderText = "RealX";
            this.realX.Name = "realX";
            // 
            // realY
            // 
            this.realY.HeaderText = "RealY";
            this.realY.Name = "realY";
            // 
            // barcodeX
            // 
            this.barcodeX.HeaderText = "BarcodeX";
            this.barcodeX.Name = "barcodeX";
            // 
            // barcodeY
            // 
            this.barcodeY.HeaderText = "BarcodeY";
            this.barcodeY.Name = "barcodeY";
            // 
            // encoderPositionX
            // 
            this.encoderPositionX.HeaderText = "EncoderX";
            this.encoderPositionX.Name = "encoderPositionX";
            // 
            // encoderPositionY
            // 
            this.encoderPositionY.HeaderText = "EncoderY";
            this.encoderPositionY.Name = "encoderPositionY";
            // 
            // sr2000LCount
            // 
            this.sr2000LCount.HeaderText = "SR2000LCount";
            this.sr2000LCount.Name = "sr2000LCount";
            // 
            // sr2000LGetDataTime
            // 
            this.sr2000LGetDataTime.HeaderText = "SR2000LGetTime";
            this.sr2000LGetDataTime.Name = "sr2000LGetDataTime";
            // 
            // sr2000LScanTime
            // 
            this.sr2000LScanTime.HeaderText = "SR2000LScanTime";
            this.sr2000LScanTime.Name = "sr2000LScanTime";
            // 
            // sr2000LMapX
            // 
            this.sr2000LMapX.HeaderText = "SR2000LMapX";
            this.sr2000LMapX.Name = "sr2000LMapX";
            // 
            // sr2000LMapY
            // 
            this.sr2000LMapY.HeaderText = "SR2000LMapY";
            this.sr2000LMapY.Name = "sr2000LMapY";
            // 
            // sr2000LMapTheta
            // 
            this.sr2000LMapTheta.HeaderText = "SR2000LMapTheta";
            this.sr2000LMapTheta.Name = "sr2000LMapTheta";
            // 
            // sr2000LBarcodeAngle
            // 
            this.sr2000LBarcodeAngle.HeaderText = "SR2000LAngle";
            this.sr2000LBarcodeAngle.Name = "sr2000LBarcodeAngle";
            // 
            // sr2000LDelta
            // 
            this.sr2000LDelta.HeaderText = "SR2000Ldelta";
            this.sr2000LDelta.Name = "sr2000LDelta";
            // 
            // sr2000LTheta
            // 
            this.sr2000LTheta.HeaderText = "SR2000LTheta";
            this.sr2000LTheta.Name = "sr2000LTheta";
            // 
            // sr2000LBarcode1ID
            // 
            this.sr2000LBarcode1ID.HeaderText = "SR2000LID1";
            this.sr2000LBarcode1ID.Name = "sr2000LBarcode1ID";
            // 
            // sr2000LBarcode2ID
            // 
            this.sr2000LBarcode2ID.HeaderText = "SR2000LID2";
            this.sr2000LBarcode2ID.Name = "sr2000LBarcode2ID";
            // 
            // sr2000RCount
            // 
            this.sr2000RCount.HeaderText = "SR2000RCount";
            this.sr2000RCount.Name = "sr2000RCount";
            // 
            // sr2000RGetDataTime
            // 
            this.sr2000RGetDataTime.HeaderText = "SR2000RGetTime";
            this.sr2000RGetDataTime.Name = "sr2000RGetDataTime";
            // 
            // sr2000RScanTime
            // 
            this.sr2000RScanTime.HeaderText = "SR2000RScanTime";
            this.sr2000RScanTime.Name = "sr2000RScanTime";
            // 
            // sr2000RMapX
            // 
            this.sr2000RMapX.HeaderText = "SR2000RMapX";
            this.sr2000RMapX.Name = "sr2000RMapX";
            // 
            // sr2000RMapY
            // 
            this.sr2000RMapY.HeaderText = "SR2000RMapY";
            this.sr2000RMapY.Name = "sr2000RMapY";
            // 
            // sr2000RMapTheta
            // 
            this.sr2000RMapTheta.HeaderText = "SR2000RMapTheta";
            this.sr2000RMapTheta.Name = "sr2000RMapTheta";
            // 
            // sr2000RBarcodeAngle
            // 
            this.sr2000RBarcodeAngle.HeaderText = "SR2000RAngle";
            this.sr2000RBarcodeAngle.Name = "sr2000RBarcodeAngle";
            // 
            // sr2000RDelta
            // 
            this.sr2000RDelta.HeaderText = "SR2000RDelta";
            this.sr2000RDelta.Name = "sr2000RDelta";
            // 
            // sr2000RTheta
            // 
            this.sr2000RTheta.HeaderText = "SR2000RTheta";
            this.sr2000RTheta.Name = "sr2000RTheta";
            // 
            // sr2000RBarcode1ID
            // 
            this.sr2000RBarcode1ID.HeaderText = "SR2000RID1";
            this.sr2000RBarcode1ID.Name = "sr2000RBarcode1ID";
            // 
            // sr2000RBarcode2ID
            // 
            this.sr2000RBarcode2ID.HeaderText = "SR2000RID2";
            this.sr2000RBarcode2ID.Name = "sr2000RBarcode2ID";
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
            // tP_Admin
            // 
            this.tP_Admin.Controls.Add(this.button_SimulationModeChange);
            this.tP_Admin.Controls.Add(this.label_SimulationMode);
            this.tP_Admin.Location = new System.Drawing.Point(4, 22);
            this.tP_Admin.Name = "tP_Admin";
            this.tP_Admin.Size = new System.Drawing.Size(1283, 601);
            this.tP_Admin.TabIndex = 4;
            this.tP_Admin.Text = "Admin";
            this.tP_Admin.UseVisualStyleBackColor = true;
            // 
            // button_SimulationModeChange
            // 
            this.button_SimulationModeChange.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_SimulationModeChange.Location = new System.Drawing.Point(136, 23);
            this.button_SimulationModeChange.Name = "button_SimulationModeChange";
            this.button_SimulationModeChange.Size = new System.Drawing.Size(79, 30);
            this.button_SimulationModeChange.TabIndex = 1;
            this.button_SimulationModeChange.Text = "關閉中";
            this.button_SimulationModeChange.UseVisualStyleBackColor = true;
            this.button_SimulationModeChange.Click += new System.EventHandler(this.button_SimulationMode_Click);
            // 
            // label_SimulationMode
            // 
            this.label_SimulationMode.AutoSize = true;
            this.label_SimulationMode.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_SimulationMode.Location = new System.Drawing.Point(30, 29);
            this.label_SimulationMode.Name = "label_SimulationMode";
            this.label_SimulationMode.Size = new System.Drawing.Size(100, 19);
            this.label_SimulationMode.TabIndex = 0;
            this.label_SimulationMode.Text = "模擬模式 : ";
            // 
            // tbxLogView_MoveControlDebugMessage
            // 
            this.tbxLogView_MoveControlDebugMessage.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxLogView_MoveControlDebugMessage.Location = new System.Drawing.Point(6, 636);
            this.tbxLogView_MoveControlDebugMessage.MaxLength = 65550;
            this.tbxLogView_MoveControlDebugMessage.Multiline = true;
            this.tbxLogView_MoveControlDebugMessage.Name = "tbxLogView_MoveControlDebugMessage";
            this.tbxLogView_MoveControlDebugMessage.ReadOnly = true;
            this.tbxLogView_MoveControlDebugMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxLogView_MoveControlDebugMessage.Size = new System.Drawing.Size(1283, 223);
            this.tbxLogView_MoveControlDebugMessage.TabIndex = 53;
            // 
            // MoveCommandDebugModeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1295, 871);
            this.Controls.Add(this.tbC_Debug);
            this.Controls.Add(this.tbxLogView_MoveControlDebugMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoveCommandDebugModeForm";
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_CSVList)).EndInit();
            this.tP_Admin.ResumeLayout(false);
            this.tP_Admin.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Button button_DebugListClear;
        private System.Windows.Forms.Label label_DebugList;
        private System.Windows.Forms.Button button_DebugCSVClear;
        private System.Windows.Forms.Label label_DebugCSVList;
        private System.Windows.Forms.Button button_DebugCSV;
        private System.Windows.Forms.DataGridView dataGridView_CSVList;
        private System.Windows.Forms.Button button_CSVListShowAll;
        private System.Windows.Forms.Button button_CSVListDisViewRang;
        private System.Windows.Forms.TabPage tP_Admin;
        private System.Windows.Forms.Button button_SimulationModeChange;
        private System.Windows.Forms.Label label_SimulationMode;
        private System.Windows.Forms.CheckBox cB_GetAllReserve;
        private UcLabelTextBox ucLabelTB_Velocity;
        private UcLabelTextBox ucLabelTB_EncoderOffset;
        private UcLabelTextBox ucLabelTB_ElmoEncoder;
        private System.Windows.Forms.Label label_MoveCommandID;
        private System.Windows.Forms.Label label_MoveCommandIDLabel;
        private System.Windows.Forms.Label label_AlarmMessage;
        private System.Windows.Forms.Label label_AlarmMessageName;
        private System.Windows.Forms.DataGridViewTextBoxColumn time;
        private System.Windows.Forms.DataGridViewTextBoxColumn moveState;
        private System.Windows.Forms.DataGridViewTextBoxColumn realEncoder;
        private System.Windows.Forms.DataGridViewTextBoxColumn nextCommand;
        private System.Windows.Forms.DataGridViewTextBoxColumn triggerEncoder;
        private System.Windows.Forms.DataGridViewTextBoxColumn delta;
        private System.Windows.Forms.DataGridViewTextBoxColumn offset;
        private System.Windows.Forms.DataGridViewTextBoxColumn realX;
        private System.Windows.Forms.DataGridViewTextBoxColumn realY;
        private System.Windows.Forms.DataGridViewTextBoxColumn barcodeX;
        private System.Windows.Forms.DataGridViewTextBoxColumn barcodeY;
        private System.Windows.Forms.DataGridViewTextBoxColumn encoderPositionX;
        private System.Windows.Forms.DataGridViewTextBoxColumn encoderPositionY;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LGetDataTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LScanTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LMapX;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LMapY;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LMapTheta;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LBarcodeAngle;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LDelta;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LTheta;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LBarcode1ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000LBarcode2ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RGetDataTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RScanTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RMapX;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RMapY;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RMapTheta;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RBarcodeAngle;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RDelta;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RTheta;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RBarcode1ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn sr2000RBarcode2ID;
        private UcLabelTextBox ucLabelTB_EncoderPosition;
        private System.Windows.Forms.TextBox tbxLogView_MoveControlDebugMessage;
        private System.Windows.Forms.Label label_SensorState;
        private System.Windows.Forms.Label label_LockResult;
        private System.Windows.Forms.Label label_SensorState_Label;
        private System.Windows.Forms.Button button_AddReadPosition;
        private System.Windows.Forms.Label label_LoopTime_Label;
        private System.Windows.Forms.Label label_LoopTime;
        private System.Windows.Forms.Label label_FlowStop;
        private System.Windows.Forms.Label label_Psuse;
        private System.Windows.Forms.Label label_WaitReserveIndex;
        private System.Windows.Forms.Label label_BeamState;
        private System.Windows.Forms.Label label_BumpState;
        private System.Windows.Forms.Label label_FlowStop_Label;
        private System.Windows.Forms.Label label_Psuse_Label;
        private System.Windows.Forms.Label label_WaitReserveIndex_Label;
        private System.Windows.Forms.Label label_BeamState_Label;
        private System.Windows.Forms.Label label_BumpState_Label;
        private System.Windows.Forms.Button button_SimulateState;
        private System.Windows.Forms.Button Button_AutoCreate;
    }
}