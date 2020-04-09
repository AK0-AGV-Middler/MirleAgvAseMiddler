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
        public string commandId { get; set; } = "";

        public AseMovingGuide() { }

        public AseMovingGuide(ID_38_GUIDE_INFO_RESPONSE response)
        {
            var info = response.GuideInfoList[0];
            this.GuideSectionIds = info.GuideSections.ToList();
            this.GuideAddressIds = info.GuideAddresses.ToList();
            this.FromAddressId = info.FromTo.From;
            this.ToAddressId = info.FromTo.To;
            this.GuideDistance = info.Distance;
            this.commandId = Vehicle.Instance.AseMovingGuide.commandId;
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
            this.commandId = aseMovingGuide.commandId;
            this.SeqNum = aseMovingGuide.SeqNum;
        }

        public AseMovingGuide(ID_51_AVOID_REQUEST request, ushort seqNum)
        {
            this.ToAddressId = string.IsNullOrEmpty(request.DestinationAdr.Trim()) ? "" : request.DestinationAdr.Trim();
            this.GuideSectionIds = request.GuideSections.Any() ? request.GuideSections.ToList() : new List<string>();
            this.GuideAddressIds = request.GuideAddresses.Any() ? request.GuideAddresses.ToList() : new List<string>();
            this.SeqNum = seqNum;
            this.commandId = string.IsNullOrEmpty(Vehicle.Instance.AseMovingGuide.commandId) ? "" : Vehicle.Instance.AseMovingGuide.commandId;
            this.ReserveStop = VhStopSingle.On;
        }

        public string GetInfo()
        {
            string fromToString = $"From = [{FromAddressId}], To = [{ToAddressId}]\r\n";
            string guideSectionsString = "GuideSections is Empty.\r\n";
            if (GuideSectionIds.Any())
            {
                guideSectionsString = string.Concat("GuideSections = ", string.Join(", ", GuideSectionIds), Environment.NewLine);
            }
            string guideAddressesString = "GuideAddresses is Empty.\r\n";
            if (GuideAddressIds.Any())
            {
                guideAddressesString = string.Concat("GuideAddresses = ", string.Join(", ", GuideAddressIds), Environment.NewLine);
            }
            string movingSectionsString = "MovingSections is Empty.\r\n";
            if (MovingSections.Any())
            {
                List<string> movingSectionIds = MovingSections.Select(x => x.Id).ToList();
                movingSectionsString = string.Concat("MovingSections = ", string.Join(", ", movingSectionIds), Environment.NewLine);
            }

            string msg = string.Concat(fromToString, guideSectionsString, guideAddressesString, movingSectionsString);
            return msg;
        }
    }
}
