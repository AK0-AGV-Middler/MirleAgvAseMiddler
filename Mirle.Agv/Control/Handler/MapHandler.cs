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
        private Dictionary<string, MapSection> SectionTable = new Dictionary<string, MapSection>();
        private Dictionary<string, List<MapSection>> SectionTableByAddress = new Dictionary<string, List<MapSection>>();
        private Dictionary<string, MapAddress> AddressTable = new Dictionary<string, MapAddress>();
        private LoggerAgent loggerAgent;
        private MapConfigs mapConfigs;
        public string SectionPath { get; set; }
        public string AddressPath { get; set; }
        public string BarcodePath { get; set; }
        public List<MapSection> mapSections;
        public List<MapAddress> mapAddresses;
        public Dictionary<string, MapAddress> dicMapAddresses;
        public List<MapRowBarcode> mapRowBarcodes;
        private MapInfo mapInfo;

        private void GetSectionTable(string SectionFilePath)
        {
            Dictionary<string, int> HeaderTable = new Dictionary<string, int>();
            string[] Rows = File.ReadAllLines(SectionFilePath);
            string[] Header = Rows[0].Split(',');
            string[] Content = null;
            SectionTable.Clear();
            SectionTableByAddress.Clear();

            for (int idx = 0; idx < Header.Length; idx++)
            {
                HeaderTable[Header[idx]] = idx;
            }
            for (int i = 1; i < Rows.Length; i++)
            {
                Content = Rows[i].Split(',');
                SectionTable[Content[HeaderTable["Id"]]] = new MapSection(HeaderTable, Content);
                if (SectionTableByAddress.ContainsKey(Content[HeaderTable["Origin"]]))
                {
                    SectionTableByAddress[Content[HeaderTable["Origin"]]].Add(SectionTable[Content[HeaderTable["Id"]]]);
                }
                else
                {
                    SectionTableByAddress[Content[HeaderTable["Origin"]]] = new List<MapSection>() { SectionTable[Content[HeaderTable["Id"]]] };
                }

                if (SectionTableByAddress.ContainsKey(Content[HeaderTable["Destination"]]))
                {
                    SectionTableByAddress[Content[HeaderTable["Destination"]]].Add(SectionTable[Content[HeaderTable["Id"]]]);
                }
                else
                {
                    SectionTableByAddress[Content[HeaderTable["Destination"]]] = new List<MapSection>() { SectionTable[Content[HeaderTable["Id"]]] };
                }
            }

        }

        private void GetAddressTable(string AddressFilePath)
        {
            Dictionary<string, int> HeaderTable = new Dictionary<string, int>();
            string[] Rows = File.ReadAllLines(AddressFilePath);
            string[] Header = Rows[0].Split(',');
            string[] Content = null;
            this.AddressTable.Clear();
            for (int idx = 0; idx < Header.Length; idx++)
            {
                HeaderTable[Header[idx]] = idx;
            }
            for (int i = 1; i < Rows.Length; i++)
            {
                Content = Rows[i].Split(',');
                this.AddressTable[Content[HeaderTable["Id"]]] = new MapAddress(HeaderTable, Content);
            }
        }

        public MapHandler(MapConfigs mapConfigs)
        {
            this.mapConfigs = mapConfigs;
            loggerAgent = LoggerAgent.Instance;
            rootDir = mapConfigs.RootDir;
            SectionPath = Path.Combine(rootDir, mapConfigs.SectionFilePath);
            AddressPath = Path.Combine(rootDir, mapConfigs.AddressFilePath);
            BarcodePath = Path.Combine(rootDir, mapConfigs.BarcodeFilePath);
            mapInfo = MapInfo.Instance;
            mapSections = mapInfo.mapSections;
            mapAddresses = mapInfo.mapAddresses;
            dicMapAddresses = mapInfo.dicMapAddresses;
            mapRowBarcodes = mapInfo.mapRowBarcodes;
            //GetMap(SectionPath, AddressPath);
            LoadSectionCsv();
            LoadAddressCsv();
            LoadRowBarcodeCsv();
            //AddressFitSectionAndRowBarcode();
        }

        public void GetMap(string SectionFilePath, string AddressFilePath)
        {
            GetSectionTable(SectionFilePath);
            GetAddressTable(AddressFilePath);
        }

        public MapSection GetMapSection(string idx)
        {
            try
            {
                return SectionTable[idx];
            }
            catch (Exception ex)
            {
                //log ex
                return new MapSection();
            }
        }

        public MapAddress GetMapAddress(string idx)
        {
            try
            {
                return AddressTable[idx];
            }
            catch (Exception ex)
            {
                //log ex
                return new MapAddress();
            }
        }

        public List<MapSection> GetSectionByAddress(string idx)
        {
            try
            {
                return SectionTableByAddress[idx];
            }
            catch (Exception ex)
            {
                //log ex
                return new List<MapSection>();
            }
        }

        public void OnMapBarcodeValuesChangedEvent(object sender, MapBarcodeReader mapBarcodeValues)
        {
            throw new NotImplementedException();
        }

        public void OnTransCmdsFinishedEvent(object sender, EnumCompleteStatus status)
        {
            throw new NotImplementedException();
        }

        public void OnMiddlerGetsNewTransCmds(object sender, AgvcTransCmd e)
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

                mapSections.Clear();

                string[] allRows = File.ReadAllLines(SectionPath);
                if (allRows == null || allRows.Length < 2)
                {
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //ID, FromAdr, ToAdr, Distance, Shape, Type, Padding, FromX, FromY, ToX, ToY
                int idIndex = -1;
                int fromAddressIndex = -1;
                int toAddressIndex = -1;
                int distanceIndex = -1;
                int shapeIndex = -1;
                int typeIndex = -1;
                int paddingIndex = -1;
                int fromXIndex = -1;
                int fromYIndex = -1;
                int toXIndex = -1;
                int toYIndex = -1;
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    switch (keyword)
                    {
                        case "Id":
                            idIndex = i;
                            break;
                        case "FromAdr":
                            fromAddressIndex = i;
                            break;
                        case "ToAdr":
                            toAddressIndex = i;
                            break;
                        case "Distance":
                            distanceIndex = i;
                            break;
                        case "Padding":
                            paddingIndex = i;
                            break;
                        case "Shape":
                            shapeIndex = i;
                            break;
                        case "Type":
                            typeIndex = i;
                            break;
                        case "FromX":
                            fromXIndex = i;
                            break;
                        case "FromY":
                            fromYIndex = i;
                            break;
                        case "ToX":
                            toXIndex = i;
                            break;
                        case "ToY":
                            toYIndex = i;
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapSection mapSection = new MapSection();
                    mapSection.Id = idIndex > -1 ? getThisRow[idIndex] : "Empty";
                    mapSection.FromAddress = fromAddressIndex > -1 ? getThisRow[fromAddressIndex] : "Empty";
                    mapSection.ToAddress = toAddressIndex > -1 ? getThisRow[toAddressIndex] : "Empty";
                    mapSection.Distance = distanceIndex > -1 ? float.Parse(getThisRow[distanceIndex]) : 0;
                    mapSection.Padding = paddingIndex > -1 ? float.Parse(getThisRow[paddingIndex]) : 0;
                    mapSection.Shape = shapeIndex > -1 ? SectionShapeConvert(getThisRow[shapeIndex]) : EnumSectionShape.None;
                    mapSection.Type = typeIndex > -1 ? SectionTypeConvert(getThisRow[typeIndex]) : EnumSectionType.None;
                    mapSection.FromAddressX = fromXIndex > -1 ? float.Parse(getThisRow[fromXIndex]) : 0;
                    mapSection.FromAddressY = fromYIndex > -1 ? float.Parse(getThisRow[fromYIndex]) : 0;
                    mapSection.ToAddressX = toXIndex > -1 ? float.Parse(getThisRow[toXIndex]) : 0;
                    mapSection.ToAddressY = toYIndex > -1 ? float.Parse(getThisRow[toYIndex]) : 0;

                    mapSections.Add(mapSection);
                    //SectionTable.Add(mapSection.Id, mapSection);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private EnumSectionType SectionTypeConvert(string v)
        {
            var keyword = v.Trim();
            switch (keyword)
            {
                case "Horizontal":
                    return EnumSectionType.Horizontal;
                case "Vertical":
                    return EnumSectionType.Vertical;
                case "QuadrantI":
                    return EnumSectionType.QuadrantI;
                case "QuadrantII":
                    return EnumSectionType.QuadrantII;
                case "QuadrantIII":
                    return EnumSectionType.QuadrantIII;
                case "QuadrantIV":
                    return EnumSectionType.QuadrantIV;
                case "None":
                default:
                    return EnumSectionType.None;
            }
        }

        private EnumSectionShape SectionShapeConvert(string v)
        {
            var keyword = v.Trim();
            switch (keyword)
            {
                case "Curve":
                    return EnumSectionShape.Curve;
                case "Straight":
                    return EnumSectionShape.Straight;
                case "None":
                default:
                    return EnumSectionShape.None;
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

                mapAddresses.Clear();

                string[] allRows = File.ReadAllLines(AddressPath);
                if (allRows == null || allRows.Length < 2)
                {
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, BarcodeH, BarcodeV, PositionX, PositionY, Type, DisplayLevel       

                int idIndex = -1;
                int barcodeHIndex = -1;
                int barcodeVIndex = -1;
                int positionXIndex = -1;
                int positionYIndex = -1;
                int typeIndex = -1;
                int displayLevelIndex = -1;

                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    switch (keyword)
                    {
                        case "Id":
                            idIndex = i;
                            break;
                        case "BarcodeH":
                            barcodeHIndex = i;
                            break;
                        case "BarcodeV":
                            barcodeVIndex = i;
                            break;
                        case "PositionX":
                            positionXIndex = i;
                            break;
                        case "PositionY":
                            positionYIndex = i;
                            break;
                        case "Type":
                            typeIndex = i;
                            break;
                        case "Display":
                            displayLevelIndex = i;
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapAddress mapAddress = new MapAddress();
                    mapAddress.Id = idIndex > -1 ? getThisRow[idIndex] : "Empty";
                    mapAddress.BarcodeH = barcodeHIndex > -1 ? float.Parse(getThisRow[barcodeHIndex]) : 0;
                    mapAddress.BarcodeV = barcodeVIndex > -1 ? float.Parse(getThisRow[barcodeVIndex]) : 0;
                    mapAddress.PositionX = positionXIndex > -1 ? float.Parse(getThisRow[positionXIndex]) : 0;
                    mapAddress.PositionY = positionYIndex > -1 ? float.Parse(getThisRow[positionYIndex]) : 0;
                    mapAddress.Type = typeIndex > -1 ? AddressTypeConvert(getThisRow[typeIndex]) : EnumAddressType.None;
                    mapAddress.DisplayLevel = displayLevelIndex > -1 ? AddressDisplayLevelConvert(getThisRow[displayLevelIndex]) : EnumDisplayLevel.Lowest;

                    mapAddresses.Add(mapAddress);
                    dicMapAddresses.Add(mapAddress.Id, mapAddress);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private EnumDisplayLevel AddressDisplayLevelConvert(string v)
        {
            v = v.Trim();
            switch (v)
            {
                case "High":
                    return EnumDisplayLevel.High;
                case "Highest":
                    return EnumDisplayLevel.Highest;
                case "Low":
                    return EnumDisplayLevel.Low;
                case "Lowest":
                    return EnumDisplayLevel.Lowest;             
                case "Normal":
                default:
                    return EnumDisplayLevel.Normal;
            }
        }

        private EnumAddressType AddressTypeConvert(string v)
        {
            v = v.Trim();

            switch (v)
            {
                case "BarcodeCross":
                    return EnumAddressType.BarcodeCross;
                case "BarcodeEnd":
                    return EnumAddressType.BarcodeEnd;
                case "Charger":
                    return EnumAddressType.Charger;
                case "Port":
                    return EnumAddressType.Port;
                case "TR":
                    return EnumAddressType.TR;
                case "None":                            
                default:
                    return EnumAddressType.None;
            }
        }

        public void AddressFitSectionAndRowBarcode()
        {
            try
            {
                if (dicMapAddresses.Count == 0)
                {
                    return;
                }
                if (mapSections.Count == 0)
                {
                    return;
                }
                if (mapAddresses.Count == 0)
                {
                    return;
                }
                if (mapRowBarcodes.Count == 0)
                {
                    return;
                }

                foreach (var sectionInfo in mapSections)
                {
                    var fromAdr = sectionInfo.FromAddress;
                    if (dicMapAddresses.ContainsKey(fromAdr))
                    {
                        sectionInfo.FromAddressX = dicMapAddresses[fromAdr].PositionX;
                        sectionInfo.FromAddressY = dicMapAddresses[fromAdr].PositionY;
                    }

                    var toAdr = sectionInfo.ToAddress;
                    if (dicMapAddresses.ContainsKey(toAdr))
                    {
                        sectionInfo.ToAddressX = dicMapAddresses[toAdr].PositionX;
                        sectionInfo.ToAddressY = dicMapAddresses[toAdr].PositionY;
                    }

                    if (sectionInfo.ToAddressX == sectionInfo.FromAddressX)
                    {
                        sectionInfo.Type = EnumSectionType.Vertical;
                    }
                    else if (sectionInfo.ToAddressY == sectionInfo.FromAddressY)
                    {
                        sectionInfo.Type = EnumSectionType.Horizontal;
                    }
                    else if (sectionInfo.ToAddressX > sectionInfo.FromAddressX && sectionInfo.ToAddressY > sectionInfo.FromAddressY)
                    {
                        sectionInfo.Type = EnumSectionType.QuadrantI;
                    }
                    else if (sectionInfo.ToAddressX < sectionInfo.FromAddressX && sectionInfo.ToAddressY < sectionInfo.FromAddressY)
                    {
                        sectionInfo.Type = EnumSectionType.QuadrantII;
                    }
                    else if (sectionInfo.ToAddressX > sectionInfo.FromAddressX && sectionInfo.ToAddressY < sectionInfo.FromAddressY)
                    {
                        //Turn left 
                        //    t
                        //    |
                        // f---
                        sectionInfo.Type = EnumSectionType.QuadrantIII;
                    }
                    else if (sectionInfo.ToAddressX < sectionInfo.FromAddressX && sectionInfo.ToAddressY > sectionInfo.FromAddressY)
                    {
                        //Turn right 
                        //    f
                        //    |
                        // t---
                        sectionInfo.Type = EnumSectionType.QuadrantIV;
                    }
                    else
                    {
                        sectionInfo.Type = EnumSectionType.None;
                    }
                    var disX = Math.Abs(sectionInfo.FromAddressX - sectionInfo.ToAddressX);
                    var disY = Math.Abs(sectionInfo.FromAddressY - sectionInfo.ToAddressY);
                    sectionInfo.Distance = disX + disY;
                }

            }
            catch (Exception)
            {

                throw;
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

                mapRowBarcodes.Clear();

                string[] allRows = File.ReadAllLines(BarcodePath);
                if (allRows == null || allRows.Length < 2)
                {
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, Type

                int idIndex = -1;
                int barcodeHeadNumIndex = -1;
                int headXIndex = -1;
                int headYIndex = -1;
                int barcodeTailNumIndex = -1;
                int tailXIndex = -1;
                int tailYIndex = -1;
                int typeIndex = -1;                

                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    switch (keyword)
                    {
                        case "Id":
                            idIndex = i;
                            break;
                        case "BarcodeHeadNum":
                            barcodeHeadNumIndex = i;
                            break;
                        case "HeadX":
                            headXIndex = i;
                            break;
                        case "HeadY":
                            headYIndex = i;
                            break;
                        case "BarcodeTailNum":
                            barcodeTailNumIndex = i;
                            break;
                        case "TailX":
                            tailXIndex = i;
                            break;
                        case "TailY":
                            tailYIndex = i;
                            break;
                        case "Type":
                            typeIndex = i;
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapRowBarcode mapRowBarcode = new MapRowBarcode();
                    mapRowBarcode.Id = idIndex > -1 ? getThisRow[idIndex] : "Empty";
                    mapRowBarcode.BarcodeHeadNum = barcodeHeadNumIndex > -1 ? float.Parse(getThisRow[barcodeHeadNumIndex]) : 0;
                    mapRowBarcode.HeadX = headXIndex > -1 ? float.Parse(getThisRow[headXIndex]) : 0;
                    mapRowBarcode.HeadY = headYIndex > -1 ? float.Parse(getThisRow[headYIndex]) : 0;
                    mapRowBarcode.BarcodeTailNum = barcodeTailNumIndex > -1 ? float.Parse(getThisRow[barcodeTailNumIndex]) : 0;
                    mapRowBarcode.TailX = tailXIndex > -1 ? float.Parse(getThisRow[tailXIndex]) : 0;
                    mapRowBarcode.TailY = tailYIndex > -1 ? float.Parse(getThisRow[tailYIndex]) : 0;
                    mapRowBarcode.Type = typeIndex > -1 ? RowBarcodeTypeConvert(getThisRow[typeIndex]) : EnumRowBarcodeType.None;

                    mapRowBarcodes.Add(mapRowBarcode);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private EnumRowBarcodeType RowBarcodeTypeConvert(string v)
        {
            v = v.Trim();

            switch (v)
            {
                case "Horizontal":
                    return EnumRowBarcodeType.Horizontal;
                case "Vertical":
                    return EnumRowBarcodeType.Vertical;
                case "None":
                default:
                    return EnumRowBarcodeType.None;
            }
        }
    }

}
