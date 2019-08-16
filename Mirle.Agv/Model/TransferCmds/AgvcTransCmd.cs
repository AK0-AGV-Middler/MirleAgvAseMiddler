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
    [Serializable]
    public class AgvcTransCmd
    {
        public EnumAgvcTransCommandType EnumCommandType { get; set; }
        public List<string> ToLoadSections { get; set; } = new List<string>();
        public List<string> ToUnloadSections { get; set; } = new List<string>();
        public List<string> ToLoadAddresses { get; set; } = new List<string>();
        public List<string> ToUnloadAddresses { get; set; } = new List<string>();
        public string LoadAddress { get; set; } = "Empty";
        public string UnloadAddress { get; set; } = "Empty";
        public string CassetteId { get; set; } = "Empty";
        public string CommandId { get; set; } = "Empty";
        public ushort SeqNum { get; set; }

        public AgvcTransCmd()
        {
            CommandId = "Empty";
            CassetteId = "Empty";
        }

        public AgvcTransCmd(ID_31_TRANS_REQUEST transRequest,ushort aSeqNum)
        {
            CommandId = transRequest.CmdID;
            if (!string.IsNullOrEmpty(transRequest.CSTID))
            {
                CassetteId = transRequest.CSTID;
            }
            SetCmdType(transRequest.ActType);
            SeqNum = aSeqNum;
        }

        protected void SetCmdType(ActiveType activeType)
        {
            switch (activeType)
            {
                case ActiveType.Move:
                    EnumCommandType = EnumAgvcTransCommandType.Move;
                    break;
                case ActiveType.Load:
                    EnumCommandType = EnumAgvcTransCommandType.Load;
                    break;
                case ActiveType.Unload:
                    EnumCommandType = EnumAgvcTransCommandType.Unload;
                    break;
                case ActiveType.Loadunload:
                    EnumCommandType = EnumAgvcTransCommandType.LoadUnload;
                    break;
                case ActiveType.Home:
                    EnumCommandType = EnumAgvcTransCommandType.Home;
                    break;
                case ActiveType.Override:
                    EnumCommandType = EnumAgvcTransCommandType.Override;
                    break;
                case ActiveType.Mtlhome:
                case ActiveType.Movetomtl:
                case ActiveType.Systemout:
                case ActiveType.Systemin:
                case ActiveType.Techingmove:
                case ActiveType.Round:
                default:
                    EnumCommandType = EnumAgvcTransCommandType.Else;
                    break;
            }
        }

        protected void SetToLoadAddresses(RepeatedField<string> guideAddressesStartToLoad)
        {
            if (guideAddressesStartToLoad != null)
            {
                ToLoadAddresses = guideAddressesStartToLoad.ToList();
            }
        }

        protected void SetToLoadSections(RepeatedField<string> guideSectionsStartToLoad)
        {
            if (guideSectionsStartToLoad != null)
            {
                ToLoadSections = guideSectionsStartToLoad.ToList();
            }
        }

        protected void SetToUnloadAddresses(RepeatedField<string> guideAddressesToDestination)
        {
            if (guideAddressesToDestination != null)
            {
                ToUnloadAddresses = guideAddressesToDestination.ToList();
            }
        }

        protected void SecToUnloadSections(RepeatedField<string> guideSectionsToDestination)
        {
            if (guideSectionsToDestination != null)
            {
                ToUnloadSections = guideSectionsToDestination.ToList();
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

                UnloadAddress = transRequest.DestinationAdr;
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
                UnloadAddress = transRequest.DestinationAdr;
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
                UnloadAddress = transRequest.DestinationAdr;
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
