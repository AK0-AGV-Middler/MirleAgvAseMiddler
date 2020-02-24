﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
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
        public EnumPermitDirection PermitDirection { get; set; } = EnumPermitDirection.None;
        public EnumPermitDirection CmdDirection { get; set; } = EnumPermitDirection.None;
        public List<MapSectionBeamDisable> BeamSensorDisables { get; set; } = new List<MapSectionBeamDisable>();
        public List<MapAddress> InsideAddresses { get; set; } = new List<MapAddress>();

        public EnumPermitDirection PermitDirectionParse(string v)
        {
            v = v.Trim();
            return (EnumPermitDirection)Enum.Parse(typeof(EnumPermitDirection), v);
        }

        public EnumSectionType SectionTypeParse(string v)
        {
            v = v.Trim();
            return (EnumSectionType)Enum.Parse(typeof(EnumSectionType), v);
        }
    }

}