using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.AgvAseMiddler.Properties;

namespace Mirle.AgvAseMiddler
{
    public partial class UcVehicleImage : UserControl
    {
        private bool loading = false;
        public bool Loading
        {
            get
            {
                return loading;
            }
            set
            {
                loading = value;
                if (loading)
                {
                    picVeh.Image = Resources.VehHasCarrier;
                }
                else
                {
                    picVeh.Image = Resources.VehHasNoCarrier;
                }
            }
        }

        public UcVehicleImage()
        {
            InitializeComponent();
        }

        public void FixToCenter()
        {
            var x = Location.X - (Width / 2) > 0 ? Location.X - (Width / 2) : 0;
            var y = Location.Y - Height > 0 ? Location.Y - Height : 0;        

            Location = new Point(x, y);
        }

    }
}
