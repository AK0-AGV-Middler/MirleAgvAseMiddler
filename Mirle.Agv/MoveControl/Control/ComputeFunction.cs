using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller
{
    public class ComputeFunction
    {
        private const int AllowableTheta = 10;

        public double GetAGVAngle(double originAngle)
        {
            if (Math.Abs(originAngle - 0) < AllowableTheta)
                return 0;
            else if (Math.Abs(originAngle - 90) < AllowableTheta)
                return 90;
            else if (Math.Abs(originAngle - -90) < AllowableTheta)
                return -90;
            else if (Math.Abs(originAngle - 180) < AllowableTheta || Math.Abs(originAngle - -180) < AllowableTheta)
                return 180;
            else
                return originAngle;
        }

        public bool IsSameAngle(double barcodeAngleInMap, double agvAngleInMap, int wheelAngle)
        {
            agvAngleInMap = GetAGVAngle(agvAngleInMap);
            return (agvAngleInMap + barcodeAngleInMap + wheelAngle) % 180 == 0;
        }

        // angle : start to end.
        // input : ( x, y ) ( x2, y2 ), output : angle ( -180 < angle <= 180 ).
        public double ComputeAngle(MapPosition start, MapPosition end)
        {
            double returnAngle = 0;

            if (start.X == end.X)
            {
                if (start.Y > end.Y)
                    returnAngle = 90;
                else
                    returnAngle = -90;
            }
            else
            {
                returnAngle = Math.Atan(-(start.Y - end.Y) / (start.X - end.X)) * 180 / Math.PI;

                if (start.X > end.X)
                {
                    if (returnAngle > 0)
                        returnAngle -= 180;
                    else
                        returnAngle += 180;
                }
            }

            return returnAngle;
        }

        public int ComputeAngleInt(MapPosition start, MapPosition end)
        {
            double returnAngle = ComputeAngle(start, end);
            return (int)returnAngle;
        }

        public MapPosition GetPositionFormEndDistance(MapPosition start, MapPosition end, double endDistance)
        {
            double distance = Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
            double x = end.X + (start.X - end.X) * endDistance / distance;
            double y = end.Y + (start.Y - end.Y) * endDistance / distance;

            MapPosition returnData = new MapPosition((float)x, (float)y);
            return returnData;
        }

        public bool GetR2000IsTurnLeftOrRight(int oldAGVAngle, int newAGVAngle, bool dirFlag, ref int wheelAngle, ref string errorMessage)
        {
            if (oldAGVAngle - newAGVAngle == 270)
                newAGVAngle += 360;
            else if (oldAGVAngle - newAGVAngle == -270)
                oldAGVAngle += 360;

            if (Math.Abs(oldAGVAngle - newAGVAngle) != 90)
            {
                errorMessage = "GetR2000IsTurnLeftOrRight 新舊角度差距不是90度..";
                return false;
            }

            if (newAGVAngle > oldAGVAngle)
                wheelAngle = 1;
            else
                wheelAngle = -1;

            if (!dirFlag)
                wheelAngle = -wheelAngle;

            return true;
        }

        public int GetAGVAngle(AGVPosition nowAGV, ref string errorMessage)
        {
            double nowAngle;
            int returnInt;

            if (nowAGV != null)
            {
                nowAngle = (int)nowAGV.AGVAngle;
            }
            else
            {
                errorMessage = "Real position為null!";
                return -1;
            }

            if (Math.Abs(nowAngle - 0) < AllowableTheta)
                returnInt = 0;
            else if (Math.Abs(nowAngle - 90) < AllowableTheta)
                returnInt = 90;
            else if (Math.Abs(nowAngle - -90) < AllowableTheta)
                returnInt = -90;
            else if (Math.Abs(nowAngle - 180) < AllowableTheta || Math.Abs(nowAngle - -180) < AllowableTheta)
                returnInt = 180;
            else
            {
                errorMessage = "Real position角度偏差過大(超過10度)!";
                returnInt = -1;
            }

            return returnInt;
        }

        public bool GetDirFlagWheelAngleStartInHorizontal(MoveCmdInfo moveCmd, ref BreakDownMoveCommandData data,
            int wheelAngle, ref string errorMessage)
        {
            int trIndex = -1;
            bool hasR2000 = false;

            for (int i = 0; i < moveCmd.AddressActions.Count; i++)
            {
                if (moveCmd.AddressActions[i] == EnumAddressAction.TR50 || moveCmd.AddressActions[i] == EnumAddressAction.TR350 ||
                    moveCmd.AddressActions[i] == EnumAddressAction.BTR50 || moveCmd.AddressActions[i] == EnumAddressAction.BTR350)
                {
                    trIndex = i;
                    break;
                }
                else if (moveCmd.AddressActions[i] == EnumAddressAction.R2000 || moveCmd.AddressActions[i] == EnumAddressAction.BR2000 ||
                         moveCmd.AddressActions[i] == EnumAddressAction.BST)
                    hasR2000 = true;
            }

            if (trIndex == -1)
            {
                if (hasR2000)
                {
                    errorMessage = "不該有橫移接續R2000的動作..";
                    return false;
                }

                if (wheelAngle == 90 || wheelAngle == -90)
                    data.WheelAngle = wheelAngle;

                return true;
            }
            else
            {
                int startSectionAngle = ComputeAngleInt(moveCmd.AddressPositions[trIndex - 1], moveCmd.AddressPositions[trIndex]);
                int endSectionAngle = ComputeAngleInt(moveCmd.AddressPositions[trIndex], moveCmd.AddressPositions[trIndex + 1]);
                int deltaAngle = endSectionAngle - startSectionAngle;

                while (deltaAngle > 180 || deltaAngle <= -180)
                {
                    if (deltaAngle > 180)
                        deltaAngle -= 360;
                    else if (deltaAngle <= -180)
                        deltaAngle += 360;
                }

                if (moveCmd.AddressActions[trIndex] == EnumAddressAction.TR350 || moveCmd.AddressActions[trIndex] == EnumAddressAction.TR50)
                    data.WheelAngle = -deltaAngle;
                else
                    data.WheelAngle = deltaAngle;

                return Math.Abs(deltaAngle) == 90;
            }
        }

        public int GetTurnWheelAngle(int oldWheelAngle, MapPosition start, MapPosition tr, MapPosition end, ref string errorMessage)
        {
            int startSectionAngle = ComputeAngleInt(start, tr);
            int endSectionAngle = ComputeAngleInt(tr, end);
            int delta = endSectionAngle - startSectionAngle;

            if (delta < -90)
                delta += 360;
            else if (delta > 90)
                delta -= 360;

            delta += oldWheelAngle;

            if (delta == 0 || delta == 90 || delta == -90)
                return delta;
            else if (delta == 180 || delta == -180)
            {
                errorMessage = "TR BTR錯誤,輪子要轉斷了!";
                return -1;
            }
            else
            {
                errorMessage = "GetTurnWheelAngle 出現奇怪角度..";
                return -1;
            }
        }

        public int GetAGVAngleAfterR2000(int oldAGVAngle, bool dirFlag, MapPosition start, MapPosition end, ref string errorMessage)
        {
            int newAGVAngle;

            if (oldAGVAngle != 90 && oldAGVAngle != -90 && oldAGVAngle != 0 && oldAGVAngle != 180)
            {
                errorMessage = "GetAGVAngleAfterR2000 oldAGVAngle出現奇怪角度..";
                return -1;
            }

            int r2000SectionAngle = ComputeAngleInt(start, end);

            if (!dirFlag)
            {
                if (oldAGVAngle <= 0)
                    oldAGVAngle += 180;
                else
                    oldAGVAngle -= 180;
            }

            if (r2000SectionAngle != 45 && r2000SectionAngle != -45 && r2000SectionAngle != 135 && r2000SectionAngle != -135)
            {
                errorMessage = "GetAGVAngleAfterR2000 r2000SectionAngle出現奇怪角度..";
                return -1;
            }

            int delta = -(oldAGVAngle - r2000SectionAngle); // 20190830....1519 改為負號.
            if (delta < -45)
                delta += 360;
            else if (delta > 45)
                delta -= 360;

            if (Math.Abs(delta) != 45)
            {
                errorMessage = "GetAGVAngleAfterR2000 r2000SectionAngle 和 oldAGVAngle出現奇怪夾角..";
                return -1;
            }

            newAGVAngle = oldAGVAngle + 2 * delta;

            if (!dirFlag)
            {
                if (newAGVAngle <= 0)
                    newAGVAngle += 180;
                else
                    newAGVAngle -= 180;
            }

            return newAGVAngle;
        }

        public MapPosition GetReversePosition(MapPosition start, MapPosition end, double distance)
        {
            if (distance > 0)
                distance = -distance;

            double distanceAll = Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));

            double x = start.X + (end.X - start.X) * distance / distanceAll;
            double y = start.Y + (end.Y - start.Y) * distance / distanceAll;

            MapPosition returnMapPosition = new MapPosition(x, y);
            return returnMapPosition;
        }

        public MapPosition GetReversePositionR2000(MapPosition end, BreakDownMoveCommandData data, bool isTurnIn, double distance)
        {
            double x = end.X;
            double y = end.Y;

            if (distance < 0)
                distance = -distance;

            int nowAngle = data.NowAGVAngleInMap;

            if (!data.DirFlag)
            {
                if (nowAngle <= 0)
                    nowAngle = nowAngle + 180;
                else
                    nowAngle = nowAngle - 180;
            }

            if (isTurnIn)
            {
                if (nowAngle <= 0)
                    nowAngle = nowAngle + 180;
                else
                    nowAngle = nowAngle - 180;
            }

            switch (nowAngle)
            {
                case 0:
                    x += distance;
                    break;
                case 90:
                    y -= distance;
                    break;
                case -90:
                    y += distance;
                    break;
                case 180:
                    x -= distance;
                    break;
                default:
                    break;
            }

            MapPosition returnMapPosition = new MapPosition(x, y);
            return returnMapPosition;
        }

        public double GetAccDecDistance(double startVel, double endVel, double accOrDec, double jerk)
        {
            if (startVel == endVel)
                return 0;

            double time = accOrDec / jerk; // acc = 0 > acc的時間.
            double deltaVelocity = time * accOrDec / 2 * 2;
            double lastDeltaVelocity;
            double lastDeltaTime;

            if (deltaVelocity == Math.Abs(startVel - endVel))
            {
                return (startVel + endVel) * time; // t = time * 2, distance = (sV+eV)*t/2.
            }
            else if (deltaVelocity > Math.Abs(startVel - endVel))
            {
                deltaVelocity = Math.Abs(startVel - endVel) / 2;
                time = Math.Sqrt(deltaVelocity * 2 / jerk);
                return (startVel + endVel) * time; // t = time * 2, distance = (sV+eV)*t/2.
            }
            else
            {
                lastDeltaVelocity = Math.Abs(startVel - endVel) - deltaVelocity;
                lastDeltaTime = lastDeltaVelocity / accOrDec;

                return (startVel + endVel) * (time + lastDeltaTime / 2); // ( start + end ) * (2*time + lastDeltaTime) / 2.
            }
        }

        public bool GetDirFlagWheelAngle(MoveCmdInfo moveCmd, ref BreakDownMoveCommandData data,
                                  AGVPosition nowAGV, int wheelAngle, ref string errorMessage)
        {
            MapPosition start = moveCmd.AddressPositions[0];
            MapPosition end = moveCmd.AddressPositions[1];
            EnumAddressAction action = moveCmd.AddressActions[0];

            int sectionAngle = 0, deltaAngle = 0;

            int agvAngle = GetAGVAngle(nowAGV, ref errorMessage);

            if (agvAngle == -1)
            {
                return false;
            }

            data.AGVAngleInMap = agvAngle;
            data.NowAGVAngleInMap = agvAngle;

            sectionAngle = ComputeAngleInt(start, end);
            deltaAngle = sectionAngle - agvAngle;

            if (deltaAngle > 180)
                deltaAngle -= 360;
            else if (deltaAngle <= -180)
                deltaAngle += 360;

            if (action == EnumAddressAction.ST)
            {
                switch (deltaAngle)
                {
                    case 0:
                        data.WheelAngle = 0;
                        data.DirFlag = true;
                        break;
                    case 180:
                        data.WheelAngle = 0;
                        data.DirFlag = false;
                        break;
                    case 90:
                    case -90:
                        data.WheelAngle = deltaAngle;
                        if (!GetDirFlagWheelAngleStartInHorizontal(moveCmd, ref data, wheelAngle, ref errorMessage))
                            return false;

                        data.DirFlag = (deltaAngle == data.WheelAngle);
                        break;
                    default:
                        errorMessage = "車姿和Section的角度差不該有0 90 -90 180以外的角度.";
                        return false;
                }
            }
            else if (action == EnumAddressAction.R2000)
            {
                data.WheelAngle = 0;
                data.DirFlag = (Math.Abs(deltaAngle) == 45);
            }
            else
            {
                errorMessage = "起點不該有除了ST, R2000的起點動作.";
                return false;
            }

            return true;
        }
    }
}
