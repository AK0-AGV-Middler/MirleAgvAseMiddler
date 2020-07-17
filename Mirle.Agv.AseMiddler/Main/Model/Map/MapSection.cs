using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    [Serializable]
    public class MapSection
    {
        //Id, FromAddress, ToAddress, Distance, Speed, Type, PermitDirection, FowardBeamSensorEnable, BackwardBeamSensorEnable
        public string Id { get; set; } = "";
        public MapAddress HeadAddress { get; set; } = new MapAddress();
        public MapAddress TailAddress { get; set; } = new MapAddress();
        public double HeadToTailDistance { get; set; }
        public double VehicleDistanceSinceHead { get; set; }
        public double Speed { get; set; }
        public EnumSectionType Type { get; set; } = EnumSectionType.None;
        public EnumCommandDirection CmdDirection { get; set; } = EnumCommandDirection.None;
        public List<MapSectionBeamDisable> BeamSensorDisables { get; set; } = new List<MapSectionBeamDisable>();
        public List<MapAddress> InsideAddresses { get; set; } = new List<MapAddress>();

        public EnumCommandDirection PermitDirectionParse(string v)
        {
            v = v.Trim();
            return (EnumCommandDirection)Enum.Parse(typeof(EnumCommandDirection), v);
        }

        public EnumSectionType SectionTypeParse(string v)
        {
            v = v.Trim();
            return (EnumSectionType)Enum.Parse(typeof(EnumSectionType), v);
        }

        public bool InSection(string addressId)
        {
            if (InsideAddresses.Count == 0) return false;

            bool result = false;
            foreach (MapAddress mapAddress in InsideAddresses)
            {
                if (mapAddress.Id == addressId)
                {
                    return true;
                }
            }
            return result;

            //var query = InsideAddresses.Where(x => x.Id == addressId);
            //return query.Any();

            //return InsideAddresses.FindIndex(x => x.Id == lastAddress.Id) > 0;
        }

        public bool InSection(MapAddress mapAddress)
        {
            return InSection(mapAddress.Id);
        }
    }

}
