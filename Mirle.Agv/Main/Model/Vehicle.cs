using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Model.TransferSteps;
using TcpIpClientSample;
using Mirle.AgvAseMiddler.Model.Configs;
using Mirle.AgvAseMiddler.Controller;
using System.Reflection;

namespace Mirle.AgvAseMiddler.Model
{
    [Serializable]
    public class Vehicle
    {
        public MainFlowConfig TheMainFlowConfig { get; set; } = new MainFlowConfig();
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }

        public VehicleIntegrateStatus TheVehicleIntegrateStatus { get; protected set; } = null;
        public AgvcTransCmd CurAgvcTransCmd { get; set; } = new AgvcTransCmd();
        public VehicleLocation VehicleLocation { get; set; } = new VehicleLocation();
        private EnumAutoState autoState = EnumAutoState.Manual;
        public EnumAutoState AutoState
        {
            get { return autoState; }
            set
            {
                if (value != autoState)
                {
                    autoState = value;
                    if (value == EnumAutoState.Auto)
                    {
                        ModeStatus = VHModeStatus.AutoRemote;
                    }
                    else
                    {
                        ModeStatus = VHModeStatus.Manual;
                    }
                    if (value != EnumAutoState.PreManual)
                    {
                        OnAutoStateChangeEvent?.Invoke(this, MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
        }
        public event EventHandler<string> OnAutoStateChangeEvent;

        #region AsePackage

        public AseMoveStatus AseMoveStatus { get; set; } = new AseMoveStatus();
        public AseRobotStatus AseRobotStatus { get; set; } = new AseRobotStatus();
        public AseCarrierSlotStatus AseCarrierSlotA { get; set; } = new AseCarrierSlotStatus();
        public AseCarrierSlotStatus AseCarrierSlotB { get; set; } = new AseCarrierSlotStatus();
        public bool IsCharging { get; set; } = false;
        public AseBatteryStatus AseBatteryStatus { get; set; } = new AseBatteryStatus();
        public double AutoChargeLowThreshold { get; set; } = 50;
        public double AutoChargeHighThreshold { get; set; } = 90;

        #endregion

        public EnumThreadStatus VisitTransferStepsStatus { get; set; } = EnumThreadStatus.None;
        public EnumThreadStatus TrackPositionStatus { get; set; } = EnumThreadStatus.None;
        public EnumThreadStatus WatchLowPowerStatus { get; set; } = EnumThreadStatus.None;
        public EnumThreadStatus AskReserveStatus { get; set; } = EnumThreadStatus.None;

        #region Comm Property
        public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
        public VhStopSingle BlockingStatus { get; set; }
        public VhChargeStatus ChargeStatus { get; set; }
        public DriveDirction DrivingDirection { get; set; }
        public VHModeStatus ModeStatus { get; set; } = VHModeStatus.Manual;
        public VhStopSingle ObstacleStatus { get; set; } = VhStopSingle.StopSingleOff;
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

    }
}
