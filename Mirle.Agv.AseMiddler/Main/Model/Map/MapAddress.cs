using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    [Serializable]
    public class MapAddress
    {
        public string Id { get; set; } = "";
        public MapPosition Position { get; set; } = new MapPosition();
        public EnumAddressDirection TransferPortDirection { get; set; } = EnumAddressDirection.None;
        public string PortNumber { get; set; } = "12";
        public EnumAddressDirection ChargeDirection { get; set; } = EnumAddressDirection.None;
        public EnumAddressDirection PioDirection { get; set; } = EnumAddressDirection.None;
        public bool CanSpin { get; set; }
        public bool IsTR50 { get; set; }
        public string InsideSectionId { get; set; }
        public MapAddressOffset AddressOffset { get; set; } = new MapAddressOffset();
        public double VehicleHeadAngle { get; set; }       

        public EnumAddressDirection AddressDirectionParse(string v)
        {           
            return (EnumAddressDirection)Enum.Parse(typeof(EnumAddressDirection), v);
        }

        public bool IsTransferPort()
        {
            return TransferPortDirection != EnumAddressDirection.None;
        }

        public bool IsPio()
        {
            return PioDirection != EnumAddressDirection.None;
        }

        public bool IsCharger()
        {
            return ChargeDirection != EnumAddressDirection.None;
        }

        public bool IsSegmentPoint()
        {
            return !IsTransferPort() && !IsCharger();
        }
    }

}

