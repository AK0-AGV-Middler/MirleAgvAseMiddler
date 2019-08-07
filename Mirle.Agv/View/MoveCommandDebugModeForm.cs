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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Mirle.Agv.View
{
    public partial class MoveCommandDebugModeForm : Form
    {
        private MoveControlHandler moveControl;
        private CreateMoveControlList createMoveControlList;
        private List<Command> moveCmdList = null;
        private List<SectionLine> sectionLineList = null;
        private List<ReserveData> reserveDataList = null;
        private MapInfo theMapInfo = new MapInfo();
        private MoveCmdInfo moveCmdInfo = new MoveCmdInfo();

        private List<string> commandStringList = new List<string>();
        private List<string> reserveStringList = new List<string>();
        private int reserveIndex;
        private int commandIndex;
        private AGVPosition tempReal = null;
        private string debugCSVHeader = "";

        #region Initail
        public MoveCommandDebugModeForm(MoveControlHandler moveControl, MapInfo theMapInfo)
        {
            InitializeComponent();
            this.moveControl = moveControl;
            this.theMapInfo = theMapInfo;
            createMoveControlList = new CreateMoveControlList(moveControl.DriverSr2000List, moveControl.moveControlConfig);
        }

        private void MoveCommandMonitor_Load(object sender, EventArgs e)
        {
            button_SendList.Enabled = false;
            ucLabelTB_RealEncoder.UcName = "Real Encoder : ";
            ucLabelTB_RealPosition.UcName = "Real Position : ";
            ucLabelTB_BarcodePosition.UcName = "Barcode Position : ";
            ucLabelTB_Delta.UcName = "Encoder Delta : ";
            ucLabelTB_CreateCommand_BarcodePosition.UcName = "Barcode Position : ";
            ucLabelTtB_CommandListState.UcName = "AGV Move State : ";
            ucLabelTB_CreateCommandState.UcName = "AGV Move State : ";
            AddListMapAddressPositions();
            AddListMapAddressActions();
            AddListMapSpeedLimits();
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
            moveControl.DebugLog = false;
        }
        #endregion

        #region timer Update
        private void Timer_Update_CreateCommand()
        {
            AGVPosition tempBarcodePosition = moveControl.position.Barcode;

            if (tempBarcodePosition != null)
            {
                ucLabelTB_CreateCommand_BarcodePosition.UcValue = "( " + tempBarcodePosition.Position.X.ToString("0") + ", " +
                                                                         tempBarcodePosition.Position.Y.ToString("0") + " )";
            }
            else
                ucLabelTB_CreateCommand_BarcodePosition.UcValue = "( ---, --- )";

            ucLabelTB_CreateCommandState.UcValue = moveControl.MoveState.ToString();
            if (moveControl.MoveState != EnumMoveState.Idle)
                button_DebugModeSend.Enabled = false;
            else
                button_DebugModeSend.Enabled = true;
        }

        private void Timer_Update_CommandList()
        {
            AGVPosition tempBarcodePosition = moveControl.position.Barcode;

            int tempResserveIndex = moveControl.GetReserveIndex();
            int tempCommandIndex = moveControl.GetCommandIndex();

            if (tempResserveIndex != -1 && tempResserveIndex > reserveIndex)
            {
                for (int i = reserveIndex + 1; i <= tempResserveIndex; i++)
                    ReserveList.Items[i] = "▶ " + reserveStringList[i];

                reserveIndex = tempResserveIndex;
            }

            if (tempCommandIndex != -1 && tempCommandIndex > commandIndex)
            {
                for (int i = commandIndex; i < tempCommandIndex; i++)
                    CommandList.Items[i] = "▶ " + commandStringList[i];

                commandIndex = tempCommandIndex;
            }

            ucLabelTB_RealEncoder.UcValue = moveControl.position.RealEncoder.ToString("0.0");
            ucLabelTB_Delta.UcValue = moveControl.position.Delta.ToString("0.0");

            tempReal = moveControl.position.Real;

            if (tempReal != null)
                ucLabelTB_RealPosition.UcValue = "( " + tempReal.Position.X.ToString("0") + ", " + tempReal.Position.Y.ToString("0") + " )";
            else
                ucLabelTB_RealPosition.UcValue = "( ---, --- )";

            if (tempBarcodePosition != null)
            {
                ucLabelTB_BarcodePosition.UcValue = "( " + tempBarcodePosition.Position.X.ToString("0") + ", " +
                                                                         tempBarcodePosition.Position.Y.ToString("0") + " )";
            }
            else
                ucLabelTB_BarcodePosition.UcValue = "( ---, --- )";

            ucLabelTtB_CommandListState.UcValue = moveControl.MoveState.ToString();
        }

        private void Timer_Update_Debug()
        {

        }

        private void Timer_Update_DebugCSV()
        {
            List<string> buffer = moveControl.debugCsvLogList;
            moveControl.debugCsvLogList = new List<string>();

            for (int i = 0; i < buffer.Count; i++)
                DebugCSVList.Items.Add(buffer[i]);

            for (int i = 3000; i < DebugCSVList.Items.Count; i++)
                DebugCSVList.Items.RemoveAt(0);
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
                int x = Int16.Parse(tB_PositionX.Text);
                int y = Int16.Parse(tB_PositionY.Text);

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
                var posX = double.Parse(posXY[0]);
                var posY = double.Parse(posXY[1]);
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

            if (createMoveControlList.CreatMoveControlListSectionListReserveList(moveCmdInfo,
                         ref moveCmdList, ref sectionLineList, ref reserveDataList, moveControl.position.Real, ref errorMessage))
            {
                moveCmdInfo = null;
                button_SendList.Enabled = true;
                ShowList();
            }
            else
                MessageBox.Show("Cycle List 產生失敗!\n" + errorMessage);
        }
        #endregion

        #region Page Command List
        private void ShowReserveList()
        {
            reserveIndex = -1;
            ReserveList.Items.Clear();
            reserveStringList = new List<string>();
            string lineString;
            for (int i = 0; i < reserveDataList.Count; i++)
            {
                lineString = "reserve node " + i.ToString() + " : ( " +
                    reserveDataList[i].Position.X.ToString("0") + ", " +
                    reserveDataList[i].Position.Y.ToString("0") + " )";
                reserveStringList.Add(lineString);

                lineString = "▷ " + lineString;
                ReserveList.Items.Add(lineString);
            }
        }

        private void ShowMoveCommandList()
        {
            commandIndex = 0;
            CommandList.Items.Clear();
            commandStringList = new List<string>();

            createMoveControlList.GetMoveCommandListInfo(moveCmdList, ref commandStringList);
            for (int i = 0; i < commandStringList.Count; i++)
                CommandList.Items.Add("▷ " + commandStringList[i]);
        }

        private void ShowList()
        {
            ShowReserveList();
            ShowMoveCommandList();
        }

        private void button_SendList_Click(object sender, EventArgs e)
        {
            moveControl.TransferMoveDebugMode(moveCmdList, sectionLineList, reserveDataList);
            moveCmdList = null;
            sectionLineList = null;
            reserveDataList = null;
            button_SendList.Enabled = false;
        }

        private void button_StopMove_Click(object sender, EventArgs e)
        {
            moveControl.StopFlagOn();
        }

        private void button_ClearCommand_Click(object sender, EventArgs e)
        {
            moveControl.StatusChange();
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
                moveControl.DebugLog = true;
            }
            else
            {
                button_DebugCSV.Text = "開啟";
                moveControl.DebugLog = false;
            }

            button_DebugCSV.Enabled = true;
        }

        private void button_DebugCSVClear_Click(object sender, EventArgs e)
        {
            DebugCSVList.Items.Clear();
        }
        #endregion

    }
}