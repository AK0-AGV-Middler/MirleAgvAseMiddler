using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model.Configs;

namespace Mirle.Agv.Model.TransferCmds
{
    [Serializable]
    public class MoveCmdInfo : TransferStep
    {
        public List<MapPosition> AddressPositions { get; set; } = new List<MapPosition>();
        public List<EnumAddressAction> AddressActions { get; set; } = new List<EnumAddressAction>();
        public List<double> SectionSpeedLimits { get; set; } = new List<double>();
        public int VehicleHeadAngle { get; set; } = 0;
        public int WheelAngle { get; set; } = 0;
        public List<string> SectionIds { get; set; } = new List<string>();
        public List<string> AddressIds { get; set; } = new List<string>();
        public List<MapSection> MovingSections { get; set; } = new List<MapSection>();
        public int MovingSectionsIndex { get; set; } = 0;
        public ushort SeqNum { get; set; } = 0;
        public string EndAddressId { get; set; } = "";
        public string StartAddressId { get; set; } = "";
        private int HalfR2000Radius { get; set; } = 1000;

        public MoveCmdInfo() : this(new MainFlowHandler()) { }
        public MoveCmdInfo(MainFlowHandler mainFlowHandler) : base(mainFlowHandler)
        {
            type = EnumTransferStepType.Move;
        }

        public void SetupMovingSections()
        {
            MovingSections = new List<MapSection>();
            for (int i = 0; i < SectionIds.Count; i++)
            {
                MapSection mapSection = new MapSection();
                try
                {
                    if (!theMapInfo.allMapSections.ContainsKey(SectionIds[i]))
                    {
                        var msg = $"[Section({SectionIds[i]}) from Agvc is not in MapInfo]";
                        middleAgent.Send_Cmd131_TransferResponse(SeqNum, 1, msg);
                        msg = $"MovcCmdInfo : Setup Moving Sections +++FAIL+++,  " + msg;
                        LoggerAgent.Instance.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", msg));
                        mainFlowHandler.StopVisitTransferSteps();
                        return;
                    }
                    mapSection = theMapInfo.allMapSections[SectionIds[i]].DeepClone();
                    mapSection.CmdDirection = (mapSection.HeadAddress.Id == AddressIds[i]) ? EnumPermitDirection.Forward : EnumPermitDirection.Backward;
                }
                catch (Exception ex)
                {
                    LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                }
                MovingSections.Add(mapSection);
            }

            RebuildLastSectionForInsideEndAddress();
        }

