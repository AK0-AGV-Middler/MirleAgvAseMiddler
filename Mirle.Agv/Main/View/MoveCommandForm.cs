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
    public partial class MoveCommandForm : Form
    {
        protected MoveControlPlate moveControlPlate;
        protected MapInfo theMapInfo;

        public MoveCommandForm()
        {
            InitializeComponent();
        }

        public MoveCommandForm(MoveControlPlate moveControlPlate, MapInfo mapInfo) : this()
        {
            this.moveControlPlate = moveControlPlate;
            theMapInfo = mapInfo;
        }
    }

    public class MoveCommandFormFactory
    {
        public MoveCommandForm GetMoveCommandForm(string type, MoveControlPlate moveControlPlate, MapInfo mapInfo)
        {
            MoveCommandForm moveCommandForm = null;

            if (type == "AUO")
            {
                moveCommandForm = new MoveCommandDebugModeForm(moveControlPlate, mapInfo);
            }

            return moveCommandForm;
        }
    }
}
