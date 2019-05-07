using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapAddress
    {
       private string strId;
        private string strBarcode;
        private float fltPositionX;
        private float fltPositionY;
        private EnumAddressType addressType;
        private string strDisplayLevel;


        public MapAddress()
        {
            addressType = EnumAddressType.None;
            strId = "Empty";
            strBarcode = "0";

        }
        public MapAddress(Dictionary<string, int> HeaderTable, string[] Content)
        {
            try
            {
                strId = Content[HeaderTable["Id"]];
                strBarcode = Content[HeaderTable["Barcode"]];
                fltPositionX = float.Parse(Content[HeaderTable["PositionX"]]);
                fltPositionY = float.Parse(Content[HeaderTable["PositionY"]]);
                addressType = (EnumAddressType)Enum.Parse(typeof(EnumAddressType), Content[HeaderTable["Type"]]);
                strDisplayLevel = Content[HeaderTable["DisplayLevel"]];
            }
            catch (Exception ex)
            {
                string Message = "Address ID : " + Content[HeaderTable["Id"]] + "\n" + ex.ToString();
                throw new System.ArgumentException(Message);               
            }
        }

        public string Id
        {
            get { return this.strId; }
            set { this.strId = value; }
        }

        public string Barcode
        {
            get { return this.strBarcode; }
            set { this.strBarcode = value; }
        }

        public float PositionX
        {
            get { return this.fltPositionX; }
            set { this.fltPositionX = value; }
        }

        public float PositionY
        {
            get { return this.fltPositionY; }
            set { this.fltPositionY = value; }
        }

        public EnumAddressType Type
        {
            get { return this.addressType; }
            set { this.addressType = value; }
        }

        public string DisplayLevel
        {
            get { return this.strDisplayLevel; }
            set { this.strDisplayLevel = value; }
        }
    }

}

