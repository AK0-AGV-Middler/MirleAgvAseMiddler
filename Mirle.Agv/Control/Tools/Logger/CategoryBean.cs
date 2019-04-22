using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Control.Tools.Logger
{
    public class CategoryBean
    {
        private int iNumber = 0;
        private string sSectionBaseName = "CategoryType";

        public int Number { get { return iNumber; } set { iNumber = value; } }
        public String SectionBaseName
        {
            get
            {
                return sSectionBaseName;
            }
            set
            {
                sSectionBaseName = value;
            }
        }
    }
}
