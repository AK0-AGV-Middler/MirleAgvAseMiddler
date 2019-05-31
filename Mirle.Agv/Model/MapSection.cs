using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapSection
    {
        //Id, FromAddress, ToAddress, Distance, Speed, Type, PermitDirection, FowardBeamSensorEnable, BackwardBeamSensorEnable
        public string Id { get; set; } = "Empty";
        public string FromAddress { get; set; } = "Empty";
        public string ToAddress { get; set; } = "Empty";
        public float Distance { get; set; }
        public float Speed { get; set; }
        public EnumSectionType Type { get; set; } = EnumSectionType.None;
        public EnumPermitDirection PermitDirection { get; set; } = EnumPermitDirection.None;
        public bool FowardBeamSensorEnable { get; set; }
        public bool BackwardBeamSensorEnable { get; set; }

        public EnumPermitDirection PermitDirectionConvert(string v)
        {
            var keyword = v.Trim();
            switch (keyword)
            {
                case "Forward":
                    return EnumPermitDirection.Forward;
                case "Backward":
                    return EnumPermitDirection.Backward;
                case "None":
                default:
                    return EnumPermitDirection.None;
            }
        }

        public EnumSectionType SectionTypeConvert(string v)
        {
            var keyword = v.Trim();
            switch (keyword)
            {
                case "Horizontal":
                    return EnumSectionType.Horizontal;
                case "Vertical":
                    return EnumSectionType.Vertical;
                case "QuadrantI":
                    return EnumSectionType.QuadrantI;
                case "QuadrantII":
                    return EnumSectionType.QuadrantII;
                case "QuadrantIII":
                    return EnumSectionType.QuadrantIII;
                case "QuadrantIV":
                    return EnumSectionType.QuadrantIV;
                case "None":
                default:
                    return EnumSectionType.None;
            }
        }
    }

}
