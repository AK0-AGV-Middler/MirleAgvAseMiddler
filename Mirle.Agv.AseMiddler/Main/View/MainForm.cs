using Mirle.Agv.AseMiddler.Controller;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using Mirle.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class MainForm : Form
    {
        public MainFlowHandler mainFlowHandler;
        private MainFlowConfig mainFlowConfig;
        public MainForm mainForm;
        private AsePackage asePackage;
        private AgvcConnector agvcConnector;
        private AgvcConnectorForm middlerForm;
        private AlarmForm alarmForm;
        private AlarmHandler alarmHandler;
        private AseMoveControlForm aseMoveControlForm;
        private AseRobotControlForm aseRobotControlForm;
        private AseAgvlConnectorForm aseAgvlConnectorForm;
        private WarningForm warningForm;
        private ConfigForm configForm;
        private Panel panelLeftUp;
        private Panel panelLeftDown;
        private Panel panelRightUp;
        private Panel panelRightDown;
        private MapInfo theMapInfo = new MapInfo();
        //PerformanceCounter performanceCounterCpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
        //PerformanceCounter performanceCounterRam = new PerformanceCounter("Memory", "Available MBytes");
        //PerformanceCounter performanceCounterRam = new PerformanceCounter("Memory", "% Committed Bytes in Use");
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private Vehicle theVehicle = Vehicle.Instance;
        public bool IsAgvcConnect { get; set; } = false;
        public bool IsAgvlConnect { get; set; } = false;
        public string DebugLogMsg { get; set; } = "";
        public string TransferCommandMsg { get; set; } = "";
        public string TransferStepMsg { get; set; } = "";
        public string MainFlowAbnormalReasonMsg { get; set; } = "";
        public string AgvcConnectorAbnormalReasonMsg { get; set; } = "";
        public string MoveControllerAbnormalReasonMsg { get; set; } = "";
        public string RobotAbnormalReasonMsg { get; set; } = "";
        public string BatterysAbnormalReasonMsg { get; set; } = "";
        public string LastAlarmMsg { get; set; } = "";


        #region PaintingItems
        private Image image;
        private Graphics gra;
        private string saveNameWithTail;
        private SolidBrush blackBrush = new SolidBrush(Color.Black);
        private SolidBrush redBrush = new SolidBrush(Color.Red);
        private Dictionary<string, Pen> allPens = new Dictionary<string, Pen>();

        public bool IsBarcodeLineShow { get; set; } = true;
        private Dictionary<string, UcSectionImage> allUcSectionImages = new Dictionary<string, UcSectionImage>();
        private Dictionary<string, UcAddressImage> allUcAddressImages = new Dictionary<string, UcAddressImage>();
        private double coefficient = 0.05f;
        private double deltaOrigion = 50;
        private double triangleCoefficient = (double)(1 / Math.Sqrt(3.0));
        private UcVehicleImage ucVehicleImage = new UcVehicleImage();
        private MapPosition minPos = new MapPosition();
        private MapPosition maxPos = new MapPosition();

        private Point mouseDownPbPoint;
        private Point mouseDownScreenPoint;

        #endregion

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
            mainFlowConfig = mainFlowHandler.GetMainFlowConfig();
            theMapInfo = mainFlowHandler.theMapInfo;
            alarmHandler = mainFlowHandler.GetAlarmHandler();
            asePackage = mainFlowHandler.GetAsePackage();

            agvcConnector = mainFlowHandler.GetAgvcConnector();
            mainForm = this;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitialForms();
            InitialPaintingItems();
            InitialPanels();
            InitialEvents();
            ResetImageAndPb();
            InitialSoc();
            InitialConnectionAndCarrierStatus();
            txtLastAlarm.Text = "";
            var msg = "MainForm : 讀取主畫面";
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
        }

        private void InitialForms()
        {
            middlerForm = new AgvcConnectorForm(agvcConnector);
            middlerForm.WindowState = FormWindowState.Normal;
            middlerForm.Show();
            middlerForm.Hide();

            configForm = new ConfigForm(mainFlowHandler);
            configForm.WindowState = FormWindowState.Normal;
            configForm.Show();
            configForm.Hide();

            var middlerConfig = agvcConnector.GetAgvcConnectorConfig();
            tstextClientName.Text = $"[{ middlerConfig.ClientName}]";
            tstextRemoteIp.Text = $"[{middlerConfig.RemoteIp}]";
            tstextRemotePort.Text = $"[{middlerConfig.RemotePort}]";
            this.Text = $"主畫面 版本編號為[{Application.ProductVersion}]";

            alarmForm = new AlarmForm(mainFlowHandler);
            alarmForm.WindowState = FormWindowState.Normal;
            alarmForm.Show();
            alarmForm.Hide();

            warningForm = new WarningForm();
            warningForm.WindowState = FormWindowState.Normal;
            warningForm.Show();
            warningForm.Hide();

            numPositionX.Maximum = decimal.MaxValue;
            numPositionY.Maximum = decimal.MaxValue;

            InitialAseMoveControlForm();
            InitialAseRobotControlForm();
            InitialAseAgvlConnectorForm();
        }

        private void InitialPaintingItems()
        {
            Pen aPen;
            aPen = new Pen(Color.Blue, 2);
            allPens.Add("Blue2", aPen);
            aPen = new Pen(Color.Black, 2);
            allPens.Add("Black2", aPen);
            aPen = new Pen(Color.Red, 2);
            allPens.Add("Red2", aPen);
            aPen = new Pen(Color.Green, 2);
            allPens.Add("Green2", aPen);
            aPen = new Pen(Color.YellowGreen, 2);
            allPens.Add("YellowGreen2", aPen);
            aPen = new Pen(Color.Black, 1);
            aPen.DashStyle = DashStyle.DashDot;
            allPens.Add("BlackDashDot1", aPen);

            aPen = new Pen(Color.Red, 4);
            allPens.Add("NotGetReserveSection", aPen);

            aPen = new Pen(Color.Green, 4);
            allPens.Add("GetReserveSection", aPen);

            aPen = new Pen(Color.Blue);
            allPens.Add("NormalSection", aPen);

            ucVehicleImage.Parent = pictureBox1;
            ucVehicleImage.BringToFront();
            ucVehicleImage.Show();
        }

        private void InitialPanels()
        {
            panelLeftUp = splitContainer3.Panel1;
            panelLeftUp.HorizontalScroll.Enabled = true;
            panelLeftUp.VerticalScroll.Enabled = true;

            panelLeftDown = splitContainer3.Panel2;
            panelLeftDown.HorizontalScroll.Enabled = true;
            panelLeftDown.VerticalScroll.Enabled = true;

            panelRightUp = splitContainer2.Panel1;
            panelRightUp.HorizontalScroll.Enabled = true;
            panelRightUp.VerticalScroll.Enabled = true;

            panelRightDown = splitContainer2.Panel2;
            panelRightDown.HorizontalScroll.Enabled = true;
            panelRightDown.VerticalScroll.Enabled = true;
        }

        private void InitialEvents()
        {
            mainFlowHandler.OnMessageShowEvent += middlerForm.SendOrReceiveCmdToTextBox;
            mainFlowHandler.OnMessageShowEvent += ShowMsgOnMainForm;
            mainFlowHandler.OnPrepareForAskingReserveEvent += MainFlowHandler_OnPrepareForAskingReserveEvent;
            mainFlowHandler.OnMoveArrivalEvent += MainFlowHandler_OnMoveArrivalEvent;
            mainFlowHandler.OnTransferCommandCheckedEvent += MainFlowHandler_OnTransferCommandCheckedEvent;
            mainFlowHandler.OnOverrideCommandCheckedEvent += MainFlowHandler_OnOverrideCommandCheckedEvent;
            mainFlowHandler.OnAvoidCommandCheckedEvent += MainFlowHandler_OnAvoidCommandCheckedEvent;
            mainFlowHandler.OnDoTransferStepEvent += MainFlowHandler_OnDoTransferStepEvent;
            agvcConnector.OnMessageShowOnMainFormEvent += ShowMsgOnMainForm;
            agvcConnector.OnConnectionChangeEvent += AgvcConnector_OnConnectionChangeEvent;
            agvcConnector.OnReserveOkEvent += AgvcConnector_OnReserveOkEvent;
            agvcConnector.OnPassReserveSectionEvent += AgvcConnector_OnPassReserveSectionEvent;
            alarmHandler.OnSetAlarmEvent += AlarmHandler_OnSetAlarmEvent;
            alarmHandler.OnResetAllAlarmsEvent += AlarmHandler_OnResetAllAlarmsEvent;

            theVehicle.OnAutoStateChangeEvent += TheVehicle_OnAutoStateChangeEvent;
            mainFlowHandler.OnAgvlConnectionChangedEvent += MainFlowHandler_OnAgvlConnectionChangedEvent;
            mainFlowHandler.GetAseMoveControl().OnMoveFinishedEvent += AseMoveControl_OnMoveFinishEvent;

            asePackage.ImportantPspLog += AsePackage_ImportantPspLog;
        }

        private void InitialSoc()
        {
            txtWatchLowPower.Text = $"High/Low : {(int)theVehicle.AutoChargeHighThreshold}/{(int)theVehicle.AutoChargeLowThreshold}";
            timer_SetupInitialSoc.Enabled = true;
        }

        private void InitialConnectionAndCarrierStatus()
        {
            IsAgvcConnect = agvcConnector.IsConnected();
            UpdateAgvcConnection();

            IsAgvlConnect = mainFlowHandler.GetAsePackage().IsConnected();
            UpdateAgvlConnection();


            if (theVehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading || theVehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
            {
                mainFlowHandler.ReadCarrierId();
            }
        }

        public void DrawBasicMap()
        {
            try
            {
                SetupImageRegion();

                // Draw Sections in blueLine
                allUcSectionImages.Clear();

                var sectionMap = theMapInfo.sectionMap.Values.ToList();
                foreach (var section in sectionMap)
                {
                    var headPos = section.HeadAddress.Position;
                    var tailPos = section.TailAddress.Position;
                    MapPosition sectionLocation = new MapPosition(Math.Min(headPos.X, tailPos.X), Math.Min(headPos.Y, tailPos.Y));

                    UcSectionImage ucSectionImage = new UcSectionImage(theMapInfo, section);
                    if (!allUcSectionImages.ContainsKey(section.Id))
                    {
                        allUcSectionImages.Add(section.Id, ucSectionImage);
                    }
                    pictureBox1.Controls.Add(ucSectionImage);
                    ucSectionImage.Location = MapPixelExchange(sectionLocation);
                    switch (section.Type)
                    {
                        case EnumSectionType.Horizontal:
                            break;
                        case EnumSectionType.Vertical:
                            ucSectionImage.Location = new Point(ucSectionImage.Location.X - (ucSectionImage.labelSize.Width / 2 + 5), ucSectionImage.Location.Y);
                            break;
                        case EnumSectionType.R2000:
                        case EnumSectionType.None:
                        default:
                            break;
                    }

                    ucSectionImage.BringToFront();

                    ucSectionImage.MouseDown += UcSectionImage_MouseDown;
                    ucSectionImage.label1.MouseDown += UcSectionImageItem_MouseDown;
                    ucSectionImage.pictureBox1.MouseDown += UcSectionImageItem_MouseDown;
                }


                //Draw Addresses in BlackRectangle(Segment) RedCircle(Port) RedTriangle(Charger)
                allUcAddressImages.Clear();

                var addressMap = theMapInfo.addressMap.Values.ToList();
                foreach (var address in addressMap)
                {
                    UcAddressImage ucAddressImage = new UcAddressImage(theMapInfo, address);
                    if (!allUcAddressImages.ContainsKey(address.Id))
                    {
                        allUcAddressImages.Add(address.Id, ucAddressImage);
                    }
                    pictureBox1.Controls.Add(ucAddressImage);
                    ucAddressImage.Location = MapPixelExchange(address.Position);
                    ucAddressImage.FixToCenter();
                    ucAddressImage.BringToFront();
                    Label label = new Label();
                    label.AutoSize = false;
                    label.Size = new Size(35, 12);
                    label.Parent = pictureBox1;
                    label.Text = address.Id;
                    label.Location = new Point(ucAddressImage.Location.X, ucAddressImage.Location.Y + 2 * (ucAddressImage.Radius + 1));
                    label.BringToFront();


                    ucAddressImage.MouseDown += UcAddressImage_MouseDown;
                    //ucAddressImage.label1.MouseDown += UcAddressImageItem_MouseDown;
                    ucAddressImage.pictureBox1.MouseDown += UcAddressImageItem_MouseDown;
                    //ucAddressImage.pictureBox1.MouseDoubleClick += ucAddressImageItem_DoubleClick;
                }



                pictureBox1.SendToBack();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }


        private void MainFlowHandler_OnPrepareForAskingReserveEvent(object sender, MoveCmdInfo moveCmd)
        {
            try
            {
                //Task.Run(() => SetMovingSectionAndEndPosition(moveCmd.MovingSections, moveCmd.EndAddress));
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void MainFlowHandler_OnMoveArrivalEvent(object sender, EventArgs e)
        {
            try
            {
                //Task.Run(() => ResetSectionColor());
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AgvcConnector_OnConnectionChangeEvent(object sender, bool isConnect)
        {
            IsAgvcConnect = isConnect;
        }
        private void AgvcConnector_OnReserveOkEvent(object sender, string sectionId)
        {
            try
            {
                //Task.Run(() => GetReserveSection(sectionId));
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void AgvcConnector_OnPassReserveSectionEvent(object sender, string passSectionId)
        {
            try
            {
                //Task.Run(() => ChangeToNormalSection(passSectionId));
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            try
            {
                var msg = $"發生 Alarm, [Id={alarm.Id}][Text={alarm.AlarmText}]";
                AppendDebugLogMsg(msg);

                LastAlarmMsg = $"[Id={alarm.Id}]\r\n[Text={alarm.AlarmText}]";

                if (alarm.Level == EnumAlarmLevel.Alarm)
                {
                    if (alarmForm.IsDisposed)
                    {
                        alarmForm = new AlarmForm(mainFlowHandler);
                    }
                    alarmForm.BringToFront();
                    alarmForm.Show();
                }
                else
                {
                    mainFlowHandler.BuzzOff();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void AlarmHandler_OnResetAllAlarmsEvent(object sender, string msg)
        {
            try
            {
                btnAlarmReset.Enabled = false;
                AppendDebugLogMsg(msg);
                LastAlarmMsg = "";
                SpinWait.SpinUntil(() => false, 500);
                btnAlarmReset.Enabled = true;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AseMoveControl_OnMoveFinishEvent(object sender, EnumMoveComplete e)
        {
            try
            {
                //Task.Run(() => ResetSectionColor());
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }


        private void TheVehicle_OnBeamDisableChangeEvent(object sender, BeamDisableArgs e)
        {
            try
            {
                var msg = $"{EnumBeamDirectionParse(e.Direction)} BeamSensor {DisableParse(!e.IsDisable)}";
                ShowMsgOnMainForm(this, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private string DisableParse(bool v)
        {
            return v ? "打開" : "關閉";
        }

        private string EnumBeamDirectionParse(EnumBeamDirection direction)
        {
            switch (direction)
            {
                case EnumBeamDirection.Front:
                    return "前方";
                case EnumBeamDirection.Back:
                    return "後方";
                case EnumBeamDirection.Left:
                    return "左方";
                case EnumBeamDirection.Right:
                    return "右方";
                default:
                    return "未知";
            }
        }

        private void MainFlowHandler_OnAgvlConnectionChangedEvent(object sender, bool e)
        {
            IsAgvlConnect = e;
        }

        private void AsePackage_ImportantPspLog(object sender, string e)
        {
            ShowMsgOnMainForm(this, e);
        }

        private void ShowMsgOnMainForm(object sender, string msg)
        {
            AppendDebugLogMsg(msg);
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
        }

        private void AppendDebugLogMsg(string msg)
        {
            try
            {
                DebugLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", msg, "\r\n", DebugLogMsg);

                if (DebugLogMsg.Length > 65535)
                {
                    DebugLogMsg = DebugLogMsg.Substring(65535);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void SetupImageRegion()
        {
            foreach (var address in theMapInfo.addressMap.Values)
            {
                if (address.Position.X > maxPos.X)
                {
                    maxPos.X = address.Position.X;
                }

                if (address.Position.X < minPos.X)
                {
                    minPos.X = address.Position.X;
                }

                if (address.Position.Y > maxPos.Y)
                {
                    maxPos.Y = address.Position.Y;
                }

                if (address.Position.Y < minPos.Y)
                {
                    minPos.Y = address.Position.Y;
                }
            }

            var maxPosInPixel = MapPixelExchange(maxPos);
            var minPosInPixel = MapPixelExchange(minPos);
            Point point = new Point(100 + (maxPosInPixel.X - minPosInPixel.X), 100 + (maxPosInPixel.Y - minPosInPixel.Y));
            pictureBox1.Size = new Size(point);
            image = new Bitmap(point.X, point.Y, PixelFormat.Format32bppArgb);
            gra = Graphics.FromImage(image);
        }

        public Point MapPixelExchange(MapPosition position)
        {
            var pixelX = Convert.ToInt32((position.X - minPos.X) * coefficient + deltaOrigion);
            var pixelY = Convert.ToInt32((position.Y - minPos.Y) * coefficient + deltaOrigion);
            return new Point(pixelX, pixelY);
        }

        public MapPosition MapPositionExchange(MapPosition pixel)
        {
            var posX = (pixel.X - deltaOrigion) / coefficient + minPos.X;
            var posY = (pixel.Y - deltaOrigion) / coefficient + minPos.Y;

            return new MapPosition(posX, posY);
        }

        public Point MoveToImageCenter(Size size, Point oldPoint)
        {
            return new Point(oldPoint.X - (Size.Width / 2), oldPoint.Y - (size.Height / 2));
        }

        private void UcAddressImageItem_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = ((Control)sender).Parent;
            UcAddressImage ucAddressImage = (UcAddressImage)control;
            mouseDownPbPoint = MapPixelExchange(ucAddressImage.Address.Position);
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();
        }

        private void UcAddressImage_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            UcAddressImage ucAddressImage = (UcAddressImage)control;
            mouseDownPbPoint = MapPixelExchange(ucAddressImage.Address.Position);
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();
        }

        private void UcSectionImageItem_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            Control parentControl = control.Parent;
            Point point = new Point(e.X + parentControl.Location.X, e.Y + parentControl.Location.Y);
            mouseDownPbPoint = point;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();

        }

        private void UcSectionImage_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            Point point = new Point(e.X + control.Location.X, e.Y + control.Location.Y);
            mouseDownPbPoint = point;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                mainFlowHandler.StopClearAndReset();

                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AgvlConnectorPage_Click(object sender, EventArgs e)
        {
            if (aseAgvlConnectorForm.IsDisposed)
            {
                InitialAseAgvlConnectorForm();
            }
            if (aseAgvlConnectorForm != null)
            {
                aseAgvlConnectorForm.BringToFront();
                aseAgvlConnectorForm.Show();
            }
        }

        private void InitialAseAgvlConnectorForm()
        {
            aseAgvlConnectorForm = new AseAgvlConnectorForm(asePackage);
            aseAgvlConnectorForm.OnException += AseControlForm_OnException;
        }

        private void AseAgvlConnectorForm_SendCommand(object sender, string e)
        {
            throw new NotImplementedException();
        }

        private void AgvcConnectorPage_Click(object sender, EventArgs e)
        {
            if (middlerForm.IsDisposed)
            {
                middlerForm = new AgvcConnectorForm(agvcConnector);
            }
            middlerForm.BringToFront();
            middlerForm.Show();

        }

        private void ManualMoveCmdPage_Click(object sender, EventArgs e)
        {
            if (aseMoveControlForm.IsDisposed)
            {
                InitialAseMoveControlForm();
            }
            if (aseMoveControlForm != null)
            {
                aseMoveControlForm.BringToFront();
                aseMoveControlForm.Show();
            }
        }

        private void InitialAseMoveControlForm()
        {
            aseMoveControlForm = new AseMoveControlForm();
            aseMoveControlForm.SendMove += AseMoveControlForm_SendMove;
            aseMoveControlForm.OnException += AseControlForm_OnException;
        }

        private void AseMoveControlForm_SendMove(object sender, AseMoveEventArgs e)
        {
            try
            {
                asePackage.aseMoveControl.PartMove(e.AddressDirection, e.MapPosition, e.HeadAngle, e.Speed);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AseControlForm_OnException(object sender, string exMsg)
        {
            LogException(sender.ToString(), exMsg);
        }

        private void RobotControlPage_Click(object sender, EventArgs e)
        {
            if (aseRobotControlForm.IsDisposed)
            {
                InitialAseRobotControlForm();
            }
            if (aseRobotControlForm != null)
            {
                aseRobotControlForm.BringToFront();
                aseRobotControlForm.Show();
            }
        }

        private void InitialAseRobotControlForm()
        {
            aseRobotControlForm = new AseRobotControlForm();
            aseRobotControlForm.SendRobotCommand += AseRobotControlForm_SendRobotCommand;
            aseRobotControlForm.OnException += AseControlForm_OnException;
        }

        private void AseRobotControlForm_SendRobotCommand(object sender, AseRobotEventArgs e)
        {
            try
            {
                RobotCommand robotCommand = GetRobotCommandFromAseRobotControlForm(e);
                asePackage.aseRobotControl.DoRobotCommand(robotCommand);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private RobotCommand GetRobotCommandFromAseRobotControlForm(AseRobotEventArgs e)
        {
            AgvcTransCmd agvcTransCmd = new AgvcTransCmd();
            agvcTransCmd.CommandId = "Cmd001";
            RobotCommand robotCommand;
            if (e.IsLoad)
            {
                robotCommand = new LoadCmdInfo(agvcTransCmd);
                robotCommand.PortAddressId = e.FromPort.PadLeft(5, '0');
                robotCommand.SlotNumber = (EnumSlotNumber)Enum.Parse(typeof(EnumSlotNumber), e.ToPort.Trim('0'));
            }
            else
            {
                robotCommand = new UnloadCmdInfo(agvcTransCmd);
                robotCommand.PortAddressId = e.ToPort.PadLeft(5, '0');
                robotCommand.SlotNumber = (EnumSlotNumber)Enum.Parse(typeof(EnumSlotNumber), e.FromPort.Trim('0'));
            }
            robotCommand.PioDirection = e.PioDirection;

            return robotCommand;
        }

        private void AlarmPage_Click(object sender, EventArgs e)
        {
            if (alarmForm.IsDisposed)
            {
                alarmForm = new AlarmForm(mainFlowHandler);
            }
            alarmForm.BringToFront();
            alarmForm.Show();
        }

        private void VehicleStatusPage_Click(object sender, EventArgs e)
        {
            if (configForm.IsDisposed)
            {
                configForm = new ConfigForm(mainFlowHandler);
            }
            configForm.BringToFront();
            configForm.Show();
        }

        public delegate void DelRenewUI(Control control, string msg);
        public void RenewUI(Control control, string msg)
        {
            if (this.InvokeRequired)
            {
                DelRenewUI del = new DelRenewUI(RenewUI);
                del(control, msg);
            }
            else
            {
                control.Text = msg;
            }
        }

        private void btnSwitchBarcodeLine_Click(object sender, EventArgs e)
        {
            IsBarcodeLineShow = !IsBarcodeLineShow;
            ResetImageAndPb();
        }

        #region Image Functions

        private void SetpupVehicleLocation()
        {
            var mdpx = mouseDownPbPoint.X;
            var mdpy = mouseDownPbPoint.Y;
            var mouseDownPointInPosition = MapPositionExchange(new MapPosition(mouseDownPbPoint.X, mouseDownPbPoint.Y));

            numPositionX.Value = (decimal)mouseDownPointInPosition.X;
            numPositionY.Value = (decimal)mouseDownPointInPosition.Y;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPbPoint = e.Location;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();
        }

        private void TmpPngToImage()
        {
            using (var stream = File.OpenRead("tmp.png"))
            {
                image = Image.FromStream(stream);
            }
        }

        private void PbLoadImage()
        {
            pictureBox1.Image = image;
        }

        public void ImageSaveToTmpPng()
        {
            if (File.Exists("tmp.png"))
            {
                File.Delete("tmp.png");
            }

            image.Save("tmp.png", ImageFormat.Png);
        }

        private void PbLoadTmpPng()
        {
            TmpPngToImage();
            PbLoadImage();
        }

        public void ResetImageAndPb()
        {
            DrawBasicMap();
            ImageSaveToTmpPng();
            TmpPngToImage();
            PbLoadImage();
            //saveNameWithTail = wrapper.SaveNameWithShift;           
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetImageAndPb();

            RenewUI(this, "Reset");
        }

        private string GetSourceFile()
        {
            string srcPicName = "tmp.png";
            if (!File.Exists(srcPicName))
            {
                ImageSaveToTmpPng();
            }

            return srcPicName;
        }

        private void UpdateTmpPng()
        {
            if (File.Exists("tmp.png"))
            {
                File.Delete("tmp.png");
            }

            File.Move("tmp_1.png", "tmp.png");
            File.Delete("tmp_clear.png");
        }

        private string AddTail(string newTail)
        {
            saveNameWithTail = saveNameWithTail + "_" + newTail;

            return saveNameWithTail;
        }

        private void NconvertCmd(string args, string tail)
        {
            string srcPicName = GetSourceFile(); //tmp.png

            ProcessGo(args, srcPicName); //get tmp_1.png

            UpdateTmpPng();//replace tmp_1.png to tmp.png.

            PbLoadTmpPng();

            string finalPicName = AddTail(tail); //saveName add new tail.

            RenewUI(this, finalPicName + " is updated.");
        }

        private void ProcessGo(string args, string picName)
        {
            ProcessStartInfo info = new ProcessStartInfo(@"nconvert.exe")
            {
                WindowStyle = ProcessWindowStyle.Minimized
            };
            info.Arguments = args + picName;
            Process pro = Process.Start(info);
            pro.WaitForExit();
        }

        private void btnXflip_Click(object sender, EventArgs e)
        {
            string xflipArgs = "-out png -xflip ";
            string xflipTail = "Xflip";
            NconvertCmd(xflipArgs, xflipTail);
        }

        private void btnYflip_Click(object sender, EventArgs e)
        {
            string yflipArgs = "-out png -yflip ";
            string yflipTail = "Yflip";
            NconvertCmd(yflipArgs, yflipTail);
        }

        #endregion

        private void MainFlowHandler_OnAvoidCommandCheckedEvent(object sender, AseMovingGuide aseMovingGuide)
        {
            SetTransferCommandMsg("[避車路徑]", aseMovingGuide);
        }

        private void MainFlowHandler_OnOverrideCommandCheckedEvent(object sender, AgvcOverrideCmd agvcOverrideCmd)
        {
            SetTransferCommandMsg("[替代路徑]", agvcOverrideCmd);
        }

        private void MainFlowHandler_OnTransferCommandCheckedEvent(object sender, AgvcTransCmd agvcTransCmd)
        {
            SetTransferCommandMsg("[一般路徑]", agvcTransCmd);
        }

        private void SetTransferCommandMsg(string type, AgvcTransCmd agvcTransCmd)
        {
            try
            {
                TransferCommandMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\r\n",
                                      type, "\t", $"{agvcTransCmd.AgvcTransCommandType}", "\r\n",
                                      $"[命令號={agvcTransCmd.CommandId}]\r\n",
                                      $"[貨號={agvcTransCmd.CassetteId}]\r\n",
                                      $"[取貨站={agvcTransCmd.LoadAddressId}]\r\n",
                                      $"[放貨站={agvcTransCmd.UnloadAddressId}]\r\n"
                                      );

                if (TransferCommandMsg.Length > 32767)
                {
                    TransferCommandMsg = TransferCommandMsg.Substring(32767);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        private void SetTransferCommandMsg(string type, AseMovingGuide aseMovingGuide)
        {
            try
            {
                TransferCommandMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\r\n",
                                      type, "\t", $"{aseMovingGuide.SeqNum}", "\r\n",
                                      $"[避車終點={aseMovingGuide.ToAddressId}]\r\n"
                                      );

                if (TransferCommandMsg.Length > 32767)
                {
                    TransferCommandMsg = TransferCommandMsg.Substring(32767);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        private void MainFlowHandler_OnDoTransferStepEvent(object sender, TransferStep transferStep)
        {
            try
            {
                string msg = "";
                switch (transferStep.GetTransferStepType())
                {
                    case EnumTransferStepType.Load:
                    case EnumTransferStepType.Unload:
                        msg = GetTransferStepMsgFromRobotCommand(transferStep);
                        break;
                    case EnumTransferStepType.Move:
                    case EnumTransferStepType.MoveToCharger:
                        msg = GetTransferStepMsgFromMoveCmdInfo(transferStep);
                        break;
                    case EnumTransferStepType.Empty:
                    default:
                        return;
                }

                SetTransferStepMsg(msg);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private string GetTransferStepMsgFromMoveCmdInfo(TransferStep transferStep)
        {
            try
            {
                MoveCmdInfo moveCmdInfo = (MoveCmdInfo)transferStep;

                string result = string.Concat($"移動\t{moveCmdInfo.GetTransferStepType()}\r\n",
                                       $"[命令號={moveCmdInfo.CmdId}]\r\n",
                                       $"[Addresses={GuideListToString(theVehicle.AseMovingGuide.GuideAddressIds)}]\r\n",
                                       $"[Sections={GuideListToString(theVehicle.AseMovingGuide.GuideSectionIds)}]"
                                       );
                AppendDebugLogMsg(result);
                return result;

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return "";
            }
        }
        private string GetTransferStepMsgFromRobotCommand(TransferStep transferStep)
        {
            try
            {
                RobotCommand robotCommand = (RobotCommand)transferStep;
                return string.Concat($"類型\t{robotCommand.GetTransferStepType()}\r\n",
                                      $"[命令號={robotCommand.CmdId}]\r\n",
                                      $"[貨號={robotCommand.CassetteId}]\r\n",
                                      $"[站點={robotCommand.PortAddressId}]\r\n",
                                      $"[PIO方向={robotCommand.PioDirection}]\r\n",
                                      $"[SlotNumber={robotCommand.SlotNumber}]\r\n",
                                      $"[GateType={theMapInfo.gateTypeMap[robotCommand.PortAddressId]}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return "";
            }
        }

        private void SetTransferStepMsg(string msg)
        {
            TransferStepMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\r\n", msg);

            if (TransferStepMsg.Length > 32767)
            {
                TransferStepMsg = TransferStepMsg.Substring(32767);
            }
        }

        private void timeUpdateUI_Tick(object sender, EventArgs e)
        {
            try
            {
                //UpdatePerformanceCounter(performanceCounterCpu, ucPerformanceCounterCpu);
                //UpdatePerformanceCounter(performanceCounterRam, ucPerformanceCounterRam);

                tbxDebugLogMsg.Text = DebugLogMsg;

                //if (Vehicle.Instance.AutoState == EnumAutoState.Manual && moveCommandDebugMode != null && !moveCommandDebugMode.IsDisposed && moveCommandDebugMode.MainShowRunSectionList)
                //{
                //    ResetSectionColor();
                //    ClearColor();
                //    SetMovingSectionAndEndPosition(moveCommandDebugMode.RunSectionList, moveCommandDebugMode.RunEndAddress);
                //    moveCommandDebugMode.MainShowRunSectionList = false;
                //}
                ucSoc.TagValue = theVehicle.AseBatteryStatus.Percentage.ToString("F1") + $"/" + theVehicle.AseBatteryStatus.Voltage.ToString("F2");

                UpdateListBoxSections(lbxNeedReserveSections, agvcConnector.GetNeedReserveSections());
                UpdateListBoxSections(lbxReserveOkSections, agvcConnector.GetReserveOkSections());

                UpdateAutoManual();
                UpdateVehLocation();
                UpdateCharginAndLoading();
                //DrawReserveSections();
                UpdateThreadPicture();
                UpdateTbxAgvcTransCmd();
                UpdateTbxTransferStep();
                UpdateLastAlarm();
                UpdateAgvcConnection();
                UpdateAgvlConnection();
                //UpdateAgvFailResult();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }


        public void UpdateAgvcConnection()
        {
            try
            {
                if (IsAgvcConnect)
                {
                    txtAgvcConnection.Text = "AGVC 連線中";
                    txtAgvcConnection.BackColor = Color.LightGreen;
                    radAgvcOnline.Checked = true;
                }
                else
                {
                    txtAgvcConnection.Text = "AGVC 斷線";
                    txtAgvcConnection.BackColor = Color.Pink;
                    radAgvcOffline.Checked = true;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void UpdateAgvlConnection()
        {
            try
            {
                if (IsAgvlConnect)
                {
                    txtAgvlConnection.Text = "AGVL 連線中";
                    txtAgvlConnection.BackColor = Color.LightGreen;
                    radAgvlOnline.Checked = true;
                }
                else
                {
                    txtAgvlConnection.Text = "AGVL 斷線";
                    txtAgvlConnection.BackColor = Color.Pink;
                    radAgvlOffline.Checked = true;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void UpdateLastAlarm()
        {
            try
            {
                txtLastAlarm.Text = LastAlarmMsg;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void UpdateTbxTransferStep()
        {
            try
            {
                tbxTransferStepMsg.Text = TransferStepMsg;
                //tspbCommding.Maximum = mainFlowHandler.GetTransferStepsCount() - 1;
                //if (mainFlowHandler.TransferStepsIndex >= tspbCommding.Maximum)
                //{
                //    tspbCommding.Value = tspbCommding.Maximum;
                //}
                //else
                //{
                //    tspbCommding.Value = mainFlowHandler.TransferStepsIndex;
                //}
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void UpdateTbxAgvcTransCmd()
        {
            try
            {
                tbxTransferCommandMsg.Text = TransferCommandMsg;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private string GuideListToString(List<string> aList)
        {
            return string.Join(", ", aList.ToArray());
        }
        private string GetListPositionsToString(List<MapPosition> positions)
        {
            List<string> result = new List<string>();
            foreach (var position in positions)
            {
                result.Add($"({position.X},{position.Y})");
            }
            return GuideListToString(result);
        }
        private void UpdateThreadPicture()
        {
            try
            {
                Vehicle theVehicle = Vehicle.Instance;

                txtTransferStep.Text = mainFlowHandler.GetCurrentTransferStepType().ToString();

                if (mainFlowHandler.GetTransferStepsCount() > 0)
                {
                    var stepIndex = mainFlowHandler.TransferStepsIndex;
                    var moveIndex = 0;
                    if (mainFlowHandler.IsMoveStep())
                    {
                        AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);
                        if (aseMovingGuide.MovingSections.Count > 0)
                        {
                            moveIndex = aseMovingGuide.MovingSectionsIndex;
                        }

                    }
                    txtTrackPosition.Text = $"{stepIndex},{moveIndex}";
                }

                txtAskingReserve.Text = $"ID:{agvcConnector.GetAskingReserveSection().Id}";
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private Color GetThreadStatusColor(EnumThreadStatus threadStatus)
        {
            switch (threadStatus)
            {
                case EnumThreadStatus.Start:
                    return Color.GreenYellow;
                case EnumThreadStatus.Pause:
                    return Color.Orange;
                case EnumThreadStatus.Working:
                    return Color.Green;
                case EnumThreadStatus.Stop:
                    return Color.Red;
                case EnumThreadStatus.None:
                default:
                    return Color.Black;
            }
        }
        private void UpdateAutoManual()
        {
            try
            {
                switch (Vehicle.Instance.AutoState)
                {
                    case EnumAutoState.Manual:
                        btnAutoManual.BackColor = Color.Pink;
                        txtCanAuto.Visible = true;
                        txtCannotAutoReason.Visible = true;
                        //if (mainFlowConfig.CustomerName == "AUO")
                        //{
                        //    if (true/*jogPitchForm.CanAuto*/)
                        //    {
                        //        txtCanAuto.BackColor = Color.LightGreen;
                        //        txtCanAuto.Text = "可以 Auto";
                        //    }
                        //    else
                        //    {
                        //        txtCanAuto.BackColor = Color.Pink;
                        //        txtCanAuto.Text = "不行 Auto";
                        //    }
                        //}
                        break;
                    case EnumAutoState.Auto:
                    default:
                        btnAutoManual.BackColor = Color.LightGreen;
                        txtCanAuto.Visible = false;
                        txtCannotAutoReason.Visible = false;
                        break;
                }

                btnAutoManual.Text = "Now : " + Vehicle.Instance.AutoState.ToString();

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void DrawReserveSections()
        {
            var transferStepCount = mainFlowHandler.GetTransferStepsCount();
            if (transferStepCount < 1)
            {
                return;
            }

            if (!mainFlowHandler.IsMoveStep())
            {
                return;
            }

            var needReserveSections = agvcConnector.GetNeedReserveSections();
            UpdateListBoxSections(lbxNeedReserveSections, needReserveSections);
            foreach (var section in needReserveSections)
            {
                allUcSectionImages[section.Id].DrawSectionImage(allPens["YellowGreen2"]);
            }

            var reserveOkSections = agvcConnector.GetReserveOkSections();
            UpdateListBoxSections(lbxReserveOkSections, reserveOkSections);
            foreach (var section in reserveOkSections)
            {
                allUcSectionImages[section.Id].DrawSectionImage(allPens["Green2"]);
            }

        }
        private void UpdateVehLocation()
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

                var lastPos = aseMoveStatus.LastMapPosition;
                ucLastPosition.TagValue = $"({(int)lastPos.X},{(int)lastPos.Y})";
                tstextLastPosX.Text = lastPos.X.ToString("F2");
                tstextLastPosY.Text = lastPos.Y.ToString("F2");

                var lastAddress = aseMoveStatus.LastAddress;
                ucMapAddress.TagValue = lastAddress.Id;

                var lastSection = aseMoveStatus.LastSection;
                ucMapSection.TagValue = lastSection.Id;

                ucVehicleImage.Hide();
                ucVehicleImage.Location = MapPixelExchange(lastPos);
                ucVehicleImage.FixToCenter();
                ucVehicleImage.Show();
                ucVehicleImage.BringToFront();

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void UpdateCharginAndLoading()
        {
            try
            {
                //TODO: SlotA and SlotB
                if (theVehicle.AseCarrierSlotL.CarrierSlotStatus != EnumAseCarrierSlotStatus.Empty)
                {
                    ucLoading.TagValue = theVehicle.AseCarrierSlotL.CarrierSlotStatus.ToString();
                    ucVehicleImage.Loading = true;
                    ucCstId.TagValue = theVehicle.AseCarrierSlotL.CarrierId;
                    aseRobotControlForm.CassetteId = theVehicle.AseCarrierSlotL.CarrierId;
                }
                else if (theVehicle.AseCarrierSlotR.CarrierSlotStatus != EnumAseCarrierSlotStatus.Empty)
                {
                    ucLoading.TagValue = theVehicle.AseCarrierSlotR.CarrierSlotStatus.ToString();
                    ucVehicleImage.Loading = true;
                    ucCstId.TagValue = theVehicle.AseCarrierSlotR.CarrierId;
                    aseRobotControlForm.CassetteId = theVehicle.AseCarrierSlotR.CarrierId;
                }
                else
                {
                    ucLoading.TagValue = EnumAseCarrierSlotStatus.Empty.ToString();
                    ucVehicleImage.Loading = false;
                    ucCstId.TagValue = "";
                    aseRobotControlForm.CassetteId = "";
                }

                ucCharging.TagValue = theVehicle.IsCharging ? "Yes" : "No";
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void UpdatePerformanceCounter(PerformanceCounter performanceCounter, UcLabelTextBox ucLabelTextBox)
        {
            double value = performanceCounter.NextValue();
            ucLabelTextBox.TagValue = string.Format("{0:0.0}%", value);
        }
        private void UpdateListBoxSections(ListBox aListBox, List<MapSection> aListOfSections)
        {
            try
            {
                aListBox.Items.Clear();
                if (aListOfSections.Count > 0)
                {
                    for (int i = 0; i < aListOfSections.Count; i++)
                    {
                        aListBox.Items.Add(aListOfSections[i].Id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void btnKeyInPosition_Click(object sender, EventArgs e)
        {
            try
            {
                int posX = (int)numPositionX.Value;
                int posY = (int)numPositionY.Value;
                Vehicle.Instance.AseMoveStatus.LastMapPosition = new MapPosition(posX, posY);
                mainFlowHandler.UpdateVehiclePositionManual();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        public delegate void RichTextBoxAppendHeadCallback(RichTextBox richTextBox, string msg);
        public void RichTextBoxAppendHead(RichTextBox richTextBox, string msg)
        {
            if (richTextBox.InvokeRequired)
            {
                RichTextBoxAppendHeadCallback mydel = new RichTextBoxAppendHeadCallback(RichTextBoxAppendHead);
                this.Invoke(mydel, new object[] { richTextBox, msg });
            }
            else
            {
                var timeStamp = DateTime.Now.ToString("[yyyy-MM-dd HH-mm-ss.fff] ");
                msg = msg + Environment.NewLine;
                richTextBox.Text = string.Concat(timeStamp, msg, richTextBox.Text);

                int RichTextBoxMaxLines = 10000;  // middlerConfig.RichTextBoxMaxLines;

                if (richTextBox.Lines.Count() > RichTextBoxMaxLines)
                {
                    string[] sNewLines = new string[RichTextBoxMaxLines];
                    Array.Copy(richTextBox.Lines, 0, sNewLines, 0, sNewLines.Length);
                    richTextBox.Lines = sNewLines;
                }
            }
        }

        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            btnAlarmReset.Enabled = false;
            TakeAPicture();
            mainFlowHandler.ResetAllarms();
            Thread.Sleep(500);
            btnAlarmReset.Enabled = true;
        }

        private void btnBuzzOff_Click(object sender, EventArgs e)
        {
            mainFlowHandler.BuzzOff();
        }

        private void btnAutoManual_Click(object sender, EventArgs e)
        {
            btnAutoManual.Enabled = false;
            SwitchAutoStatus();
            ClearColor();
            Thread.Sleep(500);
            btnAutoManual.Enabled = true;
        }

        public void SwitchAutoStatus()
        {
            try
            {
                switch (theVehicle.AutoState)
                {
                    case EnumAutoState.Auto:
                        theVehicle.AutoState = EnumAutoState.Manual;
                        mainFlowHandler.StopClearAndReset();
                        break;
                    case EnumAutoState.Manual:
                        theVehicle.AutoState = EnumAutoState.Auto;
                        break;
                    case EnumAutoState.PreManual:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void TheVehicle_OnAutoStateChangeEvent(object sender, EnumAutoState autoState)
        {
            try
            {
                switch (autoState)
                {
                    case EnumAutoState.Auto:
                        if (!mainFlowConfig.IsSimulation)
                        {
                            TakeAPicture();
                        }
                        AppendDebugLogMsg($"Manual 切換 Auto 成功");
                        ResetAllAbnormalMsg();
                        break;
                    case EnumAutoState.Manual:
                        AppendDebugLogMsg($"Auto 切換 Manual 成功");

                        break;
                    case EnumAutoState.PreManual:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void ResetAllAbnormalMsg()
        {
            mainFlowHandler.MainFlowAbnormalMsg = "";
            agvcConnector.AgvcConnectorAbnormalMsg = "";
            mainFlowHandler.ResetMoveControlStopResult();
            RobotAbnormalReasonMsg = "";
            BatterysAbnormalReasonMsg = "";

        }

        private void btnKeyInSoc_Click(object sender, EventArgs e)
        {
            mainFlowHandler.SetupVehicleSoc(decimal.ToDouble(numSoc.Value));
        }

        private void radAgvcOnline_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radAgvcOnline.Checked)
                {
                    if (!agvcConnector.IsConnected())
                    {
                        agvcConnector.ReConnect();
                    }
                }
            }
            catch (Exception ex)
            {

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void radAgvcOffline_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radAgvcOffline.Checked)
                {
                    if (agvcConnector.IsConnected())
                    {
                        agvcConnector.DisConnect();
                    }
                }
            }
            catch (Exception ex)
            {

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void radAgvlOnline_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radAgvlOnline.Checked)
                {
                    AsePackage asePackage = mainFlowHandler.GetAsePackage();
                    if (!asePackage.IsConnected())
                    {
                        asePackage.Connect();
                    }
                }
            }
            catch (Exception ex)
            {

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void radAgvlOffline_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radAgvlOffline.Checked)
                {
                    AsePackage asePackage = mainFlowHandler.GetAsePackage();
                    if (asePackage.IsConnected())
                    {
                        asePackage.DisConnect();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void 工程師ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 關閉ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer_SetupInitialSoc_Tick(object sender, EventArgs e)
        {
            if (mainFlowHandler.IsFirstAhGet)
            {
                var initialSoc = mainFlowHandler.InitialSoc;
                mainFlowHandler.SetupVehicleSoc(initialSoc);
                timer_SetupInitialSoc.Enabled = false;
            }
        }

        private void ClearColor()
        {
            foreach (UcAddressImage ucAddressImage in changeColorAddressList)
            {
                ucAddressImage.BackColor = Color.FromArgb(0, 255, 255, 255);
            }

            changeColorAddressList = new List<UcAddressImage>();
        }

        private List<MapAddress> mainFormAddNodes = new List<MapAddress>();
        private List<UcAddressImage> changeColorAddressList = new List<UcAddressImage>();

        private MapAddress endAddress = null;
        private List<UcSectionImage> changeColorSectionList = new List<UcSectionImage>();
        private int changeColorSectionListIndex = 0;
        private int removeSectionIndex = 0;

        public delegate void UcSectionImageSetColorCallback(UcSectionImage ucSectionImage, string keyword);
        public void UcSectionImageSetColor(UcSectionImage ucSectionImage, string keyword)
        {
            if (ucSectionImage.InvokeRequired)
            {
                UcSectionImageSetColorCallback mydel = new UcSectionImageSetColorCallback(UcSectionImageSetColor);
                this.Invoke(mydel, new object[] { ucSectionImage, keyword });
            }
            else
            {
                ucSectionImage.SetColor(allPens[keyword]);
            }
        }

        public delegate void RefreshMapCallback(MainForm mainForm);
        public void RefreshMap(MainForm mainForm)
        {
            if (mainForm.InvokeRequired)
            {
                RefreshMapCallback del = new RefreshMapCallback(RefreshMap);
                this.Invoke(del, mainForm);
            }
            else
            {
                mainForm.Refresh();
            }
        }

        public void ChangeToNormalSection(string sectionId)
        {
            try
            {
                if (removeSectionIndex < changeColorSectionList.Count &&
                    changeColorSectionList[removeSectionIndex].Id == sectionId)
                {
                    //changeColorSectionList[removeSectionIndex].SetColor(allPens["NormalSection"]);
                    UcSectionImageSetColor(changeColorSectionList[removeSectionIndex], "NormalSection");
                    removeSectionIndex++;
                    RefreshMap(mainForm);
                }
            }
            catch { }
        }

        private void GetReserveSection(string sectionId)
        {
            try
            {
                if (changeColorSectionListIndex < changeColorSectionList.Count &&
                    changeColorSectionList[changeColorSectionListIndex].Id == sectionId)
                {
                    //changeColorSectionList[changeColorSectionListIndex].SetColor(allPens["GetReserveSection"]);
                    UcSectionImageSetColor(changeColorSectionList[changeColorSectionListIndex], "GetReserveSection");
                    changeColorSectionListIndex++;
                    RefreshMap(mainForm);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void ResetSectionColor()
        {
            try
            {
                if (changeColorSectionList == null)
                    changeColorSectionList = new List<UcSectionImage>();
                else if (changeColorSectionList.Count != 0)
                {
                    for (int i = removeSectionIndex; i < changeColorSectionList.Count; i++)
                    {
                        //changeColorSectionList[i].SetColor(allPens["NormalSection"]);
                        UcSectionImageSetColor(changeColorSectionList[i], "NormalSection");
                    }


                    changeColorSectionList = new List<UcSectionImage>();
                }

                changeColorSectionListIndex = 0;
                removeSectionIndex = 0;

                if (endAddress != null)
                {
                    if (allUcAddressImages.ContainsKey(endAddress.Id))
                        allUcAddressImages[endAddress.Id].BackColor = Color.Transparent;

                    endAddress = null;
                }

                RefreshMap(mainForm);
            }
            catch { }
        }

        public void SetMovingSectionAndEndPosition(List<MapSection> movingSection, MapAddress movingEndAddress)
        {
            try
            {
                ResetSectionColor();

                foreach (MapSection section in movingSection)
                {
                    if (allUcSectionImages.ContainsKey(section.Id))
                    {
                        var ucSectionImage = allUcSectionImages[section.Id];
                        changeColorSectionList.Add(ucSectionImage);
                        UcSectionImageSetColor(ucSectionImage, "NotGetReserveSection");
                    }
                }

                changeColorSectionListIndex = 0;
                removeSectionIndex = 0;

                if (movingEndAddress != null)
                {
                    if (allUcAddressImages.ContainsKey(movingEndAddress.Id))
                    {
                        allUcAddressImages[movingEndAddress.Id].BackColor = Color.Green;
                        endAddress = movingEndAddress;
                    }
                }

                RefreshMap(mainForm);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void btnReloadConfig_Click(object sender, EventArgs e)
        {
            ClearColor();
            //SimulateAGVCMoveCommand();
            mainFormAddNodes = new List<MapAddress>();

            //if (theVehicle.AutoState == EnumAutoState.Manual)
            //{
            //    mainFlowHandler.ReloadConfig();
            //}
        }

        private void 模擬測試ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (mainFlowConfig.CustomerName == "AUO")
            //{
            //    ((MoveCommandDebugModeForm)moveCommandForm).button_SimulationMode_Click(this, e);
            //    integrateCommandForm.IsSimulation = !integrateCommandForm.IsSimulation;
            //    if (integrateCommandForm.IsSimulation)
            //    {
            //        mainFlowHandler.SetupVehicleSoc(100);
            //    }
            //}
            mainFlowHandler.IsSimulation = !mainFlowHandler.IsSimulation;
        }

        private void btnPrintScreen_Click(object sender, EventArgs e)
        {
            TakeAPicture();
        }

        private void TakeAPicture()
        {
            Image image = new Bitmap(1920, 1080);
            Graphics graphics = Graphics.FromImage(image);
            graphics.CopyFromScreen(0, 0, 0, 0, new Size(1920, 1080));
            IntPtr intPtr = graphics.GetHdc();
            graphics.ReleaseHdc(intPtr);
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = image;
            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpeg";
            string savename = Path.Combine(Environment.CurrentDirectory, "Log", timeStamp);
            image.Save(savename);
        }

        private void LogException(string classMethodName, string exMsg)
        {
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }

        private void LogDebug(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, "Device", "CarrierID", msg));
        }

        private void btnRefreshPosition_Click(object sender, EventArgs e)
        {
            asePackage.aseMoveControl.SendPositionReportRequest();
        }


    }
}
