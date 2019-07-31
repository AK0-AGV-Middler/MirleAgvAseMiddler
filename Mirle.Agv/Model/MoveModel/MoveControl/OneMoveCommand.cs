using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class OneceMoveCommand
    {
        public List<MapPosition> AddressPositions { get; set; }
        public List<EnumAddressAction> AddressActions { get; set; }
        public List<float> SectionSpeedLimits { get; set; }
        public bool DirFlag { get; set; }
        public int WheelAngle { get; set; }

        public OneceMoveCommand(int wheelAngle, bool dirFlag)
        {
            WheelAngle = wheelAngle;
            DirFlag = dirFlag;
            AddressPositions = new List<MapPosition>();
            AddressActions = new List<EnumAddressAction>();
            SectionSpeedLimits = new List<float>();
        }
    }
}
