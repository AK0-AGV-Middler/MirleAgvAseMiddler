using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;
using Mirle.Agv.Controller.Tools;

namespace Mirle.Agv.Model.TransferCmds
{
    [Serializable]
    public class MoveCmdInfo : TransferStep
    {
        public List<MapPosition> AddressPositions { get; set; } = new List<MapPosition>();
        public List<EnumAddressAction> AddressActions { get; set; } = new List<EnumAddressAction>();
        public List<double> SectionSpeedLimits { get; set; } = new List<double>();
        public int PredictVehicleAngle { get; set; } = 0;
        public List<string> SectionIds { get; set; } = new List<string>();
        public List<string> AddressIds { get; set; } = new List<string>();
        public List<MapSection> MovingSections { get; set; } = new List<MapSection>();
        public int MovingSectionsIndex { get; set; } = 0;
        public int FirstPositionRangeMm { get; set; } = 10;

        public MoveCmdInfo() : this(new MapInfo()) { }
        public MoveCmdInfo(MapInfo theMapInfo) : base(theMapInfo)
        {
            type = EnumTransferStepType.Move;
        }

        public void SetAddressPositions()
        {
            AddressPositions = new List<MapPosition>();
            var firstPosition = Vehicle.Instance.CurVehiclePosition.RealPosition;
            if (MovingSections.Count > 0)
            {
                //Setup first position inside MovingSections[0];
                switch (MovingSections[0].Type)
                {
                    case EnumSectionType.None:
                        break;
                    case EnumSectionType.Horizontal:
                        firstPosition.Y = MovingSections[0].HeadAddress.Position.Y;
                        break;
                    case EnumSectionType.Vertical:
                        firstPosition.X = MovingSections[0].HeadAddress.Position.X;
                        break;
                    case EnumSectionType.R2000:
                        firstPosition = theMapInfo.allMapAddresses[AddressIds[0]].Position;
                        break;
                    default:
                        break;
                }
            }          

            AddressPositions.Add(firstPosition);

            try
            {
                for (int i = 1; i < AddressIds.Count; i++)
                {
                    MapAddress mapAddress = theMapInfo.allMapAddresses[AddressIds[i]].DeepClone();
                    AddressPositions.Add(mapAddress.Position);
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public void SetNextUnloadAddressPositions()
        {
            AddressPositions = new List<MapPosition>();

            try
            {
                for (int i = 0; i < AddressIds.Count; i++)
                {
                    MapAddress mapAddress = theMapInfo.allMapAddresses[AddressIds[i]].DeepClone();
                    AddressPositions.Add(mapAddress.Position);
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public void SetSectionSpeedLimits()
        {
            SectionSpeedLimits = new List<double>();
            try
            {
                for (int i = 0; i < SectionIds.Count; i++)
                {
                    MapSection mapSection = theMapInfo.allMapSections[SectionIds[i]];
                    double SpeedLimit = mapSection.Speed;
                    SectionSpeedLimits.Add(SpeedLimit);
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public void SetAddressActions()
        {
            PredictVehicleAngle = theVehicle.CurVehiclePosition.PredictVehicleAngle;

            AddressActions = new List<EnumAddressAction>();
            try
            {
                SetupFirstAddressAction();
                MapSection firstSection = theMapInfo.allMapSections[SectionIds[0]];
                if (firstSection.Type == EnumSectionType.R2000)
                {
                    AddressActions.Add(EnumAddressAction.R2000);
                }
                else
                {
                    AddressActions.Add(EnumAddressAction.ST);
                }

                for (int i = 0; i < SectionIds.Count - 1; i++)
                {
                    MapSection currentSection = theMapInfo.allMapSections[SectionIds[i]];
                    MapSection nextSection = theMapInfo.allMapSections[SectionIds[i + 1]];
                    MapAddress curAddress = theMapInfo.allMapAddresses[AddressIds[i + 1]];
                    EnumAddressAction addressMotion = SetAddressMotion(currentSection, nextSection, curAddress);
                    AddressActions.Add(addressMotion);
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
            AddressActions.Add(EnumAddressAction.End);

            theVehicle.CurVehiclePosition.PredictVehicleAngle = PredictVehicleAngle;
        }

        private void SetupFirstAddressAction()
        {
            var firstPosition = AddressPositions[0];
            var secondPosition = AddressPositions[1];
        }

        public void SetMovingSections()
        {
            MovingSections = new List<MapSection>();
            for (int i = 0; i < SectionIds.Count; i++)
            {
                MapSection mapSection = new MapSection();
                try
                {
                    mapSection = theMapInfo.allMapSections[SectionIds[i]].DeepClone();
                    //TODO: Verify Cmd Direction
                    mapSection.CmdDirection = (mapSection.HeadAddress.Id == AddressIds[i]) ? EnumPermitDirection.Forward : EnumPermitDirection.Backward;
                }
                catch (Exception ex)
                {
                    LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , ex.StackTrace));
                }
                MovingSections.Add(mapSection);
            }
        }

        private EnumAddressAction SetAddressMotion(MapSection currentSection, MapSection nextSection, MapAddress curAddress)
        {
            if (nextSection.Type == EnumSectionType.R2000)
            {
                //水平接R2000 或是 垂直接R2000 是否不同
                return EnumAddressAction.R2000;
            }
            else if (currentSection.Type == EnumSectionType.R2000)
            {
                //R2000接水平 或是 R2000接垂直 是否不同
                return EnumAddressAction.ST;
            }
            else if (currentSection.Type == nextSection.Type)
            {
                //水平接水平 或 垂直接垂直
                return EnumAddressAction.ST;
            }
            else
            {
                //水平接垂直 或 垂直接水平
                if (IsTurnRight(currentSection, nextSection))
                {
                    //右轉
                    PredictVehicleAngle -= 90;
                    if (PredictVehicleAngle < -100)
                    {
                        PredictVehicleAngle = 0;

                        return curAddress.IsTR50 ? EnumAddressAction.BTR50 : EnumAddressAction.BTR350;
                    }
                    return curAddress.IsTR50 ? EnumAddressAction.TR50 : EnumAddressAction.TR350;
                }
                else
                {
                    //左轉
                    PredictVehicleAngle += 90;
                    if (PredictVehicleAngle > 100)
                    {
                        PredictVehicleAngle = 0;
                        return curAddress.IsTR50 ? EnumAddressAction.BTR50 : EnumAddressAction.BTR350;
                    }
                    return curAddress.IsTR50 ? EnumAddressAction.TR50 : EnumAddressAction.TR350;
                }
            }
        }

        private bool IsTurnRight(MapSection currentSection, MapSection nextSection)
        {
            MapPosition curSectionMid = new MapPosition((currentSection.HeadAddress.Position.X + currentSection.TailAddress.Position.X) / 2,
                (currentSection.HeadAddress.Position.Y + currentSection.TailAddress.Position.Y) / 2);
            MapPosition nextSectionMid = new MapPosition((nextSection.HeadAddress.Position.X + nextSection.TailAddress.Position.X) / 2,
                (nextSection.HeadAddress.Position.Y + nextSection.TailAddress.Position.Y) / 2);

            if (currentSection.Type == EnumSectionType.Horizontal)
            {
                //水平接垂直
                if (curSectionMid.X < nextSectionMid.X)
                {
                    //W > XXX
                    if (curSectionMid.Y < nextSectionMid.Y)
                    {
                        //W > S
                        return true;
                    }
                    else
                    {
                        //W > N
                        return false;
                    }
                }
                else
                {
                    //E > XXX
                    if (curSectionMid.Y < nextSectionMid.Y)
                    {
                        //E > S
                        return false;
                    }
                    else
                    {
                        //E > N
                        return true;
                    }
                }
            }
            else
            {
                //垂直接水平
                if (curSectionMid.X < nextSectionMid.X)
                {
                    //XX > E
                    if (curSectionMid.Y < nextSectionMid.Y)
                    {
                        //N > E
                        return false;
                    }
                    else
                    {
                        //S > E
                        return true;
                    }
                }
                else
                {
                    //XX > W
                    if (curSectionMid.Y < nextSectionMid.Y)
                    {
                        //N > W
                        return true;
                    }
                    else
                    {
                        //S > W
                        return false;
                    }
                }
            }

        }
    }
}
