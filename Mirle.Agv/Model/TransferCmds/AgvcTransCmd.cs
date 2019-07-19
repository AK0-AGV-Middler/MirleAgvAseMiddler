using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;
using TcpIpClientSample;
using Google.Protobuf.Collections;


namespace Mirle.Agv.Model.TransferCmds
{
    public class AgvcTransCmd
    {
        public EnumAgvcTransCmdType CmdType { get; set; }
        public string[] ToLoadSections { get; set; }
        public string[] ToUnloadSections { get; set; }
        public string[] ToLoadAddresses { get; set; }
        public string[] ToUnloadAddresses { get; set; }
        public string LoadAddress { get; set; } = "Empty";
        public string UnloadAddtess { get; set; } = "Empty";
        public string CarrierId { get; set; } = "Empty";
        public string CmdId { get; set; } = "Empty";
        public ushort SeqNum { get; set; }

        public AgvcTransCmd()
        {
            CmdId = "Empty";
            CarrierId = "Empty";
        }

        public AgvcTransCmd(ID_31_TRANS_REQUEST transRequest,ushort aSeqNum)
        {
            CmdId = transRequest.CmdID;
            if (!string.IsNullOrEmpty(transRequest.CSTID))
            {
                CarrierId = transRequest.CSTID;
            }
            SetCmdType(transRequest.ActType);
            SeqNum = aSeqNum;
        }

        protected void SetCmdType(ActiveType activeType)
        {
            switch (activeType)
            {
                case ActiveType.Move:
                    CmdType = EnumAgvcTransCmdType.Move;
                    break;
                case ActiveType.Load:
                    CmdType = EnumAgvcTransCmdType.Load;
                    break;
                case ActiveType.Unload:
                    CmdType = EnumAgvcTransCmdType.Unload;
                    break;
                case ActiveType.Loadunload:
                    CmdType = EnumAgvcTransCmdType.LoadUnload;
                    break;
                case ActiveType.Home:
                    CmdType = EnumAgvcTransCmdType.Home;
                    break;
                case ActiveType.Override:
                    CmdType = EnumAgvcTransCmdType.Override;
                    break;
                case ActiveType.Mtlhome:
                case ActiveType.Movetomtl:
                case ActiveType.Systemout:
                case ActiveType.Systemin:
                case ActiveType.Techingmove:
                case ActiveType.Round:
                default:
                    CmdType = EnumAgvcTransCmdType.Else;
                    break;
            }
        }

        protected void SetToLoadAddresses(RepeatedField<string> guideAddressesStartToLoad)
        {
            if (guideAddressesStartToLoad != null)
            {
                ToLoadAddresses = guideAddressesStartToLoad.ToArray();
            }
        }

        protected void SetToLoadSections(RepeatedField<string> guideSectionsStartToLoad)
        {
            if (guideSectionsStartToLoad != null)
            {
                ToLoadSections = guideSectionsStartToLoad.ToArray();
            }
        }

        protected void SetToUnloadAddresses(RepeatedField<string> guideAddressesToDestination)
        {
            if (guideAddressesToDestination != null)
            {
                ToUnloadAddresses = guideAddressesToDestination.ToArray();
            }
        }

        protected void SecToUnloadSections(RepeatedField<string> guideSectionsToDestination)
        {
            if (guideSectionsToDestination != null)
            {
                ToUnloadSections = guideSectionsToDestination.ToArray();
            }
        }        
    }

    public class AgvcMoveCmd : AgvcTransCmd
    {
        public AgvcMoveCmd(ID_31_TRANS_REQUEST transRequest,ushort aSeqNum) : base(transRequest,aSeqNum)
        {
            try
            {
                SecToUnloadSections(transRequest.GuideSectionsToDestination);
                SetToUnloadAddresses(transRequest.GuideAddressesToDestination);

                UnloadAddtess = transRequest.DestinationAdr;
            }
            catch (Exception ex)
            {
                var exlog = ex.StackTrace;
            }
        }
    }

    public class AgvcLoadCmd : AgvcTransCmd
    {
        public AgvcLoadCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {
            try
            {
                SetToLoadSections(transRequest.GuideSectionsStartToLoad);
                SetToLoadAddresses(transRequest.GuideAddressesStartToLoad);
                LoadAddress = transRequest.LoadAdr;
            }
            catch (Exception ex)
            {
                var exlog = ex.StackTrace;
            }
        }
    }

    public class AgvcUnloadCmd : AgvcTransCmd
    {
        public AgvcUnloadCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {
            try
            {
                SecToUnloadSections(transRequest.GuideSectionsToDestination);
                SetToUnloadAddresses(transRequest.GuideAddressesToDestination);
                UnloadAddtess = transRequest.DestinationAdr;
            }
            catch (Exception ex)
            {
                var exlog = ex.StackTrace;
            }
        }
    }

    public class AgvcLoadunloadCmd : AgvcTransCmd
    {
        public AgvcLoadunloadCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {
            try
            {
                SetToLoadSections(transRequest.GuideSectionsStartToLoad);
                SetToLoadAddresses(transRequest.GuideAddressesStartToLoad);
                LoadAddress = transRequest.LoadAdr;
                SecToUnloadSections(transRequest.GuideSectionsToDestination);
                SetToUnloadAddresses(transRequest.GuideAddressesToDestination);
                UnloadAddtess = transRequest.DestinationAdr;
            }
            catch (Exception ex)
            {
                var exlog = ex.StackTrace;
            }
        }
    }

    public class AgvcHomeCmd : AgvcTransCmd
    {
        public AgvcHomeCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {
        }
    }

    public class AgvcOverrideCmd : AgvcTransCmd
    {
        public AgvcOverrideCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {
            SetToLoadSections(transRequest.GuideSectionsStartToLoad);
            SetToLoadAddresses(transRequest.GuideAddressesStartToLoad);
            SecToUnloadSections(transRequest.GuideSectionsToDestination);
            SetToUnloadAddresses(transRequest.GuideAddressesToDestination);
        }
    }
}
