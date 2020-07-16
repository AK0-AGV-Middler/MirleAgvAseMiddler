using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    [Serializable]
    public class MapPosition
    {
        public double X { get; set; }
        public double Y { get; set; }

        public MapPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        public MapPosition() : this(0d, 0d)
        {

        }

        public int MyDistance(MapPosition targetPosition)
        {
            return (int)(Math.Abs(targetPosition.X - X) + Math.Abs(targetPosition.Y - Y));
        }
    }
}
