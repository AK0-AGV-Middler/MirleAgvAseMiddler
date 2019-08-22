using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using ElmoMotionControlComponents.Drive.EASComponents;
using Mirle.Agv.Model.Configs;

namespace Mirle.Agv.Model
{
    public class AxisInfo
    {
        public ElmoSingleAxisConfig Config { get; set; }
        public MMCSingleAxis SingleAxis { get; set; }
        public MMCGroupAxis GroupAxis { get; set; }
        public int[] GroupOlderToCommandOlder { get; set; }
        public ushort VirtualLink { get; set; }
        public ElmoAxisFeedbackData FeedbackData { get; set; } = new ElmoAxisFeedbackData();
        public bool Linking { get; set; } = false;
        public double LastCommandPosition { get; set; }
        public bool NeedAssignLastCommandPosition { get; set; } = true;
    }
}
