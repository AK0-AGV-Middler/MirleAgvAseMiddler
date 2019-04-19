using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapAddress
    {
        private string id;
        private EnumAddressType type;
        private List<MapSection> relatedSections;
    }
}
