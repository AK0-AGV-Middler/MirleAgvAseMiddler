using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;
using TcpIpClientSample;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class Vehicle
    {
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }

        public TransferStep CurTrasferStep { get; set; } = new EmptyTransferStep();
        public MapInfo TheMapInfo { get; set; } = new MapInfo();
        public PlcVehicle ThePlcVehicle { get; private set; } = new PlcVehicle();
        private AgvcTransCmd curAgvcTransCmd;
        public AgvcTransCmd CurAgvcTransCmd
        {
            get
            {
                if (curAgvcTransCmd == null)
                {
                    return new AgvcTransCmd();
                }
                else
                {
                    return curAgvcTransCmd;
                }
            }
            set
            {
                curAgvcTransCmd = value;
            }
        }
        public AgvcTransCmd LastCurAgvcTransCmd { get; set; } = new AgvcTransCmd();
        public VehiclePosition CurVehiclePosition { get; set; } = new VehiclePosition();
        public EnumAutoState AutoState { get; set; } = EnumAutoState.Manual;
        public EnumThreadStatus VisitTransferStepsStatus { get; set; } = EnumThreadStatus.None;
        public EnumThreadStatus TrackPositionStatus { get; set; } = EnumThreadStatus.None;
        public EnumThreadStatus WatchLowPowerStatus { get; set; } = EnumThreadStatus.None;
        public EnumThreadStatus AskReserveStatus { get; set; } = EnumThreadStatus.None;

        #region Comm Property

        public VHActionStatus ActionStatus { get; set; }
        public VhStopSingle BlockingStatus { get; set; }
        public VhChargeStatus ChargeStatus { get; set; }
        public DriveDirction DrivingDirection { get; set; }
        public VHModeStatus ModeStatus { get; set; }
        public VhStopSingle ObstacleStatus { get; set; }
        public int ObstDistance { get; set; }
        public string ObstVehicleID { get; set; } = "Empty";
        public VhStopSingle PauseStatus { get; set; }
        public VhPowerStatus PowerStatus { get; set; }
        public VhStopSingle ReserveStatus { get; set; }
        public string StoppedBlockID { get; set; } = "Empty";
        public VhStopSingle ErrorStatus { get; set; }
        public ActiveType Cmd131ActType { get; set; }
        public CompleteStatus CompleteStatus { get; set; }
        public uint CmdPowerConsume { get; set; }
        public int CmdDistance { get; set; }
        public EventType Cmd134EventType { get; set; }
        public CMDCancelType Cmd137ActType { get; set; }
        public PauseEvent Cmd139EventType { get; set; }
        public string TeachingFromAddress { get; internal set; } = "Empty";
        public string TeachingToAddress { get; internal set; } = "Empty";

        #endregion

        private Vehicle()
        {
        }

        #region Getter

        public PlcVehicle GetPlcVehicle() { return ThePlcVehicle; }

        #endregion

    }
}
