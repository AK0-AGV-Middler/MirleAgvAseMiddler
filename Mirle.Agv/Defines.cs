using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        Unload,
        Empty
    }

    public enum EnumAgvcTransCmdType
    {
        Move,
        Load,
        Unload,
        LoadUnload,
        Home,
        Override,
        Else
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

    public enum EnumDisplayLevel
    {
        Lowest,
        Low,
        Normal,
        High,
        Highest
    }

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
