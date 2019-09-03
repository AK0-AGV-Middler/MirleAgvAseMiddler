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
        public string SectionBeamDisablePath { get; set; }
        public MapInfo TheMapInfo { get; private set; } = new MapInfo();
        private double AddressAreaMm { get; set; } = 30;
        private Vehicle theVehicle = Vehicle.Instance;

        private string lastReadBcrLineId = "";
        private string lastReadBcrId = "";
        private string lastReadAdrId = "";
        private string lastReadSecId = "";

        public MapHandler(MapConfig mapConfig)
        {
            this.mapConfig = mapConfig;
            loggerAgent = LoggerAgent.Instance;
            SectionPath = Path.Combine(Environment.CurrentDirectory, mapConfig.SectionFileName);
            AddressPath = Path.Combine(Environment.CurrentDirectory, mapConfig.AddressFileName);
            BarcodePath = Path.Combine(Environment.CurrentDirectory, mapConfig.BarcodeFileName);
            SectionBeamDisablePath = Path.Combine(Environment.CurrentDirectory, mapConfig.SectionBeamDisablePathFileName);
            AddressAreaMm = mapConfig.AddressAreaMm;

            LoadMapInfo();
        }

        public void LoadMapInfo()
        {
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
                        , $"IsBarcodePathNull={string.IsNullOrWhiteSpace(BarcodePath)}"));
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

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
                //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, OffsetX, OffsetY
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicHeaderIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');

                    MapBarcodeLine oneRow = new MapBarcodeLine();
                    oneRow.Id = getThisRow[dicHeaderIndexes["Id"]];
                    oneRow.HeadBarcode.LineId = oneRow.Id;
                    oneRow.HeadBarcode.Number = int.Parse(getThisRow[dicHeaderIndexes["BarcodeHeadNum"]]);
                    oneRow.HeadBarcode.Position.X = double.Parse(getThisRow[dicHeaderIndexes["HeadX"]]);
                    oneRow.HeadBarcode.Position.Y = double.Parse(getThisRow[dicHeaderIndexes["HeadY"]]);
                    oneRow.TailBarcode.LineId = oneRow.Id;
                    oneRow.TailBarcode.Number = int.Parse(getThisRow[dicHeaderIndexes["BarcodeTailNum"]]);
                    oneRow.TailBarcode.Position.X = double.Parse(getThisRow[dicHeaderIndexes["TailX"]]);
                    oneRow.TailBarcode.Position.Y = double.Parse(getThisRow[dicHeaderIndexes["TailY"]]);
                    oneRow.Offset.X = double.Parse(getThisRow[dicHeaderIndexes["OffsetX"]]);
                    oneRow.Offset.Y = double.Parse(getThisRow[dicHeaderIndexes["OffsetY"]]);



                    int count = oneRow.TailBarcode.Number - oneRow.HeadBarcode.Number;
                    int absCount = Math.Abs(count);
                    if (absCount % 3 != 0)
                    {
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

                            lastReadBcrId = mapBarcode.Number.ToString();
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

                            lastReadBcrId = mapBarcode.Number.ToString();
                            TheMapInfo.allMapBarcodes.Add(mapBarcode.Number, mapBarcode);
                        }
                    }
                    lastReadBcrLineId = oneRow.Id;
                    TheMapInfo.allMapBarcodeLines.Add(oneRow.Id, oneRow);
                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"Load Barcode File Ok. [lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"[lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));

                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void LoadAddressCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddressPath))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                       , $"IsAddressPathNull={string.IsNullOrWhiteSpace(AddressPath)}"));
                    return;
                }
                TheMapInfo.allMapAddresses.Clear();
                TheMapInfo.allCouples.Clear();

                string[] allRows = File.ReadAllLines(AddressPath);
                if (allRows == null || allRows.Length < 2)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"There are no address in file"));
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
                //Id,PositionX,PositionY,
                //IsWorkStation,CanLeftLoad,CanLeftUnload,CanRightLoad,CanRightUnload,
                //IsCharger,CouplerId,ChargeDirection,IsSegmentPoint,CanSpin,IsTR50
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicHeaderIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapAddress oneRow = new MapAddress();
                    oneRow.Id = getThisRow[dicHeaderIndexes["Id"]];
                    oneRow.Position.X = double.Parse(getThisRow[dicHeaderIndexes["PositionX"]]);
                    oneRow.Position.Y = double.Parse(getThisRow[dicHeaderIndexes["PositionY"]]);
                    oneRow.IsWorkStation = bool.Parse(getThisRow[dicHeaderIndexes["IsWorkStation"]]);
                    oneRow.CanLeftLoad = bool.Parse(getThisRow[dicHeaderIndexes["CanLeftLoad"]]);
                    oneRow.CanLeftUnload = bool.Parse(getThisRow[dicHeaderIndexes["CanLeftUnload"]]);
                    oneRow.CanRightLoad = bool.Parse(getThisRow[dicHeaderIndexes["CanRightLoad"]]);
                    oneRow.CanRightUnload = bool.Parse(getThisRow[dicHeaderIndexes["CanRightUnload"]]);
                    oneRow.IsCharger = bool.Parse(getThisRow[dicHeaderIndexes["IsCharger"]]);
                    oneRow.CouplerId = getThisRow[dicHeaderIndexes["CouplerId"]];
                    oneRow.ChargeDirection = oneRow.ChargeDirectionParse(getThisRow[dicHeaderIndexes["ChargeDirection"]]);
                    oneRow.IsSegmentPoint = bool.Parse(getThisRow[dicHeaderIndexes["IsSegmentPoint"]]);
                    oneRow.CanSpin = bool.Parse(getThisRow[dicHeaderIndexes["CanSpin"]]);
                    oneRow.PioDirection = oneRow.PioDirectionParse(getThisRow[dicHeaderIndexes["PioDirection"]]);
                    oneRow.IsTR50 = bool.Parse(getThisRow[dicHeaderIndexes["IsTR50"]]);
                    oneRow.InsideSectionId = getThisRow[dicHeaderIndexes["InsideSectionId"]];

                    lastReadAdrId = oneRow.Id;
                    TheMapInfo.allMapAddresses.Add(oneRow.Id, oneRow);
                    if (oneRow.IsCharger)
                    {
                        TheMapInfo.allCouples.Add(oneRow);
                    }
                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Load Address File Ok. [lastReadAdrId={lastReadAdrId}]"));

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"[lastReadAdrId={lastReadAdrId}]"));

                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void LoadSectionCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionPath))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"IsSectionPathNull={string.IsNullOrWhiteSpace(SectionPath)}"));
                    return;
                }
                TheMapInfo.allMapSections.Clear();

                string[] allRows = File.ReadAllLines(SectionPath);
                if (allRows == null || allRows.Length < 2)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"There are no section in file"));
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
                //Id, FromAddress, ToAddress, Speed, Type, PermitDirection
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicHeaderIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapSection oneRow = new MapSection();
                    oneRow.Id = getThisRow[dicHeaderIndexes["Id"]];
                    oneRow.HeadAddress = TheMapInfo.allMapAddresses[getThisRow[dicHeaderIndexes["FromAddress"]]];
                    oneRow.InsideAddresses.Add(oneRow.HeadAddress);
                    oneRow.TailAddress = TheMapInfo.allMapAddresses[getThisRow[dicHeaderIndexes["ToAddress"]]];
                    oneRow.InsideAddresses.Add(oneRow.TailAddress);
                    oneRow.Distance = Math.Sqrt(GetDistance(oneRow.HeadAddress.Position, oneRow.TailAddress.Position));
                    oneRow.Speed = double.Parse(getThisRow[dicHeaderIndexes["Speed"]]);
                    oneRow.Type = oneRow.SectionTypeParse(getThisRow[dicHeaderIndexes["Type"]]);
                    oneRow.PermitDirection = oneRow.PermitDirectionParse(getThisRow[dicHeaderIndexes["PermitDirection"]]);

                    lastReadSecId = oneRow.Id;
                    TheMapInfo.allMapSections.Add(oneRow.Id, oneRow);
                }

                LoadBeamSensorDisable();

                AddInsideAddresses();

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , $"Load Section File Ok. [lastReadSecId={lastReadSecId}]"));

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"[lastReadSecId={lastReadSecId}]"));

                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void AddInsideAddresses()
        {
            try
            {
                foreach (var adr in TheMapInfo.allMapAddresses.Values)
                {
                    if (TheMapInfo.allMapSections.ContainsKey(adr.InsideSectionId))
                    {
                        TheMapInfo.allMapSections[adr.InsideSectionId].InsideAddresses.Add(adr);
                    }
                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , $"AddInsideAddresses Ok."));
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void LoadBeamSensorDisable()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionBeamDisablePath))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"IsSectionBeamDisablePathNull={string.IsNullOrWhiteSpace(SectionBeamDisablePath)}"));
                    return;
                }

                string[] allRows = File.ReadAllLines(SectionBeamDisablePath);
                if (allRows == null || allRows.Length < 2)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"There are no beam-disable in file"));
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
                //Id, FromAddress, ToAddress, Speed, Type, PermitDirection
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicHeaderIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapSectionBeamDisable oneRow = new MapSectionBeamDisable();
                    oneRow.SectionId = getThisRow[dicHeaderIndexes["SectionId"]];
                    oneRow.Min = double.Parse(getThisRow[dicHeaderIndexes["Min"]]);
                    oneRow.Max = double.Parse(getThisRow[dicHeaderIndexes["Max"]]);
                    oneRow.FrontDisable = bool.Parse(getThisRow[dicHeaderIndexes["FrontDisable"]]);
                    oneRow.BackDisable = bool.Parse(getThisRow[dicHeaderIndexes["BackDisable"]]);
                    oneRow.LeftDisable = bool.Parse(getThisRow[dicHeaderIndexes["LeftDisable"]]);
                    oneRow.RightDisable = bool.Parse(getThisRow[dicHeaderIndexes["RightDisable"]]);

                    AddMapSectionBeamDisableIntoList(oneRow);
                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Load BeamDisable File Ok."));

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void AddMapSectionBeamDisableIntoList(MapSectionBeamDisable oneRow)
        {
            try
            {
                if (!TheMapInfo.allMapSections.ContainsKey(oneRow.SectionId))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"AddMapSectionBeamDisableIntoList +++FAIL+++. AllMapSections.ContainsKey({oneRow.SectionId})={false}"));

                    return;
                }
                MapSection mapSection = TheMapInfo.allMapSections[oneRow.SectionId];
                if (oneRow.Min < 0)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Min < 0. [SectionId={oneRow.SectionId}][Min={oneRow.Min}]"));
                    return;
                }
                if (oneRow.Max > mapSection.Distance + 1)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"Max > Distance. [SectionId={oneRow.SectionId}][Max={oneRow.Max}][Distance={mapSection.Distance}]"));

                    return;
                }

                mapSection.BeamSensorDisables.Add(oneRow);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public bool IsPositionInThisSection(MapPosition aPosition, MapSection aSection)
        {
            MapSection mapSection = aSection.DeepClone();
            MapAddress myHeadAddr = mapSection.HeadAddress;
            MapAddress myTailAddr = mapSection.TailAddress;

            switch (aSection.Type)
            {
                case EnumSectionType.Vertical:
                    {
                        if ((int)aSection.HeadAddress.Position.Y > (int)aSection.TailAddress.Position.Y)
                        {
                            myHeadAddr = aSection.TailAddress;
                            myTailAddr = aSection.HeadAddress;
                        }
                    }
                    break;
                case EnumSectionType.Horizontal:
                case EnumSectionType.R2000:
                    {
                        if ((int)aSection.HeadAddress.Position.X > (int)aSection.TailAddress.Position.X)
                        {
                            myHeadAddr = aSection.TailAddress;
                            myTailAddr = aSection.HeadAddress;
                        }
                    }
                    break;
                case EnumSectionType.None:
                default:
                    break;
            }

            VehiclePosition location = theVehicle.CurVehiclePosition;


            #region Not in Section
            //Position 在 Head 西方過遠
            if (aPosition.X + AddressAreaMm < myHeadAddr.Position.X)
            {
                return false;
            }
            //Position 在 Tail 東方過遠
            if (aPosition.X > myTailAddr.Position.X + AddressAreaMm)
            {
                return false;
            }
            //Position 在 Head 北方過遠
            if (aPosition.Y < myHeadAddr.Position.Y - AddressAreaMm)
            {
                return false;
            }
            //Position 在 Tail 南方過遠
            if (aPosition.Y - AddressAreaMm > myTailAddr.Position.Y)
            {
                return false;
            }
            #endregion

            #region In Section           

            foreach (var insideAddress in mapSection.InsideAddresses)
            {
                if (IsPositionInThisAddress(aPosition, insideAddress.Position))
                {
                    location.LastAddress = insideAddress;
                }
            }

            location.LastSection = mapSection;

            location.LastSection.Distance = Math.Sqrt(GetDistance(aPosition, mapSection.HeadAddress.Position));
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
            return Math.Abs(aPosition.X - addressPosition.X) <= AddressAreaMm && Math.Abs(aPosition.Y - addressPosition.Y) <= AddressAreaMm;
        }

        public double GetDistance(MapPosition aPosition, MapPosition bPosition)
        {
            var diffX = Math.Abs(aPosition.X - bPosition.X);
            var diffY = Math.Abs(aPosition.Y - bPosition.Y);
            return (diffX * diffX) + (diffY * diffY);
        }
    }

}
