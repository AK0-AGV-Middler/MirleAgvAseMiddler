using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model;
using Mirle.Agv.Control.Tools;
using Mirle.Agv.Model.TransferCmds;

namespace Mirle.Agv.Control
{
    public class MiddleInterface : IMapBarcodeTaker
    {
        private List<TransCmd> transCmds;
        public List<PartialJob> partialJobs;

        public MiddleInterface()
        {
            transCmds = new List<TransCmd>();
            partialJobs = new List<PartialJob>();
        }

        public void WhenGetAgvcTransCmd(AgvcTransCmd agvcTransCmd)
        {
            //TODO : 
            //1. Convert agvcTransCmd into a list of (SomeCmd imp TransCmd) and set to the transCmds
            //2. Send transCmds to mainflow.
            throw new NotImplementedException();
        }

        public bool GetReserveFromAgvc(string sectionId)
        {
            throw new NotImplementedException();
        }

        public void UpdateMapBarcode(MapBarcodeValues mapBarcode)
        {
            //TODO: Make a Position change report from mapBarcode and send to AGVC
            throw new NotImplementedException();
        }

        public void ClearTransCmds()
        {
            transCmds.Clear();
        }

        public bool IsTransCmds()
        {
            return transCmds.Count > 0;
        }

        public List<TransCmd> GetTransCmds()
        {
            List<TransCmd> tempTransCmds = transCmds.ToList();
            return tempTransCmds;
        }
    }
}
