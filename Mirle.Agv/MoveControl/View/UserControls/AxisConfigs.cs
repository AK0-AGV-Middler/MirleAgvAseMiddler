using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Mirle.AgvAseMiddler
{
    public partial class AxisConfigs : UserControl
    {
        private string name;
        private object config;
        private List<ConfigsNameAndValue> AxisConfigsList = new List<ConfigsNameAndValue>();
        private int width;
        private int heigh;

        public AxisConfigs(string name, object config)
        {
            this.name = name;
            this.config = config;
            InitializeComponent();
            SetAxisData();
            ShowValue();
        }

        private void SetAxisData()
        {
            try
            {
                int x = 0;
                int y = 0;
                int deltaY = 32;
                Label labelName = new Label();
                labelName.Location = new System.Drawing.Point(x, y);
                labelName.Text = name;
                labelName.Font = new System.Drawing.Font("新細明體", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));

                this.Controls.Add(labelName);
                y += deltaY;

                ConfigsNameAndValue temp;

                object axis = config.GetType().GetProperty(name).GetValue(config, null);

                foreach (PropertyInfo propertyInfo in axis.GetType().GetProperties())
                {
                    if ((double)axis.GetType().GetProperty(propertyInfo.Name).GetValue(axis, null) != 0)
                    {
                        temp = new ConfigsNameAndValue(propertyInfo.Name, "Double", axis);
                        temp.Location = new System.Drawing.Point(x, y);
                        y += deltaY;
                        this.Controls.Add(temp);
                        AxisConfigsList.Add(temp);
                    }

                }

                heigh = y;
                if (AxisConfigsList.Count != 0)
                    width = AxisConfigsList[0].Size.Width;

                this.Size = new Size(width, heigh);
            }
            catch { }
        }

        private void ShowValue()
        {
            foreach (ConfigsNameAndValue temp in AxisConfigsList)
            {
                temp.ShowValue();
            }
        }

        private void SetValue()
        {
            foreach (ConfigsNameAndValue temp in AxisConfigsList)
            {
                temp.SetValue();
            }
        }
    }
}
