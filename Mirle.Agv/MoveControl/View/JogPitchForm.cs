using Mirle.AgvAseMiddler.Controller;
using Mirle.AgvAseMiddler.Model;
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
using Mirle.AgvAseMiddler.View;

namespace Mirle.AgvAseMiddler.View
{
    public partial class JogPitchForm : Form
    {
        public JogPitchData jogPitchData = new JogPitchData();

        private Thread ontimeRevise;
        private EnumJogPitchMode mode;
        private AgvMoveRevise agvRevise;
        private bool changingMode = false;
        private int wheelAngle = 0;
        private double velocityCommand = 0;
        private ComputeFunction computeFunction = new ComputeFunction();
        private EnumAxis[] AxisList = new EnumAxis[18] {EnumAxis.XFL, EnumAxis.XFR, EnumAxis.XRL, EnumAxis.XRR,
                                                EnumAxis.TFL, EnumAxis.TFR, EnumAxis.TRL, EnumAxis.TRR,
                                                EnumAxis.VXFL, EnumAxis.VXFR, EnumAxis.VXRL, EnumAxis.VXRR,
                                                EnumAxis.VTFL, EnumAxis.VTFR, EnumAxis.VTRL, EnumAxis.VTRR,
                                                EnumAxis.GX, EnumAxis.GT};
        private bool formEnable = true;
        public bool CanAuto { get; set; }
        public string CantAutoResult { get; set; } = "";

        private int retryTimes = 2;
        private string elmoAllResetMessage = "";
        private string temp;

        public JogPitchForm(MoveControlHandler moveControl)
        {
            if (moveControl == null)
                return;

            this.moveControl = moveControl;
            InitializeComponent();
            ChangeMode(EnumJogPitchMode.Normal);
            agvRevise = new AgvMoveRevise(moveControl.ontimeReviseConfig, moveControl.elmoDriver,
                                          moveControl.DriverSr2000List);

            this.allAxis = new Mirle.AgvAseMiddler.JogPitchAxis[AxisList.Count()];

            for (int i = 0; i < AxisList.Count(); i++)
            {
                this.allAxis[i] = new Mirle.AgvAseMiddler.JogPitchAxis(AxisList[i].ToString());

                this.allAxis[i].BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));

