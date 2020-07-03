using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Controller;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Google.Protobuf.Collections;
using System.Reflection;
 
using Mirle.Tools;

namespace Mirle.Agv.AseMiddler.Model.TransferSteps
{
    [Serializable]
    public class AgvcTransCmd
    {
        public string CommandId { get; set; } = "";
        public EnumAgvcTransCommandType AgvcTransCommandType { get; set; } = EnumAgvcTransCommandType.Else;
        public string LoadAddressId { get; set; } = "";
        public string UnloadAddressId { get; set; } = "";
        public string CassetteId { get; set; } = "";      
        public ushort SeqNum { get; set; }
        public double CommandDistance { get; set; }
        public CompleteStatus CompleteStatus { get; set; }
        public bool IsAvoidComplete { get; set; }
        public EnumSlotNumber SlotNumber { get; set; } = EnumSlotNumber.L;
        public CommandState EnrouteState { get; set; } = CommandState.None;
        public string LotId { get; set; } = "";
        public string LoadPortId { get; set; } = "";
        public string UnloadPortId { get; set; } = "";

        public AgvcTransCmd()
        {
        }

        public AgvcTransCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum)
        {
            CommandId = transRequest.CmdID.Trim();
            CassetteId = string.IsNullOrEmpty(transRequest.CSTID) ? "" : transRequest.CSTID.Trim();
            AgvcTransCommandType = SetupCommandType(transRequest.CommandAction);
            SeqNum = aSeqNum;
            CompleteStatus = SetupCompleteStatus(transRequest.CommandAction);
            
            LoadPortId = string.IsNullOrEmpty(transRequest.LoadPortID) ? "" : transRequest.LoadPortID.Trim();
            UnloadPortId = string.IsNullOrEmpty(transRequest.UnloadPortID) ? "" : transRequest.UnloadPortID.Trim();
        }

        protected CompleteStatus SetupCompleteStatus(CommandActionType actType)
        {           
            switch (actType)
            {
                case CommandActionType.Move:
                    return CompleteStatus.Move;                   
                case CommandActionType.Load:
                    return CompleteStatus.Load;
                case CommandActionType.Unload:
                    return CompleteStatus.Unload;
                case CommandActionType.Loadunload:
                    return CompleteStatus.Loadunload;
                case CommandActionType.Home:
                    break;
                case CommandActionType.Override:
                    return CompleteStatus.Loadunload;
                case CommandActionType.Movetocharger:
                    return CompleteStatus.MoveToCharger;
                default:
                    break;
            }
            return CompleteStatus.VehicleAbort;
        }

        protected EnumAgvcTransCommandType SetupCommandType(CommandActionType activeType)
        {
            switch (activeType)
            {
                case CommandActionType.Move:
                    return EnumAgvcTransCommandType.Move;
                case CommandActionType.Load:
                    return EnumAgvcTransCommandType.Load;
                case CommandActionType.Unload:
                    return EnumAgvcTransCommandType.Unload;
                case CommandActionType.Loadunload:
                    return EnumAgvcTransCommandType.LoadUnload;
                case CommandActionType.Home:
                    return EnumAgvcTransCommandType.Else;
                case CommandActionType.Override:
                    return EnumAgvcTransCommandType.Override;
                case CommandActionType.Movetocharger:
                    return EnumAgvcTransCommandType.MoveToCharger;
                default:
                    return EnumAgvcTransCommandType.Else;
            }
        }

        public CommandActionType GetCommandActionType()
        {
            switch (AgvcTransCommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    return CommandActionType.Move;
                case EnumAgvcTransCommandType.Load:
                    return CommandActionType.Load;
                case EnumAgvcTransCommandType.Unload:
                    return CommandActionType.Unload;
                case EnumAgvcTransCommandType.LoadUnload:
                    return CommandActionType.Loadunload;
                case EnumAgvcTransCommandType.Override:
                    return CommandActionType.Override;
                case EnumAgvcTransCommandType.MoveToCharger:
                    return CommandActionType.Movetocharger;
                case EnumAgvcTransCommandType.Else:
                default:
                    return CommandActionType.Home;
            }
        }

        protected void LogException(string source,string exMsg)
        {
            MirleLogger.Instance.Log(new LogFormat("Error", "5", source, "Device", "CarrierID", exMsg));
        }
    }

    public class AgvcMoveCmd : AgvcTransCmd
    {
        public AgvcMoveCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {
            try
            {
                UnloadAddressId = transRequest.DestinationAdr;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name,ex.Message);
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
                LoadAddressId = transRequest.LoadAdr;
                EnrouteState = CommandState.LoadEnroute;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
    }

    public class AgvcUnloadCmd : AgvcTransCmd
    {
        public AgvcUnloadCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {
            try
            {
                UnloadAddressId = transRequest.DestinationAdr;
                EnrouteState = CommandState.UnloadEnroute;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
    }

    public class AgvcLoadunloadCmd : AgvcTransCmd
    {
        public AgvcLoadunloadCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
        {
            try
            {
                LoadAddressId = transRequest.LoadAdr;
                UnloadAddressId = transRequest.DestinationAdr;
                EnrouteState = CommandState.LoadEnroute;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
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
                if (!string.IsNullOrEmpty(transRequest.LoadAdr))
                {
                    LoadAddressId = transRequest.LoadAdr;
                }
                if (!string.IsNullOrEmpty(transRequest.DestinationAdr))
                {
                    UnloadAddressId = transRequest.DestinationAdr;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
    }
}
