using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Controller;
using System.Reflection;
using System.Collections.Concurrent;

namespace Mirle.Agv.AseMiddler.Model
{

    public class Vehicle
    {
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }
        public ConcurrentDictionary<string, AgvcTransCmd> AgvcTransCmdBuffer { get; set; } = new ConcurrentDictionary<string, AgvcTransCmd>();
        public EnumAutoState AutoState { get; set; } = EnumAutoState.Manual;
        public string SoftwareVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public bool IsAgvcConnect { get; set; } = false;
        public EnumLoginLevel LoginLevel { get; set; } = EnumLoginLevel.Op;
        public EnumChargingStage ChargingStage { get; set; } = EnumChargingStage.Idle;
        public MapInfo Mapinfo { get; private set; } = new MapInfo();

        #region AsePackage

        public bool IsLocalConnect { get; set; } = false;
        public AseMoveStatus AseMoveStatus { get; set; } = new AseMoveStatus();
        public AseRobotStatus AseRobotStatus { get; set; } = new AseRobotStatus();
        public AseCarrierSlotStatus AseCarrierSlotL { get; set; } = new AseCarrierSlotStatus();
        public AseCarrierSlotStatus AseCarrierSlotR { get; set; } = new AseCarrierSlotStatus(EnumSlotNumber.R);
        public bool IsCharging { get; set; } = false;
        public AseBatteryStatus AseBatteryStatus { get; set; } = new AseBatteryStatus();
        public AseMovingGuide AseMovingGuide { get; set; } = new AseMovingGuide();
        public string PspSpecVersion { get; set; } = "1.0";

        public bool IsReAuto { get; set; } = true;
        public bool CheckStartChargeReplyEnd { get; set; } = true;
        public bool CheckStopChargeReplyEnd { get; set; } = true;

        #endregion

        #region Comm Property
        //public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
        public VhStopSingle BlockingStatus { get; set; } = VhStopSingle.Off;
        public VhChargeStatus ChargeStatus { get; set; } = VhChargeStatus.ChargeStatusNone;
        public DriveDirction DrivingDirection { get; set; } = DriveDirction.DriveDirNone;
        public VhStopSingle ObstacleStatus { get; set; } = VhStopSingle.Off;
        public int ObstDistance { get; set; }
        public string ObstVehicleID { get; set; } = "";
        public VhPowerStatus PowerStatus { get; set; } = VhPowerStatus.PowerOn;
        public string StoppedBlockID { get; set; } = "";
        public VhStopSingle ErrorStatus { get; set; } = VhStopSingle.Off;
        public uint CmdPowerConsume { get; set; }
        public int CmdDistance { get; set; }
        public string TeachingFromAddress { get; internal set; } = "";
        public string TeachingToAddress { get; internal set; } = "";
        public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
        public bool IsOptimize { get; internal set; }
        public BCRReadResult LeftReadResult { get; set; } = BCRReadResult.BcrReadFail;
        public BCRReadResult RightReadResult { get; set; } = BCRReadResult.BcrReadFail;
        public VhStopSingle OpPauseStatus { get; set; } = VhStopSingle.Off;
        public ConcurrentDictionary<PauseType, bool> PauseFlags = new ConcurrentDictionary<PauseType, bool>(Enum.GetValues(typeof(PauseType)).Cast<PauseType>().ToDictionary(x => x, x => false));

        #endregion

        #region Configs

        //Main Configs

        public MainFlowConfig MainFlowConfig { get; set; } = new MainFlowConfig();
        public AgvcConnectorConfig AgvcConnectorConfig { get; set; } = new AgvcConnectorConfig();
        public MapConfig MapConfig { get; set; } = new MapConfig();
        public AlarmConfig AlarmConfig { get; set; } = new AlarmConfig();
        public BatteryLog BatteryLog { get; set; } = new BatteryLog();

        // AsePackage Configs

        public AsePackageConfig AsePackageConfig { get; set; } = new AsePackageConfig();
        public AseBatteryConfig AseBatteryConfig { get; set; } = new AseBatteryConfig();
        public AseMoveConfig AseMoveConfig { get; set; } = new AseMoveConfig();
        public PspConnectionConfig PspConnectionConfig { get; set; } = new PspConnectionConfig();

        #endregion

        private Vehicle() { }

        public AseCarrierSlotStatus GetAseCarrierSlotStatus(EnumSlotNumber slotNumber)
        {
            switch (slotNumber)
            {
                case EnumSlotNumber.R:
                    return theVehicle.AseCarrierSlotR;
                case EnumSlotNumber.L:
                default:
                    return theVehicle.AseCarrierSlotL;
            }
        }

        public bool IsPause()
        {
            return PauseFlags.Values.Any(x => x);
        }
    }
}
