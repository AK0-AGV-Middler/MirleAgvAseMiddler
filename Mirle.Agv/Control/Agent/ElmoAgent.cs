﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Control
{
    public class ElmoAgent
    {
        private LoggerAgent loggerAgent;

        public ElmoAgent()
        {
            loggerAgent = LoggerAgent.Instance;
        }
    }
}
