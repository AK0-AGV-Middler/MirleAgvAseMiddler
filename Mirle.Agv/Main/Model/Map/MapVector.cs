using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class MapVector
    {
        public double DirX { get; set; }
        public double DirY { get; set; }

        public MapVector(double dirX, double dirY)
        {
            DirX = dirX;
            DirY = dirY;
        }

        public MapVector() : this(0d, 0d)
        {

        }
    }
}
