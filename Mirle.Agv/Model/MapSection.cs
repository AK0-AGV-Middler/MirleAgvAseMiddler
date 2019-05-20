using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    //ID, FromAdr, ToAdr, Distance, Shape, Type, Padding, FromX, FromY, ToX, ToY
    public class MapSection
    {
        public string Id { get; set; }
        //public string Origin { get; set; }
        //public string Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public float Distance { get; set; }
        public EnumSectionShape Shape { get; set; }
        public EnumSectionType Type { get; set; }
        public float Padding { get; set; }
        public float FromAddressX { get; set; }
        public float FromAddressY { get; set; }
        public float ToAddressX { get; set; }
        public float ToAddressY { get; set; }
        //public string OriginBC { get; set; }
        //public string DestinationBC { get; set; }

        public MapSection()
        {
            Type = EnumSectionType.None;
            Shape = EnumSectionShape.None;
            Id = "Empty";
            FromAddress = "Empty";
            ToAddress = "Empty";
        }
        public MapSection(Dictionary<string, int> HeaderTable, string[] Content)
        {
            try
            {
                Id = Content[HeaderTable["Id"]];
                FromAddress = Content[HeaderTable["FromAddress"]];
                ToAddress = Content[HeaderTable["ToAddress"]];
                Distance = float.Parse(Content[HeaderTable["Distance"]]);
                //Type = (EnumSectionType)Enum.Parse(typeof(EnumSectionType), Content[HeaderTable["Type"]]);
                //Shape = (EnumSectionShape)Enum.Parse(typeof(EnumSectionShape), Content[HeaderTable["Shape"]]);                
                Type = SectionTypeConvert(Content[HeaderTable["Type"]]);       
                Shape = SectionShapeConvert(Content[HeaderTable["Shape"]]);

                Padding = float.Parse(Content[HeaderTable["Padding"]]);
                FromAddressX = float.Parse(Content[HeaderTable["FromAddressX"]]);
                FromAddressY = float.Parse(Content[HeaderTable["FromAddressY"]]);
                ToAddressX = float.Parse(Content[HeaderTable["ToAddressX"]]);
                ToAddressY = float.Parse(Content[HeaderTable["ToAddressY"]]);
                //OriginBC = Content[HeaderTable["OriginBC"]];
                //DestinationBC = Content[HeaderTable["DestinationBC"]];
            }
            catch (Exception ex)
            {
                string Message = "Section ID : " + Content[HeaderTable["Id"]] + "\n" + ex.ToString();
                throw new System.ArgumentException(Message);
            }
        }

        private EnumSectionShape SectionShapeConvert(string v)
        {
            switch (v)
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

        private EnumSectionType SectionTypeConvert(string v)
        {
            switch (v)
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

    }

}
