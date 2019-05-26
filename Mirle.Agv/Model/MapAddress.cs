using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapAddress
    {
        //Id, BarcodeH, BarcodeV, PositionX, PositionY, Type, DisplayLevel
        public string Id { get; set; }
        public float BarcodeH { get; set; }
        public float BarcodeV { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public EnumAddressType Type { get; set; }
        public EnumDisplayLevel DisplayLevel { get; set; }

        public MapAddress()
        {
            DisplayLevel = EnumDisplayLevel.Normal;
            Type = EnumAddressType.None;
            Id = "Empty";   
        }

        public MapAddress(Dictionary<string, int> HeaderTable, string[] Content)
        {
            try
            {
                Id = Content[HeaderTable["Id"]];
                //Barcode = Content[HeaderTable["Barcode"]];
                PositionX = float.Parse(Content[HeaderTable["PositionX"]]);
                PositionY = float.Parse(Content[HeaderTable["PositionY"]]);
                Type = (EnumAddressType)Enum.Parse(typeof(EnumAddressType), Content[HeaderTable["Type"]]);
                //DisplayLevel = Content[HeaderTable["DisplayLevel"]];
                DisplayLevel = AddressDisplayLevelConvet( Content[HeaderTable["DisplayLevel"]]);
            }
            catch (Exception ex)
            {
                string Message = "Address ID : " + Content[HeaderTable["Id"]] + "\n" + ex.ToString();
                throw new System.ArgumentException(Message);
            }
        }

        private EnumDisplayLevel AddressDisplayLevelConvet(string v)
        {
            v = v.Trim();
            return (EnumDisplayLevel)(int.Parse(v));
        }
    }

}

