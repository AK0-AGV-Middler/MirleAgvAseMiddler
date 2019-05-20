using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Control;
using TcpIpClientSample;
using Google.Protobuf.Collections;

namespace Mirle.Agv.Model.TransferCmds
{
    public class AgvcTransCmd
    {
        public EnumAgvcTransCmdType CmdType { get; set; }
        public string[] ToLoadSections { get; set; }
        public string[] ToUnloadSections { get; set; }
        public string[] ToLoadAddress { get; set; }
        public string[] ToUnloadAddress { get; set; }
        public string LoadAddress { get; set; }
        public string UnloadAddtess { get; set; }
        public string CarrierId { get; set; }
        public string CmdId { get; set; }

        public AgvcTransCmd()
        {
            CmdId = "Empty";
            CarrierId = "Empty";
        }

        public AgvcTransCmd(ID_31_TRANS_REQUEST transRequest)
        {
            CmdId = transRequest.CmdID;
            if (!string.IsNullOrEmpty(transRequest.CSTID))
            {
                CarrierId = transRequest.CSTID;
            }
            SetCmdType(transRequest.ActType);
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

        protected void SetToLoadSections(RepeatedField<string> guideSectionsStartToLoad)
        {
            if (guideSectionsStartToLoad != null)
            {
                ToLoadSections = guideSectionsStartToLoad.ToArray();
            }
        }

        protected void SecToUnloadSections(RepeatedField<string> GuideAddressesToDestination)
        {
            if (GuideAddressesToDestination != null)
            {
                ToUnloadSections = GuideAddressesToDestination.ToArray();
            }

        }
    }

    public class AgvcMoveCmd : AgvcTransCmd
    {
        public AgvcMoveCmd(ID_31_TRANS_REQUEST transRequest) : base(transRequest)
        {
            try
            {
                SecToUnloadSections(transRequest.GuideSectionsToDestination);
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
        public AgvcLoadCmd(ID_31_TRANS_REQUEST transRequest) : base(transRequest)
        {
            try
            {
                SetToLoadSections(transRequest.GuideSectionsStartToLoad);
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
        public AgvcUnloadCmd(ID_31_TRANS_REQUEST transRequest) : base(transRequest)
        {
            try
            {
                SecToUnloadSections(transRequest.GuideSectionsToDestination);
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
        public AgvcLoadunloadCmd(ID_31_TRANS_REQUEST transRequest) : base(transRequest)
        {
            try
            {
                SetToLoadSections(transRequest.GuideSectionsStartToLoad);
                LoadAddress = transRequest.LoadAdr;
                SecToUnloadSections(transRequest.GuideSectionsToDestination);
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
        public AgvcHomeCmd(ID_31_TRANS_REQUEST transRequest) : base(transRequest)
        {
        }
    }

    public class AgvcOverrideCmd : AgvcTransCmd
    {
        public AgvcOverrideCmd(ID_31_TRANS_REQUEST transRequest) : base(transRequest)
        {
            SetToLoadSections(transRequest.GuideSectionsStartToLoad);
            SecToUnloadSections(transRequest.GuideSectionsToDestination);
        }
    }


}
