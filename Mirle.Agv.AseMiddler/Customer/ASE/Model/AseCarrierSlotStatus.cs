using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    public class AseCarrierSlotStatus
    {
        public EnumAseCarrierSlotStatus CarrierSlotStatus { get; set; } = EnumAseCarrierSlotStatus.Empty;
        public string CarrierId { get; set; } = "";
        public EnumSlotNumber SlotNumber { get; set; } = EnumSlotNumber.L;

        public AseCarrierSlotStatus() { }

        public AseCarrierSlotStatus(EnumSlotNumber slotNumber)
        {
            this.SlotNumber = slotNumber;
        }
    }
}
