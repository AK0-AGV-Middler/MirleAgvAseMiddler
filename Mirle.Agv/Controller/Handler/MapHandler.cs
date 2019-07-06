using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Generic;
using System.IO;
using Mirle.Agv.Model.Configs;
using System.Linq;

namespace Mirle.Agv.Controller
{
    public class MapHandler
    {
        private string rootDir;
        private LoggerAgent loggerAgent;
        private MapConfigs mapConfigs;
        public string SectionPath { get; set; }
        public string AddressPath { get; set; }
        public string BarcodePath { get; set; }
        private MapInfo theMapInfo;

        public MapHandler(MapConfigs mapConfigs)
        {
            this.mapConfigs = mapConfigs;
            loggerAgent = LoggerAgent.Instance;
            rootDir = mapConfigs.RootDir;
            SectionPath = Path.Combine(rootDir, mapConfigs.SectionFileName);
            AddressPath = Path.Combine(rootDir, mapConfigs.AddressFileName);
            BarcodePath = Path.Combine(rootDir, mapConfigs.BarcodeFileName);
            theMapInfo = MapInfo.Instance;
            LoadSectionCsv();
            LoadAddressCsv();
            LoadBarcodeLineCsv();
        }

        public void OnMapBarcodeValuesChangedEvent(object sender, MapBarcodeReader mapBarcodeValues)
        {
            throw new NotImplementedException();
        }

        public void OnTransCmdsFinishedEvent(object sender, EnumCompleteStatus status)
        {
            throw new NotImplementedException();
        }

        public void OnInstallTransferCommand(object sender, AgvcTransCmd e)
        {
            throw new NotImplementedException();
        }

        internal void OnMiddlerGetsCancelEvent(object sender, string e)
        {
            throw new NotImplementedException();
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
                var dicMapSections = theMapInfo.dicMapSections;
                var dicSectionIndexes = theMapInfo.dicSectionIndexes;
                mapSections.Clear();
                dicMapSections.Clear();
                dicSectionIndexes.Clear();

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
                    oneRow.FromAddress = getThisRow[dicSectionIndexes["FromAddress"]];
                    oneRow.ToAddress = getThisRow[dicSectionIndexes["ToAddress"]];
                    oneRow.Distance = float.Parse(getThisRow[dicSectionIndexes["Distance"]]);
                    oneRow.Speed = float.Parse(getThisRow[dicSectionIndexes["Speed"]]);
                    oneRow.Type = oneRow.SectionTypeConvert(getThisRow[dicSectionIndexes["Type"]]);
                    oneRow.PermitDirection = oneRow.PermitDirectionConvert(getThisRow[dicSectionIndexes["PermitDirection"]]);
                    oneRow.FowardBeamSensorEnable = bool.Parse(getThisRow[dicSectionIndexes["FowardBeamSensorEnable"]]);
                    oneRow.BackwardBeamSensorEnable = bool.Parse(getThisRow[dicSectionIndexes["BackwardBeamSensorEnable"]]);

                    mapSections.Add(oneRow);
                    dicMapSections.Add(oneRow.Id, oneRow);
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
                var dicMapAddresses = theMapInfo.dicMapAddresses;
                var dicAddressIndexes = theMapInfo.dicAddressIndexes;
                mapAddresses.Clear();
                dicMapAddresses.Clear();
                dicAddressIndexes.Clear();

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
                    oneRow.Barcode = float.Parse(getThisRow[dicAddressIndexes["Barcode"]]);
                    oneRow.PositionX = float.Parse(getThisRow[dicAddressIndexes["PositionX"]]);
                    oneRow.PositionY = float.Parse(getThisRow[dicAddressIndexes["PositionY"]]);
                    oneRow.IsWorkStation = bool.Parse(getThisRow[dicAddressIndexes["IsWorkStation"]]);
                    oneRow.CanLeftLoad = bool.Parse(getThisRow[dicAddressIndexes["CanLeftLoad"]]);
                    oneRow.CanLeftUnload = bool.Parse(getThisRow[dicAddressIndexes["CanLeftUnload"]]);
                    oneRow.CanRightLoad = bool.Parse(getThisRow[dicAddressIndexes["CanRightLoad"]]);
                    oneRow.CanRightUnload = bool.Parse(getThisRow[dicAddressIndexes["CanRightUnload"]]);
                    oneRow.IsCharger = bool.Parse(getThisRow[dicAddressIndexes["IsCharger"]]);
                    oneRow.CouplerId = getThisRow[dicAddressIndexes["CouplerId"]];
                    oneRow.ChargeDirection = oneRow.ChargeDirectionConvert(getThisRow[dicAddressIndexes["ChargeDirection"]]);
                    oneRow.IsSegmentPoint = bool.Parse(getThisRow[dicAddressIndexes["IsSegmentPoint"]]);
                    oneRow.CanSpin = bool.Parse(getThisRow[dicAddressIndexes["CanSpin"]]);
                    oneRow.PioDirection = oneRow.PioDirectionConvert(getThisRow[dicAddressIndexes["PioDirection"]]);

                    mapAddresses.Add(oneRow);
                    dicMapAddresses.Add(oneRow.Id, oneRow);
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
                    return;
                }
                var mapBarcodeLines = theMapInfo.mapBarcodeLines;
                var dicBarcodeIndexes = theMapInfo.dicBarcodeIndexes;
                var dicBarcodes = theMapInfo.dicBarcodes;
                mapBarcodeLines.Clear();
                dicBarcodeIndexes.Clear();
                dicBarcodes.Clear();

