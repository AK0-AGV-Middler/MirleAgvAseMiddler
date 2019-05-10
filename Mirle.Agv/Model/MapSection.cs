using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class MapSection
    {
        private string sectionId;
        private string strOrigin;
        private string strDestination;
        private float fltDistance;
        private EnumSectionType sectionType;
        private EnumSectionShape sectionShape;
        private float fltPadding;
        private string strOriginBC;
        private string strDestinationBC;
        private double sectionLength;


        public MapSection()
        {
            sectionType = EnumSectionType.None;
            sectionShape = EnumSectionShape.None;
            sectionId = "Empty";
            strOrigin = "Empty";
            strDestination = "Empty";
        }
        public MapSection(Dictionary<string, int> HeaderTable, string[] Content)
        {
            try
            {
                sectionId = Content[HeaderTable["Id"]];
                strOrigin = Content[HeaderTable["Origin"]];
                strDestination = Content[HeaderTable["Destination"]];
                fltDistance = float.Parse(Content[HeaderTable["Distance"]]);
                sectionType = (EnumSectionType)Enum.Parse(typeof(EnumSectionType), Content[HeaderTable["Type"]]);
                sectionShape = (EnumSectionShape)Enum.Parse(typeof(EnumSectionShape), Content[HeaderTable["Shape"]]);
                fltPadding = float.Parse(Content[HeaderTable["Padding"]]);
                strOriginBC = Content[HeaderTable["OriginBC"]];
                strDestinationBC = Content[HeaderTable["DestinationBC"]];
            }
            catch (Exception ex)
            {
                string Message = "Section ID : " + Content[HeaderTable["Id"]] + "\n" + ex.ToString();
                throw new System.ArgumentException(Message);
            }

        }

        public string Id
        {
            get { return this.sectionId; }
            set { this.sectionId = value; }
        }

        public string Origin
        {
            get { return this.strOrigin; }
            set { this.strOrigin = value; }
        }

        public string Destination
        {
            get { return this.strDestination; }
            set { this.strDestination = value; }
        }

        public float Distance
        {
            get { return this.fltDistance; }
            set { this.fltDistance = value; }
        }

        public EnumSectionType Type
        {
            get { return this.sectionType; }
            set { this.sectionType = value; }
        }

        public EnumSectionShape Shape
        {
            get { return this.sectionShape; }
            set { this.sectionShape = value; }
        }

        public float Padding
        {
            get { return this.fltPadding; }
            set { this.fltPadding = value; }
        }

        public string OriginBC
        {
            get { return this.strOriginBC; }
            set { this.strOriginBC = value; }
        }

        public string DestinationBC
        {
            get { return this.strDestinationBC; }
            set { this.strDestinationBC = value; }
        }

        public double GetSectionLength()
        {
            return sectionLength;
        }
    }

}
