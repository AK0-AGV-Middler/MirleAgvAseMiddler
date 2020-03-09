using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler
{
    #region MainEnums
    public enum EnumSectionType
    {
        None,
        Horizontal,
        Vertical,
        R2000
    }    

    public enum EnumAddressDirection
    {
        None = 0,
        Left = 1,
        Right = 2
    }

    public enum EnumPioDirection
    {
        None,
        Left,
        Right
    }

    public enum EnumCommandDirection
    {
        None,
        Forward,
        Backward
    }

    public enum PsMessageType
    {
        P,
        S
    }

    public enum EnumTransferStepType
    {
        Move,
        MoveToCharger,
        Load,
        Unload,
        Empty
    }

    public enum EnumAgvcTransCommandType
    {
        Move,
        Load,
        Unload,
        LoadUnload,
        Override,
        MoveToCharger,
        Else
    }

    public enum EnumConnectState
    {
        Offline,
        OnlineRemote,
        OnlineLoacal
    }

    public enum EnumAutoState
    {
        Auto,
        Manual,
        PreManual
    }

    public enum EnumLoginLevel
    {
        None,
        Op,
        Engineer,
        Admin,
        OneAboveAll
    }

    public enum EnumCompleteStatus
    {
        Move = 0,
        Load = 1,
        Unload = 2,
        LoadUnload = 3,
        Home = 4,
        MtlHome = 7,
        MoveToMtl = 10,
        SystemOut = 11,
        SystemIn = 12,
        Cancel = 20,
        Abort = 21,
        VehicleAbort = 22,
        IdMissmatch = 23,
        IdReadFail = 24,
        InterlockError = 64,
        TransferComplete = 123  //Yiming
    }

    public enum EnumCmdNum
    {
        Cmd000_EmptyCommand = 0,
        Cmd31_TransferRequest = 31,
        Cmd32_TransferCompleteResponse = 32,
        Cmd35_CarrierIdRenameRequest = 35,
        Cmd36_TransferEventResponse = 36,
        Cmd37_TransferCancelRequest = 37,
        Cmd38_GuideInfoResponse = 38,
        Cmd39_PauseRequest = 39,
        Cmd41_ModeChange = 41,
        Cmd43_StatusRequest = 43,
        Cmd44_StatusRequest = 44,
        Cmd45_PowerOnoffRequest = 45,
        Cmd51_AvoidRequest = 51,
        Cmd52_AvoidCompleteResponse = 52,
        Cmd71_RangeTeachRequest = 71,
        Cmd72_RangeTeachCompleteResponse = 72,
        Cmd74_AddressTeachResponse = 74,
        Cmd91_AlarmResetRequest = 91,
        Cmd94_AlarmResponse = 94,
        Cmd131_TransferResponse = 131,
        Cmd132_TransferCompleteReport = 132,
        Cmd133_ControlZoneCancelResponse = 133,
        Cmd134_TransferEventReport = 134,
        Cmd135_CarrierIdRenameResponse = 135,
        Cmd136_TransferEventReport = 136,
        Cmd137_TransferCancelResponse = 137,
        Cmd139_PauseResponse = 139,
        Cmd141_ModeChangeResponse = 141,
        Cmd143_StatusResponse = 143,
        Cmd144_StatusReport = 144,
        Cmd145_PowerOnoffResponse = 145,
        Cmd151_AvoidResponse = 151,
        Cmd152_AvoidCompleteReport = 152,
        Cmd171_RangeTeachResponse = 171,
        Cmd172_RangeTeachCompleteReport = 172,
        Cmd174_AddressTeachReport = 174,
        Cmd191_AlarmResetResponse = 191,
        Cmd194_AlarmReport = 194,
    }

    public enum EnumAddressAction
    {
        ST,
        BST,
        TR50,
        TR350,
        BTR50,
        BTR350,
        R2000,
        BR2000,
        End,
        SlowStop
    }

    public enum EnumAlarmLevel
    {
        Warn,
        Alarm
    }

    public enum EnumThreadStatus
    {
        None,
        Start,
        Pause,
        PauseComplete,
        Working,
        Stop,
        StopComplete
    }

    public enum EnumCstIdReadResult
    {
        Noraml,
        Mismatch,
        Fail
    }

    public enum EnumBarcodeMaterial
    {
        Iron,
        Papper,
        None
    }

    public enum EnumBeamDirection
    {
        Front,
        Back,
        Left,
        Right
    }

    public enum EnumVehicleLocation
    {
        None,
        Section,
        Address
    }

    public enum EnumPspConnectionState
    {
        Offline,
        Online,
        CheckCheckConnectMode
    }

    public enum EnumAseRobotState
    {
        Idle,
        Busy,
        Error
    }

    public enum EnumAseMoveState
    {
        Idle,
        Working,
        Pausing,
        Pause,
        Stoping,
        Block,
        Error
    }

    public enum EnumAseCarrierSlotStatus
    {
        Empty,
        Loading,
        ReadFail
    }

    public enum EnumMoveComplete
    {
        Success,
        Fail,
        Pause,
        Cancel
    }

    public enum EnumStageDirection
    {
        None,
        Left,
        Right
    }

    public enum EnumSlotNumber
    {
        L,
        R
    }

    public enum EnumAseArrival
    {
        Fail,
        Arrival,
        EndArrival
    }

    #endregion

    /// <summary>
    /// AgvcTransCmd/XxxCmdInfo cannot deepClone since stopwatch cannot [Serializable]
    /// </summary>
    public static class ExtensionMethods
    {
        public static T DeepClone<T>(this T item)
        {
            try
            {
                if (item != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        var formatter = new BinaryFormatter();
                        lock (item)
                        {
                            formatter.Serialize(stream, item);
                        }
                        stream.Seek(0, SeekOrigin.Begin);
                        var result = (T)formatter.Deserialize(stream);
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                return default(T);
            }

            return default(T);
        }
    }
}
