using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class PlcBumper
    {
        public string Id { get; } 
        public EnumVehicleSide VehicleSide { get; set; } = EnumVehicleSide.None;
        public Label FormLabel { get; set; } = null;
        public bool Disable { get; set; } = false; // true代表本顆Sensor的signal不會被拿來做判斷Safety Action
        public bool Signal { get; set; } 
        public string PlcSignalTagId { get; }

        public PlcBumper(string id)
        {
            Id = id;
            PlcSignalTagId = "Bumper" + id;
        }
    }
}
