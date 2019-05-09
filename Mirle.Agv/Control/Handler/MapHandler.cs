using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Generic;
using System.IO;
using Mirle.Agv.Model.Configs;


namespace Mirle.Agv.Control
{
    public class MapHandler
    {
        private Dictionary<string, MapSection> SectionTable = new Dictionary<string, MapSection>();
        private Dictionary<string, List<MapSection>> SectionTableByAddress = new Dictionary<string, List<MapSection>>();
        private Dictionary<string, MapAddress> AddressTable = new Dictionary<string, MapAddress>();
        private LoggerAgent loggerAgent;
        private MapConfigs mapConfigs;

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
            GetMap(mapConfigs.SectionFilePath, mapConfigs.AddressFilePath);
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
    }

}
