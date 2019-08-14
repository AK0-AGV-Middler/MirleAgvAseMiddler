using Mirle.Agv.Controller;
using Mirle.Agv.Model;
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
using Mirle.Agv.View;

namespace Mirle.Agv.View
{
    public partial class JogPitchForm : Form
    {
        private Thread ontimeRevise;
        private Thread homeThread;
        private EnumJogPitchMode mode;
        private AgvMoveRevise agvRevise;
        private bool changingMode = false;
        private int wheelAngle = 0;
        private EnumAxis[] AxisList = new EnumAxis[18] {EnumAxis.XFL, EnumAxis.XFR, EnumAxis.XRL, EnumAxis.XRR,
                                                EnumAxis.TFL, EnumAxis.TFR, EnumAxis.TRL, EnumAxis.TRR,
                                                EnumAxis.VXFL, EnumAxis.VXFR, EnumAxis.VXRL, EnumAxis.VXRR,
                                                EnumAxis.VTFL, EnumAxis.VTFR, EnumAxis.VTRL, EnumAxis.VTRR,
                                                EnumAxis.GX, EnumAxis.GT};
        private EnumMoveState nowState, lastState;
        private bool homing = false;

        public JogPitchForm(MoveControlHandler moveControl)
        {
            if (moveControl == null)
                return;

            this.moveControl = moveControl;
            InitializeComponent();
            ChangeMode(EnumJogPitchMode.Normal);
            agvRevise = new AgvMoveRevise(moveControl.ontimeReviseConfig, moveControl.elmoDriver,
                                          moveControl.DriverSr2000List, null);

            this.allAxis = new Mirle.Agv.JogPitchAxis[AxisList.Count()];

            for (int i = 0; i < AxisList.Count(); i++)
            {
                this.allAxis[i] = new Mirle.Agv.JogPitchAxis(AxisList[i].ToString());

                this.allAxis[i].BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));

