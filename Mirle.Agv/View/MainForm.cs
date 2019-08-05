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

namespace Mirle.Agv.View
{
    public partial class MainForm : Form
    {
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);
        private MainFlowHandler mainFlowHandler;
        private MoveControlHandler moveControlHandler;
        private MiddlerForm middlerForm;
        private AlarmForm alarmForm;
        private AlarmHandler alarmHandler;
        private PlcForm plcForm;
        private PlcAgent plcAgent;
        private MCProtocol mcProtocol;
        private MoveCommandDebugMode moveCommandDebugMode;
        private JogPitch jogPitch;
        //private MapForm mapForm;
        private Panel panelLeftUp;
        private Panel panelLeftDown;
        private Panel panelRightUp;
        private Panel panelRightDown;
        private MapInfo theMapInfo = new MapInfo();
        PerformanceCounter performanceCounterCpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
        //PerformanceCounter performanceCounterRam = new PerformanceCounter("Memory", "Available MBytes");
        PerformanceCounter performanceCounterRam = new PerformanceCounter("Memory", "% Committed Bytes in Use");
        private LoggerAgent theLoggerAgent = LoggerAgent.Instance;


        #region MouseDownCalculus

        private Point mouseDownPbPoint;
        private Point mouseDownScreenPoint;

        #endregion

        #region PaintingItems
        private Image image;
        private Graphics gra;
        private string saveNameWithTail;
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen blackPen = new Pen(Color.Black, 1);
        private Pen redPen = new Pen(Color.Red, 1);
        private Pen greenPen = new Pen(Color.Green, 1);
        private Pen blackDashPen = new Pen(Color.Black, 1);
        private SolidBrush blackBrush = new SolidBrush(Color.Black);
        private SolidBrush redBrush = new SolidBrush(Color.Red);

