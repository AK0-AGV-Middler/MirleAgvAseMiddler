using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapSection
    {
        public string sectionId;
        public string fromAddress;
        public string toAddress;
        public double length;
        public EnumSectionType sectionType;
    }
}
