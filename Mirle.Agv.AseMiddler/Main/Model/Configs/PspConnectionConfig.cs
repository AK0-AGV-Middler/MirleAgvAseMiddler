using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model.Configs
{
    public class PspConnectionConfig
    {
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5000;
        public bool IsServer { get; set; } = true;
    }
}