        public bool IsBarcodeLineShow { get; set; } = true;
        private Dictionary<MapSection, UcSectionImage> allUcSectionImages = new Dictionary<MapSection, UcSectionImage>();
        private Dictionary<MapAddress, UcAddressImage> allUcAddressImages = new Dictionary<MapAddress, UcAddressImage>();
        private float coefficient = 0.05f;
        private float deltaOrigion = 25;
        private float addressRadius = 3;
        private float triangleCoefficient = (float)(1 / Math.Sqrt(3.0));
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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitialForms();
            InitialPaintingItems();
            InitialPanels();
            InitialEvents();
            InitialVehicleLocation();
            ResetImageAndPb();
        }

        private void InitialForms()
        {
            moveCommandDebugMode = new MoveCommandDebugMode(moveControlHandler, theMapInfo);
            moveCommandDebugMode.WindowState = FormWindowState.Normal;

            middlerForm = new MiddlerForm(mainFlowHandler);
            middlerForm.WindowState = FormWindowState.Normal;

            alarmForm = new AlarmForm(alarmHandler);
            middlerForm.WindowState = FormWindowState.Normal;

            plcForm = new PlcForm(mcProtocol, plcAgent);
            plcForm.WindowState = FormWindowState.Normal;

            jogPitch = new JogPitch(moveControlHandler);
            jogPitch.WindowState = FormWindowState.Normal;

            numPositionX.Maximum = decimal.MaxValue;
            numPositionY.Maximum = decimal.MaxValue;
        }

        private void InitialPaintingItems()
        {
            blackDashPen.DashStyle = DashStyle.DashDot;
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
            mainFlowHandler.OnAgvcTransferCommandCheckedEvent += middlerForm.SendOrReceiveCmdToRichTextBox;
            mainFlowHandler.OnAgvcTransferCommandCheckedEvent += ShowMsgOnMainForm;
        }

        private void ShowMsgOnMainForm(object sender, string e)
        {
            RichTextBoxAppendHead(richTextBox1, e);
        }

        private void InitialVehicleLocation()
        {
            MapSection curSection = theMapInfo.allMapSections["sec001"];
            MapAddress curAddress = curSection.HeadAddress;
            MapPosition curPosition = curAddress.Position.DeepClone();

            ucMapSection.UcName = "Last Section";
            ucMapSection.UcValue = curSection.Id;
            ucMapAddress.UcName = "Last Address";
            ucMapAddress.UcValue = curAddress.Id;

            ucEncoderPosition.UcName = "EncoderPosition";
            ucEncoderPosition.UcValue = $"({curPosition.X},{curPosition.Y})";
            ucBarcodePosition.UcName = "BarcodePosition";
            ucBarcodePosition.UcValue = $"({curPosition.X},{curPosition.Y})";
            ucDeltaPosition.UcName = "DeltaPosition";
            ucDeltaPosition.UcValue = $"(0,0)";
            ucRealPosition.UcName = "RealPosition";
            ucRealPosition.UcValue = $"({curPosition.X},{curPosition.Y})";
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
                    float fromX = rowBarcode.HeadBarcode.Position.X * coefficient + deltaOrigion;
                    float fromY = rowBarcode.HeadBarcode.Position.Y * coefficient + deltaOrigion;
                    float toX = rowBarcode.TailBarcode.Position.X * coefficient + deltaOrigion;
                    float toY = rowBarcode.TailBarcode.Position.Y * coefficient + deltaOrigion;

                    gra.DrawLine(blackDashPen, fromX, fromY, toX, toY);
                }
            }

            allUcSectionImages.Clear();

            // Draw Sections in blueLine
            foreach (var section in theMapInfo.mapSections)
            {
                MapAddress fromAddress = section.HeadAddress;
                MapAddress toAddress = section.TailAddress;

                float fromX = fromAddress.Position.X * coefficient + deltaOrigion;
                float fromY = fromAddress.Position.Y * coefficient + deltaOrigion;
                float toX = toAddress.Position.X * coefficient + deltaOrigion;
                float toY = toAddress.Position.Y * coefficient + deltaOrigion;

                //gra.DrawLine(bluePen, fromX, fromY, toX, toY);

                UcSectionImage ucSectionImage = new UcSectionImage(theMapInfo, section);
                if (!allUcSectionImages.ContainsKey(section))
                {
                    allUcSectionImages.Add(section, ucSectionImage);
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
                if (!allUcAddressImages.ContainsKey(address))
                {
                    allUcAddressImages.Add(address, ucAddressImage);
                }
                pictureBox1.Controls.Add(ucAddressImage);
                Point addPosition = new Point((int)(address.Position.X * coefficient + deltaOrigion), (int)(address.Position.Y * coefficient + deltaOrigion));
                ucAddressImage.Location = new Point(addPosition.X - (ucAddressImage.Radius + ucAddressImage.Delta), addPosition.Y - (ucAddressImage.label1.Height + 3 + ucAddressImage.Radius));
                ucAddressImage.BringToFront();

                ucAddressImage.MouseDown += UcAddressImage_MouseDown;
                ucAddressImage.label1.MouseDown += UcAddressImageItem_MouseDown;
                ucAddressImage.pictureBox1.MouseDown += UcAddressImageItem_MouseDown;


                //if (address.IsWorkStation)
                //{
                //    var bigRadius = 2 * addressRadius;
                //    PointF pointf = new PointF(address.Position.X * coefficient + deltaOrigion - bigRadius, address.Position.Y * coefficient + deltaOrigion - bigRadius);
                //    RectangleF rectangleF = new RectangleF(pointf.X, pointf.Y, 2 * bigRadius, 2 * bigRadius);
                //    gra.DrawEllipse(redPen, rectangleF);
                //}
                //else if (address.IsSegmentPoint)
                //{
                //    PointF pointf = new PointF(address.Position.X * coefficient + deltaOrigion - addressRadius, address.Position.Y * coefficient + deltaOrigion - addressRadius);
                //    RectangleF rectangleF = new RectangleF(pointf.X, pointf.Y, 2 * addressRadius, 2 * addressRadius);
                //    gra.FillRectangle(blackBrush, rectangleF);
                //}

                //if (address.IsCharger)
                //{
                //    var bigRadius = 2 * addressRadius;
                //    PointF pointf = new PointF(address.Position.X * coefficient + deltaOrigion, address.Position.Y * coefficient + deltaOrigion);
                //    PointF p1 = new PointF(pointf.X + bigRadius, pointf.Y + bigRadius * triangleCoefficient);
                //    PointF p2 = new PointF(pointf.X - bigRadius, pointf.Y + bigRadius * triangleCoefficient);
                //    PointF p3 = new PointF(pointf.X, pointf.Y - bigRadius * triangleCoefficient * 2);
                //    PointF[] pointFs = new PointF[] { p1, p2, p3 };
                //    gra.FillPolygon(redBrush, pointFs);
                //}
            }

            pictureBox1.SendToBack();
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
            UpdateStartPbPoint();
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
            UpdateStartPbPoint();
        }

        private void UcSectionImageItem_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            Control parentControl = control.Parent;
            Point point = new Point(e.X + parentControl.Location.X, e.Y + parentControl.Location.Y);
            mouseDownPbPoint = point;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            UpdateStartPbPoint();

        }

        private void UcSectionImage_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            Point point = new Point(e.X + control.Location.X, e.Y + control.Location.Y);
            mouseDownPbPoint = point;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            UpdateStartPbPoint();
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
                middlerForm = new MiddlerForm(mainFlowHandler);
            }
            middlerForm.BringToFront();
            middlerForm.Show();

        }

        private void ManualMoveCmdPage_Click(object sender, EventArgs e)
        {
            if (moveCommandDebugMode.IsDisposed)
            {
                moveCommandDebugMode = new MoveCommandDebugMode(moveControlHandler, theMapInfo);
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
                jogPitch = new JogPitch(moveControlHandler);
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

        private void UpdateStartPbPoint()
        {
            var pX = (mouseDownPbPoint.X - deltaOrigion) / coefficient;
            var pY = (mouseDownPbPoint.Y - deltaOrigion) / coefficient;

            if (pX < 0) pX = 0;
            if (pY < 0) pY = 0;

            string msg = $"Position({pX},{pY})";

            numPositionX.Value = (decimal)pX;
            numPositionY.Value = (decimal)pY;
            //RenewUI(txtCropX, pX.ToString());
            //RenewUI(txtCropY, pY.ToString());

            RenewUI(this, msg);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPbPoint = e.Location;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            UpdateStartPbPoint();
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
            UpdateListBoxSections(listNeedReserveSections, mainFlowHandler.GetNeedReserveSections());
            UpdateListBoxSections(listReserveOkSections, mainFlowHandler.GetReserveOkSections());
            UpdateListBoxSections(listAskingReserveSections, mainFlowHandler.GetAskingReserveSections());
            UpdatePerformanceCounter(performanceCounterCpu, ucPerformanceCounterCpu);
            UpdatePerformanceCounter(performanceCounterRam, ucPerformanceCounterRam);
        }

        private void UpdatePerformanceCounter(PerformanceCounter performanceCounter, UcLabelTextBox ucLabelTextBox)
        {
            float value = performanceCounter.NextValue();
            ucLabelTextBox.UcValue = string.Format("{0:0.0}%", value);
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
            Vehicle.Instance.GetVehLoacation().RealPosition = new MapPosition((float)numPositionX.Value, (float)numPositionY.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MapSection mapSection = theMapInfo.allMapSections["sec002"];
            UcSectionImage sectionImage = allUcSectionImages[mapSection];
            sectionImage.DrawSectionImage(redPen);
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

                int RichTextBoxMaxLines = 20;  // middlerConfig.RichTextBoxMaxLines;

                if (richTextBox.Lines.Count() > RichTextBoxMaxLines)
                {
                    string[] sNewLines = new string[RichTextBoxMaxLines];
                    Array.Copy(richTextBox.Lines, 0, sNewLines, 0, sNewLines.Length);
                    richTextBox.Lines = sNewLines;
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
