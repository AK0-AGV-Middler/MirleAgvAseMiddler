using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.Controller;

namespace Mirle.Agv
{
    public partial class SafetyInformation : UserControl
    {
        public EnumMoveControlSafetyType SafetyType { get; set; }
        private MoveControlHandler moveControl;

        public SafetyInformation(MoveControlHandler moveControl, EnumMoveControlSafetyType type)
        {
            this.moveControl = moveControl;
            SafetyType = type;
            InitializeComponent();
        }

        public void SetLabelString(string safetyName, string rangeName)
        {
            label_Name.Text = safetyName;
            label_Range.Text = rangeName;
        }
        
        public void UpdateEnableRange()
        {
            try
            {
                tB_Range.Text = moveControl.moveControlConfig.Safety[SafetyType].Range.ToString("0.0");
                UpdateEnable();
            }
            catch { }
        }

        public void UpdateEnable()
        {
            try
            {
                button_Change.Text = (moveControl.moveControlConfig.Safety[SafetyType].Enable) ? "開啟中" : "關閉中";
                button_Change.BackColor = (moveControl.moveControlConfig.Safety[SafetyType].Enable) ? Color.Transparent : Color.Red;
            }
            catch { }
        }

        private void button_Change_Click(object sender, EventArgs e)
        {
            button_Change.Enabled = false;

            try
            {
                moveControl.moveControlConfig.Safety[SafetyType].Enable = (button_Change.Text == "關閉中");
                UpdateEnable();
            }
            catch { }

            button_Change.Enabled = true;
        }

        private void button_RangeSet_Click(object sender, EventArgs e)
        {
            button_RangeSet.Enabled = false;

            try
            {
                double value = double.Parse(tB_Range.Text);
                moveControl.moveControlConfig.Safety[SafetyType].Range = value;
            }
            catch { }

            button_RangeSet.Enabled = true;
            UpdateEnableRange();
        }
    }
}
