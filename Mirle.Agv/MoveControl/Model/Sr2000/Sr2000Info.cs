using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Keyence.AutoID.SDK;

namespace Mirle.AgvAseMiddler.Model
{
    public class Sr2000Info
    {
        public ReaderAccessor Reader { get; set; }
        public bool Trigger { get; set; }
        public bool Connect { get; set; }
        public Thread RunThread { get; set; }

        public Sr2000Info(string ipAddress)
        {
            Reader = new ReaderAccessor();
            Reader.IpAddress = ipAddress;
            Trigger = false;
            Connect = false;
        }
    }
}
