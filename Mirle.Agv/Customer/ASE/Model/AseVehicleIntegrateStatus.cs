using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class AseVehicleIntegrateStatus : VehicleIntegrateStatus
    {
        public CarrierSlot SecondCarrierSlot { get; set; } = new CarrierSlot();

        public AseVehicleIntegrateStatus()
        {
            Batterys = new AseBatterys();
        }
    }
}
