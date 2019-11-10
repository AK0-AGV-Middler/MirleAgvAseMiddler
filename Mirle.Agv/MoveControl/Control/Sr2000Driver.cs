using System;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using System.Threading;
using System.Text.RegularExpressions;
using Mirle.Agv.Controller.Tools;
using System.Collections.Concurrent;

namespace Mirle.Agv.Controller
{
    public class Sr2000Driver
    {
        private MapInfo theMapInfo;
        private Sr2000Config sr2000Config;
        private Sr2000Info sr2000Info;
        private LoggerAgent loggerAgent;

        private AlarmHandler alarmHandler;
        private Sr2000ReadData returnData = null;
        private ConcurrentQueue<Sr2000ReadData> readDataQueue = new ConcurrentQueue<Sr2000ReadData>();
        private string LON = "LON", LOFF = "LOFF", ChangeMode = "BLOAD,3";
        private uint count = 0;
        private const int AllowableTheta = 10;
        private int indexNumber;
        private string device;

        public Sr2000Driver(Sr2000Config sr2000Config, MapInfo theMapInfo, int indexNumber, AlarmHandler alarmHandler)
        {
            try
            {
                loggerAgent = LoggerAgent.Instance;
                this.alarmHandler = alarmHandler;
                this.theMapInfo = theMapInfo;
                this.sr2000Config = sr2000Config;
                this.indexNumber = indexNumber * 100;
                device = sr2000Config.ID;

                sr2000Info = new Sr2000Info(sr2000Config.IP);
                if (!Connect())
                    SendAlarmCode(101000);
            }
            catch (Exception ex)
            {
                //. 參考出問題,可能CPU x64 x86 anyCPU參考用錯,或sr2000Config = null. 或Connect Excpition.
                WriteLog("Error", "5", device, "", "Initail Excption : " + ex.ToString());
                SendAlarmCode(101000);
            }
        }

