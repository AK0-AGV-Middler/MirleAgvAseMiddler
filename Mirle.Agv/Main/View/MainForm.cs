using ClsMCProtocol;
using Mirle.Agv.Controller;
using Mirle.Agv.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using Mirle.Agv.Model.TransferCmds;
using Mirle.Agv;
using Mirle.Agv.Controller.Tools;
using System.Reflection;

namespace Mirle.Agv.View
{
    public partial class MainForm : Form
    {
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);
        private MainFlowHandler mainFlowHandler;
        private MoveControlHandler moveControlHandler;
        private MiddleAgent middleAgent;
        private MiddlerForm middlerForm;
        private AlarmForm alarmForm;
        private AlarmHandler alarmHandler;
        private PlcForm plcForm;
        private PlcAgent plcAgent;
        private MCProtocol mcProtocol;
        private MoveCommandDebugModeForm moveCommandDebugMode;
        private JogPitchForm jogPitch;
        private Panel panelLeftUp;
        private Panel panelLeftDown;
        private Panel panelRightUp;
        private Panel panelRightDown;
        private MapInfo theMapInfo = new MapInfo();
        //PerformanceCounter performanceCounterCpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
        //PerformanceCounter performanceCounterRam = new PerformanceCounter("Memory", "Available MBytes");
        //PerformanceCounter performanceCounterRam = new PerformanceCounter("Memory", "% Committed Bytes in Use");
        private LoggerAgent theLoggerAgent = LoggerAgent.Instance;
        private bool IsAskingReserve { get; set; }
        private string LastAskingReserveSectionId { get; set; } = "";
        private string LastAgvcTransferCommandId { get; set; } = "";
        private EnumTransferStepType LastTransferStepType { get; set; } = EnumTransferStepType.Empty;
        private int lastAlarmId = 0;

        #region MouseDownCalculus

        private Point mouseDownPbPoint;
        private Point mouseDownScreenPoint;

        #endregion

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
        private double deltaOrigion = 25;
        private double triangleCoefficient = (double)(1 / Math.Sqrt(3.0));
        private UcVehicleImage ucVehicleImage = new UcVehicleImage();
        private MapPosition minPos = new MapPosition();
        private MapPosition maxPos = new MapPosition();

