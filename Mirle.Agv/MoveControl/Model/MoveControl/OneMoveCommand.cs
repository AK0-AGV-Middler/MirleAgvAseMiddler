using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class OneceMoveCommand
    {
        public List<MapPosition> AddressPositions { get; set; }
        public List<EnumAddressAction> AddressActions { get; set; }
        public List<double> SectionSpeedLimits { get; set; }
        public bool DirFlag { get; set; }
        public int WheelAngle { get; set; }

        public OneceMoveCommand(int wheelAngle, bool dirFlag)
        {
            WheelAngle = wheelAngle;
            DirFlag = dirFlag;
            AddressPositions = new List<MapPosition>();
            AddressActions = new List<EnumAddressAction>();
            SectionSpeedLimits = new List<double>();
        }
    }
}
