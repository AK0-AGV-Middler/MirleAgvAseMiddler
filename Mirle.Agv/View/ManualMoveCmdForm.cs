using Mirle.Agv.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Model.TransferCmds;
using Mirle.Agv.Model;

namespace Mirle.Agv.View
{
    public partial class ManualMoveCmdForm : Form
    {
        private MainFlowHandler mainFlowHandler;
        private MoveCmdInfo moveCmdInfo = new MoveCmdInfo();

        public ManualMoveCmdForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
            AddCbAction();
        }

        private void AddCbAction()
        {
            cbAction.Items.Clear();
            foreach (string item in Enum.GetNames(typeof(EnumAddressAction)))
            {
                cbAction.Items.Add(item);
            }
        }

        private void btnAddAddressPosition_Click(object sender, EventArgs e)
        {
            float.TryParse(txtPositionX.Text, out float positionX);
            float.TryParse(txtPositionY.Text, out float positionY);

            moveCmdInfo.AddressPositions.Add(new MapPosition(positionX, positionY));
            UpdateTxtAddressPositions();
        }

        private void UpdateTxtAddressPositions()
        {
            List<MapPosition> addressPositions = moveCmdInfo.AddressPositions;
            if (addressPositions.Count == 0)
            {
                txtAddressPositions.Clear();
            }
            else
            {
                txtAddressPositions.Clear();
                for (int i = 0; i < addressPositions.Count; i++)
                {
                    MapPosition addressPosition = addressPositions[i];
                    string positionPair = $"({addressPosition.PositionX},{addressPosition.PositionY})";
                    if (i == 0)
                    {
                        txtAddressPositions.Text += positionPair;
                    }
                    else
                    {
                        txtAddressPositions.Text += " - " + positionPair;
                    }
                }
            }
        }

        private void btnRemoveLastAddressPosition_Click(object sender, EventArgs e)
        {
            List<MapPosition> addressPositions = moveCmdInfo.AddressPositions;
            if (addressPositions.Count > 0)
            {
                addressPositions.RemoveAt(addressPositions.Count - 1);
            }
            UpdateTxtAddressPositions();
        }

        private void btnAddressPositionsClear_Click(object sender, EventArgs e)
        {
            moveCmdInfo.AddressPositions.Clear();
            txtAddressPositions.Clear();
        }

        private void btnAddAddressAction_Click(object sender, EventArgs e)
        {
            EnumAddressAction action = (EnumAddressAction)Enum.Parse(typeof(EnumAddressAction), cbAction.Text);
            moveCmdInfo.AddressActions.Add(action);
            UpdateTxtAddressActions();
        }

        private void UpdateTxtAddressActions()
        {
            List<EnumAddressAction> addressActions = moveCmdInfo.AddressActions;
            if (addressActions.Count == 0)
            {
                txtAddressActions.Clear();
            }
            else
            {
                txtAddressActions.Clear();
                for (int i = 0; i < addressActions.Count; i++)
                {
                    EnumAddressAction addressAction = addressActions[i];
                    string actionMsg = $"({addressAction})";
                    if (i == 0)
                    {
                        txtAddressActions.Text += actionMsg;
                    }
                    else
                    {
                        txtAddressActions.Text += " - " + actionMsg;
                    }
                }
            }
        }

        private void btnRemoveLastAddressAction_Click(object sender, EventArgs e)
        {
            List<EnumAddressAction> actions = moveCmdInfo.AddressActions;
            if (actions.Count > 0)
            {
                actions.RemoveAt(actions.Count - 1);
            }
            UpdateTxtAddressActions();
        }

        private void btnAddressActionsClear_Click(object sender, EventArgs e)
        {
            moveCmdInfo.AddressActions.Clear();
            txtAddressActions.Clear();
        }

        private void btnAddSpeedLimit_Click(object sender, EventArgs e)
        {
            float speedLimit = float.Parse(txtSpeedLimit.Text);
            moveCmdInfo.SectionSpeedLimits.Add(speedLimit);
            UpdateTxtSectionSpeedLimits();
        }

        private void UpdateTxtSectionSpeedLimits()
        {
            var speedLimits = moveCmdInfo.SectionSpeedLimits;
            if (speedLimits.Count == 0)
            {
                txtSectionSpeedLimits.Clear();
            }
            else
            {
                txtSectionSpeedLimits.Clear();
                for (int i = 0; i < speedLimits.Count; i++)
                {
                    float speedLimit = speedLimits[i];
                    string msg = $"({speedLimit})";
                    if (i == 0)
                    {
                        txtSectionSpeedLimits.Text += msg;
                    }
                    else
                    {
                        txtSectionSpeedLimits.Text += " - " + msg;
                    }
                }
            }
        }

        private void btnRemoveLastSpeedLimits_Click(object sender, EventArgs e)
        {
            var speedLimits = moveCmdInfo.SectionSpeedLimits;
            if (speedLimits.Count > 0)
            {
                speedLimits.RemoveAt(speedLimits.Count - 1);
            }
            UpdateTxtSectionSpeedLimits();
        }

        private void btnClearSpeedLimits_Click(object sender, EventArgs e)
        {
            moveCmdInfo.SectionSpeedLimits.Clear();
            txtSectionSpeedLimits.Clear();
        }

        private void btnClearMoveCmdInfo_Click(object sender, EventArgs e)
        {
            moveCmdInfo = new MoveCmdInfo();
            UpdateTxtAddressPositions();
            UpdateTxtAddressActions();
            UpdateTxtSectionSpeedLimits();
        }

        private void btnCheckMoveCmdInfo_Click(object sender, EventArgs e)
        {
            WarningForm warningForm = new WarningForm();
            warningForm.TopLevel = true;

            if (moveCmdInfo == null)
            {               
                warningForm.WarningMsg = "Move Command is null";
                warningForm.Show();
                return;
            }
            var positions = moveCmdInfo.AddressPositions;
            if (positions.Count < 1)
            {
                warningForm.WarningMsg = "AddressPositionList is empty";
                warningForm.Show();
                return;
            }
            var actions = moveCmdInfo.AddressActions;
            if (actions.Count < 1)
            {
                warningForm.WarningMsg = "AddressActionList is empty";
                warningForm.Show();
                return;
            }
            if (actions.Count!=positions.Count)
            {
                warningForm.WarningMsg = "AddressPositionList and AddressActionList are not match";
                warningForm.Show();
                return;
            }
            if (actions[actions.Count-1]!= EnumAddressAction.End)
            {
                warningForm.WarningMsg = "AddressActionList is not end with EnumAddressAction.End";
                warningForm.Show();
                return;
            }
        }

        private void btnSendMoveCmdInfo_Click(object sender, EventArgs e)
        {
            mainFlowHandler.PublishTransferMoveEvent(moveCmdInfo);
        }
    }
}
