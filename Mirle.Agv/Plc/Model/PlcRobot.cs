using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class PlcRobot
    {
        public bool ForkReady { get; set; }
        public bool ForkBusy { get; set; }
        public bool ForkFinish { get; set; }
        public bool ForkHome { get; set; } = true;//20190807_Rudy 新增ForkHome
        public bool ForkNG { get; set; } = false;
        public bool ForkPrePioFail { get; set; } = false;
        public bool ForkBusyFail { get; set; } = false;
        public bool ForkPostPioFail { get; set; } = false;
        
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
        

        public double ForkAlignmentP { get; set; } = 0f;
        public double ForkAlignmentY { get; set; } = 0f;
        public double ForkAlignmentPhi { get; set; } = 0f;
        public double ForkAlignmentF { get; set; } = 0f;
        public int ForkAlignmentCode { get; set; } = 0;
        public double ForkAlignmentC { get; set; } = 0f;
        public double ForkAlignmentB { get; set; } = 0f;



    }
}
