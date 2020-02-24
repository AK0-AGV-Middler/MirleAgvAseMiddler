using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Model.TransferSteps;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Mirle.AgvAseMiddler.Model.Configs;
using Mirle.AgvAseMiddler.Controller;
using System.Reflection;

namespace Mirle.AgvAseMiddler.Model
{
    [Serializable]
    public class Vehicle
    {
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }
        public Dictionary<string, AgvcTransCmd> AgvcTransCmdBuffer { get; set; } = new Dictionary<string, AgvcTransCmd>();
        private EnumAutoState autoState = EnumAutoState.Manual;
        public EnumAutoState AutoState
        {
            get { return autoState; }
            set
            {
                if (value != autoState)
                {
                    autoState = value;                    
                    if (value != EnumAutoState.PreManual)
                    {
                        OnAutoStateChangeEvent?.Invoke(this, value);
                    }
                }
            }
        }
        public event EventHandler<EnumAutoState> OnAutoStateChangeEvent;
        public bool IsSimulation { get; set; } = false;

        #region AsePackage

        public AseMoveStatus AseMoveStatus { get; set; } = new AseMoveStatus();
        public AseRobotStatus AseRobotStatus { get; set; } = new AseRobotStatus();
        public AseCarrierSlotStatus AseCarrierSlotA { get; set; } = new AseCarrierSlotStatus();
        public AseCarrierSlotStatus AseCarrierSlotB { get; set; } = new AseCarrierSlotStatus(EnumSlotNumber.B);
        public bool IsCharging { get; set; } = false;
        public AseBatteryStatus AseBatteryStatus { get; set; } = new AseBatteryStatus();
        public double AutoChargeLowThreshold { get; set; } = 50;
        public double AutoChargeHighThreshold { get; set; } = 90;
        public AseMovingGuide AseMovingGuide { get; set; } = new AseMovingGuide();

        #endregion


        #region Comm Property
        public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
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
        #endregion

        private Vehicle() { }             

        public AseCarrierSlotStatus GetAseCarrierSlotStatus(EnumSlotNumber slotNumber)
        {
            switch (slotNumber)
            {
                case EnumSlotNumber.B:
                    return theVehicle.AseCarrierSlotB;
                case EnumSlotNumber.A:                   
                default:
                    return theVehicle.AseCarrierSlotA;
            }
        }
    }
}
