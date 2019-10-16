using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;
using TcpIpClientSample;
using Google.Protobuf.Collections;
using System.Reflection;
using Mirle.Agv.Controller.Tools;

namespace Mirle.Agv.Model.TransferSteps
{
    [Serializable]
    public class AgvcTransCmd
    {
        public EnumAgvcTransCommandType CommandType { get; set; } = EnumAgvcTransCommandType.Else;
        public List<string> ToLoadSectionIds { get; set; } = new List<string>();
        public List<string> ToUnloadSectionIds { get; set; } = new List<string>();
        public List<string> ToLoadAddressIds { get; set; } = new List<string>();
        public List<string> ToUnloadAddressIds { get; set; } = new List<string>();
        public string LoadAddressId { get; set; } = "";
        public string UnloadAddressId { get; set; } = "";
        public string CassetteId { get; set; } = "";
        public string CommandId { get; set; } = "";
        public ushort SeqNum { get; set; }
        public double CommandDistance { get; set; }
        public CompleteStatus CompleteStatus { get; set; }
        public VhStopSingle PauseStatus { get; set; } = VhStopSingle.StopSingleOff;
        public VhStopSingle ReserveStatus { get; set; } = VhStopSingle.StopSingleOff;

        public AgvcTransCmd()
        {
        }

        public AgvcTransCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum)
        {
            CommandId = transRequest.CmdID.Trim();
            CassetteId = string.IsNullOrEmpty(transRequest.CSTID) ? "" : transRequest.CSTID.Trim();
            CommandType = SetupCommandType(transRequest.ActType);
            SeqNum = aSeqNum;
            CompleteStatus = SetupCompleteStatus(transRequest.ActType);
        }

        protected CompleteStatus SetupCompleteStatus(ActiveType actType)
        {
            switch (actType)
            {
                case ActiveType.Move:
                    return CompleteStatus.CmpStatusMove;
                case ActiveType.Load:
                    return CompleteStatus.CmpStatusLoad;
                case ActiveType.Unload:
                    return CompleteStatus.CmpStatusUnload;
                case ActiveType.Loadunload:
                    return CompleteStatus.CmpStatusLoadunload;
                case ActiveType.Home:
                    break;
                case ActiveType.Override:
                    break;
                case ActiveType.Cstidrename:
                    break;
                case ActiveType.Mtlhome:
                    break;
                case ActiveType.Movetocharger:
                    return CompleteStatus.CmpStatusMoveToCharger;
                case ActiveType.Systemout:
                    break;
                case ActiveType.Systemin:
                    break;
                case ActiveType.Techingmove:
                    break;
                case ActiveType.Round:
                    break;
                default:
                    break;
            }
            return CompleteStatus.CmpStatusVehicleAbort;
        }

        protected EnumAgvcTransCommandType SetupCommandType(ActiveType activeType)
        {
            switch (activeType)
            {
                case ActiveType.Move:
                    return EnumAgvcTransCommandType.Move;
                case ActiveType.Load:
                    return EnumAgvcTransCommandType.Load;
                case ActiveType.Unload:
                    return EnumAgvcTransCommandType.Unload;
                case ActiveType.Loadunload:
                    return EnumAgvcTransCommandType.LoadUnload;
                case ActiveType.Movetocharger:
                    return EnumAgvcTransCommandType.MoveToCharger;
                case ActiveType.Override:
                    return EnumAgvcTransCommandType.Override;
                case ActiveType.Home:
                case ActiveType.Cstidrename:
                case ActiveType.Mtlhome:
                case ActiveType.Systemout:
                case ActiveType.Systemin:
                case ActiveType.Techingmove:
                case ActiveType.Round:
                default:
                    return EnumAgvcTransCommandType.Else;
            }
        }

        public ActiveType GetActiveType()
        {
            switch (CommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    return ActiveType.Move;
                case EnumAgvcTransCommandType.Load:
                    return ActiveType.Load;
                case EnumAgvcTransCommandType.Unload:
                    return ActiveType.Unload;
                case EnumAgvcTransCommandType.LoadUnload:
                    return ActiveType.Loadunload;
                case EnumAgvcTransCommandType.Override:
                    return ActiveType.Override;
                case EnumAgvcTransCommandType.MoveToCharger:
                    return ActiveType.Movetocharger;
                case EnumAgvcTransCommandType.Else:
                default:
                    return ActiveType.Mtlhome;
            }
        }

