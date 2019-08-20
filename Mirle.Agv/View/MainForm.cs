using ClsMCProtocol;
using Mirle.Agv.Controller;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using Mirle.Agv.Properties;
using Mirle.Agv.Model.TransferCmds;

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
        #endregion

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
            theMapInfo = mainFlowHandler.GetMapInfo();
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
        }

        private void InitialForms()
        {
            moveCommandDebugMode = new MoveCommandDebugModeForm(moveControlHandler, theMapInfo);
            moveCommandDebugMode.WindowState = FormWindowState.Normal;

            middlerForm = new MiddlerForm(middleAgent);
            middlerForm.WindowState = FormWindowState.Normal;

            alarmForm = new AlarmForm(alarmHandler);
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

            panelLeftDown = splitContainer3.Panel2;

            panelRightUp = splitContainer2.Panel1;

            panelRightDown = splitContainer2.Panel2;
        }

        private void InitialEvents()
        {
            mainFlowHandler.OnMessageShowEvent += middlerForm.SendOrReceiveCmdToRichTextBox;
            mainFlowHandler.OnMessageShowEvent += ShowMsgOnMainForm;
            middleAgent.OnCmdReceive += ShowMsgOnMainForm;
            middleAgent.OnCmdSend += ShowMsgOnMainForm;
            middleAgent.OnMessageShowEvent += ShowMsgOnMainForm;
            alarmHandler.OnMessageShowEvent += ShowMsgOnMainForm;
        }

        private void ShowMsgOnMainForm(object sender, string e)
        {
            RichTextBoxAppendHead(richTextBox1, e);
        }

        public void DrawBasicMap()
        {
            image = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
            gra = Graphics.FromImage(image);

            //gra.Clear(SystemColors.Control);

            if (IsBarcodeLineShow)
            {
                //Draw Barcode in blackDash
                foreach (var rowBarcode in theMapInfo.mapBarcodeLines)
                {
                    var fromX = rowBarcode.HeadBarcode.Position.X * coefficient + deltaOrigion;
                    var fromY = rowBarcode.HeadBarcode.Position.Y * coefficient + deltaOrigion;
                    var toX = rowBarcode.TailBarcode.Position.X * coefficient + deltaOrigion;
                    var toY = rowBarcode.TailBarcode.Position.Y * coefficient + deltaOrigion;

                    gra.DrawLine(allPens["BlackDashDot1"], (float)fromX, (float)fromY, (float)toX, (float)toY);
                }
            }

            allUcSectionImages.Clear();

            // Draw Sections in blueLine
            foreach (var section in theMapInfo.mapSections)
            {
                MapAddress fromAddress = section.HeadAddress;
                MapAddress toAddress = section.TailAddress;

                var fromX = MapPixelExchange(fromAddress.Position.X);
                var fromY = MapPixelExchange(fromAddress.Position.Y);
                var toX = MapPixelExchange(toAddress.Position.X);
                var toY = MapPixelExchange(toAddress.Position.Y);

                //gra.DrawLine(bluePen, fromX, fromY, toX, toY);

                UcSectionImage ucSectionImage = new UcSectionImage(theMapInfo, section);
                if (!allUcSectionImages.ContainsKey(section.Id))
                {
                    allUcSectionImages.Add(section.Id, ucSectionImage);
                }
                pictureBox1.Controls.Add(ucSectionImage);
                switch (section.Type)
                {
                    case EnumSectionType.Horizontal:
                        ucSectionImage.Location = new Point((int)fromX, (int)fromY - ucSectionImage.labelSize.Height);
                        ucSectionImage.BringToFront();
                        break;
                    case EnumSectionType.Vertical:
                        ucSectionImage.Location = new Point((int)fromX - ucSectionImage.labelSize.Width, (int)fromY);
                        ucSectionImage.SendToBack();
                        break;
                    case EnumSectionType.R2000:
                        ucSectionImage.Location = new Point((int)fromX, (int)fromY);
                        ucSectionImage.BringToFront();
                        break;
                    case EnumSectionType.None:
                    default:
                        break;
                }

                ucSectionImage.MouseDown += UcSectionImage_MouseDown;
                ucSectionImage.label1.MouseDown += UcSectionImageItem_MouseDown;
                ucSectionImage.pictureBox1.MouseDown += UcSectionImageItem_MouseDown;
            }

            //Draw Addresses in BlackRectangle(Segment) RedCircle(Port) RedTriangle(Charger)
            foreach (var address in theMapInfo.mapAddresses)
            {
                UcAddressImage ucAddressImage = new UcAddressImage(theMapInfo, address);
                if (!allUcAddressImages.ContainsKey(address.Id))
                {
                    allUcAddressImages.Add(address.Id, ucAddressImage);
                }
                pictureBox1.Controls.Add(ucAddressImage);
                ucAddressImage.Location = new Point(MapPixelExchange(address.Position.X), MapPixelExchange(address.Position.Y));
                ucAddressImage.FixToCenter();
                ucAddressImage.BringToFront();

                ucAddressImage.MouseDown += UcAddressImage_MouseDown;
                ucAddressImage.label1.MouseDown += UcAddressImageItem_MouseDown;
                ucAddressImage.pictureBox1.MouseDown += UcAddressImageItem_MouseDown;
            }

            pictureBox1.SendToBack();
        }

        public int MapPixelExchange(double num)
        {
            return (int)(num * coefficient + deltaOrigion);
        }

        public double MapPositionExchange(int pixel)
        {
            var posValue = (pixel - deltaOrigion) / coefficient;

            if (posValue > 0)
            {
                return posValue;
            }
            else
            {
                return 0;
            }
        }

        public Point MoveToImageCenter(Size size, Point oldPoint)
        {
            return new Point(oldPoint.X - (Size.Width / 2), oldPoint.Y - (size.Height / 2));
        }

        private void UcAddressImageItem_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = ((Control)sender).Parent;
            UcAddressImage ucAddressImage = (UcAddressImage)control;
            int addX = (int)(ucAddressImage.Address.Position.X * coefficient + deltaOrigion);
            int addY = (int)(ucAddressImage.Address.Position.Y * coefficient + deltaOrigion);
            Point point = new Point(addX, addY);
            mouseDownPbPoint = point;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();
        }

        private void UcAddressImage_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            UcAddressImage ucAddressImage = (UcAddressImage)control;
            int addX = (int)(ucAddressImage.Address.Position.X * coefficient + deltaOrigion);
            int addY = (int)(ucAddressImage.Address.Position.Y * coefficient + deltaOrigion);
            Point point = new Point(addX, addY);
            mouseDownPbPoint = point;
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
            mainFlowHandler.StopVisitTransCmds();
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
                alarmForm = new AlarmForm(alarmHandler);
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
            var pX = MapPositionExchange(mouseDownPbPoint.X);
            var pY = MapPositionExchange(mouseDownPbPoint.Y);

            if (pX < 0) pX = 0;
            if (pY < 0) pY = 0;

            string msg = $"Position({pX},{pY})";

            numPositionX.Value = (decimal)pX;
            numPositionY.Value = (decimal)pY;

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

        private void btnStartTrackingPosition_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StartTrackingPosition();
        }

        private void btnPauseTrackingPosition_Click(object sender, EventArgs e)
        {
            mainFlowHandler.PauseTrackingPosition();
        }

        private void btnResumeTrackingPostiion_Click(object sender, EventArgs e)
        {
            mainFlowHandler.ResumeTrackingPosition();
        }

        private void btnStopTrackingPosition_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StopTrackingPosition();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //UpdatePerformanceCounter(performanceCounterCpu, ucPerformanceCounterCpu);
            //UpdatePerformanceCounter(performanceCounterRam, ucPerformanceCounterRam);
            ucSoc.TagValue = Vehicle.Instance.GetPlcVehicle().Batterys.Percentage.ToString("F2");
            lbxAskReserveSection.Items.Clear();
            lbxAskReserveSection.Items.Add(middleAgent.GetNeedReserveSectionId());
            UpdateListBoxSections(lbxNeedReserveSections, mainFlowHandler.GetNeedReserveSections());
            UpdateListBoxSections(lbxReserveOkSections, mainFlowHandler.GetReserveOkSections());

            UpdateAutoManual();
            UpdateVehLocationAndLoading();
            //DrawReserveSections();
            UpdateThreadPicture();
        }

        private void UpdateThreadPicture()
        {
            picVisitTransferCmd.BackColor = mainFlowHandler.IsVisitTransCmdsAlive() ? Color.Green : Color.Red;
            picTrackingPosition.BackColor = mainFlowHandler.IsTrackingPositionAlive() ? Color.Green : Color.Red;
            picAskReserve.BackColor = middleAgent.IsAskReserveAlive() ? Color.Green : Color.Red;            
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

            btnAutoManual.Text = Vehicle.Instance.AutoState.ToString();
        }

        private void DrawReserveSections()
        {
            var transferStepCount = mainFlowHandler.GetTransferStepCount();
            if (transferStepCount < 1)
            {
                return;
            }

            var curTransferStep = mainFlowHandler.GetCurTransferStep();
            if (curTransferStep.GetEnumTransferCommandType() != EnumTransferCommandType.Move)
            {
                return;
            }

            var needReserveSections = mainFlowHandler.GetNeedReserveSections();           
            UpdateListBoxSections(lbxNeedReserveSections, needReserveSections);
            foreach (var section in needReserveSections)
            {
                allUcSectionImages[section.Id].DrawSectionImage(allPens["YellowGreen2"]);
            }

            var reserveOkSections = mainFlowHandler.GetReserveOkSections();
            UpdateListBoxSections(lbxReserveOkSections, reserveOkSections);
            foreach (var section in reserveOkSections)
            {
                allUcSectionImages[section.Id].DrawSectionImage(allPens["Green2"]);
            }

        }

        private void UpdateVehLocationAndLoading()
        {
            var location = mainFlowHandler.theVehicle.AVehiclePosition;

            var realPos = location.RealPosition;
            ucRealPosition.TagValue = $"({(int)realPos.X},{(int)realPos.Y})";

            var barPos = location.BarcodePosition;
            ucBarcodePosition.TagValue = $"({(int)barPos.X},{(int)barPos.Y})";

            var curAddress = location.LastAddress;
            ucMapAddress.TagValue = curAddress.Id;

            var curSection = location.LastSection;
            ucMapSection.TagValue = curSection.Id;

            ucVehicleImage.Hide();
            var loading = mainFlowHandler.theVehicle.GetPlcVehicle().Loading;
            ucLoading.TagValue = loading ? "Yes" : "No";
            ucVehicleImage.Loading = loading;
            ucVehicleImage.Location = new Point(MapPixelExchange(realPos.X), MapPixelExchange(realPos.Y));
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

        private void btnSetPosition_Click_1(object sender, EventArgs e)
        {
            Vehicle.Instance.AVehiclePosition.RealPosition = new MapPosition((double)numPositionX.Value, (double)numPositionY.Value);
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

                richTextBox.Text = timeStamp + msg + Environment.NewLine + richTextBox.Text;

                int RichTextBoxMaxLines = 10000;  // middlerConfig.RichTextBoxMaxLines;

                if (richTextBox.Lines.Count() > RichTextBoxMaxLines)
                {
                    string[] sNewLines = new string[RichTextBoxMaxLines];
                    Array.Copy(richTextBox.Lines, 0, sNewLines, 0, sNewLines.Length);
                    richTextBox.Lines = sNewLines;
                }
            }
        }


        private void btnMoveFinish_Click(object sender, EventArgs e)
        {
            if (mainFlowHandler.GetCurrentEnumTransferCommandType() == EnumTransferCommandType.Move)
            {
                moveControlHandler.MoveFinished(EnumMoveComplete.Success);
            }
            else
            {
                RichTextBoxAppendHead(richTextBox1, $"MainForm : MainFlow.GetCurTransCmd().GetCommandType()={mainFlowHandler.GetCurrentEnumTransferCommandType()}");
            }
        }

        private void btnCleanAgvcTransCmd_Click(object sender, EventArgs e)
        {
            mainFlowHandler.ClearAgvcTransferCmd();
        }

        private void btnStartVisitTransCmds_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StartVisitTransCmds();
        }

        private void btnPauseVisitTransCmds_Click(object sender, EventArgs e)
        {
            mainFlowHandler.PauseVisitTransCmds();
        }

        private void btnResumeVisitTransCmds_Click(object sender, EventArgs e)
        {
            mainFlowHandler.ResumeVisitTransCmds();
        }

        private void btnStopVisitTransCmds_Click(object sender, EventArgs e)
        {
            mainFlowHandler.StopVisitTransCmds();
        }

        private void btnSetTestTransferCmd_Click(object sender, EventArgs e)
        {
            mainFlowHandler.SetTestTransferCmd();
        }

        private void btnAutoApplyReserve_Click(object sender, EventArgs e)
        {
            middleAgent.AutoApplyReserve = !middleAgent.AutoApplyReserve;
            middleAgent.ResumeAskingReserve();
            RichTextBoxAppendHead(richTextBox1, $"Auto Apply Reserve = {middleAgent.AutoApplyReserve}");
        }

        private void btnStartAskingReserve_Click(object sender, EventArgs e)
        {
            middleAgent.StartAskingReserve();
        }

        private void btnPauseAskingReserve_Click(object sender, EventArgs e)
        {
            middleAgent.PauseAskingReserve();
        }

        private void btnResumeAskingReserve_Click(object sender, EventArgs e)
        {
            middleAgent.ResumeAskingReserve();
        }

        private void btnStopAskingReserve_Click(object sender, EventArgs e)
        {
            middleAgent.StopAskingReserve();
        }

        private void btnLoadFinish_Click(object sender, EventArgs e)
        {
            if (mainFlowHandler.GetCurrentEnumTransferCommandType() == EnumTransferCommandType.Load)
            {
                var plcVeh = mainFlowHandler.theVehicle.GetPlcVehicle();
                plcVeh.Loading = true;
                plcVeh.CassetteId = "FakeCst001";

                var plcForkLoadCmd = mainFlowHandler.PlcForkLoadCommand;
                mainFlowHandler.PlcAgent_OnForkCommandFinishEvent(this, plcForkLoadCmd);
                plcVeh.Robot.ExecutingCommand = null;
            }
            else
            {
                RichTextBoxAppendHead(richTextBox1, $"MainForm : MainFlow.GetCurTransCmd().GetCommandType()={mainFlowHandler.GetCurrentEnumTransferCommandType()}");
            }
        }

        private void btnUnloadFinish_Click(object sender, EventArgs e)
        {
            if (mainFlowHandler.GetCurrentEnumTransferCommandType() == EnumTransferCommandType.Unload)
            {
                var plcVeh = mainFlowHandler.theVehicle.GetPlcVehicle();
                plcVeh.Loading = false;
                plcVeh.CassetteId = null;

                var plcForkLoadCmd = mainFlowHandler.PlcForkUnloadCommand;
                mainFlowHandler.PlcAgent_OnForkCommandFinishEvent(this, plcForkLoadCmd);
            }
            else
            {
                RichTextBoxAppendHead(richTextBox1, $"MainForm : MainFlow.GetCurTransCmd().GetCommandType()={mainFlowHandler.GetCurrentEnumTransferCommandType()}");
            }
        }

        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            alarmHandler.ResetAllAlarms();
            plcAgent.WritePLCAlarmReset();
        }

        private void btnBuzzOff_Click(object sender, EventArgs e)
        {
            plcAgent.WritePLCBuzzserStop();
        }

        private void btnTransferComplete_Click(object sender, EventArgs e)
        {
            middleAgent.LoadUnloadComplete();
        }

        private void btnAutoManual_Click(object sender, EventArgs e)
        {           
            switch (Vehicle.Instance.AutoState)
            {
                case EnumAutoState.Manual:
                    Vehicle.Instance.AutoState = EnumAutoState.Auto;
                    break;
                case EnumAutoState.Auto:                   
                default:
                    Vehicle.Instance.AutoState = EnumAutoState.Manual;
                    break;
            }
        }

        private void btnClearAgvcTransferCmd_Click(object sender, EventArgs e)
        {
            mainFlowHandler.ClearAgvcTransferCmd();
        }

        private void btnTestSomething_Click(object sender, EventArgs e)
        {
            //MapSection mapSection = theMapInfo.allMapSections["sec003"];
            //middleAgent.SetupNeedReserveSection(mapSection);
            //middleAgent.Send_Cmd136_AskReserve();

            //middleAgent.Send_Cmd141_ModeChangeResponse();
            MapSection mapSection;
            mapSection = theMapInfo.allMapSections["sec001"];
            mainFlowHandler.AddNeedReserveSections(mapSection);
            mapSection = theMapInfo.allMapSections["sec002"];
            mainFlowHandler.AddNeedReserveSections(mapSection);
            mapSection = theMapInfo.allMapSections["sec003"];
            mainFlowHandler.AddNeedReserveSections(mapSection);
            mapSection = theMapInfo.allMapSections["sec004"];
            mainFlowHandler.AddNeedReserveSections(mapSection);
            mapSection = theMapInfo.allMapSections["sec005"];
            mainFlowHandler.AddNeedReserveSections(mapSection);

        }
    }
}
