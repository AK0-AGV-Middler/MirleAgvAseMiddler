using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Control
{
    public class BatteryHandler
    {
        private LoggerAgent loggerAgent;

        public BatteryHandler()
        {
            loggerAgent = LoggerAgent.Instance;
        }
    }
}
