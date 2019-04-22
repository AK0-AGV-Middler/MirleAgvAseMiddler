using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class PartialJob
    {
        public EnumPartialJobType partialJobType;

        public PartialJob Clone()
        {
            //make a new PartialJob with same value for each member
            throw new NotImplementedException();
        }
    }    


}
