using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class MapInfo
    {
        //Draw on MapForm
        public List<MapBarcodeLine> mapBarcodeLines = new List<MapBarcodeLine>();
        public List<MapAddress> mapAddresses = new List<MapAddress>();
        public List<MapSection> mapSections = new List<MapSection>();

        //Else
        public Dictionary<string, MapAddress> allMapAddresses = new Dictionary<string, MapAddress>();
        public Dictionary<string, MapSection> allMapSections = new Dictionary<string, MapSection>();
        public Dictionary<int, MapBarcode> allBarcodes = new Dictionary<int, MapBarcode>();

        public MapInfo()
        {
        }
    }
}
