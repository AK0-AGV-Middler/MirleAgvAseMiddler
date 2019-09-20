using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class ThetaSectionDeviation
    {
        public uint Count { get; set; }
        public int Index { get; set; }
        public double Theta { get; set; }
        public double SectionDeviation { get; set; }
        public double BarcodeAngleInMap { get; set; }
        public double AGVAngleInMap { get; set; }

        public ThetaSectionDeviation(double theta, double sectionDeviation, uint count, int index = -1)
        {
            Theta = theta;
            SectionDeviation = sectionDeviation;
            Count = count;
            Index = index;
        }

        public ThetaSectionDeviation()
        {
            Count = 0;
            Theta = 0;
            SectionDeviation = 0;
        }
    }
}
