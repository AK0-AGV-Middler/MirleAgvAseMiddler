using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class MapAddress
    {
        //Id, Barcode, PositionX, PositionY, IsWorkStation,CanLeftLoad,CanLeftUnload,CanRightLoad,CanRightUnload,IsCharger,CouplerId,ChargeDirection,IsSegmentPoint,CanSpin
        public string Id { get; set; } = "Empty";
        public MapPosition Position { get; set; } = new MapPosition();
        public bool IsWorkStation { get; set; }
        public bool CanLeftLoad { get; set; }
        public bool CanLeftUnload { get; set; }
        public bool CanRightLoad { get; set; }
        public bool CanRightUnload { get; set; }
        public bool IsCharger { get; set; }
        public string CouplerId { get; set; } = "None";
        public EnumChargeDirection ChargeDirection { get; set; } = EnumChargeDirection.None;
        public bool IsSegmentPoint { get; set; }
        public bool CanSpin { get; set; }
        public EnumPioDirection PioDirection { get; set; } = EnumPioDirection.None;
        public bool IsTR50 { get; set; }
        public string InsideSectionId { get; set; }

        public EnumChargeDirection ChargeDirectionParse(string v)
        {
            v = v.Trim();
            return (EnumChargeDirection)Enum.Parse(typeof(EnumChargeDirection), v);
        }

        public EnumPioDirection PioDirectionParse(string v)
        {
            v = v.Trim();
            return (EnumPioDirection)Enum.Parse(typeof(EnumPioDirection), v);
        }
    }

}

