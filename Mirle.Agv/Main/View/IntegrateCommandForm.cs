using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.AgvAseMiddler.Controller;
using Mirle.AgvAseMiddler.Model;

namespace Mirle.AgvAseMiddler.View
{
    public partial class IntegrateCommandForm : Form
    {
        private IntegrateControlPlate integrateControlPlate;

        public virtual bool IsSimulation { get; set; } = false;

        public IntegrateCommandForm()
        {
            InitializeComponent();
        }

        public IntegrateCommandForm(IntegrateControlPlate integrateControlPlate) : this()
        {
            this.integrateControlPlate = integrateControlPlate;
        }
    }

    public class IntegrateCommandFormFactory
    {
        public IntegrateCommandForm GetIntegrateCommandForm(string type, IntegrateControlPlate integrateControlPlate)
        {
            IntegrateCommandForm integrateCommandForm = null;

            if (type == "AUO")
            {
                integrateCommandForm = new PlcForm((AuoIntegrateControl)integrateControlPlate);
            }

            return integrateCommandForm;
        }
    }
}
