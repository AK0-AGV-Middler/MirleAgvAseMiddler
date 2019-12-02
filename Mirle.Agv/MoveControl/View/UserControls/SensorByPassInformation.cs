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
using Mirle.Agv.Controller.Tools;

namespace Mirle.Agv
{
    public partial class SensorByPassInformation : UserControl
    {
        public EnumSensorSafetyType SafetyType { get; set; }
        private MoveControlHandler moveControl;
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private string device = "MoveControl";

        public SensorByPassInformation(MoveControlHandler moveControl, EnumSensorSafetyType type)
        {
            this.moveControl = moveControl;
            SafetyType = type;
            InitializeComponent();
        }

        private void WriteLog(string category, string logLevel, string device, string carrierId, string message,
                             [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string classMethodName = GetType().Name + ":" + memberName;
            LogFormat logFormat = new LogFormat(category, logLevel, classMethodName, device, carrierId, message);

            loggerAgent.LogMsg(logFormat.Category, logFormat);
        }

        public void SetLabelString(string safetyName)
        {
            label_Name.Text = safetyName;
        }
        
        public void DisableButton()
        {
            button_Change.Enabled = false;
        }

        public void UpdateEnable()
        {
            try
            {
                button_Change.Text = (moveControl.moveControlConfig.SensorByPass[SafetyType].Enable) ? "開啟中" : "關閉中";
                button_Change.BackColor = (moveControl.moveControlConfig.SensorByPass[SafetyType].Enable) ? Color.Transparent : Color.Red;
            }
            catch { }
        }

        private void button_Change_Click(object sender, EventArgs e)
        {
            button_Change.Enabled = false;

            try
            {
                string logMessage = SafetyType.ToString() + " - Enable/Disable Change : " + (moveControl.moveControlConfig.SensorByPass[SafetyType].Enable ? "Enable" : "Disable") + " to ";
                moveControl.moveControlConfig.SensorByPass[SafetyType].Enable = (button_Change.Text == "關閉中");
                logMessage = logMessage + (moveControl.moveControlConfig.SensorByPass[SafetyType].Enable ? "Enable" : "Disable");
                WriteLog("MoveControl", "7", device, "", logMessage);
                UpdateEnable();
            }
            catch { }

            button_Change.Enabled = true;
        }
    }
}
