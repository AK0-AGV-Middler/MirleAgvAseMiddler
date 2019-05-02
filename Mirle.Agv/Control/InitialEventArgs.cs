using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Control
{
    public class InitialEventArgs : EventArgs
    {
        public bool IsOk { get; set; }
        public string ItemName { get; set; }

        public InitialEventArgs()
        {
        }
    }
}
