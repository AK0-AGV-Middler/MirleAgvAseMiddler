using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapAddress
    {
        //Id, Barcode, PositionX, PositionY, IsWorkStation,CanLeftLoad,CanLeftUnload,CanRightLoad,CanRightUnload,IsCharger,CouplerId,ChargeDirection,IsSegmentPoint,CanSpin
        public string Id { get; set; } = "Empty";
        public float Barcode { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public bool IsWorkStation { get; set; }
        public bool CanLeftLoad { get; set; }
        public bool CanLeftUnload { get; set; }
        public bool CanRightLoad { get; set; }
        public bool CanRightUnload { get; set; }
        public bool IsCharger { get; set; }
        public string CouplerId { get; set; }
        public EnumChargeDirection ChargeDirection { get; set; } = EnumChargeDirection.None;
        public bool IsSegmentPoint { get; set; }
        public bool CanSpin { get; set; }        

        public EnumChargeDirection ChargeDirectionConvert(string v)
        {
            v = v.Trim();
            return (EnumChargeDirection)Enum.Parse(typeof(EnumChargeDirection), v);
        }
    }

}

