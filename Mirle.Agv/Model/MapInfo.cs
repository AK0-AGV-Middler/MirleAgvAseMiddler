using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapInfo
    {
        public List<MapSection> mapSections = new List<MapSection>();
        public List<MapAddress> mapAddresses = new List<MapAddress>();        
        public List<MapBarcode> mapBarcodes = new List<MapBarcode>();
        public Dictionary<string, MapAddress> dicMapAddresses = new Dictionary<string, MapAddress>();
        public Dictionary<string, int> dicSectionIndexes = new Dictionary<string, int>();
        public Dictionary<string, int> dicAddressIndexes = new Dictionary<string, int>();
        public Dictionary<string, int> dicBarcodeIndexes = new Dictionary<string, int>();

        private static readonly MapInfo theMapInfo = new MapInfo();
        public static MapInfo Instance { get { return theMapInfo; } }

        private MapInfo()
        {
        }
    }
}
