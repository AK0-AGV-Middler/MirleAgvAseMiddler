using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.AseMiddler.Model;

namespace Mirle.Agv.AseMiddler
{
    public partial class UcAddressImage : UserControl
    {
        public MapAddress Address { get; set; } = new MapAddress();
        public string Id { get; set; } = "";
        public Size labelSize { get; set; } = new Size(100, 100);
        public int Delta { get; set; } = 0;
        public int Radius { get; set; } = 6;

        private MapInfo theMapInfo = new MapInfo();
        private Image image;
        private Graphics gra;
        private Pen blackPen = new Pen(Color.Black, 1);
        private Pen redPen = new Pen(Color.Red, 1);
        private SolidBrush redBrush = new SolidBrush(Color.Red);
        private SolidBrush blackBrush = new SolidBrush(Color.Black);
        private double triangleCoefficient = (double)(Math.Sqrt(3.0));

        private ToolTip toolTip = new ToolTip();

        public UcAddressImage(MapInfo theMapInfo, MapAddress address)
        {
            InitializeComponent();
            this.theMapInfo = theMapInfo;
            Address = address;
            Id = Address.Id;
            DrawAddressImage(redPen);
            SetupShowAddressInfo();
        }

        private void SetupShowAddressInfo()
        {
            string msg = $"Id = {Address.Id}\n" + $"Position = ({Address.Position.X},{Address.Position.Y})\n" + $"Coupler = {Address.CouplerId}";

            toolTip.SetToolTip(pictureBox1, msg);
        }

        public void DrawAddressImage(Pen pen)
        {
            int recSize = 2 * Radius;
            Size = new Size(recSize + 2, recSize + 2);
            image = new Bitmap(Size.Width, Size.Height);
            gra = Graphics.FromImage(image);

            if (Address.IsWorkStation)
            {
                //Port站 : 畫圓
                //RectangleF rectangleF = new RectangleF(Delta + 1, label1.Height + 3, recSize, recSize);
                RectangleF rectangleF = new RectangleF(1, 1, recSize, recSize);
                gra.DrawEllipse(redPen, rectangleF);
            }

            if (Address.IsCharger)
            {
                //充電樁 : 畫圓內接三角形
                var triangleHeight = (float)((Radius * triangleCoefficient));
                PointF pointf = new PointF(1, 1);
                PointF p1 = new PointF(pointf.X + Radius, pointf.Y);
                PointF p2 = new PointF(pointf.X + 0, pointf.Y + triangleHeight);
                PointF p3 = new PointF(pointf.X + recSize, pointf.Y + triangleHeight);
                PointF[] pointFs = new PointF[] { p1, p2, p3 };
                gra.FillPolygon(redBrush, pointFs);
            }

            if (Address.IsSegmentPoint)
            {
                //Rectangle rectangle = new Rectangle(Delta + 1, label1.Height + 3, recSize, recSize);
                Rectangle rectangle = new Rectangle(1, 1, recSize, recSize);
                gra.DrawRectangle(blackPen, rectangle);
            }

            if (!Address.IsWorkStation && !Address.IsSegmentPoint && !Address.IsCharger)
            {
                //Rectangle rectangle = new Rectangle(Delta + 1, label1.Height + 3, recSize, recSize);
                Rectangle rectangle = new Rectangle(1, 1, recSize, recSize);
                gra.FillEllipse(blackBrush, rectangle);
            }

            pictureBox1.Image = image;
        }

        public void FixToCenter()
        {
            Location = new Point(Location.X - Radius, Location.Y - Radius);
        }
    }
}
