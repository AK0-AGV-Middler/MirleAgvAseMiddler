using ClsMCProtocol;
using Mirle.Agv.Controller;
using Mirle.Agv.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.View
{
    public partial class PlcForm : Form
    {
        private MCProtocol mcProtocol;
        private PlcAgent plcAgent;

        public PlcForm(MCProtocol aMcProtocol, PlcAgent aPlcAgent)
        {
            InitializeComponent();
            mcProtocol = aMcProtocol;
            tabPage1.Controls.Add(mcProtocol);

            plcAgent = aPlcAgent;

            //一些Form Control要給進去Entity物件
            FormControlAddToEnityClass();
            EventInitial();
                  
            mcProtocol.LoadXMLConfig();
            mcProtocol.OperationMode = MCProtocol.enOperationMode.NORMAL_MODE;

            mcProtocol.Height = tabPage1.Height;
            mcProtocol.Width = tabPage1.Width;          
        }

        private void EventInitial()
        {
            mcProtocol.OnDataChangeEvent += McProtocol_OnDataChangeEvent;

            plcAgent.OnForkCommandExecutingEvent += PlcAgent_OnForkCommandExecutingEvent;
            plcAgent.OnForkCommandFinishEvent += PlcAgent_OnForkCommandFinishEvent;
            plcAgent.OnForkCommandErrorEvent += PlcAgent_OnForkCommandErrorEvent;
            plcAgent.OnCassetteIDReadFinishEvent += PlcAgent_OnCassetteIDReadFinishEvent;
        }

        private Control FindControlById(string aId)
        {
            Control aControl = null;
            aControl = grpF.Controls[aId];
            if(aControl != null)
            {
                return aControl;                
            }

            aControl = grpB.Controls[aId];
            if (aControl != null)
            {
                return aControl;
            }

            aControl = grpL.Controls[aId];
            if (aControl != null)
            {
                return aControl;
            }

            aControl = grpR.Controls[aId];
            if (aControl != null)
            {
                return aControl;
            }

            return aControl;
        }
         
        private void LabelAddToSideBeamSensor(List<PlcBeamSensor> listBeamSensor)
        {
            foreach (PlcBeamSensor aBeamSensor in listBeamSensor)
            {
                aBeamSensor.FormLabel = (Label)FindControlById("lblBeamSensor" + aBeamSensor.Id);
                ((Label)FindControlById("lblBeamSensor" + aBeamSensor.Id)).Tag = aBeamSensor;
            }
        }

        private void FormControlAddToEnityClass()
        {
            //BeamSensor
            LabelAddToSideBeamSensor(this.plcAgent.thePlcVehicle.listFrontBeamSensor);
            LabelAddToSideBeamSensor(this.plcAgent.thePlcVehicle.listBackBeamSensor);
            LabelAddToSideBeamSensor(this.plcAgent.thePlcVehicle.listLeftBeamSensor);
            LabelAddToSideBeamSensor(this.plcAgent.thePlcVehicle.listRightBeamSensor);
            //Bumper
            foreach (PlcBumper aBumper in this.plcAgent.thePlcVehicle.listBumper)
            {
                aBumper.FormLabel = (Label)FindControlById("lblBump" + aBumper.Id);
                ((Label)FindControlById("lblBump" + aBumper.Id)).Tag = aBumper;
            }
            //EMO
            foreach (PlcEmo aPlcEmo in this.plcAgent.thePlcVehicle.listPlcEmo)
            {
                aPlcEmo.FormLabel = (Label)FindControlById("lblEMO" + aPlcEmo.Id);
                ((Label)FindControlById("lblEMO" + aPlcEmo.Id)).Tag = aPlcEmo;
            }
        }
        //
        private void PlcAgent_OnForkCommandErrorEvent(object sender, PLCForkCommand aForkCommand)
        {
            triggerEvent = "PLCAgent_OnForkCommandErrorEvent";
        }

        private void PlcAgent_OnForkCommandExecutingEvent(object sender, PLCForkCommand aForkCommand)
        {
            triggerEvent = "PLCAgent_OnForkCommandExecutingEvent";
        }

        private void PlcAgent_OnForkCommandFinishEvent(object sender, PLCForkCommand aForkCommand)
        {
            triggerEvent = "PLCAgent_OnForkCommandFinishEvent";
        }

        private void PlcAgent_OnCassetteIDReadFinishEvent(object sender, string aCassetteId)
        {
            triggerEvent = "PLCAgent_OnCassetteIDReadFinishEvent cassetteID = " + aCassetteId;
        }

        private string triggerEvent;
        private void McProtocol_OnDataChangeEvent(string sMessage, clsColParameter oColParam)
        {
            //int tagChangeCount = oColParam.Count();
            //for (int i=1;i<= tagChangeCount; i++)
            //{
            //    triggerEvent = oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean.ToString();



            //}



        }

        private void btnForkCommandExecute_Click(object sender, EventArgs e)
        {
            //this.aPLCAgent.WriteForkCommand(Convert.ToUInt16(txtCommandNo.Text), (EnumForkCommand)Enum.Parse(typeof(EnumForkCommand), cmbOperationType.Text, false), txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
            
            Task.Run(() =>
            {
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, true);
                System.Threading.Thread.Sleep(1000);
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, false);

            });
        }

        private void PlcForm_Load(object sender, EventArgs e)
        {
            cmbOperationType.Items.Clear();
          
            foreach (string item in Enum.GetNames(typeof(EnumForkCommand)))
            {
                cmbOperationType.Items.Add(item);
            }
            cmbOperationType.SelectedIndex = 2;

            cmbDirection.Items.Clear();
            foreach (string item in Enum.GetNames(typeof(EnumStageDirection)))
            {
                cmbDirection.Items.Add(item);
            }
            cmbDirection.SelectedIndex = 1;
            //  //cmbChargeDirection

            cmbChargeDirection.Items.Clear();
            foreach (string item in Enum.GetNames(typeof(EnumChargeDirection)))
            {
                cmbChargeDirection.Items.Add(item);
            }
            cmbChargeDirection.SelectedIndex = 1;
            txtAutoChargeLowSOC.Text = this.plcAgent.thePlcVehicle.Batterys.PortAutoChargeLowSoc.ToString();
            
            cmbEQPIO.Items.Clear();
            cmbEQPIO.Items.Add(bool.TrueString);
            cmbEQPIO.Items.Add(bool.FalseString);
            cmbEQPIO.SelectedIndex = 0;

            //this.WindowState = FormWindowState.Minimized;
            timGUIRefresh.Enabled = true;

        }

        private void btnForkCommandClear_Click(object sender, EventArgs e)
        {
            txtCommandNo.Text = "1";
            cmbOperationType.Text = EnumForkCommand.Load.ToString();
            txtStageNo.Text = "1";
            cmbDirection.Text = EnumStageDirection.Left.ToString();
            cmbEQPIO.Text = bool.TrueString;
            txtForkSpeed.Text = "100";
            this.plcAgent.WriteForkCommandInfo(0, EnumForkCommand.None, "0", EnumStageDirection.None, true, 100);

        }

        private void btnForkCommandWrite_Click(object sender, EventArgs e)
        {
            try
            {
                this.plcAgent.WriteForkCommandInfo(Convert.ToUInt16(txtCommandNo.Text), (EnumForkCommand)Enum.Parse(typeof(EnumForkCommand), cmbOperationType.Text, false), txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));

            }
            catch (Exception ex)
            {

                this.triggerEvent = ex.ToString();
            }
            
        }

        private void btnForkCommandReadRequest_Click(object sender, EventArgs e)
        {
            
            Task.Run(() =>
            {
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, true);
                System.Threading.Thread.Sleep(1000);
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);

            });
        }
        
        private void btnChargeLeftStart_Click(object sender, EventArgs e)
        {
            this.plcAgent.ChargeStartCommand(EnumChargeDirection.Left);
        }

        private void btnChargeRightStart_Click(object sender, EventArgs e)
        {
            this.plcAgent.ChargeStartCommand(EnumChargeDirection.Right);

        }

        private void btnChargeStop_Click(object sender, EventArgs e)
        {
            this.plcAgent.ChargeStopCommand();

        }

        private void btnForkCommandFinishAck_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack,true);
                System.Threading.Thread.Sleep(1000);
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, false);

            });

                

        }

        private void timGUIRefresh_Tick(object sender, EventArgs e)
        {
            txtTriggerEvent.Text = triggerEvent;
            if (this.plcAgent.thePlcVehicle.Batterys.BatteryType == EnumBatteryType.Gotech)
            {
                this.lblGotech.BackColor = Color.LightGreen;
            }
            else
            {
                this.lblGotech.BackColor = Color.Silver;
            }

            if (this.plcAgent.thePlcVehicle.Batterys.BatteryType == EnumBatteryType.Yinda)
            {
                this.lblYinda.BackColor = Color.LightGreen;
            }
            else
            {
                this.lblYinda.BackColor = Color.Silver;
            }

            if (this.plcAgent.thePlcVehicle.Batterys.Charging)
            {
                lblCharging.BackColor = Color.LightGreen;
            }
            else
            {
                lblCharging.BackColor = Color.Silver;
            }

            txtCurrent.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.MeterCurrent);
            txtVoltage.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.MeterVoltage);
            txtWatt.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.MeterWatt);
            txtWattHour.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.MeterWattHour);
            txtAH.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.MeterAh);
            txtSOC.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.Percentage);
            txtAHWorkingRange.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.AhWorkingRange);
            txtCCModeAH.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.CcModeAh);

            txtCCModeCounter.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.CcModeCounter);
            txtMaxCCmodeCounter.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.MaxResetAhCcounter);
            txtFullChargeIndex.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.FullChargeIndex);

            txtFBatteryTemp.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.FBatteryTemperature);
            txtBBatteryTemp.Text = Convert.ToString(this.plcAgent.thePlcVehicle.Batterys.BBatteryTemperature);

            txtErrorReason.Text = this.plcAgent.GetErrorReason();

            txtCassetteID.Text = this.plcAgent.thePlcVehicle.CassetteID;

            if (this.plcAgent.thePlcVehicle.Robot.ForkBusy)
            {
                lblForkBusy.BackColor = Color.LightGreen;
            }
            else
            {
                lblForkBusy.BackColor = Color.Silver;
            }
            if (this.plcAgent.thePlcVehicle.Robot.ForkReady)
            {
                lblForkReady.BackColor = Color.LightGreen;
            }
            else
            {
                lblForkReady.BackColor = Color.Silver;
            }

            if (this.plcAgent.thePlcVehicle.Robot.ForkFinish)
            {
                lblForkFinish.BackColor = Color.LightGreen;
            }
            else
            {
                lblForkFinish.BackColor = Color.Silver;
            }

            if (this.plcAgent.thePlcVehicle.Loading)
            {
                lblLoading.BackColor = Color.LightGreen;
            }
            else
            {
                lblLoading.BackColor = Color.Silver;
            }

            if (this.plcAgent.thePlcVehicle.SafetyDisable)
            {
                tabSafety.BackColor = Color.Pink;
            }
            else
            {
                tabSafety.BackColor = Color.Transparent;
            }

            txtSafetyAction.Text = plcAgent.thePlcVehicle.VehicleSafetyAction.ToString();
            //BeamSensor color
            ShowSideBeamcolor(plcAgent.thePlcVehicle.listFrontBeamSensor);
            ShowSideBeamcolor(plcAgent.thePlcVehicle.listBackBeamSensor);
            ShowSideBeamcolor(plcAgent.thePlcVehicle.listLeftBeamSensor);
            ShowSideBeamcolor(plcAgent.thePlcVehicle.listRightBeamSensor);

            //Bumper color
            foreach (PlcBumper aPLCBumper in plcAgent.thePlcVehicle.listBumper)
            {
                if (aPLCBumper.Disable)
                {
                    aPLCBumper.FormLabel.BorderStyle = BorderStyle.None;
                }
                else
                {
                    aPLCBumper.FormLabel.BorderStyle = BorderStyle.FixedSingle;
                }

                if (aPLCBumper.Signal)
                {
                    aPLCBumper.FormLabel.BackColor = this.lblNoDetect.BackColor;
                }
                else
                {                    
                    aPLCBumper.FormLabel.BackColor = this.lblNearDetect.BackColor;
                }
            }

            //EMO color
            foreach (PlcEmo aPlcEmo in plcAgent.thePlcVehicle.listPlcEmo)
            {
                if (aPlcEmo.Disable)
                {
                    aPlcEmo.FormLabel.BorderStyle = BorderStyle.None;
                }
                else
                {
                    aPlcEmo.FormLabel.BorderStyle = BorderStyle.FixedSingle;
                }

                if (aPlcEmo.Signal)
                {
                    aPlcEmo.FormLabel.BackColor = this.lblNoDetect.BackColor;
                }
                else
                {                    
                    aPlcEmo.FormLabel.BackColor = this.lblNearDetect.BackColor;
                }
            }

            if (plcAgent.thePlcVehicle.MoveFront)
            {
                chkMoveFront.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveFront.BackColor = Color.Transparent;
            }

            if (plcAgent.thePlcVehicle.MoveBack)
            {
                chkMoveBack.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveBack.BackColor = Color.Transparent;
            }

            if (plcAgent.thePlcVehicle.MoveLeft)
            {
                chkMoveLeft.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveLeft.BackColor = Color.Transparent;
            }

            if (plcAgent.thePlcVehicle.MoveRight)
            {
                chkMoveRight.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveRight.BackColor = Color.Transparent;
            }

            if (plcAgent.thePlcVehicle.FrontBeamSensorDisable)
            {
                grpF.BackColor = Color.Pink;
            }
            else
            {
                grpF.BackColor = Color.Transparent;
            }

            if (plcAgent.thePlcVehicle.BackBeamSensorDisable)
            {
                grpB.BackColor = Color.Pink;
            }
            else
            {
                grpB.BackColor = Color.Transparent;
            }

            if (plcAgent.thePlcVehicle.LeftBeamSensorDisable)
            {
                grpL.BackColor = Color.Pink;
            }
            else
            {
                grpL.BackColor = Color.Transparent;
            }

            if (plcAgent.thePlcVehicle.RightBeamSensorDisable)
            {
                grpR.BackColor = Color.Pink;
            }
            else
            {
                grpR.BackColor = Color.Transparent;
            }

            if (plcAgent.thePlcVehicle.BeamSensorAutoSleep)
            {
                rdoBeamSensorAutoSleepEnable.BackColor = Color.LightGreen;
                rdoBeamSensorAutoSleepDisable.BackColor = Color.Transparent;
            }
            else
            {
                rdoBeamSensorAutoSleepEnable.BackColor = Color.Transparent;
                rdoBeamSensorAutoSleepDisable.BackColor = Color.LightGreen;
            }

        }

        private void ShowSideBeamcolor(List<PlcBeamSensor> listBeamSensor)
        {
            foreach (PlcBeamSensor aBeamSensor in listBeamSensor)
            {
                if (aBeamSensor.Disable)
                {
                    aBeamSensor.FormLabel.BorderStyle = BorderStyle.None;
                }
                else
                {
                    aBeamSensor.FormLabel.BorderStyle = BorderStyle.FixedSingle;
                }

                if (aBeamSensor.ReadSleepSignal)
                {
                    aBeamSensor.FormLabel.BackColor = this.lblSleep.BackColor;
                }
                else
                {
                    if (aBeamSensor.NearSignal)
                    {
                        if (aBeamSensor.FarSignal)
                        {
                            aBeamSensor.FormLabel.BackColor = this.lblNoDetect.BackColor;
                        }
                        else
                        {
                            aBeamSensor.FormLabel.BackColor = this.lblFarDetect.BackColor;                            
                        }
                    }
                    else
                    {
                        aBeamSensor.FormLabel.BackColor = this.lblNearDetect.BackColor;
                        
                    }
                }
                
            }
        }        

        private void btnMeterAHReset_Click(object sender, EventArgs e)
        {
            plcAgent.SetMeterAHToZero();

        }

        private void btnForkCommandExecuteFlow_Click(object sender, EventArgs e)
        {
            try
            {
                if (!plcAgent.IsForkCommandExist())
                {
                    PLCForkCommand aForkCommand = new PLCForkCommand(Convert.ToUInt16(txtCommandNo.Text), (EnumForkCommand)Enum.Parse(typeof(EnumForkCommand), cmbOperationType.Text, false), txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
                    plcAgent.AddForkComand(aForkCommand);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

                this.triggerEvent = ex.ToString();
            }

            
        }

        private void btnClearForkCommand_Click(object sender, EventArgs e)
        {
            try
            {
                plcAgent.ClearExecutingForkCommand();
            }
            catch (Exception ex)
            {

                this.triggerEvent = ex.ToString();
            }
            
        }      

        private void btnBuzzerStop_Click(object sender, EventArgs e)
        {
            plcAgent.WritePLCBuzzserStop();
        }

        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            plcAgent.WritePLCAlarmReset();
        }

        private void btnTriggerCassetteReader_Click(object sender, EventArgs e)
        {
            string CassetteID = "";
            this.plcAgent.TriggerCassetteIDReader(ref CassetteID);
        }

        private void btnCycle_Click(object sender, EventArgs e)
        {
            if(btnCycle.Text == "Cycle Start")
            {
                cycleForkCommandCount = 0;
                cycleChargeCommandCount = 0;
                timCycle.Enabled = true;
                btnCycle.Text = "Cycle Stop";
            }
            else
            {
                timCycle.Enabled = false;
                btnCycle.Text = "Cycle Start";
            }
        }

        private ulong cycleForkCommandCount = 0;
        private ulong cycleChargeCommandCount = 0;

        private void timCycle_Tick(object sender, EventArgs e)
        {
            if(this.plcAgent.thePlcVehicle.Robot.ForkReady && this.plcAgent.thePlcVehicle.Robot.ForkBusy == false)
            {
                if (!plcAgent.IsForkCommandExist())
                {
                    //判斷loading 決定load/unload
                    if (plcAgent.thePlcVehicle.Loading)
                    {
                        PLCForkCommand aForkCommand = new PLCForkCommand(Convert.ToUInt16(txtCommandNo.Text), EnumForkCommand.Unload, txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
                        plcAgent.AddForkComand(aForkCommand);
                        cycleForkCommandCount++;
                        txtCycleForkCommandCount.Text = cycleForkCommandCount.ToString();
                    }
                    else
                    {
                        PLCForkCommand aForkCommand = new PLCForkCommand(Convert.ToUInt16(txtCommandNo.Text), EnumForkCommand.Load, txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
                        plcAgent.AddForkComand(aForkCommand);
                        cycleForkCommandCount++;
                        txtCycleForkCommandCount.Text = cycleForkCommandCount.ToString();
                    }
                    

                }
            }

            if(this.plcAgent.thePlcVehicle.Batterys.Charging == false)
            {
                if (this.plcAgent.thePlcVehicle.Batterys.Percentage < this. plcAgent.thePlcVehicle.Batterys.PortAutoChargeLowSoc)
                {
                    //自動充電
                    if ((EnumChargeDirection)Enum.Parse(typeof(EnumChargeDirection), cmbChargeDirection.Text, false) == EnumChargeDirection.Left)
                    {
                        this.plcAgent.ChargeStartCommand(EnumChargeDirection.Left);
                        cycleChargeCommandCount++;
                        txtCycleChargeCommandCount.Text = cycleChargeCommandCount.ToString();
                    }
                    else if ((EnumChargeDirection)Enum.Parse(typeof(EnumChargeDirection), cmbChargeDirection.Text, false) == EnumChargeDirection.Right)
                    {
                        this.plcAgent.ChargeStartCommand(EnumChargeDirection.Right);
                        cycleChargeCommandCount++;
                        txtCycleChargeCommandCount.Text = cycleChargeCommandCount.ToString();
                    }
                    else
                    {
                        timCycle.Enabled = false;
                        btnCycle.Text = "Cycle Start";
                    }
                }
                else
                {

                }

            }

        }

        private void btnSafetySet_Click(object sender, EventArgs e)
        {
            if (rdoSafetyDisable.Checked)
            {
                this.plcAgent.thePlcVehicle.SafetyDisable = true;
            }
            else
            {
                this.plcAgent.thePlcVehicle.SafetyDisable = false;
            }
        }

        private void lblBump_DoubleClick(object sender, EventArgs e)
        {
            PlcBumper aPLCBumper = (PlcBumper)((Label)sender).Tag;
            aPLCBumper.Disable = !aPLCBumper.Disable;
        }

        private void chkMoveFront_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveFront.Checked)
            {
                plcAgent.thePlcVehicle.MoveFront = true;

            }
            else
            {
                plcAgent.thePlcVehicle.MoveFront = false;
            }
        }

        private void chkMoveBack_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveBack.Checked)
            {
                plcAgent.thePlcVehicle.MoveBack = true;
            }
            else
            {
                plcAgent.thePlcVehicle.MoveBack = false;
            }
        }

        private void chkMoveLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveLeft.Checked)
            {
                plcAgent.thePlcVehicle.MoveLeft = true;
            }
            else
            {
                plcAgent.thePlcVehicle.MoveLeft = false;
            }
        }

        private void chkMoveRight_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveRight.Checked)
            {
                plcAgent.thePlcVehicle.MoveRight = true;

            }
            else
            {
                plcAgent.thePlcVehicle.MoveRight = false;
            }
        }

        private void pnlF_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.thePlcVehicle.FrontBeamSensorDisable)
            {
                plcAgent.thePlcVehicle.FrontBeamSensorDisable = false;
            }
            else
            {
                plcAgent.thePlcVehicle.FrontBeamSensorDisable = true;
            }
        }

        private void pnlL_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.thePlcVehicle.LeftBeamSensorDisable)
            {
                plcAgent.thePlcVehicle.LeftBeamSensorDisable = false;
            }
            else
            {
                plcAgent.thePlcVehicle.LeftBeamSensorDisable = true;
            }
        }

        private void pnlB_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.thePlcVehicle.BackBeamSensorDisable)
            {
                plcAgent.thePlcVehicle.BackBeamSensorDisable = false;
            }
            else
            {
                plcAgent.thePlcVehicle.BackBeamSensorDisable = true;
            }
        }

        private void pnlR_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.thePlcVehicle.RightBeamSensorDisable)
            {
                plcAgent.thePlcVehicle.RightBeamSensorDisable = false;
            }
            else
            {
                plcAgent.thePlcVehicle.RightBeamSensorDisable = true;
            }
        }

        private void btnBeamSensorAutoSleepSet_Click(object sender, EventArgs e)
        {
            if (rdoBeamSensorAutoSleepEnable.Checked)
            {
                plcAgent.thePlcVehicle.BeamSensorAutoSleep = true;
            }
            else
            {
                plcAgent.thePlcVehicle.BeamSensorAutoSleep = false;
            }
        }
    }
}
