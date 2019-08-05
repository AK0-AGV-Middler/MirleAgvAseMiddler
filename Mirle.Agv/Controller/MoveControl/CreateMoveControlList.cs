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
        private Logger flow = LoggerAgent.Instance.GetLooger("MoveControl");

        private const int AllowableTheta = 10;

        public CreateMoveControlList(List<Sr2000Driver> driverSr2000List, MoveControlConfig moveControlConfig)
        {
            this.moveControlConfig = moveControlConfig;
        }

        #region NewCommand function
        public Command NewMoveCommand(MapPosition position, double realEncoder, double commandDistance, double commandVelocity, bool dirFlag, int StartWheelAngle, int reserveNumber, bool isFirstMove = false)
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

        public Command NewVChangeCommand(MapPosition position, double realEncoder, double commandVelocity, bool dirFlag, int reserveNumber, bool cancel = false, bool isTurnVChange = false, int wheelAngle = 0)
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

        public Command NewTRCommand(MapPosition position, double realEncoder, bool dirFlag, int reserveNumber, int wheelAngle, EnumAddressAction type)
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
            returnCommand.TRType = type;

            return returnCommand;
        }

        public Command NewSlowStopCommand(MapPosition position, double realEncoder, bool dirFlag, int reserveNumber, bool cancel = false)
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

        public Command NewEndCommand(MapPosition endPosition, double endEncoder, bool dirFlag, int reserveNumber)
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


        public Command NewReviseOpenCommand(int reserveNumber)
        {
            Command returnCommand = new Command();
            returnCommand.Position = null;
            returnCommand.CmdType = EnumCommandType.ReviseOpen;
            returnCommand.NextRserveCancel = false;
            returnCommand.ReserveNumber = reserveNumber;

            return returnCommand;
        }

        public Command NewReviseCloseCommand(MapPosition position, double realEncoder, int reserveNumber, bool dirFlag = true)
        {
            Command returnCommand = new Command();
            returnCommand.Position = position;
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.CmdType = EnumCommandType.ReviseClose;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.ReviseClose];
            returnCommand.NextRserveCancel = false;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.DirFlag = dirFlag;

            return returnCommand;
        }
        #endregion

        #region Write list log 
        private void WriteAGVMCommand(MoveCmdInfo moveCmd)
        {
            flow.SavePureLog("AGVM command資料 :");
            string logMessage;

            try
            {
                if (moveCmd != null && moveCmd.AddressPositions.Count > 1)
                {
                    for (int i = 1; i < moveCmd.AddressPositions.Count; i++)
                    {
                        logMessage = "AGVM 路線第 " + i.ToString() + " 條";

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

                        flow.SavePureLog(logMessage);
                    }

                    flow.SavePureLog("AGVM command資料 end~\n");
                }
                else
                {
                    flow.SavePureLog("AGVM command資料有問題(為null或address count <=1)\n");
                }
            }
            catch
            {
                flow.SavePureLog("AGVM command資料 異常end (Excption) ~\n");
            }
        }

        private void WriteBreakDownMoveCommandList(List<OneceMoveCommand> oneceMoveCommandList)
        {
            flow.SavePureLog("BreakDownMoveCommandList :");

            for (int j = 0; j < oneceMoveCommandList.Count; j++)
            {
                for (int i = 1; i < oneceMoveCommandList[j].AddressPositions.Count; i++)
                {
                    flow.SavePureLog("第 " + (j + 1).ToString() + " 次動令,第 " + i.ToString() +
                                     " 條路線 Action : " + oneceMoveCommandList[j].AddressActions[i - 1].ToString() + " -> " +
                                     oneceMoveCommandList[j].AddressActions[i].ToString() + ", from :  ( " +
                                     oneceMoveCommandList[j].AddressPositions[i - 1].X.ToString("0") + ", " +
                                     oneceMoveCommandList[j].AddressPositions[i - 1].Y.ToString("0") + " ), to :  ( " +
                                     oneceMoveCommandList[j].AddressPositions[i].X.ToString("0") + ", " +
                                     oneceMoveCommandList[j].AddressPositions[i].Y.ToString("0") + " ), velocity : " +
                                     oneceMoveCommandList[j].SectionSpeedLimits[i - 1].ToString("0"));
                }

                flow.SavePureLog("");
            }

            flow.SavePureLog("BreakDownMoveCommandList end~\n");
        }

        private void WriteReserveListLog(List<ReserveData> reserveDataList)
        {
            flow.SavePureLog("ReserveList :");

            for (int i = 0; i < reserveDataList.Count; i++)
                flow.SavePureLog("reserve node " + i.ToString() + " : ( " +
                                 reserveDataList[i].Position.X.ToString("0") + ", " +
                                 reserveDataList[i].Position.Y.ToString("0") + " )");

            flow.SavePureLog("ReserveList end~\n");
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
            flow.SavePureLog("SectionLineList :");

            for (int i = 0; i < sectionLineList.Count; i++)
            {
                flow.SavePureLog("sectionLineList 第 " + (i + 1).ToString() + " 條為 from : (" +
                                 sectionLineList[i].Start.X.ToString("0") + ", " + sectionLineList[i].Start.Y.ToString("0") + " ), to : (" +
                                 sectionLineList[i].End.X.ToString("0") + ", " + sectionLineList[i].End.Y.ToString("0") + " ), DirFlag : " +
                                 (sectionLineList[i].DirFlag ? "前進" : "後退") + ", Distance : " + sectionLineList[i].Distance.ToString("0") +
                                 ", EncoderStart : " + sectionLineList[i].EncoderStart.ToString("0") +
                                 ", EncoderEnd : " + sectionLineList[i].EncoderEnd.ToString("0"));
            }

            flow.SavePureLog("SectionLineList end~\n");
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

            logMessage = logMessage + ", 為TR " + moveControlConfig.TR[cmd.TRType].R.ToString("0") + ", 速度 : " + moveControlConfig.TR[cmd.TRType].Velocity.ToString("0") +
                                      ", 舵輪將轉為 : " + cmd.WheelAngle.ToString("0") + ", 方向 : " + (cmd.DirFlag ? "前進" : "後退") +
                                      ", Reserve index : " + cmd.ReserveNumber.ToString();

            if (cmd.NextRserveCancel)
                logMessage = logMessage + ", 取得下個Reserve點時取消此Command";

        }

        private void WriteMoveCommandListLogTypeR2000(Command cmd, ref string logMessage)
        {

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
            flow.SavePureLog("MoveCommandList :");
            List<string> logMessage = new List<string>();
            GetMoveCommandListInfo(moveCmdList, ref logMessage);
            for (int i = 0; i < logMessage.Count; i++)
                flow.SavePureLog(logMessage[i]);

            flow.SavePureLog("MoveCommandList end~\n");
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
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算Section起始方向) 失敗!" + ex.ToString());
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
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算AGV在地圖上的角度) 失敗! Real position為null (剛開機或進過JogPitch模式)且讀取不到Barcode值!");
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

            if (returnInt != -1)
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算AGV在地圖上的角度) 成功! 角度為" + returnInt.ToString("0") + "(+-10度內).");
            else
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算AGV在地圖上的角度) 失敗! 角度為" + nowAngle.ToString("0.0") + "(差超過+-10度)");

            return returnInt;
        }

        private bool CheckDirFlag(MapPosition start, MapPosition end, EnumAddressAction action, ref int wheelAngle, ref bool dirFlag,
                                  AGVPosition nowAGV, ref string errorMessage)
        {
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算起始舵輪角度、走行方向) 開始~");
            int sectionAngle = 0, deltaAngle = 0;

            int agvAngle = GetAGVAngle(nowAGV, ref errorMessage);

            if (agvAngle == -1)
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算起始舵輪角度、走行方向) 失敗!");
                return false;
            }

            if (action == EnumAddressAction.ST || action == EnumAddressAction.R2000)
            {
                sectionAngle = (int)ComputeAngle(start, end);
                deltaAngle = sectionAngle - agvAngle;

                if (deltaAngle > 180)
                    deltaAngle -= 360;
                else if (deltaAngle <= -180)
                    deltaAngle += 360;

                if (deltaAngle == 0)
                {
                    wheelAngle = 0;
                    dirFlag = true;
                }
                else if (deltaAngle == 180)
                {
                    wheelAngle = 0;
                    dirFlag = false;
                }
                else if (deltaAngle == 90)
                {
                    // 暫時by pass
                    flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算起始舵輪角度、走行方向) 失敗! 暫時by pass 90度.");
                    errorMessage = "暫時by pass 舵輪角度為90度.";
                    return false;
                }
                else if (wheelAngle == -90)
                {
                    // 暫時by pass
                    flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算起始舵輪角度、走行方向) 失敗! 暫時by pass -90度.");
                    errorMessage = "暫時by pass 舵輪角度為-90度.";
                    return false;
                }
                else
                {
                    errorMessage = "車姿和Section的角度差不該有0 90 -90 180以外的角度.";
                    flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算起始舵輪角度、走行方向) 失敗! 車姿和Section的角度差不該有0 90 -90 180以外的角度.");
                    return false;
                }
            }
            else
            {
                errorMessage = "起點不該有除了ST, R2000的起點動作.";
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(計算起始舵輪角度、走行方向) 失敗! 不該有除了ST, R2000的起點動作");
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
            tempVel = (startVel < endVel) ? startVel : endVel;

            for (; accDistance + decDistance < distance; tempVel += 10)
            {
                decDistance = GetAccDecDistance(tempVel, endVel, dec, jerk);
                accDistance = GetAccDecDistance(startVel, tempVel, acc, jerk);
            }

            return decDistance;
        }


        private bool AddTOCommandList(ref List<OneceMoveCommand> oneceMoveCommandList,
             ref List<Command> moveCmdList, ref List<SectionLine> sectionLineList, List<ReserveData> reserveDataList, ref string errorMessage)
        {
            if (oneceMoveCommandList.Count == 0)
            {
                errorMessage = "第一步命令拆解資料為空.";
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(拆解命令第二步) 失敗! 第一步命令拆解資料為空.");
                return false;
            }

            double commandDistance = 0, moveStartEncoder = 0;
            bool dirFlag = oneceMoveCommandList[0].DirFlag;
            int StartWheelAngle = 0, NowWheelAngle = 0, insertIndex = 0, moveCmdStartReserveNumber = 0;
            MapPosition lastNode, triggerPosition;
            EnumAddressAction lastAction = EnumAddressAction.ST;
            double lastVelocity = 0;
            double nowVelocityCommand = 0;
            List<Command> tempCommadnList = new List<Command>();
            Command tempCommand;
            double distance = 0, TRVChangeDistance;
            double stTotalDistance = 0, nowVelocity = 0;
            sectionLineList = new List<SectionLine>();
            SectionLine tempSectionLine;
            double trR, trVelocity, trVChangeDistance, trCloseReviseDistance;
            int indexOfReserveList = 0;

            for (int listCount = 0; listCount < oneceMoveCommandList.Count; listCount++)
            {
                StartWheelAngle = oneceMoveCommandList[listCount].WheelAngle;
                commandDistance = 0;
                nowVelocityCommand = 0;
                stTotalDistance = 0;
                nowVelocity = 0;
                lastNode = null;
                moveCmdStartReserveNumber = indexOfReserveList;

                for (int i = 0; i < oneceMoveCommandList[listCount].AddressPositions.Count; i++)
                {
                    if (lastNode != null)
                    {
                        tempSectionLine = new SectionLine(lastNode, oneceMoveCommandList[listCount].AddressPositions[i],
                                                          (dirFlag ? commandDistance : -commandDistance), dirFlag);
                        sectionLineList.Add(tempSectionLine);

                        if (reserveDataList[indexOfReserveList].Position.X == lastNode.X && reserveDataList[indexOfReserveList].Position.Y == lastNode.Y)
                        {
                            if (indexOfReserveList + 1 < reserveDataList.Count)
                                indexOfReserveList++;
                            else
                            {
                                errorMessage = "// Error. 不該發生.";
                                return false;
                                // Error. 不該發生
                            }
                        }

                        commandDistance += Math.Sqrt(Math.Pow(lastNode.X - oneceMoveCommandList[listCount].AddressPositions[i].X, 2) +
                                                     Math.Pow(lastNode.Y - oneceMoveCommandList[listCount].AddressPositions[i].Y, 2));
                        stTotalDistance += Math.Sqrt(Math.Pow(lastNode.X - oneceMoveCommandList[listCount].AddressPositions[i].X, 2) +
                                                     Math.Pow(lastNode.Y - oneceMoveCommandList[listCount].AddressPositions[i].Y, 2));

                        if (lastAction == EnumAddressAction.ST && oneceMoveCommandList[listCount].AddressActions[i] == EnumAddressAction.ST)
                        { // YA
                            if (lastVelocity != oneceMoveCommandList[listCount].SectionSpeedLimits[i])
                            {  // VChange.
                               // 先不考慮ST ST 間的V不同.
                                errorMessage = "// 先不考慮ST ST 間的V不同.";
                                return false;
                            }

                            distance = GetVChangeDistance(nowVelocity, 0, nowVelocityCommand, stTotalDistance);
                            triggerPosition = GetPositionFormEndDistance(lastNode, oneceMoveCommandList[listCount].AddressPositions[i], distance);

                            tempCommand = NewSlowStopCommand(triggerPosition, moveStartEncoder + (dirFlag ? commandDistance : -commandDistance) - (dirFlag ? distance : -distance), dirFlag, indexOfReserveList, true);
                            tempCommadnList.Add(tempCommand);
                        }
                        else
                        {
                            switch (oneceMoveCommandList[listCount].AddressActions[i])
                            {
                                case EnumAddressAction.End:
                                case EnumAddressAction.SlowStop:
                                    if (commandDistance < 2 * GetSLowStopDistance())
                                    { // 距離非常短,不做加速到站前減速,改為直接用80速度慢慢走,在停止(和正常情況相比會缺少VChange command).
                                        oneceMoveCommandList[listCount].SectionSpeedLimits[0] = (float)moveControlConfig.EQVelocity;


                                        distance = GetVChangeDistance(nowVelocity, 0, moveControlConfig.EQVelocity, stTotalDistance);
                                        triggerPosition = GetPositionFormEndDistance(lastNode, oneceMoveCommandList[listCount].AddressPositions[i], distance);

                                        tempCommand = NewSlowStopCommand(triggerPosition,
                                                        moveStartEncoder + (dirFlag ? commandDistance : -commandDistance) - (dirFlag ? distance : -distance),
                                                        dirFlag, indexOfReserveList);

                                        tempCommadnList.Add(tempCommand);
                                    }
                                    else
                                    { // 距離不會太短之情況.
                                        if (nowVelocityCommand > moveControlConfig.EQVelocity)
                                        { // 需要降速.
                                            distance = GetVChangeDistance(nowVelocity, moveControlConfig.EQVelocity, nowVelocityCommand,
                                                stTotalDistance - moveControlConfig.EQVelocityDistance);

                                            distance = distance + moveControlConfig.EQVelocityDistance;

                                            triggerPosition = GetPositionFormEndDistance(lastNode, oneceMoveCommandList[listCount].AddressPositions[i], distance);

                                            tempCommand = NewVChangeCommand(triggerPosition,
                                                moveStartEncoder + (dirFlag ? commandDistance : -commandDistance) - (dirFlag ? distance : -distance),
                                                moveControlConfig.EQVelocity, dirFlag, indexOfReserveList);
                                            tempCommadnList.Add(tempCommand);
                                        }

                                        distance = GetSLowStopDistance();
                                        triggerPosition = GetPositionFormEndDistance(lastNode, oneceMoveCommandList[listCount].AddressPositions[i], distance);

                                        tempCommand = NewSlowStopCommand(triggerPosition,
                                                        moveStartEncoder + (dirFlag ? commandDistance : -commandDistance) - (dirFlag ? distance : -distance),
                                                        dirFlag, indexOfReserveList);

                                        tempCommadnList.Add(tempCommand);
                                    }

                                    if (oneceMoveCommandList[listCount].AddressActions[i] == EnumAddressAction.End)
                                    { // 是終點.
                                        tempCommand = NewReviseCloseCommand(null, 0, indexOfReserveList);
                                        tempCommadnList.Add(tempCommand);

                                        tempCommand = NewEndCommand(oneceMoveCommandList[listCount].AddressPositions[i],
                                                   moveStartEncoder + (dirFlag ? commandDistance : -commandDistance), dirFlag, indexOfReserveList);
                                        tempCommadnList.Add(tempCommand);
                                    }

                                    break;
                                case EnumAddressAction.TR50:
                                case EnumAddressAction.TR350:
                                    trR = moveControlConfig.TR[oneceMoveCommandList[listCount].AddressActions[i]].R;
                                    trVelocity = moveControlConfig.TR[oneceMoveCommandList[listCount].AddressActions[i]].Velocity;
                                    trVChangeDistance = moveControlConfig.TR[oneceMoveCommandList[listCount].AddressActions[i]].VChangeDistance;
                                    trCloseReviseDistance = moveControlConfig.TR[oneceMoveCommandList[listCount].AddressActions[i]].CloseReviseDistance;

                                    NowWheelAngle = GetTurnWheelAngle(NowWheelAngle, lastNode, oneceMoveCommandList[listCount].AddressPositions[i],
                                        oneceMoveCommandList[listCount].AddressPositions[i + 1]);
                                    if (NowWheelAngle == -1)
                                    {
                                        errorMessage = "TR舵輪該旋轉的角度有問題.";
                                        return false;
                                    }

                                    TRVChangeDistance = 0;

                                    if (nowVelocityCommand > trVelocity)
                                    { // 降速.
                                        TRVChangeDistance = GetVChangeDistance(nowVelocity, trVelocity, nowVelocityCommand,
                                            stTotalDistance - trVChangeDistance - trR);
                                        TRVChangeDistance = TRVChangeDistance + trVChangeDistance + trR;

                                        triggerPosition = GetPositionFormEndDistance(lastNode, oneceMoveCommandList[listCount].AddressPositions[i], TRVChangeDistance);

                                        tempCommand = NewVChangeCommand(triggerPosition,
                                            moveStartEncoder + (dirFlag ? commandDistance : -commandDistance) - (dirFlag ? TRVChangeDistance : -TRVChangeDistance),
                                            trVelocity, dirFlag, indexOfReserveList + 1, false, true, NowWheelAngle);
                                        tempCommadnList.Add(tempCommand);
                                    }

                                    distance = trCloseReviseDistance + trR;

                                    triggerPosition = GetPositionFormEndDistance(lastNode, oneceMoveCommandList[listCount].AddressPositions[i], distance);
                                    tempCommand = NewReviseCloseCommand(triggerPosition,
                                        moveStartEncoder + (dirFlag ? commandDistance : -commandDistance) - (dirFlag ? distance : -distance), indexOfReserveList + 1, dirFlag);

                                    if (distance > TRVChangeDistance && TRVChangeDistance != 0 && tempCommadnList.Count > 0)
                                        tempCommadnList.Insert(tempCommadnList.Count - 1, tempCommand);
                                    else
                                        tempCommadnList.Add(tempCommand);

                                    triggerPosition = GetPositionFormEndDistance(lastNode, oneceMoveCommandList[listCount].AddressPositions[i], trR);

                                    tempCommand = NewTRCommand(triggerPosition,
                                        (dirFlag ? commandDistance : -commandDistance) - (dirFlag ? trR : -trR),
                                        dirFlag, indexOfReserveList + 1, NowWheelAngle, oneceMoveCommandList[listCount].AddressActions[i]);

                                    tempCommadnList.Add(tempCommand);

                                    tempCommand = NewVChangeCommand(null, 0, oneceMoveCommandList[listCount].SectionSpeedLimits[i], dirFlag,
                                                                     indexOfReserveList + 1);
                                    tempCommadnList.Add(tempCommand);

                                    tempCommand = NewReviseOpenCommand(indexOfReserveList + 1);
                                    tempCommadnList.Add(tempCommand);
                                    nowVelocity = trVelocity;
                                    break;
                                case EnumAddressAction.R2000:
                                    break;
                                case EnumAddressAction.ST:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    lastNode = oneceMoveCommandList[listCount].AddressPositions[i];
                    lastAction = oneceMoveCommandList[listCount].AddressActions[i];
                    if (moveControlConfig.AGVMaxVelocity < oneceMoveCommandList[listCount].SectionSpeedLimits[i])
                        nowVelocityCommand = moveControlConfig.AGVMaxVelocity;
                    else
                        nowVelocityCommand = oneceMoveCommandList[listCount].SectionSpeedLimits[i];

                }

                tempCommand = NewMoveCommand(oneceMoveCommandList[listCount].AddressPositions[0], moveStartEncoder, commandDistance,
                                          moveControlConfig.AGVMaxVelocity, dirFlag, StartWheelAngle, moveCmdStartReserveNumber, insertIndex == 0);
                tempCommadnList.Insert(insertIndex, tempCommand);
                insertIndex++;

                if (oneceMoveCommandList[listCount].SectionSpeedLimits[0] < moveControlConfig.AGVMaxVelocity)
                {
                    tempCommand = NewVChangeCommand(null, 0, oneceMoveCommandList[listCount].SectionSpeedLimits[0], dirFlag, moveCmdStartReserveNumber);
                    tempCommadnList.Insert(insertIndex, tempCommand);
                    insertIndex++;
                }

                tempCommand = NewReviseOpenCommand(moveCmdStartReserveNumber);
                tempCommadnList.Insert(insertIndex, tempCommand);

                moveStartEncoder += (dirFlag ? commandDistance : -commandDistance);
                insertIndex = tempCommadnList.Count;
                dirFlag = !dirFlag;
            }

            indexOfReserveList = 0;

            moveCmdList = tempCommadnList;

            return true;
        }

        private int GetTurnWheelAngle(int oldWheelAngle, MapPosition start, MapPosition tr, MapPosition end)
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
                return -1;
                //GGG....
            }
        }


        private bool BreakDownMoveCmd(MoveCmdInfo moveCmd, ref List<Command> moveCmdList, ref List<SectionLine> sectionLineList,
                                      List<ReserveData> reserveDataList, AGVPosition nowAGV, ref string errorMessage)
        {
            //SectionLineList改為由此產生.
            WriteAGVMCommand(moveCmd);
            List<OneceMoveCommand> oneceMoveCommandList = new List<OneceMoveCommand>();
            int wheelAngle = 0;
            bool dirFlag = true;
            if (!CheckDirFlag(moveCmd.AddressPositions[0], moveCmd.AddressPositions[1], moveCmd.AddressActions[0], ref wheelAngle, ref dirFlag, nowAGV, ref errorMessage))
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(拆解命令第一步) 失敗!\n");
                return false;
            }

            double inTR50Distance = GetAccDecDistance(0, moveControlConfig.TR[EnumAddressAction.TR50].Velocity,
                                                         moveControlConfig.Move.Acceleration, moveControlConfig.Move.Jerk);
            inTR50Distance = inTR50Distance + moveControlConfig.TR[EnumAddressAction.TR50].VChangeDistance + moveControlConfig.TR[EnumAddressAction.TR50].R;

            double outTR50Distance = GetAccDecDistance(0, moveControlConfig.TR[EnumAddressAction.TR50].Velocity,
                                                         moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            outTR50Distance = outTR50Distance + moveControlConfig.TR[EnumAddressAction.TR50].VChangeDistance + moveControlConfig.TR[EnumAddressAction.TR50].R;

            double inTR350Distance = GetAccDecDistance(0, moveControlConfig.TR[EnumAddressAction.TR350].Velocity,
                                                         moveControlConfig.Move.Acceleration, moveControlConfig.Move.Jerk);
            inTR350Distance = inTR350Distance + moveControlConfig.TR[EnumAddressAction.TR350].VChangeDistance + moveControlConfig.TR[EnumAddressAction.TR50].R;

            double outTR350Distance = GetAccDecDistance(0, moveControlConfig.TR[EnumAddressAction.TR350].Velocity,
                                                         moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk);
            outTR350Distance = outTR350Distance + moveControlConfig.TR[EnumAddressAction.TR350].VChangeDistance + moveControlConfig.TR[EnumAddressAction.TR50].R;
            
            OneceMoveCommand tempOnceMoveCmd = new OneceMoveCommand(wheelAngle, dirFlag);
            MapPosition tempPostion;
            int i = 0;

            while (moveCmd.AddressActions[i] != EnumAddressAction.End)
            {
                while (moveCmd.AddressActions[i] != EnumAddressAction.End && moveCmd.AddressActions[i] != EnumAddressAction.BR2000 &&
                       moveCmd.AddressActions[i] != EnumAddressAction.BTR350 && moveCmd.AddressActions[i] != EnumAddressAction.BTR50)
                {
                    tempOnceMoveCmd.AddressPositions.Add(moveCmd.AddressPositions[i]);
                    tempOnceMoveCmd.AddressActions.Add(moveCmd.AddressActions[i]);
                    tempOnceMoveCmd.SectionSpeedLimits.Add(moveCmd.SectionSpeedLimits[i]);
                    i++;
                }

                if (moveCmd.AddressActions[i] == EnumAddressAction.End)
                {
                    tempOnceMoveCmd.AddressPositions.Add(moveCmd.AddressPositions[i]);
                    tempOnceMoveCmd.AddressActions.Add(moveCmd.AddressActions[i]);
                    tempOnceMoveCmd.SectionSpeedLimits.Add(0);
                    oneceMoveCommandList.Add(tempOnceMoveCmd);
                }
                else
                {   // BTR50, BTR350, BR2000.
                    // B只能是 wheelAngle = 0 時, 先bypass這邊.
                    errorMessage = "暫時By pass反摺!";
                    return false;
                    tempOnceMoveCmd.AddressPositions.Add(moveCmd.AddressPositions[i]);
                    tempOnceMoveCmd.AddressActions.Add(EnumAddressAction.ST);
                    tempOnceMoveCmd.SectionSpeedLimits.Add(moveCmd.SectionSpeedLimits[i - 1]);

                    tempPostion = GetCorrectNodePosition(moveCmd.AddressPositions[i - 1], moveCmd.AddressPositions[i], moveCmd.AddressActions[i]);
                    tempOnceMoveCmd.AddressPositions.Add(tempPostion);
                    tempOnceMoveCmd.AddressActions.Add(EnumAddressAction.SlowStop);
                    tempOnceMoveCmd.SectionSpeedLimits.Add(0);

                    oneceMoveCommandList.Add(tempOnceMoveCmd);
                    dirFlag = !dirFlag;
                    tempOnceMoveCmd = new OneceMoveCommand(wheelAngle, dirFlag);

                    tempOnceMoveCmd.AddressPositions.Add(tempPostion);
                    tempOnceMoveCmd.AddressActions.Add(EnumAddressAction.ST);
                    tempOnceMoveCmd.SectionSpeedLimits.Add(moveCmd.SectionSpeedLimits[i]);

                    if (moveCmd.AddressActions[i] == EnumAddressAction.BR2000)
                        moveCmd.AddressActions[i] = EnumAddressAction.R2000;
                    else if (moveCmd.AddressActions[i] == EnumAddressAction.BTR50)
                        moveCmd.AddressActions[i] = EnumAddressAction.TR50;
                    else if (moveCmd.AddressActions[i] == EnumAddressAction.BTR350)
                        moveCmd.AddressActions[i] = EnumAddressAction.TR350;

                    tempOnceMoveCmd.AddressPositions.Add(moveCmd.AddressPositions[i]);
                    tempOnceMoveCmd.AddressActions.Add(moveCmd.AddressActions[i]);
                    tempOnceMoveCmd.SectionSpeedLimits.Add(moveCmd.SectionSpeedLimits[i]);
                    i++;
                }
            }

            WriteBreakDownMoveCommandList(oneceMoveCommandList);
            return AddTOCommandList(ref oneceMoveCommandList, ref moveCmdList, ref sectionLineList, reserveDataList, ref errorMessage);
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
            flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(分解命令) 開始~");

            try
            {
                NewReserveList(moveCmd.AddressPositions, ref reserveDataList);

                if (!BreakDownMoveCmd(moveCmd, ref moveCmdList, ref sectionLineList, reserveDataList, nowAGV, ref errorMessage))
                {
                    flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(分解命令) 失敗!\n");
                    return false;
                }
                else
                {
                    flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(分解命令) 成功!\n");
                    WriteListLog(moveCmdList, sectionLineList, reserveDataList);
                    return true;
                }
            }
            catch
            {
                flow.SavePureLog(System.Reflection.MethodBase.GetCurrentMethod().Name + "(分解命令) 失敗! (Excption)\n");
                return false;
            }
        }
    }
}