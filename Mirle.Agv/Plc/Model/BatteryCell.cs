using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class BatteryCell
    {
        public string Id { get; }
        public string TagId { get; }
        public double Voltage;
        public double Current;
        public double Temperature;

        public BatteryCell(int id)
        {
            Id = id.ToString();
            TagId = "BatteryCell_" + Id;
        }
    }
}
