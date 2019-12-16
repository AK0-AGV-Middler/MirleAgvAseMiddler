using Mirle.Agv.Controller;
using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;


namespace Mirle.Agv.View
{
    public partial class MoveCommandDebugModeForm : Form
    {
        public List<MapSection> RunSectionList { get; set; } = new List<MapSection>();
        public MapAddress RunEndAddress { get; set; }
        public bool MainShowRunSectionList { get; set; } = false;
        private MoveControlHandler moveControl;
        private MoveCommandData command;
        private MapInfo theMapInfo = new MapInfo();
        private MoveCmdInfo moveCmdInfo = new MoveCmdInfo();
        private SimulateSettingAGVAngleForm settingAngleForm;
        private MoveControlSimulateStateForm simulateStateForm;
        private ComputeFunction computeFunction = new ComputeFunction();
        private bool hideFunctionOn = false;
        private List<string> commandStringList = new List<string>();
        private List<string> reserveStringList = new List<string>();
        private int reserveIndex;
        private int commandIndex;
        private AGVPosition tempReal = null;
        private string commandID = "";
        private bool simulationModeFirstDoubleClick = false;
        private Dictionary<EnumMoveControlSafetyType, SafetyInformation> safetyUserControl = new Dictionary<EnumMoveControlSafetyType, SafetyInformation>();
        private Dictionary<EnumSensorSafetyType, SensorByPassInformation> sensorByPassUserControl = new Dictionary<EnumSensorSafetyType, SensorByPassInformation>();

        private TabPage checkBarcodePositionPage;
        private TabPage configs;

        private AGVPosition checkBarcodeNode;
        private AGVPosition nowPosition;

        private bool initailConfigForm = false;

        private EnumAxis[] AxisList = new EnumAxis[18] {EnumAxis.XFL, EnumAxis.XFR, EnumAxis.XRL, EnumAxis.XRR,
                                                        EnumAxis.TFL, EnumAxis.TFR, EnumAxis.TRL, EnumAxis.TRR,
                                                        EnumAxis.VXFL, EnumAxis.VXFR, EnumAxis.VXRL, EnumAxis.VXRR,
                                                        EnumAxis.VTFL, EnumAxis.VTFR, EnumAxis.VTRL, EnumAxis.VTRR,
                                                        EnumAxis.GX, EnumAxis.GT};

        #region Initail
        public MoveCommandDebugModeForm(MoveControlHandler moveControl, MapInfo theMapInfo)
        {
            InitializeComponent();
            this.moveControl = moveControl;
            this.theMapInfo = theMapInfo;
            this.Text = this.Text + " Version : " + moveControl.MoveControlVersion;
            checkBarcodePositionPage = tbC_Debug.TabPages[5];
            tbC_Debug.TabPages.RemoveAt(5);
            configs = tbC_Debug.TabPages[4];
            tbC_Debug.TabPages.RemoveAt(4);
        }

        private void MoveCommandMonitor_Load(object sender, EventArgs e)
        {
            button_SendList.Enabled = false;
            ucLabelTB_RealEncoder.TagName = "Real Encoder : ";
            ucLabelTB_RealPosition.TagName = "Real Position : ";
            ucLabelTB_BarcodePosition.TagName = "Barcode Position : ";
            ucLabelTB_Delta.TagName = "Encoder Delta : ";
            ucLabelTB_CreateCommand_BarcodePosition.TagName = "Barcode Position : ";
            ucLabelTtB_CommandListState.TagName = "Move State : ";
            ucLabelTB_CreateCommandState.TagName = "Move State : ";
            ucLabelTB_Velocity.TagName = "Velocity : ";
            ucLabelTB_ElmoEncoder.TagName = "Elmo encoder : ";
            ucLabelTB_EncoderOffset.TagName = "Offset : ";
            ucLabelTB_EncoderPosition.TagName = "VelocityCmd : ";
            AddListMapAddressPositions();
            AddAdminSafetyUserControl();
            HideChangeUI();
            //InitailConfigsPage();
        }

        private void AddAdminSafetyUserControl()
        {
            SafetyInformation temp;
            int x = 30;
            int y = 80;

            foreach (EnumMoveControlSafetyType item in (EnumMoveControlSafetyType[])Enum.GetValues(typeof(EnumMoveControlSafetyType)))
            {
                temp = new SafetyInformation(moveControl, item);
                temp.Location = new System.Drawing.Point(x, y);
                y += 40;
                temp.Name = item.ToString();
                temp.Size = new System.Drawing.Size(647, 30);

                switch (item)
                {
                    case EnumMoveControlSafetyType.TurnOut:
                        temp.SetLabelString("出彎保護 : ", "出彎多久內必須讀到Barcode :");
                        break;
                    case EnumMoveControlSafetyType.LineBarcodeInterval:
                        temp.SetLabelString("直線保護 : ", "直線Barcode最大間隔 :");
                        break;
                    case EnumMoveControlSafetyType.OntimeReviseTheta:
                        temp.SetLabelString("角度偏差 : ", "容許Theta偏差量 :");
                        break;
                    case EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine:
                        temp.SetLabelString("直線偏差 : ", "容許直線軌道偏差量 :");
                        break;
                    case EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal:
                        temp.SetLabelString("橫移偏差 : ", "容許橫移軌道偏差量 :");
                        break;
                    case EnumMoveControlSafetyType.OneTimeRevise:
                        temp.SetLabelString("一次修正 : ", "一次性修正距離,多少會修完 :");
                        break;
                    case EnumMoveControlSafetyType.VChangeSafetyDistance:
                        temp.SetLabelString("降速保護 : ", "多少距離檢查一次速度變化 :");
                        break;
                    case EnumMoveControlSafetyType.TRPathMonitoring:
                        temp.SetLabelString("監控TR軌跡 : ", "角度允許誤差 :");
                        break;
                    case EnumMoveControlSafetyType.IdleNotWriteLog:
                        temp.SetLabelString("Idle不Log : ", "移動完成後再多記多少ms : ");
                        break;
                    case EnumMoveControlSafetyType.BarcodePositionSafety:
                        temp.SetLabelString("Barcode保護 : ", "Config (mm) : ");
                        break;
                    case EnumMoveControlSafetyType.StopWithoutReason:
                        temp.SetLabelString("默停偵測 : ", "Config (ms) : ");
                        break;
                    case EnumMoveControlSafetyType.BeamSensorR2000:
                        temp.SetLabelString("Beam R2000 : ", "delay (ms) : ");
                        break;
                    default:
                        break;
                }

                temp.UpdateEnableRange();
                this.tP_Admin.Controls.Add(temp);
                safetyUserControl.Add(item, temp);
            }

            x = 830;
            y = 80;

            SensorByPassInformation tempSensor;
            foreach (EnumSensorSafetyType item in (EnumSensorSafetyType[])Enum.GetValues(typeof(EnumSensorSafetyType)))
            {
                tempSensor = new SensorByPassInformation(moveControl, item);
                tempSensor.Location = new System.Drawing.Point(x, y);
                y += 40;
                tempSensor.Name = item.ToString();
                tempSensor.Size = new System.Drawing.Size(222, 30);

                switch (item)
                {
                    case EnumSensorSafetyType.Charging:
                        tempSensor.SetLabelString("充電檢查 : ");
                        tempSensor.DisableButton();
                        break;
                    case EnumSensorSafetyType.ForkHome:
                        tempSensor.SetLabelString("Fork原點 : ");
                        break;
                    case EnumSensorSafetyType.BeamSensor:
                        tempSensor.SetLabelString("BeamSensor : ");
                        break;
                    case EnumSensorSafetyType.BeamSensorTR:
                        tempSensor.SetLabelString("Beam TR : ");
                        break;
                    case EnumSensorSafetyType.TRFlowStart:
                        tempSensor.SetLabelString("TR中啟動 : ");
                        break;
                    case EnumSensorSafetyType.R2000FlowStat:
                        tempSensor.SetLabelString("R2000中啟動 : ");
                        break;
                    case EnumSensorSafetyType.Bumper:
                        tempSensor.SetLabelString("Bumper : ");
                        break;
                    case EnumSensorSafetyType.CheckAxisState:
                        tempSensor.SetLabelString("監控Axis狀態 : ");
                        tempSensor.DisableButton();
                        break;
                    case EnumSensorSafetyType.EndPositionOffset:
                        tempSensor.SetLabelString("終點offset : ");
                        break;
                    case EnumSensorSafetyType.SecondCorrectionBySide:
                        tempSensor.SetLabelString("使用側邊二修 : ");
                        break;
                    default:
                        break;
                }

                tempSensor.UpdateEnable();
                this.tP_Admin.Controls.Add(tempSensor);
                sensorByPassUserControl.Add(item, tempSensor);
            }
        }

