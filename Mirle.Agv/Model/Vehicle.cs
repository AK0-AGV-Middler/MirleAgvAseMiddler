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
    public class Vehicle
    {
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }

        private VehLocation vehLoacation;
        private Dictionary<string, Carrier> dicCarriersById;
        private Dictionary<int, Carrier> dicCarriersByStageNum;
        private TransferStep transCmd;
        private LoggerAgent theLoggerAgent;
        private MapInfo theMapInfo = new MapInfo();
        private PlcVehicle plcVehicle;

        #region Comm Property

        public VHActionStatus ActionStatus { get; set; }
        public VhStopSingle BlockingStatus { get; set; }
        public VhChargeStatus ChargeStatus { get; set; }
        public DriveDirction DrivingDirection { get; set; }
        public VHModeStatus ModeStatus { get; set; }
        public VhStopSingle ObstacleStatus { get; set; }
        public string CarrierID { get; set; }
        public int ObstDistance { get; set; }
        public string ObstVehicleID { get; set; }
        public VhStopSingle PauseStatus { get; set; }
        public VhPowerStatus PowerStatus { get; set; }
        public VhStopSingle ReserveStatus { get; set; }
        public string StoppedBlockID { get; set; }
        public VhStopSingle ErrorStatus { get; set; }
        public ActiveType Cmd131ActType { get; set; }
        public CompleteStatus CompleteStatus { get; set; }
        public uint CmdPowerConsume { get; set; }
        public int CmdDistance { get; set; }
        public EventType Cmd134EventType { get; set; }
        public string CmdID { get; set; }
        public CMDCancelType Cmd137ActType { get; set; }
        public PauseEvent Cmd139EventType { get; set; }
        public VhLoadCSTStatus HasCst { get; set; }
        public string TeachingFromAddress { get; internal set; }
        public string TeachingToAddress { get; internal set; }

        #endregion

        private Vehicle()
        {
            vehLoacation = new VehLocation(theMapInfo);
            dicCarriersById = new Dictionary<string, Carrier>();
            dicCarriersByStageNum = new Dictionary<int, Carrier>();
            transCmd = new EmptyTransCmd();
            CarrierID = "Empty";
            ObstVehicleID = "Empty";
            StoppedBlockID = "Empty";
            plcVehicle = new PlcVehicle();
            theLoggerAgent = LoggerAgent.Instance;
        }

        #region Setter       

        public void UpdateStatus(VehLocation vehLoacation)
        {
            this.vehLoacation = vehLoacation;
        }

        public void UpdateStatus(TransferStep transCmd)
        {
            this.transCmd = transCmd;
        }

        public void AddCarrier(Carrier aCarrier)
        {
            Carrier tempCarrier = aCarrier.Clone();
            dicCarriersById.Add(tempCarrier.Id, tempCarrier);
            dicCarriersByStageNum.Add(tempCarrier.StageNum, tempCarrier);
        }

        

        public void SetVehicleStop()
        {

        }

        public void SetMapInfo(MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
        }

        public void SetPlcVehicle(PlcVehicle aPlcVehicle)
        {
            plcVehicle = aPlcVehicle;
        }

        #endregion

        #region Getter       

        public VehLocation GetVehLoacation()
        {
            return this.vehLoacation;
        }

        public TransferStep GetTransCmd()
        {
            return transCmd;
        }

        public Carrier GetCarrierById(string id)
        {
            try
            {
                if (dicCarriersById.ContainsKey(id))
                {
                    return dicCarriersById[id];
                }
                return new Carrier();
            }
            catch (Exception ex)
            {
                //log ex
                return new Carrier();
            }
        }

        public Carrier GetCarrierByStageNum(int stagenum)
        {
            try
            {
                if (dicCarriersByStageNum.ContainsKey(stagenum))
                {
                    return dicCarriersByStageNum[stagenum];
                }
                return new Carrier();
            }
            catch (Exception ex)
            {
                //log ex
                return new Carrier();
            }
        }

        public PlcVehicle GetPlcVehicle()
        {
            return plcVehicle;
        }
        #endregion

    }
}
