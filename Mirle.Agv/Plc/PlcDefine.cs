using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv
{
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
    public enum EnumDirectionalLightType
    {
        None = 0,
        SpinL = 1,
        SpinR = 2,
        SteerFR = 3,
        SteerFL = 4,
        RTraverse = 5,
        LTraverse = 6,
        SteerBR = 7,
        SteerBL = 8,
        Backward = 9,
        Forward = 10
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

    public enum EnumJogVehicleMode
    {
        No_Use = 0,
        Auto = 1,
        Manual = 2
    }

    public enum EnumJogElmoFunction
    {
        No_Use = 0,
        Enable = 1,
        Disable = 2,
        All_Reset = 3
    }

    public enum EnumJogRunMode
    {
        No_Use = 0,
        Normal = 1,
        ForwardWheel = 2,
        BackwardWheel = 3,
        SpinTurn = 4

    }

    public enum EnumJogOperation
    {
        No_Use = 0,
        Stop = 1,
        MoveBackward = 16,
        MoveForward = 32,
        TurnLeft = 64,
        TurnRight = 128

    }

    public enum EnumJogTurnSpeed
    {
        No_Use = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum EnumJogMoveVelocity
    {
        No_Use = 0,
        Ten = 1,
        Fifty = 2,
        OneHundred = 3,
        ThreeHundred = 4
    }

    public enum EnumElmoStatus
    {
        No_Use = 0,
        Disable = 1,
        StandStill = 16
    }

    public enum EnumPlcOperationStep
    {
        No_Use,
        VehicleModeAuto,
        VehicleModeManual,
        ElmoDisable,
        ElmoEnable,
        ElmoAllReset,
        RunModeNormal,
        RunModeForwardWheel,
        RunModeBackwardWheel,
        RunModeSpinTurn,
        JogOperationStop,
        JogOperationMoveBackward,
        JogOperationMoveForward,
        JogOperationTurnLeft,
        JogOperationTurnRight

    }

    #endregion
}
