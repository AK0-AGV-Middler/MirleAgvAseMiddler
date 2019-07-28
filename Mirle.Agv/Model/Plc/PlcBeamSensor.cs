using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.Model
{
    public class PlcBeamSensor
    {
        public string Id { get; }
        public EnumVehicleSide VehicleSide { get; set; } = EnumVehicleSide.None;
        public Label FormLabel { get; set; } = null;
        public bool Disable { get; set; }  // true代表本顆Sensor的signal不會被拿來做判斷Safety Action
        public bool FarSignal { get; set; }
        public bool NearSignal { get; set; }
        public bool ReadSleepSignal { get; set; }
        public bool WriteSleepSignal { get; set; }
        public bool BeforeWriteSleep { get; set; }
        public string PlcFarSignalTagId { get; }
        public string PlcNearSignalTagId { get; }
        public string PlcReadSleepTagId { get; }
        public string PlcWriteSleepTagId { get; }

        public PlcBeamSensor(string id)
        {
            Id = id;
            PlcNearSignalTagId = "BeamSensor" + id + "Near";
            PlcFarSignalTagId = "BeamSensor" + id + "Far";
            PlcReadSleepTagId = "RBeamSensor" + id + "_Sleep";
            PlcWriteSleepTagId = "WBeamSensor" + id + "_Sleep";
        }
    }
}