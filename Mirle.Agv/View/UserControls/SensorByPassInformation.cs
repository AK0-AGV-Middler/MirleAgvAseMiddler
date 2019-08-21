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
    public partial class SensorByPassInformation : UserControl
    {
        public EnumSensorSafetyType SafetyType { get; set; }
        private MoveControlHandler moveControl;

        public SensorByPassInformation(MoveControlHandler moveControl, EnumSensorSafetyType type)
        {
            this.moveControl = moveControl;
            SafetyType = type;
            InitializeComponent();
        }
        
        public void SetLabelString(string safetyName)
        {
            label_Name.Text = safetyName;
        }
        
        public void UpdateEnable()
        {
            try
            {
                button_Change.Text = (moveControl.moveControlConfig.SensorByPass[SafetyType].Enable) ? "關閉" : "開啟";
                button_Change.BackColor = (moveControl.moveControlConfig.SensorByPass[SafetyType].Enable) ? Color.Transparent : Color.Red;
            }
            catch { }
        }

        private void button_Change_Click(object sender, EventArgs e)
        {
            button_Change.Enabled = false;

            try
            {
                moveControl.moveControlConfig.SensorByPass[SafetyType].Enable = (button_Change.Text == "開啟");
                UpdateEnable();
            }
            catch { }

            button_Change.Enabled = true;
        }
    }
}