        public void SetupAddressPositions()
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
                for (int i = 0; i < MovingSections.Count; i++)
                {
                    MapAddress mapAddress = MovingSections[i].CmdDirection == EnumPermitDirection.Backward ? MovingSections[i].HeadAddress : MovingSections[i].TailAddress;
                    AddressPositions.Add(mapAddress.Position.DeepClone());
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void SetupNextUnloadAddressPositions()
        {
            AddressPositions = new List<MapPosition>();
            if (!theMapInfo.allMapAddresses.ContainsKey(StartAddressId))
            {
                var msg = $"[Address({StartAddressId}) from Agvc is not in MapInfo]";
                middleAgent.Send_Cmd131_TransferResponse(SeqNum, 1, msg);
                msg = $"MovcCmdInfo : Setup Next Unload Address Positions +++FAIL+++,  " + msg;
                LoggerAgent.Instance.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", msg));
                mainFlowHandler.StopVisitTransferSteps();
                return;
            }
            var firstPosition = theMapInfo.allMapAddresses[StartAddressId].Position;
            AddressPositions.Add(firstPosition);
            try
            {
                for (int i = 0; i < MovingSections.Count; i++)
                {
                    MapAddress mapAddress = MovingSections[i].CmdDirection == EnumPermitDirection.Backward ? MovingSections[i].HeadAddress : MovingSections[i].TailAddress;
                    AddressPositions.Add(mapAddress.Position.DeepClone());
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void SetupSectionSpeedLimits()
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
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void SetupAddressActions()
        {
            //PreMethod = SetupAddressPosition

            VehicleHeadAngle = (int)theVehicle.CurVehiclePosition.VehicleAngle; //車頭方向角度(0,90,180,-90)            
            WheelAngle = theVehicle.CurVehiclePosition.WheelAngle;

            AddressActions = new List<EnumAddressAction>();
            try
            {
                if (AddressPositions.Count > 1)
                {
                    AddFirstAction();

                    for (int i = 1; i < AddressPositions.Count - 1; i++)
                    {
                        MapPosition curPosition = AddressPositions[i];
                        MapPosition prePosition = AddressPositions[i - 1];
                        MapPosition nextPosition = AddressPositions[i + 1];
                        EnumAddressAction addressAction = SetupAddressAction(prePosition, curPosition, nextPosition, theMapInfo.allMapAddresses[AddressIds[i]]);
                        AddressActions.Add(addressAction);
                    }

                    //for (int i = 0; i < SectionIds.Count - 1; i++)
                    //{
                    //    MapSection currentSection = theMapInfo.allMapSections[SectionIds[i]];
                    //    MapSection nextSection = theMapInfo.allMapSections[SectionIds[i + 1]];
                    //    MapAddress curAddress = theMapInfo.allMapAddresses[AddressIds[i + 1]];
                    //    EnumAddressAction addressMotion = SetAddressMotion(currentSection, nextSection, curAddress);
                    //    AddressActions.Add(addressMotion);
                    //}
                }
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
            AddressActions.Add(EnumAddressAction.End);

            theVehicle.CurVehiclePosition.WheelAngle = WheelAngle;
        }

        private EnumAddressAction SetupAddressAction(MapPosition prePosition, MapPosition curPosition, MapPosition nextPosition, MapAddress curAddress)
        {
            MapVector vecPreToCur = new MapVector(curPosition.X - prePosition.X, curPosition.Y - prePosition.Y);
            MapVector vecCurToNext = new MapVector(nextPosition.X - curPosition.X, nextPosition.Y - curPosition.Y);


            if (Math.Abs(vecPreToCur.DirX) <= mapConfig.AddressAreaMm && Math.Abs(vecCurToNext.DirX) <= mapConfig.AddressAreaMm)
            {
                //三點在同一垂直線 => (ST)
                return EnumAddressAction.ST;
            }

            if (Math.Abs(vecPreToCur.DirY) <= mapConfig.AddressAreaMm && Math.Abs(vecCurToNext.DirY) <= mapConfig.AddressAreaMm)
            {
                //三點在同一水平線 => (ST)
                return EnumAddressAction.ST;
            }

            if (Math.Abs(vecPreToCur.DirX) > HalfR2000Radius && Math.Abs(vecPreToCur.DirY) > HalfR2000Radius)
            {
                //這三點中第一段路線是R2000
                if (Math.Abs(vecCurToNext.DirX) <= mapConfig.AddressAreaMm)
                {
                    //第二段路線是垂直 ST/BST
                    if (vecCurToNext.DirY * vecPreToCur.DirY > 0)
                    {
                        //兩段路線Y分量同向
                        return EnumAddressAction.ST;
                    }
                    else
                    {
                        //兩段路線Y分量反向
                        return EnumAddressAction.BST;
                    }
                }
                else if (Math.Abs(vecCurToNext.DirY) <= mapConfig.AddressAreaMm)
                {
                    //第二段路線是水平
                    if (vecCurToNext.DirX * vecPreToCur.DirX > 0)
                    {
                        //兩段路線X分量同向
                        return EnumAddressAction.ST;
                    }
                    else
                    {
                        //兩段路線X分量反向
                        return EnumAddressAction.BST;
                    }
                }
                else
                {
                    //第二段路線是R2000
                    var msg = $"[Cannot do R2000-R2000]";
                    middleAgent.Send_Cmd131_TransferResponse(SeqNum, 1, msg);
                    msg = $"MovcCmdInfo : Setup Address Action +++FAIL+++,  " + msg;
                    LoggerAgent.Instance.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", msg));
                    mainFlowHandler.StopVisitTransferSteps();
                    return EnumAddressAction.R2000;
                }
            }
            else if (Math.Abs(vecPreToCur.DirX) <= mapConfig.AddressAreaMm)
            {
                //這三點中第一段路線是垂直
                if (Math.Abs(vecCurToNext.DirX) > HalfR2000Radius && Math.Abs(vecCurToNext.DirY) > HalfR2000Radius)
                {
                    //第二段路線是R2000 R2000/BR2000
                    if (vecPreToCur.DirY * vecCurToNext.DirY > 0)
                    {
                        //兩段路線Y分量同向
                        return EnumAddressAction.R2000;
                    }
                    else
                    {
                        //兩段路線Y分量反向
                        return EnumAddressAction.BR2000;
                    }
                }
                else
                {
                    //第二段路線是水平 TR/BTR
                    if (vecPreToCur.DirY * vecCurToNext.DirX > 0)
                    {
                        //逆時針轉                
                        WheelAngle += 90;
                        if (WheelAngle > 100)
                        {
                            WheelAngle = 0;
                            return curAddress.IsTR50 ? EnumAddressAction.BTR50 : EnumAddressAction.BTR350;                           
                        }
                        else
                        {
                            return curAddress.IsTR50 ? EnumAddressAction.TR50 : EnumAddressAction.TR350;
                        }                        
                    }
                    else
                    {
                        //順時針轉
                        WheelAngle += -90;
                        if (WheelAngle < -100)
                        {
                            WheelAngle = 0;
                            return curAddress.IsTR50 ? EnumAddressAction.BTR50 : EnumAddressAction.BTR350;
                        }
                        else
                        {
                            return curAddress.IsTR50 ? EnumAddressAction.TR50 : EnumAddressAction.TR350;
                        }
                    }
                }
            }
            else
            {
                //這三點中第一段路線是水平
                if (Math.Abs(vecCurToNext.DirX) > HalfR2000Radius && Math.Abs(vecCurToNext.DirY) > HalfR2000Radius)
                {
                    //第二段路線是R2000 R2000/BR2000
                    if (vecPreToCur.DirX * vecCurToNext.DirX > 0)
                    {
                        //兩段路線Y分量同向
                        return EnumAddressAction.R2000;
                    }
                    else
                    {
                        //兩段路線Y分量反向
                        return EnumAddressAction.BR2000;
                    }
                }
                else
                {
                    //第二段路線是垂直 TR/BTR
                    if (vecPreToCur.DirX * vecCurToNext.DirY < 0)
                    {
                        //逆時針轉                
                        WheelAngle += 90;
                        if (WheelAngle > 100)
                        {
                            WheelAngle = 0;
                            return curAddress.IsTR50 ? EnumAddressAction.BTR50 : EnumAddressAction.BTR350;
                        }
                        else
                        {
                            return curAddress.IsTR50 ? EnumAddressAction.TR50 : EnumAddressAction.TR350;
                        }
                    }
                    else
                    {
                        //順時針轉
                        WheelAngle += -90;
                        if (WheelAngle < -100)
                        {
                            WheelAngle = 0;
                            return curAddress.IsTR50 ? EnumAddressAction.BTR50 : EnumAddressAction.BTR350;
                        }
                        else
                        {
                            return curAddress.IsTR50 ? EnumAddressAction.TR50 : EnumAddressAction.TR350;
                        }
                    }
                }
            }
        }

        private void AddFirstAction()
        {
            if (Math.Abs(AddressPositions[1].X - AddressPositions[0].X) > HalfR2000Radius && Math.Abs(AddressPositions[1].Y - AddressPositions[0].Y) > HalfR2000Radius)
            {
                AddressActions.Add(EnumAddressAction.R2000);
            }
            else
            {
                AddressActions.Add(EnumAddressAction.ST);
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
                    VehicleHeadAngle -= 90;
                    if (VehicleHeadAngle < -100)
                    {
                        VehicleHeadAngle = 0;

                        return curAddress.IsTR50 ? EnumAddressAction.BTR50 : EnumAddressAction.BTR350;
                    }
                    return curAddress.IsTR50 ? EnumAddressAction.TR50 : EnumAddressAction.TR350;
                }
                else
                {
                    //左轉
                    VehicleHeadAngle += 90;
                    if (VehicleHeadAngle > 100)
                    {
                        VehicleHeadAngle = 0;
                        return curAddress.IsTR50 ? EnumAddressAction.BTR50 : EnumAddressAction.BTR350;
                    }
                    return curAddress.IsTR50 ? EnumAddressAction.TR50 : EnumAddressAction.TR350;
                }
            }
        }
        private void SetupFirstAddressAction()
        {
            var firstPosition = AddressPositions[0];
            var secondPosition = AddressPositions[1];
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

        private void RebuildLastSectionForInsideEndAddress()
        {
            if (!theMapInfo.allMapAddresses.ContainsKey(EndAddressId))
            {
                var msg = $"[Address({EndAddressId}) from Agvc is not in MapInfo]";
                middleAgent.Send_Cmd131_TransferResponse(SeqNum, 1, msg);
                msg = $"MovcCmdInfo : Rebuild Last Section +++FAIL+++,  " + msg;
                LoggerAgent.Instance.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", msg));
                mainFlowHandler.StopVisitTransferSteps();
                return;
            }

            if (MovingSections.Count > 0)
            {
                var lastSection = MovingSections[MovingSections.Count - 1];

                if (EndAddressId == lastSection.HeadAddress.Id || EndAddressId == lastSection.TailAddress.Id)
                {
                    //Move to side address of the last section
                    return;
                }

                if (lastSection.InsideAddresses.FindIndex(x => x.Id == EndAddressId) < 0)
                {
                    var msg = $"[EndAddress({EndAddressId}) from Agvc is not in Section({lastSection.Id})]";
                    middleAgent.Send_Cmd131_TransferResponse(SeqNum, 1, msg);
                    msg = $"MovcCmdInfo : Rebuild Last Section +++FAIL+++,  " + msg;
                    LoggerAgent.Instance.LogMsg("Comm", new LogFormat("Comm", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", msg));
                    mainFlowHandler.StopVisitTransferSteps();
                    return;
                }

                switch (lastSection.CmdDirection)
                {
                    case EnumPermitDirection.None:
                        break;
                    case EnumPermitDirection.Forward:
                        {
                            lastSection.TailAddress = theMapInfo.allMapAddresses[EndAddressId];
                        }
                        break;
                    case EnumPermitDirection.Backward:
                        {
                            lastSection.HeadAddress = theMapInfo.allMapAddresses[EndAddressId];
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public MoveCmdInfo DeepClone()
        {
            MoveCmdInfo moveCmd = new MoveCmdInfo();
            moveCmd.AddressActions = AddressActions.DeepClone();
            moveCmd.AddressPositions = AddressPositions.DeepClone();
            moveCmd.SectionSpeedLimits = SectionSpeedLimits.DeepClone();
            moveCmd.SectionIds = SectionIds.DeepClone();
            moveCmd.AddressIds = AddressIds.DeepClone();
            moveCmd.MovingSections = MovingSections.DeepClone();
            moveCmd.MovingSectionsIndex = MovingSectionsIndex;
            moveCmd.CmdId = CmdId;
            moveCmd.CstId = CstId;
            moveCmd.type = type;

            return moveCmd;
        }
    }
}
