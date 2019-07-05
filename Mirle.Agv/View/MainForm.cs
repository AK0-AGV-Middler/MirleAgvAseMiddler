using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Controller;
using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Mirle.Agv.View
{
    public partial class MainForm : Form
    {
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);
        private MainFlowHandler mainFlowHandler;
        private ManualMoveCmdForm manualMoveCmdForm;
        private MiddlerForm middlerForm;
        //private MapForm mapForm;
        private Panel panelLeftUp;
        private Panel panelLeftDown;
        private Panel panelRightUp;
        private Panel panelRightDown;
        private MapInfo theMapInfo = MapInfo.Instance;

        #region MouseDownCalculus

        private Point mouseDownPbPoint;
        private Point mouseDownScreenPoint;
        private bool isMouseDown;

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
        private Dictionary<MapSection, Image> mapSectionsAsImages = new Dictionary<MapSection, Image>();
        private float coefficient = 0.50f;
        private float deltaOrigion = 50;
        private float addressRadius = 3;
        private float triangleCoefficient = (float)(1 / Math.Sqrt(3.0));

        #endregion

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitialForms();
            InitialPaintingItems();
            InitialPanels();
            InitialEvents();
            ResetImageAndPb();
            InitialVehicleLocationInfo();

            //MakeATestPanel();
        }


        private void MakeATestPanel()
        {
            Panel aPanel = new Panel();
            aPanel.BackColor = Color.Red;
            aPanel.Location = new Point(650, 420);
            aPanel.Name = "testpanel";
            aPanel.Size = new Size(257, 5);
            aPanel.Visible = true;
            // splitContainer3.Panel1.Controls.Add(aPanel);

            pictureBox1.Controls.Add(aPanel);


            //splitContainer1.Panel1.SendToBack();
            pictureBox1.SendToBack();
            aPanel.BringToFront();

            aPanel.MouseDown += APanel_MouseDown;
        }

        private void APanel_MouseDown(object sender, MouseEventArgs e)
        {
            Panel thePanel = (Panel)sender;

            RenewUI(txtCropY, thePanel.Name);
        }

        private void InitialVehicleLocationInfo()
        {
            MapSection mapSection = theMapInfo.dicMapSections["sec001"];
            MapAddress mapAddress = theMapInfo.dicMapAddresses[mapSection.FromAddress];
            MapPosition mapPosition = mapAddress.GetPosition();
            string txtPosition = $"({mapPosition.PositionX},{mapPosition.PositionY})";

            ucEncoderPosition.UcLabel = "EncoderPosition";
            ucEncoderPosition.UcTextBox = txtPosition;

            uctBarcodePosition.UcLabel = "BarcodePosition";
            uctBarcodePosition.UcTextBox = txtPosition;

            ucDeltaPosition.UcLabel = "DeltaPosition";
            ucDeltaPosition.UcTextBox = txtPosition;

            ucRealPosition.UcLabel = "RealPosition";
            ucRealPosition.UcTextBox = txtPosition;

            ucMapSection.UcLabel = "LastSection";
            ucMapSection.UcTextBox = mapSection.Id;

            ucMapAddress.UcLabel = "LastAddress";
            ucMapAddress.UcTextBox = mapAddress.Id;

            RenewVehicleLocation();
        }

        private void RenewVehicleLocation()
        {
            foreach (Control item in gbVehicleLocation.Controls)
            {
                if (item is UcTextWithLabel)
                {
                    UcTextWithLabel uc = (UcTextWithLabel)item;
                    uc.RenewUI();
                }
            }
        }

        private void InitialForms()
        {
            manualMoveCmdForm = new ManualMoveCmdForm(mainFlowHandler);
            manualMoveCmdForm.TopMost = true;
            manualMoveCmdForm.WindowState = FormWindowState.Normal;

            middlerForm = new MiddlerForm(mainFlowHandler);
            middlerForm.TopMost = true;
            middlerForm.WindowState = FormWindowState.Normal;

        }

        private void InitialPaintingItems()
        {
            DrawBasicMap();

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
            mainFlowHandler.OnAgvcTransferCommandCheckedEvent += MainFlowHandler_OnAgvcTransferCommandCheckedEvent;
        }

        private void MainFlowHandler_OnAgvcTransferCommandCheckedEvent(object sender, string msg)
        {
            //RichTextBoxAppendHead(richTextBox1, msg);
        }

        public void DrawBasicMap()
        {
            image = new Bitmap(Convert.ToInt32(1920), Convert.ToInt32(1080), PixelFormat.Format32bppArgb);
            gra = Graphics.FromImage(image);

            //gra.Clear(SystemColors.Control);

            if (IsBarcodeLineShow)
            {
                //Draw Barcode in blackDash
                foreach (var rowBarcode in theMapInfo.mapBarcodeLines)
                {
                    float fromX = rowBarcode.HeadX * coefficient + deltaOrigion;
                    float fromY = rowBarcode.HeadY * coefficient + deltaOrigion;
                    float toX = rowBarcode.TailX * coefficient + deltaOrigion;
                    float toY = rowBarcode.TailY * coefficient + deltaOrigion;

                    gra.DrawLine(blackDashPen, fromX, fromY, toX, toY);
                }
            }

            // Draw Sections in blueLine
            foreach (var section in theMapInfo.mapSections)
            {
                MapAddress fromAddress = theMapInfo.dicMapAddresses[section.FromAddress];
                MapAddress toAddress = theMapInfo.dicMapAddresses[section.ToAddress];

                float fromX = fromAddress.PositionX * coefficient + deltaOrigion;
                float fromY = fromAddress.PositionY * coefficient + deltaOrigion;
                float toX = toAddress.PositionX * coefficient + deltaOrigion;
                float toY = toAddress.PositionY * coefficient + deltaOrigion;

                gra.DrawLine(bluePen, fromX, fromY, toX, toY);

            }

            //Draw Addresses in BlackRectangle(Segment) RedCircle(Port) RedTriangle(Charger)
            foreach (var address in theMapInfo.mapAddresses)
            {
                if (address.IsWorkStation)
                {
                    var bigRadius = 2 * addressRadius;
                    PointF pointf = new PointF(address.PositionX * coefficient + deltaOrigion - bigRadius, address.PositionY * coefficient + deltaOrigion - bigRadius);
                    RectangleF rectangleF = new RectangleF(pointf.X, pointf.Y, 2 * bigRadius, 2 * bigRadius);
                    gra.DrawEllipse(redPen, rectangleF);
                }
                else if (address.IsSegmentPoint)
                {
                    PointF pointf = new PointF(address.PositionX * coefficient + deltaOrigion - addressRadius, address.PositionY * coefficient + deltaOrigion - addressRadius);
                    RectangleF rectangleF = new RectangleF(pointf.X, pointf.Y, 2 * addressRadius, 2 * addressRadius);
                    gra.FillRectangle(blackBrush, rectangleF);
                }

                if (address.IsCharger)
                {
                    var bigRadius = 2 * addressRadius;
                    PointF pointf = new PointF(address.PositionX * coefficient + deltaOrigion, address.PositionY * coefficient + deltaOrigion);
                    PointF p1 = new PointF(pointf.X + bigRadius, pointf.Y + bigRadius * triangleCoefficient);
                    PointF p2 = new PointF(pointf.X - bigRadius, pointf.Y + bigRadius * triangleCoefficient);
                    PointF p3 = new PointF(pointf.X, pointf.Y - bigRadius * triangleCoefficient * 2);
                    PointF[] pointFs = new PointF[] { p1, p2, p3 };
                    gra.FillPolygon(redBrush, pointFs);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mainFlowHandler.Stop();
            ShutdownEvent.Set();
            PauseEvent.Set();

            Application.Exit();
            Environment.Exit(Environment.ExitCode);

        }

        private void 通訊ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (middlerForm.IsDisposed)
            {
                middlerForm = new MiddlerForm(mainFlowHandler);
            }
            middlerForm.Show();

        }

        private void 手動測試動令ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (manualMoveCmdForm.IsDisposed)
            {
                manualMoveCmdForm = new ManualMoveCmdForm(mainFlowHandler);
            }
            manualMoveCmdForm.Show();
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

        private void btnFakeCmdTest_Click(object sender, EventArgs e)
        {
            mainFlowHandler.FakeCmdTest();
        }

        private void btnSwitchBarcodeLine_Click(object sender, EventArgs e)
        {
            IsBarcodeLineShow = !IsBarcodeLineShow;
            ResetImageAndPb();
        }

        #region Image Functions

        private void UpdateStartPbPoint()
        {
            RenewUI(txtCropX, mouseDownPbPoint.X.ToString());
            RenewUI(txtCropY, mouseDownPbPoint.Y.ToString());
            RenewUI(this, mouseDownPbPoint.ToString());
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPbPoint = e.Location;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            UpdateStartPbPoint();
            isMouseDown = true;
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
    }
}
