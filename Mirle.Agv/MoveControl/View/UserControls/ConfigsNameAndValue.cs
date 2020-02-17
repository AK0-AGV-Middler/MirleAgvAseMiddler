using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.AgvAseMiddler.Model.Configs;

namespace Mirle.AgvAseMiddler
{
    public partial class ConfigsNameAndValue : UserControl
    {
        private string name;
        private string type;
        private object config;

        public ConfigsNameAndValue(string name, string type, object config)
        {
            this.name = name;
            this.type = type;
            this.config = config;
            ShowValue();

            InitializeComponent();
            label_Name.Text = name;
            ShowValue();
        }

        public void ShowValue()
        {
            try
            {
                tB_Value.Text = config.GetType().GetProperty(name).GetValue(config, null).ToString();
            }
            catch { }
        }

        public void SetValue()
        {
            try
            {
                switch (type)
                {
                    case "Int32":
                    case "Int16":
                        config.GetType().GetProperty(name).SetValue(config, (object)int.Parse(tB_Value.Text));
                        break;
                    case "Double":
                        config.GetType().GetProperty(name).SetValue(config, (object)double.Parse(tB_Value.Text));
                        break;
                    case "Boolean":
                        config.GetType().GetProperty(name).SetValue(config, (object)bool.Parse(tB_Value.Text));
                        break;
                    case "String":
                        config.GetType().GetProperty(name).SetValue(config, (object)tB_Value.Text);
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        private void button_Set_Click(object sender, EventArgs e)
        {
            SetValue();
            ShowValue();
        }
    }
}
