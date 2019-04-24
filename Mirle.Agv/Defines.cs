using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv
{
    public enum EnumSectionType
    {
        None,
        Horizontal,
        Vertical,
        QuadrantI,
        QuadrantII,
        QuadrantIII,
        QuadrantIV
    }

    public enum EnumSectionShape
    {
        None,
        Straight,
        Curve
    }

    public enum EnumAddressType
    {
        None,
        Address,
        Position
    }

    public enum EnumTransCmdType
    {
        Move,
        Load,
        Unload
    }

    //public enum EnumAddressType
    //{
    //    EqPort,
    //    EqPortWithCoupler,
    //    Coupler,
    //    Stocker
    //}

    public enum EnumMoveState
    {
        Idle,
        Moving,
        MoveComplete,
        WaitForReserve,
        WaitForResume
    }

    public enum EnumMainFlowState
    {
        Idle,
        Move,
        Load,
        Unload
    }

    public enum EnumAlarmType
    {
        Alarm1,
        Alarm2
    }

    public enum EnumConnectState
    {
        Offline,
        OnlineRemote,
        OnlineLoacal
    }

    public enum EnumAutoState
    {
        Manual, //when OnlineRemote set Manual -> OnlineLoacl+Manual
        Auto//when offline set Auto -> OnlineRemote+Auto
    }

    public enum EnumLoginLevel
    {
        None,
        Op,
        Engineer,
        Admin,
        OneAboveAll
    }

    public enum EnumLogType
    {
        Debug,
        Info,
        Error,
        Comm
    }


}
