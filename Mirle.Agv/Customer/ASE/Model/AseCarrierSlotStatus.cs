using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class AseCarrierSlotStatus
    {
        public EnumAseCarrierSlotStatus CarrierSlotStatus { get; set; } = EnumAseCarrierSlotStatus.Empty;
        public string CarrierId { get; set; } = "";
    }
}
