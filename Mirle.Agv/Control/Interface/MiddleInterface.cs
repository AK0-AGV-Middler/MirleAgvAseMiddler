using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;

namespace Mirle.Agv.Control
{
    public class MiddleInterface
    {
        public List<PartialJob> partialJobs;

        public MiddleInterface()
        {

        }

        public List<PartialJob> PartialJobParse(AgvcCmd aCmd)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// transCmdInfo from AgvcCmd, MoveJob implement PartialJob
        /// </summary>
        /// <param name="transCmdInfo"></param>
        /// <returns></returns>
        private MoveCmdInfo MoveCmdParse(TransCmdInfo transCmdInfo)
        {
            throw new NotImplementedException();
        }

        private LoadCmdInfo LoadCmdParse(TransCmdInfo transCmdInfo)
        {
            throw new NotImplementedException();
        }

        private UnloadCmdInfo UnloadCmdParse(TransCmdInfo transCmdInfo)
        {
            throw new NotImplementedException();
        }

        public bool GetReserveFromAgvc(string sectionId)
        {
            throw new NotImplementedException();
        }
    }
}
