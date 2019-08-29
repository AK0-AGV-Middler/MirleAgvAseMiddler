using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv
{
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

    public enum EnumSensorSafetyType
    {
        Charging,
        ForkHome,
        BeamSensor,
        BeamSensorTR,
        TRFlowStart,
        BeamSensorR2000,
        R2000FlowStat,
        Bumper,
        CheckAxisState,
        TRPathMonitoring
    }

    public enum EnumMoveStartType
    {
        FirstMove,
        ChangeDirFlagMove,
        ReserveStopMove,
        SensorStopMove
    }
    
    public enum EnumVChangeType
    {
        Normal,
        TRTurn,
        R2000Turn,
        SensorSlow
    }
    #endregion
}
