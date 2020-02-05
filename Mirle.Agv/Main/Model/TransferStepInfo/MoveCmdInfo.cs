using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model.Configs;

namespace Mirle.Agv.Model.TransferSteps
{
    [Serializable]
    public class MoveCmdInfo : TransferStep
    {
        public List<MapPosition> AddressPositions { get; set; } = new List<MapPosition>();
        public List<EnumAddressAction> AddressActions { get; set; } = new List<EnumAddressAction>();
        public List<double> SectionSpeedLimits { get; set; } = new List<double>();
        public List<string> SectionIds { get; set; } = new List<string>();
        public List<string> AddressIds { get; set; } = new List<string>();
        public List<MapSection> MovingSections { get; set; } = new List<MapSection>();
        public int MovingSectionsIndex { get; set; } = 0;
        public MapAddress StartAddress { get; set; } = new MapAddress();
        public MapAddress EndAddress { get; set; } = new MapAddress();
        protected int HalfR2000Radius { get; set; } = 1000;
        public string Info { get; set; } = "";
        public bool IsDuelStartPosition { get; set; } = false;
        public bool IsLoadPortToUnloadPort { get; set; } = false;
        public EnumStageDirection StageDirection { get; set; } = EnumStageDirection.None;
        public bool IsMoveEndDoLoadUnload { get; set; } = false;

        public List<MapAddress> MovingAddress = new List<MapAddress>();

        public MoveCmdInfo() : this(new MainFlowHandler()) { }
        public MoveCmdInfo(MainFlowHandler mainFlowHandler) : base(mainFlowHandler)
        {
            type = EnumTransferStepType.Move;
        }

