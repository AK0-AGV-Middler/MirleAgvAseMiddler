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
        public EnumPermitDirection CmdDirection { get; set; } = EnumPermitDirection.None;

        public EnumPermitDirection PermitDirectionConvert(string v)
        {
            v = v.Trim();
            return (EnumPermitDirection)Enum.Parse(typeof(EnumPermitDirection), v);
        }

        public EnumSectionType SectionTypeConvert(string v)
        {
            v = v.Trim();
            return (EnumSectionType)Enum.Parse(typeof(EnumSectionType), v);
        }
    }

}
