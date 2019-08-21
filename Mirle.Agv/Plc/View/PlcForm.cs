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
        private EnumAutoState IpcAutoState;

        public PlcForm(MCProtocol aMcProtocol, PlcAgent aPlcAgent)
        {
            InitializeComponent();
            //mcProtocol = new MCProtocol();
            //mcProtocol.Name = "MCProtocol";
            //mcProtocol.OnDataChangeEvent += MCProtocol_OnDataChangeEvent;
            mcProtocol = aMcProtocol;
            tabPage1.Controls.Add(mcProtocol);

            plcAgent = aPlcAgent;

            //plcAgent = new PlcAgent(mcProtocol, null);
            //一些Form Control要給進去Entity物件
            FormControlAddToEnityClass();
            //this.plcAgent.OnForkCommandExecutingEvent += PLCAgent_OnForkCommandExecutingEvent;
            //this.plcAgent.OnForkCommandFinishEvent += PLCAgent_OnForkCommandFinishEvent;
            //this.plcAgent.OnForkCommandErrorEvent += PLCAgent_OnForkCommandErrorEvent;
            //this.plcAgent.OnCassetteIDReadFinishEvent += PLCAgent_OnCassetteIDReadFinishEvent;
            //OnCassetteIDReadFinishEvent
            EventInitial();


            mcProtocol.LoadXMLConfig();

            mcProtocol.OperationMode = MCProtocol.enOperationMode.NORMAL_MODE;

            //aMCProtocol.Open("127.0.0.1", "3001");
            //aMCProtocol.ConnectToPLC("127.0.0.1", "5000");

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
            plcAgent.OnIpcAutoManualChangeEvent += PlcAgent_OnIpcAutoManualChangeEvent;
        }

        private Control findControlByID(string strID)
        {
            Control aControl = null;
            aControl = grpF.Controls[strID];
            if (aControl != null)
            {
                return aControl;
            }

            aControl = grpB.Controls[strID];
            if (aControl != null)
            {
                return aControl;
            }

            aControl = grpL.Controls[strID];
            if (aControl != null)
            {
                return aControl;
            }

            aControl = grpR.Controls[strID];
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
                aBeamSensor.FormLabel = (Label)findControlByID("lblBeamSensor" + aBeamSensor.Id);
                ((Label)findControlByID("lblBeamSensor" + aBeamSensor.Id)).Tag = aBeamSensor;
            }
        }

        private void FormControlAddToEnityClass()
        {
            //BeamSensor
            LabelAddToSideBeamSensor(this.plcAgent.APLCVehicle.listFrontBeamSensor);
            LabelAddToSideBeamSensor(this.plcAgent.APLCVehicle.listBackBeamSensor);
            LabelAddToSideBeamSensor(this.plcAgent.APLCVehicle.listLeftBeamSensor);
            LabelAddToSideBeamSensor(this.plcAgent.APLCVehicle.listRightBeamSensor);
            //Bumper
            foreach (PlcBumper aBumper in this.plcAgent.APLCVehicle.listBumper)
            {
                aBumper.FormLabel = (Label)findControlByID("lblBump" + aBumper.Id);
                ((Label)findControlByID("lblBump" + aBumper.Id)).Tag = aBumper;
            }
            //EMO
            foreach (PlcEmo aPlcEmo in this.plcAgent.APLCVehicle.listPlcEmo)
            {
                aPlcEmo.FormLabel = (Label)findControlByID("lblEMO" + aPlcEmo.Id);
                ((Label)findControlByID("lblEMO" + aPlcEmo.Id)).Tag = aPlcEmo;
            }
        }
        //
        private void PlcAgent_OnForkCommandErrorEvent(Object sender, PlcForkCommand aForkCommand)
        {
            triggerEvent = "PLCAgent_OnForkCommandErrorEvent";
        }

        private void PlcAgent_OnForkCommandExecutingEvent(Object sender, PlcForkCommand aForkCommand)
        {
            triggerEvent = "PLCAgent_OnForkCommandExecutingEvent";
        }

        private void PlcAgent_OnForkCommandFinishEvent(Object sender, PlcForkCommand aForkCommand)
        {
            triggerEvent = "PLCAgent_OnForkCommandFinishEvent";
        }

        private void PlcAgent_OnCassetteIDReadFinishEvent(Object sender, String cassetteID)
        {
            triggerEvent = "PLCAgent_OnCassetteIDReadFinishEvent cassetteID = " + cassetteID;
        }

        private string triggerEvent;
        private void McProtocol_OnDataChangeEvent(string sMessage, ClsMCProtocol.clsColParameter oColParam)
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

        private void frmPLCAgent_Load(object sender, EventArgs e)
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
            txtAutoChargeLowSOC.Text = this.plcAgent.APLCVehicle.Batterys.PortAutoChargeLowSoc.ToString();

            cmbEQPIO.Items.Clear();
            cmbEQPIO.Items.Add(bool.TrueString);
            cmbEQPIO.Items.Add(bool.FalseString);
            cmbEQPIO.SelectedIndex = 0;

            //this.WindowState = FormWindowState.Minimized;
            timGUIRefresh.Enabled = true;

            FillSVToBatteryParamTbx();
            FillPVToBatteryParamTbx();

            FillSVToForkCommParamTbx();
            FillPVToForkCommParamTbx();
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
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, true);
                System.Threading.Thread.Sleep(1000);
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, false);

            });
        }
        private void PlcAgent_OnIpcAutoManualChangeEvent(Object sender, EnumAutoState state)
        {
            IpcAutoState = state;            
        }
        private void IpcModeObjEnabled(bool status)
        {
            grpForkCommdAndStat.Enabled = status;
            grpCastIDReader.Enabled = status;
            grpForkCycleRun.Enabled = status;
            pnlCharg.Enabled = status;
            grpB.Enabled = status;
            grpL.Enabled = status;
            grpR.Enabled = status;
            grpF.Enabled = status;
            grpSafety.Enabled = status;
            grpAutoSleep.Enabled = status;
            pnlMove.Enabled = status;
            palForkParams.Enabled = status;
            palChargParams.Enabled = status;
            if (!status)
            {
                rdoSafetyEnable.Checked = true;
                rdoBeamSensorAutoSleepEnable.Checked = true;
                chkMoveFront.Checked = false;
                chkMoveBack.Checked = false;
                chkMoveLeft.Checked = false;
                chkMoveRight.Checked = false;
            }
        }
        private EnumAutoState beforeIpcAutoState;
        private void timGUIRefresh_Tick(object sender, EventArgs e)
        {
            labIPcStatus.Text = Enum.GetName(typeof(EnumAutoState), Vehicle.Instance.AutoState);
            //if (IpcAutoState == EnumAutoState.Auto)
            //{
            //    if (beforeIpcAutoState != IpcAutoState)
            //    {
            //        beforeIpcAutoState = IpcAutoState;
            //        IpcModeObjEnabled(false);
            //    }
            //}
            //else
            //{
            //    if (beforeIpcAutoState != IpcAutoState)
            //    {
            //        beforeIpcAutoState = IpcAutoState;
            //        IpcModeObjEnabled(true);
            //    }
            //}

            PlcForkCommand plcForkCommand = plcAgent.APLCVehicle.Robot.ExecutingCommand;
            if (plcForkCommand != null)
            {
                tbxCommandNo_PV.Text = plcForkCommand.CommandNo.ToString();
                tbxForkCommandType_PV.Text = plcForkCommand.ForkCommandType.ToString();
                tbxDirection_PV.Text = plcForkCommand.Direction.ToString();
                tbxStageNo_PV.Text = plcForkCommand.StageNo.ToString();
                tbxIsEqPio_PV.Text = plcForkCommand.IsEqPio.ToString();
                tbxForkSpeed_PV.Text = plcForkCommand.ForkSpeed.ToString();
                tbxReason_PV.Text = plcForkCommand.Reason.ToString();
            }
            else
            {
                tbxCommandNo_PV.Text = "Null";
                tbxForkCommandType_PV.Text = "Null";
                tbxDirection_PV.Text = "Null";
                tbxStageNo_PV.Text = "Null";
                tbxIsEqPio_PV.Text = "Null";
                tbxForkSpeed_PV.Text = "Null";
                tbxReason_PV.Text = "Null";
            }


            if (this.plcAgent.APLCVehicle.Robot.ForkHome)
                lblForkHome.BackColor = Color.LightGreen;
            else
                lblForkHome.BackColor = Color.Silver;

            tbxLogView.Text = plcAgent.logMsg;

            txtTriggerEvent.Text = triggerEvent;
            if (this.plcAgent.APLCVehicle.Batterys.BatteryType == EnumBatteryType.Gotech)
            {
                this.lblGotech.BackColor = Color.LightGreen;
            }
            else
            {
                this.lblGotech.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.Batterys.BatteryType == EnumBatteryType.Yinda)
            {
                this.lblYinda.BackColor = Color.LightGreen;
            }
            else
            {
                this.lblYinda.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.Batterys.Charging)
            {
                lblCharging.BackColor = Color.LightGreen;
            }
            else
            {
                lblCharging.BackColor = Color.Silver;
            }

            txtCurrent.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.MeterCurrent);
            txtVoltage.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.MeterVoltage);
            txtWatt.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.MeterWatt);
            txtWattHour.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.MeterWattHour);
            txtAH.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.MeterAh);
            txtSOC.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.Percentage);
            txtAHWorkingRange.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.AhWorkingRange);
            txtCCModeAH.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.CcModeAh);

            txtCCModeCounter.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.CcModeCounter);
            txtMaxCCmodeCounter.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.MaxResetAhCcounter);
            txtFullChargeIndex.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.FullChargeIndex);

            txtFBatteryTemp.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.FBatteryTemperature);
            txtBBatteryTemp.Text = Convert.ToString(this.plcAgent.APLCVehicle.Batterys.BBatteryTemperature);

            txtErrorReason.Text = this.plcAgent.getErrorReason();

            txtCassetteID.Text = this.plcAgent.APLCVehicle.CassetteId;

            if (this.plcAgent.APLCVehicle.Robot.ForkBusy)
            {
                lblForkBusy.BackColor = Color.LightGreen;
            }
            else
            {
                lblForkBusy.BackColor = Color.Silver;
            }
            if (this.plcAgent.APLCVehicle.Robot.ForkReady)
            {
                lblForkReady.BackColor = Color.LightGreen;
            }
            else
            {
                lblForkReady.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.Robot.ForkFinish)
            {
                lblForkFinish.BackColor = Color.LightGreen;
            }
            else
            {
                lblForkFinish.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.Loading)
            {
                lblLoading.BackColor = Color.LightGreen;
            }
            else
            {
                lblLoading.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.SafetyDisable)
            {
                tabSafety.BackColor = Color.Pink;
            }
            else
            {
                tabSafety.BackColor = Color.Transparent;
            }

            txtSafetyAction.Text = plcAgent.APLCVehicle.VehicleSafetyAction.ToString();
            //BeamSensor color
            showSideBeamcolor(plcAgent.APLCVehicle.listFrontBeamSensor);
            showSideBeamcolor(plcAgent.APLCVehicle.listBackBeamSensor);
            showSideBeamcolor(plcAgent.APLCVehicle.listLeftBeamSensor);
            showSideBeamcolor(plcAgent.APLCVehicle.listRightBeamSensor);

            //Bumper color
            foreach (PlcBumper aPLCBumper in plcAgent.APLCVehicle.listBumper)
            {
                if (aPLCBumper.Disable)
                {
                    aPLCBumper.FormLabel.BorderStyle = BorderStyle.None;
                }
                else
                {
                    aPLCBumper.FormLabel.BorderStyle = BorderStyle.FixedSingle;
                }

                if (!aPLCBumper.Signal)
                {
                    aPLCBumper.FormLabel.BackColor = this.lblNoDetect.BackColor;
                }
                else
                {
                    aPLCBumper.FormLabel.BackColor = this.lblNearDetect.BackColor;
                }
            }
            //20190730_Rudy 新增EMO Status 顯示
            if (plcAgent.APLCVehicle.PlcEmoStatus)
            {
                lblEMO.BackColor = Color.Pink;
            }
            else
            {
                lblEMO.BackColor = Color.LightGreen;
            }

            //EMO color
            foreach (PlcEmo aPlcEmo in plcAgent.APLCVehicle.listPlcEmo)
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

            if (plcAgent.APLCVehicle.MoveFront)
            {
                chkMoveFront.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveFront.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.MoveBack)
            {
                chkMoveBack.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveBack.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.MoveLeft)
            {
                chkMoveLeft.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveLeft.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.MoveRight)
            {
                chkMoveRight.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveRight.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.FrontBeamSensorDisable)
            {
                grpF.BackColor = Color.Pink;
            }
            else
            {
                grpF.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.BackBeamSensorDisable)
            {
                grpB.BackColor = Color.Pink;
            }
            else
            {
                grpB.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.LeftBeamSensorDisable)
            {
                grpL.BackColor = Color.Pink;
            }
            else
            {
                grpL.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.RightBeamSensorDisable)
            {
                grpR.BackColor = Color.Pink;
            }
            else
            {
                grpR.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.BeamSensorAutoSleep)
            {
                rdoBeamSensorAutoSleepEnable.BackColor = Color.LightGreen;
                rdoBeamSensorAutoSleepDisable.BackColor = Color.Transparent;
            }
            else
            {
                rdoBeamSensorAutoSleepEnable.BackColor = Color.Transparent;
                rdoBeamSensorAutoSleepDisable.BackColor = Color.LightGreen;
            }

            if (plcAgent.APLCVehicle.BumperAlarmStatus)
            {
                lblBumperAlarm.BackColor = Color.Pink;
            }
            else
            {
                lblBumperAlarm.BackColor = Color.LightGreen;
            }


        }

        private void showSideBeamcolor(List<PlcBeamSensor> listBeamSensor)
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
                    PlcForkCommand aForkCommand = new PlcForkCommand(Convert.ToUInt16(txtCommandNo.Text), (EnumForkCommand)Enum.Parse(typeof(EnumForkCommand), cmbOperationType.Text, false), txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
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
                plcAgent.clearExecutingForkCommand();
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
            this.plcAgent.triggerCassetteIDReader(ref CassetteID);
        }

        private void btnCycle_Click(object sender, EventArgs e)
        {
            if (btnCycle.Text == "Cycle Start")
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

        private UInt64 cycleForkCommandCount = 0;
        private UInt64 cycleChargeCommandCount = 0;

        private void timCycle_Tick(object sender, EventArgs e)
        {
            if (this.plcAgent.APLCVehicle.Robot.ForkReady && this.plcAgent.APLCVehicle.Robot.ForkBusy == false)
            {
                if (!plcAgent.IsForkCommandExist())
                {
                    //判斷loading 決定load/unload
                    if (plcAgent.APLCVehicle.Loading)
                    {
                        PlcForkCommand aForkCommand = new PlcForkCommand(Convert.ToUInt16(txtCommandNo.Text), EnumForkCommand.Unload, txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
                        plcAgent.AddForkComand(aForkCommand);
                        cycleForkCommandCount++;
                        txtCycleForkCommandCount.Text = cycleForkCommandCount.ToString();
                    }
                    else
                    {
                        PlcForkCommand aForkCommand = new PlcForkCommand(Convert.ToUInt16(txtCommandNo.Text), EnumForkCommand.Load, txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
                        plcAgent.AddForkComand(aForkCommand);
                        cycleForkCommandCount++;
                        txtCycleForkCommandCount.Text = cycleForkCommandCount.ToString();
                    }


                }
            }

            if (this.plcAgent.APLCVehicle.Batterys.Charging == false)
            {
                if (this.plcAgent.APLCVehicle.Batterys.Percentage < this.plcAgent.APLCVehicle.Batterys.PortAutoChargeLowSoc)
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
                this.plcAgent.APLCVehicle.SafetyDisable = true;
            }
            else
            {
                this.plcAgent.APLCVehicle.SafetyDisable = false;
            }
        }


        private void lblBump_DoubleClick(object sender, EventArgs e)
        {
            //PlcBumper aPLCBumper = (PlcBumper)((Label)sender).Tag;
            //aPLCBumper.Disable = !aPLCBumper.Disable;
        }

        private void lblBeamSensor_DoubleClick(object sender, EventArgs e)
        {
            PlcBeamSensor aPlcBeamSensor = (PlcBeamSensor)((Label)sender).Tag;
            aPlcBeamSensor.Disable = !aPlcBeamSensor.Disable;
        }


        private void chkMoveFront_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveFront.Checked)
            {
                plcAgent.APLCVehicle.MoveFront = true;

            }
            else
            {
                plcAgent.APLCVehicle.MoveFront = false;
            }
        }

        private void chkMoveBack_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveBack.Checked)
            {
                plcAgent.APLCVehicle.MoveBack = true;
            }
            else
            {
                plcAgent.APLCVehicle.MoveBack = false;
            }
        }

        private void chkMoveLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveLeft.Checked)
            {
                plcAgent.APLCVehicle.MoveLeft = true;
            }
            else
            {
                plcAgent.APLCVehicle.MoveLeft = false;
            }
        }

        private void chkMoveRight_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveRight.Checked)
            {
                plcAgent.APLCVehicle.MoveRight = true;

            }
            else
            {
                plcAgent.APLCVehicle.MoveRight = false;
            }
        }

        private void pnlF_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.APLCVehicle.FrontBeamSensorDisable)
            {
                plcAgent.APLCVehicle.FrontBeamSensorDisable = false;
            }
            else
            {
                plcAgent.APLCVehicle.FrontBeamSensorDisable = true;
            }
        }

        private void pnlL_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.APLCVehicle.LeftBeamSensorDisable)
            {
                plcAgent.APLCVehicle.LeftBeamSensorDisable = false;
            }
            else
            {
                plcAgent.APLCVehicle.LeftBeamSensorDisable = true;
            }
        }

        private void pnlB_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.APLCVehicle.BackBeamSensorDisable)
            {
                plcAgent.APLCVehicle.BackBeamSensorDisable = false;
            }
            else
            {
                plcAgent.APLCVehicle.BackBeamSensorDisable = true;
            }
        }

        private void pnlR_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.APLCVehicle.RightBeamSensorDisable)
            {
                plcAgent.APLCVehicle.RightBeamSensorDisable = false;
            }
            else
            {
                plcAgent.APLCVehicle.RightBeamSensorDisable = true;
            }
        }

        private void btnBeamSensorAutoSleepSet_Click(object sender, EventArgs e)
        {
            if (rdoBeamSensorAutoSleepEnable.Checked)
            {
                plcAgent.APLCVehicle.BeamSensorAutoSleep = true;
            }
            else
            {
                plcAgent.APLCVehicle.BeamSensorAutoSleep = false;
            }
        }



        private void btnSOCSet_Click(object sender, EventArgs e)
        {
            plcAgent.APLCVehicle.Batterys.SetCcModeAh(plcAgent.APLCVehicle.Batterys.MeterAh + plcAgent.APLCVehicle.Batterys.AhWorkingRange * (100.0 - Convert.ToDouble(txtSOCSet.Text)) / 100.00, false);
        }

        private void lblEMO_DoubleClick(object sender, EventArgs e)
        {
            PlcEmo aPlcEMOSensor = (PlcEmo)((Label)sender).Tag;
            aPlcEMOSensor.Disable = !aPlcEMOSensor.Disable;
        }

        private enum ParamtTbxType
        {
            BatteryPV,
            BatterySV,
            ForkCommPV,
            ForkCommSV
        }
        //20190802_Rudy 新增XML Param 可修改   
        private void BatteryParamTbxFillToList(ref List<TextBox> tboxes, ParamtTbxType TbxType = ParamtTbxType.BatteryPV)
        {
            switch (TbxType)
            {
                case ParamtTbxType.BatteryPV:
                    tboxes.Add(tbxCCModeStopVoltage_PV);
                    tboxes.Add(tbxChargingOffDelay_PV);
                    tboxes.Add(tbxBatterysChargingTimeOut_PV);
                    tboxes.Add(tbxBatLoggerInterval_PV);
                    tboxes.Add(tbxAHWorkingRange_PV);
                    tboxes.Add(tbxMaxCCModeCounter_PV);
                    tboxes.Add(txtAutoChargeLowSOC_PV);
                    tboxes.Add(tbxResetAHTimeout_PV);
                    break;
                case ParamtTbxType.BatterySV:
                    tboxes.Add(tbxCCModeStopVoltage_SV);
                    tboxes.Add(tbxChargingOffDelay_SV);
                    tboxes.Add(tbxBatterysChargingTimeOut_SV);
                    tboxes.Add(tbxBatLoggerInterval_SV);
                    tboxes.Add(tbxAHWorkingRange_SV);
                    tboxes.Add(tbxMaxCCModeCounter_SV);
                    tboxes.Add(txtAutoChargeLowSOC_SV);
                    tboxes.Add(tbxResetAHTimeout_SV);
                    break;
                case ParamtTbxType.ForkCommPV:
                    tboxes.Add(tbxReadCassetteID_PV);
                    tboxes.Add(tbxCommReadTimeout_PV);
                    tboxes.Add(tbxCommBusyTimeout_PV);
                    tboxes.Add(tbxCommMovingTimeout_PV);
                    break;
                case ParamtTbxType.ForkCommSV:
                    tboxes.Add(new TextBox());
                    tboxes.Add(tbxCommReadTimeout_SV);
                    tboxes.Add(tbxCommBusyTimeout_SV);
                    tboxes.Add(tbxCommMovingTimeout_SV);
                    break;
            }
        }
        private void FillPVToBatteryParamTbx()
        {
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.BatteryPV);

            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxCCModeStopVoltage_PV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.Batterys.CCModeStopVoltage)).ToString();
                        break;
                    case "tbxChargingOffDelay_PV":
                        box.Text = (this.plcAgent.APLCVehicle.Batterys.Charging_Off_Delay).ToString();
                        break;
                    case "tbxBatterysChargingTimeOut_PV":
                        box.Text = (this.plcAgent.APLCVehicle.Batterys.Batterys_Charging_Time_Out / 60000).ToString();
                        break;
                    case "tbxBatLoggerInterval_PV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.Batterys.Battery_Logger_Interval) / 1000).ToString();
                        break;
                    case "tbxAHWorkingRange_PV":
                        box.Text = this.plcAgent.APLCVehicle.Batterys.AhWorkingRange.ToString();
                        break;
                    case "tbxMaxCCModeCounter_PV":
                        box.Text = this.plcAgent.APLCVehicle.Batterys.MaxResetAhCcounter.ToString();
                        break;
                    case "txtAutoChargeLowSOC_PV":
                        box.Text = this.plcAgent.APLCVehicle.Batterys.PortAutoChargeLowSoc.ToString();
                        break;
                    case "tbxResetAHTimeout_PV":
                        box.Text = (this.plcAgent.APLCVehicle.Batterys.ResetAhTimeout / 1000).ToString();
                        break;
                }
            }
            liTextbox.Clear();
        }
        private void FillSVToBatteryParamTbx()
        {
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.BatterySV);

            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxCCModeStopVoltage_SV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.Batterys.CCModeStopVoltage)).ToString();
                        break;
                    case "tbxChargingOffDelay_SV":
                        box.Text = (this.plcAgent.APLCVehicle.Batterys.Charging_Off_Delay).ToString();
                        break;
                    case "tbxBatterysChargingTimeOut_SV":
                        box.Text = (this.plcAgent.APLCVehicle.Batterys.Batterys_Charging_Time_Out / 60000).ToString();
                        break;
                    case "tbxBatLoggerInterval_SV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.Batterys.Battery_Logger_Interval) / 1000).ToString();
                        break;
                    case "tbxAHWorkingRange_SV":
                        box.Text = this.plcAgent.APLCVehicle.Batterys.AhWorkingRange.ToString();
                        break;
                    case "tbxMaxCCModeCounter_SV":
                        box.Text = this.plcAgent.APLCVehicle.Batterys.MaxResetAhCcounter.ToString();
                        break;
                    case "txtAutoChargeLowSOC_SV":
                        box.Text = this.plcAgent.APLCVehicle.Batterys.PortAutoChargeLowSoc.ToString();
                        break;
                    case "tbxResetAHTimeout_SV":
                        box.Text = (this.plcAgent.APLCVehicle.Batterys.ResetAhTimeout / 1000).ToString();
                        break;
                }
            }
            liTextbox.Clear();
        }
        private void FillPVToForkCommParamTbx()
        {
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.ForkCommPV);

            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxReadCassetteID_PV":
                        if (this.plcAgent.IsNeedReadCassetteID)
                            box.Text = "TRUE";
                        else
                            box.Text = "FALSE";
                        break;
                    case "tbxCommReadTimeout_PV":
                        box.Text = (this.plcAgent.ForkCommandReadTimeout / 1000).ToString();
                        break;
                    case "tbxCommBusyTimeout_PV":
                        box.Text = (this.plcAgent.ForkCommandBusyTimeout / 1000).ToString();
                        break;
                    case "tbxCommMovingTimeout_PV":
                        box.Text = (this.plcAgent.ForkCommandMovingTimeout / 1000).ToString();
                        break;
                }
            }
            liTextbox.Clear();
        }

        private void FillSVToForkCommParamTbx()
        {
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.ForkCommSV);

            if (this.plcAgent.IsNeedReadCassetteID)
            {
                chbCassetteID_SV.Checked = true;
                chbCassetteID_SV.Text = "TRUE";
            }
            else
            {
                chbCassetteID_SV.Checked = false;
                chbCassetteID_SV.Text = "FALSE";
            }

            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxCommReadTimeout_SV":
                        box.Text = (this.plcAgent.ForkCommandReadTimeout / 1000).ToString();
                        break;
                    case "tbxCommBusyTimeout_SV":
                        box.Text = (this.plcAgent.ForkCommandBusyTimeout / 1000).ToString();
                        break;
                    case "tbxCommMovingTimeout_SV":
                        box.Text = (this.plcAgent.ForkCommandMovingTimeout / 1000).ToString();
                        break;
                }
            }
            liTextbox.Clear();
        }
        private bool CheckBatteryParamSVInput()
        {
            bool result = true;
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.BatterySV);
            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxCCModeStopVoltage_SV":
                        {
                            if (!double.TryParse(box.Text, out double value))
                            {
                                box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.Batterys.CCModeStopVoltage)).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxChargingOffDelay_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.APLCVehicle.Batterys.Charging_Off_Delay).ToString();
                                result = false;
                            }
                        }
                        break;

                    case "tbxBatterysChargingTimeOut_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.APLCVehicle.Batterys.Batterys_Charging_Time_Out / 60000).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxBatLoggerInterval_SV":
                        {
                            if (!double.TryParse(box.Text, out double value))
                            {
                                box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.Batterys.Battery_Logger_Interval) / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxAHWorkingRange_SV":
                        {
                            if (!double.TryParse(box.Text, out double value))
                            {
                                box.Text = this.plcAgent.APLCVehicle.Batterys.AhWorkingRange.ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxMaxCCModeCounter_SV":
                        {
                            if (!ushort.TryParse(box.Text, out ushort value))
                            {
                                box.Text = this.plcAgent.APLCVehicle.Batterys.MaxResetAhCcounter.ToString();
                                result = false;
                            }
                        }
                        break;
                    case "txtAutoChargeLowSOC_SV":
                        {
                            if (!double.TryParse(box.Text, out double value))
                            {
                                box.Text = this.plcAgent.APLCVehicle.Batterys.PortAutoChargeLowSoc.ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxResetAHTimeout_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.APLCVehicle.Batterys.ResetAhTimeout / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                }
            }
            liTextbox.Clear();
            return result;
        }
        private bool CheckForkCommParamSVInput()
        {
            bool result = true;
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.ForkCommSV);
            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxCommReadTimeout_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.ForkCommandReadTimeout / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxCommBusyTimeout_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.ForkCommandBusyTimeout / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxCommMovingTimeout_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.ForkCommandMovingTimeout / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                }
            }
            liTextbox.Clear();
            return result;
        }
        private void btnBatteryParamSet_Click(object sender, EventArgs e)
        {
            if (!CheckBatteryParamSVInput()) return;
            Dictionary<string, string> dicSetValue = new Dictionary<string, string>()
            {
                {"CCMode_Stop_Voltage",tbxCCModeStopVoltage_SV.Text},
                {"Charging_Off_Delay",tbxChargingOffDelay_SV.Text},
                {"Batterys_Charging_Time_Out",tbxBatterysChargingTimeOut_SV.Text},
                {"Battery_Logger_Interval",tbxBatLoggerInterval_SV.Text },
                {"SOC_AH",tbxAHWorkingRange_SV.Text },
                {"Ah_Reset_CCmode_Counter", tbxMaxCCModeCounter_SV.Text},
                {"Port_AutoCharge_Low_SOC",txtAutoChargeLowSOC_SV.Text},
                {"Ah_Reset_Timeout", tbxResetAHTimeout_SV.Text}
            };
            plcAgent.WritePlcConfigToXML(dicSetValue);
            FillPVToBatteryParamTbx();
            FillSVToBatteryParamTbx();
            dicSetValue.Clear();
        }

        private void btnForkCommParamSet_Click(object sender, EventArgs e)
        {
            if (!CheckForkCommParamSVInput()) return;
            string strReadCassetteID = "";
            if (chbCassetteID_SV.Checked) strReadCassetteID = "true"; else strReadCassetteID = "false";

            Dictionary<string, string> dicSetValue = new Dictionary<string, string>()
            {
                {"IsNeedReadCassetteID", strReadCassetteID},
                {"Fork_Command_Read_Timeout", tbxCommReadTimeout_SV.Text},
                {"Fork_Command_Busy_Timeout",tbxCommBusyTimeout_SV.Text},
                {"Fork_Command_Moving_Timeout", tbxCommMovingTimeout_SV.Text}
            };
            plcAgent.WritePlcConfigToXML(dicSetValue);
            FillPVToForkCommParamTbx();
            FillSVToForkCommParamTbx();
            dicSetValue.Clear();
        }
        private void chbCassetteID_SV_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked) ((CheckBox)sender).Text = "TRUE";
            else ((CheckBox)sender).Text = "FALSE";
        }

        private void btnFormHide_Click(object sender, EventArgs e)
        {
            Hide();
        }
        private bool bIPcStatusChange = false;
        private void labIPcStatusManual_Click(object sender, EventArgs e)
        {
            bIPcStatusChange = !bIPcStatusChange;
            if (bIPcStatusChange)
                Vehicle.Instance.AutoState = EnumAutoState.Manual;
            else
                Vehicle.Instance.AutoState = EnumAutoState.Auto;
        }
    }
}
