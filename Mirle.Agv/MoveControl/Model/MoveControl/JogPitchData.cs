using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class JogPitchData
    {
        public Dictionary<EnumAxis, ElmoAxisFeedbackData> AxisData { get; set; }
        public double MapX { get; set; }
        public double MapY { get; set; }
        public double MapTheta { get; set; }
        public double SectionDeviation { get; set; }
        public double Theta { get; set; }
        public bool ElmoFunctionCompelete { get; set; }

        public JogPitchData()
        {
            ElmoFunctionCompelete = true;
            AxisData = new Dictionary<EnumAxis, ElmoAxisFeedbackData>();
            foreach (EnumAxis item in (EnumAxis[])Enum.GetValues(typeof(EnumAxis)))
            {
                AxisData.Add(item, null);
            }
        }
    }
}