        #endregion

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
            theMapInfo = mainFlowHandler.TheMapInfo;
            alarmHandler = mainFlowHandler.GetAlarmHandler();
            moveControlHandler = mainFlowHandler.GetMoveControlHandler();
            plcAgent = mainFlowHandler.GetPlcAgent();
            mcProtocol = mainFlowHandler.GetMcProtocol();
            middleAgent = mainFlowHandler.GetMiddleAgent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitialForms();
            InitialPaintingItems();
            InitialPanels();
            InitialEvents();
            ResetImageAndPb();
            InitialSoc();
        }

        private void InitialForms()
        {
            moveCommandDebugMode = new MoveCommandDebugModeForm(moveControlHandler, theMapInfo);
            moveCommandDebugMode.WindowState = FormWindowState.Normal;

            middlerForm = new MiddlerForm(middleAgent);
            middlerForm.WindowState = FormWindowState.Normal;

            alarmForm = new AlarmForm(mainFlowHandler);
            middlerForm.WindowState = FormWindowState.Normal;

            plcForm = new PlcForm(mcProtocol, plcAgent);
            plcForm.WindowState = FormWindowState.Normal;

            jogPitch = new JogPitchForm(moveControlHandler);
            jogPitch.WindowState = FormWindowState.Normal;

            numPositionX.Maximum = decimal.MaxValue;
            numPositionY.Maximum = decimal.MaxValue;
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
            mainFlowHandler.OnMessageShowEvent += middlerForm.SendOrReceiveCmdToRichTextBox;
            mainFlowHandler.OnMessageShowEvent += ShowMsgOnMainForm;
            middleAgent.OnMessageShowOnMainFormEvent += ShowMsgOnMainForm;
            middleAgent.OnConnectionChangeEvent += MiddleAgent_OnConnectionChangeEvent;
            alarmHandler.OnSetAlarmEvent += AlarmHandler_OnSetAlarmEvent;
            alarmHandler.OnResetAllAlarmsEvent += AlarmHandler_OnResetAllAlarmsEvent;

        }

        private void InitialSoc()
        {
            mainFlowHandler.SetupFakeVehicleSoc(decimal.ToDouble(numSoc.Value));
        }

        public delegate void RadioButtonCheckDel(RadioButton radioButton, bool isCheck);
        public void RadioButtonCheck(RadioButton radioButton, bool isCheck)
        {
            if (radioButton.InvokeRequired)
            {
                RadioButtonCheckDel mydel = new RadioButtonCheckDel(RadioButtonCheck);
                this.Invoke(mydel, new object[] { radioButton, isCheck });
            }
            else
            {
                radioButton.Checked = isCheck;
            }
        }
        private void MiddleAgent_OnConnectionChangeEvent(object sender, bool isConnect)
        {
            if (isConnect)
            {
                RadioButtonCheck(radOnline, true);
            }
            else
            {
                RadioButtonCheck(radOffline, true);
            }
        }

        private void AlarmHandler_OnSetAlarmEvent(object sender, Alarm alarm)
        {
            var msg = $"AlarmHandler : Set Alarm, [Id={alarm.Id}][Text={alarm.AlarmText}]";
            RichTextBoxAppendHead(richTextBox1, msg);
        }
        private void AlarmHandler_OnResetAllAlarmsEvent(object sender, List<Alarm> alarms)
        {
            btnAlarmReset.Enabled = false;
            var msg = $"AlarmHandler : Reset All Alarms, [Count={alarms.Count}]";
            RichTextBoxAppendHead(richTextBox1, msg);

            try
            {
                var xx = alarmHandler.allAlarms.First(x => x.Key != 0).Key;
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }

            btnAlarmReset.Enabled = true;
        }

        private void ShowMsgOnMainForm(object sender, string msg)
        {
            RichTextBoxAppendHead(richTextBox1, msg);
        }

        public void DrawBasicMap()
        {
            try
            {

                //pictureBox1.Parent = panelLeftUp;
                SetupImageRegion();
                //pictureBox1.Size = new Size(2000, 2000);
                //image = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
                //gra = Graphics.FromImage(image);


                if (IsBarcodeLineShow)
                {
                    //Draw Barcode in blackDash
                    var allMapBarcodeLines = theMapInfo.allMapBarcodeLines.Values.ToList();
                    foreach (var rowBarcode in allMapBarcodeLines)
                    {
                        var headPosInPixel = MapPixelExchange(rowBarcode.HeadBarcode.Position);
                        var tailPosInPixel = MapPixelExchange(rowBarcode.TailBarcode.Position);

                        gra.DrawLine(allPens["BlackDashDot1"], headPosInPixel.X, headPosInPixel.Y, tailPosInPixel.X, tailPosInPixel.Y);
                    }
                }

                allUcSectionImages.Clear();
                // Draw Sections in blueLine
                var allMapSections = theMapInfo.allMapSections.Values.ToList();
                foreach (var section in allMapSections)
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
                            ucSectionImage.Location = new Point(ucSectionImage.Location.X - ucSectionImage.labelSize.Width, ucSectionImage.Location.Y);
                            break;
                        case EnumSectionType.R2000:
                            break;
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
                var allMapAddresses = theMapInfo.allMapAddresses.Values.ToList();
                foreach (var address in allMapAddresses)
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
                }

                pictureBox1.SendToBack();
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void SetupImageRegion()
        {
            foreach (var address in theMapInfo.allMapAddresses.Values)
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
            Point point = new Point(2 * (maxPosInPixel.X - minPosInPixel.X), 2 * (maxPosInPixel.Y - minPosInPixel.Y));
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
            mainFlowHandler.StopVisitTransferSteps();
            ShutdownEvent.Set();
            PauseEvent.Set();

            Application.Exit();
            Environment.Exit(Environment.ExitCode);

        }

        private void MiddlerPage_Click(object sender, EventArgs e)
        {
            if (middlerForm.IsDisposed)
            {
                middlerForm = new MiddlerForm(middleAgent);
            }
            middlerForm.BringToFront();
            middlerForm.Show();

        }

        private void ManualMoveCmdPage_Click(object sender, EventArgs e)
        {
            if (moveCommandDebugMode.IsDisposed)
            {
                moveCommandDebugMode = new MoveCommandDebugModeForm(moveControlHandler, theMapInfo);
            }
            moveCommandDebugMode.BringToFront();
            moveCommandDebugMode.Show();
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

        private void PlcPage_Click(object sender, EventArgs e)
        {
            if (plcForm.IsDisposed)
            {
                plcForm = new PlcForm(mcProtocol, plcAgent);
            }
            plcForm.BringToFront();
            plcForm.Show();
        }

        private void JogPage_Click(object sender, EventArgs e)
        {
            if (jogPitch.IsDisposed)
            {
                jogPitch = new JogPitchForm(moveControlHandler);
            }
            jogPitch.BringToFront();
            jogPitch.Show();
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

            string msg = $"Position({mouseDownPointInPosition.X},{mouseDownPointInPosition.Y})";

            numPositionX.Value = (decimal)mouseDownPointInPosition.X;
            numPositionY.Value = (decimal)mouseDownPointInPosition.Y;

            RenewUI(this, msg);
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

        private void btnResizePercent_Click(object sender, EventArgs e)
        {
            string resizePercentArgs = "-out png -resize " + txtResizePercent.Text + "% " + txtResizePercent.Text + "% ";
            string resizePercentTail = "Resize" + txtResizePercent.Text;
            NconvertCmd(resizePercentArgs, resizePercentTail);
        }

        private void btnRotate_Click(object sender, EventArgs e)
        {
            string rotateArgs = "-out png -rotate " + txtRotateAngle.Text + " ";
            string rotateTail = "Rotate" + txtRotateAngle.Text;
            NconvertCmd(rotateArgs, rotateTail);

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

        private void timeUpdateUI_Tick(object sender, EventArgs e)
        {
            //UpdatePerformanceCounter(performanceCounterCpu, ucPerformanceCounterCpu);
            //UpdatePerformanceCounter(performanceCounterRam, ucPerformanceCounterRam);
            ucSoc.TagValue = Vehicle.Instance.ThePlcVehicle.Batterys.Percentage.ToString("F2");
            if (middleAgent.GetAskingReserveSectionClone().Id != LastAskingReserveSectionId)
            {
                LastAskingReserveSectionId = middleAgent.GetAskingReserveSectionClone().Id;
                lbxAskReserveSection.Items.Clear();
                lbxAskReserveSection.Items.Add(LastAskingReserveSectionId);
            }
            UpdateListBoxSections(lbxNeedReserveSections, middleAgent.GetNeedReserveSections());
            UpdateListBoxSections(lbxReserveOkSections, middleAgent.GetReserveOkSections());

            UpdateAutoManual();
            UpdateVehLocationAndLoading();
            //DrawReserveSections();
            UpdateThreadPicture();
            UpdateRtbAgvcTransCmd();
            UpdateRtbTransferStep();
            UpdateLastAlarm();
        }

        private void UpdateLastAlarm()
        {
            var alarm = alarmHandler.LastAlarm;
            if (lastAlarmId != alarm.Id)
            {
                lastAlarmId = alarm.Id;
                var msg = $"[{alarm.Id}]\n[{alarm.AlarmText}]";
                txtLastAlarm.Text = msg;
            }
        }

        private void UpdateRtbTransferStep()
        {
            if (mainFlowHandler.GetTransferStepsCount() == 0)
            {
                return;
            }

            TransferStep transferStep = mainFlowHandler.GetCurTransferStep();

            if (transferStep.GetTransferStepType() == LastTransferStepType && mainFlowHandler.GetAgvcTransCmd().CommandId == LastAgvcTransferCommandId)
            {
                return;
            }
            LastTransferStepType = transferStep.GetTransferStepType();

            switch (transferStep.GetTransferStepType())
            {
                case EnumTransferStepType.Move:
                    UpdateMoveCmdInfo(transferStep);
                    break;
                case EnumTransferStepType.Load:
                    UpdateLoadCmdInfo(transferStep);
                    break;
                case EnumTransferStepType.Unload:
                    UpdateUnloadCmdInfo(transferStep);
                    break;
                case EnumTransferStepType.Empty:
                default:
                    break;
            }
        }

        private void UpdateRtbAgvcTransCmd()
        {
            if (mainFlowHandler.IsAgvcTransferCommandEmpty())
            {
                return;
            }

            AgvcTransCmd agvcTransCmd = mainFlowHandler.GetAgvcTransCmd();

            if (agvcTransCmd.CommandId == LastAgvcTransferCommandId)
            {
                return;
            }
            LastAgvcTransferCommandId = agvcTransCmd.CommandId;

            var cmdInfo = $"\n" +
                          $"[SeqNum={agvcTransCmd.SeqNum}] [Type={agvcTransCmd.CommandType}]\n" +
                          $"[CmdId={agvcTransCmd.CommandId}] [CstId={agvcTransCmd.CassetteId}]\n" +
                          $"[LoadAdr={agvcTransCmd.LoadAddress}] [UnloadAdr={agvcTransCmd.UnloadAddress}]\n" +
                          $"[LoadAdrs={GuideListToString(agvcTransCmd.ToLoadAddresses)}]\n" +
                          $"[LoadSecs={GuideListToString(agvcTransCmd.ToLoadSections)}]\n" +
                          $"[UnloadAdrs={GuideListToString(agvcTransCmd.ToUnloadAddresses)}]\n" +
                          $"[UnloadSecs={GuideListToString(agvcTransCmd.ToUnloadSections)}]";

            RichTextBoxAppendHead(rtbAgvcTransCmd, cmdInfo);
        }

        private void UpdateUnloadCmdInfo(TransferStep transferStep)
        {
            UnloadCmdInfo unloadCmdInfo = (UnloadCmdInfo)transferStep;

            var cmdInfo = $"\n" +
               $"[Type={transferStep.GetTransferStepType()}]\n" +
               $"[CmdId={unloadCmdInfo.CmdId}] [CstId={unloadCmdInfo.CstId}]\n" +
               $"[UnoadAdr={unloadCmdInfo.UnloadAddress}]\n" +
               $"[StageDirection={unloadCmdInfo.StageDirection}]\n" +
               $"[StageNum={unloadCmdInfo.StageNum}]\n" +
               $"[IsEqPio={unloadCmdInfo.IsEqPio}]\n" +
               $"[ForkSpeed={unloadCmdInfo.ForkSpeed}]";

            RichTextBoxAppendHead(rtbTransferStep, cmdInfo);
        }

        private void UpdateLoadCmdInfo(TransferStep transferStep)
        {
            LoadCmdInfo loadCmdInfo = (LoadCmdInfo)transferStep;

            var cmdInfo = $"\n" +
               $"[Type={transferStep.GetTransferStepType()}]\n" +
               $"[CmdId={loadCmdInfo.CmdId}] [CstId={loadCmdInfo.CstId}]\n" +
               $"[LoadAdr={loadCmdInfo.LoadAddress}]\n" +
               $"[StageDirection={loadCmdInfo.StageDirection}]\n" +
               $"[StageNum={loadCmdInfo.StageNum}]\n" +
               $"[IsEqPio={loadCmdInfo.IsEqPio}]\n" +
               $"[ForkSpeed={loadCmdInfo.ForkSpeed}]";

            RichTextBoxAppendHead(rtbTransferStep, cmdInfo);
        }

        private void UpdateMoveCmdInfo(TransferStep transferStep)
        {
            MoveCmdInfo moveCmdInfo = (MoveCmdInfo)transferStep;

            var cmdInfo = $"\n" +
                         $"[Type={transferStep.GetTransferStepType()}]\n" +
                         $"[CmdId={moveCmdInfo.CmdId}] [CstId={moveCmdInfo.CstId}]\n" +
                         $"[Adrs={GuideListToString(moveCmdInfo.AddressIds)}]\n" +
                         $"[Secs={GuideListToString(moveCmdInfo.SectionIds)}]\n" +
                         $"[Positions={GetListPositionsToString(moveCmdInfo.AddressPositions)}]\n" +
                         $"[Actions={GetListActionsToString(moveCmdInfo.AddressActions)}]\n" +
                         $"[Speeds={GetListSpeedsToString(moveCmdInfo.SectionSpeedLimits)}]";

            RichTextBoxAppendHead(rtbTransferStep, cmdInfo);
        }

        private object GetListSpeedsToString(List<double> speeds)
        {
            List<string> result = new List<string>();
            foreach (var speed in speeds)
            {
                result.Add($"({(int)speed})");
            }
            return GuideListToString(result);
        }

        private string GetListActionsToString(List<EnumAddressAction> actions)
        {
            List<string> result = new List<string>();
            foreach (var action in actions)
            {
                result.Add($"({action})");
            }
            return GuideListToString(result);
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
            Vehicle theVehicle = Vehicle.Instance;

            picVisitTransferSteps.BackColor = GetThreadStatusColor(theVehicle.VisitTransferStepsStatus);
            txtTransferStep.Text = "Step : " + mainFlowHandler.GetCurrentTransferStepType().ToString();

            picTrackPosition.BackColor = GetThreadStatusColor(theVehicle.TrackPositionStatus);
            var realPos = Vehicle.Instance.CurVehiclePosition.RealPosition;
            var posText = $"({(int)realPos.X},{(int)realPos.Y})";
            txtTrackPosition.Text = mainFlowHandler.GetTransferStepsCount() > 0 ? "Cmd : " + posText : "NoCmd : " + posText;

            picAskReserve.BackColor = GetThreadStatusColor(theVehicle.AskReserveStatus);
            txtAskingReserve.Text = $"Asking : {middleAgent.GetAskingReserveSectionClone().Id}";

            picWatchLowPower.BackColor = GetThreadStatusColor(theVehicle.WatchLowPowerStatus);
            var batterys = theVehicle.ThePlcVehicle.Batterys;
            txtWatchLowPower.Text = $"Soc/Gap : {(int)batterys.Percentage}/{(int)batterys.PortAutoChargeLowSoc}";
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
            switch (Vehicle.Instance.AutoState)
            {
                case EnumAutoState.Manual:
                    btnAutoManual.ForeColor = Color.OrangeRed;
                    break;
                case EnumAutoState.Auto:
                default:
                    btnAutoManual.ForeColor = Color.ForestGreen;
                    break;
            }

            btnAutoManual.Text = "Now : " + Vehicle.Instance.AutoState.ToString();
        }
        private void DrawReserveSections()
        {
            var transferStepCount = mainFlowHandler.GetTransferStepsCount();
            if (transferStepCount < 1)
            {
                return;
            }

            var curTransferStep = mainFlowHandler.GetCurTransferStep();
            if (curTransferStep.GetTransferStepType() != EnumTransferStepType.Move)
            {
                return;
            }

            var needReserveSections = middleAgent.GetNeedReserveSections();
            UpdateListBoxSections(lbxNeedReserveSections, needReserveSections);
            foreach (var section in needReserveSections)
            {
                allUcSectionImages[section.Id].DrawSectionImage(allPens["YellowGreen2"]);
            }

            var reserveOkSections = middleAgent.GetReserveOkSections();
            UpdateListBoxSections(lbxReserveOkSections, reserveOkSections);
            foreach (var section in reserveOkSections)
            {
                allUcSectionImages[section.Id].DrawSectionImage(allPens["Green2"]);
            }

        }
        private void UpdateVehLocationAndLoading()
        {
            var location = mainFlowHandler.theVehicle.CurVehiclePosition;

            var realPos = location.RealPosition;
            ucRealPosition.TagValue = $"({(int)realPos.X},{(int)realPos.Y})";

            var barPos = location.BarcodePosition;
            ucBarcodePosition.TagValue = $"({(int)barPos.X},{(int)barPos.Y})";

            var curAddress = location.LastAddress;
            ucMapAddress.TagValue = curAddress.Id;

            var curSection = location.LastSection;
            ucMapSection.TagValue = curSection.Id;

            ucVehicleImage.Hide();
            var loading = mainFlowHandler.theVehicle.ThePlcVehicle.Loading;
            ucLoading.TagValue = loading ? "Yes" : "No";
            ucVehicleImage.Loading = loading;
            ucVehicleImage.Location = MapPixelExchange(realPos);
            ucVehicleImage.FixToCenter();
            ucVehicleImage.Show();
            ucVehicleImage.BringToFront();
        }
        private void UpdatePerformanceCounter(PerformanceCounter performanceCounter, UcLabelTextBox ucLabelTextBox)
        {
            double value = performanceCounter.NextValue();
            ucLabelTextBox.TagValue = string.Format("{0:0.0}%", value);
        }
        private void UpdateListBoxSections(ListBox aListBox, List<MapSection> aListOfSections)
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

        private void btnKeyInPosition_Click(object sender, EventArgs e)
        {
            try
            {
                var curVehPos = Vehicle.Instance.CurVehiclePosition;
                var posX = (int)numPositionX.Value;
                var posY = (int)numPositionY.Value;
                var tempRealPosRangeMm = curVehPos.RealPositionRangeMm;
                curVehPos.RealPositionRangeMm = 0;
                curVehPos.RealPosition = new MapPosition(posX, posY);
                curVehPos.RealPositionRangeMm = tempRealPosRangeMm;

                var barNum = int.Parse(txtBarNum.Text);
                var mapBar = theMapInfo.allMapBarcodes[barNum];
                ucBarPos.TagValue = $"({(int)mapBar.Position.X},{(int)mapBar.Position.Y})";
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                var timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff] ");
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


        #region Thd Visit TransferSteps
        private void btnStartVisitTransferSteps_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StartVisitTransferSteps();
        }

        private void btnPauseVisitTransferSteps_Click(object sender, EventArgs e)
        {
            mainFlowHandler.PauseVisitTransferSteps();
        }

        private void btnResumeVisitTransferSteps_Click(object sender, EventArgs e)
        {
            mainFlowHandler.ResumeVisitTransferSteps();
        }

        private void btnStopVisitTransferSteps_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StopVisitTransferSteps();
        }
        #endregion

        #region Thd Track Position
        private void btnStartTrackPosition_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StartTrackPosition();
        }

        private void btnPauseTrackPosition_Click(object sender, EventArgs e)
        {
            mainFlowHandler.PauseTrackPosition();
        }

        private void btnResumeTrackPostiion_Click(object sender, EventArgs e)
        {
            mainFlowHandler.ResumeTrackPosition();
        }

        private void btnStopTrackPosition_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StopTrackPosition();
        }
        #endregion

        #region Thd Ask Reserve
        private void btnAutoApplyReserveOnce_Click(object sender, EventArgs e)
        {
            middleAgent.OnGetReserveOk();
            RichTextBoxAppendHead(richTextBox1, $"Auto Apply Reserve Once by MainForm");
        }

        private void btnStartAskReserve_Click(object sender, EventArgs e)
        {
            middleAgent.StartAskReserve();
        }

        private void btnPauseAskReserve_Click(object sender, EventArgs e)
        {
            middleAgent.PauseAskReserve();
        }

        private void btnResumeAskReserve_Click(object sender, EventArgs e)
        {
            middleAgent.ResumeAskReserve();
        }

        private void btnStopAskReserve_Click(object sender, EventArgs e)
        {
            middleAgent.StopAskReserve();
        }
        #endregion

        #region Thd Watch LowPower
        private void btnStartWatchLowPower_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StartWatchLowPower();
        }

        private void btnPauseWatchLowPower_Click(object sender, EventArgs e)
        {
            mainFlowHandler.PauseWatchLowPower();
        }

        private void btnResumeWatchLowPower_Click(object sender, EventArgs e)
        {
            mainFlowHandler.ResumeWatchLowPower();
        }

        private void btnStopWatchLowPower_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StopWatchLowPower();
        }
        #endregion

        #region Fake Finished
        private void btnMoveFinish_Click(object sender, EventArgs e)
        {
            if (mainFlowHandler.GetCurrentTransferStepType() == EnumTransferStepType.Move)
            {
                mainFlowHandler.MoveControlHandler_OnMoveFinished(this, EnumMoveComplete.Success);
            }
            else
            {
                RichTextBoxAppendHead(richTextBox1, $"MainForm : btnMoveFinish_Click, [CurStepType={mainFlowHandler.GetCurrentTransferStepType()}");
            }
        }
        private void btnLoadFinish_Click(object sender, EventArgs e)
        {
            if (mainFlowHandler.GetCurrentTransferStepType() == EnumTransferStepType.Load)
            {
                var plcVeh = mainFlowHandler.theVehicle.ThePlcVehicle;
                plcVeh.Loading = true;
                plcVeh.CassetteId = "FakeCst001";

                var plcForkLoadCmd = mainFlowHandler.PlcForkLoadCommand;
                mainFlowHandler.PlcAgent_OnForkCommandFinishEvent(this, plcForkLoadCmd);
                plcVeh.Robot.ExecutingCommand = null;
            }
            else
            {
                RichTextBoxAppendHead(richTextBox1, $"MainForm : MainFlow.GetCurTransCmd().GetCommandType()={mainFlowHandler.GetCurrentTransferStepType()}");
            }
        }
        private void btnUnloadFinish_Click(object sender, EventArgs e)
        {
            if (mainFlowHandler.GetCurrentTransferStepType() == EnumTransferStepType.Unload)
            {
                var plcVeh = mainFlowHandler.theVehicle.ThePlcVehicle;
                plcVeh.Loading = false;
                plcVeh.CassetteId = null;

                var plcForkLoadCmd = mainFlowHandler.PlcForkUnloadCommand;
                mainFlowHandler.PlcAgent_OnForkCommandFinishEvent(this, plcForkLoadCmd);
            }
            else
            {
                RichTextBoxAppendHead(richTextBox1, $"MainForm : MainFlow.GetCurTransCmd().GetCommandType()={mainFlowHandler.GetCurrentTransferStepType()}");
            }
        }
        private void btnTransferComplete_Click(object sender, EventArgs e)
        {
            middleAgent.LoadUnloadComplete();
        }
        #endregion


        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            btnAlarmReset.Enabled = false;
            mainFlowHandler.ResetAllarms();
            btnAlarmReset.Enabled = true;
        }

        private void btnBuzzOff_Click(object sender, EventArgs e)
        {
            plcAgent.WritePLCBuzzserStop();
        }

        private void btnAutoManual_Click(object sender, EventArgs e)
        {
            switch (Vehicle.Instance.AutoState)
            {
                case EnumAutoState.Manual:
                    if (SetManualToAuto())
                    {
                        mainFlowHandler.SetupPlcAutoManualState(EnumIPCStatus.Run);
                        Vehicle.Instance.AutoState = EnumAutoState.Auto;
                        if (Vehicle.Instance.ThePlcVehicle.Loading)
                        {
                            string cstid = "";
                            plcAgent.triggerCassetteIDReader(ref cstid);
                        }
                    }
                    break;
                case EnumAutoState.Auto:
                default:
                    Vehicle.Instance.AutoState = EnumAutoState.PreManual;
                    mainFlowHandler.StopAndClear();
                    mainFlowHandler.StopWatchLowPower();
                    mainFlowHandler.SetupPlcAutoManualState(EnumIPCStatus.Manual);
                    Vehicle.Instance.AutoState = EnumAutoState.Manual;
                    break;
            }
        }

        private bool SetManualToAuto()
        {
            return mainFlowHandler.SetManualToAuto();
        }

        private void btnStopAndClear_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StopAndClear();
        }

        private void btnStopVehicle_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StopVehicle();
        }

        private void btnNeedReserveClear_Click(object sender, EventArgs e)
        {
            middleAgent.DequeueNeedReserveSections();
        }

        private void btnAskReserveClear_Click(object sender, EventArgs e)
        {
            middleAgent.ClearAskingReserveSection();
        }

        private void btnGetReserveOkClear_Click(object sender, EventArgs e)
        {
            middleAgent.DequeueGotReserveOkSections();
        }

        private void btnTestSomething_Click(object sender, EventArgs e)
        {
            mainFlowHandler.SetupPlcAutoManualState(EnumIPCStatus.Run);
            Vehicle.Instance.AutoState = EnumAutoState.Auto;

            //MapSection mapSection = theMapInfo.allMapSections["sec003"];
            //middleAgent.SetupAskingReserveSection(mapSection);

            //middleAgent.Send_Cmd136_AskReserve();

            //middleAgent.Send_Cmd141_ModeChangeResponse();


            //List<MapSection> mapSections = new List<MapSection>();
            //MapSection mapSection;
            //mapSection = theMapInfo.allMapSections["102"];
            //mapSections.Add(mapSection);
            //mapSection = theMapInfo.allMapSections["101"];
            //mapSections.Add(mapSection);
            //mapSection = theMapInfo.allMapSections["92"];
            //mapSections.Add(mapSection);
            //mapSection = theMapInfo.allMapSections["91"];
            //mapSections.Add(mapSection);
            //mapSection = theMapInfo.allMapSections["111"];
            //mapSections.Add(mapSection);

            //mainFlowHandler.SetupTestMoveCmd(mapSections);
            //middleAgent.StopAskReserve();
            //middleAgent.SetupNeedReserveSections(mapSections);
            //middleAgent.StartAskReserve();
            RichTextBoxAppendHead(rtbTransferStep, "line001");
            RichTextBoxAppendHead(rtbTransferStep, "line002");
            RichTextBoxAppendHead(rtbTransferStep, "line003");

            var xx = rtbTransferStep.Text.ToList();

            Console.WriteLine();

        }

        private void btnSetupTestAgvcTransferCmd_Click(object sender, EventArgs e)
        {
            mainFlowHandler.SetupTestAgvcTransferCmd();
        }

        private void btnKeyInSoc_Click(object sender, EventArgs e)
        {
            mainFlowHandler.SetupFakeVehicleSoc(decimal.ToDouble(numSoc.Value));
        }

        private void radOnline_CheckedChanged(object sender, EventArgs e)
        {
            if (radOnline.Checked)
            {
                if (!middleAgent.IsConnected())
                {
                    middleAgent.ReConnect();
                }
            }
        }
        private void radOffline_CheckedChanged(object sender, EventArgs e)
        {
            if (radOffline.Checked)
            {
                if (middleAgent.IsConnected())
                {
                    middleAgent.DisConnect();
                }
            }
        }


        private void btnSemiAutoManual_Click(object sender, EventArgs e)
        {
            switch (Vehicle.Instance.AutoState)
            {
                case EnumAutoState.Manual:
                    if (true/*SetManualToAuto()*/)
                    {
                        mainFlowHandler.SetupPlcAutoManualState(EnumIPCStatus.Run);
                        Vehicle.Instance.AutoState = EnumAutoState.Auto;
                        if (Vehicle.Instance.ThePlcVehicle.Loading)
                        {
                            string cstid = "";
                            plcAgent.triggerCassetteIDReader(ref cstid);
                        }
                    }
                    break;
                case EnumAutoState.Auto:
                default:
                    Vehicle.Instance.AutoState = EnumAutoState.PreManual;
                    mainFlowHandler.StopAndClear();
                    mainFlowHandler.StopWatchLowPower();
                    mainFlowHandler.SetupPlcAutoManualState(EnumIPCStatus.Manual);
                    Vehicle.Instance.AutoState = EnumAutoState.Manual;
                    break;
            }

        }

    }
}
