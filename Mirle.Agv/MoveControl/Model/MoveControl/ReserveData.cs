using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class ReserveData
    {
        public MapPosition Position { get; set; }
        public bool GetReserve { get; set; }
        public EnumAddressAction Action { get; set; }

        public ReserveData(MapPosition position, EnumAddressAction action)
        {
            Position = position;
            GetReserve = false;
            Action = action;
        }
    }
}
