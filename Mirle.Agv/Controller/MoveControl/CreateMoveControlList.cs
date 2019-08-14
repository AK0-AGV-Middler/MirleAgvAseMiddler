using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Controller;
using System.IO;
using System.Xml;

namespace Mirle.Agv.Controller
{
    public class CreateMoveControlList
    {
        private MoveControlConfig moveControlConfig;
        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private string device = "MoveControl";

        private const int AllowableTheta = 10;
        public bool TurnOutSafetyDistance { get; set; } = false;

        public CreateMoveControlList(List<Sr2000Driver> driverSr2000List, MoveControlConfig moveControlConfig)
        {
            this.moveControlConfig = moveControlConfig;
        }

        private void WriteLog(string category, string logLevel, string device, string carrierId, string message,
                             [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string classMethodName = GetType().Name + ":" + memberName;
            LogFormat logFormat = new LogFormat(category, logLevel, classMethodName, device, carrierId, message);

            loggerAgent.LogMsg(logFormat.Category, logFormat);
        }

        #region NewCommand function
        private Command NewMoveCommand(MapPosition position, double realEncoder, double commandDistance, double commandVelocity, bool dirFlag, int StartWheelAngle, int reserveNumber, bool isFirstMove = false)
        {
            Command returnCommand = new Command();
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.Move];
            returnCommand.TriggerEncoder = realEncoder - (dirFlag ? returnCommand.SafetyDistance / 2 : -returnCommand.SafetyDistance / 2);
            returnCommand.CmdType = EnumCommandType.Move;
            returnCommand.Distance = commandDistance * moveControlConfig.MoveCommandDistanceMagnification;
            returnCommand.Velocity = commandVelocity;
            returnCommand.DirFlag = dirFlag;
            returnCommand.WheelAngle = StartWheelAngle;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.NextRserveCancel = false;
            returnCommand.IsFirstMove = isFirstMove;

            return returnCommand;
        }

        private Command NewVChangeCommand(MapPosition position, double realEncoder, double commandVelocity, bool dirFlag, int reserveNumber, bool cancel = false, bool isTurnVChange = false, int wheelAngle = 0)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.Vchange];
            returnCommand.CmdType = EnumCommandType.Vchange;
            returnCommand.Velocity = commandVelocity;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.NextRserveCancel = cancel;
            returnCommand.DirFlag = dirFlag;
            returnCommand.IsTurnVChange = isTurnVChange;
            returnCommand.WheelAngle = wheelAngle;