                string[] allRows = File.ReadAllLines(BarcodePath);
                if (allRows == null || allRows.Length < 2)
                {
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
                    oneRow.Id = getThisRow[dicBarcodeIndexes["Id"]];
                    int HeadNum = int.Parse(getThisRow[dicBarcodeIndexes["BarcodeHeadNum"]]);
                    int TailNum = int.Parse(getThisRow[dicBarcodeIndexes["BarcodeTailNum"]]);
                    float HeadX = float.Parse(getThisRow[dicBarcodeIndexes["HeadX"]]);
                    float HeadY = float.Parse(getThisRow[dicBarcodeIndexes["HeadY"]]);
                    float TailX = float.Parse(getThisRow[dicBarcodeIndexes["TailX"]]);
                    float TailY = float.Parse(getThisRow[dicBarcodeIndexes["TailY"]]);
                    int Direction = oneRow.BarcodeDirectionConvert(getThisRow[dicBarcodeIndexes["Direction"]]);
                    float OffsetX = float.Parse(getThisRow[dicBarcodeIndexes["OffsetX"]]);
                    float OffsetY = float.Parse(getThisRow[dicBarcodeIndexes["OffsetY"]]);

                    oneRow.BarcodeHeadNum = HeadNum;
                    oneRow.HeadX = HeadX;
                    oneRow.HeadY = HeadY;
                    oneRow.BarcodeTailNum = TailNum;
                    oneRow.TailX = TailX;
                    oneRow.TailY = TailY;
                    oneRow.Direction = Direction;
                    oneRow.OffsetX = OffsetX;
                    oneRow.OffsetY = OffsetY;

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
                        for (int j = 0; j <= count; j+=3)
                        {
                            MapBarcode mapBarcode = new MapBarcode();
                            mapBarcode.BarcodeNum = TailNum + j;
                            mapBarcode.PositionX = (j * HeadX + (count - j) * TailX) / count;
                            mapBarcode.PositionY = (j * HeadY + (count - j) * TailY) / count;
                            mapBarcode.Direction = Direction;
                            mapBarcode.OffsetX = OffsetX;
                            mapBarcode.OffsetY = OffsetY;

                            dicBarcodes.Add(mapBarcode.BarcodeNum, mapBarcode);
                        }
                    }
                    else
                    {
                        for (int j = 0; j <= count; j+=3)
                        {
                            MapBarcode mapBarcode = new MapBarcode();
                            mapBarcode.BarcodeNum = HeadNum + j;
                            mapBarcode.PositionX = (j * TailX + (count - j) * HeadX) / count;
                            mapBarcode.PositionY = (j * TailY + (count - j) * HeadY) / count;
                            mapBarcode.Direction = Direction;
                            mapBarcode.OffsetX = OffsetX;
                            mapBarcode.OffsetY = OffsetY;

                            dicBarcodes.Add(mapBarcode.BarcodeNum, mapBarcode);
                        }
                    }

                    mapBarcodeLines.Add(oneRow);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public bool BooleanConvert(string v)
        {
            v = v.Trim().ToUpper();
            switch (v)
            {
                case "TRUE":
                    return true;
                case "FALSE":
                default:
                    return false;
            }
        }
    }

}