        private void AddListMapAddressPositions()
        {
            listMapAddressPositions.Items.Clear();
            foreach (var valuePair in theMapInfo.allMapAddresses)
            {
                MapAddress mapAddress = valuePair.Value;
                MapPosition mapPosition = mapAddress.Position;
                string txtPosition = $"{mapPosition.X},{mapPosition.Y}";
                listMapAddressPositions.Items.Add(txtPosition);
            }
        }
        #endregion

        #region timer Update
        private void Timer_Update_CreateCommand()
        {
            AGVPosition tempBarcodePosition = moveControl.location.agvPosition;

            if (tempBarcodePosition != null)
            {
                ucLabelTB_CreateCommand_BarcodePosition.TagValue = "( " + tempBarcodePosition.Position.X.ToString("0") + ", " +
                                                                         tempBarcodePosition.Position.Y.ToString("0") + " )";
            }
            else
                ucLabelTB_CreateCommand_BarcodePosition.TagValue = "( ---, --- )";

            ucLabelTB_CreateCommandState.TagValue = moveControl.MoveState.ToString();

            bool buttonEnable = false;
            string lockResult = "";

            if (Vehicle.Instance.AutoState != EnumAutoState.Manual)
                lockResult = "Lock Result : AutoMode中!";
            else if (moveControl.MoveState != EnumMoveState.Idle)
                lockResult = "Lock Result : MoveState動作中!";
            else if (moveControl.IsCharging())
                lockResult = "Lock Result : Charging中!";
            else if (moveControl.ForkNotHome())
                lockResult = "Lock Result : Fork不在Home點!";
            else
                buttonEnable = true;

            button_DebugModeSend.Enabled = (cB_OverrideTest.Checked && moveControl.MoveState != EnumMoveState.Error) ? true : buttonEnable;
            button_FromTo.Enabled = (cB_OverrideTest.Checked && moveControl.MoveState != EnumMoveState.Error) ? true : buttonEnable; ;
            label_LockResult.Text = lockResult;
        }

        private void UpdateList()
        {
            ShowReserveList(null);
            ShowMoveCommandList(null);
        }

        private void SetStateWithColor(Label label, EnumVehicleSafetyAction state)
        {
            label.Text = state.ToString();

            switch (state)
            {
                case EnumVehicleSafetyAction.Normal:
                    label.ForeColor = System.Drawing.Color.Green;
                    break;

                case EnumVehicleSafetyAction.LowSpeed:
                    label.ForeColor = System.Drawing.Color.Yellow;
                    break;

                case EnumVehicleSafetyAction.Stop:
                    label.ForeColor = System.Drawing.Color.Red;
                    break;

                default:
                    break;
            }
        }

        private void Timer_Update_CommandList()
        {
            string nowCommandID = moveControl.MoveCommandID;
            if (nowCommandID != commandID || (moveControl.MoveState != EnumMoveState.Idle && moveControl.command.CommandList.Count != commandStringList.Count))
            {
                ReserveList.TopIndex = 0;
                CommandList.TopIndex = 0;

                UpdateList();
                commandID = nowCommandID;
                label_MoveCommandID.Text = commandID;
            }

            AGVPosition tempBarcodePosition = moveControl.location.agvPosition;

            label_AlarmMessage.Text = moveControl.AGVStopResult;

            ucLabelTB_RealEncoder.TagValue = moveControl.location.RealEncoder.ToString("0.0");
            ucLabelTB_Delta.TagValue = moveControl.location.Delta.ToString("0.0");
            ucLabelTB_Velocity.TagValue = moveControl.location.Velocity.ToString("0");
            ucLabelTB_ElmoEncoder.TagValue = moveControl.location.ElmoEncoder.ToString("0");
            ucLabelTB_EncoderOffset.TagValue = moveControl.location.Offset.ToString("0");

            double looptime = moveControl.LoopTime;
            if (looptime > 10)
                label_LoopTime.ForeColor = System.Drawing.Color.Red;
            else
                label_LoopTime.ForeColor = System.Drawing.Color.Black;

            label_LoopTime.Text = looptime.ToString("0") + "ms";
            tempReal = moveControl.location.Real;

            ucLabelTB_RealPosition.TagValue = (tempReal != null) ?
                "( " + tempReal.Position.X.ToString("0") + ", " + tempReal.Position.Y.ToString("0") + " )" : "( ---, --- )";

            ucLabelTB_BarcodePosition.TagValue = (tempBarcodePosition != null) ?
                 "( " + tempBarcodePosition.Position.X.ToString("0") + ", " +
                 tempBarcodePosition.Position.Y.ToString("0") + " )" : "( ---, --- )";

            tempBarcodePosition = moveControl.location.Encoder;
            ucLabelTB_EncoderPosition.TagValue = moveControl.ControlData.VelocityCommand.ToString("0");

            ucLabelTtB_CommandListState.TagValue = moveControl.MoveState.ToString();

            SetStateWithColor(label_SensorState, moveControl.ControlData.SensorState);
            SetStateWithColor(label_BeamState, moveControl.ControlData.BeamSensorState);
            SetStateWithColor(label_BumpState, moveControl.ControlData.BumpSensorState);

            label_WaitReserveIndex.Text = moveControl.ControlData.WaitReserveIndex.ToString("0");
            label_WaitReserveIndex.ForeColor = label_WaitReserveIndex.Text == "-1" ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            label_Psuse.Text = (moveControl.ControlData.PauseRequest || moveControl.ControlData.PauseAlready) ? "Pause" : "Normal";
            label_Psuse.ForeColor = (label_Psuse.Text == "Normal") ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            button_SimulateState.BackColor = (hideFunctionOn ? Color.Red : Color.Transparent);
            button_RetryMove.Enabled = moveControl.MoveState == EnumMoveState.Idle;
            button_RetryMove.Visible = hideFunctionOn;
            try
            {
                int tempResserveIndex = moveControl.GetReserveIndex();
                int tempCommandIndex = moveControl.command.IndexOfCmdList;
                int temp = ReserveList.TopIndex;
                if (tempResserveIndex > reserveIndex)
                {
                    for (int i = reserveIndex; i < tempResserveIndex; i++)
                        ReserveList.Items[i] = String.Concat("▶ ", reserveStringList[i]);

                    reserveIndex = tempResserveIndex;
                }

                ReserveList.TopIndex = temp;

                temp = CommandList.TopIndex;
                if (tempCommandIndex != -1 && tempCommandIndex > commandIndex)
                {
                    for (int i = commandIndex; i < tempCommandIndex; i++)
                        CommandList.Items[i] = String.Concat("▶ ", commandStringList[i]);

                    commandIndex = tempCommandIndex;
                }

                CommandList.TopIndex = temp;
            }
            catch
            {
            }
        }

        private void Timer_Update_LogMessage()
        {
            try
            {
                tbxLogView_CreateCommandMessage.Text = moveControl.CreateMoveCommandList.CreateCommandListLog;
                tbxLogView_ElmoMessage.Text = moveControl.elmoDriver.ElmoLog;
            }
            catch { }
        }

        private void Timer_Update_Admin()
        {
            button_SimulationModeChange.Text = moveControl.SimulationMode ? "開啟中" : "關閉中";
            button_SimulationModeChange.BackColor = moveControl.SimulationMode ? Color.Red : Color.Transparent;
            button_SimulationModeChange.Enabled = !moveControl.elmoDriver.Connected;

            foreach (EnumMoveControlSafetyType item in (EnumMoveControlSafetyType[])Enum.GetValues(typeof(EnumMoveControlSafetyType)))
            {
                try
                {
                    safetyUserControl[item].UpdateEnable();
                }
                catch { }
            }

            foreach (EnumSensorSafetyType item in (EnumSensorSafetyType[])Enum.GetValues(typeof(EnumSensorSafetyType)))
            {
                try
                {
                    sensorByPassUserControl[item].UpdateEnable();
                }
                catch { }
            }
        }

        private void timer_UpdateData_Tick(object sender, EventArgs e)
        {//"▷▶"
            if (tbC_Debug.SelectedIndex == 0)
                Timer_Update_CreateCommand();
            else if (tbC_Debug.SelectedIndex == 1)
                Timer_Update_CommandList();
            else if (tbC_Debug.SelectedIndex == 2)
                Timer_Update_LogMessage();
            else if (tbC_Debug.SelectedIndex == 3)
                Timer_Update_Admin();

            tbxLogView_MoveControlDebugMessage.Text = moveControl.DebugFlowLog;
        }
        #endregion

