
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
            this.timer_UpdateData = new System.Windows.Forms.Timer(this.components);
            this.tbxLogView_MoveControlDebugMessage = new System.Windows.Forms.TextBox();
            this.tP_Admin = new System.Windows.Forms.TabPage();
            this.button_SimulationModeChange = new System.Windows.Forms.Button();
            this.label_SimulationMode = new System.Windows.Forms.Label();
            this.tbP_LogMessage = new System.Windows.Forms.TabPage();
            this.label_ElmoMessage = new System.Windows.Forms.Label();
            this.tbxLogView_CreateCommandMessage = new System.Windows.Forms.TextBox();
            this.tbxLogView_ElmoMessage = new System.Windows.Forms.TextBox();
            this.label_CreateCommandMessage = new System.Windows.Forms.Label();
            this.tbP_List = new System.Windows.Forms.TabPage();
            this.button_SimulateState = new System.Windows.Forms.Button();
            this.label_Psuse = new System.Windows.Forms.Label();
            this.label_WaitReserveIndex = new System.Windows.Forms.Label();
            this.label_BeamState = new System.Windows.Forms.Label();
            this.label_BumpState = new System.Windows.Forms.Label();
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
            this.button_ClearCommand = new System.Windows.Forms.Button();
            this.label_ReserveList = new System.Windows.Forms.Label();
            this.label_CommandList = new System.Windows.Forms.Label();
            this.ucLabelTB_EncoderPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_Velocity = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_EncoderOffset = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_ElmoEncoder = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTtB_CommandListState = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_BarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_RealPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_Delta = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_RealEncoder = new Mirle.Agv.UcLabelTextBox();
            this.ReserveList = new System.Windows.Forms.ListBox();
            this.button_SendList = new System.Windows.Forms.Button();
            this.ucLabelTextBox1 = new Mirle.Agv.UcLabelTextBox();
            this.CommandList = new System.Windows.Forms.ListBox();
            this.button_StopMove = new System.Windows.Forms.Button();
            this.tbP_CreateCommand = new System.Windows.Forms.TabPage();
            this.button_CreateCommandList_StopAndClear = new System.Windows.Forms.Button();
            this.button_CreateCommandList_Stop = new System.Windows.Forms.Button();
            this.cB_OverrideTest = new System.Windows.Forms.CheckBox();
            this.button_FromTo = new System.Windows.Forms.Button();
            this.label_FormTo = new System.Windows.Forms.Label();
            this.button_ReveseAddressList = new System.Windows.Forms.Button();
            this.button_RunTimesOdd = new System.Windows.Forms.Button();
            this.button_RunTimesEven = new System.Windows.Forms.Button();
            this.tB_RunTimes = new System.Windows.Forms.TextBox();
            this.tB_HorizontalVelocity = new System.Windows.Forms.TextBox();
            this.tB_LineVelocity = new System.Windows.Forms.TextBox();
            this.tB_ChangeVelocity = new System.Windows.Forms.TextBox();
            this.tB_PositionY = new System.Windows.Forms.TextBox();
            this.tB_PositionX = new System.Windows.Forms.TextBox();
            this.label_RunTimes = new System.Windows.Forms.Label();
            this.label_HorizontalVelocity = new System.Windows.Forms.Label();
            this.label_LineVelocity = new System.Windows.Forms.Label();
            this.cB_ChangeAction = new System.Windows.Forms.ComboBox();
            this.label_DebugFormCreateCommandVelocitys = new System.Windows.Forms.Label();
            this.label_DebugFormCreateCommandActions = new System.Windows.Forms.Label();
            this.Button_AutoCreate = new System.Windows.Forms.Button();
            this.button_AddReadPosition = new System.Windows.Forms.Button();
            this.label_LockResult = new System.Windows.Forms.Label();
            this.ucLabelTB_CreateCommandState = new Mirle.Agv.UcLabelTextBox();
            this.ucLabelTB_CreateCommand_BarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.button_DebugModeSend = new System.Windows.Forms.Button();
            this.listCmdSpeedLimits = new System.Windows.Forms.ListBox();
            this.listCmdAddressActions = new System.Windows.Forms.ListBox();
            this.listCmdAddressPositions = new System.Windows.Forms.ListBox();
            this.btnRemoveLastAddressPosition = new System.Windows.Forms.Button();
            this.btnPositionXY = new System.Windows.Forms.Button();
            this.btnAddAddressPosition = new System.Windows.Forms.Button();
            this.listMapAddressPositions = new System.Windows.Forms.ListBox();
            this.btnClearMoveCmdInfo = new System.Windows.Forms.Button();
            this.label_DebugFormCreateCommandPositions = new System.Windows.Forms.Label();
            this.tbC_Debug = new System.Windows.Forms.TabControl();
            this.tP_CheckBarcodePosition = new System.Windows.Forms.TabPage();
            this.button_ComputeDelta = new System.Windows.Forms.Button();
            this.button_Back = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.button_CheckTRStartStop = new System.Windows.Forms.Button();
            this.button_Front = new System.Windows.Forms.Button();
            this.tB_MoveDistance = new System.Windows.Forms.TextBox();
            this.label_MoveDistance = new System.Windows.Forms.Label();
            this.button_GTMove = new System.Windows.Forms.Button();
            this.tB_GTMoveAngle = new System.Windows.Forms.TextBox();
            this.button_GTRight = new System.Windows.Forms.Button();
            this.button_GTLeft = new System.Windows.Forms.Button();
            this.button_GT0 = new System.Windows.Forms.Button();
            this.ucBox_WheelAngle = new Mirle.Agv.UcLabelTextBox();
            this.ucBox_BarcodePosition = new Mirle.Agv.UcLabelTextBox();
            this.ucBox_NowPosition = new Mirle.Agv.UcLabelTextBox();
            this.ucBox_NodePosition = new Mirle.Agv.UcLabelTextBox();
            this.tP_SettingConfigs = new System.Windows.Forms.TabPage();
            this.tC_Configs = new System.Windows.Forms.TabControl();
            this.tB_MoveControlConst = new System.Windows.Forms.TabPage();
            this.tB_MoveControlDictory = new System.Windows.Forms.TabPage();
            this.tB_MoveControlTurn = new System.Windows.Forms.TabPage();
            this.tP_Admin.SuspendLayout();
            this.tbP_LogMessage.SuspendLayout();
            this.tbP_List.SuspendLayout();
            this.tbP_CreateCommand.SuspendLayout();
            this.tbC_Debug.SuspendLayout();
            this.tP_CheckBarcodePosition.SuspendLayout();
            this.tP_SettingConfigs.SuspendLayout();
            this.tC_Configs.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer_UpdateData
            // 
            this.timer_UpdateData.Enabled = true;
            this.timer_UpdateData.Interval = 200;
            this.timer_UpdateData.Tick += new System.EventHandler(this.timer_UpdateData_Tick);
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
            this.tP_Admin.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tP_Admin_MouseDoubleClick);
            // 
            // button_SimulationModeChange
            // 
            this.button_SimulationModeChange.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_SimulationModeChange.Location = new System.Drawing.Point(136, 23);
            this.button_SimulationModeChange.Name = "button_SimulationModeChange";
            this.button_SimulationModeChange.Size = new System.Drawing.Size(80, 30);
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
            // tbP_LogMessage
            // 
            this.tbP_LogMessage.Controls.Add(this.label_ElmoMessage);
            this.tbP_LogMessage.Controls.Add(this.tbxLogView_CreateCommandMessage);
            this.tbP_LogMessage.Controls.Add(this.tbxLogView_ElmoMessage);
            this.tbP_LogMessage.Controls.Add(this.label_CreateCommandMessage);
            this.tbP_LogMessage.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbP_LogMessage.Location = new System.Drawing.Point(4, 22);
            this.tbP_LogMessage.Name = "tbP_LogMessage";
            this.tbP_LogMessage.Size = new System.Drawing.Size(1283, 601);
            this.tbP_LogMessage.TabIndex = 2;
            this.tbP_LogMessage.Text = "LogMessage";
            this.tbP_LogMessage.UseVisualStyleBackColor = true;
            // 
            // label_ElmoMessage
            // 
            this.label_ElmoMessage.AutoSize = true;
            this.label_ElmoMessage.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_ElmoMessage.Location = new System.Drawing.Point(3, 311);
            this.label_ElmoMessage.Name = "label_ElmoMessage";
            this.label_ElmoMessage.Size = new System.Drawing.Size(92, 19);
            this.label_ElmoMessage.TabIndex = 57;
            this.label_ElmoMessage.Text = "Elmo Log :";
            // 
            // tbxLogView_CreateCommandMessage
            // 
            this.tbxLogView_CreateCommandMessage.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxLogView_CreateCommandMessage.Location = new System.Drawing.Point(0, 35);
            this.tbxLogView_CreateCommandMessage.MaxLength = 65550;
            this.tbxLogView_CreateCommandMessage.Multiline = true;
            this.tbxLogView_CreateCommandMessage.Name = "tbxLogView_CreateCommandMessage";
            this.tbxLogView_CreateCommandMessage.ReadOnly = true;
            this.tbxLogView_CreateCommandMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxLogView_CreateCommandMessage.Size = new System.Drawing.Size(1283, 264);
            this.tbxLogView_CreateCommandMessage.TabIndex = 56;
            // 
            // tbxLogView_ElmoMessage
            // 
            this.tbxLogView_ElmoMessage.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxLogView_ElmoMessage.Location = new System.Drawing.Point(0, 334);
            this.tbxLogView_ElmoMessage.MaxLength = 65550;
            this.tbxLogView_ElmoMessage.Multiline = true;
            this.tbxLogView_ElmoMessage.Name = "tbxLogView_ElmoMessage";
            this.tbxLogView_ElmoMessage.ReadOnly = true;
            this.tbxLogView_ElmoMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxLogView_ElmoMessage.Size = new System.Drawing.Size(1283, 264);
            this.tbxLogView_ElmoMessage.TabIndex = 55;
            // 
            // label_CreateCommandMessage
            // 
            this.label_CreateCommandMessage.AutoSize = true;
            this.label_CreateCommandMessage.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_CreateCommandMessage.Location = new System.Drawing.Point(3, 13);
            this.label_CreateCommandMessage.Name = "label_CreateCommandMessage";
            this.label_CreateCommandMessage.Size = new System.Drawing.Size(180, 19);
            this.label_CreateCommandMessage.TabIndex = 41;
            this.label_CreateCommandMessage.Text = "Create Command Log :";
            // 
            // tbP_List
            // 
            this.tbP_List.Controls.Add(this.button_SimulateState);
            this.tbP_List.Controls.Add(this.label_Psuse);
            this.tbP_List.Controls.Add(this.label_WaitReserveIndex);
            this.tbP_List.Controls.Add(this.label_BeamState);
            this.tbP_List.Controls.Add(this.label_BumpState);
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
            // button_ClearCommand
            // 
            this.button_ClearCommand.Location = new System.Drawing.Point(1159, 561);
            this.button_ClearCommand.Name = "button_ClearCommand";
            this.button_ClearCommand.Size = new System.Drawing.Size(104, 36);
            this.button_ClearCommand.TabIndex = 46;
            this.button_ClearCommand.Text = "清除狀態";
            this.button_ClearCommand.UseVisualStyleBackColor = true;
            this.button_ClearCommand.Click += new System.EventHandler(this.button_ClearCommand_Click);
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
            // tbP_CreateCommand
            // 
            this.tbP_CreateCommand.Controls.Add(this.button_CreateCommandList_StopAndClear);
            this.tbP_CreateCommand.Controls.Add(this.button_CreateCommandList_Stop);
            this.tbP_CreateCommand.Controls.Add(this.cB_OverrideTest);
            this.tbP_CreateCommand.Controls.Add(this.button_FromTo);
            this.tbP_CreateCommand.Controls.Add(this.label_FormTo);
            this.tbP_CreateCommand.Controls.Add(this.button_ReveseAddressList);
            this.tbP_CreateCommand.Controls.Add(this.button_RunTimesOdd);
            this.tbP_CreateCommand.Controls.Add(this.button_RunTimesEven);
            this.tbP_CreateCommand.Controls.Add(this.tB_RunTimes);
            this.tbP_CreateCommand.Controls.Add(this.tB_HorizontalVelocity);
            this.tbP_CreateCommand.Controls.Add(this.tB_LineVelocity);
            this.tbP_CreateCommand.Controls.Add(this.tB_ChangeVelocity);
            this.tbP_CreateCommand.Controls.Add(this.tB_PositionY);
            this.tbP_CreateCommand.Controls.Add(this.tB_PositionX);
            this.tbP_CreateCommand.Controls.Add(this.label_RunTimes);
            this.tbP_CreateCommand.Controls.Add(this.label_HorizontalVelocity);
            this.tbP_CreateCommand.Controls.Add(this.label_LineVelocity);
            this.tbP_CreateCommand.Controls.Add(this.cB_ChangeAction);
            this.tbP_CreateCommand.Controls.Add(this.label_DebugFormCreateCommandVelocitys);
            this.tbP_CreateCommand.Controls.Add(this.label_DebugFormCreateCommandActions);
            this.tbP_CreateCommand.Controls.Add(this.Button_AutoCreate);
            this.tbP_CreateCommand.Controls.Add(this.button_AddReadPosition);
            this.tbP_CreateCommand.Controls.Add(this.label_LockResult);
            this.tbP_CreateCommand.Controls.Add(this.ucLabelTB_CreateCommandState);
            this.tbP_CreateCommand.Controls.Add(this.ucLabelTB_CreateCommand_BarcodePosition);
            this.tbP_CreateCommand.Controls.Add(this.button_DebugModeSend);
            this.tbP_CreateCommand.Controls.Add(this.listCmdSpeedLimits);
            this.tbP_CreateCommand.Controls.Add(this.listCmdAddressActions);
            this.tbP_CreateCommand.Controls.Add(this.listCmdAddressPositions);
            this.tbP_CreateCommand.Controls.Add(this.btnRemoveLastAddressPosition);
            this.tbP_CreateCommand.Controls.Add(this.btnPositionXY);
            this.tbP_CreateCommand.Controls.Add(this.btnAddAddressPosition);
            this.tbP_CreateCommand.Controls.Add(this.listMapAddressPositions);
            this.tbP_CreateCommand.Controls.Add(this.btnClearMoveCmdInfo);
            this.tbP_CreateCommand.Controls.Add(this.label_DebugFormCreateCommandPositions);
            this.tbP_CreateCommand.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbP_CreateCommand.Location = new System.Drawing.Point(4, 22);
            this.tbP_CreateCommand.Name = "tbP_CreateCommand";
            this.tbP_CreateCommand.Padding = new System.Windows.Forms.Padding(3);
            this.tbP_CreateCommand.Size = new System.Drawing.Size(1283, 601);
            this.tbP_CreateCommand.TabIndex = 0;
            this.tbP_CreateCommand.Text = "產生命令";
            this.tbP_CreateCommand.UseVisualStyleBackColor = true;
            // 
            // button_CreateCommandList_StopAndClear
            // 
            this.button_CreateCommandList_StopAndClear.Location = new System.Drawing.Point(1060, 468);
            this.button_CreateCommandList_StopAndClear.Name = "button_CreateCommandList_StopAndClear";
            this.button_CreateCommandList_StopAndClear.Size = new System.Drawing.Size(104, 36);
            this.button_CreateCommandList_StopAndClear.TabIndex = 100;
            this.button_CreateCommandList_StopAndClear.Text = "清除狀態";
            this.button_CreateCommandList_StopAndClear.UseVisualStyleBackColor = true;
            this.button_CreateCommandList_StopAndClear.Click += new System.EventHandler(this.button_ClearCommand_Click);
            // 
            // button_CreateCommandList_Stop
            // 
            this.button_CreateCommandList_Stop.Location = new System.Drawing.Point(935, 468);
            this.button_CreateCommandList_Stop.Name = "button_CreateCommandList_Stop";
            this.button_CreateCommandList_Stop.Size = new System.Drawing.Size(104, 36);
            this.button_CreateCommandList_Stop.TabIndex = 99;
            this.button_CreateCommandList_Stop.Text = "Stop";
            this.button_CreateCommandList_Stop.UseVisualStyleBackColor = true;
            this.button_CreateCommandList_Stop.Click += new System.EventHandler(this.button_StopMove_Click);
            // 
            // cB_OverrideTest
            // 
            this.cB_OverrideTest.AutoSize = true;
            this.cB_OverrideTest.Location = new System.Drawing.Point(1185, 481);
            this.cB_OverrideTest.Name = "cB_OverrideTest";
            this.cB_OverrideTest.Size = new System.Drawing.Size(92, 23);
            this.cB_OverrideTest.TabIndex = 98;
            this.cB_OverrideTest.Text = "Override";
            this.cB_OverrideTest.UseVisualStyleBackColor = true;
            this.cB_OverrideTest.Visible = false;
            // 
            // button_FromTo
            // 
            this.button_FromTo.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_FromTo.Location = new System.Drawing.Point(1166, 316);
            this.button_FromTo.Name = "button_FromTo";
            this.button_FromTo.Size = new System.Drawing.Size(106, 30);
            this.button_FromTo.TabIndex = 97;
            this.button_FromTo.Text = "路徑計算";
            this.button_FromTo.UseVisualStyleBackColor = true;
            this.button_FromTo.Click += new System.EventHandler(this.button_FromTo_Click);
            // 
            // label_FormTo
            // 
            this.label_FormTo.AutoSize = true;
            this.label_FormTo.Location = new System.Drawing.Point(952, 321);
            this.label_FormTo.Name = "label_FormTo";
            this.label_FormTo.Size = new System.Drawing.Size(208, 19);
            this.label_FormTo.TabIndex = 96;
            this.label_FormTo.Text = "From to : (只需要點終點)";
            // 
            // button_ReveseAddressList
            // 
            this.button_ReveseAddressList.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_ReveseAddressList.Location = new System.Drawing.Point(1009, 248);
            this.button_ReveseAddressList.Name = "button_ReveseAddressList";
            this.button_ReveseAddressList.Size = new System.Drawing.Size(156, 30);
            this.button_ReveseAddressList.TabIndex = 95;
            this.button_ReveseAddressList.Text = " 路徑全部顛倒";
            this.button_ReveseAddressList.UseVisualStyleBackColor = true;
            this.button_ReveseAddressList.Click += new System.EventHandler(this.button_ReveseAddressList_Click);
            // 
            // button_RunTimesOdd
            // 
            this.button_RunTimesOdd.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_RunTimesOdd.Location = new System.Drawing.Point(1194, 179);
            this.button_RunTimesOdd.Name = "button_RunTimesOdd";
            this.button_RunTimesOdd.Size = new System.Drawing.Size(68, 30);
            this.button_RunTimesOdd.TabIndex = 94;
            this.button_RunTimesOdd.Text = "次";
            this.button_RunTimesOdd.UseVisualStyleBackColor = true;
            this.button_RunTimesOdd.Click += new System.EventHandler(this.button_RunTimesOdd_Click);
            // 
            // button_RunTimesEven
            // 
            this.button_RunTimesEven.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_RunTimesEven.Location = new System.Drawing.Point(1112, 179);
            this.button_RunTimesEven.Name = "button_RunTimesEven";
            this.button_RunTimesEven.Size = new System.Drawing.Size(68, 30);
            this.button_RunTimesEven.TabIndex = 93;
            this.button_RunTimesEven.Text = "趟";
            this.button_RunTimesEven.UseVisualStyleBackColor = true;
            this.button_RunTimesEven.Click += new System.EventHandler(this.button_RunTimesEven_Click);
            // 
            // tB_RunTimes
            // 
            this.tB_RunTimes.Location = new System.Drawing.Point(1030, 179);
            this.tB_RunTimes.Name = "tB_RunTimes";
            this.tB_RunTimes.Size = new System.Drawing.Size(72, 30);
            this.tB_RunTimes.TabIndex = 92;
            this.tB_RunTimes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tB_RunTimes.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tB_RunTimes_KeyPress);
            // 
            // tB_HorizontalVelocity
            // 
            this.tB_HorizontalVelocity.Location = new System.Drawing.Point(1101, 106);
            this.tB_HorizontalVelocity.Name = "tB_HorizontalVelocity";
            this.tB_HorizontalVelocity.Size = new System.Drawing.Size(117, 30);
            this.tB_HorizontalVelocity.TabIndex = 90;
            this.tB_HorizontalVelocity.Text = "400";
            this.tB_HorizontalVelocity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tB_HorizontalVelocity.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tB_HorizontalVelocity_KeyPress);
            // 
            // tB_LineVelocity
            // 
            this.tB_LineVelocity.Location = new System.Drawing.Point(1101, 66);
            this.tB_LineVelocity.Name = "tB_LineVelocity";
            this.tB_LineVelocity.Size = new System.Drawing.Size(117, 30);
            this.tB_LineVelocity.TabIndex = 89;
            this.tB_LineVelocity.Text = "400";
            this.tB_LineVelocity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tB_LineVelocity.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tB_LineVelocity_KeyPress);
            // 
            // tB_ChangeVelocity
            // 
            this.tB_ChangeVelocity.Location = new System.Drawing.Point(743, 63);
            this.tB_ChangeVelocity.Name = "tB_ChangeVelocity";
            this.tB_ChangeVelocity.Size = new System.Drawing.Size(117, 30);
            this.tB_ChangeVelocity.TabIndex = 86;
            this.tB_ChangeVelocity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tB_ChangeVelocity.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tB_ChangeVelocity_KeyPress);
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
            // label_RunTimes
            // 
            this.label_RunTimes.AutoSize = true;
            this.label_RunTimes.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_RunTimes.Location = new System.Drawing.Point(952, 186);
            this.label_RunTimes.Name = "label_RunTimes";
            this.label_RunTimes.Size = new System.Drawing.Size(71, 19);
            this.label_RunTimes.TabIndex = 91;
            this.label_RunTimes.Text = "來回跑:";
            // 
            // label_HorizontalVelocity
            // 
            this.label_HorizontalVelocity.AutoSize = true;
            this.label_HorizontalVelocity.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_HorizontalVelocity.Location = new System.Drawing.Point(952, 110);
            this.label_HorizontalVelocity.Name = "label_HorizontalVelocity";
            this.label_HorizontalVelocity.Size = new System.Drawing.Size(133, 19);
            this.label_HorizontalVelocity.TabIndex = 88;
            this.label_HorizontalVelocity.Text = "橫移速度設定 :";
            // 
            // label_LineVelocity
            // 
            this.label_LineVelocity.AutoSize = true;
            this.label_LineVelocity.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_LineVelocity.Location = new System.Drawing.Point(952, 71);
            this.label_LineVelocity.Name = "label_LineVelocity";
            this.label_LineVelocity.Size = new System.Drawing.Size(133, 19);
            this.label_LineVelocity.TabIndex = 87;
            this.label_LineVelocity.Text = "直線速度設定 :";
            // 
            // cB_ChangeAction
            // 
            this.cB_ChangeAction.FormattingEnabled = true;
            this.cB_ChangeAction.Location = new System.Drawing.Point(536, 63);
            this.cB_ChangeAction.Name = "cB_ChangeAction";
            this.cB_ChangeAction.Size = new System.Drawing.Size(117, 27);
            this.cB_ChangeAction.TabIndex = 85;
            this.cB_ChangeAction.SelectedIndexChanged += new System.EventHandler(this.cB_ChangeAction_SelectedIndexChanged);
            // 
            // label_DebugFormCreateCommandVelocitys
            // 
            this.label_DebugFormCreateCommandVelocitys.AutoSize = true;
            this.label_DebugFormCreateCommandVelocitys.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_DebugFormCreateCommandVelocitys.Location = new System.Drawing.Point(739, 25);
            this.label_DebugFormCreateCommandVelocitys.Name = "label_DebugFormCreateCommandVelocitys";
            this.label_DebugFormCreateCommandVelocitys.Size = new System.Drawing.Size(78, 19);
            this.label_DebugFormCreateCommandVelocitys.TabIndex = 84;
            this.label_DebugFormCreateCommandVelocitys.Text = "Velocitys";
            // 
            // label_DebugFormCreateCommandActions
            // 
            this.label_DebugFormCreateCommandActions.AutoSize = true;
            this.label_DebugFormCreateCommandActions.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_DebugFormCreateCommandActions.Location = new System.Drawing.Point(543, 25);
            this.label_DebugFormCreateCommandActions.Name = "label_DebugFormCreateCommandActions";
            this.label_DebugFormCreateCommandActions.Size = new System.Drawing.Size(65, 19);
            this.label_DebugFormCreateCommandActions.TabIndex = 83;
            this.label_DebugFormCreateCommandActions.Text = "Actions";
            // 
            // Button_AutoCreate
            // 
            this.Button_AutoCreate.AutoEllipsis = true;
            this.Button_AutoCreate.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Button_AutoCreate.Location = new System.Drawing.Point(777, 547);
            this.Button_AutoCreate.Name = "Button_AutoCreate";
            this.Button_AutoCreate.Size = new System.Drawing.Size(179, 40);
            this.Button_AutoCreate.TabIndex = 82;
            this.Button_AutoCreate.Text = "產生動作及速度";
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
            this.ucLabelTB_CreateCommandState.Location = new System.Drawing.Point(493, 557);
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
            this.ucLabelTB_CreateCommand_BarcodePosition.Location = new System.Drawing.Point(18, 565);
            this.ucLabelTB_CreateCommand_BarcodePosition.Margin = new System.Windows.Forms.Padding(5);
            this.ucLabelTB_CreateCommand_BarcodePosition.Name = "ucLabelTB_CreateCommand_BarcodePosition";
            this.ucLabelTB_CreateCommand_BarcodePosition.Size = new System.Drawing.Size(332, 27);
            this.ucLabelTB_CreateCommand_BarcodePosition.TabIndex = 77;
            this.ucLabelTB_CreateCommand_BarcodePosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLabelTB_CreateCommand_BarcodePosition.TagName = "label1";
            this.ucLabelTB_CreateCommand_BarcodePosition.TagValue = "";
            // 
            // button_DebugModeSend
            // 
            this.button_DebugModeSend.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_DebugModeSend.Location = new System.Drawing.Point(979, 547);
            this.button_DebugModeSend.Name = "button_DebugModeSend";
            this.button_DebugModeSend.Size = new System.Drawing.Size(160, 40);
            this.button_DebugModeSend.TabIndex = 84;
            this.button_DebugModeSend.Text = "產生移動命令";
            this.button_DebugModeSend.UseVisualStyleBackColor = true;
            this.button_DebugModeSend.Click += new System.EventHandler(this.button_DebugModeSend_Click);
            // 
            // listCmdSpeedLimits
            // 
            this.listCmdSpeedLimits.FormattingEnabled = true;
            this.listCmdSpeedLimits.ItemHeight = 19;
            this.listCmdSpeedLimits.Location = new System.Drawing.Point(743, 63);
            this.listCmdSpeedLimits.Name = "listCmdSpeedLimits";
            this.listCmdSpeedLimits.ScrollAlwaysVisible = true;
            this.listCmdSpeedLimits.Size = new System.Drawing.Size(180, 441);
            this.listCmdSpeedLimits.TabIndex = 70;
            this.listCmdSpeedLimits.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listCmdSpeedLimits_MouseDoubleClick);
            // 
            // listCmdAddressActions
            // 
            this.listCmdAddressActions.FormattingEnabled = true;
            this.listCmdAddressActions.ItemHeight = 19;
            this.listCmdAddressActions.Location = new System.Drawing.Point(536, 63);
            this.listCmdAddressActions.Name = "listCmdAddressActions";
            this.listCmdAddressActions.ScrollAlwaysVisible = true;
            this.listCmdAddressActions.Size = new System.Drawing.Size(180, 441);
            this.listCmdAddressActions.TabIndex = 69;
            this.listCmdAddressActions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listCmdAddressActions_MouseDoubleClick);
            // 
            // listCmdAddressPositions
            // 
            this.listCmdAddressPositions.FormattingEnabled = true;
            this.listCmdAddressPositions.ItemHeight = 19;
            this.listCmdAddressPositions.Location = new System.Drawing.Point(267, 63);
            this.listCmdAddressPositions.Name = "listCmdAddressPositions";
            this.listCmdAddressPositions.ScrollAlwaysVisible = true;
            this.listCmdAddressPositions.Size = new System.Drawing.Size(243, 441);
            this.listCmdAddressPositions.TabIndex = 66;
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
            this.listMapAddressPositions.Location = new System.Drawing.Point(18, 62);
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
            // label_DebugFormCreateCommandPositions
            // 
            this.label_DebugFormCreateCommandPositions.AutoSize = true;
            this.label_DebugFormCreateCommandPositions.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_DebugFormCreateCommandPositions.Location = new System.Drawing.Point(18, 25);
            this.label_DebugFormCreateCommandPositions.Name = "label_DebugFormCreateCommandPositions";
            this.label_DebugFormCreateCommandPositions.Size = new System.Drawing.Size(75, 19);
            this.label_DebugFormCreateCommandPositions.TabIndex = 43;
            this.label_DebugFormCreateCommandPositions.Text = "Positions";
            // 
            // tbC_Debug
            // 
            this.tbC_Debug.Controls.Add(this.tbP_CreateCommand);
            this.tbC_Debug.Controls.Add(this.tbP_List);
            this.tbC_Debug.Controls.Add(this.tbP_LogMessage);
            this.tbC_Debug.Controls.Add(this.tP_Admin);
            this.tbC_Debug.Controls.Add(this.tP_SettingConfigs);
            this.tbC_Debug.Controls.Add(this.tP_CheckBarcodePosition);
            this.tbC_Debug.Location = new System.Drawing.Point(2, 3);
            this.tbC_Debug.Name = "tbC_Debug";
            this.tbC_Debug.SelectedIndex = 0;
            this.tbC_Debug.Size = new System.Drawing.Size(1291, 627);
            this.tbC_Debug.TabIndex = 50;
            // 
            // tP_CheckBarcodePosition
            // 
            this.tP_CheckBarcodePosition.Controls.Add(this.button_ComputeDelta);
            this.tP_CheckBarcodePosition.Controls.Add(this.button_Back);
            this.tP_CheckBarcodePosition.Controls.Add(this.textBox3);
            this.tP_CheckBarcodePosition.Controls.Add(this.button_CheckTRStartStop);
            this.tP_CheckBarcodePosition.Controls.Add(this.button_Front);
            this.tP_CheckBarcodePosition.Controls.Add(this.tB_MoveDistance);
            this.tP_CheckBarcodePosition.Controls.Add(this.label_MoveDistance);
            this.tP_CheckBarcodePosition.Controls.Add(this.button_GTMove);
            this.tP_CheckBarcodePosition.Controls.Add(this.tB_GTMoveAngle);
            this.tP_CheckBarcodePosition.Controls.Add(this.button_GTRight);
            this.tP_CheckBarcodePosition.Controls.Add(this.button_GTLeft);
            this.tP_CheckBarcodePosition.Controls.Add(this.button_GT0);
            this.tP_CheckBarcodePosition.Controls.Add(this.ucBox_WheelAngle);
            this.tP_CheckBarcodePosition.Controls.Add(this.ucBox_BarcodePosition);
            this.tP_CheckBarcodePosition.Controls.Add(this.ucBox_NowPosition);
            this.tP_CheckBarcodePosition.Controls.Add(this.ucBox_NodePosition);
            this.tP_CheckBarcodePosition.Location = new System.Drawing.Point(4, 22);
            this.tP_CheckBarcodePosition.Name = "tP_CheckBarcodePosition";
            this.tP_CheckBarcodePosition.Size = new System.Drawing.Size(1283, 601);
            this.tP_CheckBarcodePosition.TabIndex = 5;
            this.tP_CheckBarcodePosition.Text = "踩點專用";
            this.tP_CheckBarcodePosition.UseVisualStyleBackColor = true;
            // 
            // button_ComputeDelta
            // 
            this.button_ComputeDelta.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_ComputeDelta.Location = new System.Drawing.Point(755, 206);
            this.button_ComputeDelta.Name = "button_ComputeDelta";
            this.button_ComputeDelta.Size = new System.Drawing.Size(75, 28);
            this.button_ComputeDelta.TabIndex = 94;
            this.button_ComputeDelta.Text = "計算";
            this.button_ComputeDelta.UseVisualStyleBackColor = true;
            // 
            // button_Back
            // 
            this.button_Back.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_Back.Location = new System.Drawing.Point(816, 101);
            this.button_Back.Name = "button_Back";
            this.button_Back.Size = new System.Drawing.Size(58, 30);
            this.button_Back.TabIndex = 93;
            this.button_Back.Text = "往後";
            this.button_Back.UseVisualStyleBackColor = true;
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox3.Location = new System.Drawing.Point(81, 253);
            this.textBox3.MaxLength = 65550;
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox3.Size = new System.Drawing.Size(808, 89);
            this.textBox3.TabIndex = 54;
            // 
            // button_CheckTRStartStop
            // 
            this.button_CheckTRStartStop.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_CheckTRStartStop.Location = new System.Drawing.Point(344, 35);
            this.button_CheckTRStartStop.Name = "button_CheckTRStartStop";
            this.button_CheckTRStartStop.Size = new System.Drawing.Size(75, 28);
            this.button_CheckTRStartStop.TabIndex = 89;
            this.button_CheckTRStartStop.Text = "開始";
            this.button_CheckTRStartStop.UseVisualStyleBackColor = true;
            this.button_CheckTRStartStop.Click += new System.EventHandler(this.button_CheckTRStartStop_Click);
            // 
            // button_Front
            // 
            this.button_Front.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_Front.Location = new System.Drawing.Point(737, 101);
            this.button_Front.Name = "button_Front";
            this.button_Front.Size = new System.Drawing.Size(58, 31);
            this.button_Front.TabIndex = 88;
            this.button_Front.Text = "往前";
            this.button_Front.UseVisualStyleBackColor = true;
            // 
            // tB_MoveDistance
            // 
            this.tB_MoveDistance.Font = new System.Drawing.Font("新細明體", 14F);
            this.tB_MoveDistance.Location = new System.Drawing.Point(608, 101);
            this.tB_MoveDistance.Name = "tB_MoveDistance";
            this.tB_MoveDistance.Size = new System.Drawing.Size(105, 30);
            this.tB_MoveDistance.TabIndex = 87;
            // 
            // label_MoveDistance
            // 
            this.label_MoveDistance.AutoSize = true;
            this.label_MoveDistance.Font = new System.Drawing.Font("新細明體", 14F);
            this.label_MoveDistance.Location = new System.Drawing.Point(477, 107);
            this.label_MoveDistance.Name = "label_MoveDistance";
            this.label_MoveDistance.Size = new System.Drawing.Size(100, 19);
            this.label_MoveDistance.TabIndex = 86;
            this.label_MoveDistance.Text = "移動距離 : ";
            // 
            // button_GTMove
            // 
            this.button_GTMove.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_GTMove.Location = new System.Drawing.Point(1181, 32);
            this.button_GTMove.Name = "button_GTMove";
            this.button_GTMove.Size = new System.Drawing.Size(75, 31);
            this.button_GTMove.TabIndex = 85;
            this.button_GTMove.Text = "旋轉";
            this.button_GTMove.UseVisualStyleBackColor = true;
            this.button_GTMove.Click += new System.EventHandler(this.button_GTMove_Click);
            // 
            // tB_GTMoveAngle
            // 
            this.tB_GTMoveAngle.Font = new System.Drawing.Font("新細明體", 14F);
            this.tB_GTMoveAngle.Location = new System.Drawing.Point(1084, 32);
            this.tB_GTMoveAngle.Name = "tB_GTMoveAngle";
            this.tB_GTMoveAngle.Size = new System.Drawing.Size(79, 30);
            this.tB_GTMoveAngle.TabIndex = 84;
            // 
            // button_GTRight
            // 
            this.button_GTRight.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_GTRight.Location = new System.Drawing.Point(952, 32);
            this.button_GTRight.Name = "button_GTRight";
            this.button_GTRight.Size = new System.Drawing.Size(75, 31);
            this.button_GTRight.TabIndex = 83;
            this.button_GTRight.Text = "-90";
            this.button_GTRight.UseVisualStyleBackColor = true;
            this.button_GTRight.Click += new System.EventHandler(this.button_GTRight_Click);
            // 
            // button_GTLeft
            // 
            this.button_GTLeft.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_GTLeft.Location = new System.Drawing.Point(845, 32);
            this.button_GTLeft.Name = "button_GTLeft";
            this.button_GTLeft.Size = new System.Drawing.Size(75, 31);
            this.button_GTLeft.TabIndex = 82;
            this.button_GTLeft.Text = "90";
            this.button_GTLeft.UseVisualStyleBackColor = true;
            this.button_GTLeft.Click += new System.EventHandler(this.button_GTLeft_Click);
            // 
            // button_GT0
            // 
            this.button_GT0.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_GT0.Location = new System.Drawing.Point(734, 32);
            this.button_GT0.Name = "button_GT0";
            this.button_GT0.Size = new System.Drawing.Size(75, 31);
            this.button_GT0.TabIndex = 81;
            this.button_GT0.Text = "0";
            this.button_GT0.UseVisualStyleBackColor = true;
            this.button_GT0.Click += new System.EventHandler(this.button_GT0_Click);
            // 
            // ucBox_WheelAngle
            // 
            this.ucBox_WheelAngle.Location = new System.Drawing.Point(446, 35);
            this.ucBox_WheelAngle.Margin = new System.Windows.Forms.Padding(8);
            this.ucBox_WheelAngle.Name = "ucBox_WheelAngle";
            this.ucBox_WheelAngle.Size = new System.Drawing.Size(267, 27);
            this.ucBox_WheelAngle.TabIndex = 92;
            this.ucBox_WheelAngle.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucBox_WheelAngle.TagName = "label1";
            this.ucBox_WheelAngle.TagValue = "";
            // 
            // ucBox_BarcodePosition
            // 
            this.ucBox_BarcodePosition.Location = new System.Drawing.Point(32, 158);
            this.ucBox_BarcodePosition.Margin = new System.Windows.Forms.Padding(8);
            this.ucBox_BarcodePosition.Name = "ucBox_BarcodePosition";
            this.ucBox_BarcodePosition.Size = new System.Drawing.Size(267, 27);
            this.ucBox_BarcodePosition.TabIndex = 91;
            this.ucBox_BarcodePosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucBox_BarcodePosition.TagName = "label1";
            this.ucBox_BarcodePosition.TagValue = "";
            // 
            // ucBox_NowPosition
            // 
            this.ucBox_NowPosition.Location = new System.Drawing.Point(32, 96);
            this.ucBox_NowPosition.Margin = new System.Windows.Forms.Padding(8);
            this.ucBox_NowPosition.Name = "ucBox_NowPosition";
            this.ucBox_NowPosition.Size = new System.Drawing.Size(267, 27);
            this.ucBox_NowPosition.TabIndex = 90;
            this.ucBox_NowPosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucBox_NowPosition.TagName = "label1";
            this.ucBox_NowPosition.TagValue = "";
            // 
            // ucBox_NodePosition
            // 
            this.ucBox_NodePosition.Location = new System.Drawing.Point(32, 36);
            this.ucBox_NodePosition.Margin = new System.Windows.Forms.Padding(8);
            this.ucBox_NodePosition.Name = "ucBox_NodePosition";
            this.ucBox_NodePosition.Size = new System.Drawing.Size(267, 27);
            this.ucBox_NodePosition.TabIndex = 79;
            this.ucBox_NodePosition.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucBox_NodePosition.TagName = "label1";
            this.ucBox_NodePosition.TagValue = "";
            // 
            // tP_SettingConfigs
            // 
            this.tP_SettingConfigs.Controls.Add(this.tC_Configs);
            this.tP_SettingConfigs.Location = new System.Drawing.Point(4, 22);
            this.tP_SettingConfigs.Name = "tP_SettingConfigs";
            this.tP_SettingConfigs.Size = new System.Drawing.Size(1283, 601);
            this.tP_SettingConfigs.TabIndex = 6;
            this.tP_SettingConfigs.Text = "Configs";
            this.tP_SettingConfigs.UseVisualStyleBackColor = true;
            // 
            // tC_Configs
            // 
            this.tC_Configs.Controls.Add(this.tB_MoveControlConst);
            this.tC_Configs.Controls.Add(this.tB_MoveControlDictory);
            this.tC_Configs.Controls.Add(this.tB_MoveControlTurn);
            this.tC_Configs.Location = new System.Drawing.Point(3, 3);
            this.tC_Configs.Name = "tC_Configs";
            this.tC_Configs.SelectedIndex = 0;
            this.tC_Configs.Size = new System.Drawing.Size(1274, 595);
            this.tC_Configs.TabIndex = 0;
            // 
            // tB_MoveControlConst
            // 
            this.tB_MoveControlConst.Location = new System.Drawing.Point(4, 22);
            this.tB_MoveControlConst.Name = "tB_MoveControlConst";
            this.tB_MoveControlConst.Padding = new System.Windows.Forms.Padding(3);
            this.tB_MoveControlConst.Size = new System.Drawing.Size(1266, 569);
            this.tB_MoveControlConst.TabIndex = 0;
            this.tB_MoveControlConst.Text = "MoveControl";
            this.tB_MoveControlConst.UseVisualStyleBackColor = true;
            // 
            // tB_MoveControlDictory
            // 
            this.tB_MoveControlDictory.Location = new System.Drawing.Point(4, 22);
            this.tB_MoveControlDictory.Name = "tB_MoveControlDictory";
            this.tB_MoveControlDictory.Padding = new System.Windows.Forms.Padding(3);
            this.tB_MoveControlDictory.Size = new System.Drawing.Size(1266, 569);
            this.tB_MoveControlDictory.TabIndex = 1;
            this.tB_MoveControlDictory.Text = "MoveControlDictory";
            this.tB_MoveControlDictory.UseVisualStyleBackColor = true;
            // 
            // tB_MoveControlTurn
            // 
            this.tB_MoveControlTurn.Location = new System.Drawing.Point(4, 22);
            this.tB_MoveControlTurn.Name = "tB_MoveControlTurn";
            this.tB_MoveControlTurn.Size = new System.Drawing.Size(1266, 569);
            this.tB_MoveControlTurn.TabIndex = 2;
            this.tB_MoveControlTurn.Text = "MoveControlTurn";
            this.tB_MoveControlTurn.UseVisualStyleBackColor = true;
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
            this.Text = "MoveControlDebugForm";
            this.Load += new System.EventHandler(this.MoveCommandMonitor_Load);
            this.tP_Admin.ResumeLayout(false);
            this.tP_Admin.PerformLayout();
            this.tbP_LogMessage.ResumeLayout(false);
            this.tbP_LogMessage.PerformLayout();
            this.tbP_List.ResumeLayout(false);
            this.tbP_List.PerformLayout();
            this.tbP_CreateCommand.ResumeLayout(false);
            this.tbP_CreateCommand.PerformLayout();
            this.tbC_Debug.ResumeLayout(false);
            this.tP_CheckBarcodePosition.ResumeLayout(false);
            this.tP_CheckBarcodePosition.PerformLayout();
            this.tP_SettingConfigs.ResumeLayout(false);
            this.tC_Configs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer_UpdateData;
        private System.Windows.Forms.TextBox tbxLogView_MoveControlDebugMessage;
        private System.Windows.Forms.TabPage tP_Admin;
        private System.Windows.Forms.Button button_SimulationModeChange;
        private System.Windows.Forms.Label label_SimulationMode;
        private System.Windows.Forms.TabPage tbP_LogMessage;
        private System.Windows.Forms.Label label_ElmoMessage;
        private System.Windows.Forms.TextBox tbxLogView_CreateCommandMessage;
        private System.Windows.Forms.TextBox tbxLogView_ElmoMessage;
        private System.Windows.Forms.Label label_CreateCommandMessage;
        private System.Windows.Forms.TabPage tbP_List;
        private System.Windows.Forms.Button button_SimulateState;
        private System.Windows.Forms.Label label_Psuse;
        private System.Windows.Forms.Label label_WaitReserveIndex;
        private System.Windows.Forms.Label label_BeamState;
        private System.Windows.Forms.Label label_BumpState;
        private System.Windows.Forms.Label label_Psuse_Label;
        private System.Windows.Forms.Label label_WaitReserveIndex_Label;
        private System.Windows.Forms.Label label_BeamState_Label;
        private System.Windows.Forms.Label label_BumpState_Label;
        private System.Windows.Forms.Label label_SensorState_Label;
        private System.Windows.Forms.Label label_LoopTime_Label;
        private System.Windows.Forms.Label label_LoopTime;
        private System.Windows.Forms.Label label_SensorState;
        private System.Windows.Forms.Label label_AlarmMessage;
        private System.Windows.Forms.Label label_AlarmMessageName;
        private System.Windows.Forms.Label label_MoveCommandID;
        private System.Windows.Forms.Label label_MoveCommandIDLabel;
        private System.Windows.Forms.CheckBox cB_GetAllReserve;
        private System.Windows.Forms.Button button_ClearCommand;
        private System.Windows.Forms.Label label_ReserveList;
        private System.Windows.Forms.Label label_CommandList;
        private UcLabelTextBox ucLabelTB_EncoderPosition;
        private UcLabelTextBox ucLabelTB_Velocity;
        private UcLabelTextBox ucLabelTB_EncoderOffset;
        private UcLabelTextBox ucLabelTB_ElmoEncoder;
        private UcLabelTextBox ucLabelTtB_CommandListState;
        private UcLabelTextBox ucLabelTB_BarcodePosition;
        private UcLabelTextBox ucLabelTB_RealPosition;
        private UcLabelTextBox ucLabelTB_Delta;
        private UcLabelTextBox ucLabelTB_RealEncoder;
        private System.Windows.Forms.ListBox ReserveList;
        private System.Windows.Forms.Button button_SendList;
        private UcLabelTextBox ucLabelTextBox1;
        private System.Windows.Forms.ListBox CommandList;
        private System.Windows.Forms.Button button_StopMove;
        private System.Windows.Forms.TabPage tbP_CreateCommand;
        private System.Windows.Forms.CheckBox cB_OverrideTest;
        private System.Windows.Forms.Button button_FromTo;
        private System.Windows.Forms.Label label_FormTo;
        private System.Windows.Forms.Button button_ReveseAddressList;
        private System.Windows.Forms.Button button_RunTimesOdd;
        private System.Windows.Forms.Button button_RunTimesEven;
        private System.Windows.Forms.TextBox tB_RunTimes;
        private System.Windows.Forms.TextBox tB_HorizontalVelocity;
        private System.Windows.Forms.TextBox tB_LineVelocity;
        private System.Windows.Forms.TextBox tB_ChangeVelocity;
        private System.Windows.Forms.TextBox tB_PositionY;
        private System.Windows.Forms.TextBox tB_PositionX;
        private System.Windows.Forms.Label label_RunTimes;
        private System.Windows.Forms.Label label_HorizontalVelocity;
        private System.Windows.Forms.Label label_LineVelocity;
        private System.Windows.Forms.ComboBox cB_ChangeAction;
        private System.Windows.Forms.Label label_DebugFormCreateCommandVelocitys;
        private System.Windows.Forms.Label label_DebugFormCreateCommandActions;
        private System.Windows.Forms.Button Button_AutoCreate;
        private System.Windows.Forms.Button button_AddReadPosition;
        private System.Windows.Forms.Label label_LockResult;
        private UcLabelTextBox ucLabelTB_CreateCommandState;
        private UcLabelTextBox ucLabelTB_CreateCommand_BarcodePosition;
        private System.Windows.Forms.Button button_DebugModeSend;
        private System.Windows.Forms.ListBox listCmdSpeedLimits;
        private System.Windows.Forms.ListBox listCmdAddressActions;
        private System.Windows.Forms.ListBox listCmdAddressPositions;
        private System.Windows.Forms.Button btnRemoveLastAddressPosition;
        private System.Windows.Forms.Button btnPositionXY;
        private System.Windows.Forms.Button btnAddAddressPosition;
        private System.Windows.Forms.ListBox listMapAddressPositions;
        private System.Windows.Forms.Button btnClearMoveCmdInfo;
        private System.Windows.Forms.Label label_DebugFormCreateCommandPositions;
        private System.Windows.Forms.TabControl tbC_Debug;
        private System.Windows.Forms.TabPage tP_CheckBarcodePosition;
        private System.Windows.Forms.TextBox textBox3;
        private UcLabelTextBox ucBox_BarcodePosition;
        private UcLabelTextBox ucBox_NowPosition;
        private System.Windows.Forms.Button button_CheckTRStartStop;
        private System.Windows.Forms.Button button_Front;
        private System.Windows.Forms.TextBox tB_MoveDistance;
        private System.Windows.Forms.Label label_MoveDistance;
        private System.Windows.Forms.Button button_GTMove;
        private System.Windows.Forms.TextBox tB_GTMoveAngle;
        private System.Windows.Forms.Button button_GTRight;
        private System.Windows.Forms.Button button_GTLeft;
        private System.Windows.Forms.Button button_GT0;
        private UcLabelTextBox ucBox_NodePosition;
        private UcLabelTextBox ucBox_WheelAngle;
        private System.Windows.Forms.Button button_Back;
        private System.Windows.Forms.Button button_ComputeDelta;
        private System.Windows.Forms.Button button_CreateCommandList_StopAndClear;
        private System.Windows.Forms.Button button_CreateCommandList_Stop;
        private System.Windows.Forms.TabPage tP_SettingConfigs;
        private System.Windows.Forms.TabControl tC_Configs;
        private System.Windows.Forms.TabPage tB_MoveControlConst;
        private System.Windows.Forms.TabPage tB_MoveControlDictory;
        private System.Windows.Forms.TabPage tB_MoveControlTurn;
    }
}