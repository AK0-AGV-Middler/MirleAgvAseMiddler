using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Generic;
using System.IO;
using Mirle.Agv.Model.Configs;
using System.Linq;
using Mirle.Agv.Controller.Tools;
using System.Reflection;

namespace Mirle.Agv.Controller
{
    public class MapHandler
    {
        private LoggerAgent loggerAgent;
        private MapConfig mapConfig;
        public string SectionPath { get; set; }
        public string AddressPath { get; set; }
        public string BarcodePath { get; set; }
        public MapInfo TheMapInfo { get; private set; } = new MapInfo();
        private double AddressArea { get; set; } = 10;
        private Vehicle theVehicle = Vehicle.Instance;

        public MapHandler(MapConfig mapConfig)
        {
            this.mapConfig = mapConfig;
            loggerAgent = LoggerAgent.Instance;
            SectionPath = Path.Combine(Environment.CurrentDirectory, mapConfig.SectionFileName);
            AddressPath = Path.Combine(Environment.CurrentDirectory, mapConfig.AddressFileName);
            BarcodePath = Path.Combine(Environment.CurrentDirectory, mapConfig.BarcodeFileName);

            LoadBarcodeLineCsv();
            LoadAddressCsv();
            LoadSectionCsv();
        }

        public void LoadBarcodeLineCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BarcodePath))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , $"BarcodePath={string.IsNullOrWhiteSpace(BarcodePath)}"));
                    return;
                }
                TheMapInfo.allMapBarcodeLines.Clear();
                TheMapInfo.allMapBarcodes.Clear();

                string[] allRows = File.ReadAllLines(BarcodePath);
                if (allRows == null || allRows.Length < 2)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , "There are no barcodes in file"));
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicBarcodeIndexes = new Dictionary<string, int>();
                //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, OffsetX, OffsetY
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicBarcodeIndexes.Add(keyword, i);
                    }
                }

                var isLoadBarcodeFail = false;

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');

                    MapBarcodeLine oneRow = new MapBarcodeLine();
                    oneRow.Id = getThisRow[dicBarcodeIndexes["Id"]];
                    oneRow.HeadBarcode.LineId = oneRow.Id;
                    oneRow.HeadBarcode.Number = int.Parse(getThisRow[dicBarcodeIndexes["BarcodeHeadNum"]]);
                    oneRow.HeadBarcode.Position.X = double.Parse(getThisRow[dicBarcodeIndexes["HeadX"]]);
                    oneRow.HeadBarcode.Position.Y = double.Parse(getThisRow[dicBarcodeIndexes["HeadY"]]);
                    oneRow.TailBarcode.LineId = oneRow.Id;
                    oneRow.TailBarcode.Number = int.Parse(getThisRow[dicBarcodeIndexes["BarcodeTailNum"]]);
                    oneRow.TailBarcode.Position.X = double.Parse(getThisRow[dicBarcodeIndexes["TailX"]]);
                    oneRow.TailBarcode.Position.Y = double.Parse(getThisRow[dicBarcodeIndexes["TailY"]]);
                    oneRow.Offset.X = double.Parse(getThisRow[dicBarcodeIndexes["OffsetX"]]);
                    oneRow.Offset.Y = double.Parse(getThisRow[dicBarcodeIndexes["OffsetY"]]);

                    int count = oneRow.TailBarcode.Number - oneRow.HeadBarcode.Number;
                    int absCount = Math.Abs(count);
                    if (absCount % 3 != 0)
                    {
                        isLoadBarcodeFail = true;
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                            , $"BarcodeLineNum mod 3 is not zero, [Id = {oneRow.Id}][HeadNum={oneRow.HeadBarcode.Number}][TailNum={oneRow.TailBarcode.Number}]"));
                        break;
                    }
                    if (count < 0)
                    {
                        count = -count;
                        for (int j = 0; j <= count; j += 3)
                        {
                            MapBarcode mapBarcode = new MapBarcode();
                            mapBarcode.Number = oneRow.TailBarcode.Number + j;
                            mapBarcode.Position.X = (j * oneRow.HeadBarcode.Position.X + (count - j) * oneRow.TailBarcode.Position.X) / count;
                            mapBarcode.Position.Y = (j * oneRow.HeadBarcode.Position.Y + (count - j) * oneRow.TailBarcode.Position.Y) / count;
                            mapBarcode.Offset.X = oneRow.Offset.X;
                            mapBarcode.Offset.Y = oneRow.Offset.Y;
                            mapBarcode.LineId = oneRow.Id;

                            TheMapInfo.allMapBarcodes.Add(mapBarcode.Number, mapBarcode);
                        }
                    }
                    else
                    {
                        for (int j = 0; j <= count; j += 3)
                        {
                            MapBarcode mapBarcode = new MapBarcode();
                            mapBarcode.Number = oneRow.HeadBarcode.Number + j;
                            mapBarcode.Position.X = (j * oneRow.TailBarcode.Position.X + (count - j) * oneRow.HeadBarcode.Position.X) / count;
                            mapBarcode.Position.Y = (j * oneRow.TailBarcode.Position.Y + (count - j) * oneRow.HeadBarcode.Position.Y) / count;
                            mapBarcode.Offset.X = oneRow.Offset.X;
                            mapBarcode.Offset.Y = oneRow.Offset.Y;
                            mapBarcode.LineId = oneRow.Id;

                            TheMapInfo.allMapBarcodes.Add(mapBarcode.Number, mapBarcode);
                        }
                    }

                    TheMapInfo.allMapBarcodeLines.Add(oneRow.Id, oneRow);
                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"Load Barcode File Ok. [IsLoadBarcodeFail={isLoadBarcodeFail}]"));
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public void LoadAddressCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddressPath))
                {
                    return;
                }
                TheMapInfo.allMapAddresses.Clear();

                string[] allRows = File.ReadAllLines(AddressPath);
                if (allRows == null || allRows.Length < 2)
                {
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicAddressIndexes = new Dictionary<string, int>();
                //Id,PositionX,PositionY,
                //IsWorkStation,CanLeftLoad,CanLeftUnload,CanRightLoad,CanRightUnload,
                //IsCharger,CouplerId,ChargeDirection,IsSegmentPoint,CanSpin,IsTR50
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicAddressIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapAddress oneRow = new MapAddress();
                    oneRow.Id = getThisRow[dicAddressIndexes["Id"]];
                    oneRow.Position.X = double.Parse(getThisRow[dicAddressIndexes["PositionX"]]);
                    oneRow.Position.Y = double.Parse(getThisRow[dicAddressIndexes["PositionY"]]);
                    oneRow.IsWorkStation = bool.Parse(getThisRow[dicAddressIndexes["IsWorkStation"]]);
                    oneRow.CanLeftLoad = bool.Parse(getThisRow[dicAddressIndexes["CanLeftLoad"]]);
                    oneRow.CanLeftUnload = bool.Parse(getThisRow[dicAddressIndexes["CanLeftUnload"]]);
                    oneRow.CanRightLoad = bool.Parse(getThisRow[dicAddressIndexes["CanRightLoad"]]);
                    oneRow.CanRightUnload = bool.Parse(getThisRow[dicAddressIndexes["CanRightUnload"]]);
                    oneRow.IsCharger = bool.Parse(getThisRow[dicAddressIndexes["IsCharger"]]);
                    oneRow.CouplerId = getThisRow[dicAddressIndexes["CouplerId"]];
                    oneRow.ChargeDirection = oneRow.ChargeDirectionParse(getThisRow[dicAddressIndexes["ChargeDirection"]]);
                    oneRow.IsSegmentPoint = bool.Parse(getThisRow[dicAddressIndexes["IsSegmentPoint"]]);
                    oneRow.CanSpin = bool.Parse(getThisRow[dicAddressIndexes["CanSpin"]]);
                    oneRow.PioDirection = oneRow.PioDirectionParse(getThisRow[dicAddressIndexes["PioDirection"]]);
                    oneRow.IsTR50 = bool.Parse(getThisRow[dicAddressIndexes["IsTR50"]]);

                    TheMapInfo.allMapAddresses.Add(oneRow.Id, oneRow);
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public void LoadSectionCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionPath))
                {
                    return;
                }
                TheMapInfo.allMapSections.Clear();

                string[] allRows = File.ReadAllLines(SectionPath);
                if (allRows == null || allRows.Length < 2)
                {
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicSectionIndexes = new Dictionary<string, int>();
                //Id, FromAddress, ToAddress, Speed, Type, PermitDirection
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicSectionIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapSection oneRow = new MapSection();
                    oneRow.Id = getThisRow[dicSectionIndexes["Id"]];
                    oneRow.HeadAddress = TheMapInfo.allMapAddresses[getThisRow[dicSectionIndexes["FromAddress"]]];
                    oneRow.TailAddress = TheMapInfo.allMapAddresses[getThisRow[dicSectionIndexes["ToAddress"]]];
                    oneRow.Distance = GetDistance(oneRow.HeadAddress.Position, oneRow.TailAddress.Position);
                    oneRow.Speed = double.Parse(getThisRow[dicSectionIndexes["Speed"]]);
                    oneRow.Type = oneRow.SectionTypeParse(getThisRow[dicSectionIndexes["Type"]]);
                    oneRow.PermitDirection = oneRow.PermitDirectionParse(getThisRow[dicSectionIndexes["PermitDirection"]]);

                    TheMapInfo.allMapSections.Add(oneRow.Id, oneRow);
                }
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public bool IsPositionInThisSection(MapPosition aPosition, MapSection aSection)
        {
            MapSection mapSection = aSection.DeepClone();
            var headPosition = mapSection.HeadAddress.Position;
            var tailPosition = mapSection.TailAddress.Position;

            VehiclePosition location = theVehicle.CurVehiclePosition;

            #region Not in Section
            //Position 在 Head 西方過遠
            if (aPosition.X + AddressArea < headPosition.X)
            {
                return false;
            }
            //Position 在 Tail 東方過遠
            if (aPosition.X > tailPosition.X + AddressArea)
            {
                return false;
            }
            //Position 在 Head 北方過遠
            if (aPosition.Y < headPosition.Y - AddressArea)
            {
                return false;
            }
            //Position 在 Tail 南方過遠
            if (aPosition.Y - AddressArea > tailPosition.Y)
            {
                return false;
            }
            #endregion

            #region In Address
            if (IsPositionInThisAddress(aPosition, headPosition))
            {
                location.LastAddress = mapSection.HeadAddress;
                location.LastSection = mapSection;
                location.LastSection.Distance = 0;
                return true;
            }
            if (IsPositionInThisAddress(aPosition, tailPosition))
            {
                location.LastAddress = mapSection.TailAddress;
                location.LastSection = mapSection;
                location.LastSection.Distance = aSection.Distance;
                return true;
            }
            #endregion

            #region Else
            location.LastSection = mapSection;
            location.LastSection.Distance = GetDistance(aPosition, headPosition);
            return true;
            #endregion
        }

        private double GetVectorRatio(MapPosition aPosition, MapSection mapSection)
        {
            var headPosition = mapSection.HeadAddress.Position;
            var tailPosition = mapSection.TailAddress.Position;
            var num1 = (tailPosition.X - headPosition.X) * (aPosition.Y - headPosition.Y);
            var num2 = (tailPosition.Y - headPosition.Y) * (aPosition.X - headPosition.X);
            return Math.Abs(num1 - num2);
        }

        public bool IsPositionInThisAddress(MapPosition aPosition, MapPosition addressPosition)
        {
            return GetDistance(aPosition, addressPosition) <= AddressArea * AddressArea;
        }

        private double GetDistance(MapPosition aPosition, MapPosition bPosition)
        {
            var diffX = Math.Abs(aPosition.X - bPosition.X);
            var diffY = Math.Abs(aPosition.Y - bPosition.Y);
            return diffX * diffX + diffY * diffY;
        }
    }

}
