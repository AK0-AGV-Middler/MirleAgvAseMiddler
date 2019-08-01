using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Model;

namespace Mirle.Agv
{
    public partial class UcAddressImage : UserControl
    {
        public MapAddress Address { get; set; } = new MapAddress();
        public string Id { get; set; } = "Empty";
        public Size labelSize { get; set; } = new Size(100, 100);

        private MapInfo theMapInfo = new MapInfo();
        private Image image;
        private Graphics gra;
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen blackPen = new Pen(Color.Black, 1);
        private Pen redPen = new Pen(Color.Red, 1);
        private SolidBrush blackBrush = new SolidBrush(Color.Black);
        private SolidBrush redBrush = new SolidBrush(Color.Red);
        private float coefficient = 0.05f;
        private float deltaOrigion = 25;
        private float addressRadius = 3;
        private float triangleCoefficient = (float)(1 / Math.Sqrt(3.0));

        private ToolTip toolTip = new ToolTip();

        public UcAddressImage(MapInfo theMapInfo, MapAddress address)
        {
            InitializeComponent();
            this.theMapInfo = theMapInfo;
            Address = address;
            Id = Address.Id;
            //label1.Text = Id;
            label1.Text = "";
            labelSize = label1.Size.DeepClone();
            DrawAddressImage();
            SetupShowAddressInfo();
        }

        private void SetupShowAddressInfo()
        {

        }

        public void DrawAddressImage(Pen pen)
        {
            var bigRadius = 2 * addressRadius;
            int recSize = (int)(2 * bigRadius);
            Size = new Size(Math.Max(label1.Width, recSize), label1.Height + recSize);
            image = new Bitmap(Size.Width, Size.Height);
            gra = Graphics.FromImage(image);
            int delta = 0;
            if (label1.Width >= recSize)
            {
                delta = (label1.Width - recSize) / 2;
            }

            if (Address.IsWorkStation)
            {
                RectangleF rectangleF = new RectangleF(delta, label1.Height, recSize, recSize);
                gra.DrawEllipse(redPen, rectangleF);
            }

            if (Address.IsCharger)
            {
                PointF pointf = new PointF(delta, label1.Height);
                PointF p1 = new PointF(pointf.X + bigRadius, pointf.Y + 0);
                PointF p2 = new PointF(pointf.X + 0, pointf.Y + bigRadius * triangleCoefficient * 2);
                PointF p3 = new PointF(pointf.X + 2 * bigRadius, pointf.Y + bigRadius * triangleCoefficient * 2);
                PointF[] pointFs = new PointF[] { p1, p2, p3 };
                gra.FillPolygon(redBrush, pointFs);
            }

            if (Address.IsSegmentPoint)
            {

            }

            pictureBox1.Image = image;
        }

        public void DrawAddressImage()
        {
            DrawAddressImage(redPen);
        }
    }
}
