using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class AseBatterys : Batterys
    {
        public AseBatterys()
        {
            PortAutoChargeLowSoc = 50.0;
            PortAutoChargeHighSoc = 90.0;
            Percentage = 0;  //剩餘電量s分比
        }
    }
}