            return returnCommand;
        }

        private Command NewR2000Command(MapPosition position, double realEncoder, bool dirFlag, int reserveNumber, int wheelAngle, EnumAddressAction type)
        {

            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.R2000];
            returnCommand.CmdType = EnumCommandType.R2000;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.NextRserveCancel = false;
            returnCommand.DirFlag = dirFlag;
            returnCommand.WheelAngle = wheelAngle;
            returnCommand.TurnType = type;

            return returnCommand;
        }

        private Command NewTRCommand(MapPosition position, double realEncoder, bool dirFlag, int reserveNumber, int wheelAngle, EnumAddressAction type)
        {

            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.TR];
            returnCommand.CmdType = EnumCommandType.TR;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.NextRserveCancel = false;
            returnCommand.DirFlag = dirFlag;
            returnCommand.WheelAngle = wheelAngle;
            returnCommand.TurnType = type;

            return returnCommand;
        }

        private Command NewSlowStopCommand(MapPosition position, double realEncoder, bool dirFlag, int reserveNumber, bool cancel = false)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.SlowStop];
            returnCommand.CmdType = EnumCommandType.SlowStop;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.NextRserveCancel = cancel;
            returnCommand.DirFlag = dirFlag;

            return returnCommand;
        }

        private Command NewEndCommand(MapPosition endPosition, double endEncoder, bool dirFlag, int reserveNumber)
        {
            Command returnCommand = new Command();
            returnCommand.Position = null;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.End];
            returnCommand.CmdType = EnumCommandType.End;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.NextRserveCancel = false;
            returnCommand.DirFlag = dirFlag;
            returnCommand.EndPosition = endPosition;
            returnCommand.EndEncoder = endEncoder;

            return returnCommand;
        }


        private Command NewReviseOpenCommand(int reserveNumber)
        {
            Command returnCommand = new Command();
            returnCommand.Position = null;
            returnCommand.CmdType = EnumCommandType.ReviseOpen;
            returnCommand.NextRserveCancel = false;
            returnCommand.ReserveNumber = reserveNumber;

            return returnCommand;
        }

        private Command NewReviseCloseCommand(MapPosition position, double realEncoder, int reserveNumber, bool dirFlag = true, EnumAddressAction type = EnumAddressAction.ST)
        {
            Command returnCommand = new Command();
            returnCommand.Position = position;
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.CmdType = EnumCommandType.ReviseClose;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.ReviseClose];
            returnCommand.NextRserveCancel = false;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.DirFlag = dirFlag;
            returnCommand.TurnType = type;

            return returnCommand;
        }
        #endregion

        #region Write list log 
        private void WriteAGVMCommand(MoveCmdInfo moveCmd)
        {
            string logMessage = "AGVM command資料 : ";

            try
            {
                if (moveCmd != null && moveCmd.AddressPositions.Count > 1)
                {
                    for (int i = 1; i < moveCmd.AddressPositions.Count; i++)
                    {
                        logMessage = logMessage + "\nAGVM 路線第 " + i.ToString() + " 條";

                        if (moveCmd.SectionIds != null && moveCmd.SectionIds.Count > i)
                            logMessage = logMessage + ", Section : " + moveCmd.SectionIds[i - 1];

                        logMessage = logMessage + ", Action : " + moveCmd.AddressActions[i - 1].ToString() + " -> " + moveCmd.AddressActions[i].ToString() + ", from : ";

                        if (moveCmd.AddressIds != null && moveCmd.AddressIds.Count > i)
                            logMessage = logMessage + moveCmd.AddressIds[i - 1];

                        logMessage = logMessage + " ( " + moveCmd.AddressPositions[i - 1].X.ToString("0") + ", " +
                                                          moveCmd.AddressPositions[i - 1].Y.ToString("0") + " ), to : ";

                        if (moveCmd.AddressIds != null && moveCmd.AddressIds.Count > i)
                            logMessage = logMessage + moveCmd.AddressIds[i];

                        logMessage = logMessage + " ( " + moveCmd.AddressPositions[i].X.ToString("0") + ", " +
                                                          moveCmd.AddressPositions[i].Y.ToString("0") +
                                                  " ), velocity : " + moveCmd.SectionSpeedLimits[i - 1].ToString("0");

                    }
                    
                    WriteLog("MoveControl", "7", device, "", logMessage);
                }
                else
                {
                    WriteLog("MoveControl", "4", device, "", "AGVM command資料有問題(為null或address count <=1)");
                }
            }
            catch (Exception ex)
            {
                WriteLog("MoveControl", "3", device, "", "AGVM command資料 異常end (Excption) ~ " + ex.ToString() );
            }
        }

        private void WriteBreakDownMoveCommandList(List<OneceMoveCommand> oneceMoveCommandList)
        {
            string logMessage = "BreakDownMoveCommandList :";

            for (int j = 0; j < oneceMoveCommandList.Count; j++)
            {
                for (int i = 1; i < oneceMoveCommandList[j].AddressPositions.Count; i++)
                {
                    logMessage = logMessage + "\n第 " + (j + 1).ToString() + " 次動令,第 " + i.ToString() +
                                     " 條路線 Action : " + oneceMoveCommandList[j].AddressActions[i - 1].ToString() + " -> " +
                                     oneceMoveCommandList[j].AddressActions[i].ToString() + ", from :  ( " +
                                     oneceMoveCommandList[j].AddressPositions[i - 1].X.ToString("0") + ", " +
                                     oneceMoveCommandList[j].AddressPositions[i - 1].Y.ToString("0") + " ), to :  ( " +
                                     oneceMoveCommandList[j].AddressPositions[i].X.ToString("0") + ", " +
                                     oneceMoveCommandList[j].AddressPositions[i].Y.ToString("0") + " ), velocity : " +
                                     oneceMoveCommandList[j].SectionSpeedLimits[i - 1].ToString("0");
                }
            }

            WriteLog("MoveControl", "7", device, "", logMessage);
        }

        private void WriteReserveListLog(List<ReserveData> reserveDataList)
        {
            string logMessage = "ReserveList :";

            for (int i = 0; i < reserveDataList.Count; i++)
                logMessage = logMessage + "\nreserve node " + i.ToString() + " : ( " +
                                 reserveDataList[i].Position.X.ToString("0") + ", " +
                                 reserveDataList[i].Position.Y.ToString("0") + " )";

            WriteLog("MoveControl", "7", device, "", logMessage);
        }

        private void TriggerLog(Command cmd, ref string logMessage)
        {
            logMessage = logMessage + "command type : " + cmd.CmdType.ToString();

            if (cmd.Position != null)
            {
                logMessage = logMessage + ", 觸發Encoder為 " + cmd.TriggerEncoder.ToString("0") + " ~ " +
                                          (cmd.TriggerEncoder + (cmd.DirFlag ? cmd.SafetyDistance : -cmd.SafetyDistance)).ToString("0") +
                                          ", position : ( " + cmd.Position.X.ToString("0") + ", " + cmd.Position.Y.ToString("0") + " )";

            }
            else
                logMessage = logMessage + ", 為立即觸發";
        }

        private void WritSectionLineListLog(List<SectionLine> sectionLineList)
        {
            string logMessage = "SectionLineList :";

            for (int i = 0; i < sectionLineList.Count; i++)
            {
                logMessage = logMessage + "\nsectionLineList 第 " + (i + 1).ToString() + " 條為 from : (" +
                                 sectionLineList[i].Start.X.ToString("0") + ", " + sectionLineList[i].Start.Y.ToString("0") + " ), to : (" +
                                 sectionLineList[i].End.X.ToString("0") + ", " + sectionLineList[i].End.Y.ToString("0") + " ), DirFlag : " +
                                 (sectionLineList[i].DirFlag ? "前進" : "後退") + ", Distance : " + sectionLineList[i].Distance.ToString("0") +
                                 ", EncoderStart : " + sectionLineList[i].EncoderStart.ToString("0") +
                                 ", EncoderEnd : " + sectionLineList[i].EncoderEnd.ToString("0");
            }

            WriteLog("MoveControl", "7", device, "", logMessage);
        }

        private void WriteMoveCommandListLogTypeMove(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = logMessage + ", 啟動舵輪角度 : " + cmd.WheelAngle.ToString("0") + ", 方向 : " + (cmd.DirFlag ? "前進" : "後退") +
                                      ", 距離 : " + cmd.Distance.ToString("0") + ", 速度 : " + cmd.Velocity.ToString("0") +
                                      ", Reserve index : " + cmd.ReserveNumber.ToString();

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";
        }

        private void WriteMoveCommandListLogTypeReviseOpen(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = logMessage + ", Reserve index : " + cmd.ReserveNumber.ToString();

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";
        }

        private void WriteMoveCommandListLogTypeReviseClose(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";
        }

        private void WriteMoveCommandListLogTypeTR(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = logMessage + ", 為TR " + moveControlConfig.TurnParameter[cmd.TurnType].R.ToString("0") + ", 速度 : " + moveControlConfig.TurnParameter[cmd.TurnType].Velocity.ToString("0") +
                                      ", 舵輪將轉為 : " + cmd.WheelAngle.ToString("0") + ", 方向 : " + (cmd.DirFlag ? "前進" : "後退") +
                                      ", Reserve index : " + cmd.ReserveNumber.ToString();

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";

        }

        private void WriteMoveCommandListLogTypeR2000(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = logMessage + ", 為R " + moveControlConfig.TurnParameter[cmd.TurnType].R.ToString("0") + ", 速度 : " + moveControlConfig.TurnParameter[cmd.TurnType].Velocity.ToString("0") +
                                      ", 前後輪子為向" + (cmd.WheelAngle == -1 ? "右" : "左") + "轉, 方向 : " + (cmd.DirFlag ? "前進" : "後退") +
                                      ", Reserve index : " + cmd.ReserveNumber.ToString();

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";
        }

        private void WriteMoveCommandListLogTypeVchange(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = logMessage + ", 方向 : " + (cmd.DirFlag ? "前進" : "後退") + ", 速度變更為 : " + cmd.Velocity.ToString("0");

            if (cmd.IsTurnVChange)
                logMessage = logMessage + ", 為TR前的 VChange, 舵輪將轉為 : " + cmd.WheelAngle.ToString("0");

            logMessage = logMessage + ", Reserve index : " + cmd.ReserveNumber.ToString();

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";
        }

        private void WriteMoveCommandListLogTypeSlowStop(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = logMessage + ", 方向 : " + (cmd.DirFlag ? "前進" : "後退") + ", Reserve index : " + cmd.ReserveNumber.ToString();

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";
        }

        private void WriteMoveCommandListLogTypeStop(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = logMessage + ", 方向 : " + (cmd.DirFlag ? "前進" : "後退") + ", Reserve index : " + cmd.ReserveNumber.ToString();

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";
        }

        private void WriteMoveCommandListLogTypeEnd(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = logMessage + ", 方向 : " + (cmd.DirFlag ? "前進" : "後退") +
                                      ", 終點Encoder : " + cmd.EndEncoder.ToString("0") + ", position : ( " + cmd.EndPosition.X.ToString("0") +
                                      ", " + cmd.EndPosition.Y.ToString("0") + " )" +
                                      ", Reserve index : " + cmd.ReserveNumber.ToString();

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";
        }

        public void GetMoveCommandListInfo(List<Command> moveCmdList, ref List<string> logMessage)
        {
            string tempLogMessage;

            for (int i = 0; i < moveCmdList.Count; i++)
            {
                tempLogMessage = "";
                switch (moveCmdList[i].CmdType)
                {
                    case EnumCommandType.Move:
                        WriteMoveCommandListLogTypeMove(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.ReviseOpen:
                        WriteMoveCommandListLogTypeReviseOpen(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.ReviseClose:
                        WriteMoveCommandListLogTypeReviseClose(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.TR:
                        WriteMoveCommandListLogTypeTR(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.R2000:
                        WriteMoveCommandListLogTypeR2000(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.Vchange:
                        WriteMoveCommandListLogTypeVchange(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.SlowStop:
                        WriteMoveCommandListLogTypeSlowStop(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.Stop:
                        WriteMoveCommandListLogTypeStop(moveCmdList[i], ref tempLogMessage);
                        break;
                    case EnumCommandType.End:
                        WriteMoveCommandListLogTypeEnd(moveCmdList[i], ref tempLogMessage);
                        break;
                    default:
                        tempLogMessage = "command type : default ??";
                        break;
                }

                logMessage.Add(tempLogMessage);
            }
        }

        private void WriteMoveCommandListLog(List<Command> moveCmdList)
        {
            string totalLogMessage = "MoveCommandList :";
            List<string> logMessage = new List<string>();
            GetMoveCommandListInfo(moveCmdList, ref logMessage);
            for (int i = 0; i < logMessage.Count; i++)
                totalLogMessage = totalLogMessage + "\n" + logMessage[i];

            WriteLog("MoveControl", "7", device, "", totalLogMessage);
        }

        private void WriteListLog(List<Command> moveCmdList, List<SectionLine> sectionLineList, List<ReserveData> reserveDataList)
        {
            WriteReserveListLog(reserveDataList);
            WritSectionLineListLog(sectionLineList);
            WriteMoveCommandListLog(moveCmdList);
        }
        #endregion

        private MapPosition GetPositionFormEndDistance(MapPosition start, MapPosition end, double endDistance)
        {
            double distance = Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
            double x = end.X + (start.X - end.X) * endDistance / distance;
            double y = end.Y + (start.Y - end.Y) * endDistance / distance;

            MapPosition returnData = new MapPosition((float)x, (float)y);
            return returnData;
        }

        #region 角度計算function
        // angle : start to end.
        // input : ( x, y ) ( x2, y2 ), output : angle ( -180 < angle <= 180 ).
        private double ComputeAngle(MapPosition start, MapPosition end)
        {
            double returnAngle = 0;
            try
            {
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
            catch (Exception ex)
            {
                WriteLog("Elmo", "3", device, "", "Excption : " + ex.ToString());
                return returnAngle;
            }
        }


        private int GetAGVAngle(AGVPosition nowAGV, ref string errorMessage)
        {
            double nowAngle;
            int returnInt;

            if (nowAGV != null)
            {
                nowAngle = nowAGV.AGVAngle;
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

        private bool CheckDirFlag(MapPosition start, MapPosition end, EnumAddressAction action, ref BreakDownMoveCommandData data,
                                  AGVPosition nowAGV, ref string errorMessage)
        {
            int sectionAngle = 0, deltaAngle = 0;

            int agvAngle = GetAGVAngle(nowAGV, ref errorMessage);

            if (agvAngle == -1)
            {
                return false;
            }

            data.AGVAngleInMap = agvAngle;
            data.NowAGVAngleInMap = agvAngle;

            if (action == EnumAddressAction.ST )
            {
                sectionAngle = (int)ComputeAngle(start, end);
                deltaAngle = sectionAngle - agvAngle;

                if (deltaAngle > 180)
                    deltaAngle -= 360;
                else if (deltaAngle <= -180)
                    deltaAngle += 360;

                if (deltaAngle == 0)
                {
                    data.WheelAngle = 0;
                    data.DirFlag = true;
                }
                else if (deltaAngle == 180)
                {
                    data.WheelAngle = 0;
                    data.DirFlag = false;
                }
                else if (deltaAngle == 90)
                {
                    // 暫時by pass
                    errorMessage = "暫時by pass 舵輪角度為90度.";
                    return false;
                }
                else if (deltaAngle == -90)
                {
                    // 暫時by pass
                    errorMessage = "暫時by pass 舵輪角度為-90度.";
                    return false;
                }
                else
                {
                    errorMessage = "車姿和Section的角度差不該有0 90 -90 180以外的角度.";
                    return false;
                }
            }
            else
            {
                errorMessage = "起點不該有除了ST, R2000的起點動作.";
                return false;
            }

            return true;
        }
        #endregion

        private MapPosition GetCorrectNodePosition(MapPosition start, MapPosition end, EnumAddressAction action)
        {

            return null;
        }

        private double GetAccDecDistance(double startVel, double endVel, double accOrDec, double jerk)
        {
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

        private double GetSLowStopDistance()
        {
            double jerk = moveControlConfig.Move.Jerk;
            double dec = moveControlConfig.Move.Deceleration;
            return GetAccDecDistance(moveControlConfig.EQVelocity, 0, dec, jerk);
        }

        private double GetVChangeDistance(double startVel, double endVel, double tragetVel, double distance)
        {
            double jerk = moveControlConfig.Move.Jerk;
            double acc = moveControlConfig.Move.Acceleration;
            double dec = moveControlConfig.Move.Deceleration;
            double tempVel = 0;

            double accDistance = 0;
            double decDistance = 0;

            decDistance = GetAccDecDistance(tragetVel, endVel, dec, jerk);
            accDistance = GetAccDecDistance(startVel, tragetVel, acc, jerk);

            if (accDistance + decDistance < distance)
                return decDistance;

            decDistance = 0;
            accDistance = 0;
            //tempVel = (startVel < endVel) ? startVel : endVel;
            tempVel = startVel;

            for (; accDistance + decDistance < distance; tempVel += 5)
            {
                decDistance = GetAccDecDistance(tempVel, endVel, dec, jerk);
                accDistance = GetAccDecDistance(startVel, tempVel, acc, jerk);
            }

            return decDistance;
        }

        private bool GetCorrectReserve(List<ReserveData> reserveDataList, ref AddToCommandListData data, ref string errorMessage)
        {
            if (reserveDataList[data.IndexOfReserveList].Position.X == data.LastNode.X &&
                reserveDataList[data.IndexOfReserveList].Position.Y == data.LastNode.Y)
            {
                if (data.IndexOfReserveList + 1 < reserveDataList.Count)
                    data.IndexOfReserveList++;
                else
                {
                    errorMessage = "// Error. 不該發生.";
                    return false;
                    // Error. 不該發生
                }
            }

            return true;
        }

        private bool AddOneMoveCommandToCommandListActionST(MapPosition position, EnumAddressAction action, double velocityCommand,
                     ref List<Command> moveCmdList, List<ReserveData> reserveDataList, ref AddToCommandListData data, ref string errorMessage)
        {
            MapPosition triggerPosition;
            double distance;
            Command tempCommand;

            if (data.LastAction == EnumAddressAction.R2000)
            {

            }
            else
            {
                if (data.NowVelocityCommand != velocityCommand)
                {  // VChange.
                   // 先不考慮ST ST 間的V不同.
                    errorMessage = "// 先不考慮ST ST 間的V不同.";
                    return false;
                }

                distance = GetVChangeDistance(data.NowVelocity, 0, data.NowVelocityCommand, data.STDistance - data.TurnOutDistance);
                triggerPosition = GetPositionFormEndDistance(data.LastNode, position, data.Distance);
                tempCommand = NewSlowStopCommand(triggerPosition, data.MoveStartEncoder +
                                                                      (data.DirFlag ? data.CommandDistance - distance :
                                                                                    -(data.CommandDistance - distance)),
                                                                      data.DirFlag, data.IndexOfReserveList, true);
                moveCmdList.Add(tempCommand);
            }


            return true;
        }

        private bool AddOneMoveCommandToCommandListActionEndAndSlowStop(MapPosition position, EnumAddressAction action, double velocityCommand,
                     ref List<Command> moveCmdList, List<ReserveData> reserveDataList, ref AddToCommandListData data, ref string errorMessage)
        {
            double distance;
            MapPosition triggerPosition;
            Command tempCommand;

            // 距離非常短,不做加速到站前減速,改為直接用80速度慢慢走,在停止(和正常情況相比會缺少VChange command).
            if (data.CommandDistance < 2 * GetSLowStopDistance())
            {
                // 取得停止距離(vel:80->0),和座標點.
                distance = GetVChangeDistance(data.NowVelocity, 0, moveControlConfig.EQVelocity, data.STDistance);
                triggerPosition = GetPositionFormEndDistance(data.LastNode, position, distance);

                // 直接塞入降速至80命令和SlowStop命令
                tempCommand = NewVChangeCommand(null, 0, moveControlConfig.EQVelocity, data.DirFlag, data.MoveCmdStartReserveNumber);
                moveCmdList.Add(tempCommand);

                tempCommand = NewSlowStopCommand(triggerPosition, data.MoveStartEncoder +
                                              (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance)),
                                               data.DirFlag, data.IndexOfReserveList);
                moveCmdList.Add(tempCommand);
            }
            else
            { // 距離不會太短之情況.
                // 算出直接減速距離是否足夠.
                distance = GetAccDecDistance(data.NowVelocity, 0, moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);

                // 需要直接減速.
                if (distance > data.STDistance - data.TurnOutDistance - moveControlConfig.EQVelocityDistance)
                {
                    //// 插入立即停止指令(基本上會進入這邊都是TR完反折).
                    //tempCommand = NewSlowStopCommand(null, 0, data.DirFlag, data.IndexOfReserveList);
                    //moveCmdList.Add(tempCommand);

                    tempCommand = NewVChangeCommand(null, 0, moveControlConfig.EQVelocity, data.DirFlag, data.IndexOfReserveList);
                    moveCmdList.Add(tempCommand);
                }
                else
                {
                    if (data.NowVelocityCommand > moveControlConfig.EQVelocity)
                    { // 需要降速.
                        // 算出減速距離跟減速座標.
                        distance = GetVChangeDistance(data.NowVelocity, moveControlConfig.EQVelocity, data.NowVelocityCommand,
                                                      data.STDistance - moveControlConfig.EQVelocityDistance - data.TurnOutDistance);

                        distance = distance + moveControlConfig.EQVelocityDistance;
                        triggerPosition = GetPositionFormEndDistance(data.LastNode, position, distance);

                        // 插入減速指令.
                        tempCommand = NewVChangeCommand(triggerPosition,
                            data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance)),
                            moveControlConfig.EQVelocity, data.DirFlag, data.IndexOfReserveList);
                        moveCmdList.Add(tempCommand);
                    }
                }
            }

            //data.TurnOutDistance = 0;

            // 算出停止距離跟座標.
            distance = GetSLowStopDistance();
            triggerPosition = GetPositionFormEndDistance(data.LastNode, position, distance);

            // 插入停止指令.
            tempCommand = NewSlowStopCommand(triggerPosition,
                            data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance)),
                            data.DirFlag, data.IndexOfReserveList);
            moveCmdList.Add(tempCommand);

            // 插入關修正命令.
            tempCommand = NewReviseCloseCommand(null, 0, data.IndexOfReserveList);
            moveCmdList.Add(tempCommand);

            // 是終點.
            if (action == EnumAddressAction.End)
            {
                // 二修命令.
                tempCommand = NewEndCommand(position, data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance : -data.CommandDistance),
                                                      data.DirFlag, data.IndexOfReserveList);
                moveCmdList.Add(tempCommand);
            }

            return true;
        }

        private bool AddOneMoveCommandToCommandListActionTR(MapPosition position, EnumAddressAction action, double velocityCommand, MapPosition nextPosition,
                     ref List<Command> moveCmdList, List<ReserveData> reserveDataList, ref AddToCommandListData data, ref string errorMessage)
        {
            double r = moveControlConfig.TurnParameter[action].R;
            double velocity = moveControlConfig.TurnParameter[action].Velocity;
            double vChangeSafetyDistance = moveControlConfig.TurnParameter[action].VChangeSafetyDistance;
            double closeReviseDistance = moveControlConfig.TurnParameter[action].CloseReviseDistance;
            double trVChangeDistance = 0;
            double distance;
            MapPosition triggerPosition;
            Command tempCommand;

            data.NowWheelAngle = GetTurnWheelAngle(data.NowWheelAngle, data.LastNode, position, nextPosition, ref errorMessage);

            if (data.NowWheelAngle == -1)
                return false;

            // 需要降速.
            if (data.NowVelocityCommand > velocity)
            {
                distance = GetAccDecDistance(data.NowVelocity, velocity, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Jerk);

                // 啟動且距離非常短
                if (data.NowVelocity == 0 && distance + 2 * moveControlConfig.TurnParameter[action].VChangeSafetyDistance > data.STDistance - data.TurnOutDistance)
                {
                    tempCommand = NewVChangeCommand(null, 0, velocity, data.DirFlag, data.IndexOfReserveList + 1, false, true, data.NowWheelAngle);
                    moveCmdList.Add(tempCommand);
                }
                else
                {
                    trVChangeDistance = GetVChangeDistance(data.NowVelocity, velocity, data.NowVelocityCommand,
                        data.STDistance - vChangeSafetyDistance - r - data.TurnOutDistance);
                    trVChangeDistance = trVChangeDistance + trVChangeDistance + r;

                    triggerPosition = GetPositionFormEndDistance(data.LastNode, position, trVChangeDistance);

                    tempCommand = NewVChangeCommand(triggerPosition,
                        data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - trVChangeDistance : -(data.CommandDistance - trVChangeDistance)),
                        velocity, data.DirFlag, data.IndexOfReserveList + 1, false, true, data.NowWheelAngle);
                    moveCmdList.Add(tempCommand);
                }
            }

            //data.TurnOutDistance = 0;

            distance = closeReviseDistance + r;

            triggerPosition = GetPositionFormEndDistance(data.LastNode, position, distance);
            tempCommand = NewReviseCloseCommand(triggerPosition,
                                 data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance)),
                                 data.IndexOfReserveList + 1, data.DirFlag, action);

            if (distance > trVChangeDistance && trVChangeDistance != 0 && moveCmdList.Count > 0)
                moveCmdList.Insert(moveCmdList.Count - 1, tempCommand);
            else
                moveCmdList.Add(tempCommand);

            triggerPosition = GetPositionFormEndDistance(data.LastNode, position, r);

            tempCommand = NewTRCommand(triggerPosition, data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - r : -(data.CommandDistance - r)),
                                       data.DirFlag, data.IndexOfReserveList + 1, data.NowWheelAngle, action);

            moveCmdList.Add(tempCommand);

            if (velocityCommand < moveControlConfig.Move.Velocity)
                tempCommand = NewVChangeCommand(null, 0, velocityCommand, data.DirFlag, data.IndexOfReserveList + 1);
            else
                tempCommand = NewVChangeCommand(null, 0, moveControlConfig.Move.Velocity, data.DirFlag, data.IndexOfReserveList + 1);

            moveCmdList.Add(tempCommand);

            tempCommand = NewReviseOpenCommand(data.IndexOfReserveList + 1);
            moveCmdList.Add(tempCommand);
            data.NowVelocity = velocity;
            data.TurnOutDistance = r;
            data.STDistance = 0;

            return true;
        }

        private bool GetR2000IsTurnLeftOrRight(int oldAGVAngle, int newAGVAngle, bool dirFlag, ref int wheelAngle, ref string errorMessage)
        {
            //if (oldAGVAngle < 0)
            //    oldAGVAngle += 360;

            //if (newAGVAngle < 0)
            //    newAGVAngle += 360;

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
                wheelAngle = -1;
            else
                wheelAngle = 1;

            if (!dirFlag)
                wheelAngle = -wheelAngle;

            return true;
        }

        private bool AddOneMoveCommandToCommandListActionR2000(MapPosition position, EnumAddressAction action, double velocityCommand,
                                                               MapPosition nextPosition, double nextVelocityCommand,
                     ref List<Command> moveCmdList, List<ReserveData> reserveDataList, ref AddToCommandListData data, ref string errorMessage)
        {
            double r = moveControlConfig.TurnParameter[action].R;
            double velocity = moveControlConfig.TurnParameter[action].Velocity;
            double vChangeSafetyDistance = moveControlConfig.TurnParameter[action].VChangeSafetyDistance;
            double closeReviseDistance = moveControlConfig.TurnParameter[action].CloseReviseDistance;
            double trVChangeDistance = 0;
            double distance;
            MapPosition triggerPosition;
            Command tempCommand;
            int wheelAngle = 0;

            int newAgvAngle = GetAGVAngleAfterR2000(data.AGVAngleInMap, data.DirFlag, position, nextPosition, ref errorMessage);


            if (data.NowWheelAngle == -1)
                return false;

            if (!GetR2000IsTurnLeftOrRight(data.AGVAngleInMap, newAgvAngle, data.DirFlag, ref wheelAngle, ref errorMessage))
                return false;

            // 需要降速.
            if (data.NowVelocityCommand > velocity)
            {
                distance = GetAccDecDistance(data.NowVelocity, velocity, moveControlConfig.Move.Acceleration, moveControlConfig.Move.Jerk);

                // 啟動且距離非常短
                if (data.NowVelocity == 0 && distance + 2 * moveControlConfig.TurnParameter[action].VChangeSafetyDistance > data.STDistance - data.TurnOutDistance)
                {
                    tempCommand = NewVChangeCommand(null, 0, velocity, data.DirFlag, data.IndexOfReserveList + 1, false, true, data.NowWheelAngle);
                    moveCmdList.Add(tempCommand);
                }
                else
                {
                    trVChangeDistance = GetVChangeDistance(data.NowVelocity, velocity, data.NowVelocityCommand,
                        data.STDistance - vChangeSafetyDistance - data.TurnOutDistance);
                    trVChangeDistance = trVChangeDistance + trVChangeDistance;

                    triggerPosition = GetPositionFormEndDistance(data.LastNode, position, trVChangeDistance);

                    tempCommand = NewVChangeCommand(triggerPosition,
                        data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - trVChangeDistance : -(data.CommandDistance - trVChangeDistance)),
                        velocity, data.DirFlag, data.IndexOfReserveList + 1, false, true, data.NowWheelAngle);
                    moveCmdList.Add(tempCommand);
                }
            }

            //data.TurnOutDistance = 0;

            distance = closeReviseDistance;

            triggerPosition = GetPositionFormEndDistance(data.LastNode, position, distance);
            tempCommand = NewReviseCloseCommand(triggerPosition,
                                 data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance)),
                                 data.IndexOfReserveList + 1, data.DirFlag, action);

            if (distance > trVChangeDistance && trVChangeDistance != 0 && moveCmdList.Count > 0)
                moveCmdList.Insert(moveCmdList.Count - 1, tempCommand);
            else
                moveCmdList.Add(tempCommand);

            triggerPosition = GetPositionFormEndDistance(data.LastNode, position, 0);

            tempCommand = NewR2000Command(triggerPosition, data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance : -data.CommandDistance),
                                       data.DirFlag, data.IndexOfReserveList + 1, wheelAngle, action);

            moveCmdList.Add(tempCommand);

            if (nextVelocityCommand != 0 && nextVelocityCommand != velocityCommand)
            {
                tempCommand = NewVChangeCommand(null, 0, nextVelocityCommand, data.DirFlag, data.IndexOfReserveList + 1);
                data.NowVelocityCommand = nextVelocityCommand;
            }
            else
                data.NowVelocityCommand = velocity;

            moveCmdList.Add(tempCommand);
            data.NowVelocity = velocity;
            tempCommand = NewReviseOpenCommand(data.IndexOfReserveList + 1);
            moveCmdList.Add(tempCommand);
            data.TurnOutDistance = 0;
            //data.TurnOutDistance = r;
            data.STDistance = 0;

            return true;
        }

        private bool AddOneMoveCommandToCommandList(OneceMoveCommand oneceMoveCommand, ref List<Command> moveCmdList,
                                                    List<ReserveData> reserveDataList, ref AddToCommandListData data, ref string errorMessage)
        {
            double tempDistance;
            data.StartWheelAngle = oneceMoveCommand.WheelAngle;
            data.DirFlag = oneceMoveCommand.DirFlag;
            data.CommandDistance = 0;
            data.NowVelocity = 0;
            data.NowVelocityCommand = 0;
            data.LastNode = null;
            data.STDistance = 0;
            data.MoveCmdStartReserveNumber = data.IndexOfReserveList;
            Command tempCommand;

            for (int i = 0; i < oneceMoveCommand.AddressPositions.Count; i++)
            {
                if (data.LastNode != null)
                {
                    // Reserve
                    if (!GetCorrectReserve(reserveDataList, ref data, ref errorMessage))
                        return false;

                    // 更新總長和直線段長度.
                    tempDistance = Math.Sqrt(Math.Pow(data.LastNode.X - oneceMoveCommand.AddressPositions[i].X, 2) +
                                             Math.Pow(data.LastNode.Y - oneceMoveCommand.AddressPositions[i].Y, 2));
                    data.CommandDistance += tempDistance;
                    data.STDistance += tempDistance;

                    switch (oneceMoveCommand.AddressActions[i])
                    {
                        case EnumAddressAction.ST:
                            if (!AddOneMoveCommandToCommandListActionST(oneceMoveCommand.AddressPositions[i], oneceMoveCommand.AddressActions[i],
                                            oneceMoveCommand.SectionSpeedLimits[i], ref moveCmdList, reserveDataList, ref data, ref errorMessage))
                                return false;
                            break;
                        case EnumAddressAction.End:
                        case EnumAddressAction.SlowStop:
                            if (!AddOneMoveCommandToCommandListActionEndAndSlowStop(oneceMoveCommand.AddressPositions[i], oneceMoveCommand.AddressActions[i],
                                            oneceMoveCommand.SectionSpeedLimits[i], ref moveCmdList, reserveDataList, ref data, ref errorMessage))
                                return false;
                            break;
                        case EnumAddressAction.TR50:
                        case EnumAddressAction.TR350:
                            if (!AddOneMoveCommandToCommandListActionTR(oneceMoveCommand.AddressPositions[i], oneceMoveCommand.AddressActions[i],
                                            oneceMoveCommand.SectionSpeedLimits[i], oneceMoveCommand.AddressPositions[i + 1],
                                            ref moveCmdList, reserveDataList, ref data, ref errorMessage))
                                return false;
                            break;
                        case EnumAddressAction.R2000:
                            if (!AddOneMoveCommandToCommandListActionR2000(oneceMoveCommand.AddressPositions[i], oneceMoveCommand.AddressActions[i],
                               oneceMoveCommand.SectionSpeedLimits[i], oneceMoveCommand.AddressPositions[i + 1], oneceMoveCommand.SectionSpeedLimits[i + 1],
                                            ref moveCmdList, reserveDataList, ref data, ref errorMessage))
                                return false;
                            break;
                        default:
                            errorMessage = "action switch case Default..";
                            return false;
                    }
                }

                // 設定上一點的資訊(位置、動作、速度).
                data.LastNode = oneceMoveCommand.AddressPositions[i];
                data.LastAction = oneceMoveCommand.AddressActions[i];

                if (moveControlConfig.Move.Velocity < oneceMoveCommand.SectionSpeedLimits[i])
                    data.NowVelocityCommand = moveControlConfig.Move.Velocity;
                else
                    data.NowVelocityCommand = oneceMoveCommand.SectionSpeedLimits[i];
            }

            // 分解第一次移動命令完成,插入Move指令.
            tempCommand = NewMoveCommand(oneceMoveCommand.AddressPositions[0], data.MoveStartEncoder, data.CommandDistance,
                                              moveControlConfig.Move.Velocity, data.DirFlag, data.StartWheelAngle,
                                              data.MoveCmdStartReserveNumber, data.InsertIndex == 0);
            moveCmdList.Insert(data.InsertIndex, tempCommand);
            data.InsertIndex++;

            // 如果第一段Section速度比AGV Config速度慢且下一個不是VChange命令，插入VChange(立刻執行)指令.
            if (oneceMoveCommand.SectionSpeedLimits[0] < moveControlConfig.Move.Velocity &&
                (moveCmdList[data.InsertIndex].CmdType != EnumCommandType.Vchange || moveCmdList[data.InsertIndex].Position != null))
            {
                tempCommand = NewVChangeCommand(null, 0, oneceMoveCommand.SectionSpeedLimits[0], data.DirFlag, data.MoveCmdStartReserveNumber);
                moveCmdList.Insert(data.InsertIndex, tempCommand);
                data.InsertIndex++;
            }

            // 插入開修正指令.
            tempCommand = NewReviseOpenCommand(data.MoveCmdStartReserveNumber);
            moveCmdList.Insert(data.InsertIndex, tempCommand);

            // 計算出下一次移動命令的起始Encoder和需要插入Move的Index和方向改為反方向(只有反折會需要拆成多次命令吧?).
            data.MoveStartEncoder += (data.DirFlag ? data.CommandDistance : -data.CommandDistance);
            data.InsertIndex = moveCmdList.Count;

            return true;
        }

        private bool AddTOCommandList(ref List<OneceMoveCommand> oneceMoveCommandList,
             ref List<Command> moveCmdList, List<ReserveData> reserveDataList, int agvAngle, ref string errorMessage)
        {
            if (oneceMoveCommandList.Count == 0)
            {
                errorMessage = "第一步命令拆解資料為空.";
                return false;
            }

            AddToCommandListData data = new AddToCommandListData();
            data.AGVAngleInMap = agvAngle;
            moveCmdList = new List<Command>();

            for (int i = 0; i < oneceMoveCommandList.Count; i++)
            {
                if (!AddOneMoveCommandToCommandList(oneceMoveCommandList[i], ref moveCmdList, reserveDataList, ref data, ref errorMessage))
                    return false;
            }

            return true;
        }

        private int GetTurnWheelAngle(int oldWheelAngle, MapPosition start, MapPosition tr, MapPosition end, ref string errorMessage)
        {
            if (oldWheelAngle != 0)
                return 0;

            double startSectionAngle = ComputeAngle(start, tr);
            double endSectionAngle = ComputeAngle(tr, end);
            double delta = endSectionAngle - startSectionAngle;

            if (delta < -90)
                delta += 360;
            else if (delta > 90)
                delta -= 360;

            if (delta == 90)
                return 90;
            else if (delta == -90)
                return -90;
            else
            {
                errorMessage = "GetTurnWheelAngle 出現奇怪角度..";
                return -1;
                //GGG....
            }
        }

        private int GetAGVAngleAfterR2000(int oldAGVAngle, bool dirFlag, MapPosition start, MapPosition end, ref string errorMessage)
        {
            int newAGVAngle;

            if (oldAGVAngle != 90 && oldAGVAngle != -90 && oldAGVAngle != 0 && oldAGVAngle != 180)
            {
                errorMessage = "GetAGVAngleAfterR2000 oldAGVAngle出現奇怪角度..";
                return -1;
            }

            if (!dirFlag)
            {
                if (oldAGVAngle <= 0)
                    oldAGVAngle += 180;
                else
                    oldAGVAngle -= 180;
            }

            //if (oldAGVAngle < 0)
            //    oldAGVAngle += 360;

            int r2000SectionAngle = (int)ComputeAngle(start, end);

            if (r2000SectionAngle != 45 && r2000SectionAngle != -45 && r2000SectionAngle != 135 && r2000SectionAngle != -135)
            {
                errorMessage = "GetAGVAngleAfterR2000 r2000SectionAngle出現奇怪角度..";
                return -1;
            }

            int delta = oldAGVAngle - r2000SectionAngle;
            if (delta < -45)
                delta += 360;
            else if (delta > 45)
                delta -= 360;
            
            if (Math.Abs(delta) != 45)
            {
                errorMessage = "GetAGVAngleAfterR2000 r2000SectionAngle 和 oldAGVAngle出現奇怪夾角..";
                return -1;
            }

            //if (Math.Abs(oldAGVAngle - r2000SectionAngle) != 45)
            //{
            //    errorMessage = "GetAGVAngleAfterR2000 r2000SectionAngle 和 oldAGVAngle出現奇怪夾角..";
            //    return -1;
            //}

            //if (r2000SectionAngle < 0)
            //    r2000SectionAngle += 360;


            newAGVAngle = oldAGVAngle + 2 * delta;
            //newAGVAngle = oldAGVAngle + 2 * (r2000SectionAngle - oldAGVAngle);

            //while (newAGVAngle > 180 || newAGVAngle <= -180)
            //{
            //    if (newAGVAngle > 180)
            //        newAGVAngle -= 360;
            //    else
            //        newAGVAngle += 360;
            //}

            if (!dirFlag)
            {
                if (newAGVAngle <= 0)
                    newAGVAngle += 180;
                else
                    newAGVAngle -= 180;
            }

            return newAGVAngle;
        }

        private MapPosition GetReversePosition(MapPosition start, MapPosition end, double distance)
        {
            if (distance > 0)
                distance = -distance;

            double distanceAll = Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));

            double x = start.X + (end.X - start.X) * distance / distanceAll;
            double y = start.Y + (end.Y - start.Y) * distance / distanceAll;

            MapPosition returnMapPosition = new MapPosition(x, y);
            return returnMapPosition;
        }

        private MapPosition GetReversePositionR2000(MapPosition end, BreakDownMoveCommandData data, bool isTurnIn, double distance)
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

        // 計算出所有轉彎入彎、出灣的安全距離(加速、減速距離 加上安全距離(出彎可能會沒有、可以設定)).
        private bool GetTurnInOutSafetyDistance(ref BreakDownMoveCommandData data, ref string errorMessage)
        {
            try
            {
                double tempDouble = 0;

                foreach (EnumAddressAction action in (EnumAddressAction[])Enum.GetValues(typeof(EnumAddressAction)))
                {
                    if (action == EnumAddressAction.TR50 || action == EnumAddressAction.TR350)
                    {
                        tempDouble = GetAccDecDistance(0, moveControlConfig.TurnParameter[action].Velocity,
                                                          moveControlConfig.Move.Acceleration, moveControlConfig.Move.Jerk) +
                                                          moveControlConfig.TurnParameter[action].VChangeSafetyDistance + moveControlConfig.TurnParameter[action].R;
                        data.TurnInSafetyDistance.Add(action, tempDouble);

                        tempDouble = GetAccDecDistance(0, moveControlConfig.TurnParameter[action].Velocity,
                                                          moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk) +
                                                          moveControlConfig.TurnParameter[action].R;
                        if (TurnOutSafetyDistance)
                            tempDouble = tempDouble + moveControlConfig.TurnParameter[action].VChangeSafetyDistance;

                        data.TurnOutSafetyDistance.Add(action, tempDouble);
                    }
                    else if (action == EnumAddressAction.R2000)
                    {
                        tempDouble = GetAccDecDistance(0, moveControlConfig.TurnParameter[action].Velocity,
                                                          moveControlConfig.Move.Acceleration, moveControlConfig.Move.Jerk) +
                                                          moveControlConfig.TurnParameter[action].VChangeSafetyDistance;
                        data.TurnInSafetyDistance.Add(action, tempDouble);

                        tempDouble = GetAccDecDistance(0, moveControlConfig.TurnParameter[action].Velocity,
                                                          moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
                        if (TurnOutSafetyDistance)
                            tempDouble = tempDouble + moveControlConfig.TurnParameter[action].VChangeSafetyDistance;

                        data.TurnOutSafetyDistance.Add(action, tempDouble);
                    }
                }

                return true;
            }
            catch
            {
                errorMessage = "GetTurnInOutSafetyDistance Excption!";
                return false;
            }
        }

        private bool IsNotEndOrBackAction(EnumAddressAction action)
        {
            switch (action)
            {
                case EnumAddressAction.BR2000:
                case EnumAddressAction.BTR350:
                case EnumAddressAction.BTR50:
                case EnumAddressAction.BST:
                    return false;
                case EnumAddressAction.End:
                    return false;
                case EnumAddressAction.ST:
                case EnumAddressAction.TR350:
                case EnumAddressAction.TR50:
                case EnumAddressAction.R2000:
                    return true;
                case EnumAddressAction.SlowStop:
                default:
                    return false;
            }
        }

        private void AddMoveCmdToOneceMoveCommand(MoveCmdInfo moveCmd, ref OneceMoveCommand tempOnceMoveCmd, ref BreakDownMoveCommandData data, bool speedIsZero = false)
        {
            tempOnceMoveCmd.AddressPositions.Add(moveCmd.AddressPositions[data.Index]);
            tempOnceMoveCmd.AddressActions.Add(moveCmd.AddressActions[data.Index]);
            if (speedIsZero)
                tempOnceMoveCmd.SectionSpeedLimits.Add(0);
            else
            {
                tempOnceMoveCmd.SectionSpeedLimits.Add(moveCmd.SectionSpeedLimits[data.Index]);
                data.Index++;
            }
        }

        private void AddOneceMoveCommand(ref OneceMoveCommand tempOnceMoveCmd, MapPosition position, EnumAddressAction action, double speed)
        {
            tempOnceMoveCmd.AddressPositions.Add(position);
            tempOnceMoveCmd.AddressActions.Add(action);
            tempOnceMoveCmd.SectionSpeedLimits.Add(speed);
        }

        private bool AddNewSectionLine(ref List<SectionLine> sectionLineList, ref BreakDownMoveCommandData data,
                                           MapPosition startNode, MapPosition endNode, double startEncoder, bool dirFlag, ref string errorMessage)
        {
            try
            {
                SectionLine tempSectionLine = new SectionLine(startNode, endNode, startEncoder, dirFlag, data.StartByPassDistance, data.EndByPassDistance);
                data.StartByPassDistance = 0;
                data.EndByPassDistance = 0;
                sectionLineList.Add(tempSectionLine);
                return true;
            }
            catch
            {
                errorMessage = "AddNewSectionLine Excption!";
                return false;
            }
        }

        private bool CheckFirstTurnDistance(MoveCmdInfo moveCmd, ref List<SectionLine> sectionLineList, ref BreakDownMoveCommandData data,
               ref List<OneceMoveCommand> oneceMoveCommandList, ref OneceMoveCommand tempOnceMoveCmd, ref string errorMessage)
        {
            // 第一次入彎才需要檢查距離是否不夠.
            if (data.IsFirstTurnIn)
            {
                data.TurnInOutDistance = data.TurnInSafetyDistance[moveCmd.AddressActions[data.Index]];
                data.IsFirstTurnIn = false;

                if (data.TempDistance == 0 && moveCmd.AddressActions[data.Index] != EnumAddressAction.R2000)
                {
                    errorMessage = "起點不能是TR50,TR350.";
                    return false;
                }

                // 第一次入彎距離不夠,需直接後退.
                if (data.TempDistance < data.TurnInOutDistance)
                {
                    // 必定是起點,從起點需要往後多少距離(正值).
                    data.BackDistance = data.TurnInOutDistance - data.TempDistance;

                    // 取得反折點的座標.
                    if (moveCmd.AddressActions[data.Index] != EnumAddressAction.R2000)
                        data.TempNode = GetReversePosition(data.StartNode, data.EndNode, data.BackDistance);
                    else
                        data.TempNode = GetReversePositionR2000(data.StartNode, data, true, data.BackDistance);

                    // SectionLineList加入從起點到反折點,且轉換座標只能是起點.
                    if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.TempNode, data.StartMoveEncoder, !data.DirFlag, ref errorMessage))
                        return false;

                    // 第一次移動命令加入在反折點SlowStop且移動方向改為反方向,加入到第一次移動命令.
                    AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.SlowStop, 0);
                    tempOnceMoveCmd.DirFlag = !tempOnceMoveCmd.DirFlag;
                    oneceMoveCommandList.Add(tempOnceMoveCmd);

                    // 宣告第二次移動命令.
                    tempOnceMoveCmd = new OneceMoveCommand(data.WheelAngle, data.DirFlag);
                    AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index - 1]);
                    // 起點修改為反折點,SectionLine開始encoder設定,設定轉換座標的最小值起點offset.
                    data.StartNode = data.TempNode;
                    data.StartMoveEncoder = data.StartMoveEncoder + (!data.DirFlag ? data.BackDistance : -data.BackDistance);
                    data.StartByPassDistance = data.BackDistance;
                    // 重新該新此段SectionLine的距離.
                    data.TempDistance = Math.Sqrt(Math.Pow(data.EndNode.X - data.StartNode.X, 2) + Math.Pow(data.EndNode.Y - data.StartNode.Y, 2));
                }
            }

            return true;
        }


        private bool BreakDownMoveCmd_Turn(MoveCmdInfo moveCmd, ref List<SectionLine> sectionLineList, ref BreakDownMoveCommandData data,
               ref List<OneceMoveCommand> oneceMoveCommandList, ref OneceMoveCommand tempOnceMoveCmd, ref string errorMessage)
        {
            try
            {
                data.TurnType = moveCmd.AddressActions[data.Index];
                data.EndNode = moveCmd.AddressPositions[data.Index];
                // 此段SectionLine距離.
                data.TempDistance = Math.Sqrt(Math.Pow(data.EndNode.X - data.StartNode.X, 2) + Math.Pow(data.EndNode.Y - data.StartNode.Y, 2));

                // 檢查第一次入彎距離是否不夠.
                if (!CheckFirstTurnDistance(moveCmd, ref sectionLineList, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                    return false;

                // 把從start到TR的Node點(EndNode)設定成一條SectionLine並加進List內.
                if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.EndNode, data.StartMoveEncoder,
                                       data.DirFlag, ref errorMessage))
                    return false;

                if (moveCmd.AddressActions[data.Index] == EnumAddressAction.TR350 || moveCmd.AddressActions[data.Index] == EnumAddressAction.TR50)
                {
                    data.WheelAngle = GetTurnWheelAngle(data.WheelAngle, data.StartNode, data.EndNode, data.NextNode, ref errorMessage);
                    if (data.WheelAngle == -1)
                        return false;
                }
                else
                {
                    data.NowAGVAngleInMap = GetAGVAngleAfterR2000(data.NowAGVAngleInMap, data.DirFlag, data.EndNode, data.NextNode, ref errorMessage);
                    if (data.NowAGVAngleInMap == -1)
                        return false;
                }

                // 移動命令加入TR點.
                AddMoveCmdToOneceMoveCommand(moveCmd, ref tempOnceMoveCmd, ref data);

                // 起點修改為TR點,SectionLine開始encoder設定,計算出下一段路線時AGV舵輪角度,並設定現在是準備出彎.
                data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TempDistance : -data.TempDistance);


                data.StartNode = data.EndNode;
                data.IsTurnOut = true;

                return true;
            }
            catch
            {
                errorMessage = "BreakDownMoveCmd_Turn Excption.";
                return false;
            }
        }

        // 應該沒人看得懂.
        private bool BreakDownMoveCmd(MoveCmdInfo moveCmd, ref List<Command> moveCmdList, ref List<SectionLine> sectionLineList,
                                      List<ReserveData> reserveDataList, AGVPosition nowAGV, ref string errorMessage)
        {
            List<OneceMoveCommand> oneceMoveCommandList = new List<OneceMoveCommand>();
            sectionLineList = new List<SectionLine>();

            BreakDownMoveCommandData data = new BreakDownMoveCommandData();
            data.StartNode = moveCmd.AddressPositions[0];

            // 確認啟動時的舵輪角度應該為多少、前進方向 retrun false表示不知道目前位置或者是角度偏差過大(10度).
            // 取得所有入彎、出彎所需要的距離(可能會含保護距離(可以設定)).
            if (!CheckDirFlag(moveCmd.AddressPositions[0], moveCmd.AddressPositions[1], moveCmd.AddressActions[0],
                              ref data, nowAGV, ref errorMessage) ||
                !GetTurnInOutSafetyDistance(ref data, ref errorMessage))
            {
                return false;
            }

            // 設定第一次移動命令的起始舵輪角度和方向性.
            OneceMoveCommand tempOnceMoveCmd = new OneceMoveCommand(data.WheelAngle, data.DirFlag);


            while (!data.IsEnd)
            {
                // 只要不是需要反折(BST,BTR50,BTR350,BR2000)的指令動作.
                while (IsNotEndOrBackAction(moveCmd.AddressActions[data.Index]))
                {
                    // 如果是ST直接加入.
                    if (moveCmd.AddressActions[data.Index] == EnumAddressAction.ST)
                    {
                        if (data.Index > 0 && moveCmd.AddressActions[data.Index - 1] == EnumAddressAction.R2000)
                        {
                            data.EndNode = moveCmd.AddressPositions[data.Index];
                            data.TempDistance = Math.Sqrt(Math.Pow(data.EndNode.X - data.StartNode.X, 2) + Math.Pow(data.EndNode.Y - data.StartNode.Y, 2));

                            if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.EndNode, data.StartMoveEncoder,
                                                   data.DirFlag, ref errorMessage))
                                return false;

                            data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TempDistance : -data.TempDistance);
                            data.StartNode = data.EndNode;
                        }

                        AddMoveCmdToOneceMoveCommand(moveCmd, ref tempOnceMoveCmd, ref data);
                    }
                    else // TR50, TR350, R2000.
                    {
                        if (moveCmd.AddressActions[data.Index] == EnumAddressAction.TR50 || moveCmd.AddressActions[data.Index] == EnumAddressAction.TR350)
                        {
                            data.NextNode = moveCmd.AddressPositions[data.Index + 1];

                            if (!BreakDownMoveCmd_Turn(moveCmd, ref sectionLineList, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                return false;
                        }
                        else if (moveCmd.AddressActions[data.Index] == EnumAddressAction.R2000)
                        {
                            if (moveCmd.SectionSpeedLimits[data.Index] != moveControlConfig.TurnParameter[EnumAddressAction.R2000].Velocity)
                            {
                                errorMessage = "R2000 速度必須要是138!";
                                return false;
                            }

                            data.NextNode = moveCmd.AddressPositions[data.Index + 1];

                            if (!BreakDownMoveCmd_Turn(moveCmd, ref sectionLineList, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                return false;
                        }
                        else
                        {
                            errorMessage = "這邊應該只有轉彎Actino.";
                            return false;
                        }
                    }
                }

                // 終點指令動作.
                if (moveCmd.AddressActions[data.Index] == EnumAddressAction.End)
                {
                    data.IsEnd = true;
                    data.EndNode = moveCmd.AddressPositions[data.Index];
                    data.TempDistance = Math.Sqrt(Math.Pow(data.EndNode.X - data.StartNode.X, 2) + Math.Pow(data.EndNode.Y - data.StartNode.Y, 2));

                    // 如果出彎動作中且出彎距離不夠.
                    if (data.IsTurnOut && data.TempDistance < data.TurnOutSafetyDistance[data.TurnType])
                    {
                        // 計算出終點不夠的距離(正值).
                        data.TurnInOutDistance = data.TurnOutSafetyDistance[data.TurnType];
                        data.BackDistance = data.TurnInOutDistance - data.TempDistance;

                        // 計算出終點的反折點座標.
                        data.TempNode = GetReversePosition(data.EndNode, data.StartNode, data.BackDistance);

                        // SectionLineList加入從start到反折點,且轉換座標只能是start到終點.
                        data.EndByPassDistance = data.BackDistance;
                        if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.TempNode, data.StartMoveEncoder,
                                               data.DirFlag, ref errorMessage))
                            return false;

                        // 此段移動命令加入SlowStop和反折點座標並加入到移動命令List內.
                        AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.SlowStop, 0);
                        oneceMoveCommandList.Add(tempOnceMoveCmd);

                        // 前進方向改成反方向、設定SectionLine開始encoder,設定startByPassDisance,設定新起點和新的啟動encoder和距離.
                        data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TempDistance : -data.TempDistance);
                        data.DirFlag = !data.DirFlag;
                        tempOnceMoveCmd = new OneceMoveCommand(data.WheelAngle, data.DirFlag);
                        AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index - 1]);
                        data.IsTurnOut = false;
                        data.StartByPassDistance = data.BackDistance;
                        data.StartNode = data.TempNode;
                        data.TempDistance = Math.Sqrt(Math.Pow(data.EndNode.X - data.StartNode.X, 2) + Math.Pow(data.EndNode.Y - data.StartNode.Y, 2));
                        data.StartByPassDistance = data.BackDistance;
                    }

                    if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.EndNode, data.StartMoveEncoder,
                                           data.DirFlag, ref errorMessage))
                        return false;

                    AddMoveCmdToOneceMoveCommand(moveCmd, ref tempOnceMoveCmd, ref data, true);
                    oneceMoveCommandList.Add(tempOnceMoveCmd);
                }
                else // BTR50, BTR350, BR2000, BST
                {
                    data.EndNode = moveCmd.AddressPositions[data.Index];
                    data.TempDistance = Math.Sqrt(Math.Pow(data.EndNode.X - data.StartNode.X, 2) + Math.Pow(data.EndNode.Y - data.StartNode.Y, 2));

                    if (moveCmd.AddressActions[data.Index] == EnumAddressAction.BR2000)
                        moveCmd.AddressActions[data.Index] = EnumAddressAction.R2000;
                    else if (moveCmd.AddressActions[data.Index] == EnumAddressAction.BTR50)
                        moveCmd.AddressActions[data.Index] = EnumAddressAction.TR50;
                    else if (moveCmd.AddressActions[data.Index] == EnumAddressAction.BTR350)
                        moveCmd.AddressActions[data.Index] = EnumAddressAction.TR350;
                    else if (moveCmd.AddressActions[data.Index] == EnumAddressAction.ST)
                        moveCmd.AddressActions[data.Index] = EnumAddressAction.ST;

                    // 後反折 : 先轉完彎在反折,轉彎前的舵輪角度不為0 (目前都是在舵輪0度時反折) 或 ST(BST:R2000完的反折).
                    if (moveCmd.AddressActions[data.Index] == EnumAddressAction.ST || data.WheelAngle != 0)
                    {
                        if (moveCmd.AddressActions[data.Index] == EnumAddressAction.ST)
                        { //
                            // 先加入R2000終點BST->ST. 和SectionLineList
                            AddMoveCmdToOneceMoveCommand(moveCmd, ref tempOnceMoveCmd, ref data);
                            if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.EndNode, data.StartMoveEncoder,
                                                   data.DirFlag, ref errorMessage))
                                return false;

                            data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TempDistance : -data.TempDistance);

                            // 算出R2000 出彎點到停止所需要的做短距離.
                            data.TurnInOutDistance = data.TurnOutSafetyDistance[EnumAddressAction.R2000];

                            // 取得即將反折的反折點座標.
                            data.TempNode = GetReversePositionR2000(data.EndNode, data, false, data.TurnInOutDistance);

                            // SectionLineList加入從BST點到反折點,且轉換座標只能是BST點.
                            data.EndByPassDistance = data.TurnInOutDistance;
                            if (!AddNewSectionLine(ref sectionLineList, ref data, data.EndNode, data.TempNode, data.StartMoveEncoder,
                                                   data.DirFlag, ref errorMessage))
                                return false;

                            AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.SlowStop, 0);
                            oneceMoveCommandList.Add(tempOnceMoveCmd);

                            // 前進方向改成反方向、設定SectionLine開始encoder,設定startByPassDisance,設定新起點和新的啟動encoder.
                            data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TurnInOutDistance : -data.TurnInOutDistance);
                            data.DirFlag = !data.DirFlag;
                            tempOnceMoveCmd = new OneceMoveCommand(data.WheelAngle, data.DirFlag);
                            // 這邊速度必須是下一段Section的速度(因為不可能拿上一段的138太慢).
                            AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index]);
                            data.IsTurnOut = false;
                            data.StartByPassDistance = data.TurnInOutDistance;
                            data.StartNode = data.TempNode;
                        }
                        else if (moveCmd.AddressActions[data.Index] == EnumAddressAction.TR50 || moveCmd.AddressActions[data.Index] == EnumAddressAction.TR350)
                        {
                            // 算出TR node點到停止所需要的做短距離.
                            data.TurnInOutDistance = data.TurnOutSafetyDistance[moveCmd.AddressActions[data.Index]];

                            // 取得即將反折的反折點座標(會跟TR下一個Node點反方向).
                            data.NextNode = GetReversePosition(data.EndNode, moveCmd.AddressPositions[data.Index + 1], data.TurnInOutDistance);

                            if (!BreakDownMoveCmd_Turn(moveCmd, ref sectionLineList, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                return false;

                            // SectionLineList加入從TR node點到反折點,且轉換座標只能是node點.
                            data.EndByPassDistance = data.TurnInOutDistance;
                            if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.NextNode, data.StartMoveEncoder,
                                                   data.DirFlag, ref errorMessage))
                                return false;

                            AddOneceMoveCommand(ref tempOnceMoveCmd, data.NextNode, EnumAddressAction.SlowStop, 0);
                            oneceMoveCommandList.Add(tempOnceMoveCmd);


                            // 前進方向改成反方向、設定SectionLine開始encoder,設定startByPassDisance,設定新起點和新的啟動encoder.
                            data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TurnInOutDistance : -data.TurnInOutDistance);
                            data.DirFlag = !data.DirFlag;
                            tempOnceMoveCmd = new OneceMoveCommand(data.WheelAngle, data.DirFlag);
                            AddOneceMoveCommand(ref tempOnceMoveCmd, data.NextNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index - 1]);
                            data.IsTurnOut = false;
                            data.StartByPassDistance = data.TurnInOutDistance;
                            data.StartNode = data.NextNode;
                        }
                        else
                        {
                            errorMessage = "這邊應該只有轉彎BST,BTR(後反折).";
                            return false;
                        }
                    }
                    else
                    {  // 先返折, TurnIn.
                        data.TurnInOutDistance = data.TurnInSafetyDistance[moveCmd.AddressActions[data.Index]];

                        // 取得即將反折的反折點座標(會在直行過TR後一點的地方).
                        data.TempNode = GetReversePosition(data.EndNode, data.StartNode, data.TurnInOutDistance);

                        // SectionLineList加入從start經過TR node點到反折點,且轉換座標只能是start到TR node點.
                        data.EndByPassDistance = data.TurnInOutDistance;
                        if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.TempNode, data.StartMoveEncoder,
                                               data.DirFlag, ref errorMessage))
                            return false;

                        // 在反折點加入SlowStop指令.
                        AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.SlowStop, 0);
                        oneceMoveCommandList.Add(tempOnceMoveCmd);

                        // 前進方向改成反方向、設定SectionLine開始encoder,設定startByPassDisance,設定新起點和新的啟動encoder.
                        data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? (data.TurnInOutDistance + data.TempDistance) :
                                                                                       -(data.TurnInOutDistance + data.TempDistance));
                        data.DirFlag = !data.DirFlag;
                        data.StartByPassDistance = data.TurnInOutDistance;
                        tempOnceMoveCmd = new OneceMoveCommand(data.WheelAngle, data.DirFlag);
                        AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index - 1]);
                        data.IsTurnOut = false;
                        data.StartNode = data.TempNode;
                        data.NextNode = moveCmd.AddressPositions[data.Index + 1];

                        if (moveCmd.AddressActions[data.Index] == EnumAddressAction.TR50 || moveCmd.AddressActions[data.Index] == EnumAddressAction.TR350)
                        {
                            if (!BreakDownMoveCmd_Turn(moveCmd, ref sectionLineList, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                return false;
                        }
                        else if (moveCmd.AddressActions[data.Index] == EnumAddressAction.R2000)
                        {
                            if (!BreakDownMoveCmd_Turn(moveCmd, ref sectionLineList, ref data, ref oneceMoveCommandList, ref tempOnceMoveCmd, ref errorMessage))
                                return false;
                        }
                        else
                        {
                            errorMessage = "這邊應該只有轉彎Actino(先反折).";
                            return false;
                        }
                    }
                }
            }

            WriteBreakDownMoveCommandList(oneceMoveCommandList);
            return AddTOCommandList(ref oneceMoveCommandList, ref moveCmdList, reserveDataList, data.AGVAngleInMap, ref errorMessage);
        }

        private void NewReserveList(List<MapPosition> positionsList, ref List<ReserveData> reserveDataList)
        {
            reserveDataList = new List<ReserveData>();
            ReserveData tempRserveData;

            for (int i = 1; i < positionsList.Count; i++)
            {
                tempRserveData = new ReserveData(positionsList[i]);
                reserveDataList.Add(tempRserveData);
            }
        }

        public bool CreatMoveControlListSectionListReserveList(MoveCmdInfo moveCmd, ref List<Command> moveCmdList, ref List<SectionLine> sectionLineList,
                                                               ref List<ReserveData> reserveDataList, AGVPosition nowAGV, ref string errorMessage)
        {
            try
            {
                if (moveCmd.SectionSpeedLimits == null || moveCmd.AddressActions == null || moveCmd.AddressPositions == null ||
                    moveCmd.SectionSpeedLimits.Count == 0 || (moveCmd.SectionSpeedLimits.Count + 1) != moveCmd.AddressPositions.Count ||
                    moveCmd.AddressActions.Count != moveCmd.AddressPositions.Count)
                {
                    errorMessage = "moveCmd的三種List(Action, Position, Speed)數量不正確!";
                    return false;
                }
                else if (moveCmd.AddressActions[moveCmd.AddressActions.Count - 1] != EnumAddressAction.End)
                {
                    errorMessage = "Action結尾必須是End!";
                    return false;
                }

                WriteAGVMCommand(moveCmd);
                NewReserveList(moveCmd.AddressPositions, ref reserveDataList);

                for (int i = 0; i < moveCmd.SectionSpeedLimits.Count; i++)
                {
                    if (moveCmd.SectionSpeedLimits[i] > moveControlConfig.Move.Velocity)
                        moveCmd.SectionSpeedLimits[i] = moveControlConfig.Move.Velocity;
                }

                if (!BreakDownMoveCmd(moveCmd, ref moveCmdList, ref sectionLineList, reserveDataList, nowAGV, ref errorMessage))
                {
                    return false;
                }
                else
                {
                    WriteListLog(moveCmdList, sectionLineList, reserveDataList);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Excption! " + ex.ToString();
                return false;
            }
        }
    }
}