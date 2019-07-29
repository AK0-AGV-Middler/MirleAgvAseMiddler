using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class PLCRobot
    {
        public bool ForkReady { get; set; }
        public bool ForkBusy { get; set; }
        public bool ForkFinish { get; set; }

        private ushort currentCommandNo = 0;
        public ushort CurrentCommandNo
        {
            get
            {
                currentCommandNo = Convert.ToUInt16((Convert.ToInt32(CurrentCommandNo) + 1) % 65535);
                return currentCommandNo;
            }
        }
        public PlcForkCommand ExecutingCommand { get; set; } = null;
    }
}
