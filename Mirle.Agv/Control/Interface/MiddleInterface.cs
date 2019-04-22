using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Control.Tools;

namespace Mirle.Agv.Control
{
    public class MiddleInterface
    {
        public List<PartialJob> partialJobs;

        private MainFlowHandler mainFlowHandler;

        public MiddleInterface(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
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
