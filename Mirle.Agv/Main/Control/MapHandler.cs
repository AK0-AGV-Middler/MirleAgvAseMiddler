﻿using Mirle.Agv.Model;
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

        public void LoadSectionCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionPath))
                {
                    return;
                }
                var mapSections = TheMapInfo.mapSections;
                var allMapSections = TheMapInfo.allMapSections;
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
                    oneRow.HeadAddress = TheMapInfo.allMapAddresses[getThisRow[dicSectionIndexes["FromAddress"]]];
                    oneRow.TailAddress = TheMapInfo.allMapAddresses[getThisRow[dicSectionIndexes["ToAddress"]]];
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
                var mapAddresses = TheMapInfo.mapAddresses;
                var allMapAddresses = TheMapInfo.allMapAddresses;
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
                var mapBarcodeLines = TheMapInfo.mapBarcodeLines;
                Dictionary<string, int> dicBarcodeIndexes = new Dictionary<string, int>(); // theMapInfo.dicBarcodeIndexes;
                var allBarcodes = TheMapInfo.allBarcodes;
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
