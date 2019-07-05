using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.View
{
    public partial class UcTextWithLabel : UserControl
    {
        public string UcLabel { get; set; } = "MyLabel";
        public string UcTextBox { get; set; } = "MyTextBox";

        public UcTextWithLabel()
        {
            InitializeComponent();
            RenewUI();
        }

        public void RenewUI()
        {
            ucLabel.Text = UcLabel;
            ucTextBox.Text = UcTextBox;
        }
    }
}
