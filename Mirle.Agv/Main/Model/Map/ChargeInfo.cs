using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class ChargeInfo
    {
        public MapAddress Address { get; set; } = new MapAddress();
        public bool IsCharger { get; set; } = false;
        public string CouplerId { get; set; } = "";
        public EnumChargeDirection ChargeDirection { get; set; } = EnumChargeDirection.None;
        public EnumPioDirection PioDirection { get; set; } = EnumPioDirection.None;
        public bool IsCharging { get; set; } = false;


    }
}
