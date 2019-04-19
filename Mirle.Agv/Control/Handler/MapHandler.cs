using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;

namespace Mirle.Agv.Control
{
    public class MapHandler
    {
        public Dictionary<string, MapSection> dicSectionsByName;
        public Dictionary<MapAddress, MapSection> dicSectionsByRelatedAddress;
        public Dictionary<string, MapAddress> dicAddressesByName;
        public Dictionary<string, MapPosition> dicPositon;

        public MapHandler()
        {
            dicSectionsByName = new Dictionary<string, MapSection>();
            dicSectionsByRelatedAddress = new Dictionary<MapAddress, MapSection>();
            dicAddressesByName = new Dictionary<string, MapAddress>();
            FillDictionary();
        }

        private void FillDictionary()
        {
            throw new NotImplementedException();
        }
    }
}