                if (i < 4)
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * i, 30);
                else if (i < 8)
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * (i - 4), 30 + 110 * 1);
                else if (i < 12)
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * (i - 8), 30 + 110 * 2);
                else if (i < 16)
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * (i - 12), 30 + 110 * 3);
                else
                    this.allAxis[i].Location = new System.Drawing.Point(735 + 100 * 4, 30 + 110 * (i - 16));

                this.allAxis[i].Name = AxisList[i].ToString();
                this.allAxis[i].Size = new System.Drawing.Size(92, 95);
                this.allAxis[i].TabIndex = 133;

                this.Controls.Add(this.allAxis[i]);
                cB_JogPitch_SelectAxis.Items.Add(AxisList[i].ToString());
            }

            cB_JogPitch_SelectAxis.SelectedIndex = AxisList.Count() - 2;
            lastState = moveControl.MoveState;
        }

        private MoveControlHandler moveControl = null;

        private void EnableDisableForm(bool flag)
        {
            foreach (Control control in this.Controls)
            {
                control.Enabled = flag;
            }
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if (moveControl == null)
                return;

            #region Update Sr2000
            AGVPosition agvPosition;
            ThetaSectionDeviation thetaSectionDeviation;

            if (moveControl.DriverSr2000List.Count > 0)
            {
                agvPosition = moveControl.DriverSr2000List[0].GetAGVPosition();
                if (agvPosition != null)
                {
                    tB_JogPitch_MapX_L.Text = agvPosition.Position.X.ToString("0.0");
                    tB_JogPitch_MapY_L.Text = agvPosition.Position.Y.ToString("0.0");
                    tB_JogPitch_MapTheta_L.Text = agvPosition.AGVAngle.ToString("0.0");
                }
                else
                {
                    tB_JogPitch_MapX_L.Text = "-----";
                    tB_JogPitch_MapY_L.Text = "-----";
                    tB_JogPitch_MapTheta_L.Text = "-----";
                }

                thetaSectionDeviation = moveControl.DriverSr2000List[0].GetThetaSectionDeviation();
                if (thetaSectionDeviation != null)
                {
                    tB_JogPitch_SectionDeviation_L.Text = thetaSectionDeviation.SectionDeviation.ToString("0.0");
                    tB_JogPitch_Theta_L.Text = thetaSectionDeviation.Theta.ToString("0.0");
                }
                else
                {
                    tB_JogPitch_SectionDeviation_L.Text = "-----";
                    tB_JogPitch_Theta_L.Text = "-----";
                }
            }

            if (moveControl.DriverSr2000List.Count > 1)
            {
                agvPosition = moveControl.DriverSr2000List[1].GetAGVPosition();
                if (agvPosition != null)
                {
                    tB_JogPitch_MapX_R.Text = agvPosition.Position.X.ToString("0.0");
                    tB_JogPitch_MapY_R.Text = agvPosition.Position.Y.ToString("0.0");
                    tB_JogPitch_MapTheta_R.Text = agvPosition.AGVAngle.ToString("0.0");
                }
                else
                {
                    tB_JogPitch_MapX_R.Text = "-----";
                    tB_JogPitch_MapY_R.Text = "-----";
                    tB_JogPitch_MapTheta_R.Text = "-----";
                }

                thetaSectionDeviation = moveControl.DriverSr2000List[1].GetThetaSectionDeviation();
                if (thetaSectionDeviation != null)
                {
                    tB_JogPitch_SectionDeviation_R.Text = thetaSectionDeviation.SectionDeviation.ToString("0.0");
                    tB_JogPitch_Theta_R.Text = thetaSectionDeviation.Theta.ToString("0.0");
                }
                else
                {
                    tB_JogPitch_SectionDeviation_R.Text = "-----";
                    tB_JogPitch_Theta_R.Text = "-----";
                }
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
                if (moveControl.elmoDriver.MoveCompelete(EnumAxis.GT) && moveControl.elmoDriver.MoveCompelete(EnumAxis.GX) && !homing)
                {
                    changingMode = false;
                    EnalbeDisableButton(true);
                }
            }

            nowState = moveControl.MoveState;
            if (nowState != lastState)
            {
                EnableDisableForm(nowState == EnumMoveState.Idle);
                lastState = nowState;
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
            button_JogPitch_Home.Enabled = flag;

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
                if (agvRevise.OntimeRevise(ref reviseWheelAngle, wheelAngle, ref str))
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

        private void button_JogPitch_STOP_Click(object sender, EventArgs e)
        {
            if (homing)
            {
                homeThread.Abort();
                homing = false;
            }

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

        private void button_JogPitch_Normal_Click(object sender, EventArgs e)
        {
            ChangeMode(EnumJogPitchMode.Normal);
        }

        private void button_JogPitch_ForwardWheel_Click(object sender, EventArgs e)
        {
            ChangeMode(EnumJogPitchMode.ForwardWheel);
        }

        private void button_JogPitch_BackwardWheel_Click(object sender, EventArgs e)
        {
            ChangeMode(EnumJogPitchMode.BackwardWheel);
        }

        private void button_JogPitch_SpinTurn_Click(object sender, EventArgs e)
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

        private void button_JogPitch_Turn_MouseUp(object sender, MouseEventArgs e)
        {
            moveControl.elmoDriver.ElmoStop(EnumAxis.GT, moveControl.moveControlConfig.Turn.Deceleration, moveControl.moveControlConfig.Turn.Jerk);
            moveControl.location.Real = null;
        }

        private void Turn_MouseDown(double angle)
        {
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

        private void button_JogPitch_TurnRight_MouseDown(object sender, MouseEventArgs e)
        {
            Turn_MouseDown(-90);
        }

        private void button_JogPitch_TurnLeft_MouseDown(object sender, MouseEventArgs e)
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

                moveControl.elmoDriver.ElmoMove(EnumAxis.GX, (flag ? distance : -distance), velocity, EnumMoveType.Relative,
                    moveControl.moveControlConfig.Move.Acceleration, moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);

                ontimeRevise = new Thread(OntimeReviseThread);
                ontimeRevise.Start();
            }
            else
            {
                moveControl.elmoDriver.ElmoMove(EnumAxis.GX, (flag ? distance : -distance), velocity, EnumMoveType.Relative,
                    moveControl.moveControlConfig.Move.Acceleration, moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);
            }
        }

        private void button_JogPitch_Forward_MouseDown(object sender, MouseEventArgs e)
        {
            Move_MouseDown(true);
        }

        private void button_JogPitch_Backward_MouseDown(object sender, MouseEventArgs e)
        {
            Move_MouseDown(false);
        }

        private void button_JogPitch_Move_MouseUp(object sender, MouseEventArgs e)
        {
            moveControl.elmoDriver.ElmoStop(EnumAxis.GX, moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);
            moveControl.location.Real = null;
        }

        private void button_JogPitch_ElmoEnable_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => { moveControl.elmoDriver.EnableAllAxis(); });
        }

        private void button_JogPitch_ElmoDisable_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => { moveControl.elmoDriver.DisableAllAxis(); });
        }

        private void button_JogPitch_ElmoReset_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => { moveControl.elmoDriver.ResetErrorAll(); });
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

        private void HomeThread()
        {
            double oneCycleDistance = Math.PI * Math.Sqrt(Math.Pow(1400, 2) + Math.Pow(2400, 2));
            double spinDistance;
            System.Diagnostics.Stopwatch servoOnTimer = new System.Diagnostics.Stopwatch();
            ThetaSectionDeviation reviseData = null;

            for (int i = 0; i < moveControl.DriverSr2000List.Count; i++)
            {
                reviseData = moveControl.DriverSr2000List[i].GetThetaSectionDeviation();
                if (reviseData != null)
                {
                    if (moveControl.IsSameAngle(reviseData.BarodeAngleInMap, reviseData.AGVAngleInMap, 0))
                        break;
                    else
                        reviseData = null;
                }
            }

            if (reviseData == null)
            {
                MessageBox.Show("請移至可以讀取到和軌道平行的Barocde上!");
                homing = false;
                return;
            }

            if (Math.Abs(reviseData.Theta) > 20 || Math.Abs(reviseData.SectionDeviation) > 50)
            {
                MessageBox.Show("偏差過大,請手動調整(角度超過20度或偏差超過50mm)");
                homing = false;
                return;
            }

            if (Math.Abs(reviseData.Theta) > 0.1)
            {
                moveControl.elmoDriver.ElmoMove(EnumAxis.GT, -59.744, 59.744, 59.744, -59.744, moveControl.moveControlConfig.Turn.Velocity, EnumMoveType.Absolute,
                            moveControl.moveControlConfig.Turn.Acceleration, moveControl.moveControlConfig.Turn.Deceleration, moveControl.moveControlConfig.Turn.Jerk);

                spinDistance = reviseData.Theta / 360 * oneCycleDistance;
                double[] move = new double[4] { -spinDistance, spinDistance, -spinDistance, spinDistance };

                servoOnTimer.Reset();
                servoOnTimer.Start();

                while (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GT) && servoOnTimer.ElapsedMilliseconds < moveControl.moveControlConfig.TurnTimeoutValue)
                {
                    Thread.Sleep(50);
                }

                if (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GT))
                {
                    moveControl.elmoDriver.ElmoStop(EnumAxis.GT);
                    MessageBox.Show("轉向角度至Spin Turn角度時Timeout!");
                    homing = false;
                    return;
                }

                moveControl.elmoDriver.ElmoMove(EnumAxis.GX, move[0], move[1], move[2], move[3], 10, EnumMoveType.Relative, moveControl.moveControlConfig.Move.Acceleration,
                                         moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);

                servoOnTimer.Reset();
                servoOnTimer.Start();

                while (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GX) && servoOnTimer.ElapsedMilliseconds < moveControl.moveControlConfig.TurnTimeoutValue)
                {
                    Thread.Sleep(50);
                }

                if (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GX))
                {
                    moveControl.elmoDriver.ElmoStop(EnumAxis.GX);
                    MessageBox.Show("Spin Turn至軌道平行時Timeout!");
                    homing = false;
                    return;
                }
            }

            for (int i = 0; i < moveControl.DriverSr2000List.Count; i++)
            {
                reviseData = moveControl.DriverSr2000List[i].GetThetaSectionDeviation();
                if (reviseData != null)
                {
                    if (moveControl.IsSameAngle(reviseData.BarodeAngleInMap, reviseData.AGVAngleInMap, 0))
                        break;
                    else
                        reviseData = null;
                }
            }

            if (reviseData == null)
            {
                MessageBox.Show("請移至可以讀取到和軌道平行的Barocde上!");
                homing = false;
                return;
            }

            if (Math.Abs(reviseData.SectionDeviation) > 5)
            {
                moveControl.elmoDriver.ElmoMove(EnumAxis.GT, 90, 90, 90, 90, moveControl.moveControlConfig.Turn.Velocity, EnumMoveType.Absolute,
                moveControl.moveControlConfig.Turn.Acceleration, moveControl.moveControlConfig.Turn.Deceleration, moveControl.moveControlConfig.Turn.Jerk);

                servoOnTimer.Reset();
                servoOnTimer.Start();

                while (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GT) && servoOnTimer.ElapsedMilliseconds < moveControl.moveControlConfig.TurnTimeoutValue)
                {
                    Thread.Sleep(50);
                }

                if (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GT))
                {
                    moveControl.elmoDriver.ElmoStop(EnumAxis.GT);
                    MessageBox.Show("轉向角度至90度時Timeout!");
                    homing = false;
                    return;
                }

                moveControl.elmoDriver.ElmoMove(EnumAxis.GX, reviseData.SectionDeviation, 50, EnumMoveType.Relative, moveControl.moveControlConfig.Move.Acceleration,
                                         moveControl.moveControlConfig.Move.Deceleration, moveControl.moveControlConfig.Move.Jerk);

                servoOnTimer.Reset();
                servoOnTimer.Start();

                while (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GX) && servoOnTimer.ElapsedMilliseconds < moveControl.moveControlConfig.TurnTimeoutValue)
                {
                    Thread.Sleep(50);
                }

                if (!moveControl.elmoDriver.MoveCompelete(EnumAxis.GX))
                {
                    moveControl.elmoDriver.ElmoStop(EnumAxis.GX);
                    MessageBox.Show("平移行時Timeout!");
                    homing = false;
                    return;
                }

                homing = false;
            }
        }

        private void button_JogPitch_Home_Click(object sender, EventArgs e)
        {
            if (!homing)
            {
                homing = true;
                changingMode = true;
                EnalbeDisableButton(false);
                homeThread = new Thread(HomeThread);
                homeThread.Start();
            }
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
                this.Size = new System.Drawing.Size(740, 520);
            }
            else
            {
                button_JogPitch_ChangeFormSize.Text = "<\n<\n<";
                this.Size = new System.Drawing.Size(1251, 520);
            }

            button_JogPitch_ChangeFormSize.Enabled = true;
        }
    }
}
