using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Generic;
using System.IO;
using Mirle.Agv.Model.Configs;
using System.Linq;

namespace Mirle.Agv.Control
{
    public class MapHandler
    {
        private string rootDir;
        private LoggerAgent loggerAgent;
        private MapConfigs mapConfigs;
        public string SectionPath { get; set; }
        public string AddressPath { get; set; }
        public string BarcodePath { get; set; }
        private MapInfo mapInfo;

        public MapHandler(MapConfigs mapConfigs)
        {
            this.mapConfigs = mapConfigs;
            loggerAgent = LoggerAgent.Instance;
            rootDir = mapConfigs.RootDir;
            SectionPath = Path.Combine(rootDir, mapConfigs.SectionFileName);
            AddressPath = Path.Combine(rootDir, mapConfigs.AddressFileName);
            BarcodePath = Path.Combine(rootDir, mapConfigs.BarcodeFileName);
            mapInfo = MapInfo.Instance;
            LoadSectionCsv();
            LoadAddressCsv();
            LoadRowBarcodeCsv();
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
                var mapSections = mapInfo.mapSections;
                var dicMapSections = mapInfo.dicMapSections;
                var dicSectionIndexes = mapInfo.dicSectionIndexes;
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
                    oneRow.FromAddress = getThisRow[dicSectionIndexes["FromAdr"]];
                    oneRow.ToAddress = getThisRow[dicSectionIndexes["ToAddress"]];
                    oneRow.Distance = float.Parse(getThisRow[dicSectionIndexes["Distance"]]);
                    oneRow.Speed = float.Parse(getThisRow[dicSectionIndexes["Speed"]]);
                    oneRow.Type = oneRow.SectionTypeConvert(getThisRow[dicSectionIndexes["Type"]]);
                    oneRow.PermitDirection = oneRow.PermitDirectionConvert(getThisRow[dicSectionIndexes["PermitDirection"]]);
                    oneRow.FowardBeamSensorEnable = BooleanConvert(getThisRow[dicSectionIndexes["FowardBeamSensorEnable"]]);
                    oneRow.BackwardBeamSensorEnable = BooleanConvert(getThisRow[dicSectionIndexes["BackwardBeamSensorEnable"]]);

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
                var mapAddresses = mapInfo.mapAddresses;
                var dicMapAddresses = mapInfo.dicMapAddresses;
                var dicAddressIndexes = mapInfo.dicAddressIndexes;
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

                //Id, BarcodeH, BarcodeV, PositionX, PositionY, 
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
                    oneRow.BarcodeH = float.Parse(getThisRow[dicAddressIndexes["BarcodeH"]]);
                    oneRow.BarcodeV = float.Parse(getThisRow[dicAddressIndexes["BarcodeV"]]);
                    oneRow.PositionX = float.Parse(getThisRow[dicAddressIndexes["PositionX"]]);
                    oneRow.PositionY = float.Parse(getThisRow[dicAddressIndexes["PositionY"]]);
                    oneRow.IsWorkStation = BooleanConvert(getThisRow[dicAddressIndexes["IsWorkStation"]]);
                    oneRow.CanLeftLoad = BooleanConvert(getThisRow[dicAddressIndexes["CanLeftLoad"]]);
                    oneRow.CanLeftUnload = BooleanConvert(getThisRow[dicAddressIndexes["CanLeftUnload"]]);
                    oneRow.CanRightLoad = BooleanConvert(getThisRow[dicAddressIndexes["CanRightLoad"]]);
                    oneRow.CanRightUnload = BooleanConvert(getThisRow[dicAddressIndexes["CanRightUnload"]]);
                    oneRow.IsCharger = BooleanConvert(getThisRow[dicAddressIndexes["IsCharger"]]);
                    oneRow.CouplerId = getThisRow[dicAddressIndexes["CouplerId"]];
                    oneRow.ChargeDirection = oneRow.ChargeDirectionConvert(getThisRow[dicAddressIndexes["ChargeDirection"]]);
                    oneRow.IsSegmentPoint = BooleanConvert(getThisRow[dicAddressIndexes["IsSegmentPoint"]]);
                    oneRow.CanSpin = BooleanConvert(getThisRow[dicAddressIndexes["CanSpin"]]);

                    mapAddresses.Add(oneRow);
                    dicMapAddresses.Add(oneRow.Id, oneRow);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void LoadRowBarcodeCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BarcodePath))
                {
                    return;
                }
                var mapBarcodes = mapInfo.mapBarcodes;
                var dicBarcodeIndexes = mapInfo.dicBarcodeIndexes;
                mapBarcodes.Clear();
                dicBarcodeIndexes.Clear();

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
                    MapBarcode oneRow = new MapBarcode();
                    oneRow.Id = getThisRow[dicBarcodeIndexes["Id"]];
                    oneRow.BarcodeHeadNum = float.Parse(getThisRow[dicBarcodeIndexes["BarcodeHeadNum"]]);
                    oneRow.HeadX = float.Parse(getThisRow[dicBarcodeIndexes["HeadX"]]);
                    oneRow.HeadY = float.Parse(getThisRow[dicBarcodeIndexes["HeadY"]]);
                    oneRow.BarcodeTailNum = float.Parse(getThisRow[dicBarcodeIndexes["BarcodeTailNum"]]);
                    oneRow.TailX = float.Parse(getThisRow[dicBarcodeIndexes["TailX"]]);
                    oneRow.TailY = float.Parse(getThisRow[dicBarcodeIndexes["TailY"]]);
                    oneRow.Direction = oneRow.BarcodeDirectionConvert(getThisRow[dicBarcodeIndexes["Direction"]]);

                    mapBarcodes.Add(oneRow);
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
