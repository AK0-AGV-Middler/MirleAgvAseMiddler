using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapInfo
    {
        public List<MapSection> mapSections;
        public List<MapAddress> mapAddresses;
        public Dictionary<string, MapAddress> dicMapAddresses;

        private static readonly MapInfo theMapInfo = new MapInfo();
        public static MapInfo Instance { get { return theMapInfo; } }

        private MapInfo()
        {
            mapSections = new List<MapSection>();
            mapAddresses = new List<MapAddress>();
            dicMapAddresses = new Dictionary<string, MapAddress>();
        }
    }
}
