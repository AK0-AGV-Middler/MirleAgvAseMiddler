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

        private VehLocation vehLoacation;
        private TransferStep transCmd = new EmptyTransCmd();
        private LoggerAgent theLoggerAgent = LoggerAgent.Instance;
        private MapInfo theMapInfo = new MapInfo();
        private PlcVehicle plcVehicle = new PlcVehicle();
        private AgvcTransCmd agvcTransCmd = new AgvcTransCmd();

        public PlcVehicle someTestVeh { get; private set; }

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
            vehLoacation = new VehLocation(theMapInfo);
        }

        #region Setter       

        public void UpdateStatus(VehLocation vehLoacation) { this.vehLoacation = vehLoacation; }

        public void UpdateStatus(TransferStep transCmd) { this.transCmd = transCmd; }

        public void SetVehicleStop() { }

        public void SetMapInfo(MapInfo theMapInfo) { this.theMapInfo = theMapInfo; }

        public void SetPlcVehicle(PlcVehicle plcVehicle) { this.plcVehicle = plcVehicle; }

        public void SetAgvcTransCmd(AgvcTransCmd agvcTransCmd) { this.agvcTransCmd = agvcTransCmd; }

        #endregion

        #region Getter       

        public VehLocation GetVehLoacation() { return vehLoacation; }

        public TransferStep GetTransCmd() { return transCmd; }

        public PlcVehicle GetPlcVehicle() { return plcVehicle; }

        public AgvcTransCmd GetAgvcTransCmd() { return agvcTransCmd; }

        #endregion

    }
}
