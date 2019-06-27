using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferCmds;
using TcpIpClientSample;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Control;

namespace Mirle.Agv.Model
{
    public class Vehicle
    {
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }

        private Battery battery;
        private VehLocation vehLoacation;
        private Dictionary<string, Carrier> dicCarriersById;
        private Dictionary<int, Carrier> dicCarriersByStageNum;
        private PlcRobot PlcRobot;
        private TransCmd transCmd;
        private LoggerAgent theLoggerAgent;

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
            battery = new Battery();
            vehLoacation = new VehLocation();
            dicCarriersById = new Dictionary<string, Carrier>();
            dicCarriersByStageNum = new Dictionary<int, Carrier>();
            transCmd = new EmptyTransCmd();
            CarrierID = "Empty";
            ObstVehicleID = "Empty";
            StoppedBlockID = "Empty";
            PlcRobot = new PlcRobot();
            theLoggerAgent = LoggerAgent.Instance;
        }

        #region Setter

        public void UpdateStatus(Battery battery)
        {
            this.battery = battery;
        }

        public void UpdateStatus(VehLocation vehLoacation)
        {
            this.vehLoacation = vehLoacation;
        }

        public void UpdateStatus(TransCmd transCmd)
        {
            this.transCmd = transCmd;
        }

        public void AddCarrier(Carrier aCarrier)
        {
            Carrier tempCarrier = aCarrier.Clone();
            dicCarriersById.Add(tempCarrier.Id, tempCarrier);
            dicCarriersByStageNum.Add(tempCarrier.StageNum, tempCarrier);
        }

        public void SetupBattery(BatteryConfigs batteryConfigs)
        {
            battery.SetupBattery(batteryConfigs);
        }

        public void SetVehicleStop()
        {

        }



        #endregion

        #region Getter

        public Battery GetBattery()
        {
            return this.battery;
        }

        public VehLocation GetVehLoacation()
        {
            return this.vehLoacation;
        }

        public TransCmd GetTransCmd()
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

        #endregion
    }
}
