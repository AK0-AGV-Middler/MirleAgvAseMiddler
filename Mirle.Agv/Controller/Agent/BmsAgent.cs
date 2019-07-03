using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller
{
    public class BmsAgent
    {
        private LoggerAgent loggerAgent;

        public BmsAgent()
        {
            loggerAgent = LoggerAgent.Instance;
        }
    }
}
