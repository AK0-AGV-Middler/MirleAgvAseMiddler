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
        private MapInfo theMapInfo = new MapInfo();
        private double SectionWidth { get; set; } = 50;
        private double AddressArea { get; set; } = 50;
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

        public void LoadSectionCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionPath))
                {
                    return;
                }
                var mapSections = theMapInfo.mapSections;
                var allMapSections = theMapInfo.allMapSections;
                Dictionary<string, int> dicSectionIndexes = new Dictionary<string, int>(); //theMapInfo.dicSectionIndexes;
                mapSections.Clear();
                allMapSections.Clear();

                string[] allRows = File.ReadAllLines(SectionPath);
                if (allRows == null || allRows.Length < 2)
                {
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, FromAddress, ToAddress, Distance, Speed, Type, PermitDirection, FowardBeamSensorEnable, BackwardBeamSensorEnable   
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
                    oneRow.HeadAddress = theMapInfo.allMapAddresses[getThisRow[dicSectionIndexes["FromAddress"]]];
                    oneRow.TailAddress = theMapInfo.allMapAddresses[getThisRow[dicSectionIndexes["ToAddress"]]];
                    oneRow.Distance = double.Parse(getThisRow[dicSectionIndexes["Distance"]]);
                    oneRow.Speed = double.Parse(getThisRow[dicSectionIndexes["Speed"]]);
                    oneRow.Type = oneRow.SectionTypeParse(getThisRow[dicSectionIndexes["Type"]]);
                    oneRow.PermitDirection = oneRow.PermitDirectionParse(getThisRow[dicSectionIndexes["PermitDirection"]]);
                    oneRow.FowardBeamSensorDisable = bool.Parse(getThisRow[dicSectionIndexes["FowardBeamSensorDisable"]]);
                    oneRow.BackwardBeamSensorDisable = bool.Parse(getThisRow[dicSectionIndexes["BackwardBeamSensorDisable"]]);
                    oneRow.LeftBeamSensorDisable = bool.Parse(getThisRow[dicSectionIndexes["LeftBeamSensorDisable"]]);
                    oneRow.RightBeamSensorDisable = bool.Parse(getThisRow[dicSectionIndexes["RightBeamSensorDisable"]]);

                    mapSections.Add(oneRow);
                    allMapSections.Add(oneRow.Id, oneRow);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
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
                var mapAddresses = theMapInfo.mapAddresses;
                var allMapAddresses = theMapInfo.allMapAddresses;
                Dictionary<string, int> dicAddressIndexes = new Dictionary<string, int>(); // theMapInfo.dicAddressIndexes;
                mapAddresses.Clear();
                allMapAddresses.Clear();

                string[] allRows = File.ReadAllLines(AddressPath);
                if (allRows == null || allRows.Length < 2)
                {
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, Barcode, PositionX, PositionY, 
                //IsWorkStation,CanLeftLoad,CanLeftUnload,CanRightLoad,CanRightUnload,
                //IsCharger,CouplerId,ChargeDirection,IsSegmentPoint,CanSpin
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
                    oneRow.Barcode = double.Parse(getThisRow[dicAddressIndexes["Barcode"]]);
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

                    mapAddresses.Add(oneRow);
                    allMapAddresses.Add(oneRow.Id, oneRow);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
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
                var mapBarcodeLines = theMapInfo.mapBarcodeLines;
                Dictionary<string, int> dicBarcodeIndexes = new Dictionary<string, int>(); // theMapInfo.dicBarcodeIndexes;
                var allBarcodes = theMapInfo.allBarcodes;
                mapBarcodeLines.Clear();
                allBarcodes.Clear();

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

                //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, Direction
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicBarcodeIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapBarcodeLine oneRow = new MapBarcodeLine();
                    string Id = getThisRow[dicBarcodeIndexes["Id"]];
                    int HeadNum = int.Parse(getThisRow[dicBarcodeIndexes["BarcodeHeadNum"]]);
                    int TailNum = int.Parse(getThisRow[dicBarcodeIndexes["BarcodeTailNum"]]);
                    double HeadX = double.Parse(getThisRow[dicBarcodeIndexes["HeadX"]]);
                    double HeadY = double.Parse(getThisRow[dicBarcodeIndexes["HeadY"]]);
                    double TailX = double.Parse(getThisRow[dicBarcodeIndexes["TailX"]]);
                    double TailY = double.Parse(getThisRow[dicBarcodeIndexes["TailY"]]);
                    int Direction = oneRow.BarcodeDirectionConvert(getThisRow[dicBarcodeIndexes["Direction"]]);
                    double OffsetX = double.Parse(getThisRow[dicBarcodeIndexes["OffsetX"]]);
                    double OffsetY = double.Parse(getThisRow[dicBarcodeIndexes["OffsetY"]]);

                    oneRow.Id = Id;
                    oneRow.HeadBarcode.Number = HeadNum;
                    oneRow.HeadBarcode.Position.X = HeadX;
                    oneRow.HeadBarcode.Position.Y = HeadY;
                    oneRow.TailBarcode.Number = TailNum;
                    oneRow.TailBarcode.Position.X = TailX;
                    oneRow.TailBarcode.Position.Y = TailY;
                    oneRow.Direction = Direction;
                    oneRow.Offset.X = OffsetX;
                    oneRow.Offset.Y = OffsetY;

                    int count = TailNum - HeadNum;
                    int absCount = Math.Abs(count);
                    if (absCount % 3 != 0)
                    {
                        //TODO: Log BarcodeLineNum mod 3 is not zero
                        break;
                    }
                    if (count < 0)
                    {
                        count = -count;
                        for (int j = 0; j <= count; j += 3)
                        {
                            MapBarcode mapBarcode = new MapBarcode();
                            mapBarcode.Number = TailNum + j;
                            mapBarcode.Position.X = (j * HeadX + (count - j) * TailX) / count;
                            mapBarcode.Position.Y = (j * HeadY + (count - j) * TailY) / count;
                            mapBarcode.Offset.X = OffsetX;
                            mapBarcode.Offset.Y = OffsetY;
                            mapBarcode.Direction = Direction;
                            mapBarcode.LineId = Id;

                            allBarcodes.Add(mapBarcode.Number, mapBarcode);
                        }
                    }
                    else
                    {
                        for (int j = 0; j <= count; j += 3)
                        {
                            MapBarcode mapBarcode = new MapBarcode();
                            mapBarcode.Number = HeadNum + j;
                            mapBarcode.Position.X = (j * TailX + (count - j) * HeadX) / count;
                            mapBarcode.Position.Y = (j * TailY + (count - j) * HeadY) / count;
                            mapBarcode.Offset.X = OffsetX;
                            mapBarcode.Offset.Y = OffsetY;
                            mapBarcode.Direction = Direction;
                            mapBarcode.LineId = Id;

                            allBarcodes.Add(mapBarcode.Number, mapBarcode);
                        }
                    }

                    mapBarcodeLines.Add(oneRow);
                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , "Load Barcode File Ok"));
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , ex.StackTrace));
            }
        }

        public MapInfo GetMapInfo()
        {
            return theMapInfo;
        }

        public bool IsPositionInThisSection(MapPosition aPosition, MapSection aSection)
        {
            MapSection mapSection = aSection.DeepClone();
            MapAddress headAdr = mapSection.HeadAddress;
            MapAddress tailAdr = mapSection.TailAddress;

            VehiclePosition location = theVehicle.AVehiclePosition;


            if (IsPositionInThisAddress(aPosition, headAdr))
            {
                if (mapSection.CmdDirection == EnumPermitDirection.Forward)
                {
                    mapSection.Distance = 0;
                }
                location.LastAddress = headAdr;
                location.LastSection = mapSection;
                location.LastSection.Distance = 0;
                return true;
            }

            if (IsPositionInThisAddress(aPosition, tailAdr))
            {
                if (mapSection.CmdDirection != EnumPermitDirection.Forward)
                {
                    mapSection.Distance = 0;
                }
                location.LastAddress = tailAdr;
                location.LastSection = mapSection;
                location.LastSection.Distance = aSection.Distance;
                return true;
            }

            switch (aSection.Type)
            {
                case EnumSectionType.Horizontal:
                    {
                        double diffY = Math.Abs(aPosition.Y - headAdr.Position.Y);
                        if (diffY <= SectionWidth)
                        {
                            if (aPosition.X > tailAdr.Position.X || aPosition.X < headAdr.Position.X)
                            {
                                return false;
                            }
                            else
                            {
                                if (mapSection.CmdDirection == EnumPermitDirection.Forward)
                                {
                                    mapSection.Distance = Math.Abs(aPosition.X - headAdr.Position.X);
                                }
                                else
                                {
                                    mapSection.Distance = Math.Abs(tailAdr.Position.X - aPosition.X);
                                }
                                location.LastSection = mapSection;

                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                case EnumSectionType.Vertical:
                    {
                        double diffX = Math.Abs(aPosition.X - headAdr.Position.X);
                        if (diffX <= SectionWidth)
                        {
                            if (aPosition.Y > tailAdr.Position.Y || aPosition.Y < headAdr.Position.Y)
                            {
                                return false;
                            }
                            else
                            {
                                if (mapSection.CmdDirection == EnumPermitDirection.Forward)
                                {
                                    mapSection.Distance = Math.Abs(aPosition.Y - headAdr.Position.Y);
                                }
                                else
                                {
                                    mapSection.Distance = Math.Abs(tailAdr.Position.Y - aPosition.Y);
                                }
                                location.LastSection = mapSection;
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                case EnumSectionType.R2000:
                    //TODO: Analysis diff <= SectionWidth?
                    //TODO: Analysis position is in the R2000 rectangle(sin45/cos45)
                    break;
                case EnumSectionType.None:
                default:
                    break;
            }

            return true;
        }

        public bool IsPositionInThisAddress(MapPosition aPosition, MapAddress anAddress)
        {
            var diffX = Math.Abs(aPosition.X - anAddress.Position.X);
            var diffY = Math.Abs(aPosition.Y - anAddress.Position.Y);
            return diffX * diffX + diffY * diffY <= AddressArea * AddressArea;
        }

    }

}
