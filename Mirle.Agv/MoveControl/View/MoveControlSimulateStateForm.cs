using Mirle.AgvAseMiddler.Controller;
using Mirle.AgvAseMiddler.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.AgvAseMiddler.View
{
    public partial class MoveControlSimulateStateForm : Form
    {
        private SimulateState fakeState;
        private MoveControlHandler moveControl;
        private bool hideFunctionOn;

        public MoveControlSimulateStateForm(MoveControlHandler moveControl, bool hideFunctionOn)
        {
            this.moveControl = moveControl;
            this.hideFunctionOn = hideFunctionOn;
            this.fakeState = moveControl.FakeState;
            InitializeComponent();

            if (hideFunctionOn)
                this.ForeColor = Color.Red;

            switch (fakeState.BeamSensorState)
            {
                case EnumVehicleSafetyAction.Normal:
                    radioButton_BeamSensor_Normal.Checked = true;
                    break;
                case EnumVehicleSafetyAction.LowSpeed:
                    radioButton_BeamSensor_LowSpeed.Checked = true;
                    break;
                case EnumVehicleSafetyAction.Stop:
                    radioButton_BeamSensor_Stop.Checked = true;
                    break;
                default:
                    break;
            }

            switch (fakeState.BumpSensorState)
            {
                case EnumVehicleSafetyAction.Normal:
                    radioButton_BumpSensor_Normal.Checked = true;
                    break;
                case EnumVehicleSafetyAction.Stop:
                    radioButton_BumpSensor_Stop.Checked = true;
                    break;
                case EnumVehicleSafetyAction.LowSpeed:
                default:
                    break;
            }

            if (fakeState.AxisNormal)
                radioButton_SimulateAxisNormal.Checked = true;
            else
                radioButton_SimulateAxisError.Checked = true;

            if (fakeState.IsCharging)
                radioButton_SimulateChargingYes.Checked = true;
            else
                radioButton_SimulateChargingNo.Checked = true;

            if (fakeState.ForkNotHome)
                radioButton_SimulateForkNotHome.Checked = true;
            else
                radioButton_SimulateForkHome.Checked = true;
        }

        public void ResetHideFunctionOnOff(bool hideFunctionOnOff)
        {
            this.hideFunctionOn = hideFunctionOnOff;
            this.ForeColor = hideFunctionOnOff ? Color.Red : Color.Black;
        }

        private void button_Pause_Click(object sender, EventArgs e)
        {
            moveControl.VehclePause(hideFunctionOn);
        }

        private void button_Continue_Click(object sender, EventArgs e)
        {
            moveControl.VehcleContinue();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            moveControl.VehcleCancel();
        }

        private void radioButton_BeamSensor_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButton_BeamSensor_Normal)
                fakeState.BeamSensorState = EnumVehicleSafetyAction.Normal;
            else if (sender == radioButton_BeamSensor_LowSpeed)
                fakeState.BeamSensorState = EnumVehicleSafetyAction.LowSpeed;
            else
                fakeState.BeamSensorState = EnumVehicleSafetyAction.Stop;
        }

        private void radioButton_BumpSensor_CheckedChanged(object sender, EventArgs e)
        {
            fakeState.BumpSensorState = (sender == radioButton_BumpSensor_Normal) ? EnumVehicleSafetyAction.Normal : EnumVehicleSafetyAction.Stop;
        }

        private void radioButton_SimulateAxisState_CheckedChanged(object sender, EventArgs e)
        {
            fakeState.AxisNormal = (sender == radioButton_SimulateAxisNormal);
        }

        private void radioButton_SimulateChargingState_CheckedChanged(object sender, EventArgs e)
        {
            fakeState.IsCharging = (sender == radioButton_SimulateChargingYes);
        }

        private void radioButton_SimulateForkState_CheckedChanged(object sender, EventArgs e)
        {
            fakeState.ForkNotHome = (sender == radioButton_SimulateForkNotHome);
        }
    }
}
