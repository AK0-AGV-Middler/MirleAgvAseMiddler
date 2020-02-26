using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;

namespace Mirle.Agv.AseMiddler.Model
{
    public class AseMovingGuide
    {
        public List<string> GuideSectionIds { get; set; } = new List<string>();
        public List<string> GuideAddressIds { get; set; } = new List<string>();
        public string FromAddressId { get; set; } = "";
        public string ToAddressId { get; set; } = "";
        public uint GuideDistance { get; set; } = 0;
        public VhStopSingle ReserveStop { get; set; } = VhStopSingle.Off;
        public VhStopSingle PauseStatus { get; set; } = VhStopSingle.Off;
        public bool IsAvoidComplete { get; set; } = false;
        public List<MapSection> MovingSections { get; set; } = new List<MapSection>();
        public int MovingSectionsIndex { get; set; } = 0;
        public ushort SeqNum { get; set; }

        public AseMovingGuide() { }

        public AseMovingGuide(ID_38_GUIDE_INFO_RESPONSE response)
        {
            var info = response.GuideInfoList[0];
            this.GuideSectionIds = info.GuideSections.ToList();
            this.GuideAddressIds = info.GuideAddresses.ToList();
            this.FromAddressId = info.FromTo.From;
            this.ToAddressId = info.FromTo.To;
            this.GuideDistance = info.Distance;
        }

        public AseMovingGuide(AseMovingGuide aseMovingGuide)
        {
            this.GuideSectionIds = aseMovingGuide.GuideSectionIds;
            this.GuideAddressIds = aseMovingGuide.GuideAddressIds;
            this.FromAddressId = aseMovingGuide.FromAddressId;
            this.ToAddressId = aseMovingGuide.ToAddressId;
            this.GuideDistance = aseMovingGuide.GuideDistance;
            this.ReserveStop = aseMovingGuide.ReserveStop;
            this.PauseStatus = aseMovingGuide.PauseStatus;
            this.IsAvoidComplete = aseMovingGuide.IsAvoidComplete;
            this.MovingSections = aseMovingGuide.MovingSections;
            this.MovingSectionsIndex = aseMovingGuide.MovingSectionsIndex;
        }

        public AseMovingGuide(ID_51_AVOID_REQUEST request,ushort seqNum)
        {
            this.ToAddressId = request.DestinationAdr;
            this.GuideSectionIds = request.GuideSections.ToList();
            this.GuideAddressIds = request.GuideAddresses.ToList();
            this.SeqNum = SeqNum;
        }
    }
}
