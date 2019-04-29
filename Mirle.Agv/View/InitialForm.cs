using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Mirle.Agv.View
{
    public partial class InitialForm : Form
    {
        public InitialForm()
        {
            InitializeComponent();
        }

        public delegate void ListBoxAppendCallback(ListBox listBox, string msg);
        private void ListBoxAppend(ListBox listBox,string msg)
        {
            if (listBox.InvokeRequired)
            {
                ListBoxAppendCallback mydel = new ListBoxAppendCallback(ListBoxAppend);
                this.Invoke(mydel, new object[] { listBox, msg });
            }
            else
            {
                listBox.Items.Add(msg);
            }
        }

        private void InitialForm_Shown(object sender, EventArgs e)
        {
            Thread thdInitial = new Thread(new ThreadStart(ForInitial));
            thdInitial.IsBackground = true;
            thdInitial.Start();

        }

        private void ForInitial()
        {
            ListBoxAppend(lst_StartUpMsg, "第一行");
            ListBoxAppend(lst_StartUpMsg, "第二行");
            ListBoxAppend(lst_StartUpMsg, "第三行");
        }
    }
}
