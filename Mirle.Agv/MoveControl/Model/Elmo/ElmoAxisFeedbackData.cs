using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class ElmoAxisFeedbackData
    {
        public uint Count { get; set; }
        public int Feedback_Velocity { get; set; }
        public double Feedback_Position_Error { get; set; }
        public int Feedback_Torque { get; set; }
        public string Feedback_Now_Mode { get; set; }
        public double Feedback_Position { get; set; }
        public double Feedback_PositionBulk { get; set; }
        public bool StandStill { get; set; }
        public bool Inmotion { get; set; }
        public bool Disable { get; set; }
        public bool Homing { get; set; }
        public bool Stopping { get; set; }
        public bool ErrorStop { get; set; }
        public int ErrorCode { get; set; }
        public DateTime GetDataTime { get; set; }
    }
}
