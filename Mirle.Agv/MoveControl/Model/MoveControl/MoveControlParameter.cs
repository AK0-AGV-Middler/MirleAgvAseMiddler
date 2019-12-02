using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MoveControlParameter
    {
        public bool MoveControlCommandMoving { get; set; }
        public bool MoveControlCommandComplete { get; set; }
        public bool ResetMoveControlCommandMoving { get; set; }
        public System.Diagnostics.Stopwatch MoveControlCommandCompleteTimer { get; set; }

        public bool StopWithoutReason { get; set; }
        public System.Diagnostics.Stopwatch StopWithoutReasonTimer { get; set; }


        public bool R2000Stop { get; set; }
        public System.Diagnostics.Stopwatch R2000StopTimer { get; set; }
        public bool DirFlag { get; set; }
        public bool PositionDirFlag { get; set; }
        public int WheelAngle { get; set; }
        public bool MoveControlStop { get; set; }
        public double VelocityCommand { get; set; }
        public double RealVelocity { get; set; }
        public Thread MoveControlThread { get; set; }
        public bool OntimeReviseFlag { get; set; }
        public double TrigetEndEncoder { get; set; }
        public bool FlowStopRequeset { get; set; }
        public bool FlowClear { get; set; }
        public int WaitReserveIndex { get; set; }
        public EnumAddressAction TurnType { get; set; }
        public double TurnStartEncoder { get; set; }
        public EnumVehicleSafetyAction SensorState { get; set; }
        public EnumVehicleSafetyAction BeamSensorState { get; set; }
        public EnumVehicleSafetyAction BumpSensorState { get; set; }

        public bool MoveStartNoWaitTime { get; set; }

        public bool CommandMoving { get; set; }
        public bool PauseRequest { get; set; }
        public bool PauseAlready { get; set; }
        public bool PauseNotSendEvent { get; set; }
        public bool CancelRequest { get; set; }
        public bool CanPause { get; set; }
        public bool CancelNotSendEvent { get; set; }
        public bool SecondCorrection { get; set; }
        public EnumVChangeSpeedLowerSafety VChangeSafetyType { get; set; }
        public double VChangeSafetyTargetEncoder { get; set; }
        public double VChangeSafetyVelocity { get; set; }

        public double SectionDeviationOffset { get; set; }
        public bool EQVChange { get; set; }

        public EnumVehicleSafetyAction KeepsLowSpeedStateByEQVChange { get; set; }

        public bool CloseMoveControl { get; set; }

        public MoveControlParameter()
        {
            R2000Stop = false;
            R2000StopTimer = new System.Diagnostics.Stopwatch();
            StopWithoutReason = false;
            ResetMoveControlCommandMoving = false;
            MoveControlCommandMoving = false;
            MoveControlCommandComplete = false;
            MoveControlCommandCompleteTimer = new System.Diagnostics.Stopwatch();
            StopWithoutReasonTimer = new System.Diagnostics.Stopwatch();
            KeepsLowSpeedStateByEQVChange = EnumVehicleSafetyAction.Stop;
            VChangeSafetyType = EnumVChangeSpeedLowerSafety.None;
            CanPause = true;
            DirFlag = true;
            PositionDirFlag = true;
            WheelAngle = 0;
            MoveControlStop = false;
            VelocityCommand = 100;
            MoveStartNoWaitTime = false;
            OntimeReviseFlag = false;
            FlowStopRequeset = false;
            FlowClear = false;
            WaitReserveIndex = -1;
            CommandMoving = false;
            EQVChange = false;
            CloseMoveControl = false;
            SensorState = EnumVehicleSafetyAction.Normal;
            BeamSensorState = EnumVehicleSafetyAction.Normal;
            BumpSensorState = EnumVehicleSafetyAction.Normal;
        }
    }
}
