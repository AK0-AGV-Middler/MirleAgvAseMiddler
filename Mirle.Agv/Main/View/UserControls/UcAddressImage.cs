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
        public int Delta { get; set; } = 0;
        public int Radius { get; set; } = 6;

        private MapInfo theMapInfo = new MapInfo();
        private Image image;
        private Graphics gra;
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen blackPen = new Pen(Color.Black, 1);
        private Pen redPen = new Pen(Color.Red, 1);
        private SolidBrush blackBrush = new SolidBrush(Color.Black);
        private SolidBrush redBrush = new SolidBrush(Color.Red);
        //private double addressRadius = 6;
        private double triangleCoefficient = (double)(Math.Sqrt(3.0));

        private ToolTip toolTip = new ToolTip();

        public UcAddressImage(MapInfo theMapInfo, MapAddress address)
        {
            InitializeComponent();
            this.theMapInfo = theMapInfo;
            Address = address;
            Id = Address.Id;
            label1.Text = Id;
            labelSize = label1.Size.DeepClone();
            DrawAddressImage();
            SetupShowAddressInfo();
        }

        private void SetupShowAddressInfo()
        {
            string msg = $"Id = {Address.Id}\n" + $"Position = ({Address.Position.X},{Address.Position.Y})\n" + $"Coupler = {Address.CouplerId}";

            toolTip.SetToolTip(pictureBox1, msg);
            toolTip.SetToolTip(label1, msg);
        }

        public void DrawAddressImage(Pen pen)
        {
            int recSize = (int)(2 * Radius);
            Size = new Size(Math.Max(label1.Width, recSize) + 2, label1.Height + recSize + 4);
            image = new Bitmap(Size.Width, Size.Height);
            gra = Graphics.FromImage(image);
            if (label1.Width >= recSize)
            {
                Delta = (label1.Width - recSize) / 2;
            }

            if (Address.IsWorkStation)
            {
                RectangleF rectangleF = new RectangleF(Delta + 1, label1.Height + 3, recSize, recSize);
                gra.DrawEllipse(redPen, rectangleF);
            }

            if (Address.IsCharger)
            {
                PointF pointf = new PointF(Delta + 1, label1.Height + 3);
                PointF p1 = new PointF(pointf.X + Radius, pointf.Y);
                PointF p2 = new PointF(pointf.X, (float)(pointf.Y + (Radius * triangleCoefficient)));
                PointF p3 = new PointF(pointf.X + 2 * Radius, (float)(pointf.Y + (Radius * triangleCoefficient)));
                PointF[] pointFs = new PointF[] { p1, p2, p3 };
                gra.FillPolygon(redBrush, pointFs);
            }

            if (Address.IsSegmentPoint)
            {
                Rectangle rectangle = new Rectangle(Delta + 1, label1.Height + 3, recSize, recSize);
                gra.DrawRectangle(blackPen, rectangle);
            }

            pictureBox1.Image = image;
        }

        public void DrawAddressImage()
        {
            DrawAddressImage(redPen);
        }

        public void FixToCenter()
        {
            Location = new Point(Location.X - (Radius + Delta), Location.Y - (label1.Height + 3 + Radius));
        }
    }
}
