using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapPosition
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }

        public MapPosition(float x,float y)
        {
            PositionX = x;
            PositionY = y;
        }

        public MapPosition() : this(0, 0)
        {

        }
    }
}
