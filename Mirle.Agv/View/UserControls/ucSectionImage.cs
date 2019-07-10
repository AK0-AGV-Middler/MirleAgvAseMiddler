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

        private MapInfo theMapInfo = MapInfo.Instance;
        private Image image;
        private Graphics gra;
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen redPen = new Pen(Color.Red, 1);
        private SolidBrush blackBrush = new SolidBrush(Color.Black);
        private float coefficient = 0.50f;

        private ToolTip toolTip = new ToolTip();


        public UcSectionImage() : this(new MapSection())
        {
        }

        public UcSectionImage(MapSection aSection)
        {
            InitializeComponent();
            Section = aSection;
            Id = Section.Id;
            label1.Text = Id;
            labelSize = label1.Size.DeepClone();
            DrawSectionImage(bluePen);
            //pictureBox1.BackColor = Color.Red;
            SetupShowInfo();
        }

        private void SetupShowInfo()
        {
            string msg = $"Id = {Section.Id}\n" + $"FromAdr = {Section.FromAddress}\n" + $"ToAdr = {Section.ToAddress}";

            toolTip.SetToolTip(pictureBox1, msg);
        }

        private void DrawSectionImage(Pen aPen)
        {
            MapAddress fromAdr = theMapInfo.allMapAddresses[Section.FromAddress];
            MapAddress toAdr = theMapInfo.allMapAddresses[Section.ToAddress];

            float disX = Math.Abs(toAdr.PositionX - fromAdr.PositionX) * coefficient;
            float disY = Math.Abs(toAdr.PositionY - fromAdr.PositionY) * coefficient;

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