                if (i < 4)
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * i, 43);
                else if (i < 8)
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * (i - 4), 43 + 110 * 1);
                else if (i < 12)
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * (i - 8), 43 + 110 * 2);
                else if (i < 16)
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * (i - 12), 43 + 110 * 3);
                else
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * 4, 43 + 110 * (i - 16));

                this.allAxis[i].Name = AxisList[i].ToString();
                this.allAxis[i].Size = new System.Drawing.Size(92, 95);
                this.allAxis[i].TabIndex = 133;

                this.Controls.Add(this.allAxis[i]);
                cB_JogPitch_SelectAxis.Items.Add(AxisList[i].ToString());
            }

            cB_JogPitch_SelectAxis.SelectedIndex = AxisList.Count() - 2;
            label_SR2000Connected.Text = moveControl.SR2000Connected ? "" : "SR2000連線失敗!";
        }

        private MoveControlHandler moveControl = null;

        private void EnableDisableForm(bool flag)
        {
            if (formEnable == flag)
                return;

            formEnable = flag;
            foreach (Control control in this.Controls)
            {
                control.Enabled = flag;
            }

            label_LockResult.Enabled = true;
            button_JogPitch_ChangeFormSize.Enabled = true;
            button_JogPitchHide.Enabled = true;
        }

        private void UpdateCanAuto()
        {
            int errorcode = 0;
            bool canAuto = true;
            string cantAutoResult = "";


            label_AGVStateValue.Text = moveControl.MoveState.ToString();
            label_AGVStateValue.ForeColor = (label_AGVStateValue.Text == EnumMoveState.Idle.ToString() ? Color.Green : Color.Red);
            if (label_AGVStateValue.Text != EnumMoveState.Idle.ToString())
            {
                canAuto = false;


                if (cantAutoResult == "")
                {
                    if (label_AGVStateValue.Text == EnumMoveState.Error.ToString())
                        cantAutoResult = "AGV Error狀態!";
                    else
                        cantAutoResult = "流程移動中!";
                }
            }

            if (moveControl.location.Real == null)
            {
                if (cantAutoResult == "")
                    cantAutoResult = "迷航中,沒讀取到鐵Barcode!";

                label_ReadIronValue.Text = "無";
                label_ReadIronValue.ForeColor = Color.Red;
                canAuto = false;
            }
            else
            {
                label_ReadIronValue.Text = "有";
                label_ReadIronValue.ForeColor = Color.Green;
            }

            if (Vehicle.Instance.VehicleLocation.LastAddress.Id == "" || Vehicle.Instance.VehicleLocation.LastSection.Id == "")
            {
                if (cantAutoResult == "")
                    cantAutoResult = "迷航中,認不出目前所在Address、Section!";

                label_CheckAddressSectionValue.Text = "-----";
                label_CheckAddressSectionValue.ForeColor = Color.Red;
                canAuto = false;
            }
            else
            {
                label_CheckAddressSectionValue.Text = Vehicle.Instance.VehicleLocation.LastAddress.Id;
                label_CheckAddressSectionValue.ForeColor = Color.Green;
            }

            if (!moveControl.elmoDriver.CheckAxisNoError(ref errorcode))
            {
                if (cantAutoResult == "")
                    cantAutoResult = "有軸異常!";

                label_AxisErrorValue.Text = "有";
                label_AxisErrorValue.ForeColor = Color.Red;
                canAuto = false;
            }
            else
            {
                label_AxisErrorValue.Text = "無";
                label_AxisErrorValue.ForeColor = Color.Green;
            }

            if (!moveControl.elmoDriver.ElmoAxisTypeAllServoOn(EnumAxisType.Turn))
            {
                if (cantAutoResult == "")
                    cantAutoResult = "請Enable所有軸!";

                label_TurnServoOnValue.Text = "無";
                label_TurnServoOnValue.ForeColor = Color.Red;
                canAuto = false;
            }
            else
            {
                label_TurnServoOnValue.Text = "有";
                label_TurnServoOnValue.ForeColor = Color.Green;
            }

            if (!moveControl.elmoDriver.MoveAxisStop())
            {
                if (cantAutoResult == "")
                    cantAutoResult = "AGV移動中!";

                label_MoveStopValue.Text = "Moving";
                label_MoveStopValue.ForeColor = Color.Red;
                canAuto = false;
            }
            else
            {
                label_MoveStopValue.Text = "Stop";
                label_MoveStopValue.ForeColor = Color.Green;
            }

            label_CanAutoValue.Text = (canAuto ? "可以" : "不能");
            label_CanAutoValue.ForeColor = (canAuto ? Color.Green : Color.Red);

            CanAuto = canAuto;
            CantAutoResult = cantAutoResult;
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if (moveControl == null)
                return;

            if (Vehicle.Instance.AutoState == EnumAutoState.Manual)
                UpdateCanAuto();
            
            #region Update Sr2000
            AGVPosition agvPositionL = null;
            AGVPosition agvPositionR = null;
            ThetaSectionDeviation thetaSectionDeviationL = null;
            ThetaSectionDeviation thetaSectionDeviationR = null;

            if (moveControl.DriverSr2000List.Count > 0)
            {
                agvPositionL = moveControl.DriverSr2000List[0].GetAGVPosition();
                if (agvPositionL != null)
                {
                    tB_JogPitch_MapX_L.Text = agvPositionL.Position.X.ToString("0.0");
                    tB_JogPitch_MapY_L.Text = agvPositionL.Position.Y.ToString("0.0");
                    tB_JogPitch_MapTheta_L.Text = agvPositionL.AGVAngle.ToString("0.0");
                }
                else
                {
                    tB_JogPitch_MapX_L.Text = "-----";
                    tB_JogPitch_MapY_L.Text = "-----";
                    tB_JogPitch_MapTheta_L.Text = "-----";
                }

                thetaSectionDeviationL = moveControl.DriverSr2000List[0].GetThetaSectionDeviation();
                if (thetaSectionDeviationL != null)
                {
                    tB_JogPitch_SectionDeviation_L.Text = thetaSectionDeviationL.SectionDeviation.ToString("0.0");
                    tB_JogPitch_Theta_L.Text = thetaSectionDeviationL.Theta.ToString("0.0");
                }
                else
                {
                    tB_JogPitch_SectionDeviation_L.Text = "-----";
                    tB_JogPitch_Theta_L.Text = "-----";
                }
            }

            if (moveControl.DriverSr2000List.Count > 1)
            {
                agvPositionR = moveControl.DriverSr2000List[1].GetAGVPosition();
                if (agvPositionR != null)
                {
                    tB_JogPitch_MapX_R.Text = agvPositionR.Position.X.ToString("0.0");
                    tB_JogPitch_MapY_R.Text = agvPositionR.Position.Y.ToString("0.0");
                    tB_JogPitch_MapTheta_R.Text = agvPositionR.AGVAngle.ToString("0.0");
                }
                else
                {
                    tB_JogPitch_MapX_R.Text = "-----";
                    tB_JogPitch_MapY_R.Text = "-----";
                    tB_JogPitch_MapTheta_R.Text = "-----";
                }

                thetaSectionDeviationR = moveControl.DriverSr2000List[1].GetThetaSectionDeviation();
                if (thetaSectionDeviationR != null)
                {
                    tB_JogPitch_SectionDeviation_R.Text = thetaSectionDeviationR.SectionDeviation.ToString("0.0");
                    tB_JogPitch_Theta_R.Text = thetaSectionDeviationR.Theta.ToString("0.0");
                }
                else
                {
                    tB_JogPitch_SectionDeviation_R.Text = "-----";
                    tB_JogPitch_Theta_R.Text = "-----";
                }
            }

            if ((agvPositionL != null && agvPositionL.Type == EnumBarcodeMaterial.Iron) ||
                (agvPositionR != null && agvPositionR.Type == EnumBarcodeMaterial.Iron))
            {
                if (agvPositionL != null && agvPositionL.Type == EnumBarcodeMaterial.Iron)
                {
                    jogPitchData.MapX = agvPositionL.Position.X;
                    jogPitchData.MapY = agvPositionL.Position.Y;
                    jogPitchData.MapTheta = agvPositionL.AGVAngle;
                }
                else
                {
                    jogPitchData.MapX = agvPositionR.Position.X;
                    jogPitchData.MapY = agvPositionR.Position.Y;
                    jogPitchData.MapTheta = agvPositionR.AGVAngle;
                }
            }
            else
            {
                jogPitchData.MapX = 0;
                jogPitchData.MapY = 0;
                jogPitchData.MapTheta = 0;
            }

            if (thetaSectionDeviationL != null || thetaSectionDeviationR != null)
            {
                if (thetaSectionDeviationL != null)
                {
                    jogPitchData.SectionDeviation = thetaSectionDeviationL.SectionDeviation;
                    jogPitchData.Theta = thetaSectionDeviationL.Theta;
                }
                else
                {
                    jogPitchData.SectionDeviation = thetaSectionDeviationR.SectionDeviation;
                    jogPitchData.Theta = thetaSectionDeviationR.Theta;
                }
            }
            else
            {
                jogPitchData.SectionDeviation = 0;
                jogPitchData.Theta = 0;
            }
            #endregion

            #region Update Elmo Status
            ElmoAxisFeedbackData tempData;
            bool standStill;
            string position;
            bool link;
            for (int i = 0; i < AxisList.Count(); i++)
            {
                tempData = moveControl.elmoDriver.ElmoGetFeedbackData(AxisList[i]);

                try
                {
                    if (AxisList[i] == EnumAxis.GX || AxisList[i] == EnumAxis.GT)
                        tempData.StandStill = moveControl.elmoDriver.MoveCompelete(AxisList[i]);

                    jogPitchData.AxisData[AxisList[i]] = tempData;
                }
                catch
                {
                    jogPitchData.AxisData[AxisList[i]] = null;
                }

                if (tempData != null)
                {
                    position = tempData.Feedback_Position.ToString("0");
                    link = moveControl.elmoDriver.GetLink(AxisList[i]);

                    if (AxisList[i] == EnumAxis.GX || AxisList[i] == EnumAxis.GT)
                        standStill = moveControl.elmoDriver.MoveCompelete(AxisList[i]);
                    else
                        standStill = tempData.StandStill;

                    allAxis[i].Update(position, tempData.Disable, standStill, tempData.ErrorStop, link);
                }
            }
            #endregion

            if (changingMode)
            {
                if (moveControl.elmoDriver.MoveAxisStop() && moveControl.elmoDriver.TurnAxisStop())
                {
                    changingMode = false;
                    EnalbeDisableButton(true);
                }
            }

            string lockResult = "";

            bool notLock = false;
            bool skipLock = false;

            if (!moveControl.elmoDriver.Connected)
                lockResult = "Lock Result : Elmo drive連線失敗!";
            else if (Vehicle.Instance.AutoState != EnumAutoState.Manual)
                lockResult = "Lock Result : AutoMode中!";
            //else if (Vehicle.Instance.VisitTransferStepsStatus != EnumThreadStatus.None &&
            //         Vehicle.Instance.VisitTransferStepsStatus != EnumThreadStatus.Stop)
            //    lockResult = "Lock Result : 主流程動作中!";
            else if (moveControl.MoveState != EnumMoveState.Idle)
                lockResult = "Lock Result : MoveState動作中!";
            else if (/*button_Skip.Text != "強制\r\n手動" &&*/ moveControl.IsCharging())
            {
                skipLock = true;
                lockResult = "Lock Result : Charging中!";
            }
            else if (button_Skip.Text != "強制\r\n手動" && moveControl.ForkNotHome())
            {
                skipLock = true;
                lockResult = "Lock Result : Fork不在Home點!";
            }
            else
            {
                skipLock = true;
                notLock = true;
            }

            label_LockResult.Text = lockResult;

            EnableDisableForm(notLock);

            if (skipLock)
            {
                if (!gB_JogPitch_ElmoFunction.Enabled)
                    gB_JogPitch_ElmoFunction.Enabled = true;

                if (!button_Skip.Enabled)
                    button_Skip.Enabled = true;
            }
            else
            {
                if (button_Skip.Text == "強制\r\n手動")
                {
                    button_Skip.Text = "一般\r\n模式";
                    button_Skip.BackColor = (button_Skip.Text == "強制\r\n手動") ? Color.Red : Color.Transparent;
                }
            }
        }

        private void EnalbeDisableButton(bool flag)
        {
            button_JogPitch_TurnLeft.Enabled = flag;
            button_JogPitch_TurnRight.Enabled = flag;
            button_JogPitch_Forward.Enabled = flag;
            button_JogPitch_Backward.Enabled = flag;
            button_JogPitch_Normal.Enabled = flag;
            button_JogPitch_ForwardWheel.Enabled = flag;
            button_JogPitch_BackwardWheel.Enabled = flag;
            button_JogPitch_SpinTurn.Enabled = flag;
            button_JogpitchResetAll.Enabled = flag;

            if (flag)
            {
                switch (mode)
                {
                    case EnumJogPitchMode.Normal:
                        button_JogPitch_Normal.Enabled = false;
                        break;
                    case EnumJogPitchMode.ForwardWheel:
                        button_JogPitch_ForwardWheel.Enabled = false;
                        break;
                    case EnumJogPitchMode.BackwardWheel:
                        button_JogPitch_BackwardWheel.Enabled = false;
                        break;
                    case EnumJogPitchMode.SpinTurn:
                        button_JogPitch_SpinTurn.Enabled = false;
                        button_JogPitch_TurnLeft.Enabled = false;
                        button_JogPitch_TurnRight.Enabled = false;
                        break;
                    default:
                        break;
                }
            }
        }


        private void ChangeMode(EnumJogPitchMode changemode)
        {
            EnalbeDisableButton(false);
            rB_JogPitch_TurnSpeed_High.Checked = true;
            mode = changemode;
            label_JogPich_NowMode.Text = ((EnumJogPitchModeName)(int)mode).ToString();

            switch (mode)
            {
                case EnumJogPitchMode.Normal:
                case EnumJogPitchMode.ForwardWheel:
                case EnumJogPitchMode.BackwardWheel:
                    button_JogPitch_Forward.Text = "前進";
                    button_JogPitch_Backward.Text = "後退";
                    moveControl.elmoDriver.ElmoStop(EnumAxis.GX, moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);
                    moveControl.elmoDriver.ElmoMove(EnumAxis.GT, 0, moveControl.moveControlConfig.Turn.Velocity, EnumMoveType.Absolute,
                        moveControl.moveControlConfig.Turn.Acceleration, moveControl.moveControlConfig.Turn.Deceleration, moveControl.moveControlConfig.Turn.Jerk);
                    tB_JogPitch_Distance.Text = "1000";
                    rB_JogPitch_MoveVelocity_100.Checked = true;
                    rB_JogPitch_MoveVelocity_300.Enabled = true;
                    break;
                case EnumJogPitchMode.SpinTurn:
                    button_JogPitch_Forward.Text = "原地左轉";
                    button_JogPitch_Backward.Text = "原地右轉";
                    moveControl.elmoDriver.ElmoStop(EnumAxis.GX, moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);
                    moveControl.elmoDriver.ElmoMove(EnumAxis.GT, -59.744, 59.744, 59.744, -59.744, moveControl.moveControlConfig.Turn.Velocity, EnumMoveType.Absolute,
                        moveControl.moveControlConfig.Turn.Acceleration, moveControl.moveControlConfig.Turn.Deceleration, moveControl.moveControlConfig.Turn.Jerk);
                    tB_JogPitch_Distance.Text = "1000";
                    rB_JogPitch_MoveVelocity_10.Checked = true;
                    rB_JogPitch_MoveVelocity_300.Enabled = false;
                    break;
                default:
                    break;
            }

            Thread.Sleep(100);
            changingMode = true;
        }

        private void OntimeReviseThread()
        {
            double[] reviseWheelAngle = new double[4];
            Thread.Sleep(20);
            string str = "";

            while (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GX))
            {
                if (agvRevise.OntimeRevise(ref reviseWheelAngle, wheelAngle, velocityCommand, ref str))
                {
                    moveControl.elmoDriver.ElmoMove(EnumAxis.GT, reviseWheelAngle[0], reviseWheelAngle[1], reviseWheelAngle[2], reviseWheelAngle[3],
                        moveControl.ontimeReviseConfig.ThetaSpeed, EnumMoveType.Absolute,
                        moveControl.moveControlConfig.Turn.Acceleration, moveControl.moveControlConfig.Turn.Deceleration,
                        moveControl.moveControlConfig.Turn.Jerk);
                }

                Thread.Sleep(50);
            }

            moveControl.elmoDriver.ElmoMove(EnumAxis.GT, 0, moveControl.ontimeReviseConfig.ThetaSpeed, EnumMoveType.Absolute,
                moveControl.moveControlConfig.Turn.Acceleration, moveControl.moveControlConfig.Turn.Deceleration,
                moveControl.moveControlConfig.Turn.Jerk);
        }

        public void button_JogPitch_STOP_Click(object sender, EventArgs e)
        {
            moveControl.elmoDriver.ElmoStop(EnumAxis.VTFL);
            moveControl.elmoDriver.ElmoStop(EnumAxis.VTFR);
            moveControl.elmoDriver.ElmoStop(EnumAxis.VTRL);
            moveControl.elmoDriver.ElmoStop(EnumAxis.VTRR);

            moveControl.elmoDriver.ElmoStop(EnumAxis.VXFL);
            moveControl.elmoDriver.ElmoStop(EnumAxis.VXFR);
            moveControl.elmoDriver.ElmoStop(EnumAxis.VXRL);
            moveControl.elmoDriver.ElmoStop(EnumAxis.VXRR);

            moveControl.elmoDriver.ElmoMove(EnumAxis.VTFL, 0, 10, EnumMoveType.Absolute);
            moveControl.elmoDriver.ElmoMove(EnumAxis.VTFR, 0, 10, EnumMoveType.Absolute);
            moveControl.elmoDriver.ElmoMove(EnumAxis.VTRL, 0, 10, EnumMoveType.Absolute);
            moveControl.elmoDriver.ElmoMove(EnumAxis.VTRR, 0, 10, EnumMoveType.Absolute);
            ChangeMode(EnumJogPitchMode.Normal);
        }

        public void button_JogPitch_Normal_Click(object sender, EventArgs e)
        {
            ChangeMode(EnumJogPitchMode.Normal);
        }

        public void button_JogPitch_ForwardWheel_Click(object sender, EventArgs e)
        {
            ChangeMode(EnumJogPitchMode.ForwardWheel);
        }

        public void button_JogPitch_BackwardWheel_Click(object sender, EventArgs e)
        {
            ChangeMode(EnumJogPitchMode.BackwardWheel);
        }

        public void button_JogPitch_SpinTurn_Click(object sender, EventArgs e)
        {
            ChangeMode(EnumJogPitchMode.SpinTurn);
        }

        private void CheckTurnData(ref double vel, ref double acc, ref double dec, ref double jerk)
        {
            vel = moveControl.moveControlConfig.Turn.Velocity;
            acc = moveControl.moveControlConfig.Turn.Acceleration;
            dec = moveControl.moveControlConfig.Turn.Deceleration;
            jerk = moveControl.moveControlConfig.Turn.Jerk;

            if (rB_JogPitch_TurnSpeed_Low.Checked)
            {
                vel /= 10;
                acc /= 5;
                dec /= 5;
                jerk /= 5;
            }
            else if (rB_JogPitch_TurnSpeed_Medium.Checked)
            {
                vel /= 5;
                acc /= 2;
                dec /= 2;
                jerk /= 2;
            }
        }

        public void button_JogPitch_Turn_MouseUp(object sender, MouseEventArgs e)
        {
            moveControl.elmoDriver.ElmoStop(EnumAxis.GT, moveControl.moveControlConfig.Turn.Deceleration, moveControl.moveControlConfig.Turn.Jerk);
            moveControl.location.Real = null;
        }

        private void Turn_MouseDown(double angle)
        {
            if (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GT) || !moveControl.elmoDriver.MoveCompelete(EnumAxis.GX))
                return;

            double vel = 0, acc = 0, dec = 0, jerk = 0;
            CheckTurnData(ref vel, ref acc, ref dec, ref jerk);

            double[] turn;

            switch (mode)
            {
                case EnumJogPitchMode.Normal:
                    turn = new double[4] { angle, angle, angle, angle };
                    break;
                case EnumJogPitchMode.ForwardWheel:
                    turn = new double[4] { angle, angle, 0, 0 };
                    break;
                case EnumJogPitchMode.BackwardWheel:
                    turn = new double[4] { 0, 0, angle, angle };
                    break;
                default:
                    turn = new double[4] { 0, 0, 0, 0 };
                    break;
            }

            moveControl.elmoDriver.ElmoMove(EnumAxis.GT, turn[0], turn[1], turn[2], turn[3], vel, EnumMoveType.Absolute, acc, dec, jerk);
        }

        public void button_JogPitch_TurnRight_MouseDown(object sender, MouseEventArgs e)
        {
            Turn_MouseDown(-90);
        }

        public void button_JogPitch_TurnLeft_MouseDown(object sender, MouseEventArgs e)
        {
            Turn_MouseDown(90);
        }

        private void rB_JogPitch_MoveVelocity_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_JogPitch_MoveVelocity_10.Checked)
                tB_JogPitch_Velocity.Text = "10";
            else if (rB_JogPitch_MoveVelocity_50.Checked)
                tB_JogPitch_Velocity.Text = "50";
            else if (rB_JogPitch_MoveVelocity_100.Checked)
                tB_JogPitch_Velocity.Text = "100";
            else if (rB_JogPitch_MoveVelocity_300.Checked)
                tB_JogPitch_Velocity.Text = "300";
            else
                tB_JogPitch_Velocity.Text = "100";
        }

        private bool GetVelocityAndDistance(ref double velocity, ref double distance)
        {
            if (double.TryParse(tB_JogPitch_Distance.Text, out distance) && distance > 0 &&
                double.TryParse(tB_JogPitch_Velocity.Text, out velocity) && velocity > 0)
                return true;
            else
                return false;
        }

        private void Move_MouseDown(bool flag)
        {
            if (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GT) || !moveControl.elmoDriver.MoveCompelete(EnumAxis.GX))
                return;

            double velocity = 0, distance = 0;
            if (!GetVelocityAndDistance(ref velocity, ref distance))
                return;

            if (mode == EnumJogPitchMode.SpinTurn)
            { // turn left
                double[] move;
                if (flag) // right
                    move = new double[4] { -distance, distance, -distance, distance };
                else
                    move = new double[4] { distance, -distance, distance, -distance };

                moveControl.elmoDriver.ElmoMove(EnumAxis.GX, move[0], move[1], move[2], move[3], velocity, EnumMoveType.Relative, moveControl.moveControlConfig.Move.Acceleration,
                                         moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);
            }
            else if (cB_JogPitch_MoveAndOntimeRevise.Checked)
            {
                agvRevise.SettingReviseData(velocity, flag);

                double vel = 0, acc = 0, dec = 0, jerk = 0;
                CheckTurnData(ref vel, ref acc, ref dec, ref jerk);
                if (moveControl.elmoDriver.WheelAngleCompare(90, 10))
                    wheelAngle = 90;
                else if (moveControl.elmoDriver.WheelAngleCompare(-90, 10))
                    wheelAngle = -90;
                else
                    wheelAngle = 0;

                velocityCommand = velocity;
                moveControl.elmoDriver.ElmoMove(EnumAxis.GX, (flag ? distance : -distance), velocity, EnumMoveType.Relative,
                    moveControl.moveControlConfig.Move.Acceleration, moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);

                if (wheelAngle == 0)
                {
                    ontimeRevise = new Thread(OntimeReviseThread);
                    ontimeRevise.Start();
                }
            }
            else
            {
                moveControl.elmoDriver.ElmoMove(EnumAxis.GX, (flag ? distance : -distance), velocity, EnumMoveType.Relative,
                    moveControl.moveControlConfig.Move.Acceleration, moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);
            }
        }

        public void button_JogPitch_Forward_MouseDown(object sender, MouseEventArgs e)
        {
            Move_MouseDown(true);
        }

        public void button_JogPitch_Backward_MouseDown(object sender, MouseEventArgs e)
        {
            Move_MouseDown(false);
        }

        public void button_JogPitch_Move_MouseUp(object sender, MouseEventArgs e)
        {
            moveControl.elmoDriver.ElmoStop(EnumAxis.GX, moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);
            moveControl.location.Real = null;
        }

        public void button_JogPitch_ElmoEnable_Click(object sender, EventArgs e)
        {
            jogPitchData.ElmoFunctionCompelete = false;
            Task.Factory.StartNew(() =>
            {
                moveControl.elmoDriver.EnableAllAxis();
                jogPitchData.ElmoFunctionCompelete = true;
            });
        }

        public void button_JogPitch_ElmoDisable_Click(object sender, EventArgs e)
        {
            jogPitchData.ElmoFunctionCompelete = false;
            Task.Factory.StartNew(() =>
            {
                moveControl.elmoDriver.DisableAllAxis();
                jogPitchData.ElmoFunctionCompelete = true;
            });
        }

        public void button_JogPitch_ElmoReset_Click(object sender, EventArgs e)
        {
            jogPitchData.ElmoFunctionCompelete = false;
            Task.Factory.StartNew(() =>
            {
                moveControl.elmoDriver.ResetErrorAll();
                jogPitchData.ElmoFunctionCompelete = true;
            });
        }

        private bool GetVelocityAndDistanceAndSingleAxis(ref EnumAxis axis, ref double velocity, ref double distance)
        {
            if (double.TryParse(tB_JogPitch_AxisMove_Disance.Text, out distance) && distance >= 0 &&
                double.TryParse(tB_JogPitch_AxisMove_Velocity.Text, out velocity) && velocity > 0)
            {
                axis = AxisList[cB_JogPitch_SelectAxis.SelectedIndex];
                return true;
            }
            else
                return false;
        }

        private void button_JogPitch_AxisMove_Absolute_Click(object sender, EventArgs e)
        {
            double velocity = 0;
            double distance = 0;
            EnumAxis axis = EnumAxis.None;
            if (!GetVelocityAndDistanceAndSingleAxis(ref axis, ref velocity, ref distance))
                return;

            moveControl.elmoDriver.ElmoMove(axis, distance, velocity, EnumMoveType.Absolute);
        }

        private void button_JogPitch_AxisMove_RelativeAdd_Click(object sender, EventArgs e)
        {
            double velocity = 0;
            double distance = 0;
            EnumAxis axis = EnumAxis.None;
            if (!GetVelocityAndDistanceAndSingleAxis(ref axis, ref velocity, ref distance))
                return;

            moveControl.elmoDriver.ElmoMove(axis, distance, velocity, EnumMoveType.Relative);
        }

        private void button_JogPitch_AxisMove_RelativeLess_Click(object sender, EventArgs e)
        {
            double velocity = 0;
            double distance = 0;
            EnumAxis axis = EnumAxis.None;
            if (!GetVelocityAndDistanceAndSingleAxis(ref axis, ref velocity, ref distance))
                return;

            moveControl.elmoDriver.ElmoMove(axis, -distance, velocity, EnumMoveType.Relative);
        }

        private void JogPitchForm_Shown(object sender, EventArgs e)
        {
            timerUpdate.Enabled = true;
        }

        private void button_JogPitch_ChangeFormSize_Click(object sender, EventArgs e)
        {
            button_JogPitch_ChangeFormSize.Enabled = false;
            if (button_JogPitch_ChangeFormSize.Text == "<\n<\n<")
            {
                button_JogPitch_ChangeFormSize.Text = ">\n>\n>";
                this.Size = new System.Drawing.Size(740, 533);
                button_JogPitchHide.Location = new Point(688, 0);
            }
            else
            {
                button_JogPitch_ChangeFormSize.Text = "<\n<\n<";
                this.Size = new System.Drawing.Size(1251, 533);
                button_JogPitchHide.Location = new Point(1199, 0);
            }

            button_JogPitch_ChangeFormSize.Enabled = true;
        }

        private void button_JogPitchHide_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private double GetDeltaTime(double endVel, double acc, double jerk)
        {
            double time = acc / jerk; // acc = 0 > acc的時間.
            double deltaVelocity = time * acc / 2 * 2;
            double lastDeltaVelocity;
            double lastDeltaTime;
            double distance;
            double origionTime;

            if (deltaVelocity == Math.Abs(-endVel))
            {
                time = time * 2;
            }
            else if (deltaVelocity > Math.Abs(-endVel))
            {
                deltaVelocity = Math.Abs(-endVel) / 2;
                time = Math.Sqrt(deltaVelocity * 2 / jerk);
                time = time * 2;
            }
            else
            {
                lastDeltaVelocity = Math.Abs(-endVel) - deltaVelocity;
                lastDeltaTime = lastDeltaVelocity / acc;
                time = 2 * time + lastDeltaTime;
            }

            distance = endVel * time / 2;
            origionTime = distance / endVel;
            return time - origionTime;
        }

        private void GetAccTimeAndDistance(double vel, double acc, double jerk, ref double time, ref double distance)
        {
            time = acc / jerk;
            vel = Math.Abs(vel);
            double deltaVelocity = time * acc / 2 * 2;
            double lastDeltaVelocity;
            double lastDeltaTime;

            if (deltaVelocity == vel)
            {
                time = time * 2;
            }
            else if (deltaVelocity > vel)
            {
                deltaVelocity = vel / 2;
                time = Math.Sqrt(deltaVelocity * 2 / jerk);
                time = time * 2;
            }
            else
            {
                lastDeltaVelocity = vel - deltaVelocity;
                lastDeltaTime = lastDeltaVelocity / acc;
                time = 2 * time + lastDeltaTime;
            }

            distance = vel * time / 2;
        }

        private double GetCurrectAcc(double distance, double velocity, double dec, double jerk, double deltaTime)
        {
            double time = 0;
            double dis = 0;
            GetAccTimeAndDistance(velocity, dec, jerk, ref time, ref dis);
            double allTime = time + time + (distance - dis * 2) / velocity;
            allTime = allTime + deltaTime;

            double time2 = 0;
            double dis2 = 0;

            for (double i = dec; i >= 0; i = i - 5)
            {
                GetAccTimeAndDistance(velocity, i, jerk, ref time2, ref dis2);
                if ((time2 + time + (distance - dis - dis2) / velocity) > allTime)
                    return i;
            }

            return -1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double distance = 0;
            double time = 0;
            GetAccTimeAndDistance(300, 500, 2000, ref time, ref distance);
            double origionTime = distance / 300;
            double deltaTime = time - origionTime;

            double realAcc = GetCurrectAcc(90, 75, 165, 990, deltaTime);

            moveControl.elmoDriver.ElmoMove(EnumAxis.GX, 600, 300, EnumMoveType.Relative, 500, 500, 2000);
            moveControl.elmoDriver.ElmoMove(EnumAxis.GT, 90, 75, EnumMoveType.Absolute, realAcc, 165, 990);
        }

        private void button_Skip_Click(object sender, EventArgs e)
        {
            button_Skip.Enabled = false;
            button_Skip.Text = (button_Skip.Text == "強制\r\n手動") ? "一般\r\n模式" : "強制\r\n手動";
            button_Skip.BackColor = (button_Skip.Text == "強制\r\n手動") ? Color.Red : Color.Transparent;
            button_Skip.Enabled = true;
        }


        #region PlcJog

        public delegate void plcJog_SettingValue();
        public plcJog_SettingValue plcJog_mySettingValue;
        public EnumJogTurnSpeed turnS;
        public EnumJogMoveVelocity moveV;
        public double distance;
        public bool JogPitchRevise = false;

        public void PlcJog_SettingDelegate(EnumJogTurnSpeed turnS, EnumJogMoveVelocity moveV, double distance, bool revise)
        {
            this.turnS = turnS;
            this.moveV = moveV;
            this.distance = distance;
            this.JogPitchRevise = revise;
            plcJog_mySettingValue = new plcJog_SettingValue(PlcJog_SetOperationValue);
        }

        public void PlcJog_RunSettingDelegate()
        {
            this.Invoke(plcJog_mySettingValue);
        }

        public void PlcJog_SetOperationValue()
        {
            switch (turnS)
            {
                case EnumJogTurnSpeed.High:
                    rB_JogPitch_TurnSpeed_High.Checked = true;
                    break;
                case EnumJogTurnSpeed.Medium:
                    rB_JogPitch_TurnSpeed_Medium.Checked = true;
                    break;
                case EnumJogTurnSpeed.Low:
                    rB_JogPitch_TurnSpeed_Low.Checked = true;
                    break;
                default:
                    rB_JogPitch_TurnSpeed_Low.Checked = true;
                    break;
            }

            switch (moveV)
            {
                case EnumJogMoveVelocity.ThreeHundred:
                    rB_JogPitch_MoveVelocity_300.Checked = true;
                    break;
                case EnumJogMoveVelocity.OneHundred:
                    rB_JogPitch_MoveVelocity_100.Checked = true;
                    break;
                case EnumJogMoveVelocity.Fifty:
                    rB_JogPitch_MoveVelocity_50.Checked = true;
                    break;
                case EnumJogMoveVelocity.Ten:
                    rB_JogPitch_MoveVelocity_10.Checked = true;
                    break;
                default:
                    rB_JogPitch_MoveVelocity_10.Checked = true;
                    break;
            }
            tB_JogPitch_Distance.Text = distance.ToString();
            cB_JogPitch_MoveAndOntimeRevise.Checked = JogPitchRevise;
        }

        public void PlcJog_SetOperationValue(EnumJogTurnSpeed turnS, EnumJogMoveVelocity moveV, double distance)
        {
            switch (turnS)
            {
                case EnumJogTurnSpeed.High:
                    rB_JogPitch_TurnSpeed_High.Checked = true;
                    break;
                case EnumJogTurnSpeed.Medium:
                    rB_JogPitch_TurnSpeed_Medium.Checked = true;
                    break;
                case EnumJogTurnSpeed.Low:
                    rB_JogPitch_TurnSpeed_Low.Checked = true;
                    break;
                default:
                    rB_JogPitch_TurnSpeed_Low.Checked = true;
                    break;
            }

            switch (moveV)
            {
                case EnumJogMoveVelocity.ThreeHundred:
                    rB_JogPitch_MoveVelocity_300.Checked = true;
                    break;
                case EnumJogMoveVelocity.OneHundred:
                    rB_JogPitch_MoveVelocity_100.Checked = true;
                    break;
                case EnumJogMoveVelocity.Fifty:
                    rB_JogPitch_MoveVelocity_50.Checked = true;
                    break;
                case EnumJogMoveVelocity.Ten:
                    rB_JogPitch_MoveVelocity_10.Checked = true;
                    break;
                default:
                    rB_JogPitch_MoveVelocity_10.Checked = true;
                    break;
            }

            tB_JogPitch_Distance.Text = distance.ToString();


        }

        public void PlcJog_GetOperationStatus(ref PlcOperation ipcOperation)
        {

            if (ipcOperation.JogElmoFunction != EnumJogElmoFunction.All_Reset)
            {
                ipcOperation.JogElmoFunction = EnumJogElmoFunction.Enable;
                bool bElmoStatus = true;
                foreach (KeyValuePair<EnumAxis, ElmoAxisFeedbackData> item in jogPitchData.AxisData)
                {
                    if (null != item.Value)
                    {
                        bElmoStatus = bElmoStatus & item.Value.StandStill;
                        if (bElmoStatus == false)
                        {
                            ipcOperation.JogElmoFunction = EnumJogElmoFunction.Disable;
                            break;
                        }
                    }

                }
            }

            EnumJogRunMode runMode = EnumJogRunMode.No_Use;

            switch (mode)
            {
                case EnumJogPitchMode.BackwardWheel:
                    runMode = EnumJogRunMode.BackwardWheel;
                    break;
                case EnumJogPitchMode.ForwardWheel:
                    runMode = EnumJogRunMode.ForwardWheel;
                    break;
                case EnumJogPitchMode.Normal:
                    runMode = EnumJogRunMode.Normal;
                    break;
                case EnumJogPitchMode.SpinTurn:
                    runMode = EnumJogRunMode.SpinTurn;
                    break;

            }
            ipcOperation.JogRunMode = runMode;

            EnumJogTurnSpeed turnS = EnumJogTurnSpeed.No_Use;
            if (rB_JogPitch_TurnSpeed_High.Checked)
            {
                turnS = EnumJogTurnSpeed.High;
            }
            if (rB_JogPitch_TurnSpeed_Medium.Checked)
            {
                turnS = EnumJogTurnSpeed.Medium;
            }
            if (rB_JogPitch_TurnSpeed_Low.Checked)
            {
                turnS = EnumJogTurnSpeed.Low;
            }
            ipcOperation.JogTurnSpeed = turnS;

            EnumJogMoveVelocity moveVelocity = EnumJogMoveVelocity.No_Use;
            if (rB_JogPitch_MoveVelocity_300.Checked)
            {
                moveVelocity = EnumJogMoveVelocity.ThreeHundred;
            }
            if (rB_JogPitch_MoveVelocity_100.Checked)
            {
                moveVelocity = EnumJogMoveVelocity.OneHundred;
            }
            if (rB_JogPitch_MoveVelocity_50.Checked)
            {
                moveVelocity = EnumJogMoveVelocity.Fifty;
            }
            if (rB_JogPitch_MoveVelocity_10.Checked)
            {
                moveVelocity = EnumJogMoveVelocity.Ten;
            }

            ipcOperation.JogMoveVelocity = moveVelocity;


            if (cB_JogPitch_MoveAndOntimeRevise.Checked)
            {
                ipcOperation.JogMoveOntimeRevise = true;
            }
            else
            {
                ipcOperation.JogMoveOntimeRevise = false;
            }

            //  ipcOperation.JogOperation = EnumJogOperation.No_Use; 不進行任何判斷
            ipcOperation.JogMaxDistance = Convert.ToDouble(tB_JogPitch_Distance.Text);

        }
        #endregion

        private void ElmoResetAndCheckLinked()
        {
            int errorCode = 0;

            for (int i = 0; i < retryTimes; i++)
            {
                if (!moveControl.elmoDriver.CheckAxisNoError(ref errorCode))
                {
                    moveControl.elmoDriver.ResetErrorAll();
                    Thread.Sleep(500);
                }

                moveControl.elmoDriver.DisableAllAxis();
                Thread.Sleep(500);
                moveControl.elmoDriver.EnableAllAxis();
                Thread.Sleep(500);
                if (moveControl.elmoDriver.CheckAxisNoError(ref errorCode))
                    break;

                if (retryTimes == i)
                {
                    elmoAllResetMessage = "axis error 清除不掉!";
                    return;
                }
            }

            if (!moveControl.elmoDriver.ElmoAxisTypeAllServoOn(EnumAxisType.Move) || !moveControl.elmoDriver.ElmoAxisTypeAllServoOn(EnumAxisType.Turn))
            {
                elmoAllResetMessage = "ServoOn all Axis 失敗!";
                return;
            }

            if (moveControl.elmoDriver.SetAllVirtualServoOnAndLinked())
                elmoAllResetMessage = "Elmo Reset 成功!";
            else
                elmoAllResetMessage = "Elmo Reset 失敗!";
        }

        public void button_JogpitchResetAll_Click(object sender, EventArgs e)
        {
            jogPitchData.ElmoFunctionCompelete = false;
            Task.Factory.StartNew(() =>
            {
                ElmoResetAndCheckLinked();
                jogPitchData.ElmoFunctionCompelete = true;
            });
        }

        private void ElmoFunctionFlagChange(bool compelete)
        {
            button_JogPitch_ElmoEnable.Enabled = compelete;
            button_JogPitch_ElmoDisable.Enabled = compelete;
            button_JogPitch_ElmoReset.Enabled = compelete;
            button_JogpitchResetAll.Enabled = compelete;
        }

        private void timer_UpdateElmoFunction_Tick(object sender, EventArgs e)
        {
            if (elmoAllResetMessage != "")
            {
                temp = elmoAllResetMessage;
                elmoAllResetMessage = "";
                MessageBox.Show(temp);
            }

            ElmoFunctionFlagChange(jogPitchData.ElmoFunctionCompelete);
        }
    }
}