        #region Page Create Command

        private void ResetListAndMoveCommandInfo()
        {
            moveCmdInfo = new MoveCmdInfo();
            listCmdAddressActions.Items.Clear();
            listCmdSpeedLimits.Items.Clear();
            HideChangeUI();
        }

        private void btnAddAddressPosition_Click(object sender, EventArgs e)
        {
            if (listMapAddressPositions.Items.Count < 1)
                return;
            if (listMapAddressPositions.SelectedIndex < 0)
                listMapAddressPositions.SelectedIndex = 0;

            listCmdAddressPositions.Items.Add(listMapAddressPositions.SelectedItem);
            ResetListAndMoveCommandInfo();
        }

        private void btnRemoveLastAddressPosition_Click(object sender, EventArgs e)
        {
            if (listCmdAddressPositions.Items.Count < 1)
                return;
            if (listCmdAddressPositions.SelectedIndex < 0)
                listCmdAddressPositions.SelectedIndex = listCmdAddressPositions.Items.Count - 1;

            listCmdAddressPositions.Items.RemoveAt(listCmdAddressPositions.SelectedIndex);

            ResetListAndMoveCommandInfo();
        }

        private void btnAddressPositionsClear_Click(object sender, EventArgs e)
        {
            listCmdAddressPositions.Items.Clear();
        }

        private void btnPositionXY_Click(object sender, EventArgs e)
        {
            try
            {
                double x = double.Parse(tB_PositionX.Text);
                double y = double.Parse(tB_PositionY.Text);

                string txtPosition = $"{x.ToString()},{y.ToString()}";
                listCmdAddressPositions.Items.Add(txtPosition);

                ResetListAndMoveCommandInfo();
            }
            catch
            {
                MessageBox.Show("請輸入正確格式..");
            }
        }

        private void SetSpeedLimits()
        {
            if (moveCmdInfo == null)
            {
                moveCmdInfo = new MoveCmdInfo();
                moveCmdInfo.SectionSpeedLimits = new List<double>();
            }
            else if (moveCmdInfo.SectionSpeedLimits == null)
                moveCmdInfo.SectionSpeedLimits = new List<double>();
            else
                moveCmdInfo.SectionSpeedLimits.Clear();

            for (int i = 0; i < listCmdSpeedLimits.Items.Count; i++)
            {
                double limit = double.Parse(listCmdSpeedLimits.Items[i].ToString());
                moveCmdInfo.SectionSpeedLimits.Add(limit);
            }
        }

        private void SetActions()
        {
            if (moveCmdInfo == null)
            {
                moveCmdInfo = new MoveCmdInfo();
                moveCmdInfo.AddressActions = new List<EnumAddressAction>();
            }
            else if (moveCmdInfo.AddressActions == null)
                moveCmdInfo.AddressActions = new List<EnumAddressAction>();
            else
                moveCmdInfo.AddressActions.Clear();

            for (int i = 0; i < listCmdAddressActions.Items.Count; i++)
            {
                string strAction = (string)listCmdAddressActions.Items[i];
                EnumAddressAction action = (EnumAddressAction)Enum.Parse(typeof(EnumAddressAction), strAction);
                moveCmdInfo.AddressActions.Add(action);
            }
        }

        private void SetPositions()
        {
            if (moveCmdInfo == null)
            {
                moveCmdInfo = new MoveCmdInfo();
                moveCmdInfo.AddressPositions = new List<MapPosition>();
            }
            else if (moveCmdInfo.AddressPositions == null)
                moveCmdInfo.AddressPositions = new List<MapPosition>();
            else
                moveCmdInfo.AddressPositions.Clear();

            for (int i = 0; i < listCmdAddressPositions.Items.Count; i++)
            {
                string positionPair = (string)listCmdAddressPositions.Items[i];
                string[] posXY = positionPair.Split(',');
                var posX = float.Parse(posXY[0]);
                var posY = float.Parse(posXY[1]);
                moveCmdInfo.AddressPositions.Add(new MapPosition(posX, posY));
            }
        }

        private void btnClearMoveCmdInfo_Click(object sender, EventArgs e)
        {
            listCmdAddressPositions.Items.Clear();
            ResetListAndMoveCommandInfo();
        }

        private void button_DebugModeSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (listCmdAddressActions.Items.Count == 0 || listCmdSpeedLimits.Items.Count == 0)
                {
                    if (listCmdAddressPositions.Items.Count == 0)
                        return;
                    else if (listCmdAddressPositions.Items.Count == 1)
                        button_FromTo_Click(null, null);

                    Button_AutoCreate_Click(null, null);
                }

                if (listCmdAddressActions.Items.Count == 0 || listCmdSpeedLimits.Items.Count == 0)
                    return;

                string errorMessage = "";
                SetPositions();
                SetActions();
                SetSpeedLimits();