        protected void SetToLoadAddresses(RepeatedField<string> guideAddressesStartToLoad)
        {
            if (guideAddressesStartToLoad != null)
            {
                ToLoadAddressIds = guideAddressesStartToLoad.ToList();
            }
        }

        protected void SetToLoadSections(RepeatedField<string> guideSectionsStartToLoad)
        {
            if (guideSectionsStartToLoad != null)
            {
                ToLoadSectionIds = guideSectionsStartToLoad.ToList();
            }
        }

        protected void SetToUnloadAddresses(RepeatedField<string> guideAddressesToDestination)
        {
            if (guideAddressesToDestination != null)
            {
                ToUnloadAddressIds = guideAddressesToDestination.ToList();
            }
        }

        protected void SecToUnloadSections(RepeatedField<string> guideSectionsToDestination)
        {
            if (guideSectionsToDestination != null)
            {
                ToUnloadSectionIds = guideSectionsToDestination.ToList();
            }
        }

        public AgvcTransCmd DeepClone()
        {
            AgvcTransCmd agvcTransCmd = new AgvcTransCmd();
            agvcTransCmd.CommandType = CommandType;
            agvcTransCmd.ToLoadSectionIds = ToLoadSectionIds.DeepClone();
            agvcTransCmd.ToLoadAddressIds = ToLoadAddressIds.DeepClone();
            agvcTransCmd.ToUnloadSectionIds = ToUnloadSectionIds.DeepClone();
            agvcTransCmd.ToUnloadAddressIds = ToUnloadAddressIds.DeepClone();
            agvcTransCmd.LoadAddressId = LoadAddressId;
            agvcTransCmd.UnloadAddressId = UnloadAddressId;
            agvcTransCmd.CassetteId = CassetteId;
            agvcTransCmd.CommandId = CommandId;
            agvcTransCmd.SeqNum = SeqNum;
            agvcTransCmd.CompleteStatus = CompleteStatus;
            agvcTransCmd.PauseStatus = PauseStatus;

            return agvcTransCmd;
        }

        public void ExchangeSectionsAndAddress(AgvcOverrideCmd agvcOverrideCmd)
        {
            ToLoadSectionIds = agvcOverrideCmd.ToLoadSectionIds;
            ToLoadAddressIds = agvcOverrideCmd.ToLoadAddressIds;
            ToUnloadSectionIds = agvcOverrideCmd.ToUnloadSectionIds;
            ToUnloadAddressIds = agvcOverrideCmd.ToUnloadAddressIds;
        }
    }

    public class AgvcMoveCmd : AgvcTransCmd
    {
        public AgvcMoveCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {
            try
            {
                SecToUnloadSections(transRequest.GuideSectionsToDestination);
                SetToUnloadAddresses(transRequest.GuideAddressesToDestination);

                UnloadAddressId = transRequest.DestinationAdr;
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public AgvcMoveCmd(ID_51_AVOID_REQUEST transRequest, ushort aSeqNum)
        {
            try
            {
                SeqNum = aSeqNum;
                SecToUnloadSections(transRequest.GuideSections);
                SetToUnloadAddresses(transRequest.GuideAddresses);

               // UnloadAddressId = transRequest.DestinationAdr;    10.16.2019 300尚未提供
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }
    }
    public class AgvcMoveToChargerCmd : AgvcMoveCmd
    {
        public AgvcMoveToChargerCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {

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
                LoadAddressId = transRequest.LoadAdr;
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                UnloadAddressId = transRequest.DestinationAdr;
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                LoadAddressId = transRequest.LoadAdr;
                SecToUnloadSections(transRequest.GuideSectionsToDestination);
                SetToUnloadAddresses(transRequest.GuideAddressesToDestination);
                UnloadAddressId = transRequest.DestinationAdr;
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
            try
            {
                SetToLoadSections(transRequest.GuideSectionsStartToLoad);
                SetToLoadAddresses(transRequest.GuideAddressesStartToLoad);
                if (!string.IsNullOrEmpty(transRequest.LoadAdr))
                {
                    LoadAddressId = transRequest.LoadAdr;
                }
                SecToUnloadSections(transRequest.GuideSectionsToDestination);
                SetToUnloadAddresses(transRequest.GuideAddressesToDestination);
                if (!string.IsNullOrEmpty(transRequest.DestinationAdr))
                {
                    UnloadAddressId = transRequest.DestinationAdr;
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }
    }
}
