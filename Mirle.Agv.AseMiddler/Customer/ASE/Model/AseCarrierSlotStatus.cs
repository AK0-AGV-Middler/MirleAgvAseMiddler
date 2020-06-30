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
        public bool ManualDeleteCST { get; set; } = false;

        public AseCarrierSlotStatus() { }

        public AseCarrierSlotStatus(EnumSlotNumber slotNumber)
        {
            this.SlotNumber = slotNumber;
        }

        public AseCarrierSlotStatus(AseCarrierSlotStatus aseCarrierSlotStatus)
        {
            this.CarrierSlotStatus = aseCarrierSlotStatus.CarrierSlotStatus;
            this.CarrierId = aseCarrierSlotStatus.CarrierId;
            this.SlotNumber = aseCarrierSlotStatus.SlotNumber;
            this.ManualDeleteCST = aseCarrierSlotStatus.ManualDeleteCST;
        }
    }
}