                if (cB_OverrideTest.Checked)
                {
                    moveCmdInfo.MovingAddress = new List<MapAddress>();
                    moveCmdInfo.MovingSections = new List<MapSection>();
                    MapSection tempMapSection;
                    MapAddress tempMapAddress;

                    for (int i = 0; i < moveCmdInfo.AddressPositions.Count; i++)
                    {
                        tempMapAddress = new MapAddress();
                        tempMapAddress.IsTR50 = IsTR50(moveCmdInfo.AddressPositions[i]);
                        moveCmdInfo.MovingAddress.Add(tempMapAddress);

                        if (i + 1 < moveCmdInfo.AddressPositions.Count)
                        {
                            tempMapSection = new MapSection();
                            if (IsSectionR2000(moveCmdInfo.AddressPositions[i], moveCmdInfo.AddressPositions[i + 1]))
                                tempMapSection.Type = EnumSectionType.R2000;
                            else
                                tempMapSection.Type = EnumSectionType.None;

                            moveCmdInfo.MovingSections.Add(tempMapSection);
                        }
                    }

                    if (!moveControl.TransferMove_Override(moveCmdInfo, ref errorMessage))
                    {
                        MessageBox.Show("Override 失敗!\n" + errorMessage);
                    }
                    else
                    {
                        MainShowRunSectionList = true;
                        tbC_Debug.SelectedIndex = 1;
                    }
                }
                else
                {
                    if (moveControl.MoveState != EnumMoveState.Idle)
                    {
                        MessageBox.Show("動作命令中!");
                        return;
                    }

                    command = moveControl.CreateMoveControlListSectionListReserveList(moveCmdInfo, ref errorMessage);

                    if (command != null)
                    {
                        tbC_Debug.SelectedIndex = 1;
                        button_SendList.Enabled = true;
                        ShowList();
                        MainShowRunSectionList = true;
                    }
                    else
                        MessageBox.Show("List 產生失敗!\n" + errorMessage);
                }
            }
            catch
            {
                MessageBox.Show("List 產生失敗 (Excption)!");
            }
        }
        #endregion

        #region Page Command List
        private void ShowReserveList(List<ReserveData> reserveList)
        {
            reserveIndex = 0;
            ReserveList.Items.Clear();
            reserveStringList = new List<string>();

            moveControl.GetReserveListInfo(reserveList, ref reserveStringList);
            for (int i = 0; i < reserveStringList.Count; i++)
                ReserveList.Items.Add(String.Concat("▷ ", reserveStringList[i]));
        }

        private void ShowMoveCommandList(List<Command> cmdList)
        {
            commandIndex = 0;
            CommandList.Items.Clear();
            commandStringList = new List<string>();

            moveControl.GetMoveCommandListInfo(cmdList, ref commandStringList);
            for (int i = 0; i < commandStringList.Count; i++)
                CommandList.Items.Add(String.Concat("▷ ", commandStringList[i]));
        }

        private void ShowList()
        {
            ShowReserveList(command.ReserveList);
            ShowMoveCommandList(command.CommandList);
        }

        private void button_SendList_Click(object sender, EventArgs e)
        {
            moveControl.TransferMoveDebugMode(command);
            button_SendList.Enabled = false;

            if (cB_GetAllReserve.Checked)
            {
                Thread.Sleep(100);
                moveControl.AddAllReserve();
            }
        }

        private void button_StopMove_Click(object sender, EventArgs e)
        {
            moveControl.StopFlagOn();
        }

        private void button_ClearCommand_Click(object sender, EventArgs e)
        {
            moveControl.StopAndClear();
        }

        private MoveCmdInfo GetBackCmdInfo(MoveCmdInfo AGVMCommand)
        {
            MoveCmdInfo backAGVMCommand = new MoveCmdInfo();

            backAGVMCommand.SectionSpeedLimits = new List<double>();
            backAGVMCommand.AddressActions = new List<EnumAddressAction>();
            backAGVMCommand.AddressPositions = new List<MapPosition>();

            for (int i = AGVMCommand.SectionSpeedLimits.Count - 1; i >= 0; i--)
                backAGVMCommand.SectionSpeedLimits.Add(AGVMCommand.SectionSpeedLimits[i]);

            for (int i = AGVMCommand.AddressActions.Count - 1; i >= 0; i--)
                backAGVMCommand.AddressActions.Add(AGVMCommand.AddressActions[i]);

            for (int i = AGVMCommand.AddressPositions.Count - 1; i >= 0; i--)
                backAGVMCommand.AddressPositions.Add(AGVMCommand.AddressPositions[i]);

            return backAGVMCommand;
        }

        private void ReserveList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.ReserveList.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                if (ReserveList.Items.Count < 1)
                    return;
                if (ReserveList.SelectedIndex < 0)
                    return;

                moveControl.AddReservedIndexForDebugModeTest(ReserveList.SelectedIndex);
            }
        }
        #endregion

        #region tabPage Admin
        public void button_SimulationMode_Click(object sender, EventArgs e)
        {
            if (!button_SimulationModeChange.Enabled)
                return;

            button_SimulationModeChange.Enabled = false;

            moveControl.SimulationMode = (button_SimulationModeChange.Text == "關閉中");

            button_SimulateState.Visible = (hideFunctionOn ? true : moveControl.SimulationMode);

            if (simulateStateForm != null && !simulateStateForm.IsDisposed)
                simulateStateForm.ResetHideFunctionOnOff(hideFunctionOn);

            simulationModeFirstDoubleClick = moveControl.SimulationMode;
            button_SimulationModeChange.Text = (button_SimulationModeChange.Text == "關閉中") ? "開啟中" : "關閉中";
            button_SimulationModeChange.BackColor = moveControl.SimulationMode ? Color.Red : Color.Transparent;

            button_SimulationModeChange.Enabled = true;
        }
        #endregion

        private void listMapAddressPositions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listMapAddressPositions.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                if (listMapAddressPositions.Items.Count < 1)
                    return;
                if (listMapAddressPositions.SelectedIndex < 0)
                    return;

                listCmdAddressPositions.Items.Add(listMapAddressPositions.SelectedItem);
            }

            ResetListAndMoveCommandInfo();
        }

        public void AddAddressPositionByMainFormDoubleClick(string id)
        {
            if (theMapInfo.allMapAddresses.ContainsKey(id))
            {
                if (tbC_Debug.SelectedIndex == 4)
                {
                    if (moveControl.location.Real != null)
                    {
                        checkBarcodeNode = new AGVPosition();

                        checkBarcodeNode.AGVAngle = moveControl.location.Real.AGVAngle;
                        checkBarcodeNode.Position = new MapPosition(theMapInfo.allMapAddresses[id].Position.X, theMapInfo.allMapAddresses[id].Position.Y);
                    }
                }
                else
                {
                    string str = theMapInfo.allMapAddresses[id].Position.X.ToString() + "," + theMapInfo.allMapAddresses[id].Position.Y.ToString();
                    listCmdAddressPositions.Items.Add(str);
                    tbC_Debug.SelectedIndex = 0;

                    ResetListAndMoveCommandInfo();

                    if (moveControl.SimulationMode && simulationModeFirstDoubleClick)
                    {
                        simulationModeFirstDoubleClick = false;
                        moveControl.location.Real = new AGVPosition();
                        moveControl.location.Real.AGVAngle = 0;
                        settingAngleForm = new SimulateSettingAGVAngleForm(moveControl);
                        settingAngleForm.Show();
                        settingAngleForm.TopMost = true;

                        moveControl.location.Real.Position = theMapInfo.allMapAddresses[id].Position;
                        Vehicle.Instance.VehicleLocation.RealPosition = moveControl.location.Real.Position;
                        Vehicle.Instance.VehicleLocation.VehicleAngle = moveControl.location.Real.AGVAngle;
                    }
                }
            }
        }

        private bool IsAddress(MapPosition now)
        {
            MapPosition tempPosition = null;

            foreach (var valuePair in theMapInfo.allMapAddresses)
            {
                if (Math.Abs(now.X - valuePair.Value.Position.X) <= 30 &&
                    Math.Abs(now.Y - valuePair.Value.Position.Y) <= 30)
                {
                    if (tempPosition == null || Math.Abs(Math.Pow(valuePair.Value.Position.X - now.X, 2) + Math.Pow(valuePair.Value.Position.Y - now.Y, 2)) <
                                                Math.Abs(Math.Pow(tempPosition.X - now.X, 2) + Math.Pow(tempPosition.Y - now.Y, 2)))
                        tempPosition = valuePair.Value.Position;
                }
            }

            if (tempPosition != null)
            {
                string str = tempPosition.X.ToString() + "," + tempPosition.Y.ToString();
                listCmdAddressPositions.Items.Add(str);
                return true;
            }
            else
                return false;
        }

        private void FindPositionBySection(MapPosition now)
        {
            foreach (var valuePair in theMapInfo.allMapSections)
            {
                if (valuePair.Value.HeadAddress.Position.X == valuePair.Value.TailAddress.Position.X &&
                     Math.Abs(valuePair.Value.HeadAddress.Position.X - now.X) <= 30)
                {
                    string str = valuePair.Value.HeadAddress.Position.X.ToString() + "," + now.Y.ToString();
                    listCmdAddressPositions.Items.Add(str);
                    return;
                }
                else if (valuePair.Value.HeadAddress.Position.Y == valuePair.Value.TailAddress.Position.Y &&
                     Math.Abs(valuePair.Value.HeadAddress.Position.Y - now.Y) <= 30)
                {
                    string str = now.X.ToString() + "," + valuePair.Value.HeadAddress.Position.Y.ToString();
                    listCmdAddressPositions.Items.Add(str);
                    return;
                }
            }
        }

        private void button_AddReadPosition_Click(object sender, EventArgs e)
        {
            if (moveControl.location.Real != null)
            {
                if (!IsAddress(moveControl.location.Real.Position))
                {
                    FindPositionBySection(moveControl.location.Real.Position);

                    ResetListAndMoveCommandInfo();
                }
            }
        }

        private void button_SimulateState_Click(object sender, EventArgs e)
        {
            if (simulateStateForm == null || simulateStateForm.IsDisposed)
            {
                simulateStateForm = new MoveControlSimulateStateForm(moveControl, hideFunctionOn);
                simulateStateForm.Show();
                simulateStateForm.TopMost = true;
            }

        }

        private bool IsSectionR2000(MapPosition start, MapPosition end)
        {
            foreach (var valuePair in theMapInfo.allMapSections)
            {
                if ((valuePair.Value.HeadAddress.Position.X == start.X && valuePair.Value.HeadAddress.Position.Y == start.Y) &&
                    (valuePair.Value.TailAddress.Position.X == end.X && valuePair.Value.TailAddress.Position.Y == end.Y))
                    return valuePair.Value.Type == EnumSectionType.R2000;
                else if ((valuePair.Value.HeadAddress.Position.X == end.X && valuePair.Value.HeadAddress.Position.Y == end.Y) &&
                         (valuePair.Value.TailAddress.Position.X == start.X && valuePair.Value.TailAddress.Position.Y == start.Y))
                    return valuePair.Value.Type == EnumSectionType.R2000;
            }

            return false;
        }

        private bool IsTR50(MapPosition temp)
        {
            foreach (var valuePair in theMapInfo.allMapAddresses)
            {
                if (temp.X == valuePair.Value.Position.X && temp.Y == valuePair.Value.Position.Y)
                    return valuePair.Value.IsTR50;
            }

            return true;
        }

        public void GetSectionSpeed()
        {
            if (moveCmdInfo == null)
                return;

            if (moveCmdInfo.AddressPositions == null || moveCmdInfo.AddressPositions.Count < 2)
                return;

            if (moveCmdInfo.AddressActions == null || moveCmdInfo.AddressActions.Count < 2)
                return;

            moveCmdInfo.SectionSpeedLimits = new List<double>();
            bool isLine;

            int firstSectionAngle = computeFunction.ComputeAngleInt(moveCmdInfo.AddressPositions[0], moveCmdInfo.AddressPositions[1]);

            if (moveControl.location.Real == null)
                return;

            int agvAngle = (int)moveControl.location.Real.AGVAngle;

            isLine = (agvAngle == firstSectionAngle) || (computeFunction.GetCurrectAngle(agvAngle - firstSectionAngle) == 180);

            if (moveCmdInfo.AddressActions[0] == EnumAddressAction.R2000 || moveCmdInfo.AddressActions[0] == EnumAddressAction.BR2000)
                isLine = true;

            for (int i = 0; i < moveCmdInfo.AddressActions.Count - 1; i++)
            {
                if (moveCmdInfo.AddressActions[i] == EnumAddressAction.R2000 ||
                    moveCmdInfo.AddressActions[i] == EnumAddressAction.BR2000)
                    moveCmdInfo.SectionSpeedLimits.Add(moveControl.moveControlConfig.TurnParameter[EnumAddressAction.R2000].Velocity);
                else
                {
                    if (moveCmdInfo.AddressActions[i] == EnumAddressAction.TR350 ||
                        moveCmdInfo.AddressActions[i] == EnumAddressAction.BTR350 ||
                        moveCmdInfo.AddressActions[i] == EnumAddressAction.TR50 ||
                        moveCmdInfo.AddressActions[i] == EnumAddressAction.BTR50)
                        isLine = !isLine;

                    moveCmdInfo.SectionSpeedLimits.Add(isLine ? double.Parse(tB_LineVelocity.Text) : double.Parse(tB_HorizontalVelocity.Text));
                }
            }
        }

        private void ShowAddressActionsAndVelocitys()
        {
            listCmdAddressActions.Items.Clear();
            listCmdSpeedLimits.Items.Clear();

            for (int i = 0; i < moveCmdInfo.AddressActions.Count - 1; i++)
            {
                listCmdAddressActions.Items.Add(moveCmdInfo.AddressActions[i].ToString());
                listCmdSpeedLimits.Items.Add(moveCmdInfo.SectionSpeedLimits[i].ToString());
            }

            listCmdAddressActions.Items.Add(moveCmdInfo.AddressActions[moveCmdInfo.AddressActions.Count - 1].ToString());
        }

        private void Button_AutoCreate_Click(object sender, EventArgs e)
        {
            try
            {
                if (listCmdAddressActions.Items.Count == 0 || listCmdSpeedLimits.Items.Count == 0)
                {
                    if (listCmdAddressPositions.Items.Count == 0)
                        return;
                    else if (listCmdAddressPositions.Items.Count == 1)
                        button_FromTo_Click(null, null);

                    if (listCmdAddressPositions.Items.Count == 1)
                        return;
                }

                moveCmdInfo = new MoveCmdInfo();
                moveCmdInfo.AddressActions = new List<EnumAddressAction>();
                moveCmdInfo.AddressPositions = new List<MapPosition>();
                moveCmdInfo.SectionSpeedLimits = new List<double>();

                moveCmdInfo.StartAddress = new MapAddress();
                moveCmdInfo.EndAddress = new MapAddress();
                moveCmdInfo.StartAddress.AddressOffset = new MapAddressOffset();
                moveCmdInfo.StartAddress.AddressOffset.OffsetX = 0;
                moveCmdInfo.StartAddress.AddressOffset.OffsetY = 0;
                moveCmdInfo.StartAddress.AddressOffset.OffsetTheta = 0;

                moveCmdInfo.EndAddress.AddressOffset = new MapAddressOffset();
                moveCmdInfo.EndAddress.AddressOffset.OffsetX = 0;
                moveCmdInfo.EndAddress.AddressOffset.OffsetY = 0;
                moveCmdInfo.EndAddress.AddressOffset.OffsetTheta = 0;

                for (int i = 0; i < listCmdAddressPositions.Items.Count; i++)
                {
                    string positionPair = (string)listCmdAddressPositions.Items[i];
                    string[] posXY = positionPair.Split(',');
                    var posX = float.Parse(posXY[0]);
                    var posY = float.Parse(posXY[1]);
                    moveCmdInfo.AddressPositions.Add(new MapPosition(posX, posY));
                }

                moveCmdInfo.MovingAddress = new List<MapAddress>();
                moveCmdInfo.MovingSections = new List<MapSection>();
                MapSection tempMapSection;
                MapAddress tempMapAddress;

                for (int i = 0; i < moveCmdInfo.AddressPositions.Count; i++)
                {
                    tempMapAddress = new MapAddress();
                    tempMapAddress.IsTR50 = IsTR50(moveCmdInfo.AddressPositions[i]);
                    moveCmdInfo.MovingAddress.Add(tempMapAddress);

                    if (i + 1 < moveCmdInfo.AddressPositions.Count)
                    {
                        tempMapSection = new MapSection();
                        if (IsSectionR2000(moveCmdInfo.AddressPositions[i], moveCmdInfo.AddressPositions[i + 1]))
                            tempMapSection.Type = EnumSectionType.R2000;
                        else
                            tempMapSection.Type = EnumSectionType.None;

                        moveCmdInfo.MovingSections.Add(tempMapSection);
                    }
                }

                string errorMessage = "";

                if (moveControl.GetPositionActions(ref moveCmdInfo, ref errorMessage))
                {
                    GetSectionSpeed();
                    ShowAddressActionsAndVelocitys();
                }
                else
                    MessageBox.Show(errorMessage);
            }
            catch { }
        }

        private void tP_Admin_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Location.X > 136 + 80 && e.Location.X < 136 + 80 + 80 &&
                e.Location.Y > 23 && e.Location.Y < 23 + 30)
            {
                hideFunctionOn = !hideFunctionOn;
                cB_OverrideTest.Visible = hideFunctionOn;

                if (!hideFunctionOn)
                    cB_OverrideTest.Checked = false;

                if (hideFunctionOn)
                {
                    if (!initailConfigForm)
                    {
                        initailConfigForm = true;
                        InitailConfigsPage();
                    }

                    tbC_Debug.TabPages.Add(configs);
                    tbC_Debug.TabPages.Add(checkBarcodePositionPage);
                }
                else
                {
                    tbC_Debug.TabPages.RemoveAt(5);
                    tbC_Debug.TabPages.RemoveAt(4);
                }

                button_SimulateState.Visible = (hideFunctionOn ? true : moveControl.SimulationMode);
                if (simulateStateForm != null && !simulateStateForm.IsDisposed)
                    simulateStateForm.ResetHideFunctionOnOff(hideFunctionOn);
                MessageBox.Show("隱藏Function " + (hideFunctionOn ? "開啟!" : "關閉!"));
            }
        }

        private void HideChangeUI()
        {
            cB_ChangeAction.Visible = false;
            tB_ChangeVelocity.Visible = false;
            RunSectionList = new List<MapSection>();
            RunEndAddress = null;
        }

        private void listCmdAddressActions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listCmdAddressActions.IndexFromPoint(e.Location);

            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                EnumAddressAction action = (EnumAddressAction)Enum.Parse(typeof(EnumAddressAction), (string)listCmdAddressActions.Items[index]);

                bool actionCanChange = false;

                switch (action)
                {
                    case EnumAddressAction.TR50:
                    case EnumAddressAction.TR350:
                    case EnumAddressAction.BTR50:
                    case EnumAddressAction.BTR350:
                        cB_ChangeAction.Items.Clear();
                        cB_ChangeAction.Items.Add(EnumAddressAction.TR350.ToString());
                        cB_ChangeAction.Items.Add(EnumAddressAction.BTR350.ToString());
                        cB_ChangeAction.Items.Add(EnumAddressAction.TR50.ToString());
                        cB_ChangeAction.Items.Add(EnumAddressAction.BTR50.ToString());

                        actionCanChange = true;
                        break;
                    default:
                        break;
                }

                if (actionCanChange)
                {
                    int x = listCmdAddressActions.Location.X + listCmdAddressActions.Width / 2 - cB_ChangeAction.Width / 2;
                    int y = listCmdAddressActions.Location.Y + (int)(listCmdAddressActions.ItemHeight * index);
                    cB_ChangeAction.Location = new Point(x, y);

                    cB_ChangeAction.Visible = true;
                }
            }
        }

        private void cB_ChangeAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int index = (cB_ChangeAction.Location.Y - listCmdAddressActions.Location.Y) / listCmdAddressActions.ItemHeight;
                listCmdAddressActions.Items[index] = (string)cB_ChangeAction.SelectedItem;
                cB_ChangeAction.Visible = false;
            }
            catch { }
        }

        private void listCmdSpeedLimits_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listCmdSpeedLimits.IndexFromPoint(e.Location);

            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                if (index < listCmdAddressActions.Items.Count)
                {
                    EnumAddressAction action = (EnumAddressAction)Enum.Parse(typeof(EnumAddressAction), (string)listCmdAddressActions.Items[index]);
                    if (action == EnumAddressAction.R2000)
                        return;
                }

                int x = listCmdSpeedLimits.Location.X + listCmdSpeedLimits.Width / 2 - tB_ChangeVelocity.Width / 2;
                int y = listCmdSpeedLimits.Location.Y + (int)(listCmdSpeedLimits.ItemHeight * index);
                tB_ChangeVelocity.Location = new Point(x, y);

                tB_ChangeVelocity.Visible = true;
                tB_ChangeVelocity.Focus();
            }
        }

        private void tB_ChangeVelocity_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if ((e.KeyChar >= (char)48 && e.KeyChar <= (char)57) || (e.KeyChar == (char)8))
                    e.Handled = false;
                else if (e.KeyChar == (char)13)
                {
                    if (Int32.Parse(tB_ChangeVelocity.Text) < moveControl.moveControlConfig.EQ.Velocity)
                    {
                        MessageBox.Show("速度請勿設定低於EQ速度(80mm/s)!");
                        return;
                    }

                    int index = (tB_ChangeVelocity.Location.Y - listCmdSpeedLimits.Location.Y) / listCmdSpeedLimits.ItemHeight;
                    listCmdSpeedLimits.Items[index] = tB_ChangeVelocity.Text;
                    tB_ChangeVelocity.Visible = false;
                    e.Handled = true;
                    tB_ChangeVelocity.Text = "";
                }
                else
                    e.Handled = true;
            }
            catch { }
        }

        private void tB_LineVelocity_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if ((e.KeyChar >= (char)48 && e.KeyChar <= (char)57) || (e.KeyChar == (char)8))
                    e.Handled = false;
                else if (e.KeyChar == (char)13)
                {
                    if (Int32.Parse(tB_LineVelocity.Text) < moveControl.moveControlConfig.EQ.Velocity)
                    {
                        MessageBox.Show("速度請勿設定低於EQ速度(80mm/s)!");
                        tB_LineVelocity.Text = "400";
                    }
                    else
                    {
                        if (listCmdAddressActions.Items.Count != 0 && listCmdSpeedLimits.Items.Count != 0)
                            GetSectionSpeed();

                        ShowAddressActionsAndVelocitys();
                    }
                }
                else
                    e.Handled = true;
            }
            catch { }
        }

        private void tB_HorizontalVelocity_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if ((e.KeyChar >= (char)48 && e.KeyChar <= (char)57) || (e.KeyChar == (char)8))
                    e.Handled = false;
                else if (e.KeyChar == (char)13)
                {
                    if (Int32.Parse(tB_HorizontalVelocity.Text) < moveControl.moveControlConfig.EQ.Velocity)
                    {
                        MessageBox.Show("速度請勿設定低於EQ速度(80mm/s)!");
                        tB_HorizontalVelocity.Text = "400";
                    }
                    else
                    {
                        if (listCmdAddressActions.Items.Count != 0 && listCmdSpeedLimits.Items.Count != 0)
                            GetSectionSpeed();

                        ShowAddressActionsAndVelocitys();
                    }
                }
                else
                    e.Handled = true;
            }
            catch { }
        }

        private void tB_RunTimes_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= (char)48 && e.KeyChar <= (char)57) || (e.KeyChar == (char)8))
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void ProcessRunTimes(int runTimes)
        {
            int count = listCmdAddressPositions.Items.Count;

            if (runTimes <= 0)
            {
                MessageBox.Show("請輸入 > 0 的數字!");
                return;
            }

            if ((count - 1) * runTimes + 1 > 200)
            {
                MessageBox.Show("不要亂玩, 程式會當掉!");
                return;
            }

            if (count < 2)
                return;

            bool reverse = true;

            for (int i = 1; i < runTimes; i++)
            {
                if (reverse)
                    for (int j = count - 2; j >= 0; j--)
                        listCmdAddressPositions.Items.Add(listCmdAddressPositions.Items[j]);
                else
                    for (int j = 1; j < count; j++)
                        listCmdAddressPositions.Items.Add(listCmdAddressPositions.Items[j]);

                reverse = !reverse;
            }

            ResetListAndMoveCommandInfo();
        }

        private void button_RunTimesEven_Click(object sender, EventArgs e)
        {
            int runTimes = Int32.Parse(tB_RunTimes.Text);
            ProcessRunTimes(runTimes * 2);
        }

        private void button_RunTimesOdd_Click(object sender, EventArgs e)
        {
            int runTimes = Int32.Parse(tB_RunTimes.Text);
            ProcessRunTimes(runTimes);
        }

        private void button_ReveseAddressList_Click(object sender, EventArgs e)
        {
            int count = listCmdAddressPositions.Items.Count;

            if (count < 2)
                return;

            for (int i = count - 1; i >= 0; i--)
                listCmdAddressPositions.Items.Add(listCmdAddressPositions.Items[i]);

            for (int i = 0; i < count; i++)
                listCmdAddressPositions.Items.RemoveAt(0);

            ResetListAndMoveCommandInfo();
        }


        private string GetAddressID(MapPosition now)
        {
            string address = "";
            MapPosition tempPosition = null;

            foreach (var valuePair in theMapInfo.allMapAddresses)
            {
                if (Math.Abs(now.X - valuePair.Value.Position.X) <= 30 &&
                    Math.Abs(now.Y - valuePair.Value.Position.Y) <= 30)
                {
                    if (tempPosition == null || Math.Abs(Math.Pow(valuePair.Value.Position.X - now.X, 2) + Math.Pow(valuePair.Value.Position.Y - now.Y, 2)) <
                                                Math.Abs(Math.Pow(tempPosition.X - now.X, 2) + Math.Pow(tempPosition.Y - now.Y, 2)))
                    {
                        tempPosition = valuePair.Value.Position;
                        address = valuePair.Value.Id;
                    }
                }
            }

            return address;
        }

        private string GetSectionID(MapPosition now)
        {
            foreach (var valuePair in theMapInfo.allMapSections)
            {
                if (valuePair.Value.HeadAddress.Position.X == valuePair.Value.TailAddress.Position.X &&
                     Math.Abs(valuePair.Value.HeadAddress.Position.X - now.X) <= 30)
                {
                    if ((valuePair.Value.HeadAddress.Position.Y > valuePair.Value.TailAddress.Position.Y &&
                        valuePair.Value.HeadAddress.Position.Y + 30 > now.Y && now.Y > valuePair.Value.TailAddress.Position.Y - 30) ||
                        (valuePair.Value.HeadAddress.Position.Y < valuePair.Value.TailAddress.Position.Y &&
                        valuePair.Value.HeadAddress.Position.Y + 30 < now.Y && now.Y < valuePair.Value.TailAddress.Position.Y - 30))
                        return valuePair.Value.Id;
                }
                else if (valuePair.Value.HeadAddress.Position.Y == valuePair.Value.TailAddress.Position.Y &&
                         Math.Abs(valuePair.Value.HeadAddress.Position.Y - now.Y) <= 30)
                {
                    if ((valuePair.Value.HeadAddress.Position.X > valuePair.Value.TailAddress.Position.X &&
                        valuePair.Value.HeadAddress.Position.X + 30 > now.X && now.X > valuePair.Value.TailAddress.Position.X - 30) ||
                        (valuePair.Value.HeadAddress.Position.X < valuePair.Value.TailAddress.Position.X &&
                        valuePair.Value.HeadAddress.Position.X + 30 < now.X && now.X < valuePair.Value.TailAddress.Position.X - 30))
                        return valuePair.Value.Id;
                }
            }

            return "";
        }

        private bool GetNowAGVLocation(MapPosition now, ref string address, ref string section)
        {
            address = GetAddressID(now);

            if (address == "")
            {
                section = GetSectionID(now);

                if (section == "")
                    return false;
                else
                    return true;
            }
            else
                return true;
        }

        private void FindAGVFromToPath(string endAddress, List<string> nodeList, double nowDistance, ref List<string> minNodeList, ref double minDistance, List<MapSection> tempSectionList, ref List<MapSection> mapSectionList)
        {
            if (endAddress == nodeList[nodeList.Count - 1])
            {
                if (minDistance == -1 || minDistance > nowDistance)
                {
                    minNodeList = new List<string>();
                    for (int i = 0; i < nodeList.Count; i++)
                        minNodeList.Add(nodeList[i]);

                    mapSectionList = new List<MapSection>();
                    for (int i = 0; i < tempSectionList.Count; i++)
                        mapSectionList.Add(tempSectionList[i]);

                    minDistance = nowDistance;
                }
            }
            else
            {
                if (minDistance == -1 || nowDistance < minDistance)
                {
                    bool notRepeat;
                    string nextNode;

                    foreach (var valuePair in theMapInfo.allMapSections)
                    {
                        nextNode = "";

                        if (nodeList[nodeList.Count - 1] == valuePair.Value.HeadAddress.Id)
                            nextNode = valuePair.Value.TailAddress.Id;
                        else if (nodeList[nodeList.Count - 1] == valuePair.Value.TailAddress.Id)
                            nextNode = valuePair.Value.HeadAddress.Id;

                        if (nextNode != "")
                        {
                            notRepeat = true;

                            for (int i = 0; i < nodeList.Count && notRepeat; i++)
                            {
                                if (nodeList[i] == nextNode)
                                    notRepeat = false;
                            }

                            if (notRepeat)
                            {
                                nodeList.Add(nextNode);
                                tempSectionList.Add(valuePair.Value);
                                FindAGVFromToPath(endAddress, nodeList, nowDistance + valuePair.Value.HeadToTailDistance / valuePair.Value.Speed, ref minNodeList, ref minDistance, tempSectionList, ref mapSectionList);
                                nodeList.RemoveAt(nodeList.Count - 1);
                                tempSectionList.RemoveAt(tempSectionList.Count - 1);
                            }
                        }
                    }
                }
            }
        }

        private void button_FromTo_Click(object sender, EventArgs e)
        {
            try
            {
                List<MapSection> SectionList = new List<MapSection>();
                List<MapSection> tempSectionList = new List<MapSection>();
                string address = "";
                string section = "";
                MapPosition endPosition;
                double distance;
                double x, y;
                string txtPosition;

                List<double> startAddressToSectionNodeDistance = new List<double>();
                List<string> startSectionNode = new List<string>();

                string endAddressID = "";
                List<double> endAddressToSectionNodeDistance = new List<double>();
                List<string> endSectionNode = new List<string>();

                if (listCmdAddressPositions.Items.Count != 1)
                {
                    MessageBox.Show("Address數量應為1個!");
                    return;
                }

                if (moveControl.location.Real == null)
                {
                    MessageBox.Show("迷航中!");
                    return;
                }

                MapPosition now = moveControl.location.Real.Position;

                if (!GetNowAGVLocation(now, ref address, ref section))
                {
                    MessageBox.Show("和任何Address或Section接差距超過30mm,找不到目前所在位置!");
                    return;
                }

                if (address != "")
                {
                    if (theMapInfo.allMapSections.ContainsKey(theMapInfo.allMapAddresses[address].InsideSectionId))
                    {
                        section = theMapInfo.allMapAddresses[address].InsideSectionId;
                        address = "";
                    }
                }

                if (section != "")
                {
                    SectionList.Add(theMapInfo.allMapSections[section]);
                    distance = Math.Sqrt(Math.Pow(now.X - theMapInfo.allMapSections[section].HeadAddress.Position.X, 2) +
                                         Math.Pow(now.Y - theMapInfo.allMapSections[section].HeadAddress.Position.Y, 2));

                    startAddressToSectionNodeDistance.Add(distance / theMapInfo.allMapSections[section].Speed);
                    startSectionNode.Add(theMapInfo.allMapSections[section].HeadAddress.Id);

                    distance = Math.Sqrt(Math.Pow(now.X - theMapInfo.allMapSections[section].TailAddress.Position.X, 2) +
                                         Math.Pow(now.Y - theMapInfo.allMapSections[section].TailAddress.Position.Y, 2));

                    startAddressToSectionNodeDistance.Add(distance / theMapInfo.allMapSections[section].Speed);
                    startSectionNode.Add(theMapInfo.allMapSections[section].TailAddress.Id);
                }
                else
                {
                    startAddressToSectionNodeDistance.Add(0);
                    startSectionNode.Add(address);
                }

                string positionPair = (string)listCmdAddressPositions.Items[0];
                string[] posXY = positionPair.Split(',');
                endPosition = new MapPosition(double.Parse(posXY[0]), double.Parse(posXY[1]));

                foreach (var valuePair in theMapInfo.allMapAddresses)
                {
                    if ((int)valuePair.Value.Position.X == (int)endPosition.X &&
                        (int)valuePair.Value.Position.Y == (int)endPosition.Y)
                    {
                        endAddressID = valuePair.Value.Id;
                        break;
                    }
                }

                if (endAddressID == "")
                {
                    MessageBox.Show("GG double 轉成 string 再轉回 double 資料不同了!");
                    return;
                }

                if (theMapInfo.allMapSections.ContainsKey(theMapInfo.allMapAddresses[endAddressID].InsideSectionId))
                {
                    if (theMapInfo.allMapAddresses[endAddressID].InsideSectionId == section)
                    {
                        listCmdAddressPositions.Items.Clear();

                        if (theMapInfo.allMapSections[section].TailAddress.Position.X == theMapInfo.allMapSections[section].HeadAddress.Position.X)
                        {
                            x = theMapInfo.allMapSections[section].TailAddress.Position.X;
                            y = now.Y;
                        }
                        else
                        {
                            x = now.X;
                            y = theMapInfo.allMapSections[section].TailAddress.Position.Y;
                        }

                        txtPosition = $"{x.ToString()},{y.ToString()}";
                        listCmdAddressPositions.Items.Add(txtPosition);

                        x = endPosition.X;
                        y = endPosition.Y;

                        txtPosition = $"{x.ToString()},{y.ToString()}";
                        listCmdAddressPositions.Items.Add(txtPosition);

                        RunEndAddress = theMapInfo.allMapAddresses[endAddressID];
                        SectionList = new List<MapSection>();
                        SectionList.Add(theMapInfo.allMapSections[section]);
                        RunSectionList = SectionList;
                        return;
                    }

                    distance = Math.Sqrt(Math.Pow(endPosition.X - theMapInfo.allMapSections[theMapInfo.allMapAddresses[endAddressID].InsideSectionId].HeadAddress.Position.X, 2) +
                                         Math.Pow(endPosition.Y - theMapInfo.allMapSections[theMapInfo.allMapAddresses[endAddressID].InsideSectionId].HeadAddress.Position.Y, 2));

                    endAddressToSectionNodeDistance.Add(distance / theMapInfo.allMapSections[theMapInfo.allMapAddresses[endAddressID].InsideSectionId].Speed);
                    endSectionNode.Add(theMapInfo.allMapSections[theMapInfo.allMapAddresses[endAddressID].InsideSectionId].HeadAddress.Id);

                    distance = Math.Sqrt(Math.Pow(endPosition.X - theMapInfo.allMapSections[theMapInfo.allMapAddresses[endAddressID].InsideSectionId].TailAddress.Position.X, 2) +
                                         Math.Pow(endPosition.Y - theMapInfo.allMapSections[theMapInfo.allMapAddresses[endAddressID].InsideSectionId].TailAddress.Position.Y, 2));

                    endAddressToSectionNodeDistance.Add(distance / theMapInfo.allMapSections[theMapInfo.allMapAddresses[endAddressID].InsideSectionId].Speed);
                    endSectionNode.Add(theMapInfo.allMapSections[theMapInfo.allMapAddresses[endAddressID].InsideSectionId].TailAddress.Id);
                }
                else
                {
                    endAddressToSectionNodeDistance.Add(0);
                    endSectionNode.Add(endAddressID);
                }

                List<string> nodeList = new List<string>();
                List<string> minNodeList = new List<string>();
                double minDistance = -1;

                for (int i = 0; i < startSectionNode.Count; i++)
                {
                    for (int j = 0; j < endSectionNode.Count; j++)
                    {
                        nodeList.Add(startSectionNode[i]);
                        FindAGVFromToPath(endSectionNode[j], nodeList, startAddressToSectionNodeDistance[i] + endAddressToSectionNodeDistance[j], ref minNodeList, ref minDistance, tempSectionList, ref SectionList);
                        nodeList = new List<string>();
                    }
                }

                if (minDistance == -1)
                    MessageBox.Show("找不到路徑!");
                else
                {
                    listCmdAddressPositions.Items.Clear();

                    if (section != "" || address != minNodeList[0])
                    {
                        if (theMapInfo.allMapSections[section].TailAddress.Position.X == theMapInfo.allMapSections[section].HeadAddress.Position.X)
                        {
                            x = theMapInfo.allMapSections[section].TailAddress.Position.X;
                            y = now.Y;
                        }
                        else
                        {
                            x = now.X;
                            y = theMapInfo.allMapSections[section].TailAddress.Position.Y;
                        }

                        txtPosition = $"{x.ToString()},{y.ToString()}";
                        listCmdAddressPositions.Items.Add(txtPosition);
                    }

                    for (int i = 0; i < minNodeList.Count; i++)
                    {
                        x = theMapInfo.allMapAddresses[minNodeList[i]].Position.X;
                        y = theMapInfo.allMapAddresses[minNodeList[i]].Position.Y;
                        txtPosition = $"{x.ToString()},{y.ToString()}";
                        listCmdAddressPositions.Items.Add(txtPosition);
                    }

                    if (endAddressID != minNodeList[minNodeList.Count - 1])
                    {
                        x = theMapInfo.allMapAddresses[endAddressID].Position.X;
                        y = theMapInfo.allMapAddresses[endAddressID].Position.Y;
                        txtPosition = $"{x.ToString()},{y.ToString()}";
                        listCmdAddressPositions.Items.Add(txtPosition);
                    }

                    ResetListAndMoveCommandInfo();

                    if (section != "")
                        SectionList.Insert(0, theMapInfo.allMapSections[section]);

                    if (theMapInfo.allMapSections.ContainsKey(theMapInfo.allMapAddresses[endAddressID].InsideSectionId))
                        SectionList.Add(theMapInfo.allMapSections[theMapInfo.allMapAddresses[endAddressID].InsideSectionId]);

                    RunEndAddress = theMapInfo.allMapAddresses[endAddressID];
                    RunSectionList = SectionList;
                }
            }
            catch { }
        }

        private void ElmoGTMove(double angle)
        {
            moveControl.elmoDriver.ElmoMove(EnumAxis.GT, angle, moveControl.moveControlConfig.Turn.Velocity, EnumMoveType.Absolute, moveControl.moveControlConfig.Turn.Acceleration, moveControl.moveControlConfig.Turn.Deceleration, moveControl.moveControlConfig.Turn.Jerk);
        }

        private void button_GT0_Click(object sender, EventArgs e)
        {
            ElmoGTMove(0);
        }

        private void button_GTLeft_Click(object sender, EventArgs e)
        {
            ElmoGTMove(90);
        }

        private void button_GTRight_Click(object sender, EventArgs e)
        {
            ElmoGTMove(-90);
        }

        private void button_GTMove_Click(object sender, EventArgs e)
        {
            try
            {
                ElmoGTMove(double.Parse(tB_GTMoveAngle.Text));
            }
            catch { }
        }

        private void button_CheckTRStartStop_Click(object sender, EventArgs e)
        {
            if (button_CheckTRStartStop.Text == "開始")
            {

            }
            else
            {

            }
        }


        #region Congis
        private void InitailConfigsMoveControlPage()
        {

            SafetyInformation temp;
            int x = 30;
            int y = 80;

            foreach (EnumMoveControlSafetyType item in (EnumMoveControlSafetyType[])Enum.GetValues(typeof(EnumMoveControlSafetyType)))
            {
                temp = new SafetyInformation(moveControl, item);
                temp.Location = new System.Drawing.Point(x, y);
                y += 40;
                temp.Name = item.ToString();
                temp.Size = new System.Drawing.Size(647, 30);

                switch (item)
                {
                    case EnumMoveControlSafetyType.TurnOut:
                        temp.SetLabelString("出彎保護 : ", "出彎多久內必須讀到Barcode :");
                        break;
                    case EnumMoveControlSafetyType.LineBarcodeInterval:
                        temp.SetLabelString("直線保護 : ", "直線Barcode最大間隔 :");
                        break;
                    case EnumMoveControlSafetyType.OntimeReviseTheta:
                        temp.SetLabelString("角度偏差 : ", "容許Theta偏差量 :");
                        break;
                    case EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine:
                        temp.SetLabelString("直線偏差 : ", "容許直線軌道偏差量 :");
                        break;
                    case EnumMoveControlSafetyType.OntimeReviseSectionDeviationHorizontal:
                        temp.SetLabelString("橫移偏差 : ", "容許橫移軌道偏差量 :");
                        break;
                    case EnumMoveControlSafetyType.OneTimeRevise:
                        temp.SetLabelString("一次修正 : ", "一次性修正距離,多少會修完 :");
                        break;
                    case EnumMoveControlSafetyType.VChangeSafetyDistance:
                        temp.SetLabelString("降速保護 : ", "多少距離檢查一次速度變化 :");
                        break;
                    case EnumMoveControlSafetyType.TRPathMonitoring:
                        temp.SetLabelString("監控TR軌跡 : ", "角度允許誤差 :");
                        break;
                    case EnumMoveControlSafetyType.IdleNotWriteLog:
                        temp.SetLabelString("Idle不Log : ", "移動完成後再多記多少ms : ");
                        break;
                    case EnumMoveControlSafetyType.BarcodePositionSafety:
                        temp.SetLabelString("Barcode保護 : ", "Config (mm) : ");
                        break;
                    case EnumMoveControlSafetyType.StopWithoutReason:
                        temp.SetLabelString("默停偵測 : ", "Config (ms) : ");
                        break;
                    case EnumMoveControlSafetyType.BeamSensorR2000:
                        temp.SetLabelString("Beam R2000 : ", "delay (ms) : ");
                        break;
                    default:
                        break;
                }

                temp.UpdateEnableRange();
                this.tC_Configs.TabPages[0].Controls.Add(temp);
            }

        }

        private void InitailConfigsPage()
        {
            ConfigsNameAndValue temp;
            AxisConfigs tempAxis;

            int intailX = 15;
            int intailY = 20;
            int deltaX = 415;
            int deltaY = 35;

            int x = intailX;
            int y = intailY;

            int axisX = x + 2 * deltaX;
            int axisY = intailY;

            int axisDeltaY = 32;

            int formHeight = tC_Configs.Location.Y + tC_Configs.Size.Height - 2 * deltaY;

            foreach (PropertyInfo propertyInfo in moveControl.moveControlConfig.GetType().GetProperties())
            {
                if (propertyInfo.Name == "Move")
                    ;

                switch (propertyInfo.PropertyType.Name)
                {
                    case "Int32":
                    case "Int16":
                    case "Double":
                    case "Boolean":
                    case "String":
                        temp = new ConfigsNameAndValue(propertyInfo.Name, propertyInfo.PropertyType.Name, moveControl.moveControlConfig);
                        temp.Location = new System.Drawing.Point(x, y);

                        y += deltaY;
                        if (y > formHeight)
                        {
                            y = intailY;
                            x += deltaX;
                        }

                        this.tC_Configs.TabPages[0].Controls.Add(temp);
                        break;
                    case "AxisData":
                        {
                            //tempAxis = new AxisConfigs(propertyInfo.Name, moveControl.moveControlConfig);
                            //tempAxis.Location = new System.Drawing.Point(AxisX, AxisY);
                            //AxisY += tempAxis.Size.Height + 5;
                            //this.tC_Configs.TabPages[0].Controls.Add(tempAxis);
                        }
                        try
                        {
                            Label labelName = new Label();
                            labelName.Location = new System.Drawing.Point(axisX, axisY);
                            labelName.Text = propertyInfo.Name;
                            labelName.Font = new System.Drawing.Font("新細明體", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));

                            this.tC_Configs.TabPages[0].Controls.Add(labelName);
                            axisY += axisDeltaY;


                            object axis = moveControl.moveControlConfig.GetType().GetProperty(propertyInfo.Name).GetValue(moveControl.moveControlConfig, null);

                            foreach (PropertyInfo axisPropertyInfo in axis.GetType().GetProperties())
                            {
                                if ((double)axis.GetType().GetProperty(axisPropertyInfo.Name).GetValue(axis, null) != 0)
                                {
                                    temp = new ConfigsNameAndValue(axisPropertyInfo.Name, "Double", axis);
                                    temp.Location = new System.Drawing.Point(axisX, axisY);
                                    axisY += axisDeltaY;
                                    this.tC_Configs.TabPages[0].Controls.Add(temp);
                                }

                            }

                            axisY += 5;
                        }
                        catch { }


                        break;
                    default:
                        ;
                        break;
                }

                //moveControl.moveControlConfig.GetType().GetProperty(propertyInfo.Name).Get();
                // do stuff here
            }
        }
        #endregion

        private void button_RetryMove_Click(object sender, EventArgs e)
        {
            moveControl.TransferMove_RetryMove();
        }
    }
}