using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller
{
    public class PlcAgent
    {
        private LoggerAgent loggerAgent;

        public PlcAgent()
        {
            loggerAgent = LoggerAgent.Instance;
        }
    }
}