        private void WriteLog(string category, string logLevel, string device, string carrierId, string message,
                             [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string classMethodName = GetType().Name + ":" + memberName;
            LogFormat logFormat = new LogFormat(category, logLevel, classMethodName, device, carrierId, message);

            loggerAgent.LogMsg(logFormat.Category, logFormat);
        }

        private void WriteLog(Sr2000ReadData sr2000ReadData)
        {
            Sr2000ReadData deletaQueue;
            readDataQueue.Enqueue(sr2000ReadData);
            if (sr2000ReadData.Count > 3000)
                readDataQueue.TryDequeue(out deletaQueue);

            if (sr2000Config.LogMode)
            {
                if (sr2000ReadData == null)
                {
                }
                else
                {
                }
            }
        }

        private void SendAlarmCode(int alarmCode)
        {
            alarmCode += indexNumber;

            try
            {
                WriteLog("Error", "3", device, "", "SetAlarm, alarmCode : " + alarmCode.ToString());
                alarmHandler.SetAlarm(alarmCode);
            }
            catch (Exception ex)
            {
                WriteLog("Error", "3", device, "", "SetAlarm失敗, Excption : " + ex.ToString());
            }
        }

        public void Disconnect()
        {
            try
            {
                if (sr2000Info.Connect)
                {
                    sr2000Info.Trigger = false;
                    SendCommandLOFF();
                    sr2000Info.Reader.Dispose();
                    sr2000Info.Connect = false;
                }
            }
            catch
            {
                //. Dispose Excption, 不該發生.

            }
        }

        public bool Trigger
        {
            get
            {
                return sr2000Info.Trigger;
            }

            set
            {
                if (sr2000Info.Trigger != value)
                {
                    sr2000Info.Trigger = value;

                    if (!sr2000Info.Trigger)
                        SendCommandLOFF();
                }
            }
        }

        public bool GetConnect()
        {
            return sr2000Info.Connect;
        }

        public Sr2000ReadData GetReadData()
        {
            return returnData;
        }

        public ThetaSectionDeviation GetThetaSectionDeviation()
        {
            if (returnData == null)
                return null;

            return returnData.ReviseData;
        }

        public AGVPosition GetAGVPosition()
        {
            if (returnData == null)
                return null;

            return returnData.AGV;
        }

        public bool LogMode
        {
            get
            {
                return sr2000Config.LogMode;
            }

            set
            {
                sr2000Config.LogMode = value;
            }
        }

        private MapPosition XChangeTheta55Single(MapPosition barcode)
        {
            double mid = (sr2000Config.Up + sr2000Config.Down) / 2;
            double value = sr2000Config.Down + (sr2000Config.Up - sr2000Config.Down) * (barcode.Y / (2 * sr2000Config.ViewCenter.Y)); ;
            double x = sr2000Config.ViewCenter.X + (barcode.X - sr2000Config.ViewCenter.X) * (mid / value);
            MapPosition returnPosition = new MapPosition((float)x, barcode.Y);
            return returnPosition;
        }

        private void XChangeTheta55ALL(Sr2000ReadData sr2000ReadData)
        {
            sr2000ReadData.Barcode1.ViewPosition = XChangeTheta55Single(sr2000ReadData.Barcode1.ViewPosition);
            sr2000ReadData.Barcode2.ViewPosition = XChangeTheta55Single(sr2000ReadData.Barcode2.ViewPosition);
        }

        private bool CheckDistanceSafetyRange(Sr2000ReadData sr2000ReadData)
        {
            double realDistance = Math.Sqrt(Math.Pow(sr2000ReadData.Barcode1.MapPosition.X - sr2000ReadData.Barcode2.MapPosition.X, 2) +
                                            Math.Pow(sr2000ReadData.Barcode1.MapPosition.Y - sr2000ReadData.Barcode2.MapPosition.Y, 2));
            double deltaX = (sr2000ReadData.Barcode1.ViewPosition.X - sr2000ReadData.Barcode2.ViewPosition.X) * sr2000Config.Change.X;
            double deltaY = (sr2000ReadData.Barcode1.ViewPosition.Y - sr2000ReadData.Barcode2.ViewPosition.Y) * sr2000Config.Change.Y;
            double computeDistance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            double delta = Math.Abs(computeDistance - realDistance);

            return (delta / realDistance) < sr2000Config.DistanceSafetyRange;
        }

        private Sr2000ReadData SendCommandLON()
        {
            Sr2000ReadData sr2000ReadData;

            try
            {
                string receivedData = sr2000Info.Reader.ExecCommand(LON, sr2000Config.TimeOutValue);

                if (receivedData == null || receivedData == "")
                {
                    sr2000ReadData = new Sr2000ReadData(LON, receivedData, count);
                }
                else
                {
                    string[] splitResult = Regex.Split(receivedData, "[: / ,]+", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

                    if (splitResult.Length == 7)
                    {

                        sr2000ReadData = new Sr2000ReadData(splitResult, LON, receivedData, count);

                        XChangeTheta55ALL(sr2000ReadData);
                        if (sr2000ReadData.ScanTime < sr2000Config.TimeOutValue && GetReadDataPosition(sr2000ReadData))
                        {
                            //if (CheckDistanceSafetyRange(sr2000ReadData))
                            //{
                            ComputeMapPosition(sr2000ReadData);
                            ComputeThetaSectionDeviation(sr2000ReadData);
                            sr2000ReadData.ReviseData.BarcodeAngleInMap = sr2000ReadData.AGV.BarcodeAngleInMap;
                            sr2000ReadData.ReviseData.AGVAngleInMap = sr2000ReadData.AGV.AGVAngle;
                            //}
                        }
                        // scanTime > timeoutvalue ??
                    }
                    else
                    {
                        SendAlarmCode(101001);
                        // SR200回傳資料有問題,設定跑掉.
                        sr2000ReadData = new Sr2000ReadData(LON, receivedData, count);
                    }
                }
            }
            catch
            {
                sr2000ReadData = new Sr2000ReadData(LON, "excption", count);
            }

            return sr2000ReadData;
        }

        private void SendCommandLOFF()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    string receivedData = sr2000Info.Reader.ExecCommand(LOFF);
                    Sr2000ReadData sr2000ReadData = new Sr2000ReadData(LOFF, receivedData);
                    WriteLog(sr2000ReadData);
                });
            }
            catch
            {
                // log..
            }
        }

        private void SendCommandChangeMode()
        {
            try
            {
                string receivedData = sr2000Info.Reader.ExecCommand(ChangeMode);
                Sr2000ReadData sr2000ReadData = new Sr2000ReadData(ChangeMode, receivedData);
                WriteLog(sr2000ReadData);
            }
            catch
            {
                // log..
            }
        }

        private bool Connect()
        {
            if (sr2000Info.Reader.Connect())
            {
                SendCommandChangeMode();

                sr2000Info.Connect = true;
                sr2000Info.Trigger = true;
                sr2000Info.RunThread = new Thread(TriggerThread);
                sr2000Info.RunThread.Start();
                return true;
            }
            else
            {
                return false;
                //. 連線失敗 查看ip或者是 172.168.9.5 AGV車的類似問題.
            }
        }


        private bool GetBarcodePosition(BarcodeData barcodeData)
        {
            if (theMapInfo.allMapBarcodes.ContainsKey(barcodeData.ID))
            {
                barcodeData.MapPosition = new MapPosition(theMapInfo.allMapBarcodes[barcodeData.ID].Position.X, theMapInfo.allMapBarcodes[barcodeData.ID].Position.Y);
                barcodeData.MapPositionOffset = new MapPosition(theMapInfo.allMapBarcodes[barcodeData.ID].Offset.X, theMapInfo.allMapBarcodes[barcodeData.ID].Offset.Y);
                barcodeData.LineId = theMapInfo.allMapBarcodes[barcodeData.ID].LineId;
                barcodeData.Type = theMapInfo.allMapBarcodes[barcodeData.ID].Material;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool GetReadDataPosition(Sr2000ReadData sr2000ReadData)
        {
            if (GetBarcodePosition(sr2000ReadData.Barcode1) && GetBarcodePosition(sr2000ReadData.Barcode2))
            {
                return sr2000ReadData.Barcode1.LineId == sr2000ReadData.Barcode2.LineId;
                // 不同條Barcode就不計算?
            }
            else
            {
                return false;
            }
        }

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
            catch
            {
                // log..
                return returnAngle;
            }
        }

        private double Interpolation(double start, double startValue, double end, double endValue, double target)
        {
            if (start == end)
                return (startValue + endValue) / 2;
            else
                return startValue + (endValue - startValue) * (target - start) / (end - start);
        }

        private void ComputeMapPosition(Sr2000ReadData sr2000ReadData)
        {
            double barcodeAngleInView = ComputeAngle(sr2000ReadData.Barcode1.ViewPosition, sr2000ReadData.Barcode2.ViewPosition); // pointer barcode1 to barcode2.
                                                                                                                                  // barcode在reader上面的角度 = barcode - reader.
            double barcodeAngleInMap = ComputeAngle(sr2000ReadData.Barcode1.MapPosition, sr2000ReadData.Barcode2.MapPosition);    // barcode in Map's angle.
                                                                                                                                  // barcode在Map上的角度 = barcode - Map.
            double barcode1ToCenterAngleInView = ComputeAngle(sr2000ReadData.Barcode1.ViewPosition, sr2000Config.Target);
            double barcode1ToCenterAngleInMap = 0;
            double agvAngleInMap = 0;
            double barcode1ToCenterDistance = Math.Sqrt(Math.Pow((sr2000Config.Target.X - sr2000ReadData.Barcode1.ViewPosition.X) * sr2000Config.Change.X, 2) +
                                                        Math.Pow((sr2000Config.Target.Y - sr2000ReadData.Barcode1.ViewPosition.Y) * sr2000Config.Change.Y, 2));
            barcodeAngleInView += sr2000Config.OffsetTheta;
            barcode1ToCenterAngleInView += sr2000Config.OffsetTheta;
            // Map在reader上的角度 = Map -reader = barcodeInViewAngle - barcodeInMapAngle;
            agvAngleInMap = barcodeAngleInMap - barcodeAngleInView - sr2000Config.ReaderSetupAngle;
            if (agvAngleInMap > 180)
                agvAngleInMap -= 360;
            else if (agvAngleInMap <= -180)
                agvAngleInMap += 360;

            barcode1ToCenterAngleInMap = barcodeAngleInMap - barcodeAngleInView + barcode1ToCenterAngleInView;

            sr2000ReadData.TargetCenter =
                new MapPosition(sr2000ReadData.Barcode1.MapPosition.X + (float)(barcode1ToCenterDistance * Math.Cos(-barcode1ToCenterAngleInMap / 180 * Math.PI)),
                                sr2000ReadData.Barcode1.MapPosition.Y + (float)(barcode1ToCenterDistance * Math.Sin(-barcode1ToCenterAngleInMap / 180 * Math.PI)));

            MapPosition agvPosition = new MapPosition(
                sr2000ReadData.TargetCenter.X + (float)(sr2000Config.ReaderToCenterDistance *
                Math.Cos(-(agvAngleInMap + sr2000Config.ReaderToCenterDegree + 180) / 180 * Math.PI)),
                sr2000ReadData.TargetCenter.Y + (float)(sr2000Config.ReaderToCenterDistance *
                Math.Sin(-(agvAngleInMap + sr2000Config.ReaderToCenterDegree + 180) / 180 * Math.PI)));

            sr2000ReadData.AGV = new AGVPosition(agvPosition, sr2000ReadData.TargetCenter, agvAngleInMap, barcodeAngleInView, sr2000ReadData.ScanTime,
                                                 sr2000ReadData.GetDataTime, sr2000ReadData.Count, barcodeAngleInMap, sr2000ReadData.Barcode1.Type, sr2000ReadData.Barcode1.LineId);

            sr2000ReadData.AngleOfMapInReader = barcodeAngleInView - barcodeAngleInMap;
            if (sr2000ReadData.AngleOfMapInReader > 180)
                sr2000ReadData.AngleOfMapInReader -= 360;
            else if (sr2000ReadData.AngleOfMapInReader <= -180)
                sr2000ReadData.AngleOfMapInReader += 360;
        }

        private MapPosition BarcodeOffsetChange(BarcodeData barcodeData, double AngleOfMapInReader)
        {
            MapPosition returnData = new MapPosition(barcodeData.ViewPosition.X, barcodeData.ViewPosition.Y);

            if (barcodeData.MapPositionOffset.X != 0)
            {
                returnData.X += barcodeData.MapPositionOffset.X * (float)Math.Cos(-AngleOfMapInReader / 180 * Math.PI);
                returnData.Y += barcodeData.MapPositionOffset.X * (float)Math.Sin(-AngleOfMapInReader / 180 * Math.PI);
            }

            if (barcodeData.MapPositionOffset.Y != 0)
            {
                returnData.X += barcodeData.MapPositionOffset.Y * (float)Math.Cos(-(AngleOfMapInReader - 90) / 180 * Math.PI);
                returnData.Y += barcodeData.MapPositionOffset.Y * (float)Math.Sin(-(AngleOfMapInReader - 90) / 180 * Math.PI);
            }

            return returnData;
        }

        private void ComputeThetaSectionDeviation(Sr2000ReadData sr2000ReadData)
        {
            double theta = 0, sectionDeviation = 0, centerPixel = 0;
            MapPosition offsetBarcode1 = BarcodeOffsetChange(sr2000ReadData.Barcode1, sr2000ReadData.AngleOfMapInReader);
            MapPosition offsetBarcode2 = BarcodeOffsetChange(sr2000ReadData.Barcode2, sr2000ReadData.AngleOfMapInReader);

            if (Math.Abs(sr2000ReadData.AGV.BarcodeAngle - 0) <= AllowableTheta ||
                Math.Abs(sr2000ReadData.AGV.BarcodeAngle - 180) <= AllowableTheta ||
                Math.Abs(sr2000ReadData.AGV.BarcodeAngle - -180) <= AllowableTheta)
            {
                if (Math.Abs(sr2000ReadData.AGV.AGVAngle - 90) <= AllowableTheta)
                    theta = sr2000ReadData.AGV.AGVAngle - 90;
                else if (Math.Abs(sr2000ReadData.AGV.AGVAngle - -90) <= AllowableTheta)
                    theta = sr2000ReadData.AGV.AGVAngle - -90;
                else if (Math.Abs(sr2000ReadData.AGV.AGVAngle - 180) <= AllowableTheta)
                    theta = sr2000ReadData.AGV.AGVAngle - 180;
                else if (Math.Abs(sr2000ReadData.AGV.AGVAngle - -180) <= AllowableTheta)
                    theta = sr2000ReadData.AGV.AGVAngle - -180;
                else
                    theta = sr2000ReadData.AGV.AGVAngle;

                centerPixel = offsetBarcode1.Y + (double)(offsetBarcode2.Y - offsetBarcode1.Y) *
                              (sr2000Config.Target.X - offsetBarcode1.X) / (offsetBarcode2.X - offsetBarcode1.X);

                sectionDeviation = -Math.Cos(-sr2000Config.ReaderSetupAngle / 180 * Math.PI) * (centerPixel - sr2000Config.Target.Y) * sr2000Config.Change.Y;
                sr2000ReadData.ReviseData = new ThetaSectionDeviation(theta, sectionDeviation, sr2000ReadData.Count);
            }
            else if (Math.Abs(sr2000ReadData.AGV.BarcodeAngle - 90) <= AllowableTheta ||
                     Math.Abs(sr2000ReadData.AGV.BarcodeAngle - -90) <= AllowableTheta)
            {
                if (Math.Abs(sr2000ReadData.AGV.AGVAngle - 90) <= AllowableTheta)
                    theta = sr2000ReadData.AGV.AGVAngle - 90;
                else if (Math.Abs(sr2000ReadData.AGV.AGVAngle - -90) <= AllowableTheta)
                    theta = sr2000ReadData.AGV.AGVAngle - -90;
                else if (Math.Abs(sr2000ReadData.AGV.AGVAngle - 180) <= AllowableTheta)
                    theta = sr2000ReadData.AGV.AGVAngle - 180;
                else if (Math.Abs(sr2000ReadData.AGV.AGVAngle - -180) <= AllowableTheta)
                    theta = sr2000ReadData.AGV.AGVAngle - -180;
                else
                    theta = sr2000ReadData.AGV.AGVAngle;

                centerPixel = offsetBarcode1.X + (double)(offsetBarcode2.X - offsetBarcode1.X) *
                              (sr2000Config.Target.Y - offsetBarcode1.Y) / (offsetBarcode2.Y - offsetBarcode1.Y);

                sectionDeviation = -Math.Cos(-sr2000Config.ReaderSetupAngle / 180 * Math.PI) * (centerPixel - sr2000Config.Target.X) * sr2000Config.Change.X;
                sr2000ReadData.ReviseData = new ThetaSectionDeviation(theta, sectionDeviation, sr2000ReadData.Count);
            }
            else
            {
                //. Error theta ??
            }
        }

        private void TriggerThread()
        {
            Sr2000ReadData sr2000ReadData;

            while (sr2000Info.Connect)
            {
                if (sr2000Info.Trigger)
                    sr2000ReadData = SendCommandLON();
                else
                {
                    Thread.Sleep(sr2000Config.TimeOutValue - sr2000Config.SleepTime);
                    sr2000ReadData = new Sr2000ReadData("No Trigger", "", count);
                }

                count++;
                returnData = sr2000ReadData;
                WriteLog(sr2000ReadData);
                Thread.Sleep(sr2000Config.SleepTime);
            }
        }
    }
}
