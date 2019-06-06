using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Control;
using Mirle.Agv.Model;

namespace Mirle.Agv.View
{
    public partial class MainForm : Form
    {
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);
        private MainFlowHandler mainFlowHandler;
        private CommToAgvcForm commToAgvcForm;
        private MapForm mapForm;
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen blackPen = new Pen(Color.Black, 1);
        private Graphics gra;
        private Panel panelleft;
        private MapInfo mapInfo;

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            mapInfo = MapInfo.Instance;
            commToAgvcForm = new CommToAgvcForm(mainFlowHandler);
            mapForm = new MapForm();
            gra = mapForm.CreateGraphics();

            InitializeComponent();
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
            if (commToAgvcForm.IsDisposed)
            {
                commToAgvcForm = new CommToAgvcForm(mainFlowHandler);
            }
            commToAgvcForm.TopMost = true;
            commToAgvcForm.WindowState = FormWindowState.Normal;
            commToAgvcForm.Show();

        }

        private void btnRefreshMap_Click(object sender, EventArgs e)
        {
            //DrawSomething();
           // mapForm.DrawTheMap();
        }

        private void DrawSomething()
        {
            float coefficient = 0.20f;
            float deltaOrigion = 20;
            // Draw Sections
            //foreach (var section in mapInfo.mapSections)
            //{
            //    float fromX = section.FromAddressX * coefficient + deltaOrigion;
            //    float fromY = section.FromAddressY * coefficient + deltaOrigion;
            //    float toX = section.ToAddressX * coefficient + deltaOrigion;
            //    float toY = section.ToAddressY * coefficient + deltaOrigion;


            //    if (section.Type == EnumSectionType.Horizontal || section.Type == EnumSectionType.Vertical)
            //    {
            //        gra.DrawLine(bluePen, fromX, fromY, toX, toY);
            //    }
            //    else if (section.Type == EnumSectionType.QuadrantIII)
            //    {
            //        //Turn left 
            //        //    t
            //        //    |
            //        // f---

            //        gra.DrawLine(bluePen, fromX, fromY, toX, fromY);
            //        gra.DrawLine(bluePen, toX, fromY, toX, toY);
            //    }
            //    else if (section.Type == EnumSectionType.QuadrantIV)
            //    {
            //        //Turn right 
            //        //    f
            //        //    |
            //        // t---
            //        gra.DrawLine(bluePen, fromX, fromY, fromX, toY);
            //        gra.DrawLine(bluePen, fromX, toY, toX, toY);
            //    }
            //    else
            //    {

            //    }
            //}

            Bitmap bitmap = new Bitmap(@"D:\CsProject\Mirle.Agv\Mirle.Agv\Resource\Auto_16x16.png");
            //Draw Addresses
            foreach (var address in mapInfo.mapAddresses)
            {
                PointF pointf = new PointF(address.PositionX * coefficient + deltaOrigion, address.PositionY * coefficient + deltaOrigion);
                gra.DrawImage(bitmap, pointf);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            panelleft = splitContainer1.Panel1;
            mapForm.TopLevel = false;
            panelleft.Controls.Add(mapForm);
            mapForm.Show();
        }
    }
}
