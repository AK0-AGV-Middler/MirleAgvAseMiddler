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
    [Serializable]
    public class Vehicle
    {
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }
        public ConcurrentDictionary<string, AgvcTransCmd> AgvcTransCmdBuffer { get; set; } = new ConcurrentDictionary<string, AgvcTransCmd>();
        public EnumAutoState AutoState { get; set; } = EnumAutoState.Manual;
        public bool IsSimulation { get; set; } = false;
        public string SoftwareVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public bool IsAgvcConnect { get; set; } = false;

        #region AsePackage

        public AseMoveStatus AseMoveStatus { get; set; } = new AseMoveStatus();
        public AseRobotStatus AseRobotStatus { get; set; } = new AseRobotStatus();
        public AseCarrierSlotStatus AseCarrierSlotL { get; set; } = new AseCarrierSlotStatus();
        public AseCarrierSlotStatus AseCarrierSlotR { get; set; } = new AseCarrierSlotStatus(EnumSlotNumber.R);
        public bool IsCharging { get; set; } = false;
        public AseBatteryStatus AseBatteryStatus { get; set; } = new AseBatteryStatus();
        public double AutoChargeLowThreshold { get; set; } = 50;
        public double AutoChargeHighThreshold { get; set; } = 90;
        public AseMovingGuide AseMovingGuide { get; set; } = new AseMovingGuide();
        public string PspSpecVersion { get; set; } = "1.0";

        public bool IsReAuto { get; set; } = true;
        #endregion

        #region Comm Property
        //public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
        public VhStopSingle BlockingStatus { get; set; }
        public VhChargeStatus ChargeStatus { get; set; }
        public DriveDirction DrivingDirection { get; set; }
        public VhStopSingle ObstacleStatus { get; set; } = VhStopSingle.Off;
        public int ObstDistance { get; set; }
        public string ObstVehicleID { get; set; } = "";
        public VhPowerStatus PowerStatus { get; set; }
        public string StoppedBlockID { get; set; } = "";
        public VhStopSingle ErrorStatus { get; set; }
        public uint CmdPowerConsume { get; set; }
        public int CmdDistance { get; set; }
        public string TeachingFromAddress { get; internal set; } = "";
        public string TeachingToAddress { get; internal set; } = "";
        public VHActionStatus ActionStatus { get;  set; }
        public bool IsOptimize { get; internal set; }
        public BCRReadResult LeftReadResult { get; set; } = BCRReadResult.BcrReadFail;
        public BCRReadResult RightReadResult { get; set; } = BCRReadResult.BcrReadFail;

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
    }
}
