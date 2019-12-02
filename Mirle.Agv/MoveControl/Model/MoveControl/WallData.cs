using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class WallData
    {
        public string Id { get; set; }
        public MapPosition HeadPosition { get; set; }
        public MapPosition TailPosition { get; set; }
        public double ByPassDistance { get; set; }
        public int Angle { get; set; }

        public WallData(string id, MapPosition head, MapPosition tail, double byPassDistance)
        {
            Id = id;
            HeadPosition = head;
            TailPosition = tail;
            ByPassDistance = byPassDistance;
        }
    }
}
