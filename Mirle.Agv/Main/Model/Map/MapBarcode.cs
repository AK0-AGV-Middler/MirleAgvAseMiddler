using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class MapBarcode
    {
        public int Number { get; set; }
        public MapPosition Position { get; set; } = new MapPosition();
        public MapPosition Offset { get; set; } = new MapPosition();
        public string LineId { get; set; }
    }
}
