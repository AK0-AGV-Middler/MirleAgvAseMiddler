using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Control
{
    public class CoupleHandler
    {
        private LoggerAgent loggerAgent;

        public CoupleHandler()
        {
            loggerAgent = LoggerAgent.Instance;
        }
    }
}
