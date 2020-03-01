using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    [Serializable]
    public class MapInfo
    {
        public Dictionary<string, MapAddress> addressMap = new Dictionary<string, MapAddress>();
        public Dictionary<string, MapSection> sectionMap = new Dictionary<string, MapSection>();
        public Dictionary<int, MapBarcode> barcodeMap = new Dictionary<int, MapBarcode>();
        public Dictionary<string, MapBarcodeLine> barcodeLineMap = new Dictionary<string, MapBarcodeLine>();
        public List<MapAddress> chargerAddressMap = new List<MapAddress>();
        public Dictionary<string, string> portNumberMap = new Dictionary<string, string>();

        public MapInfo()
        {
        }
    }
}
