using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class BarcodeSafetyData
    {
        public double StartEncoder { get; set; }
        public double EndEncoder { get; set; }
        public string BarcodeLineID { get; set; }
        public bool MustRead { get; set; }

        public BarcodeSafetyData(double startEncoder, double endEncoder, string barcodeLineID, bool mustRead)
        {
            StartEncoder = startEncoder;
            EndEncoder = endEncoder;
            BarcodeLineID = barcodeLineID;
            MustRead = mustRead;
        }
    }
}
