using Mirle.Agv.Controller;
using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Mirle.Agv.View
{
    public partial class MoveCommandDebugModeForm : Form
    {
        private MoveControlHandler moveControl;
        private MoveCommandData command;
        private MapInfo theMapInfo = new MapInfo();
        private MoveCmdInfo moveCmdInfo = new MoveCmdInfo();

        private List<string> commandStringList = new List<string>();
        private List<string> reserveStringList = new List<string>();
        private int reserveIndex;
        private int commandIndex;
        private AGVPosition tempReal = null;
        private int firstSelect;
        private int secondSelect;
        private string commandID = "";
        private bool simulationModeFirstDoubleClick = false;
        private Dictionary<EnumMoveControlSafetyType, SafetyInformation> safetyUserControl = new Dictionary<EnumMoveControlSafetyType, SafetyInformation>();
        private Dictionary<EnumSensorSafetyType, SensorByPassInformation> sensorByPassUserControl = new Dictionary<EnumSensorSafetyType, SensorByPassInformation>();

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
        }

        private void MoveCommandMonitor_Load(object sender, EventArgs e)
        {
            button_SendList.Enabled = false;
            ucLabelTB_RealEncoder.TagName = "Real Encoder : ";
            ucLabelTB_RealPosition.TagName = "Real Position : ";
            ucLabelTB_BarcodePosition.TagName = "Barcode Position : ";
            ucLabelTB_Delta.TagName = "Encoder Delta : ";
            ucLabelTB_CreateCommand_BarcodePosition.TagName = "Barcode Position : ";
            ucLabelTtB_CommandListState.TagName = "AGV Move State : ";
            ucLabelTB_CreateCommandState.TagName = "AGV Move State : ";
            ucLabelTB_Velocity.TagName = "Velocity : ";
            ucLabelTB_ElmoEncoder.TagName = "Elmo encoder : ";
            ucLabelTB_EncoderOffset.TagName = "Offset : ";
            ucLabelTB_EncoderPosition.TagName = "VelocityCmd : ";
            AddListMapAddressPositions();
            AddListMapAddressActions();
            AddListMapSpeedLimits();
            AddDataGridViewColumn();
            AddAdminSafetyUserControl();
            moveControl.DebugFlowMode = true;
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
                    case EnumMoveControlSafetyType.UpdateDeltaPositionRange:
                        temp.SetLabelString("更新偏差 : ", "容許Barcode和目前位置偏差 :");
                        break;
                    case EnumMoveControlSafetyType.OneTimeRevise:
                        temp.SetLabelString("一次修正 : ", "一次性修正距離,多少會修完 :");
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
                    case EnumSensorSafetyType.BeamSensorR2000:
                        tempSensor.SetLabelString("Beam R2000 : ");
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
                    case EnumSensorSafetyType.TRPathMonitoring:
                        tempSensor.SetLabelString("監控TR軌跡 : ");
                        break;
                    default:
                        break;
                }

                tempSensor.UpdateEnable();
                this.tP_Admin.Controls.Add(tempSensor);
                sensorByPassUserControl.Add(item, tempSensor);
            }
        }

        private void AddDataGridViewColumn()
        {
            System.Windows.Forms.DataGridViewTextBoxColumn[][] AxisColumn = new DataGridViewTextBoxColumn[18][];

            for (int i = 0; i < 8; i++)
            {
                AxisColumn[i] = new DataGridViewTextBoxColumn[8];
                for (int j = 0; j < 8; j++)
                    AxisColumn[i][j] = new DataGridViewTextBoxColumn();
                //  count   position	velocity	toc	disable	moveComplete	error
                AxisColumn[i][0].HeaderText = AxisList[i].ToString();
                AxisColumn[i][0].Name = AxisList[i].ToString();
                AxisColumn[i][1].HeaderText = "Position";
                AxisColumn[i][1].Name = AxisList[i].ToString() + "Position";
                AxisColumn[i][2].HeaderText = "Velocity";
                AxisColumn[i][2].Name = AxisList[i].ToString() + "Velocity";
                AxisColumn[i][3].HeaderText = "ErrorPosition";
                AxisColumn[i][3].Name = AxisList[i].ToString() + "ErrorPosition";
                AxisColumn[i][4].HeaderText = "toc";
                AxisColumn[i][4].Name = AxisList[i].ToString() + "toc";
                AxisColumn[i][5].HeaderText = "Disable";
                AxisColumn[i][5].Name = AxisList[i].ToString() + "Disable";
                AxisColumn[i][6].HeaderText = "Complete";
                AxisColumn[i][6].Name = AxisList[i].ToString() + "Complete";
                AxisColumn[i][7].HeaderText = "Error";
                AxisColumn[i][7].Name = AxisList[i].ToString() + "Error";
            }

            for (int i = 8; i < 16; i++)
            {
                AxisColumn[i] = new DataGridViewTextBoxColumn[5];
                for (int j = 0; j < 5; j++)
                    AxisColumn[i][j] = new DataGridViewTextBoxColumn();
                //  count   position		disable	moveComplete	error
                AxisColumn[i][0].HeaderText = AxisList[i].ToString();
                AxisColumn[i][0].Name = AxisList[i].ToString();
                AxisColumn[i][1].HeaderText = "Position";
                AxisColumn[i][1].Name = AxisList[i].ToString() + "Position";
                AxisColumn[i][2].HeaderText = "Disable";
                AxisColumn[i][2].Name = AxisList[i].ToString() + "Disable";
                AxisColumn[i][3].HeaderText = "Complete";
                AxisColumn[i][3].Name = AxisList[i].ToString() + "Complete";
                AxisColumn[i][4].HeaderText = "Error";
                AxisColumn[i][4].Name = AxisList[i].ToString() + "Error";
            }

            for (int i = 16; i < 18; i++)
            {
                AxisColumn[i] = new DataGridViewTextBoxColumn[1];
                // 		disable	moveComplete
                AxisColumn[i][0] = new DataGridViewTextBoxColumn();
                AxisColumn[i][0].HeaderText = AxisList[i].ToString() + "Complete";
                AxisColumn[i][0].Name = AxisList[i].ToString() + "Complete";
            }

            for (int i = 0; i < 18; i++)
            {
                for (int j = 0; j < AxisColumn[i].Count(); j++)
                    this.dataGridView_CSVList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { AxisColumn[i][j] });
            }
        }

        private void AddListMapAddressPositions()
        {
            listMapAddressPositions.Items.Clear();
            foreach (var valuePair in theMapInfo.allMapAddresses)
            {
                MapAddress mapAddress = valuePair.Value;
                MapPosition mapPosition = mapAddress.Position.DeepClone();
                string txtPosition = $"{mapPosition.X},{mapPosition.Y}";
                listMapAddressPositions.Items.Add(txtPosition);
            }
        }

        private void AddListMapAddressActions()
        {
            listMapAddressActions.Items.Clear();
            foreach (string item in Enum.GetNames(typeof(EnumAddressAction)))
            {
                listMapAddressActions.Items.Add(item);
            }
        }

        private void AddListMapSpeedLimits()
        {
            listMapSpeedLimits.Items.Clear();
            Dictionary<double, short> dicSpeedLimits = new Dictionary<double, short>();
            foreach (var valuePair in theMapInfo.allMapSections)
            {
                MapSection mapSection = valuePair.Value;
                double speedLimit = mapSection.Speed;
                if (!dicSpeedLimits.ContainsKey(speedLimit))
                {
                    dicSpeedLimits.Add(speedLimit, 1);
                    listMapSpeedLimits.Items.Add(speedLimit);
                }
            }
        }

        private void MoveCommandDebugMode_Leave(object sender, EventArgs e)
        {
            moveControl.DebugCSVMode = false;
        }
        #endregion

        #region timer Update
        private void Timer_Update_CreateCommand()
        {
            AGVPosition tempBarcodePosition = moveControl.location.Barcode;

            if (tempBarcodePosition != null)
            {
                ucLabelTB_CreateCommand_BarcodePosition.TagValue = "( " + tempBarcodePosition.Position.X.ToString("0") + ", " +
                                                                         tempBarcodePosition.Position.Y.ToString("0") + " )";
            }
            else
                ucLabelTB_CreateCommand_BarcodePosition.TagValue = "( ---, --- )";

            ucLabelTB_CreateCommandState.TagValue = moveControl.MoveState.ToString();

            bool buttonEnable = true;
            string lockResult = "";

            if (Vehicle.Instance.AutoState != EnumAutoState.Manual)
            {
                buttonEnable = false;
                lockResult = "Lock Result : AutoMode中!";
            }
            else if (Vehicle.Instance.VisitTransferStepsStatus != EnumThreadStatus.None &&
                     Vehicle.Instance.VisitTransferStepsStatus != EnumThreadStatus.Stop)
            {
                buttonEnable = false;
                lockResult = "Lock Result : 主流程動作中!";
            }
            else if (moveControl.MoveState != EnumMoveState.Idle)
            {
                buttonEnable = false;
                lockResult = "Lock Result : MoveState動作中!";
            }
            else if (moveControl.IsCharging())
            {
                buttonEnable = false;
                lockResult = "Lock Result : Charging中!";
            }
            else if (moveControl.ForkNotHome())
            {
                buttonEnable = false;
                lockResult = "Lock Result : Fork不在Home點!";
            }

            button_DebugModeSend.Enabled = buttonEnable;
            label_LockResult.Text = lockResult;
        }

        private void UpdateList()
        {
            ShowReserveList(null);
            ShowMoveCommandList(null);
        }

        private void Timer_Update_CommandList()
        {
            string nowCommandID = moveControl.MoveCommandID;
            if (nowCommandID != commandID || (moveControl.MoveState != EnumMoveState.Idle && moveControl.command.CommandList.Count != commandStringList.Count))
            {
                UpdateList();
                commandID = nowCommandID;
                label_MoveCommandID.Text = commandID;
            }

            AGVPosition tempBarcodePosition = moveControl.location.Barcode;

            label_AlarmMessage.Text = moveControl.AGVStopResult;

            ucLabelTB_RealEncoder.TagValue = moveControl.location.RealEncoder.ToString("0.0");
            ucLabelTB_Delta.TagValue = moveControl.location.Delta.ToString("0.0");
            ucLabelTB_Velocity.TagValue = moveControl.location.XFLVelocity.ToString("0");
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

            label_WaitReserve.Text = "Wait index : " + (moveControl.WaitReseveIndex == -1 ? "" : moveControl.WaitReseveIndex.ToString());
            label_SensorState.Text = moveControl.ControlData.SensorState.ToString();

            label_LastIdealTime.Text = moveControl.autoTime;
            label_LastIErrorTime.Text = moveControl.errorTime;

            try
            {
                int tempResserveIndex = moveControl.GetReserveIndex();
                int tempCommandIndex = moveControl.command.IndexOfCmdList;

                if (tempResserveIndex > reserveIndex)
                {
                    for (int i = reserveIndex; i < tempResserveIndex; i++)
                        ReserveList.Items[i] = "▶ " + reserveStringList[i];

                    reserveIndex = tempResserveIndex;
                }

                if (tempCommandIndex != -1 && tempCommandIndex > commandIndex)
                {
                    for (int i = commandIndex; i < tempCommandIndex; i++)
                        CommandList.Items[i] = "▶ " + commandStringList[i];

                    commandIndex = tempCommandIndex;
                }
            }
            catch
            {
            }
        }

        private void Timer_Update_Debug()
        {
        }

        private void Timer_Update_DebugCSV()
        {
            List<string[]> bufferList;

            lock (moveControl.deubgCsvLogList)
            {
                bufferList = moveControl.deubgCsvLogList;
                moveControl.deubgCsvLogList = new List<string[]>();
            }

            for (int i = 0; i < bufferList.Count; i++)
                dataGridView_CSVList.Rows.Add(bufferList[i].ToArray());

            while (bufferList.Count > 3000)
                dataGridView_CSVList.Rows.RemoveAt(0);
        }

        private void Timer_Update_Admin()
        {
            button_SimulationModeChange.Text = moveControl.SimulationMode ? "開啟中" : "關閉中";
            button_SimulationModeChange.BackColor = moveControl.SimulationMode ? Color.Red : Color.Transparent;

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
                Timer_Update_Debug();
            else if (tbC_Debug.SelectedIndex == 3)
                Timer_Update_DebugCSV();
            else if (tbC_Debug.SelectedIndex == 4)
                Timer_Update_Admin();

            tbxLogView_MoveControlDebugMessage.Text = moveControl.DebugFlowLog;
        }
        #endregion

        #region Page Create Command

        private void btnAddAddressPosition_Click(object sender, EventArgs e)
        {
            if (listMapAddressPositions.Items.Count < 1)
                return;
            if (listMapAddressPositions.SelectedIndex < 0)
                listMapAddressPositions.SelectedIndex = 0;

            listCmdAddressPositions.Items.Add(listMapAddressPositions.SelectedItem);
        }

        private void btnRemoveLastAddressPosition_Click(object sender, EventArgs e)
        {
            if (listCmdAddressPositions.Items.Count < 1)
                return;
            if (listCmdAddressPositions.SelectedIndex < 0)
                listCmdAddressPositions.SelectedIndex = listCmdAddressPositions.Items.Count - 1;

            listCmdAddressPositions.Items.RemoveAt(listCmdAddressPositions.SelectedIndex);
        }

        private void btnAddressPositionsClear_Click(object sender, EventArgs e)
        {
            listCmdAddressPositions.Items.Clear();
        }

        private void btnAddAddressAction_Click(object sender, EventArgs e)
        {
            if (listMapAddressActions.Items.Count < 1)
                return;
            if (listMapAddressActions.SelectedIndex < 0)
                listMapAddressActions.SelectedIndex = 0;

            listCmdAddressActions.Items.Add(listMapAddressActions.SelectedItem);
        }

        private void btnRemoveLastAddressAction_Click(object sender, EventArgs e)
        {
            if (listCmdAddressActions.Items.Count < 1)
                return;
            if (listCmdAddressActions.SelectedIndex < 0)
                listCmdAddressActions.SelectedIndex = listCmdAddressActions.Items.Count - 1;

            listCmdAddressActions.Items.RemoveAt(listCmdAddressActions.SelectedIndex);
        }

        private void btnClearAddressActions_Click(object sender, EventArgs e)
        {
            listCmdAddressActions.Items.Clear();
        }

        private void btnAddSpeedLimit_Click(object sender, EventArgs e)
        {
            if (listMapSpeedLimits.Items.Count < 1)
                return;
            if (listMapSpeedLimits.SelectedIndex < 0)
                listMapSpeedLimits.SelectedIndex = 0;

            listCmdSpeedLimits.Items.Add(listMapSpeedLimits.SelectedItem);
        }

        private void btnRemoveSpeedLimit_Click(object sender, EventArgs e)
        {
            if (listCmdSpeedLimits.Items.Count < 1)
                return;
            if (listCmdSpeedLimits.SelectedIndex < 0)
                listCmdSpeedLimits.SelectedIndex = listCmdSpeedLimits.Items.Count - 1;

            listCmdSpeedLimits.Items.RemoveAt(listCmdSpeedLimits.SelectedIndex);
        }

        private void btnClearSpeedLimit_Click(object sender, EventArgs e)
        {
            listCmdSpeedLimits.Items.Clear();
        }

        private void btnPositionXY_Click(object sender, EventArgs e)
        {
            try
            {
                int x = Int32.Parse(tB_PositionX.Text);
                int y = Int32.Parse(tB_PositionY.Text);

                string txtPosition = $"{x.ToString()},{y.ToString()}";
                listCmdAddressPositions.Items.Add(txtPosition);
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
                double limit = (double)listCmdSpeedLimits.Items[i];
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
            listCmdAddressActions.Items.Clear();
            listCmdSpeedLimits.Items.Clear();
        }

        private void button_DebugModeSend_Click(object sender, EventArgs e)
        {
            string errorMessage = "";
            SetPositions();
            SetActions();
            SetSpeedLimits();

            tbC_Debug.SelectedIndex = 1;

            if (moveControl.MoveState != EnumMoveState.Idle)
            {
                MessageBox.Show("動作命令中!");
                return;
            }

            command = moveControl.CreateMoveControlListSectionListReserveList(moveCmdInfo, ref errorMessage);

            if (command != null)
            {
                moveCmdInfo = null;
                button_SendList.Enabled = true;
                ShowList();
            }
            else
                MessageBox.Show("List 產生失敗!\n" + errorMessage);
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
                ReserveList.Items.Add("▷ " + reserveStringList[i]);
        }

        private void ShowMoveCommandList(List<Command> cmdList)
        {
            commandIndex = 0;
            CommandList.Items.Clear();
            commandStringList = new List<string>();

            moveControl.GetMoveCommandListInfo(cmdList, ref commandStringList);
            for (int i = 0; i < commandStringList.Count; i++)
                CommandList.Items.Add("▷ " + commandStringList[i]);
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
                moveControl.AddReservedIndexForDebugModeTest(ReserveList.Items.Count - 1);
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

        #region Page Debug CSV
        private void button_DebugCSV_Click(object sender, EventArgs e)
        {
            button_DebugCSV.Enabled = false;
            if (button_DebugCSV.Text == "開啟")
            {
                button_DebugCSV.Text = "關閉";
                moveControl.DebugCSVMode = true;
            }
            else
            {
                button_DebugCSV.Text = "開啟";
                moveControl.DebugCSVMode = false;
            }

            button_DebugCSV.Enabled = true;
        }

        private void button_DebugCSVClear_Click(object sender, EventArgs e)
        {
            dataGridView_CSVList.Rows.Clear();
        }

        private void dataGridView_CSVList_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int index = e.ColumnIndex;

            if (index < dataGridView_CSVList.ColumnCount)
            {
                dataGridView_CSVList.Columns[index].Visible = false;
            }
        }

        private void button_CSVListShowAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView_CSVList.ColumnCount; i++)
                dataGridView_CSVList.Columns[i].Visible = true;
        }

        private void dataGridView_CSVList_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (button_CSVListDisViewRang.Text == "隱藏區域關閉")
            {
                int index = e.ColumnIndex;
                if (firstSelect == -1)
                {
                    firstSelect = index;
                }
                else if (secondSelect == -1)
                {
                    if (index < firstSelect)
                    {
                        secondSelect = firstSelect;
                        firstSelect = index;
                    }
                    else
                        secondSelect = index;

                    for (int i = firstSelect; i <= secondSelect; i++)
                    {
                        if (i < dataGridView_CSVList.ColumnCount)
                            dataGridView_CSVList.Columns[i].Visible = false;
                    }

                    firstSelect = -1;
                    secondSelect = -1;
                }
            }
        }

        private void button_CSVListDisViewRang_Click(object sender, EventArgs e)
        {
            button_CSVListDisViewRang.Enabled = false;

            if (button_CSVListDisViewRang.Text == "隱藏區域開啟")
            {
                firstSelect = -1;
                secondSelect = -1;
                button_CSVListDisViewRang.Text = "隱藏區域關閉";
            }
            else
                button_CSVListDisViewRang.Text = "隱藏區域開啟";

            button_CSVListDisViewRang.Enabled = true;
        }
        #endregion

        #region tabPage Admin
        private void button_SimulationMode_Click(object sender, EventArgs e)
        {
            button_SimulationModeChange.Enabled = false;
            moveControl.SimulationMode = (button_SimulationModeChange.Text == "關閉中");
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
        }

        private void listMapAddressActions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listMapAddressActions.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                if (listMapAddressActions.Items.Count < 1)
                    return;
                if (listMapAddressActions.SelectedIndex < 0)
                    return;

                listCmdAddressActions.Items.Add(listMapAddressActions.SelectedItem);
            }
        }

        private void listMapSpeedLimits_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listMapSpeedLimits.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                if (listMapSpeedLimits.Items.Count < 1)
                    return;
                if (listMapSpeedLimits.SelectedIndex < 0)
                    return;

                listCmdSpeedLimits.Items.Add(listMapSpeedLimits.SelectedItem);
            }
        }

        public void AddAddressPositionByMainFormDoubleClick(string id)
        {
            if (theMapInfo.allMapAddresses.ContainsKey(id))
            {
                string str = theMapInfo.allMapAddresses[id].Position.X.ToString() + "," + theMapInfo.allMapAddresses[id].Position.Y.ToString();
                listCmdAddressPositions.Items.Add(str);

                if (moveControl.SimulationMode && simulationModeFirstDoubleClick)
                {
                    simulationModeFirstDoubleClick = false;
                    moveControl.location.Real = new AGVPosition();
                    moveControl.location.Real.AGVAngle = 0;
                    moveControl.location.Real.Position = theMapInfo.allMapAddresses[id].Position.DeepClone();
                    Vehicle.Instance.CurVehiclePosition.RealPosition = moveControl.location.Real.Position;
                    Vehicle.Instance.CurVehiclePosition.VehicleAngle = moveControl.location.Real.AGVAngle;
                }
            }
        }

        private bool IsAddress(MapPosition now)
        {
            foreach (var valuePair in theMapInfo.allMapAddresses)
            {
                if (Math.Abs(now.X - valuePair.Value.Position.X) <= 10 &&
                    Math.Abs(now.X - valuePair.Value.Position.Y) <= 10)
                {
                    string str = valuePair.Value.Position.X.ToString() + "," + valuePair.Value.Position.Y.ToString();
                    listCmdAddressPositions.Items.Add(str);
                    return true;
                }
            }

            return false;
        }

        private void FindPositionBySection(MapPosition now)
        {
            foreach (var valuePair in theMapInfo.allMapSections)
            {
                if (valuePair.Value.HeadAddress.Position.X == valuePair.Value.TailAddress.Position.X &&
                     Math.Abs(valuePair.Value.HeadAddress.Position.X - now.X) <= 10)
                {
                    string str = valuePair.Value.HeadAddress.Position.X.ToString() + "," + now.Y.ToString();
                    listCmdAddressPositions.Items.Add(str);
                    return;
                }
                else if (valuePair.Value.HeadAddress.Position.Y == valuePair.Value.TailAddress.Position.Y &&
                     Math.Abs(valuePair.Value.HeadAddress.Position.Y - now.Y) <= 10)
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
                    FindPositionBySection(moveControl.location.Real.Position);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            moveControl.test = EnumVehicleSafetyAction.Normal;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            moveControl.test = EnumVehicleSafetyAction.Stop;
        }
    }
}