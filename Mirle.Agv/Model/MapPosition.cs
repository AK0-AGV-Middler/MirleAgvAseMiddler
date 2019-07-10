using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapPosition
    {
        public float X { get; set; }
        public float Y { get; set; }

        public MapPosition(float x,float y)
        {
            X = x;
            Y = y;
        }

        public MapPosition() : this(0, 0)
        {

        }
    }
}
