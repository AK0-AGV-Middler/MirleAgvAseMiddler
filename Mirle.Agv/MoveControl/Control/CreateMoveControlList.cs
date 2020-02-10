using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Model.TransferSteps;
using System;
using System.Collections.Generic;
using Mirle.Tools;

namespace Mirle.Agv.Controller
{
    public class CreateMoveControlList
    {
        private MoveControlConfig moveControlConfig;
        private ComputeFunction computeFunction = new ComputeFunction();
        private AlarmHandler alarmHandler;
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private string device = "MoveControl";
        private List<VChangeData> vChangeList;
        private List<Sr2000Config> sr2000Config;

        public string CreateCommandListLog { get; set; } = "";
        private const int createCommandListLogMaxLength = 50000;

        private Dictionary<int, List<MapBarcodeLine>> barcodeLineAngleData = new Dictionary<int, List<MapBarcodeLine>>();
        private double sr2000Width = 20;

        public CreateMoveControlList(List<Sr2000Driver> driverSr2000List, MoveControlConfig moveControlConfig, List<Sr2000Config> sr2000Config, AlarmHandler alarmHandler, Dictionary<string, MapBarcodeLine> barcodeLineData)
        {
            this.sr2000Config = sr2000Config;
            this.alarmHandler = alarmHandler;
            this.moveControlConfig = moveControlConfig;

            //if (moveControlConfig.Safety[ EnumMoveControlSafetyType.BarcodePositionSafety].Enable)
            ProcessBarcodeLineWithAngle(barcodeLineData);
        }

        private void SetCreateCommandListLog(string functionName, string message)
        {
            CreateCommandListLog = String.Concat(DateTime.Now.ToString("HH:mm:ss.fff"), "\t", functionName, "\t", message, "\r\n", CreateCommandListLog);

            if (CreateCommandListLog.Length > createCommandListLogMaxLength)
                CreateCommandListLog = CreateCommandListLog.Substring(0, createCommandListLogMaxLength);
        }

        private void WriteLog(string category, string logLevel, string device, string carrierId, string message,
                             [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string classMethodName = String.Concat(GetType().Name, ":", memberName);
            LogFormat logFormat = new LogFormat(category, logLevel, classMethodName, device, carrierId, message);

            mirleLogger.Log(logFormat);
            SetCreateCommandListLog(memberName, message);
        }

        #region NewCommand function
        public Command NewMoveCommand(MapPosition position, double realEncoder, double commandDistance, double commandVelocity, bool dirFlag, int StartWheelAngle, EnumMoveStartType moveType, int reserveNumber = -1)
        {
            Command returnCommand = new Command();
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.Move];
            returnCommand.TriggerEncoder = realEncoder - (dirFlag ? returnCommand.SafetyDistance / 2 : -returnCommand.SafetyDistance / 2);
            returnCommand.CmdType = EnumCommandType.Move;
            returnCommand.Distance = commandDistance;
            returnCommand.Velocity = commandVelocity;
            returnCommand.DirFlag = dirFlag;
            returnCommand.WheelAngle = StartWheelAngle;
            returnCommand.ReserveNumber = reserveNumber;
            returnCommand.MoveType = moveType;

            return returnCommand;
        }

        public Command NewVChangeCommand(MapPosition position, double realEncoder, double commandVelocity, bool dirFlag, EnumVChangeType vChangeType = EnumVChangeType.Normal, int wheelAngle = 0)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.Vchange];
            returnCommand.CmdType = EnumCommandType.Vchange;
            returnCommand.Velocity = commandVelocity;
            returnCommand.DirFlag = dirFlag;
            returnCommand.VChangeType = vChangeType;
            returnCommand.WheelAngle = wheelAngle;