        public void FilterUselessFirstSection()
        {
            try
            {
                VehicleLocation vehicleLocation = theVehicle.VehicleLocation;
                if (AddressIds.Count > 1)
                {
                    if (vehicleLocation.LastAddress.Id == AddressIds[1] && mainFlowHandler.IsPositionInThisAddress(vehicleLocation.RealPosition, vehicleLocation.LastAddress.Position))
                    {
                        SectionIds.RemoveAt(0);
                        AddressIds.RemoveAt(0);
                        IsDuelStartPosition = true;
                        mainFlowHandler.LogDuel();
                    }
                }

            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.Log("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }


        public void FilterUselessNextToLoadFirstSection()
        {
            try
            {
                VehicleLocation vehicleLocation = theVehicle.VehicleLocation;
                if (AddressIds.Count > 1)
                {
                    if (StartAddress.Id == AddressIds[1])
                    {
                        SectionIds.RemoveAt(0);
                        AddressIds.RemoveAt(0);
                        IsDuelStartPosition = true;
                        mainFlowHandler.LogDuel();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.Log("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void SetupStartAddress()
        {
            try
            {
                VehicleLocation vehicleLocation = theVehicle.VehicleLocation;

                if (mainFlowHandler.IsPositionInThisAddress(vehicleLocation.RealPosition, vehicleLocation.LastAddress.Position))
                {
                    StartAddress = vehicleLocation.LastAddress;
                }
                else
                {
                    StartAddress = new MapAddress();
                    StartAddress.Id = "StartAddress";
                    switch (vehicleLocation.LastSection.Type)
                    {                        
                        case EnumSectionType.Horizontal:
                            StartAddress.Position = new MapPosition(vehicleLocation.RealPosition.X, vehicleLocation.LastAddress.Position.Y);
                            break;
                        case EnumSectionType.Vertical:
                            StartAddress.Position = new MapPosition(vehicleLocation.LastAddress.Position.X, vehicleLocation.RealPosition.Y);
                            break;
                        case EnumSectionType.R2000:                         
                        case EnumSectionType.None:                           
                        default:
                            StartAddress.Position = vehicleLocation.RealPosition;
                            break;
                    }
                    
                    StartAddress.AddressOffset = new MapAddressOffset();
                    StartAddress.VehicleHeadAngle = (vehicleLocation.LastSection.HeadAddress.VehicleHeadAngle + vehicleLocation.LastSection.TailAddress.VehicleHeadAngle) / 2;
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.Log("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void SetupMovingSectionsAndAddresses()
        {
            MovingSections = new List<MapSection>();
            if (SectionIds.Count > 0)
            {
                for (int i = 0; i < SectionIds.Count; i++)
                {
                    MapSection mapSection = theMapInfo.allMapSections[SectionIds[i]].DeepClone();                    
                    mapSection.CmdDirection = (mapSection.HeadAddress.Id == AddressIds[i]) ? EnumPermitDirection.Forward : EnumPermitDirection.Backward;
                    MovingSections.Add(mapSection);
                }
               
                var endSection = MovingSections[MovingSections.Count - 1];
                if (endSection.CmdDirection == EnumPermitDirection.Forward)
                {
                    endSection.TailAddress = EndAddress;
                }
                else
                {
                    endSection.HeadAddress = EndAddress;
                }
            }

            SetupMovingAddress();
        }

        public void SetupMovingAddress()
        {
            MovingAddress = new List<MapAddress>();           
            if (AddressIds.Count > 0)
            {
                MovingAddress.Add(StartAddress);
                for (int i = 1; i < AddressIds.Count-1; i++)
                {
                    MapAddress mapAddress = theMapInfo.allMapAddresses[AddressIds[i]];
                    MovingAddress.Add(mapAddress);
                }              
                MovingAddress.Add(EndAddress);
            }
        }

        public void SetupAddressPositions()
        {
            AddressPositions = new List<MapPosition>();

            try
            {
                #region version 1.0
                //if (MovingSections.Count > 0)
                //{
                //    //Setup first position inside MovingSections[0];
                //    var realPos = Vehicle.Instance.VehicleLocation.RealPosition;
                //    MapPosition firstPosition = new MapPosition(realPos.X, realPos.Y);

                //    switch (MovingSections[0].Type)
                //    {
                //        case EnumSectionType.None:
                //            break;
                //        case EnumSectionType.Horizontal:
                //            firstPosition.Y = MovingSections[0].HeadAddress.Position.Y;
                //            break;
                //        case EnumSectionType.Vertical:
                //            firstPosition.X = MovingSections[0].HeadAddress.Position.X;
                //            break;
                //        case EnumSectionType.R2000:
                //            firstPosition = theMapInfo.allMapAddresses[AddressIds[0]].Position;
                //            break;
                //        default:
                //            break;
                //    }

                //    AddressPositions.Add(firstPosition);

                //    for (int i = 0; i < MovingSections.Count - 1; i++)
                //    {
                //        MapAddress mapAddress = MovingSections[i].CmdDirection == EnumPermitDirection.Backward ? MovingSections[i].HeadAddress : MovingSections[i].TailAddress;
                //        var pos = mapAddress.Position;
                //        AddressPositions.Add(mapAddress.Position);
                //    }
                //}

                //var endPosition = theMapInfo.allMapAddresses[EndAddress.Id].Position;
                //AddressPositions.Add(endPosition);

                #endregion

                #region version 2.0
                foreach (var mapAddress in MovingAddress)
                {
                    AddressPositions.Add(mapAddress.Position);
                }
                #endregion
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.Log("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void SetupNextUnloadAddressPositions()
        {
            AddressPositions = new List<MapPosition>();
            try
            {
                if (MovingSections.Count > 0)
                {
                    var firstPosition = theMapInfo.allMapAddresses[StartAddress.Id].Position;
                    AddressPositions.Add(firstPosition);

                    for (int i = 0; i < MovingSections.Count - 1; i++)
                    {
                        MapAddress mapAddress = MovingSections[i].CmdDirection == EnumPermitDirection.Backward ? MovingSections[i].HeadAddress : MovingSections[i].TailAddress;
                        AddressPositions.Add(mapAddress.Position);
                    }
                }

                var endPosition = theMapInfo.allMapAddresses[EndAddress.Id].Position;
                AddressPositions.Add(endPosition);
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.Log("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void SetupSectionSpeedLimits()
        {
            SectionSpeedLimits = new List<double>();
            try
            {
                if (MovingSections.Count > 0)
                {
                    for (int i = 0; i < MovingSections.Count; i++)
                    {
                        MapSection mapSection = theMapInfo.allMapSections[MovingSections[i].Id];
                        double SpeedLimit = mapSection.Speed;
                        SectionSpeedLimits.Add(SpeedLimit);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.Log("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private bool IsClockwise(MapVector vecPreToCur, MapVector vecCurToNext)
        {
            if (vecPreToCur.DirY < 0 && vecCurToNext.DirX > 0)
            {
                return true;
            }
            else if (vecPreToCur.DirX > 0 && vecCurToNext.DirY > 0)
            {
                return true;
            }
            else if (vecPreToCur.DirY > 0 && vecCurToNext.DirX < 0)
            {
                return true;
            }
            else if (vecPreToCur.DirX < 0 && vecCurToNext.DirY < 0)
            {
                return true;
            }
            else
            {
                //CounterClockwise
                return false;
            }
        }

        public void SetupInfo()
        {
            try
            {
                Info = Environment.NewLine + "[AddressPositions=";
                foreach (var pos in AddressPositions)
                {
                    Info += $"({Convert.ToInt32(pos.X)},{Convert.ToInt32(pos.Y)})";
                }
                //Info += "]" + Environment.NewLine + "[AddressActions=";
                //foreach (var act in AddressActions)
                //{
                //    Info += $"({act})";
                //}
                Info += "]" + Environment.NewLine + "[SectionSpeedLimits=";
                foreach (var speed in SectionSpeedLimits)
                {
                    Info += $"({Convert.ToInt32(speed)})";
                }
                Info += "]" + Environment.NewLine + "[MovingAddress=";
                foreach (var addr in MovingAddress)
                {
                    Info += $"({addr.Id})";
                }
                Info += "]";

                //LoggerAgent.Instance.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", Info));

            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.Log("Error", new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }        
    }

    [Serializable]
    public class MoveToChargerCmdInfo : MoveCmdInfo
    {
        public MoveToChargerCmdInfo(MainFlowHandler mainFlowHandler) : base(mainFlowHandler)
        {
            type = EnumTransferStepType.MoveToCharger;
        }


    }
}
