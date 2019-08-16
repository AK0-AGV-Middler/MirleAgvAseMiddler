using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv
{
    #region MainEnums
    public enum EnumSectionType
    {
        None,
        Horizontal,
        Vertical,
        R2000
    }

    public enum EnumChargeDirection
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

    public enum EnumPermitDirection
    {
        None,
        Forward,
        Backward
    }

    public enum EnumRowBarcodeType
    {
        None,
        Horizontal,
        Vertical
    }

    public enum EnumTransferCommandType
    {
        Move,
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
        Home,
        Override,
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
        Manual,
        Auto
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

    public enum EnumMapBarcodeReaderSide
    {
        None,
        Left,
        Right
    }

    public enum EnumCmdNums
    {
        Cmd000_EmptyCommand = 0,
        Cmd31_TransferRequest = 31,
        Cmd32_TransferCompleteResponse = 32,
        Cmd33_ControlZoneCancelRequest = 33,
        Cmd35_CarrierIdRenameRequest = 35,
        Cmd36_TransferEventResponse = 36,
        Cmd37_TransferCancelRequest = 37,
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
    #endregion

    #region PlcEnums
    public enum EnumForkCommandState
    {
        Queue = 0,
        Executing = 1,
        Finish = 2,
        Error = 99

    }

    public enum EnumVehicleSide
    {
        None = 0,
        Forward = 1,
        Backward = 2,
        Left = 3,
        Right = 4

    }

    public enum EnumVehicleSafetyAction
    {
        Normal = 0,
        LowSpeed = 1,
        Stop = 2
    }
    public enum EnumForkCommand
    {
        Load = 2,
        Unload = 4,
        Pre_Load = 1,
        Pre_Unload = 3,
        Home = 255,
        None = 0
    }


    public enum EnumStageDirection
    {
        Left = 1,
        Right = 2,
        None = 0

    }

    public enum EnumForkCommandExecutionType
    {
        Command_Read_Request = 1,
        Command_Start = 2,
        Command_Finish_Ack = 3,
        None = 0

    }

    public enum EnumBatteryType
    {
        Gotech = 1,
        Yinda = 2,
        None = 0
    }

    public enum EnumIPCStatus
    {
        No_Use = 0,
        Run = 1,
        Idle = 2,
        Down = 3,
        Stop = 4,
        Pause = 5,
        Initial = 6,
        Manual = 7,
        Maintance = 8,
        Teaching = 9

    }
    #endregion

    #region MoveControlEnums

    public enum EnumLineReviseType
    {
        None,
        Theta,
        SectionDeviation
    }

    public enum EnumCommandType
    {
        TR,
        R2000,
        Vchange,
        ReviseOpen,
        ReviseClose,
        Move,
        SlowStop,
        Stop,
        End
    }

    public enum EnumJogPitchMode
    {
        Normal,
        ForwardWheel,
        BackwardWheel,
        SpinTurn
    }

    public enum EnumJogPitchModeName
    {
        四軸同動模式 = EnumJogPitchMode.Normal,
        前輪轉動模式 = EnumJogPitchMode.ForwardWheel,
        後輪轉動模式 = EnumJogPitchMode.BackwardWheel,
        原地旋轉模式 = EnumJogPitchMode.SpinTurn,
    }

    public enum EnumAxis
    {
        None,
        // 走行單軸.
        XFL,
        XFR,
        XRL,
        XRR,
        // 轉向單軸.
        TFL,
        TFR,
        TRL,
        TRR,
        // 走行單軸虛擬軸.
        VXFL,
        VXFR,
        VXRL,
        VXRR,
        // 轉向單軸虛擬軸.
        VTFL,
        VTFR,
        VTRL,
        VTRR,
        // Group.
        GX,
        GT
    }

    public enum EnumAxisType
    {
        Move,
        Turn
    }

    public enum EnumMoveType
    {
        Relative,
        Absolute
    }

    public enum EnumBeamSensorLocate
    {
        Front,
        Back,
        Left,
        Right
    }

    public enum EnumMoveState
    {
        Idle,
        Moving,
        TR,
        R2000,
        Error,
        WaitForReserve,
        WaitForResume
    }

    public enum EnumMoveComplete
    {
        Success,
        Fail
    }

    public enum EnumR2000Parameter
    {
        InnerWheelTurn,
        OuterWheelTurn,
        InnerWheelMove,
        OuterWheelMove
    }

    public enum EnumMoveControlSafetyType
    {
        TurnOut,
        LineBarcodeInterval,
        OntimeReviseTheta,
        OntimeReviseSectionDeviation,
        UpdateDeltaPositionRange
    }
    #endregion

    public static class ExtensionMethods
    {
        public static T DeepClone<T>(this T item)
        {
            if (item != null)
            {
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, item);
                    stream.Seek(0, SeekOrigin.Begin);
                    var result = (T)formatter.Deserialize(stream);
                    return result;
                }
            }

            return default(T);
        }
    }
}