            return returnCommand;
        }

        private Command NewR2000Command(MapPosition position, double realEncoder, bool dirFlag, int wheelAngle, EnumAddressAction type)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.R2000];
            returnCommand.TurnType = EnumAddressAction.R2000;
            returnCommand.CmdType = EnumCommandType.R2000;
            returnCommand.DirFlag = dirFlag;
            returnCommand.WheelAngle = wheelAngle;
            returnCommand.TurnType = type;

            return returnCommand;
        }

        private Command NewTRCommand(MapPosition position, double realEncoder, bool dirFlag, int wheelAngle, EnumAddressAction type)
        {

            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.TR];
            returnCommand.CmdType = EnumCommandType.TR;
            returnCommand.DirFlag = dirFlag;
            returnCommand.WheelAngle = wheelAngle;
            returnCommand.TurnType = type;

            return returnCommand;
        }

        public Command NewSlowStopCommand(MapPosition position, double realEncoder, bool dirFlag, int nextReserveNumber = -1)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.SlowStop];
            returnCommand.CmdType = EnumCommandType.SlowStop;
            returnCommand.ReserveNumber = -1;
            returnCommand.NextRserveCancel = (nextReserveNumber != -1);
            returnCommand.NextReserveNumber = nextReserveNumber;
            returnCommand.DirFlag = dirFlag;

            return returnCommand;
        }

        private Command NewEndCommand(MapPosition endPosition, double endEncoder, bool dirFlag)
        {
            Command returnCommand = new Command();
            returnCommand.Position = null;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.End];
            returnCommand.CmdType = EnumCommandType.End;
            returnCommand.DirFlag = dirFlag;
            returnCommand.EndPosition = endPosition;
            returnCommand.EndEncoder = endEncoder;

            return returnCommand;
        }

        private Command NewReviseOpenCommand()
        {
            Command returnCommand = new Command();
            returnCommand.Position = null;
            returnCommand.CmdType = EnumCommandType.ReviseOpen;

            return returnCommand;
        }

        private Command NewReviseCloseCommand(MapPosition position, double realEncoder, bool dirFlag, EnumAddressAction type = EnumAddressAction.ST)
        {
            Command returnCommand = new Command();
            returnCommand.Position = position;
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.CmdType = EnumCommandType.ReviseClose;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.ReviseClose];
            returnCommand.DirFlag = dirFlag;
            returnCommand.TurnType = type;

            return returnCommand;
        }

        public Command NewStopCommand(MapPosition position, double realEncoder, bool dirFlag, int nextReserveNumber = -1)
        {
            Command returnCommand = new Command();
            returnCommand.TriggerEncoder = realEncoder;
            returnCommand.Position = position;
            returnCommand.SafetyDistance = moveControlConfig.SafteyDistance[EnumCommandType.Stop];
            returnCommand.CmdType = EnumCommandType.Stop;
            returnCommand.ReserveNumber = -1;
            returnCommand.NextRserveCancel = (nextReserveNumber != -1);
            returnCommand.NextReserveNumber = nextReserveNumber;
            returnCommand.DirFlag = dirFlag;

            return returnCommand;
        }

        private void AddToVChangeList(ref List<Command> moveCmdList, Command command, AddToCommandListData data)
        {
            double startEncoder = 0;
            double startVelocity = 0;
            double endEncoder = 0;
            double velocityCommand = command.Velocity;
            VChangeData temp;

            if (command.Position != null)
                startEncoder = command.TriggerEncoder;
            else
            {
                for (int i = moveCmdList.Count - 1; i >= 0; i--)
                {
                    if (moveCmdList[i].Position != null)
                    {
                        double deltaEncoder = 0;

                        if (moveCmdList[i].CmdType == EnumCommandType.TR)
                            deltaEncoder = moveControlConfig.TurnParameter[moveCmdList[i].TurnType].R * 2;
                        else if (moveCmdList[i].CmdType == EnumCommandType.R2000)
                            deltaEncoder = moveControlConfig.TurnParameter[EnumAddressAction.R2000].R * Math.Sqrt(2);
                        else if (moveCmdList[i].CmdType == EnumCommandType.Move)
                            deltaEncoder = moveCmdList[i].SafetyDistance / 2;

                        startEncoder = moveCmdList[i].TriggerEncoder + (data.DirFlag ? deltaEncoder : -deltaEncoder);
                        break;
                    }
                }
            }

            if (vChangeList.Count != 0)
                startVelocity = vChangeList[vChangeList.Count - 1].VelocityCommand;

            if (startVelocity == velocityCommand)
                endEncoder = startEncoder;
            else
            {
                double distance = GetAccDecDistanceFormMove(startVelocity, command.Velocity);
                endEncoder = startEncoder + (data.DirFlag ? distance : -distance);
            }

            if (vChangeList.Count > 0 &&
               ((data.DirFlag && vChangeList[vChangeList.Count - 1].EndEncoder > startEncoder) ||
               (!data.DirFlag && vChangeList[vChangeList.Count - 1].EndEncoder < startEncoder)))
            {
                double velocity = 0;
                double distance = GetVChangeDistance(vChangeList[vChangeList.Count - 1].StartVelocity, velocityCommand,
                    vChangeList[vChangeList.Count - 1].VelocityCommand, Math.Abs(vChangeList[vChangeList.Count - 1].StartEncoder - startEncoder), ref velocity);

                OverRrideLastVChangeCommand(ref moveCmdList, data, velocity);
            }

            temp = new VChangeData(startEncoder, startVelocity, endEncoder, velocityCommand, command.VChangeType);
            vChangeList.Add(temp);
            data.NowVelocityCommand = temp.VelocityCommand;
        }

        private void AddCommandToCommandList(ref List<Command> moveCmdList, Command command, AddToCommandListData data)
        {
            if (command.CmdType == EnumCommandType.Vchange)
                AddToVChangeList(ref moveCmdList, command, data);

            if (command.Position == null)
            {
                moveCmdList.Add(command);
            }
            else
            {
                int inserIndex = data.StartMoveIndex;
                for (; inserIndex < moveCmdList.Count; inserIndex++)
                {
                    if (moveCmdList[inserIndex].Position != null)
                    {
                        if ((data.DirFlag && command.TriggerEncoder < moveCmdList[inserIndex].TriggerEncoder) ||
                           (!data.DirFlag && command.TriggerEncoder > moveCmdList[inserIndex].TriggerEncoder))
                            break;
                    }
                }

                moveCmdList.Insert(inserIndex, command);
            }
        }
        #endregion

        #region Write list log 
        private void WriteAGVMCommand(MoveCmdInfo moveCmd, bool NeedWriteAction)
        {
            string logMessage = "AGVM command資料 : ";

            try
            {
                if (moveCmd != null && moveCmd.AddressPositions.Count > 1)
                {
                    for (int i = 1; i < moveCmd.AddressPositions.Count; i++)
                    {
                        logMessage = String.Concat(logMessage, "\r\nAGVM 路線第 ", i.ToString(), " 條");

                        if (moveCmd.SectionIds != null && moveCmd.SectionIds.Count > i)
                            logMessage = String.Concat(logMessage, ", Section : ", moveCmd.SectionIds[i - 1]);

                        if (NeedWriteAction)
                            logMessage = String.Concat(logMessage, ", Action : ", moveCmd.AddressActions[i - 1].ToString(), " -> ", moveCmd.AddressActions[i].ToString(), ", from : ");

                        if (moveCmd.AddressIds != null && moveCmd.AddressIds.Count > i)
                            logMessage = String.Concat(logMessage, moveCmd.AddressIds[i - 1]);

                        logMessage = String.Concat(logMessage, " ( ", moveCmd.AddressPositions[i - 1].X.ToString("0"), ", ",
                                                          moveCmd.AddressPositions[i - 1].Y.ToString("0"), " ), to : ");

                        if (moveCmd.AddressIds != null && moveCmd.AddressIds.Count > i)
                            logMessage = String.Concat(logMessage, moveCmd.AddressIds[i]);

                        logMessage = String.Concat(logMessage, " ( ", moveCmd.AddressPositions[i].X.ToString("0"), ", ",
                                                          moveCmd.AddressPositions[i].Y.ToString("0"),
                                                  " ), velocity : ", moveCmd.SectionSpeedLimits[i - 1].ToString("0"));

                    }

                    WriteLog("MoveControl", "7", device, "", logMessage);

                    WriteLog("MoveControl", "7", device, "", String.Concat("起點Offset x = ", moveCmd.StartAddress.AddressOffset.OffsetX.ToString("0.0"),
                                                             ", y = ", moveCmd.StartAddress.AddressOffset.OffsetX.ToString("0.0"),
                                                             ",theta = ", moveCmd.StartAddress.AddressOffset.OffsetTheta.ToString("0.0")));

                    WriteLog("MoveControl", "7", device, "", String.Concat("終點Offset x = ", moveCmd.EndAddress.AddressOffset.OffsetX.ToString("0.0"),
                                                          ", y = ", moveCmd.EndAddress.AddressOffset.OffsetX.ToString("0.0"),
                                                          ",theta = ", moveCmd.EndAddress.AddressOffset.OffsetTheta.ToString("0.0")));
                }
                else
                {
                    WriteLog("MoveControl", "4", device, "", "AGVM command資料有問題(為null或address count <=1)");
                }
            }
            catch (Exception ex)
            {
                WriteLog("MoveControl", "3", device, "", String.Concat("AGVM command資料 異常end (Excption) ~ ", ex.ToString(), "\r\n目前資料 : ", logMessage));
            }
        }

        private void WriteBreakDownMoveCommandList(List<OneceMoveCommand> oneceMoveCommandList)
        {
            string logMessage = "BreakDownMoveCommandList :";

            for (int j = 0; j < oneceMoveCommandList.Count; j++)
            {
                for (int i = 1; i < oneceMoveCommandList[j].AddressPositions.Count; i++)
                {
                    logMessage = String.Concat(logMessage, "\r\n第 ", (j + 1).ToString(), " 次動令,第 " + i.ToString(),
                                     " 條路線 Action : ", oneceMoveCommandList[j].AddressActions[i - 1].ToString(), " -> ",
                                     oneceMoveCommandList[j].AddressActions[i].ToString(), ", from :  ( ",
                                     oneceMoveCommandList[j].AddressPositions[i - 1].X.ToString("0"), ", ",
                                     oneceMoveCommandList[j].AddressPositions[i - 1].Y.ToString("0"), " ), to :  ( ",
                                     oneceMoveCommandList[j].AddressPositions[i].X.ToString("0"), ", ",
                                     oneceMoveCommandList[j].AddressPositions[i].Y.ToString("0"), " ), velocity : ",
                                     oneceMoveCommandList[j].SectionSpeedLimits[i - 1].ToString("0"));
                }
            }

            WriteLog("MoveControl", "7", device, "", logMessage);
        }

        private void WriteReserveListLog(List<ReserveData> reserveDataList)
        {
            string logMessage = "ReserveList :";

            for (int i = 0; i < reserveDataList.Count; i++)
                logMessage = String.Concat(logMessage, "\r\nreserve node ", i.ToString(), " : ( ",
                                 reserveDataList[i].Position.X.ToString("0"), ", ",
                                 reserveDataList[i].Position.Y.ToString("0"), " )");

            WriteLog("MoveControl", "7", device, "", logMessage);
        }


        public void GetReserveListInfo(List<ReserveData> reserveDataList, ref List<string> logMessage)
        {
            string lineString;

            for (int i = 0; i < reserveDataList.Count; i++)
            {
                lineString = String.Concat("reserve node ", i.ToString(), " : ( ",
                    reserveDataList[i].Position.X.ToString("0"), ", ",
                    reserveDataList[i].Position.Y.ToString("0"), " )");
                logMessage.Add(lineString);
            }
        }

        private void TriggerLog(Command cmd, ref string logMessage)
        {
            logMessage = String.Concat(logMessage, "command type : ", cmd.CmdType.ToString());

            if (cmd.Position != null)
            {
                logMessage = String.Concat(logMessage, ", 觸發Encoder為 ", cmd.TriggerEncoder.ToString("0"), " ~ ",
                                          (cmd.TriggerEncoder + (cmd.DirFlag ? cmd.SafetyDistance : -cmd.SafetyDistance)).ToString("0"),
                                          ", position : ( ", cmd.Position.X.ToString("0"), ", " + cmd.Position.Y.ToString("0") + " )");

            }
            else
                logMessage = String.Concat(logMessage, ", 為立即觸發");
        }

        private void WritSectionLineListLog(List<SectionLine> sectionLineList)
        {
            string logMessage = "SectionLineList :";

            for (int i = 0; i < sectionLineList.Count; i++)
            {
                logMessage = String.Concat(logMessage, "\r\nsectionLineList 第 ", (i + 1).ToString(), " 條為 from : (",
                                 sectionLineList[i].Start.X.ToString("0"), ", ", sectionLineList[i].Start.Y.ToString("0"), " ), to : (",
                                 sectionLineList[i].End.X.ToString("0"), ", ", sectionLineList[i].End.Y.ToString("0"), " ), DirFlag : ",
                                 (sectionLineList[i].DirFlag ? "前進" : "後退"), ", Distance : ", sectionLineList[i].Distance.ToString("0"),
                                 ", EncoderStart : ", sectionLineList[i].EncoderStart.ToString("0"),
                                 ", EncoderEnd : ", sectionLineList[i].EncoderEnd.ToString("0"));
            }

            WriteLog("MoveControl", "7", device, "", logMessage);
        }

        private void WriteMoveCommandListLogTypeMove(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 啟動舵輪角度 : ", cmd.WheelAngle.ToString("0"), ", 方向 : ", (cmd.DirFlag ? "前進" : "後退"),
                                      ", 距離 : ", cmd.Distance.ToString("0"), ", 速度 : ", cmd.Velocity.ToString("0"));

            if (cmd.ReserveNumber != -1)
                logMessage = String.Concat(logMessage, ", Reserve index : ", cmd.ReserveNumber.ToString());

            if (cmd.NextRserveCancel)
                logMessage = String.Concat(logMessage, ", 取得Reserve index = ", cmd.NextReserveNumber.ToString(), "時取消此Command");
        }

        private void WriteMoveCommandListLogTypeReviseOpen(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            if (cmd.ReserveNumber != -1)
                logMessage = String.Concat(logMessage, ", Reserve index : ", cmd.ReserveNumber.ToString());

            if (cmd.NextRserveCancel)
                logMessage = String.Concat(logMessage, ", 取得Reserve index = ", cmd.NextReserveNumber.ToString(), "時取消此Command");
        }

        private void WriteMoveCommandListLogTypeReviseClose(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);
        }

        private void WriteMoveCommandListLogTypeTR(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 為TR ", moveControlConfig.TurnParameter[cmd.TurnType].R.ToString("0"), ", 速度 : ", moveControlConfig.TurnParameter[cmd.TurnType].Velocity.ToString("0"),
                                      ", 舵輪將轉為 : ", cmd.WheelAngle.ToString("0"), ", 方向 : ", (cmd.DirFlag ? "前進" : "後退"));

            if (cmd.ReserveNumber != -1)
                logMessage = String.Concat(logMessage, ", Reserve index : ", cmd.ReserveNumber.ToString());
        }

        private void WriteMoveCommandListLogTypeR2000(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 為R ", moveControlConfig.TurnParameter[cmd.TurnType].R.ToString("0"), ", 速度 : ", moveControlConfig.TurnParameter[cmd.TurnType].Velocity.ToString("0"),
                                      ", 前後輪子為向", (cmd.WheelAngle == -1 ? "右" : "左"), "轉, 方向 : ", (cmd.DirFlag ? "前進" : "後退"));

            if (cmd.ReserveNumber != -1)
                logMessage = String.Concat(logMessage, ", Reserve index : ", cmd.ReserveNumber.ToString());
        }

        private void WriteMoveCommandListLogTypeVchange(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 方向 : ", (cmd.DirFlag ? "前進" : "後退"), ", 速度變更為 : ", cmd.Velocity.ToString("0"));

            if (cmd.VChangeType == EnumVChangeType.TRTurn)
                logMessage = String.Concat(logMessage, ", 為TR前的 VChange, 舵輪將轉為 : ", cmd.WheelAngle.ToString("0"));

            if (cmd.ReserveNumber != -1)
                logMessage = String.Concat(logMessage, ", Reserve index : ", cmd.ReserveNumber.ToString());
        }

        private void WriteMoveCommandListLogTypeSlowStop(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 方向 : ", (cmd.DirFlag ? "前進" : "後退"));

            if (cmd.ReserveNumber != -1)
                logMessage = String.Concat(logMessage, ", Reserve index : ", cmd.ReserveNumber.ToString());
        }

        private void WriteMoveCommandListLogTypeStop(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 方向 : ", (cmd.DirFlag ? "前進" : "後退"));

            if (cmd.ReserveNumber != -1)
                logMessage = String.Concat(logMessage, ", Reserve index : ", cmd.ReserveNumber.ToString());

            if (cmd.NextRserveCancel)
                logMessage = String.Concat(logMessage, ", 取得Reserve index = ", cmd.NextReserveNumber.ToString(), "時取消此Command");
        }

        private void WriteMoveCommandListLogTypeEnd(Command cmd, ref string logMessage)
        {
            TriggerLog(cmd, ref logMessage);

            logMessage = String.Concat(logMessage, ", 方向 : ", (cmd.DirFlag ? "前進" : "後退"),
                                      ", 終點Encoder : ", cmd.EndEncoder.ToString("0"), ", position : ( ", cmd.EndPosition.X.ToString("0"),
                                      ", ", cmd.EndPosition.Y.ToString("0"), " )");

            if (cmd.ReserveNumber != -1)
                logMessage = String.Concat(logMessage, ", Reserve index : ", cmd.ReserveNumber.ToString());
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
                totalLogMessage = String.Concat(totalLogMessage, "\r\n", logMessage[i]);

            WriteLog("MoveControl", "7", device, "", totalLogMessage);
        }

        private void WriteListLog(List<Command> moveCmdList, List<SectionLine> sectionLineList, List<ReserveData> reserveDataList)
        {
            WriteReserveListLog(reserveDataList);
            WritSectionLineListLog(sectionLineList);
            WriteMoveCommandListLog(moveCmdList);
        }
        #endregion

        #region Barcode最終保護機制測試
        private void ProcessBarcodeLineWithAngle(Dictionary<string, MapBarcodeLine> barcodeLineData)
        {
            int angle = 0;

            foreach (MapBarcodeLine barcodeLine in barcodeLineData.Values)
            {
                if (barcodeLine.Material == EnumBarcodeMaterial.Iron)
                {
                    angle = computeFunction.ComputeAngleInt(barcodeLine.HeadBarcode.Position, barcodeLine.TailBarcode.Position);
                    if (angle > 90)
                        angle -= 180;
                    else if (angle <= -90)
                        angle += 180;

                    if (!barcodeLineAngleData.ContainsKey(angle))
                    {
                        List<MapBarcodeLine> temp = new List<MapBarcodeLine>();
                        barcodeLineAngleData.Add(angle, temp);
                    }

                    barcodeLineAngleData[angle].Add(barcodeLine);
                }
            }
        }

        private bool GetReadThisBarcodeDistance(double start, double end, double barcodeLineStart, double barcodeLineEnd, bool dirFlag, double nowEncoder, ref double encoderStart, ref double encoderEnd)
        {
            double big;
            double small;

            double distanceSmall;
            double distanceBig;

            if (barcodeLineStart > barcodeLineEnd)
            {
                big = barcodeLineStart;
                small = barcodeLineEnd;
            }
            else
            {
                small = barcodeLineStart;
                big = barcodeLineEnd;
            }

            if (start > end)
            {
                distanceSmall = computeFunction.GetDistanceToStart(start, end, big);
                distanceBig = computeFunction.GetDistanceToStart(start, end, small);
            }
            else
            {
                distanceSmall = computeFunction.GetDistanceToStart(start, end, small);
                distanceBig = computeFunction.GetDistanceToStart(start, end, big);
            }

            if (distanceSmall == distanceBig)
                return false;

            encoderStart = nowEncoder + (dirFlag ? distanceSmall : -distanceSmall);
            encoderEnd = nowEncoder + (dirFlag ? distanceBig : -distanceBig);
            return true;
        }

        private void CreateBarcodeSafetyListOneLineMovingOneSR2000(MapPosition start, MapPosition end, bool dirFlag, double nowEncoder, ref List<BarcodeSafetyData> oneMoveList)
        {
            BarcodeSafetyData temp;
            int i;
            int angle = computeFunction.ComputeAngleInt(start, end);
            double encoderStart = 0;
            double encoderEnd = 0;

            if (angle <= -90)
                angle += 180;
            else if (angle > 90)
                angle -= 180;

            if (!barcodeLineAngleData.ContainsKey(angle))
                return;

            if (angle == 0)
            {
                foreach (MapBarcodeLine tempLine in barcodeLineAngleData[angle])
                {
                    if (Math.Abs(tempLine.HeadBarcode.Position.Y - start.Y) <= sr2000Width &&
                        GetReadThisBarcodeDistance(start.X, end.X, tempLine.HeadBarcode.Position.X,
                                      tempLine.TailBarcode.Position.X, dirFlag, nowEncoder, ref encoderStart, ref encoderEnd))
                    {
                        temp = new BarcodeSafetyData(encoderStart, encoderEnd, tempLine.Id, (Math.Abs(encoderStart - encoderEnd) > moveControlConfig.Safety[EnumMoveControlSafetyType.BarcodePositionSafety].Range ? true : false));

                        i = 0;

                        while (i < oneMoveList.Count)
                        {
                            if ((dirFlag && temp.StartEncoder < oneMoveList[i].StartEncoder) ||
                               (!dirFlag && temp.StartEncoder > oneMoveList[i].StartEncoder))
                            {
                                oneMoveList.Insert(i, temp);
                                break;
                            }

                            i++;
                        }

                        if (i == oneMoveList.Count)
                            oneMoveList.Add(temp);
                    }
                }
            }
            else if (angle == 90)
            {
                foreach (MapBarcodeLine tempLine in barcodeLineAngleData[angle])
                {
                    if (tempLine.Id == "32")
                        ;
                    if (Math.Abs(tempLine.HeadBarcode.Position.X - start.X) <= sr2000Width &&
                        GetReadThisBarcodeDistance(start.Y, end.Y, tempLine.HeadBarcode.Position.Y,
                                      tempLine.TailBarcode.Position.Y, dirFlag, nowEncoder, ref encoderStart, ref encoderEnd))
                    {
                        temp = new BarcodeSafetyData(encoderStart, encoderEnd, tempLine.Id, (Math.Abs(encoderStart - encoderEnd) > moveControlConfig.Safety[EnumMoveControlSafetyType.BarcodePositionSafety].Range ? true : false));

                        i = 0;

                        while (i < oneMoveList.Count)
                        {
                            if ((dirFlag && temp.StartEncoder < oneMoveList[i].StartEncoder) ||
                               (!dirFlag && temp.StartEncoder > oneMoveList[i].StartEncoder))
                            {
                                oneMoveList.Insert(i, temp);
                                break;
                            }

                            i++;
                        }

                        if (i == oneMoveList.Count)
                            oneMoveList.Add(temp);
                    }
                }
            }
        }

        private void CreateBarcodeSafetyListOneLineMoving(MapPosition start, MapPosition end, double nowEncoder, bool dirFlag, int agvAngle, double startByPassDistance,
               double endByPassDistance, ref List<BarcodeSafetyData> oneMoveListLeft, ref List<BarcodeSafetyData> oneMoveListRight)
        {
            if (endByPassDistance != 0)
                end = computeFunction.GetPositionFormEndDistance(start, end, endByPassDistance);

            if (startByPassDistance != 0)
                start = computeFunction.GetPositionFormEndDistance(end, start, startByPassDistance);

            double deltaX = 0;
            double deltaY = 0;
            MapPosition tempStart;
            MapPosition tempEnd;
            // left:
            deltaX = Math.Cos((sr2000Config[0].ReaderToCenterDegree + agvAngle) / 180 * Math.PI) * sr2000Config[0].ReaderToCenterDistance;
            deltaY = -Math.Sin((sr2000Config[0].ReaderToCenterDegree + agvAngle) / 180 * Math.PI) * sr2000Config[0].ReaderToCenterDistance;
            tempStart = new MapPosition(start.X + deltaX, start.Y + deltaY);
            tempEnd = new MapPosition(end.X + deltaX, end.Y + deltaY);
            CreateBarcodeSafetyListOneLineMovingOneSR2000(tempStart, tempEnd, dirFlag, nowEncoder, ref oneMoveListLeft);
            // right:
            deltaX = Math.Cos((sr2000Config[1].ReaderToCenterDegree + agvAngle) / 180 * Math.PI) * sr2000Config[1].ReaderToCenterDistance;
            deltaY = -Math.Sin((sr2000Config[1].ReaderToCenterDegree + agvAngle) / 180 * Math.PI) * sr2000Config[1].ReaderToCenterDistance;
            tempStart = new MapPosition(start.X + deltaX, start.Y + deltaY);
            tempEnd = new MapPosition(end.X + deltaX, end.Y + deltaY);
            CreateBarcodeSafetyListOneLineMovingOneSR2000(tempStart, tempEnd, dirFlag, nowEncoder, ref oneMoveListRight);

        }

        private bool CreateBarcodeSafetyListOneMoving(OneceMoveCommand onceMoveCommand, ref int agvAngle, ref double nowEncoder,
                      ref List<BarcodeSafetyData> oneMoveListLeft, ref List<BarcodeSafetyData> oneMoveListRight, ref string errorMessage)
        {
            try
            {
                oneMoveListLeft = new List<BarcodeSafetyData>();
                oneMoveListRight = new List<BarcodeSafetyData>();
                int wheelAngle = onceMoveCommand.WheelAngle;
                double startByPassDistance = 0;
                double endByPassDistance = 0;
                double distance = 0;
                int startIndex = 0;

                for (int i = 0; i < onceMoveCommand.AddressActions.Count; i++)
                {
                    switch (onceMoveCommand.AddressActions[i])
                    {
                        case EnumAddressAction.ST:
                            break;
                        case EnumAddressAction.TR350:
                        case EnumAddressAction.TR50:
                            endByPassDistance = moveControlConfig.TurnParameter[onceMoveCommand.AddressActions[i]].R;

                            CreateBarcodeSafetyListOneLineMoving(onceMoveCommand.AddressPositions[startIndex], onceMoveCommand.AddressPositions[i],
                                           nowEncoder, onceMoveCommand.DirFlag, agvAngle, startByPassDistance, endByPassDistance, ref oneMoveListLeft, ref oneMoveListRight);
                            startByPassDistance = endByPassDistance;
                            endByPassDistance = 0;

                            wheelAngle = computeFunction.GetTurnWheelAngle(wheelAngle, onceMoveCommand.AddressPositions[startIndex],
                                onceMoveCommand.AddressPositions[i], onceMoveCommand.AddressPositions[i + 1], ref errorMessage);

                            if (wheelAngle == -1)
                                return false;

                            distance = computeFunction.GetTwoPositionDistance(onceMoveCommand.AddressPositions[startIndex], onceMoveCommand.AddressPositions[i]);
                            nowEncoder = nowEncoder + (onceMoveCommand.DirFlag ? distance : -distance);

                            startIndex = i;
                            break;
                        case EnumAddressAction.R2000:
                            CreateBarcodeSafetyListOneLineMoving(onceMoveCommand.AddressPositions[startIndex], onceMoveCommand.AddressPositions[i],
                                                nowEncoder, onceMoveCommand.DirFlag, agvAngle, startByPassDistance, endByPassDistance, ref oneMoveListLeft, ref oneMoveListRight);
                            startByPassDistance = 0;
                            endByPassDistance = 0;

                            distance = computeFunction.GetTwoPositionDistance(onceMoveCommand.AddressPositions[startIndex], onceMoveCommand.AddressPositions[i]);
                            nowEncoder = nowEncoder + (onceMoveCommand.DirFlag ? distance : -distance);

                            distance = computeFunction.GetTwoPositionDistance(onceMoveCommand.AddressPositions[i], onceMoveCommand.AddressPositions[i + 1]);
                            nowEncoder = nowEncoder + (onceMoveCommand.DirFlag ? distance : -distance);

                            agvAngle = computeFunction.GetAGVAngleAfterR2000(agvAngle, onceMoveCommand.DirFlag, onceMoveCommand.AddressPositions[i], onceMoveCommand.AddressPositions[i + 1], ref errorMessage);
                            if (wheelAngle == -1)
                                return false;

                            i++;
                            startIndex = i;

                            break;
                        case EnumAddressAction.SlowStop:
                        case EnumAddressAction.End:
                            CreateBarcodeSafetyListOneLineMoving(onceMoveCommand.AddressPositions[startIndex], onceMoveCommand.AddressPositions[i],
                    nowEncoder, onceMoveCommand.DirFlag, agvAngle, startByPassDistance, endByPassDistance, ref oneMoveListLeft, ref oneMoveListRight);

                            distance = computeFunction.GetTwoPositionDistance(onceMoveCommand.AddressPositions[startIndex], onceMoveCommand.AddressPositions[i]);
                            nowEncoder = nowEncoder + (onceMoveCommand.DirFlag ? distance : -distance);
                            break;

                        default:
                            break;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CreateBarcodeSafetyList(List<OneceMoveCommand> oneceMoveCommandList, int agvAngle,
                             ref List<List<BarcodeSafetyData>> sr2000LeftList, ref List<List<BarcodeSafetyData>> sr2000RightList,
                             ref string errorMessage)
        {
            List<BarcodeSafetyData> tempLeft = new List<BarcodeSafetyData>();
            List<BarcodeSafetyData> tempRight = new List<BarcodeSafetyData>();
            double nowEncoder = 0;

            for (int i = 0; i < oneceMoveCommandList.Count; i++)
            {
                if (!CreateBarcodeSafetyListOneMoving(oneceMoveCommandList[i], ref agvAngle, ref nowEncoder, ref tempLeft, ref tempRight, ref errorMessage))
                    return false;

                sr2000LeftList.Add(tempLeft);
                sr2000RightList.Add(tempRight);
            }

            return true;
        }
        #endregion

        public double GetAccDecDistanceFormMove(double startVel, double endVel)
        {
            double accOrDec = 0;
            double jerk = moveControlConfig.Move.Jerk;

            if (startVel > endVel)
                accOrDec = moveControlConfig.Move.Deceleration;
            else if (startVel < endVel)
                accOrDec = moveControlConfig.Move.Acceleration;

            return computeFunction.GetAccDecDistance(startVel, endVel, accOrDec, jerk);
        }

        private int FindLastVChangeCommand(List<Command> moveCmdList)
        {
            for (int i = moveCmdList.Count - 1; i >= 0; i--)
            {
                if (moveCmdList[i].CmdType == EnumCommandType.Vchange)
                    return i;
            }

            return -1;
        }

        private void OverRrideLastVChangeCommand(ref List<Command> moveCmdList, AddToCommandListData data, double velocity, EnumVChangeType type = EnumVChangeType.Normal, int wheelAngle = 0)
        {
            int index = FindLastVChangeCommand(moveCmdList);

            if (index != -1)
            {
                moveCmdList[index].Velocity = velocity;
                moveCmdList[index].VChangeType = type;
                moveCmdList[index].WheelAngle = wheelAngle;
                vChangeList[vChangeList.Count - 1].VelocityCommand = velocity;
                double distance = GetAccDecDistanceFormMove(vChangeList[vChangeList.Count - 1].StartVelocity, vChangeList[vChangeList.Count - 1].VelocityCommand);
                vChangeList[vChangeList.Count - 1].EndEncoder = vChangeList[vChangeList.Count - 1].StartEncoder + (data.DirFlag ? distance : -distance);
            }
        }

        private void OverRrideLastVChangeCommand(ref List<Command> moveCmdList, AddToCommandListData data, double velocity)
        {
            int index = FindLastVChangeCommand(moveCmdList);

            if (index != -1)
            {
                moveCmdList[index].Velocity = velocity;
                vChangeList[vChangeList.Count - 1].VelocityCommand = velocity;
                double distance = GetAccDecDistanceFormMove(vChangeList[vChangeList.Count - 1].StartVelocity, vChangeList[vChangeList.Count - 1].VelocityCommand);
                vChangeList[vChangeList.Count - 1].EndEncoder = vChangeList[vChangeList.Count - 1].StartEncoder + (data.DirFlag ? distance : -distance);
            }
        }

        private void RemoveLastVChangeCommand(ref List<Command> moveCmdList)
        {
            int index = FindLastVChangeCommand(moveCmdList);

            if (index != -1)
                moveCmdList.RemoveAt(index);
        }

        private void ProcessVChangeCommand(ref List<Command> moveCmdList, ref Command command, AddToCommandListData data, MapPosition position)
        {
            MapPosition triggerPosition;
            if (vChangeList.Count == 0)
                return;

            VChangeData lastVChange = vChangeList[vChangeList.Count - 1];
            double distanceToPosition = command.TriggerEncoder;
            double distance;
            double triggerEncoder;

            if (lastVChange.VelocityCommand == command.Velocity)
            {
                triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, distanceToPosition);
                command.TriggerEncoder = data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distanceToPosition : -(data.CommandDistance - distanceToPosition));
                command.Position = triggerPosition;
                AddCommandToCommandList(ref moveCmdList, command, data);
            }
            else
            {
                distance = GetAccDecDistanceFormMove(lastVChange.VelocityCommand, command.Velocity) + distanceToPosition;
                triggerEncoder = data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance));

                if ((data.DirFlag && triggerEncoder > lastVChange.EndEncoder) ||
                    (!data.DirFlag && triggerEncoder < lastVChange.EndEncoder))
                { // 正常情況.
                    triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, distance);
                    command.TriggerEncoder = triggerEncoder;
                    command.Position = triggerPosition;
                    AddCommandToCommandList(ref moveCmdList, command, data);
                }
                else
                { // 上次速度命令無法執行完.
                    distance = GetAccDecDistanceFormMove(lastVChange.StartVelocity, command.Velocity) + distanceToPosition;
                    triggerEncoder = data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance));

                    if ((data.DirFlag && triggerEncoder > lastVChange.StartEncoder) ||
                        (!data.DirFlag && triggerEncoder < lastVChange.StartEncoder))
                    { // 上次變速命令可以執行,但是需要修改速度.
                        double velocity = 0;
                        double realDistance = Math.Abs((data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance : -data.CommandDistance)) - lastVChange.StartEncoder) - distanceToPosition;
                        distance = GetVChangeDistance(lastVChange.StartVelocity, command.Velocity, lastVChange.VelocityCommand, realDistance, ref velocity) + distanceToPosition;
                        distance = GetAccDecDistanceFormMove(velocity, command.Velocity) + distanceToPosition;

                        if (lastVChange.StartVelocity < velocity && velocity < command.Velocity)
                        {
                            OverRrideLastVChangeCommand(ref moveCmdList, data, command.Velocity);
                        }
                        else
                        {
                            triggerEncoder = data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance));
                            triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, distance);
                            command.TriggerEncoder = triggerEncoder;
                            command.Position = triggerPosition;
                            OverRrideLastVChangeCommand(ref moveCmdList, data, velocity);
                            data.NowVelocityCommand = velocity;
                            AddCommandToCommandList(ref moveCmdList, command, data);
                        }
                    }
                    else
                    { // 上次速度命令無法執行.
                        if (lastVChange.Type == EnumVChangeType.TurnOut)
                        {
                            int index = FindLastVChangeCommand(moveCmdList);
                            command.VChangeType = EnumVChangeType.TurnOut;
                            command.TriggerEncoder = moveCmdList[index].TriggerEncoder;
                            command.Position = moveCmdList[index].Position;
                            RemoveLastVChangeCommand(ref moveCmdList);
                            vChangeList.RemoveAt(vChangeList.Count - 1);
                            AddCommandToCommandList(ref moveCmdList, command, data);
                        }
                        else if (vChangeList.Count == 1)
                        {
                            OverRrideLastVChangeCommand(ref moveCmdList, data, command.Velocity);
                        }
                        else
                        {
                            RemoveLastVChangeCommand(ref moveCmdList);
                            vChangeList.RemoveAt(vChangeList.Count - 1);
                            ProcessVChangeCommand(ref moveCmdList, ref command, data, position);
                        }
                    }
                }
            }
        }

        private bool ProcessStopCommand(ref List<Command> moveCmdList, ref Command command, AddToCommandListData data, MapPosition position, double nextVelocity)
        {
            MapPosition triggerPosition;
            VChangeData lastVChange;
            double distanceToPosition = command.TriggerEncoder;
            double distance;
            double triggerEncoder;

            for (int i = vChangeList.Count - 1; i >= 0; i--)
            {
                lastVChange = vChangeList[i];

                distance = GetAccDecDistanceFormMove(lastVChange.VelocityCommand, 0) +
                           GetAccDecDistanceFormMove(0, nextVelocity) + distanceToPosition;
                triggerEncoder = data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance));

                if ((data.DirFlag && triggerEncoder > lastVChange.EndEncoder) ||
                    (!data.DirFlag && triggerEncoder < lastVChange.EndEncoder))
                { // 正常情況.
                    triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, distance);
                    command.TriggerEncoder = triggerEncoder;
                    command.Position = triggerPosition;
                    return distance < data.STDistance - data.TurnOutDistance;
                }
                else
                { // 上次速度命令無法執行完.
                    distance = GetAccDecDistanceFormMove(lastVChange.StartVelocity, 0) +
                               GetAccDecDistanceFormMove(0, nextVelocity) + distanceToPosition;

                    triggerEncoder = data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance));

                    if ((data.DirFlag && triggerEncoder > lastVChange.StartEncoder) ||
                        (!data.DirFlag && triggerEncoder < lastVChange.StartEncoder))
                    { // 上次變速命令可以執行,但是需要修改速度.
                        double velocity = 0;
                        distance = GetVChangeDistance(lastVChange.StartVelocity, 0, lastVChange.VelocityCommand, data.CommandDistance - data.TurnOutDistance - distanceToPosition, ref velocity) +
                                   GetAccDecDistanceFormMove(0, nextVelocity) + distanceToPosition;

                        distance = GetAccDecDistanceFormMove(velocity, 0) + GetAccDecDistanceFormMove(0, nextVelocity) + distanceToPosition;

                        triggerEncoder = data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance));
                        triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, distance);
                        command.TriggerEncoder = triggerEncoder;
                        command.Position = triggerPosition;
                        data.NowVelocityCommand = velocity;
                        return distance < data.STDistance - data.TurnOutDistance;
                    }
                }
            }

            return false;
        }

        public double GetFirstVChangeCommandVelocity(double moveCommandVelocity,
            double firstVChangeDistance, double firstVChangeVelocity, double secondVChangeDistance)
        {
            double nowVelocity = 0;
            double tempNowVelocity = 0;

            nowVelocity = GetNowVelocity(nowVelocity, moveCommandVelocity, firstVChangeDistance);

            tempNowVelocity = GetNowVelocity(nowVelocity, firstVChangeVelocity, secondVChangeDistance - firstVChangeDistance);

            if (tempNowVelocity != firstVChangeVelocity)
            {
                if (moveCommandVelocity < firstVChangeVelocity)
                {  // 升速.
                    return (int)(tempNowVelocity / 100) * 100;
                }
                else
                {  // 降速.
                    return (int)(tempNowVelocity / 100) * 100 + 100;
                }
            }
            else
                return firstVChangeVelocity;
        }

        public double GetSLowStopDistance()
        {
            return GetAccDecDistanceFormMove(moveControlConfig.EQ.Velocity, 0);
        }

        public double GetVChangeDistance(double startVel, double endVel, double targetVel, double distance, ref double velocity)
        {
            double jerk = moveControlConfig.Move.Jerk;
            double acc = moveControlConfig.Move.Acceleration;
            double dec = moveControlConfig.Move.Deceleration;
            velocity = targetVel;

            double accDistance = 0;
            double decDistance = 0;
            double accDecDistance = 0;

            decDistance = GetAccDecDistanceFormMove(targetVel, endVel);
            accDistance = GetAccDecDistanceFormMove(startVel, targetVel);

            if (accDistance + decDistance < distance)
                return decDistance;

            decDistance = 0;
            accDistance = 0;
            //tempVel = (startVel < endVel) ? startVel : endVel;
            velocity = startVel;
            double vel = 0;

            for (; accDistance + decDistance + accDecDistance < distance; velocity += 5)
            {
                accDecDistance = computeFunction.GetDecDistanceOneJerk(velocity, endVel,
                           moveControlConfig.Move.Deceleration, moveControlConfig.Move.Jerk, ref vel);
                decDistance = GetAccDecDistanceFormMove(velocity, endVel);
                accDistance = GetAccDecDistanceFormMove(startVel, velocity);
            }

            return decDistance + accDecDistance;
        }

        private int GetReserveIndex(List<ReserveData> reserveDataList, MapPosition position)
        {
            if (reserveDataList == null)
                return -1;

            for (int i = 0; i < reserveDataList.Count; i++)
            {
                if (position.X == reserveDataList[i].Position.X &&
                    position.Y == reserveDataList[i].Position.Y)
                {
                    if (reserveDataList[i].GetReserve)
                        return -1;
                    else
                    {
                        if (i > 0 && (reserveDataList[i - 1].Action == EnumAddressAction.R2000 ||
                                      reserveDataList[i - 1].Action == EnumAddressAction.BR2000))
                        {
                            reserveDataList[i].GetReserve = true;
                            return -1;
                        }
                        else
                        {
                            reserveDataList[i].GetReserve = true;
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        private bool CheckNotInTRTurn(OneceMoveCommand oneceMoveCommand, List<ReserveData> reserveDataList, int index, int reserveIndex)
        {
            if (reserveIndex == -1)
                return false;

            double totalDistance;
            double accDistance;
            //distance = distance + vChangeSafetyDistance + r + accDistance + moveControlConfig.ReserveSafetyDistance;
            for (int i = index; i >= 0; i--)
            {
                if (oneceMoveCommand.AddressActions[i] == EnumAddressAction.TR350 ||
                    oneceMoveCommand.AddressActions[i] == EnumAddressAction.TR50)
                {
                    double distance = Math.Sqrt(Math.Pow(oneceMoveCommand.AddressPositions[i].X - reserveDataList[reserveIndex].Position.X, 2) +
                                                Math.Pow(oneceMoveCommand.AddressPositions[i].Y - reserveDataList[reserveIndex].Position.Y, 2));

                    accDistance = GetAccDecDistanceFormMove(0, moveControlConfig.TurnParameter[oneceMoveCommand.AddressActions[i]].Velocity);

                    totalDistance = moveControlConfig.TurnParameter[oneceMoveCommand.AddressActions[i]].R +
                                    moveControlConfig.TurnParameter[oneceMoveCommand.AddressActions[i]].VChangeSafetyDistance +
                                    accDistance + moveControlConfig.ReserveSafetyDistance;

                    if (distance < totalDistance)
                        return false;
                }
            }

            for (int i = index; i < oneceMoveCommand.AddressPositions.Count; i++)
            {
                if (oneceMoveCommand.AddressActions[i] == EnumAddressAction.TR350 ||
                    oneceMoveCommand.AddressActions[i] == EnumAddressAction.TR50)
                {
                    double distance = Math.Sqrt(Math.Pow(oneceMoveCommand.AddressPositions[i].X - reserveDataList[reserveIndex].Position.X, 2) +
                                                Math.Pow(oneceMoveCommand.AddressPositions[i].Y - reserveDataList[reserveIndex].Position.Y, 2));

                    if (distance < moveControlConfig.TurnParameter[oneceMoveCommand.AddressActions[i]].R)
                        return false;
                }
            }

            return true;
        }

        private bool AddOneMoveCommandToCommandListActionST(MapPosition position, EnumAddressAction action, double velocityCommand,
                     ref List<Command> moveCmdList, List<ReserveData> reserveDataList, ref AddToCommandListData data,
                     OneceMoveCommand oneceMoveCommand, int indexOfOneceMoveCommand, ref string errorMessage)
        {
            Command tempCommand;
            double r = moveControlConfig.TurnParameter[EnumAddressAction.TR350].R;
            double velocity = moveControlConfig.TurnParameter[EnumAddressAction.TR350].Velocity;
            double vChangeSafetyDistance = moveControlConfig.TurnParameter[EnumAddressAction.TR350].VChangeSafetyDistance;

            if (data.LastAction == EnumAddressAction.R2000)
            {

            }
            else
            {
                int reserveIndex = GetReserveIndex(reserveDataList, position);

                if (velocityCommand > data.NowVelocityCommand)
                {
                    tempCommand = NewVChangeCommand(position, data.MoveStartEncoder +
                                   (data.DirFlag ? data.CommandDistance : -data.CommandDistance), velocityCommand, data.DirFlag);
                    AddCommandToCommandList(ref moveCmdList, tempCommand, data);
                }
                else if (velocityCommand < data.NowVelocityCommand)
                {
                    tempCommand = NewVChangeCommand(null, 0, velocityCommand, data.DirFlag);
                    ProcessVChangeCommand(ref moveCmdList, ref tempCommand, data, position);
                }

                if (reserveIndex != -1 && CheckNotInTRTurn(oneceMoveCommand, reserveDataList, indexOfOneceMoveCommand, reserveIndex))
                {
                    tempCommand = NewStopCommand(null, vChangeSafetyDistance + r + moveControlConfig.ReserveSafetyDistance, data.DirFlag, reserveIndex + 1);

                    if (ProcessStopCommand(ref moveCmdList, ref tempCommand, data, position, velocity))
                        AddCommandToCommandList(ref moveCmdList, tempCommand, data);
                }
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
                OverRrideLastVChangeCommand(ref moveCmdList, data, moveControlConfig.EQ.Velocity);
                triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, GetSLowStopDistance());
                tempCommand = NewSlowStopCommand(triggerPosition, data.MoveStartEncoder +
                                              (data.DirFlag ? data.CommandDistance - GetSLowStopDistance() : -(data.CommandDistance - GetSLowStopDistance())),
                                               data.DirFlag);
                AddCommandToCommandList(ref moveCmdList, tempCommand, data);
            }
            else
            { // 距離不會太短之情況.

                if (data.NowVelocityCommand > moveControlConfig.EQ.Velocity)
                { // 需要降速.
                    distance = (action == EnumAddressAction.End ? moveControlConfig.EQ.Distance : moveControlConfig.NormalStopDistance) + GetSLowStopDistance();
                    tempCommand = NewVChangeCommand(null, distance, moveControlConfig.EQ.Velocity, data.DirFlag, (action == EnumAddressAction.End ? EnumVChangeType.EQ : EnumVChangeType.SlowStop));
                    ProcessVChangeCommand(ref moveCmdList, ref tempCommand, data, position);

                    int index = FindLastVChangeCommand(moveCmdList);
                    if (index != -1)
                        moveCmdList[index].NowVelocity = vChangeList[vChangeList.Count - 1].StartVelocity;
                }

                // 算出停止距離跟座標.
                distance = GetSLowStopDistance();
                triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, distance);

                // 插入停止指令.
                tempCommand = NewSlowStopCommand(triggerPosition,
                                data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance)),
                                data.DirFlag);
                AddCommandToCommandList(ref moveCmdList, tempCommand, data);
            }

            // 插入關修正命令.
            tempCommand = NewReviseCloseCommand(null, 0, data.DirFlag);
            AddCommandToCommandList(ref moveCmdList, tempCommand, data);

            // 是終點.
            if (action == EnumAddressAction.End)
            {
                // 二修命令.
                tempCommand = NewEndCommand(position, data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance : -data.CommandDistance), data.DirFlag);
                AddCommandToCommandList(ref moveCmdList, tempCommand, data);
            }

            data.TurnOutDistance = 0;
            return true;
        }

        private bool AddOneMoveCommandToCommandListActionTR(MapPosition position, EnumAddressAction action, double velocityCommand, MapPosition nextPosition,
                     ref List<Command> moveCmdList, List<ReserveData> reserveDataList, ref AddToCommandListData data, ref string errorMessage)
        {
            double r = moveControlConfig.TurnParameter[action].R;
            double velocity = moveControlConfig.TurnParameter[action].Velocity;
            double vChangeSafetyDistance = moveControlConfig.TurnParameter[action].VChangeSafetyDistance;
            double closeReviseDistance = moveControlConfig.TurnParameter[action].CloseReviseDistance;
            MapPosition triggerPosition;
            Command tempCommand;

            data.NowWheelAngle = computeFunction.GetTurnWheelAngle(data.NowWheelAngle, data.LastNode, position, nextPosition, ref errorMessage);

            if (data.NowWheelAngle == -1)
                return false;

            double distance = GetAccDecDistanceFormMove(data.NowVelocity, velocity);

            if (distance + 2 * moveControlConfig.TurnParameter[action].VChangeSafetyDistance + moveControlConfig.TurnParameter[action].R > data.STDistance - data.TurnOutDistance)
            {
                OverRrideLastVChangeCommand(ref moveCmdList, data, velocity, EnumVChangeType.TRTurn, data.NowWheelAngle);
            }
            else
            {
                tempCommand = NewVChangeCommand(null, r + vChangeSafetyDistance, velocity, data.DirFlag, EnumVChangeType.TRTurn, data.NowWheelAngle);
                ProcessVChangeCommand(ref moveCmdList, ref tempCommand, data, position);
            }

            distance = closeReviseDistance + r;

            triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, distance);
            tempCommand = NewReviseCloseCommand(triggerPosition,
                                 data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance)),
                                 data.DirFlag, action);

            AddCommandToCommandList(ref moveCmdList, tempCommand, data);

            triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, r);

            tempCommand = NewTRCommand(triggerPosition, data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - r : -(data.CommandDistance - r)),
                                       data.DirFlag, data.NowWheelAngle, action);

            AddCommandToCommandList(ref moveCmdList, tempCommand, data);

            int reserveIndex = GetReserveIndex(reserveDataList, position);

            if (reserveIndex != -1)
            {
                tempCommand = NewStopCommand(null, vChangeSafetyDistance + r + moveControlConfig.ReserveSafetyDistance, data.DirFlag, reserveIndex + 1);

                if (ProcessStopCommand(ref moveCmdList, ref tempCommand, data, position, velocity))
                    AddCommandToCommandList(ref moveCmdList, tempCommand, data);
            }

            tempCommand = NewVChangeCommand(null, 0, velocityCommand, data.DirFlag, EnumVChangeType.TurnOut);
            AddCommandToCommandList(ref moveCmdList, tempCommand, data);
            tempCommand = NewReviseOpenCommand();
            AddCommandToCommandList(ref moveCmdList, tempCommand, data);
            data.NowVelocity = velocity;
            data.TurnOutDistance = r;
            data.STDistance = 0;

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
            MapPosition triggerPosition;
            Command tempCommand;
            int wheelAngle = 0;

            int newAgvAngle = computeFunction.GetAGVAngleAfterR2000(data.AGVAngleInMap, data.DirFlag, position, nextPosition, ref errorMessage);

            if (newAgvAngle == -1)
                return false;

            if (!computeFunction.GetR2000IsTurnLeftOrRight(data.AGVAngleInMap, newAgvAngle, data.DirFlag, ref wheelAngle, ref errorMessage))
                return false;

            data.AGVAngleInMap = newAgvAngle;


            double distance = GetAccDecDistanceFormMove(data.NowVelocity, velocity);

            if (distance + 2 * moveControlConfig.TurnParameter[action].VChangeSafetyDistance > data.STDistance - data.TurnOutDistance)
            {
                OverRrideLastVChangeCommand(ref moveCmdList, data, velocity, EnumVChangeType.R2000Turn, wheelAngle);
            }
            else
            {
                tempCommand = NewVChangeCommand(null, vChangeSafetyDistance, velocity, data.DirFlag, EnumVChangeType.R2000Turn, wheelAngle);
                ProcessVChangeCommand(ref moveCmdList, ref tempCommand, data, position);
            }

            distance = closeReviseDistance;

            triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, distance);
            tempCommand = NewReviseCloseCommand(triggerPosition,
                                 data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance - distance : -(data.CommandDistance - distance)),
                                 data.DirFlag, action);


            AddCommandToCommandList(ref moveCmdList, tempCommand, data);

            triggerPosition = computeFunction.GetPositionFormEndDistance(data.LastNode, position, 0);

            tempCommand = NewR2000Command(triggerPosition, data.MoveStartEncoder + (data.DirFlag ? data.CommandDistance : -data.CommandDistance),
                                       data.DirFlag, wheelAngle, action);

            AddCommandToCommandList(ref moveCmdList, tempCommand, data);

            int reserveIndex = GetReserveIndex(reserveDataList, position);

            if (reserveIndex != -1)
            {
                tempCommand = NewStopCommand(null, vChangeSafetyDistance + moveControlConfig.ReserveSafetyDistance, data.DirFlag, reserveIndex + 1);

                if (ProcessStopCommand(ref moveCmdList, ref tempCommand, data, position, velocity))
                    AddCommandToCommandList(ref moveCmdList, tempCommand, data);
            }

            tempCommand = NewVChangeCommand(null, 0, nextVelocityCommand, data.DirFlag, EnumVChangeType.TurnOut);
            AddCommandToCommandList(ref moveCmdList, tempCommand, data);

            tempCommand = NewReviseOpenCommand();
            AddCommandToCommandList(ref moveCmdList, tempCommand, data);
            data.TurnOutDistance = 0;
            data.STDistance = 0;
            return true;
        }

        private bool AddOneMoveCommandToCommandList(OneceMoveCommand oneceMoveCommand, ref List<Command> moveCmdList,
                                                    List<ReserveData> reserveDataList, ref AddToCommandListData data, ref string errorMessage)
        {
            vChangeList = new List<VChangeData>();
            double tempDistance;
            data.StartWheelAngle = oneceMoveCommand.WheelAngle;
            data.NowWheelAngle = oneceMoveCommand.WheelAngle;
            data.DirFlag = oneceMoveCommand.DirFlag;
            data.CommandDistance = 0;
            data.NowVelocity = 0;
            data.NowVelocityCommand = 0;
            data.LastNode = null;
            data.STDistance = 0;

            Command tempCommand;

            // 插入Move指令.變速.開修正.
            tempCommand = NewMoveCommand(oneceMoveCommand.AddressPositions[0], data.MoveStartEncoder, 0,
                                              moveControlConfig.Move.Velocity, data.DirFlag, data.StartWheelAngle,
                                              (data.StartMoveIndex == 0 ? EnumMoveStartType.FirstMove : EnumMoveStartType.ChangeDirFlagMove),
                                              (data.StartMoveIndex == 0 ? 0 : -1));
            moveCmdList.Add(tempCommand);

            tempCommand = NewVChangeCommand(null, 0, oneceMoveCommand.SectionSpeedLimits[0], data.DirFlag);
            AddCommandToCommandList(ref moveCmdList, tempCommand, data);

            tempCommand = NewReviseOpenCommand();
            AddCommandToCommandList(ref moveCmdList, tempCommand, data);

            for (int i = 0; i < oneceMoveCommand.AddressPositions.Count; i++)
            {
                if (data.LastNode != null)
                {
                    // 更新總長和直線段長度.
                    tempDistance = Math.Sqrt(Math.Pow(data.LastNode.X - oneceMoveCommand.AddressPositions[i].X, 2) +
                                             Math.Pow(data.LastNode.Y - oneceMoveCommand.AddressPositions[i].Y, 2));
                    data.CommandDistance += tempDistance;
                    if (data.LastAction != EnumAddressAction.R2000)
                        data.STDistance += tempDistance;

                    switch (oneceMoveCommand.AddressActions[i])
                    {
                        case EnumAddressAction.ST:
                            if (!AddOneMoveCommandToCommandListActionST(oneceMoveCommand.AddressPositions[i], oneceMoveCommand.AddressActions[i],
                                 oneceMoveCommand.SectionSpeedLimits[i], ref moveCmdList, reserveDataList, ref data, oneceMoveCommand, i, ref errorMessage))
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

            // 計算出下一次移動命令的起始Encoder和需要插入Move的Index和方向改為反方向(只有反折會需要拆成多次命令吧?).
            moveCmdList[data.StartMoveIndex].Distance = data.CommandDistance * moveControlConfig.MoveCommandDistanceMagnification + moveControlConfig.MoveCommandDistanceConstant;
            data.MoveStartEncoder += (data.DirFlag ? data.CommandDistance : -data.CommandDistance);
            data.StartMoveIndex = moveCmdList.Count;
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
                        tempDouble = GetAccDecDistanceFormMove(0, moveControlConfig.TurnParameter[action].Velocity) +
                                                          moveControlConfig.TurnParameter[action].VChangeSafetyDistance + moveControlConfig.TurnParameter[action].R;
                        data.TurnInSafetyDistance.Add(action, tempDouble);

                        tempDouble = GetAccDecDistanceFormMove(0, moveControlConfig.TurnParameter[action].Velocity) +
                                                          moveControlConfig.TurnParameter[action].R;

                        data.TurnOutSafetyDistance.Add(action, tempDouble);
                    }
                    else if (action == EnumAddressAction.R2000)
                    {
                        tempDouble = GetAccDecDistanceFormMove(0, moveControlConfig.TurnParameter[action].Velocity) +
                                                          moveControlConfig.TurnParameter[action].VChangeSafetyDistance;
                        data.TurnInSafetyDistance.Add(action, tempDouble);

                        tempDouble = GetAccDecDistanceFormMove(0, moveControlConfig.TurnParameter[action].Velocity);
                        tempDouble = tempDouble + 100;
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
                double sectionAngle = computeFunction.ComputeAngle(startNode, endNode);
                SectionLine tempSectionLine = new SectionLine(startNode, endNode, sectionAngle, startEncoder, dirFlag, data.StartByPassDistance, data.EndByPassDistance);
                data.StartByPassDistance = 0;
                data.EndByPassDistance = 0;
                sectionLineList.Add(tempSectionLine);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "AddNewSectionLine Excption : " + ex.ToString() + "!";
                return false;
            }
        }

        private bool CheckFirstTurnDistance(MoveCmdInfo moveCmd, ref List<SectionLine> sectionLineList, ref BreakDownMoveCommandData data,
               ref List<OneceMoveCommand> oneceMoveCommandList, ref OneceMoveCommand tempOnceMoveCmd, ref string errorMessage)
        {
            // 第一次入彎才需要檢查距離是否不夠.
            double temp = data.TurnInOutDistance;

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
                if (data.TempDistance + 1 < data.TurnInOutDistance)
                {
                    if (moveCmd.AddressActions[data.Index] == EnumAddressAction.R2000)
                    {
                        // 必定是起點,從起點需要往後多少距離(正值).
                        data.BackDistance = data.TurnInOutDistance - data.TempDistance;

                        // 取得反折點的座標.
                        data.TempNode = computeFunction.GetReversePositionR2000(data.StartNode, data, true, data.BackDistance);

                        // SectionLineList加入從起點到反折點,且轉換座標只能是起點.
                        if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.TempNode, data.StartMoveEncoder, !data.DirFlag, ref errorMessage))
                            return false;

                        // 第一次移動命令加入在反折點SlowStop且移動方向改為反方向,加入到第一次移動命令.
                        tempOnceMoveCmd.AddressActions = new List<EnumAddressAction>();
                        tempOnceMoveCmd.AddressPositions = new List<MapPosition>();
                        tempOnceMoveCmd.SectionSpeedLimits = new List<double>();
                        AddOneceMoveCommand(ref tempOnceMoveCmd, data.StartNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index]);
                        AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.SlowStop, 0);
                        tempOnceMoveCmd.DirFlag = !tempOnceMoveCmd.DirFlag;
                        oneceMoveCommandList.Add(tempOnceMoveCmd);

                        // 宣告第二次移動命令.
                        tempOnceMoveCmd = new OneceMoveCommand(data.WheelAngle, data.DirFlag);
                        AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index]);
                        // 起點修改為反折點,SectionLine開始encoder設定,設定轉換座標的最小值起點offset.
                        data.StartNode = data.TempNode;
                        data.StartMoveEncoder = data.StartMoveEncoder + (!data.DirFlag ? data.BackDistance : -data.BackDistance);
                        data.StartByPassDistance = data.BackDistance;
                        // 重新該新此段SectionLine的距離.
                        data.TempDistance = Math.Sqrt(Math.Pow(data.EndNode.X - data.StartNode.X, 2) + Math.Pow(data.EndNode.Y - data.StartNode.Y, 2));

                    }
                    else
                    {
                        // 必定是起點,從起點需要往後多少距離(正值).
                        data.BackDistance = data.TurnInOutDistance - data.TempDistance;

                        // 取得反折點的座標.
                        data.TempNode = computeFunction.GetReversePosition(data.StartNode, data.EndNode, data.BackDistance);

                        // SectionLineList加入從起點到反折點,且轉換座標只能是起點.
                        if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.TempNode, data.StartMoveEncoder, !data.DirFlag, ref errorMessage))
                            return false;

                        // 第一次移動命令加入在反折點SlowStop且移動方向改為反方向,加入到第一次移動命令.
                        for ( int i = tempOnceMoveCmd.AddressPositions.Count -1; i > 0; i--)
                        {
                            tempOnceMoveCmd.AddressPositions.RemoveAt(i);
                            tempOnceMoveCmd.AddressActions.RemoveAt(i);
                            tempOnceMoveCmd.SectionSpeedLimits.RemoveAt(i);
                        }

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
            }

            data.TurnInOutDistance = temp;
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
                    data.WheelAngle = computeFunction.GetTurnWheelAngle(data.WheelAngle, data.StartNode, data.EndNode, data.NextNode, ref errorMessage);
                    if (data.WheelAngle == -1)
                        return false;
                }
                else
                {
                    data.NowAGVAngleInMap = computeFunction.GetAGVAngleAfterR2000(data.NowAGVAngleInMap, data.DirFlag, data.EndNode, data.NextNode, ref errorMessage);
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
                                  ref List<List<BarcodeSafetyData>> leftBarcodeList, ref List<List<BarcodeSafetyData>> rightBarcodeList,
                                      List<ReserveData> reserveDataList, AGVPosition nowAGV, int wheelAngle, ref string errorMessage)
        {
            List<OneceMoveCommand> oneceMoveCommandList = new List<OneceMoveCommand>();
            sectionLineList = new List<SectionLine>();

            BreakDownMoveCommandData data = new BreakDownMoveCommandData();
            data.StartNode = moveCmd.AddressPositions[0];

            // 確認啟動時的舵輪角度應該為多少、前進方向 retrun false表示不知道目前位置或者是角度偏差過大(10度).
            // 取得所有入彎、出彎所需要的距離.
            if (!computeFunction.GetDirFlagWheelAngle(moveCmd, ref data, nowAGV, wheelAngle, ref errorMessage) ||
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
                    if (moveCmd.AddressActions[data.Index - 1] == EnumAddressAction.R2000)
                    {
                        data.EndNode = moveCmd.AddressPositions[data.Index];
                        data.TempDistance = Math.Sqrt(Math.Pow(data.EndNode.X - data.StartNode.X, 2) + Math.Pow(data.EndNode.Y - data.StartNode.Y, 2));

                        if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.EndNode, data.StartMoveEncoder,
                                               data.DirFlag, ref errorMessage))
                            return false;

                        data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TempDistance : -data.TempDistance);
                        data.StartNode = data.EndNode;
                        AddOneceMoveCommand(ref tempOnceMoveCmd, data.StartNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index - 1]);
                    }

                    data.TempDistance = Math.Sqrt(Math.Pow(data.EndNode.X - data.StartNode.X, 2) + Math.Pow(data.EndNode.Y - data.StartNode.Y, 2));

                    // 如果出彎動作中且出彎距離不夠.
                    if (data.IsTurnOut && data.TempDistance < data.TurnOutSafetyDistance[data.TurnType])
                    {
                        // 計算出終點不夠的距離(正值).
                        data.TurnInOutDistance = data.TurnOutSafetyDistance[data.TurnType];
                        data.BackDistance = data.TurnInOutDistance - data.TempDistance;

                        // 計算出終點的反折點座標.
                        if (moveCmd.AddressActions[data.Index - 1] != EnumAddressAction.R2000)
                            data.TempNode = computeFunction.GetReversePosition(data.EndNode, data.StartNode, data.BackDistance);
                        else
                            data.TempNode = computeFunction.GetReversePositionR2000(data.EndNode, data, false, data.BackDistance);

                        // SectionLineList加入從start到反折點,且轉換座標只能是start到終點.
                        data.EndByPassDistance = data.BackDistance;
                        if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.TempNode, data.StartMoveEncoder,
                                               data.DirFlag, ref errorMessage))
                            return false;

                        // 此段移動命令加入SlowStop和反折點座標並加入到移動命令List內.
                        AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.SlowStop, 0);
                        oneceMoveCommandList.Add(tempOnceMoveCmd);

                        // 前進方向改成反方向、設定SectionLine開始encoder,設定startByPassDisance,設定新起點和新的啟動encoder和距離.
                        data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TurnOutSafetyDistance[data.TurnType] : -data.TurnOutSafetyDistance[data.TurnType]);
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
                    else if (moveCmd.AddressActions[data.Index] == EnumAddressAction.BST)
                        moveCmd.AddressActions[data.Index] = EnumAddressAction.ST;

                    // 後反折 : 先轉完彎在反折,轉彎前的舵輪角度不為0 (目前都是在舵輪0度時反折) 或 ST(BST:R2000完的反折).
                    if (moveCmd.AddressActions[data.Index] == EnumAddressAction.ST || data.WheelAngle != 0)
                    {
                        if (moveCmd.AddressActions[data.Index] == EnumAddressAction.ST)
                        { //
                            if (moveCmd.AddressActions[data.Index - 1] == EnumAddressAction.R2000)
                            {
                                // 先加入R2000終點BST->ST. 和SectionLineList
                                AddOneceMoveCommand(ref tempOnceMoveCmd, moveCmd.AddressPositions[data.Index],
                                                    moveCmd.AddressActions[data.Index], moveCmd.SectionSpeedLimits[data.Index - 1]);
                                data.Index++;
                                // 這樣出彎的速度會有問題.
                                //AddMoveCmdToOneceMoveCommand(moveCmd, ref tempOnceMoveCmd, ref data);
                                if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.EndNode, data.StartMoveEncoder,
                                                       data.DirFlag, ref errorMessage))
                                    return false;

                                data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TempDistance : -data.TempDistance);

                                // 算出R2000 出彎點到停止所需要的做短距離.
                                data.TurnInOutDistance = data.TurnOutSafetyDistance[EnumAddressAction.R2000];

                                // 取得即將反折的反折點座標.
                                data.TempNode = computeFunction.GetReversePositionR2000(data.EndNode, data, false, data.TurnInOutDistance);

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
                                AddOneceMoveCommand(ref tempOnceMoveCmd, data.TempNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index - 1]);
                                data.IsTurnOut = false;
                                data.StartByPassDistance = data.TurnInOutDistance;
                                data.StartNode = data.TempNode;
                            }
                            else
                            {
                                AddOneceMoveCommand(ref tempOnceMoveCmd, data.EndNode, EnumAddressAction.SlowStop, 0);
                                data.Index++;
                                oneceMoveCommandList.Add(tempOnceMoveCmd);

                                if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.EndNode, data.StartMoveEncoder,
                                                       data.DirFlag, ref errorMessage))
                                    return false;

                                data.StartMoveEncoder = data.StartMoveEncoder + (data.DirFlag ? data.TempDistance : -data.TempDistance);
                                data.DirFlag = !data.DirFlag;
                                tempOnceMoveCmd = new OneceMoveCommand(data.WheelAngle, data.DirFlag);
                                // 這邊速度必須是下一段Section的速度(因為不可能拿上一段的138太慢).
                                AddOneceMoveCommand(ref tempOnceMoveCmd, data.EndNode, EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index - 1]);
                                data.IsTurnOut = false;
                                data.StartByPassDistance = data.TurnInOutDistance;
                                data.StartNode = data.EndNode;
                            }
                        }
                        else if (moveCmd.AddressActions[data.Index] == EnumAddressAction.TR50 || moveCmd.AddressActions[data.Index] == EnumAddressAction.TR350)
                        {
                            // 算出TR node點到停止所需要的做短距離.
                            data.TurnInOutDistance = data.TurnOutSafetyDistance[moveCmd.AddressActions[data.Index]];

                            // 取得即將反折的反折點座標(會跟TR下一個Node點反方向).
                            data.NextNode = computeFunction.GetReversePosition(data.EndNode, moveCmd.AddressPositions[data.Index + 1], data.TurnInOutDistance);

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
                        data.TempNode = computeFunction.GetReversePosition(data.EndNode, data.StartNode, data.TurnInOutDistance);

                        // SectionLineList加入從start經過TR node點到反折點,且轉換座標只能是start到TR node點.
                        data.EndByPassDistance = data.TurnInOutDistance;
                        if (!AddNewSectionLine(ref sectionLineList, ref data, data.StartNode, data.TempNode, data.StartMoveEncoder,
                                               data.DirFlag, ref errorMessage))
                            return false;

                        // 在反折點加入SlowStop指令.
                        AddOneceMoveCommand(ref tempOnceMoveCmd, moveCmd.AddressPositions[data.Index], EnumAddressAction.ST, moveCmd.SectionSpeedLimits[data.Index - 1]);

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
                            if (moveCmd.SectionSpeedLimits[data.Index] != moveControlConfig.TurnParameter[EnumAddressAction.R2000].Velocity)
                            {
                                errorMessage = "R2000 速度必須要是138!";
                                return false;
                            }

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

            if (moveControlConfig.Safety[EnumMoveControlSafetyType.BarcodePositionSafety].Enable)
            {
                if (!CreateBarcodeSafetyList(oneceMoveCommandList, data.AGVAngleInMap, ref leftBarcodeList, ref rightBarcodeList, ref errorMessage))
                    return false;
            }

            return AddTOCommandList(ref oneceMoveCommandList, ref moveCmdList, reserveDataList, data.AGVAngleInMap, ref errorMessage);
        }

        private void NewReserveList(List<MapPosition> positionsList, List<EnumAddressAction> actionList, ref List<ReserveData> reserveDataList)
        {
            reserveDataList = new List<ReserveData>();
            ReserveData tempRserveData;

            for (int i = 1; i < positionsList.Count; i++)
            {
                tempRserveData = new ReserveData(positionsList[i], actionList[i]);
                reserveDataList.Add(tempRserveData);
            }
        }

        private void ResetReserveList(ref List<ReserveData> reserveDataList)
        {
            if (reserveDataList == null)
                return;

            for (int i = 0; i < reserveDataList.Count; i++)
                reserveDataList[i].GetReserve = false;
        }

        private void CommandListChangeReserveIndexToCurrectIndex(ref List<Command> moveCmdList, List<ReserveData> reserveList)
        {
            int nowIndex = reserveList.Count - 1;
            int temp = 0;
            for (int i = moveCmdList.Count - 1; i >= 0; i--)
            {
                if (moveCmdList[i].NextRserveCancel || moveCmdList[i].ReserveNumber != -1)
                {
                    if (moveCmdList[i].NextRserveCancel)
                    {
                        temp = moveCmdList[i].NextReserveNumber;
                        moveCmdList[i].NextReserveNumber = nowIndex;
                    }
                    else if (moveCmdList[i].ReserveNumber != -1)
                    {
                        temp = moveCmdList[i].ReserveNumber;
                        moveCmdList[i].ReserveNumber = nowIndex;
                    }

                    nowIndex = temp - 1;
                }
            }
        }

        private double GetNowVelocity(double startVelocity, double velocityCommand, double distance)
        {
            if (distance == 0)
                return startVelocity;

            double vChangeDistance;

            if (velocityCommand > startVelocity)
                vChangeDistance = GetAccDecDistanceFormMove(startVelocity, velocityCommand);
            else
                vChangeDistance = GetAccDecDistanceFormMove(startVelocity, velocityCommand);

            if (vChangeDistance <= distance)
                return velocityCommand;

            double deltaVelocity = (velocityCommand > startVelocity ? -1 : 1);

            for (; vChangeDistance > distance; velocityCommand += deltaVelocity)
            {
                if (velocityCommand > startVelocity)
                    vChangeDistance = GetAccDecDistanceFormMove(startVelocity, velocityCommand);
                else
                    vChangeDistance = GetAccDecDistanceFormMove(startVelocity, velocityCommand);
            }

            return velocityCommand;
        }

        public bool GetMoveCommandAddressAction(ref MoveCmdInfo moveCmd, AGVPosition nowAGV, int wheelAngle, ref string errorMessage, bool middlerSend = true)
        {
            moveCmd.AddressActions = new List<EnumAddressAction>();
            int lastSectionAngle = 0;
            int thisSectionAngle = 0;
            int newWheelAngle = 0;

            if (moveCmd.AddressPositions.Count < 2)
            {
                errorMessage = "只有" + moveCmd.AddressPositions.Count.ToString() + "個點,別鬧了!";
                return false;
            }

            if (moveCmd.MovingAddress == null || moveCmd.MovingAddress.Count != moveCmd.AddressPositions.Count)
            {
                errorMessage = (moveCmd.MovingAddress == null) ? "moveCmd.MovingAddress == null" :
                    "MovingAddress.Count : " + moveCmd.MovingAddress.Count.ToString() +
                    ",AddressPositions.Count : " + moveCmd.AddressPositions.Count.ToString();

                return false;
            }

            if (middlerSend && (moveCmd.MovingSections == null || moveCmd.MovingSections.Count != moveCmd.SectionSpeedLimits.Count))
            {
                errorMessage = (moveCmd.MovingSections == null) ? "moveCmd.MovingAddress == null" :
                    "MovingSections.Count : " + moveCmd.MovingSections.Count.ToString() +
                    ",SectionSpeedLimits.Count : " + moveCmd.SectionSpeedLimits.Count.ToString();

                return false;
            }

            BreakDownMoveCommandData data = new BreakDownMoveCommandData();

            if (moveCmd.AddressPositions[0].X == moveCmd.AddressPositions[1].X &&
                moveCmd.AddressPositions[0].Y == moveCmd.AddressPositions[1].Y)
            {
                errorMessage = "有相同Position!";
                return false;
            }

            lastSectionAngle = computeFunction.ComputeAngleInt(moveCmd.AddressPositions[0], moveCmd.AddressPositions[1]);

            if (moveCmd.MovingSections[0].Type == EnumSectionType.R2000)
                moveCmd.AddressActions.Add(EnumAddressAction.R2000);
            else
                moveCmd.AddressActions.Add(EnumAddressAction.ST);

            if (!computeFunction.GetDirFlagWheelAngle(moveCmd, ref data, nowAGV, wheelAngle, ref errorMessage))
                return false;

            if (moveCmd.MovingSections[0].Type == EnumSectionType.R2000)
            {
                data.AGVAngleInMap = computeFunction.GetAGVAngleAfterR2000(data.AGVAngleInMap, data.DirFlag, moveCmd.AddressPositions[0], moveCmd.AddressPositions[1], ref errorMessage);

                if (data.AGVAngleInMap == -1)
                    return false;
            }

            for (int i = 1; i < moveCmd.AddressPositions.Count - 1; i++)
            {
                if (moveCmd.AddressPositions[i].X == moveCmd.AddressPositions[i + 1].X &&
                    moveCmd.AddressPositions[i].Y == moveCmd.AddressPositions[i + 1].Y)
                {
                    errorMessage = "有相同Position!";
                    return false;
                }

                thisSectionAngle = computeFunction.ComputeAngleInt(moveCmd.AddressPositions[i], moveCmd.AddressPositions[i + 1]);

                if (moveCmd.MovingSections[i].Type == EnumSectionType.R2000)
                {
                    if (Math.Abs(computeFunction.GetCurrectAngle(thisSectionAngle - lastSectionAngle)) == 45)
                    {
                        moveCmd.AddressActions.Add(EnumAddressAction.R2000);
                    }
                    else if (Math.Abs(computeFunction.GetCurrectAngle(thisSectionAngle - (lastSectionAngle - 180))) == 45)
                    {
                        moveCmd.AddressActions.Add(EnumAddressAction.BR2000);
                        data.DirFlag = !data.DirFlag;
                    }
                    else
                    {
                        errorMessage = "奇怪角度R2000!";
                        return false;
                    }

                    data.AGVAngleInMap = computeFunction.GetAGVAngleAfterR2000(data.AGVAngleInMap, data.DirFlag, moveCmd.AddressPositions[i], moveCmd.AddressPositions[i + 1], ref errorMessage);

                    if (data.AGVAngleInMap == -1)
                        return false;
                }
                else
                {
                    switch (Math.Abs(computeFunction.GetCurrectAngle(thisSectionAngle - lastSectionAngle)))
                    {
                        case 0:
                            moveCmd.AddressActions.Add(EnumAddressAction.ST);
                            break;
                        case 90:
                            if (data.WheelAngle != 0 && data.WheelAngle != 90 && data.WheelAngle != -90)
                            {
                                errorMessage = "不該有舵輪不是0、90、-90度時Section轉90度的情況!";
                                return false;
                            }

                            newWheelAngle = computeFunction.GetTurnWheelAngle(data.WheelAngle, moveCmd.AddressPositions[i - 1],
                                moveCmd.AddressPositions[i], moveCmd.AddressPositions[i + 1], ref errorMessage);

                            if (newWheelAngle != -1)
                            {
                                data.WheelAngle = newWheelAngle;
                                moveCmd.AddressActions.Add(moveCmd.MovingAddress[i].IsTR50 ? EnumAddressAction.TR50 : EnumAddressAction.TR350);
                            }
                            else
                            {
                                data.WheelAngle = 0;
                                data.DirFlag = !data.DirFlag;
                                moveCmd.AddressActions.Add(moveCmd.MovingAddress[i].IsTR50 ? EnumAddressAction.BTR50 : EnumAddressAction.BTR350);
                            }

                            break;
                        case 180:
                            if (data.WheelAngle != 0)
                            {
                                errorMessage = "不該有舵輪不是0度時BST的情況!";
                                return false;
                            }

                            moveCmd.AddressActions.Add(EnumAddressAction.BST);
                            data.DirFlag = !data.DirFlag;
                            break;
                        case 30:
                        default:
                            errorMessage = "section 角度差異 " + Math.Abs(thisSectionAngle - lastSectionAngle).ToString() + " 度, 異常!";
                            return false;
                    }
                }

                if (moveCmd.AddressActions[i] == EnumAddressAction.R2000 || moveCmd.AddressActions[i] == EnumAddressAction.BR2000)
                {
                    lastSectionAngle = data.AGVAngleInMap;

                    if (!data.DirFlag)
                    {
                        if (lastSectionAngle > 0)
                            lastSectionAngle -= 180;
                        else
                            lastSectionAngle += 180;
                    }
                }
                else
                    lastSectionAngle = thisSectionAngle;
            }

            moveCmd.AddressActions.Add(EnumAddressAction.End);
            return true;
        }

        private bool CheckFirstAddressPositionLetItInSection(MoveCmdInfo moveCmd, ref string errorMessage)
        {
            if (moveCmd == null)
            {
                errorMessage = "moveCmd == null..";
                return false;
            }

            if (moveCmd.MovingSections == null)
            {
                errorMessage = "moveCmd.MovingSections == null..";
                return false;
            }

            if (moveCmd.MovingSections.Count == 0)
            {
                errorMessage = "moveCmd.MovingSections.Count == 0..";
                return false;
            }

            if (moveCmd.AddressPositions == null)
            {
                errorMessage = "moveCmd.AddressPositions == null..";
                return false;
            }

            if (moveCmd.AddressPositions.Count == 0)
            {
                errorMessage = "moveCmd.AddressPositions.Count == 0..";
                return false;
            }

            if (moveCmd.MovingSections[0].Type == EnumSectionType.R2000)
                return true;

            if (moveCmd.MovingSections[0].HeadAddress.Position.X == moveCmd.MovingSections[0].TailAddress.Position.X)
            {
                if (moveCmd.MovingSections[0].HeadAddress.Position.X == moveCmd.AddressPositions[0].X)
                    return true;

                if (moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Enable &&
                    Math.Abs(moveCmd.MovingSections[0].HeadAddress.Position.X - moveCmd.AddressPositions[0].X) >
                    moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range)
                {
                    errorMessage = "偏差過大..";
                    return false;
                }
                else
                {
                    WriteLog("MoveControl", "7", device, "", String.Concat("幫Middler壓回Section.. form : ( ",
                        moveCmd.AddressPositions[0].X.ToString("0.0"), ", ", moveCmd.AddressPositions[0].Y.ToString("0.0"), " to : ( ",
                        moveCmd.MovingSections[0].HeadAddress.Position.X.ToString("0.0"), ", ", moveCmd.AddressPositions[0].Y.ToString("0.0"), " ).."));

                    moveCmd.AddressPositions[0].X = moveCmd.MovingSections[0].HeadAddress.Position.X;

                    return true;
                }
            }
            else if (moveCmd.MovingSections[0].HeadAddress.Position.Y == moveCmd.MovingSections[0].TailAddress.Position.Y)
            {
                if (moveCmd.MovingSections[0].HeadAddress.Position.Y == moveCmd.AddressPositions[0].Y)
                    return true;

                if (moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Enable &&
                    Math.Abs(moveCmd.MovingSections[0].HeadAddress.Position.Y - moveCmd.AddressPositions[0].Y) >
                    moveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range)
                {
                    errorMessage = "偏差過大..";
                    return false;
                }
                else
                {
                    WriteLog("MoveControl", "7", device, "", String.Concat("幫Middler壓回Section.. form : ( ",
                        moveCmd.AddressPositions[0].X.ToString("0.0"), ", ", moveCmd.AddressPositions[0].Y.ToString("0.0"), " to : ( ",
                        moveCmd.AddressPositions[0].X.ToString("0.0"), ", ", moveCmd.MovingSections[0].HeadAddress.Position.Y.ToString("0.0"), " ).."));

                    moveCmd.AddressPositions[0].Y = moveCmd.MovingSections[0].HeadAddress.Position.Y;

                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        public MoveCommandData CreateMoveControlListSectionListReserveList(MoveCmdInfo moveCmd,
                                  AGVPosition nowAGV, int wheelAngle, ref string errorMessage, bool middlerSend = true)
        {
            try
            {
                System.Diagnostics.Stopwatch createListTimer = new System.Diagnostics.Stopwatch();
                createListTimer.Restart();

                if (middlerSend)
                {
                    //if (!CheckFirstAddressPositionLetItInSection(moveCmd, ref errorMessage))
                    //{
                    //    WriteLog("MoveControl", "3", device, "", "CheckFirstAddressPositionLetItInSection return false, errorMessage : " + errorMessage + " !");
                    //    return null;
                    //}

                    if (!GetMoveCommandAddressAction(ref moveCmd, nowAGV, wheelAngle, ref errorMessage))
                    {
                        WriteAGVMCommand(moveCmd, false);
                        WriteLog("MoveControl", "3", device, "", "GetMoveCommandAddressAction return false, errorMessage : " + errorMessage + " !");
                        return null;
                    }
                }
                else
                    WriteLog("MoveControl", "7", device, "", "debug form send, 因此略過node action計算!");

                if (moveCmd.SectionSpeedLimits == null || moveCmd.AddressActions == null || moveCmd.AddressPositions == null ||
                    moveCmd.SectionSpeedLimits.Count == 0 || (moveCmd.SectionSpeedLimits.Count + 1) != moveCmd.AddressPositions.Count ||
                    moveCmd.AddressActions.Count != moveCmd.AddressPositions.Count)
                {
                    WriteAGVMCommand(moveCmd, false);
                    errorMessage = "moveCmd的三種List(Action, Position, Speed)數量不正確!";
                    WriteLog("MoveControl", "7", device, "", "命令分解失敗, 分解時間 : " + createListTimer.ElapsedMilliseconds + "ms!");
                    return null;
                }
                else if (moveCmd.AddressActions[moveCmd.AddressActions.Count - 1] != EnumAddressAction.End)
                {
                    WriteAGVMCommand(moveCmd, false);
                    errorMessage = "Action結尾必須是End!";
                    WriteLog("MoveControl", "7", device, "", "命令分解失敗, 分解時間 : " + createListTimer.ElapsedMilliseconds + "ms!");
                    return null;
                }

                WriteAGVMCommand(moveCmd, true);

                List<ReserveData> reserveList = new List<ReserveData>();
                List<Command> moveCmdList = new List<Command>();
                List<SectionLine> sectionLineList = new List<SectionLine>();
                List<List<BarcodeSafetyData>> leftBarcodeList = new List<List<BarcodeSafetyData>>();
                List<List<BarcodeSafetyData>> rightBarcodeList = new List<List<BarcodeSafetyData>>();

                NewReserveList(moveCmd.AddressPositions, moveCmd.AddressActions, ref reserveList);

                for (int i = 0; i < moveCmd.SectionSpeedLimits.Count; i++)
                {
                    if (moveCmd.SectionSpeedLimits[i] > moveControlConfig.Move.Velocity)
                        moveCmd.SectionSpeedLimits[i] = moveControlConfig.Move.Velocity;
                }

                double distance = computeFunction.GetTwoPositionDistance(moveCmd.AddressPositions[moveCmd.AddressPositions.Count - 2], moveCmd.AddressPositions[moveCmd.AddressPositions.Count - 1]);

                if (distance > moveControlConfig.RetryMoveDistance)
                    distance = moveControlConfig.RetryMoveDistance;

                MapPosition retryMovePosition = computeFunction.GetPositionFormEndDistance(moveCmd.AddressPositions[moveCmd.AddressPositions.Count - 2], moveCmd.AddressPositions[moveCmd.AddressPositions.Count - 1], distance);

                if (BreakDownMoveCmd(moveCmd, ref moveCmdList, ref sectionLineList, ref leftBarcodeList, ref rightBarcodeList, reserveList, nowAGV, wheelAngle, ref errorMessage))
                {
                    ResetReserveList(ref reserveList);
                    CommandListChangeReserveIndexToCurrectIndex(ref moveCmdList, reserveList);
                    WriteListLog(moveCmdList, sectionLineList, reserveList);

                    MoveCommandData returnCommand = new MoveCommandData(moveCmdList, sectionLineList, reserveList, leftBarcodeList, rightBarcodeList);

                    if (moveCmd.EndAddress != null && moveCmd.EndAddress.AddressOffset != null)
                    {
                        returnCommand.EndOffsetX = moveCmd.EndAddress.AddressOffset.OffsetX;
                        returnCommand.EndOffsetY = moveCmd.EndAddress.AddressOffset.OffsetY;
                        returnCommand.EndOffsetTheta = moveCmd.EndAddress.AddressOffset.OffsetTheta;
                    }
                    else
                    {
                        WriteLog("MoveControl", "3", device, "", "moveCmd.EndAddress or moveCmd.EndAddress.AddressOffset == null!");
                    }

                    returnCommand.EndAddressLoadUnload = moveCmd.IsMoveEndDoLoadUnload;

                    if (moveCmd.StartAddress != null && moveCmd.StartAddress.AddressOffset != null)
                    {
                        returnCommand.StartOffsetX = moveCmd.StartAddress.AddressOffset.OffsetX;
                        returnCommand.StartOffsetY = moveCmd.StartAddress.AddressOffset.OffsetY;
                        returnCommand.StartOffsetTheta = moveCmd.StartAddress.AddressOffset.OffsetTheta;
                    }
                    else
                    {
                        WriteLog("MoveControl", "3", device, "", "moveCmd.StartAddress or moveCmd.StartAddress.AddressOffset == null!");
                    }

                    returnCommand.End = reserveList[reserveList.Count - 1].Position;
                    returnCommand.RetryMovePosition = retryMovePosition;

                    WriteLog("MoveControl", "7", device, "", "命令分解成功, 分解時間 : " + createListTimer.ElapsedMilliseconds + "ms!");
                    return returnCommand;
                }
                else
                {
                    WriteLog("MoveControl", "7", device, "", "命令分解失敗, 分解時間 : " + createListTimer.ElapsedMilliseconds + "ms!");
                    return null;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Excption! " + ex.ToString();
                return null;
            }
        }
    }
}