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
using System.Drawing.Imaging;

namespace Mirle.Agv.AseMiddler
{
    public partial class UcSectionImage : UserControl
    {
        public MapSection Section { get; set; } = new MapSection();
        public MapVector VectorHeadToTail { get; set; } = new MapVector();
        public string Id { get; set; } = "";
        public Size labelSize { get; set; } = new Size(100, 100);

        private MapInfo theMapInfo = new MapInfo();
        private Image image;
        private Graphics gra;
        private Pen bluePen = new Pen(Color.Blue, 2);
        private double coefficient = 0.05f;

        private ToolTip toolTip = new ToolTip();

        private int x1 = 0;
        private int y1 = 0;
        private int x2 = 0;
        private int y2 = 0;

        public UcSectionImage() : this(new MapInfo(), new MapSection()) { }
        public UcSectionImage(MapInfo theMapInfo) : this(theMapInfo, new MapSection()) { }
        public UcSectionImage(MapInfo theMapInfo, MapSection section)
        {
            InitializeComponent();
            this.theMapInfo = theMapInfo;
            Section = section;
            VectorHeadToTail = new MapVector(Section.TailAddress.Position.X - Section.HeadAddress.Position.X, Section.TailAddress.Position.Y - Section.HeadAddress.Position.Y);
            Id = Section.Id;
            label1.Text = Id;
            labelSize = label1.Size;
            DrawSectionImage(bluePen);
            SetupShowSectionInfo();
        }

        private void SetupShowSectionInfo()
        {
            string msg = $"Id = {Section.Id}\n" + $"FromAdr = {Section.HeadAddress.Id}\n" + $"ToAdr = {Section.TailAddress.Id}";

            toolTip.SetToolTip(pictureBox1, msg);
            toolTip.SetToolTip(label1, msg);
        }

        public void SetColor(Pen pen)
        {
            gra.Clear(Color.Transparent);
            gra.DrawLine(pen, x1, y1, x2, y2);
        }

        public void DrawSectionImage(Pen aPen)
        {
            MapAddress headAdr = Section.HeadAddress;
            MapAddress tailAdr = Section.TailAddress;

            var disX = Convert.ToInt32(Math.Abs(tailAdr.Position.X - headAdr.Position.X) * coefficient);
            var disY = Convert.ToInt32(Math.Abs(tailAdr.Position.Y - headAdr.Position.Y) * coefficient);

            switch (Section.Type)
            {
                case EnumSectionType.Horizontal:
                    {
                        Size = new Size(disX, label1.Height * 3);
                        label1.Location = new Point(disX / 2, label1.Height * 2);
                        image = new Bitmap(Size.Width, Size.Height);
                        gra = Graphics.FromImage(image);

                        x1 = 0;
                        y1 = 0;
                        x2 = disX;
                        y2 = 0;
                    }
                    break;
                case EnumSectionType.Vertical:
                    {
                        Size = new Size(label1.Width + 10, disY);
                        label1.Location = new Point(5, disY / 2);
                        image = new Bitmap(Size.Width, Size.Height);
                        gra = Graphics.FromImage(image);

                        x1 = label1.Width / 2 + 5;
                        y1 = 0;
                        x2 = label1.Width / 2 + 5;
                        y2 = disY;
                    }
                    break;
                case EnumSectionType.R2000:
                    {
                        Size = new Size(disX, disY);
                        label1.Location = new Point(disX / 2, disY / 2);
                        image = new Bitmap(Size.Width, Size.Height);
                        gra = Graphics.FromImage(image);
                        if (VectorHeadToTail.DirX * VectorHeadToTail.DirY > 0)
                        {
                            //左上右下型
                            x1 = 0;
                            y1 = 0;
                            x2 = disX;
                            y2 = disY;
                        }
                        else
                        {
                            //左下右上型
                            x1 = 0;
                            y1 = disY;
                            x2 = disX;
                            y2 = 0;
                        }
                    }
                    break;
                case EnumSectionType.None:
                default:
                    break;
            }

            gra.DrawLine(aPen, x1, y1, x2, y2);
            pictureBox1.Image = image;

        }
    }
}
