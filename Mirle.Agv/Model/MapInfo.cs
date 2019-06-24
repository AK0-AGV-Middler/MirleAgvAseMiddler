using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapInfo
    {
        //Draw on MapForm
        public List<MapBarcodeLine> mapBarcodeLines = new List<MapBarcodeLine>();
        public List<MapAddress> mapAddresses = new List<MapAddress>();
        public List<MapSection> mapSections = new List<MapSection>();

        //Else
        public Dictionary<string, MapAddress> dicMapAddresses = new Dictionary<string, MapAddress>();
        public Dictionary<string, MapSection> dicMapSections = new Dictionary<string, MapSection>();
        public Dictionary<string, int> dicSectionIndexes = new Dictionary<string, int>();
        public Dictionary<string, int> dicAddressIndexes = new Dictionary<string, int>();
        public Dictionary<string, int> dicBarcodeIndexes = new Dictionary<string, int>();
        public Dictionary<int, MapBarcode> dicBarcodes = new Dictionary<int, MapBarcode>();

        private static readonly MapInfo theMapInfo = new MapInfo();
        public static MapInfo Instance { get { return theMapInfo; } }

        private MapInfo()
        {
        }
    }
}
