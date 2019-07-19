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
using System.Drawing.Imaging;

namespace Mirle.Agv
{
    public partial class UcSectionImage : UserControl
    {
        public MapSection Section { get; set; } = new MapSection();
        public string Id { get; set; } = "Empty";
        public Size labelSize { get; set; } = new Size(100, 100);

        private MapInfo theMapInfo = new MapInfo();
        private Image image;
        private Graphics gra;
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen redPen = new Pen(Color.Red, 1);
        private SolidBrush blackBrush = new SolidBrush(Color.Black);
        private float coefficient = 0.05f;

        private ToolTip toolTip = new ToolTip();

        public UcSectionImage() : this(new MapInfo(), new MapSection()) { }
        public UcSectionImage(MapInfo theMapInfo) : this(theMapInfo, new MapSection()) { }
        public UcSectionImage(MapInfo theMapInfo, MapSection aSection)
        {
            InitializeComponent();
            this.theMapInfo = theMapInfo;
            Section = aSection;
            Id = Section.Id;
            label1.Text = Id;
            labelSize = label1.Size.DeepClone();
            DrawSectionImage(bluePen);
            SetupShowSectionInfo();
        }

        private void SetupShowSectionInfo()
        {
            string msg = $"Id = {Section.Id}\n" + $"FromAdr = {Section.HeadAddress.Id}\n" + $"ToAdr = {Section.TailAddress.Id}";

            toolTip.SetToolTip(pictureBox1, msg);
            toolTip.SetToolTip(label1, msg);
        }

        private void DrawSectionImage(Pen aPen)
        {
            MapAddress headAdr = Section.HeadAddress;
            MapAddress tailAdr = Section.TailAddress;

            float disX = Math.Abs(tailAdr.Position.X - headAdr.Position.X) * coefficient;
            float disY = Math.Abs(tailAdr.Position.Y - headAdr.Position.Y) * coefficient;

            switch (Section.Type)
            {
                case EnumSectionType.Horizontal:
                    {
                        int sizeX = (int)disX;
                        int sizeY = label1.Height * 2;
                        this.Size = new Size(sizeX, sizeY);
                        label1.Location = new Point(sizeX / 2, 0);
                        image = new Bitmap(Size.Width, Size.Height);
                        gra = Graphics.FromImage(image);
                        gra.DrawLine(aPen, 0, label1.Height, sizeX, label1.Height);
                    }
                    break;
                case EnumSectionType.Vertical:
                    {
                        int sizeX = label1.Width * 2;
                        int sizeY = (int)disY;
                        this.Size = new Size(sizeX, sizeY);
                        label1.Location = new Point(0, sizeY / 2);
                        image = new Bitmap(Size.Width, Size.Height);
                        gra = Graphics.FromImage(image);
                        gra.DrawLine(aPen, label1.Width, 0, label1.Width, sizeY);
                    }
                    break;
                case EnumSectionType.R2000:
                    {
                        int sizeX = (int)disX;
                        int sizeY = (int)disY;
                        this.Size = new Size(sizeX, sizeY);
                        label1.Location = new Point(sizeX / 2, sizeY / 2);
                        image = new Bitmap(Size.Width, Size.Height);
                        gra = Graphics.FromImage(image);
                        gra.DrawLine(aPen, 0, 0, sizeX, sizeY);
                    }
                    break;
                case EnumSectionType.None:
                default:
                    break;
            }

            pictureBox1.Image = image;

        }

    }
}
