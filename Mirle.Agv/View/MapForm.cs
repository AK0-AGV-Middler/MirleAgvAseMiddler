using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Control;
using Mirle.Agv.Model;


namespace Mirle.Agv.View
{
    public partial class MapForm : Form
    {
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen blackPen = new Pen(Color.Black, 1);
        private Pen redPen = new Pen(Color.Red, 1);
        private Pen blackDashPen = new Pen(Color.Black, 1);
        private SolidBrush blackBrush = new SolidBrush(Color.Black);
        private SolidBrush redBrush = new SolidBrush(Color.Red);
        private Graphics gra;
        private MapInfo mapInfo;
        public bool IsBarcodeLineShow { get; set; } = true;

        public MapForm()
        {
            InitializeComponent();

            mapInfo = MapInfo.Instance;

            blackDashPen.DashStyle = DashStyle.DashDot;
        }

        private void MapForm_Paint(object sender, PaintEventArgs e)
        {
            gra = e.Graphics;
            DrawTheMap();
        }

        public void DrawTheMap()
        {
            float coefficient = 0.50f;
            float deltaOrigion = 50;
            float addressRadius = 3;
            float triangleCoefficient = (float)(1 / Math.Sqrt(3.0));

            if (IsBarcodeLineShow)
            {
                //Draw Barcode in blackDash
                foreach (var rowBarcode in mapInfo.mapBarcodeLines)
                {
                    float fromX = rowBarcode.HeadX * coefficient + deltaOrigion;
                    float fromY = rowBarcode.HeadY * coefficient + deltaOrigion;
                    float toX = rowBarcode.TailX * coefficient + deltaOrigion;
                    float toY = rowBarcode.TailY * coefficient + deltaOrigion;

                    gra.DrawLine(blackDashPen, fromX, fromY, toX, toY);
                }
            }

            // Draw Sections in blueLine
            foreach (var section in mapInfo.mapSections)
            {
                MapAddress fromAddress = mapInfo.dicMapAddresses[section.FromAddress];
                MapAddress toAddress = mapInfo.dicMapAddresses[section.ToAddress];

                float fromX = fromAddress.PositionX * coefficient + deltaOrigion;
                float fromY = fromAddress.PositionY * coefficient + deltaOrigion;
                float toX = toAddress.PositionX * coefficient + deltaOrigion;
                float toY = toAddress.PositionY * coefficient + deltaOrigion;

                gra.DrawLine(bluePen, fromX, fromY, toX, toY);
            }

            //Draw Addresses in BlackRectangle(Segment) RedCircle(Port) RedTriangle(Charger)
            foreach (var address in mapInfo.mapAddresses)
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
    }
}
